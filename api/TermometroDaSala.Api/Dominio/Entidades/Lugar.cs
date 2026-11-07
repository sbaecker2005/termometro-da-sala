namespace TermometroDaSala.Api.Dominio.Entidades;

public class Lugar
{
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public string Nome { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public bool Vulneravel { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
}
