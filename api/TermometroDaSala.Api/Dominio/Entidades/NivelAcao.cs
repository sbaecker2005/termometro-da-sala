namespace TermometroDaSala.Api.Dominio.Entidades;

public class NivelAcao
{
    public Nivel Nivel { get; set; } = Nivel.normal;
    public string Nome { get; set; } = "";
    public string AcaoSugerida { get; set; } = "";
    public double IndiceMin { get; set; }
}
