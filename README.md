# DBEnigmistica

Sistema intelligente di estrazione automatica di giochi enigmistici da riviste PDF, con database PostgreSQL a schema dinamico e analisi AI.

## 🎯 Caratteristiche

- 🔍 **Estrazione automatica** da file PDF con OCR (Tesseract)
- 🤖 **Classificazione AI** tramite Anthropic Claude per identificare tipi di gioco
- 📊 **Schema dinamico** - Crea automaticamente nuove tabelle per nuovi tipi di gioco
- 💾 **Database PostgreSQL** con Entity Framework Core
- 🧩 **Gestione Soluzioni** - Tracciamento e associazione automatica delle soluzioni
- 💾 **Caching OCR** - Cache intelligente per evitare riprocessamento
- 📤 **Export Multi-formato** - JSON, CSV per giochi e statistiche
- 🖥️ **CLI moderna** con System.CommandLine (14 comandi)
- 📈 **Progress reporting** in tempo reale
- 🎨 **Output formattato** con colori e box drawing characters

## 🏗️ Architettura

Architettura a 5 layer con Clean Architecture:

```
┌─────────────────────────────────────┐
│   Console UI Layer                  │
│   (CLI Commands, Program.cs)        │
└─────────────────────────────────────┘
          ↓
┌─────────────────────────────────────┐
│   Application Layer                 │
│   (Orchestration, Workflows)        │
└─────────────────────────────────────┘
          ↓
┌──────────────┬───────────────┬──────┐
│ Infrastructure│ AI Services  │Domain│
│   (Data/OCR) │   (Claude)   │      │
└──────────────┴───────────────┴──────┘
          ↓
┌─────────────────────────────────────┐
│   PostgreSQL Database               │
└─────────────────────────────────────┘
```

### Progetti

- **DBEnigmistica.Console**: Applicazione console con comandi CLI
- **DBEnigmistica.Domain**: Entità, interfacce, enumerazioni
- **DBEnigmistica.Application**: Servizi di orchestrazione e business logic
- **DBEnigmistica.Infrastructure**: Repository, servizi PDF/OCR, database
- **DBEnigmistica.AI**: Integrazione con Claude per classificazione e analisi

## 🛠️ Stack Tecnologico

- **.NET 9.0** - Framework applicativo
- **PostgreSQL 16** - Database relazionale
- **Entity Framework Core 9** - ORM
- **Tesseract.NET 5.2** - OCR open-source
- **PdfPig 0.1.13** - Elaborazione PDF
- **Anthropic Claude API** - Intelligenza artificiale
- **System.CommandLine** - Parsing CLI moderno
- **Serilog** - Logging strutturato
- **xUnit + Moq + FluentAssertions** - Testing

## 🚀 Setup Sviluppo

