# Fase 1, ondata 1.2 - Preparazione all'avvio

Scritto il 21/11/2026 dall'agente di pianificazione (Opus), su `main` al merge ed202e4 (ondata 1b.1 unita). Base: `PLAN.md` sezione 4, `progetto-1.1.md`, `.planning/phases/1b/PLAN.md` sezione 2, ADR 001-011. Nessun codice scritto. Le proposte marcate [A] le conferma l'agente principale. Le proposte [P] sono decise dal proprietario il 21/11/2026: vedi sezione 7.

## 1. Prerequisiti

### 1.1 Presenti in `main` (verificati nel codice)

| Serve alla 1.2 | Dove | Chi lo usa |
|---|---|---|
| Meta-schema dei descrittori, con `versione` minima 0 | `tipologie/_meta/descrittore.schema.json` (`/properties/versione` minimum 0; famiglie, tipi, ruoli, validatori e rese a lista chiusa) | A, B |
| Esempi di descrittori validi e non validi | `tipologie/_meta/esempi/descrittore/*.json` | A (test del caricatore) |
| Schemi di risultato atteso e risposta per pagina, con esempi | `tipologie/_meta/risultato-atteso.schema.json`, `risposta-pagina.schema.json`, `esempi/risultato-atteso/**`, `esempi/risposta-pagina/**` | A (derivazione), C (metriche) |
| Regole di traduzione in SQL, catalogo, versione del prompt, metriche, Set sintetico, squawk | `progetto-1.1.md`, sezioni 2.4, 3.1-3.2, 4, 5.2-5.3, 6, 8 | A, B, C |
| Libreria JSON Schema scelta e già nel progetto | `Corvus.Text.Json.Validator` 5.8.12 in `Directory.Packages.props`; involucro `src/DBEnigmistica.AI/Services/ValidatoreSchemaJson.cs` dietro `Application/FornitoreAi/IValidatoreSchemaJson.cs` | A |
| Modello del prompt di estrazione e sua versione | `src/DBEnigmistica.Application/FornitoreAi/ModelloPromptEstrazione.cs`, `tipologie/_meta/prompt-estrazione-v1.md` | C |
| Registrazioni, `Riproduci`, limite per esecuzione | `Application/FornitoreAi/Registrazioni.cs`, `FornitoriAiDecoratori.cs` (`FornitoreAiRiproduci`, `FornitoreAiConLimite`); `Infrastructure/Services/Registrazioni/ArchivioRegistrazioniFileSystem.cs` | C |
| Rendering, impronta della pagina, PDF sintetici | `Application/Acquisizione/IRenderPagine.cs`; `Infrastructure/Services/Pdf/PdfRenderPagine.cs`, `GeneratorePdfSintetici.cs` | C |
| Eseguibile finto di Claude Code | `tests/DBEnigmistica.Tests/Fixtures/ClaudeFinto/` | C (`--live` solo con il finto) |
| Default privileges dei ruoli in `riviste` e `catalogo` | `Migrations/20261111120802_VincoliRuoliERegistro.cs:223-230` | A (le tabelle generate ereditano i permessi se `migrate` gira con il ruolo proprietario) |
| `catalogo.giochi` con chiave `uuid` e `CHECK versione_tipologia > 0` | `Infrastructure/Data/Mappings/CatalogoMapping.cs:44-81` | A (riferimento del nucleo in `catalogo`) |
| Nessun DDL a runtime residuo | `grep ExecuteSql` in `src/` trova solo lucchetti e `SELECT` (`CandidatiRepository.cs`, `RegistroEventiRepository.cs`) | - |

### 1.2 Mancanti (li crea la 1.2)

- Progetto `src/DBEnigmistica.Tipologie`: non esiste.
- Cartelle `tipologie/cruciverba-classico`, `anagramma`, `sciarada`, `cambio-di-lettera`: non esistono. In `tipologie/` ci sono solo i cinque `v0.json` della 1b.1.
- Formattatore del catalogo (sezione 5.2) e derivazione dello schema della risposta per pagina (sezione 3.2): non esistono in C#. Li scrive A.
- Tabella `tipologie.migrazioni_applicate` e schema `tipologie`: non esistono.
- `.squawk.toml`, `scripts/`, `Application/Valutazione/`, comandi `types` ed `eval`: non esistono.
- `MigrateCommand.cs` applica solo EF (`MigrateAsync`).

### 1.3 Blocchi del proprietario

- **Nessuno blocca l'avvio.** La 1.2 non fa chiamate dal vivo: Set sintetico, Registrazioni scritte dal progetto ed eseguibile finto. `eval extraction --live` si scrive e si prova solo con l'eseguibile finto. Nessuna esecuzione con Claude Code vero prima della riga `PHASE:1 WAVE:1.1-prova STATUS:complete`.
- **Regole delle 4 Tipologie** (D3): la 1.2 le scrive in bozza e le invia entro il 27/11. Il proprietario le approva tutte e 4 entro il 4/12. La 1b.2 aspetta almeno `cruciverba-classico` e `anagramma`.
- **Prova dello script locale** (D6): il proprietario lancia `scripts/regressione-locale.ps1` sul Set sintetico provvisorio entro il 27/11. Dopo il ricalcolo del 2/12 lo rilancia, circa 5 minuti, senza nuova approvazione.
- Set di PDF veri (ROADMAP, domanda 14, 7/12) e `1.1-prova` (13/12): non servono alla 1.2.

