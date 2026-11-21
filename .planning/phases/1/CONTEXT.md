# Fase 1 - Motore di estrazione: cruciverba e giochi brevi - contesto

Scritto il 19/11/2026, dopo la chiusura della fase 0 (tag `v0.1.1`). Aggiornato lo stesso giorno con le decisioni D1-D7 del proprietario (`docs/fase-1-decisioni.md`, ADR 008-011). Fonte di verità: `ROADMAP.md`, sezione "### Fase 1". Se questo file e il roadmap divergono, vince il roadmap, salvo le decisioni del proprietario prese dopo questa data e registrate in `docs/`.

## 1. Obiettivo

Dalla riga di comando, un PDF o una cartella di Riviste diventano Giochi salvati nelle tabelle dedicate di quattro Tipologie: `cruciverba-classico`, `anagramma`, `sciarada`, `cambio-di-lettera`. L'accuratezza si misura sul Set di riferimento, sul PC del proprietario. Il Fornitore AI predefinito per l'estrazione si sceglie per misura (ROADMAP, fase 1, criteri 1-8). La fase si chiude con `v0.2.0` (D6), stimata il 21/2/2027 (D1).

## 2. Perimetro

**Dentro**
- Progetto `DBEnigmistica.Tipologie`: meta-schema dei descrittori, descrittori v1 delle 4 Tipologie, generatore SQL a lista chiusa, derivazione dello JSON Schema per l'output dell'AI.
- `dbe types generate [--check]` e `dbe migrate` esteso alle migrazioni delle Tipologie (con checksum).
- Scrittore e lettore parametrizzati delle tabelle per Tipologia (Npgsql, identificatori solo dal descrittore validato).
- Rendering delle pagine con PDFiumCore e la libreria nativa di `bblanchon.PDFium.*`, PNG codificato dal progetto (D3, ADR 009). Testo nativo con PdfPig. Impronte SHA-256. Archivio delle Riviste e delle immagini delle pagine nella cartella `Archivio:Cartella` (D4).
- Porte del PDF e del Fornitore AI spostate da Domain ad Application (ADR 004). Configurazione del Fornitore AI spostata da Console al progetto AI.
- Adattatore Claude Code non interattivo con login OAuth su Windows (D5, ADR 010): output verificato, retry, finestra esaurita, limite per esecuzione, `ANTHROPIC_API_KEY` tolta dall'ambiente del processo figlio. Modalità registra e riproduci. Adattatori dei piani gratuiti scelti e del modello aperto locale (ondata 1.4).
- Pipeline per pagina con una transazione per pagina, idempotenza e ripresa. `dbe scan-file`, `dbe scan-folder [--recursive]`. Metadati della Rivista proposti dal modello e sovrascrivibili.
- Tabella cancellabile `riviste.chiamate_ai` con la risposta grezza. Nel registro solo metadati e SHA-256 della risposta (D7, ADR 011). Limite di pagine per esecuzione. Tetto di spesa solo con l'adattatore API. `dbe costs`.
- `dbe eval draft`, `dbe eval extraction [--replay|--live] [--fornitore <id>]`, metriche e rapporto JSON e leggibile. Set sintetico nel repository e in CI. Script locale di regressione sul Set di riferimento, con rapporto da allegare alla release (D2, ADR 008). Confronto dei Fornitori AI con le pagine lette per Finestra dell'abbonamento.
- Rifattorizzazione del nucleo `riviste`: Tipologia come dato al posto dell'enum `GameType`, riquadro normalizzato tra 0 e 1, impronte su Rivista e Pagina, eliminazione delle entità e tabelle di dettaglio cablate.
- CI: coerenza descrittori e SQL, fuzz test del generatore, squawk, migrazioni su Testcontainers, regressione riprodotta sul Set sintetico, rendering con PDFium nell'immagine chiseled.