### Prerequisiti

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker & Docker Compose](https://www.docker.com/products/docker-desktop)
- [Anthropic API Key](https://console.anthropic.com/) (per classificazione AI)
- Tessdata files per Tesseract (italiano: `ita.traineddata`)

### 1. Clone e Dipendenze

```bash
git clone <repository-url>
cd DBEnigmistica
dotnet restore
```

### 2. Avvio Database

```bash
docker-compose up -d
```

Servizi disponibili:
- **PostgreSQL**: `localhost:5432` (utente e password definiti in `.env`)
- **PgAdmin**: `http://localhost:5050` (email e password definite in `.env`)

### 3. Configurazione

Crea `src/DBEnigmistica.Console/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=enigmistica_db;Username=enigmistica;Password=YOUR_PASSWORD"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-api03-YOUR_API_KEY_HERE"
  },
  "Ocr": {
    "TessdataPath": "C:/Program Files/Tesseract-OCR/tessdata",
    "Language": "ita"
  }
}
```

### 4. Migrazioni Database

```bash
cd src/DBEnigmistica.Console
dotnet ef database update
```

Oppure usa il comando CLI:

```bash
dotnet run -- migrate
```

### 5. Build & Run

```bash
dotnet build
dotnet run --project src/DBEnigmistica.Console -- --help
```

## 📖 Comandi CLI

### Scansione PDF

**Scansiona una cartella di PDF:**
```bash
dotnet run -- scan-folder ./riviste --title "La Settimana Enigmistica"
```

**Scansiona un singolo file:**
```bash
dotnet run -- scan-file rivista.pdf --title "Enigmistica Italiana" --issue 4500 --date 2024-01-15
```

Opzioni:
- `--title, -t` - Titolo della rivista
- `--issue, -i` - Numero della rivista
- `--date, -d` - Data pubblicazione (yyyy-MM-dd)
- `--periodicity, -r` - Periodicità (Weekly/Monthly/Biweekly)

### Analisi Nuovi Tipi di Gioco

**Da file Markdown:**
```bash
dotnet run -- analyze-game nuovo_gioco.md
```

**Da descrizione testuale:**
```bash
dotnet run -- analyze-game-text "Un gioco dove bisogna trovare parole nascoste in una griglia 10x10" --name word_search
```

### Gestione Database

**Lista tipologie di giochi:**
```bash
dotnet run -- list-games --details
```

**Esporta schemi in JSON:**
```bash
dotnet run -- export-schema --output schemas.json --pretty
```

**Applica migrazioni:**
```bash
dotnet run -- migrate
```

### Gestione Soluzioni

**Associa manualmente una soluzione a un gioco:**
```bash
dotnet run -- link-solution --game-id 123 --solution "PAROLA" --type InSameIssue
```

**Lista giochi senza soluzione:**
```bash
dotnet run -- list-unsolved --magazine-id 5 --export unsolved.json
```

**Scansiona una pagina per trovare soluzioni:**
```bash
dotnet run -- scan-solutions --page-id 89
```

**Verifica una soluzione proposta:**
```bash
dotnet run -- verify-solution --solution-id 45 --verified true --notes "Verificata manualmente"
```

**Esporta giochi con soluzioni:**
```bash
# Tutte le soluzioni in JSON
dotnet run -- export-solutions --output solutions.json

# Solo soluzioni verificate in CSV
dotnet run -- export-solutions --output solutions.csv --format csv --verified-only

# Filtra per rivista
dotnet run -- export-solutions --output magazine_5_solutions.json --magazine-id 5
```

### Export Dati

**Esporta giochi in formati diversi:**
```bash
# JSON
dotnet run -- export-games --output games.json --format json

# CSV
dotnet run -- export-games --output games.csv --format csv --game-type crossword
```

**Esporta statistiche riviste:**
```bash
dotnet run -- export-stats --output stats.json
```

## 🧪 Testing

### Esegui tutti i test

```bash
dotnet test
```

### Test con coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage
```

### Test specifici

```bash
# Unit test
dotnet test --filter "FullyQualifiedName~Unit"

# Integration test
dotnet test --filter "FullyQualifiedName~Integration"
```

## 🎮 Workflow Completo

### Workflow Estrazione Giochi

1. **Caricamento PDF** → Estrazione pagine come immagini
2. **OCR** → Riconoscimento testo con Tesseract
3. **Layout Analysis** → Identificazione regioni potenziali giochi
4. **Classificazione AI** → Claude determina il tipo di gioco
5. **Schema Check** → Verifica se esiste tabella per quel tipo
   - Se NO → Proposta schema AI → Validazione utente → Creazione tabella
   - Se SI → Continua
6. **Estrazione Dati** → Claude estrae campi specifici
7. **Persistenza** → Salvataggio nel database

### Workflow Gestione Soluzioni

1. **Scansione Soluzione** → Riconoscimento soluzioni da pagine dedicate
2. **Pattern Matching** → Identificazione riferimenti a giochi (es: "Pag. 12 - Cruciverba: SOLUZIONE")
3. **AI Matching** → Claude associa soluzione al gioco corretto
4. **Link Automatico** → Creazione associazione con confidence score
5. **Verifica Manuale** → Utente conferma o corregge l'associazione
6. **Persistenza** → Salvataggio in `game_solutions` + aggiornamento `games.solution`

#### Tipi di Soluzione

- **InSamePage** - Soluzione nella stessa pagina del gioco
- **InSameIssue** - Soluzione in un'altra pagina dello stesso numero
- **InNextIssue** - Soluzione nel numero successivo (flag `SolutionsInNextIssue` su Magazine)
- **External** - Soluzione in fonte esterna

## 📊 Database Schema

### Tabelle Statiche (EF Core)

- `magazines` - Riviste elaborate (+ campo `solutions_in_next_issue`)
- `pages` - Pagine delle riviste
- `games` - Giochi estratti (base + campi soluzione)
- `game_solutions` - Soluzioni complesse associate ai giochi
- `game_type_schemas` - Metadata schemi dinamici

### Tabelle Dinamiche (Runtime SQL)

Create automaticamente per ogni nuovo tipo di gioco scoperto:
- `crossword_games`
- `rebus_games`
- `logic_games`
- `anagram_games`
- `[custom_game_type]_games`

Ogni tabella dinamica include:
- `id` (SERIAL PRIMARY KEY)
- `game_id` (FK → games)
- Colonne custom definite dall'AI
- Indici automatici su game_id

## 🤝 Contribuire

1. Fork del repository
2. Crea un feature branch (`git checkout -b feature/amazing-feature`)
3. Commit dei cambiamenti (`git commit -m 'Add amazing feature'`)
4. Push al branch (`git push origin feature/amazing-feature`)
5. Apri una Pull Request

## 📝 Note Tecniche

### Schema Dinamico

Il sistema può creare nuove tabelle a runtime quando scopre un tipo di gioco non mappato:
1. AI genera proposta schema JSON
2. Validazione automatica (nomi, tipi, duplicati)
3. Conferma interattiva utente
4. Esecuzione SQL CREATE TABLE
5. Salvataggio metadata in `game_type_schemas`

### OCR Configuration

Tesseract richiede i file `tessdata` installati. Download da:
https://github.com/tesseract-ocr/tessdata

File necessari: `ita.traineddata` (italiano)

### Performance

- Batch processing per cartelle di PDF
- Progress reporting non bloccante
- Connection pooling PostgreSQL
- Async/await pervasivo

## 🐛 Troubleshooting

**Errore: "Tesseract not found"**
- Installa Tesseract OCR: https://github.com/tesseract-ocr/tesseract
- Configura `TessdataPath` in appsettings

**Errore: "Connection refused 5432"**
- Verifica Docker: `docker-compose ps`
- Check logs: `docker-compose logs postgres`

**Errore: "Claude API key invalid"**
- Verifica API key in appsettings.Development.json
- Check account Anthropic: https://console.anthropic.com/

**Test falliti**
- Rebuild: `dotnet clean && dotnet build`
- Check migration: `dotnet ef database update`

## 📄 Licenza

Questo progetto è rilasciato sotto licenza MIT. Vedi file `LICENSE` per dettagli.

## 👥 Autori

- Progetto sviluppato come case study per estrazione intelligente di contenuti strutturati da PDF

## 🔮 Roadmap Future

- [ ] Caching risultati OCR
- [ ] Export dati in formati vari (JSON, CSV, Excel)
- [ ] Web API REST per accesso dati
- [ ] Dashboard web di monitoraggio
- [ ] Support per più lingue (multilingua)
- [ ] Miglioramento accuracy con fine-tuning AI