## 2. Riconciliazione con la 1b.1

| # | Sovrapposizione | Proposta | |
|---|---|---|---|
| R1 | Due lettori dei descrittori: `Application/Creazione/DescrittoreTipologia.cs` (lettura senza meta-schema, solo i campi della creazione) e il caricatore di `DBEnigmistica.Tipologie` (A, con meta-schema e controlli della sezione 2.4) | Nella 1.2 restano due. A non tocca `Creazione/**`. B aggiunge un test che legge ogni `tipologie/*/v*.json` con entrambi i lettori. L'unificazione (la creazione usa il caricatore di Tipologie) si fa nella 1b.2, con i test della 1b invariati | [A] |
| R2 | Derivazione degli schemi: `Creazione/DerivatoreSchemaCreazione.cs` (risposta di creazione) e la derivazione per pagina di A (sezione 3.2) | Restano separate: contratti diversi (un Gioco senza riquadro contro elenco di Giochi per pagina). A non tocca il derivatore della creazione | [A] |
| R3 | Descrittori `v0.json` | Il caricatore li accetta. `types generate [--check]` e `migrate` li saltano: nessun SQL, nessuna tabella. Test dedicato (criterio di uscita della 1.2). `ci.yml:66` e `publish.yml:178` controllano già la presenza dei cinque `v0.json`: non si toccano | [A] |
| R4 | Compatibilità dei `v1.json` di B con la creazione | I descrittori di `cruciverba-classico` e `anagramma` servono alla 1b.2. Devono passare anche `DescrittoreTipologia.Leggi` (`elenchi` sempre presente, anche vuoto; `regole` come oggetto; `resa.modello`) e `DerivatoreSchemaCreazione.Generico`. Coperto dal test di R1 | [A] |
| R5 | Validazione con Corvus: l'involucro sta in `DBEnigmistica.AI` | Tipologie referenzia direttamente `Corvus.Text.Json.Validator` (già fissato, nessun pacchetto nuovo) con un proprio involucro minimo. Tipologie non referenzia Application, AI, Infrastructure né Console. Nessuno spostamento di `ValidatoreSchemaJson` nella 1.2. Il `.csproj` copia la soppressione di `CTJ001` usata in `DBEnigmistica.Web.csproj` | [A] |
| R6 | Dipendenze fra progetti | Tipologie → nessun progetto. Application → Tipologie (per C: catalogo e schema per la versione del prompt). Infrastructure → Tipologie. Test di architettura nuovo di A | [A] |
| R7 | `DBEnigmistica.slnx`, `Directory.Packages.props`, tutti i `.csproj` (compresi Application, Infrastructure, Console, Tests) | Un solo proprietario: A. C e B chiedono le righe nel rapporto | [A] |
| R8 | `ci.yml`: A (generate, squawk, copertura) e C (regressione sintetica) | Proprietario A. C consegna il testo esatto del passo entro il 2/12; A lo inserisce. Lavori esistenti invariati, compreso il passo "Pubblicazione di controllo (prototipo)" | [A] |
| R9 | `Console/Cli/CliApp.cs`, `Commands/HandlerRegistration.cs` (A: `types`, `migrate`; C: `eval`) | Proprietario A. C consegna le righe di registrazione | [A] |
| R10 | `Infrastructure/Configuration/DependencyInjection.cs` (A: servizi di Tipologie; B: riga 31 `IGameRepository`) | Proprietario A. B consegna le modifiche esatte, se servono | [A] |
| R11 | `Application/Configuration/DependencyInjection.cs:23` (registra `GameExtractionService`) | Proprietario B. C registra i propri servizi in `Application/Valutazione/DependencyInjection.cs`, come ha fatto la 1b con `Creazione/DependencyInjection.cs` | [A] |
| R12 | `Migrations/**`: A ha bisogno di `tipologie.migrazioni_applicate`, B della migrazione del nucleo | Due migrazioni in sequenza, mai in parallelo. A crea per primo, entro il 24/11, `Migrations/<ts>_TipologieMigrazioniApplicate.cs` con lo schema `tipologie` e la tabella (id, versione, schema, checksum, applicata_il), senza entità EF. B crea la migrazione del nucleo dopo, sul ramo aggiornato. Snapshot rigenerata solo da B | [A] |
| R13 | Nome del nucleo usato dal generatore (`riviste."games"("Id")`, `catalogo."giochi"("id")`, `progetto-1.1.md` 3.1) | B non rinomina `riviste.games` né la colonna `"Id"`. La classe `Game` resta `Game`: così `DipendenzeTests.cs:11` e `FornitoreAiArchitetturaTests.cs:12` non cambiano | [A] |
| R14 | Dove `migrate` trova SQL e descrittori. `Console.csproj` copia `tipologie/**/v*.json` solo con `IncludePrototipo`, e il `Dockerfile:18` lo spegne | Descrittori `v*.json` e SQL `v*.*.sql` come risorse incorporate in `DBEnigmistica.Tipologie`. `migrate` legge le risorse, quindi funziona anche nell'immagine. `types generate` scrive e confronta i file del repository. Il glob di `Console.csproj` non cambia | [A] |
| R15 | Proprietà dei file SQL generati, che stanno nelle cartelle dei descrittori di B | B possiede `tipologie/<id>/v1.json`. A possiede `tipologie/<id>/v1.riviste.sql` e `v1.catalogo.sql`, prodotti solo con `dbe types generate`, mai a mano | [A] |
| R16 | `tipologie/_meta/**` non ha proprietario nella 1.2 | Nessuna modifica. Se B ha bisogno di un nome di validatore nuovo (per esempio per la sciarada), lo chiede all'agente principale; B cambia solo l'enumerazione `$defs/validatore` | [A] |
| R17 | `src/DBEnigmistica.Tipologie/Descrittori/**` assegnato a B nel PLAN, mentre A scrive il caricatore | B non usa `Tipologie/Descrittori/**`. A possiede `Modello/**`, `Caricamento/**`, `Generatore/**`, `Derivazione/**` e `Catalogo/**`. B possiede solo `Validatori/**`, sui tipi di `Modello/**` consegnati da A il 24/11 | [A] |
| R18 | Set sintetico: `set.json` prevede `pdf/sintetico.pdf`, ma `.gitignore:51` esclude `*.pdf` e la 1.1 preferisce PDF generati al volo | C estende `GeneratorePdfSintetici.cs` aggiungendo solo le pagine s01-s06 (le pagine esistenti non cambiano). `set.json` del Set sintetico porta `"pdf": null` e `"generatore": "set-sintetico-v1"`. Il lettore accetta `generatore` solo con `tipo: sintetico`. Piccola estensione della sezione 6.1, annotata nel rapporto di C | [A] |
| R19 | La versione del prompt contiene descrittori e regole (5.3). Ogni ritocco ai descrittori, e l'approvazione delle regole, cambia la chiave delle Registrazioni sintetiche e la Linea di base | C scrive le risposte sintetiche in file indipendenti dalla chiave (`risposte/<id>.json`). Il comando nascosto `dbe eval sintetico aggiorna-chiavi` ricalcola cartelle e Linea di base. Rifiuta `tipo: riferimento`. Il commit che registra l'approvazione delle regole lo esegue | [A] |

