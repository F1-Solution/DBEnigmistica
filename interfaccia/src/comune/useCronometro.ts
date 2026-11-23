import { useEffect, useState } from 'react';
import type { Api } from './api';
import { creaCronometro, statoIniziale } from './cronometro';
import type { Cronometro, TipoAttivita } from './cronometro';
import type { Candidato } from './tipi';

const EVENTI: ReadonlyArray<TipoAttivita> = ['pointerdown', 'keydown', 'input', 'wheel', 'scroll', 'pointermove'];

export interface CronometroRevisione {
  cronometro: Cronometro;
  /** Millisecondi di tempo attivo, aggiornati ogni secondo per la visualizzazione. */
  tempoAttivoMs: number;
}

/**
 * Cronometro del tempo attivo di un candidato: parte dai valori salvati, conta apertura, input e ritorno del fuoco,
 * invia ogni 15 secondi se è cambiato e si ferma allo smontaggio.
 */
export function useCronometro(candidato: Candidato, servizi: Pick<Api, 'salvaTempo'>): CronometroRevisione {
  const [tempoAttivoMs, setTempo] = useState(candidato.tempoAttivoMs);

  // Un cronometro per candidato (il chiamante usa key={candidato.id}): i valori salvati servono solo alla partenza.
  const [cronometro] = useState(() =>
    creaCronometro({
      ora: () => Date.now(),
      invia: (misura) => servizi.salvaTempo(candidato.id, misura),
      iniziale: statoIniziale(candidato.tempoAttivoMs, candidato.ultimaAttivitaIl),
    }),
  );

  useEffect(() => {
    const registra = (tipo: TipoAttivita) => () => cronometro.attivita(tipo);
    const ascoltatori = EVENTI.map((tipo) => [tipo, registra(tipo)] as const);
    for (const [tipo, funzione] of ascoltatori) window.addEventListener(tipo, funzione, { capture: true, passive: true });
    const alFuoco = registra('focus');
    window.addEventListener('focus', alFuoco);
    const allaChiusura = () => void cronometro.inviaOra();
    window.addEventListener('pagehide', allaChiusura);

    cronometro.attivita('apertura');
    cronometro.avvia();
    const visualizza = setInterval(() => setTempo(cronometro.misura().tempoAttivoMs), 1000);

    return () => {
      for (const [tipo, funzione] of ascoltatori) window.removeEventListener(tipo, funzione, { capture: true });
      window.removeEventListener('focus', alFuoco);
      window.removeEventListener('pagehide', allaChiusura);
      clearInterval(visualizza);
      // Ultimo invio allo smontaggio (cambio di percorso o chiusura): non fa nulla se l'esito è già stato mandato.
      void cronometro.chiudi();
    };
  }, [cronometro]);

  return { cronometro, tempoAttivoMs };
}

/** Minuti e secondi, per esempio 12:05. */
export function formattaTempo(ms: number): string {
  const secondi = Math.floor(Math.max(0, ms) / 1000);
  const minuti = Math.floor(secondi / 60);
  return `${String(minuti).padStart(2, '0')}:${String(secondi % 60).padStart(2, '0')}`;
}
