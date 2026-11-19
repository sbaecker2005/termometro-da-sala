import { StyleSheet } from 'react-native';

export const cores = {
  marinho: '#102D40',
  turquesa: '#007077',
  turquesaClaro: '#E1F4F2',
  fundo: '#F2F6F8',
  superficie: '#FFFFFF',
  texto: '#163447',
  secundario: '#526A78',
  borda: '#D9E4EA',
  erro: '#B91C1C',
  aviso: '#FFF3D7',
  avisoTexto: '#78500D',
};

export const ui = StyleSheet.create({
  pagina: { width: '100%', maxWidth: 760, alignSelf: 'center', padding: 20, paddingBottom: 36, gap: 20, flexGrow: 1 },
  cartao: { backgroundColor: cores.superficie, borderWidth: 1, borderColor: cores.borda, borderRadius: 18, padding: 20, gap: 12 },
  titulo: { color: cores.texto, fontSize: 28, fontWeight: '800', letterSpacing: -0.7 },
  secao: { color: cores.texto, fontSize: 20, fontWeight: '700', letterSpacing: -0.3 },
  legenda: { color: cores.turquesa, fontSize: 12, fontWeight: '800', letterSpacing: 1.5 },
  texto: { color: cores.secundario, fontSize: 15, lineHeight: 23 },
  rotulo: { color: cores.texto, fontSize: 15, fontWeight: '700' },
  campo: { minHeight: 52, borderWidth: 1, borderColor: cores.borda, borderRadius: 12, padding: 14, backgroundColor: cores.fundo, color: cores.texto, fontSize: 16 },
  botao: { minHeight: 52, backgroundColor: cores.turquesa, borderRadius: 12, paddingHorizontal: 20, paddingVertical: 15, alignItems: 'center', justifyContent: 'center' },
  botaoTexto: { color: '#fff', fontWeight: '700', fontSize: 16, textAlign: 'center' },
  secundario: { minHeight: 52, borderWidth: 1, borderColor: cores.turquesa, backgroundColor: cores.turquesaClaro, borderRadius: 12, padding: 14, alignItems: 'center', justifyContent: 'center' },
  secundarioTexto: { color: cores.turquesa, fontWeight: '700', fontSize: 15, textAlign: 'center' },
  aviso: { backgroundColor: cores.aviso, color: cores.avisoTexto, padding: 14, borderRadius: 12, fontSize: 15, lineHeight: 23 },
  erro: { color: cores.erro, fontSize: 15, lineHeight: 23 },
  centro: { alignItems: 'center', justifyContent: 'center', paddingVertical: 36, gap: 16 },
});
