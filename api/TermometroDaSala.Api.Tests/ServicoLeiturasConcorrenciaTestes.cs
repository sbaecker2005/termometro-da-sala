using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Tests;

public class ServicoLeiturasConcorrenciaTestes
{
    private const string DispositivoId = "esp32-sala-03";

    [Theory]
    [InlineData(0, 400, 1)]
    [InlineData(1, 400, 1)]
    [InlineData(-1, 201, 2)]
    public async Task LeituraConfirmadaEntreValidacaoEReservaNaoDuplicaNemRegride(
        int diferencaSegundos, int statusEsperado, int quantidadeEsperada)
    {
        using var banco = new BancoCompartilhado();
        using var segundo = banco.Contexto();
        var instante = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds();
        var entrada = Entrada(instante, 30);
        var concorrente = Entrada(instante + diferencaSegundos, 24);
        ResultadoLeitura? resultadoConcorrente = null;
        var intercalacao = new AntesDaTransacao(async () =>
        {
            resultadoConcorrente = await Servico(segundo).Receber(concorrente);
        });
        using var primeiro = banco.Contexto(intercalacao);

        var resultado = await Servico(primeiro).Receber(entrada);

        Assert.True(intercalacao.Executou);
        Assert.NotNull(resultadoConcorrente);
        Assert.Equal(201, resultadoConcorrente.Status);
        Assert.Equal(statusEsperado, resultado.Status);
        Assert.Equal(statusEsperado == 400 ? "reenvio" : null, resultado.Motivo);
        var leituras = await segundo.Leituras.AsNoTracking().ToListAsync();
        Assert.Equal(quantidadeEsperada, leituras.Count);
        Assert.Contains(leituras, l => l.Id == resultadoConcorrente.Id
            && l.MedidoEm.ToUnixTimeSeconds() == concorrente.MedidoEm && l.TemperaturaC == 24);
        if (statusEsperado == 201)
        {
            Assert.Contains(leituras, l => l.Id == resultado.Id
                && l.MedidoEm.ToUnixTimeSeconds() == entrada.MedidoEm && l.TemperaturaC == 30);
            // A leitura concorrente mais antiga também precisa entrar no cálculo de salto térmico.
            Assert.True(resultado.Suspeita);
        }
        else
            Assert.Null(resultado.Id);
        var ultimo = await segundo.Dispositivos.AsNoTracking().Where(d => d.Id == DispositivoId)
            .Select(d => d.UltimoMedidoEm).SingleAsync();
        Assert.Equal<DateTimeOffset?>(DateTimeOffset.FromUnixTimeSeconds(
            Math.Max(entrada.MedidoEm, concorrente.MedidoEm)), ultimo);
    }

    [Fact]
    public async Task RevogacaoConfirmadaEntreValidacaoEReservaImpedeInsercao()
    {
        using var banco = new BancoCompartilhado();
        using var segundo = banco.Contexto();
        var intercalacao = new AntesDaTransacao(async () =>
        {
            var dispositivo = await segundo.Dispositivos.SingleAsync(d => d.Id == DispositivoId);
            dispositivo.Revogar();
            await segundo.SaveChangesAsync();
        });
        using var primeiro = banco.Contexto(intercalacao);
        var entrada = Entrada(DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds(), 30);

        var resultado = await Servico(primeiro).Receber(entrada);

        Assert.True(intercalacao.Executou);
        Assert.Equal(400, resultado.Status);
        Assert.Equal("dispositivo-revogado", resultado.Motivo);
        Assert.Null(resultado.Id);
        Assert.Empty(await segundo.Leituras.AsNoTracking().ToListAsync());
        var revogado = await segundo.Dispositivos.AsNoTracking().SingleAsync(d => d.Id == DispositivoId);
        Assert.False(revogado.Ativo);
        Assert.Null(revogado.UltimoMedidoEm);
    }

