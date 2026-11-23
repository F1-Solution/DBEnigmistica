# Consegne di A per C (ondata 1b.2, correzioni dopo la revisione)

## Salvataggio e esito: API finale dei componenti comuni

Tutto in `interfaccia/src/comune/`. Scopo: Accetta e Scarta usano il contenuto mostrato a schermo, non una versione più vecchia.

### `useSalvataggio(candidato, servizi)` (`useSalvataggio.ts`)

- `servizi`: basta `{ salvaFinale(id, contenuto) }` (l'`Api` va bene così).
- Restituisce:
  - `contenuto`: il contenuto mostrato (anche se non ancora salvato);
  - `controlli`: i controlli dell'ultimo salvataggio riuscito;
  - `stato`: `'fermo' | 'in-corso' | 'errore'`;
  - `errore`: messaggio dell'ultimo salvataggio fallito, con i motivi dell'host (422) tramite `messaggioDiErrore`; `null` se l'ultimo è riuscito;
  - `modifica(cambia)`: calcola il nuovo contenuto dall'ultimo noto, lo mostra e lo accoda al salvataggio (`PUT .../finale`); conta solo l'ultima risposta;
  - `attendiCoda()`: risolve quando la coda è vuota, riuscita o no.
- Registrazione automatica: se la schermata fornisce `ContestoRegistro` (lo fa `Revisione.tsx`), l'hook vi registra una maniglia. Non serve altro codice negli editor: basta sostituire il proprio hook con questo.
- Utilità: `conDati(contenuto, parziale)` sostituisce campi di `dati`; `contenutoCorrente(candidato)` dà il contenuto di partenza.

### `EsitoRevisione` (`EsitoRevisione.tsx`)

- Da mettere nei `children` dell'editor, come ora. Legge il registro dal contesto.
- Accetta: aspetta la coda, poi manda `tempoAttivoMs`, `ultimaAttivitaIl` e `contenutoFinale` (il contenuto mostrato). Se l'ultimo salvataggio è fallito non accetta e mostra il motivo. È disabilitato mentre un salvataggio è in corso o dopo un errore.
- Scarta: aspetta la coda, poi manda motivo, nota, tempo e, se non ci sono errori di salvataggio, il contenuto mostrato. Uno scarto resta possibile anche con modifiche non valide (l'host non salva il contenuto non valido).
- Se l'host risponde 422 si mostrano gli errori e il cronometro continua.

### Per adottarli in `griglie/**` e `rebus/**`

1. Sostituire `useSalvataggio` locale con quello comune: `import { conDati, useSalvataggio } from '../../comune/useSalvataggio'` (firma `(candidato, servizi)`, senza `dopoSalvataggio`).
2. Mostrare `errore` con `AvvisoSalvataggio` (`comune/AvvisoSalvataggio.tsx`, props `stato` e `errore`) e `controlli` con `ElencoControlli` (`comune/ElencoControlli.tsx`).
3. Il tipo `Candidato` è in `comune/tipi.ts` (superset di `CandidatoRevisione`).

## Altri cambi lato A

- Cronometro: ultimo invio allo smontaggio della schermata e alla chiusura della pagina (`pagehide`), solo se il tempo è cambiato e l'esito non è stato mandato.
- `GET /api/prototipo/tipologie` espone `generazioneConsentita`; "Nuovo candidato" abilita Genera con quel valore e mostra comunque l'avviso quando le regole sono in bozza. Il tipo `Tipologia` ha il nuovo campo: i test di C che costruiscono una `Tipologia` devono aggiungerlo.
- Rotta del tempo: 422 per data senza fuso orario, tempo negativo o data oltre un giorno nel futuro; stesso controllo su `tempoAttivoMs` e `ultimaAttivitaIl` di Accetta e Scarta.
- CSP: `default-src 'self'; base-uri 'none'; form-action 'self'; object-src 'none'; frame-ancestors 'none'`.
