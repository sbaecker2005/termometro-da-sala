import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { apresentarNivel, nivelDeIndice } from './nivel.ts';

describe('nivelDeIndice', () => {
  it('separa as faixas nos limites do contrato', () => {
    assert.equal(nivelDeIndice(26.9), 'normal');
    assert.equal(nivelDeIndice(27), 'atencao');
    assert.equal(nivelDeIndice(31.9), 'atencao');
    assert.equal(nivelDeIndice(32), 'cuidado');
    assert.equal(nivelDeIndice(40.9), 'cuidado');
    assert.equal(nivelDeIndice(41), 'perigo');
    assert.equal(nivelDeIndice(34.2), 'cuidado');
  });

  it('recusa índice que não é número', () => {
    assert.throws(() => nivelDeIndice(Number.NaN), /indice invalido/);
  });
});

describe('apresentarNivel', () => {
  it('traz nome e ação junto da cor', () => {
    const cuidado = apresentarNivel('cuidado');
    assert.equal(cuidado.nome, 'Cuidado extremo');
    assert.match(cuidado.acao, /Educação Física/);
    assert.ok(cuidado.cor.length > 0);
  });
});
