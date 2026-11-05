using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
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
        _db.LeiturasRua.Where(r => r.MedidoEm == utc).Select(r => (double?)r.IndiceCalor).FirstOrDefault();

    public double? TemperaturaNoInstante(DateTimeOffset utc) =>
        _db.LeiturasRua.Where(r => r.MedidoEm == utc).Select(r => (double?)r.TemperaturaC).FirstOrDefault();

    // shortcut: sem modelo.json válido, a rua daqui a 2 h repete o índice de agora
    public double? PreverDaquiA2h(DateTimeOffset utc) => Ultima(_db, utc)?.IndiceCalor;

    internal static LeituraRua? Ultima(SalaDb db, DateTimeOffset utc) =>
        db.LeiturasRua.Where(r => r.MedidoEm <= utc).OrderByDescending(r => r.MedidoEm).FirstOrDefault();
}

public class PrevisorRuaModelo : IPrevisorRua
{
    private readonly SalaDb _db;
    private readonly ILogger<PrevisorRuaModelo> _log;
    private readonly PrevisorRuaBaseline _provisorio;
    private readonly Func<Dictionary<string, double>, double>? _modelo; // linear ou arvore, conforme ml/modelo.json

    public PrevisorRuaModelo(SalaDb db, ILogger<PrevisorRuaModelo> log, string caminho)
    {
        _db = db;
        _log = log;
        _provisorio = new PrevisorRuaBaseline(db);
        _modelo = ModeloRua.Carregar(caminho, log);
    }

    public double? IndiceNoInstante(DateTimeOffset utc) => _provisorio.IndiceNoInstante(utc);
    public double? TemperaturaNoInstante(DateTimeOffset utc) => _provisorio.TemperaturaNoInstante(utc);

    public double? PreverDaquiA2h(DateTimeOffset utc)
    {
        if (_modelo is null) return _provisorio.PreverDaquiA2h(utc);
        var agora = PrevisorRuaBaseline.Ultima(_db, utc);
        if (agora is null) return null;

        // shortcut: só casa a leitura no mesmo segundo; janela de minutos quando a rua não for horária
        var inicio = DateTimeOffset.FromUnixTimeSeconds(agora.MedidoEm.AddHours(-1).ToUnixTimeSeconds());
        var fim = inicio.AddSeconds(1);
        var antes = _db.LeiturasRua.Where(r => r.MedidoEm >= inicio && r.MedidoEm < fim).Select(r => (double?)r.TemperaturaC).FirstOrDefault();
        if (antes is null)
            _log.LogInformation("variacao_1h sem leitura de 1 h antes de {Instante}", agora.MedidoEm);

        return _modelo(ModeloRua.Atributos(Relogio.HoraLocal(agora.MedidoEm), agora.TemperaturaC, agora.UmidadePct,
            antes is null ? 0 : agora.TemperaturaC - antes.Value));
    }
}

// Leitura do ml/modelo.json, usada fora do Demo (rua gravada no banco) e no Demo (dia do CSV).
public static class ModeloRua
{
    static readonly HashSet<string> Nomes = new() { "temperatura_c", "umidade_pct", "hora_sen", "hora_cos", "mes_sen", "mes_cos", "variacao_1h", "indice_agora" };

    // arvore exportada pelo notebook: {atributo, limite, menor_ou_igual, maior} nos nos internos e {valor} nas folhas
    static double Avaliar(JsonElement no, Dictionary<string, double> x)
    {
        while (!no.TryGetProperty("valor", out var folha))
        {
            var atributo = no.GetProperty("atributo").GetString() ?? throw new InvalidDataException("atributo");
            no = x[atributo] <= no.GetProperty("limite").GetDouble() ? no.GetProperty("menor_ou_igual") : no.GetProperty("maior");
        }
        return no.GetProperty("valor").GetDouble();
    }

    static void ConferirArvore(JsonElement no, int profundidade = 0)
    {
        if (profundidade > 64) throw new InvalidDataException("profundidade");
        if (no.TryGetProperty("valor", out var v)) { v.GetDouble(); return; }
        if (!Nomes.Contains(no.GetProperty("atributo").GetString() ?? "")) throw new InvalidDataException("atributo");
        no.GetProperty("limite").GetDouble();
        ConferirArvore(no.GetProperty("menor_ou_igual"), profundidade + 1);
        ConferirArvore(no.GetProperty("maior"), profundidade + 1);
    }

    public static Dictionary<string, double> Atributos(DateTimeOffset local, double temperaturaC, int umidadePct, double variacao1h) => new()
    {
        ["temperatura_c"] = temperaturaC,
        ["umidade_pct"] = umidadePct,
        ["hora_sen"] = Math.Sin(2 * Math.PI * local.Hour / 24),
        ["hora_cos"] = Math.Cos(2 * Math.PI * local.Hour / 24),
        ["mes_sen"] = Math.Sin(2 * Math.PI * local.Month / 12),
        ["mes_cos"] = Math.Cos(2 * Math.PI * local.Month / 12),
        ["variacao_1h"] = variacao1h,
        ["indice_agora"] = IndiceCalor.Calcular(temperaturaC, umidadePct),
    };

