import Constants from 'expo-constants';
import { lerOcorrencias, RESPOSTA_INESPERADA, type Ocorrencia } from '../domain/ocorrencia';
import { ehNivel, type Nivel } from '../domain/nivel';
import { lerPrevisao, semDadosParaPrever, type Previsao } from '../domain/previsao';

export type { Ocorrencia, Previsao };

export type Ambiente = {
  id: string;
  nome: string;
  andar: number;
  faceSol: string;
  nivelAtual: Nivel;
  indiceAtual: number;
  ultimaLeituraEm: string;
  semLeitura: boolean;
};

export type FotoOcorrencia = { uri: string; nome: string; tipo: string };

const ocorrenciasSimuladas: Ocorrencia[] = [];
let sequenciaLocal = 0;

// Hermes do Expo Go não implementa crypto.randomUUID.
function idLocal(): string {
  sequenciaLocal += 1;
  return `${Date.now().toString(36)}-${sequenciaLocal}`;
}

const MINUTO = 60_000;

type AmbienteBase = Omit<Ambiente, 'ultimaLeituraEm' | 'semLeitura'> & { minutosDesdeLeitura: number };

// Sala 9 fica com leitura de 5 h atrás para o aviso de "sem leituras recentes" fazer sentido.
const AMBIENTES: AmbienteBase[] = [
  { id: 'sala-05', nome: 'Sala 5', andar: 0, faceSol: 'sul', nivelAtual: 'frio', indiceAtual: 17.9, minutosDesdeLeitura: 2 },
  { id: 'sala-03', nome: 'Sala 3', andar: 1, faceSol: 'oeste', nivelAtual: 'cuidado', indiceAtual: 34.2, minutosDesdeLeitura: 2 },
  { id: 'sala-01', nome: 'Sala 1', andar: 0, faceSol: 'leste', nivelAtual: 'normal', indiceAtual: 24.5, minutosDesdeLeitura: 4 },
  { id: 'sala-12', nome: 'Sala 12', andar: 2, faceSol: 'norte', nivelAtual: 'atencao', indiceAtual: 29.4, minutosDesdeLeitura: 3 },
  { id: 'sala-07', nome: 'Sala 7', andar: 1, faceSol: 'sul', nivelAtual: 'perigo', indiceAtual: 42.1, minutosDesdeLeitura: 1 },
  { id: 'sala-09', nome: 'Sala 9', andar: 2, faceSol: 'oeste', nivelAtual: 'atencao', indiceAtual: 28.0, minutosDesdeLeitura: 300 },
];

// As datas acompanham o relógio do aparelho: o vídeo gravado em qualquer dia mostra leituras de minutos atrás.
function ambientesSimulados(agora: number): Ambiente[] {
  return AMBIENTES.map(({ minutosDesdeLeitura, ...sala }) => ({
    ...sala,
    ultimaLeituraEm: new Date(agora - minutosDesdeLeitura * MINUTO).toISOString(),
    semLeitura: false,
  }));
}

type PrevisaoBase = Omit<Previsao, 'ambienteId' | 'previstoPara'>;

