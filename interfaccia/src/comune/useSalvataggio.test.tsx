import { act, renderHook } from '@testing-library/react';
import type { ReactNode } from 'react';
import { describe, expect, it, vi } from 'vitest';
import { ErroreApi } from './api';
import { conDati, useSalvataggio } from './useSalvataggio';
import { ContestoRegistro, creaRegistro } from './registroSalvataggio';
import { candidatoProva } from '../prova/apiFinta';

function differita<T>() {
  let risolvi!: (v: T) => void;
  let rifiuta!: (e: unknown) => void;
  const promessa = new Promise<T>((s, r) => {
    risolvi = s;
    rifiuta = r;
  });
  return { promessa, risolvi, rifiuta };
}

describe('useSalvataggio', () => {
  it('attendiCoda aspetta i salvataggi in corso', async () => {
    const d = differita<ReturnType<typeof candidatoProva>>();
    const servizi = { salvaFinale: vi.fn(() => d.promessa) };
    const { result } = renderHook(() => useSalvataggio(candidatoProva(), servizi));

    act(() => result.current.modifica((c) => conDati(c, { esposto: 'NUOVO' })));
    let finita = false;
    const attesa = result.current.attendiCoda().then(() => (finita = true));
    await Promise.resolve();
    expect(finita).toBe(false);

    await act(async () => {
      d.risolvi(candidatoProva());
      await attesa;
    });
    expect(finita).toBe(true);
    expect(result.current.stato).toBe('fermo');
  });

  it("la maniglia espone il contenuto mostrato e l'errore dell'ultimo salvataggio", async () => {
    const registro = creaRegistro();
    const wrapper = ({ children }: { children: ReactNode }) => <ContestoRegistro.Provider value={registro}>{children}</ContestoRegistro.Provider>;
    const servizi = { salvaFinale: vi.fn(() => Promise.reject(new ErroreApi('no', 422, ['Soluzione non valida.']))) };
    const { result } = renderHook(() => useSalvataggio(candidatoProva(), servizi), { wrapper });

    act(() => result.current.modifica((c) => conDati(c, { esposto: 'MODIFICATO' })));
    await act(async () => {
      await registro.maniglia()!.attendiCoda();
    });

    expect(registro.maniglia()!.contenuto().dati['esposto']).toBe('MODIFICATO');
    expect(registro.maniglia()!.errore()).toBe('Soluzione non valida.');
    expect(registro.istantanea()).toEqual({ stato: 'errore', errore: 'Soluzione non valida.' });
    expect(result.current.stato).toBe('errore');
  });

  it("un salvataggio riuscito dopo un errore azzera l'errore", async () => {
    const registro = creaRegistro();
    const wrapper = ({ children }: { children: ReactNode }) => <ContestoRegistro.Provider value={registro}>{children}</ContestoRegistro.Provider>;
    const salvaFinale = vi.fn().mockRejectedValueOnce(new ErroreApi('no', 422, ['x'])).mockResolvedValue(candidatoProva());
    const { result } = renderHook(() => useSalvataggio(candidatoProva(), { salvaFinale }), { wrapper });

    act(() => result.current.modifica((c) => conDati(c, { esposto: 'A' })));
    await act(async () => registro.maniglia()!.attendiCoda());
    act(() => result.current.modifica((c) => conDati(c, { esposto: 'AB' })));
    await act(async () => registro.maniglia()!.attendiCoda());

    expect(registro.maniglia()!.errore()).toBeNull();
    expect(registro.istantanea().stato).toBe('fermo');
  });
});
