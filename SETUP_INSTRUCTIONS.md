# Preparazione dell'ambiente di sviluppo

Questa guida porta un PC Windows (o Linux) dallo stato di repository clonato a un database locale, una configurazione valida e i test verdi. Lo stato del progetto è in [ROADMAP.md](ROADMAP.md). Le decisioni di architettura sono in [docs/adr](docs/adr/README.md).

## Indice

1. Prerequisiti
2. Segreti e configurazione: dove stanno
3. Database locale con Docker Compose
4. Stringa di connessione
5. Migrazioni
6. Fornitore AI e `dbe check-ai`
7. Test
8. Hook gitleaks
9. Risoluzione dei problemi

## 1. Prerequisiti

- **.NET 10 SDK** (`dotnet --version` deve restituire `10.0.x`).
- **Docker Desktop**, in esecuzione. Serve per PostgreSQL in locale e per i test d'integrazione, che usano Testcontainers.
- **Git**.
- Per il Fornitore AI predefinito: **Claude Code** installato e autenticato (vedi sezione 6). Non serve una chiave API.
- Per l'hook dei segreti: **gitleaks** (sezione 8).

## 2. Segreti e configurazione: dove stanno

Nessun segreto sta nei file tracciati da git. Le fonti sono queste, nell'ordine in cui il programma le legge:

| Fonte | Quando si usa | Note |
|---|---|---|
| `appsettings.json` | Sempre | Contiene solo valori non sensibili (per esempio il Fornitore AI predefinito). |
| `appsettings.<ambiente>.json` | Sempre, per l'ambiente corrente (`DOTNET_ENVIRONMENT`, predefinito Production) | Ignorato da git. Il modello di partenza è `appsettings.Development.json.template`. |
| User-secrets | Solo ambiente Development (`dotnet run`) | Configurati con `dotnet user-secrets`. |
| Variabili d'ambiente | Sempre (anche in produzione e nel container) | Hanno la precedenza sulle altre fonti. |
| `.env` | Solo per Docker Compose | Ignorato da git. Il modello è `.env.template`. |

Regola: i valori reali stanno in `.env`, in user-secrets o nelle variabili d'ambiente. I file `.template` contengono solo nomi e segnaposto.

## 3. Database locale con Docker Compose

1. Copia il modello e imposta i valori:

   ```bash
   cp .env.template .env
   ```

   Imposta in `.env` le variabili `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `PGADMIN_DEFAULT_EMAIL` e `PGADMIN_DEFAULT_PASSWORD`. Se una variabile manca, Compose si ferma con un messaggio che la nomina.

2. Avvia PostgreSQL:

   ```bash
   docker compose up -d postgres
   docker compose ps
   ```

   Il servizio `postgres` deve risultare `healthy`. Il database ascolta solo su `127.0.0.1:5432`. Il servizio `pgadmin` è su `127.0.0.1:5050`.

Il file `docker-compose.yml` usa PostgreSQL 16. I test d'integrazione usano PostgreSQL 17. Le due versioni sono da allineare (vedi `docs/adr/006-stack-e-cli.md`).

## 4. Stringa di connessione

Il programma legge la stringa di connessione dalla chiave `ConnectionStrings:DefaultConnection`. Senza di essa i comandi che usano il database si fermano con un messaggio che indica le due alternative.

Scegli una delle due forme. Sostituisci i segnaposto con i valori di `.env`.

**Con user-secrets** (per `dotnet run`, ambiente Development):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=<database>;Username=<utente>;Password=<password>" --project src/DBEnigmistica.Console
```

**Con variabile d'ambiente** (sempre valida, anche per `dbe` già compilato e per le migrazioni di EF):

```powershell
# PowerShell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=<database>;Username=<utente>;Password=<password>"
```

```bash
# Bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=<database>;Username=<utente>;Password=<password>"
```

Non scrivere la password in file tracciati. Il file `appsettings.Development.json.template` contiene un segnaposto da sostituire solo nella copia locale ignorata da git.

## 5. Migrazioni

Le migrazioni applicano lo schema e i ruoli PostgreSQL (schemi `riviste`, `catalogo`, `tipologie`, `registro`).

```bash
dotnet run --project src/DBEnigmistica.Console -- migrate
```

Il comando è idempotente: una seconda esecuzione non modifica nulla.

