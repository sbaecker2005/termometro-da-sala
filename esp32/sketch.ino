/*
 * Termômetro da Sala: sensor da porta da sala (ESP32 DevKit V1 no Wokwi)
 *
 * O que faz: a cada 10 s lê temperatura e umidade do DHT22, calcula o índice
 * de calor (NOAA/Rothfusz, a mesma fórmula da API) e acende o LED do nível.
 * Quando o relógio já veio do NTP, assina a leitura com HMAC-SHA256 e publica
 * por MQTT no tópico que a API assina. A API grava a leitura na sala-03, que o
 * professor vê no app e a coordenação vê no desktop e no painel web.
 *
 * Circuito: DHT22 em 3,3 V com DATA no GPIO 15 (pull-up de 10 kΩ);
 * LEDs verde (GPIO 18), amarelo (GPIO 19) e vermelho (GPIO 21), cada um com
 * resistor de 220 Ω e cátodo no GND.
 *
 * Fluxo: sensor -> LED local (funciona sem internet) -> fila de 1 posição ->
 * tarefa MQTT separada -> broker -> API (valida HMAC, ordem e horário).
 * Formato da mensagem, texto canônico e vetor de teste: ../CONTRATO.md.
 */
#include <WiFi.h>
#include <PubSubClient.h>
#include <DHT.h>
#include <time.h>
#include <esp_sntp.h>
#include <mbedtls/md.h>
#include <assert.h>
#include <math.h>
#include <freertos/FreeRTOS.h>
#include <freertos/queue.h>
#include <freertos/task.h>
#include "secrets.h"  // AMBIENTE_ID, DISPOSITIVO e SEGREDO (copiar de secrets.exemplo.h)

// Pinos (GPIO) do DevKit V1, os mesmos D15, D18, D19 e D21 do diagram.json.
constexpr int PIN_DHT = 15, VERDE = 18, AMARELO = 19, VERMELHO = 21;
const char TOPICO[] = "fiap-gs/termometro-da-sala/leituras";

// Níveis de calor do contrato (seção 3). SEM_LEITURA apaga todos os LEDs.
enum Nivel { SEM_LEITURA = -1, NORMAL = 0, ATENCAO = 1, CUIDADO = 2, PERIGO = 3 };
const char *const NOME_NIVEL[] = {"Normal", "Atencao", "Cuidado extremo", "Perigo"};

DHT dht(PIN_DHT, DHT22);
WiFiClient rede;
PubSubClient mqtt(rede);
volatile bool relogioSincronizado = false;  // vira true no primeiro acerto do NTP

// Leitura já formatada: o mesmo texto da temperatura vai no JSON e no HMAC.
struct Leitura { char temperatura[16]; int umidade; long long instante; };
QueueHandle_t fila;  // 1 posição: só a leitura mais recente espera a conexão
Nivel nivelAtual = SEM_LEITURA;

// Índice de calor em °C com uma casa decimal (NOAA/Rothfusz, CONTRATO.md seção 3).
double indiceCalor(double tempC, double rh) {
  double t = tempC * 9 / 5 + 32;  // a fórmula trabalha em Fahrenheit
  double simples = 0.5 * (t + 61.0 + (t - 68.0) * 1.2 + rh * 0.094);
  double hi;
  if ((simples + t) / 2 < 80) {
    hi = simples;  // calor baixo: a regressão completa não se aplica
  } else {
    hi = -42.379 + 2.04901523*t + 10.14333127*rh - 0.22475541*t*rh
      - 0.00683783*t*t - 0.05481717*rh*rh + 0.00122874*t*t*rh
      + 0.00085282*t*rh*rh - 0.00000199*t*t*rh*rh;
    // Ajustes da NOAA para ar muito seco ou muito úmido.
    if (rh < 13 && t >= 80 && t <= 112)
      hi -= ((13-rh)/4) * sqrt((17-fabs(t-95))/17);
    else if (rh > 85 && t >= 80 && t <= 87)
      hi += ((rh-85)/10) * ((87-t)/5);
  }
  return round((hi - 32) * 5 / 9 * 10) / 10;
}