**Fuori**
- Crittografie, giochi a chiave, flusso "tipologia nuova, proposta, conferma" (`dbe types apply`): fase 4. Qui un Gioco di Tipologia non prevista si salva come "nuova tipologia proposta" e basta (criterio 7).
- Rebus, sudoku, giochi logici nell'estrazione: fase 5. `analyze-game` resta "disponibile dalla fase 4" (ADR 002).
- Seconda chiamata sul ritaglio ingrandito: si costruisce solo se la misura dell'ondata 1.4 la giustifica (ADR 003, punto 5).
- Interfaccia web, profili `studio`/`pubblico`, Studio in container, Claude Code in container: fase 2 (ADR 010). La fase 1b anticipa solo l'host web minimo del prototipo.
- Set di riferimento e Registrazioni su GitHub, repository `DBEnigmistica-refset`, runner self-hosted: esclusi (D2, ADR 008).
- Adattatore API a consumo: non si costruisce in fase 1 salvo nuova decisione del proprietario.
- Lavori per la fase 1b (prototipo di creazione): pianificati in `.planning/phases/1b/`, non qui.
- Soluzioni collegate automaticamente: fase 4.

## 3. Punto di partenza reale del codice (verificato il 19/11/2026)

Verificato su `main` a `ad670dd` (tag `v0.1.0` e `v0.1.1` presenti). Diversi riferimenti di riga della tabella "Riuso, riscrittura, eliminazione" della fase 1 nel ROADMAP sono superati:

| Riferimento nel ROADMAP | Stato reale |
|---|---|
| `PdfService.cs:102-111` | Il file ha 93 righe. Usa PdfPig 0.1.13 per testo e conteggio. `RenderPageAsync` lancia `NotSupportedException("disponibile dalla fase 1")` alla riga 75. Nessun rendering, nessuna impronta. |
| `ClaudeApiClient.cs:29`, `:50-53`, `:124-144` | **File eliminato in fase 0** (ondata 0.2, con `Anthropic.SDK`). L'adattatore nasce da zero. |
| `ClaudeGameAnalyzer.cs:26`, `:176-214` | **File eliminato in fase 0.** |
| `MagazineProcessingService.cs:180-187`, `:213-214`, `:279` | **File eliminato in fase 0.** Il ciclo per pagina si scrive da zero nell'ondata 1.3. |
| `PromptTemplates.cs:18` | Esiste ancora (`src/DBEnigmistica.AI/Prompts/PromptTemplates.cs`), con campi cablati. Da sostituire con prompt costruiti dai descrittori. Restano anche i modelli `src/DBEnigmistica.AI/Models/Claude*Response.cs`, legati al vecchio flusso. |
| `GameExtractionService.cs:44-45`, `:76-90` | Esiste. La conversione nell'enum `GameType` è alla riga 45 (scarta i tipi non riconosciuti). La chiamata di estrazione dei dati è alle righe 78-91 e il risultato non viene salvato. Lavora su "regioni" e "testo OCR", modello superato. Usa `IAiGameAnalyzer`, non `IPdfService`. |
| `CrosswordGame.cs`, `AnagramGame.cs`, `LogicGame.cs` | Esistono, con le tabelle `riviste.crossword_games`, `anagram_games`, `logic_games`. Esiste anche `RebusGame` (`riviste.rebus_games`), non citato dal ROADMAP: va eliminato con gli altri (il rebus torna come descrittore in fase 5). |

