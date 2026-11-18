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
    public void PrecaucaoDoPontoPreservaOrigemMesmoComRotulosIguais()
    {
        var sala = Sala();
        sala.Lugares = [new() { Id = 1, Nome = "a", X = 0, Y = 0, Vulneravel = true },
            new() { Id = 2, Nome = "Centro", X = 2, Y = 0 }];
        var mapa = MapaTermico.Montar(sala, [Leitura("a", 16, 16), Leitura("b", 23, 23)], Agora);
        var lugar = mapa.Pontos.Single(p => p.Tipo == "lugar" && p.Id == "1");
        Assert.StartsWith("trocar a para Centro", Assert.Single(lugar.Precaucoes));
        Assert.Contains(lugar.Precaucoes[0], mapa.Precaucoes);
        Assert.StartsWith("ambiente desigual", Assert.Single(mapa.Pontos.Single(p => p.Tipo == "sensor" && p.Id == "a").Precaucoes));
        Assert.Empty(mapa.Pontos.Single(p => p.Id == "2").Precaucoes);
    }

    [Fact]
    public void PrevisaoCrescenteMudaTemperaturaNivelEGradeSemInventarSensorAusente()
    {
        var sala = Sala();
        sala.Lugares = [new() { Nome = "Cadeira", X = 1, Y = 0 }];
        var leituras = new[] { Leitura("a", 16, 16), Leitura("b", 23, 23, 16) };
        var previsao = new PrevisaoSaida(sala.Id, Agora.AddHours(2), 30, 0, 30, "atencao", false, false, true);
        var atual = MapaTermico.Montar(sala, leituras, Agora);
        var futuro = MapaTermico.Montar(sala, leituras, Agora, true, previsao);
        Assert.Null(atual.PrevistoPara);
        Assert.Equal(Agora.AddHours(1), futuro.PrevistoPara);
        Assert.Equal(atual.Pontos.Select(p => (p.Id, p.X, p.Y)), futuro.Pontos.Select(p => (p.Id, p.X, p.Y)));
        Assert.Equal(16, atual.Pontos[0].TemperaturaC);
        Assert.Equal("frio", atual.Pontos[0].Nivel);
        Assert.All(futuro.Pontos.Where(p => !p.SemLeitura), p => { Assert.Equal(23, p.TemperaturaC); Assert.Equal(23, p.IndiceCalor); Assert.Equal("normal", p.Nivel); });
        Assert.All(futuro.Grade, c => { Assert.Equal(23, c.TemperaturaC); Assert.Equal("normal", c.Nivel); });
        Assert.True(futuro.Pontos.Single(p => p.Id == "b").SemLeitura);
        Assert.Null(futuro.Pontos.Single(p => p.Id == "b").Nivel);
        Assert.StartsWith("Previsão do ambiente aplicada aos pontos", futuro.Precaucoes[0]);
        Assert.All(MapaTermico.Montar(sala, leituras, Agora, true).Pontos, p => Assert.True(p.SemLeitura));
        Assert.All(MapaTermico.Montar(sala, leituras, Agora, true, previsao with { IndiceSalaPrevisto = null }).Grade,
            c => Assert.Null(c.TemperaturaC));
    }

    [Fact]
    public void GradeCobreBordasFracionariasSemLacunas()
    {
        var sala = Sala();
        sala.LarguraM = 6.4;
        sala.ProfundidadeM = 1.3;
        var mapa = MapaTermico.Montar(sala, [Leitura("a", 20, 20)], Agora);
        Assert.Equal(56, mapa.Grade.Count);
        Assert.Single(mapa.Grade.Where(c => c.X == 6.4 && c.Y == 1.3));
        void Coberto(IEnumerable<double> eixo, double limite)
        {
            double fim = 0;
            foreach (var centro in eixo.Distinct().Order())
            {
                Assert.True(Math.Max(0, centro - mapa.PassoM / 2) <= fim);
                fim = Math.Min(limite, centro + mapa.PassoM / 2);
            }
            Assert.Equal(limite, fim);
        }
        Coberto(mapa.Grade.Select(c => c.X), 6.4);
        Coberto(mapa.Grade.Select(c => c.Y), 1.3);
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
    public async Task DemoTemQuatroSensoresVinteLugaresEDoisVulneraveis(string periodo, string rotulo, string nivel)
    {
        using var f = new ApiFabrica("Demo", new RelogioFixo(Agora));
        var c = f.Cliente();
        for (var i = 0; i < 2; i++)
            Assert.Equal(HttpStatusCode.Created, (await c.PostAsync($"/demo/mapa?periodo={periodo}", null)).StatusCode);
        var mapa = (await c.GetFromJsonAsync<MapaSaida>("/ambientes/sala-07/mapa"))!;
        Assert.Equal(4, mapa.Pontos.Count(p => p.Tipo == "sensor"));
        var lugares = mapa.Pontos.Where(p => p.Tipo == "lugar").OrderBy(p => p.Rotulo).ToArray();
        Assert.Equal(20, lugares.Length);
        for (var i = 0; i < lugares.Length; i++)
        {
            Assert.Equal($"Cadeira {i + 1:00}", lugares[i].Rotulo);
            Assert.Equal(0.75 + 1.5 * (i % 4), lugares[i].X);
            Assert.Equal(0.4 + 0.8 * (i / 4), lugares[i].Y);
            Assert.Equal(i is 8 or 19, lugares[i].Vulneravel);
        }
        Assert.Equal(2, mapa.Pontos.Count(p => p.Vulneravel));
        Assert.Equal(nivel, mapa.Pontos.Single(p => p.Tipo == "sensor" && p.Rotulo == rotulo).Nivel);
        if (periodo == "madrugada") Assert.Equal("frio", lugares[^1].Nivel);
        Assert.Contains(mapa.Precaucoes, p => p.StartsWith("trocar"));
        var futuro = (await c.GetFromJsonAsync<MapaSaida>("/ambientes/sala-07/mapa?futuro=true"))!;
        Assert.Equal(Agora.AddHours(1), futuro.PrevistoPara);
        Assert.StartsWith("Previsão do ambiente aplicada aos pontos", futuro.Precaucoes[0]);
        Assert.Equal(mapa.Pontos.Select(p => (p.Id, p.X, p.Y)), futuro.Pontos.Select(p => (p.Id, p.X, p.Y)));
        using var escopo = f.Services.CreateScope();
        var previsao = await escopo.ServiceProvider.GetRequiredService<ConsultaPrevisao>().Montar("sala-07");
        var deslocamento = (previsao!.IndiceSalaPrevisto!.Value - mapa.Pontos.Where(p => p.Tipo == "sensor").Average(p => p.IndiceCalor!.Value)) / 2;
        foreach (var p in futuro.Pontos)
            Assert.Equal(mapa.Pontos.Single(a => a.Tipo == p.Tipo && a.Id == p.Id).TemperaturaC!.Value + deslocamento, p.TemperaturaC!.Value, 8);
        var historico = escopo.ServiceProvider.GetRequiredService<SalaDb>().Leituras.Where(l => l.AmbienteId == "sala-07").ToArray();
        Assert.Equal(12, historico.Length);
        Assert.Equal(3, historico.Select(l => l.MedidoEm).Distinct().Count());
    }

    [Fact]
    public async Task DemoAtualizaSeisCadeirasOriginaisUmaVezEPreservaPersonalizacao()
    {
        using var f = new ApiFabrica("Demo", new RelogioFixo(Agora));
        var c = f.Cliente();
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/demo/mapa", null)).StatusCode);
        using (var escopo = f.Services.CreateScope())
        {
            var db = escopo.ServiceProvider.GetRequiredService<SalaDb>();
            db.Lugares.RemoveRange(db.Lugares.Where(l => l.AmbienteId == "sala-07"));
            db.Lugares.AddRange(Enumerable.Range(0, 6).Select(i => new Lugar { AmbienteId = "sala-07",
                Nome = $"Cadeira {i + 1}", X = i % 3 * 3, Y = i < 3 ? 2 : 4, Vulneravel = i is 0 or 5 }));
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/demo/mapa", null)).StatusCode);
        var atualizado = (await c.GetFromJsonAsync<MapaSaida>("/ambientes/sala-07/mapa"))!;
        Assert.Equal(20, atualizado.Pontos.Count(p => p.Tipo == "lugar"));
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsync("/demo/mapa?periodo=madrugada", null)).StatusCode);
        var repetido = (await c.GetFromJsonAsync<MapaSaida>("/ambientes/sala-07/mapa"))!;
        Assert.Equal(atualizado.Pontos.Select(p => (p.Id, p.Rotulo, p.X, p.Y)), repetido.Pontos.Select(p => (p.Id, p.Rotulo, p.X, p.Y)));
        using (var escopo = f.Services.CreateScope())
        {
            var db = escopo.ServiceProvider.GetRequiredService<SalaDb>();
            db.Lugares.First(l => l.AmbienteId == "sala-07").Nome = "Lugar personalizado";
            await db.SaveChangesAsync();
        }
        var personalizado = await c.GetStringAsync("/ambientes/sala-07/mapa");
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsync("/demo/mapa", null)).StatusCode);
        Assert.Equal(personalizado, await c.GetStringAsync("/ambientes/sala-07/mapa"));
    }

    [Fact]
    public async Task DemoForaDoModoDemoNaoExiste()
    {
        using var f = new ApiFabrica();
        Assert.Equal(HttpStatusCode.NotFound, (await f.Cliente().PostAsync("/demo/mapa", null)).StatusCode);
    }
}
