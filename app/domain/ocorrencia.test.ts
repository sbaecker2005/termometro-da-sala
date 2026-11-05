import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { lerOcorrencias, validarOcorrencia } from './ocorrencia.ts';

describe('validarOcorrencia', () => {
  it('aceita descrição no intervalo e com foto', () => {
    assert.deepEqual(validarOcorrencia({ descricao: 'Ventilador parou', temFoto: true }), []);
    assert.deepEqual(validarOcorrencia({ descricao: 'a'.repeat(500), temFoto: true }), []);
  });

  it('corta espaço e recusa fora do intervalo', () => {
    assert.deepEqual(validarOcorrencia({ descricao: '   curta   ', temFoto: true }), ['descricao-curta']);
    assert.deepEqual(validarOcorrencia({ descricao: 'a'.repeat(9), temFoto: true }), ['descricao-curta']);
    assert.deepEqual(validarOcorrencia({ descricao: 'a'.repeat(501), temFoto: true }), ['descricao-longa']);
  });

  it('exige foto mesmo com texto válido', () => {
    assert.deepEqual(validarOcorrencia({ descricao: 'Janela emperrada', temFoto: false }), ['sem-foto']);
  });
});

describe('lerOcorrencias', () => {
  it('aceita a lista e recusa o que não tem os campos', () => {
    const lista = [{ id: '1', ambienteId: 'sala-03', descricao: 'Ventilador parou', criadaEm: '2026-11-06T17:10:00Z' }];
    assert.deepEqual(lerOcorrencias(lista), lista);
    assert.throws(() => lerOcorrencias({}), /resposta invalida/);
    assert.throws(() => lerOcorrencias([{ id: '1' }]), /resposta invalida/);
  });
});