Altri fatti che vincolano il lavoro:
- **Domain**: `Game` ha `Id` intero, enum `GameType`, riquadro in pixel interi (`PositionX`, `PositionY`, `BoundingBoxWidth`, `BoundingBoxHeight`), `ClassificationConfidence`, `Origine` obbligatoria (solo `Rivista`, vincolo `ck_games_origine_rivista`). `Page` ha `OcrText`, `ImagePath`, `Width`, `Height`, nessuna impronta. `Magazine` ha `SourceFilePath`, nessuna impronta. La rifattorizzazione "tipologia come dato, riquadro normalizzato" prevista in fase 0 è stata fatta solo per l'Origine.
- **Porte**: `IAiGameAnalyzer` e `IPdfService` stanno in `Domain/Interfaces`, non nell'Application layer come vuole l'ADR 004. `IPdfService` è usato solo da `PdfService` e dalla registrazione in `Infrastructure/Configuration/DependencyInjection.cs`. `IAiGameAnalyzer` è usato da `GameExtractionService`, `SchemaProposalService` (resta fino alla fase 4), dal segnaposto `FornitoreAiNonDisponibile` e da tre file di test. I tipi di risultato sono in `Domain/AI`.
- **Progetto AI**: referenzia solo Domain, non Application. Con le porte in Application servirà il riferimento ad Application. Pacchetti: solo le astrazioni di configurazione, DI e log.
- **Configurazione AI**: `FornitoreAiOptions`, il validatore e `IAmbienteFornitoreAi` stanno nel progetto Console (`Console/Configuration/FornitoreAiOptions.cs`) e si registrano in `Cli/CliHost.cs` (righe 116-119). Tipi ammessi: `claude-code`, `api-anthropic`. `appsettings.json`: `claude-code`, `claude-sonnet-5-5`, eseguibile `claude`. Il validatore considera autenticato Claude Code se c'è `CLAUDE_CODE_OAUTH_TOKEN` oppure `~/.claude/.credentials.json`. Non guarda `ANTHROPIC_API_KEY`. `dbe check-ai` esiste e usa `HostRequirements.Ai`.
- **CLI**: `scan-file` e `scan-folder` rispondono "Comando disponibile dalla fase 1" con codice 2 (`NotAvailableAction`). `ExitCodes.LimitReached = 4` esiste già. `migrate` applica solo le migrazioni EF.
- **Database**: baseline `20261111120801_Baseline` più `20261111120802_VincoliRuoliERegistro`. Schemi `riviste`, `catalogo`, `tipologie`, `registro`. Ruoli `dbe_estrazione`, `dbe_creazione`, `dbe_originalita`, `dbe_consegna`. `tipologie.game_type_schemas` è un residuo del vecchio Schema dinamico. `catalogo.giochi` ha già `tipologia` e `versione_tipologia`.
- **Registro**: `NuovoEvento(avvenutoIl, tipo, entita, entitaId, autore, dati)` con `dati` testuale nell'impronta concatenata. Nessun tipo di evento per le chiamate AI: arriva con la 1.3 (D7).
- **Pacchetti** (`Directory.Packages.props`): nessun pacchetto PDFium, SkiaSharp, JSON Schema, `Microsoft.Extensions.Options` o di fornitore AI. Il test di architettura `PacchettiTests` vieta pacchetti `Anthropic*` ovunque. `DipendenzeTests` vieta ad Application di dipendere da AI, Infrastructure e Console.
- **Docker**: `Dockerfile` su `runtime:10.0-noble-chiseled`, pubblicazione senza `--runtime` e con `UseAppHost=false`. Un passo `RUN` fa fallire la build se la pubblicazione contiene librerie native (`*.so`, `*.so.*`, `*.dylib`, `runtimes/*/native`). Va sostituito con un elenco ammesso (ADR 009). Il commento finale cita `CLAUDE_CODE_OAUTH_TOKEN`, che in fase 1 non si usa (ADR 010).
- **CI**: `ci.yml` con 4 lavori: "Build e test" (con `dotnet format`, `-warnaserror`, soglia di copertura 80% con `.github/scripts/soglia-copertura.py`), "Vulnerabilita pacchetti", "Controllo segreti", "Build immagine Docker". Il lavoro Docker oggi esegue solo `docker run --rm dbe:ci --version`.
- **Pubblicazione**: `publish.yml` parte sui tag `vX.Y.Z` e `vX.Y.Z-suffisso`. Le pre-release non spostano `latest`. Il lavoro "Eseguibile console" pubblica `win-x64` e `linux-x64` autonomi su Ubuntu e controlla solo `appsettings.json` e l'eseguibile. La release carica solo gli archivi: il rapporto della regressione locale si allega a mano (D2).
- **Repository**: `.gitignore` esclude `*.pdf` e `.planning/`. I file di questa cartella non sono tracciati da git.

