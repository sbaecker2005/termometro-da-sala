"""Um modelo de rua compartilhado, com ajuste observacional separado por sala."""
from bisect import bisect_right
from datetime import datetime, timedelta, timezone
import json
import math
from pathlib import Path

from ferramentas.indice_calor import indice_calor_c, nivel
from ml.mapa import prever_json
from ml.ocupantes import estimar_ocupantes, exemplo_terceirao

JANELA = timedelta(hours=3)
IDADE_RUA = timedelta(hours=1)
IDADE_SALA = timedelta(minutes=15)


def _instante(valor):
    if isinstance(valor, str):
        valor = datetime.fromisoformat(valor.replace('Z', '+00:00'))
    if not isinstance(valor, datetime) or valor.tzinfo is None or valor.utcoffset() is None:
        raise ValueError('Instante precisa de data, hora e fuso explícitos.')
    return valor.astimezone(timezone.utc)


def _finito(valor):
    try:
        return type(valor) in (int, float) and math.isfinite(valor)
    except OverflowError:
        return False


def _arredondar(valor):
    # Math.Round(x, 1) no C# escala antes de arredondar; round(x, 1) diverge em empates como 2.45.
    return round(valor * 10) / 10 if abs(valor) < 1e16 else valor


def _rua_ordenada(rua):
    return sorted(((_instante(r['instante']), r.get('id', i), r)
                   for i, r in enumerate(rua) if _finito(r.get('indice'))
                   and _finito(r.get('temperatura_c', 0))
                   and -10 <= r.get('temperatura_c', 0) <= 60
                   and _finito(r.get('umidade_pct', 0))
                   and 0 <= r.get('umidade_pct', 0) <= 100),
                  key=lambda r: (r[0], r[1]))


def _indice_rua(rua, horarios, quando):
    pos = bisect_right(horarios, quando) - 1
    if pos < 0 or quando - rua[pos][0] > IDADE_RUA:
        return None
    return rua[pos][2]['indice']


def ajuste_por_sala(leituras, rua, ambiente_id, agora):
    """Igual peso por sensor/minuto e depois por minuto, sem observações futuras.

    Três minutos pareados são uma regra operacional, não três observações
    meteorológicas independentes nem validação estatística da sala.
    """
    if not isinstance(ambiente_id, str) or not ambiente_id.strip():
        raise ValueError('Informe o ambiente da previsão.')
    agora = _instante(agora)
    validas = []
    for i, leitura in enumerate(leituras):
        if leitura['ambiente_id'] != ambiente_id:
            continue
        if type(leitura.get('suspeita', False)) is not bool:
            raise ValueError('suspeita deve ser booleano.')
        if (leitura.get('suspeita', False) or not _finito(leitura.get('indice'))
                or not _finito(leitura.get('temperatura_c', 0))):
            continue
        instante = _instante(leitura['instante'])
        if not agora - JANELA <= instante <= agora:
            continue
        sensor = leitura['sensor']
        if not isinstance(sensor, str) or not sensor.strip():
            raise ValueError('Informe o sensor da leitura.')
        validas.append((instante, leitura.get('id', i), sensor, leitura))
    validas.sort(key=lambda r: (r[0], r[1]))
    por_sensor_minuto = {}
    recentes = {}
    for instante, _, sensor, leitura in validas:
        por_sensor_minuto[(sensor, instante.replace(second=0, microsecond=0))] = (instante, leitura)
        if agora - instante <= IDADE_SALA:
            recentes[sensor] = leitura['indice']

    externas = _rua_ordenada(rua)
    horarios = [r[0] for r in externas]
    minutos = {}
    ultima_pareada = None
    for (_, minuto), (instante, leitura) in por_sensor_minuto.items():
        externo = _indice_rua(externas, horarios, instante)
        if externo is None:
            continue
        diferenca = leitura['indice'] - externo
        if not _finito(diferenca):
            continue
        minutos.setdefault(minuto, []).append(diferenca)
        ultima_pareada = max(ultima_pareada, instante) if ultima_pareada else instante
    delta = None
    if len(minutos) >= 3 and agora - ultima_pareada <= IDADE_SALA:
        # Mesma soma sequencial do Average em C#; overflow deixa o resultado ausente.
        delta = _arredondar(sum(sum(v) / len(v) for v in minutos.values()) / len(minutos))
    persistencia = _arredondar(sum(recentes.values()) / len(recentes)) if recentes else None
    return {
        'delta_sala': delta if _finito(delta) else None, 'minutos_pareados': len(minutos),
        'ultima_pareada_em': ultima_pareada.isoformat() if ultima_pareada else None,
        'persistencia_sala': persistencia if _finito(persistencia) else None,
        'demonstracao': any(r[3].get('origem') == 'simulada' for r in validas),
    }


