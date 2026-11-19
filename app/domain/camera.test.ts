import assert from 'node:assert/strict';
import { it } from 'node:test';
import { retomarCamera } from '../services/camera.ts';

it('reconsulta a permissão ao voltar dos ajustes e abre a câmera depois da autorização', async () => {
  let permissao = { granted: false, canAskAgain: false };
  const chamadas: string[] = [];
  const consultar = async () => { chamadas.push('consultar'); return permissao; };
  const fotografar = async () => { chamadas.push('camera'); };
  const ajustes = async () => { chamadas.push('ajustes'); };

  await retomarCamera(consultar, fotografar, ajustes);
  assert.deepEqual(chamadas, ['consultar', 'ajustes']);
  permissao = { granted: true, canAskAgain: false };
  await retomarCamera(consultar, fotografar, ajustes);
  assert.deepEqual(chamadas, ['consultar', 'ajustes', 'consultar', 'camera']);
});

it('segue para o pedido de permissão da câmera quando ainda pode perguntar', async () => {
  const chamadas: string[] = [];
  await retomarCamera(
    async () => ({ granted: false, canAskAgain: true }),
    async () => { chamadas.push('camera'); },
    async () => { chamadas.push('ajustes'); },
  );
  assert.deepEqual(chamadas, ['camera']);
});
