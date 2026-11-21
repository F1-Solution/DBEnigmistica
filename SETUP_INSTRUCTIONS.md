# Preparazione dell'ambiente di sviluppo

Questa guida porta un PC Windows (o Linux) dallo stato di repository clonato a un database locale, una configurazione valida e i test verdi. Lo stato del progetto è in [ROADMAP.md](ROADMAP.md). Le decisioni di architettura sono in [docs/adr](docs/adr/README.md).

## Indice

1. Prerequisiti
2. Segreti e configurazione: dove stanno
3. Database locale con Docker Compose
4. Stringa di connessione
5. Migrazioni
6. Fornitore AI, login di Claude Code e `dbe check-ai`
7. Archivio e rendering delle pagine
8. Test
9. Hook gitleaks
10. Risoluzione dei problemi
11. Prototipo di revisione dei Giochi originali

## 1. Prerequisiti

- **.NET 10 SDK** (`dotnet --version` deve restituire `10.0.x`).
- **Docker Desktop**, in esecuzione. Serve per PostgreSQL in locale e per i test d'integrazione, che usano Testcontainers.
- **Git**.
- Per il Fornitore AI predefinito: **Claude Code** installato e autenticato con `/login` (vedi sezione 6). Non serve una chiave API.
- Per l'hook dei segreti: **gitleaks** (sezione 9).

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

Il file `docker-compose.yml` e i test d'integrazione usano PostgreSQL 18. Un volume creato con PostgreSQL 16 non si apre con la 18. Scegli una delle due opzioni.

**Opzione A, senza dati da conservare:** `docker compose down -v`, poi `docker compose up -d postgres`.

**Opzione B, con dati da conservare:**

1. Con il container 16 ancora attivo, PRIMA di aggiornare l'immagine: `docker compose exec postgres pg_dump -U <utente> -Fc <db> > backup.dump`.
2. `docker compose down -v`.
3. Aggiorna e avvia: `docker compose up -d postgres`.
4. Ripristina con `docker compose exec -T postgres pg_restore -U <utente> -d <db> < backup.dump`.

Dall'immagine 18 il volume dati si monta su `/var/lib/postgresql` (vedi `docs/fase-0-chiusura-decisioni.md`).

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

## 6. Fornitore AI, login di Claude Code e `dbe check-ai`

Il programma raggiunge l'AI tramite il **Fornitore AI**, configurato nella sezione `FornitoreAi`. In fase 1 l'unico fornitore disponibile è Claude Code non interattivo, con l'abbonamento. Non servono chiavi API.

### 6.1 Login di Claude Code su Windows

1. Installa Claude Code. Verifica che `claude` sia nel PATH, oppure indica in `FornitoreAi__ClaudeCode__Eseguibile` il percorso completo di `claude.exe`.
2. Apri un terminale ed esegui `claude`.
3. Dentro Claude Code digita `/login` e accedi con l'account dell'abbonamento (OAuth).
4. Verifica con `dbe check-ai` (sezione 6.3).

Il programma accetta il login se esiste il file `%USERPROFILE%\.claude\.credentials.json` oppure se è impostata la variabile `CLAUDE_CODE_OAUTH_TOKEN`. Se non c'è né l'uno né l'altra, `check-ai` segnala il login mancante.

**Non impostare `ANTHROPIC_API_KEY`.** Con una chiave nell'ambiente l'uso verrebbe addebitato a chiave API, non all'abbonamento. Il programma non passa la chiave a Claude Code, ma `check-ai` la segnala con un avviso. Rimuovila dalle variabili d'ambiente di Windows e riapri il terminale.

In fase 1 non si usa `CLAUDE_CODE_OAUTH_TOKEN`: il login è quello del punto 3. Il programma la accetta come alternativa al login, ma la decisione del proprietario su questo punto è ancora aperta.

### 6.2 Chiavi del Fornitore AI

