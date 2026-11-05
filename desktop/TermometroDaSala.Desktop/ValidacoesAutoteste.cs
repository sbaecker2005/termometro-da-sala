using System.Net;
using System.Text.Json;

namespace TermometroDaSala.Desktop;

public static class ValidacoesAutoteste
{
    private static void Exigir(bool condicao, string mensagem)
    {
        if (!condicao) throw new InvalidOperationException(mensagem);
    }

    public static int Executar()
    {
        try
        {
            Cadastro();
            CsvERelogio();
            Persistencia().GetAwaiter().GetResult();
            ContratoApi().GetAwaiter().GetResult();
            Console.WriteLine("Autoteste: 4 verificacoes passaram.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("Autoteste falhou: " + ex.Message); return 1; }
    }

    public static void Cadastro()
    {
        Exigir(Validacoes.Sala(null).Count > 0 && Validacoes.Descricao(null) is not null, "Valores ausentes aceitos.");
        var sala = new Sala { Id = "sala-03", Nome = "Sala 3", Andar = 0, FaceSol = "oeste" };
        Exigir(Validacoes.Sala(sala).Count == 0, "Cadastro válido recusado.");
        sala.Andar = 20;
        Exigir(Validacoes.Sala(sala).Count == 0, "Andar 20 recusado.");
        foreach (var id in new[] { "sala-3", "sala-123", "sala-０３", "sala-03\n", "../sala-03" })
        {
            sala.Id = id;
            Exigir(Validacoes.Sala(sala).ContainsKey(nameof(Sala.Id)), "ID inválido aceito.");
        }
        sala.Id = "sala-03";
        sala.Nome = "  "; sala.Andar = -1; sala.FaceSol = "qualquer";
        Exigir(Validacoes.Sala(sala).Count == 3, "Validação por campo incompleta.");
        sala.Andar = 21;
        Exigir(Validacoes.Sala(sala).ContainsKey(nameof(Sala.Andar)), "Andar 21 aceito.");
        Exigir(Validacoes.Descricao("  água  ") is not null, "Descrição curta aceita.");
        Exigir(Validacoes.Descricao("12345") is null && Validacoes.Descricao(new string('a', 300)) is null,
            "Limites válidos de descrição recusados.");
        Exigir(Validacoes.Descricao(new string('a', 301)) is not null, "Descrição longa aceita.");
    }

    public static void CsvERelogio()
    {
        var hora = DateTimeOffset.Parse("2026-01-05T17:00:00Z");
        Exigir(Validacoes.HoraEscola(hora).Hour == 14, "Conversão UTC incorreta.");
        Exigir(Validacoes.DiaEscola(DateTimeOffset.Parse("2026-01-06T02:59:00Z")) == new DateOnly(2026, 1, 5), "Virada do dia incorreta.");
        var csv = ExportacaoCsv.Gerar([new Acao { AmbienteId = "sala-03", Descricao = "=1;\"teste\"\nágua", RegistradoEm = hora }]);
        Exigir(csv.Contains("\"'=1;\"\"teste\"\"\nágua\"") && csv.Contains("14:00:00 -03:00"), "CSV não protege fórmulas ou não escapa o conteúdo.");
    }

    public static async Task Persistencia()
    {
        var pasta = Path.Combine(Path.GetTempPath(), "termometro-teste-" + Guid.NewGuid());
        var caminho = Path.Combine(pasta, "dados.json");
        try
        {
            var servico = new ServicoSalasLocal(caminho);
            Exigir((await servico.Listar()).Count == 0, "Modo local deveria iniciar vazio.");
            var sala = new Sala { Id = "sala-03", Nome = "Sala 3", FaceSol = "oeste" };
            await servico.Cadastrar(sala);
            try { await servico.Cadastrar(sala); throw new InvalidOperationException("Duplicata aceita."); }
            catch (ArgumentException) { }
            await servico.Registrar(new Acao { AmbienteId = sala.Id, Descricao = "Abrir janelas", RegistradoEm = DateTimeOffset.Parse("2026-01-06T02:59:00Z") });
            await servico.Registrar(new Acao { AmbienteId = sala.Id, Descricao = "Trocar de sala", RegistradoEm = DateTimeOffset.Parse("2026-01-06T03:00:00Z") });
            await servico.Revogar(sala);
            var reaberto = new ServicoSalasLocal(caminho);
            Exigir((await reaberto.Listar()).Single().SensorRevogado, "Revogação não persistiu.");
            Exigir((await reaberto.AcoesDoDia(new DateOnly(2026, 1, 5))).Single() is { Id: 1, Descricao: "Abrir janelas" }, "Filtro do dia incorreto.");
            Exigir((await reaberto.AcoesDoDia(new DateOnly(2026, 1, 6))).Single().Id == 2, "Modo local não numerou as ações.");
            Exigir((await reaberto.Listar()).Single().Leitura is null, "Modo local inventou leitura.");
            Exigir(await reaberto.Prever(sala.Id) is null, "Modo local inventou previsão.");
            // Arquivo da versão 0.1.0: ação com id GUID em texto continua legível e ganha id numérico.
            File.WriteAllText(caminho, """{"salas":[{"id":"sala-03","nome":"Sala 3","andar":0,"faceSol":"oeste"}],"acoes":[{"id":"3f2b8c1e-0000-4000-8000-000000000001","ambienteId":"sala-03","nivel":"cuidado","descricao":"Abrir janelas","registradoEm":"2026-01-05T17:00:00+00:00"},{"id":7,"ambienteId":"sala-03","nivel":"cuidado","descricao":"Trocar de sala","registradoEm":"2026-01-05T18:00:00+00:00"}]}""");
            var antigo = new ServicoSalasLocal(caminho);
            Exigir((await antigo.Listar()).Single().Id == "sala-03", "Arquivo da versão anterior não abriu.");
            var acoesAntigas = await antigo.AcoesDoDia(new DateOnly(2026, 1, 5));
            Exigir(acoesAntigas.Count == 2 && acoesAntigas.Any(a => a is { Id: 8, Descricao: "Abrir janelas" }) && acoesAntigas.Any(a => a.Id == 7),
                "Ação antiga não foi convertida preservando o registro.");
            File.WriteAllText(caminho, """{"salas":[],"acoes":[{"id":"1","ambienteId":"sala-03","nivel":"cuidado","descricao":"Abrir janelas","registradoEm":"2026-01-05T17:00:00+00:00"},{"id":"3f2b8c1e-0000-4000-8000-000000000002","ambienteId":"sala-03","nivel":"cuidado","descricao":"Ligar ventilador","registradoEm":"2026-01-05T18:00:00+00:00"}]}""");
            var ids = (await new ServicoSalasLocal(caminho).AcoesDoDia(new DateOnly(2026, 1, 5))).Select(a => a.Id).Order().ToArray();
            Exigir(ids.SequenceEqual(new long[] { 1, 2 }), "Id numérico em texto colidiu com id convertido.");
            File.WriteAllText(caminho, "arquivo corrompido");
            try { await reaberto.Cadastrar(new Sala { Id = "sala-04", Nome = "Sala 4" }); throw new InvalidOperationException("Arquivo corrompido aceito."); }
            catch (JsonException) { }
            Exigir(File.ReadAllText(caminho) == "arquivo corrompido", "Arquivo corrompido sobrescrito.");
        }
        finally { if (Directory.Exists(pasta)) Directory.Delete(pasta, true); }
    }

    public static async Task ContratoApi()
    {
        var chamadas = new List<string>();
        using var http = new HttpClient(new HttpSimulado(async request =>
        {
            var rota = request.RequestUri!.PathAndQuery;
            chamadas.Add(request.Method + " " + rota);
            if (request.Method == HttpMethod.Post && rota == "/acoes")
            {
                using var corpo = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                Exigir(corpo.RootElement.GetProperty("nivel").GetString() == "cuidado", "Nível da API incompatível.");
                Exigir(corpo.RootElement.GetProperty("ambienteId").GetString() == "sala-03", "Sala ausente na ação.");
                Exigir(corpo.RootElement.GetProperty("registradoEm").GetString()!.EndsWith("+00:00"), "Gravação fora de UTC.");
            }
            // Respostas copiadas da API: AmbienteSaida, AcaoSaida e PrevisaoSaida (api/TermometroDaSala.Api/Contratos/Saidas.cs)
            // serializadas com as mesmas opções do ASP.NET Core (JsonSerializerDefaults.Web), inclusive o "\u002B" do fuso.
            var json = rota switch
            {
                "/ambientes" when request.Method == HttpMethod.Get => """[{"id":"sala-03","nome":"Sala 3","andar":1,"faceSol":"oeste","nivelAtual":"cuidado","indiceAtual":34.2,"ultimaLeituraEm":"2026-11-06T17:10:00.0000000\u002B00:00","semLeitura":false,"temVentilador":true,"dispositivoId":"esp32-sala-03"},{"id":"sala-04","nome":"Sala 4","andar":2,"faceSol":"norte","nivelAtual":"normal","indiceAtual":0,"ultimaLeituraEm":"","semLeitura":true,"temVentilador":false,"dispositivoId":null}]""",
                "/ambientes/sala-03/previsao" => """{"ambienteId":"sala-03","previstoPara":"2026-11-06T19:10:00+00:00","indiceRuaPrevisto":34,"deltaSala":null,"indiceSalaPrevisto":null,"nivelPrevisto":"cuidado","semLeiturasRecentes":true,"demonstracao":true}""",
                "/acoes?dia=2026-11-06" => """[{"id":1,"ambienteId":"sala-03","nivel":"cuidado","descricao":"Abrir janelas","registradoEm":"2026-11-06T17:15:00+00:00"}]""",
                "/acoes" => """{"id":1}""",
                "/ambientes" => """{"id":"sala-03"}""",
                _ => "{}"
            };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        })) { BaseAddress = new Uri("http://teste.local/") };
        var api = new ServicoSalasApi(http);
        var lista = await api.Listar();
        var sala = lista[0];
        Exigir(sala.Leitura?.Nivel == NivelCalor.cuidado && sala.Leitura.MedidoEm == DateTimeOffset.Parse("2026-11-06T17:10:00Z"),
            "Leitura da API não mapeada.");
        Exigir(lista[1] is { SemLeitura: true, UltimaLeituraEm: null, Leitura: null, DispositivoId: null },
            "Sala sem leitura deveria aparecer como Sem leitura, não como Normal.");
        Exigir((await api.Prever(sala.Id))?.SemLeiturasRecentes == true, "Previsão sem delta recusada.");
        await api.Cadastrar(sala);
        await api.Registrar(new Acao { AmbienteId = sala.Id, Nivel = NivelCalor.cuidado, Descricao = "Abrir janelas" });
        var acao = (await api.AcoesDoDia(new DateOnly(2026, 11, 6))).Single();
        Exigir(acao.Id == 1 && acao.Nivel == NivelCalor.cuidado, "Ação com id numérico da API não mapeada.");
        await api.Revogar(sala);
        Exigir(chamadas.Contains("POST /dispositivos/esp32-sala-03/revogar") &&
            chamadas.Contains("GET /acoes?dia=2026-11-06") && chamadas.Contains("POST /ambientes"), "Rotas incompatíveis.");
        try { await api.Revogar(new Sala { Id = sala.Id }); throw new InvalidOperationException("Revogação sem ID aceita."); }
        catch (ArgumentException) { }
        Exigir(ServicoSalasApi.Indisponivel(new HttpRequestException("offline")) &&
            ServicoSalasApi.Indisponivel(new HttpRequestException("erro", null, HttpStatusCode.ServiceUnavailable)) &&
            !ServicoSalasApi.Indisponivel(new HttpRequestException("inválido", null, HttpStatusCode.BadRequest)), "Política de indisponibilidade incorreta.");
    }

    private sealed class HttpSimulado(Func<HttpRequestMessage, Task<HttpResponseMessage>> resposta) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => resposta(request);
    }
}
