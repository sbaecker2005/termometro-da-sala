using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class OcorrenciasController(SalaDb _db, IHostEnvironment _ambiente, IConfiguration _config, TimeProvider _tempo, ILogger<OcorrenciasController> _log) : ControllerBase
{
    [HttpPost("/ocorrencias")]
    [EnableRateLimiting("escrita")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Ocorrencia([FromForm] OcorrenciaEntrada entrada)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == entrada.AmbienteId)) return NotFound();
        var texto = (entrada.Descricao ?? "").Trim();
        if (texto.Length is < 10 or > 500) return BadRequest(new MotivoSaida("descricao"));
        if (entrada.Foto is null || entrada.Foto.Length is 0 or > 5 * 1024 * 1024) return BadRequest(new MotivoSaida("arquivo"));
        if (string.IsNullOrWhiteSpace(entrada.Foto.FileName) || entrada.Foto.FileName.Contains("..") || entrada.Foto.FileName.Contains('/') || entrada.Foto.FileName.Contains('\\'))
            return Recusar("nome-invalido");
        if (!await Jpeg(entrada.Foto)) return BadRequest(new MotivoSaida("nao-jpeg"));

        await using var bruto = new MemoryStream();
        await entrada.Foto.CopyToAsync(bruto);
        var limpo = SemApp1(bruto.ToArray());
        if (limpo.Length < 3 || limpo[0] != 0xFF || limpo[1] != 0xD8) return BadRequest(new MotivoSaida("nao-jpeg"));

        var nome = Guid.NewGuid().ToString("N") + ".jpg";
        var pasta = PastaFotos();
        Directory.CreateDirectory(pasta);
        await System.IO.File.WriteAllBytesAsync(Path.Combine(pasta, nome), limpo);

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
    }

    // shortcut: não junta 0xFF de preenchimento antes do marcador; reescrever se uma câmera gravar padding.
    // Devolve vazio se faltar quadro (SOF com altura e largura) ou varredura (SOS): confere a estrutura, não decodifica os pixels.
    private static byte[] SemApp1(byte[] jpeg)
    {
        var temQuadro = false;
        var entrada = new MemoryStream(jpeg);
        var saida = new MemoryStream(jpeg.Length);
        var marcador = new byte[2];
        var tam = new byte[2];
        if (entrada.Read(marcador, 0, 2) != 2 || marcador[0] != 0xFF || marcador[1] != 0xD8) return [];
        saida.Write(marcador, 0, 2);
        while (true)
        {
            if (entrada.Read(marcador, 0, 2) != 2 || marcador[0] != 0xFF) break;
            var m = marcador[1];
            if (m == 0xD9) return []; // fim antes da varredura: não há imagem
            if (m == 0xDA)
            {
                if (!temQuadro) return [];
                saida.Write(marcador, 0, 2);
                entrada.CopyTo(saida);
                return saida.ToArray();
            }
            if (m == 0x01 || m is >= 0xD0 and <= 0xD8)
            {
                saida.Write(marcador, 0, 2);
                continue;
            }
            if (entrada.Read(tam, 0, 2) != 2) break;
            var n = (tam[0] << 8) | tam[1];
            if (n < 2 || entrada.Length - entrada.Position < n - 2) break;
            var dados = new byte[n - 2];
            if (entrada.Read(dados, 0, dados.Length) != dados.Length) break;
            if (m == 0xE1) continue;
            // SOF0..SOF15 (C4 é DHT, C8 é reservado, CC é DAC): precisão, altura (2), largura (2)
            if (m is >= 0xC0 and <= 0xCF and not (0xC4 or 0xC8 or 0xCC) && dados.Length >= 5)
                temQuadro = ((dados[1] << 8) | dados[2]) > 0 && ((dados[3] << 8) | dados[4]) > 0;
            saida.Write(marcador, 0, 2);
            saida.Write(tam, 0, 2);
            saida.Write(dados, 0, dados.Length);
        }
        return [];
    }

    private BadRequestObjectResult Recusar(string motivo)
    {
        _log.LogWarning("ocorrencia recusada: {Motivo}", motivo);
        return BadRequest(new MotivoSaida(motivo));
    }
}