## 3. Analisi d'impatto per B (nucleo `riviste`)

Ricerca del 21/11/2026 su `main` (ed202e4), con `grep -nP` sui file tracciati. Simboli: `Game`, `Games`, `GameId`, `GameType`, `CrosswordGame`, `AnagramGame`, `LogicGame`, `RebusGame`, `IGameRepository`, `GameRepository`, `GameExtractionService`, `PromptTemplates`, `Claude*Response`, le proprietà del riquadro (`PositionX`, `PositionY`, `BoundingBoxWidth`, `BoundingBoxHeight`), `ClassificationConfidence`, `ExtractedDate`, e le tabelle `crossword_games`, `anagram_games`, `logic_games`, `rebus_games`. Esclusi i `.Designer.cs` e la snapshot, che EF rigenera.

### 3.1 Sorgenti

| File | Righe | Azione |
|---|---|---|
| `src/DBEnigmistica.AI/Models/ClaudeExtractionResponse.cs` | 9, 12 | Eliminare. Zero usi fuori dal file |
| `src/DBEnigmistica.AI/Models/ClaudeGameClassificationResponse.cs` | 8, 11 | Eliminare. Zero usi |
| `src/DBEnigmistica.AI/Models/ClaudeSchemaResponse.cs` | 35 | Eliminare. Zero usi |
| `src/DBEnigmistica.AI/Prompts/PromptTemplates.cs` | 6, 47, 67, 70, 107, 127, 130 | Eliminare. Zero usi |
| `src/DBEnigmistica.Application/Configuration/DependencyInjection.cs` | 23 | Togliere la registrazione |
| `src/DBEnigmistica.Application/Services/Processing/GameExtractionService.cs` | 11-20, 46-102, 154 | Eliminare. La pipeline arriva nella 1.3 (ROADMAP: "riscrivere") |
| `src/DBEnigmistica.Application/Services/Export/DataExportService.cs` | 15, 20, 38, 51, 55-58, 73, 89, 102, 108, 112-117, 143-147 | Adattare: colonna `Tipologia` al posto di `GameType`, riquadro normalizzato in `X`, `Y`, `Larghezza`, `Altezza`, confidenza (D5) |
| `src/DBEnigmistica.Application/Services/Solutions/GameSolutionService.cs` | 10, 15, 41, 46, 53, 86 | Adattare, se usa proprietà rimosse |
| `src/DBEnigmistica.Console/Commands/Solution/ExportSolutionsCommand.cs` | 83, 92, 98-100, 113, 119, 125-127 | Adattare |
| `src/DBEnigmistica.Console/Commands/Solution/ListUnsolvedCommand.cs` | 16, 38, 43, 54, 66, 74, 76, 77 | Adattare |
| `src/DBEnigmistica.Domain/Entities/Game.cs` | 9, 21, 31, 36, 41, 46, 61, 66 | Nuove colonne (PLAN, sezione 2, riga "Nucleo", più `trascrizione_proposta`); via `GameType` e riquadro in pixel |
| `src/DBEnigmistica.Domain/Entities/CrosswordGame.cs`, `AnagramGame.cs`, `LogicGame.cs`, `RebusGame.cs` | 6, 13, ~55 ciascuno | Eliminare |
| `src/DBEnigmistica.Domain/Enums/GameType.cs` | 6 | Eliminare |
| `src/DBEnigmistica.Domain/Entities/GameSolution.cs` | 15, 35, 40, 55, 68 | Verificare; resta il legame con `Game` |
| `src/DBEnigmistica.Domain/Entities/Page.cs` | 47 | Nessuna modifica attesa (`Games`) |
| `src/DBEnigmistica.Domain/Interfaces/IGameRepository.cs` | 5, 7-10 | Adattare (filtri per `GameType` → tipologia) |
| `src/DBEnigmistica.Domain/Interfaces/IGameSolutionRepository.cs` | 8 | Verificare |
| `src/DBEnigmistica.Domain/AI/GameClassificationResult.cs`, `GameExtractionResult.cs` | 8 | Nessuna modifica: `GameType` è una stringa. I tipi di `Domain/AI` restano (PLAN, sezione 2) |
| `src/DBEnigmistica.Infrastructure/Configuration/DependencyInjection.cs` | 31 | Nessuna modifica attesa; il file è di A (R10) |
| `src/DBEnigmistica.Infrastructure/Data/EnigmisticaDbContext.cs` | 22-26 | Togliere i `DbSet` di dettaglio |
| `src/DBEnigmistica.Infrastructure/Data/Mappings/RivisteMapping.cs` | 62, 68, 76, 86, 94-132, 146, 153 | Nuove colonne, `CHECK` del riquadro 0-1, via le mappature di dettaglio |
| `src/DBEnigmistica.Infrastructure/Repositories/GameRepository.cs` | 8-42 | Adattare |
| `src/DBEnigmistica.Infrastructure/Repositories/GameSolutionRepository.cs` | 17-75 | Adattare |
| `src/DBEnigmistica.Infrastructure/Repositories/MagazineRepository.cs` | 39, 69, 75, 76 | Adattare gli `Include` |
| `src/DBEnigmistica.Infrastructure/Migrations/20261111120801_Baseline.cs` | 39 righe | Non si tocca: migrazione storica |
| `src/DBEnigmistica.Infrastructure/Migrations/20261111120802_VincoliRuoliERegistro.cs` | 20 | Non si tocca. Attenzione: nomina `FK_games_pages_SolutionPageId`, che la nuova migrazione non deve rinominare |
| Nuova migrazione del nucleo, snapshot rigenerata | - | Creare |

