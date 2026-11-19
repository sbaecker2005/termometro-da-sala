import { Stack, router, useFocusEffect } from 'expo-router';
import { useCallback, useState } from 'react';
import { ActivityIndicator, FlatList, Pressable, StyleSheet, Text, View } from 'react-native';
import { RotuloNivel } from '../../components/RotuloNivel';
import { apresentarNivel } from '../../domain/nivel';
import { listarAmbientes, type Ambiente } from '../../services/api';
import { horaSaoPaulo, indiceEmGraus } from '../../services/hora';
import type { RotaParams } from '../../rotas';
import { lerProfessor, type Professor } from '../../services/professor';
import { cores, ui } from '../../components/tema';

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
              style={styles.acaoCabecalho}
              onPress={() => router.push({ pathname: '/', params: { editar: '1' } satisfies RotaParams['/'] })}
              accessibilityRole="button"
            >
              <Text style={styles.linkCabecalho}>Professor</Text>
            </Pressable>
          ),
        }}
      />
      {carregando ? (
        <View style={[ui.pagina, ui.centro]}>
          <ActivityIndicator color={cores.turquesa} />
          <Text style={ui.texto}>Carregando salas</Text>
        </View>
      ) : erro ? (
        <View style={[ui.pagina, ui.centro]}>
          <Text style={ui.secao}>Vamos tentar novamente</Text>
          <Text accessibilityRole="alert" style={ui.erro}>{erro}</Text>
          <Pressable style={ui.botao} onPress={carregar} accessibilityRole="button">
            <Text style={ui.botaoTexto}>Tentar de novo</Text>
          </Pressable>
        </View>
      ) : (
        <FlatList
          data={salas}
          keyExtractor={(sala) => sala.id}
          contentContainerStyle={ui.pagina}
          ListHeaderComponent={<View style={styles.abertura}>
            <Text style={ui.legenda}>AMBIENTES</Text>
            <Text accessibilityRole="header" style={ui.titulo}>Como estão as salas?</Text>
            {professor ? <View style={styles.professor}>
              <Text style={styles.professorNome}>{professor.nome}</Text>
              <Text style={ui.texto}>{professor.escola}</Text>
            </View> : null}
            <Text style={ui.texto}>Selecione uma sala para ver as condições e as orientações.</Text>
          </View>}
          ListEmptyComponent={<View style={ui.cartao}><Text style={ui.secao}>Nenhuma sala cadastrada</Text><Text style={ui.texto}>As salas disponíveis aparecerão aqui.</Text></View>}
          renderItem={({ item }) => (
            <Pressable
              style={({ pressed }) => [ui.cartao, styles.cartao, { borderLeftColor: item.semLeitura ? cores.borda : apresentarNivel(item.nivelAtual).cor }, pressed && { opacity: 0.8 }]}
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
                <Text style={ui.texto}>O sensor desta sala ainda não enviou leitura.</Text>
              ) : (
                <>
                  <View style={styles.leitura}>
                    <Text style={styles.indice}>{indiceEmGraus(item.indiceAtual)}</Text>
                    <Text style={styles.unidade}>ÍNDICE DE CALOR</Text>
                  </View>
                  <Text style={ui.texto}>Última leitura {horaSaoPaulo(item.ultimaLeituraEm)}</Text>
                </>
              )}
              <View style={styles.rodape}><Text style={styles.local}>{item.andar}º andar · face {item.faceSol}</Text><Text style={styles.link}>Ver sala →</Text></View>
            </Pressable>
          )}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  tela: { flex: 1 },
  abertura: { gap: 12 },
  professor: { borderLeftWidth: 3, borderLeftColor: cores.turquesa, paddingLeft: 12, gap: 2 },
  professorNome: { color: cores.texto, fontSize: 15, fontWeight: '700' },
  cartao: { borderLeftWidth: 5 },
  linha: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between', alignItems: 'center', gap: 10 },
  nome: { fontSize: 21, fontWeight: '700', color: cores.texto, flexShrink: 1 },
  leitura: { gap: 2 },
  indice: { fontSize: 36, fontWeight: '800', color: cores.texto, letterSpacing: -1 },
  unidade: { fontSize: 10, fontWeight: '700', color: cores.secundario, letterSpacing: 1.1 },
  rodape: { flexDirection: 'row', flexWrap: 'wrap', gap: 10, justifyContent: 'space-between', paddingTop: 12, borderTopWidth: 1, borderTopColor: cores.borda },
  local: { color: cores.secundario, fontSize: 13 },
  link: { color: cores.turquesa, fontSize: 14, fontWeight: '700' },
  semLeitura: {
    backgroundColor: '#E4E4E7',
    color: '#27272A',
    fontWeight: '700',
    borderRadius: 6,
    paddingHorizontal: 8,
    paddingVertical: 4,
    overflow: 'hidden',
  },
  acaoCabecalho: { minHeight: 44, minWidth: 44, paddingHorizontal: 8, justifyContent: 'center' },
  linkCabecalho: { color: '#91E6DE', fontWeight: '700' },
});
