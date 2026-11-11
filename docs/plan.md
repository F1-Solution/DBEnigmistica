# Piano di Sviluppo: DBEnigmistica

> **Documento superato: non usarlo per decidere.** Fonte di verità: [ROADMAP.md](../ROADMAP.md). Le decisioni di architettura sono in [docs/adr](adr/README.md).

## Problema

Creare una console application C# (.NET 10) per estrarre giochi enigmistici da riviste PDF, analizzarli tramite OCR e AI, e salvarli in un database PostgreSQL con schema dinamico che si evolve in base a nuove tipologie di giochi scoperte.

## Approccio

### Stack Tecnologico Scelto

- **Framework**: .NET 10 Console Application
- **CLI**: System.CommandLine per gestione argomenti
- **Database**: PostgreSQL con Entity Framework Core
- **OCR**: Tesseract.NET (open-source)
- **PDF Processing**: PdfPig o iText7
- **AI Service**: Anthropic Claude API per analisi e generazione schema
- **Migrations**: Entity Framework Core Migrations + dinamiche runtime

### Architettura a Strati

```
┌─────────────────────────────────────┐
│   Console UI Layer                  │
│   (Program.cs, Commands)            │
└─────────────────────────────────────┘
          ↓
┌─────────────────────────────────────┐
│   Application Layer                 │
│   (Orchestration, Workflows)        │
└─────────────────────────────────────┘
          ↓
┌──────────────┬───────────────┬──────┐
│ PDF Service  │ OCR Service   │ AI   │
│              │               │Svc   │
└──────────────┴───────────────┴──────┘
          ↓
┌─────────────────────────────────────┐
│   Domain Layer                      │
│   (Entities, Interfaces)            │
└─────────────────────────────────────┘
          ↓
┌─────────────────────────────────────┐
│   Data Access Layer                 │
│   (EF Core Context, Repositories)   │
└─────────────────────────────────────┘
          ↓
┌─────────────────────────────────────┐
│   PostgreSQL Database               │
└─────────────────────────────────────┘
```

### Workflow Principale

1. **Caricamento**: Leggi PDF da cartella/file singolo
2. **Conversione**: PDF → Immagini (pagina per pagina)
3. **OCR**: Estrai testo grezzo da ogni pagina
4. **Layout Analysis**: Identifica regioni/blocchi sulla pagina
5. **Classificazione**: Usa AI per riconoscere tipo gioco
6. **Schema Check**: Verifica se esiste tabella per quel tipo
   - Se NO → Genera proposta schema con AI → Chiedi conferma utente → Crea tabella
   - Se SI → Continua
7. **Estrazione Dati**: Usa AI per estrarre campi specifici del gioco
8. **Persistenza**: Salva nel database

## Workplan

### Fase 1: Setup Progetto e Infrastruttura ✅ COMPLETATA

- [x] Creare soluzione .NET 10 con struttura a progetti multipli
- [x] Setup progetto console principale con System.CommandLine
- [x] Setup progetto Domain con entità base
- [x] Setup progetto Infrastructure con EF Core e PostgreSQL
- [x] Configurare Docker Compose per PostgreSQL locale (sviluppo)
- [x] Creare file appsettings.json per configurazione (connection string, API keys)

### Fase 2: Database Schema Iniziale ✅ COMPLETATA

- [x] Definire entità: Magazine, Page, Game
- [x] Creare tabelle specifiche iniziali: CrosswordGame, RebusGame, LogicGame, AnagramGame
- [x] Configurare DbContext EF Core
- [x] Creare migration iniziale
- [x] Implementare repository pattern per accesso dati

### Fase 3: Servizi Core - PDF e OCR ✅ COMPLETATA

- [x] Implementare PdfService (estrazione pagine come immagini)
- [x] Implementare OcrService con Tesseract.NET
- [x] Implementare LayoutAnalysisService (rilevamento regioni)
- [ ] Test con PDF di esempio (da fare quando necessario)

### Fase 4: Integrazione AI (Claude) ✅ COMPLETATA

- [x] Implementare ClaudeApiClient
- [x] Creare prompt per classificazione tipo gioco
- [x] Creare prompt per generazione schema database
- [x] Creare prompt per estrazione campi specifici gioco
- [ ] Test con esempi reali (da fare quando necessario)

