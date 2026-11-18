using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;

namespace TermometroDaSala.Api.Servicos;

public sealed record ResultadoLeitura(int Status, string? Motivo, long? Id, bool Suspeita, double Indice, string Nivel);

public class ServicoLeituras
{
    private readonly SalaDb _db;
    private readonly TimeProvider _tempo;
    private readonly IPrevisorRua _rua;
    private readonly ILogger<ServicoLeituras> _log;

    public ServicoLeituras(SalaDb db, TimeProvider tempo, IPrevisorRua rua, ILogger<ServicoLeituras> log)
    {
        _db = db;
        _tempo = tempo;
        _rua = rua;
        _log = log;
    }

    public async Task<ResultadoLeitura> Receber(LeituraEntrada entrada, Origem origem = Origem.dispositivo)
    {
        var dispositivo = await _db.Dispositivos.AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == entrada.Dispositivo);
        var motivo = ValidadorLeitura.Motivo(
            dispositivo?.AmbienteId,
            dispositivo?.Ativo,
            dispositivo?.Segredo ?? "",
            entrada.AmbienteId,
            entrada.Dispositivo,
            entrada.TemperaturaC,
            entrada.UmidadePct,
            entrada.MedidoEm,
            entrada.Assinatura,
            _tempo.GetUtcNow(),
            dispositivo?.UltimoMedidoEm?.ToUnixTimeSeconds());
        if (motivo is not null)
        {
            _log.LogWarning("leitura recusada: {Motivo}", motivo);
            return new ResultadoLeitura(400, motivo, null, false, 0, "");
        }

        var quando = DateTimeOffset.FromUnixTimeSeconds(entrada.MedidoEm);
        // Reserva e leitura são atômicas, inclusive entre HTTP e MQTT em processos diferentes.
        await using var transacao = await _db.Database.BeginTransactionAsync();
        var reservados = await _db.Dispositivos
            .Where(d => d.Id == entrada.Dispositivo && d.Ativo
                && (d.UltimoMedidoEm == null || d.UltimoMedidoEm < quando))
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.UltimoMedidoEm, quando));
        if (reservados == 0)
        {
            var ativo = await _db.Dispositivos.Where(d => d.Id == entrada.Dispositivo)
                .Select(d => (bool?)d.Ativo).SingleOrDefaultAsync();
            motivo = ativo is null ? "assinatura-invalida" : ativo.Value ? "reenvio" : "dispositivo-revogado";
            _log.LogWarning("leitura recusada: {Motivo}", motivo);
            return new ResultadoLeitura(400, motivo, null, false, 0, "");
        }

        var anterior = await _db.Leituras.Where(l => l.DispositivoId == entrada.Dispositivo)
            .OrderByDescending(l => l.MedidoEm).Select(l => (double?)l.TemperaturaC).FirstOrDefaultAsync();
        var suspeita = ValidadorLeitura.Suspeita(entrada.TemperaturaC, anterior, _rua.TemperaturaNoInstante(quando));
        if (suspeita) _log.LogWarning("leitura marcada: {Motivo}", "suspeita");

        var indice = IndiceCalor.Calcular(entrada.TemperaturaC, entrada.UmidadePct);
        var leitura = new Leitura
        {
            AmbienteId = entrada.AmbienteId,
            DispositivoId = entrada.Dispositivo,
            TemperaturaC = entrada.TemperaturaC,
            UmidadePct = entrada.UmidadePct,
            IndiceCalor = indice,
            Nivel = IndiceCalor.Nivel(indice),
            MedidoEm = quando,
            Origem = origem,
            Suspeita = suspeita,
        };
        _db.Leituras.Add(leitura);
        await _db.SaveChangesAsync();
        await transacao.CommitAsync();
        _log.LogInformation("leitura aceita {Ambiente} {Nivel}", entrada.AmbienteId, leitura.Nivel);
        return new ResultadoLeitura(201, null, leitura.Id, suspeita, indice, leitura.Nivel.ToString());
    }
}
