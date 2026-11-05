namespace TermometroDaSala.Desktop;

/// <summary>Origem dos dados da tela: a API (ServicoSalasApi) ou o arquivo deste computador (ServicoSalasLocal).</summary>
public interface IServicoSalas
{
    Task<List<Sala>> Listar();
    Task<Previsao?> Prever(string id);
    Task Cadastrar(Sala sala);
    Task Registrar(Acao acao);
    Task<List<Acao>> AcoesDoDia(DateOnly dia);
    Task Revogar(Sala sala);
}