## 4. Decisioni già prese che vincolano la fase

| Decisione | Fonte |
|---|---|
| Lettura con immagine della pagina più testo nativo, una chiamata per pagina, output strutturato verificato dal programma; niente OCR | ADR 003; ROADMAP 2.3 |
| Rendering con PDFiumCore 157.0.8100 per i collegamenti e `bblanchon.PDFium.*` 160.0.8165 per la libreria nativa, PNG codificato dal progetto con test; da verificare nella 1.1 l'immagine chiseled linux-x64 e i font non incorporati | D3; ADR 009 (sostituisce ADR 003, punto 1) |
| 150-200 DPI configurabili; impronte SHA-256 per l'idempotenza; una transazione per pagina | ADR 003, punti 2 e 7 |
| Archivio: copia della Rivista con nome uguale alla sua impronta, PNG delle pagine in cartelle per impronta, radice `Archivio:Cartella`, testo nativo nel database; la cartella va nel backup | D4 |
| Seconda chiamata sul ritaglio disattivata finché la misura non la giustifica | ADR 003, punto 5 |
| Fornitore AI dietro un'unica interfaccia nell'Application layer; adattatori nell'anello esterno; Claude Code non interattivo con l'abbonamento come riferimento, solo nello Studio; API a consumo possibile ma non predefinita | ADR 004; `docs/fase-0-decisioni.md`, Q3-Q5 |
| Estrazione dal vivo in fase 1 sul PC Windows con `dbe` win-x64 della CI e Claude Code con login OAuth (`claude`, poi `/login`); mai una chiave API; `ANTHROPIC_API_KEY` tolta dall'ambiente del processo figlio; `check-ai` segnala il login mancante; `CLAUDE_CODE_OAUTH_TOKEN` solo per un container, non usato in fase 1; container rimandato alla fase 2 | D5; ADR 010 (integra ADR 004) |
| Risposta grezza in `riviste.chiamate_ai`, cancellabile; nel registro solo fornitore, modello, durata, esito, token e costo se disponibili, più SHA-256 della risposta | D7; ADR 011 (integra ADR 005) |
| Set di riferimento, risultati attesi e Registrazioni solo sul PC del proprietario, mai su GitHub; CI sul Set sintetico; regressione sul Set di riferimento con uno script locale prima di ogni release, rapporto allegato; criterio 3 verificato in locale | D2; ADR 008 |
| Regola di scelta: predefinito il fornitore che supera le soglie al costo minore, tra quelli con condizioni d'uso verificate dal proprietario; i piani gratuiti ricevono pagine solo dopo la verifica | ADR 004, punto 6; `docs/fase-0-decisioni.md`, Q5 |
| Modello predefinito `claude-sonnet-5-5`; confronto tra modelli alla linea di base della fase 1 | `docs/fase-0-chiusura-decisioni.md`, D4 |
| Descrittore versionato `tipologie/<id>/v<n>.json`; tipi SQL, identificatori e indici da lista chiusa; niente DEFAULT, trigger, funzioni, GRANT; il generatore non accetta stringhe SQL; migrazioni delle Tipologie di base registrate nel repository con PR | ADR 002 |
| Campo `regole` scritto in bozza da un agente e approvato dal proprietario con data e versione | ADR 002, punto 3 |
| Giochi estratti con Origine `rivista`, mai pubblicabili; scritti solo dal ruolo `dbe_estrazione` in `riviste` | ADR 001, ADR 005 |
| Registro in sola aggiunta con impronta concatenata; nessuna modifica né cancellazione | ADR 005; `CLAUDE.md` sezione 5 |
| Studio sul PC Windows del proprietario, Docker Desktop, solo rete di casa; Riviste non su macchine di terzi in attesa del parere legale | `docs/fase-0-decisioni.md`, Q2; ADR 001 |
| PostgreSQL 18 per test e Compose | `docs/fase-0-chiusura-decisioni.md`, D3 |
| Codici d'uscita: 2 non disponibile, 3 configurazione, 4 limite raggiunto, 5 registro alterato | ADR 006; `ExitCodes.cs` |
| Librerie native nell'immagine solo da un elenco ammesso (la libreria nativa di PDFium per linux-x64) | ADR 006, punto 9; ADR 009 |
| Test e pubblicazione solo con GitHub Actions, con l'unica eccezione della regressione sul Set di riferimento; protezione di `main` rimandata | `CLAUDE.md` sezione 4; ADR 008; `docs/fase-0-chiusura-decisioni.md`, D1 |
| Ordine delle ondate 1.1, 1b.1, 1.2, 1b.2, 1.3, poi 1.4 accanto alla 1b.3; 1.1 dal 23/11/2026; al massimo 3 agenti insieme; la 1b ha la priorità sulla calibrazione; prototipo verso il 10/1/2027 (scadenza 17/1); chiusura della fase 1 il 21/2/2027 | D1 |
| Calendario anticipato dopo la chiusura della 1.1 il 20/11/2026: 1b.1 23/11-6/12, 1.2 7/12-20/12, 1b.2 21/12-3/1, 1.3 4/1-17/1, 1.4 dal 18/1 accanto alla 1b.3; scadenze esterne invariate | Proprietario, 20/11/2026 (`docs/fase-1b-decisioni.md`) |
| Il meta-schema ammette `versione` 0 per i descrittori di prototipo della 1b; il generatore SQL e `dbe types generate` li saltano | Proprietario, 20/11/2026 (`.planning/phases/1b/PLAN.md`, sezione 2) |
| Correzioni della 1b.3 come `v0.2.0-prova.N` fino a `v0.2.0`, poi `v0.2.1` e seguenti | D1b-8 (`.planning/phases/1b/CONTEXT.md`, sezione 9) |
| Un ramo per ondata con PR verso `main`; prototipo della 1b come pre-release `v0.2.0-prova.1` senza spostare `latest`; fase 1 chiusa con `v0.2.0`; correzioni della 1b.3 come `v0.2.1` e seguenti | D6 |
| Libreria di validazione JSON Schema scelta nella 1.1 dopo la verifica delle licenze | `docs/fase-1-decisioni.md`, "Rimandate" |

