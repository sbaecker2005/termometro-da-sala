using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Tests;

[Collection("api")]
public class MapaTestes
{
    static readonly DateTimeOffset Agora = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);
    static Ambiente Sala() => new()
    {
        Id = "sala-07", LarguraM = 2, ProfundidadeM = 1,
        Dispositivos = [new() { Id = "a", PosX = 0, PosY = 0 }, new() { Id = "b", PosX = 2, PosY = 0 }]
    };
    static Leitura Leitura(string id, double t, double indice, int minutos = 0) => new()
        { DispositivoId = id, TemperaturaC = t, IndiceCalor = indice, MedidoEm = Agora.AddMinutes(-minutos) };

    [Fact]
    public void IdwPreservaSensorEMediaNoMeioEUsaSomenteAUltimaLeitura()
    {
        var mapa = MapaTermico.Montar(Sala(), [Leitura("a", 20, 22), Leitura("b", 30, 34, 2), Leitura("a", 10, 12, 3)], Agora);
        var origem = mapa.Grade.Single(c => c.X == 0 && c.Y == 0);
        Assert.Equal(20, origem.TemperaturaC);
        Assert.Equal(22, origem.IndiceCalor);
        var meio = mapa.Grade.Single(c => c.X == 1 && c.Y == 0);
        Assert.Equal(25, meio.TemperaturaC);
        Assert.Equal(28, meio.IndiceCalor);
        Assert.Equal("atencao", meio.Nivel);
        Assert.Equal(Agora.AddMinutes(-2), mapa.LeituraMaisAntigaEm);
        Assert.Equal(-5, mapa.Pontos.Single(p => p.Id == "a").DesvioC);
    }

    [Theory]
    [InlineData(15, false)]
    [InlineData(16, true)]
    [InlineData(-1, true)]
    public void LeiturasForaDaJanelaNaoParticipam(int minutos, bool semLeitura)
    {
        var mapa = MapaTermico.Montar(Sala(), [Leitura("a", 20, 22), Leitura("b", 30, 34, minutos)], Agora);
        Assert.Equal(semLeitura, mapa.Pontos.Single(p => p.Id == "b").SemLeitura);
        Assert.Equal(semLeitura ? 20 : 25, mapa.Grade.Single(c => c.X == 1 && c.Y == 0).TemperaturaC);
    }

    [Fact]
    public void UmSensorSemPosicaoPreservaLeituraENivel()
    {
        var sala = new Ambiente { Id = "sala-01", Dispositivos = [new() { Id = "a" }] };
        var mapa = MapaTermico.Montar(sala, [Leitura("a", 16, 17)], Agora);
        Assert.Equal(16, Assert.Single(mapa.Grade).TemperaturaC);
        Assert.Equal("frio", Assert.Single(mapa.Pontos).Nivel);
        Assert.Equal(0, mapa.Pontos[0].DesvioC);
        Assert.Empty(mapa.Precaucoes);
        Assert.Empty(MapaTermico.Montar(sala, [], Agora).Precaucoes);
        Assert.Null(MapaTermico.Montar(sala, [], Agora).Grade[0].TemperaturaC);
    }

    [Theory]
    [InlineData(3.99, false)]
    [InlineData(4, true)]
    [InlineData(4.01, true)]
    public void DesigualdadeTemBordaDeQuatroGraus(double delta, bool alerta)
    {
        var mapa = MapaTermico.Montar(Sala(), [Leitura("a", 20, 20), Leitura("b", 20 + delta, 24)], Agora);
        Assert.Equal(alerta, mapa.Precaucoes.Any(p => p.Contains("ambiente desigual")));
    }

    [Theory]
    [InlineData(17.9, true, true)]
    [InlineData(18, true, false)]
    [InlineData(31.9, true, false)]
    [InlineData(32, true, true)]
    [InlineData(41, true, true)]
    [InlineData(17, false, false)]
    public void VulneravelTrocaParaLugarMaisProximoDaFaixaNormal(double indice, bool vulneravel, bool troca)
    {
        var sala = Sala();
        sala.Lugares = [new() { Nome = "Janela", X = 0, Y = 0, Vulneravel = vulneravel }, new() { Nome = "Centro", X = 2, Y = 0 }];
        var mapa = MapaTermico.Montar(sala, [Leitura("a", 20, indice), Leitura("b", 22, 23)], Agora);
        Assert.Equal(troca, mapa.Precaucoes.Any(p => p.StartsWith("trocar Janela para Centro")));
    }

    [Fact]
    public void TrocaPrefereNormalENaoSugerePiorar()
    {
        PontoMapa Lugar(string id, double indice, string nivel, bool vulneravel = false) =>
            new(id, id, "lugar", 0, 0, vulneravel, 20, indice, nivel, 0, false);
        var origem = Lugar("origem", 16, "frio", true);
        var sugestoes = MapaTermico.Precaucoes([origem, Lugar("atencao", 27, "atencao"), Lugar("normal", 26.9, "normal")]);
        Assert.StartsWith("trocar origem para normal", Assert.Single(sugestoes));
        var semMelhora = MapaTermico.Precaucoes([origem, Lugar("mais frio", 10, "frio")]);
        Assert.Contains("sem lugar mais próximo", Assert.Single(semMelhora));
    }

    [Fact]
    public async Task PutExigeChaveValidaPosicoesEPreservaDadosAoRecusar()
    {
        using var f = new ApiFabrica();
        var semChave = f.Cliente(false);
        var c = f.Cliente();
        var entrada = new MapaEntrada { LarguraM = 6, ProfundidadeM = 4,
            Sensores = [new() { Id = "esp32-sala-01", PosX = 1, PosY = 2, Rotulo = "janela" }],
            Lugares = [new() { Nome = "Cadeira", X = 1, Y = 2, Vulneravel = true }] };
        Assert.Equal(HttpStatusCode.Unauthorized, (await semChave.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        semChave.DefaultRequestHeaders.Add("X-Chave-Coordenacao", "errada");
        Assert.Equal(HttpStatusCode.Unauthorized, (await semChave.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await c.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await c.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        entrada.Sensores[0].Id = "esp32-sala-02";
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        entrada.Sensores[0].Id = "esp32-sala-01";
        entrada.Lugares[0].X = 7;
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        var mapa = await c.GetFromJsonAsync<MapaSaida>("/ambientes/sala-01/mapa");
        Assert.Equal(1, mapa!.Pontos.Single(p => p.Tipo == "lugar").X);
        Assert.Equal("janela", mapa.Pontos.Single(p => p.Tipo == "sensor").Rotulo);
        Assert.All(mapa.Pontos, p => Assert.True(p.SemLeitura));
        entrada.Lugares[0].X = 1;
        entrada.Lugares.Add(new() { Nome = "cadeira", X = 1, Y = 2 });
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        entrada.Lugares.Clear();
        entrada.Sensores[0].PosY = null;
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        entrada.Sensores.Clear();
        Assert.Equal(HttpStatusCode.NoContent, (await c.PutAsJsonAsync("/ambientes/sala-01/mapa", entrada)).StatusCode);
        mapa = await c.GetFromJsonAsync<MapaSaida>("/ambientes/sala-01/mapa");
        Assert.Single(mapa!.Pontos);
        Assert.Equal("janela", mapa.Pontos[0].Rotulo);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/ambientes/inexistente/mapa")).StatusCode);
    }

    [Theory]
    [InlineData("tarde", "janela", "perigo")]
    [InlineData("madrugada", "fundo", "frio")]
    public async Task DemoTemQuatroSensoresSeisLugaresEDoisVulneraveis(string periodo, string rotulo, string nivel)
    {
        using var f = new ApiFabrica("Demo", new RelogioFixo(Agora));
        var c = f.Cliente();
        for (var i = 0; i < 2; i++)
            Assert.Equal(HttpStatusCode.Created, (await c.PostAsync($"/demo/mapa?periodo={periodo}", null)).StatusCode);
        var mapa = (await c.GetFromJsonAsync<MapaSaida>("/ambientes/sala-07/mapa"))!;
        Assert.Equal(4, mapa.Pontos.Count(p => p.Tipo == "sensor"));
        Assert.Equal(6, mapa.Pontos.Count(p => p.Tipo == "lugar"));
        Assert.Equal(2, mapa.Pontos.Count(p => p.Vulneravel));
        Assert.Equal(nivel, mapa.Pontos.Single(p => p.Tipo == "sensor" && p.Rotulo == rotulo).Nivel);
        Assert.Contains(mapa.Precaucoes, p => p.StartsWith("trocar"));
        using var escopo = f.Services.CreateScope();
        Assert.Equal(4, escopo.ServiceProvider.GetRequiredService<SalaDb>().Leituras.Count(l => l.AmbienteId == "sala-07"));
    }

    [Fact]
    public async Task DemoForaDoModoDemoNaoExiste()
    {
        using var f = new ApiFabrica();
        Assert.Equal(HttpStatusCode.NotFound, (await f.Cliente().PostAsync("/demo/mapa", null)).StatusCode);
    }
}
