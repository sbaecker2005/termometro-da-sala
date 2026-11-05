using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;

namespace TermometroDaSala.Api.Servicos;

public interface IPrevisorRua
{
    double? IndiceNoInstante(DateTimeOffset utc);
    double? TemperaturaNoInstante(DateTimeOffset utc);
    double? PreverDaquiA2h(DateTimeOffset utc);
}

public class PrevisorRuaBaseline : IPrevisorRua
{
    private readonly SalaDb _db;
    public PrevisorRuaBaseline(SalaDb db) => _db = db;

    public double? IndiceNoInstante(DateTimeOffset utc) =>
        _db.LeiturasRua.AsEnumerable().FirstOrDefault(r => r.MedidoEm == utc)?.IndiceCalor;

    public double? TemperaturaNoInstante(DateTimeOffset utc) =>
        _db.LeiturasRua.AsEnumerable().FirstOrDefault(r => r.MedidoEm == utc)?.TemperaturaC;

    // shortcut: sem modelo.json válido, a rua daqui a 2 h repete o índice de agora
    public double? PreverDaquiA2h(DateTimeOffset utc) =>
        _db.LeiturasRua.AsEnumerable().Where(r => r.MedidoEm <= utc).OrderByDescending(r => r.MedidoEm).FirstOrDefault()?.IndiceCalor;
}

public class PrevisorRuaModelo : IPrevisorRua
{
    private readonly SalaDb _db;
    private readonly ILogger<PrevisorRuaModelo> _log;
    private readonly PrevisorRuaBaseline _provisorio;
    private readonly (double Intercepto, Dictionary<string, double> Coef)? _modelo;

    public PrevisorRuaModelo(SalaDb db, ILogger<PrevisorRuaModelo> log, string caminho)
    {
        _db = db;
        _log = log;
        _provisorio = new PrevisorRuaBaseline(db);
        _modelo = Carregar(caminho, log);
    }

    public double? IndiceNoInstante(DateTimeOffset utc) => _provisorio.IndiceNoInstante(utc);
    public double? TemperaturaNoInstante(DateTimeOffset utc) => _provisorio.TemperaturaNoInstante(utc);

    public double? PreverDaquiA2h(DateTimeOffset utc)
    {
        if (_modelo is null) return _provisorio.PreverDaquiA2h(utc);
        var agora = _db.LeiturasRua.AsEnumerable().Where(r => r.MedidoEm <= utc).OrderByDescending(r => r.MedidoEm).FirstOrDefault();
        if (agora is null) return null;

        var alvo = agora.MedidoEm.AddHours(-1).ToUnixTimeSeconds();
        // shortcut: só casa a leitura no mesmo segundo; janela de minutos quando a rua não for horária
        var antes = _db.LeiturasRua.AsEnumerable().FirstOrDefault(r => r.MedidoEm.ToUnixTimeSeconds() == alvo);
        double variacao = 0;
        if (antes is null)
            _log.LogInformation("variacao_1h sem leitura de 1 h antes de {Instante}", agora.MedidoEm);
        else
            variacao = agora.TemperaturaC - antes.TemperaturaC;

        var local = Relogio.HoraLocal(agora.MedidoEm);
        var valores = new Dictionary<string, double>
        {
            ["temperatura_c"] = agora.TemperaturaC,
            ["umidade_pct"] = agora.UmidadePct,
            ["hora_sen"] = Math.Sin(2 * Math.PI * local.Hour / 24),
            ["hora_cos"] = Math.Cos(2 * Math.PI * local.Hour / 24),
            ["mes_sen"] = Math.Sin(2 * Math.PI * local.Month / 12),
            ["mes_cos"] = Math.Cos(2 * Math.PI * local.Month / 12),
            ["variacao_1h"] = variacao,
        };
        var (intercepto, coef) = _modelo.Value;
        var y = intercepto;
        foreach (var par in coef)
            y += par.Value * valores[par.Key];
        return y;
    }

    static (double, Dictionary<string, double>)? Carregar(string caminho, ILogger log)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(caminho));
            var raiz = doc.RootElement;
            if (raiz.GetProperty("tipo").GetString() != "linear")
                throw new InvalidDataException("tipo");
            var coef = new Dictionary<string, double>();
            var tabela = raiz.GetProperty("coeficientes");
            foreach (var item in raiz.GetProperty("atributos").EnumerateArray())
            {
                var nome = item.GetString() ?? throw new InvalidDataException("atributo");
                if (nome is not ("temperatura_c" or "umidade_pct" or "hora_sen" or "hora_cos" or "mes_sen" or "mes_cos" or "variacao_1h"))
                    throw new InvalidDataException(nome);
                coef[nome] = tabela.GetProperty(nome).GetDouble();
            }
            if (coef.Count == 0) throw new InvalidDataException("vazio");
            return (raiz.GetProperty("intercepto").GetDouble(), coef);
        }
        catch (Exception ex)
        {
            log.LogWarning("modelo.json indisponivel em {Caminho} ({Motivo}); previsao provisoria", caminho, ex.Message);
            return null;
        }
    }
}

