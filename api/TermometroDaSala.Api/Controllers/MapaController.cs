using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Filtros;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class MapaController(SalaDb db, TimeProvider tempo) : ControllerBase
{
    [HttpGet("/ambientes/{id}/mapa")]
    public async Task<IActionResult> Obter(string id)
    {
        var sala = await db.Ambientes.AsNoTracking().Include(a => a.Dispositivos).Include(a => a.Lugares).AsSplitQuery().SingleOrDefaultAsync(a => a.Id == id);
        if (sala is null) return NotFound();
        var agora = tempo.GetUtcNow();
        var leituras = await db.Leituras.AsNoTracking().Where(l => l.AmbienteId == id && l.MedidoEm >= agora.AddMinutes(-15) && l.MedidoEm <= agora).ToListAsync();
        return Ok(MapaTermico.Montar(sala, leituras, agora));
    }

    [HttpPut("/ambientes/{id}/mapa")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Configurar(string id, MapaEntrada entrada)
    {
        var sala = await db.Ambientes.Include(a => a.Dispositivos).Include(a => a.Lugares).AsSplitQuery().SingleOrDefaultAsync(a => a.Id == id);
        if (sala is null) return NotFound();
        var largura = entrada.LarguraM;
        var profundidade = entrada.ProfundidadeM;
        bool Dentro(double x, double y) => double.IsFinite(x) && double.IsFinite(y) && x >= 0 && y >= 0
            && largura.HasValue && profundidade.HasValue && x <= largura && y <= profundidade;
        if (largura.HasValue != profundidade.HasValue
            || entrada.Sensores.Any(s => s is null) || entrada.Lugares.Any(l => l is null)
            || entrada.Sensores.Select(s => s.Id).Distinct().Count() != entrada.Sensores.Count
            || entrada.Lugares.Any(l => string.IsNullOrWhiteSpace(l.Nome))
            || entrada.Lugares.Select(l => l.Nome.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != entrada.Lugares.Count
            || entrada.Lugares.Any(l => !Dentro(l.X, l.Y)))
            return BadRequest(new MotivoSaida("mapa-invalido"));
        foreach (var sensor in entrada.Sensores)
            if (!sala.Dispositivos.Any(d => d.Id == sensor.Id)
                || sensor.PosX.HasValue != sensor.PosY.HasValue
                || (sensor.PosX.HasValue && !Dentro(sensor.PosX.Value, sensor.PosY!.Value)))
                return BadRequest(new MotivoSaida("sensor-invalido"));
        foreach (var d in sala.Dispositivos)
        {
            var s = entrada.Sensores.SingleOrDefault(s => s.Id == d.Id);
            if (s is null && d.PosX.HasValue && !Dentro(d.PosX.Value, d.PosY ?? 0))
                return BadRequest(new MotivoSaida("sensor-fora-da-planta"));
        }
        sala.LarguraM = largura;
        sala.ProfundidadeM = profundidade;
        foreach (var s in entrada.Sensores)
        {
            var d = sala.Dispositivos.Single(d => d.Id == s.Id);
            d.PosX = s.PosX;
            d.PosY = s.PosY;
            d.Rotulo = string.IsNullOrWhiteSpace(s.Rotulo) ? null : s.Rotulo.Trim();
        }
        var nomes = entrada.Lugares.Select(l => l.Nome.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        db.Lugares.RemoveRange(sala.Lugares.Where(l => !nomes.Contains(l.Nome)));
        foreach (var l in entrada.Lugares)
        {
            var lugar = sala.Lugares.SingleOrDefault(p => string.Equals(p.Nome, l.Nome.Trim(), StringComparison.OrdinalIgnoreCase));
            if (lugar is null) { lugar = new Lugar { AmbienteId = id, Nome = l.Nome.Trim() }; db.Lugares.Add(lugar); }
            lugar.X = l.X;
            lugar.Y = l.Y;
            lugar.Vulneravel = l.Vulneravel;
        }
        await db.SaveChangesAsync();
        return NoContent();
    }
}
