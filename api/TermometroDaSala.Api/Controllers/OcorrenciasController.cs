using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class OcorrenciasController(SalaDb _db, IHostEnvironment _ambiente, IConfiguration _config, TimeProvider _tempo) : ControllerBase
{
    [HttpPost("/ocorrencias")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Ocorrencia([FromForm] OcorrenciaEntrada entrada)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == entrada.AmbienteId)) return NotFound();
        var texto = (entrada.Descricao ?? "").Trim();
        if (texto.Length is < 10 or > 500) return BadRequest(new MotivoSaida("descricao"));
        if (entrada.Foto is null || entrada.Foto.Length is 0 or > 5 * 1024 * 1024) return BadRequest(new MotivoSaida("arquivo"));
        if (string.IsNullOrWhiteSpace(entrada.Foto.FileName) || entrada.Foto.FileName.Contains("..") || entrada.Foto.FileName.Contains('/') || entrada.Foto.FileName.Contains('\\'))
            return BadRequest(new MotivoSaida("nome-invalido"));
        if (!await Jpeg(entrada.Foto)) return BadRequest(new MotivoSaida("nao-jpeg"));

        var nome = Guid.NewGuid().ToString("N") + ".jpg";
        var pasta = PastaFotos();
        Directory.CreateDirectory(pasta);
        await using (var destino = System.IO.File.Create(Path.Combine(pasta, nome)))
            await entrada.Foto.CopyToAsync(destino);

        var id = Guid.NewGuid();
        _db.Ocorrencias.Add(new Ocorrencia { Id = id, AmbienteId = entrada.AmbienteId, Descricao = texto, CaminhoFoto = nome, CriadaEm = _tempo.GetUtcNow() });
        await _db.SaveChangesAsync();
        return Created($"/ocorrencias/{id}/foto", new IdSaida(id.ToString()));
    }

    [HttpGet("/ocorrencias/{id:guid}/foto")]
    public async Task<IActionResult> Foto(Guid id)
    {
        var ocorrencia = await _db.Ocorrencias.FindAsync(id);
        if (ocorrencia is null) return NotFound();
        var nome = Path.GetFileName(ocorrencia.CaminhoFoto);
        var caminho = Path.Combine(PastaFotos(), nome);
        if (!System.IO.File.Exists(caminho)) return NotFound();
        return PhysicalFile(caminho, "image/jpeg");
    }

    private string PastaFotos() =>
        Path.IsPathRooted(_config["Fotos:Pasta"] ?? "")
            ? _config["Fotos:Pasta"]!
            : Path.Combine(_ambiente.ContentRootPath, _config["Fotos:Pasta"] ?? "fotos");

    private static async Task<bool> Jpeg(IFormFile foto)
    {
        if (foto.ContentType is not ("image/jpeg" or "image/jpg" or "image/pjpeg")) return false;
        var cabeca = new byte[3];
        await using var fluxo = foto.OpenReadStream();
        var lidos = await fluxo.ReadAsync(cabeca.AsMemory(0, 3));
        return lidos == 3 && cabeca[0] == 0xFF && cabeca[1] == 0xD8 && cabeca[2] == 0xFF;
    }}
