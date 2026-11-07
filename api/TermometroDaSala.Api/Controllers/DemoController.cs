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
public class DemoController(SalaDb _db, IHostEnvironment _ambiente, ILogger<DemoController> _log, TimeProvider tempo) : ControllerBase
{
    [HttpPost("/demo/mapa")]
    [EnableRateLimiting("escrita")]
    public async Task<IActionResult> SemearMapa([FromQuery] string periodo = "tarde")
    {
        if (!_ambiente.IsEnvironment("Demo")) return NotFound();
        if (periodo is not ("tarde" or "madrugada")) return BadRequest(new MotivoSaida("periodo-invalido"));
        var sala = await _db.Ambientes.Include(a => a.Dispositivos).Include(a => a.Lugares)
            .AsSplitQuery().SingleOrDefaultAsync(a => a.Id == "sala-07");
        if (sala is not null && (sala.Nome != "Sala 7" || await _db.Leituras.AnyAsync(l => l.AmbienteId == sala.Id && l.Origem == Origem.dispositivo)))
            return Conflict(new MotivoSaida("ambiente-em-uso"));
        var rotulos = new[] { "janela", "fundo", "porta", "centro" };
        if (sala is null)
        {
            sala = new Ambiente { Id = "sala-07", Nome = "Sala 7", LarguraM = 6, ProfundidadeM = 4 };
            var xs = new double[] { 0, 6, 6, 3 };
            var ys = new double[] { 2, 4, 0, 2 };
            for (var i = 0; i < 4; i++)
                sala.Dispositivos.Add(new Dispositivo { Id = $"demo-sala-07-{rotulos[i]}", Rotulo = rotulos[i], PosX = xs[i], PosY = ys[i], Segredo = "somente-demonstracao-sala-07" });
            for (var i = 0; i < 6; i++)
                sala.Lugares.Add(new Lugar { Nome = $"Cadeira {i + 1}", X = i % 3 * 3, Y = i < 3 ? 2 : 4, Vulneravel = i is 0 or 5 });
            _db.Ambientes.Add(sala);
        }
        if (sala.Dispositivos.Count != 4 || sala.Dispositivos.Any(d => !rotulos.Contains(d.Rotulo)))
            return Conflict(new MotivoSaida("ambiente-em-uso"));
        _db.Leituras.RemoveRange(await _db.Leituras.Where(l => l.AmbienteId == sala.Id && l.Origem == Origem.simulada).ToListAsync());
        var agora = tempo.GetUtcNow();
        foreach (var d in sala.Dispositivos)
        {
            var t = periodo == "tarde" ? (d.Rotulo == "janela" ? 35 : 24)
                : (d.Rotulo == "fundo" ? 15 : 22);
            var indice = IndiceCalor.Calcular(t, 60);
            _db.Leituras.Add(new Leitura { AmbienteId = sala.Id, DispositivoId = d.Id, TemperaturaC = t,
                UmidadePct = 60, IndiceCalor = indice, Nivel = IndiceCalor.Nivel(indice), MedidoEm = agora, Origem = Origem.simulada });
        }
        await _db.SaveChangesAsync();
        return Created("/ambientes/sala-07/mapa", new DemoSaida(4));
    }

    [HttpPost("/demo/semear")]
    [EnableRateLimiting("escrita")]
    public async Task<IActionResult> Semear([FromQuery] string ambienteId)
    {
        if (!_ambiente.IsEnvironment("Demo")) { _log.LogWarning("demo semear recusado: fora-do-ambiente"); return NotFound(); }
        if (!await _db.Ambientes.AnyAsync(a => a.Id == ambienteId)) return NotFound();
        var aoVivo = await _db.Leituras.Where(l => l.AmbienteId == ambienteId && l.Origem == Origem.dispositivo)
            .OrderBy(l => l.MedidoEm)
            .ToListAsync();
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
                Nivel = IndiceCalor.Nivel(indice),
                MedidoEm = primeira.AddMinutes(-linha.Minutos),
                Origem = Origem.simulada,
            });
            n++;
        }
        await _db.SaveChangesAsync();
        return Created("/demo/semear", new DemoSaida(n));
    }

}
