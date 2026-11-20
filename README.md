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
| `migrate` | Applica le migrazioni pending al database | database |
| `list-games` | Elenca le Tipologie presenti nel database | database |
| `export-schema` | Esporta gli schemi di gioco in JSON | database |
| `export-games` | Esporta i Giochi estratti in JSON o CSV | database |
| `export-stats` | Esporta le statistiche delle Riviste in JSON | database |
| `link-solution` | Associa una soluzione a un Gioco | database |
| `verify-solution` | Verifica o rifiuta una soluzione | database |
| `list-unsolved` | Elenca i Giochi senza soluzione | database |
| `export-solutions` | Esporta le soluzioni in JSON o CSV | database |
| `check-ai` | Verifica la configurazione del Fornitore AI e il login di Claude Code, senza chiamate di rete: codice 0 se valida, codice 3 con il messaggio per ogni errore (login mancante, `api-anthropic` non disponibile in fase 1, eseguibile `.cmd` o `.bat`) | configurazione AI |
| `verify-integrity` | Verifica la catena delle impronte del registro in sola aggiunta: codice 0 e "Registro integro: N eventi verificati.", oppure codice 5 con l'id del primo evento non valido su stderr; codice 1 per gli errori generici, per esempio database non raggiungibile | database |

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
