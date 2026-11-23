import { describe, expect, it, vi } from 'vitest';
import { creaApi } from './api';

function conFetch() {
  const fetchFinto = vi.fn<typeof fetch>(() => Promise.resolve(new Response('{}', { status: 200, headers: { 'Content-Type': 'application/json' } })));
  return { fetchFinto, api: creaApi(fetchFinto) };
}

describe('creaApi', () => {
  it("l'invio del tempo usa keepalive: non si perde alla chiusura o al ricaricamento della pagina", async () => {
    const { fetchFinto, api } = conFetch();

    await api.salvaTempo('c1', { tempoAttivoMs: 1000, ultimaAttivitaIl: '2026-12-01T10:00:00.000Z' });

    const [url, init] = fetchFinto.mock.calls[0] ?? [];
    expect(url).toBe('/api/prototipo/candidati/c1/tempo');
    expect(init?.method).toBe('PUT');
    expect(init?.keepalive).toBe(true);
  });

  it('le altre richieste non usano keepalive', async () => {
    const { fetchFinto, api } = conFetch();

    await api.genera({});

    expect(fetchFinto.mock.calls[0]?.[1]?.keepalive).toBeUndefined();
  });
});
