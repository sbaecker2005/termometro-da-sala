using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Servicos;

namespace TermometroDaSala.Api.Controllers;

[ApiController]
public class TermometroController : ControllerBase
{
    private readonly SalaDb _db;
    private readonly ServicoLeituras _leituras;
    private readonly ConsultaPrevisao _previsao;
    private readonly IHostEnvironment _ambiente;
    private readonly IConfiguration _config;
    private readonly TimeProvider _tempo;

    public TermometroController(SalaDb db, ServicoLeituras leituras, ConsultaPrevisao previsao, IHostEnvironment ambiente, IConfiguration config, TimeProvider tempo)
    {
        _db = db;
        _leituras = leituras;
        _previsao = previsao;
        _ambiente = ambiente;
        _config = config;
        _tempo = tempo;
    }

    [HttpGet("/ambientes")]
    public async Task<IActionResult> Ambientes()
    {
        var salas = await _db.Ambientes.OrderBy(a => a.Id).ToListAsync();
        var ultimas = await _db.Leituras.ToListAsync();
        var porSala = ultimas.GroupBy(l => l.AmbienteId).ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.MedidoEm).First());
        return Ok(salas.Select(s =>
        {
            porSala.TryGetValue(s.Id, out var leitura);
            return new
            {
                id = s.Id,
                nome = s.Nome,
                andar = s.Andar,
                faceSol = s.FaceSol,
                nivelAtual = leitura?.Nivel ?? "normal",
                indiceAtual = leitura?.IndiceCalor ?? 0,
                ultimaLeituraEm = leitura is null ? "" : leitura.MedidoEm.ToString("o"),
                semLeitura = leitura is null,
            };
        }));
    }

    [HttpGet("/ambientes/{id}/leituras")]
    public async Task<IActionResult> Leituras(string id, [FromQuery] string? formato)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == id)) return NotFound();
        var linhas = (await _db.Leituras.Where(l => l.AmbienteId == id).ToListAsync()).OrderBy(l => l.MedidoEm).ToList();
        if (!string.Equals(formato, "csv", StringComparison.OrdinalIgnoreCase))
            return Ok(linhas.Select(l => new
            {
                l.MedidoEm,
                l.TemperaturaC,
                l.UmidadePct,
                l.IndiceCalor,
                l.Nivel,
                l.Origem,
                l.Suspeita,
                l.DispositivoId,
            }));

        var csv = new StringBuilder("medidoEm,temperaturaC,umidadePct,indiceCalor,nivel,origem,suspeita\n");
        foreach (var l in linhas)
            csv.Append(CultureInfo.InvariantCulture, $"{l.MedidoEm:o},{l.TemperaturaC:0.0},{l.UmidadePct},{l.IndiceCalor:0.0},{l.Nivel},{l.Origem},{l.Suspeita}\n");
        return Content(csv.ToString(), "text/csv; charset=utf-8");
    }

    [HttpGet("/ambientes/{id}/previsao")]
    public async Task<IActionResult> Previsao(string id)
    {
        var corpo = await _previsao.Montar(id);
        return corpo is null ? NotFound() : Ok(corpo);
    }

    [HttpPost("/leituras")]
    public async Task<IActionResult> PostLeitura(LeituraEntrada entrada)
    {
        var r = await _leituras.Receber(entrada);
        if (r.Status != 201) return BadRequest(new { motivo = r.Motivo });
        return Created($"/ambientes/{entrada.AmbienteId}/leituras", new { id = r.Id, suspeita = r.Suspeita, indiceCalor = r.Indice, nivel = r.Nivel });
    }

    [HttpPost("/ocorrencias")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Ocorrencia([FromForm] string ambienteId, [FromForm] string descricao, IFormFile? foto)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == ambienteId)) return NotFound();
        var texto = (descricao ?? "").Trim();
        if (texto.Length is < 10 or > 500) return BadRequest(new { motivo = "descricao" });
        if (foto is null || foto.Length is 0 or > 5 * 1024 * 1024) return BadRequest(new { motivo = "arquivo" });
        if (string.IsNullOrWhiteSpace(foto.FileName) || foto.FileName.Contains("..") || foto.FileName.Contains('/') || foto.FileName.Contains('\\'))
            return BadRequest(new { motivo = "nome-invalido" });
        if (!await Jpeg(foto)) return BadRequest(new { motivo = "nao-jpeg" });

        var nome = Guid.NewGuid().ToString("N") + ".jpg";
        var pasta = PastaFotos();
        Directory.CreateDirectory(pasta);
        await using (var destino = System.IO.File.Create(Path.Combine(pasta, nome)))
            await foto.CopyToAsync(destino);

        var id = Guid.NewGuid();
        _db.Ocorrencias.Add(new Ocorrencia { Id = id, AmbienteId = ambienteId, Descricao = texto, CaminhoFoto = nome, CriadaEm = _tempo.GetUtcNow() });
        await _db.SaveChangesAsync();
        return Created($"/ocorrencias/{id}/foto", new { id = id.ToString() });
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

    [HttpPost("/acoes")]
    public async Task<IActionResult> Acao([FromBody] AcaoEntrada entrada)
    {
        if (!await _db.Ambientes.AnyAsync(a => a.Id == entrada.AmbienteId)) return NotFound();
        if (!IndiceCalor.EhNivel(entrada.Nivel) || string.IsNullOrWhiteSpace(entrada.Descricao)) return BadRequest();
        var acao = new Acao
        {
            AmbienteId = entrada.AmbienteId,
            Nivel = entrada.Nivel,
            Descricao = entrada.Descricao.Trim(),
            RegistradaEm = _tempo.GetUtcNow(),
        };
        _db.Acoes.Add(acao);
        await _db.SaveChangesAsync();
        return Created($"/acoes/{acao.Id}", new { id = acao.Id });
    }

    [HttpPost("/dispositivos/{id}/revogar")]
    public async Task<IActionResult> Revogar(string id)
    {
        var dispositivo = await _db.Dispositivos.FindAsync(id);
        if (dispositivo is null) return NotFound();
        dispositivo.Ativo = false;
        await _db.SaveChangesAsync();
        return Ok(new { id = dispositivo.Id, ativo = false });
    }

    [HttpPost("/demo/semear")]
    public async Task<IActionResult> Semear([FromQuery] string ambienteId)
    {
        if (!_ambiente.IsEnvironment("Demo")) return NotFound();
        if (!await _db.Ambientes.AnyAsync(a => a.Id == ambienteId)) return NotFound();
        var aoVivo = (await _db.Leituras.Where(l => l.AmbienteId == ambienteId && l.Origem == "dispositivo").ToListAsync())
            .OrderBy(l => l.MedidoEm)
            .ToList();
        if (aoVivo.Count == 0) return Conflict(new { motivo = "ligue o sensor primeiro" });

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
        if (linhas.Count == 0) return Conflict(new { motivo = "csv-vazio" });

        var ancora = linhas[^1];
        var ultima = aoVivo[^1];
        var dTemp = ultima.TemperaturaC - ancora.Temp;
        var dUmid = ultima.UmidadePct - ancora.Umid;
        var primeira = aoVivo[0].MedidoEm;
        var velhas = _db.Leituras.Where(l => l.AmbienteId == ambienteId && l.Origem == "simulada");
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
                Origem = "simulada",
            });
            n++;
        }
        await _db.SaveChangesAsync();
        return Created("/demo/semear", new { inseridas = n });
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
}

public class AcaoEntrada
{
    public string AmbienteId { get; set; } = "";
    public string Nivel { get; set; } = "";
    public string Descricao { get; set; } = "";
}
