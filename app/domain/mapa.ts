import { apresentarNivel, ehNivel, nivelDeIndice, type Nivel } from './nivel.ts';

export type CelulaMapa = { x: number; y: number; temperaturaC: number | null; indiceCalor: number | null; nivel: Nivel | null };
export type PontoMapa = CelulaMapa & { id: string; rotulo: string; tipo: string; vulneravel: boolean; desvioC: number | null; semLeitura: boolean; precaucoes?: string[] };
export type Mapa = { ambienteId: string; larguraM: number | null; profundidadeM: number | null; passoM: number; grade: CelulaMapa[]; pontos: PontoMapa[]; precaucoes: string[] };

export function diametro(desvio: number | null): number {
  return 12 + Math.min(Math.abs(desvio ?? 0), 8) * 3;
}

export function textoPonto(ponto: PontoMapa): string {
  const valores = ponto.semLeitura || ponto.temperaturaC === null || ponto.nivel === null
    ? 'Sem leitura; verificar o sensor antes de decidir.'
    : `${ponto.temperaturaC.toFixed(1)} °C; ${apresentarNivel(ponto.nivel).nome}. Precaução: ${ponto.precaucoes?.length ? ponto.precaucoes.join('; ') : apresentarNivel(ponto.nivel).acao}.`;
  return `${ponto.rotulo} (${ponto.tipo})${ponto.vulneravel ? ', lugar vulnerável' : ''}: ${valores}`;
}

export function lerMapa(valor: unknown, id: string): Mapa {
  const erro = () => { throw new Error('Mapa inválido recebido do servidor.'); };
  if (!valor || typeof valor !== 'object') return erro();
  const m = valor as Mapa;
  const numero = (v: unknown): v is number => typeof v === 'number' && Number.isFinite(v);
  const opcional = (v: unknown) => v === null || numero(v);
  if (m.ambienteId !== id || !numero(m.passoM) || m.passoM <= 0
    || !((m.larguraM === null && m.profundidadeM === null)
      || (numero(m.larguraM) && m.larguraM >= .5 && m.larguraM <= 50 && numero(m.profundidadeM) && m.profundidadeM >= .5 && m.profundidadeM <= 50))
    || !Array.isArray(m.grade) || m.grade.length > 10201 || !Array.isArray(m.pontos) || m.pontos.length > 300
    || !Array.isArray(m.precaucoes) || !m.precaucoes.every(p => typeof p === 'string')) return erro();
  const celula = (c: CelulaMapa) => c && numero(c.x) && numero(c.y) && c.x >= 0 && c.y >= 0
    && c.x <= (m.larguraM ?? 0) && c.y <= (m.profundidadeM ?? 0)
    && opcional(c.temperaturaC) && opcional(c.indiceCalor)
    && (c.nivel === null || (typeof c.nivel === 'string' && ['frio', 'normal', 'atencao', 'cuidado', 'perigo'].includes(c.nivel) && ehNivel(c.nivel)));
  if (!m.grade.every(celula) || !m.pontos.every(p => celula(p) && typeof p.id === 'string'
    && typeof p.rotulo === 'string' && p.rotulo.trim().length > 0 && ['sensor', 'lugar'].includes(p.tipo)
    && typeof p.vulneravel === 'boolean' && typeof p.semLeitura === 'boolean' && opcional(p.desvioC)
    && (p.precaucoes === undefined || (Array.isArray(p.precaucoes) && p.precaucoes.every(t => typeof t === 'string'))))) return erro();
  return m;
}

export function mapaDemonstracao(id: string, futuro: boolean): Mapa {
  const valor = (x: number) => (id === 'sala-05' ? 15 : 35) + (24 - (id === 'sala-05' ? 15 : 35)) * x / 6 - (futuro ? 1 : 0);
  const celula = (x: number, y: number): CelulaMapa => ({ x, y, temperaturaC: valor(x), indiceCalor: valor(x), nivel: nivelDeIndice(valor(x)) });
  const grade: CelulaMapa[] = [];
  for (let x = 0; x <= 6; x += .5) for (let y = 0; y <= 4; y += .5) grade.push(celula(x, y));
  const pontos = [
    { id: 'janela', rotulo: 'Janela', tipo: 'sensor', x: 0, y: 2 },
    { id: 'fundo', rotulo: 'Fundo', tipo: 'sensor', x: 6, y: 2 },
    { id: 'cadeira-1', rotulo: 'Cadeira 1', tipo: 'lugar', x: 1, y: 1 },
    { id: 'cadeira-2', rotulo: 'Cadeira 2', tipo: 'lugar', x: 5, y: 3 },
  ].map(p => ({ ...p, ...celula(p.x, p.y), vulneravel: p.tipo === 'lugar', semLeitura: false, desvioC: valor(p.x) - valor(3) }));
  const origem = pontos[2], destino = pontos[3];
  const precaucoes = origem.nivel === 'frio' || origem.nivel === 'cuidado' || origem.nivel === 'perigo'
    ? [`trocar ${origem.rotulo} para ${destino.rotulo} (${valor(origem.x).toFixed(1)} → ${valor(destino.x).toFixed(1)} °C de índice)`] : [];
  return { ambienteId: id, larguraM: 6, profundidadeM: 4, passoM: .5, grade,
    pontos: pontos.map(p => ({ ...p, precaucoes: p.id === origem.id ? precaucoes : [] })),
    precaucoes: ['DEMONSTRAÇÃO: valores simulados, inclusive a previsão +1 h.', ...precaucoes, 'Verificar janela, sol e corrente de ar; avaliar presencialmente os lugares vulneráveis.'] };
}
