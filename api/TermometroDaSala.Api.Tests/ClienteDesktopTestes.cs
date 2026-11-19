using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TermometroDaSala.Desktop;

namespace TermometroDaSala.Api.Tests;

// Mesmo domínio e mesmos serviços usados pelo executável Windows, ligados aos
// controllers reais da API e ao SQLite de teste. Não substitui o ensaio das telas.
[Collection("api")]
public class ClienteDesktopTestes
{
    [Fact]
    public async Task CadastroSemSensorEAcoesDoDesktopPersistemNaApi()
    {
        using var fabrica = new ApiFabrica();
        using var http = fabrica.Cliente();
        var desktop = new ServicoSalasApi(http);

        await desktop.Cadastrar(Sala.Criar("sala-05", "Sala da coordenação", 1, "oeste", true));
        var sala = Assert.Single((await desktop.Listar()).Where(s => s.Id == "sala-05"));
        Assert.Equal("Sala da coordenação", sala.Nome);
        Assert.True(sala.SemLeitura);
        Assert.Null(sala.Leitura);
        Assert.Null(sala.DispositivoId);
        Assert.False(sala.SensorRevogado);
        Assert.Null(await desktop.Prever(sala.Id));

        await desktop.Registrar(new Acao(sala.Id, NivelCalor.Normal, "Abrir janelas e oferecer água"));
        var hoje = HorarioEscola.Dia(DateTimeOffset.UtcNow);
        var acao = Assert.Single(await desktop.AcoesDoDia(hoje));
        Assert.True(acao.Id > 0);
        Assert.Equal(sala.Id, acao.AmbienteId);
        Assert.Equal(NivelCalor.Normal, acao.Nivel);
        Assert.Contains("Abrir janelas", ExportacaoCsv.Gerar([acao]));
    }

    [Fact]
    public async Task SemChaveOClienteRecebe401ESalaNaoEGravada()
    {
        using var fabrica = new ApiFabrica();
        using var http = fabrica.Cliente(chave: false);
        var desktop = new ServicoSalasApi(http);

        var erro = await Assert.ThrowsAsync<HttpRequestException>(() =>
            desktop.Cadastrar(Sala.Criar("sala-05", "Sala sem autorização", 0, "sul", false)));

        Assert.Equal(HttpStatusCode.Unauthorized, erro.StatusCode);
        Assert.False(ServicoSalasApi.Indisponivel(erro));
        Assert.DoesNotContain(await desktop.Listar(), s => s.Id == "sala-05");
    }

    [Fact]
    public async Task LeituraEPrevisaoSaoLidasERevogacaoBloqueiaOSensorNaApi()
    {
        var agora = DateTimeOffset.Parse("2026-11-17T15:00:00Z");
        using var fabrica = new ApiFabrica(tempo: new RelogioFixo(agora));
        using var http = fabrica.Cliente();
        var desktop = new ServicoSalasApi(http);

        using var aceita = await http.PostAsJsonAsync("/leituras", Leitura(agora));
        Assert.Equal(HttpStatusCode.Created, aceita.StatusCode);
        var sala = Assert.Single((await desktop.Listar()).Where(s => s.Id == "sala-03"));
        Assert.False(sala.SemLeitura);
        Assert.NotNull(sala.Leitura);
        Assert.Equal("esp32-sala-03", sala.DispositivoId);
        Assert.False(sala.SensorRevogado);
        var previsao = await desktop.Prever(sala.Id);
        Assert.NotNull(previsao);
        Assert.False(previsao.RuaDisponivel);
        Assert.Equal(sala.Leitura.IndiceCalor, previsao.IndiceSalaPrevisto);

        await desktop.Revogar(sala);
        var revogada = Assert.Single((await desktop.Listar()).Where(s => s.Id == sala.Id));
        Assert.Equal(sala.DispositivoId, revogada.DispositivoId);
        Assert.True(revogada.SensorRevogado);
        using var detalhe = JsonDocument.Parse(await http.GetStringAsync($"/ambientes/{sala.Id}"));
        Assert.Equal(sala.DispositivoId, detalhe.RootElement.GetProperty("dispositivoId").GetString());
        Assert.True(detalhe.RootElement.GetProperty("sensorRevogado").GetBoolean());
        using var recusada = await http.PostAsJsonAsync("/leituras", Leitura(agora.AddSeconds(1)));
        Assert.Equal(HttpStatusCode.BadRequest, recusada.StatusCode);
        using var corpo = JsonDocument.Parse(await recusada.Content.ReadAsStringAsync());
        Assert.Equal("dispositivo-revogado", corpo.RootElement.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task DesktopLeMapaEPrecaucoesReaisNoDemoAtualEFuturo()
    {
        using var fabrica = new ApiFabrica("Demo");
        using var http = fabrica.Cliente();
        using var preparo = await http.PostAsync("/demo/mapa?periodo=tarde", null);
        Assert.Equal(HttpStatusCode.Created, preparo.StatusCode);
        var desktop = new ServicoSalasApi(http);

        var atual = await desktop.ObterMapa("sala-07", futuro: false);
        var futuro = await desktop.ObterMapa("sala-07", futuro: true);
        Assert.NotNull(atual);
        Assert.NotNull(futuro);
        Assert.Equal(24, atual.Pontos.Count);
        Assert.Equal(atual.Pontos.Select(p => (p.Id, p.X, p.Y)), futuro.Pontos.Select(p => (p.Id, p.X, p.Y)));
        var janela = Assert.Single(atual.Pontos.Where(p => p.Rotulo == "janela"));
        Assert.Equal(35, janela.TemperaturaC);
        Assert.Equal(NivelCalor.Perigo, janela.Nivel);
        Assert.NotEmpty(janela.Precaucoes);
        Assert.All(janela.Precaucoes, aviso => Assert.Contains(aviso, janela.Texto));
        Assert.NotEmpty(futuro.Grade);

        using var outroSensor = await http.PostAsJsonAsync("/dispositivos/demo-sala-07-janela/revogar", new { });
        Assert.Equal(HttpStatusCode.OK, outroSensor.StatusCode);
        var sala = Assert.Single((await desktop.Listar()).Where(s => s.Id == "sala-07"));
        Assert.Equal("demo-sala-07-centro", sala.DispositivoId);
        Assert.False(sala.SensorRevogado);
        await desktop.Revogar(sala);
        Assert.True(Assert.Single((await desktop.Listar()).Where(s => s.Id == sala.Id)).SensorRevogado);
    }

    static object Leitura(DateTimeOffset instante)
    {
        var medidoEm = instante.ToUnixTimeSeconds();
        var canonico = $"sala-03|esp32-sala-03|33.4|58|{medidoEm}";
        var assinatura = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("segredo-de-teste-sala-03"), Encoding.UTF8.GetBytes(canonico))).ToLowerInvariant();
        return new { ambienteId = "sala-03", dispositivo = "esp32-sala-03", temperaturaC = 33.4, umidadePct = 58, medidoEm, assinatura };
    }
}
