namespace TermometroDaSala.Api.Dominio.Entidades;

public class Leitura
{
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public string DispositivoId { get; set; } = "";
    public double TemperaturaC { get; set; }
    public int UmidadePct { get; set; }
    public double IndiceCalor { get; set; }
    public Nivel Nivel { get; set; } = Nivel.normal;
    public DateTimeOffset MedidoEm { get; set; }
    public Origem Origem { get; set; } = Origem.dispositivo;
    public bool Suspeita { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
    public Dispositivo Dispositivo { get; set; } = null!;
}
