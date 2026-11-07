using System.Globalization;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Dominio.Entidades;

namespace TermometroDaSala.Api.Servicos;

public static class MapaTermico
{
    public static MapaSaida Montar(Ambiente sala, IEnumerable<Leitura> leituras, DateTimeOffset agora)
    {
        var recentes = leituras.Where(l => l.MedidoEm >= agora.AddMinutes(-15) && l.MedidoEm <= agora)
            .GroupBy(l => l.DispositivoId).ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.MedidoEm).ThenByDescending(l => l.Id).First());
        var sensores = sala.Dispositivos.OrderBy(d => d.Id).ToList();
        var fontes = sensores.Where(d => recentes.ContainsKey(d.Id)).ToList();
        var media = fontes.Count == 0 ? (double?)null : fontes.Average(d => recentes[d.Id].TemperaturaC);

        CelulaMapa Estimar(double x, double y)
        {
            double peso = 0, temperatura = 0, indice = 0;
            foreach (var d in fontes)
            {
                var l = recentes[d.Id];
                var distancia2 = Math.Pow(x - (d.PosX ?? (sala.LarguraM ?? 0) / 2), 2)
                    + Math.Pow(y - (d.PosY ?? (sala.ProfundidadeM ?? 0) / 2), 2);
                if (distancia2 == 0) return new(x, y, l.TemperaturaC, l.IndiceCalor, IndiceCalor.Nivel(l.IndiceCalor).ToString());
                var p = 1 / distancia2;
                peso += p;
                temperatura += l.TemperaturaC * p;
                indice += l.IndiceCalor * p;
            }
            return peso == 0 ? new(x, y, null, null, null)
                : new(x, y, temperatura / peso, indice / peso, IndiceCalor.Nivel(indice / peso).ToString());
        }

        var grade = new List<CelulaMapa>();
        for (double y = 0; y <= (sala.ProfundidadeM ?? 0); y += 0.5)
            for (double x = 0; x <= (sala.LarguraM ?? 0); x += 0.5)
                grade.Add(Estimar(x, y));

        var pontos = new List<PontoMapa>();
        foreach (var d in sensores)
        {
            var x = d.PosX ?? (sala.LarguraM ?? 0) / 2;
            var y = d.PosY ?? (sala.ProfundidadeM ?? 0) / 2;
            recentes.TryGetValue(d.Id, out var l);
            pontos.Add(new(d.Id, d.Rotulo ?? d.Id, "sensor", x, y, false, l?.TemperaturaC,
                l?.IndiceCalor, l is null ? null : IndiceCalor.Nivel(l.IndiceCalor).ToString(), l?.TemperaturaC - media, l is null));
        }
        foreach (var lugar in sala.Lugares.OrderBy(l => l.Nome, StringComparer.Ordinal))
        {
            var c = Estimar(lugar.X, lugar.Y);
            pontos.Add(new(lugar.Id.ToString(CultureInfo.InvariantCulture), lugar.Nome, "lugar", lugar.X, lugar.Y,
                lugar.Vulneravel, c.TemperaturaC, c.IndiceCalor, c.Nivel, c.TemperaturaC - media, c.TemperaturaC is null));
        }
        var horarios = fontes.Select(d => recentes[d.Id].MedidoEm).ToList();
        return new(sala.Id, sala.LarguraM, sala.ProfundidadeM, 0.5, grade, pontos, Precaucoes(pontos),
            horarios.Count == 0 ? null : horarios.Min());
    }

    public static IReadOnlyList<string> Precaucoes(IReadOnlyList<PontoMapa> pontos)
    {
        var textos = new List<string>();
        var lugares = pontos.Where(p => p.Tipo == "lugar" && !p.SemLeitura).ToList();
        static double DistanciaNormal(PontoMapa p) => p.IndiceCalor < 18 ? 18 - p.IndiceCalor.Value
            : p.IndiceCalor >= 27 ? p.IndiceCalor.Value - 27 : 0;
        static string Numero(double? v) => v!.Value.ToString("0.0", CultureInfo.InvariantCulture);
        foreach (var p in lugares.Where(p => p.Vulneravel && p.Nivel is "frio" or "cuidado" or "perigo"))
        {
            var destino = lugares.Where(q => q != p && DistanciaNormal(q) < DistanciaNormal(p))
                .OrderBy(q => q.Nivel == "normal" ? 0 : 1).ThenBy(DistanciaNormal)
                .ThenBy(q => Math.Pow(q.X - p.X, 2) + Math.Pow(q.Y - p.Y, 2)).ThenBy(q => q.Rotulo, StringComparer.Ordinal).FirstOrDefault();
            textos.Add(destino is null
                ? $"{p.Rotulo}: sem lugar mais próximo da faixa normal para troca ({Numero(p.IndiceCalor)} °C de índice)"
                : $"trocar {p.Rotulo} para {destino.Rotulo} ({Numero(p.IndiceCalor)} → {Numero(destino.IndiceCalor)} °C de índice)");
        }
        var medidos = pontos.Where(p => p.Tipo == "sensor" && !p.SemLeitura).OrderBy(p => p.TemperaturaC).ToList();
        if (medidos.Count > 1 && medidos[^1].TemperaturaC - medidos[0].TemperaturaC >= 4)
            textos.Add($"ambiente desigual: verificar janela, sol e corrente de ar perto de {medidos[0].Rotulo} e {medidos[^1].Rotulo} ({Numero(medidos[0].TemperaturaC)} a {Numero(medidos[^1].TemperaturaC)} °C)");
        return textos;
    }
}
