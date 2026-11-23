# DBEnigmistica

DBEnigmistica è un prodotto per l'enigmistica digitale in italiano, costruito attorno a Riviste e Giochi. Tre superfici: lo **Studio** (privato, sui dispositivi del proprietario), il **Sito pubblico** (per i giocatori) e l'**Admin** (gestione del Sito). Tre componenti: il **Motore di estrazione** (legge le Riviste in PDF), il **Motore di creazione** (genera Giochi originali) e il **Lettore** (fa giocare a schermo).

Lo stato reale del progetto è in [ROADMAP.md](ROADMAP.md). Questo file descrive il codice com'è oggi.

## Stato

**Fase 0 (Fondamenta) conclusa il 19/11/2026. Fase 1 (Motore di estrazione) in corso: ondata 1.1 (fondamenta).**

Oggi la riga di comando funziona per la gestione del database, delle soluzioni e dell'esportazione, per la verifica della configurazione dell'AI (login di Claude Code compreso) e per la verifica dell'integrità del registro. La scansione delle Riviste e la creazione dei Giochi non sono ancora disponibili: arrivano dalla fase 1.

## Requisiti

- .NET 10 SDK
- Docker Desktop (per PostgreSQL in locale e per i test d'integrazione)
- Claude Code installato e autenticato con `claude` e `/login`, per il Fornitore AI predefinito (vedi [SETUP_INSTRUCTIONS.md](SETUP_INSTRUCTIONS.md), sezione 6). Su Windows l'eseguibile deve essere `claude.exe`. Non serve una chiave API; `ANTHROPIC_API_KEY` non deve essere nell'ambiente.

## Avvio rapido

```bash
# 1. Configurazione del database: copia il modello e imposta i valori
cp .env.template .env

# 2. Avvio di PostgreSQL
docker compose up -d postgres

# 3. Stringa di connessione (user-secrets, solo per 'dotnet run')
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=<database>;Username=<utente>;Password=<password>" --project src/DBEnigmistica.Console

# 4. Aiuto e comandi
dotnet run --project src/DBEnigmistica.Console -- --help
dotnet run --project src/DBEnigmistica.Console -- migrate
dotnet run --project src/DBEnigmistica.Console -- list-games
dotnet run --project src/DBEnigmistica.Console -- check-ai
```

Il dettaglio di ogni passo (variabili d'ambiente, user-secrets, configurazione del Fornitore AI, risoluzione dei problemi) è in [SETUP_INSTRUCTIONS.md](SETUP_INSTRUCTIONS.md).

## Comandi

Il programma si chiama `dbe`. `dbe --help` elenca i comandi. Il flag `--verbose` mostra i dettagli completi degli errori.

### Funzionanti

| Comando | Cosa fa | Richiede |
|---|---|---|
| `migrate` | Applica le migrazioni EF pending, poi gli SQL delle Tipologie; chiede conferma salvo `--yes` | database |
| `list-games` | Elenca le Tipologie presenti nel database | database |
| `export-schema` | Esporta gli schemi di gioco in JSON | database |
| `export-games` | Esporta i Giochi estratti in JSON o CSV. Il CSV ha le colonne `Tipologia`, `X`, `Y`, `Larghezza` e `Altezza`; il riquadro è normalizzato tra 0 e 1 | database |
| `export-stats` | Esporta le statistiche delle Riviste in JSON | database |
| `link-solution` | Associa una soluzione a un Gioco | database |
| `verify-solution` | Verifica o rifiuta una soluzione | database |
| `list-unsolved` | Elenca i Giochi senza soluzione; filtri `--tipologia` (per esempio `cruciverba-classico`) e `--magazine-id` | database |
| `export-solutions` | Esporta le soluzioni in JSON o CSV | database |
| `check-ai` | Verifica la configurazione del Fornitore AI e il login di Claude Code, senza chiamate di rete: codice 0 se valida, codice 3 con il messaggio per ogni errore (login mancante, `api-anthropic` non disponibile in fase 1, eseguibile `.cmd` o `.bat`) | configurazione AI |
| `verify-integrity` | Verifica la catena delle impronte del registro in sola aggiunta: codice 0 e "Registro integro: N eventi verificati.", oppure codice 5 con l'id del primo evento non valido su stderr; codice 1 per gli errori generici, per esempio database non raggiungibile | database |
| `types generate` | Genera l'SQL delle Tipologie dai descrittori in `tipologie/`; con `--check` non scrive ed esce con 1 se un file SQL è diverso o manca | nessuno |
| `eval extraction` | Misura l'estrazione su un Set e la confronta con la Linea di base. `--replay` legge le Registrazioni, senza chiamate AI; esce con 1 se una metrica scende | nessuno (con `--live`: configurazione AI) |
| `eval draft` | Scrive le bozze dei risultati attesi da una Registrazione; le pagine con un risultato già presente restano intatte | nessuno |
| `eval review-time` | Rapporto dei tempi di revisione dei candidati del prototipo, a confronto con le soglie G1. `--output <cartella>` scrive `review-time.json` e `review-time.md`; `--candidati <file>` legge i candidati da un file JSON invece che dal database. Esce con 0 anche se una soglia non è rispettata | database (senza `--candidati`) |

### Tipologie e valutazione dell'estrazione

Gli SQL delle Tipologie si generano dai descrittori, mai a mano:

```bash
dotnet run --project src/DBEnigmistica.Console -- types generate
dotnet run --project src/DBEnigmistica.Console -- types generate --check
```

`migrate` applica prima le migrazioni EF e poi gli SQL delle Tipologie. Un file già applicato con un checksum diverso dà errore con codice 1. Le versioni `v0` non hanno SQL e non si applicano mai.

Misura sul Set sintetico (`tests/DBEnigmistica.Tests/Fixtures/SetSintetico`), senza chiamate AI. `--output` scrive `rapporto.json` e `rapporto.md`:

```bash
dotnet run --project src/DBEnigmistica.Console -- eval extraction --replay --set tests/DBEnigmistica.Tests/Fixtures/SetSintetico --output <cartella>
```

Il confronto usa `linea-di-base.json` del set. `--live` chiama il Fornitore AI e richiede `--limite <n>`: non si usa finché `PROGRESS.md` non contiene `PHASE:1 WAVE:1.1-prova STATUS:complete`. Il comando rifiuta di partire finché `FornitoreAi:DalVivoAbilitato` vale `false`, che è il predefinito. Impostala a `true` solo dopo il via della prova `1.1-prova`.

Dopo ogni modifica a descrittori o regole delle Tipologie, prima di misurare, si ricalcolano Registrazioni e Linea di base del Set sintetico. Il comando è nascosto nell'aiuto:

```bash
dotnet run --project src/DBEnigmistica.Console -- eval sintetico aggiorna-chiavi --set tests/DBEnigmistica.Tests/Fixtures/SetSintetico
```

### Tempi di revisione

Il rapporto confronta i tempi di revisione dei candidati con le soglie G1. Sul Set di candidati sintetici, senza database e senza chiamate AI:

```bash
dotnet run --project src/DBEnigmistica.Console -- eval review-time --candidati tests/DBEnigmistica.Tests/Fixtures/CandidatiSintetici/candidati.json --output <cartella>
```

Il rapporto ha sei gruppi: crittografia breve, anagramma, cruciverba, rebus, sudoku e giochi logici (futoshiki e nonogramma insieme). Ogni gruppo riporta conteggi, quota accettata, mediana e quartili del tempo attivo. Le soglie G1 valgono per crittografia breve, anagramma e cruciverba; per gli altri gruppi il rapporto riporta solo i tempi. Un gruppo senza candidati rivisti non riporta valori di tempo.

### Codici d'uscita

| Codice | Significato |
|---|---|
| 0 | Riuscito |
| 1 | Errore generico, per esempio database non raggiungibile |
| 2 | Comando non ancora disponibile in questa fase |
| 3 | Configurazione non valida |
| 4 | Limite dell'esecuzione raggiunto (`FornitoreAi__LimiteChiamate`) |
| 5 | Registro alterato (`verify-integrity`) |
| 130 | Operazione annullata |

### In arrivo

Questi comandi esistono nella riga di comando ma rispondono `Comando disponibile dalla fase N` con codice d'uscita 2:

| Comando | Fase |
|---|---|
| `scan-file`, `scan-folder` | 1 |
| `analyze-game`, `analyze-game-text`, `scan-solutions` | 4 |

## Test

```bash
dotnet restore DBEnigmistica.slnx
dotnet build DBEnigmistica.slnx --configuration Release --no-restore
dotnet test DBEnigmistica.slnx --configuration Release --no-build
```

I test d'integrazione usano Testcontainers con PostgreSQL 18: serve Docker in esecuzione. Non esistono test su database in memoria.

In GitHub Actions la sequenza aggiunge il controllo del formato (`dotnet format --verify-no-changes`), `-warnaserror` e la soglia di copertura (vedi [docs/CI.md](docs/CI.md)).

## Struttura

```
src/
  DBEnigmistica.Domain/          Entità, valori, policy di pubblicazione, registro (nessuna dipendenza)
  DBEnigmistica.Application/     Casi d'uso e servizi (dipende dal Domain)
  DBEnigmistica.Infrastructure/  EF Core, Npgsql, migrazioni, repository, PDF
  DBEnigmistica.AI/              Fornitore AI (adattatore Claude Code, fase 1)
  DBEnigmistica.Console/         Riga di comando dbe, configurazione, presentazione
  DBEnigmistica.Tipologie/       Descrittori e validatori delle Tipologie, SQL generato (nessun riferimento ad altri progetti)
tipologie/                       Descrittori v*.json e SQL generato per Tipologia
tests/
  DBEnigmistica.Tests/           Unit, Integration (PostgreSQL vero), Architecture, EndToEnd (CLI in processo)
docs/
  adr/                           Decisioni di architettura (ADR 001-006)
  CI.md                          Flussi GitHub Actions e impostazioni del repository
```

Le direzioni delle dipendenze sono verificate dai test di architettura.

## Documenti

- [ROADMAP.md](ROADMAP.md): fasi, stato, decisioni e domande aperte. È la fonte di verità.
- [SETUP_INSTRUCTIONS.md](SETUP_INSTRUCTIONS.md): preparazione dell'ambiente di sviluppo, login di Claude Code, Archivio e verifica del rendering.
- [docs/CI.md](docs/CI.md): flussi GitHub Actions, pubblicazione e creazione dei tag.
- [docs/adr/README.md](docs/adr/README.md): indice delle decisioni di architettura.
