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
#include "secrets.h"

constexpr int PIN_DHT = 15, VERDE = 18, AMARELO = 19, VERMELHO = 21;
const char TOPICO[] = "fiap-gs/termometro-da-sala/leituras";
DHT dht(PIN_DHT, DHT22);
WiFiClient rede;
PubSubClient mqtt(rede);
volatile bool relogioSincronizado = false;
struct Leitura { char temperatura[16]; int umidade; long long instante; };
QueueHandle_t fila;
int nivelAtual = -1;

double indiceCalor(double tempC, double rh) {
  double t = tempC * 9 / 5 + 32;
  double simples = 0.5 * (t + 61.0 + (t - 68.0) * 1.2 + rh * 0.094);
  double hi;
  if ((simples + t) / 2 < 80) {
    hi = simples;
  } else {
    hi = -42.379 + 2.04901523*t + 10.14333127*rh - 0.22475541*t*rh
      - 0.00683783*t*t - 0.05481717*rh*rh + 0.00122874*t*t*rh
      + 0.00085282*t*rh*rh - 0.00000199*t*t*rh*rh;
    if (rh < 13 && t >= 80 && t <= 112)
      hi -= ((13-rh)/4) * sqrt((17-fabs(t-95))/17);
    else if (rh > 85 && t >= 80 && t <= 87)
      hi += ((rh-85)/10) * ((87-t)/5);
  }
  return round((hi - 32) * 5 / 9 * 10) / 10;
}

int nivel(double indice) {
  if (indice < 27) return 0;
  if (indice < 32) return 1;
  if (indice < 41) return 2;
  return 3;
}

bool assinar(const char *texto, const char *chave, char *hex) {
  unsigned char hash[32];
  const auto *info = mbedtls_md_info_from_type(MBEDTLS_MD_SHA256);
  if (!info || mbedtls_md_hmac(info, (const unsigned char *)chave, strlen(chave),
      (const unsigned char *)texto, strlen(texto), hash) != 0) return false;
  for (int i = 0; i < 32; ++i) snprintf(hex + i*2, 3, "%02x", (unsigned int)hash[i]);
  return true;
}

void conferirVetores() {
  char hex[65];
  bool ok = assinar("sala-03|esp32-sala-03|33.4|58|1794331200",
                    "segredo-de-teste-sala-03", hex);
  Serial.printf("Vetor HMAC: %s\n", ok ? hex : "falha");
  assert(ok && strcmp(hex, "2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0") == 0);
  const double casos[][4] = {{24,40,23.5,0}, {30,50,31.0,1},
                            {33.4,58,39.8,2}, {38,70,62.5,3}};
  for (const auto &c : casos) {
    double indice = indiceCalor(c[0], c[1]);
    assert(fabs(indice-c[2]) <= 0.1 && nivel(indice) == c[3]);
  }
  assert(nivel(26.9) == 0 && nivel(27) == 1 && nivel(32) == 2 && nivel(41) == 3);
  Serial.println("Vetores de calor OK");
}

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
  bool bufferOk = mqtt.setBufferSize(512);
  assert(bufferOk);
  mqtt.setSocketTimeout(1);
  WiFi.mode(WIFI_STA);
  WiFi.setAutoReconnect(true);
  WiFi.begin("Wokwi-GUEST", "", 6);
  sntp_set_time_sync_notification_cb([](struct timeval *) { relogioSincronizado = true; });
  sntp_set_sync_interval(60000);
  configTime(0, 0, "pool.ntp.org", "time.nist.gov");
  // A rede pode bloquear em DNS/conexao; a tarefa separada preserva os LEDs locais.
  BaseType_t criada = xTaskCreate(conectarEPublicar, "mqtt", 6144, nullptr, 1, nullptr);
  assert(criada == pdPASS);
}

void loop() {
  static uint32_t ultimaLeitura = millis();
  uint32_t agora = millis();
  if (uint32_t(agora - ultimaLeitura) >= 10000) {
    ultimaLeitura = agora;
    float temp = dht.readTemperature(), umidade = dht.readHumidity();
    if (!isfinite(temp) || !isfinite(umidade) || temp < -10 || temp > 60 || umidade < 0 || umidade > 100) {
      nivelAtual = -1;
      Serial.println("Sensor invalido; LEDs apagados, sem publicar");
    } else {
      Leitura l;
      snprintf(l.temperatura, sizeof l.temperatura, "%.1f", temp);
      l.umidade = (int)lround(umidade);
      double indice = indiceCalor(atof(l.temperatura), l.umidade);
      nivelAtual = nivel(indice);
      Serial.printf("Temperatura %s C, umidade %d%%, indice %.1f C, nivel %d\n",
                    l.temperatura, l.umidade, indice, nivelAtual);
      if (relogioSincronizado) {
        l.instante = (long long)time(nullptr);
        // shortcut: QoS 0 e so a leitura mais recente, evoluir se precisar recuperar quedas.
        xQueueOverwrite(fila, &l);
      } else Serial.println("Aguardando NTP; LED local ativo");
    }
  }
  digitalWrite(VERDE, nivelAtual == 0);
  digitalWrite(AMARELO, nivelAtual == 1);
  digitalWrite(VERMELHO, nivelAtual == 2 || (nivelAtual == 3 && (agora / 500) % 2 == 0));
  delay(10);
}
