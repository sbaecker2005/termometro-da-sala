using TermometroDaSala.Desktop;
using Xunit;

namespace TermometroDaSala.Desktop.Tests;

public class DesktopTests
{
    [Fact] public void ValidaCamposELimites() => ValidacoesAutoteste.Cadastro();
    [Fact] public void ExportaCsvSeguroEConverteODia() => ValidacoesAutoteste.CsvERelogio();
    [Fact] public Task PersisteSemSobrescreverArquivoInvalido() => ValidacoesAutoteste.Persistencia();
    [Fact] public Task UsaContratoHttpEValidaRevogacao() => ValidacoesAutoteste.ContratoApi();
}
