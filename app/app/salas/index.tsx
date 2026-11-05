import { Stack, router, useFocusEffect } from 'expo-router';
import { useCallback, useState } from 'react';
import { ActivityIndicator, FlatList, Pressable, StyleSheet, Text, View } from 'react-native';
import { RotuloNivel } from '../../components/RotuloNivel';
import { apresentarNivel } from '../../domain/nivel';
import { listarAmbientes, type Ambiente } from '../../services/api';
import { horaSaoPaulo, indiceEmGraus } from '../../services/hora';
import type { RotaParams } from '../../rotas';
import { lerProfessor, type Professor } from '../../services/professor';

export default function Salas() {
  const [salas, setSalas] = useState<Ambiente[]>([]);
  const [professor, setProfessor] = useState<Professor | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  const carregar = useCallback(() => {
    setCarregando(true);
    setErro(null);
    listarAmbientes()
      .then(setSalas)
      .catch((falha: unknown) => {
        setSalas([]);
        setErro(falha instanceof Error ? falha.message : 'Não foi possível carregar as salas.');
      })
      .finally(() => setCarregando(false));
  }, []);

  useFocusEffect(
    useCallback(() => {
      lerProfessor()
        .then(setProfessor)
        .catch(() => setProfessor(null));
      carregar();
    }, [carregar]),
  );

  return (
    <View style={styles.tela}>
      <Stack.Screen
        options={{
          title: 'Salas',
          headerRight: () => (
            <Pressable
              onPress={() => router.push({ pathname: '/', params: { editar: '1' } satisfies RotaParams['/'] })}
              accessibilityRole="button"
            >
              <Text style={styles.linkCabecalho}>Professor</Text>
            </Pressable>
          ),
        }}
      />
      {professor ? (
        <Text style={styles.professor}>
          {professor.nome} · {professor.escola}
        </Text>
      ) : null}
      {carregando ? (
        <View style={styles.centro}>
          <ActivityIndicator color="#14532D" />
          <Text>Carregando salas</Text>
        </View>
      ) : erro ? (
        <View style={styles.centro}>
          <Text style={styles.erro}>{erro}</Text>
          <Pressable style={styles.botao} onPress={carregar} accessibilityRole="button">
            <Text style={styles.botaoTexto}>Tentar de novo</Text>
          </Pressable>
        </View>
      ) : (
        <FlatList
          data={salas}
          keyExtractor={(sala) => sala.id}
          contentContainerStyle={salas.length === 0 ? styles.centro : styles.lista}
          ListEmptyComponent={<Text>Nenhuma sala cadastrada.</Text>}
          renderItem={({ item }) => (
            <Pressable
              style={styles.cartao}
              onPress={() =>
                router.push({ pathname: '/salas/[id]', params: { id: item.id } satisfies RotaParams['/salas/[id]'] })
              }
              accessibilityRole="button"
              accessibilityLabel={
                item.semLeitura
                  ? `${item.nome}, sem leitura`
                  : `${item.nome}, ${apresentarNivel(item.nivelAtual).nome}, ${indiceEmGraus(item.indiceAtual)}`
              }
            >
              <View style={styles.linha}>
                <Text style={styles.nome}>{item.nome}</Text>
                {item.semLeitura ? <Text style={styles.semLeitura}>Sem leitura</Text> : <RotuloNivel nivel={item.nivelAtual} />}
              </View>
              {item.semLeitura ? (
                <Text style={styles.detalhe}>O sensor desta sala ainda não enviou leitura.</Text>
              ) : (
                <>
                  <Text style={styles.detalhe}>Índice de calor {indiceEmGraus(item.indiceAtual)}</Text>
                  <Text style={styles.detalhe}>Última leitura {horaSaoPaulo(item.ultimaLeituraEm)}</Text>
                </>
              )}
            </Pressable>
          )}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  tela: { flex: 1 },
  professor: { paddingHorizontal: 16, paddingTop: 12, color: '#3F3F46' },
  lista: { padding: 16, gap: 12 },
  centro: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24, gap: 12 },
  cartao: { backgroundColor: '#fff', borderRadius: 12, padding: 14, gap: 6 },
  linha: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: 8 },
  nome: { fontSize: 18, fontWeight: '700', color: '#18181B' },
  detalhe: { color: '#3F3F46' },
  semLeitura: {
    backgroundColor: '#E4E4E7',
    color: '#27272A',
    fontWeight: '700',
    borderRadius: 6,
    paddingHorizontal: 8,
    paddingVertical: 4,
    overflow: 'hidden',
  },
  erro: { color: '#B91C1C', textAlign: 'center' },
  botao: { backgroundColor: '#14532D', borderRadius: 8, paddingHorizontal: 16, paddingVertical: 12 },
  botaoTexto: { color: '#fff', fontWeight: '700' },
  linkCabecalho: { color: '#fff', fontWeight: '600', paddingRight: 4 },
});
