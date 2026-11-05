import { Stack, router, useLocalSearchParams } from 'expo-router';
import * as ImagePicker from 'expo-image-picker';
import { useState } from 'react';
import { Image, Linking, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';
import { mensagemErroOcorrencia, validarOcorrencia } from '../../../domain/ocorrencia';
import type { RotaParams } from '../../../rotas';
import { registrarOcorrencia, type FotoOcorrencia } from '../../../services/api';

function idParam(valor: string | string[] | undefined): string {
  return typeof valor === 'string' ? valor : (valor?.[0] ?? '');
}

export default function NovaOcorrencia() {
  const params = useLocalSearchParams<RotaParams['/salas/[id]/ocorrencia']>();
  const ambienteId = idParam(params.id);
  const [descricao, setDescricao] = useState('');
  const [foto, setFoto] = useState<FotoOcorrencia | null>(null);
  const [erros, setErros] = useState<string[]>([]);
  const [avisoCamera, setAvisoCamera] = useState<string | null>(null);
  const [abrirAjustes, setAbrirAjustes] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [protocolo, setProtocolo] = useState<string | null>(null);

  async function fotografar() {
    setAvisoCamera(null);
    const permissao = await ImagePicker.requestCameraPermissionsAsync();
    if (!permissao.granted) {
      setAbrirAjustes(!permissao.canAskAgain);
      setAvisoCamera(
        permissao.canAskAgain
          ? 'A câmera está sem permissão. Toque em tentar de novo para autorizar.'
          : 'A câmera está sem permissão. Libere a câmera nos ajustes e toque em tentar de novo.',
      );
      return;
    }
    try {
      const resultado = await ImagePicker.launchCameraAsync({ mediaTypes: ['images'], quality: 0.7, exif: false });
      if (resultado.canceled || !resultado.assets[0]) return;
      setFoto({ uri: resultado.assets[0].uri, nome: 'foto.jpg', tipo: 'image/jpeg' });
      setAbrirAjustes(false);
    } catch {
      setAvisoCamera('Não foi possível abrir a câmera. Tente de novo neste aparelho.');
    }
  }

  async function tentarCamera() {
    if (abrirAjustes) {
      await Linking.openSettings();
      return;
    }
    await fotografar();
  }

  async function enviar() {
    const falhas = validarOcorrencia({ descricao, temFoto: foto !== null }).map(mensagemErroOcorrencia);
    setErros(falhas);
    if (falhas.length > 0 || !foto) return;
    setEnviando(true);
    try {
      const criada = await registrarOcorrencia({ ambienteId, descricao, foto });
      setProtocolo(criada.id);
    } catch (falha: unknown) {
      setErros([falha instanceof Error ? falha.message : 'Não foi possível registrar a ocorrência.']);
    } finally {
      setEnviando(false);
    }
  }

  if (protocolo) {
    return (
      <View style={styles.conteudo}>
        <Stack.Screen options={{ title: 'Nova ocorrência' }} />
        <Text style={styles.titulo}>Ocorrência registrada</Text>
        <Text style={styles.texto}>Protocolo {protocolo}</Text>
        <Pressable style={styles.botao} onPress={() => router.back()} accessibilityRole="button">
          <Text style={styles.botaoTexto}>Voltar à sala</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <ScrollView contentContainerStyle={styles.conteudo} keyboardShouldPersistTaps="handled">
      <Stack.Screen options={{ title: 'Nova ocorrência' }} />
      <Text style={styles.aviso}>Fotografe a sala ou o equipamento, sem pessoas.</Text>
      <Text style={styles.rotulo}>O que aconteceu</Text>
      <TextInput
        style={styles.campo}
        value={descricao}
        onChangeText={setDescricao}
        multiline
        placeholder="Descreva o que aconteceu, sem identificar aluno."
        textAlignVertical="top"
      />
      <Text style={styles.contador}>{descricao.trim().length}/500</Text>
      <Pressable style={styles.botaoSecundario} onPress={() => void fotografar()} accessibilityRole="button">
        <Text style={styles.botaoSecundarioTexto}>{foto ? 'Tirar outra foto' : 'Fotografar com a câmera'}</Text>
      </Pressable>
      {foto ? <Image source={{ uri: foto.uri }} style={styles.foto} accessibilityLabel="Foto da sala ou do equipamento" /> : null}
      {avisoCamera ? (
        <View style={styles.bloco}>
          <Text style={styles.erro}>{avisoCamera}</Text>
          <Pressable style={styles.botaoSecundario} onPress={() => void tentarCamera()} accessibilityRole="button">
            <Text style={styles.botaoSecundarioTexto}>Tentar de novo</Text>
          </Pressable>
        </View>
      ) : null}
      {erros.map((erro) => (
        <Text key={erro} style={styles.erro}>
          {erro}
        </Text>
      ))}
      <Pressable
        style={[styles.botao, enviando ? styles.botaoOff : null]}
        disabled={enviando}
        onPress={() => void enviar()}
        accessibilityRole="button"
      >
        <Text style={styles.botaoTexto}>{enviando ? 'Enviando' : 'Registrar'}</Text>
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  conteudo: { padding: 20, gap: 8 },
  bloco: { gap: 8 },
  titulo: { fontSize: 22, fontWeight: '700', color: '#14532D' },
  texto: { color: '#3F3F46' },
  aviso: { backgroundColor: '#FEF3C7', color: '#92400E', padding: 12, borderRadius: 8, fontWeight: '600' },
  rotulo: { fontWeight: '700', marginTop: 8 },
  campo: {
    minHeight: 120,
    borderWidth: 1,
    borderColor: '#D4D4D8',
    borderRadius: 8,
    padding: 12,
    backgroundColor: '#fff',
  },
  contador: { color: '#71717A', alignSelf: 'flex-end' },
  foto: { width: '100%', height: 220, borderRadius: 8, backgroundColor: '#E4E4E7' },
  erro: { color: '#B91C1C' },
  botao: { backgroundColor: '#14532D', borderRadius: 8, padding: 14, marginTop: 8, alignItems: 'center' },
  botaoOff: { opacity: 0.6 },
  botaoTexto: { color: '#fff', fontWeight: '700' },
  botaoSecundario: { borderWidth: 1, borderColor: '#14532D', borderRadius: 8, padding: 12, alignItems: 'center' },
  botaoSecundarioTexto: { color: '#14532D', fontWeight: '700' },
});
