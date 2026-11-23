import type { Dati } from '../../comune/tipi';

export interface Vignetta {
  vignetta: number;
  descrizione: string;
  lettere: string | null;
  parola: string;
}

export function leggiVignette(dati: Dati): Vignetta[] {
  const v = dati.vignette;
  return Array.isArray(v) ? (v as Vignetta[]) : [];
}

export function aggiornaVignetta(elenco: readonly Vignetta[], indice: number, parziale: Partial<Vignetta>): Vignetta[] {
  return elenco.map((v, i) => (i === indice ? { ...v, ...parziale } : v));
}

export function rimuoviVignetta(elenco: readonly Vignetta[], indice: number): Vignetta[] {
  return elenco.filter((_, i) => i !== indice);
}

export function aggiungiVignetta(elenco: readonly Vignetta[]): Vignetta[] {
  const ultima = elenco.at(-1)?.vignetta ?? 1;
  return [...elenco, { vignetta: ultima, descrizione: '', lettere: null, parola: '' }];
}
