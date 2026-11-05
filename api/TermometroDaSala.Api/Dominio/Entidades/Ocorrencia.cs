namespace TermometroDaSala.Api.Dominio.Entidades;

public class Ocorrencia
{
    public Guid Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string CaminhoFoto { get; set; } = "";
    public DateTimeOffset CriadaEm { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
}
