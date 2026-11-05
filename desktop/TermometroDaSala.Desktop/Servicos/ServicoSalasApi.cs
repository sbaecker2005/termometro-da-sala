using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TermometroDaSala.Desktop;

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
        if (salas.Any(s => s.Problemas().Count > 0) || salas.Select(s => s.Id).Distinct().Count() != salas.Count)
            throw new JsonException("Cadastro inválido recebido da API.");
        return salas;
    }

    // null: a API respondeu 503 "sem-dados-para-prever" (sem rua e sem leitura válida recente da sala). 404 sai como HttpRequestException.
    public async Task<Previsao?> Prever(string id)
    {
        using var resposta = await http.GetAsync($"ambientes/{Uri.EscapeDataString(id)}/previsao");
        if (resposta.StatusCode == HttpStatusCode.ServiceUnavailable && await SemDadosParaPrever(resposta)) return null;
        Conferir(resposta);
        var previsao = await resposta.Content.ReadFromJsonAsync<Previsao>(Json) ?? throw new JsonException("Resposta vazia da API.");
        if (!previsao.Valida(id)) throw new JsonException("Previsão inválida recebida da API.");
        return previsao;
    }

    // Outro 503 (proxy, API fora) continua sendo indisponibilidade.
    private static async Task<bool> SemDadosParaPrever(HttpResponseMessage resposta)
    {
        try { return (await resposta.Content.ReadFromJsonAsync<JsonObject>(Json))?["motivo"]?.GetValue<string>() == "sem-dados-para-prever"; }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or NotSupportedException) { return false; }
    }

    private async Task Enviar(string rota, object entrada)
    {
        using var resposta = await http.PostAsJsonAsync(rota, entrada, Json);
        Conferir(resposta);
    }

    public Task Cadastrar(Sala sala)
    {
        if (sala.Problemas() is { Count: > 0 } erros) throw new CadastroInvalidoException(erros);
        return Enviar("ambientes", new { sala.Id, sala.Nome, sala.Andar, sala.FaceSol, sala.TemVentilador, tipo = "sala" });
    }

    public Task Registrar(Acao acao)
    {
        acao.Conferir();
        return Enviar("acoes", new { acao.AmbienteId, acao.Nivel, acao.Descricao, acao.RegistradoEm });
    }

    public async Task<List<Acao>> AcoesDoDia(DateOnly dia)
    {
        var acoes = await Obter<List<Acao>>($"acoes?dia={dia:yyyy-MM-dd}");
        foreach (var acao in acoes) acao.Conferir();
        return acoes.Where(a => HorarioEscola.Dia(a.RegistradoEm) == dia).ToList();
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
