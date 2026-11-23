import { cleanup, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it } from 'vitest';
import { ErroreApi } from '../../comune/api';
import type { Api } from '../../comune/api';
import type { CampoBrief, EsitoGenerazione, Tipologia } from '../../comune/tipi';
import { apiFinta, candidatoProva, tipologiaProva } from '../../prova/apiFinta';
import { NuovoCandidatoGriglie } from './NuovoCandidatoGriglie';

afterEach(cleanup);

const difficolta: CampoBrief = { nome: 'difficolta', etichetta: 'Difficoltà', tipo: 'intero', obbligatorio: true, valori: [], minimo: 1, massimo: 5 };

function logica(id: string, nome: string, dimensioni: Partial<CampoBrief>, versione = 0): Tipologia {
  return tipologiaProva({
    id, nome, versione, famiglia: 'logico', modo: 'algoritmo', statoRegole: 'approvata', regoleApprovate: true, generazioneConsentita: true,
    campiBrief: [difficolta, { nome: 'dimensioni', etichetta: 'Dimensioni', tipo: 'dimensioni', obbligatorio: true, valori: [], minimo: null, massimo: null, ...dimensioni }],
  });
}

const TIPOLOGIE = [
  logica('sudoku', 'Sudoku', { tipo: 'dimensioni-quadrate', valori: ['9'] }, 3),
  logica('futoshiki', 'Futoshiki', { tipo: 'dimensioni-quadrate', valori: ['5', '6', '7'] }),
  logica('nonogramma', 'Nonogramma', { minimo: 10, massimo: 15 }),
];

function prepara(genera?: Api['genera']) {
  const brief: unknown[] = [];
  const api = apiFinta({
    tipologie: () => Promise.resolve(TIPOLOGIE),
    genera: (b) => {
      brief.push(b);
      return genera ? genera(b) : Promise.resolve({ candidati: [], valido: null, messaggio: null });
    },
  });
  render(<NuovoCandidatoGriglie servizi={api} />);
  return { brief };
}

describe('NuovoCandidatoGriglie', () => {
  it('non offre tema né vincoli e manda un brief con la versione della Tipologia', async () => {
    const { brief } = prepara();
    const utente = userEvent.setup();
    await screen.findByLabelText('Gioco logico');

    expect(screen.queryByLabelText(/tema/i)).toBeNull();
    expect(screen.queryByLabelText(/vincoli/i)).toBeNull();

    await utente.click(screen.getByRole('button', { name: 'Genera' }));
    await utente.selectOptions(screen.getByLabelText('Gioco logico'), 'futoshiki');
    await utente.selectOptions(screen.getByLabelText('Lato della griglia'), '6');
    await utente.selectOptions(screen.getByLabelText('Difficoltà'), '4');
    await utente.click(screen.getByRole('button', { name: 'Genera' }));

    await waitFor(() => expect(brief).toHaveLength(2));
    expect(brief[0]).toMatchObject({ tipologia: 'sudoku', versione_tipologia: 3, famiglia: 'logico', tema: null, vincoli: [], dimensioni: { righe: 9, colonne: 9 } });
    expect(brief[1]).toMatchObject({ tipologia: 'futoshiki', versione_tipologia: 0, difficolta: 4, dimensioni: { righe: 6, colonne: 6 } });
  });

  it('il nonogramma ha righe e colonne distinte', async () => {
    const { brief } = prepara();
    const utente = userEvent.setup();
    await screen.findByLabelText('Gioco logico');

    await utente.selectOptions(screen.getByLabelText('Gioco logico'), 'nonogramma');
    await utente.selectOptions(screen.getByLabelText('Righe'), '12');
    await utente.selectOptions(screen.getByLabelText('Colonne'), '15');
    await utente.click(screen.getByRole('button', { name: 'Genera' }));

    await waitFor(() => expect(brief).toHaveLength(1));
    expect(brief[0]).toMatchObject({ tipologia: 'nonogramma', dimensioni: { righe: 12, colonne: 15 } });
  });

  it('mostra i candidati non validi e il messaggio dopo 3 tentativi', async () => {
    const nonValido = (n: number) => candidatoProva({ id: `n${n}`, ordineGenerazione: n, stato: 'non_valido_schema', messaggio: `Schema non rispettato ${n}.` });
    const esito: EsitoGenerazione = { candidati: [nonValido(1), nonValido(2), nonValido(3)], valido: null, messaggio: 'Nessun candidato valido dopo 3 tentativi.' };
    prepara(() => Promise.resolve(esito));
    await userEvent.setup().click(await screen.findByRole('button', { name: 'Genera' }));

    expect(await screen.findByText('Nessun candidato valido dopo 3 tentativi.')).toBeTruthy();
    expect(screen.getByText(/Candidato 2 \(non valido per lo schema\): Schema non rispettato 2\./)).toBeTruthy();
  });

  it("mostra il motivo del 422 dell'host", async () => {
    prepara(() => Promise.reject(new ErroreApi('no', 422, ['Il brief non è valido: dimensioni fuori limite.'])));
    await userEvent.setup().click(await screen.findByRole('button', { name: 'Genera' }));
    expect(await screen.findByText('Il brief non è valido: dimensioni fuori limite.')).toBeTruthy();
  });

  it('disattiva Genera quando la generazione non è consentita', async () => {
    const bloccata = { ...logica('sudoku', 'Sudoku', { tipo: 'dimensioni-quadrate', valori: ['9'] }), generazioneConsentita: false };
    render(<NuovoCandidatoGriglie servizi={apiFinta({ tipologie: () => Promise.resolve([bloccata]) })} />);
    const pulsante = await screen.findByRole('button', { name: 'Genera' });
    expect((pulsante as HTMLButtonElement).disabled).toBe(true);
  });
});
