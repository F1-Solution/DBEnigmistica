# ADR 002: descrittore di Tipologia e Schema dinamico come migrazioni da rivedere

- **Stato:** Proposto, in attesa dell'approvazione del proprietario
- **Data:** 11/11/2026

## Contesto

Ogni Tipologia (per esempio `cruciverba-classico`, `anagramma`, `rebus`) ha campi diversi. Il codice precedente aveva un enum `GameType` chiuso, quindi ogni nuova Tipologia richiedeva una modifica al programma, e creava le tabelle a runtime con SQL composto da stringhe. Quel percorso era vulnerabile a SQL injection e non era rivedibile. Nella fase 0 il DDL a runtime è stato eliminato (`ROADMAP.md`, sezione 3, fase 0). Il glossario di `CLAUDE.md` chiama lo Schema dinamico il modo in cui una Tipologia ottiene tabelle proprie, generate come migrazioni da rivedere.

## Decisione

1. **Il descrittore è un file versionato**: `tipologie/<id>/v<n>.json`. Per le Tipologie di base sta nel repository; per quelle confermate nello Studio sta nel database, nello schema `tipologie`.
2. **Dal descrittore derivano tre cose**: la migrazione SQL delle tabelle dedicate, lo JSON Schema dell'output strutturato dell'AI, la resa nel Lettore. Il descrittore minimo contiene `id`, `versione`, `famiglia`, `regole`, `campi`, `elenchi` (che diventano tabelle figlie), `resa` e `validatori`.
3. **Regole approvate**: il campo `regole` è scritto in bozza da un agente e approvato dal proprietario, con data e versione. È l'unica conoscenza delle Tipologie che il Motore di creazione riceve, finché l'interruttore degli esempi è spento.
4. **Tipi ammessi (lista chiusa)**, con il tipo SQL unico per ciascuno (`PLAN.md`, sezione 1.10):

   | Tipo del descrittore | Tipo SQL |
   |---|---|
   | `testo` con `max` (1-4000) | `varchar(max)` |
   | `testo_lungo` | `text` |
   | `intero` | `integer` |
   | `decimale` | `numeric(12,4)` |
   | `booleano` | `boolean` |
   | `data` | `timestamptz` |
   | `enumerazione` | `text` con `CHECK (col IN (...))`, valori con regex `^[a-z0-9_]{1,40}$` |
   | `griglia` | `jsonb` |
   | `immagine` | `uuid` con chiave esterna verso i ritagli (fase 1) |

5. **Identificatori**: regex `^[a-z][a-z0-9_]{0,40}$`, esclusa la lista delle parole riservate di PostgreSQL, sempre quotati.
6. **Indici ammessi, e solo questi**: chiave primaria; `UNIQUE (gioco_id)` nella tabella 1:1; btree sulle chiavi esterne delle tabelle figlie; btree sui campi con `"indice": true` e tipo in `intero`, `testo`, `enumerazione`, `data`.
7. **Nessun `DEFAULT`, nessun trigger, nessuna funzione.** `NOT NULL` solo da `obbligatorio: true`.
8. **Permessi ereditati** dai privilegi predefiniti dello schema (ADR 001). Il generatore non produce mai `GRANT`.
9. **Il generatore non accetta stringhe SQL** da descrittori, dall'AI o dalla configurazione. Tipi di dato e indici si scelgono solo dall'elenco fisso.
10. **Migrazioni**: per le Tipologie di base il generatore le produce in fase di sviluppo e le registra nel repository, con pull request. Per quelle confermate nello Studio vengono salvate come *in attesa* e si applicano solo con un comando esplicito (`dbe migrate` o `dbe types apply <id>`). Nessun DDL viene eseguito al volo.
11. **Modelli di resa** del Lettore: `griglia` (cruciverba, cruciverba crittografato), `esposto_diagramma` (crittografie, giochi brevi), `immagine_diagramma` (rebus), `griglia_logica` (sudoku e giochi logici). Un formato nuovo riusa un modello esistente e cambia solo i dati.

## Conseguenze

- Il generatore arriva nella fase 1. Fino ad allora non esiste alcuna tabella per Tipologia. `analyze-game` resta disattivato fino alla fase 4 e risponde "Comando disponibile dalla fase 4" con codice 2.
- Ogni migrazione generata si rivede a mano prima di essere registrata. La revisione è il controllo principale contro i tipi e gli identificatori non voluti.
- La CI verificherà, dalla fase 1, che gli SQL registrati coincidano con quelli generati (`dbe types generate --check`) e che un test a campi casuali, con payload di iniezione, non esegua istruzioni estranee.
- Per le Tipologie confermate nello Studio non richiede più una release del programma, ma una migrazione da approvare; quelle di base passano da una pull request.
- `tipologie` contiene i descrittori e le loro versioni; il Motore di creazione lo legge solo in lettura.

## Alternative scartate

- **Enum `GameType` chiuso nel codice**: ogni nuova Tipologia richiede una modifica al programma. Il roadmap la sostituisce con un dato versionato.
- **DDL composto a runtime**: scartato per SQL injection e per l'impossibilità di rivederlo prima dell'esecuzione (`ROADMAP.md`, sezione 3, fase 0).
- **Tipi SQL e indici liberi, scelti dal descrittore**: scartati. Il generatore li prende solo dall'elenco fisso (regola fissata nell'ondata 0.2).

## Riferimenti

- `ROADMAP.md`, sezione 2.2 (il descrittore di Tipologia) e sezione 3, fase 0 (eliminazione del DDL a runtime).
- `.planning/phases/0/PLAN.md`, sezione 1.10 (regola per il generatore di migrazioni).
- `CLAUDE.md`, sezioni 2 e 5 (Schema dinamico; tabelle per Tipologia come migrazioni da rivedere).
- Requisiti: DB-05, DB-09, DB-12 (fase 1 del roadmap), OUT-14, OUT-17.
