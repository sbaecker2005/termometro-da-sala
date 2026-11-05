export const DESCRICAO_MIN = 10;
export const DESCRICAO_MAX = 500;

export type ErroOcorrencia = 'descricao-curta' | 'descricao-longa' | 'sem-foto';

export function validarOcorrencia(entrada: { descricao: string; temFoto: boolean }): ErroOcorrencia[] {
  const texto = entrada.descricao.trim();
  const erros: ErroOcorrencia[] = [];
  if (texto.length < DESCRICAO_MIN) erros.push('descricao-curta');
  else if (texto.length > DESCRICAO_MAX) erros.push('descricao-longa');
  if (!entrada.temFoto) erros.push('sem-foto');
  return erros;
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
