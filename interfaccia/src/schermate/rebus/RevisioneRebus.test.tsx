import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { EsitoRevisione } from '../../comune/EsitoRevisione';
import { ContestoRegistro, creaRegistro } from '../../comune/registroSalvataggio';
import { candidato, chiamata, rebus, servizioFinto } from '../griglie/fixture';
import { RevisioneRebus } from './RevisioneRebus';

afterEach(cleanup);

describe('RevisioneRebus', () => {
  it('non ha elementi img né campi file né sfondi a immagine', () => {
    const { container } = render(<RevisioneRebus candidato={candidato(rebus())} servizi={servizioFinto()} />);
    expect(container.querySelectorAll('img, picture, svg image, input[type="file"], canvas')).toHaveLength(0);
  });

  it('mostra le vignette descritte a parole', () => {
    render(<RevisioneRebus candidato={candidato(rebus())} servizi={servizioFinto()} />);
    expect((screen.getByLabelText('Descrizione') as HTMLTextAreaElement).value).toContain('disco chiaro');
    expect((screen.getByLabelText('Parola letta') as HTMLInputElement).value).toBe('luna');
  });

  it("salva ogni modifica di un campo e mostra i controlli", async () => {
    const servizi = servizioFinto(['Il diagramma non corrisponde alla soluzione.']);
    render(<RevisioneRebus candidato={candidato(rebus())} servizi={servizi} />);
    const utente = userEvent.setup();

    const diagramma = screen.getByLabelText('Diagramma');
    await utente.clear(diagramma);
    await utente.type(diagramma, '3 4');

    await waitFor(() => expect(servizi.chiamate.at(-1)?.dati.diagramma).toBe('3 4'));
    expect(await screen.findByText('Il diagramma non corrisponde alla soluzione.')).toBeTruthy();
  });

  it('le lettere vuote diventano null e la spiegazione vuota resta null', async () => {
    const servizi = servizioFinto();
    render(<RevisioneRebus candidato={candidato(rebus())} servizi={servizi} />);
    const utente = userEvent.setup();

    await utente.clear(screen.getByLabelText('Lettere'));

    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    const vignette = chiamata(servizi, 0).dati.vignette as { lettere: string | null }[];
    expect(vignette[0]?.lettere).toBeNull();
    expect(chiamata(servizi, 0).dati.spiegazione).toBeNull();
  });

  it('aggiunge e rimuove una figura', async () => {
    const servizi = servizioFinto();
    render(<RevisioneRebus candidato={candidato(rebus())} servizi={servizi} />);
    const utente = userEvent.setup();

    await utente.click(screen.getByRole('button', { name: 'Aggiungi figura' }));
    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    expect((chiamata(servizi, 0).dati.vignette as unknown[]).length).toBe(2);

    await utente.click(screen.getByRole('button', { name: 'Rimuovi figura 1' }));
    await waitFor(() => expect(servizi.chiamate).toHaveLength(2));
    expect((chiamata(servizi, 1).dati.vignette as unknown[]).length).toBe(1);
  });

  it("modifica un campo e Accetta subito: aspetta il salvataggio e manda l'ultimo contenuto", async () => {
    const c = candidato(rebus());
    let finito = false;
    const salvati: string[] = [];
    const servizi = {
      salvaFinale: async (_id: string, contenuto: { dati: Record<string, unknown> }) => {
        await new Promise((r) => setTimeout(r, 30));
        salvati.push(String(contenuto.dati.soluzione));
        finito = true;
        return c;
      },
    };
    const accetta = vi.fn((...[, misura]: [string, { contenutoFinale?: { dati: Record<string, unknown> } }]) => {
      expect(misura.contenutoFinale?.dati.soluzione).toBe('La luna piena');
      expect(finito).toBe(true);
      return Promise.resolve({} as never);
    });
    const cronometro = { concludi: () => ({ tempoAttivoMs: 1, ultimaAttivitaIl: '2026-12-01T10:00:00.000Z' }), ferma: vi.fn() };
    render(
      <ContestoRegistro.Provider value={creaRegistro()}>
        <RevisioneRebus candidato={c} servizi={servizi}>
          <EsitoRevisione candidato={c} servizi={{ accetta, scarta: vi.fn() }} cronometro={cronometro} onConcluso={() => undefined} />
        </RevisioneRebus>
      </ContestoRegistro.Provider>,
    );
    const utente = userEvent.setup();

    const soluzione = screen.getByLabelText('Soluzione');
    await utente.clear(soluzione);
    await utente.type(soluzione, 'La luna piena');
    const pulsante = screen.getByRole('button', { name: 'Accetta' }) as HTMLButtonElement;
    expect(pulsante.disabled).toBe(true);
    await waitFor(() => expect(pulsante.disabled).toBe(false));
    await utente.click(pulsante);

    await waitFor(() => expect(accetta).toHaveBeenCalledTimes(1));
    expect(salvati.at(-1)).toBe('La luna piena');
    expect(accetta.mock.calls[0]?.[1].contenutoFinale?.dati.soluzione).toBe('La luna piena');
  });
});