public class PrevisorRuaDemo : IPrevisorRua
{
    private readonly Dictionary<int, (double Temp, int Umid, double Indice)> _horas;

    public PrevisorRuaDemo(string contentRoot)
    {
        var caminho = ArquivosDemo.Achar(contentRoot, "rua-dia-quente.csv");
        _horas = new Dictionary<int, (double, int, double)>();
        foreach (var linha in File.ReadLines(caminho).Skip(1))
        {
            var c = linha.Split(',');
            if (c.Length < 3) continue;
            var hora = int.Parse(c[0], CultureInfo.InvariantCulture);
            var temp = double.Parse(c[1], CultureInfo.InvariantCulture);
            var umid = int.Parse(c[2], CultureInfo.InvariantCulture);
            _horas[hora] = (temp, umid, IndiceCalor.Calcular(temp, umid));
        }
    }

    public double? IndiceNoInstante(DateTimeOffset utc) =>
        _horas.TryGetValue(Relogio.HoraLocal(utc).Hour, out var h) ? h.Indice : null;

    public double? TemperaturaNoInstante(DateTimeOffset utc) =>
        _horas.TryGetValue(Relogio.HoraLocal(utc).Hour, out var h) ? h.Temp : null;

    public double? PreverDaquiA2h(DateTimeOffset utc) => IndiceNoInstante(utc.AddHours(2));
}

public static class ArquivosDemo
{
    public static string Achar(string inicio, string nome)
    {
        for (var dir = new DirectoryInfo(inicio); dir is not null; dir = dir.Parent)
        {
            var candidato = Path.Combine(dir.FullName, "demo", nome);
            if (File.Exists(candidato)) return candidato;
        }
        throw new FileNotFoundException(nome);
    }
}

public class ConsultaPrevisao
{
    private readonly SalaDb _db;
    private readonly IPrevisorRua _rua;
    private readonly TimeProvider _tempo;
    private readonly IHostEnvironment _ambiente;

    public ConsultaPrevisao(SalaDb db, IPrevisorRua rua, TimeProvider tempo, IHostEnvironment ambiente)
    {
        _db = db;
        _rua = rua;
        _tempo = tempo;
        _ambiente = ambiente;
    }

    public async Task<double?> Delta(string ambienteId, DateTimeOffset agora)
    {
        var desde = agora.AddHours(-3);
        var leituras = (await _db.Leituras.Where(l => l.AmbienteId == ambienteId && !l.Suspeita).ToListAsync())
            .Where(l => l.MedidoEm >= desde && l.MedidoEm <= agora)
            .ToList();
        var pares = new List<double>();
        foreach (var leitura in leituras)
        {
            var rua = _rua.IndiceNoInstante(leitura.MedidoEm);
            if (rua is null) continue;
            pares.Add(leitura.IndiceCalor - rua.Value);
        }
        return pares.Count < 3 ? null : Math.Round(pares.Average(), 1);
    }

    public async Task<object?> Montar(string ambienteId)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == ambienteId)) return null;
        var agora = _tempo.GetUtcNow();
        var rua = _rua.PreverDaquiA2h(agora);
        var delta = await Delta(ambienteId, agora);
        double? sala = delta is null || rua is null ? null : Math.Round(rua.Value + delta.Value, 1);
        var desde = agora.AddHours(-3);
        var demonstracao = _ambiente.IsEnvironment("Demo")
            || (await _db.Leituras.Where(l => l.AmbienteId == ambienteId && l.Origem == "simulada").ToListAsync()).Any(l => l.MedidoEm >= desde);
        return new
        {
            ambienteId,
            previstoPara = agora.AddHours(2),
            indiceRuaPrevisto = Math.Round(rua ?? 0, 1),
            deltaSala = delta,
            indiceSalaPrevisto = sala,
            nivelPrevisto = IndiceCalor.Nivel(sala ?? rua ?? 0),
            semLeiturasRecentes = delta is null,
            demonstracao,
        };
    }
}
