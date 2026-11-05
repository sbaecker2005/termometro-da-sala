namespace TermometroDaSala.Api.Dominio.Entidades;

public class Dispositivo
{
    public string Id { get; set; } = "";
    public string AmbienteId { get; set; } = "";
    public string Segredo { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public DateTimeOffset? UltimoMedidoEm { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
    public ICollection<Leitura> Leituras { get; set; } = new List<Leitura>();
}
