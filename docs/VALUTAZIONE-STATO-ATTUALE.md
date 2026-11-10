# Valutazione dello stato attuale di DBEnigmistica

Data: 2026-11-10. Valutazione indipendente del codice in `src/` e `tests/` confrontato con `docs/prompt_iniziale`.
Legenda delle fonti: **[ESEGUITO]** = verificato lanciando comandi; **[LETTO]** = dedotto dalla lettura del codice.

## 1. Sintesi

- Il progetto **compila**, ma **non funziona da nessun punto di ingresso**: tutti i 14 comandi CLI si bloccano subito con "Service provider not available" perché il contenitore DI non viene mai collegato a System.CommandLine [ESEGUITO].
- Anche risolto quello, la pipeline PDF→giochi produce zero risultati: il "rendering" PDF restituisce **pagine bianche** (placeholder), il motore OCR **non viene mai inizializzato**, l'analisi layout è un'euristica di prossimità con soglie fisse e il rilevamento griglie è vuoto [LETTO].
- I dati estratti dall'AI **non vengono mai salvati** nelle tabelle per tipologia; i tipi non previsti dall'enum vengono scartati; il flusso "nuovo tipo → proposta schema → conferma" **non è collegato alla scansione** [LETTO].
- Lo schema dinamico genera DDL concatenando stringhe provenienti dall'AI e dall'utente: **SQL injection** possibile su tipo, default, indici e nome tabella modificato; i metadati dello schema non vengono mai salvati [LETTO].
- Test: **24 test, 9 passati, 15 falliti**; nessuno tocca un DB reale o l'API [ESEGUITO].
- Le 10 fasi "completate" di `docs/plan.md` sono in realtà uno scheletro ben organizzato con stub nei punti critici. Il repository **non ha nessun commit**.
- **Verdetto complessivo: RIPARTIRE DALLO SCHELETRO, NON DA ZERO.** Conviene tenere modello di dominio, EF Core e repository, organizzazione della soluzione, set di comandi ed export. La pipeline di estrazione va riscritta attorno a Claude con visione e output strutturato, eliminando Tesseract e l'analisi layout euristica. Lo schema dinamico va riprogettato.

## 2. Esito build e test

### Ambiente
SDK installati: 8.0.427, 10.0.114, 10.0.303, 10.0.401. Runtime: 8.0.33 e 10.0.11/12/14. **Il runtime .NET 9 non è installato**, mentre tutti i progetti puntano a `net9.0` (il piano e il prompt dicevano .NET 10). Il supporto di .NET 9 (STS) termina a novembre 2026.

### Build — [ESEGUITO] `dotnet build DBEnigmistica.slnx`
- **0 errori, 1 warning**: MSB3277, conflitto tra `Microsoft.EntityFrameworkCore.Relational` 9.0.0 e 9.0.1 nel progetto di test (EF InMemory 9.0.1 contro Npgsql 9.0.2 che porta Relational 9.0.0/9.0.1).

