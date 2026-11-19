import { useCallback, useState } from 'react';
import { useFocusEffect } from 'expo-router';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { apresentarNivel, type Nivel } from '../domain/nivel';
import { diametro, textoPonto, type Mapa, type PontoMapa } from '../domain/mapa';
import { obterMapa } from '../services/api';
import { cores, ui } from './tema';

const cor = (nivel: Nivel | null) => nivel ? apresentarNivel(nivel).cor : '#71717A';
const niveis: Nivel[] = ['frio', 'normal', 'atencao', 'cuidado', 'perigo'];

export function MapaTermico({ id }: { id: string }) {
  const [futuro, setFuturo] = useState(false);
  const [mapa, setMapa] = useState<Mapa | null>(null);
  const [selecionado, setSelecionado] = useState<PontoMapa | null>(null);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [largura, setLargura] = useState(0);
  useFocusEffect(useCallback(() => {
    let ativo = true;
    setCarregando(true);
    setMapa(null);
    setSelecionado(null);
    setErro('');
    obterMapa(id, futuro).then(m => { if (ativo) setMapa(m); })
      .catch((e: unknown) => { if (ativo) setErro(e instanceof Error ? e.message : 'Mapa indisponível.'); })
      .finally(() => { if (ativo) setCarregando(false); });
    return () => { ativo = false; };
  }, [id, futuro]));
  const escala = largura / (mapa?.larguraM ?? 6);
  const altura = (mapa?.profundidadeM ?? 4) * escala;
  return <View style={ui.cartao}>
    <Text style={ui.legenda}>DENTRO DA SALA</Text>
    <Text accessibilityRole="header" style={ui.secao}>Mapa térmico</Text>
    <View style={styles.alternador}>
      {[false, true].map(f => <Pressable key={String(f)} accessibilityRole="button" accessibilityState={{ selected: futuro === f }}
        onPress={() => setFuturo(f)} style={[styles.opcao, futuro === f && styles.opcaoAtiva]}>
        <Text style={[styles.opcaoTexto, futuro === f && styles.opcaoTextoAtiva]}>{f ? 'Previsão +1 h' : 'Agora'}</Text>
      </Pressable>)}
    </View>
    {carregando ? <View style={ui.centro}><ActivityIndicator color={cores.turquesa} accessibilityLabel="Carregando mapa" /><Text style={ui.texto}>Carregando mapa</Text></View> : erro ? <Text style={ui.erro} accessibilityRole="alert">{erro}</Text> : !mapa ?
      <Text style={ui.aviso}>Previsão +1 h por ponto indisponível nesta origem de dados.</Text> : <>
        <Text style={ui.texto}>Planta {mapa.larguraM ?? 'sem dimensões'} × {mapa.profundidadeM ?? 'sem dimensões'} m · origem no canto inferior esquerdo.</Text>
        <View style={styles.legenda}>
          {niveis.map(nivel => <View key={nivel} style={styles.itemLegenda}><View style={[styles.corLegenda, { backgroundColor: cor(nivel) }]} /><Text style={styles.textoLegenda}>{apresentarNivel(nivel).nome}</Text></View>)}
          <View style={styles.itemLegenda}><View style={[styles.corLegenda, { backgroundColor: cor(null) }]} /><Text style={styles.textoLegenda}>Sem leitura</Text></View>
        </View>
        <Text style={styles.ajuda}>Círculos: sensores · quadrados: cadeiras. Símbolo maior indica maior desvio absoluto da média (limite visual: 8 °C).</Text>
        <View onLayout={e => setLargura(Math.max(0, e.nativeEvent.layout.width - 40))} style={styles.planta}>
          <View style={{ width: largura, height: altura, borderWidth: 1, borderColor: cores.secundario, backgroundColor: cores.fundo }}>
            {mapa.grade.map((c, i) => {
              const x = Math.max(0, c.x - mapa.passoM / 2), y = Math.max(0, c.y - mapa.passoM / 2);
              return <View key={i} accessible={false} style={{ position: 'absolute', left: x * escala, bottom: y * escala,
                width: Math.max(0, Math.min(c.x + mapa.passoM / 2, mapa.larguraM ?? 0) - x) * escala,
                height: Math.max(0, Math.min(c.y + mapa.passoM / 2, mapa.profundidadeM ?? 0) - y) * escala,
                backgroundColor: cor(c.nivel), opacity: .25 }} />;
            })}
            {mapa.pontos.map(p => {
              const d = diametro(p.desvioC);
              return <Pressable key={`${p.tipo}-${p.id}`} accessibilityRole="button" accessibilityLabel={textoPonto(p)} hitSlop={12}
                accessibilityState={{ selected: selecionado === p }}
                onPress={() => setSelecionado(p)} style={{ position: 'absolute', left: (mapa.larguraM ? p.x * escala : largura / 2) - d / 2,
                  bottom: (mapa.profundidadeM ? p.y * escala : altura / 2) - d / 2,
                  width: d, height: d, borderRadius: p.tipo === 'sensor' ? d / 2 : 3, borderWidth: selecionado === p ? 3 : 1, borderColor: cores.marinho, backgroundColor: cor(p.semLeitura ? null : p.nivel) }} />;
            })}
          </View>
        </View>
        {selecionado ? <Text style={styles.detalheSelecionado} accessibilityLiveRegion="polite">{textoPonto(selecionado)}</Text> : <Text style={ui.texto}>Toque em um ponto ou selecione-o na lista abaixo.</Text>}
        <Text accessibilityRole="header" style={styles.subtitulo}>Pontos da sala</Text>
        {mapa.pontos.length === 0 ? <Text style={ui.texto}>Nenhum ponto cadastrado nesta sala.</Text> : mapa.pontos.map(p => <Pressable key={`${p.tipo}-${p.id}`} onPress={() => setSelecionado(p)} accessibilityRole="button" accessibilityState={{ selected: selecionado === p }} style={[styles.ponto, selecionado === p && styles.pontoSelecionado]}>
          <View style={[styles.corPonto, { backgroundColor: cor(p.semLeitura ? null : p.nivel), borderRadius: p.tipo === 'sensor' ? 8 : 2 }]} />
          <Text style={styles.textoPonto}>{textoPonto(p)}</Text>
        </Pressable>)}
        <Text accessibilityRole="header" style={styles.subtitulo}>Precauções</Text>
        <View style={styles.precaucoes}>{mapa.precaucoes.length ? mapa.precaucoes.map((p, i) => <Text style={ui.texto} key={i}>• {p}</Text>) : <Text style={ui.texto}>Nenhuma precaução adicional informada.</Text>}</View>
      </>}
  </View>;
}

