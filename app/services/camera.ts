export async function retomarCamera(
  consultarPermissao: () => Promise<{ granted: boolean; canAskAgain: boolean }>,
  fotografar: () => Promise<void>,
  abrirAjustes: () => Promise<void>,
): Promise<void> {
  const permissao = await consultarPermissao();
  if (!permissao.granted && !permissao.canAskAgain) {
    await abrirAjustes();
    return;
  }
  await fotografar();
}