| Chiave | Variabile d'ambiente | Valore predefinito | Significato |
|---|---|---|---|
| `FornitoreAi:Tipo` | `FornitoreAi__Tipo` | `claude-code` | In fase 1 va lasciato `claude-code`. Con `api-anthropic` la configurazione è rifiutata (codice 3). |
| `FornitoreAi:Modello` | `FornitoreAi__Modello` | `claude-sonnet-5-5` in `appsettings.json` | Modello usato dal fornitore. Obbligatorio. |
| `FornitoreAi:ClaudeCode:Eseguibile` | `FornitoreAi__ClaudeCode__Eseguibile` | `claude` | Nome o percorso di Claude Code. Su Windows deve essere `claude.exe` (o un nome senza estensione che si risolve in `.exe`): i file `.cmd` e `.bat` sono rifiutati. Se è un percorso, il file deve esistere. |
| `FornitoreAi:ClaudeCode:TimeoutSecondi` | `FornitoreAi__ClaudeCode__TimeoutSecondi` | `300` | Tempo massimo di una chiamata a Claude Code, in secondi. Valori da 1 a 3600. |
| `FornitoreAi:LimiteChiamate` | `FornitoreAi__LimiteChiamate` | nessun limite | Se presente, almeno 1. Conta le chiamate reali a Claude Code, comprese le ripetizioni e le richieste di correzione; le risposte riprodotte non contano. Quando il limite è raggiunto, la pagina successiva viene rifiutata con «Limite dell'esecuzione raggiunto» e codice 4. Una pagina già iniziata può superare il limite con le sue ripetizioni. |
| `FornitoreAi:Registrazioni:Modalita` | `FornitoreAi__Registrazioni__Modalita` | vuota (spenta) | `registra` salva la risposta di ogni pagina. `riproduci` usa solo le risposte salvate: non serve Claude Code né il login e non si fa nessuna chiamata. Un altro valore è un errore di configurazione (codice 3). |
| `FornitoreAi:Registrazioni:Cartella` | `FornitoreAi__Registrazioni__Cartella` | nessuna | Cartella delle Registrazioni. Obbligatoria se `Modalita` è impostata: senza, il comando si ferma con codice 3. |

Nessun comando pubblico chiama ancora Claude Code in questa versione. Le chiavi di chiamata (`LimiteChiamate`, `TimeoutSecondi`, `Registrazioni`) avranno effetto con l'estrazione. Il controllo della configurazione, invece, vale già da ora.

**Registrazioni.** Con `Modalita` impostata, la risposta di ogni pagina si salva in `<Cartella>/<fornitore>/<modello>/<versione_prompt>/<impronta>.json`. In `riproduci` il programma rifiuta con un errore:

- una pagina senza Registrazione («Registrazione mancante»);
- una Registrazione fatta a una risoluzione diversa da quella richiesta;
- un file illeggibile, o la cui risposta non corrisponde al SHA-256 salvato (file modificato o danneggiato) («Registrazione non valida»).

Le Registrazioni derivano dalle Riviste e restano solo sul PC del proprietario, mai su GitHub (ADR 008).

### 6.3 Verifica con `dbe check-ai`

```bash
dotnet run --project src/DBEnigmistica.Console -- check-ai
```

Il comando non fa chiamate di rete. Con una configurazione valida stampa il fornitore, il modello e `Configurazione valida.`, ed esce con codice 0.

| Situazione | Esito |
|---|---|
| Login mancante | Codice 3. Su stderr: `Claude Code non autenticato: esegui claude e poi /login` |
| Eseguibile non trovato | Codice 3. Su stderr: `Claude Code non trovato: ...`, con la chiave da impostare |
| Eseguibile `.cmd` o `.bat` su Windows | Codice 3. Su stderr: `Serve l'eseguibile nativo di Claude Code (.exe), non '...'` |
| `FornitoreAi__Tipo=api-anthropic` | Codice 3. Su stderr: `Il Fornitore AI api-anthropic non è disponibile in fase 1: imposta FornitoreAi__Tipo=claude-code.` |
| Chiave `ANTHROPIC_API_KEY` presente | Avviso su stdout: la chiave non viene passata a Claude Code. Il comando prosegue |
| Modello mancante, valori fuori intervallo, Registrazioni incomplete | Codice 3, un messaggio per ogni errore su stderr |

`check-ai` controlla che esista il file di login oppure che `CLAUDE_CODE_OAUTH_TOKEN` sia impostata. Non prova che il login funzioni: la prova vera è la prima chiamata reale, che arriverà con l'estrazione. La chiave API non viene mai stampata.

## 7. Archivio e rendering delle pagine

### 7.1 Cartella dell'Archivio

- Chiave `Archivio:Cartella`, oppure variabile `Archivio__Cartella`. Deve essere un percorso assoluto.
- Nell'Archivio il programma conserva la copia di ogni Rivista e le immagini delle pagine. Il nome dei file deriva dall'impronta SHA-256.
- Se manca, i comandi che usano l'Archivio si fermano con un messaggio che nomina la chiave. Nessun comando pubblico la usa ancora in questa versione.
- **Backup:** la cartella va inclusa nel backup che fai tu (decisione D4 in `docs/fase-1-decisioni.md`). Il programma non fa backup in fase 1. Le Riviste occupano spazio due volte: nella cartella e nel database, come testo.

### 7.2 Risoluzione del rendering

