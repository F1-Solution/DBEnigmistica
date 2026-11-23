import type { Dati } from '../../comune/tipi';

export interface Griglia {
  righe: number;
  colonne: number;
  celle: string[];
  numeri: unknown[];
}

export type Verso = 'destra' | 'sotto';
export type TipoSegno = 'maggiore' | 'minore';

export interface Segno {
  riga: number;
  colonna: number;
  verso: Verso;
  segno: TipoSegno;
}

export interface IndiziNonogramma {
  righe: string[];
  colonne: string[];
}

export function leggiGriglia(dati: Dati, nome: string): Griglia {
  const g = dati[nome] as Griglia | undefined;
  if (!g || !Array.isArray(g.celle)) throw new Error(`Il campo «${nome}» non è una griglia.`);
  return g;
}

/** Restituisce una nuova griglia con un solo carattere cambiato. */
export function impostaCella(griglia: Griglia, riga: number, colonna: number, carattere: string): Griglia {
  const celle = griglia.celle.map((r, i) =>
    i === riga ? r.slice(0, colonna) + carattere + r.slice(colonna + 1) : r,
  );
  return { ...griglia, celle };
}

export function alternaPiena(griglia: Griglia, riga: number, colonna: number): Griglia {
  const attuale = griglia.celle[riga]?.[colonna];
  return impostaCella(griglia, riga, colonna, attuale === '#' ? '.' : '#');
}

function blocchiDi(linea: string): string {
  const blocchi = linea.split(/\.+/).filter((b) => b.length > 0).map((b) => String(b.length));
  return blocchi.length === 0 ? '0' : blocchi.join(' ');
}

/** Indizi dal disegno: derivazione dei margini, non risoluzione del gioco. */
export function calcolaIndizi(celle: readonly string[]): IndiziNonogramma {
  const colonne = celle[0]?.length ?? 0;
  return {
    righe: celle.map(blocchiDi),
    colonne: Array.from({ length: colonne }, (_, c) => blocchiDi(celle.map((r) => r[c]).join(''))),
  };
}

export function indiziComeElenco(indizi: string[]): { indice: number; blocchi: string }[] {
  return indizi.map((blocchi, i) => ({ indice: i + 1, blocchi }));
}

function ordine(a: Segno, b: Segno): number {
  return a.riga - b.riga || a.colonna - b.colonna || (a.verso === b.verso ? 0 : a.verso === 'destra' ? -1 : 1);
}

/** Nessun segno, poi maggiore, poi minore, poi di nuovo nessuno; l'elenco resta nell'ordine del formato. */
export function ciclaSegno(segni: readonly Segno[], riga: number, colonna: number, verso: Verso): Segno[] {
  const attuale = segni.find((s) => s.riga === riga && s.colonna === colonna && s.verso === verso);
  const altri = segni.filter((s) => s !== attuale);
  if (!attuale) return [...altri, { riga, colonna, verso, segno: 'maggiore' as const }].sort(ordine);
  if (attuale.segno === 'maggiore') return [...altri, { ...attuale, segno: 'minore' as const }].sort(ordine);
  return altri;
}

export function simboloSegno(verso: Verso, segno: TipoSegno | null): string {
  if (segno === null) return '';
  if (verso === 'destra') return segno === 'maggiore' ? '>' : '<';
  return segno === 'maggiore' ? '∨' : '∧';
}

export function descriviSegno(riga: number, colonna: number, verso: Verso, segno: TipoSegno | null): string {
  const vicina = verso === 'destra' ? `(${riga},${colonna + 1})` : `(${riga + 1},${colonna})`;
  const testo = segno === null ? 'nessun segno' : segno === 'maggiore' ? 'prima maggiore della seconda' : 'prima minore della seconda';
  return `Segno tra (${riga},${colonna}) e ${vicina}: ${testo}`;
}