def prever_sala(modelo, leituras, rua, ambiente_id, agora, grupos=None):
    """Percorre o modelo já exportado e aplica somente o delta observado da sala."""
    agora = _instante(agora)
    ajuste = ajuste_por_sala(leituras, rua, ambiente_id, agora)
    externas = [r for r in _rua_ordenada(rua) if agora - IDADE_RUA <= r[0] <= agora]
    indice_rua = None
    base = None
    if externas:
        base, _, atual = externas[-1]
        temp, umidade = atual['temperatura_c'], atual['umidade_pct']
        if not _finito(temp) or not -10 <= temp <= 60 or not _finito(umidade) or not 0 <= umidade <= 100:
            raise ValueError('Temperatura ou umidade externa fora do contrato.')
        inicio = (base - timedelta(hours=1)).replace(microsecond=0)
        anteriores = [r for r in _rua_ordenada(rua) if inicio <= r[0] < inicio + timedelta(seconds=1)]
        antes = anteriores[-1][2]['temperatura_c'] if anteriores else None
        if antes is not None and (not _finito(antes) or not -10 <= antes <= 60):
            raise ValueError('Temperatura externa anterior inválida.')
        local = base.astimezone(timezone(timedelta(hours=-3)))
        atributos = {
            'temperatura_c': temp, 'umidade_pct': umidade,
            'hora_sen': math.sin(2 * math.pi * local.hour / 24),
            'hora_cos': math.cos(2 * math.pi * local.hour / 24),
            'mes_sen': math.sin(2 * math.pi * local.month / 12),
            'mes_cos': math.cos(2 * math.pi * local.month / 12),
            'variacao_1h': 0 if antes is None else temp - antes,
            'indice_agora': indice_calor_c(temp, umidade),
        }
        indice_rua = prever_json(modelo, atributos)
        if not _finito(indice_rua):
            raise ValueError('Modelo retornou uma previsão não finita.')
        sala = None if ajuste['delta_sala'] is None else _arredondar(indice_rua + ajuste['delta_sala'])
        metodo = 'modelo_compartilhado_com_ajuste' if sala is not None else 'modelo_rua_sem_ajuste'
    else:
        sala = ajuste['persistencia_sala']
        metodo = 'persistencia_sala' if sala is not None else 'sem_dados'
    indice_nivel = sala if sala is not None else indice_rua
    return {
        'ambiente_id': ambiente_id, 'calculada_em': agora.isoformat(),
        'base_rua_em': base.isoformat() if base else None,
        'previsto_para': ((base or agora) + timedelta(hours=2)).isoformat() if indice_nivel is not None else None,
        'indice_rua_previsto': _arredondar(indice_rua) if indice_rua is not None else None,
        'indice_sala_previsto': sala,
        'nivel_previsto': nivel(indice_nivel) if indice_nivel is not None else None,
        'metodo': metodo, **ajuste,
        'ocupantes': None if grupos is None else estimar_ocupantes(grupos),
    }


def demonstracao():
    agora = datetime(2026, 1, 15, 17, 10, tzinfo=timezone.utc)
    base = agora.replace(minute=0)
    rua = [{'instante': base - timedelta(hours=1), 'temperatura_c': 28.5, 'umidade_pct': 50,
            'indice': indice_calor_c(28.5, 50)},
           {'instante': base, 'temperatura_c': 30, 'umidade_pct': 50, 'indice': indice_calor_c(30, 50)}]
    leituras = [
        {'ambiente_id': 'sala-demonstracao', 'sensor': f'S{i+1}',
         'instante': agora - timedelta(minutes=minuto), 'indice': 31 + desvio, 'origem': 'simulada'}
        for minuto in (2, 1, 0) for i, desvio in enumerate((2, 3, 4, 7))
    ]
    modelo = json.loads((Path(__file__).parent / 'modelo.json').read_text(encoding='utf-8'))
    ocupacao = exemplo_terceirao()
    return {
        'cenario': ocupacao['cenario'], 'validado_em_campo': False,
        'hipotese_ocupacao': ocupacao['hipotese'],
        'limite': 'Histórico interno simulado; watts informativos, sem efeito na previsão em graus.',
        **prever_sala(modelo, leituras, rua, 'sala-demonstracao', agora, ocupacao['grupos']),
    }


if __name__ == '__main__':
    print(json.dumps(demonstracao(), indent=2, allow_nan=False))
