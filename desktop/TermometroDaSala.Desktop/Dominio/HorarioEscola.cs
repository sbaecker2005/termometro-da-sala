namespace TermometroDaSala.Desktop;

/// <summary>A escola fica em São Paulo (UTC−3, sem horário de verão), qualquer que seja o fuso do computador.</summary>
public static class HorarioEscola
{
    public static DateTimeOffset Hora(DateTimeOffset hora) => hora.ToOffset(TimeSpan.FromHours(-3));
    public static DateOnly Dia(DateTimeOffset hora) => DateOnly.FromDateTime(Hora(hora).DateTime);
}