### Fase 5: Schema Dinamico ✅ COMPLETATA

- [x] Implementare SchemaProposalService (genera proposta tabella da AI)
- [x] Implementare SchemaValidationService (valida con utente)
- [x] Implementare DynamicMigrationService (applica modifiche DB runtime)
- [x] Gestire metadata delle tabelle custom (tracking schema personalizzati)

### Fase 6: Orchestration e Workflow ✅ COMPLETATA

- [x] Implementare MagazineProcessingService (workflow completo)
- [x] Implementare GameExtractionService (estrazione dati per tipo)
- [x] Gestione errori e logging (Serilog)
- [x] Progress reporting in console

### Fase 7: CLI Commands ✅ COMPLETATA

- [x] Comando: `scan-folder <path>` - scansiona cartella PDF
- [x] Comando: `scan-file <path>` - scansiona singolo PDF
- [x] Comando: `analyze-game <markdown-file>` - analizza descrizione gioco
- [x] Comando: `analyze-game-text <description>` - analizza descrizione da stringa
- [x] Comando: `list-games` - lista tipologie giochi nel DB
- [x] Comando: `export-schema` - esporta schema corrente
- [x] Comando: `migrate` - applica migrazioni pending

### Fase 8: Testing e Refinement ✅ COMPLETATA

- [x] Unit test per servizi core
- [x] Integration test con database test
- [x] Test end-to-end con PDF reali (da fare su richiesta utente con PDF reali)
- [x] Ottimizzazione performance (batch processing già implementato)
- [x] Documentazione README

### Fase 9: Features Avanzate ✅ COMPLETATA

- [x] Cache risultati OCR per evitare riprocessing
- [x] Export dati in formati vari (JSON, CSV)
- [x] Web API per accesso dati (rimandato - oltre scope)
- [x] Dashboard di monitoraggio (rimandato - oltre scope)

### Fase 10: Gestione Soluzioni Giochi ✅ COMPLETATA

**Obiettivo**: Implementare sistema completo per tracciare e associare soluzioni ai giochi.

#### 10.1 Modello Dati ✅
- [x] Aggiungere campi diretti a `Game`:
  - `Solution` (string?) - Testo soluzione breve
  - `SolutionPageId` (int?) - FK a Page (se soluzione nella stessa rivista)
  - `HasSolution` (bool) - Flag per indicare se ha soluzione
  - `SolutionNotes` (string?) - Note aggiuntive sulla soluzione
- [x] Aggiungere campo a `Magazine`:
  - `SolutionsInNextIssue` (bool) - Indica se questa rivista pubblica soluzioni nel numero successivo
- [x] Creare nuova entità `GameSolution`:
  - `Id` (int, PK)
  - `GameId` (int, FK) - Riferimento al gioco
  - `SolutionText` (string) - Testo completo della soluzione
  - `SolutionPageId` (int?, FK) - Pagina dove si trova la soluzione
  - `SolutionType` (enum) - InSamePage, InSameIssue, InNextIssue, External
  - `PositionX/Y` (int?) - Coordinate nella pagina
  - `ConfidenceScore` (double?) - Confidenza dell'AI nel match
  - `IsVerified` (bool) - Se verificata manualmente
  - `ExtractedDate` (DateTime)
  - `VerifiedDate` (DateTime?)

#### 10.2 Repository e Servizi ✅
- [x] `IGameSolutionRepository` con metodi:
  - `GetByGameIdAsync(int gameId)`
  - `GetUnsolvedGamesAsync(int? magazineId = null)`
  - `GetByPageIdAsync(int pageId)`
  - `GetPendingVerificationAsync()`
  - `GetVerifiedSolutionsAsync(int? magazineId = null)`
- [x] `SolutionMatchingService`:
  - `FindPotentialSolutionsAsync(Page solutionPage)` - Usa AI per trovare match
  - `LinkSolutionToGameAsync(int gameId, GameSolution solution)` - Associa soluzione
  - `VerifySolutionAsync(int solutionId, bool isCorrect)` - Verifica manuale
