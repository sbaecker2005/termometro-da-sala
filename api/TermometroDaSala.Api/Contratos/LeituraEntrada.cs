namespace TermometroDaSala.Api.Contratos;

public class LeituraEntrada
{
    public string AmbienteId { get; set; } = "";
    public string Dispositivo { get; set; } = "";
    public double TemperaturaC { get; set; }
    public int UmidadePct { get; set; }
    public long MedidoEm { get; set; }
    public string Assinatura { get; set; } = "";
}

