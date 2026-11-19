namespace TermometroDaSala.Api.Dominio.Entidades;

public class Ambiente
{
    public Ambiente() { }

    public Ambiente(string id, string nome, int andar, FaceSol faceSol, bool temVentilador)
    {
        Id = id;
        Alterar(nome, andar, faceSol, temVentilador);
    }

    public string Id { get; set; } = "";
    public string Nome { get; private set; } = "";
    public TipoAmbiente Tipo { get; set; } = TipoAmbiente.sala;
    public int Andar { get; private set; }
    public FaceSol FaceSol { get; private set; } = FaceSol.leste;
    public bool TemVentilador { get; private set; }
    public double? LarguraM { get; set; }
    public double? ProfundidadeM { get; set; }
    public ICollection<Lugar> Lugares { get; set; } = new List<Lugar>();
    public ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
    public ICollection<Leitura> Leituras { get; set; } = new List<Leitura>();
    public ICollection<Previsao> Previsoes { get; set; } = new List<Previsao>();
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = new List<Ocorrencia>();
    public ICollection<Acao> Acoes { get; set; } = new List<Acao>();

    // Nome, andar, face e ventilador só mudam juntos e por aqui, com as mesmas regras da entrada da API.
    public void Alterar(string nome, int andar, FaceSol faceSol, bool temVentilador)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 80)
            throw new ArgumentException("O nome da sala deve ter de 1 a 80 caracteres.", nameof(nome));
        if (andar is < 0 or > 20)
            throw new ArgumentOutOfRangeException(nameof(andar), "O andar deve ficar entre 0 e 20.");
        Nome = nome.Trim();
        Andar = andar;
        FaceSol = faceSol;
        TemVentilador = temVentilador;
    }
}
