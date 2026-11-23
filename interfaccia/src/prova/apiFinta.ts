import { vi } from 'vitest';
import type { Api } from '../comune/api';
import type { Candidato, Involucro, Tipologia } from '../comune/tipi';

export function involucroCrittografia(esposto = 'CENA SOVRANO'): Involucro {
  return {
    versione_formato: 1,
    tipologia: 'crittografia-breve',
    versione_tipologia: 0,
    titolo: null,
    dati: {
      sottotipo: 'sinonimica',
      esposto,
      diagramma: '5 2 = 7',
      prima_lettura: 'pasto re',
      soluzione: 'PASTORE',
      spiegazione: null,
    },
    avvisi: [],
  };
}

export function candidatoProva(parziale: Partial<Candidato> = {}): Candidato {
  return {
    id: '11111111-1111-1111-1111-111111111111',
    tipologia: 'crittografia-breve',
    versioneTipologia: 0,
    ordineGenerazione: 1,
    stato: 'in_revisione',
    modo: 'ai',
    messaggio: null,
    versionePrompt: 'creazione-v1-000000000000',
    versioneGeneratore: null,
    seme: null,
    tempoAttivoMs: 0,
    ultimaAttivitaIl: null,
    motivoScarto: null,
    brief: null,
    contenutoGenerato: involucroCrittografia(),
    contenutoFinale: null,
    differenze: null,
    schemaCaselle: null,
    controlli: [],
    ...parziale,
  };
}

export function tipologiaProva(parziale: Partial<Tipologia> = {}): Tipologia {
  return {
    id: 'crittografia-breve',
    versione: 0,
    famiglia: 'crittografia',
    nome: 'Crittografia breve',
    modo: 'ai',
    statoRegole: 'bozza',
    regoleApprovate: false,
    generazioneConsentita: false,
    motivoGenerazioneNonConsentita: null,
    campiBrief: [
      { nome: 'tema', etichetta: 'Tema', tipo: 'testo', obbligatorio: false, valori: [], minimo: null, massimo: 200 },
      { nome: 'sottotipo', etichetta: 'Sottotipo', tipo: 'scelta', obbligatorio: true, valori: ['sinonimica', 'perifrastica'], minimo: null, massimo: null },
      { nome: 'vincoli', etichetta: 'Vincoli', tipo: 'elenco-testi', obbligatorio: false, valori: [], minimo: null, massimo: 8 },
      { nome: 'difficolta', etichetta: 'Difficoltà', tipo: 'intero', obbligatorio: true, valori: [], minimo: 1, massimo: 5 },
    ],
    ...parziale,
  };
}

/** Api finta con ogni metodo sostituibile: i test controllano le chiamate. */
export function apiFinta(parziale: Partial<Api> = {}): { [K in keyof Api]: ReturnType<typeof vi.fn> & Api[K] } {
  const base: Api = {
    tipologie: () => Promise.resolve([tipologiaProva()]),
    genera: () => Promise.resolve({ candidati: [], valido: null, messaggio: null }),
    prossimo: () => Promise.resolve(null),
    salvaFinale: (_id, contenuto) => Promise.resolve(candidatoProva({ contenutoFinale: contenuto })),
    salvaTempo: () => Promise.resolve(candidatoProva()),
    accetta: () => Promise.resolve(candidatoProva({ stato: 'accettato' })),
    scarta: () => Promise.resolve(candidatoProva({ stato: 'scartato' })),
  };
  const unita = { ...base, ...parziale };
  return Object.fromEntries(Object.entries(unita).map(([nome, f]) => [nome, vi.fn(f)])) as ReturnType<typeof apiFinta>;
}
