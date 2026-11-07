namespace TermometroDaSala.Api.Dominio.Entidades;

public class Dispositivo
{
    public string Id { get; set; } = "";
    public string AmbienteId { get; set; } = "";
    public string Segredo { get; set; } = "";
    public double? PosX { get; set; }
    public double? PosY { get; set; }
    public string? Rotulo { get; set; }
    public bool Ativo { get; private set; } = true;
    public DateTimeOffset? UltimoMedidoEm { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
    public ICollection<Leitura> Leituras { get; set; } = new List<Leitura>();

    // Revogar não apaga o histórico: as leituras ficam, e as novas são recusadas com dispositivo-revogado.
    public void Revogar() => Ativo = false;
}
