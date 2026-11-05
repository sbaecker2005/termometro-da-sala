import { StyleSheet, Text, View } from 'react-native';
import { apresentarNivel } from '../domain/nivel';

export function RotuloNivel({ nivel }: { nivel: string }) {
  const info = apresentarNivel(nivel);
  return (
    <View style={[styles.selo, { backgroundColor: info.cor }]} accessibilityRole="text">
      <Text style={styles.texto}>{info.nome}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  selo: { alignSelf: 'flex-start', borderRadius: 6, paddingHorizontal: 8, paddingVertical: 4 },
  texto: { color: '#fff', fontWeight: '700' },
});
