import { Stack, router, useLocalSearchParams } from 'expo-router';
import * as ImagePicker from 'expo-image-picker';
import { useState } from 'react';
import { Image, Linking, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';
import { mensagemErroOcorrencia, validarOcorrencia } from '../../../domain/ocorrencia';
import type { RotaParams } from '../../../rotas';
import { registrarOcorrencia, type FotoOcorrencia } from '../../../services/api';
import { retomarCamera } from '../../../services/camera';
import { cores, ui } from '../../../components/tema';

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
  const [enviando, setEnviando] = useState(false);
  const [protocolo, setProtocolo] = useState<string | null>(null);

  async function fotografar() {
    setAvisoCamera(null);
    try {
      const permissao = await ImagePicker.requestCameraPermissionsAsync();
      if (!permissao.granted) {
        setAvisoCamera(
          permissao.canAskAgain
            ? 'A câmera está sem permissão. Toque em tentar de novo para autorizar.'
            : 'A câmera está sem permissão. Libere a câmera nos ajustes e toque em tentar de novo.',
        );
        return;
      }
      const resultado = await ImagePicker.launchCameraAsync({ mediaTypes: ['images'], quality: 0.7, exif: false });
      if (resultado.canceled || !resultado.assets[0]) return;
      setFoto({ uri: resultado.assets[0].uri, nome: 'foto.jpg', tipo: 'image/jpeg' });
    } catch {
      setAvisoCamera('Não foi possível abrir a câmera. Tente de novo neste aparelho.');
    }
  }

  async function tentarCamera() {
    try {
      await retomarCamera(ImagePicker.getCameraPermissionsAsync, fotografar, Linking.openSettings);
    } catch {
      setAvisoCamera('Não foi possível consultar a permissão ou abrir os ajustes. Tente de novo.');
    }
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
      <ScrollView contentContainerStyle={ui.pagina}>
        <Stack.Screen options={{ title: 'Nova ocorrência' }} />
        <View style={[ui.cartao, styles.sucesso]}>
        <Text style={ui.legenda}>REGISTRO CONCLUÍDO</Text>
        <Text accessibilityRole="header" style={ui.titulo}>Ocorrência registrada</Text>
        <Text style={ui.texto}>Protocolo</Text>
        <Text selectable style={styles.protocolo}>{protocolo}</Text>
        <Pressable style={ui.botao} onPress={() => router.back()} accessibilityRole="button">
          <Text style={ui.botaoTexto}>Voltar à sala</Text>
        </Pressable>
        </View>
      </ScrollView>
    );
  }

  return (
    <ScrollView contentContainerStyle={ui.pagina} keyboardShouldPersistTaps="handled" automaticallyAdjustKeyboardInsets>
      <Stack.Screen options={{ title: 'Nova ocorrência' }} />
      <View style={styles.bloco}>
        <Text style={ui.legenda}>CUIDADO COM O AMBIENTE</Text>
        <Text accessibilityRole="header" style={ui.titulo}>Registrar ocorrência</Text>
        <Text style={ui.texto}>Conte o que aconteceu e acrescente uma foto.</Text>
      </View>
      <View style={ui.cartao}>
      <Text accessibilityRole="header" style={ui.secao}>O que aconteceu</Text>
      <Text style={ui.texto}>Descreva a situação em 10 a 500 caracteres.</Text>
      <TextInput
        style={[ui.campo, styles.campo]}
        accessibilityLabel="Descrição da ocorrência"
        value={descricao}
        onChangeText={setDescricao}
        multiline
        placeholder="Descreva o que aconteceu, sem identificar aluno."
        placeholderTextColor={cores.secundario}
        textAlignVertical="top"
      />
      <Text style={styles.contador}>{descricao.trim().length}/500</Text>
      </View>
      <View style={ui.cartao}>
      <Text accessibilityRole="header" style={ui.secao}>Foto do ambiente</Text>
      <Text style={ui.aviso}>Fotografe a sala ou o equipamento, sem pessoas.</Text>
      <Pressable style={ui.secundario} onPress={() => void fotografar()} accessibilityRole="button">
        <Text style={ui.secundarioTexto}>{foto ? 'Tirar outra foto' : 'Fotografar com a câmera'}</Text>
      </Pressable>
      {foto ? <Image source={{ uri: foto.uri }} style={styles.foto} accessibilityLabel="Foto da sala ou do equipamento" /> : null}
      {avisoCamera ? (
        <View style={styles.bloco}>
          <Text accessibilityRole="alert" style={ui.erro}>{avisoCamera}</Text>
          <Pressable style={ui.secundario} onPress={() => void tentarCamera()} accessibilityRole="button">
            <Text style={ui.secundarioTexto}>Tentar de novo</Text>
          </Pressable>
        </View>
      ) : null}
      </View>
      {erros.map((erro) => (
        <Text key={erro} accessibilityRole="alert" style={ui.erro}>
          {erro}
        </Text>
      ))}
      <Pressable
        style={[ui.botao, enviando ? styles.botaoOff : null]}
        disabled={enviando}
        accessibilityState={{ disabled: enviando, busy: enviando }}
        onPress={() => void enviar()}
        accessibilityRole="button"
      >
        <Text style={ui.botaoTexto}>{enviando ? 'Enviando…' : 'Registrar ocorrência'}</Text>
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  bloco: { gap: 8 },
  campo: { minHeight: 150 },
  contador: { color: cores.secundario, alignSelf: 'flex-end', fontSize: 13 },
  foto: { width: '100%', height: 220, borderRadius: 12, backgroundColor: cores.fundo },
  botaoOff: { opacity: 0.6 },
  sucesso: { borderTopWidth: 5, borderTopColor: cores.turquesa },
  protocolo: { color: cores.texto, fontWeight: '700', fontSize: 18 },
});
