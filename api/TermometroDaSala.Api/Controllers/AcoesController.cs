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
        // o dia civil de São Paulo vira um intervalo UTC [início, fim) e o filtro roda no banco
        var inicio = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(data.ToDateTime(TimeOnly.MinValue), Relogio.SaoPaulo));
        var fim = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(data.AddDays(1).ToDateTime(TimeOnly.MinValue), Relogio.SaoPaulo));
        var acoes = await _db.Acoes.AsNoTracking()
            .Where(a => a.RegistradaEm >= inicio && a.RegistradaEm < fim)
            .OrderByDescending(a => a.RegistradaEm).ThenByDescending(a => a.Id)
            .ToListAsync();
        return Ok(acoes.Select(a => new AcaoSaida(a.Id, a.AmbienteId, a.Nivel.ToString(), a.Descricao, a.RegistradaEm)));
    }

    [HttpPost("/acoes")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Acao([FromBody] AcaoEntrada entrada)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == entrada.AmbienteId)) return NotFound();
        if (!IndiceCalor.EhNivel(entrada.Nivel)) return BadRequest(new MotivoSaida("nivel"));
        if (string.IsNullOrWhiteSpace(entrada.Descricao)) return BadRequest(new MotivoSaida("descricao"));
        var acao = new Acao
        {
            AmbienteId = entrada.AmbienteId,
            Nivel = Enum.Parse<Nivel>(entrada.Nivel),
            Descricao = entrada.Descricao.Trim(),
            RegistradaEm = _tempo.GetUtcNow(),
        };
        _db.Acoes.Add(acao);
        await _db.SaveChangesAsync();
        var dia = DateOnly.FromDateTime(Relogio.HoraLocal(acao.RegistradaEm).DateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return Created($"/acoes?dia={dia}", new AcaoCriadaSaida(acao.Id));
    }

}
