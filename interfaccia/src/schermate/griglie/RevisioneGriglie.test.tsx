import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { EsitoRevisione } from '../../comune/EsitoRevisione';
import { ContestoRegistro, creaRegistro } from '../../comune/registroSalvataggio';
import type { Candidato } from '../../comune/tipi';
import { candidato, chiamata, futoshiki, nonogramma, servizioCheRifiuta, servizioFinto, sudoku } from './fixture';
import { RevisioneGriglie } from './RevisioneGriglie';
import type { Griglia, Segno } from './griglia';

afterEach(cleanup);

/** La schermata di revisione come la monta l'host: registro dei salvataggi più Accetta e Scarta. */
function conEsito(c: Candidato, servizi: Parameters<typeof RevisioneGriglie>[0]['servizi']) {
  const accetta = vi.fn(() => Promise.resolve({} as never));
  const cronometro = { concludi: () => ({ tempoAttivoMs: 1, ultimaAttivitaIl: '2026-12-01T10:00:00.000Z' }), ferma: vi.fn() };
  render(
    <ContestoRegistro.Provider value={creaRegistro()}>
      <RevisioneGriglie candidato={c} servizi={servizi}>
        <EsitoRevisione candidato={c} servizi={{ accetta, scarta: vi.fn() }} cronometro={cronometro} onConcluso={() => undefined} />
      </RevisioneGriglie>
    </ContestoRegistro.Provider>,
  );
  return accetta;
}

const celleDi = (c: { dati: Record<string, unknown> }, nome: string) => (c.dati[nome] as Griglia).celle;

describe('RevisioneGriglie, sudoku', () => {
  it('si modifica da tastiera e salva il contenuto nella forma del descrittore', async () => {
    const servizi = servizioFinto();
    render(<RevisioneGriglie candidato={candidato(sudoku())} servizi={servizi} />);
    const utente = userEvent.setup();

    screen.getByRole('button', { name: /^Schema, riga 1, colonna 3/ }).focus();
    await utente.keyboard('9');

    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    const salvato = chiamata(servizi, 0);
    expect(celleDi(salvato, 'griglia')[0]).toBe('539.7....');
    expect(salvato.dati.righe).toBe(9);
    expect((salvato.dati.griglia as Griglia).righe).toBe(9);
  });

  it('con le frecce si sposta e con Backspace svuota la casella', async () => {
    const servizi = servizioFinto();
    render(<RevisioneGriglie candidato={candidato(sudoku())} servizi={servizi} />);
    const utente = userEvent.setup();

    screen.getByRole('button', { name: /^Schema, riga 1, colonna 1/ }).focus();
    await utente.keyboard('{ArrowRight}{Backspace}');

    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    expect(celleDi(chiamata(servizi, 0), 'griglia')[0]).toBe('5...7....');
  });

  it('ignora i caratteri non ammessi', async () => {
    const servizi = servizioFinto();
    render(<RevisioneGriglie candidato={candidato(sudoku())} servizi={servizi} />);
    const utente = userEvent.setup();

    screen.getByRole('button', { name: /^Schema, riga 1, colonna 3/ }).focus();
    await utente.keyboard('x0');

    expect(servizi.chiamate).toHaveLength(0);
  });

  it("manda i salvataggi in ordine e mostra i controlli dell'ultima risposta", async () => {
    const servizi = servizioFinto(['Una nota di prova per chi rivede.']);
    render(<RevisioneGriglie candidato={candidato(sudoku())} servizi={servizi} />);
    const utente = userEvent.setup();

    screen.getByRole('button', { name: /^Schema, riga 1, colonna 3/ }).focus();
    await utente.keyboard('9{ArrowRight}8');

    await waitFor(() => expect(servizi.chiamate).toHaveLength(2));
    expect(celleDi(chiamata(servizi, 1), 'griglia')[0]).toBe('53987....');
    expect(await screen.findByText('Una nota di prova per chi rivede.')).toBeTruthy();
  });

  it("un indizio tolto che lascia due soluzioni: il 422 dell'host si vede e Accetta resta disabilitato", async () => {
    const servizi = servizioCheRifiuta('Il sudoku ha più di una soluzione.');
    const accetta = conEsito(candidato(sudoku()), servizi);
    const utente = userEvent.setup();

    screen.getByRole('button', { name: /^Schema, riga 1, colonna 1/ }).focus();
    await utente.keyboard('{Backspace}');

    expect((await screen.findByRole('alert')).textContent).toContain('Il sudoku ha più di una soluzione.');
    expect((screen.getByRole('button', { name: 'Accetta' }) as HTMLButtonElement).disabled).toBe(true);
    expect(accetta).not.toHaveBeenCalled();
  });
});

describe('RevisioneGriglie, futoshiki', () => {
  it("un segno cambiato che rompe l'unicità mostra il motivo dell'host e blocca Accetta", async () => {
    const servizi = servizioCheRifiuta('Il gioco ha più di una soluzione.');
    conEsito(candidato(futoshiki()), servizi);
    const utente = userEvent.setup();

    await utente.click(screen.getByRole('button', { name: /^Segno tra \(1,1\) e \(1,2\): prima minore/ }));

    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    expect(chiamata(servizi, 0).dati.segni as Segno[]).toEqual([]);
    expect((await screen.findByRole('alert')).textContent).toContain('Il gioco ha più di una soluzione.');
    expect((screen.getByRole('button', { name: 'Accetta' }) as HTMLButtonElement).disabled).toBe(true);
  });

  it("un segno nuovo entra nell'elenco", async () => {
    const servizi = servizioFinto();
    render(<RevisioneGriglie candidato={candidato(futoshiki())} servizi={servizi} />);
    const utente = userEvent.setup();

    await utente.click(screen.getByRole('button', { name: /^Segno tra \(2,3\) e \(3,3\): nessun segno/ }));

    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    const segni = chiamata(servizi, 0).dati.segni as Segno[];
    expect(segni).toHaveLength(2);
    expect(segni[1]).toEqual({ riga: 2, colonna: 3, verso: 'sotto', segno: 'maggiore' });
  });
});

describe('RevisioneGriglie, nonogramma', () => {
  it('alterna la casella e ricalcola gli indizi nel formato del descrittore', async () => {
    const servizi = servizioFinto();
    render(<RevisioneGriglie candidato={candidato(nonogramma())} servizi={servizi} />);
    const utente = userEvent.setup();

    await utente.click(screen.getByRole('button', { name: /^Disegno, riga 5, colonna 1/ }));

    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    const dati = chiamata(servizi, 0).dati;
    expect(celleDi(chiamata(servizi, 0), 'soluzione_griglia')[4]).toBe('#....');
    expect(dati.indizi_righe).toEqual([
      { indice: 1, blocchi: '3' }, { indice: 2, blocchi: '1 1' }, { indice: 3, blocchi: '5' },
      { indice: 4, blocchi: '1 1' }, { indice: 5, blocchi: '1' },
    ]);
    expect((dati.indizi_colonne as { blocchi: string }[])[0]?.blocchi).toBe('4');
  });

  it('con la barra spaziatrice alterna la casella con il fuoco', async () => {
    const servizi = servizioFinto();
    render(<RevisioneGriglie candidato={candidato(nonogramma())} servizi={servizi} />);
    const utente = userEvent.setup();

    screen.getByRole('button', { name: /^Disegno, riga 1, colonna 1/ }).focus();
    await utente.keyboard(' ');

    await waitFor(() => expect(servizi.chiamate).toHaveLength(1));
    expect(celleDi(chiamata(servizi, 0), 'soluzione_griglia')[0]).toBe('####.');
  });
});
