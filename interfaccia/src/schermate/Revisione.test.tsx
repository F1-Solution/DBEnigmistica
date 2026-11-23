import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { ErroreApi } from '../comune/api';
import { apiFinta, candidatoProva } from '../prova/apiFinta';
import { Revisione } from './Revisione';

describe('Revisione', () => {
  it('senza candidati propone di generarne uno', async () => {
    render(<Revisione tipologia="crittografia-breve" api={apiFinta()} />);

    expect(await screen.findByText(/Nessun candidato da rivedere/)).toBeInTheDocument();
  });

  it('mostra i campi, salva le modifiche e aggiorna i controlli', async () => {
    const api = apiFinta({
      prossimo: () => Promise.resolve(candidatoProva({ controlli: ['Controllo iniziale.'] })),
      salvaFinale: (_id, contenuto) => Promise.resolve(candidatoProva({ contenutoFinale: contenuto, controlli: ['Controllo aggiornato.'] })),
    });
    render(<Revisione tipologia="crittografia-breve" api={api} />);

    expect(await screen.findByText('Controllo iniziale.')).toBeInTheDocument();
    const esposto = screen.getByLabelText('Esposto');
    await userEvent.clear(esposto);
    await userEvent.type(esposto, 'PRANZO');

    expect(await screen.findByText('Controllo aggiornato.')).toBeInTheDocument();
    const ultimo = api.salvaFinale.mock.calls.at(-1) as [string, { dati: { esposto: string } }];
    expect(ultimo[1].dati.esposto).toBe('PRANZO');
  });

  it('un campo facoltativo svuotato si salva come null', async () => {
    const api = apiFinta({
      prossimo: () => Promise.resolve(candidatoProva({ contenutoGenerato: { ...candidatoProva().contenutoGenerato!, dati: { ...candidatoProva().contenutoGenerato!.dati, spiegazione: 'x' } } })),
    });
    render(<Revisione tipologia="crittografia-breve" api={api} />);

    await userEvent.clear(await screen.findByLabelText('Spiegazione'));

    await waitFor(() => expect(api.salvaFinale).toHaveBeenCalled());
    const ultimo = api.salvaFinale.mock.calls.at(-1) as [string, { dati: { spiegazione: unknown } }];
    expect(ultimo[1].dati.spiegazione).toBeNull();
  });

  it('Accetta manda il tempo, poi carica il prossimo candidato', async () => {
    const primo = candidatoProva({ id: 'a', tempoAttivoMs: 5000, ultimaAttivitaIl: new Date().toISOString() });
    const secondo = candidatoProva({ id: 'b', ordineGenerazione: 2 });
    const coda = [primo, secondo, null];
    const api = apiFinta({ prossimo: () => Promise.resolve(coda.shift() ?? null) });
    render(<Revisione tipologia="crittografia-breve" api={api} />);

    await userEvent.click(await screen.findByRole('button', { name: 'Accetta' }));

    expect(await screen.findByText('Candidato 2', { exact: false })).toBeInTheDocument();
    expect(screen.getByText('Candidato accettato.')).toBeInTheDocument();
    const [id, misura] = api.accetta.mock.calls[0] as [string, { tempoAttivoMs: number }];
    expect(id).toBe('a');
    expect(misura.tempoAttivoMs).toBeGreaterThanOrEqual(5000);
  });

  it("modifica e Accetta subito: l'host riceve l'ultimo contenuto mostrato", async () => {
    let sbloccato = false;
    const api = apiFinta({
      prossimo: () => Promise.resolve(candidatoProva()),
      salvaFinale: (_id, contenuto) =>
        new Promise((ok) => {
          const intervallo = setInterval(() => {
            if (!sbloccato) return;
            clearInterval(intervallo);
            ok(candidatoProva({ contenutoFinale: contenuto }));
          }, 5);
        }),
    });
    render(<Revisione tipologia="crittografia-breve" api={api} />);
    const esposto = await screen.findByLabelText('Esposto');
    await userEvent.clear(esposto);
    await userEvent.type(esposto, 'PRANZO');

    // Il salvataggio è ancora in coda: Accetta resta disabilitato finché non finisce.
    expect(screen.getByRole('button', { name: 'Accetta' })).toBeDisabled();
    sbloccato = true;
    await waitFor(() => expect(screen.getByRole('button', { name: 'Accetta' })).toBeEnabled());
    await userEvent.click(screen.getByRole('button', { name: 'Accetta' }));

    await waitFor(() => expect(api.accetta).toHaveBeenCalled());
    const [, richiesta] = api.accetta.mock.calls[0] as [string, { contenutoFinale: { dati: { esposto: string } } }];
    expect(richiesta.contenutoFinale.dati.esposto).toBe('PRANZO');
  });

  it("dopo un 422 sull'ultimo salvataggio Accetta non accetta e mostra il motivo", async () => {
    const api = apiFinta({
      prossimo: () => Promise.resolve(candidatoProva()),
      salvaFinale: () => Promise.reject(new ErroreApi('no', 422, ['Il diagramma non corrisponde alla soluzione.'])),
    });
    render(<Revisione tipologia="crittografia-breve" api={api} />);

    await userEvent.type(await screen.findByLabelText('Diagramma'), 'x');

    expect(await screen.findByText(/Il diagramma non corrisponde alla soluzione\./, { selector: 'p.errore:not([role])' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Accetta' })).toBeDisabled();
    expect(api.accetta).not.toHaveBeenCalled();
  });

  it('una Tipologia senza editor lo dice', async () => {
    const api = apiFinta({ prossimo: () => Promise.resolve(candidatoProva({ tipologia: 'sconosciuta' })) });
    render(<Revisione tipologia="sconosciuta" api={api} />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Nessun editor');
  });
});