    [Fact]
    public async Task FalhaAoSalvarReverteReservaEPermiteNovaTentativaDoMesmoInstante()
    {
        using var banco = new BancoCompartilhado();
        using var observador = banco.Contexto();
        var instante = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds();
        var anterior = Entrada(instante - 10, 24);
        Assert.Equal(201, (await Servico(observador).Receber(anterior)).Status);
        var falha = new FalharAoSalvar();
        using var primeiro = banco.Contexto(falha);
        var entrada = Entrada(instante, 30);

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() => Servico(primeiro).Receber(entrada));

        Assert.Equal("Falha de persistência simulada.", erro.Message);
        Assert.Equal<DateTimeOffset?>(DateTimeOffset.FromUnixTimeSeconds(instante), falha.ReservaObservada);
        var ultimo = await observador.Dispositivos.AsNoTracking().Where(d => d.Id == DispositivoId)
            .Select(d => d.UltimoMedidoEm).SingleAsync();
        Assert.Equal<DateTimeOffset?>(DateTimeOffset.FromUnixTimeSeconds(anterior.MedidoEm), ultimo);
        Assert.Equal(anterior.MedidoEm,
            Assert.Single(await observador.Leituras.AsNoTracking().ToListAsync()).MedidoEm.ToUnixTimeSeconds());

        Assert.Equal(201, (await Servico(observador).Receber(entrada)).Status);
        Assert.Equal(2, await observador.Leituras.CountAsync());
        ultimo = await observador.Dispositivos.AsNoTracking().Where(d => d.Id == DispositivoId)
            .Select(d => d.UltimoMedidoEm).SingleAsync();
        Assert.Equal<DateTimeOffset?>(DateTimeOffset.FromUnixTimeSeconds(instante), ultimo);
    }

    private static ServicoLeituras Servico(SalaDb db) =>
        new(db, TimeProvider.System, new PrevisorRuaBaseline(db), NullLogger<ServicoLeituras>.Instance);

    private static LeituraEntrada Entrada(long instante, double temperatura) => new()
    {
        AmbienteId = "sala-03",
        Dispositivo = DispositivoId,
        TemperaturaC = temperatura,
        UmidadePct = 50,
        MedidoEm = instante,
        // Vetor público de teste já usado pelo seed; não utiliza configuração privada.
        Assinatura = Assinatura.Hex("segredo-de-teste-sala-03", "sala-03", DispositivoId, temperatura, 50, instante),
    };

    private sealed class BancoCompartilhado : IDisposable
    {
        private readonly SqliteConnection conexao = new("Data Source=:memory:");

        public BancoCompartilhado()
        {
            conexao.Open();
            using var db = Contexto();
            db.Database.EnsureCreated();
        }

        public SalaDb Contexto(params IInterceptor[] interceptores)
        {
            var opcoes = new DbContextOptionsBuilder<SalaDb>();
            SqliteDeTeste.Usar(opcoes, conexao);
            opcoes.AddInterceptors(interceptores);
            return new SalaDb(opcoes.Options);
        }

        public void Dispose() => conexao.Dispose();
    }

    private sealed class AntesDaTransacao(Func<Task> concorrente) : DbTransactionInterceptor
    {
        public bool Executou { get; private set; }

        public override async ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            DbConnection connection, TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result, CancellationToken cancellationToken = default)
        {
            if (!Executou)
            {
                Executou = true;
                // O segundo contexto confirma antes de a transação do primeiro começar, sem timing ou threads.
                await concorrente();
            }
            return result;
        }
    }

    private sealed class FalharAoSalvar : SaveChangesInterceptor
    {
        public DateTimeOffset? ReservaObservada { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var db = (SalaDb)eventData.Context!;
            ReservaObservada = await db.Dispositivos.AsNoTracking().Where(d => d.Id == DispositivoId)
                .Select(d => d.UltimoMedidoEm).SingleAsync(cancellationToken);
            throw new InvalidOperationException("Falha de persistência simulada.");
        }
    }
}
