using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TermometroDaSala.Desktop;

[JsonConverter(typeof(JsonStringEnumConverter<NivelCalor>))]
public enum NivelCalor { normal, atencao, cuidado, perigo }

public sealed class Sala
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public int Andar { get; set; }
    public string FaceSol { get; set; } = "norte";
    public bool TemVentilador { get; set; }
    public string? DispositivoId { get; set; }
    public bool SensorRevogado { get; set; }
    public NivelCalor? NivelAtual { get; set; }
    public double? IndiceAtual { get; set; }
    [JsonConverter(typeof(DataOuVazio))] public DateTimeOffset? UltimaLeituraEm { get; set; }
    // A API manda semLeitura=true com nivelAtual "normal" e indiceAtual 0 quando a sala nunca foi medida.
    public bool SemLeitura { get; set; }
    [JsonIgnore] public Leitura? Leitura => !SemLeitura && UltimaLeituraEm is { } hora && NivelAtual is { } nivel && IndiceAtual is { } indice
        ? new Leitura(indice, nivel, hora) : null;
}

/// <summary>A API devolve ultimaLeituraEm como "" para sala sem leitura; aqui isso vira null.</summary>
public sealed class DataOuVazio : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader leitor, Type tipo, JsonSerializerOptions opcoes) =>
        leitor.TokenType == JsonTokenType.String && leitor.GetString() == "" ? null : leitor.GetDateTimeOffset();

    public override void Write(Utf8JsonWriter escritor, DateTimeOffset? valor, JsonSerializerOptions opcoes)
    {
        if (valor is { } data) escritor.WriteStringValue(data);
        else escritor.WriteNullValue();
    }
}

public sealed record Leitura(double IndiceCalor, NivelCalor Nivel, DateTimeOffset MedidoEm);

public sealed class Previsao
{
    public required string AmbienteId { get; set; }
    public required DateTimeOffset PrevistoPara { get; set; }
    public required double IndiceRuaPrevisto { get; set; }
    public double? DeltaSala { get; set; }
    public double? IndiceSalaPrevisto { get; set; }
    public required NivelCalor NivelPrevisto { get; set; }
    public required bool SemLeiturasRecentes { get; set; }
    public required bool Demonstracao { get; set; }
    // false: sem previsão da rua, a API repete o índice da última leitura válida da sala e manda IndiceRuaPrevisto 0.
    public required bool RuaDisponivel { get; set; }
}

public sealed class Acao
{
    // A API gera o id (long). No modo local, o serviço numera em sequência.
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public NivelCalor Nivel { get; set; }
    public string Descricao { get; set; } = "";
    public DateTimeOffset RegistradoEm { get; set; } = DateTimeOffset.UtcNow;
}

public static class Validacoes
{
    public static readonly string[] Faces = ["norte", "sul", "leste", "oeste"];
    public static Dictionary<string, string> Sala(Sala? sala)
    {
        var erros = new Dictionary<string, string>();
        if (sala is null) return new() { ["Sala"] = "Cadastro de sala ausente." };
        if (!Regex.IsMatch(sala.Id ?? "", @"\Asala-[0-9]{2}\z")) erros[nameof(sala.Id)] = "Use sala-NN, por exemplo sala-03.";
        if (string.IsNullOrWhiteSpace(sala.Nome)) erros[nameof(sala.Nome)] = "Informe o nome da sala.";
        if (sala.Andar is < 0 or > 20) erros[nameof(sala.Andar)] = "O andar deve estar entre 0 e 20.";
        if (!Faces.Contains(sala.FaceSol)) erros[nameof(sala.FaceSol)] = "Escolha norte, sul, leste ou oeste.";
        return erros;
    }

    public static string? Descricao(string? texto) => (texto?.Trim().Length ?? 0) is < 5 or > 300
        ? "Descreva a ação com 5 a 300 caracteres." : null;

    public static void Conferir(Acao? acao)
    {
        if (acao is null || !Regex.IsMatch(acao.AmbienteId ?? "", @"\Asala-[0-9]{2}\z") || !Enum.IsDefined(acao.Nivel))
            throw new ArgumentException("Sala ou nível inválido.");
        if (Descricao(acao.Descricao) is { } erro) throw new ArgumentException(erro);
    }

    public static string Texto(NivelCalor nivel) => nivel switch
    {
        NivelCalor.normal => "Normal", NivelCalor.atencao => "Atenção",
        NivelCalor.cuidado => "Cuidado extremo", NivelCalor.perigo => "Perigo", _ => "Nível inválido"
    };

    public static DateTimeOffset HoraEscola(DateTimeOffset hora) => hora.ToOffset(TimeSpan.FromHours(-3));
    public static DateOnly DiaEscola(DateTimeOffset hora) => DateOnly.FromDateTime(HoraEscola(hora).DateTime);
}

public static class Configuracao
{
    public static Uri LerUrl()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));
        if (!json.RootElement.TryGetProperty("ApiUrl", out var campo) || campo.ValueKind != JsonValueKind.String ||
            !Uri.TryCreate(campo.GetString(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https") || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("ApiUrl deve ser uma URL HTTP ou HTTPS sem credenciais, consulta ou fragmento.");
        return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
    }
}
