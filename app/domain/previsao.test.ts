import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { lerPrevisao, semDadosParaPrever } from './previsao.ts';

// Corpos como a API devolve em GET /ambientes/{id}/previsao (PrevisaoSaida em camelCase).
const comRua = {
  ambienteId: 'sala-03',
  previstoPara: '2026-11-06T19:10:00+00:00',
  indiceRuaPrevisto: 34,
  deltaSala: 4,
  indiceSalaPrevisto: 38,
  nivelPrevisto: 'cuidado',
  semLeiturasRecentes: false,
  demonstracao: true,
  ruaDisponivel: true,
};

const semRua = {
  ...comRua,
  indiceRuaPrevisto: 0,
  deltaSala: null,
  indiceSalaPrevisto: 31.5,
  nivelPrevisto: 'atencao',
  demonstracao: false,
  ruaDisponivel: false,
};

describe('lerPrevisao', () => {
  it('aceita a previsão com e sem a rua', () => {
    assert.deepEqual(lerPrevisao(comRua), comRua);
    assert.deepEqual(lerPrevisao(semRua), semRua);
  });

  it('recusa sem ruaDisponivel ou sem rua e sem índice da sala', () => {
    const { ruaDisponivel: _, ...antiga } = comRua;
    assert.throws(() => lerPrevisao(antiga), /formato inesperado/);
    assert.throws(() => lerPrevisao({ ...semRua, indiceSalaPrevisto: null }), /formato inesperado/);
    assert.throws(() => lerPrevisao({ ...comRua, nivelPrevisto: 'quente' }), /formato inesperado/);
  });
});

describe('semDadosParaPrever', () => {
  it('reconhece só o motivo do 503 da previsão', () => {
    assert.equal(semDadosParaPrever({ motivo: 'sem-dados-para-prever' }), true);
    assert.equal(semDadosParaPrever({ motivo: 'outro' }), false);
    assert.equal(semDadosParaPrever(null), false);
    assert.equal(semDadosParaPrever('sem-dados-para-prever'), false);
  });
});
