import type { Tipologia } from './tipi';

export interface ValoriBrief {
  tema: string;
  sottotipo: string;
  /** Un vincolo per riga. */
  vincoli: string;
  difficolta: number;
  righe: number;
  colonne: number;
}

export const ETICHETTE_DIFFICOLTA: ReadonlyArray<string> = ['Molto facile', 'Facile', 'Media', 'Difficile', 'Molto difficile'];

/** Il brief nella forma del contratto (snake_case, campi non ammessi a null): il server aggiunge la variante. */
export function costruisciBrief(tipologia: Tipologia, valori: ValoriBrief): Record<string, unknown> {
  const campi = new Set(tipologia.campiBrief.map((c) => c.nome));
  const tema = valori.tema.trim();
  return {
    versione_formato: 1,
    tipologia: tipologia.id,
    versione_tipologia: tipologia.versione,
    famiglia: tipologia.famiglia,
    tema: campi.has('tema') && tema !== '' ? tema : null,
    vincoli: campi.has('vincoli')
      ? valori.vincoli
          .split('\n')
          .map((v) => v.trim())
          .filter((v) => v !== '')
      : [],
    difficolta: valori.difficolta,
    dimensioni: campi.has('dimensioni') ? { righe: valori.righe, colonne: valori.colonne } : null,
    sottotipo: campi.has('sottotipo') && valori.sottotipo !== '' ? valori.sottotipo : null,
  };
}

/** Lati ammessi per le dimensioni quadrate, oppure l'intervallo di righe e colonne. */
export function limitiDimensioni(tipologia: Tipologia): { lati: number[] | null; minimo: number; massimo: number } | null {
  const campo = tipologia.campiBrief.find((c) => c.nome === 'dimensioni');
  if (!campo) return null;
  if (campo.tipo === 'dimensioni-quadrate') {
    const lati = campo.valori.map(Number);
    return { lati, minimo: Math.min(...lati), massimo: Math.max(...lati) };
  }
  return { lati: null, minimo: campo.minimo ?? 1, massimo: campo.massimo ?? 99 };
}
