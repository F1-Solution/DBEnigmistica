# Fase 1b - Prototipo di revisione della creazione - contesto

Scritto il 20/11/2026, dopo la chiusura dell'ondata 1.1 (unita in `main`, PR #31). Aggiornato lo stesso giorno con le decisioni D1b-1 - D1b-9 del proprietario (sezione 9; `docs/fase-1b-decisioni.md`). Fonte di verità: `ROADMAP.md`, sezione "### Fase 1b". Se questo file e il roadmap divergono, vince il roadmap, salvo le decisioni del proprietario prese dopo questa data e registrate in `docs/`. Il piano delle ondate è in `PLAN.md`, nella stessa cartella.

## 1. Obiettivo

Misurare, entro il 21/2/2027, i minuti mediani di revisione e la quota di candidati accettati per sei famiglie di Giochi, 5 candidati ciascuna: crittografia breve, anagramma a tema, rebus, cruciverba classico, sudoku e giochi logici (futoshiki e nonogramma). Il proprietario decide la soglia G1 il 28/2/2027. Con 5 candidati per famiglia la misura è indicativa (ROADMAP, fase 1b, "Obiettivo").

Soglie G1 (piano commerciale, sezione G1; ROADMAP, fase 1b): crittografia breve e anagramma ≤ 20 minuti, cruciverba ≤ 60 minuti, almeno il 50% dei candidati accettato. Rebus, sudoku e giochi logici: tempi misurati, senza soglia.

## 2. Perimetro

