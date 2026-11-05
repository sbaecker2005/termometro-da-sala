using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;

namespace TermometroDaSala.Api.Tests;

[CollectionDefinition("api", DisableParallelization = true)]
public class ApiCollection { }

[Collection("api")]
public class ApiTestes
{
    const string Segredo = "segredo-de-teste-sala-03";

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
            Nivel = "cuidado",
            MedidoEm = quando,
            Origem = "dispositivo",
        });
        db.SaveChanges();

        var semeou = await c.PostAsync("/demo/semear?ambienteId=sala-03", new StringContent(""));
        Assert.Equal(HttpStatusCode.Created, semeou.StatusCode);
        var simuladas = db.Leituras.Where(l => l.Origem == "simulada").ToList();
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

    static void InserirPar(SalaDb db, DateTimeOffset quando, double sala, double rua)
    {
        db.Leituras.Add(new Leitura
        {
            AmbienteId = "sala-03",
            DispositivoId = "esp32-sala-03",
            TemperaturaC = 30,
            UmidadePct = 50,
            IndiceCalor = sala,
            Nivel = IndiceCalor.Nivel(sala),
            MedidoEm = quando,
            Origem = "manual",
        });
        db.LeiturasRua.Add(new LeituraRua { MedidoEm = quando, TemperaturaC = rua, UmidadePct = 40, IndiceCalor = rua });
    }

    static StringContent Corpo(string sala, string dispositivo, double temp, int umidade, long medidoEm, string? assinatura = null)
    {
        assinatura ??= Assinatura.Hex(Segredo, sala, dispositivo, temp, umidade, medidoEm);
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

    public HttpClient Cliente()
    {
        var cliente = CreateClient();
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
        builder.UseEnvironment(_ambiente);
        builder.UseSetting("Banco:Migrar", "false");
        builder.UseSetting("Mqtt:Desligado", "true");
        builder.UseSetting("Fotos:Pasta", _fotos);
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
