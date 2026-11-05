namespace TermometroDaSala.Desktop;

/// <summary>Sala recusada, com a mensagem de cada campo (Id, Nome, Andar, FaceSol) para a tela marcar o controle certo.</summary>
public sealed class CadastroInvalidoException(IReadOnlyDictionary<string, string> erros)
    : ArgumentException("Confira os campos do cadastro: " + string.Join(" ", erros.Values))
{
    public IReadOnlyDictionary<string, string> Erros { get; } = erros;
}
