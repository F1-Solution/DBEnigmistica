# ADR 011: risposta grezza del Fornitore AI fuori dal registro immutabile

- **Stato:** Accettato il 19/11/2026
- **Data:** 19/11/2026
- **Integra:** ADR 005. Il registro in sola aggiunta contiene solo metadati e impronte delle chiamate al Fornitore AI.
- **Fonte:** `docs/fase-1-decisioni.md`, decisione D7

## Contesto

L'ADR 003, punto 8, prevede che ogni chiamata al Fornitore AI registri fornitore, modello, durata, esito e risposta grezza. Il ROADMAP della fase 1 diceva di mettere la risposta grezza nel registro.

Il registro in sola aggiunta (ADR 005) rifiuta `UPDATE`, `DELETE` e `TRUNCATE`. Una risposta grezza può contenere testo tratto dalle Riviste. Se il parere legale chiedesse di eliminarla, nel registro non sarebbe possibile.

## Decisione

1. La risposta grezza va nella tabella `riviste.chiamate_ai`, che si può cancellare.
2. Nel registro in sola aggiunta finiscono solo i metadati della chiamata: fornitore, modello, durata, esito, token e costo, quando il fornitore li riporta.
3. Nel registro va anche lo SHA-256 della risposta grezza. Così la prova del contenuto resta finché la tabella esiste.
4. Il ROADMAP della fase 1 si corregge: oggi dice «risposta grezza nel registro».

## Alternative scartate

- **Risposta grezza nel registro**: non si potrebbe cancellare nemmeno su richiesta del legale, perché il registro vieta cancellazioni.
- **Nessuna risposta conservata**: si perderebbe il materiale per l'analisi degli errori dell'ondata 1.4.

## Conseguenze

- Cancellare righe di `riviste.chiamate_ai` non tocca il registro. Il registro non contiene il testo della risposta, quindi la sua catena resta valida.
- Rischio: la prova del contenuto regge solo finché la tabella non viene svuotata. Dopo lo svuotamento resta solo l'impronta, che non basta a ricostruire la risposta.
- La scelta vale per le risposte grezze. I metadati di chiamata restano nel registro e seguono la regola dell'ADR 005.

## Riferimenti

- `docs/fase-1-decisioni.md`, D7 e la sezione «Rischi».
- ADR 003, punto 8 (registrazione di ogni chiamata).
- ADR 005, punto 6 (registro in sola aggiunta) e conseguenze.
- `ROADMAP.md`, fase 1 (correzione richiesta).
