using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class AmbientesController(SalaDb _db, ConsultaPrevisao _previsao) : ControllerBase
{
    [HttpPost("/ambientes")]
    public async Task<IActionResult> Cadastrar(AmbienteEntrada entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada.Nome)) return BadRequest();
        if (await _db.Ambientes.AnyAsync(a => a.Id == entrada.Id))
            return Conflict(new MotivoSaida("ambiente-existente"));
        var sala = new Ambiente
        {
            Id = entrada.Id,
            Nome = entrada.Nome.Trim(),
            Andar = entrada.Andar,
            FaceSol = Enum.Parse<FaceSol>(entrada.FaceSol),
            TemVentilador = entrada.TemVentilador,
        };
        _db.Ambientes.Add(sala);
        await _db.SaveChangesAsync();
        return Created("/ambientes", new IdSaida(sala.Id));
    }

    [HttpGet("/ambientes")]
    public async Task<IActionResult> Ambientes()
    {
        var salas = await _db.Ambientes.OrderBy(a => a.Id).ToListAsync();
        var ultimas = await _db.Leituras.ToListAsync();
        var porSala = ultimas.GroupBy(l => l.AmbienteId).ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.MedidoEm).First());
        var dispositivos = await _db.Dispositivos.OrderBy(d => d.Id).ToListAsync();
        return Ok(salas.Select(s =>
        {
            porSala.TryGetValue(s.Id, out var leitura);
            return new AmbienteSaida
            {
                Id = s.Id,
                Nome = s.Nome,
                Andar = s.Andar,
                FaceSol = s.FaceSol.ToString(),
                NivelAtual = leitura?.Nivel.ToString() ?? "normal",
                IndiceAtual = leitura?.IndiceCalor ?? 0,
                UltimaLeituraEm = leitura is null ? "" : leitura.MedidoEm.ToString("o"),
                SemLeitura = leitura is null,
                TemVentilador = s.TemVentilador,
                DispositivoId = dispositivos.FirstOrDefault(d => d.AmbienteId == s.Id)?.Id,
            };
        }));
    }

    [HttpGet("/ambientes/{id}/previsao")]
    public async Task<IActionResult> Previsao(string id)
    {
        var corpo = await _previsao.Montar(id);
        return corpo is null ? NotFound() : Ok(corpo);
    }

}