Per creare una nuova migrazione di EF Core serve lo strumento `dotnet-ef` e la variabile `ConnectionStrings__DefaultConnection` impostata (la fabbrica del contesto la legge dall'ambiente, anche se la migrazione non si connette):

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <Nome> --project src/DBEnigmistica.Infrastructure --startup-project src/DBEnigmistica.Console
```

Le migrazioni scritte a mano (per esempio `VincoliRuoliERegistro`) non si rigenerano con EF: si modificano a mano.

## 6. Fornitore AI e `dbe check-ai`

Il programma raggiunge l'AI tramite il **Fornitore AI**, configurato nella sezione `FornitoreAi`. Il predefinito è Claude Code non interattivo con l'abbonamento: non richiede chiavi API.

| Chiave | Valore predefinito | Significato |
|---|---|---|
| `FornitoreAi:Tipo` | `claude-code` | `claude-code` oppure `api-anthropic` |
| `FornitoreAi:Modello` | `claude-sonnet-5-5` in `appsettings.json` | Modello usato dal fornitore |
| `FornitoreAi:ClaudeCode:Eseguibile` | `claude` | Nome o percorso dell'eseguibile di Claude Code |
| `FornitoreAi:Api:ChiaveApi` | non impostata | Richiesta solo con `api-anthropic` |

Per Claude Code, il controllo all'avvio verifica che l'eseguibile esista (nel PATH o al percorso indicato) e che Claude Code sia autenticato. L'autenticazione è valida se è impostata la variabile `CLAUDE_CODE_OAUTH_TOKEN`, oppure se hai già eseguito l'accesso con `claude` (le credenziali sono salvate nella cartella utente).

Per usare `api-anthropic` imposta, da variabile d'ambiente o user-secrets, `FornitoreAi:Api:ChiaveApi` e `FornitoreAi:Tipo=api-anthropic`. Questa modalità non è quella predefinita (vedi `docs/adr/004-fornitore-ai.md`).

Verifica la configurazione senza chiamate di rete:

```bash
dotnet run --project src/DBEnigmistica.Console -- check-ai
```

Con una configurazione valida il comando stampa il fornitore, il modello e `Configurazione valida.`. Con una configurazione incompleta termina con codice diverso da 0 e un messaggio che indica la chiave da impostare. La chiave API non viene mai stampata.

Nota: oggi nessun adattatore del Fornitore AI è implementato. L'estrazione e la creazione arrivano dalla fase 1, e fino ad allora rispondono con un errore esplicito.

## 7. Test

I test d'integrazione avviano un PostgreSQL 17 con Testcontainers. Docker deve essere in esecuzione.

```bash
dotnet restore DBEnigmistica.slnx
dotnet build DBEnigmistica.slnx --configuration Release --no-restore
dotnet test DBEnigmistica.slnx --configuration Release --no-build
```

Per l'elenco delle categorie di test e i test di architettura vedi `tests/DBEnigmistica.Tests`. Se un test d'integrazione fallisce subito con un errore del container, controlla che Docker Desktop sia avviato.

## 8. Hook gitleaks

Il progetto include un hook `pre-commit` in `.githooks/` che blocca il commit se gitleaks trova segreti nei file in staging. I valori trovati non vengono stampati.

1. Installa gitleaks:

   ```powershell
   winget install gitleaks.gitleaks
   ```

   Su macOS si usa `brew install gitleaks`.

2. Attiva l'hook nel repository locale:

   ```bash
   git config core.hooksPath .githooks
   ```

   Il comando va eseguito una volta per clone. Senza questa impostazione git non esegue l'hook.
3. Verifica: se gitleaks non è installato, il commit viene bloccato con un messaggio che lo dice.

La stessa scansione gira in GitHub Actions sull'intera storia git (lavoro "Controllo segreti" di `ci.yml`).

## 9. Risoluzione dei problemi

**"Stringa di connessione al database mancante"**
Imposta `ConnectionStrings__DefaultConnection` oppure, con `dotnet run`, la chiave `ConnectionStrings:DefaultConnection` nei user-secrets (sezione 4).

**Compose si ferma con "Imposta POSTGRES_... nel file .env"**
Manca una variabile in `.env`. Copia `.env.template` e completa tutti i valori (sezione 3).

**`postgres` non diventa `healthy`**
Controlla `docker compose logs postgres`. Se la porta 5432 è già usata da un altro PostgreSQL, fermalo o cambia la porta mappata in `docker-compose.yml`.

**"Claude Code non trovato"**
Installa Claude Code e verifica che `claude` sia nel PATH, oppure imposta `FornitoreAi:ClaudeCode:Eseguibile` con il percorso completo.

**"Claude Code non autenticato"**
Esegui l'accesso con `claude`, oppure imposta `CLAUDE_CODE_OAUTH_TOKEN`.

**"Chiave API mancante" (solo con `api-anthropic`)**
Imposta `FornitoreAi:Api:ChiaveApi` da variabile d'ambiente o user-secrets.

**"Comando disponibile dalla fase N" con codice 2**
Il comando non è ancora implementato. Lo stato per fase è in `ROADMAP.md`.

**I test d'integrazione non partono**
Docker Desktop deve essere in esecuzione. Testcontainers scarica `postgres:17-alpine` alla prima esecuzione: serve la connessione alla rete.

**Il commit è bloccato dall'hook**
Installa gitleaks (sezione 8). Se trova un segreto, rimuovilo dal file, ruota il segreto se era reale, e riprova. Non disattivare l'hook con `--no-verify`.
