namespace TermometroDaSala.Api.Dominio.Entidades;

public class Previsao
{
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public DateTimeOffset PrevistoPara { get; set; }
    public double IndiceRuaPrevisto { get; set; }
    public double? DeltaSala { get; set; }
    public double? IndiceSalaPrevisto { get; set; }
    public Nivel NivelPrevisto { get; set; } = Nivel.normal;
    public bool SemLeiturasRecentes { get; set; }
    public DateTimeOffset CalculadaEm { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
}