### 3.2 Test

| File | Righe |
|---|---|
| `tests/DBEnigmistica.Tests/Unit/Services/GameExtractionServiceTests.cs` | 19-167 (24 righe): eliminare con il servizio |
| `tests/DBEnigmistica.Tests/Unit/Services/DataExportServiceTests.cs` | 16, 70, 123-141, 158, 174-177 |
| `tests/DBEnigmistica.Tests/Unit/Services/CsvExportTests.cs` | 59-140 (13 righe) |
| `tests/DBEnigmistica.Tests/Unit/Cli/GameSolutionServiceTests.cs` | 14, 23, 33, 42, 52, 53, 69 |
| `tests/DBEnigmistica.Tests/Integration/RepositoryIntegrationTests.cs` | 49, 52, 172-271 (29 righe) |
| `tests/DBEnigmistica.Tests/Integration/IntegrationTestBase.cs` | 135-191 (10 righe) |
| `tests/DBEnigmistica.Tests/Integration/DatabaseConstraintsIntegrationTests.cs` | 43, 125, 130, 146, 152, 156 |
| `tests/DBEnigmistica.Tests/Integration/GameSolutionRepositoryIntegrationTests.cs` | 24, 38, 122, 166, 182 |
| `tests/DBEnigmistica.Tests/Integration/DateEPagineIntegrationTests.cs` | 180, 181, 189, 191 |
| `tests/DBEnigmistica.Tests/Integration/ProvenienzaIntegrationTests.cs` | 42, 69 |
| `tests/DBEnigmistica.Tests/Architecture/DipendenzeTests.cs`, `FornitoreAiArchitetturaTests.cs` | 11 e 12 (`typeof(Game)`): nessuna modifica se `Game` resta (R13) |

### 3.3 Conteggio e rischio 5

- File trovati: 41 (29 sorgenti, 12 test), più due `.Designer.cs` e la snapshot.
- File da modificare o eliminare: circa 36. Ne escludiamo cinque: due falsi positivi in `Domain/AI`, la Baseline e i due test di architettura.
- **Oltre 30: il rischio 5 del PLAN scatta.** B si divide in due consegne sequenziali dello stesso agente Sonnet. B1 si occupa di descrittori, validatori e regole. B2 si occupa del nucleo, con questo elenco. Restano 3 agenti insieme [A].
- Fuori dall'elenco del PLAN, ma necessari a B2: `Domain/Interfaces/IGameRepository.cs`, `IGameSolutionRepository.cs`, `Application/Services/Solutions/**`, `Application/Configuration/DependencyInjection.cs` e `Console/Commands/Solution/{ExportSolutions,ListUnsolved}Command.cs`. Si aggiungono a B2 [A].
- Comandi da tenere funzionanti, che passano dai servizi: `list-games`, `export-games`, `export-stats` e i comandi `solution`.

## 4. Brief degli agenti