const PREVISOES: Record<string, PrevisaoBase> = {
  'sala-05': {
    indiceRuaPrevisto: 17,
    deltaSala: 0,
    indiceSalaPrevisto: 17,
    nivelPrevisto: 'frio',
    semLeiturasRecentes: false,
    demonstracao: true,
    ruaDisponivel: true,
  },
  'sala-03': {
    indiceRuaPrevisto: 34,
    deltaSala: 4,
    indiceSalaPrevisto: 38,
    nivelPrevisto: 'cuidado',
    semLeiturasRecentes: false,
    demonstracao: true,
    ruaDisponivel: true,
  },
  'sala-01': {
    indiceRuaPrevisto: 34,
    deltaSala: -10,
    indiceSalaPrevisto: 24,
    nivelPrevisto: 'normal',
    semLeiturasRecentes: false,
    demonstracao: false,
    ruaDisponivel: true,
  },
  'sala-12': {
    indiceRuaPrevisto: 34,
    deltaSala: -4,
    indiceSalaPrevisto: 30,
    nivelPrevisto: 'atencao',
    semLeiturasRecentes: false,
    demonstracao: false,
    ruaDisponivel: true,
  },
  'sala-07': {
    indiceRuaPrevisto: 36,
    deltaSala: 6,
    indiceSalaPrevisto: 42,
    nivelPrevisto: 'perigo',
    semLeiturasRecentes: false,
    demonstracao: false,
    ruaDisponivel: true,
  },
  'sala-09': {
    indiceRuaPrevisto: 36,
    deltaSala: null,
    indiceSalaPrevisto: null,
    nivelPrevisto: 'cuidado',
    semLeiturasRecentes: true,
    demonstracao: false,
    ruaDisponivel: true,
  },
};

const SEM_CONEXAO = 'Não foi possível falar com o servidor. Confira a rede e tente de novo.';

// EXPO_PUBLIC_API_MOCK: '0' usa a API; 'vazio' e 'erro' forçam esses estados da lista; o resto liga o simulado.
type ModoSimulado = 'desligado' | 'normal' | 'vazio' | 'erro';

function lerModo(valor: unknown): ModoSimulado {
  if (valor === '0' || valor === false) return 'desligado';
  if (valor === 'vazio' || valor === 'erro') return valor;
  return 'normal';
}

// Atraso curto para o estado "carregando" aparecer também no simulado.
async function simular<T>(modo: ModoSimulado, resposta: () => T): Promise<T> {
  await new Promise((resolver) => setTimeout(resolver, 600));
  if (modo === 'erro') throw new Error(SEM_CONEXAO);
  return resposta();
}

// O fetch do React Native rejeita com "Network request failed", em inglês; a tela recebe o texto em português.
async function buscar(url: string, init?: RequestInit): Promise<Response> {
  try {
    return await fetch(url, init);
  } catch {
    throw new Error(SEM_CONEXAO);
  }
}

function configuracao(): { apiUrl: string; modo: ModoSimulado } {
  const bruto: unknown = Constants.expoConfig?.extra;
  const extra = bruto && typeof bruto === 'object' ? (bruto as { apiUrl?: unknown; apiMock?: unknown }) : {};
  const apiUrl = typeof extra.apiUrl === 'string' && extra.apiUrl.length > 0 ? extra.apiUrl : 'http://localhost:5080';
  return { apiUrl: apiUrl.replace(/\/$/, ''), modo: lerModo(extra.apiMock) };
}

function lerAmbiente(valor: unknown): Ambiente {
  if (!valor || typeof valor !== 'object') throw new Error(RESPOSTA_INESPERADA);
  const item = valor as Record<string, unknown>;
  if (
    typeof item.id !== 'string' ||
    typeof item.nome !== 'string' ||
    typeof item.andar !== 'number' ||
    typeof item.faceSol !== 'string' ||
    typeof item.nivelAtual !== 'string' ||
    !ehNivel(item.nivelAtual) ||
    typeof item.indiceAtual !== 'number' ||
    typeof item.ultimaLeituraEm !== 'string' ||
    (item.semLeitura !== undefined && typeof item.semLeitura !== 'boolean')
  ) {
    throw new Error(RESPOSTA_INESPERADA);
  }
  return {
    id: item.id,
    nome: item.nome,
    andar: item.andar,
    faceSol: item.faceSol,
    nivelAtual: item.nivelAtual,
    indiceAtual: item.indiceAtual,
    ultimaLeituraEm: item.ultimaLeituraEm,
    // Sala sem leitura chega com nível "normal", índice 0 e data vazia; esses valores não podem ir para a tela.
    semLeitura: item.semLeitura === true || item.ultimaLeituraEm === '',
  };
}

