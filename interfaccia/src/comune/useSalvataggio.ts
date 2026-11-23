import { useCallback, useContext, useEffect, useRef, useState } from 'react';
import { messaggioDiErrore } from './api';
import type { Api } from './api';
import { ContestoRegistro } from './registroSalvataggio';
import type { StatoSalvataggio } from './registroSalvataggio';
import type { Candidato, Involucro } from './tipi';

export type { StatoSalvataggio } from './registroSalvataggio';

export interface Salvataggio {
  /** Il contenuto mostrato a schermo. */
  contenuto: Involucro;
  controlli: string[];
  stato: StatoSalvataggio;
  /** Messaggio dell'ultimo salvataggio fallito (con i motivi dell'host), null se l'ultimo è riuscito. */
  errore: string | null;
  /** Calcola il nuovo contenuto dall'ultimo noto, lo mostra e lo salva sul server (PUT .../finale). */
  modifica: (cambia: (corrente: Involucro) => Involucro) => void;
  /** Risolve quando la coda dei salvataggi è vuota (riusciti o falliti). Accetta e Scarta la aspettano. */
  attendiCoda: () => Promise<void>;
}

export function contenutoCorrente(candidato: Candidato): Involucro {
  const contenuto = candidato.contenutoFinale ?? candidato.contenutoGenerato;
  if (contenuto === null) throw new Error('Il candidato non ha un contenuto da rivedere.');
  return contenuto;
}

/**
 * Salvataggi in ordine: ogni modifica parte dopo la precedente e conta solo l'ultima risposta.
 * Se la schermata fornisce un registro (ContestoRegistro), l'hook vi si registra: Accetta e Scarta aspettano la coda,
 * leggono il contenuto mostrato e si bloccano dopo un salvataggio fallito.
 */
export function useSalvataggio(candidato: Candidato, servizi: Pick<Api, 'salvaFinale'>): Salvataggio {
  const registro = useContext(ContestoRegistro);
  const iniziale = contenutoCorrente(candidato);
  const ultimo = useRef<Involucro>(iniziale);
  const coda = useRef<Promise<void>>(Promise.resolve());
  const sequenza = useRef(0);
  const ultimoErrore = useRef<string | null>(null);
  const [contenuto, setContenuto] = useState<Involucro>(iniziale);
  const [controlli, setControlli] = useState<string[]>(candidato.controlli);
  const [stato, setStato] = useState<StatoSalvataggio>('fermo');
  const [errore, setErrore] = useState<string | null>(null);

  const attendiCoda = useCallback(async () => {
    let attesa: Promise<void>;
    do {
      attesa = coda.current;
      await attesa;
    } while (attesa !== coda.current);
  }, []);

  const modifica = useCallback(
    (cambia: (corrente: Involucro) => Involucro) => {
      const prossimo = cambia(ultimo.current);
      ultimo.current = prossimo;
      setContenuto(prossimo);
      setStato('in-corso');
      const mia = ++sequenza.current;
      coda.current = coda.current.then(async () => {
        try {
          const risposta = await servizi.salvaFinale(candidato.id, prossimo);
          ultimoErrore.current = null;
          if (mia !== sequenza.current) return;
          setControlli(risposta.controlli);
          setErrore(null);
          setStato('fermo');
        } catch (e) {
          const messaggio = messaggioDiErrore(e);
          ultimoErrore.current = messaggio;
          if (mia !== sequenza.current) return;
          setErrore(messaggio);
          setStato('errore');
        }
      });
    },
    [candidato.id, servizi],
  );

  useEffect(() => {
    registro?.registra({ attendiCoda, contenuto: () => ultimo.current, errore: () => ultimoErrore.current });
    return () => registro?.registra(null);
  }, [registro, attendiCoda]);

  useEffect(() => {
    registro?.imposta({ stato, errore });
  }, [registro, stato, errore]);

  return { contenuto, controlli, stato, errore, modifica, attendiCoda };
}

/** Sostituisce un campo di `dati` senza toccare il resto. */
export function conDati(contenuto: Involucro, parziale: Record<string, unknown>): Involucro {
  return { ...contenuto, dati: { ...contenuto.dati, ...parziale } };
}