Regole comuni, valide per tutti (`CLAUDE.md`, `PLAN.md` sezione 1):
- Ramo `fase-1/ondata-1.2` da `main` aggiornato. Nessun commit senza richiesta del proprietario.
- TDD: prima il test rosso, poi il codice. Copertura delle righe di Domain, Application e Tipologie almeno 80%.
- Prima di consegnare: `dotnet format DBEnigmistica.slnx --include <file propri>`, `dotnet build DBEnigmistica.slnx -c Release -warnaserror`, test della propria area con Docker attivo.
- **Nessuna chiamata AI dal vivo.** Solo l'eseguibile finto e le Registrazioni sintetiche.
- **Nessun testo, pagina, PDF o Registrazione di Riviste** nel repository o nei test. Solo testi inventati dal progetto.
- **Tabelle per Tipologia solo come SQL generato e rivisto**, applicato da `dbe migrate`. Mai DDL costruito a runtime da input.
- Mai aprire `.env` né `appsettings.Development.json`. Nessun segreto nei file.
- Ogni agente tocca solo i propri file. Per un file altrui scrive la modifica esatta nel rapporto.
- Commenti di una frase. Termini del glossario. Messaggi all'utente in italiano.

### Agente A (Sonnet) - Progetto Tipologie, generatore SQL, `types`, `migrate`

**Obiettivo.** Caricare e validare i descrittori. Generare SQL deterministico e sicuro. Applicarlo con `dbe migrate`. Scrivere e leggere le tabelle per Tipologia.

**File esclusivi.**
- `src/DBEnigmistica.Tipologie/**`: nuovo, con `.csproj` e risorse incorporate (R14). Escluso `Validatori/**`, che è di B.
- `tipologie/*/v*.riviste.sql`, `tipologie/*/v*.catalogo.sql` (generati, R15); `.squawk.toml` (nuovo).
- `src/DBEnigmistica.Infrastructure/Tipologie/**`: nuovo. Contiene scrittore, lettore e applicazione delle migrazioni.
- `src/DBEnigmistica.Infrastructure/Migrations/<ts>_TipologieMigrazioniApplicate.cs`: solo questo file, entro il 24/11 (R12).
- `src/DBEnigmistica.Infrastructure/Configuration/DependencyInjection.cs` (R10).
- `src/DBEnigmistica.Console/Commands/MigrateCommand.cs`, `TypesCommand.cs` (nuovo), `Cli/CliApp.cs`, `Commands/HandlerRegistration.cs` (R9).
- `DBEnigmistica.slnx`, `Directory.Packages.props` e tutti i `*.csproj` (R7).
- `.github/workflows/ci.yml` (R8), `.github/scripts/soglia-copertura.py` (aggiunge `DBEnigmistica.Tipologie` a `PACCHETTI`).
- `tests/DBEnigmistica.Tests/Unit/Tipologie/Generatore/**`, `Unit/Tipologie/Caricamento/**`, `Integration/Tipologie/**`, `Architecture/TipologieArchitetturaTests.cs` (nuovi).

**Compiti.**
0. Entro il 24/11, migrazione `tipologie` sul ramo: `Migrations/<ts>_TipologieMigrazioniApplicate.cs` con schema e tabella (R12). È il primo compito di A.
1. Entro il 24/11, consegnare a B e a C i tipi del modello (`Modello/**`) e l'interfaccia del validatore strutturale.
2. Caricatore: validazione contro il meta-schema con Corvus (R5), poi i controlli della sezione 2.4. Accetta `versione` 0.
3. Entro il 25/11, consegnare a C il formattatore del catalogo (5.2) e la derivazione dello schema della risposta per pagina (3.2). Lo schema derivato dei due esempi deve coincidere con `esempi/risposta-pagina/derivato-esempio.schema.json`.
4. Generatore SQL secondo la sezione 3.1: lista chiusa, identificatori validati e quotati, nomi oltre 63 byte accorciati, nessun `DEFAULT`, trigger, funzione, `GRANT`, `ALTER`, `SET`. Salta `versione` 0.
5. `dbe types generate [--check]`: con `--check` esce con un codice diverso da 0 se l'SQL registrato è diverso da quello generato o se manca. Salta `v0`.
6. `dbe migrate`: prima EF, poi le Tipologie dalle risorse. Una transazione per file, con `SET LOCAL lock_timeout = '5s'` e `statement_timeout = '60s'`. Registra in `tipologie.migrazioni_applicate` con il checksum SHA-256. È idempotente. Un checksum alterato dà errore. Quali Tipologie applicare: vedi D2.
7. Scrittore e lettore Npgsql parametrizzati. Gli identificatori vengono solo dal descrittore validato.
8. Passi CI: `dbe types generate --check`; squawk 2.73.0 con versione e checksum fissati e `.squawk.toml` della sezione 8; copertura estesa. Inserire il passo di C.
9. Test di architettura: Tipologie non dipende da Application, Infrastructure, AI, Console, EF o Npgsql.

**Test da scrivere prima.**
- Lo snapshot dell'esempio della sezione 3.1 coincide byte per byte.
- `v0.json` validi: nessun file SQL, `--check` a 0, nessuna tabella creata da `migrate`.
- Payload d'iniezione, parole riservate e tipi fuori lista vengono rifiutati.
- Fuzz con 10.000 descrittori e seme fisso registrato: ognuno è rifiutato o quotato. Quelli accettati si applicano su PostgreSQL in una transazione annullata, e il conteggio delle tabelle fuori da quelle attese non cambia.
- `migrate` due volte: la seconda non fa nulla. Il checksum alterato dà errore.
- Il test d'integrazione verifica `SET LOCAL` prima di ogni file.
- I valori ostili passati allo scrittore si salvano come dati.
- `--check` con un descrittore v1 privo di SQL esce con un codice diverso da 0.

