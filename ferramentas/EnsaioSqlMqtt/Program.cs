using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using TermometroDaSala.Api.Contratos;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio;
using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Servicos;

// Ensaio local descartavel: banco novo, broker em loopback e chave somente em memoria.
var raiz = Directory.GetCurrentDirectory();
var projeto = Path.Combine(raiz, "api", "TermometroDaSala.Api");
var binario = Path.Combine(projeto, "bin", "Validacao", "net8.0", "TermometroDaSala.Api.dll");
if (!File.Exists(binario)) throw new InvalidOperationException("Execute na raiz com -c Validacao.");
if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
    .Any(p => p.Port is 5082 or 18884))
    throw new InvalidOperationException("Porta 5082 ou 18884 ocupada; nenhum processo existente sera alterado.");
var banco = "TermometroGS_ModeloGeral_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..8];
var conexao = $"Server=.\\SQLEXPRESS;Database={banco};Integrated Security=True;TrustServerCertificate=True";
var chave = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
var opcoes = new DbContextOptionsBuilder<SalaDb>().UseSqlServer(conexao).Options;
var destino = Path.Combine(raiz, "docs", "resultados-testes-2026-11-17", banco);
Directory.CreateDirectory(destino);
var resultados = new List<string> { DateTimeOffset.Now.ToString("O"), "SQL Server Express real; HTTP 5082; MQTT 18884 em loopback; " + banco };
var checks = 0;
void Conferir(bool condicao, string descricao)
{
    if (!condicao) throw new InvalidOperationException("FALHOU: " + descricao);
    resultados.Add("OK: " + descricao);
    Console.WriteLine(resultados[^1]);
    checks++;
}

await using (var master = new SqlConnection(conexao.Replace("Database=" + banco, "Database=master")))
{
    await master.OpenAsync();
    await using var comando = master.CreateCommand();
    comando.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name=@nome";
    comando.Parameters.AddWithValue("@nome", banco);
    if (Convert.ToInt32(await comando.ExecuteScalarAsync()) != 0)
        throw new InvalidOperationException("Banco ja existe; repita para gerar outro nome exclusivo.");
}
await using (var db = new SalaDb(opcoes))
{
    await db.Database.MigrateAsync();
    Conferir((await db.Database.GetAppliedMigrationsAsync()).Count() == 5, "cinco migracoes aplicadas em banco novo");
}
var fabrica = new MqttFactory();
using var broker = fabrica.CreateMqttServer(new MqttServerOptionsBuilder().WithDefaultEndpoint()
    .WithDefaultEndpointPort(18884).WithDefaultEndpointBoundIPAddress(IPAddress.Loopback)
    .WithDefaultEndpointBoundIPV6Address(IPAddress.IPv6Loopback).Build());
var inscricao = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
broker.ClientSubscribedTopicAsync += _ => { inscricao.TrySetResult(); return Task.CompletedTask; };
await broker.StartAsync();
using var clienteMqtt = fabrica.CreateMqttClient();
using var http = new HttpClient(new HttpClientHandler { UseProxy = false })
    { BaseAddress = new Uri("http://127.0.0.1:5082"), Timeout = TimeSpan.FromSeconds(15) };
var inicio = new ProcessStartInfo("dotnet") { WorkingDirectory = projeto, UseShellExecute = false,
    CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
inicio.ArgumentList.Add(binario);
inicio.ArgumentList.Add("--urls");
inicio.ArgumentList.Add("http://127.0.0.1:5082");
foreach (var par in new Dictionary<string,string> {
    ["ConnectionStrings__Sala"] = conexao, ["ChaveCoordenacao"] = chave,
    ["DOTNET_ENVIRONMENT"] = "Development", ["ASPNETCORE_ENVIRONMENT"] = "Development",
    ["Banco__Migrar"] = "false", ["Mqtt__Desligado"] = "false", ["Mqtt__Broker"] = "127.0.0.1",
    ["Modelo__Caminho"] = Path.Combine(projeto, "modelo", "modelo.json"),
    ["Mqtt__Porta"] = "18884", ["Mqtt__Topico"] = "ensaio-local/leituras",
    ["Logging__LogLevel__Default"] = "Warning", ["Logging__EventLog__LogLevel__Default"] = "None" })
    inicio.Environment[par.Key] = par.Value;
using var api = Process.Start(inicio) ?? throw new InvalidOperationException("API nao iniciou.");
var stdout = api.StandardOutput.ReadToEndAsync();
var stderr = api.StandardError.ReadToEndAsync();
var instante = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 120;
LeituraEntrada Entrada(int sala, long t, double temperatura = 30) => new() {
    AmbienteId = $"sala-{sala:00}", Dispositivo = $"esp32-sala-{sala:00}", TemperaturaC = temperatura,
    UmidadePct = 50, MedidoEm = t,
    Assinatura = Assinatura.Hex($"segredo-de-teste-sala-{sala:00}", $"sala-{sala:00}", $"esp32-sala-{sala:00}", temperatura, 50, t)
};
async Task<int> Post(LeituraEntrada entrada)
{
    using var resposta = await http.PostAsJsonAsync("/leituras", entrada);
    return (int)resposta.StatusCode;
}
async Task Publicar(LeituraEntrada entrada) => await clienteMqtt.PublishAsync(new MqttApplicationMessageBuilder()
    .WithTopic("ensaio-local/leituras").WithPayload(JsonSerializer.Serialize(entrada))
    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce).Build());
