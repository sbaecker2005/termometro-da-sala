using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Filtros;

namespace TermometroDaSala.Api.Tests;

[CollectionDefinition("api", DisableParallelization = true)]
public class ApiCollection { }

[Collection("api")]
public class ApiTestes
{
    [Fact]
    public async Task RecusasELeituraAceita()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        var agora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Assert.Equal("assinatura-invalida", await Motivo(await c.PostAsync("/leituras", Corpo("sala-03", "esp32-sala-03", 30, 40, agora, "00"))));

        var ok = await c.PostAsync("/leituras", Corpo("sala-03", "esp32-sala-03", 30, 40, agora));
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);

        Assert.Equal("reenvio", await Motivo(await c.PostAsync("/leituras", Corpo("sala-03", "esp32-sala-03", 30, 40, agora))));

        var salto = await c.PostAsync("/leituras", Corpo("sala-03", "esp32-sala-03", 36, 40, agora + 5));
        Assert.Equal(HttpStatusCode.Created, salto.StatusCode);
        using (var doc = JsonDocument.Parse(await salto.Content.ReadAsStringAsync()))
            Assert.True(doc.RootElement.GetProperty("suspeita").GetBoolean());

        Assert.Equal(HttpStatusCode.OK, (await c.PostAsync("/dispositivos/esp32-sala-01/revogar", new StringContent(""))).StatusCode);
        Assert.Equal("dispositivo-revogado", await Motivo(await c.PostAsync("/leituras", Corpo("sala-01", "esp32-sala-01", 30, 40, agora))));
        Assert.Equal("atrasada", await Motivo(await c.PostAsync("/leituras", Corpo("sala-02", "esp32-sala-02", 30, 40, agora - 11 * 60))));
        Assert.Equal("fora-da-faixa", await Motivo(await c.PostAsync("/leituras", Corpo("sala-04", "esp32-sala-04", 70, 40, agora))));

        using var salas = JsonDocument.Parse(await (await c.GetAsync("/ambientes")).Content.ReadAsStringAsync());
        var sala3 = salas.RootElement.EnumerateArray().Single(s => s.GetProperty("id").GetString() == "sala-03");
        Assert.Equal("oeste", sala3.GetProperty("faceSol").GetString());
        Assert.Equal(1, sala3.GetProperty("andar").GetInt32());
        Assert.Equal(IndiceCalor.Nivel(IndiceCalor.Calcular(36, 40)), sala3.GetProperty("nivelAtual").GetString());
    }

    [Fact]
    public async Task DeltaSemTresParesENuloEComTresFechaOExemplo()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        using var escopo = f.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<SalaDb>();
        var t0 = DateTimeOffset.UtcNow.AddMinutes(-20);
        InserirPar(db, t0, 36, 32);
        InserirPar(db, t0.AddMinutes(1), 37, 33);
        db.SaveChanges();

        using (var doc = JsonDocument.Parse(await (await c.GetAsync("/ambientes/sala-03/previsao")).Content.ReadAsStringAsync()))
        {
            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("deltaSala").ValueKind);
            Assert.True(doc.RootElement.GetProperty("semLeiturasRecentes").GetBoolean());
            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("indiceSalaPrevisto").ValueKind);
        }

        InserirPar(db, t0.AddMinutes(2), 37, 33);
        db.SaveChanges();
        using (var doc = JsonDocument.Parse(await (await c.GetAsync("/ambientes/sala-03/previsao")).Content.ReadAsStringAsync()))
        {
            Assert.Equal(4.0, doc.RootElement.GetProperty("deltaSala").GetDouble());
            Assert.False(doc.RootElement.GetProperty("semLeiturasRecentes").GetBoolean());
        }
    }

    [Fact]
    public async Task LeiturasEmCsv()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        var agora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/leituras", Corpo("sala-03", "esp32-sala-03", 30, 40, agora))).StatusCode);
        var csv = await (await c.GetAsync("/ambientes/sala-03/leituras?formato=csv")).Content.ReadAsStringAsync();
        Assert.StartsWith("medidoEm,temperaturaC,umidadePct,indiceCalor,nivel,origem,suspeita", csv);
        Assert.Contains("dispositivo", csv, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/ambientes/nao-existe/leituras")).StatusCode);
    }

    [Fact]
    public async Task DemoSemearForaDoAmbienteDemoResponde404()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        Assert.Equal(HttpStatusCode.NotFound, (await c.PostAsync("/demo/semear?ambienteId=sala-03", new StringContent(""))).StatusCode);
    }

    [Fact]
    public async Task DemoSemearExigeLeituraAoVivoEAncoraNela()
    {
        using var f = new ApiFabrica("Demo");
        var c = f.Cliente();
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsync("/demo/semear?ambienteId=sala-03", new StringContent(""))).StatusCode);

        using var escopo = f.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<SalaDb>();
        var quando = DateTimeOffset.UtcNow;
        db.Leituras.Add(new Leitura
        {
            AmbienteId = "sala-03",
            DispositivoId = "esp32-sala-03",
            TemperaturaC = 33.4,
            UmidadePct = 58,
            IndiceCalor = IndiceCalor.Calcular(33.4, 58),
            Nivel = Nivel.cuidado,
            MedidoEm = quando,
            Origem = Origem.dispositivo,
        });
        db.SaveChanges();

        var semeou = await c.PostAsync("/demo/semear?ambienteId=sala-03", new StringContent(""));
        Assert.Equal(HttpStatusCode.Created, semeou.StatusCode);
        var simuladas = db.Leituras.Where(l => l.Origem == Origem.simulada).ToList();
        Assert.Equal(18, simuladas.Count); // demo/leituras-simuladas.csv: 3 h a cada 10 min (gerador.py)
        var ancora = simuladas.OrderBy(l => l.MedidoEm).Last();
        Assert.Equal(33.4, ancora.TemperaturaC, 1);
        Assert.Equal(58, ancora.UmidadePct);
        Assert.Equal(quando.AddMinutes(-10), ancora.MedidoEm);
    }

    [Fact]
    public async Task UploadRecusaOQueNaoEJpegENomeComDoisPontos()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsync("/ocorrencias", Form("sala-03", "Sala muito quente no fim da aula", "nota.txt", "text/plain", Encoding.UTF8.GetBytes("oi")))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsync("/ocorrencias", Form("sala-03", "Sala muito quente no fim da aula", "../foto.jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }))).StatusCode);
        var travessia = await c.PostAsync("/ocorrencias", Form("sala-03", "Sala muito quente no fim da aula", "../x.jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }));
        Assert.Equal(HttpStatusCode.BadRequest, travessia.StatusCode);
        Assert.Contains("nome-invalido", await travessia.Content.ReadAsStringAsync());

        var ok = await c.PostAsync("/ocorrencias", Form("sala-03", "Sala muito quente no fim da aula", "foto.jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }));
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
        using var doc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync());
        var id = doc.RootElement.GetProperty("id").GetString();
        var foto = await c.GetAsync($"/ocorrencias/{id}/foto");
        Assert.Equal(HttpStatusCode.OK, foto.StatusCode);
        var bytes = await foto.Content.ReadAsByteArrayAsync();
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xD8, bytes[1]);
    }

    [Fact]
    public async Task RegistraAcao()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        var corpo = new StringContent("""{"ambienteId":"sala-03","nivel":"cuidado","descricao":"pausa para água"}""", Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/acoes", corpo)).StatusCode);
    }

    [Fact]
    public async Task CadastraSalaSemCriarSensorERecusaDuplicata()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        var entrada = new { id = "sala-05", nome = " Sala 5 ", andar = 2, faceSol = "sul", temVentilador = false };
        var resposta = await c.PostAsJsonAsync("/ambientes", entrada);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var criado = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        Assert.Equal("sala-05", criado.RootElement.GetProperty("id").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsJsonAsync("/ambientes", entrada)).StatusCode);

        using var salas = JsonDocument.Parse(await c.GetStringAsync("/ambientes"));
        var nova = salas.RootElement.EnumerateArray().Single(s => s.GetProperty("id").GetString() == "sala-05");
        Assert.Equal("Sala 5", nova.GetProperty("nome").GetString());
        Assert.Equal(2, nova.GetProperty("andar").GetInt32());
        Assert.Equal("sul", nova.GetProperty("faceSol").GetString());
        Assert.False(nova.GetProperty("temVentilador").GetBoolean());
        Assert.True(nova.GetProperty("semLeitura").GetBoolean());
        Assert.Equal(JsonValueKind.Null, nova.GetProperty("dispositivoId").ValueKind);
        var existente = salas.RootElement.EnumerateArray().Single(s => s.GetProperty("id").GetString() == "sala-03");
        Assert.True(existente.GetProperty("temVentilador").GetBoolean());
        Assert.Equal("esp32-sala-03", existente.GetProperty("dispositivoId").GetString());
        Assert.DoesNotContain("segredo", salas.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("invalido", "Sala", 0, "leste")]
    [InlineData("sala-05", " ", 0, "leste")]
    [InlineData("sala-05", "Sala", -1, "leste")]
    [InlineData("sala-05", "Sala", 21, "leste")]
    [InlineData("sala-05", "Sala", 0, "0")]
    [InlineData("sala-05", "Sala", 0, "desconhecida")]
    public async Task CadastroRecusaEntradaInvalida(string id, string nome, int andar, string faceSol)
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/ambientes", new { id, nome, andar, faceSol })).StatusCode);
        using var escopo = f.Services.CreateScope();
        Assert.Equal(4, escopo.ServiceProvider.GetRequiredService<SalaDb>().Ambientes.Count());
    }

    [Fact]
    public async Task AcoesDoDiaUsamLimitesDeSaoPauloEContratoDoDesktop()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        using var escopo = f.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<SalaDb>();
        var inicio = new DateTimeOffset(2026, 11, 5, 3, 0, 0, TimeSpan.Zero);
        foreach (var instante in new[] { inicio.AddTicks(-1), inicio, inicio.AddDays(1).AddTicks(-1), inicio.AddDays(1) })
            db.Acoes.Add(new Acao { AmbienteId = "sala-03", Nivel = Nivel.cuidado, Descricao = "pausa para água", RegistradaEm = instante });
        await db.SaveChangesAsync();

        var acoes = (await c.GetFromJsonAsync<AcaoSaida[]>("/acoes?dia=2026-11-05"))!;
        Assert.Equal(2, acoes.Length);
        Assert.Equal(inicio.AddDays(1).AddTicks(-1), acoes[0].RegistradoEm);
        Assert.Equal(inicio, acoes[1].RegistradoEm);
        Assert.All(acoes, a => { Assert.Equal("cuidado", a.Nivel); Assert.Equal("sala-03", a.AmbienteId); });
        Assert.Empty((await c.GetFromJsonAsync<AcaoSaida[]>("/acoes?dia=2026-11-10"))!);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.GetAsync("/acoes?dia=2026-02-30")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.GetAsync("/acoes?dia=05-11-2026")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.GetAsync("/acoes")).StatusCode);
    }

    [Fact]
    public async Task SegredoDeOutraSalaNaoAssina()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        var agora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var alheio = Assinatura.Hex("segredo-de-teste-sala-03", "sala-01", "esp32-sala-01", 30, 40, agora);
        Assert.Equal("assinatura-invalida", await Motivo(await c.PostAsync("/leituras", Corpo("sala-01", "esp32-sala-01", 30, 40, agora, alheio))));
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/leituras", Corpo("sala-01", "esp32-sala-01", 30, 40, agora))).StatusCode);
    }

    [Fact]
    public async Task CoordenacaoSemChaveNaoGravaELeituraContinuaComHmac()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente(false);
        var agora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        StringContent Acao() => new("""{"ambienteId":"sala-03","nivel":"cuidado","descricao":"pausa para água"}""", Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsync("/acoes", Acao())).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsJsonAsync("/ambientes", new { id = "sala-05", nome = "Sala 5", andar = 1, faceSol = "oeste", temVentilador = true })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsync("/dispositivos/esp32-sala-01/revogar", new StringContent(""))).StatusCode);
        c.DefaultRequestHeaders.Add(ExigeChaveCoordenacaoAttribute.Cabecalho, "outra");
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsync("/acoes", Acao())).StatusCode);

        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/leituras", Corpo("sala-03", "esp32-sala-03", 30, 40, agora))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/ocorrencias", Form("sala-03", "Sala muito quente no fim da aula", "foto.jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }))).StatusCode);

        using var escopo = f.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<SalaDb>();
        Assert.Equal(0, db.Acoes.Count());
        Assert.Equal(4, db.Ambientes.Count());
        Assert.True(db.Dispositivos.Single(d => d.Id == "esp32-sala-01").Ativo);
    }

    [Fact]
    public async Task EscritaAcimaDeTrintaPorMinutoResponde429()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        for (var i = 0; i < 40; i++)
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/ambientes")).StatusCode);

        HttpStatusCode primeiro = 0, ultimo = 0;
        for (var i = 0; i < 31; i++)
        {
            ultimo = (await c.PostAsync("/leituras", new StringContent("", Encoding.UTF8, "application/json"))).StatusCode;
            if (i == 0) primeiro = ultimo;
        }
        Assert.NotEqual(HttpStatusCode.TooManyRequests, primeiro);
        Assert.Equal(HttpStatusCode.TooManyRequests, ultimo);
    }

    [Fact]
    public async Task UploadRemoveSegmentoApp1()
    {
        var app1 = Encoding.ASCII.GetBytes("EXIF\0\0GPS-TESTE");
        using var montagem = new MemoryStream();
        montagem.Write(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x06, (byte)'J', (byte)'F', (byte)'I', (byte)'F' });
        montagem.Write(new byte[] { 0xFF, 0xE1, (byte)((app1.Length + 2) >> 8), (byte)(app1.Length + 2) });
        montagem.Write(app1);
        montagem.Write(new byte[] { 0xFF, 0xDA, 0x00, 0x02, 0x01, 0xFF, 0xD9 });
        var jpeg = montagem.ToArray();

        using var f = new ApiFabrica();
        var c = f.Cliente();
        var ok = await c.PostAsync("/ocorrencias", Form("sala-03", "Sala muito quente no fim da aula", "foto.jpg", "image/jpeg", jpeg));
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
        using var doc = JsonDocument.Parse(await ok.Content.ReadAsStringAsync());
        var bytes = await (await c.GetAsync($"/ocorrencias/{doc.RootElement.GetProperty("id").GetString()}/foto")).Content.ReadAsByteArrayAsync();
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xD8, bytes[1]);
        Assert.Contains("JFIF", Encoding.ASCII.GetString(bytes));
        Assert.DoesNotContain("GPS-TESTE", Encoding.ASCII.GetString(bytes));
        Assert.False(Tem(bytes, [0xFF, 0xE1]));
    }

    [Fact]
    public async Task ConsultaDeLeiturasParametrizada()
    {
        using var f = new ApiFabrica();
        var c = f.Cliente();
        var agora = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/leituras", Corpo("sala-03", "esp32-sala-03", 30, 40, agora))).StatusCode);

        using var escopo = f.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<SalaDb>();
        // variavel (como o id da rota no controller): o EF manda como parametro, nunca no texto do SQL
        var injecao = "sala-03' OR 1=1 --";
        var sql = db.Leituras.Where(l => l.AmbienteId == injecao).ToQueryString();
        var where = sql.Split('\n').Last(l => l.Contains("AmbienteId", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("@", where);
        Assert.DoesNotContain("OR 1=1", where);

        var opcoesSql = new DbContextOptionsBuilder<SalaDb>().UseSqlServer("Server=localhost;Database=TermometroDaSala").Options;
        using var sqlServer = new SalaDb(opcoesSql);
        var comando = sqlServer.Leituras.Where(l => l.AmbienteId == injecao).ToQueryString();
        var whereSql = comando.Split('\n').Last(l => l.Contains("AmbienteId", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("@", whereSql);
        Assert.DoesNotContain("OR 1=1", whereSql);

        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/ambientes/" + Uri.EscapeDataString(injecao) + "/leituras")).StatusCode);
        Assert.Equal(1, db.Leituras.Count(l => l.AmbienteId == "sala-03"));
    }

    [Fact]
    public void FactoryUsaConnectionStringDoAmbiente()
    {
        const string chave = "ConnectionStrings__Sala";
        const string conexao = "Server=(localdb)\\MSSQLLocalDB;Database=FactoryConfigTest;Integrated Security=True";
        var anterior = Environment.GetEnvironmentVariable(chave);
        try
        {
            Environment.SetEnvironmentVariable(chave, conexao);
            using var db = new SalaDbFactory().CreateDbContext(Array.Empty<string>());
            Assert.Equal(conexao, db.Database.GetConnectionString());
        }
        finally { Environment.SetEnvironmentVariable(chave, anterior); }
    }

    static void InserirPar(SalaDb db, DateTimeOffset quando, double sala, double rua)
    {
        db.Leituras.Add(new Leitura
        {
            AmbienteId = "sala-03",
            DispositivoId = "esp32-sala-03",
            TemperaturaC = 30,
            UmidadePct = 50,
            IndiceCalor = sala,
            Nivel = Enum.Parse<Nivel>(IndiceCalor.Nivel(sala)),
            MedidoEm = quando,
            Origem = Origem.manual,
        });
        db.LeiturasRua.Add(new LeituraRua { MedidoEm = quando, TemperaturaC = rua, UmidadePct = 40, IndiceCalor = rua });
    }

    static bool Tem(byte[] dados, byte[] trecho)
    {
        for (var i = 0; i <= dados.Length - trecho.Length; i++)
            if (dados.AsSpan(i, trecho.Length).SequenceEqual(trecho)) return true;
        return false;
    }

    static string SegredoDe(string dispositivo) => "segredo-de-teste-" + dispositivo["esp32-".Length..];

    static StringContent Corpo(string sala, string dispositivo, double temp, int umidade, long medidoEm, string? assinatura = null)
    {
        assinatura ??= Assinatura.Hex(SegredoDe(dispositivo), sala, dispositivo, temp, umidade, medidoEm);
        var json = JsonSerializer.Serialize(new
        {
            ambienteId = sala,
            dispositivo,
            temperaturaC = temp,
            umidadePct = umidade,
            medidoEm,
            assinatura,
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    static async Task<string?> Motivo(HttpResponseMessage resposta)
    {
        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("motivo").GetString();
    }

    static MultipartFormDataContent Form(string sala, string descricao, string nome, string tipo, byte[] bytes)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(sala), "ambienteId");
        form.Add(new StringContent(descricao), "descricao");
        var arquivo = new ByteArrayContent(bytes);
        arquivo.Headers.ContentType = new MediaTypeHeaderValue(tipo);
        form.Add(arquivo, "foto", nome);
        return form;
    }
}

public class ApiFabrica : WebApplicationFactory<Program>
{
    readonly string _ambiente;
    readonly SqliteConnection _conexao = new("Data Source=:memory:");
    readonly string _fotos;
    bool _pronto;

    public ApiFabrica(string ambiente = "Testing")
    {
        _ambiente = ambiente;
        _fotos = Path.Combine(Path.GetTempPath(), "tds-" + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable("Banco__Migrar", "false");
        Environment.SetEnvironmentVariable("Mqtt__Desligado", "true");
        Environment.SetEnvironmentVariable("Fotos__Pasta", _fotos);
        _conexao.Open();
    }

    public HttpClient Cliente(bool chave = true)
    {
        var cliente = CreateClient();
        if (chave)
            cliente.DefaultRequestHeaders.Add(ExigeChaveCoordenacaoAttribute.Cabecalho, "chave-teste");
        if (!_pronto)
        {
            using var escopo = Services.CreateScope();
            escopo.ServiceProvider.GetRequiredService<SalaDb>().Database.EnsureCreated();
            _pronto = true;
        }
        return cliente;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders().AddConsole());
        builder.UseEnvironment(_ambiente);
        builder.UseSetting("Banco:Migrar", "false");
        builder.UseSetting("Mqtt:Desligado", "true");
        builder.UseSetting("Fotos:Pasta", _fotos);
        builder.UseSetting("ChaveCoordenacao", "chave-teste");
        builder.ConfigureTestServices(services =>
        {
            services.Where(d => d.ServiceType == typeof(DbContextOptions<SalaDb>) || d.ServiceType == typeof(SalaDb)).ToList()
                .ForEach(d => services.Remove(d));
            services.AddDbContext<SalaDb>(o => o.UseSqlite(_conexao));
        });
    }

    protected override void Dispose(bool disposing)
    {
        _conexao.Dispose();
        if (Directory.Exists(_fotos)) Directory.Delete(_fotos, true);
        base.Dispose(disposing);
    }
}
