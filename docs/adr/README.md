# Decisioni di architettura (ADR)

Ogni file registra una decisione con il suo contesto e le sue conseguenze. Le decisioni sono tratte dal roadmap (`ROADMAP.md`, sezione 2) e dai documenti di intervista dell'ondata 0.2. Un ADR non si riscrive: se una decisione cambia, se ne scrive uno nuovo che sostituisce il precedente.

| N. | Titolo | Stato | Data |
|---|---|---|---|
| 001 | [Superfici, profili e separazione dei dati](001-superfici-profili-e-dati.md) | Accettato | 13/11/2026 |
| 002 | [Descrittore di Tipologia e Schema dinamico come migrazioni da rivedere](002-descrittore-di-tipologia-e-schema-dinamico.md) | Accettato | 13/11/2026 |
| 003 | [Lettura delle pagine con visione, senza OCR](003-lettura-delle-pagine-con-visione.md) | Accettato | 13/11/2026 |
| 004 | [Fornitore AI dietro un'unica interfaccia](004-fornitore-ai.md) | Accettato | 13/11/2026 |
| 005 | [Provenienza, diritti e registro in sola aggiunta](005-provenienza-diritti-e-registro.md) | Accettato | 13/11/2026 |
| 006 | [Stack e riga di comando](006-stack-e-cli.md) | Accettato | 13/11/2026 |
| 008 | [Set di riferimento e Registrazioni solo sul PC del proprietario](008-set-di-riferimento-solo-sul-pc.md) | Accettato | 19/11/2026 |
| 009 | [Rendering delle pagine con PDFium, PNG codificato dal progetto](009-rendering-con-pdfium.md) | Accettato | 19/11/2026 |
| 010 | [Estrazione dal vivo su Windows con Claude Code e login OAuth](010-estrazione-dal-vivo-con-claude-code.md) | Accettato | 19/11/2026 |
| 011 | [Risposta grezza del Fornitore AI fuori dal registro immutabile](011-risposta-grezza-fuori-dal-registro.md) | Accettato | 19/11/2026 |

Relazioni con gli ADR precedenti:

- **009** sostituisce il punto 1 dell'ADR 003 (la libreria di rendering). Il resto dell'ADR 003 resta valido.
- **010** integra l'ADR 004 (autenticazione di Claude Code in fase 1 e container rimandato alla fase 2).
- **011** integra l'ADR 005 (il registro contiene solo metadati e impronte delle chiamate al Fornitore AI).

Stato degli ADR 001-006: **accettati dal proprietario il 13/11/2026**, con la chiusura della fase 0. Stato degli ADR 008-011: **accettati dal proprietario il 19/11/2026**, con le decisioni D2, D3, D5 e D7 della fase 1 (`docs/fase-1-decisioni.md`). Un ADR si sostituisce con uno nuovo, non si riscrive.

Il numero 007 è riservato al primo ADR di interfaccia (React e TypeScript con API ASP.NET Core, e involucro Tauri), previsto dalla fase 2. Non è ancora stato scritto.
