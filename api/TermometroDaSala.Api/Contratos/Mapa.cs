using System.ComponentModel.DataAnnotations;

namespace TermometroDaSala.Api.Contratos;

public class MapaEntrada
{
    [Range(0.5, 50)] public double? LarguraM { get; set; }
    [Range(0.5, 50)] public double? ProfundidadeM { get; set; }
    [Required, MaxLength(100)] public List<SensorPosicao> Sensores { get; set; } = [];
    [Required, MaxLength(200)] public List<LugarEntrada> Lugares { get; set; } = [];
}

public class SensorPosicao
{
    [Required, StringLength(64)] public string Id { get; set; } = "";
    [Range(0, 50)] public double? PosX { get; set; }
    [Range(0, 50)] public double? PosY { get; set; }
    [StringLength(80)] public string? Rotulo { get; set; }
}

public class LugarEntrada
{
    [Required, StringLength(80)] public string Nome { get; set; } = "";
    [Range(0, 50)] public double X { get; set; }
    [Range(0, 50)] public double Y { get; set; }
    public bool Vulneravel { get; set; }
}

public sealed record CelulaMapa(double X, double Y, double? TemperaturaC, double? IndiceCalor, string? Nivel);
public sealed record PontoMapa(string Id, string Rotulo, string Tipo, double X, double Y, bool Vulneravel,
    double? TemperaturaC, double? IndiceCalor, string? Nivel, double? DesvioC, bool SemLeitura);
public sealed record MapaSaida(string AmbienteId, double? LarguraM, double? ProfundidadeM, double PassoM,
    IReadOnlyList<CelulaMapa> Grade, IReadOnlyList<PontoMapa> Pontos, IReadOnlyList<string> Precaucoes,
    DateTimeOffset? LeituraMaisAntigaEm);