- Chiave `Estrazione:Dpi`, oppure variabile `Estrazione__Dpi`. Valori da 150 a 300. Predefinito: 200. Un valore fuori da questo intervallo, o non numerico, è un errore di configurazione che nomina la chiave: il programma non usa il predefinito al suo posto.

### 7.3 Verifica di PDFium con `diagnostica-render`

```bash
dotnet run --project src/DBEnigmistica.Console -- diagnostica-render
```

Il comando è nascosto: non compare in `dbe --help`. Rende un PDF sintetico incluso nel programma, senza database e senza Claude Code.

- `--salva-png <cartella>`: salva i PNG delle pagine nella cartella indicata.
- `--dpi <valore>`: usa una risoluzione diversa da `Estrazione:Dpi`.

Per ogni pagina stampa dimensioni, DPI e numero di pixel con contenuto. Una pagina con meno di 2.000 pixel con contenuto è segnalata come `VUOTA`. Se una pagina è vuota il comando esce con codice 1 e scrive «Rendering non riuscito». Altrimenti scrive «Rendering verificato.» ed esce con codice 0.

Non serve installare PDFium: la libreria nativa è inclusa nella pubblicazione.

## 8. Test

I test d'integrazione avviano un PostgreSQL 18 con Testcontainers. Docker deve essere in esecuzione.

```bash
dotnet restore DBEnigmistica.slnx
dotnet build DBEnigmistica.slnx --configuration Release --no-restore
dotnet test DBEnigmistica.slnx --configuration Release --no-build
```

Per l'elenco delle categorie di test e i test di architettura vedi `tests/DBEnigmistica.Tests`. Se un test d'integrazione fallisce subito con un errore del container, controlla che Docker Desktop sia avviato.

## 9. Hook gitleaks

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

## 10. Risoluzione dei problemi

**"Stringa di connessione al database mancante"**
Imposta `ConnectionStrings__DefaultConnection` oppure, con `dotnet run`, la chiave `ConnectionStrings:DefaultConnection` nei user-secrets (sezione 4).

**Compose si ferma con "Imposta POSTGRES_... nel file .env"**
Manca una variabile in `.env`. Copia `.env.template` e completa tutti i valori (sezione 3).

**`postgres` non diventa `healthy`**
Controlla `docker compose logs postgres`. Se la porta 5432 è già usata da un altro PostgreSQL, fermalo o cambia la porta mappata in `docker-compose.yml`.

**"Claude Code non trovato"**
Installa Claude Code e verifica che `claude` sia nel PATH, oppure imposta `FornitoreAi__ClaudeCode__Eseguibile` con il percorso completo di `claude.exe` (sezione 6.1).

**"Serve l'eseguibile nativo di Claude Code (.exe)"**
Su Windows l'eseguibile non può essere un file `.cmd` o `.bat`. Indica il percorso di `claude.exe` (sezione 6.2).

**"Il Fornitore AI api-anthropic non è disponibile in fase 1"**
Imposta `FornitoreAi__Tipo=claude-code` (sezione 6.2).

**"Claude Code non autenticato: esegui claude e poi /login"**
Esegui `claude`, poi `/login` (sezione 6.1). Poi rilancia `check-ai`.

**Avviso "ANTHROPIC_API_KEY è presente nell'ambiente"**
Rimuovi la variabile da Windows e riapri il terminale (sezione 6.1).

**"Cartella dell'Archivio mancante"**
Imposta `Archivio__Cartella` con un percorso assoluto (sezione 7.1).

**"Risoluzione non valida: Estrazione:Dpi"**
Imposta un numero intero da 150 a 300 (sezione 7.2).

**"Modalità delle Registrazioni non valida" o "Cartella delle Registrazioni mancante"**
Imposta `registra` o `riproduci` in `FornitoreAi__Registrazioni__Modalita`, e la cartella in `FornitoreAi__Registrazioni__Cartella` (sezione 6.2).

**"Registrazione mancante"**
In modalità `riproduci` manca la risposta salvata di una pagina. Esegui prima in modalità `registra`, con lo stesso modello e la stessa versione del prompt.

**"Registrazione non valida"**
Il file della Registrazione non si legge, non corrisponde al suo SHA-256 (è stato modificato) oppure è a un'altra risoluzione. Riregistra la pagina in modalità `registra` (sezione 6.2).

**"Limite dell'esecuzione raggiunto" (codice 4)**
È stato raggiunto `FornitoreAi__LimiteChiamate`. Alza il limite o rimuovilo.

**`diagnostica-render` segnala una pagina `VUOTA`**
Rilancia con `--salva-png <cartella>` e guarda i PNG. Se la pagina è bianca, controlla il file del rendering e la risoluzione (`--dpi`).