**Fatto quando.** Il criterio 1 e la parte di A del criterio 2 sono verdi in CI. Squawk passa senza errori sugli SQL delle 4 Tipologie. `migrate` gira anche nell'immagine, perché legge le risorse. Il passo di C è inserito in `ci.yml`.

### Agente B (Sonnet) - Due consegne sequenziali

**B1 - Descrittori, validatori, regole (23/11-27/11)**

**File esclusivi.** `tipologie/cruciverba-classico/v1.json`, `tipologie/anagramma/v1.json`, `tipologie/sciarada/v1.json`, `tipologie/cambio-di-lettera/v1.json`; `src/DBEnigmistica.Tipologie/Validatori/**`; `tests/DBEnigmistica.Tests/Unit/Tipologie/Validatori/**`, `Unit/Tipologie/Descrittori/**`; `tipologie/_meta/descrittore.schema.json` (solo `$defs/validatore`, se approvato, R16).

**Compiti.**
1. Entro il 24/11, struttura dei 4 descrittori (campi, elenchi, resa, validatori), consegnata ad A e a C. Famiglie: `cruciverba` per il cruciverba e `gioco-breve` per gli altri tre.
2. Regole in bozza (`stato: bozza`, `versione: 1`), scritte dalle regole del gioco e senza testi di Riviste. Entro il 27/11 vanno tutte e 4 all'agente principale, che le inoltra al proprietario.
3. Validatori strutturali dell'elenco chiuso: numerazione coerente con la griglia, lunghezze, incroci, stesse lettere dell'anagramma, cambio di una lettera, diagramma coerente con la soluzione.
4. Ogni descrittore si legge con entrambi i lettori (R1, R4).

**Test da scrivere prima.** Ogni validatore ha casi validi e non validi. I 4 descrittori sono conformi al meta-schema. Ogni `tipologie/*/v*.json` (compresi i `v0`) si legge con il caricatore di A e con `DescrittoreTipologia.Leggi`. `DerivatoreSchemaCreazione.Generico` funziona su `cruciverba-classico` e `anagramma`.

