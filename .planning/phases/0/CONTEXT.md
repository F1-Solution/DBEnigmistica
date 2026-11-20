# Fase 0 - Fondamenta: contesto

Aggiornato l'11/11/2026, prima dell'ondata 0.2. Fonte di verità: `ROADMAP.md`, sezione "Fase 0 - Fondamenta". Se questo file e il roadmap divergono, vince il roadmap.

## Obiettivo

Un codice che compila senza avvisi, con test verdi su PostgreSQL vero e una CLI funzionante. Segreti fuori dai file, CI attiva. Il dominio contiene già il modello di provenienza, diritti e registro, su cui si appoggia tutto il resto (criteri di accettazione 1-13 del roadmap).

## Stato verificato sul codice (11/11/2026)

- Ramo `fase-0-fondamenta` a `bfcdb30`, uguale a `origin/main` (PR #12 unita). Le ondate 0.1-A, 0.1-B e 0.1-C sono in `main` (`PROGRESS.md`). La riga di stato del roadmap che le dice "da unire" è superata.
- `dotnet build -c Release --no-incremental`: **0 avvisi, 0 errori**. Il criterio 1 (`-warnaserror`) oggi passerebbe.
- `dotnet test`: **124 superati**, 0 falliti, 0 saltati, con Docker attivo. Integrazione su `postgres:17-alpine`.
- Copertura delle righe: Domain 57,9%, Application 68,4%, insieme **65,7%** (360 su 548). Il criterio 11 chiede 80%. Righe scoperte: `DataExportService` (76), `SchemaProposalService` (35), entità di dettaglio mai usate (`AnagramGame`, `CrosswordGame`, `LogicGame`, `RebusGame`).
- `dotnet format --verify-no-changes`: **267 errori WHITESPACE in 18 file**. Mancano `.editorconfig` e `.gitattributes`; `core.autocrlf=true`; nella cartella di lavoro 60 file hanno CRLF, 87 LF, 1 misto.
- `Anthropic.SDK` 5.9.0 è ancora in `Directory.Packages.props` e in `DBEnigmistica.AI.csproj`, usato solo da `ClaudeApiClient` e `ClaudeGameAnalyzer`.
- La configurazione AI è la sezione `Anthropic` (chiave API, modello, MaxTokens), validata solo con `HostRequirements.Ai`. **Nessun comando oggi chiede `HostRequirements.Ai`**: tutti i comandi con AI rispondono "disponibile dalla fase N" (codice 2).
- `Dockerfile` e `publish.yml` non contengono più `cp -n`; `.env.template` non cita più Tesseract. Restano riferimenti a `Anthropic__ApiKey` in `Dockerfile` (commento), `docs/CI.md`, `CLAUDE.md`, `.env.template` e `appsettings.Development.json.template`.
- Migrazioni EF del 31/01/2026 (`InitialCreate`, `AddGameSolutions`): mai applicate. Tabelle nello schema `public`, nomi inglesi, colonne PascalCase.
- `games.SolutionPageId` ha la chiave esterna senza `ON DELETE` (NO ACTION): cancellare una pagina-soluzione ancora usata è rifiutato. `game_solutions.SolutionPageId` invece ha `ON DELETE SET NULL`.
- Tutte le colonne di data esistenti sono già `timestamp with time zone`.
- `docker-compose.yml` usa PostgreSQL 16; i test usano 17. Deciso il 13/11/2026: PostgreSQL 18 per test e Compose (`docs/fase-0-chiusura-decisioni.md`).
- `.planning/` è in `.gitignore`: questi due documenti non vengono tracciati da git.

## Decisioni vincolanti

Del proprietario, 11/11/2026:
1. Giochi logici della Prova 1: **sudoku, futoshiki e nonogramma**. Riguarda la fase 1b: nella fase 0 nessun lavoro.
2. Titolare dei diritti: **anagrafica unica dei Soggetti** (persona o organizzazione), usata per titolare, licenziante e cliente di una Cessione.
3. Cancellazione di una pagina usata come pagina-soluzione da un Gioco di un'altra Rivista: **rifiuto**. Il messaggio dice quali Giochi la usano. Test d'integrazione (criterio 13).
4. Stato di inedito: **due stati senza ritorno**. Inedito finché non c'è una prima pubblicazione, di cui si salvano data e luogo. Il non-ritorno è protetto da un vincolo del database.
5. Prove dell'Origine per `licenza` (licenziante, estremi dell'accordo, periodo) e `pubblico dominio` (autore, anno, motivo): **obbligatorie alla pubblicazione, non al salvataggio**. Il Gioco si salva incompleto; la `PolicyDiPubblicazione` lo blocca finché mancano. L'Origine resta `NOT NULL` senza default.

Dai documenti (`ROADMAP.md`, `docs/fase-0-decisioni.md`):
- Studio sul PC Windows del proprietario con Docker Desktop.
- Nessuna API a consumo: Claude Code non interattivo con l'abbonamento è il Fornitore AI predefinito. Nessuna chiave richiesta con il fornitore predefinito.
- Nella fase 0 non entra alcun pacchetto di un Fornitore AI (criterio 6).
- Migrazioni: nuova baseline. Nessun DDL a runtime.
- Ruoli PostgreSQL per contesto: `dbe_estrazione`, `dbe_creazione` (non legge `riviste`), `dbe_originalita`, `dbe_consegna`.
- Tutte le date in `timestamptz`, valori in UTC.

## Vincoli non negoziabili che toccano la fase

- I Giochi estratti non si pubblicano mai: origine `rivista` mai pubblicabile, qualunque sia il resto (criterio 10).
- Registro in sola aggiunta: niente `UPDATE` né `DELETE` su `registro.eventi`, nemmeno dall'applicazione.
- Tabelle per Tipologia solo da migrazioni riviste; tipi e indici da un elenco fisso.
- Nessun segreto nei file tracciati; gitleaks in CI e in pre-commit.
- Test e pubblicazione solo con GitHub Actions. La fase non si chiude senza CI verde e approvazione del proprietario.
- Non aprire `.env` né `src/DBEnigmistica.Console/appsettings.Development.json`.

## Fuori dall'ondata 0.2

- **Ondata 0.3** (Haiku A, Sonnet B): `CLAUDE.md`, ADR 001-006, README e SETUP riscritti, `docs/CI.md` aggiornato (anche i nuovi nomi di configurazione del Fornitore AI e l'hook gitleaks), avviso "superato" su `docs/plan.md`; comando `dbe verify-integrity` e test end-to-end della CLI in CI; tag `v0.1.0`.
- **Revisione** di fine fase: Opus con sforzo massimo sul diff completo.
- **Proprietario**: impostazioni manuali di `docs/CI.md` (protezione di `main` e dei tag, GHCR privato, Dependabot), lettura degli ADR, domande al legale entro il 15/12/2026, set di PDF di prova prima della fase 1.
