import { ehNivel, type Nivel } from './nivel.ts';
import { RESPOSTA_INESPERADA } from './ocorrencia.ts';

export type Previsao = {
  ambienteId: string;
  previstoPara: string;
  indiceRuaPrevisto: number;
  deltaSala: number | null;
  indiceSalaPrevisto: number | null;
  nivelPrevisto: Nivel;
  semLeiturasRecentes: boolean;
  demonstracao: boolean;
  // false: sem previsão da rua, a sala fica no índice da última leitura válida e indiceRuaPrevisto vem 0.
  ruaDisponivel: boolean;
};

export const SEM_DADOS_PARA_PREVER =
  'Sem dados para prever: não há previsão da rua nem leitura válida recente desta sala para calcular.';
export const RUA_INDISPONIVEL =
  'A previsão da rua não está disponível. A previsão usa a última leitura válida da sala.';

function numeroOuNulo(valor: unknown): valor is number | null {
  return valor === null || typeof valor === 'number';
}

export function lerPrevisao(valor: unknown): Previsao {
  if (!valor || typeof valor !== 'object') throw new Error(RESPOSTA_INESPERADA);
  const item = valor as Record<string, unknown>;
  if (
    typeof item.ambienteId !== 'string' ||
    typeof item.previstoPara !== 'string' ||
    typeof item.indiceRuaPrevisto !== 'number' ||
    !numeroOuNulo(item.deltaSala) ||
    !numeroOuNulo(item.indiceSalaPrevisto) ||
    typeof item.nivelPrevisto !== 'string' ||
    !ehNivel(item.nivelPrevisto) ||
    typeof item.semLeiturasRecentes !== 'boolean' ||
    typeof item.demonstracao !== 'boolean' ||
    typeof item.ruaDisponivel !== 'boolean' ||
    // Sem a rua, o único número da previsão é o índice da sala.
    (!item.ruaDisponivel && item.indiceSalaPrevisto === null)
  ) {
    throw new Error(RESPOSTA_INESPERADA);
  }
  return {
    ambienteId: item.ambienteId,
    previstoPara: item.previstoPara,
    indiceRuaPrevisto: item.indiceRuaPrevisto,
    deltaSala: item.deltaSala,
    indiceSalaPrevisto: item.indiceSalaPrevisto,
    nivelPrevisto: item.nivelPrevisto,
    semLeiturasRecentes: item.semLeiturasRecentes,
    demonstracao: item.demonstracao,
    ruaDisponivel: item.ruaDisponivel,
  };
}

// Corpo do 503 da previsão: {"motivo":"sem-dados-para-prever"}. Outro 503 continua sendo falha do servidor.
export function semDadosParaPrever(corpo: unknown): boolean {
  return !!corpo && typeof corpo === 'object' && (corpo as Record<string, unknown>).motivo === 'sem-dados-para-prever';
}