async Task EsperarInstante(int sala, long t)
{
    using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    while (true)
    {
        limite.Token.ThrowIfCancellationRequested();
        await using var db = new SalaDb(opcoes);
        var ultimo = await db.Dispositivos.AsNoTracking().Where(d => d.Id == $"esp32-sala-{sala:00}")
            .Select(d => d.UltimoMedidoEm).SingleAsync(limite.Token);
        if (ultimo?.ToUnixTimeSeconds() == t) return;
        await Task.Delay(50, limite.Token);
    }
}
async Task<int> Quantidade(int sala)
{
    await using var db = new SalaDb(opcoes);
    return await db.Leituras.CountAsync(l => l.AmbienteId == $"sala-{sala:00}");
}
try
{
    await inscricao.Task.WaitAsync(TimeSpan.FromSeconds(25));
    await clienteMqtt.ConnectAsync(new MqttClientOptionsBuilder().WithTcpServer("127.0.0.1", 18884).Build());
    for (var n = 0; ; n++)
    {
        if (api.HasExited) throw new InvalidOperationException("API encerrou antes da verificacao HTTP.");
        try { using var resposta = await http.GetAsync("/niveis"); if (resposta.IsSuccessStatusCode) break; }
        catch (HttpRequestException) when (n < 50) { }
        if (n >= 50) throw new InvalidOperationException("HTTP indisponivel.");
        await Task.Delay(100);
    }
    Conferir(await Post(Entrada(1, instante)) == 201, "leitura HTTP persistida no SQL Server");
    await Publicar(Entrada(2, instante));
    await EsperarInstante(2, instante);
    Conferir(await Quantidade(2) == 1, "leitura MQTT atravessa broker, assinante real e SQL Server");
    for (var n = 1; n <= 10; n++)
    {
        var leitura = Entrada(3, instante + n);
        var pedido = Post(leitura);
        await Task.WhenAll(pedido, Publicar(leitura));
        await EsperarInstante(3, leitura.MedidoEm);
        Conferir(pedido.Result is 201 or 400 && await Quantidade(3) == n,
            $"corrida HTTP/MQTT {n}: exatamente uma linha para o mesmo instante");
    }
    var antigo = Entrada(3, instante - 1);
    Conferir(await Post(antigo) == 400, "HTTP antigo recusado apos concorrencia");
    await Publicar(antigo);
    await Publicar(Entrada(4, instante + 20));
    await EsperarInstante(4, instante + 20);
    await using (var db = new SalaDb(opcoes))
        Conferir(await Quantidade(3) == 10 && (await db.Dispositivos.AsNoTracking().SingleAsync(d => d.Id == "esp32-sala-03"))
            .UltimoMedidoEm?.ToUnixTimeSeconds() == instante + 10, "MQTT antigo nao duplica nem regride o timestamp");
    using (var pedido = new HttpRequestMessage(HttpMethod.Post, "/dispositivos/esp32-sala-02/revogar"))
    {
        pedido.Headers.Add("X-Chave-Coordenacao", chave);
        using var resposta = await http.SendAsync(pedido);
        Conferir((int)resposta.StatusCode == 200, "revogacao confirmada pela API real");
    }
    await Publicar(Entrada(2, instante + 30));
    Conferir(await Post(Entrada(2, instante + 31)) == 400, "HTTP de dispositivo revogado recusado");
    await Publicar(Entrada(4, instante + 40));
    await EsperarInstante(4, instante + 40);
    Conferir(await Quantidade(2) == 1, "MQTT de dispositivo revogado nao grava leitura");
    await using (var db = new SalaDb(opcoes))
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Leituras ADD CONSTRAINT EnsaioFalhaSalvar CHECK (TemperaturaC <> 44.4)");
    Conferir(await Post(Entrada(1, instante + 50, 44.4)) == 500, "falha SQL controlada apos reserva do timestamp");
    await using (var db = new SalaDb(opcoes))
    {
        Conferir(await Quantidade(1) == 1 && (await db.Dispositivos.AsNoTracking().SingleAsync(d => d.Id == "esp32-sala-01"))
            .UltimoMedidoEm?.ToUnixTimeSeconds() == instante, "rollback SQL preserva leitura e timestamp anteriores");
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Leituras DROP CONSTRAINT EnsaioFalhaSalvar");
    }
    Conferir(await Post(Entrada(1, instante + 50, 44.4)) == 201, "mesma leitura aceita depois de retirar a falha controlada");

    // Fixtures sinteticas no banco exclusivo: HTTP consulta o modelo e as queries SQL reais.
    var referencia = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60 * 60 - 30);
    var baseRua = referencia.AddMinutes(-30);
    await using (var db = new SalaDb(opcoes))
    {
        db.LeiturasRua.AddRange(
            new LeituraRua { MedidoEm = baseRua.AddHours(-1), TemperaturaC = 28.5, UmidadePct = 50, IndiceCalor = 29 },
            new LeituraRua { MedidoEm = baseRua, TemperaturaC = 30, UmidadePct = 50, IndiceCalor = 30 },
            new LeituraRua { MedidoEm = referencia.AddMinutes(-1), TemperaturaC = 61, UmidadePct = 50, IndiceCalor = 60 },
            new LeituraRua { MedidoEm = referencia.AddMinutes(10), TemperaturaC = 45, UmidadePct = 50, IndiceCalor = 60 });
        foreach (var numero in new[] { 8, 9, 10, 11, 12 })
        {
            var id = $"sala-{numero:00}";
            db.Ambientes.Add(new Ambiente(id, $"Ensaio sintetico {numero}", 0, FaceSol.leste, false) { LarguraM = 6, ProfundidadeM = 4 });
            foreach (var sensor in new[] { 1, 2 })
                db.Dispositivos.Add(new Dispositivo { Id = $"ensaio-{numero}-{sensor}", AmbienteId = id,
                    Segredo = "fixture-publica-sem-uso", PosX = sensor == 1 ? 0 : 6, PosY = sensor == 1 ? 0 : 4 });
            if (numero == 12) continue;
            var minutos = numero == 10 ? new[] { -3 } : numero == 11 ? new[] { -23, -22, -21 } : new[] { -5, -4, -3 };
            foreach (var minuto in minutos)
                foreach (var sensor in new[] { 1, 2 })
                {
                    var indice = numero == 9 ? 26 : sensor == 1 ? 32 : 36;
                    db.Leituras.Add(new Leitura { AmbienteId = id, DispositivoId = $"ensaio-{numero}-{sensor}",
                        MedidoEm = referencia.AddMinutes(minuto), TemperaturaC = indice, UmidadePct = 50,
                        IndiceCalor = indice, Origem = Origem.simulada });
                    if (numero == 8 && sensor == 1)
                        db.Leituras.Add(new Leitura { AmbienteId = id, DispositivoId = $"ensaio-{numero}-{sensor}",
                            MedidoEm = referencia.AddMinutes(minuto).AddSeconds(-20), TemperaturaC = 50, UmidadePct = 50,
                            IndiceCalor = 50, Origem = Origem.simulada });
                }
        }
        db.Leituras.AddRange(
            new Leitura { AmbienteId = "sala-08", DispositivoId = "ensaio-8-1", MedidoEm = referencia.AddMinutes(-1),
                TemperaturaC = 55, UmidadePct = 50, IndiceCalor = 55, Origem = Origem.simulada, Suspeita = true },
            new Leitura { AmbienteId = "sala-08", DispositivoId = "ensaio-8-2", MedidoEm = referencia.AddMinutes(10),
                TemperaturaC = 55, UmidadePct = 50, IndiceCalor = 55, Origem = Origem.simulada });
        await db.SaveChangesAsync();
    }
    var sala8 = (await http.GetFromJsonAsync<PrevisaoSaida>("/ambientes/sala-08/previsao"))!;
    var sala9 = (await http.GetFromJsonAsync<PrevisaoSaida>("/ambientes/sala-09/previsao"))!;
    var arvore = ModeloRua.Carregar(Path.Combine(projeto, "modelo", "modelo.json"), NullLogger.Instance)
        ?? throw new InvalidOperationException("Modelo exportado indisponivel no ensaio.");
    var indiceModelo = Math.Round(arvore(ModeloRua.Atributos(Relogio.HoraLocal(baseRua), 30, 50, 1.5)), 1);
    Conferir(sala8.IndiceRuaPrevisto == indiceModelo, "SQL/HTTP: previsao corresponde a arvore exportada com os atributos da observacao");
    Conferir(sala8.DeltaSala == 4 && sala9.DeltaSala == -4, "SQL/HTTP: ajuste isolado por sala, pesos por sensor/minuto, futuro e suspeita excluidos");
    Conferir(sala8.RuaDisponivel && sala8.IndiceRuaPrevisto == sala9.IndiceRuaPrevisto,
        "SQL/HTTP: modelo de rua compartilhado entre salas");
    Conferir(Math.Abs(sala8.IndiceSalaPrevisto!.Value - sala9.IndiceSalaPrevisto!.Value - 8) < 0.11,
        "SQL/HTTP: previsoes internas refletem os ajustes distintos");
    Conferir(sala8.PrevistoPara == baseRua.AddHours(2),
        "SQL/HTTP: alvo ancorado na observacao valida, rua futura e temperatura invalida excluidas");
    Conferir(sala8.Demonstracao && sala9.Demonstracao, "SQL/HTTP: origem simulada identificada");
    var insuficiente = (await http.GetFromJsonAsync<PrevisaoSaida>("/ambientes/sala-10/previsao"))!;
    var antiga = (await http.GetFromJsonAsync<PrevisaoSaida>("/ambientes/sala-11/previsao"))!;
    Conferir(insuficiente.DeltaSala is null && insuficiente.SemLeiturasRecentes,
        "SQL/HTTP: sensores simultaneos nao substituem tres minutos de historico");
    Conferir(antiga.DeltaSala is null && antiga.SemLeiturasRecentes,
        "SQL/HTTP: ultimo par mais antigo que 15 minutos nao calibra sala");
    await using (var db = new SalaDb(opcoes))
        Conferir(await db.Previsoes.CountAsync(p => p.AmbienteId == "sala-08") == 1,
            "SQL: previsao calculada persistida no historico");
    using (var mapa = await http.GetFromJsonAsync<JsonDocument>("/ambientes/sala-09/mapa?futuro=true"))
    {
        var json = mapa!.RootElement;
        var pontos = json.GetProperty("pontos").EnumerateArray().ToArray();
        var alvoMapa = json.GetProperty("previstoPara").GetDateTimeOffset();
        var esperado = Math.Round(26 + (sala9.IndiceSalaPrevisto!.Value - 26)
            / (sala9.PrevistoPara - alvoMapa.AddHours(-1)).TotalHours, 1);
        Conferir(pontos.Length == 2 && pontos.All(p => !p.GetProperty("semLeitura").GetBoolean()
            && Math.Abs(p.GetProperty("indiceCalor").GetDouble() - esperado) < 0.11),
            "SQL/HTTP: mapa +1 h usa o tempo real restante ate o alvo da previsao");
        await File.WriteAllTextAsync(Path.Combine(destino, "mapa-futuro.json"), json.GetRawText());
    }
    await File.WriteAllTextAsync(Path.Combine(destino, "previsoes-modelo.json"),
        JsonSerializer.Serialize(new[] { sala8, sala9, insuficiente, antiga }, new JsonSerializerOptions { WriteIndented = true }));
    await using (var db = new SalaDb(opcoes))
    {
        // Expira apenas a fixture externa deste ensaio para exercitar a persistencia pela API.
        foreach (var leitura in await db.LeiturasRua.ToListAsync()) leitura.MedidoEm = leitura.MedidoEm.AddHours(-4);
        await db.SaveChangesAsync();
    }
    var persistencia = (await http.GetFromJsonAsync<PrevisaoSaida>("/ambientes/sala-08/previsao"))!;
    Conferir(!persistencia.RuaDisponivel && persistencia.IndiceSalaPrevisto == 34,
        "SQL/HTTP: sem rua recente, media da ultima leitura valida por sensor");
    using (var semDados = await http.GetAsync("/ambientes/sala-12/previsao"))
        Conferir(semDados.StatusCode == HttpStatusCode.ServiceUnavailable,
            "SQL/HTTP: sem rua e sem sensor retorna 503, sem inventar previsao");
    resultados.Add($"CONCLUIDO: {checks} verificacoes aprovadas. Banco preservado; nenhum banco anterior alterado.");
}
catch (Exception ex)
{
    resultados.Add("FALHA: " + ex.GetType().Name + ": " + ex.Message.Replace(chave, "***").Replace(conexao, "***"));
    Environment.ExitCode = 1;
}
finally
{
    if (!api.HasExited) { api.Kill(); await api.WaitForExitAsync(); }
    await broker.StopAsync();
    await File.WriteAllTextAsync(Path.Combine(destino, "api.log"), ((await stdout) + (await stderr)).Replace(chave, "***").Replace(conexao, "***"));
    await File.WriteAllLinesAsync(Path.Combine(destino, "resultado.txt"), resultados);
    Console.WriteLine(resultados[^1]);
    Console.WriteLine("Registro: " + Path.Combine(destino, "resultado.txt"));
}
