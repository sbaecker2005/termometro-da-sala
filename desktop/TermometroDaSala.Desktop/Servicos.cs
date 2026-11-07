using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TermometroDaSala.Desktop;

public interface IServicoSalas
{
    Task<List<Sala>> Listar();
    Task<Previsao?> Prever(string id);
    Task Cadastrar(Sala sala);
    Task Registrar(Acao acao);
    Task<List<Acao>> AcoesDoDia(DateOnly dia);
    Task Revogar(Sala sala);
}

public sealed class ServicoSalasApi(HttpClient http) : IServicoSalas
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static void Conferir(HttpResponseMessage resposta)
    {
        if (!resposta.IsSuccessStatusCode)
            throw new HttpRequestException($"A API respondeu {(int)resposta.StatusCode}. Nenhuma operação local foi criada automaticamente.", null, resposta.StatusCode);
    }

    private async Task<T> Obter<T>(string rota)
    {
        using var resposta = await http.GetAsync(rota);
        Conferir(resposta);
        return await resposta.Content.ReadFromJsonAsync<T>(Json) ?? throw new JsonException("Resposta vazia da API.");
    }

    public async Task<List<Sala>> Listar()
    {
        var salas = await Obter<List<Sala>>("ambientes");
        if (salas.Any(s => Validacoes.Sala(s).Count > 0 ||
            s.NivelAtual is { } n && !Enum.IsDefined(n) || s.IndiceAtual is { } i && !double.IsFinite(i)) ||
            salas.Select(s => s.Id).Distinct().Count() != salas.Count)
            throw new JsonException("Cadastro inválido recebido da API.");
        return salas;
    }

    public async Task<Previsao?> Prever(string id)
    {
        var previsao = await Obter<Previsao>($"ambientes/{Uri.EscapeDataString(id)}/previsao");
        if (previsao.AmbienteId != id || !Enum.IsDefined(previsao.NivelPrevisto) ||
            !double.IsFinite(previsao.IndiceRuaPrevisto) ||
            previsao.DeltaSala is { } d && !double.IsFinite(d) ||
            previsao.IndiceSalaPrevisto is { } i && !double.IsFinite(i) ||
            !previsao.SemLeiturasRecentes && (previsao.DeltaSala is null || previsao.IndiceSalaPrevisto is null))
            throw new JsonException("Previsão inválida recebida da API.");
        return previsao;
    }

    private async Task Enviar(string rota, object entrada)
    {
        using var resposta = await http.PostAsJsonAsync(rota, entrada, Json);
        Conferir(resposta);
    }

    public Task Cadastrar(Sala sala)
    {
        if (Validacoes.Sala(sala).Count > 0) throw new ArgumentException("Confira os campos do cadastro.");
        return Enviar("ambientes", new { sala.Id, sala.Nome, sala.Andar, sala.FaceSol, sala.TemVentilador, tipo = "sala" });
    }

    public Task Registrar(Acao acao)
    {
        Validacoes.Conferir(acao);
        return Enviar("acoes", new { acao.AmbienteId, acao.Nivel, descricao = acao.Descricao.Trim(), acao.RegistradoEm });
    }

    public async Task<List<Acao>> AcoesDoDia(DateOnly dia)
    {
        var acoes = await Obter<List<Acao>>($"acoes?dia={dia:yyyy-MM-dd}");
        foreach (var acao in acoes) Validacoes.Conferir(acao);
        return acoes.Where(a => Validacoes.DiaEscola(a.RegistradoEm) == dia).ToList();
    }

    public Task Revogar(Sala sala)
    {
        if (string.IsNullOrWhiteSpace(sala.DispositivoId))
            throw new ArgumentException("A API não informou dispositivoId para esta sala. Não é seguro adivinhar o sensor.");
        return Enviar($"dispositivos/{Uri.EscapeDataString(sala.DispositivoId)}/revogar", new { });
    }

    public static bool Indisponivel(Exception ex) => ex is TaskCanceledException || ex is HttpRequestException h &&
        (h.StatusCode is null || (int)h.StatusCode >= 500);
}

