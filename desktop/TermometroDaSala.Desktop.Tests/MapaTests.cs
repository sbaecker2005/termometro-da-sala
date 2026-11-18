using System.Drawing;
using System.Text.Json;
using TermometroDaSala.Desktop;
using Xunit;

namespace TermometroDaSala.Desktop.Tests;

public class MapaTests
{
    [Fact]
    public void CodigosSeparamCadeirasESensoresMesmoNaMesmaPosicao()
    {
        var demo = Mapa.Demonstracao("sala-07", false);
        var mapa = demo with { Pontos = demo.Pontos.Take(4).ToList() };
        var sensor = mapa.Pontos[0];
        var cadeira = sensor with { Id = "cadeira-junto-janela", Tipo = "lugar", Rotulo = "Cadeira junto à janela", Precaucoes = ["Trocar de lugar."] };
        mapa.Pontos.Insert(2, cadeira);

        var identificados = mapa.PontosIdentificados().ToList();
        Assert.Equal(mapa.Pontos, identificados.Select(p => p.Ponto));
        Assert.Equal(new[] { "S1", "S2", "C01", "C02", "C03" }, identificados.Select(p => p.Codigo));
        Assert.Contains("Trocar de lugar.", identificados[2].Ponto.Texto);
        var futuro = mapa with { Pontos = mapa.Pontos.Select(p => p with { TemperaturaC = 20 }).ToList() };
        Assert.Equal(identificados.Select(p => p.Codigo), futuro.PontosIdentificados().Select(p => p.Codigo));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DesserializaPrecaucoesDoPontoNoTextoDaLista(bool semLeitura)
    {
        var mapa = JsonSerializer.Deserialize<Mapa>("""
            {"ambienteId":"sala-05","larguraM":6,"profundidadeM":4,"passoM":0.5,
             "grade":[],"precaucoes":[],"pontos":[
               {"id":"cadeira-1","rotulo":"Cadeira 1","tipo":"lugar","x":1,"y":1,
                "vulneravel":true,"temperaturaC":15,"nivel":"Frio","semLeitura":false,
                "precaucoes":["Afastar da corrente de ar.","Priorizar pessoa vulnerável."]}]}
            """, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        mapa = mapa with { Pontos = [mapa.Pontos[0] with { SemLeitura = semLeitura }] };
        mapa.Conferir("sala-05");
        var ponto = Assert.Single(mapa.Pontos);
        Assert.Equal(2, ponto.Precaucoes.Count);
        Assert.Contains("Cadeira 1 (lugar):", ponto.Texto);
        Assert.Contains(semLeitura ? "sem leitura" : "Frio", ponto.Texto);
        Assert.Contains("Afastar da corrente de ar.; Priorizar pessoa vulnerável.", ponto.Texto);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[null]")]
    public void RejeitaPrecaucoesInvalidasDoPonto(string precaucoes)
    {
        var ponto = JsonSerializer.Deserialize<PontoMapa>(
            "{\"id\":\"cadeira-1\",\"rotulo\":\"Cadeira 1\",\"tipo\":\"lugar\",\"x\":1,\"y\":1,\"precaucoes\":" + precaucoes + "}",
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var mapa = Mapa.Demonstracao("sala-05", false) with { Pontos = [ponto] };
        Assert.Throws<JsonException>(() => mapa.Conferir("sala-05"));
    }

    [Fact]
    public void CoordenadasMantemEscalaEInvertemEixoY()
    {
        var area = new RectangleF(10, 20, 600, 600);
        Assert.Equal(new PointF(10, 520), Mapa.ParaTela(0, 0, 6, 4, area));
        Assert.Equal(new PointF(610, 120), Mapa.ParaTela(6, 4, 6, 4, area));
        Assert.Equal(new PointF(310, 320), Mapa.ParaTela(0, 0, 0, 0, area));
    }

    [Fact]
    public void BolaUsaFaixaEModuloDoDesvio()
    {
        Assert.Equal(Color.LightBlue, Mapa.Cor(NivelCalor.Frio));
        Assert.Equal(Color.LightGreen, Mapa.Cor(NivelCalor.Normal));
        Assert.Equal(Color.Khaki, Mapa.Cor(NivelCalor.Atencao));
        Assert.Equal(Color.Red, Mapa.Cor(NivelCalor.Perigo));
        Assert.Equal(Color.LightGray, Mapa.Cor(null));
        Assert.Equal(24, Mapa.Diametro(-4));
        Assert.Equal(Mapa.Diametro(-4), Mapa.Diametro(4));
        Assert.Equal(36, Mapa.Diametro(100));
        Assert.Equal(12, Mapa.Diametro(null));
    }

    [Fact]
    public void DemonstracaoPreservaPontosEValidaContrato()
    {
        var agora = Mapa.Demonstracao("sala-05", false);
        var futuro = Mapa.Demonstracao("sala-05", true);
        agora.Conferir("sala-05");
        futuro.Conferir("sala-05");
        var cadeiras = agora.Pontos.Where(p => p.Tipo == "lugar").ToList();
        Assert.Equal(20, cadeiras.Count);
        Assert.Equal(Enumerable.Range(1, 20).Select(i => $"C{i:00}"),
            agora.PontosIdentificados().Where(p => p.Ponto.Tipo == "lugar").Select(p => p.Codigo));
        Assert.Equal(4, cadeiras.Select(p => p.X).Distinct().Count());
        Assert.Equal(5, cadeiras.Select(p => p.Y).Distinct().Count());
        Assert.Contains(cadeiras, p => p.Nivel == NivelCalor.Frio);
        Assert.Equal(agora.Pontos.Select(p => (p.Id, p.X, p.Y)), futuro.Pontos.Select(p => (p.Id, p.X, p.Y)));
        Assert.Equal(14, futuro.Pontos[0].TemperaturaC);
        Assert.Contains("Frio", agora.Pontos[0].Texto);
        Assert.Throws<JsonException>(() => agora.Conferir("outra"));
        Assert.Throws<JsonException>(() => (agora with { LarguraM = -1 }).Conferir("sala-05"));
    }

    [Fact]
    public async Task ConsultaContratoHttpAtualEPrevisaoDeUmaHora()
    {
        using var handler = new MapaHttp();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var servico = new ServicoSalasApi(http);
        var mapa = await servico.ObterMapa("sala-05", false);
        Assert.NotNull(mapa);
        Assert.Equal(NivelCalor.Frio, mapa.Pontos[0].Nivel);
        Assert.Equal("/ambientes/sala-05/mapa", handler.Rota);
        handler.Rota = null;
        var futuro = await servico.ObterMapa("sala-05", true);
        Assert.NotNull(futuro);
        Assert.Equal("/ambientes/sala-05/mapa?futuro=true", handler.Rota);
        Assert.Equal(14, futuro.Pontos[0].TemperaturaC);
        Assert.Equal(mapa.Pontos.Select(p => (p.Id, p.X, p.Y)), futuro.Pontos.Select(p => (p.Id, p.X, p.Y)));
    }

    [Fact]
    public async Task MapaLocalNaoGravaDemonstracao()
    {
        var arquivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var local = new ServicoSalasLocal(arquivo);
        var mapa = await local.ObterMapa("sala-05", true);
        Assert.NotNull(mapa);
        Assert.Contains(mapa.Precaucoes, p => p.StartsWith("DEMONSTRAÇÃO"));
        Assert.False(File.Exists(arquivo));
        await Assert.ThrowsAsync<ArgumentException>(() => local.ObterMapa("inexistente", false));
    }

    private sealed class MapaHttp : HttpMessageHandler
    {
        public string? Rota { get; set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Rota = request.RequestUri!.PathAndQuery;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(Mapa.Demonstracao("sala-05", request.RequestUri.Query == "?futuro=true"),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)), System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