## 5. Dipendenze dal proprietario, con date

| Cosa | Entro | Sblocca | Note |
|---|---|---|---|
| Verifica delle condizioni d'uso dell'abbonamento Claude per l'uso non interattivo di Claude Code (domanda aperta 13) | 6/12/2026; prova `1.1-prova` entro il 13/12/2026 | Prova reale dell'adattatore Claude Code (compito `1.1-prova`, aperto dopo la chiusura della 1.1 il 20/11/2026) e ogni estrazione o generazione dal vivo. Se le condizioni non lo permettono, nuovo ADR (ADR 010, punto 7) | Fino ad allora l'adattatore si prova con un processo finto |
| Login OAuth di Claude Code sul PC (`claude`, poi `/login`) e nessuna `ANTHROPIC_API_KEY` nell'ambiente utente | prima della prova reale | Prova reale | Guida passo passo a cura dell'agente principale |
| Set di PDF di prova: almeno 4 testate, almeno 5 pagine da scansioni, 15-20 pagine con cruciverba e giochi brevi (domanda aperta 14) | 7/12/2026 | Bozze dei risultati attesi, misura delle pagine per finestra (1.3), ondata 1.4 | Restano sul PC (D2). Se arrivano in ritardo, la 1.4 slitta; le ondate 1.1-1.3 no |
| Cartella del Set di riferimento e delle Registrazioni sul PC, inclusa nel backup insieme all'Archivio | prima della fine dell'ondata 1.2 | Script locale di regressione (criterio 3 in locale) | D2, D4 |
| Approvazione delle regole delle 4 Tipologie (circa 1 h) | fine ondata 1.2 | Prompt di estrazione definitivi, prompt di creazione della 1b | Le stesse regole di `cruciverba-classico` e `anagramma` servono alla 1b.2 |
| Correzione dei risultati attesi con `dbe eval draft` (8-10 h) | prima dell'ondata 1.4 (circa 18/1/2027) | Linea di base, criterio 5 | Se supera 30 minuti per pagina: si parte con 10 pagine (rischio del ROADMAP) |
| Verifica delle condizioni dei piani gratuiti candidati, compreso l'uso dei dati per addestramento | prima dell'ondata 1.4 | Invio di pagine ai piani gratuiti | Anche tra le domande al legale (entro il 15/12/2026) |
| Installazione del modello aperto locale, se la scheda grafica lo consente (circa 1 h) | prima dell'ondata 1.4 | Confronto con il modello locale | |
| Esecuzione dello script di regressione locale e allegato del rapporto a ogni release | prima di ogni tag | Release (D2) | La checklist di release lo richiede |
| Approvazione di ogni PR di ondata e dei tag `v0.2.0-prova.1` (1b) e `v0.2.0` | fine di ogni ondata; 21/2/2027 | Unione in `main`, chiusura | D6 |
| Scelta del fornitore predefinito sul rapporto di confronto; conferma delle soglie dopo la linea di base (domanda aperta 6) | fine fase 1 | Chiusura della fase, ADR del fornitore predefinito | |

