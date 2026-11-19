import { Stack, router, useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';
import { validarProfessor } from '../domain/professor';
import type { RotaParams } from '../rotas';
import { gravarProfessor, lerProfessor } from '../services/professor';
import { cores, ui } from '../components/tema';

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
    try {
      await gravarProfessor({ nome: nome.trim(), escola: escola.trim() });
    } catch {
      setErro('Não foi possível guardar os dados neste aparelho. Tente de novo.');
      return;
    }
    // Ao editar, a lista já está na pilha: voltar evita duas telas Salas empilhadas.
    if (editar === '1' && router.canGoBack()) router.back();
    else router.replace('/salas');
  }

  return (
    <ScrollView contentContainerStyle={ui.pagina} keyboardShouldPersistTaps="handled" automaticallyAdjustKeyboardInsets>
      <Stack.Screen options={{ title: 'Identificação' }} />
      <View style={styles.hero}>
        <View style={styles.marca} accessible={false}><Text style={styles.graus}>°</Text></View>
        <Text style={styles.legenda}>CONFORTO TÉRMICO NA ESCOLA</Text>
        <Text accessibilityRole="header" style={styles.titulo}>Termômetro{ '\n' }da Sala</Text>
        <Text style={styles.descricao}>Acompanhe o ambiente. Cuide de quem está nele.</Text>
      </View>
      {pronto ? (
        <View style={ui.cartao}>
          <Text accessibilityRole="header" style={ui.secao}>{editar === '1' ? 'Sua identificação' : 'Vamos começar'}</Text>
          <Text style={ui.texto}>Estes dados ficam neste aparelho e voltam quando o app abre de novo.</Text>
          <Text style={ui.rotulo}>Nome do professor</Text>
          <TextInput style={ui.campo} accessibilityLabel="Nome do professor" value={nome} onChangeText={setNome} autoCapitalize="words" placeholder="Seu nome" placeholderTextColor={cores.secundario} />
          <Text style={ui.rotulo}>Escola</Text>
          <TextInput style={ui.campo} accessibilityLabel="Escola" value={escola} onChangeText={setEscola} autoCapitalize="words" placeholder="Nome da escola" placeholderTextColor={cores.secundario} />
          {erro ? <Text accessibilityRole="alert" style={ui.erro}>{erro}</Text> : null}
          <Pressable style={({ pressed }) => [ui.botao, pressed && { opacity: 0.8 }]} onPress={() => void salvar()} accessibilityRole="button">
            <Text style={ui.botaoTexto}>Continuar</Text>
          </Pressable>
        </View>
      ) : (
        <ActivityIndicator color={cores.turquesa} accessibilityLabel="Carregando identificação" />
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  hero: { backgroundColor: cores.marinho, padding: 24, borderRadius: 22, gap: 16 },
  marca: { width: 48, height: 48, borderRadius: 15, backgroundColor: '#4DE0CD', alignItems: 'center', justifyContent: 'center' },
  graus: { fontSize: 42, lineHeight: 48, fontWeight: '700', color: cores.marinho },
  legenda: { color: '#7CE7DB', fontSize: 11, letterSpacing: 1.5, fontWeight: '800' },
  titulo: { fontSize: 34, fontWeight: '800', color: '#fff', letterSpacing: -1 },
  descricao: { color: '#D1E1EA', fontSize: 16, lineHeight: 25 },
});
