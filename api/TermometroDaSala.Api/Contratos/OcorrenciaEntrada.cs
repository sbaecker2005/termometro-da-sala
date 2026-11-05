namespace TermometroDaSala.Api.Contratos;

public class OcorrenciaEntrada
{
    public string AmbienteId { get; set; } = "";
    public string Descricao { get; set; } = "";
    public IFormFile? Foto { get; set; }
}
