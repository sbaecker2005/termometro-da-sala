using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Filtros;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class AmbientesController(SalaDb _db, ConsultaPrevisao _previsao, IConfiguration _config, IHostEnvironment _ambiente) : ControllerBase
{
    [HttpPost("/ambientes")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Cadastrar(AmbienteEntrada entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada.Nome)) return BadRequest(new MotivoSaida("nome"));
        if (await _db.Ambientes.AnyAsync(a => a.Id == entrada.Id))
            return Conflict(new MotivoSaida("ambiente-existente"));
        var sala = new Ambiente { Id = entrada.Id };
        Preencher(sala, entrada);
        _db.Ambientes.Add(sala);
        await _db.SaveChangesAsync();
        return Created($"/ambientes/{sala.Id}", new IdSaida(sala.Id));
    }

    [HttpGet("/ambientes")]
    public async Task<IActionResult> Ambientes() => Ok(await Saidas(_db.Ambientes));

    [HttpGet("/ambientes/{id}")]
    public async Task<IActionResult> Ambiente(string id)
    {
        var sala = (await Saidas(_db.Ambientes.Where(a => a.Id == id))).SingleOrDefault();
        return sala is null ? NotFound() : Ok(sala);
    }

    [HttpPut("/ambientes/{id}")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Alterar(string id, AmbienteEntrada entrada)
    {
        if (entrada.Id != id) return BadRequest(new MotivoSaida("id-divergente"));
        if (string.IsNullOrWhiteSpace(entrada.Nome)) return BadRequest(new MotivoSaida("nome"));
        var sala = await _db.Ambientes.FindAsync(id);
        if (sala is null) return NotFound();
        Preencher(sala, entrada);
        await _db.SaveChangesAsync();
        return Ok(new IdSaida(sala.Id));
    }

    // Apagar leva dispositivos, ocorrências, ações e previsões (cascata) e apaga do disco as fotos das ocorrências
    // (podem mostrar pessoas, R12); com leituras, 409 para não perder o histórico.
    [HttpDelete("/ambientes/{id}")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Apagar(string id)
    {
        var sala = await _db.Ambientes.FindAsync(id);
        if (sala is null) return NotFound();
        if (await _db.Leituras.AnyAsync(l => l.AmbienteId == id))
            return Conflict(new MotivoSaida("ambiente-com-leituras"));
        var fotos = await _db.Ocorrencias.Where(o => o.AmbienteId == id).Select(o => o.CaminhoFoto).ToListAsync();
        _db.Ambientes.Remove(sala);
        await _db.SaveChangesAsync();
        var pasta = OcorrenciasController.PastaFotos(_config, _ambiente);
        foreach (var foto in fotos)
            System.IO.File.Delete(Path.Combine(pasta, Path.GetFileName(foto)));
        return NoContent();
    }

    [HttpGet("/ambientes/{id}/ocorrencias")]
    public async Task<IActionResult> Ocorrencias(string id)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == id)) return NotFound();
        var lista = await _db.Ocorrencias.AsNoTracking().Where(o => o.AmbienteId == id)
            .OrderByDescending(o => o.CriadaEm).ToListAsync();
        return Ok(lista.Select(o => new OcorrenciaSaida(o.Id.ToString(), o.AmbienteId, o.Descricao, o.CriadaEm)));
    }

    [HttpGet("/ambientes/{id}/previsao")]
    public async Task<IActionResult> Previsao(string id)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == id)) return NotFound();
        var corpo = await _previsao.Montar(id);
        return corpo is null ? StatusCode(StatusCodes.Status503ServiceUnavailable, new MotivoSaida("sem-dados-para-prever")) : Ok(corpo);
    }

    static void Preencher(Ambiente sala, AmbienteEntrada entrada)
    {
        sala.Nome = entrada.Nome.Trim();
        sala.Andar = entrada.Andar;
        sala.FaceSol = Enum.Parse<FaceSol>(entrada.FaceSol);
        sala.TemVentilador = entrada.TemVentilador;
    }

    async Task<List<AmbienteSaida>> Saidas(IQueryable<Ambiente> consulta)
    {
        // a última leitura de cada sala sai do banco por subconsulta, sem trazer o histórico
        var linhas = await consulta.AsNoTracking().OrderBy(a => a.Id).Select(s => new
        {
            Sala = s,
            Leitura = _db.Leituras.Where(l => l.AmbienteId == s.Id).OrderByDescending(l => l.MedidoEm).ThenByDescending(l => l.Id).FirstOrDefault(),
            DispositivoId = _db.Dispositivos.Where(d => d.AmbienteId == s.Id).OrderBy(d => d.Id).Select(d => d.Id).FirstOrDefault(),
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
        }).ToList();
    }
}
