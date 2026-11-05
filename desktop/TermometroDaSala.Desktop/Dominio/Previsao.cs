namespace TermometroDaSala.Desktop;

/// <summary>Previsão do nível da sala para daqui a 2 h. Imutável: chega pronta da API (ou da demonstração).</summary>
public sealed class Previsao
{
    public required string AmbienteId { get; init; }
    public required DateTimeOffset PrevistoPara { get; init; }
    public required double IndiceRuaPrevisto { get; init; }
    public double? DeltaSala { get; init; }
    public double? IndiceSalaPrevisto { get; init; }
    public required NivelCalor NivelPrevisto { get; init; }
    public required bool SemLeiturasRecentes { get; init; }
    public required bool Demonstracao { get; init; }
    // false: sem previsão da rua, a API repete o índice da última leitura válida da sala e manda IndiceRuaPrevisto 0.
    public required bool RuaDisponivel { get; init; }

    /// <summary>Confere a previsão recebida para a sala pedida; as regras dependem de haver previsão da rua.</summary>
    public bool Valida(string ambienteId) =>
        AmbienteId == ambienteId && Enum.IsDefined(NivelPrevisto) && double.IsFinite(IndiceRuaPrevisto) &&
        (DeltaSala is not { } d || double.IsFinite(d)) && (IndiceSalaPrevisto is not { } i || double.IsFinite(i)) &&
        (RuaDisponivel ? SemLeiturasRecentes || DeltaSala is not null && IndiceSalaPrevisto is not null : IndiceSalaPrevisto is not null);
}
