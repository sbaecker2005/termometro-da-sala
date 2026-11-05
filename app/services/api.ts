import Constants from 'expo-constants';
import { lerOcorrencias, type Ocorrencia } from '../domain/ocorrencia';
import { ehNivel, type Nivel } from '../domain/nivel';

export type { Ocorrencia };

export type Ambiente = {
  id: string;
  nome: string;
  andar: number;
  faceSol: string;
  nivelAtual: Nivel;
  indiceAtual: number;
  ultimaLeituraEm: string;
};

export type Previsao = {
  ambienteId: string;
  previstoPara: string;
  indiceRuaPrevisto: number;
  deltaSala: number | null;
  indiceSalaPrevisto: number | null;
  nivelPrevisto: Nivel;
  semLeiturasRecentes: boolean;
  demonstracao: boolean;
};

export type FotoOcorrencia = { uri: string; nome: string; tipo: string };

const ocorrenciasSimuladas: Ocorrencia[] = [];
let sequenciaLocal = 0;

// Hermes do Expo Go não implementa crypto.randomUUID.
function idLocal(): string {
  sequenciaLocal += 1;
  return `${Date.now().toString(36)}-${sequenciaLocal}`;
}

const AMBIENTES: Ambiente[] = [
  {
    id: 'sala-03',
    nome: 'Sala 3',
    andar: 1,
    faceSol: 'oeste',
    nivelAtual: 'cuidado',
    indiceAtual: 34.2,
    ultimaLeituraEm: '2026-11-06T17:10:00Z',
  },
  {
    id: 'sala-01',
    nome: 'Sala 1',
    andar: 0,
    faceSol: 'leste',
    nivelAtual: 'normal',
    indiceAtual: 24.5,
    ultimaLeituraEm: '2026-11-06T17:08:00Z',
  },
  {
    id: 'sala-12',
    nome: 'Sala 12',
    andar: 2,
    faceSol: 'norte',
    nivelAtual: 'atencao',
    indiceAtual: 29.4,
    ultimaLeituraEm: '2026-11-06T17:09:00Z',
  },
  {
    id: 'sala-07',
    nome: 'Sala 7',
    andar: 1,
    faceSol: 'sul',
    nivelAtual: 'perigo',
    indiceAtual: 42.1,
    ultimaLeituraEm: '2026-11-06T17:11:00Z',
  },
  {
    id: 'sala-09',
    nome: 'Sala 9',
    andar: 2,
    faceSol: 'oeste',
    nivelAtual: 'atencao',
    indiceAtual: 28.0,
    ultimaLeituraEm: '2026-11-06T12:00:00Z',
  },
];

const PREVISOES: Record<string, Previsao> = {
  'sala-03': {
    ambienteId: 'sala-03',
    previstoPara: '2026-11-06T19:10:00Z',
    indiceRuaPrevisto: 34,
    deltaSala: 4,
    indiceSalaPrevisto: 38,
    nivelPrevisto: 'cuidado',
    semLeiturasRecentes: false,
    demonstracao: true,
  },
  'sala-01': {
    ambienteId: 'sala-01',
    previstoPara: '2026-11-06T19:08:00Z',
    indiceRuaPrevisto: 34,
    deltaSala: -10,
    indiceSalaPrevisto: 24,
    nivelPrevisto: 'normal',
    semLeiturasRecentes: false,
    demonstracao: false,
  },
  'sala-12': {
    ambienteId: 'sala-12',
    previstoPara: '2026-11-06T19:09:00Z',
    indiceRuaPrevisto: 34,
    deltaSala: -4,
    indiceSalaPrevisto: 30,
    nivelPrevisto: 'atencao',
    semLeiturasRecentes: false,
    demonstracao: false,
  },
  'sala-07': {
    ambienteId: 'sala-07',
    previstoPara: '2026-11-06T19:11:00Z',
    indiceRuaPrevisto: 36,
    deltaSala: 6,
    indiceSalaPrevisto: 42,
    nivelPrevisto: 'perigo',
    semLeiturasRecentes: false,
    demonstracao: false,
  },
  'sala-09': {
    ambienteId: 'sala-09',
    previstoPara: '2026-11-06T19:10:00Z',
    indiceRuaPrevisto: 36,
    deltaSala: null,
    indiceSalaPrevisto: null,
    nivelPrevisto: 'cuidado',
    semLeiturasRecentes: true,
    demonstracao: false,
  },
};

function configuracao(): { apiUrl: string; apiMock: boolean } {
  const bruto: unknown = Constants.expoConfig?.extra;
  const extra = bruto && typeof bruto === 'object' ? (bruto as { apiUrl?: unknown; apiMock?: unknown }) : {};
  const apiUrl = typeof extra.apiUrl === 'string' && extra.apiUrl.length > 0 ? extra.apiUrl : 'http://localhost:5080';
  const apiMock = extra.apiMock === false ? false : true;
  return { apiUrl: apiUrl.replace(/\/$/, ''), apiMock };
}

