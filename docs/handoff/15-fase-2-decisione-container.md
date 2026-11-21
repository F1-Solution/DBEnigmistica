# 15 - Fase 2: decisione sul container per Claude Code

**Stato:** Da fare (dipende dal file 01, domanda d)
**Fase:** 2 (ADR 010)

> **Da dettagliare prima della fase 2.** Le opzioni sono quelle della ROADMAP e del documento di prova. La scelta si fa con l'agente tramite la skill `grill-with-ui`.

## Scopo

Decidere se Claude Code gira nel container dello Studio. La decisione dipende dalle condizioni d'uso dell'abbonamento per l'uso in container.

## Scadenza

Entro la chiusura della fase 2, stimata al **21 febbraio 2027**.

## Tempo stimato

Incluso nelle circa 6 ore della fase 2 (file 14).

## Prerequisiti

- File 01, domanda d, con una risposta chiara.

## Opzioni

- **A. Claude Code nel container.** Richiede un token dell'abbonamento, ottenuto con `claude setup-token`. Il token dura un anno. Va passato come segreto di Compose, mai in un file tracciato. Richiede la conferma delle condizioni.
- **B. Nessun container per Claude Code.** L'estrazione resta sul PC.
- **C. Lo Studio in container chiama `dbe` sul PC Windows.** È l'opzione C della decisione D5.

AVVERTENZA: Il token dell'abbonamento è un segreto. Non scriverlo in `.env`, in un file del repository, nei log o nell'immagine. Scrivi solo il nome della variabile: `CLAUDE_CODE_OAUTH_TOKEN`.

## Procedura (struttura prevista)

1. Leggi la risposta del file 01, domanda d.
2. Chiedi all'agente il confronto tra le tre opzioni, con i rischi di `prova-claude-code-1.1.md`, sezione 4.
3. Scegli un'opzione. Decidi con la skill `grill-with-ui`.
4. Comunica la scelta all'agente. Passo da dettagliare.

## Verifica finale

- La scelta è scritta in un ADR con data.

## Cosa consegnare all'agente principale

1. Opzione scelta: A, B o C.
2. Data della decisione.

## Cosa sblocca

- Configurazione del container dello Studio (fase 2).

---
Fonti: `ROADMAP.md`, sezione 2.4 ("Container, rimandato alla fase 2"), fase 2 ("Decisione rinviata"), sezione 7 (domanda 13); `.planning/phases/1/prova-claude-code-1.1.md`, sezione 4 (nota sul container); `docs/adr/010-estrazione-dal-vivo-con-claude-code.md`.
