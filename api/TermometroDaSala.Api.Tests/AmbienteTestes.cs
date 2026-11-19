using TermometroDaSala.Api.Dominio.Entidades;

namespace TermometroDaSala.Api.Tests;

public class AmbienteTestes
{
    [Fact]
    public void AlterarAplicaOsQuatroCamposERecortaONome()
    {
        var sala = new Ambiente("sala-05", "Sala 5", 1, FaceSol.oeste, true);

        sala.Alterar("  Sala 5B  ", 2, FaceSol.sul, false);

        Assert.Equal(("Sala 5B", 2, FaceSol.sul, false), (sala.Nome, sala.Andar, sala.FaceSol, sala.TemVentilador));
    }

    [Theory]
    [InlineData("   ", 0)]
    [InlineData("Sala 5", -1)]
    [InlineData("Sala 5", 21)]
    public void AlterarRecusaNomeVazioOuAndarForaDaFaixaSemMudarASala(string nome, int andar)
    {
        var sala = new Ambiente("sala-05", "Sala 5", 1, FaceSol.oeste, true);

        Assert.ThrowsAny<ArgumentException>(() => sala.Alterar(nome, andar, FaceSol.sul, false));
        Assert.Equal(("Sala 5", 1, FaceSol.oeste, true), (sala.Nome, sala.Andar, sala.FaceSol, sala.TemVentilador));
    }
}
