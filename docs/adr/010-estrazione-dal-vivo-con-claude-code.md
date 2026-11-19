# ADR 010: estrazione dal vivo su Windows con Claude Code e login OAuth

- **Stato:** Accettato il 19/11/2026
- **Data:** 19/11/2026
- **Integra:** ADR 004. Precisa come Claude Code si autentica in fase 1 e sposta la decisione sul container alla fase 2.
- **Fonte:** `docs/fase-1-decisioni.md`, decisione D5

## Contesto

L'ADR 004 prevede Claude Code non interattivo con l'abbonamento del proprietario, solo nello Studio sul PC. Lo stesso ADR chiedeva di verificare nella fase 1 se lo Studio potesse girare in container. Il container dello Studio non esiste prima della fase 2.

Claude Code preferisce una chiave API, se è presente nell'ambiente, al login con l'abbonamento. Con la chiave, l'uso verrebbe addebitato a consumo, contro la decisione del proprietario (ADR 004, alternative scartate).

## Decisione

1. In fase 1 l'estrazione dal vivo gira sul PC Windows del proprietario. Usa l'eseguibile `dbe` win-x64 pubblicato dalla CI e Claude Code installato sul PC.
2. Claude Code si autentica con il login OAuth dell'abbonamento: si esegue `claude` e poi `/login`. Non si usa mai una chiave API.
3. L'adattatore rimuove `ANTHROPIC_API_KEY` dall'ambiente del processo figlio, così la chiave non può essere preferita al login.
4. `dbe check-ai` segnala se manca il login. Il controllo è locale, senza chiamate di rete.
5. `CLAUDE_CODE_OAUTH_TOKEN`, ottenuto con `claude setup-token` anch'esso via OAuth, non si usa in fase 1. Serve solo in un container.
6. La prova di Claude Code nel container, prevista dall'ADR 004, diventa una nota scritta. La decisione sul container passa alla fase 2.
7. La decisione dipende dalla verifica delle condizioni d'uso dell'abbonamento, entro il 7/12/2026 (domanda aperta 13). Se le condizioni non lo permettono, si scrive un nuovo ADR.

## Alternative scartate

- **Claude Code in un'immagine non chiseled, con il token in `.env`**: allarga l'immagine pubblicata e mette un token in un container prima della verifica delle condizioni d'uso.
- **Servizio su Windows chiamato dal container**: anticipa lavoro della fase 2.

## Conseguenze

- L'estrazione dal vivo dipende dal PC acceso e dal login del proprietario.
- Estrazione e sviluppo condividono la Finestra dell'abbonamento. L'estrazione va pianificata in orari diversi dal lavoro degli agenti (ADR 004).
- Lo Studio in container, e la sua immagine, sono rimandati alla fase 2.
- Rischio: Claude Code non interattivo non è ancora provato. Non si conoscono ancora output strutturato, immagini e segnale di Finestra dell'abbonamento esaurita. Contromisura: la prova nell'ondata 1.1.
- Rischio: le condizioni d'uso dell'abbonamento non sono ancora verificate. Scadenza: 7/12/2026.

## Riferimenti

- `docs/fase-1-decisioni.md`, D5, la sezione «Rischi» e la sezione «Rimandate».
- ADR 004, punto 3 (configurazione) e conseguenze (container).
- `CLAUDE.md`, sezione 6 (variabili d'ambiente di Claude Code).
