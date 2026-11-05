namespace TermometroDaSala.Api.Dominio.Entidades;

public class Acao
{
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public Nivel Nivel { get; set; } = Nivel.normal;
    public string Descricao { get; set; } = "";
    public DateTimeOffset RegistradaEm { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
}
