using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Contratos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class DispositivosController(SalaDb _db) : ControllerBase
{
    [HttpPost("/dispositivos/{id}/revogar")]
    public async Task<IActionResult> Revogar(string id)
    {
        var dispositivo = await _db.Dispositivos.FindAsync(id);
        if (dispositivo is null) return NotFound();
        dispositivo.Ativo = false;
        await _db.SaveChangesAsync();
        return Ok(new DispositivoSaida(dispositivo.Id, false));
    }

}