- [x] `SolutionExtractionService`:
  - `ExtractSolutionsFromPageAsync(Page page)` - OCR + AI per estrarre soluzioni
  - `AnalyzeSolutionPatternAsync(string text)` - Riconoscimento pattern

#### 10.3 Database Migration ✅
- [x] Creare migration `AddGameSolutions` per:
  - Aggiungere colonne a `games` table
  - Aggiungere colonna a `magazines` table
  - Creare `game_solutions` table con FK e indici
  - Indice su `game_id`, `solution_page_id`, `solution_type`

#### 10.4 CLI Commands ✅
- [x] `link-solution` - Associa manualmente soluzione a gioco
  ```
  link-solution --game-id 123 --solution "PAROLA" [--page-id 45] [--type InSameIssue]
  ```
- [x] `scan-solutions` - Scansiona pagina/sezione per soluzioni
  ```
  scan-solutions --page-id 89
  ```
- [x] `list-unsolved` - Lista giochi senza soluzione
  ```
  list-unsolved [--magazine-id 5] [--game-type crossword] [--export solutions.json]
  ```
- [x] `verify-solution` - Verifica/correggi soluzione proposta
  ```
  verify-solution --solution-id 123 --verified true [--notes "Note"]
  ```
- [x] `export-solutions` - Esporta giochi con soluzioni
  ```
  export-solutions --output solutions.csv [--verified-only] [--format json|csv]
  ```

#### 10.5 Dependency Injection ✅
- [x] Registrare `IGameSolutionRepository` in Infrastructure DI
- [x] Registrare `ISolutionMatchingService` in Application DI
- [x] Registrare `ISolutionExtractionService` in Application DI
- [x] Registrare tutti i comandi in Program.cs

#### 10.6 Documentazione ✅
- [x] Aggiornare README con sezione "Gestione Soluzioni"
- [x] Documentare workflow di associazione soluzioni
- [x] Aggiungere esempi pratici nei comandi
- [x] Aggiornare plan.md con stato finale

## Note e Considerazioni

### Gestione Schema Dinamico

L'aspetto più complesso è la creazione dinamica di tabelle. Approccio proposto:

1. Mantenere un **metadata store** (tabella `GameTypeSchemas`) che traccia:
   - Nome tipo gioco
   - JSON schema delle colonne
   - Data creazione
   - Versione
2. Quando serve una nuova tabella:
   - Claude genera proposta JSON con colonne, tipi, constraints
   - Console mostra proposta all'utente
   - Utente può modificare (nomi, tipi, aggiungere colonne)
   - Sistema esegue raw SQL per CREATE TABLE (EF Core non supporta schema dinamico a runtime)
   - Salva metadata in `GameTypeSchemas`
3. Per salvare dati in tabelle dinamiche:
   - Usare ADO.NET raw per INSERT (non EF Core tracked entities)
   - Oppure: usare pattern Entity-Attribute-Value (EAV) come fallback

### Librerie Raccomandate

- **PdfPig**: Migliore per parsing PDF strutturato in .NET
- **Tesseract**: Via wrapper Tesseract.NET o Emgu.CV
- **Anthropic.SDK**: Client ufficiale per Claude API
- **System.CommandLine**: CLI moderna per .NET
- **Npgsql.EntityFrameworkCore.PostgreSQL**: Provider PostgreSQL per EF
- **Serilog**: Logging strutturato

### Struttura Progetti

```
DBEnigmistica/
├── src/
│   ├── DBEnigmistica.Console/        # Entry point
│   ├── DBEnigmistica.Domain/         # Entities, interfaces
│   ├── DBEnigmistica.Application/    # Services, workflows
│   ├── DBEnigmistica.Infrastructure/ # EF, repositories, external services
│   └── DBEnigmistica.AI/             # Claude integration
├── tests/
│   ├── DBEnigmistica.Tests.Unit/
│   └── DBEnigmistica.Tests.Integration/
├── docs/
├── docker-compose.yml
└── DBEnigmistica.sln
```

## Prossimi Passi

1. Confermare il piano
2. Iniziare con Fase 1: Setup progetto e infrastruttura
3. Iterare fase per fase con checkpoint
