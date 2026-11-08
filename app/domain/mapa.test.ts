import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { diametro, lerMapa, mapaDemonstracao, textoPonto } from './mapa.ts';

describe('mapa térmico', () => {
  it('mostra frio, desvio e texto com precaução, preservando as posições na previsão', () => {
    const agora = lerMapa(mapaDemonstracao('sala-05', false), 'sala-05');
    const futuro = lerMapa(mapaDemonstracao('sala-05', true), 'sala-05');
    const ponto = agora.pontos[0];
    assert.equal(ponto.nivel, 'frio');
    assert.equal(ponto.desvioC, -4.5);
    assert.match(textoPonto(ponto), /Janela \(sensor\): 15.0 °C; Frio.*agasalhar/);
    assert.equal(futuro.pontos[0].temperaturaC, 14);
    assert.deepEqual(futuro.pontos.map(p => [p.id, p.x, p.y]), agora.pontos.map(p => [p.id, p.x, p.y]));
    assert.match(agora.precaucoes[0], /DEMONSTRAÇÃO/);
    assert.match(textoPonto(agora.pontos[2]), /trocar Cadeira 1 para Cadeira 2/);
    assert.match(textoPonto(futuro.pontos[2]), /trocar Cadeira 1 para Cadeira 2/);
  });
  it('separa calor e normal e usa o módulo do desvio com limite visual', () => {
    const mapa = mapaDemonstracao('sala-07', false);
    assert.equal(mapa.pontos[0].nivel, 'cuidado');
    assert.equal(mapa.pontos[1].nivel, 'normal');
    assert.match(textoPonto(mapa.pontos[2]), /trocar Cadeira 1 para Cadeira 2/);
    assert.equal(diametro(-4), diametro(4));
    assert.equal(diametro(0), 12);
    assert.equal(diametro(null), 12);
    assert.equal(diametro(100), 36);
    assert.match(textoPonto({ ...mapa.pontos[0], semLeitura: true, temperaturaC: null, nivel: null }), /Sem leitura; verificar o sensor/);
  });
  it('recusa mapas de outro ambiente, coordenadas fora da planta e valores inválidos', () => {
    const mapa = mapaDemonstracao('sala-07', false);
    assert.throws(() => lerMapa(mapa, 'outra'));
    assert.throws(() => lerMapa({ ...mapa, pontos: [{ ...mapa.pontos[0], x: 7 }] }, 'sala-07'));
    assert.throws(() => lerMapa({ ...mapa, pontos: [{ ...mapa.pontos[0], desvioC: Infinity }] }, 'sala-07'));
    assert.throws(() => lerMapa({ ...mapa, grade: [null] }, 'sala-07'));
    assert.throws(() => lerMapa({ ...mapa, profundidadeM: null }, 'sala-07'));
    assert.throws(() => lerMapa({ ...mapa, pontos: [{ ...mapa.pontos[0], precaucoes: [42] }] }, 'sala-07'));
    assert.throws(() => lerMapa({ ...mapa, pontos: [{ ...mapa.pontos[0], precaucoes: null }] }, 'sala-07'));
  });
  it('associa a precaução calculada ao ponto no texto usado pelo toque e pela lista acessível', () => {
    const base = mapaDemonstracao('sala-07', false);
    const precaucao = 'trocar Cadeira 1 para Cadeira 2 (33.2 → 25.8 °C de índice)';
    const mapa = lerMapa({ ...base, pontos: base.pontos.map(p => ({ ...p,
      precaucoes: p.id === 'cadeira-1' ? [precaucao] : [] })), precaucoes: [precaucao] }, 'sala-07');
    assert.ok(textoPonto(mapa.pontos[2]).includes(precaucao));
    assert.ok(!textoPonto(mapa.pontos[3]).includes(precaucao));
    assert.ok(!textoPonto(mapa.pontos[0]).includes(precaucao));
    assert.match(textoPonto({ ...mapa.pontos[2], semLeitura: true }), /Sem leitura/);
    assert.ok(!textoPonto({ ...mapa.pontos[2], semLeitura: true }).includes(precaucao));
  });
  it('aceita planta sem dimensões e valores ausentes sem presumir normal', () => {
    const mapa = { ambienteId: 's', larguraM: null, profundidadeM: null, passoM: .5,
      grade: [{ x: 0, y: 0, temperaturaC: null, indiceCalor: null, nivel: null }], pontos: [], precaucoes: [] };
    assert.equal(lerMapa(mapa, 's').grade[0].nivel, null);
  });
});
