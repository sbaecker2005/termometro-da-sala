import AsyncStorage from '@react-native-async-storage/async-storage';
import type { Professor } from '../domain/professor';

export type { Professor };

const CHAVE = 'professor';

export async function lerProfessor(): Promise<Professor | null> {
  const bruto = await AsyncStorage.getItem(CHAVE);
  if (!bruto) return null;
  const dados: unknown = JSON.parse(bruto);
  if (!dados || typeof dados !== 'object') return null;
  const nome = 'nome' in dados && typeof dados.nome === 'string' ? dados.nome : '';
  const escola = 'escola' in dados && typeof dados.escola === 'string' ? dados.escola : '';
  if (!nome || !escola) return null;
  return { nome, escola };
}

export async function gravarProfessor(professor: Professor): Promise<void> {
  await AsyncStorage.setItem(CHAVE, JSON.stringify(professor));
}
