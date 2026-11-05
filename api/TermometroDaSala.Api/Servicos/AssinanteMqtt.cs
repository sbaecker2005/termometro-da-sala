using TermometroDaSala.Api.Dominio.Entidades;
using TermometroDaSala.Api.Contratos;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;

namespace TermometroDaSala.Api.Servicos;

public class AssinanteMqtt : BackgroundService
{
    // uma leitura assinada tem cerca de 250 bytes; acima disso a mensagem é descartada antes do JSON
    public const int MaximoBytes = 1024;
    private readonly IServiceScopeFactory _escopos;
    private readonly IConfiguration _config;
    private readonly ILogger<AssinanteMqtt> _log;

    public AssinanteMqtt(IServiceScopeFactory escopos, IConfiguration config, ILogger<AssinanteMqtt> log)
    {
        _escopos = escopos;
        _config = config;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var broker = _config["Mqtt:Broker"] ?? "broker.hivemq.com";
        var porta = _config.GetValue("Mqtt:Porta", 1883);
        var topico = _config["Mqtt:Topico"] ?? "fiap-gs/termometro-da-sala/leituras";
        var fabrica = new MqttFactory();
        using var cliente = fabrica.CreateMqttClient();
        cliente.ApplicationMessageReceivedAsync += e => Tratar(e.ApplicationMessage.PayloadSegment);
        var opcoes = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, porta)
            .WithClientId("tds-" + Guid.NewGuid().ToString("N")[..12])
            .Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!cliente.IsConnected)
                {
                    await cliente.ConnectAsync(opcoes, stoppingToken);
                    await cliente.SubscribeAsync(fabrica.CreateSubscribeOptionsBuilder().WithTopicFilter(topico).Build(), stoppingToken);
                    _log.LogInformation("mqtt conectado em {Broker} {Topico}", broker, topico);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogWarning("mqtt indisponivel, nova tentativa: {Mensagem}", ex.Message);
            }

            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    public async Task Tratar(ArraySegment<byte> carga)
    {
        if (carga.Count > MaximoBytes)
        {
            _log.LogWarning("leitura mqtt ignorada: {Mensagem}", "acima-do-limite");
            return;
        }
        try
        {
            var entrada = JsonSerializer.Deserialize<LeituraEntrada>(carga.AsSpan(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (entrada is null) return;
            using var escopo = _escopos.CreateScope();
            await escopo.ServiceProvider.GetRequiredService<ServicoLeituras>().Receber(entrada);
        }
        catch (Exception ex)
        {
            _log.LogWarning("leitura mqtt ignorada: {Mensagem}", ex.Message);
        }
    }
}
