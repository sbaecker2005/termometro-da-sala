using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Filtros;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class AcoesController(SalaDb _db, TimeProvider _tempo) : ControllerBase
{
    [HttpGet("/acoes")]
    public async Task<IActionResult> Listar([FromQuery] string? dia)
    {
        if (!DateOnly.TryParseExact(dia, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var data)) return BadRequest(new MotivoSaida("dia-invalido"));
        // shortcut: filtra na memória para suportar DateTimeOffset no SQLite; mover ao SQL quando o histórico crescer.
        var acoes = await _db.Acoes.AsNoTracking().ToListAsync();
        return Ok(acoes.Where(a => DateOnly.FromDateTime(Relogio.HoraLocal(a.RegistradaEm).DateTime) == data)
            .OrderByDescending(a => a.RegistradaEm).ThenByDescending(a => a.Id)
            .Select(a => new AcaoSaida(a.Id, a.AmbienteId, a.Nivel.ToString(), a.Descricao, a.RegistradaEm)));
    }

    [HttpPost("/acoes")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Acao([FromBody] AcaoEntrada entrada)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == entrada.AmbienteId)) return NotFound();
        if (!IndiceCalor.EhNivel(entrada.Nivel) || string.IsNullOrWhiteSpace(entrada.Descricao)) return BadRequest();
        var acao = new Acao
        {
            AmbienteId = entrada.AmbienteId,
            Nivel = Enum.Parse<Nivel>(entrada.Nivel),
            Descricao = entrada.Descricao.Trim(),
            RegistradaEm = _tempo.GetUtcNow(),
        };
        _db.Acoes.Add(acao);
        await _db.SaveChangesAsync();
        return Created($"/acoes/{acao.Id}", new AcaoCriadaSaida(acao.Id));
    }

}