    public static Func<Dictionary<string, double>, double>? Carregar(string caminho, ILogger log)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(caminho));
            var raiz = doc.RootElement;
            var tipo = raiz.GetProperty("tipo").GetString();
            if (tipo == "arvore")
            {
                var no = raiz.GetProperty("raiz").Clone();
                ConferirArvore(no);
                return x => Avaliar(no, x);
            }
            if (tipo != "linear")
                throw new InvalidDataException("tipo");
            var coef = new Dictionary<string, double>();
            var tabela = raiz.GetProperty("coeficientes");
            foreach (var item in raiz.GetProperty("atributos").EnumerateArray())
            {
                var nome = item.GetString() ?? throw new InvalidDataException("atributo");
                if (!Nomes.Contains(nome))
                    throw new InvalidDataException(nome);
                coef[nome] = tabela.GetProperty(nome).GetDouble();
            }
            if (coef.Count == 0) throw new InvalidDataException("vazio");
            var intercepto = raiz.GetProperty("intercepto").GetDouble();
            return x => intercepto + coef.Sum(par => par.Value * x[par.Key]);
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
    private readonly Func<Dictionary<string, double>, double>? _modelo;

    public PrevisorRuaDemo(string contentRoot, string caminhoModelo, ILogger log)
    {
        _modelo = ModeloRua.Carregar(caminhoModelo, log);
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

    // o CSV é um dia de exemplo, não a rua medida agora: não serve para marcar o sensor ao vivo como suspeito (regra dos 8 °C)
    public double? TemperaturaNoInstante(DateTimeOffset utc) => null;

    // O modelo recebe a hora do CSV como a rua de agora, com o mês de hoje e a variação desde a hora anterior.
    public double? PreverDaquiA2h(DateTimeOffset utc)
    {
        if (_modelo is null) return IndiceNoInstante(utc.AddHours(2)); // sem modelo.json: a hora do CSV 2 h à frente
        var local = Relogio.HoraLocal(utc);
        if (!_horas.TryGetValue(local.Hour, out var agora)) return null;
        // o CSV é um dia só: à meia-noite, a hora anterior é a das 23 h do mesmo dia
        var variacao = _horas.TryGetValue((local.Hour + 23) % 24, out var antes) ? agora.Temp - antes.Temp : 0;
        return _modelo(ModeloRua.Atributos(local, agora.Temp, agora.Umid, variacao));
    }
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
        var leituras = await _db.Leituras
            .Where(l => l.AmbienteId == ambienteId && !l.Suspeita && l.MedidoEm >= desde && l.MedidoEm <= agora)
            .ToListAsync();
        var pares = new List<double>();
        foreach (var leitura in leituras)
        {
            var rua = _rua.IndiceNoInstante(leitura.MedidoEm);
            if (rua is null) continue;
            pares.Add(leitura.IndiceCalor - rua.Value);
        }
        return pares.Count < 3 ? null : Math.Round(pares.Average(), 1);
    }

    // null: sem rua e sem leitura válida da sala nas últimas 3 h, não há o que prever sem inventar
    public async Task<PrevisaoSaida?> Montar(string ambienteId)
    {
        var agora = _tempo.GetUtcNow();
        var desde = agora.AddHours(-3);
        var demonstracao = _ambiente.IsEnvironment("Demo")
            || await _db.Leituras.AnyAsync(l => l.AmbienteId == ambienteId && l.Origem == Origem.simulada && l.MedidoEm >= desde);
        var rua = _rua.PreverDaquiA2h(agora);
        if (rua is not null)
        {
            var delta = await Delta(ambienteId, agora);
            double? sala = delta is null ? null : Math.Round(rua.Value + delta.Value, 1);
            return await Gravar(new PrevisaoSaida(ambienteId, agora.AddHours(2), Math.Round(rua.Value, 1),
                delta, sala, IndiceCalor.Nivel(sala ?? rua.Value).ToString(), delta is null, demonstracao, true), agora);
        }

        // Sem rua (fora do Demo nada grava LeiturasRua): a sala se mantém no índice da última leitura válida.
        var ultima = await _db.Leituras
            .Where(l => l.AmbienteId == ambienteId && !l.Suspeita && l.MedidoEm >= desde && l.MedidoEm <= agora)
            .OrderByDescending(l => l.MedidoEm).FirstOrDefaultAsync();
        if (ultima is null) return null;
        return await Gravar(new PrevisaoSaida(ambienteId, agora.AddHours(2), 0,
            null, ultima.IndiceCalor, IndiceCalor.Nivel(ultima.IndiceCalor).ToString(), false, demonstracao, false), agora);
    }

    // A previsão servida entra no histórico (tabela Previsoes), para comparar depois com o que a sala mediu.
    // No máximo uma por sala a cada 10 min: o GET é anônimo e painel e desktop consultam a cada 30 s.
    async Task<PrevisaoSaida> Gravar(PrevisaoSaida p, DateTimeOffset agora)
    {
        var janela = agora.AddMinutes(-10);
        if (await _db.Previsoes.AnyAsync(x => x.AmbienteId == p.AmbienteId && x.CalculadaEm > janela)) return p;
        _db.Previsoes.Add(new Previsao
        {
            AmbienteId = p.AmbienteId,
            PrevistoPara = p.PrevistoPara,
            IndiceRuaPrevisto = p.IndiceRuaPrevisto,
            DeltaSala = p.DeltaSala,
            IndiceSalaPrevisto = p.IndiceSalaPrevisto,
            NivelPrevisto = Enum.Parse<Nivel>(p.NivelPrevisto),
            SemLeiturasRecentes = p.SemLeiturasRecentes,
            CalculadaEm = agora,
        });
        await _db.SaveChangesAsync();
        return p;
    }
}
