namespace TermometroDaSala.Desktop;

/// <summary>Conteúdo do arquivo dados.json do modo local.</summary>
public sealed class DadosLocais
{
    public List<Sala> Salas { get; set; } = [];
    public List<Acao> Acoes { get; set; } = [];
}