## 6. Rapporto con la fase 1b

- Ordine (D1): 1.1 (chiusa il 20/11/2026) → 1b.1 (23/11-6/12) → 1.2 (7/12-20/12) → 1b.2 (21/12-3/1) → 1.3 (4/1-17/1) → 1.4 (dal 18/1), con la 1b.3 durante la prova 1 (18/1-21/2/2027). Mai più di 3 agenti insieme: la 1.4 (2 agenti) gira accanto alla 1b.3 (1 agente).
- La 1b.1 riusa dalla 1.1 l'interfaccia del Fornitore AI, l'adattatore Claude Code e la forma dei descrittori del meta-schema. La 1b.2 riusa i descrittori `cruciverba-classico` e `anagramma` v1 e le regole approvate della 1.2.
- La 1b ha la priorità sulla calibrazione. Il prototipo è pronto verso il 10/1/2027, con scadenza il 17/1 e G1 il 28/2/2027. Se la 1b.2 non chiude entro il 10/1, la 1.3 aspetta.
- Contesa della Finestra dell'abbonamento: la misura delle pagine per finestra (1.3), il confronto (1.4), la generazione dei candidati (1b) e lo sviluppo usano lo stesso abbonamento. Le esecuzioni dal vivo si pianificano fuori dalle ore degli agenti.
- File in comune: migrazioni EF (`Infrastructure/Migrations`), `Directory.Packages.props`, `DBEnigmistica.slnx`, `ci.yml`, `publish.yml`, `Dockerfile`. Le ondate sono sequenziali e ognuna parte da `main` aggiornato, con il proprio ramo e la propria PR (D6).

## 7. Rischi