const styles = StyleSheet.create({
  alternador: { flexDirection: 'row', backgroundColor: cores.fundo, borderRadius: 12, padding: 4, gap: 4 },
  opcao: { flex: 1, minHeight: 48, padding: 10, alignItems: 'center', justifyContent: 'center', borderRadius: 9 },
  opcaoAtiva: { backgroundColor: cores.marinho },
  opcaoTexto: { color: cores.secundario, fontSize: 14, fontWeight: '700', textAlign: 'center' },
  opcaoTextoAtiva: { color: '#fff' },
  legenda: { flexDirection: 'row', flexWrap: 'wrap', gap: 10 },
  itemLegenda: { flexDirection: 'row', alignItems: 'center', gap: 5 },
  corLegenda: { width: 9, height: 9, borderRadius: 3 },
  textoLegenda: { color: cores.secundario, fontSize: 12 },
  ajuda: { color: cores.secundario, fontSize: 13, lineHeight: 20 },
  planta: { padding: 20, backgroundColor: cores.fundo, borderRadius: 12 },
  detalheSelecionado: { color: cores.texto, backgroundColor: cores.turquesaClaro, borderRadius: 10, padding: 12, fontSize: 15, lineHeight: 23 },
  subtitulo: { color: cores.texto, fontSize: 16, fontWeight: '700', marginTop: 8 },
  ponto: { minHeight: 48, flexDirection: 'row', alignItems: 'flex-start', gap: 10, backgroundColor: cores.fundo, borderRadius: 10, padding: 12, borderWidth: 1, borderColor: cores.borda },
  pontoSelecionado: { borderColor: cores.turquesa, backgroundColor: cores.turquesaClaro },
  corPonto: { width: 12, height: 12, marginTop: 5 },
  textoPonto: { flex: 1, color: cores.texto, fontSize: 14, lineHeight: 22 },
  precaucoes: { gap: 8 },
});
