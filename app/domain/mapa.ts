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
  // Mesma sala da demonstração da API (POST /demo/mapa) e do desktop: 4 sensores (S1–S4) e 20 cadeiras (C01–C20),
  // com as cadeiras 09 e 20 marcadas como lugar vulnerável.
  const sensores = [['janela', 'Janela', 0, 2], ['fundo', 'Fundo', 6, 4], ['porta', 'Porta', 6, 0], ['centro', 'Centro', 3, 2]] as const;
  const cadeiras = Array.from({ length: 20 }, (_, i) => {
    const n = String(i + 1).padStart(2, '0');
    return { id: `cadeira-${n}`, rotulo: `Cadeira ${n}`, tipo: 'lugar', x: .75 + 1.5 * (i % 4), y: .4 + .8 * Math.floor(i / 4), vulneravel: i === 8 || i === 19 };
  });
  const pontos = [
    ...sensores.map(([id, rotulo, x, y]) => ({ id, rotulo, tipo: 'sensor', x, y, vulneravel: false })),
    ...cadeiras,
  ].map(p => ({ ...p, ...celula(p.x, p.y), semLeitura: false, desvioC: valor(p.x) - valor(3) }));
  // A troca sai da cadeira vulnerável mais exposta para a cadeira comum, em nível normal, mais perto de 22,5 °C
  // (meio da faixa normal): a melhor da sala tanto no calor quanto no frio.
  const origem = pontos.find(p => p.id === 'cadeira-09')!;
  const destino = pontos.filter(p => p.tipo === 'lugar' && !p.vulneravel && p.nivel === 'normal')
    .sort((a, b) => Math.abs(valor(a.x) - 22.5) - Math.abs(valor(b.x) - 22.5))[0] ?? origem;
  const precaucoes = origem.nivel === 'frio' || origem.nivel === 'cuidado' || origem.nivel === 'perigo'
    ? [`trocar ${origem.rotulo} para ${destino.rotulo} (${valor(origem.x).toFixed(1)} → ${valor(destino.x).toFixed(1)} °C de índice)`] : [];
  return { ambienteId: id, larguraM: 6, profundidadeM: 4, passoM: .5, grade,
    pontos: pontos.map(p => ({ ...p, precaucoes: p.id === origem.id ? precaucoes : [] })),
    precaucoes: ['DEMONSTRAÇÃO: valores simulados, inclusive a previsão +1 h.', ...precaucoes, 'Verificar janela, sol e corrente de ar; avaliar presencialmente os lugares vulneráveis.'] };
}
