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
            Demonstracao().GetAwaiter().GetResult();
            Telas();
            Console.WriteLine("Autoteste: 6 verificacoes passaram.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("Autoteste falhou: " + ex.Message); return 1; }
    }

    public static void Cadastro()
    {
        Exigir(Sala.Validar(null, null, 0, null).Count == 3 && Acao.ValidarDescricao(null) is not null, "Valores ausentes aceitos.");
        Exigir(Sala.Validar("sala-03", "Sala 3", 0, "oeste").Count == 0, "Cadastro válido recusado.");
        Exigir(Sala.Validar("sala-03", "Sala 3", 20, "oeste").Count == 0, "Andar 20 recusado.");
        foreach (var id in new[] { "sala-3", "sala-123", "sala-０３", "sala-03\n", "../sala-03" })
            Exigir(Sala.Validar(id, "Sala 3", 0, "oeste").ContainsKey(nameof(Sala.Id)), "ID inválido aceito.");
        Exigir(Sala.Validar("sala-03", "  ", -1, "qualquer").Count == 3, "Validação por campo incompleta.");
        Exigir(Sala.Validar("sala-03", "Sala 3", 21, "oeste").ContainsKey(nameof(Sala.Andar)), "Andar 21 aceito.");
        Exigir(Acao.ValidarDescricao("  água  ") is not null, "Descrição curta aceita.");
        Exigir(Acao.ValidarDescricao("12345") is null && Acao.ValidarDescricao(new string('a', 300)) is null,
            "Limites válidos de descrição recusados.");
        Exigir(Acao.ValidarDescricao(new string('a', 301)) is not null, "Descrição longa aceita.");

        // Encapsulamento: o código não consegue criar sala ou ação inválida, e o estado só muda pelos métodos.
        try { Sala.Criar("sala-3", " ", 0, "oeste", false); throw new InvalidOperationException("Sala inválida criada."); }
        catch (CadastroInvalidoException ex)
        { Exigir(ex.Erros.Keys.Order().SequenceEqual([nameof(Sala.Id), nameof(Sala.Nome)]), "Erros por campo ausentes na sala recusada."); }
        try { _ = new Acao("sala-03", NivelCalor.Cuidado, "ok"); throw new InvalidOperationException("Ação curta criada."); }
        catch (ArgumentException) { }
        try { _ = new Acao("sala-3", NivelCalor.Cuidado, "Abrir janelas"); throw new InvalidOperationException("Ação de sala inválida criada."); }
        catch (ArgumentException) { }
        try { _ = new Acao("sala-03", (NivelCalor)9, "Abrir janelas"); throw new InvalidOperationException("Ação de nível inválido criada."); }
        catch (ArgumentException) { }
        var sala = Sala.Criar("sala-03", "  Sala 3 ", 0, "oeste", true);
        Exigir(sala is { Nome: "Sala 3", SensorRevogado: false, SemLeitura: true, Leitura: null, Demonstracao: false }, "Sala nova com estado errado.");
        sala.Revogar();
        sala.ReceberLeitura(new Leitura(30.5, NivelCalor.Atencao, DateTimeOffset.Parse("2026-01-05T17:00:00Z")));
        Exigir(sala is { SensorRevogado: true, SemLeitura: false, Leitura: { Nivel: NivelCalor.Atencao, IndiceCalor: 30.5 } }, "Revogar ou receber leitura não mudou a sala.");
        try { sala.ReceberLeitura(new Leitura(double.NaN, NivelCalor.Normal, DateTimeOffset.UtcNow)); throw new InvalidOperationException("Leitura inválida aceita."); }
        catch (ArgumentException) { }
        Exigir(new Acao("sala-03", NivelCalor.Perigo, "  Abrir janelas  ").Descricao == "Abrir janelas", "Descrição não foi aparada.");

        // Enum em PascalCase no código e minúsculo no JSON, como a API grava.
        Exigir(JsonSerializer.Serialize(NivelCalor.Cuidado) == "\"cuidado\"" &&
            JsonSerializer.Deserialize<NivelCalor>("\"atencao\"") == NivelCalor.Atencao, "Nível fora do formato da API.");
        Exigir(NivelCalor.Cuidado.Texto() == "Cuidado extremo" && ((NivelCalor)9).Texto() == "Nível inválido", "Texto do nível incorreto.");
    }

    public static void CsvERelogio()
    {
        var hora = DateTimeOffset.Parse("2026-01-05T17:00:00Z");
        Exigir(HorarioEscola.Hora(hora).Hour == 14, "Conversão UTC incorreta.");
        Exigir(HorarioEscola.Dia(DateTimeOffset.Parse("2026-01-06T02:59:00Z")) == new DateOnly(2026, 1, 5), "Virada do dia incorreta.");
        var csv = ExportacaoCsv.Gerar([new Acao("sala-03", NivelCalor.Normal, "=1;\"teste\"\nágua", hora)]);
        Exigir(csv.Contains("\"'=1;\"\"teste\"\"\nágua\"") && csv.Contains("14:00:00 -03:00"), "CSV não protege fórmulas ou não escapa o conteúdo.");
    }

    public static async Task Persistencia()
    {
        var pasta = Path.Combine(Path.GetTempPath(), "termometro-teste-" + Guid.NewGuid());
        var caminho = Path.Combine(pasta, "dados.json");
        try
        {
            var servico = new ServicoSalasLocal(caminho);
            var sala = Sala.Criar("sala-03", "Sala 3", 0, "oeste", false);
            await servico.Cadastrar(sala);
            Exigir((await servico.Listar()).Single().Id == "sala-03", "O primeiro cadastro local deveria começar sem as salas de exemplo.");
            try { await servico.Cadastrar(sala); throw new InvalidOperationException("Duplicata aceita."); }
            catch (ArgumentException) { }
            await servico.Registrar(new Acao(sala.Id, NivelCalor.Normal, "Abrir janelas", DateTimeOffset.Parse("2026-01-06T02:59:00Z")));
            await servico.Registrar(new Acao(sala.Id, NivelCalor.Cuidado, "Trocar de sala", DateTimeOffset.Parse("2026-01-06T03:00:00Z")));
            Exigir(File.ReadAllText(caminho).Contains("\"nivel\":\"cuidado\""), "Arquivo local deveria gravar o nível em minúsculas, como a API.");
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
            try { await reaberto.Cadastrar(Sala.Criar("sala-04", "Sala 4", 0, "norte", false)); throw new InvalidOperationException("Arquivo corrompido aceito."); }
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
                "/ambientes/sala-03/previsao" => """{"ambienteId":"sala-03","previstoPara":"2026-11-06T19:10:00+00:00","indiceRuaPrevisto":34,"deltaSala":null,"indiceSalaPrevisto":null,"nivelPrevisto":"cuidado","semLeiturasRecentes":true,"demonstracao":true,"ruaDisponivel":true}""",
                "/ambientes/sala-04/previsao" => """{"ambienteId":"sala-04","previstoPara":"2026-11-06T19:10:00+00:00","indiceRuaPrevisto":0,"deltaSala":null,"indiceSalaPrevisto":31.5,"nivelPrevisto":"atencao","semLeiturasRecentes":false,"demonstracao":false,"ruaDisponivel":false}""",
                "/ambientes/sala-sem-dados/previsao" => """{"motivo":"sem-dados-para-prever"}""",
                "/ambientes/sala-proxy/previsao" => "<html>503</html>",
                "/acoes?dia=2026-11-06" => """[{"id":1,"ambienteId":"sala-03","nivel":"cuidado","descricao":"Abrir janelas","registradoEm":"2026-11-06T17:15:00+00:00"}]""",
                "/acoes" => """{"id":1}""",
                "/ambientes" => """{"id":"sala-03"}""",
                _ => "{}"
            };
            var codigo = rota switch
            {
                "/ambientes/sala-sem-dados/previsao" or "/ambientes/sala-proxy/previsao" => HttpStatusCode.ServiceUnavailable,
                "/ambientes/sala-99/previsao" => HttpStatusCode.NotFound,
                _ => HttpStatusCode.OK
            };
            return new HttpResponseMessage(codigo) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        })) { BaseAddress = new Uri("http://teste.local/") };
        var api = new ServicoSalasApi(http);
        var lista = await api.Listar();
        var sala = lista[0];
        Exigir(sala.Leitura?.Nivel == NivelCalor.Cuidado && sala.Leitura.MedidoEm == DateTimeOffset.Parse("2026-11-06T17:10:00Z"),
            "Leitura da API não mapeada.");
        Exigir(lista[1] is { SemLeitura: true, UltimaLeituraEm: null, Leitura: null, DispositivoId: null },
            "Sala sem leitura deveria aparecer como Sem leitura, não como Normal.");
        Exigir((await api.Prever(sala.Id))?.SemLeiturasRecentes == true, "Previsão sem delta recusada.");
        Exigir(await api.Prever("sala-04") is { RuaDisponivel: false, IndiceSalaPrevisto: 31.5, NivelPrevisto: NivelCalor.Atencao },
            "Previsão sem a rua (última leitura válida da sala) recusada.");
        Exigir(await api.Prever("sala-sem-dados") is null, "503 sem-dados-para-prever deveria virar 'sem previsão', não erro.");
        try { await api.Prever("sala-proxy"); throw new InvalidOperationException("503 sem motivo tratado como falta de dados."); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable) { }
        try { await api.Prever("sala-99"); throw new InvalidOperationException("Previsão de sala inexistente aceita."); }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound) { }
        await api.Cadastrar(sala);
        await api.Registrar(new Acao(sala.Id, NivelCalor.Cuidado, "Abrir janelas"));
        var acao = (await api.AcoesDoDia(new DateOnly(2026, 11, 6))).Single();
        Exigir(acao.Id == 1 && acao.Nivel == NivelCalor.Cuidado, "Ação com id numérico da API não mapeada.");
        await api.Revogar(sala);
        Exigir(chamadas.Contains("POST /dispositivos/esp32-sala-03/revogar") &&
            chamadas.Contains("GET /acoes?dia=2026-11-06") && chamadas.Contains("POST /ambientes"), "Rotas incompatíveis.");
        try { await api.Revogar(Sala.Criar(sala.Id, sala.Nome, sala.Andar, sala.FaceSol, sala.TemVentilador)); throw new InvalidOperationException("Revogação sem ID aceita."); }
        catch (ArgumentException) { }
        Exigir(ServicoSalasApi.Indisponivel(new HttpRequestException("offline")) &&
            ServicoSalasApi.Indisponivel(new HttpRequestException("erro", null, HttpStatusCode.ServiceUnavailable)) &&
            !ServicoSalasApi.Indisponivel(new HttpRequestException("inválido", null, HttpStatusCode.BadRequest)), "Política de indisponibilidade incorreta.");
    }

    public static async Task Demonstracao()
    {
        var pasta = Path.Combine(Path.GetTempPath(), "termometro-teste-" + Guid.NewGuid());
        var caminho = Path.Combine(pasta, "dados.json");
        try
        {
            // Sem arquivo (quem abre o ZIP sem a API): uma sala de exemplo por nível, tudo marcado como demonstração.
            var servico = new ServicoSalasLocal(caminho);
            var salas = await servico.Listar();
            Exigir(salas.Count == 5 && salas.All(s => s.Demonstracao && s.Leitura is not null) &&
                salas.Select(s => s.Leitura!.Nivel).Order().SequenceEqual(Enum.GetValues<NivelCalor>()), "Demonstração deveria ter uma sala por nível.");
            Exigir(salas.All(s => DateTimeOffset.UtcNow - s.UltimaLeituraEm!.Value < TimeSpan.FromMinutes(10)), "Demonstração com leitura antiga.");
            Exigir(await servico.Prever("sala-03") is { Demonstracao: true, NivelPrevisto: NivelCalor.Cuidado } previsao && previsao.Valida("sala-03"),
                "Previsão de exemplo deveria vir marcada como demonstração.");
            var hoje = HorarioEscola.Dia(DateTimeOffset.UtcNow);
            var acoes = await servico.AcoesDoDia(hoje);
            Exigir(acoes.Count == 2 && acoes.All(a => a.Descricao.StartsWith("DEMONSTRAÇÃO:")), "Ações de exemplo ausentes ou sem a marca DEMONSTRAÇÃO.");
            // A grade e o CSV mostram a descrição, então a marca vai junto para a planilha.
            Exigir(ExportacaoCsv.Gerar(acoes).Split("DEMONSTRAÇÃO:").Length == 3, "CSV da demonstração sem a marca DEMONSTRAÇÃO.");
            try { await servico.Registrar(new Acao("sala-03", NivelCalor.Cuidado, "Abrir janelas")); throw new InvalidOperationException("Ação gravada em sala de exemplo."); }
            catch (ArgumentException ex) { Exigir(ex.Message.Contains("DEMONSTRAÇÃO"), "Recusa sem dizer que é demonstração."); }
            try { await servico.Revogar(salas[0]); throw new InvalidOperationException("Revogação gravada em sala de exemplo."); }
            catch (ArgumentException) { }
            Exigir(!File.Exists(caminho), "A demonstração não pode criar o arquivo local.");

            // O primeiro cadastro de verdade troca a demonstração pelos dados deste computador, sem levar os exemplos.
            await servico.Cadastrar(Sala.Criar("sala-03", "Sala 3", 1, "oeste", false));
            Exigir((await servico.Listar()).Single() is { Id: "sala-03", Demonstracao: false, Leitura: null }, "Exemplos foram gravados junto com a sala real.");
            Exigir(await servico.Prever("sala-03") is null && (await servico.AcoesDoDia(hoje)).Count == 0,
                "Fora da demonstração o modo local não pode inventar previsão nem ações.");
            Exigir(!File.ReadAllText(caminho).Contains("exemplo", StringComparison.OrdinalIgnoreCase), "Arquivo local com dados de exemplo.");
        }
        finally { if (Directory.Exists(pasta)) Directory.Delete(pasta, true); }
    }

    /// <summary>As telas montam pelo InitializeComponent do Designer, com a legenda nas cores da grade e o ícone embutido.</summary>
    public static void Telas()
    {
        Exception? falha = null;
        // Controles do Windows Forms pedem thread STA; o xUnit roda em MTA.
        var thread = new Thread(() =>
        {
            try
            {
                var local = new ServicoSalasLocal(Path.Combine(Path.GetTempPath(), "termometro-teste-" + Guid.NewGuid(), "dados.json"));
                using var tela = new TelaCoordenacao(local, local);
                Exigir(tela.Text.StartsWith("Termômetro da Sala") && tela.Icon is { Width: > 0 }, "Tela sem título ou ícone.");
                var grade = (DataGridView)tela.Controls.Find("salas", true).Single();
                Exigir(grade.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).SequenceEqual(
                    ["Sala", "Nome", "Nível atual", "Última leitura (SP)", "Nível previsto (+2 h)", "Avisos / sensor"]), "Colunas da grade mudaram.");
                foreach (var nivel in Enum.GetValues<NivelCalor>())
                {
                    var rotulo = tela.Controls.Find("legenda" + nivel, true).Single();
                    Exigir(rotulo.BackColor == TelaCoordenacao.CorDoNivel(nivel) && rotulo.Text.StartsWith(nivel.Texto()),
                        $"Legenda de {nivel.Texto()} diferente da cor da grade.");
                }
                var botoes = tela.Controls.Find("comandos", true).Single().Controls.Cast<Control>().Select(c => c.Text);
                Exigir(botoes.SequenceEqual(["&Atualizar", "&Cadastrar sala", "Re&vogar sensor", "&Exportar dia (CSV)"]), "Comandos da tela mudaram.");

                using var cadastro = new CadastroSala();
                cadastro.Controls.Find("id", true).Single().Text = " sala-3 ";
                Exigir(!cadastro.Salvar() && cadastro.Sala is null, "Cadastro inválido aceito pela tela.");
                cadastro.Controls.Find("id", true).Single().Text = " sala-05 ";
                cadastro.Controls.Find("nome", true).Single().Text = "Sala 5";
                Exigir(cadastro.Salvar() && cadastro.Sala is { Id: "sala-05", Nome: "Sala 5", FaceSol: "norte" }, "Cadastro válido recusado pela tela.");
            }
            catch (Exception ex) { falha = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (falha is not null) throw falha;
    }

    private sealed class HttpSimulado(Func<HttpRequestMessage, Task<HttpResponseMessage>> resposta) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => resposta(request);
    }
}
