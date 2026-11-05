using System.Globalization;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Tests;

public class VetoresTestes
{
    const string Segredo = "segredo-de-teste-sala-03";
    const string Esperado = "2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0";

    [Fact]
    public void HmacDoContrato()
    {
        var cultura = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ",";
        var anterior = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = cultura;
        try
        {
            Assert.Equal("sala-03|esp32-sala-03|33.4|58|1794331200", Assinatura.Canonico("sala-03", "esp32-sala-03", 33.4, 58, 1794331200));
            Assert.Equal(Esperado, Assinatura.Hex(Segredo, "sala-03", "esp32-sala-03", 33.4, 58, 1794331200));
        }
        finally
        {
            CultureInfo.CurrentCulture = anterior;
        }
    }

    [Fact]
    public void RelogioInmet1700UtcE14hEmSaoPaulo()
    {
        var local = Relogio.HoraLocal(DateTimeOffset.FromUnixTimeSeconds(1768496400));
        Assert.Equal(2026, local.Year);
        Assert.Equal(1, local.Month);
        Assert.Equal(15, local.Day);
        Assert.Equal(14, local.Hour);
        Assert.Equal(0, local.Minute);
    }

    [Theory]
    [InlineData(24.0, 40, 23.5, "normal")]
    [InlineData(30.0, 50, 31.0, "atencao")]
    [InlineData(33.4, 58, 39.8, "cuidado")]
    [InlineData(38.0, 70, 62.5, "perigo")]
    public void IndiceDeCalor(double temp, int umidade, double esperado, string nivel)
    {
        var indice = IndiceCalor.Calcular(temp, umidade);
        Assert.Equal(esperado, indice);
        Assert.Equal(nivel, IndiceCalor.Nivel(indice));
    }

    [Fact]
    public void LimitesDaJanelaEDaFaixa()
    {
        var agora = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);
        Assert.Null(Motivo(30, 40, agora.ToUnixTimeSeconds() - 600, agora, null));
        Assert.Equal("atrasada", Motivo(30, 40, agora.ToUnixTimeSeconds() - 601, agora, null));
        Assert.Null(Motivo(30, 40, agora.ToUnixTimeSeconds() + 60, agora, null));
        Assert.Equal("atrasada", Motivo(30, 40, agora.ToUnixTimeSeconds() + 61, agora, null));
        Assert.Null(Motivo(-10, 0, agora.ToUnixTimeSeconds(), agora, null));
        Assert.Null(Motivo(60, 100, agora.ToUnixTimeSeconds(), agora, null));
        Assert.Equal("fora-da-faixa", Motivo(-10.1, 40, agora.ToUnixTimeSeconds(), agora, null));
        Assert.Equal("fora-da-faixa", Motivo(60.1, 40, agora.ToUnixTimeSeconds(), agora, null));
        Assert.Equal("fora-da-faixa", Motivo(30, -1, agora.ToUnixTimeSeconds(), agora, null));
        Assert.Equal("fora-da-faixa", Motivo(30, 101, agora.ToUnixTimeSeconds(), agora, null));
        Assert.Equal("reenvio", Motivo(30, 40, agora.ToUnixTimeSeconds(), agora, agora.ToUnixTimeSeconds()));
        Assert.Equal("dispositivo-revogado", ValidadorLeitura.Motivo("sala-03", false, Segredo, "sala-03", "esp32-sala-03", 30, 40, agora.ToUnixTimeSeconds(), Assinatura.Hex(Segredo, "sala-03", "esp32-sala-03", 30, 40, agora.ToUnixTimeSeconds()), agora, null));
        Assert.Equal("assinatura-invalida", ValidadorLeitura.Motivo("sala-03", true, Segredo, "sala-03", "esp32-sala-03", 30, 40, agora.ToUnixTimeSeconds(), "00", agora, null));
    }

    [Fact]
    public void SuspeitaNoSaltoENaRua()
    {
        Assert.False(ValidadorLeitura.Suspeita(30, 25, null));
        Assert.True(ValidadorLeitura.Suspeita(30, 24, null));
        Assert.False(ValidadorLeitura.Suspeita(23, null, 31));
        Assert.True(ValidadorLeitura.Suspeita(22, null, 31));
        Assert.False(ValidadorLeitura.Suspeita(20, null, 30));
    }

    [Fact]
    public void DiaQuenteProjetadoNaHoraLocal()
    {
        var previsor = new PrevisorRuaDemo(AppContext.BaseDirectory);
        var utc = DateTimeOffset.FromUnixTimeSeconds(1768496400);
        Assert.Equal(IndiceCalor.Calcular(33.8, 40), previsor.IndiceNoInstante(utc));
        Assert.Equal(IndiceCalor.Calcular(33.6, 40), previsor.PreverDaquiA2h(utc));
    }

    static string? Motivo(double temp, int umidade, long medidoEm, DateTimeOffset agora, long? ultimo) =>
        ValidadorLeitura.Motivo(
            "sala-03", true, Segredo, "sala-03", "esp32-sala-03", temp, umidade, medidoEm,
            Assinatura.Hex(Segredo, "sala-03", "esp32-sala-03", temp, umidade, medidoEm),
            agora, ultimo);
}
