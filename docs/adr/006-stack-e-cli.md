# ADR 006: stack e riga di comando

- **Stato:** Accettato il 13/11/2026
- **Data:** 11/11/2026

## Contesto

Il progetto parte da un codice con migrazioni mai applicate, una CLI che non funzionava, test su un database in memoria che non verificava le regole del database, e nessuna CI. La fase 0 deve dare fondamenta verificabili: codice che compila senza avvisi, test su PostgreSQL vero, CLI funzionante, pubblicazione automatica.

## Decisione

1. **.NET 10** (`net10.0`), con gestione centralizzata dei pacchetti in `Directory.Packages.props`. Versioni principali: EF Core 10.0.14 con Npgsql 10.0.3; Microsoft.Extensions.Hosting 10.0.14.
2. **PostgreSQL** come unico database, con schemi e ruoli per contesto (ADR 001). Le date sono `timestamptz` in UTC. Il DDL a runtime non esiste (ADR 002).
3. **Migrazioni EF**: una baseline rigenerata da zero, più una migrazione scritta a mano per le parti che EF non esprime (colonne generate, vincoli di esclusione, trigger, ruoli, permessi). Le migrazioni del 31/01/2026 non sono mai state applicate e sono state eliminate.
4. **Riga di comando** `dbe` su System.CommandLine 2.0.15, con Generic Host (`Microsoft.Extensions.Hosting`). I servizi si risolvono dal contenitore con validazione alla costruzione (`ValidateOnBuild`, `ValidateScopes`). Le opzioni usano l'options pattern con `ValidateOnStart`. Il logging passa per Serilog. L'interfaccia di console (Spectre.Console) sta nel progetto Console, mai nell'Application layer.
5. **Codici d'uscita** restituiti dai gestori. Nessun `Environment.Exit`. Un comando non ancora disponibile esce con 2 e il messaggio "Comando disponibile dalla fase N".
6. **Configurazione**: `appsettings.json` copiato in output dal progetto, poi `appsettings.<ambiente>.json`, poi user-secrets (solo ambiente Development), poi variabili d'ambiente. Nessun segreto nei file tracciati (ADR 004).
7. **Test**: xUnit con FluentAssertions e Moq. I test d'integrazione usano Testcontainers con PostgreSQL 18. Nessun test usa un database in memoria. I test di architettura (NetArchTest) verificano le direzioni delle dipendenze, compreso il divieto per Application di dipendere da `Anthropic`. Un test separato verifica che né `Directory.Packages.props` né i progetti dichiarino pacchetti `Anthropic*` (fase 0, criterio 6). Il confinamento degli adattatori in Infrastructure arriva con la fase 1.
8. **Pubblicazione solo da GitHub Actions.** `ci.yml` esegue build con avvisi come errori, formato, test con copertura (soglia dell'80% per Domain e Application), vulnerabilità dei pacchetti, gitleaks e build dell'immagine Docker. `publish.yml` gira su tag `vX.Y.Z` o avvio manuale, rifiuta i commit che non discendono da `main` e pubblica l'immagine su GHCR e gli eseguibili. Nessuna pubblicazione manuale.
9. **Librerie native** non ammesse nell'immagine finale, con eccezione da definire nella fase 1 (PDFium, ADR 003).

## Conseguenze

- I test d'integrazione richiedono Docker in esecuzione, anche in locale. Un test senza Docker non può dare un esito valido.
- PostgreSQL 18 è la versione unica per i test e per Compose (`docker-compose.yml`), come deciso il 13/11/2026 (`docs/fase-0-chiusura-decisioni.md`). Un volume creato con la 16 va ricreato o esportato con `pg_dump`.
- Il progetto di console contiene la configurazione dei comandi; la logica di dominio resta nei progetti interni. Ogni nuovo comando passa da `CliApp` e da `HandlerRegistration`.
- La pubblicazione richiede che il tag punti a un commit di `main`. Un tag creato su un ramo di lavoro fallisce prima di pubblicare.
- Ogni fase aggiunge i propri controlli alla CI, senza toglierne quelli già presenti.

## Alternative scartate

- **Test su database in memoria** (`UseInMemoryDatabase`): non verifica i vincoli, i trigger né i ruoli. Eliminato dai test.
- **`Environment.Exit` negli handler** (23 occorrenze): eliminato. Il codice d'uscita si restituisce dal gestore.
- **System.CommandLine in versione precedente**: sostituita dalla 2.0 stabile.
- **Pacchetto non ufficiale per l'AI** (`Anthropic.SDK`): eliminato (ADR 004).
- **Tabelle create a runtime** per le Tipologie: eliminate (ADR 002).
- **Immagine con librerie di sistema per l'OCR** (Tesseract, SkiaSharp): eliminate (ADR 003).
- **Azione ufficiale gitleaks**: richiede una licenza nei repository di organizzazione. Si usa il programma da riga di comando, verificato con checksum (`docs/CI.md`).

## Riferimenti

- `ROADMAP.md`, sezione 1.3 (CI e pubblicazione), sezione 3, fase 0 (riuso, riscrittura, eliminazione; criteri 1-13).
- `.planning/phases/0/PLAN.md`, sezione 1.1 (scelte tecniche) e 1.7 (baseline delle migrazioni).
- `docs/CI.md`.
- `Directory.Packages.props`, `DBEnigmistica.slnx`, `Dockerfile`, `docker-compose.yml`, `.github/workflows/ci.yml`, `.github/workflows/publish.yml`.
- Requisiti: TEC-01, TEC-02, TEC-03, TEC-07, TEC-11, TEC-12, CLI-01, OUT-03 (fase 0 del roadmap).
