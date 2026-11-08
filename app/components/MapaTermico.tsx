import { useCallback, useState } from 'react';
import { useFocusEffect } from 'expo-router';
import { ActivityIndicator, Pressable, Text, View } from 'react-native';
import { apresentarNivel, type Nivel } from '../domain/nivel';
import { diametro, textoPonto, type Mapa, type PontoMapa } from '../domain/mapa';
import { obterMapa } from '../services/api';

const cor = (nivel: Nivel | null) => nivel ? apresentarNivel(nivel).cor : '#71717A';

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
  return <View style={{ gap: 10 }}>
    <Text accessibilityRole="header" style={{ fontSize: 18, fontWeight: '700' }}>Mapa térmico</Text>
    <View style={{ flexDirection: 'row', gap: 12 }}>
      {[false, true].map(f => <Pressable key={String(f)} accessibilityRole="button" accessibilityState={{ selected: futuro === f }}
        onPress={() => setFuturo(f)} style={{ padding: 12, borderWidth: futuro === f ? 2 : 1, borderRadius: 6 }}>
        <Text>{f ? 'Previsão +1 h' : 'Agora'}</Text>
      </Pressable>)}
    </View>
    {carregando ? <ActivityIndicator accessibilityLabel="Carregando mapa" /> : erro ? <Text accessibilityRole="alert">{erro}</Text> : !mapa ?
      <Text>Previsão +1 h por ponto indisponível nesta origem de dados.</Text> : <>
        <Text>Planta {mapa.larguraM ?? 'sem dimensões'} × {mapa.profundidadeM ?? 'sem dimensões'} m. Origem no canto inferior esquerdo.</Text>
        <Text>Azul: Frio; verde: Normal; amarelo: Atenção; vermelho: Cuidado extremo / Perigo; cinza: sem leitura. Bola maior: maior desvio absoluto da média (limite visual: 8 °C).</Text>
        <View onLayout={e => setLargura(Math.max(0, e.nativeEvent.layout.width - 40))} style={{ padding: 20 }}>
          <View style={{ width: largura, height: altura, borderWidth: 1, backgroundColor: '#F4F4F5' }}>
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
                onPress={() => setSelecionado(p)} style={{ position: 'absolute', left: (mapa.larguraM ? p.x * escala : largura / 2) - d / 2,
                  bottom: (mapa.profundidadeM ? p.y * escala : altura / 2) - d / 2,
                  width: d, height: d, borderRadius: d / 2, borderWidth: 1, borderColor: '#18181B', backgroundColor: cor(p.semLeitura ? null : p.nivel) }} />;
            })}
          </View>
        </View>
        {selecionado ? <Text accessibilityLiveRegion="polite">{textoPonto(selecionado)}</Text> : <Text>Toque em um ponto para ver seus detalhes.</Text>}
        <Text accessibilityRole="header">Pontos — lista acessível</Text>
        {mapa.pontos.map(p => <Text key={`${p.tipo}-${p.id}`}>{textoPonto(p)}</Text>)}
        <Text accessibilityRole="header">Precauções</Text>
        {mapa.precaucoes.length ? mapa.precaucoes.map((p, i) => <Text key={i}>• {p}</Text>) : <Text>Nenhuma precaução adicional informada.</Text>}
      </>}
  </View>;
}
