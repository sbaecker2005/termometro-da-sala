export type Professor = { nome: string; escola: string };

export function validarProfessor(nome: string, escola: string): string | null {
  if (!nome.trim() || !escola.trim()) return 'Informe o nome e a escola.';
  return null;
}
