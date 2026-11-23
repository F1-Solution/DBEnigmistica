import type { MisuraTempo } from './tipi';

export const PAUSA_MASSIMA_MS = 120_000;
export const INTERVALLO_POINTERMOVE_MS = 5_000;
export const INTERVALLO_INVIO_MS = 15_000;

export type TipoAttivita = 'apertura' | 'pointerdown' | 'keydown' | 'input' | 'wheel' | 'scroll' | 'pointermove' | 'focus' | 'esito';

export interface StatoTempo {
  tempoAttivoMs: number;
  ultimaAttivitaMs: number | null;
}

/** Stato di partenza dai valori salvati dal server: una data illeggibile vale come nessuna attività. */
export function statoIniziale(tempoAttivoMs: number, ultimaAttivitaIl: string | null): StatoTempo {
  const ms = ultimaAttivitaIl === null ? Number.NaN : Date.parse(ultimaAttivitaIl);
  return { tempoAttivoMs, ultimaAttivitaMs: Number.isNaN(ms) ? null : ms };
}

/** Aggiunge un istante di attività: conta l'intervallo dal precedente solo se è tra 0 e 120.000 ms compresi. */
export function registraIstante(stato: StatoTempo, istante: number): StatoTempo {
  if (stato.ultimaAttivitaMs === null) return { ...stato, ultimaAttivitaMs: istante };
  const intervallo = istante - stato.ultimaAttivitaMs;
  const conta = intervallo >= 0 && intervallo <= PAUSA_MASSIMA_MS;
  return { tempoAttivoMs: stato.tempoAttivoMs + (conta ? intervallo : 0), ultimaAttivitaMs: istante };
}

/** Tempo attivo da un elenco di istanti in ordine di arrivo (funzione pura). */
export function tempoAttivo(istanti: readonly number[]): number {
  const vuoto: StatoTempo = { tempoAttivoMs: 0, ultimaAttivitaMs: null };
  return istanti.reduce(registraIstante, vuoto).tempoAttivoMs;
}

export interface OpzioniCronometro {
  ora: () => number;
  invia: (misura: MisuraTempo) => Promise<unknown> | void;
  iniziale: StatoTempo;
}

export interface Cronometro {
  attivita(tipo: TipoAttivita): void;
  misura(): MisuraTempo;
  /** Registra il clic su Accetta o Scarta e restituisce la misura finale; i timer restano finché non si chiama ferma. */
  concludi(): MisuraTempo;
  avvia(): void;
  /** Manda subito la misura se è cambiata dall'ultimo invio (per esempio alla chiusura della pagina). */
  inviaOra(): Promise<void>;
  /** Ultimo invio e arresto: per lo smontaggio della schermata. Dopo l'esito (ferma già chiamato) non manda nulla. */
  chiudi(): Promise<void>;
  ferma(): void;
}

function aMisura(stato: StatoTempo, ora: number): MisuraTempo {
  return { tempoAttivoMs: stato.tempoAttivoMs, ultimaAttivitaIl: new Date(stato.ultimaAttivitaMs ?? ora).toISOString() };
}

export function creaCronometro({ ora, invia, iniziale }: OpzioniCronometro): Cronometro {
  let stato = iniziale;
  let ultimoPointermove = Number.NEGATIVE_INFINITY;
  let ultimoInviato: StatoTempo | null = null;
  let timer: ReturnType<typeof setInterval> | null = null;
  let fermo = false;
  let inInvio = false;

  const invioNecessario = () =>
    ultimoInviato === null || ultimoInviato.tempoAttivoMs !== stato.tempoAttivoMs || ultimoInviato.ultimaAttivitaMs !== stato.ultimaAttivitaMs;

  async function giroDiInvio() {
    if (fermo || inInvio || stato.ultimaAttivitaMs === null || !invioNecessario()) return;
    const inviato = stato;
    inInvio = true;
    try {
      await invia(aMisura(inviato, ora()));
      ultimoInviato = inviato;
    } catch {
      // Si ritenta al giro successivo: nel frattempo il tempo resta nel browser.
    } finally {
      inInvio = false;
    }
  }

  function fermaTutto() {
    fermo = true;
    if (timer !== null) clearInterval(timer);
    timer = null;
  }

  return {
    inviaOra: giroDiInvio,
    async chiudi() {
      const invio = giroDiInvio();
      fermaTutto();
      await invio;
    },
    attivita(tipo) {
      if (fermo) return;
      const adesso = ora();
      if (tipo === 'pointermove') {
        if (adesso - ultimoPointermove < INTERVALLO_POINTERMOVE_MS) return;
        ultimoPointermove = adesso;
      }
      stato = registraIstante(stato, adesso);
    },
    misura: () => aMisura(stato, ora()),
    concludi() {
      stato = registraIstante(stato, ora());
      return aMisura(stato, ora());
    },
    avvia() {
      fermo = false;
      if (timer !== null) return;
      timer = setInterval(() => void giroDiInvio(), INTERVALLO_INVIO_MS);
    },
    ferma: fermaTutto,
  };
}
