using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Filtros;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class AmbientesController(IServicoAmbientes _salas, ConsultaPrevisao _previsao) : ControllerBase
{
    [HttpPost("/ambientes")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Cadastrar(AmbienteEntrada entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada.Nome)) return BadRequest(new MotivoSaida("nome"));
        return await _salas.Cadastrar(entrada) == ResultadoAmbiente.Existente
            ? Conflict(new MotivoSaida("ambiente-existente"))
            : Created($"/ambientes/{entrada.Id}", new IdSaida(entrada.Id));
    }

    [HttpGet("/ambientes")]
    public async Task<IActionResult> Ambientes() => Ok(await _salas.Listar());

    [HttpGet("/ambientes/{id}")]
    public async Task<IActionResult> Ambiente(string id)
    {
        var sala = (await _salas.Listar(id)).SingleOrDefault();
        return sala is null ? NotFound() : Ok(sala);
    }

    [HttpPut("/ambientes/{id}")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Alterar(string id, AmbienteEntrada entrada)
    {
        if (entrada.Id != id) return BadRequest(new MotivoSaida("id-divergente"));
        if (string.IsNullOrWhiteSpace(entrada.Nome)) return BadRequest(new MotivoSaida("nome"));
        return await _salas.Alterar(entrada) == ResultadoAmbiente.NaoEncontrado ? NotFound() : Ok(new IdSaida(id));
    }

    // Com leituras, 409 para não perder o histórico; o resto da sala sai em cascata (ServicoAmbientes.Apagar).
    [HttpDelete("/ambientes/{id}")]
    [EnableRateLimiting("escrita")]
    [ExigeChaveCoordenacao]
    public async Task<IActionResult> Apagar(string id) => await _salas.Apagar(id) switch
    {
        ResultadoAmbiente.NaoEncontrado => NotFound(),
        ResultadoAmbiente.ComLeituras => Conflict(new MotivoSaida("ambiente-com-leituras")),
        _ => NoContent(),
    };

    [HttpGet("/ambientes/{id}/ocorrencias")]
    public async Task<IActionResult> Ocorrencias(string id)
    {
        var lista = await _salas.Ocorrencias(id);
        return lista is null ? NotFound() : Ok(lista);
    }

    [HttpGet("/ambientes/{id}/previsao")]
    public async Task<IActionResult> Previsao(string id)
    {
        if (!await _salas.Existe(id)) return NotFound();
        var corpo = await _previsao.Montar(id);
        return corpo is null ? StatusCode(StatusCodes.Status503ServiceUnavailable, new MotivoSaida("sem-dados-para-prever")) : Ok(corpo);
    }
}
