import { Stack, router, useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';
import { validarProfessor } from '../domain/professor';
import type { RotaParams } from '../rotas';
import { gravarProfessor, lerProfessor } from '../services/professor';

export default function Identificacao() {
  const { editar } = useLocalSearchParams<RotaParams['/']>();
  const [nome, setNome] = useState('');
  const [escola, setEscola] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [pronto, setPronto] = useState(false);

  useEffect(() => {
    let ativo = true;
    lerProfessor()
      .then((professor) => {
        if (!ativo) return;
        if (professor && editar !== '1') {
          router.replace('/salas');
          return;
        }
        if (professor) {
          setNome(professor.nome);
          setEscola(professor.escola);
        }
        setPronto(true);
      })
      .catch(() => {
        if (ativo) setPronto(true);
      });
    return () => {
      ativo = false;
    };
  }, [editar]);

  async function salvar() {
    const erro = validarProfessor(nome, escola);
    setErro(erro);
    if (erro) return;
    await gravarProfessor({ nome: nome.trim(), escola: escola.trim() });
    router.replace('/salas');
  }

  return (
    <ScrollView contentContainerStyle={styles.conteudo} keyboardShouldPersistTaps="handled">
      <Stack.Screen options={{ title: 'Identificação' }} />
      <Text style={styles.titulo}>Termômetro da Sala</Text>
      <Text style={styles.texto}>Estes dados ficam neste aparelho e voltam quando o app abre de novo.</Text>
      {pronto ? (
        <View>
          <Text style={styles.rotulo}>Nome do professor</Text>
          <TextInput style={styles.campo} value={nome} onChangeText={setNome} autoCapitalize="words" />
          <Text style={styles.rotulo}>Escola</Text>
          <TextInput style={styles.campo} value={escola} onChangeText={setEscola} autoCapitalize="words" />
          {erro ? <Text style={styles.erro}>{erro}</Text> : null}
          <Pressable style={styles.botao} onPress={() => void salvar()} accessibilityRole="button">
            <Text style={styles.botaoTexto}>Continuar</Text>
          </Pressable>
        </View>
      ) : (
        <ActivityIndicator color="#14532D" />
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  conteudo: { padding: 20, gap: 8 },
  titulo: { fontSize: 22, fontWeight: '700', color: '#14532D' },
  texto: { color: '#3F3F46', marginBottom: 12 },
  rotulo: { fontWeight: '600', marginTop: 8 },
  campo: { borderWidth: 1, borderColor: '#D4D4D8', borderRadius: 8, padding: 12, backgroundColor: '#fff', marginTop: 4 },
  erro: { color: '#B91C1C', marginTop: 8 },
  botao: { backgroundColor: '#14532D', borderRadius: 8, padding: 14, marginTop: 16, alignItems: 'center' },
  botaoTexto: { color: '#fff', fontWeight: '700' },
});
