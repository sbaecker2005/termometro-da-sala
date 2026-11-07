namespace TermometroDaSala.Api.Dominio.Entidades;

public class Ambiente
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public TipoAmbiente Tipo { get; set; } = TipoAmbiente.sala;
    public int Andar { get; set; }
    public FaceSol FaceSol { get; set; } = FaceSol.leste;
    public bool TemVentilador { get; set; }
    public double? LarguraM { get; set; }
    public double? ProfundidadeM { get; set; }
    public ICollection<Lugar> Lugares { get; set; } = new List<Lugar>();
    public ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
    public ICollection<Leitura> Leituras { get; set; } = new List<Leitura>();
    public ICollection<Previsao> Previsoes { get; set; } = new List<Previsao>();
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();
    public ICollection<Acao> Acoes { get; set; } = new List<Acao>();
}
