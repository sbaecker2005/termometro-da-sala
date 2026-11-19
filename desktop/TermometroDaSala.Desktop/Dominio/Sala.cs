using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TermometroDaSala.Desktop;

/// <summary>
/// Sala monitorada. Pelo código ela só nasce válida (Criar); a que vem da API ou do arquivo local é conferida
/// pelo serviço com Problemas(). Os dados só mudam pelos métodos Revogar e ReceberLeitura.
/// </summary>
public sealed class Sala
{
    public static readonly string[] Faces = ["norte", "sul", "leste", "oeste"];

    [JsonInclude] public string Id { get; private set; } = "";
    [JsonInclude] public string Nome { get; private set; } = "";
    [JsonInclude] public int Andar { get; private set; }
    [JsonInclude] public string FaceSol { get; private set; } = "norte";
    [JsonInclude] public bool TemVentilador { get; private set; }
    [JsonInclude] public string? DispositivoId { get; private set; }
    [JsonInclude] public bool SensorRevogado { get; private set; }
    [JsonInclude] public NivelCalor? NivelAtual { get; private set; }
    [JsonInclude] public double? IndiceAtual { get; private set; }
    [JsonInclude, JsonConverter(typeof(DataOuVazio))] public DateTimeOffset? UltimaLeituraEm { get; private set; }
    // A API manda semLeitura=true com nivelAtual "normal" e indiceAtual 0 quando a sala nunca foi medida.
    [JsonInclude] public bool SemLeitura { get; private set; }
    // Sala de exemplo do modo local: não vai para o arquivo e não aceita gravação.
    [JsonIgnore] public bool Demonstracao { get; private set; }
    [JsonIgnore] public Leitura? Leitura => !SemLeitura && UltimaLeituraEm is { } hora && NivelAtual is { } nivel && IndiceAtual is { } indice
        ? new Leitura(indice, nivel, hora) : null;

    [JsonConstructor] private Sala() { }

    public static Sala Criar(string? id, string? nome, int andar, string? faceSol, bool temVentilador)
    {
        var erros = Validar(id, nome, andar, faceSol);
        if (erros.Count > 0) throw new CadastroInvalidoException(erros);
        return new Sala { Id = id!, Nome = nome!.Trim(), Andar = andar, FaceSol = faceSol!, TemVentilador = temVentilador, SemLeitura = true };
    }

    internal static Sala Exemplo(string id, string nome, int andar, string faceSol, Leitura leitura)
    {
        var sala = Criar(id, nome, andar, faceSol, false);
        sala.Demonstracao = true;
        sala.ReceberLeitura(leitura);
        return sala;
    }

    /// <summary>Erro por campo; vazio quando o cadastro é válido.</summary>
    public static Dictionary<string, string> Validar(string? id, string? nome, int andar, string? faceSol)
    {
        var erros = new Dictionary<string, string>();
        if (!Regex.IsMatch(id ?? "", @"\Asala-[0-9]{2}\z")) erros[nameof(Id)] = "Use sala-NN, por exemplo sala-03.";
        if (string.IsNullOrWhiteSpace(nome)) erros[nameof(Nome)] = "Informe o nome da sala.";
        if (andar is < 0 or > 20) erros[nameof(Andar)] = "O andar deve estar entre 0 e 20.";
        if (!Faces.Contains(faceSol)) erros[nameof(FaceSol)] = "Escolha norte, sul, leste ou oeste.";
        return erros;
    }

    /// <summary>Confere uma sala que veio de fora (API ou arquivo), inclusive a leitura atual.</summary>
    public Dictionary<string, string> Problemas()
    {
        var erros = Validar(Id, Nome, Andar, FaceSol);
        if (NivelAtual is { } nivel && !Enum.IsDefined(nivel)) erros[nameof(NivelAtual)] = "Nível inválido.";
        if (IndiceAtual is { } indice && !double.IsFinite(indice)) erros[nameof(IndiceAtual)] = "Índice inválido.";
        return erros;
    }

    public void Revogar() => SensorRevogado = true;

    public void ReceberLeitura(Leitura leitura)
    {
        if (!Enum.IsDefined(leitura.Nivel) || !double.IsFinite(leitura.IndiceCalor)) throw new ArgumentException("Leitura inválida.");
        NivelAtual = leitura.Nivel;
        IndiceAtual = leitura.IndiceCalor;
        UltimaLeituraEm = leitura.MedidoEm;
        SemLeitura = false;
    }
}