**Fatto quando.** I descrittori sono stabili (ogni cambio successivo passa dall'agente principale, per R19). I test sono verdi. Le regole sono inviate.

**B2 - Nucleo `riviste` (27/11-3/12)**, dopo l'approvazione di questo elenco da parte dell'agente principale e dopo la migrazione di A.

**File esclusivi.** Quelli della sezione 3.1, tranne `Infrastructure/Configuration/DependencyInjection.cs` e la Baseline. In più: `Domain/Entities/**`, `Domain/Enums/GameType.cs`, `Infrastructure/Data/**`, `Infrastructure/Migrations/**` (tranne il file di A), `Infrastructure/Repositories/**`, `Application/Services/Processing/**`, `Export/**`, `Solutions/**`. Poi i test della sezione 3.2.

**Compiti.** Una migrazione EF del nucleo: `tipologia`, `versione_tipologia`, `stato_tipologia` (`nota` o `proposta`), riquadro `numeric(6,5)` con `CHECK` 0-1, `confidenza`, `avvisi jsonb`, `trascrizione_proposta text NULL` (solo con `proposta`), `magazines.impronta` e `pages.impronta` `bytea UNIQUE`. Eliminare le tabelle di dettaglio, le entità e l'enum. Gestire le righe esistenti come dice D4: se `riviste.games` ha righe, la migrazione si ferma con un messaggio che chiede di svuotarla, senza conversioni. Aggiornare repository ed esportazioni. Tenere `riviste.games` e `"Id"` (R13).

**Test da scrivere prima.**
- Il database rifiuta un riquadro fuori da 0-1.
- Un Gioco con `stato_tipologia = proposta` si salva senza tabella dedicata, con `trascrizione_proposta`.
- `trascrizione_proposta` con `stato_tipologia = nota` viene rifiutata.
- Le esportazioni CSV e JSON non cambiano nel contenuto dei campi rimasti.
- `impronta` duplicata viene rifiutata.
- Su un database vuoto, la migrazione si applica e poi si annulla.

**Fatto quando.** Build e test sono verdi. `list-games`, `export-*` e i comandi `solution` funzionano. La snapshot è rigenerata. `grep` non trova più `GameType`, `CrosswordGame`, `AnagramGame`, `LogicGame`, `RebusGame`, `PromptTemplates` né `Claude*Response` in `src/` e `tests/`, tranne le migrazioni storiche e `Domain/AI`.

### Agente C (Sonnet) - Valutazione, Set sintetico, regressione locale

**Obiettivo.** Rendere misurabile l'estrazione: metriche, regressione riprodotta in CI, script locale per il Set di riferimento.

**File esclusivi.**
- `src/DBEnigmistica.Application/Valutazione/**` (nuovo, con `DependencyInjection.cs`, R11).
- `src/DBEnigmistica.Console/Commands/EvalCommand.cs` (nuovo: `draft`, `extraction`, `sintetico aggiorna-chiavi` nascosto).
- `src/DBEnigmistica.Infrastructure/Services/Pdf/GeneratorePdfSintetici.cs` (solo aggiunte, R18).
- `tests/DBEnigmistica.Tests/Unit/Valutazione/**`, `tests/DBEnigmistica.Tests/Fixtures/SetSintetico/**` (nuovi).
- `scripts/regressione-locale.ps1` (nuovo).

**Compiti.**
1. Primo compito, entro il 26/11: `scripts/regressione-locale.ps1`, con una prima versione minima di `eval extraction --replay` (solo lettura del set, Registrazioni e rapporto), che C scrive per prima insieme allo script. Metriche complete e Linea di base seguono nei compiti successivi. Senza Linea di base la prima versione esce con 0 e stampa che il confronto manca; l'uscita diversa da 0 quando una metrica scende vale da quando la Linea di base esiste. Lo script riceve la cartella come parametro, esegue `--replay`, scrive il rapporto in una cartella con la versione e stampa `gh release upload`. Esce con un codice diverso da 0 se una metrica scende. Nessun dato esce dal PC. Usa il Set sintetico provvisorio: le risposte cambiano il 2/12. Il proprietario lo prova entro il 27/11 (D6).
2. Lettore del set (6.1). Rifiuta id fuori forma e percorsi con `..`, assoluti o esterni. Accetta `generatore` solo con il tipo sintetico (R18).
3. Metriche della sezione 4: abbinamento con IoU ≥ 0,5 e stessa tipologia, CER normalizzato, caselle, campi esatti, gruppi digitale, scansione e totale. Rapporto JSON e Markdown, deterministico.
4. `eval extraction --replay`: Registrazioni tramite `FornitoreAiRiproduci` e versione del prompt dal catalogo e dallo schema di A. Confronto con la Linea di base (6.3). `--live` sopra `IFornitoreAiEstrazione` con limite per esecuzione, provato solo con l'eseguibile finto.
5. `eval draft`: bozze dei risultati attesi da una Registrazione, file JSON per pagina controllato contro lo schema.
6. Set sintetico s01-s06 (6.4), con testi inventati. Contiene un errore voluto e un Gioco in più. Le risposte stanno in `risposte/<id>.json`, poi ci sono le Registrazioni e la Linea di base (R19). Le ricalcola il 2/12, quando i descrittori sono finali.
7. Testo esatto del passo CI, consegnato ad A entro il 2/12.
8. Righe per `CliApp.cs` e `HandlerRegistration.cs`, consegnate ad A (R9).

**Test da scrivere prima.**
- Metriche su casi costruiti a mano: Gioco mancante, Gioco in più, tipologia sbagliata, IoU sotto 0,5, CER con maiuscole e apostrofi, caselle.
- Due esecuzioni producono lo stesso rapporto.
- Una metrica sotto la Linea di base dà un codice diverso da 0.
- Una pagina senza Registrazione dà un errore esplicito e zero chiamate.
- Un `set.json` con `..` o con un percorso assoluto viene rifiutato.
- `aggiorna-chiavi` su un set di riferimento viene rifiutato.

**Fatto quando.** `eval extraction --replay` sul Set sintetico è verde in CI contro la sua Linea di base, e la Linea di base non è tutta al 100%. Lo script locale produce il rapporto sul Set sintetico. In CI non avviene alcuna chiamata.

### Agente D (Haiku) - Documentazione, dopo A, B e C (4/12)

**File esclusivi.** `docs/CI.md` (passi nuovi e checklist di release con il rapporto della regressione locale, D2), `README.md` (comandi `types`, `migrate`, `eval`), `docs/handoff/05-regole-tipologie-fase-1.md` (date nuove, sezione 5), `docs/handoff/13-regressione-locale-e-release.md` (uso dello script), `CLAUDE.md` (solo sezione 6, comandi).

**Compiti.** Descrivere solo comandi, chiavi e comportamenti che esistono nel ramo. Usare frasi brevi e nessun nome di classe interna.

**Fatto quando.** L'agente principale verifica ogni comando citato contro il ramo.

### Revisione finale (Opus `xhigh`, 5/12)

Sul diff completo, con attenzione a:
- quotatura del generatore;
- scrittore parametrizzato;
- `SET LOCAL` e checksum di `migrate`;
- migrazione del nucleo (righe esistenti, `CHECK`, permessi);
- percorsi del lettore del set;
- determinismo del rapporto;
- assenza di testi di Riviste.

Poi gitleaks. Vanno corretti tutti i CRITICAL e gli HIGH.

## 5. Date

La 1b.1 si è chiusa il 21/11/2026 (`PROGRESS.md`), circa due settimane prima del previsto (6/12). Le scadenze esterne non cambiano.

| Data | Evento |
|---|---|
| 23/11 | Avvio della 1.2: A, B1 |
| 24/11 | A consegna i tipi del modello. B1 consegna la struttura dei 4 descrittori |
| 24/11 | Migrazione `tipologie` di A sul ramo; avvio di C |
| 25/11 | A consegna catalogo e derivazione a C |
| 26/11 | C consegna `scripts/regressione-locale.ps1` con il Set sintetico provvisorio |
| 27/11 | Regole in bozza (tutte e 4) al proprietario. Il proprietario prova lo script. Inizio di B2 |
| 2/12 | C ricalcola le Registrazioni sintetiche e consegna il passo CI |
| 3/12 | B2 consegna il nucleo. Fine di A e C |
| 4/12 | Haiku D. PR e CI. Il proprietario approva tutte e 4 le regole |
| 5/12 | Revisione Opus `xhigh` e correzioni |
| 6/12 | Rilancio di controllo dello script da parte del proprietario (circa 5 minuti), approvazione dell'ondata, unione, riga `PHASE:1 WAVE:1.2 STATUS:complete` |

Decisa (D1): **1.2 dal 23/11 al 6/12/2026**. Seguono, come indicazione: 1b.2 dal 7/12 al 20/12, 1.3 dal 21/12/2026 al 3/1/2027, 1.4 dal 18/1/2027 accanto alla 1b.3 (invariata).

Rischio: se la migrazione di A slitta oltre il 24/11, la consegna dello script del 26/11 slitta di altrettanto.

Scadenze esterne invariate:
- condizioni d'uso: 6/12;
- `1.1-prova`: 13/12;
- regole v0 e brief: 3/1;
- prototipo: verso il 10/1, scadenza 17/1;
- prova 1: dal 18/1 al 21/2;
- `v0.2.0`: 21/2;
- G1: 28/2/2027.

Il tempo guadagnato resta margine.

## 6. Domande per il proprietario

1. **Date della 1.2: dal 23/11 al 6/12/2026, al posto di 7/12-20/12?** Raccomandazione: sì. Nessuna dipendenza esterna, e il margine resta prima della prova 1.

   **Decisione:** sì, la 1.2 va dal 23/11 al 6/12/2026 (D1).
2. **Quali Tipologie applica `dbe migrate`?** La ROADMAP dice "approvate", il criterio 2 chiede le 4 tabelle, ma le regole saranno ancora in bozza. Raccomandazione: per "approvata" si intende l'SQL rivisto e unito in `main` (che passa `--check`). Lo stato delle `regole` conta solo per la creazione (ADR 002, punto 3). Le versioni `v0` non si applicano mai.

   **Decisione:** opzione raccomandata. "Approvata" vuol dire SQL rivisto, unito in `main`, con `--check` superato. Le `v0` non si applicano mai (D2).
3. **Quando approvi le regole di `cruciverba-classico` e `anagramma`?** Raccomandazione: entro il 4/12 (le ricevi il 27/11, circa 30 minuti), così la 1b.2 parte il 7/12 con regole approvate. Sciarada e cambio di lettera entro il 18/12. La scadenza della handoff 05 si anticipa.

   **Decisione:** bozze delle 4 regole al proprietario entro il 27/11; approvazione di tutte e 4 entro il 4/12 (D3).
4. **Ci sono Giochi del vecchio codice nel tuo database locale che vuoi tenere?** Raccomandazione: no. La migrazione del nucleo elimina le tabelle di dettaglio. Se `riviste.games` ha righe, la migrazione si ferma con un messaggio che chiede di svuotarla, senza convertire dati vecchi e non verificati.

   **Decisione:** opzione raccomandata. Nessun Gioco del vecchio codice da tenere; se `riviste.games` ha righe, la migrazione si ferma (D4).
5. **Colonne di `export-games` in CSV.** `GameType` e la posizione in pixel spariscono. Raccomandazione: al posto di `GameType` la colonna `Tipologia`; al posto di `PositionX`, `PositionY`, `Width` e `Height` le colonne `X`, `Y`, `Larghezza` e `Altezza` normalizzate tra 0 e 1. Le altre colonne restano uguali. Nessuna compatibilità con il formato vecchio.

   **Decisione:** opzione raccomandata. Colonne `Tipologia`, `X`, `Y`, `Larghezza`, `Altezza` (normalizzate 0-1); nessuna compatibilità (D5).
6. **Puoi provare `scripts/regressione-locale.ps1` sul Set sintetico tra il 4 e il 6/12?** È un criterio di uscita. Servono circa 15 minuti, con la guida della handoff 13. Raccomandazione: sì. Se non puoi, l'ondata si chiude con la prova come compito aperto, come per `1.1-prova`.

   **Decisione:** sì, sul Set sintetico provvisorio: prova entro il 27/11, rilancio dopo il ricalcolo del 2/12, senza nuova approvazione (D6).

## 7. Decisioni del proprietario (21/11/2026)

| # | Decisione |
|---|---|
| D1 | La 1.2 va dal 23/11 al 6/12/2026, con le date indicative delle ondate successive della sezione 5. |
| D2 | "Approvata" vuol dire SQL rivisto, unito in `main`, con `--check` superato; lo stato delle `regole` conta solo per la creazione, e le `v0` non si applicano mai. |
| D3 | Le bozze delle 4 regole arrivano al proprietario entro il 27/11; il proprietario le approva tutte e 4 entro il 4/12. |
| D4 | Nessun Gioco del vecchio codice da tenere; se `riviste.games` ha righe, la migrazione si ferma e chiede di svuotarla, senza conversioni. |
| D5 | Colonne `Tipologia`, `X`, `Y`, `Larghezza`, `Altezza` (normalizzate 0-1) al posto delle vecchie; nessuna compatibilità col formato vecchio. |
| D6 | C consegna `scripts/regressione-locale.ps1` entro il 26/11 con un Set sintetico provvisorio; il proprietario lo prova entro il 27/11 e lo rilancia dopo il ricalcolo del 2/12, senza nuova approvazione. |