**"Comando disponibile dalla fase N" con codice 2**
Il comando non è ancora implementato. Lo stato per fase è in `ROADMAP.md`.

**I test d'integrazione non partono**
Docker Desktop deve essere in esecuzione. Testcontainers scarica `postgres:18-alpine` alla prima esecuzione: serve la connessione alla rete.

**Il commit è bloccato dall'hook**
Installa gitleaks (sezione 9). Se trova un segreto, rimuovilo dal file, ruota il segreto se era reale, e riprova. Non disattivare l'hook con `--no-verify`.

## 11. Prototipo di revisione dei Giochi originali

Il prototipo è un host web locale per rivedere i Giochi originali. Gira solo sul PC Windows, dall'archivio `win-x64` pubblicato dalla CI. Non è nell'immagine Docker: lì `dbe prototipo` risponde con codice 2.

### 11.1 Prerequisiti

1. PostgreSQL in esecuzione (sezione 3) e stringa di connessione impostata (sezione 4).
2. Migrazioni applicate: `dbe migrate` (sezione 5).
3. Cartella dei descrittori delle Tipologie. Il predefinito è la cartella `tipologie` accanto all'eseguibile, già presente nell'archivio. Per usarne un'altra, imposta `Prototipo:CartellaTipologie` (variabile `Prototipo__CartellaTipologie`). Un valore relativo si risolve dalla cartella corrente.
4. Modello di creazione: `FornitoreAi:Creazione:Modello` (variabile `FornitoreAi__Creazione__Modello`), predefinito `claude-opus-5-5` in `appsettings.json`. Il modello dell'estrazione non cambia.
5. Registrazioni di creazione: `FornitoreAi:Creazione:Registrazioni`, con `Modalita` e `Cartella` (variabili `FornitoreAi__Creazione__Registrazioni__Modalita` e `FornitoreAi__Creazione__Registrazioni__Cartella`).

### 11.2 Avvio

Estrai l'archivio `dbenigmistica-console-X.Y.Z-win-x64.zip` e, in un terminale nella cartella estratta, esegui:

```powershell
.\dbe.exe prototipo
```

Il programma applica la configurazione, porta a errore le generazioni rimaste interrotte e stampa l'indirizzo di ascolto, per esempio `http://127.0.0.1:5180`. Ctrl+C lo termina con codice 0.

Se manca una chiave richiesta o un valore non è valido, il programma esce con codice 3.

**Sviluppo dai sorgenti.** Con la configurazione impostata, lo stesso comando funziona da sorgente, dalla radice del repository:

```powershell
dotnet run --project src\DBEnigmistica.Console -- prototipo
```

L'archivio `win-x64` resta la via per il proprietario.

### 11.3 Indirizzo e porta

- Il prototipo ascolta solo su `127.0.0.1`. L'indirizzo è fisso e non si cambia da configurazione, URL o variabili d'ambiente.
- La porta predefinita è `5180`. Si cambia con `Prototipo:Porta` (variabile `Prototipo__Porta`).
- Le richieste con un nome host diverso da `127.0.0.1` o `localhost` ricevono 421.
- Non c'è una pagina alla radice. Le API stanno sotto `/api/prototipo`. Per verificare che l'host risponda, apri `http://127.0.0.1:5180/api/prototipo/salute`: deve comparire `{"stato":"ok"}`.

### 11.4 Creazione dal vivo

Finché la prova reale di Claude Code non è registrata (riga `PHASE:1 WAVE:1.1-prova STATUS:complete` in `PROGRESS.md`), la creazione usa solo le Registrazioni in riproduzione. Nessuna chiamata a Claude Code parte dal prototipo.

### 11.5 Risoluzione dei problemi

**Codice 2 con "Il prototipo non è incluso in questa pubblicazione"**
Stai usando l'immagine Docker. Usa l'archivio `win-x64` (sezione 11.2).

**Codice 3 all'avvio**
Controlla le chiavi elencate in 11.1, in particolare la stringa di connessione e la cartella dei descrittori. Se il messaggio dice che il login del database non può usare `dbe_creazione`, il login deve poter eseguire `SET ROLE dbe_creazione` (superutente o membro del ruolo). Cambia utente in `ConnectionStrings__DefaultConnection`.

**Risposta 421**
La richiesta usa un nome host diverso. Usa `http://127.0.0.1:<porta>`.

**Risposta 403**
La richiesta arriva da un'altra origine o dichiara un contesto cross-site. Chiama l'API direttamente da `127.0.0.1` o `localhost`.

**Risposta 415**
Una POST o una PUT non dichiara `Content-Type: application/json`. Aggiungi l'intestazione.
