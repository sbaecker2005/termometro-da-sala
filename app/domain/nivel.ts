export type Nivel = 'frio' | 'normal' | 'atencao' | 'cuidado' | 'perigo';

export type ApresentacaoNivel = {
  nome: string;
  acao: string;
  cor: string;
};

const APRESENTACAO: Record<Nivel, ApresentacaoNivel> = {
  frio: { nome: 'Frio', acao: 'Fechar janelas, agasalhar e oferecer cobertor; à noite, avisar o plantão', cor: '#1D4ED8' },
  normal: { nome: 'Normal', acao: 'Nenhuma', cor: '#166534' },
  atencao: {
    nome: 'Atenção',
    acao: 'Liberar água, abrir janelas e ligar ventiladores',
    cor: '#A16207',
  },
  cuidado: {
    nome: 'Cuidado extremo',
    acao: 'Pausa, troca de sala ou adiar a Educação Física',
    cor: '#B91C1C',
  },
  perigo: { nome: 'Perigo', acao: 'Tirar a turma da sala', cor: '#7F1D1D' },
};

export function ehNivel(valor: string): valor is Nivel {
  return valor in APRESENTACAO;
}

// 27 entra em atenção, 32 em cuidado e 41 em perigo. O topo da faixa anterior fica de fora.
export function nivelDeIndice(indice: number): Nivel {
  if (!Number.isFinite(indice)) throw new Error('índice inválido');
  if (indice < 18) return 'frio';
  if (indice < 27) return 'normal';
  if (indice < 32) return 'atencao';
  if (indice < 41) return 'cuidado';
  return 'perigo';
}

export function apresentarNivel(nivel: Nivel): ApresentacaoNivel {
  return APRESENTACAO[nivel];
}
