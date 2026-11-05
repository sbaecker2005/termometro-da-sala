import { Stack, router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { useCallback, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { apresentarNivel } from '../../domain/nivel';
import { RotuloNivel } from '../../components/RotuloNivel';
import { listarAmbientes, obterPrevisao, type Ambiente, type Previsao } from '../../services/api';
import { horaSaoPaulo, indiceEmGraus } from '../../services/hora';

function idParam(valor: string | string[] | undefined): string {
  return typeof valor === 'string' ? valor : (valor?.[0] ?? '');
}

export default function DetalheSala() {
  const params = useLocalSearchParams<{ id: string }>();
  const id = idParam(params.id);
  const [sala, setSala] = useState<Ambiente | null>(null);
  const [previsao, setPrevisao] = useState<Previsao | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  const carregar = useCallback(() => {
    setCarregando(true);
    setErro(null);
    Promise.all([listarAmbientes(), obterPrevisao(id)])
      .then(([salas, prev]) => {
        const encontrada = salas.find((item) => item.id === id);
        if (!encontrada) throw new Error('Sala não encontrada.');
        setSala(encontrada);
        setPrevisao(prev);
      })
      .catch((falha: unknown) => {
        setSala(null);
        setPrevisao(null);
        setErro(falha instanceof Error ? falha.message : 'Não foi possível carregar a sala.');
      })
      .finally(() => setCarregando(false));
  }, [id]);

  useFocusEffect(
    useCallback(() => {
      carregar();
    }, [carregar]),
  );

  const agora = sala ? apresentarNivel(sala.nivelAtual) : null;
  const previsto = previsao ? apresentarNivel(previsao.nivelPrevisto) : null;

  return (
    <ScrollView contentContainerStyle={styles.conteudo}>
      <Stack.Screen options={{ title: sala?.nome ?? 'Sala' }} />
      {carregando ? (
        <View style={styles.centro}>
          <ActivityIndicator color="#14532D" />
          <Text>Carregando sala</Text>
        </View>
      ) : erro || !sala || !previsao || !agora ? (
        <View style={styles.centro}>
          <Text style={styles.erro}>{erro ?? 'Sala não encontrada.'}</Text>
          <Pressable style={styles.botao} onPress={carregar} accessibilityRole="button">
            <Text style={styles.botaoTexto}>Tentar de novo</Text>
          </Pressable>
        </View>
      ) : (
        <View style={styles.bloco}>
          <Text style={styles.meta}>
            {sala.andar}º andar · face {sala.faceSol}
          </Text>
          {previsao.demonstracao ? <Text style={styles.etiqueta}>DEMONSTRAÇÃO</Text> : null}

          {previsao.semLeiturasRecentes ? (
            <View style={styles.bloco}>
              <Text style={styles.aviso}>sala sem leituras recentes</Text>
              <Text style={styles.secao}>Previsão da rua, daqui a 2 h</Text>
              <RotuloNivel nivel={previsao.nivelPrevisto} />
              <Text style={styles.indice}>{indiceEmGraus(previsao.indiceRuaPrevisto)}</Text>
              <Text style={styles.meta}>{horaSaoPaulo(previsao.previstoPara)}</Text>
            </View>
          ) : (
            <View style={styles.bloco}>
              <Text style={styles.secao}>Agora</Text>
              <RotuloNivel nivel={sala.nivelAtual} />
              <Text style={styles.indice}>{indiceEmGraus(sala.indiceAtual)}</Text>
              <Text style={styles.meta}>Última leitura {horaSaoPaulo(sala.ultimaLeituraEm)}</Text>
              <Text style={styles.acao}>Ação: {agora.acao}</Text>
              <Text style={styles.secao}>Daqui a 2 h</Text>
              <RotuloNivel nivel={previsao.nivelPrevisto} />
              <Text style={styles.indice}>
                {previsao.indiceSalaPrevisto === null ? 'Sem índice da sala' : indiceEmGraus(previsao.indiceSalaPrevisto)}
              </Text>
              <Text style={styles.meta}>Rua {indiceEmGraus(previsao.indiceRuaPrevisto)}</Text>
              <Text style={styles.meta}>{horaSaoPaulo(previsao.previstoPara)}</Text>
              <Text style={styles.acao}>Ação: {previsto?.acao}</Text>
            </View>
          )}

          <Pressable
            style={styles.botao}
            onPress={() => router.push(`/salas/${sala.id}/ocorrencia`)}
            accessibilityRole="button"
          >
            <Text style={styles.botaoTexto}>Registrar ocorrência</Text>
          </Pressable>
        </View>
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  conteudo: { padding: 20, gap: 8 },
  bloco: { gap: 8 },
  centro: { alignItems: 'center', gap: 12, paddingVertical: 32 },
  secao: { fontSize: 18, fontWeight: '700', marginTop: 12, color: '#18181B' },
  indice: { fontSize: 28, fontWeight: '700', color: '#18181B' },
  meta: { color: '#3F3F46' },
  acao: { color: '#18181B' },
  aviso: { backgroundColor: '#FEF3C7', color: '#92400E', padding: 12, borderRadius: 8, fontWeight: '700' },
  etiqueta: {
    alignSelf: 'flex-start',
    backgroundColor: '#1E3A8A',
    color: '#fff',
    fontWeight: '800',
    letterSpacing: 0.5,
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
    overflow: 'hidden',
  },
  erro: { color: '#B91C1C', textAlign: 'center' },
  botao: { backgroundColor: '#14532D', borderRadius: 8, padding: 14, marginTop: 16, alignItems: 'center' },
  botaoTexto: { color: '#fff', fontWeight: '700' },
});