Dal ROADMAP, con i segnali precoci:
- *Griglie lette male dalle scansioni*: caselle corrette sotto il 98% alla prima misura. Contromisure: DPI più alti, seconda chiamata sul ritaglio, griglie delle scansioni "da verificare".
- *Troppe poche pagine per finestra*: una Rivista di 96 pagine richiede più di due finestre (misura dell'ondata 1.3). Contromisure: estrazione di notte o nel fine settimana; fornitore gratuito o locale per le pagine senza griglie; API solo con spesa accettata.
- *Fornitori gratuiti o locali sotto soglia*: resta Claude con l'abbonamento.
- *Risultati attesi costosi*: più di 30 minuti per pagina. Si parte con 10 pagine.

Da `docs/fase-1-decisioni.md` e dalla verifica del codice:
- *Claude Code non interattivo non ancora provato*: output strutturato, immagini e segnale di finestra esaurita sconosciuti. Contromisura: la prova nella 1.1 (agente C).
- *PDFium nell'immagine chiseled e font non incorporati*: la libreria nativa linux-x64 è costruita per glibc e l'immagine non ha font di sistema. Contromisura: le due verifiche della 1.1 (agente B). Se una fallisce, la decisione si riapre con un nuovo ADR (ADR 009).
- *Collegamenti e libreria nativa di versioni diverse* (PDFiumCore 157 e nativa 160): possibile differenza nelle funzioni esportate. Segnale: i test di rendering della 1.1. Va anche verificato che PDFiumCore non porti una propria copia nativa.
- *Condizioni d'uso dell'abbonamento non verificate*: da esse dipendono D5 e l'estrazione dal vivo. Scadenza 6/12/2026; prova `1.1-prova` entro il 13/12/2026.
- *Regressione locale dimenticata*: dipende dalla disciplina del proprietario. Contromisura: checklist di release.
- *Chiusura della fase 1 al 21/2/2027*, dopo la data del ROADMAP (24/1).
- *Refactoring del nucleo `riviste`*: cambiare `Game` (enum, riquadro, chiavi) tocca repository, esportazioni CSV/JSON e test esistenti. Va fatto in un'ondata sola (1.2), con l'elenco dei punti d'uso.
- *Tetto di copertura*: il nuovo progetto `DBEnigmistica.Tipologie` va incluso nella soglia dell'80% o dichiarato fuori. La regola si decide nell'ondata 1.2 (proposta: incluso).

## 8. Decisioni aperte per il proprietario

Le domande q1-q7 del 19/11/2026 sono chiuse dalle decisioni del proprietario dello stesso giorno (`docs/fase-1-decisioni.md`). La q8 è chiusa da D1b-8 del 20/11/2026.

| Id | Titolo | Chiusa da | ADR |
|---|---|---|---|
| q1 | Ordine delle ondate di fase 1 e 1b e uso del tempo guadagnato | D1: partenza il 23/11/2026, ondate alternate, chiusura il 21/2/2027 | - |
| q2 | Dove sta il set di riferimento e cosa legge la CI | D2: solo sul PC del proprietario; CI sul Set sintetico; script locale prima di ogni release | ADR 008 |
| q3 | Pacchetto per il rendering con PDFium (PDFtoImage porta SkiaSharp) | D3: PDFiumCore 157.0.8100 e `bblanchon.PDFium.*` 160.0.8165, PNG del progetto | ADR 009 |
| q4 | Archivio dei file: copia della Rivista o riferimento | D4: copia con nome per impronta, PNG per impronta, `Archivio:Cartella` | - |
| q5 | Dove gira l'estrazione in fase 1 e Claude Code nel container | D5: Windows con login OAuth; container alla fase 2 | ADR 010 |
| q6 | Rami per ondata e numerazione delle versioni con la 1b | D6: ramo per ondata, `v0.2.0-prova.1`, `v0.2.0`, `v0.2.1` e seguenti | - |
| q7 | Risposta grezza dell'AI nel registro immutabile o fuori | D7: `riviste.chiamate_ai` cancellabile, nel registro metadati e SHA-256 | ADR 011 |
| q8 | Numerazione delle correzioni della 1b.3 prima di `v0.2.0` | D1b-8 del 20/11/2026: `v0.2.0-prova.N` fino a `v0.2.0`, poi `v0.2.1` e seguenti (`.planning/phases/1b/CONTEXT.md`, sezione 9) | - |

Restano aperte, da porre con `grill-with-ui` quando servono:

| Id | Titolo | Serve per |
|---|---|---|
| domanda aperta 6 | Conferma delle soglie di accuratezza dopo la linea di base | Fine fase 1 |
| domanda aperta 13 | Condizioni d'uso dell'abbonamento | 6/12/2026 |
| domanda aperta 14 | Set di PDF di prova | 7/12/2026 |

Non sono domande per il proprietario, perché già decise o tecniche: nome `DBEnigmistica.Tipologie` (ROADMAP, fase 0, tabella del riuso); uso di squawk (ROADMAP, fase 1, criterio 2; le regole da escludere si decidono nel piano); DPI iniziale (ADR 003 lo fissa tra 150 e 200; il piano parte da 200 e la 1.4 lo tara); libreria JSON Schema (si sceglie nella 1.1 dopo la verifica delle licenze).
