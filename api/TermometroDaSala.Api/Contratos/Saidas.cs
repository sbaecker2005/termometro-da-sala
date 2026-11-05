namespace TermometroDaSala.Api.Contratos;

public class AmbienteSaida
{
    public string Id { get; init; } = "";
    public string Nome { get; init; } = "";
    public int Andar { get; init; }
    public string FaceSol { get; init; } = "";
    public string NivelAtual { get; init; } = "normal";
    public double IndiceAtual { get; init; }
    public string UltimaLeituraEm { get; init; } = "";
    public bool SemLeitura { get; init; }
    public bool TemVentilador { get; init; }
    public string? DispositivoId { get; init; }
}

public class LeituraSaida
{
    public DateTimeOffset MedidoEm { get; init; }
    public double TemperaturaC { get; init; }
    public int UmidadePct { get; init; }
    public double IndiceCalor { get; init; }
    public string Nivel { get; init; } = "";
    public string Origem { get; init; } = "";
    public bool Suspeita { get; init; }
    public string DispositivoId { get; init; } = "";
}

public sealed record MotivoSaida(string Motivo);
public sealed record IdSaida(string Id);
public sealed record AcaoCriadaSaida(long Id);
public sealed record LeituraCriadaSaida(long Id, bool Suspeita, double IndiceCalor, string Nivel);
public sealed record DispositivoSaida(string Id, bool Ativo);
public sealed record DemoSaida(int Inseridas);
public sealed record NivelSaida(string Nivel, string Nome, double IndiceMin, string AcaoSugerida);
public sealed record OcorrenciaSaida(string Id, string AmbienteId, string Descricao, DateTimeOffset CriadaEm);
public sealed record AcaoSaida(long Id, string AmbienteId, string Nivel, string Descricao, DateTimeOffset RegistradoEm);
public sealed record PrevisaoSaida(string AmbienteId, DateTimeOffset PrevistoPara, double IndiceRuaPrevisto,
    double? DeltaSala, double? IndiceSalaPrevisto, string NivelPrevisto, bool SemLeiturasRecentes, bool Demonstracao, bool RuaDisponivel);
