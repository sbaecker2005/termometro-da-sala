using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Contratos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class NiveisController(SalaDb _db) : ControllerBase
{
    [HttpGet("/niveis")]
    public async Task<IActionResult> Niveis()
    {
        var niveis = await _db.NiveisAcao.AsNoTracking().OrderBy(n => n.IndiceMin).ToListAsync();
        return Ok(niveis.Select(n => new NivelSaida(n.Nivel.ToString(), n.Nome, n.IndiceMin, n.AcaoSugerida)));
    }
}
