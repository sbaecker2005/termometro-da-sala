import { Stack, router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { useCallback, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { apresentarNivel } from '../../domain/nivel';
import { RUA_INDISPONIVEL, SEM_DADOS_PARA_PREVER } from '../../domain/previsao';
import { RotuloNivel } from '../../components/RotuloNivel';
import { MapaTermico } from '../../components/MapaTermico';
import type { RotaParams } from '../../rotas';
import {
  listarAmbientes,
  listarOcorrencias,
  obterPrevisao,
  type Ambiente,
  type Ocorrencia,
  type Previsao,
} from '../../services/api';
import { horaSaoPaulo, indiceEmGraus } from '../../services/hora';
import { cores, ui } from '../../components/tema';

function idParam(valor: string | string[] | undefined): string {
  return typeof valor === 'string' ? valor : (valor?.[0] ?? '');
}

export default function DetalheSala() {
  const params = useLocalSearchParams<RotaParams['/salas/[id]']>();
  const id = idParam(params.id);
  const [sala, setSala] = useState<Ambiente | null>(null);
  const [previsao, setPrevisao] = useState<Previsao | null>(null);
  const [ocorrencias, setOcorrencias] = useState<Ocorrencia[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  const carregar = useCallback(() => {
    setCarregando(true);
    setErro(null);
    Promise.all([listarAmbientes(), obterPrevisao(id), listarOcorrencias(id)])
      .then(([salas, prev, lista]) => {
        const encontrada = salas.find((item) => item.id === id);
        if (!encontrada) throw new Error('Sala não encontrada.');
        setSala(encontrada);
        setPrevisao(prev);
        setOcorrencias(lista);
      })
      .catch((falha: unknown) => {
        setSala(null);
        setPrevisao(null);
        setOcorrencias([]);
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
    <ScrollView contentContainerStyle={ui.pagina}>
      <Stack.Screen options={{ title: sala?.nome ?? 'Sala' }} />
      {carregando ? (
        <View style={ui.centro}>
          <ActivityIndicator color={cores.turquesa} />
          <Text style={ui.texto}>Carregando sala</Text>
        </View>
      ) : erro || !sala || !agora ? (
        <View style={ui.centro}>
          <Text accessibilityRole="alert" style={ui.erro}>{erro ?? 'Sala não encontrada.'}</Text>
          <Pressable style={ui.botao} onPress={carregar} accessibilityRole="button">
            <Text style={ui.botaoTexto}>Tentar de novo</Text>
          </Pressable>
        </View>
      ) : (
        <View style={styles.bloco}>
          <View style={styles.abertura}>
            <Text style={ui.legenda}>VISÃO DA SALA</Text>
            <Text accessibilityRole="header" style={ui.titulo}>{sala.nome}</Text>
            <Text style={ui.texto}>{sala.andar}º andar · face {sala.faceSol}</Text>
            {previsao?.demonstracao ? <Text style={styles.etiqueta}>DEMONSTRAÇÃO</Text> : null}
          </View>

          {previsao?.semLeiturasRecentes ? (
            <View style={ui.cartao}>
              <Text style={ui.aviso}>Histórico insuficiente para ajustar a sala</Text>
              <Text accessibilityRole="header" style={ui.secao}>Previsão da rua</Text>
              <RotuloNivel nivel={previsao.nivelPrevisto} />
              <Text style={styles.indice}>{indiceEmGraus(previsao.indiceRuaPrevisto)}</Text>
              <Text style={ui.texto}>{horaSaoPaulo(previsao.previstoPara)}</Text>
            </View>
          ) : (
            <View style={styles.bloco}>
              <View style={[ui.cartao, styles.termico, { borderLeftColor: sala.semLeitura ? cores.borda : agora.cor }]}>
              <Text style={ui.legenda}>CONDIÇÃO ATUAL</Text>
              <Text accessibilityRole="header" style={ui.secao}>Agora</Text>
              {sala.semLeitura ? (
                <Text style={ui.aviso}>Sem leitura: o sensor desta sala ainda não enviou dados.</Text>
              ) : (
                <>
                  <RotuloNivel nivel={sala.nivelAtual} />
                  <Text style={styles.indice}>{indiceEmGraus(sala.indiceAtual)}</Text>
                  <Text style={styles.unidade}>ÍNDICE DE CALOR</Text>
                  <Text style={ui.texto}>Última leitura {horaSaoPaulo(sala.ultimaLeituraEm)}</Text>
                  <Text style={styles.acao}>Ação: {agora.acao}</Text>
                </>
              )}
              </View>
              <View style={ui.cartao}>
              <Text style={ui.legenda}>PREVISÃO</Text>
              <Text accessibilityRole="header" style={ui.secao}>Previsão da sala</Text>
              {previsao === null ? (
                <Text style={ui.aviso}>{SEM_DADOS_PARA_PREVER}</Text>
              ) : (
                <>
                  {previsao.ruaDisponivel ? null : <Text style={ui.aviso}>{RUA_INDISPONIVEL}</Text>}
                  <RotuloNivel nivel={previsao.nivelPrevisto} />
                  <Text style={styles.indice}>
                    {previsao.indiceSalaPrevisto === null ? 'Sem índice da sala' : indiceEmGraus(previsao.indiceSalaPrevisto)}
                  </Text>
                  {/* Sem a rua, indiceRuaPrevisto vem 0 e não pode ir para a tela. */}
                  {previsao.ruaDisponivel ? (
                    <Text style={ui.texto}>Rua {indiceEmGraus(previsao.indiceRuaPrevisto)}</Text>
                  ) : null}
                  <Text style={ui.texto}>{horaSaoPaulo(previsao.previstoPara)}</Text>
                  <Text style={styles.acao}>Ação: {previsto?.acao}</Text>
                </>
              )}
              </View>
            </View>
          )}

          <MapaTermico id={sala.id} />
          <View style={ui.cartao}>
          <Text accessibilityRole="header" style={ui.secao}>Ocorrências</Text>
          {ocorrencias.length === 0 ? (
            <Text style={ui.texto}>Nenhuma ocorrência nesta sala.</Text>
          ) : (
            ocorrencias.map((item) => (
              <View key={item.id} style={styles.ocorrencia}>
                <Text style={styles.descricao}>{item.descricao}</Text>
                <Text style={ui.texto}>{horaSaoPaulo(item.criadaEm)}</Text>
              </View>
            ))
          )}

          <Pressable
            style={ui.botao}
            onPress={() =>
              router.push({
                pathname: '/salas/[id]/ocorrencia',
                params: { id: sala.id } satisfies RotaParams['/salas/[id]/ocorrencia'],
              })
            }
            accessibilityRole="button"
          >
            <Text style={ui.botaoTexto}>Registrar ocorrência</Text>
          </Pressable>
          </View>
        </View>
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  bloco: { gap: 20 },
  abertura: { gap: 8 },
  termico: { borderLeftWidth: 5 },
  indice: { fontSize: 40, fontWeight: '800', color: cores.texto, letterSpacing: -1 },
  unidade: { fontSize: 11, fontWeight: '700', color: cores.secundario, letterSpacing: 1 },
  acao: { color: cores.texto, backgroundColor: cores.fundo, padding: 14, borderRadius: 10, fontSize: 16, lineHeight: 25, fontWeight: '600' },
  ocorrencia: { borderTopWidth: 1, borderTopColor: cores.borda, paddingTop: 12, gap: 5 },
  descricao: { color: cores.texto, fontSize: 16, lineHeight: 24 },
  etiqueta: {
    alignSelf: 'flex-start',
    backgroundColor: cores.turquesaClaro,
    color: cores.turquesa,
    fontWeight: '800',
    letterSpacing: 0.5,
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
    overflow: 'hidden',
  },
});
