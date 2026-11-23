import type { Dati, ParolaSchema } from '../../comune/tipi';

export interface NumeroCasella {
  riga: number;
  colonna: number;
  numero: number;
}

export interface GrigliaCruciverba {
  righe: number;
  colonne: number;
  celle: string[];
  numeri: NumeroCasella[];
}

export interface Definizione {
  numero: number;
  direzione: 'orizzontale' | 'verticale';
  testo: string;
  soluzione: string;
}

export const CASELLA_NERA = '#';
/** Casella bianca ancora vuota: non è valida per lo schema, quindi il server la segnala. */
export const CASELLA_VUOTA = '.';

const ELEMENTO_NON_VALIDO = 'Il contenuto del cruciverba non ha la forma attesa.';

export function leggiGriglia(dati: Dati): GrigliaCruciverba {
  const g = dati['griglia'] as Partial<GrigliaCruciverba> | undefined;
  if (!g || !Array.isArray(g.celle) || typeof g.righe !== 'number' || typeof g.colonne !== 'number') throw new Error(ELEMENTO_NON_VALIDO);
  return { righe: g.righe, colonne: g.colonne, celle: g.celle, numeri: Array.isArray(g.numeri) ? g.numeri : [] };
}

export function leggiDefinizioni(dati: Dati): Definizione[] {
  const d = dati['definizioni'];
  if (!Array.isArray(d)) throw new Error(ELEMENTO_NON_VALIDO);
  return d as Definizione[];
}

/** Una sola lettera maiuscola A-Z; qualsiasi altro carattere svuota la casella. */
export function normalizzaLettera(testo: string): string {
  const ultima = testo.slice(-1).toUpperCase();
  return /^[A-Z]$/.test(ultima) ? ultima : CASELLA_VUOTA;
}

/** Nuova griglia con la lettera impostata (coordinate da 0); le caselle nere non si toccano. */
export function impostaLettera(griglia: GrigliaCruciverba, riga: number, colonna: number, testo: string): GrigliaCruciverba {
  const attuale = griglia.celle[riga];
  if (attuale === undefined || attuale[colonna] === undefined || attuale[colonna] === CASELLA_NERA) return griglia;
  const nuova = attuale.slice(0, colonna) + normalizzaLettera(testo) + attuale.slice(colonna + 1);
  return { ...griglia, celle: griglia.celle.map((r, i) => (i === riga ? nuova : r)) };
}

export function numeroDellaCasella(griglia: GrigliaCruciverba, riga: number, colonna: number): number | null {
  return griglia.numeri.find((n) => n.riga === riga + 1 && n.colonna === colonna + 1)?.numero ?? null;
}

/** Lettere della griglia lungo una parola dello schema delle caselle. */
export function lettereDellaParola(griglia: GrigliaCruciverba, parola: ParolaSchema): string {
  let testo = '';
  for (let i = 0; i < parola.lunghezza; i++) {
    const riga = parola.riga - 1 + (parola.direzione === 'verticale' ? i : 0);
    const colonna = parola.colonna - 1 + (parola.direzione === 'orizzontale' ? i : 0);
    testo += griglia.celle[riga]?.[colonna] ?? CASELLA_VUOTA;
  }
  return testo;
}

/** Copia le lettere della griglia nelle soluzioni delle definizioni (stessa parola per numero e direzione). */
export function soluzioniDallaGriglia(griglia: GrigliaCruciverba, definizioni: Definizione[], parole: ParolaSchema[]): Definizione[] {
  return definizioni.map((d) => {
    const parola = parole.find((p) => p.numero === d.numero && p.direzione === d.direzione);
    return parola ? { ...d, soluzione: lettereDellaParola(griglia, parola) } : d;
  });
}

export function conDefinizione(definizioni: Definizione[], indice: number, parziale: Partial<Definizione>): Definizione[] {
  return definizioni.map((d, i) => (i === indice ? { ...d, ...parziale } : d));
}