async function lerJson(resposta: Response): Promise<unknown> {
  try {
    return (await resposta.json()) as unknown;
  } catch {
    throw new Error(RESPOSTA_INESPERADA);
  }
}

export async function listarAmbientes(): Promise<Ambiente[]> {
  const { apiUrl, modo } = configuracao();
  if (modo !== 'desligado') return simular(modo, () => (modo === 'vazio' ? [] : ambientesSimulados(Date.now())));
  const resposta = await buscar(`${apiUrl}/ambientes`);
  if (!resposta.ok) throw new Error(`Não foi possível carregar as salas (${resposta.status}).`);
  const dados = await lerJson(resposta);
  if (!Array.isArray(dados)) throw new Error(RESPOSTA_INESPERADA);
  return dados.map(lerAmbiente);
}

// null: a API respondeu 503 "sem-dados-para-prever" (sem rua e sem leitura válida recente da sala); a tela avisa sem tratar como erro.
export async function obterPrevisao(ambienteId: string): Promise<Previsao | null> {
  const { apiUrl, modo } = configuracao();
  if (modo !== 'desligado') {
    return simular(modo, () => {
      const base = PREVISOES[ambienteId];
      if (!base) throw new Error('Sala não encontrada.');
      return { ...base, ambienteId, previstoPara: new Date(Date.now() + 120 * MINUTO).toISOString() };
    });
  }
  const resposta = await buscar(`${apiUrl}/ambientes/${encodeURIComponent(ambienteId)}/previsao`);
  if (resposta.status === 404) throw new Error('Sala não encontrada.');
  if (resposta.status === 503 && semDadosParaPrever(await resposta.json().catch(() => null))) return null;
  if (!resposta.ok) throw new Error(`Não foi possível carregar a previsão (${resposta.status}).`);
  return lerPrevisao(await lerJson(resposta));
}

export async function listarOcorrencias(ambienteId: string): Promise<Ocorrencia[]> {
  const { apiUrl, modo } = configuracao();
  if (modo !== 'desligado') {
    return simular(modo, () => ocorrenciasSimuladas.filter((item) => item.ambienteId === ambienteId));
  }
  const resposta = await buscar(`${apiUrl}/ambientes/${encodeURIComponent(ambienteId)}/ocorrencias`);
  // 404 é sala inexistente; a tela já mostra "Sala não encontrada" pela lista de salas.
  if (resposta.status === 404) return [];
  if (!resposta.ok) throw new Error(`Não foi possível carregar as ocorrências (${resposta.status}).`);
  return lerOcorrencias(await lerJson(resposta));
}

export async function registrarOcorrencia(entrada: {
  ambienteId: string;
  descricao: string;
  foto: FotoOcorrencia;
}): Promise<{ id: string }> {
  const { apiUrl, modo } = configuracao();
  if (modo !== 'desligado') {
    return simular(modo, () => {
      const criada: Ocorrencia = {
        id: idLocal(),
        ambienteId: entrada.ambienteId,
        descricao: entrada.descricao.trim(),
        criadaEm: new Date().toISOString(),
      };
      ocorrenciasSimuladas.push(criada);
      return { id: criada.id };
    });
  }
  const form = new FormData();
  form.append('ambienteId', entrada.ambienteId);
  form.append('descricao', entrada.descricao.trim());
  // O FormData do React Native aceita { uri, name, type } no lugar de Blob.
  const arquivo = { uri: entrada.foto.uri, name: entrada.foto.nome, type: entrada.foto.tipo };
  form.append('foto', arquivo as unknown as Blob);
  const resposta = await buscar(`${apiUrl}/ocorrencias`, { method: 'POST', body: form });
  if (resposta.status !== 201) throw new Error(`Não foi possível registrar a ocorrência (${resposta.status}).`);
  const corpo = await lerJson(resposta);
  if (!corpo || typeof corpo !== 'object' || !('id' in corpo) || typeof corpo.id !== 'string') {
    throw new Error(RESPOSTA_INESPERADA);
  }
  return { id: corpo.id };
}
