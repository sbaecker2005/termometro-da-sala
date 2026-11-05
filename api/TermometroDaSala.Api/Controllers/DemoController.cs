using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class DemoController(SalaDb _db, IHostEnvironment _ambiente, ILogger<DemoController> _log) : ControllerBase
{
    [HttpPost("/demo/semear")]
    [EnableRateLimiting("escrita")]
    public async Task<IActionResult> Semear([FromQuery] string ambienteId)
    {
        if (!_ambiente.IsEnvironment("Demo")) { _log.LogWarning("demo semear recusado: fora-do-ambiente"); return NotFound(); }
        if (!await _db.Ambientes.AnyAsync(a => a.Id == ambienteId)) return NotFound();
        var aoVivo = (await _db.Leituras.Where(l => l.AmbienteId == ambienteId && l.Origem == Origem.dispositivo).ToListAsync())
            .OrderBy(l => l.MedidoEm)
            .ToList();
        if (aoVivo.Count == 0) return Conflict(new MotivoSaida("ligue o sensor primeiro"));

        var dispositivo = await _db.Dispositivos.FirstAsync(d => d.AmbienteId == ambienteId);
        var linhas = System.IO.File.ReadLines(ArquivosDemo.Achar(_ambiente.ContentRootPath, "leituras-simuladas.csv"))
            .Skip(1)
            .Select(l => l.Split(','))
            .Where(c => c.Length >= 3)
            .Select(c => (
                Minutos: int.Parse(c[0], CultureInfo.InvariantCulture),
                Temp: double.Parse(c[1], CultureInfo.InvariantCulture),
                Umid: int.Parse(c[2], CultureInfo.InvariantCulture)))
            .Where(l => l.Minutos is > 0 and <= 180)
            .OrderByDescending(l => l.Minutos)
            .ToList();
        if (linhas.Count == 0) return Conflict(new MotivoSaida("csv-vazio"));

        var ancora = linhas[^1];
        var ultima = aoVivo[^1];
        var dTemp = ultima.TemperaturaC - ancora.Temp;
        var dUmid = ultima.UmidadePct - ancora.Umid;
        var primeira = aoVivo[0].MedidoEm;
        var velhas = _db.Leituras.Where(l => l.AmbienteId == ambienteId && l.Origem == Origem.simulada);
        _db.Leituras.RemoveRange(velhas);

        var n = 0;
        foreach (var linha in linhas)
        {
            var temp = Math.Clamp(linha.Temp + dTemp, -10, 60);
            var umid = Math.Clamp(linha.Umid + dUmid, 0, 100);
            var indice = IndiceCalor.Calcular(temp, umid);
            _db.Leituras.Add(new Leitura
            {
                AmbienteId = ambienteId,
                DispositivoId = dispositivo.Id,
                TemperaturaC = temp,
                UmidadePct = umid,
                IndiceCalor = indice,
                Nivel = Enum.Parse<Nivel>(IndiceCalor.Nivel(indice)),
                MedidoEm = primeira.AddMinutes(-linha.Minutos),
                Origem = Origem.simulada,
            });
            n++;
        }
        await _db.SaveChangesAsync();
        return Created("/demo/semear", new DemoSaida(n));
    }

}
