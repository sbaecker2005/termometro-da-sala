import assert from 'node:assert/strict';
import { registerHooks } from 'node:module';
import { it } from 'node:test';
import { mapaDemonstracao } from './mapa.ts';

it('consulta o mapa atual e +1 h na API, preservando validação e erros HTTP', async t => {
  const hook = registerHooks({
    resolve(specifier, context, nextResolve) {
      if (specifier === 'expo-constants') return { shortCircuit: true,
        url: 'data:text/javascript,export default {expoConfig:{extra:{apiMock:"0",apiUrl:"http://servidor"}}}' };
      return nextResolve(specifier.startsWith('../domain/') && !specifier.endsWith('.ts')
        ? `${specifier}.ts` : specifier, context);
    },
  });
  let obterMapa;
  try { ({ obterMapa } = await import('../services/api.ts')); }
  finally { hook.deregister(); }
  const urls: string[] = [];
  const mapa = mapaDemonstracao('sala/05', false);
  const fetch = t.mock.method(globalThis, 'fetch', async (url: string) => {
    urls.push(url);
    return new Response(JSON.stringify(mapa));
  });
  assert.deepEqual(await obterMapa('sala/05', false), mapa);
  assert.deepEqual(await obterMapa('sala/05', true), mapa);
  assert.deepEqual(urls, ['http://servidor/ambientes/sala%2F05/mapa', 'http://servidor/ambientes/sala%2F05/mapa?futuro=true']);
  fetch.mock.mockImplementation(async () => new Response('{}'));
  await assert.rejects(obterMapa('sala/05', true), /Mapa inválido/);
  fetch.mock.mockImplementation(async () => new Response('', { status: 503 }));
  await assert.rejects(obterMapa('sala/05', true), /carregar o mapa \(503\)/);
});
