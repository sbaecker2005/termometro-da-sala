export const DESCRICAO_MIN = 10;
export const DESCRICAO_MAX = 500;

// Texto único para JSON fora do formato esperado; ele chega à tela do professor.
export const RESPOSTA_INESPERADA = 'O servidor respondeu num formato inesperado. Tente de novo.';

export type ErroOcorrencia = 'descricao-curta' | 'descricao-longa' | 'sem-foto';

export function validarOcorrencia(entrada: { descricao: string; temFoto: boolean }): ErroOcorrencia[] {
  const texto = entrada.descricao.trim();
  const erros: ErroOcorrencia[] = [];
  if (texto.length < DESCRICAO_MIN) erros.push('descricao-curta');
  else if (texto.length > DESCRICAO_MAX) erros.push('descricao-longa');
  if (!entrada.temFoto) erros.push('sem-foto');
  return erros;
}

export type Ocorrencia = {
  id: string;
  ambienteId: string;
  descricao: string;
  criadaEm: string;
};

export function lerOcorrencias(valor: unknown): Ocorrencia[] {
  if (!Array.isArray(valor)) throw new Error(RESPOSTA_INESPERADA);
  return valor.map((item: unknown) => {
    if (!item || typeof item !== 'object') throw new Error(RESPOSTA_INESPERADA);
    const linha = item as Record<string, unknown>;
    if (
      typeof linha.id !== 'string' ||
      typeof linha.ambienteId !== 'string' ||
      typeof linha.descricao !== 'string' ||
      typeof linha.criadaEm !== 'string'
    ) {
      throw new Error(RESPOSTA_INESPERADA);
    }
    return { id: linha.id, ambienteId: linha.ambienteId, descricao: linha.descricao, criadaEm: linha.criadaEm };
  });
}

export function mensagemErroOcorrencia(erro: ErroOcorrencia): string {
  switch (erro) {
    case 'descricao-curta':
      return 'Descreva o que aconteceu com pelo menos 10 caracteres.';
    case 'descricao-longa':
      return 'Use no máximo 500 caracteres.';
    case 'sem-foto':
      return 'Fotografe a sala ou o equipamento, sem pessoas.';
  }
}
