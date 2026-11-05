using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class LeiturasController(SalaDb _db, ServicoLeituras _leituras) : ControllerBase
{
    [HttpGet("/ambientes/{id}/leituras")]
    public async Task<IActionResult> Leituras(string id, [FromQuery] string? formato)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == id)) return NotFound();
        var linhas = (await _db.Leituras.Where(l => l.AmbienteId == id).ToListAsync()).OrderBy(l => l.MedidoEm).ToList();
        if (!string.Equals(formato, "csv", StringComparison.OrdinalIgnoreCase))
            return Ok(linhas.Select(l => new LeituraSaida
            {
                MedidoEm = l.MedidoEm,
                TemperaturaC = l.TemperaturaC,
                UmidadePct = l.UmidadePct,
                IndiceCalor = l.IndiceCalor,
                Nivel = l.Nivel.ToString(),
                Origem = l.Origem.ToString(),
                Suspeita = l.Suspeita,
                DispositivoId = l.DispositivoId,
            }));

        var csv = new StringBuilder("medidoEm,temperaturaC,umidadePct,indiceCalor,nivel,origem,suspeita\n");
        foreach (var l in linhas)
            csv.Append(CultureInfo.InvariantCulture, $"{l.MedidoEm:o},{l.TemperaturaC:0.0},{l.UmidadePct},{l.IndiceCalor:0.0},{l.Nivel},{l.Origem},{l.Suspeita}\n");
        return Content(csv.ToString(), "text/csv; charset=utf-8");
    }

    [HttpPost("/leituras")]
    public async Task<IActionResult> PostLeitura(LeituraEntrada entrada)
    {
        var r = await _leituras.Receber(entrada);
        if (r.Status != 201) return BadRequest(new MotivoSaida(r.Motivo!));
        return Created($"/ambientes/{entrada.AmbienteId}/leituras", new LeituraCriadaSaida(r.Id!.Value, r.Suspeita, r.Indice, r.Nivel));
    }

}