public sealed class DadosLocais
{
    public List<Sala> Salas { get; set; } = [];
    public List<Acao> Acoes { get; set; } = [];
}

public sealed class ServicoSalasLocal(string arquivo) : IServicoSalas
{
    private DadosLocais Ler()
    {
        if (!File.Exists(arquivo)) return new DadosLocais();
        var dados = JsonSerializer.Deserialize<DadosLocais>(File.ReadAllText(arquivo), ServicoSalasApi.Json)
            ?? throw new JsonException("Arquivo local vazio ou inválido. Preserve-o antes de corrigir.");
        if (dados.Salas is null || dados.Acoes is null || dados.Salas.Any(s => Validacoes.Sala(s).Count > 0) ||
            dados.Salas.Select(s => s.Id).Distinct().Count() != dados.Salas.Count)
            throw new JsonException("Dados locais inválidos. O arquivo não foi sobrescrito.");
        foreach (var acao in dados.Acoes) Validacoes.Conferir(acao);
        return dados;
    }

    private void Salvar(DadosLocais dados)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(arquivo))!);
        var temporario = arquivo + ".tmp";
        File.WriteAllText(temporario, JsonSerializer.Serialize(dados, ServicoSalasApi.Json));
        File.Move(temporario, arquivo, true);
    }

    public Task<List<Sala>> Listar() => Task.FromResult(Ler().Salas);
    public Task<Previsao?> Prever(string id) => Task.FromResult<Previsao?>(null);
    public Task Cadastrar(Sala sala)
    {
        if (Validacoes.Sala(sala).Count > 0) throw new ArgumentException("Confira os campos do cadastro.");
        var dados = Ler();
        if (dados.Salas.Any(s => s.Id == sala.Id)) throw new ArgumentException("Este identificador já está cadastrado.");
        dados.Salas.Add(sala);
        Salvar(dados);
        return Task.CompletedTask;
    }

    public Task Registrar(Acao acao)
    {
        Validacoes.Conferir(acao);
        var dados = Ler();
        if (!dados.Salas.Any(s => s.Id == acao.AmbienteId)) throw new ArgumentException("Sala não encontrada.");
        acao.Descricao = acao.Descricao.Trim();
        acao.Id = dados.Acoes.Count == 0 ? 1 : dados.Acoes.Max(a => a.Id) + 1;
        dados.Acoes.Add(acao);
        Salvar(dados);
        return Task.CompletedTask;
    }

    public Task<List<Acao>> AcoesDoDia(DateOnly dia) => Task.FromResult(
        Ler().Acoes.Where(a => Validacoes.DiaEscola(a.RegistradoEm) == dia).ToList());

    public Task Revogar(Sala sala)
    {
        var dados = Ler();
        var registro = dados.Salas.SingleOrDefault(s => s.Id == sala.Id) ?? throw new ArgumentException("Sala não encontrada.");
        registro.SensorRevogado = true;
        Salvar(dados);
        return Task.CompletedTask;
    }
}

public static class ExportacaoCsv
{
    private static string Campo(string valor)
    {
        // Planilhas interpretam fórmulas mesmo em células entre aspas.
        if (valor.TrimStart().FirstOrDefault() is '=' or '+' or '-' or '@' or '\t' or '\r' or '\n') valor = "'" + valor;
        return "\"" + valor.Replace("\"", "\"\"") + "\"";
    }

    public static string Gerar(IEnumerable<Acao> acoes)
    {
        var texto = new StringBuilder("Sala;Nível;Descrição;Hora (São Paulo)\r\n");
        foreach (var acao in acoes)
            texto.AppendLine(string.Join(";", new[] { acao.AmbienteId, Validacoes.Texto(acao.Nivel), acao.Descricao,
                Validacoes.HoraEscola(acao.RegistradoEm).ToString("yyyy-MM-dd HH:mm:ss zzz") }.Select(Campo)));
        return texto.ToString();
    }
}
