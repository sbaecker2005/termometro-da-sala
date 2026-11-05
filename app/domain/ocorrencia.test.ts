import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { validarOcorrencia } from './ocorrencia.ts';

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