### Test — [ESEGUITO] `dotnet test`
- Senza runtime 9: **test run aborted** ("You must install or update .NET... Microsoft.NETCore.App 9.0.0").
- Con `DOTNET_ROLL_FORWARD=Major` (impostato solo come variabile d'ambiente, nessun file modificato) su runtime 10: **Totale 24, Passati 9, Falliti 15, Saltati 0**.

| Classe di test | Esito | Causa |
|---|---|---|
| `DynamicMigrationServiceTests` (9) | 9 falliti | `new Mock<EnigmisticaDbContext>()` senza argomenti al costruttore: Castle non riesce a creare il proxy (`tests/DBEnigmistica.Tests/Unit/Services/DynamicMigrationServiceTests.cs:18`). Inoltre un caso si aspetta `decimal`→`DECIMAL` mentre il codice produce `NUMERIC`. Il test sbagliato non ha mai girato. |
| `SchemaValidationServiceTests` (7) | 6 falliti, 1 passato | I test si aspettano messaggi in italiano ("almeno una colonna", "Nome tabella"), il codice li emette in inglese (`src/DBEnigmistica.Application/Services/Schema/SchemaValidationService.cs:72-118`). Il test "nome colonna riservato" fallisce perché il controllo sulle parole riservate non esiste. |
| `GameExtractionServiceTests` (3) | 3 passati | L'AI e il repository sono mock. |
| `RepositoryIntegrationTests` (5) | 5 passati | Usano **EF Core InMemory**, quindi non un DB vero: non verificano SQL, vincoli, JSONB né migrazioni. |

**Test che richiedono DB o servizi esterni: nessuno.** Non esiste un solo test contro PostgreSQL, Tesseract, PDF reali o l'API Claude.

### Smoke test CLI — [ESEGUITO]
`dotnet DBEnigmistica.Console.dll scan-file C:\nonexistent\x.pdf` e `list-games`: entrambi terminano con `InvalidOperationException: Service provider not available` (`src/DBEnigmistica.Console/Commands/ScanFileCommand.cs:50`), exit code 1. Il comando si blocca prima di leggere il file, quindi senza chiamare DB né API.

### Pacchetti NuGet — [ESEGUITO] `dotnet list package --vulnerable --include-transitive` / `--outdated`
- **Nessuna vulnerabilità nota** sui feed attuali.
- Pacchetti superati: `System.CommandLine 2.0.0-beta4.22272.1` (2022) è ora alla 2.0.14 stabile, con **API incompatibile** (`SetHandler`/`InvocationContext` sostituiti). EF Core/Npgsql sono alla 9.x, mentre l'ultima è la 10.x. `SkiaSharp` è alla 2.88.8, mentre l'ultima è la 4.x. Anche `PdfPig` (0.1.13→0.1.16), FluentAssertions (6→8) e xunit runner (3→4) sono indietro.
- `Anthropic.SDK 5.9.0` è un **SDK della community**, non quello ufficiale di Anthropic.
- Mix incoerente: pacchetti `Microsoft.Extensions.*` 10.0.2 su un target `net9.0`.

## 3. Requisiti del prompt iniziale e stato reale

Stato: **OK** = implementato e plausibilmente funzionante; **PARZIALE**; **STUB** = presente ma non fa nulla di utile; **ROTTO** = presente ma non può funzionare; **MANCA**.

| # | Requisito (riga in `docs/prompt_iniziale`) | Stato | Evidenza |
|---|---|---|---|
| 1 | Console app .NET recente con System.CommandLine (86-87) | **ROTTO** | Target `net9.0`, beta4 di System.CommandLine; DI non collegato: `src/DBEnigmistica.Console/Program.cs:100-104` crea uno scope che non usa, i comandi chiedono `IServiceProvider` al `BindingContext`, che non lo conosce [ESEGUITO] |
| 2 | Caricare PDF da cartella / file (20, 77-78) | PARZIALE | `MagazineProcessingService.cs:279` enumera i `*.pdf` (non ricorsivo); i comandi `scan-folder`/`scan-file` esistono ma sono irraggiungibili (riga 1) |
| 3 | PDF → immagini pagina per pagina (92) | **STUB** | `src/DBEnigmistica.Infrastructure/Services/Pdf/PdfService.cs:102-111`: crea una tela bianca delle dimensioni giuste; PdfPig non renderizza. Il layer di testo nativo del PDF (PdfPig lo sa estrarre) non viene usato |
| 4 | OCR delle pagine (21) | **ROTTO** | `TesseractOcrService` è codice reale, ma `InitializeAsync` (`src/DBEnigmistica.Infrastructure/Services/Ocr/TesseractOcrService.cs:26`) non è chiamato da nessuno, quindi ogni pagina fallisce con "OCR engine not initialized". OCR per regione non filtra nulla (`:126-145`, `Words = []`). Chiave di config sbagliata: `appsettings.json:18` usa `TesseractDataPath`, il codice legge `Ocr:TessdataPath` (`src/DBEnigmistica.Infrastructure/Configuration/DependencyInjection.cs:44`) |
| 5 | Analisi layout: titolo, didascalia, griglia, definizioni (22) | **STUB** | `src/DBEnigmistica.Infrastructure/Services/LayoutAnalysis/BasicLayoutAnalysisService.cs`: raggruppa parole consecutive con soglie fisse in pixel (`:75-76`); una "griglia" è un blocco "quasi quadrato con più di 5 parole" (`:146`). `DetectGridsAsync` restituisce sempre una lista vuota (`:62`). Una griglia di cruciverba ha poche parole OCR, quindi questa euristica la scarta |
| 6 | Classificazione tipo di gioco (23) | PARZIALE | `ClaudeGameAnalyzer.ClassifyGameAsync` (`src/DBEnigmistica.AI/Services/ClaudeGameAnalyzer.cs:26`) manda **solo testo OCR**, niente immagine. Vengono classificate solo le regioni `Grid`/`Table` (`MagazineProcessingService.cs:213-214`), quindi rebus, crittografie e giochi in versi non arrivano mai all'AI |
| 7 | Salvataggio nel DB: Magazines, Pages, Games (24, 30-49) | PARZIALE | Entità e migrazioni ci sono. `Page.OcrText`/`ImagePath` non vengono mai valorizzati (`MagazineProcessingService.cs:180-187`). La periodicità non viene dedotta, arriva solo da opzione CLI. Non c'è controllo sui duplicati se si riscansiona un PDF |
| 8 | Tabelle specifiche per tipologia (51-57) | **ROTTO** | Esistono 4 tabelle EF (`CrosswordGame`, `RebusGame`, `LogicGame`, `AnagramGame`) ma **nessun codice ci scrive**. Il risultato di `ExtractGameDataAsync` viene restituito e poi scartato (`src/DBEnigmistica.Application/Services/Processing/GameExtractionService.cs:76-90`); nelle tabelle dinamiche non si scrive mai (manca un INSERT) |
| 9 | Nuovo tipo → proposta tabella → conferma/modifica → creazione (59-64, 72-75) | **ROTTO** | Il flusso esiste solo per `analyze-game`/`analyze-game-text`. Nella scansione, i tipi fuori dall'enum `GameType` vengono scartati (`GameExtractionService.cs:44-45`). I metadati dello schema non vengono mai salvati: `AddAsync` senza `SaveChangesAsync` (`src/DBEnigmistica.Application/Services/Schema/DynamicSchemaOrchestrator.cs:115`, `:238`), quindi la tabella viene creata ma il sistema non la conosce. La modifica interattiva non rivalida lo schema (`SchemaValidationService.cs:52-53`); `modify-column` è annunciato (`:181`) ma non implementato |
| 10 | Uso di `giochi_enigmistici.md` per far generare le tabelle all'AI (66) | PARZIALE | `analyze-game <file.md>` passa l'intero markdown in un solo prompt. Il file descrive circa 30 giochi, quindi si ottiene una sola tabella. Non c'è suddivisione per gioco |
| 11 | Riepilogo in console dei giochi riconosciuti (71) | PARZIALE | `scan-file` mostra solo conteggi (pagine, giochi). Mancano i dettagli per gioco: tipo, pagina, posizione |
| 12 | CLI: script SQL da inserire nel DB (79) | **MANCA** | `IDynamicMigrationService.ExecuteSqlAsync` esiste (`src/DBEnigmistica.Infrastructure/Services/Database/DynamicMigrationService.cs:114`) ma nessun comando lo espone |
| 13 | CLI: file .md / stringa descrittiva per l'agente AI (80-81) | PARZIALE | `analyze-game` e `analyze-game-text` sono implementati ma irraggiungibili (riga 1); l'opzione `--yes` viene letta e ignorata (`AnalyzeGameCommand.cs:17-41`) |
| 14 | Altri comandi utili (82) | PARZIALE | `list-games`, `export-*`, `migrate` sono plausibili; export JSON/CSV con escape CSV corretto (`DataExportService.cs:196`). Nessuno è eseguibile (riga 1) |
| 15 | Gestione soluzioni (fase 10 del piano, extra) | **STUB** | `SolutionExtractionService.AnalyzeSolutionPatternAsync` restituisce sempre lista vuota (`src/DBEnigmistica.Application/Services/Solution/SolutionExtractionService.cs:41-44`); `FindPotentialSolutionsAsync` idem (`SolutionMatchingService.cs:36-38`). Funziona solo il collegamento manuale `link-solution` / `verify-solution` |
| 16 | Agente AI con API (93) | PARZIALE | Vedi sezione 4.2 |
| 17 | EF Core, design migrabile (88-89) | OK | DbContext e 2 migrazioni coerenti col modello; mai applicate a un DB reale. Il DDL dinamico è scritto a mano per PostgreSQL (`SERIAL`, `JSONB`), quindi non è portabile |
| 18 | Separazione dei layer (94-100) | PARZIALE | Progetti separati e interfacce in Domain, ma il layer Application scrive su console (`DynamicSchemaOrchestrator.cs:51` e seguenti, `SchemaValidationService.cs` intero). Vedi sezione 4.1 |
| 19 | Cache OCR (fase 9, extra) | STUB funzionale | `IMemoryCache` in un processo CLI che dura una sola esecuzione: non evita mai il riprocessing tra due run |

## 4. Qualità e rischi

### 4.1 Architettura e dipendenze tra layer [LETTO]
- La direzione delle dipendenze è **corretta a livello di progetto**: Domain senza dipendenze, Application → Domain, Infrastructure/AI → Domain, Console → tutto. È la parte migliore del progetto.
- Violazioni: la UI (console) sta in Application (`SchemaValidationService`, `DynamicSchemaOrchestrator`, `Services/UI/ConsoleProgressReporter.cs`). `ClaudeApiClient` è una classe concreta senza interfaccia. La configurazione è letta come stringhe grezze, senza Options pattern.
- `Environment.Exit` dentro gli handler (oltre 30 occorrenze, per esempio `ScanFileCommand.cs:85`) aggira il dispose dell'host e il flush di Serilog.
- `appsettings.json` non viene copiato in output (manca `CopyToOutputDirectory` nel csproj), e `Program.cs:14` usa la directory corrente. L'app funziona solo se lanciata dalla cartella del progetto.
- `SETUP_INSTRUCTIONS.md:82-83` indica `dotnet user-secrets`, ma il csproj non ha `UserSecretsId` e `Program.cs:13-18` non aggiunge gli user secrets. Le istruzioni non funzionano.
- La chiave API vuota (`""`) supera il controllo `?? throw` (`src/DBEnigmistica.AI/Services/ClaudeApiClient.cs:26-27`): l'errore arriva solo alla prima chiamata.

### 4.2 Integrazione Claude [LETTO]
- Modello `claude-3-5-sonnet-20241022` (`appsettings.json:14`, default in `ClaudeApiClient.cs:29`): **ritirato**, le chiamate fallirebbero.
- Solo testo OCR, nessun input immagine/PDF (`ClaudeApiClient.cs:50-53`). Per rebus, griglie e crittografie illustrate l'informazione è nell'immagine.
- Formato risposte: JSON "chiesto per favore" nel prompt. Il parsing toglie i fence ```` ``` ```` e poi deserializza (`ClaudeApiClient.cs:124-144`). Non usa output strutturato né tool use, non valida contro uno schema e non fa retry su JSON non valido.
- I template sono stringhe `@"..."` **non interpolate** che contengono `{{`/`}}` (`src/DBEnigmistica.AI/Prompts/PromptTemplates.cs:18` e seguenti). Il modello riceve quindi un "esempio JSON" con doppie graffe, cioè non valido.
- Mancano retry/backoff su 429/529, controllo di `stop_reason == max_tokens` (con `MaxTokens` a 4096 un cruciverba completo può troncarsi), prompt caching, Batch API e stima dei costi.
- I campi da estrarre sono una lista cablata per 4 tipi (`ClaudeGameAnalyzer.cs:176-214`), scollegata dalle colonne reali di tabelle EF e tabelle dinamiche.

### 4.3 Schema dinamico: sicurezza e coerenza [LETTO]
- `GenerateCreateTableSql` concatena **senza quoting né whitelist**: nome tabella (`DynamicMigrationService.cs:82`), tipo di dato (il fallback `_ => type.ToUpper()` passa qualunque stringa, `:186`), `DEFAULT` (`:157`), nomi degli indici (`:104`).
- Queste stringhe vengono dalla risposta dell'AI, che a sua volta dipende dal testo OCR o markdown (prompt injection possibile), e dalla console. La validazione dei nomi esiste (`SchemaValidationService.cs:254`) ma **non copre tipo e default** e **viene saltata dopo le modifiche interattive**. È un vettore reale di SQL injection, anche se l'app è locale e l'utente è l'unico operatore.
- Incoerenza con EF Core: le tabelle dinamiche sono invisibili al modello EF e alle migrazioni, manca il percorso di scrittura e `CREATE TABLE IF NOT EXISTS` maschera i conflitti. Mancano versionamento ed evoluzione (ALTER) di uno schema esistente.

### 4.4 Test [ESEGUITO + LETTO]
- 24 test, 37% di successo, nessuno su pipeline PDF/OCR/AI reali, nessun dataset di pagine di riferimento, nessun test su PostgreSQL. I test usano reflection su metodi pubblici e il mock di DbContext. Nessuna misura di copertura.

### 4.5 Segreti [LETTO]
- `src/DBEnigmistica.Console/appsettings.Development.json` è correttamente ignorato da git (`.gitignore:28`); non l'ho aperto.
- Nei file che finirebbero su git **non ci sono chiavi reali**, solo credenziali di sviluppo locali in chiaro:
  - `docker-compose.yml:9` (password PostgreSQL di sviluppo)
  - `docker-compose.yml:26` (password pgAdmin banale)
  - `src/DBEnigmistica.Infrastructure/Configuration/EnigmisticaDbContextFactory.cs:17` (connection string con password)
- Il resto sono placeholder (`.env.template`, `README.md:97-100`, `SETUP_INSTRUCTIONS.md`). Il compose **ignora** `.env.template` (valori cablati).

### 4.6 Debito tecnico principale
- Il repository git non ha nessun commit: tutto il lavoro è non tracciato.
- `docs/plan.md` dichiara completate funzioni che non esistono e segna come fatti i test E2E "da fare su richiesta" (riga 127). Non è affidabile come base.
- System.CommandLine beta4 va migrato all'API 2.0 stabile, quindi va riscritto tutto il wiring dei 14 comandi.
- La console usa emoji e box drawing ovunque (problemi di encoding su console Windows non UTF-8).

## 5. Aree e verdetto

| Area | Verdetto | Motivazione |
|---|---|---|
| Struttura della soluzione (5 progetti, direzione dipendenze) | **TENERE** | Dipendenze corrette; basta spostare la UI fuori da Application e trattare AI come adapter di infrastruttura |
| Entità di dominio Magazine/Page/Game/GameSolution, enum | **RIFATTORIZZARE** | Modello sensato; il tipo di gioco deve diventare dato (tabella tipi) e non enum chiuso, e serve un payload per tipo |
| Entità tipizzate Crossword/Rebus/Logic/AnagramGame | **RIFATTORIZZARE** | Mai scritte; vanno ricondotte al nuovo modello di schema per tipo (sezione 6.3) |
| DbContext EF Core + migrazioni | **RIFATTORIZZARE** | Corretti ma mai applicati; vanno rigenerati su .NET 10/EF 10 dopo la revisione del modello |
| Repository | **TENERE** | Sottili e corretti; va chiarita la semantica Add/Save (causa del bug sui metadati schema) |
| `PdfService` (rendering) | **RISCRIVERE** | Restituisce pagine bianche; serve un renderer reale basato su PDFium e l'estrazione del layer di testo nativo |
| `TesseractOcrService` + cache OCR | **ELIMINARE** | Mai inizializzato; sostituito dalla visione di Claude. Cache in memoria inutile in una CLI |
| `BasicLayoutAnalysisService` | **ELIMINARE** | Euristica a soglie fisse, griglie non rilevate; il modello fa segmentazione e classificazione meglio |
| `ClaudeApiClient` | **RISCRIVERE** | Modello ritirato, solo testo, niente output strutturato, retry o batch; SDK non ufficiale |
| `PromptTemplates` / `ClaudeGameAnalyzer` | **RISCRIVERE** | Prompt con graffe doppie, campi cablati scollegati dagli schemi |
| `MagazineProcessingService` | **RIFATTORIZZARE** | Lo scheletro (ciclo per pagina, errori per pagina, risultato) è buono; servono idempotenza, ripresa e persistenza del payload |
| `GameExtractionService` | **RISCRIVERE** | Scarta i dati estratti e i tipi nuovi; va ricostruito sul nuovo contratto AI |
| Schema dinamico (Orchestrator, Validation, DynamicMigration) | **RISCRIVERE** | SQL injection, metadati non salvati, nessuna scrittura dati, nessuna evoluzione dello schema |
| Interazione console di conferma schema | **RIFATTORIZZARE** | Il flusso s/n/m è riutilizzabile; va spostato in Console, rivalidato dopo le modifiche e completato |
| Comandi CLI (set di comandi) | **RIFATTORIZZARE** | Set di comandi ragionevole; wiring DI rotto e API beta da migrare a System.CommandLine 2.0 |
| Export JSON/CSV | **TENERE** | Semplice e corretto in lettura (non verificato a runtime) |
| Gestione soluzioni | **ELIMINARE (dall'MVP)** | Estrazione e matching sono stub; tenere entità e link manuale, rimandare il resto |
| Test | **RISCRIVERE** | 15/24 rossi, nessun test reale; servono Testcontainers PostgreSQL e un set di pagine di riferimento con risultati attesi |
| Configurazione e segreti | **RIFATTORIZZARE** | Options pattern, user secrets realmente collegati, appsettings copiato in output, compose che legge `.env` |
| `docker-compose.yml` | **TENERE** | Adeguato per sviluppo locale; parametrizzare le password |
| `docs/plan.md`, README, SETUP | **RISCRIVERE** | Descrivono funzioni inesistenti; sostituire con piano e stato reali |

## 6. Raccomandazioni tecnologiche

### 6.1 Raccomandazione: Claude con visione e output strutturato al posto di Tesseract e dell'analisi layout
Pipeline consigliata:

1. **Rendering locale delle pagine** con una libreria basata su PDFium (per esempio PDFtoImage o Docnet.Core) a 150-200 DPI. Se il PDF ha un layer di testo nativo, va **estratto con PdfPig** e allegato come contesto. Le riviste digitali spesso lo hanno, e allora è più affidabile di qualunque OCR.
2. **Una chiamata per pagina** a **`claude-sonnet-5-5`**: immagine della pagina, testo nativo se presente, catalogo dei tipi noti con il loro JSON Schema. La risposta usa **output strutturato** (JSON Schema imposto dall'API o tool use con `input_schema`). Restituisce l'elenco dei giochi: tipo noto o "nuovo tipo proposto", titolo, bounding box approssimativo, payload conforme allo schema del tipo, confidenza, avvisi.
3. **Batch API** per le scansioni di cartelle (costo dimezzato, nessuna esigenza di latenza) e **prompt caching** sul catalogo dei tipi, che è identico per tutte le pagine.
4. **`claude-opus-5-5` solo per la progettazione di nuovi schemi di gioco** (`analyze-game`, nuovo tipo emerso in scansione): sono chiamate rare, dove conta la qualità del ragionamento e non il costo per pagina.

Motivazione:
- L'informazione dei giochi enigmistici è visiva (griglie, caselle nere, vignette dei rebus, impaginazione a colonne): un OCR più un'euristica a soglie la perde, e il codice attuale lo dimostra.
- Un modello con visione fa segmentazione, classificazione ed estrazione in un solo passaggio, eliminando tre componenti (OCR, layout, classificazione testuale) e le relative dipendenze native (Tesseract, tessdata, SkiaSharp).
- L'output strutturato elimina il parsing fragile del JSON.

Sonnet è il punto di equilibrio costo/qualità per centinaia di pagine per rivista. Haiku conviene solo se un benchmark su pagine reali mostra qualità equivalente. Non ho informazioni affidabili sul posizionamento di `claude-fable-5-2` e non lo raccomando senza una prova.

Prerequisito non negoziabile: un **set di riferimento** di 20-30 pagine reali con l'output atteso, su cui misurare modello, DPI e prompt prima di scalare.

### 6.2 SDK e piattaforma
- Passare all'**SDK C# ufficiale di Anthropic** (verificare la versione stabile corrente) al posto di `Anthropic.SDK` della community. Va dietro un'interfaccia `IPageExtractor` in Domain/Application, così da poterlo simulare nei test.
- Target **.NET 10 (LTS)**, EF Core 10, Npgsql 10, System.CommandLine 2.0 stabile; Spectre.Console per prompt e tabelle in console (sostituisce il codice di box drawing a mano).

### 6.3 Schema per tipo di gioco
Raccomando un modello ibrido al posto del DDL generato a runtime:

- Tabella `game_types` con nome e **JSON Schema versionato** (proposto dall'AI, mostrato, modificato e confermato dall'utente, come chiede il prompt).
- `games.payload` in **JSONB**, validato contro lo schema del tipo prima del salvataggio.
- Le **tabelle o viste specifiche per tipo** vengono generate come **file di migrazione SQL** da un generatore con whitelist di tipi e identificatori quotati. L'utente li rivede e li applica con un comando esplicito.

Così restano il requisito di conferma e la possibilità di avere tabelle per tipo. In più si eliminano la SQL injection a runtime e l'incoerenza con EF Core, e l'evoluzione degli schemi diventa versionabile.

Se il proprietario considera irrinunciabili vere tabelle fisiche popolate dall'app, il generatore deve comunque usare una whitelist chiusa di tipi, identificatori quotati e nessun `DEFAULT` libero, e serve un percorso di INSERT parametrizzato. Oggi nessuna delle due cose esiste.

### 6.4 Idempotenza e costi
- Hash SHA-256 del PDF e della pagina persistiti in DB, così una riscansione salta le pagine già elaborate. Questo sostituisce la cache OCR in memoria.
- Registrare per pagina modello, token, costo, esito e tempo. Serve un limite di spesa configurabile per esecuzione.

## 7. Ordine suggerito per la ripianificazione
1. Primo commit dello stato attuale e passaggio a .NET 10; correzione del wiring DI dei comandi.
2. Set di riferimento di pagine reali e spike Claude con visione su 5 pagine (misura di qualità e costo).
3. Nuovo modello `game_types` + payload JSONB, migrazione EF rigenerata, Testcontainers.
4. Pipeline per pagina (rendering, estrazione strutturata, persistenza, idempotenza), poi Batch API per le cartelle.
5. Flusso nuovo tipo → proposta → conferma integrato nella scansione; generatore di migrazioni per le tabelle per tipo.
6. Comandi accessori (export, script SQL, riepilogo per rivista); soluzioni per ultime.
