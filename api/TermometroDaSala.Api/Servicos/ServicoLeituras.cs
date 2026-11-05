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
        var dispositivo = await _db.Dispositivos.FindAsync(entrada.Dispositivo);
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
        // shortcut: ordena na memória, o SQLite do teste não ordena DateTimeOffset; volte ao ORDER BY se uma sala passar de um dia de leituras
        var anteriores = await _db.Leituras.Where(l => l.DispositivoId == entrada.Dispositivo).ToListAsync();
        double? anterior = anteriores.OrderByDescending(l => l.MedidoEm).FirstOrDefault()?.TemperaturaC;
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
            Nivel = Enum.Parse<Nivel>(IndiceCalor.Nivel(indice)),
            MedidoEm = quando,
            Origem = origem,
            Suspeita = suspeita,
        };
        _db.Leituras.Add(leitura);
        dispositivo!.UltimoMedidoEm = quando;
        await _db.SaveChangesAsync();
        _log.LogInformation("leitura aceita {Ambiente} {Nivel}", entrada.AmbienteId, leitura.Nivel);
        return new ResultadoLeitura(201, null, leitura.Id, suspeita, indice, leitura.Nivel.ToString());
    }
}
