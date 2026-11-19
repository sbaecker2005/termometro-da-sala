using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Tests;

public class PrevisorModeloTestes
{
    [Fact]
    public void CopiaDaApiEIdenticaAoModeloExportado()
    {
        var api = AcharModelo();
        var ml = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(api)!, "../../../ml"));
        Assert.Equal(File.ReadAllBytes(Path.Combine(ml, "modelo.json")), File.ReadAllBytes(api));
    }

    [Fact]
    public void DuzentasPrevisoesBatemComScikitLearnDoNotebook()
    {
        var api = AcharModelo();
        var ml = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(api)!, "../../../ml"));
        var prever = ModeloRua.Carregar(api, NullLogger.Instance);
        Assert.NotNull(prever);
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(ml, "paridade-api.json")));
        Assert.Equal(200, doc.RootElement.GetArrayLength());
        foreach (var linha in doc.RootElement.EnumerateArray())
        {
            var atributos = linha.EnumerateObject().Where(p => p.Name != "previsto")
                .ToDictionary(p => p.Name, p => p.Value.GetDouble());
            Assert.InRange(Math.Abs(prever(atributos) - linha.GetProperty("previsto").GetDouble()), 0, 0.00005);
        }
    }

    [Fact]
    public void TresLinhasBatemComAFormulaEComAHoraDeSaoPaulo()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(AcharModelo()));
        var modelo = doc.RootElement;
        // 17:00 UTC em 15/01/2026 = 14h em São Paulo. Valores de 2 casas calculados na seção 6 do notebook, percorrendo ml/modelo.json.
        (DateTimeOffset Utc, double Temp, int Umid, double? Antes, int Hora, int Mes, double Variacao, string Maos)[] linhas =
        {
            (new DateTimeOffset(2026, 1, 15, 17, 0, 0, TimeSpan.Zero), 30, 50, 28.5, 14, 1, 1.5, "27.16"),
            (new DateTimeOffset(2026, 7, 15, 3, 0, 0, TimeSpan.Zero), 18, 80, 18.4, 0, 7, -0.4, "17.48"),
            (new DateTimeOffset(2025, 12, 21, 15, 0, 0, TimeSpan.Zero), 27.2, 62, null, 12, 12, 0, "26.63"),
        };

        using var banco = new BancoRua();
        foreach (var linha in linhas)
        {
            banco.Db.LeiturasRua.Add(Leitura(linha.Utc, linha.Temp, linha.Umid));
            if (linha.Antes is not null)
                banco.Db.LeiturasRua.Add(Leitura(linha.Utc.AddHours(-1), linha.Antes.Value, linha.Umid));
        }
        banco.Db.SaveChanges();
        var previsor = new PrevisorRuaModelo(banco.Db, NullLogger<PrevisorRuaModelo>.Instance, AcharModelo());

        foreach (var linha in linhas)
        {
            var local = Relogio.HoraLocal(linha.Utc);
            Assert.Equal(linha.Hora, local.Hour);
            Assert.Equal(linha.Mes, local.Month);
            var obtido = previsor.PreverDaquiA2h(linha.Utc)!.Value;
            Assert.Equal(Formula(modelo, linha.Temp, linha.Umid, local.Hour, local.Month, linha.Variacao), obtido, 6);
            Assert.Equal(linha.Maos, Math.Round(obtido, 2).ToString("0.00", CultureInfo.InvariantCulture));
        }

        var janeiro = linhas[0];
        var comoUtc = Formula(modelo, janeiro.Temp, janeiro.Umid, janeiro.Utc.Hour, 1, janeiro.Variacao);
        var previsto = previsor.PreverDaquiA2h(janeiro.Utc)!.Value;
        Assert.Equal(17, janeiro.Utc.Hour);
        Assert.Equal(14, Relogio.HoraLocal(janeiro.Utc).Hour);
        Assert.NotEqual(Math.Round(comoUtc, 2).ToString("0.00", CultureInfo.InvariantCulture), Math.Round(previsto, 2).ToString("0.00", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void DemoAplicaOModeloAoDiaDoCsvEDaOMesmoValor()
    {
        // O mesmo vetor do teste acima (27,16 °C), agora como horas do CSV do Demo: 13 h a 28,5 °C e 14 h a 30 °C, 50%.
        var pasta = Path.Combine(Path.GetTempPath(), "demo-modelo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(pasta, "demo"));
        File.WriteAllText(Path.Combine(pasta, "demo", "rua-dia-quente.csv"), "horaLocal,temperaturaC,umidadePct\n13,28.5,50\n14,30,50\n16,33.6,40\n");
        try
        {
            var utc = new DateTimeOffset(2026, 1, 15, 17, 0, 0, TimeSpan.Zero);
            var demo = new PrevisorRuaDemo(pasta, AcharModelo(), NullLogger.Instance).PreverDaquiA2h(utc)!.Value;

            using var banco = new BancoRua();
            banco.Db.LeiturasRua.Add(Leitura(utc, 30, 50));
            banco.Db.LeiturasRua.Add(Leitura(utc.AddHours(-1), 28.5, 50));
            banco.Db.SaveChanges();
            var modelo = new PrevisorRuaModelo(banco.Db, NullLogger<PrevisorRuaModelo>.Instance, AcharModelo()).PreverDaquiA2h(utc)!.Value;

            Assert.Equal(modelo, demo);
            Assert.Equal("27.16", Math.Round(demo, 2).ToString("0.00", CultureInfo.InvariantCulture));
            // sem o modelo, o Demo volta à hora do CSV duas horas à frente (16 h)
            var ausente = Path.Combine(pasta, "nao-existe.json");
            Assert.Equal(IndiceCalor.Calcular(33.6, 40), new PrevisorRuaDemo(pasta, ausente, NullLogger.Instance).PreverDaquiA2h(utc));
            // hora fora do CSV: sem rua, nada é inventado
            Assert.Null(new PrevisorRuaDemo(pasta, AcharModelo(), NullLogger.Instance).PreverDaquiA2h(utc.AddHours(5)));
        }
        finally
        {
            Directory.Delete(pasta, true);
        }
    }

    [Fact]
    public void ArquivoAusenteOuInvalidoCaiNoProvisorio()
    {
        using var banco = new BancoRua();
        var utc = new DateTimeOffset(2026, 1, 15, 17, 0, 0, TimeSpan.Zero);
        banco.Db.LeiturasRua.Add(new LeituraRua { MedidoEm = utc, TemperaturaC = 30, UmidadePct = 40, IndiceCalor = 31.2 });
        banco.Db.SaveChanges();

        var ausente = Path.Combine(Path.GetTempPath(), "modelo-ausente-" + Guid.NewGuid().ToString("N") + ".json");
        Assert.Equal(31.2, new PrevisorRuaModelo(banco.Db, NullLogger<PrevisorRuaModelo>.Instance, ausente).PreverDaquiA2h(utc));

        var ruim = Path.Combine(Path.GetTempPath(), "modelo-ruim-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(ruim, "{");
        try
        {
            Assert.Equal(31.2, new PrevisorRuaModelo(banco.Db, NullLogger<PrevisorRuaModelo>.Instance, ruim).PreverDaquiA2h(utc));
        }
        finally
        {
            File.Delete(ruim);
        }
    }

    internal static double Formula(JsonElement modelo, double temp, int umid, int hora, int mes, double variacao)
    {
        var atributos = new Dictionary<string, double>
        {
            ["temperatura_c"] = temp,
            ["umidade_pct"] = umid,
            ["hora_sen"] = Math.Sin(2 * Math.PI * hora / 24),
            ["hora_cos"] = Math.Cos(2 * Math.PI * hora / 24),
            ["mes_sen"] = Math.Sin(2 * Math.PI * mes / 12),
            ["mes_cos"] = Math.Cos(2 * Math.PI * mes / 12),
            ["variacao_1h"] = variacao,
            ["indice_agora"] = IndiceCalor.Calcular(temp, umid),
        };
        if (modelo.GetProperty("tipo").GetString() == "arvore")
        {
            var no = modelo.GetProperty("raiz");
            while (!no.TryGetProperty("valor", out _))
                no = atributos[no.GetProperty("atributo").GetString()!] <= no.GetProperty("limite").GetDouble()
                    ? no.GetProperty("menor_ou_igual") : no.GetProperty("maior");
            return no.GetProperty("valor").GetDouble();
        }
        var y = modelo.GetProperty("intercepto").GetDouble();
        var coef = modelo.GetProperty("coeficientes");
        foreach (var nome in modelo.GetProperty("atributos").EnumerateArray())
            y += coef.GetProperty(nome.GetString()!).GetDouble() * atributos[nome.GetString()!];
        return y;
    }

    static LeituraRua Leitura(DateTimeOffset quando, double temp, int umid) =>
        new() { MedidoEm = quando, TemperaturaC = temp, UmidadePct = umid, IndiceCalor = 0 };

    internal static string AcharModelo()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidato = Path.Combine(dir.FullName, "TermometroDaSala.Api", "modelo", "modelo.json");
            if (File.Exists(candidato)) return candidato;
        }
        throw new FileNotFoundException("modelo.json");
    }

    sealed class BancoRua : IDisposable
    {
        readonly SqliteConnection _conexao = new("Data Source=:memory:");
        public SalaDb Db { get; }

        public BancoRua()
        {
            _conexao.Open();
            var opcoes = new DbContextOptionsBuilder<SalaDb>();
            SqliteDeTeste.Usar(opcoes, _conexao);
            Db = new SalaDb(opcoes.Options);
            Db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Db.Dispose();
            _conexao.Dispose();
        }
    }
}
