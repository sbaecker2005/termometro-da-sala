using TermometroDaSala.Desktop;
using Xunit;

namespace TermometroDaSala.Desktop.Tests;

public class DesktopTests
{
    [Theory]
    [InlineData(17.9, "frio", NivelCalor.Frio, "Frio")]
    [InlineData(18.0, "normal", NivelCalor.Normal, "Normal")]
    [InlineData(27.0, "atencao", NivelCalor.Atencao, "Atenção")]
    public void RecebeNivelDaApiEExportaTexto(double indice, string nivel, NivelCalor esperado, string texto)
    {
        var leitura = System.Text.Json.JsonSerializer.Deserialize<Leitura>(
            System.Text.Json.JsonSerializer.Serialize(new { IndiceCalor = indice, Nivel = nivel, MedidoEm = DateTimeOffset.UtcNow }))!;
        Assert.Equal(esperado, leitura.Nivel);
        Assert.Equal(indice, leitura.IndiceCalor);
        Assert.Equal(texto, leitura.Nivel.Texto());
        Assert.Contains(texto, ExportacaoCsv.Gerar([new Acao("sala-03", leitura.Nivel, "Oferecer cobertor")]));
    }

    [Fact] public void ValidaCamposELimites() => ValidacoesAutoteste.Cadastro();
    [Fact] public void ExportaCsvSeguroEConverteODia() => ValidacoesAutoteste.CsvERelogio();
    [Fact] public Task PersisteSemSobrescreverArquivoInvalido() => ValidacoesAutoteste.Persistencia();
    [Fact] public Task UsaContratoHttpEValidaRevogacao() => ValidacoesAutoteste.ContratoApi();
    [Fact] public Task MostraDemonstracaoSemGravarExemplos() => ValidacoesAutoteste.Demonstracao();

    [Theory]
    [InlineData("2026-11-19T03:05:00Z")] // 00h05 em São Paulo
    [InlineData("2026-11-19T02:59:00Z")] // 23h59 em São Paulo
    [InlineData("2026-11-19T15:00:00Z")]
    public void AcoesDeExemploFicamNoDiaDaEscola(string instante)
    {
        var agora = DateTimeOffset.Parse(instante, System.Globalization.CultureInfo.InvariantCulture);
        var acoes = DadosDemonstracao.Gerar(agora).Acoes;
        Assert.Equal(2, acoes.Count);
        Assert.All(acoes, a => Assert.Equal(HorarioEscola.Dia(agora), HorarioEscola.Dia(a.RegistradoEm)));
        Assert.All(acoes, a => Assert.True(a.RegistradoEm <= agora));
    }
    [Fact] public void MontaTelasPeloDesignerComLegenda() => ValidacoesAutoteste.Telas();
}
