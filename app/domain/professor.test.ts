import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { validarProfessor } from './professor.ts';

describe('validarProfessor', () => {
  it('aceita nome e escola com texto', () => {
    assert.equal(validarProfessor('Ana Lima', 'EE Prof. Paulo'), null);
    assert.equal(validarProfessor('  Ana  ', '  Escola  '), null);
  });

  it('recusa nome ou escola vazios, inclusive só com espaço', () => {
    const erro = 'Informe o nome e a escola.';
    assert.equal(validarProfessor('', 'EE Prof. Paulo'), erro);
    assert.equal(validarProfessor('Ana Lima', ''), erro);
    assert.equal(validarProfessor('   ', '   '), erro);
  });
});