function numeroOuNulo(valor: unknown): valor is number | null {
  return valor === null || typeof valor === 'number';
}

function lerAmbiente(valor: unknown): Ambiente {
  if (!valor || typeof valor !== 'object') throw new Error('resposta invalida');
  const item = valor as Record<string, unknown>;
  if (
    typeof item.id !== 'string' ||
    typeof item.nome !== 'string' ||
    typeof item.andar !== 'number' ||
    typeof item.faceSol !== 'string' ||
    typeof item.nivelAtual !== 'string' ||
    !ehNivel(item.nivelAtual) ||
    typeof item.indiceAtual !== 'number' ||
    typeof item.ultimaLeituraEm !== 'string'
  ) {
    throw new Error('resposta invalida');
  }
  return {
    id: item.id,
    nome: item.nome,
    andar: item.andar,
    faceSol: item.faceSol,
    nivelAtual: item.nivelAtual,
    indiceAtual: item.indiceAtual,
    ultimaLeituraEm: item.ultimaLeituraEm,
  };
}

function lerPrevisao(valor: unknown): Previsao {
  if (!valor || typeof valor !== 'object') throw new Error('resposta invalida');
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
    typeof item.demonstracao !== 'boolean'
  ) {
    throw new Error('resposta invalida');
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
  };
}

async function lerJson(resposta: Response): Promise<unknown> {
  return (await resposta.json()) as unknown;
}

export async function listarAmbientes(): Promise<Ambiente[]> {
  const { apiUrl, apiMock } = configuracao();
  if (apiMock) return AMBIENTES;
  const resposta = await fetch(`${apiUrl}/ambientes`);
  if (!resposta.ok) throw new Error(`Não foi possível carregar as salas (${resposta.status}).`);
  const dados = await lerJson(resposta);
  if (!Array.isArray(dados)) throw new Error('resposta invalida');
  return dados.map(lerAmbiente);
}

export async function obterPrevisao(ambienteId: string): Promise<Previsao> {
  const { apiUrl, apiMock } = configuracao();
  if (apiMock) {
    const previsao = PREVISOES[ambienteId];
    if (!previsao) throw new Error('Sala não encontrada.');
    return previsao;
  }
  const resposta = await fetch(`${apiUrl}/ambientes/${encodeURIComponent(ambienteId)}/previsao`);
  if (resposta.status === 404) throw new Error('Sala não encontrada.');
  if (!resposta.ok) throw new Error(`Não foi possível carregar a previsão (${resposta.status}).`);
  return lerPrevisao(await lerJson(resposta));
}

export async function listarOcorrencias(ambienteId: string): Promise<Ocorrencia[]> {
  const { apiUrl, apiMock } = configuracao();
  if (apiMock) return ocorrenciasSimuladas.filter((item) => item.ambienteId === ambienteId);
  const resposta = await fetch(`${apiUrl}/ambientes/${encodeURIComponent(ambienteId)}/ocorrencias`);
  // 404 é lista vazia: a rota pode ainda não existir e a sala pode não ter o recurso.
  if (resposta.status === 404) return [];
  if (!resposta.ok) throw new Error(`Não foi possível carregar as ocorrências (${resposta.status}).`);
  return lerOcorrencias(await lerJson(resposta));
}

export async function registrarOcorrencia(entrada: {
  ambienteId: string;
  descricao: string;
  foto: FotoOcorrencia;
}): Promise<{ id: string }> {
  const { apiUrl, apiMock } = configuracao();
  if (apiMock) {
    const criada: Ocorrencia = {
      id: idLocal(),
      ambienteId: entrada.ambienteId,
      descricao: entrada.descricao.trim(),
      criadaEm: new Date().toISOString(),
    };
    ocorrenciasSimuladas.push(criada);
    return { id: criada.id };
  }
  const form = new FormData();
  form.append('ambienteId', entrada.ambienteId);
  form.append('descricao', entrada.descricao.trim());
  // O FormData do React Native aceita { uri, name, type } no lugar de Blob.
  const arquivo = { uri: entrada.foto.uri, name: entrada.foto.nome, type: entrada.foto.tipo };
  form.append('foto', arquivo as unknown as Blob);
  const resposta = await fetch(`${apiUrl}/ocorrencias`, { method: 'POST', body: form });
  if (resposta.status !== 201) throw new Error(`Não foi possível registrar a ocorrência (${resposta.status}).`);
  const corpo = await lerJson(resposta);
  if (!corpo || typeof corpo !== 'object' || !('id' in corpo) || typeof corpo.id !== 'string') {
    throw new Error('resposta invalida');
  }
  return { id: corpo.id };
}
