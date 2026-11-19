using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Filtros;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class DispositivosController(SalaDb _db) : ControllerBase
{
    [HttpPost("/dispositivos/{id}/revogar")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Revogar(string id)
    {
        var dispositivo = await _db.Dispositivos.FindAsync(id);
        if (dispositivo is null) return NotFound();
        dispositivo.Revogar();
        await _db.SaveChangesAsync();
        return Ok(new DispositivoSaida(dispositivo.Id, false));
    }

}
