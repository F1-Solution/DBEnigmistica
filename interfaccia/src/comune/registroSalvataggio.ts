import { createContext } from 'react';
import type { Involucro } from './tipi';

export type StatoSalvataggio = 'fermo' | 'in-corso' | 'errore';

/** Quello che l'esito (Accetta e Scarta) deve sapere dell'editor: contenuto mostrato, coda dei salvataggi, ultimo errore. */
export interface ManigliaSalvataggio {
  /** Risolve quando tutti i salvataggi accodati sono finiti (riusciti o falliti). */
  attendiCoda(): Promise<void>;
  /** Il contenuto mostrato a schermo, anche se non ancora salvato. */
  contenuto(): Involucro;
  /** Messaggio dell'ultimo salvataggio fallito, oppure null se l'ultimo è riuscito. */
  errore(): string | null;
}

export interface StatoRegistro {
  stato: StatoSalvataggio;
  errore: string | null;
}

/** Punto di incontro tra l'editor (che salva) e l'esito (che accetta o scarta): un piccolo store osservabile. */
export interface RegistroSalvataggio {
  registra(maniglia: ManigliaSalvataggio | null): void;
  maniglia(): ManigliaSalvataggio | null;
  imposta(stato: StatoRegistro): void;
  istantanea(): StatoRegistro;
  iscriviti(ascoltatore: () => void): () => void;
}

export function creaRegistro(): RegistroSalvataggio {
  let corrente: ManigliaSalvataggio | null = null;
  let stato: StatoRegistro = { stato: 'fermo', errore: null };
  const ascoltatori = new Set<() => void>();

  return {
    registra: (maniglia) => {
      corrente = maniglia;
    },
    maniglia: () => corrente,
    imposta: (nuovo) => {
      if (nuovo.stato === stato.stato && nuovo.errore === stato.errore) return;
      stato = nuovo;
      for (const a of ascoltatori) a();
    },
    istantanea: () => stato,
    iscriviti: (a) => {
      ascoltatori.add(a);
      return () => {
        ascoltatori.delete(a);
      };
    },
  };
}

/** Il registro del candidato in revisione: lo fornisce la schermata di revisione, lo usa useSalvataggio. */
export const ContestoRegistro = createContext<RegistroSalvataggio | null>(null);
