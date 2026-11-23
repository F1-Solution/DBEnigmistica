// Forme JSON dell'API del prototipo (nomi in camelCase come li scrive l'host; i contenuti dei Giochi restano in snake_case).

export type Dati = Record<string, unknown>;

export interface Involucro {
  versione_formato: number;
  tipologia: string;
  versione_tipologia: number;
  titolo: string | null;
  dati: Dati;
  avvisi: string[];
}

export interface Differenza {
  percorso: string;
  prima: unknown;
  dopo: unknown;
}

export interface ParolaSchema {
  numero: number;
  direzione: 'orizzontale' | 'verticale';
  riga: number;
  colonna: number;
  lunghezza: number;
}

export interface SchemaCaselle {
  righe: number;
  colonne: number;
  celle: string[];
  parole: ParolaSchema[];
}

export interface Candidato {
  id: string;
  tipologia: string;
  versioneTipologia: number;
  ordineGenerazione: number;
  stato: string;
  modo: 'ai' | 'algoritmo';
  messaggio: string | null;
  versionePrompt: string | null;
  versioneGeneratore: string | null;
  seme: number | null;
  tempoAttivoMs: number;
  ultimaAttivitaIl: string | null;
  motivoScarto: string | null;
  brief: Dati | null;
  contenutoGenerato: Involucro | null;
  contenutoFinale: Involucro | null;
  differenze: Differenza[] | null;
  schemaCaselle: SchemaCaselle | null;
  controlli: string[];
}

export interface EsitoGenerazione {
  candidati: Candidato[];
  valido: string | null;
  messaggio: string | null;
}

export type TipoCampoBrief = 'testo' | 'elenco-testi' | 'intero' | 'scelta' | 'dimensioni-quadrate' | 'dimensioni';

export interface CampoBrief {
  nome: 'tema' | 'sottotipo' | 'vincoli' | 'difficolta' | 'dimensioni';
  etichetta: string;
  tipo: TipoCampoBrief;
  obbligatorio: boolean;
  valori: string[];
  minimo: number | null;
  massimo: number | null;
}

export interface Tipologia {
  id: string;
  versione: number;
  famiglia: string;
  nome: string;
  modo: 'ai' | 'algoritmo';
  statoRegole: 'bozza' | 'approvata';
  regoleApprovate: boolean;
  /** Vero per gli algoritmi, per le regole approvate e per le regole in bozza con le Registrazioni in modalità riproduci. */
  generazioneConsentita: boolean;
  /** Perché Genera è disattivato (interruttore dal vivo spento o regole in bozza); null se è consentito. */
  motivoGenerazioneNonConsentita: string | null;
  campiBrief: CampoBrief[];
}

export interface MisuraTempo {
  tempoAttivoMs: number;
  ultimaAttivitaIl: string;
}

/** Accetta: tempo finale e, se l'editor lo conosce, il contenuto mostrato a schermo (l'host lo ricontrolla). */
export interface RichiestaAccetta extends MisuraTempo {
  contenutoFinale?: Involucro;
}

export interface RichiestaScarto extends RichiestaAccetta {
  motivo: string;
  nota: string;
}

/** Motivi dello scarto: lista chiusa del progetto (1b.1, sezione 4.2). */
export const MOTIVI_SCARTO: ReadonlyArray<{ codice: string; etichetta: string }> = [
  { codice: 'soluzione_errata', etichetta: 'Soluzione errata' },
  { codice: 'regole_non_rispettate', etichetta: 'Regole non rispettate' },
  { codice: 'incroci_incoerenti', etichetta: 'Incroci incoerenti' },
  { codice: 'qualita_insufficiente', etichetta: 'Qualità insufficiente' },
  { codice: 'banale_o_troppo_facile', etichetta: 'Banale o troppo facile' },
  { codice: 'contenuto_inadatto', etichetta: 'Contenuto inadatto' },
  { codice: 'altro', etichetta: 'Altro' },
];

export const MASSIMO_NOTA = 1000;
