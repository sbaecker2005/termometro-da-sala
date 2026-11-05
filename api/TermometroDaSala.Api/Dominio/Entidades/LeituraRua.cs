namespace TermometroDaSala.Api.Dominio.Entidades;

public class LeituraRua
{
    public long Id { get; set; }
    public DateTimeOffset MedidoEm { get; set; }
    public double TemperaturaC { get; set; }
    public int UmidadePct { get; set; }
    public double IndiceCalor { get; set; }
}
