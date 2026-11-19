using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Tests;

public class AjusteSalaTestes
{
    static readonly DateTimeOffset Agora = new(2026, 11, 17, 12, 0, 0, TimeSpan.Zero);

    public static IEnumerable<object[]> CasosCompartilhados()
    {
        var pasta = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(PrevisorModeloTestes.AcharModelo())!, "../../../ml"));
        using var documento = JsonDocument.Parse(File.ReadAllText(Path.Combine(pasta, "casos-ajuste-salas.json")));
        var agora = documento.RootElement.GetProperty("agora").GetString()!;
        return documento.RootElement.GetProperty("casos").EnumerateArray().Select(c => new object[]
            { c.GetProperty("nome").GetString()!, c.GetRawText(), agora }).ToArray();
    }

    [Theory]
    [MemberData(nameof(CasosCompartilhados))]
    public async Task VetoresSaoCompartilhadosComOPipelinePython(string nome, string json, string instante)
    {
        using var documento = JsonDocument.Parse(json);
        var caso = documento.RootElement;
        var agora = DateTimeOffset.Parse(instante);
        var sala = caso.GetProperty("ambiente_id").GetString()!;
        using var b = new Banco();
        foreach (var leitura in caso.GetProperty("rua").EnumerateArray())
            b.Rua(agora.AddSeconds(leitura.GetProperty("segundos").GetDouble()), leitura.GetProperty("indice").GetDouble());
        foreach (var leitura in caso.GetProperty("leituras").EnumerateArray())
            b.Interna(leitura.GetProperty("ambiente_id").GetString()!, leitura.GetProperty("sensor").GetString()!,
                agora.AddSeconds(leitura.GetProperty("segundos").GetDouble()), leitura.GetProperty("indice").GetDouble(),
                leitura.TryGetProperty("suspeita", out var suspeita) && suspeita.GetBoolean());
        b.Db.SaveChanges();
        static double? Esperado(JsonElement item) => item.ValueKind == JsonValueKind.Null ? null : item.GetDouble();
        var deltaEsperado = Esperado(caso.GetProperty("delta_esperado"));
        var delta = await b.Consulta(agora).Delta(sala, agora);
        Assert.True(deltaEsperado == delta, $"{nome}: delta esperado {deltaEsperado}; obtido {delta}.");
        var persistencia = await b.Consulta(agora, new SemRua()).Montar(sala);
        var esperado = Esperado(caso.GetProperty("persistencia_esperada"));
        if (esperado is null) Assert.Null(persistencia);
        else
        {
            Assert.NotNull(persistencia);
            Assert.Equal(esperado, persistencia.IndiceSalaPrevisto);
            Assert.False(persistencia.RuaDisponivel);
        }
    }

    [Fact]
    public void RuaHorariaUsaSomentePassadoAteSessentaMinutos()
    {
        using var b = new Banco();
        b.Rua(Agora.AddHours(-1), 28);
        b.Rua(Agora.AddSeconds(1), 50);
        b.Db.SaveChanges();
        var rua = new PrevisorRuaBaseline(b.Db);
        Assert.Equal(28, rua.IndiceNoInstante(Agora));
        Assert.Null(rua.IndiceNoInstante(Agora.AddTicks(1)));
        Assert.Null(rua.IndiceNoInstante(Agora.AddHours(-1).AddTicks(-1)));
        // A regra de plausibilidade mantém sua correspondência exata.
        Assert.Null(rua.TemperaturaNoInstante(Agora));
        Assert.Equal(25, rua.TemperaturaNoInstante(Agora.AddHours(-1)));
    }

    [Fact]
    public void ConsultaEmLoteMantemAsMesmasBordasETrocaDeObservacao()
    {
        using var b = new Banco();
        b.Rua(Agora.AddHours(-2), 20);
        b.Rua(Agora.AddMinutes(-30), 28);
        b.Rua(Agora.AddMinutes(-30), 29);
        b.Rua(Agora.AddMinutes(-10), double.PositiveInfinity);
        b.Db.SaveChanges();
        var rua = new PrevisorRuaBaseline(b.Db);
        var instantes = new[] { Agora, Agora.AddHours(-1), Agora.AddHours(-1).AddTicks(1),
            Agora.AddMinutes(-30), Agora.AddHours(-3), Agora };
        var lote = rua.IndicesNosInstantes(instantes);
        Assert.Equal(5, lote.Count);
        foreach (var t in instantes) Assert.Equal(rua.IndiceNoInstante(t), lote[t]);
        Assert.Equal(29, lote[Agora]);
        Assert.Null(rua.IndicesNosInstantes([Agora.AddHours(2)])[Agora.AddHours(2)]);
        Assert.Empty(rua.IndicesNosInstantes([]));
    }

    [Fact]
    public async Task QuatroSensoresNoMesmoMinutoNaoSubstituemTresMinutos()
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-20), 28);
        for (var sensor = 0; sensor < 4; sensor++) b.Interna("sala-01", $"s{sensor}", Agora, 32);
        b.Db.SaveChanges();
        var consulta = b.Consulta();
        Assert.Null(await consulta.Delta("sala-01", Agora));
        foreach (var minuto in new[] { -2, -1 }) b.Interna("sala-01", "s0", Agora.AddMinutes(minuto), 32);
        b.Db.SaveChanges();
        Assert.Equal(4, await consulta.Delta("sala-01", Agora));
    }

    [Fact]
    public async Task UltimoPorSensorTemPesoIgualEOsMinutosTambem()
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-20), 28);
        foreach (var minuto in new[] { -2, -1 })
        {
            b.Interna("sala-01", "a", Agora.AddMinutes(minuto).AddSeconds(5), 40);
            b.Interna("sala-01", "a", Agora.AddMinutes(minuto).AddSeconds(15), 32);
            b.Interna("sala-01", "a", Agora.AddMinutes(minuto).AddSeconds(15), 32);
            b.Interna("sala-01", "b", Agora.AddMinutes(minuto).AddSeconds(10), 28);
        }
        b.Interna("sala-01", "a", Agora, 36);
        b.Db.SaveChanges();
        // Minutos: (32+28)/2-28 = 2; 2; 36-28 = 8. Média = 4.
        Assert.Equal(4, await b.Consulta().Delta("sala-01", Agora));
    }

    [Fact]
    public async Task CadaSalaMantemSeuProprioAjuste()
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-20), 28);
        foreach (var minuto in new[] { -2, -1, 0 })
        {
            b.Interna("sala-01", "a", Agora.AddMinutes(minuto), 32);
            b.Interna("sala-02", "b", Agora.AddMinutes(minuto), 26);
        }
        b.Db.SaveChanges();
        var consulta = b.Consulta();
        Assert.Equal(4, await consulta.Delta("sala-01", Agora));
        Assert.Equal(-2, await consulta.Delta("sala-02", Agora));
        Assert.Null(await consulta.Delta("sala-03", Agora));
    }

    [Theory]
    [InlineData(0, 4.0)]
    [InlineData(-1, null)]
    public async Task HistoricoIncluiExatamenteTresHoras(long ticks, double? esperado)
    {
        using var b = new Banco();
        b.Rua(Agora.AddHours(-3).AddMinutes(-1), 28);
        b.Rua(Agora.AddMinutes(-20), 28);
        b.Interna("sala-01", "a", Agora.AddHours(-3).AddTicks(ticks), 32);
        b.Interna("sala-01", "a", Agora.AddMinutes(-1), 32);
        b.Interna("sala-01", "a", Agora, 32);
        b.Db.SaveChanges();
        Assert.Equal(esperado, await b.Consulta().Delta("sala-01", Agora));
    }

    [Fact]
    public async Task UltimoParPrecisaEstarNosQuinzeMinutosRecentes()
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-30), 28);
        foreach (var minuto in new[] { -17, -16, -15 }) b.Interna("sala-01", "a", Agora.AddMinutes(minuto), 32);
        b.Db.SaveChanges();
        Assert.Equal(4, await b.Consulta().Delta("sala-01", Agora));
        Assert.Null(await b.Consulta().Delta("sala-01", Agora.AddTicks(1)));
    }

    [Fact]
    public async Task FuturosSuspeitosENaoFinitosNaoAlteramAjuste()
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-20), 28);
        foreach (var minuto in new[] { -2, -1, 0 }) b.Interna("sala-01", "a", Agora.AddMinutes(minuto), 32);
        b.Interna("sala-01", "a", Agora.AddMinutes(-1).AddSeconds(10), 50, suspeita: true);
        b.Interna("sala-01", "a", Agora.AddMinutes(-1).AddSeconds(20), double.PositiveInfinity);
        b.Interna("sala-01", "a", Agora.AddSeconds(1), 50);
        b.Db.SaveChanges();
        Assert.Equal(4, await b.Consulta().Delta("sala-01", Agora));
    }

    [Fact]
    public async Task OverflowEmDadosCorrompidosNaoProduzNumeroNaoFinito()
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-20), 28);
        foreach (var minuto in new[] { -2, -1, 0 })
            foreach (var sensor in new[] { "a", "b" })
                b.Interna("sala-01", sensor, Agora.AddMinutes(minuto), 1e308);
        b.Db.SaveChanges();
        Assert.Null(await b.Consulta().Delta("sala-01", Agora));
        Assert.Null(await b.Consulta(rua: new SemRua()).Montar("sala-01"));
    }

    [Fact]
    public async Task SemRuaUsaUltimaLeituraValidaDeCadaSensorComPesoIgual()
    {
        using var b = new Banco();
        b.Rua(Agora.AddHours(-1).AddTicks(-1), 28);
        b.Interna("sala-01", "a", Agora.AddMinutes(-5), 30);
        b.Interna("sala-01", "a", Agora.AddMinutes(-1), 34);
        b.Interna("sala-01", "b", Agora.AddMinutes(-2), 24);
        b.Interna("sala-01", "c", Agora.AddMinutes(-15).AddTicks(-1), 50);
        b.Interna("sala-01", "c", Agora.AddSeconds(1), 50);
        b.Interna("sala-01", "c", Agora, 50, suspeita: true);
        b.Db.SaveChanges();
        var prevista = await b.Consulta().Montar("sala-01");
        Assert.NotNull(prevista);
        Assert.False(prevista.RuaDisponivel);
        Assert.Equal(29, prevista.IndiceSalaPrevisto);
        Assert.Null(prevista.DeltaSala);
        Assert.Equal(Agora.AddHours(2), prevista.PrevistoPara);
    }

    [Fact]
    public async Task PersistenciaTambemExpiraDepoisDeQuinzeMinutos()
    {
        using var b = new Banco();
        b.Interna("sala-01", "a", Agora.AddMinutes(-15), 32);
        b.Db.SaveChanges();
        Assert.NotNull(await b.Consulta().Montar("sala-01"));
        Assert.Null(await b.Consulta(Agora.AddTicks(1)).Montar("sala-01"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AlvoPreservaDuasHorasDesdeAObservacaoExterna(bool modelo)
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-90), 28);
        b.Rua(Agora.AddMinutes(-30), 30);
        foreach (var minuto in new[] { -2, -1, 0 }) b.Interna("sala-01", "a", Agora.AddMinutes(minuto), 34);
        b.Db.SaveChanges();
        IPrevisorRua rua = modelo
            ? new PrevisorRuaModelo(b.Db, NullLogger<PrevisorRuaModelo>.Instance, PrevisorModeloTestes.AcharModelo())
            : new PrevisorRuaBaseline(b.Db);
        var prevista = await b.Consulta(rua: rua).Montar("sala-01");
        Assert.NotNull(prevista);
        Assert.True(prevista.RuaDisponivel);
        Assert.Equal(Agora.AddMinutes(90), prevista.PrevistoPara);
        Assert.Equal(Agora, Assert.Single(b.Db.Previsoes).CalculadaEm);
        b.Rua(Agora, 35);
        b.Db.SaveChanges();
        Assert.Equal(Agora.AddMinutes(-30), rua.InstanteBase(Agora));
    }

    [Fact]
    public void ModeloUsaUltimaObservacaoValidaNoEmpateDaHoraAnterior()
    {
        using var b = new Banco();
        foreach (var temperatura in new[] { 10.0, 26.5, double.PositiveInfinity })
        {
            b.Db.LeiturasRua.Add(new LeituraRua { MedidoEm = Agora.AddHours(-1),
                TemperaturaC = temperatura, UmidadePct = 50, IndiceCalor = 28 });
            b.Db.SaveChanges();
        }
        b.Db.LeiturasRua.Add(new LeituraRua { MedidoEm = Agora, TemperaturaC = 28,
            UmidadePct = 50, IndiceCalor = 28 });
        b.Db.SaveChanges();
        var caminho = PrevisorModeloTestes.AcharModelo();
        var modelo = ModeloRua.Carregar(caminho, NullLogger.Instance)!;
        var esperado = modelo(ModeloRua.Atributos(TermometroDaSala.Api.Dominio.Relogio.HoraLocal(Agora), 28, 50, 1.5));
        var previsor = new PrevisorRuaModelo(b.Db, NullLogger<PrevisorRuaModelo>.Instance, caminho);
        Assert.Equal(esperado, previsor.PreverDaquiA2h(Agora));
    }

    [Fact]
    public void RuaRecenteInvalidaNaoSubstituiAValidaAnterior()
    {
        using var b = new Banco();
        b.Rua(Agora.AddMinutes(-30), 28);
        b.Db.LeiturasRua.Add(new LeituraRua { MedidoEm = Agora, TemperaturaC = double.PositiveInfinity,
            UmidadePct = 50, IndiceCalor = 40 });
        b.Db.LeiturasRua.Add(new LeituraRua { MedidoEm = Agora, TemperaturaC = 30,
            UmidadePct = 101, IndiceCalor = 40 });
        b.Db.SaveChanges();
        var previsor = new PrevisorRuaBaseline(b.Db);
        Assert.Equal(28, previsor.PreverDaquiA2h(Agora));
        Assert.Equal(Agora.AddMinutes(-30), previsor.InstanteBase(Agora));
        Assert.Equal(28, previsor.IndicesNosInstantes([Agora])[Agora]);
    }

    sealed class Banco : IDisposable
    {
        readonly SqliteConnection _conexao = new("Data Source=:memory:");
        public SalaDb Db { get; }

        public Banco()
        {
            _conexao.Open();
            var opcoes = new DbContextOptionsBuilder<SalaDb>();
            SqliteDeTeste.Usar(opcoes, _conexao);
            Db = new SalaDb(opcoes.Options);
            Db.Database.EnsureCreated();
        }

        public ConsultaPrevisao Consulta(DateTimeOffset? agora = null, IPrevisorRua? rua = null) =>
            new(Db, rua ?? new PrevisorRuaBaseline(Db), new RelogioFixo(agora ?? Agora), new AmbienteTeste());

        public void Rua(DateTimeOffset quando, double indice) => Db.LeiturasRua.Add(new LeituraRua
        { MedidoEm = quando, TemperaturaC = 25, UmidadePct = 60, IndiceCalor = indice });

        public void Interna(string sala, string sensor, DateTimeOffset quando, double indice, bool suspeita = false)
        {
            var id = $"{sala}-{sensor}";
            if (!Db.Dispositivos.Local.Any(d => d.Id == id))
                Db.Dispositivos.Add(new Dispositivo { Id = id, AmbienteId = sala });
            Db.Leituras.Add(new Leitura { AmbienteId = sala, DispositivoId = id, MedidoEm = quando,
                TemperaturaC = 25, UmidadePct = 60, IndiceCalor = indice, Suspeita = suspeita });
        }

        public void Dispose() { Db.Dispose(); _conexao.Dispose(); }
    }

    sealed class AmbienteTeste : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "AjusteSalaTestes";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    sealed class SemRua : IPrevisorRua
    {
        public double? IndiceNoInstante(DateTimeOffset utc) => null;
        public double? TemperaturaNoInstante(DateTimeOffset utc) => null;
        public double? PreverDaquiA2h(DateTimeOffset utc) => null;
    }
}
