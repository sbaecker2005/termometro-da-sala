using System.Drawing;
using System.Text.Json;

namespace TermometroDaSala.Desktop;

public record CelulaMapa(double X, double Y, double? TemperaturaC, double? IndiceCalor, NivelCalor? Nivel);
public record PontoMapa(string Id, string Rotulo, string Tipo, double X, double Y, bool Vulneravel,
    double? TemperaturaC, double? IndiceCalor, NivelCalor? Nivel, double? DesvioC, bool SemLeitura)
{
    public IReadOnlyList<string> Precaucoes { get; init; } = [];
    public string Texto => $"{Rotulo} ({Tipo}): {(SemLeitura || TemperaturaC is null ? "sem leitura" : $"{TemperaturaC:F1} °C; {Nivel?.Texto() ?? "sem nível"}")}"
        + (Precaucoes.Count == 0 ? "" : "; " + string.Join("; ", Precaucoes));
}
public record Mapa(string AmbienteId, double? LarguraM, double? ProfundidadeM, double PassoM,
    List<CelulaMapa> Grade, List<PontoMapa> Pontos, List<string> Precaucoes)
{
    public IEnumerable<(string Codigo, PontoMapa Ponto)> PontosIdentificados()
    {
        var sensores = 0;
        var cadeiras = 0;
        foreach (var ponto in Pontos)
        {
            var codigo = ponto.Tipo == "sensor" ? $"S{++sensores}" : $"C{++cadeiras:00}";
            yield return (codigo, ponto);
        }
    }

    public void Conferir(string id)
    {
        bool Numero(double? n) => n is null || double.IsFinite(n.Value);
        bool NivelValido(NivelCalor? n) => n is null || Enum.IsDefined(n.Value);
        bool Dentro(double x, double y) => double.IsFinite(x) && double.IsFinite(y) && x >= 0 && y >= 0
            && x <= (LarguraM ?? 0) && y <= (ProfundidadeM ?? 0);
        if (AmbienteId != id || LarguraM.HasValue != ProfundidadeM.HasValue
            || LarguraM is { } l && (!double.IsFinite(l) || l < .5 || l > 50)
            || ProfundidadeM is { } p && (!double.IsFinite(p) || p < .5 || p > 50)
            || !double.IsFinite(PassoM) || PassoM <= 0 || Grade is null || Pontos is null || Precaucoes is null
            || Grade.Count > 10201 || Pontos.Count > 300
            || Grade.Any(c => c is null || !Dentro(c.X, c.Y) || !Numero(c.TemperaturaC) || !Numero(c.IndiceCalor) || !NivelValido(c.Nivel))
            || Pontos.Any(c => c is null || !Dentro(c.X, c.Y) || string.IsNullOrWhiteSpace(c.Id) || string.IsNullOrWhiteSpace(c.Rotulo)
                || c.Tipo is not ("sensor" or "lugar") || !Numero(c.TemperaturaC) || !Numero(c.IndiceCalor) || !Numero(c.DesvioC) || !NivelValido(c.Nivel)
                || c.Precaucoes is null || c.Precaucoes.Any(p => p is null))
            || Precaucoes.Any(c => c is null))
            throw new JsonException("Mapa inválido recebido da API.");
    }
    public static PointF ParaTela(double x, double y, double largura, double profundidade, RectangleF area)
    {
        if (largura <= 0 || profundidade <= 0) return new(area.Left + area.Width / 2, area.Top + area.Height / 2);
        var escala = (float)Math.Min(area.Width / largura, area.Height / profundidade);
        return new(area.Left + (area.Width - (float)largura * escala) / 2 + (float)x * escala,
            area.Top + (area.Height + (float)profundidade * escala) / 2 - (float)y * escala);
    }
    public static float Diametro(double? desvio) => 12 + (float)Math.Min(Math.Abs(desvio ?? 0), 8) * 3;
    public static Color Cor(NivelCalor? nivel) => nivel switch
    {
        NivelCalor.Frio => Color.LightBlue, NivelCalor.Normal => Color.LightGreen,
        NivelCalor.Atencao => Color.Khaki, NivelCalor.Cuidado => Color.OrangeRed,
        NivelCalor.Perigo => Color.Red, _ => Color.LightGray
    };

    public static Mapa Demonstracao(string id, bool futuro)
    {
        double Valor(double x) => (id == "sala-05" ? 15 : 35) + (24 - (id == "sala-05" ? 15 : 35)) * x / 6 + (futuro ? -1 : 0);
        NivelCalor Nivel(double t) => t < 18 ? NivelCalor.Frio : t < 27 ? NivelCalor.Normal : t < 32 ? NivelCalor.Atencao : t < 41 ? NivelCalor.Cuidado : NivelCalor.Perigo;
        var grade = new List<CelulaMapa>();
        for (double x = 0; x <= 6; x += .5)
            for (double y = 0; y <= 4; y += .5) grade.Add(new(x, y, Valor(x), Valor(x), Nivel(Valor(x))));
        var pontos = new List<PontoMapa>();
        foreach (var (nome, tipo, x, y) in new[] { ("Janela", "sensor", 0d, 2d), ("Fundo", "sensor", 6d, 2d) })
            pontos.Add(new(nome, nome, tipo, x, y, tipo == "lugar", Valor(x), Valor(x), Nivel(Valor(x)), Valor(x) - Valor(3), false));
        for (var i = 0; i < 20; i++)
        {
            var x = .75 + 1.5 * (i % 4);
            var y = .4 + .8 * (i / 4);
            var nome = $"Cadeira {i + 1:00}";
            pontos.Add(new(nome, nome, "lugar", x, y, i is 8 or 19, Valor(x), Valor(x), Nivel(Valor(x)), Valor(x) - Valor(3), false));
        }
        return new(id, 6, 4, .5, grade, pontos, ["DEMONSTRAÇÃO: valores simulados, inclusive a previsão +1 h.", "Verificar janela, sol e corrente de ar; avaliar presencialmente os lugares vulneráveis."]);
    }
}