// Limites sem sobreposição: <27 Normal, <32 Atenção, <41 Cuidado extremo, senão Perigo.
Nivel nivel(double indice) {
  if (indice < 27) return NORMAL;
  if (indice < 32) return ATENCAO;
  if (indice < 41) return CUIDADO;
  return PERIGO;
}

// HMAC-SHA256 do texto canônico em 64 caracteres hex minúsculos (mbedTLS do core ESP32).
bool assinar(const char *texto, const char *chave, char *hex) {
  unsigned char hash[32];
  const auto *info = mbedtls_md_info_from_type(MBEDTLS_MD_SHA256);
  if (!info || mbedtls_md_hmac(info, (const unsigned char *)chave, strlen(chave),
      (const unsigned char *)texto, strlen(texto), hash) != 0) return false;
  for (int i = 0; i < 32; ++i) snprintf(hex + i*2, 3, "%02x", (unsigned int)hash[i]);
  return true;
}

// Autoteste ao ligar: vetor HMAC público e os quatro casos de calor do contrato.
// Se algum falhar, o assert para o ESP32 antes de medir ou publicar.
void conferirVetores() {
  char hex[65];
  bool ok = assinar("sala-03|esp32-sala-03|33.4|58|1794331200",
                    "segredo-de-teste-sala-03", hex);
  Serial.printf("Vetor HMAC: %s\n", ok ? hex : "falha");
  assert(ok && strcmp(hex, "2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0") == 0);
  struct Caso { double temp, umidade, indice; Nivel esperado; };
  const Caso casos[] = {{24, 40, 23.5, NORMAL}, {30, 50, 31.0, ATENCAO},
                        {33.4, 58, 39.8, CUIDADO}, {38, 70, 62.5, PERIGO}};
  for (const auto &c : casos) {
    double indice = indiceCalor(c.temp, c.umidade);
    assert(fabs(indice - c.indice) <= 0.1 && nivel(indice) == c.esperado);
  }
  // Bordas dos limites: o valor exato do limite já pertence ao nível de cima.
  assert(nivel(26.9) == NORMAL && nivel(27) == ATENCAO &&
         nivel(32) == CUIDADO && nivel(41) == PERIGO);
  Serial.println("Vetores de calor OK");
}

// Monta o texto canônico, assina e publica o JSON do contrato (QoS 0, sem retenção).
void publicar(const Leitura &l) {
  char canonico[160], assinatura[65], json[320];
  int n = snprintf(canonico, sizeof canonico, "%s|%s|%s|%d|%lld",
      AMBIENTE_ID, DISPOSITIVO, l.temperatura, l.umidade, l.instante);
  if (n < 0 || n >= sizeof canonico || !assinar(canonico, SEGREDO, assinatura)) {
    Serial.println("Falha ao assinar leitura");
    return;
  }
  n = snprintf(json, sizeof json,
      "{\"ambienteId\":\"%s\",\"dispositivo\":\"%s\",\"temperaturaC\":%s,"
      "\"umidadePct\":%d,\"medidoEm\":%lld,\"assinatura\":\"%s\"}",
      AMBIENTE_ID, DISPOSITIVO, l.temperatura, l.umidade, l.instante, assinatura);
  if (n < 0 || n >= sizeof json || !mqtt.publish(TOPICO, json, false))
    Serial.println("Falha na publicacao MQTT");
  else Serial.println("Leitura publicada");
}

// Tarefa FreeRTOS da rede: reconecta o MQTT a cada 5 s e publica o que estiver na fila.
// Fica separada do loop() para que DNS ou broker lentos não travem a medição nem o pisca.
void conectarEPublicar(void *) {
  uint32_t tentativa = millis() - 5000;
  long long ultimoPublicado = 0;
  for (;;) {
    if (WiFi.status() == WL_CONNECTED) {
      if (!mqtt.connected() && uint32_t(millis()-tentativa) >= 5000) {
        tentativa = millis();
        String cliente = String(DISPOSITIVO) + "-" + String((uint32_t)ESP.getEfuseMac(), HEX);
        if (!mqtt.connect(cliente.c_str())) Serial.println("MQTT indisponivel; tentando novamente");
      }
      if (mqtt.connected()) {
        mqtt.loop();
        Leitura l;
        if (xQueueReceive(fila, &l, 0) == pdTRUE) {
          // Mesma janela da API (600 s atrás, 60 s à frente) e ordem estrita contra reenvio.
          long long idade = (long long)time(nullptr) - l.instante;
          if (l.instante > ultimoPublicado && idade >= -60 && idade <= 600) {
            publicar(l);
            ultimoPublicado = l.instante;
          } else Serial.println("Relogio ou leitura antiga; aguardando proxima medicao");
        }
      }
    }
    vTaskDelay(pdMS_TO_TICKS(20));
  }
}

