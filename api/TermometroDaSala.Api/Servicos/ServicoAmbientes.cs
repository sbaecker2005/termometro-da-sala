using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Controllers;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;

namespace TermometroDaSala.Api.Servicos;

public enum ResultadoAmbiente { Ok, NaoEncontrado, Existente, ComLeituras }

// Casos de uso das salas. O controller só traduz o resultado em status HTTP.
public interface IServicoAmbientes
{
    Task<List<AmbienteSaida>> Listar(string? id = null);
    Task<bool> Existe(string id);
    Task<ResultadoAmbiente> Cadastrar(AmbienteEntrada entrada);
    Task<ResultadoAmbiente> Alterar(AmbienteEntrada entrada);
    Task<ResultadoAmbiente> Apagar(string id);
    Task<List<OcorrenciaSaida>?> Ocorrencias(string id);
}

public class ServicoAmbientes(SalaDb _db, IConfiguration _config, IHostEnvironment _ambiente) : IServicoAmbientes
{
    public Task<bool> Existe(string id) => _db.Ambientes.AnyAsync(a => a.Id == id);

    public async Task<ResultadoAmbiente> Cadastrar(AmbienteEntrada entrada)
    {
        if (await Existe(entrada.Id)) return ResultadoAmbiente.Existente;
        _db.Ambientes.Add(new Ambiente(entrada.Id, entrada.Nome, entrada.Andar, Enum.Parse<FaceSol>(entrada.FaceSol), entrada.TemVentilador));
        await _db.SaveChangesAsync();
        return ResultadoAmbiente.Ok;
    }

    public async Task<ResultadoAmbiente> Alterar(AmbienteEntrada entrada)
    {
        var sala = await _db.Ambientes.FindAsync(entrada.Id);
        if (sala is null) return ResultadoAmbiente.NaoEncontrado;
        sala.Alterar(entrada.Nome, entrada.Andar, Enum.Parse<FaceSol>(entrada.FaceSol), entrada.TemVentilador);
        await _db.SaveChangesAsync();
        return ResultadoAmbiente.Ok;
    }

    // Apagar leva dispositivos, ocorrências, ações e previsões (cascata) e apaga do disco as fotos das ocorrências
    // (podem mostrar pessoas, R12); com leituras, recusa para não perder o histórico.
    public async Task<ResultadoAmbiente> Apagar(string id)
    {
        var sala = await _db.Ambientes.FindAsync(id);
        if (sala is null) return ResultadoAmbiente.NaoEncontrado;
        if (await _db.Leituras.AnyAsync(l => l.AmbienteId == id)) return ResultadoAmbiente.ComLeituras;
        var fotos = await _db.Ocorrencias.Where(o => o.AmbienteId == id).Select(o => o.CaminhoFoto).ToListAsync();
        _db.Ambientes.Remove(sala);
        await _db.SaveChangesAsync();
        var pasta = OcorrenciasController.PastaFotos(_config, _ambiente);
        foreach (var foto in fotos)
            File.Delete(Path.Combine(pasta, Path.GetFileName(foto)));
        return ResultadoAmbiente.Ok;
    }

    public async Task<List<OcorrenciaSaida>?> Ocorrencias(string id)
    {
        if (!await Existe(id)) return null;
        var lista = await _db.Ocorrencias.AsNoTracking().Where(o => o.AmbienteId == id)
            .OrderByDescending(o => o.CriadaEm).ToListAsync();
        return lista.Select(o => new OcorrenciaSaida(o.Id.ToString(), o.AmbienteId, o.Descricao, o.CriadaEm)).ToList();
    }

    public async Task<List<AmbienteSaida>> Listar(string? id = null)
    {
        var consulta = id is null ? _db.Ambientes : _db.Ambientes.Where(a => a.Id == id);
        // a última leitura de cada sala sai do banco por subconsulta, sem trazer o histórico
        var linhas = await consulta.AsNoTracking().OrderBy(a => a.Id).Select(s => new
        {
            Sala = s,
            Leitura = _db.Leituras.Where(l => l.AmbienteId == s.Id).OrderByDescending(l => l.MedidoEm).ThenByDescending(l => l.Id).FirstOrDefault(),
            DispositivoId = _db.Dispositivos.Where(d => d.AmbienteId == s.Id).OrderBy(d => d.Id).Select(d => d.Id).FirstOrDefault(),
            SensorRevogado = _db.Dispositivos.Where(d => d.AmbienteId == s.Id).OrderBy(d => d.Id).Select(d => !d.Ativo).FirstOrDefault(),
        }).ToListAsync();
        return linhas.Select(x => new AmbienteSaida
        {
            Id = x.Sala.Id,
            Nome = x.Sala.Nome,
            Andar = x.Sala.Andar,
            FaceSol = x.Sala.FaceSol.ToString(),
            NivelAtual = x.Leitura?.Nivel.ToString() ?? "normal",
            IndiceAtual = x.Leitura?.IndiceCalor ?? 0,
            UltimaLeituraEm = x.Leitura is null ? "" : x.Leitura.MedidoEm.ToString("o"),
            SemLeitura = x.Leitura is null,
            TemVentilador = x.Sala.TemVentilador,
            DispositivoId = x.DispositivoId,
            SensorRevogado = x.SensorRevogado,
        }).ToList();
    }
}
