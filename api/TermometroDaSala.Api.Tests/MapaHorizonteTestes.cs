using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Tests;

public class MapaHorizonteTestes
{
    [Theory]
    [InlineData(120, 23)]
    [InlineData(90, 24)]
    [InlineData(60, 26)]
    public void UmaHoraRespeitaOHorizonteRealDaPrevisao(int minutos, double esperado)
    {
        var agora = new DateTimeOffset(2026, 11, 17, 15, 30, 0, TimeSpan.Zero);
        var sala = new Ambiente { Id = "sala-01", LarguraM = 1, ProfundidadeM = 1,
            Dispositivos = [new() { Id = "S1", PosX = 0, PosY = 0 }] };
        var leitura = new Leitura { DispositivoId = "S1", MedidoEm = agora,
            TemperaturaC = 20, IndiceCalor = 20 };
        var previsao = new PrevisaoSaida(sala.Id, agora.AddMinutes(minutos), 26, 0, 26,
            "normal", false, false, true);
        var mapa = MapaTermico.Montar(sala, [leitura], agora, true, previsao);
        Assert.Equal(esperado, Assert.Single(mapa.Pontos).IndiceCalor);
        Assert.Equal(esperado, mapa.Pontos[0].TemperaturaC);
        Assert.Equal(agora.AddHours(1), mapa.PrevistoPara);
    }

    [Theory]
    [InlineData(59)]
    [InlineData(0)]
    [InlineData(-1)]
    public void NaoExtrapolaMapaAlemDoHorizonteDisponivel(int minutos)
    {
        var agora = new DateTimeOffset(2026, 11, 17, 15, 30, 0, TimeSpan.Zero);
        var sala = new Ambiente { Id = "sala-01", Dispositivos = [new() { Id = "S1" }] };
        var leitura = new Leitura { DispositivoId = "S1", MedidoEm = agora,
            TemperaturaC = 20, IndiceCalor = 20 };
        var previsao = new PrevisaoSaida(sala.Id, agora.AddMinutes(minutos), 26, 0, 26,
            "normal", false, false, true);
        var mapa = MapaTermico.Montar(sala, [leitura], agora, true, previsao);
        Assert.True(Assert.Single(mapa.Pontos).SemLeitura);
        Assert.Null(mapa.Pontos[0].IndiceCalor);
        Assert.StartsWith("Previsão do ambiente indisponível", mapa.Precaucoes[0]);
    }
}
