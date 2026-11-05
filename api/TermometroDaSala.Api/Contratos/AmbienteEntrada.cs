using System.ComponentModel.DataAnnotations;

namespace TermometroDaSala.Api.Contratos;

public class AmbienteEntrada
{
    [Required, RegularExpression(@"^sala-\d{2}$")]
    public string Id { get; set; } = "";
    [Required, StringLength(80)]
    public string Nome { get; set; } = "";
    [Range(0, 20)]
    public int Andar { get; set; }
    [Required, RegularExpression("^(leste|norte|oeste|sul)$")]
    public string FaceSol { get; set; } = "";
    public bool TemVentilador { get; set; }
}