void setup() {
  Serial.begin(115200);
  pinMode(VERDE, OUTPUT); pinMode(AMARELO, OUTPUT); pinMode(VERMELHO, OUTPUT);
  dht.begin();
  conferirVetores();
  fila = xQueueCreate(1, sizeof(Leitura));
  assert(fila);
  mqtt.setServer("broker.hivemq.com", 1883);
  bool bufferOk = mqtt.setBufferSize(512);  // o JSON assinado passa do padrão de 256 bytes
  assert(bufferOk);
  mqtt.setSocketTimeout(1);
  WiFi.mode(WIFI_STA);
  WiFi.setAutoReconnect(true);
  WiFi.begin("Wokwi-GUEST", "", 6);  // rede aberta do simulador, canal 6
  // O NTP fornece o medidoEm; sem relógio a leitura não é publicada (não inventamos data).
  sntp_set_time_sync_notification_cb([](struct timeval *) { relogioSincronizado = true; });
  sntp_set_sync_interval(60000);
  configTime(0, 0, "pool.ntp.org", "time.nist.gov");
  // A rede pode bloquear em DNS/conexao; a tarefa separada preserva os LEDs locais.
  BaseType_t criada = xTaskCreate(conectarEPublicar, "mqtt", 6144, nullptr, 1, nullptr);
  assert(criada == pdPASS);
}

// Medição a cada 10 s e LEDs atualizados a cada 10 ms (o vermelho pisca em Perigo).
void loop() {
  static uint32_t ultimaLeitura = millis();
  uint32_t agora = millis();
  if (uint32_t(agora - ultimaLeitura) >= 10000) {
    ultimaLeitura = agora;
    float temp = dht.readTemperature(), umidade = dht.readHumidity();
    // Mesma faixa da API (-10 a 60 °C, 0 a 100 %): fora dela, LEDs apagados e nada publicado.
    if (!isfinite(temp) || !isfinite(umidade) || temp < -10 || temp > 60 || umidade < 0 || umidade > 100) {
      nivelAtual = SEM_LEITURA;
      Serial.println("Sensor invalido; LEDs apagados, sem publicar");
    } else {
      Leitura l;
      snprintf(l.temperatura, sizeof l.temperatura, "%.1f", temp);
      l.umidade = (int)lround(umidade);
      // Calcula com os valores já formatados, para ter as mesmas entradas da API.
      double indice = indiceCalor(atof(l.temperatura), l.umidade);
      nivelAtual = nivel(indice);
      Serial.printf("Temperatura %s C, umidade %d%%, indice %.1f C, nivel %s\n",
                    l.temperatura, l.umidade, indice, NOME_NIVEL[nivelAtual]);
      if (relogioSincronizado) {
        l.instante = (long long)time(nullptr);
        // shortcut: QoS 0 e so a leitura mais recente, evoluir se precisar recuperar quedas.
        xQueueOverwrite(fila, &l);
      } else Serial.println("Aguardando NTP; LED local ativo");
    }
  }
  digitalWrite(VERDE, nivelAtual == NORMAL);
  digitalWrite(AMARELO, nivelAtual == ATENCAO);
  // Cuidado extremo: fixo. Perigo: 500 ms aceso e 500 ms apagado.
  digitalWrite(VERMELHO, nivelAtual == CUIDADO || (nivelAtual == PERIGO && (agora / 500) % 2 == 0));
  delay(10);
}