**Dentro** (ROADMAP, fase 1b, "Perimetro" e "Risultati consegnati")
- Generazione con il Fornitore AI di creazione (Claude Code con l'abbonamento, modello Opus, D1b-6) per crittografia breve (sinonimica e perifrastica, D1b-5), anagramma a tema, rebus e cruciverba: una richiesta per candidato, output strutturato conforme al descrittore. Il prompt contiene solo il brief e le regole approvate. Per il cruciverba il programma genera lo schema delle caselle nere e l'AI riempie parole e definizioni (D1b-4).
- Generazione algoritmica, senza AI, di sudoku, futoshiki (5x5-7x7) e nonogramma (10x10-15x15), con soluzione unica verificata e difficoltà stimata dalle tecniche di risoluzione.
- Rebus con vignette descritte a parole. Nessuna immagine generata o importata.
- Schermate "Nuovo candidato" e "Revisione", con cronometro del tempo attivo (pause oltre 2 minuti escluse), esiti *Accetta* o *Scarta* con motivo, differenze tra risposta dell'AI e versione finale.
- Schema `prototipo` nel database dello Studio. È nuovo. I candidati accettati restano lì, con Origine `originale` (D1b-2).
- Comando nuovo `dbe prototipo` dell'eseguibile win-x64: avvia l'host web su `127.0.0.1` del PC Windows (D1b-1).
- `dbe eval review-time` con mediana, quartili, quota accettata e confronto con le soglie G1. Il comando è nuovo e arriva nell'ondata 1b.2.
- Host web minimo `DBEnigmistica.Web` (nuovo) e ossatura React e TypeScript con Vite (nuova, ondata 1b.2). La fase 2 li estende.
- Pre-release `v0.2.0-prova.1` (D6); correzioni della 1b.3 come `v0.2.0-prova.N` (D1b-8).
- ADR 007, limitato all'host ASP.NET Core e all'ossatura React, scritto nell'ondata 1b.2 (D1b-9).

**Fuori**
- Lotti, validatori completi, registro completo del contributo umano, pubblicazione, autenticazione: fase 6 e seguenti.
- Profili `studio` e `pubblico`, passkey, involucro "Banco": fase 2.
- Immagini vere dei rebus: dipendono dal parere legale (domanda aperta 9, fase 7).
- Uso dei Giochi estratti come esempi: escluso (sezione 4).
- Tabelle dedicate per Tipologia in `catalogo` per i candidati del prototipo: il prototipo salva nello schema `prototipo`. L'eventuale importazione nel `catalogo` la decide la fase 6 (D1b-2).
- Accesso da altri dispositivi e prototipo in container: il prototipo gira solo su `127.0.0.1` del PC del proprietario (D1b-1).

## 3. Stato di partenza (verificato il 20/11/2026 su `main`, commit 6a31344)

- Fase 0 conclusa (tag `v0.1.1`). Ondata 1.1 conclusa il 20/11/2026 (`PROGRESS.md`: `PHASE:1 WAVE:1.1 STATUS:complete`).
- **Prova reale di Claude Code aperta**: il compito `1.1-prova` (`.planning/phases/1/prova-claude-code-1.1.md`) aspetta la verifica delle condizioni d'uso dell'abbonamento, entro il 6/12/2026 (domanda aperta 13; data fissata dal proprietario il 20/11/2026). Regola: nessuna estrazione o generazione dal vivo finché `PROGRESS.md` non contiene `PHASE:1 WAVE:1.1-prova STATUS:complete`.
- Le opzioni di Claude Code usate dall'adattatore sono provvisorie (prova, sezione 2). Si confermano con la prova.
- Non esistono ancora: il progetto `DBEnigmistica.Web`, lo schema `prototipo`, codice React, descrittori veri in `tipologie/` (solo `tipologie/_meta/`), il comando `eval`. I descrittori v1 di `cruciverba-classico` e `anagramma` arrivano con l'ondata 1.2, dopo la 1b.1.
- Il ruolo `dbe_creazione` esiste: scrive in `catalogo`, legge `tipologie`, aggiunge al `registro`, non ha permessi su `riviste` (migrazione `20261111120802_VincoliRuoliERegistro`).
- L'immagine Docker usa `runtime:10.0-noble-chiseled`: non contiene ASP.NET Core.
- CI: `ci.yml` con 4 lavori ("Build e test", "Vulnerabilita pacchetti", "Controllo segreti", "Build immagine Docker"). Soglia di copertura dell'80% su Domain e Application (`.github/scripts/soglia-copertura.py`).
- `.planning/` ora è tracciato da git. Il ramo dell'ondata è `fase-1b/ondata-1b.1`.

## 4. Vincoli (non negoziabili)

Da `CLAUDE.md`, sezione 5, e dagli ADR.

| Vincolo | Effetto sulla fase 1b | Fonte |
|---|---|---|
| Il Motore di creazione usa solo regole di formato, cioè le regole delle Tipologie approvate dal proprietario | Il prompt contiene solo brief e campo `regole` approvato. Nessuna generazione dal vivo con regole in bozza | `CLAUDE.md` 5; ADR 002, punto 3; ROADMAP 2.2 |
| I Giochi estratti non si usano mai come esempi senza parere legale favorevole e interruttore | Il codice di creazione non legge `riviste`. Test con stringhe sentinella (criterio 3). L'interruttore degli esempi non si costruisce in 1b | `CLAUDE.md` 5; ROADMAP 2.1 |
| I Giochi estratti non vanno mai pubblicati; blocco su tre livelli | Il prototipo non pubblica nulla e non tocca `riviste` | `CLAUDE.md` 5; ADR 001 |
| Ogni Gioco ha origine, diritti e registro del contributo umano; registro in sola aggiunta | I candidati hanno Origine `originale`. Gli eventi nel registro solo in aggiunta. Registro parziale: completo in fase 6 | `CLAUDE.md` 5; ADR 005 |
| Tabelle per Tipologia solo come migrazioni da rivedere, mai DDL al volo | Lo schema `prototipo` entra con una migrazione EF registrata e rivista in PR | `CLAUDE.md` 5; ADR 002 |
| Nessuna API a consumo come scelta predefinita; Claude Code con l'abbonamento solo nello Studio sul PC | Generazione con Claude Code sul PC del proprietario; nessun pacchetto `Anthropic*` | ADR 004; ADR 010; `docs/fase-0-decisioni.md`, Q3-Q5 |
| Mai una chiave API; `ANTHROPIC_API_KEY` tolta dall'ambiente del processo figlio | Vale anche per l'uso di creazione | D5; ADR 010 |
| Claude Code nel container rimandato alla fase 2 | Il prototipo gira nativo su Windows e chiama Claude Code come l'estrazione (D1b-1) | ADR 010 |
| Risposta grezza fuori dal registro immutabile, in una tabella cancellabile | Per la creazione: risposta grezza nello schema `prototipo`, cancellabile; nel registro solo metadati e SHA-256 | D7; ADR 011 (applicato per analogia) |
| Test e pubblicazione solo con GitHub Actions | CI verde prima di chiudere ogni ondata; pre-release dalla CI | `CLAUDE.md` 4 |
| Accessibilità del Sito (18-20 px, tocco 44 px, axe-core) | Non si applica al prototipo dello Studio. La fase 2 la applica all'involucro | `CLAUDE.md` 5 |

## 5. Decisioni già prese

| Decisione | Fonte |
|---|---|
| Fase 1b confermata; date del piano commerciale tenute; G1 il 28/2/2027 | Decisioni del 10/11/2026 (ROADMAP, sezione 7) |
| Sei famiglie, 5 candidati ciascuna, tutte nel prototipo della prova 1 | `docs/fase-0-decisioni.md`, Q1 e Q6 |
| Nessuna spesa API; abbonamento per la creazione | `docs/fase-0-decisioni.md`, Q3 |
| Giochi logici: futoshiki e nonogramma, oltre al sudoku | `docs/ondata-0-2-decisioni.md`, q1 (chiude la domanda aperta 12) |
| Rebus con immagini segnaposto descritte a parole | `docs/fase-0-decisioni.md`, "Rimandati" |
| Ordine delle ondate 1.1 → 1b.1 → 1.2 → 1b.2 → 1.3 → 1.4 con 1b.3; al massimo 3 agenti insieme; la 1b ha la priorità sulla calibrazione; prototipo verso il 10/1/2027, scadenza 17/1 | D1 (`docs/fase-1-decisioni.md`) |
| Un ramo per ondata con PR verso `main`; prototipo come pre-release `v0.2.0-prova.1` senza spostare `latest` | D6 |
| Estrazione e Claude Code sul PC Windows con login OAuth; container alla fase 2 | D5; ADR 010 |
| Risposta grezza in una tabella cancellabile, nel registro solo metadati e SHA-256 | D7; ADR 011 |
| Modello predefinito `claude-sonnet-5-5` (chiave `FornitoreAi:Modello`) | `docs/fase-0-chiusura-decisioni.md`, D4 |
| Interfaccia React e TypeScript con API ASP.NET Core | Decisioni del 10/11/2026 (ROADMAP, Sintesi 1) |
| PostgreSQL 18; codici d'uscita 2, 3, 4, 5 | `docs/fase-0-chiusura-decisioni.md`; ADR 006 |
| Libreria JSON Schema `Corvus.Text.Json.Validator` 5.8.12 | `.planning/phases/1/progetto-1.1.md`, sezione 1 |
| Nessuna generazione dal vivo prima di `PHASE:1 WAVE:1.1-prova STATUS:complete` | `.planning/phases/1/PLAN.md`, criteri di uscita 1.1; ROADMAP, fase 1, "Aperto" |
| Decisioni D1b-1 - D1b-9 | Sezione 9; `docs/fase-1b-decisioni.md` |
| Approvazione delle regole: il proprietario approva in chat; l'agente scrive data e versione dell'approvazione nel campo `regole` del descrittore, in un commit dedicato che il proprietario chiede. Nessun comando nuovo | Proprietario, 20/11/2026; `docs/fase-1b-decisioni.md` |
| Calendario: 1b.1 dal 23/11/2026; le ondate seguenti si anticipano; le scadenze esterne non cambiano (prototipo verso il 10/1/2027, scadenza 17/1; prova 1 18/1-21/2; G1 28/2); il tempo guadagnato diventa margine | Proprietario, 20/11/2026; `docs/fase-1b-decisioni.md` |
| Scelte tecniche confermate: descrittore `versione` 0 = prototipo (il meta-schema ammette 0, il generatore SQL della 1.2 lo salta); rebus v0 con resa `esposto_diagramma`; porta di creazione separata; risposte grezze nello schema `prototipo`, non in `riviste.chiamate_ai` | Proprietario, 20/11/2026; `PLAN.md`, sezione 2 |

## 6. Cosa si riusa dalla fase 1

**Dall'ondata 1.1 (già in `main`)**
- Porta `IFornitoreAiEstrazione` e i suoi decoratori (limite per esecuzione, Registra, Riproduci, registro delle chiamate, contatore) come modello per una porta di creazione parallela. Il contratto di estrazione non cambia (`.planning/phases/1/PLAN.md`, sezione 10).
- Adattatore `FornitoreClaudeCode`: avvio del processo con `ArgumentList`, prompt da stdin, timeout, retry, riconoscimento della Finestra dell'abbonamento esaurita, rimozione di `ANTHROPIC_API_KEY` e delle variabili affini. Per la creazione non serve l'immagine né lo strumento `Read`.
- Interprete dell'uscita di Claude Code e validatore JSON Schema (Corvus).
- Contratto `ChiamataAi` (riga di metadati) con il campo `Uso`, oggi solo `estrazione`. L'uso `creazione` è nuovo.
- Formato delle Registrazioni e degli eventi `chiamata_ai` (`progetto-1.1.md`, sezioni 6.2 e 7).
- Meta-schema dei descrittori, regole di derivazione dello JSON Schema (sezione 3.2) e mappa dei ruoli per la resa (sezione 3.3). Descrittori d'esempio in `tipologie/_meta/esempi/descrittore/`.
- Eseguibile finto di Claude Code per i test (`tests/DBEnigmistica.Tests/Fixtures/ClaudeFinto/`).
- `dbe check-ai` (login OAuth).

**Dall'ondata 1.2 (dopo la 1b.1, prima della 1b.2)**
- Descrittori v1 di `cruciverba-classico` e `anagramma`, con le regole approvate dal proprietario.
- Validatori strutturali `lunghezze-coerenti`, `incroci-coerenti`, `anagramma-stesse-lettere`, `numerazione-coerente-con-griglia`: base dei controlli leggeri della 1b.2.
- Caricatore dei descrittori e comando `eval` (1.2, agente C): la 1b.2 aggiunge `review-time`.

**Che cosa la 1b consegna alle fasi successive**
- Host `DBEnigmistica.Web` e ossatura React: la fase 2 li completa.
- Descrittori v0 di crittografia breve, rebus, sudoku, futoshiki, nonogramma: le fasi 4 e 5 li completano in v1.
- Cronometro e differenze: passano nella Postazione della fase 6.
- Generatori e risolutori di sudoku e giochi logici: riusati in fase 5 (estrazione) e fase 6 (creazione).

## 7. Dipendenze dal proprietario, con date

Date secondo il calendario anticipato del 20/11/2026 (sezione 5). Le scadenze esterne non cambiano.

| Cosa | Entro | Sblocca |
|---|---|---|
| Verifica delle condizioni d'uso dell'abbonamento (domanda aperta 13) | 6/12/2026 | `1.1-prova`, quindi ogni generazione dal vivo |
| Esecuzione della prova `1.1-prova` sul PC, con il login OAuth | dopo la verifica, al più tardi il 13/12/2026 | Prima generazione dal vivo nella 1b.2 |
| Approvazione in chat delle regole di `cruciverba-classico` e `anagramma` (con la fase 1); l'agente scrive data e versione nel descrittore, in un commit dedicato chiesto dal proprietario | fine ondata 1.2 (circa 20/12/2026) | Generazione dal vivo di cruciverba e anagrammi |
| Approvazione in chat delle regole v0 di crittografia breve, rebus, sudoku, futoshiki, nonogramma (bozze della 1b.1), con la stessa procedura | 3/1/2027 | Generazione dal vivo di crittografie e rebus; testi di gioco |
| Correzione dei brief tipo scritti in bozza da un agente Opus (circa 1-1,5 h, D1b-7) | 3/1/2027 | Generazione dei candidati della prova |
| Approvazione della PR di ogni ondata | fine di ogni ondata | Unione in `main` |
| Prova e collaudo del prototipo (circa 1,5 h) | entro il 10/1/2027, scadenza 17/1 | Pre-release `v0.2.0-prova.1` |
| Revisione anticipata di sudoku e giochi logici | prima metà di gennaio 2027 | Margine sulla prova 1 |
| Prova 1: 5 candidati per famiglia a cronometro (circa 14 h) | 18/1 - 21/2/2027 | Rapporto G1 |
| Decisione G1 | 28/2/2027 | Strategia commerciale |

Ore del proprietario: circa 7 di costruzione (meno 1,5-2 h con D1b-7) e 14 di prova 1 (ROADMAP, stime).

## 8. Rischi

Dal ROADMAP (fase 1b e rischi trasversali 2 e 6):
- *Cruciverba incoerente da una sola richiesta*: più di 10 incroci incoerenti per griglia nei primi 5 candidati. Già mitigato: lo schema delle caselle nere lo genera il programma da subito (D1b-4). Resta il rischio che l'AI riempia male gli incroci; il dato resta una misura del lavoro di revisione.
- *Prototipo non pronto il 17/1*: la 1b.2 non chiude entro il 10/1. Contromisura: le ondate della fase 1 cedono il posto alla 1b. Il calendario anticipato dà circa una settimana di margine in più.
- *Misura indicativa*: quartili molto distanti. Contromisura: G1 sulle tre famiglie con soglia lette insieme; conferma nelle fasi 6 e 7.
- *Prototipo più largo*: ore di costruzione oltre 7. Contromisura: sudoku e giochi logici rivisti a gennaio; se serve, il rebus esce dalla prova di febbraio (misura entro il 31/3/2027) e la 1.4 passa dopo il 28/2.
- *Contesa della Finestra dell'abbonamento*: generazioni interrotte durante le sessioni. Contromisura: candidati generati in anticipo, fuori dalle ore degli agenti. Con Opus (D1b-6) ogni chiamata consuma di più: le generazioni si fanno in lotti piccoli e fuori dallo sviluppo.

Rischi nuovi, dalla verifica del 20/11/2026:
- *`1.1-prova` in ritardo o negativa.* Se le condizioni d'uso non permettono l'uso non interattivo, la creazione con Claude Code non si può fare. Segnale: nessuna riga `1.1-prova` al 13/12/2026. Contromisura: l'agente principale porta la questione al proprietario (ADR 010, punto 7). Le alternative (adattatore API a spesa, altro fornitore) sono decisioni sue. La 1b.1 non dipende dalla prova.
- *Opzioni provvisorie di Claude Code.* L'adattatore di creazione usa le stesse opzioni non verificate dell'estrazione. Contromisura: allineamento dopo la prova, con i test sull'eseguibile finto.
- *Tempi dei test di proprietà.* 1.000 nonogrammi 15x15 con verifica dell'unicità possono pesare sulla CI. Segnale: il lavoro "Build e test" cresce di oltre 3 minuti. Contromisura: risolutore per righe prima della ricerca; semi fissi.
- *Conflitti con l'ondata 1.2.* La 1b.1 aggiunge una migrazione EF e cambia il minimo di `versione` nel meta-schema; la 1.2 rifà il nucleo `riviste` e scrive il generatore SQL. Contromisura: la 1b.1 si unisce in `main` prima che parta la 1.2, e l'agente principale comunica alla 1.2 che la versione 0 non genera SQL.

## 9. Decisioni prese il 20/11/2026

Decisioni del proprietario, tutte come raccomandato. Fonte: `docs/fase-1b-decisioni.md`.

| Id | Decisione | Perché | Opzioni scartate |
|---|---|---|---|
| D1b-1 | **A. Nativo su Windows.** Comando nuovo `dbe prototipo` dell'eseguibile win-x64 della CI: avvia l'host web su `127.0.0.1`. PostgreSQL nel Compose. Claude Code chiamato come nell'estrazione | Coerente con D5 e ADR 010; il criterio 4 resta com'è; l'immagine pubblicata non cambia | B (container per la revisione, generazione su Windows); C (container con Claude Code dentro) |
| D1b-2 | **A. Candidati accettati solo nello schema `prototipo`**, Origine `originale`. Nel registro solo eventi di metadati: chiamata di creazione con SHA-256 della risposta, esito della revisione. La fase 6 decide l'importazione nel `catalogo` | `catalogo` pulito finché il registro non è completo; traccia immutabile da subito | B (copia in `catalogo.giochi`); C (nessun evento nel registro) |
| D1b-3 | **A. Tutti i candidati validi per lo schema, in ordine di generazione**, senza scelta a mano. I non validi si rigenerano e si contano a parte | Misura il motore com'è; quota accettata confrontabile con le fasi 6 e 7 | B (scarto automatico per i controlli leggeri); C (scelta del proprietario) |
| D1b-4 | **A. Schema delle caselle nere dal programma da subito**; l'AI riempie parole e definizioni | Più vicino al motore della fase 6; con 5 candidati non c'è spazio per ripartire | B (contromisura solo dopo il segnale); C (due varianti) |
| D1b-5 | **B. Crittografia sinonimica e perifrastica** | I formati più strutturati; due sottotipi bastano su 5 candidati | A (solo sinonimica); C (tutte) |
| D1b-6 | **B. Opus per la creazione**, con una chiave di configurazione separata, nuova | Poche chiamate; la qualità pesa direttamente su G1 | A (stesso modello dell'estrazione); C (prova con entrambi) |
| D1b-7 | **B. Brief in bozza da un agente Opus dalle regole, corretti dal proprietario** (circa 1-1,5 h); pronti entro il 3/1/2027 | Risparmia circa 1,5 h; i brief restano del proprietario | A (li scrive il proprietario); C (brief liberi) |
| D1b-8 | **A. `v0.2.0-prova.N` fino a `v0.2.0`, poi `v0.2.1` e seguenti.** Chiude anche q8 della fase 1 | Non sposta `latest` prima della chiusura della fase 1 | B (`v0.2.1` subito, come D6) |
| D1b-9 | **A. ADR 007 nell'ondata 1b.2**, limitato all'host ASP.NET Core e all'ossatura React | Il codice che la fase 2 eredita nasce nella 1b | B (ADR 007 in fase 2) |

Altre decisioni dello stesso giorno: verifica delle condizioni d'uso entro il 6/12/2026; procedura di approvazione delle regole e calendario anticipato (sezione 5); scelte tecniche confermate (sezione 5 e `PLAN.md`, sezione 2).

Non restano decisioni aperte del proprietario per la fase 1b, salvo la verifica delle condizioni d'uso (domanda aperta 13) e la decisione G1 del 28/2/2027. Non erano domande per il proprietario, perché già decise o tecniche: anagramma a tema come Tipologia `anagramma` con il tema nel brief (ROADMAP, fase 1b, "Riuso"); dimensioni di futoshiki e nonogramma (ROADMAP); pause oltre 2 minuti (ROADMAP); ruolo del database usato dall'host (`PLAN.md`, sezione 2).
