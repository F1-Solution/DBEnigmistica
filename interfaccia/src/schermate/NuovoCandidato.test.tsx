import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { ErroreApi } from '../comune/api';
import { apiFinta, candidatoProva, tipologiaProva } from '../prova/apiFinta';
import { NuovoCandidato } from './NuovoCandidato';

describe('NuovoCandidato', () => {
  it("con le regole in bozza mostra il motivo dell'host e disattiva Genera per le Tipologie con AI", async () => {
    const motivo = "Regole in bozza: la generazione con l'AI non è disponibile finché il proprietario non approva le regole di questa Tipologia.";
    const api = apiFinta({ tipologie: () => Promise.resolve([tipologiaProva({ motivoGenerazioneNonConsentita: motivo })]) });
    render(<NuovoCandidato api={api} />);

    expect(await screen.findByRole('note')).toHaveTextContent(motivo);
    expect(screen.getByRole('button', { name: 'Genera' })).toBeDisabled();
  });

  it("con l'interruttore delle chiamate dal vivo spento mostra il messaggio di D8 accanto al pulsante disattivato", async () => {
    const d8 = 'Le chiamate dal vivo sono disattivate. Si abilitano con FornitoreAi:DalVivoAbilitato=true solo dopo la prova 1.1-prova (PROGRESS.md).';
    const api = apiFinta({
      tipologie: () =>
        Promise.resolve([tipologiaProva({ statoRegole: 'approvata', regoleApprovate: true, generazioneConsentita: false, motivoGenerazioneNonConsentita: d8 })]),
    });
    render(<NuovoCandidato api={api} />);

    expect(await screen.findByText(d8)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Genera' })).toBeDisabled();
    expect(screen.queryByText(/Regole in bozza/)).not.toBeInTheDocument();
  });

  it("con un host senza motivo ma regole in bozza usa il testo di riserva", async () => {
    render(<NuovoCandidato api={apiFinta()} />);

    expect(await screen.findByRole('note')).toHaveTextContent('Regole in bozza');
  });

  it('con regole in bozza ma generazione consentita (riproduci) avvisa e abilita Genera', async () => {
    const api = apiFinta({ tipologie: () => Promise.resolve([tipologiaProva({ generazioneConsentita: true })]) });
    render(<NuovoCandidato api={api} />);

    expect(await screen.findByRole('note')).toHaveTextContent('Registrazioni in modalità riproduci');
    expect(screen.getByRole('button', { name: 'Genera' })).toBeEnabled();
  });

  it('con le regole approvate genera con il brief nella forma del contratto', async () => {
    const api = apiFinta({ tipologie: () => Promise.resolve([tipologiaProva({ regoleApprovate: true, statoRegole: 'approvata', generazioneConsentita: true })]) });
    render(<NuovoCandidato api={api} />);

    await userEvent.selectOptions(await screen.findByLabelText('Sottotipo'), 'perifrastica');
    await userEvent.type(screen.getByLabelText(/Vincoli/), 'niente nomi propri');
    await userEvent.click(screen.getByRole('button', { name: 'Genera' }));

    expect(api.genera).toHaveBeenCalledWith({
      versione_formato: 1,
      tipologia: 'crittografia-breve',
      versione_tipologia: 0,
      famiglia: 'crittografia',
      tema: null,
      vincoli: ['niente nomi propri'],
      difficolta: 3,
      dimensioni: null,
      sottotipo: 'perifrastica',
    });
  });

  it('mostra i candidati non validi e il messaggio dopo 3 tentativi', async () => {
    const api = apiFinta({
      tipologie: () => Promise.resolve([tipologiaProva({ regoleApprovate: true, generazioneConsentita: true })]),
      genera: () =>
        Promise.resolve({
          valido: null,
          messaggio: 'Dopo 3 tentativi nessun candidato è valido per lo schema: riprova o cambia il brief.',
          candidati: [1, 2, 3].map((n) => candidatoProva({ id: `id-${n}`, ordineGenerazione: n, stato: 'non_valido_schema', messaggio: `campo ${n} mancante` })),
        }),
    });
    render(<NuovoCandidato api={api} />);

    await userEvent.selectOptions(await screen.findByLabelText('Sottotipo'), 'sinonimica');
    await userEvent.click(screen.getByRole('button', { name: 'Genera' }));

    expect(await screen.findByText(/Dopo 3 tentativi/)).toBeInTheDocument();
    expect(screen.getAllByText(/non valido per lo schema/)).toHaveLength(3);
    expect(screen.getByText(/campo 2 mancante/)).toBeInTheDocument();
  });

  it("mostra l'errore dell'host (per esempio il rifiuto con l'interruttore spento)", async () => {
    const api = apiFinta({
      tipologie: () => Promise.resolve([tipologiaProva({ regoleApprovate: true, generazioneConsentita: true })]),
      genera: () => Promise.reject(new ErroreApi('no', 422, ['Le chiamate AI dal vivo non sono abilitate.'])),
    });
    render(<NuovoCandidato api={api} />);

    await userEvent.selectOptions(await screen.findByLabelText('Sottotipo'), 'sinonimica');
    await userEvent.click(screen.getByRole('button', { name: 'Genera' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('Le chiamate AI dal vivo non sono abilitate.');
  });

  it('i giochi logici usano il modulo dedicato: niente tema, brief senza vincoli', async () => {
    const nonogramma = tipologiaProva({
      id: 'nonogramma',
      famiglia: 'logico',
      modo: 'algoritmo',
      nome: 'Nonogramma',
      generazioneConsentita: true,
      campiBrief: [
        { nome: 'difficolta', etichetta: 'Difficoltà', tipo: 'intero', obbligatorio: true, valori: [], minimo: 1, massimo: 5 },
        { nome: 'dimensioni', etichetta: 'Dimensioni', tipo: 'dimensioni', obbligatorio: true, valori: [], minimo: 10, massimo: 15 },
      ],
    });
    const api = apiFinta({ tipologie: () => Promise.resolve([nonogramma]) });
    render(<NuovoCandidato api={api} tipologiaIniziale="nonogramma" />);

    expect(await screen.findByRole('option', { name: 'Giochi logici' }, { timeout: 5000 })).toBeInTheDocument();
    expect(screen.queryByLabelText(/Tema/)).not.toBeInTheDocument();
    expect(screen.queryByRole('note')).not.toBeInTheDocument();
    await userEvent.click(await screen.findByRole('button', { name: /Genera/ }, { timeout: 5000 }));

    expect(api.genera).toHaveBeenCalledWith(expect.objectContaining({ tipologia: 'nonogramma', tema: null, vincoli: [] }));
  });
});
