# 🚀 DBEnigmistica - Istruzioni di Setup Complete

Questa guida ti accompagnerà passo-passo nella configurazione del progetto **DBEnigmistica**, dalla configurazione dei secret fino all'esecuzione della prima scansione.

## 📋 Indice

1. [Prerequisiti](#-prerequisiti)
2. [Gestione Secret e Sicurezza](#-gestione-secret-e-sicurezza)
3. [Setup Database](#-setup-database)
4. [Setup Tesseract OCR](#-setup-tesseract-ocr)
5. [Setup Anthropic Claude API](#-setup-anthropic-claude-api)
6. [Configurazione Applicazione](#-configurazione-applicazione)
7. [Migrazioni Database](#-migrazioni-database)
8. [Verifica Installazione](#-verifica-installazione)
9. [Primo Utilizzo](#-primo-utilizzo)
10. [Troubleshooting](#-troubleshooting)

---

## 🔧 Prerequisiti

### Software Richiesto

1. **[.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)**
   - Verifica installazione: `dotnet --version` (deve essere ≥ 9.0)

2. **[Docker Desktop](https://www.docker.com/products/docker-desktop)**
   - Verifica installazione: `docker --version` e `docker-compose --version`
   - Assicurati che Docker Desktop sia avviato

3. **[Tesseract OCR](https://github.com/tesseract-ocr/tesseract)**
   - **Windows**: Scarica l'installer da [UB-Mannheim](https://github.com/UB-Mannheim/tesseract/wiki)
   - **Linux**: `sudo apt-get install tesseract-ocr tesseract-ocr-ita`
   - **macOS**: `brew install tesseract tesseract-lang`

4. **Git** (per clonare il repository)

### Account e Servizi

1. **Anthropic Claude API**
   - Registrati su [Anthropic Console](https://console.anthropic.com/)
   - Crea un account e ottieni la tua API key
   - Assicurati di avere credito disponibile (free tier disponibile per test)

---

## 🔒 Gestione Secret e Sicurezza

> **⚠️ IMPORTANTE**: I file contenenti secret **NON** devono mai essere committati su Git!

### Strategia di Sicurezza

Il progetto implementa una strategia multi-livello per proteggere i secret:

#### 1. `.gitignore` Configurato

Il file `.gitignore` già esclude:
```gitignore
# File di configurazione con secret
appsettings.Development.json
appsettings.Production.json
appsettings.*.json
!appsettings.json  # Solo il template senza secret è tracciato
.env
.env.local
secrets.json
```

#### 2. File Template

Il repository include file **template** da copiare e configurare:
- `appsettings.Development.json.template` → copia in `appsettings.Development.json`
- `.env.template` → copia in `.env`

#### 3. User Secrets (Metodo Raccomandato per .NET)

Per maggiore sicurezza, puoi usare .NET User Secrets per lo sviluppo locale:

```bash
cd src/DBEnigmistica.Console
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dbenigmistica;Username=dbenigmistica;Password=YOUR_PASSWORD"
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-api03-YOUR_API_KEY"
```

I User Secrets sono memorizzati in:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\`
- **Linux/macOS**: `~/.microsoft/usersecrets/`

#### 4. Variabili d'Ambiente (Produzione)

In produzione, usa variabili d'ambiente:
```bash
export ConnectionStrings__DefaultConnection="Host=prod-server;..."
export Anthropic__ApiKey="sk-ant-api03-..."
```

---

## 💾 Setup Database

### 1. Avvio Docker Compose

```bash
# Dalla root del progetto
docker-compose up -d
```

Questo comando avvia:
- **PostgreSQL 16** sulla porta `5432`
- **PgAdmin 4** su `http://localhost:5050`

### 2. Verifica Stato Container

```bash
docker-compose ps
```

Dovresti vedere:
```
NAME                        STATUS
dbenigmistica_postgres      Up (healthy)
dbenigmistica_pgadmin       Up
```

### 3. Credenziali di Default

Le credenziali sono definite in `docker-compose.yml`:

**PostgreSQL:**
- Host: `localhost`
- Port: `5432`
- Database: `dbenigmistica`
- Username: `dbenigmistica`
- Password: il valore di `POSTGRES_PASSWORD` nel file `.env`

**PgAdmin:**
- URL: `http://localhost:5050`
- Email: `admin@dbenigmistica.local`
- Password: il valore di `PGADMIN_DEFAULT_PASSWORD` nel file `.env`

### 4. Modifica Password (Raccomandato)

Per sicurezza, modifica le password di default:

**Metodo 1: File `.env` (Raccomandato)**

```bash
# Copia il template
cp .env.template .env

# Modifica .env con le tue credenziali
# Usa un editor di testo per modificare:
POSTGRES_PASSWORD=TUA_PASSWORD_SICURA_QUI
PGADMIN_DEFAULT_PASSWORD=TUA_PASSWORD_PGADMIN_QUI
```

Poi aggiorna `docker-compose.yml` per usare `.env`:

```yaml
services:
  postgres:
    env_file:
      - .env
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
```

**Metodo 2: Modifica Diretta docker-compose.yml (Locale)**

⚠️ Se usi questo metodo, assicurati di NON committare le modifiche!

```bash
# Dopo la modifica, usa:
git update-index --assume-unchanged docker-compose.yml
```

### 5. Riavvia i Container

```bash
docker-compose down
docker-compose up -d
```

---

## 🔍 Setup Tesseract OCR

### Windows

1. **Scarica Installer**
   - [Tesseract Installer per Windows](https://github.com/UB-Mannheim/tesseract/wiki)
   - Scegli versione 64-bit

2. **Installa Tesseract**
   - Path predefinito: `C:\Program Files\Tesseract-OCR`
   - ✅ Seleziona l'opzione per installare i language data durante l'installazione
   - ✅ Assicurati che sia installato `ita.traineddata` (lingua italiana)

3. **Verifica Installazione**
   ```bash
   tesseract --version
   ```

4. **Scarica Language Data (se non incluso)**
   - [tessdata repository](https://github.com/tesseract-ocr/tessdata)
   - Scarica `ita.traineddata`
   - Copia in `C:\Program Files\Tesseract-OCR\tessdata\`

### Linux

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install tesseract-ocr tesseract-ocr-ita

# Verifica
tesseract --version
tesseract --list-langs  # Deve mostrare 'ita'
```

### macOS

```bash
# Con Homebrew
brew install tesseract tesseract-lang

# Verifica
tesseract --version
tesseract --list-langs  # Deve mostrare 'ita'
```

### Path Tesseract

Annota il path della cartella `tessdata`:
- **Windows**: `C:\Program Files\Tesseract-OCR\tessdata`
- **Linux**: `/usr/share/tesseract-ocr/4.00/tessdata`
- **macOS**: `/usr/local/share/tessdata` o `/opt/homebrew/share/tessdata`

---

## 🤖 Setup Anthropic Claude API

### 1. Crea Account Anthropic

1. Vai su [Anthropic Console](https://console.anthropic.com/)
2. Registrati o accedi con il tuo account
3. Completa la verifica dell'account (potrebbe richiedere email o telefono)

### 2. Ottieni API Key

1. Nel dashboard, vai su **API Keys**
2. Clicca su **Create Key**
3. Copia la chiave (formato: `sk-ant-api03-...`)
   
   ⚠️ **IMPORTANTE**: Salva la chiave in un posto sicuro! Non sarà più visibile dopo la creazione.

### 3. Verifica Crediti

- Controlla il saldo crediti nel dashboard
- Free tier: ~$5 di credito per testare
- Per uso estensivo: configura metodo di pagamento

### 4. Modelli Disponibili

Il progetto è configurato per usare:
- **claude-sonnet-5-5** (default, bilanciato costo/prestazioni)
- Altri modelli: vedi l'elenco ufficiale Anthropic; imposta `Anthropic:Model` di conseguenza

---

## ⚙️ Configurazione Applicazione

### Metodo 1: File appsettings.Development.json (Semplice)

```bash
cd src/DBEnigmistica.Console

# Copia il template
copy appsettings.Development.json.template appsettings.Development.json

# Su Linux/macOS
# cp appsettings.Development.json.template appsettings.Development.json
```

Modifica `appsettings.Development.json` con un editor:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dbenigmistica;Username=dbenigmistica;Password=LA_TUA_PASSWORD_POSTGRES"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-api03-LA_TUA_API_KEY_ANTHROPIC",
    "Model": "claude-sonnet-5-5",
    "MaxTokens": 4096
  },
  "Ocr": {
    "TesseractDataPath": "C:/Program Files/Tesseract-OCR/tessdata",
    "Language": "ita"
  },
  "Processing": {
    "BatchSize": 5,
    "MaxDegreeOfParallelism": 4
  }
}
```

### Metodo 2: User Secrets (Raccomandato per Sviluppo)

```bash
cd src/DBEnigmistica.Console

# Inizializza user secrets
dotnet user-secrets init

# Configura connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dbenigmistica;Username=dbenigmistica;Password=LA_TUA_PASSWORD"

# Configura Anthropic API key
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-api03-LA_TUA_API_KEY"

# Configura Tesseract path (opzionale, usa default se non specificato)
dotnet user-secrets set "Ocr:TesseractDataPath" "C:/Program Files/Tesseract-OCR/tessdata"

# Visualizza tutti i secrets configurati
dotnet user-secrets list
```

### Metodo 3: Variabili d'Ambiente (Produzione/CI/CD)

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection = "Host=prod-db;..."
$env:Anthropic__ApiKey = "sk-ant-api03-..."

# Linux/macOS/Git Bash
export ConnectionStrings__DefaultConnection="Host=prod-db;..."
export Anthropic__ApiKey="sk-ant-api03-..."
```

### Configurazione Parametri

#### Connection String

Formato: `Host={host};Port={port};Database={database};Username={username};Password={password}`

**Esempio Locale:**
```
Host=localhost;Port=5432;Database=dbenigmistica;Username=dbenigmistica;Password=mypassword123
```

**Esempio Produzione:**
```
Host=prod-server.example.com;Port=5432;Database=dbenigmistica_prod;Username=app_user;Password=secure_password;SSL Mode=Require
```

#### Anthropic Configuration

```json
"Anthropic": {
  "ApiKey": "sk-ant-api03-...",  // La tua API key
  "Model": "claude-sonnet-5-5",  // Modello da usare
  "MaxTokens": 4096  // Max token per risposta (1000-8000)
}
```

#### OCR Configuration

```json
"Ocr": {
  "TesseractDataPath": "C:/Program Files/Tesseract-OCR/tessdata",  // Path tessdata
  "Language": "ita"  // Codice lingua (ita=italiano, eng=inglese)
}
```

**Path comuni:**
- Windows: `C:/Program Files/Tesseract-OCR/tessdata`
- Linux: `/usr/share/tesseract-ocr/4.00/tessdata`
- macOS: `/usr/local/share/tessdata` o `/opt/homebrew/share/tessdata`

#### Processing Configuration

```json
"Processing": {
  "BatchSize": 5,  // N. pagine elaborate in parallelo (5-10)
  "MaxDegreeOfParallelism": 4  // Thread paralleli (2-8, dipende da CPU)
}
```

---

## 🗄️ Migrazioni Database

### 1. Installazione Entity Framework Tools

```bash
# Installa tools globali (se non già fatto)
dotnet tool install --global dotnet-ef

# Verifica installazione
dotnet ef --version
```

### 2. Restore Dipendenze

```bash
# Dalla root del progetto
dotnet restore
```

### 3. Applica Migrazioni

**Metodo CLI Built-in (Raccomandato):**

```bash
cd src/DBEnigmistica.Console
dotnet run -- migrate
```

**Metodo Entity Framework Tools:**

```bash
cd src/DBEnigmistica.Console
dotnet ef database update
```

### 4. Verifica Database

Connettiti con PgAdmin o psql:

```bash
# Con psql
psql -h localhost -U dbenigmistica -d dbenigmistica

# Query di verifica
\dt  # Lista tabelle

# Dovresti vedere:
# - magazines
# - pages
# - games
# - game_solutions
# - game_type_schemas
# - __EFMigrationsHistory
```

---

## ✅ Verifica Installazione

### 1. Build Progetto

```bash
# Dalla root
dotnet build

# Verifica che build sia successful senza errori
```

### 2. Test Configurazione

```bash
cd src/DBEnigmistica.Console

# Mostra help
dotnet run -- --help

# Dovrebbe mostrare lista comandi disponibili
```

### 3. Test Database Connection

```bash
# Lista giochi (dovrebbe essere vuoto inizialmente)
dotnet run -- list-games

# Output atteso: "No games found in database."
```

### 4. Test Tesseract

Crea un file di test `test.png` con del testo e:

```bash
tesseract test.png stdout -l ita
```

Dovrebbe riconoscere il testo nell'immagine.

### 5. Test Anthropic API

```bash
# Puoi testare con un curl (sostituisci YOUR_API_KEY)
curl https://api.anthropic.com/v1/messages \
  -H "x-api-key: $ANTHROPIC_API_KEY" \
  -H "anthropic-version: 2023-06-01" \
  -H "content-type: application/json" \
  -d '{"model":"claude-sonnet-5-5","max_tokens":100,"messages":[{"role":"user","content":"Hello"}]}'
```

---

## 🎮 Primo Utilizzo

### Scenario: Scansione Prima Rivista

1. **Prepara PDF**
   ```bash
   # Crea una cartella per i PDF
   mkdir riviste
   # Copia alcuni PDF di riviste enigmistiche nella cartella
   ```

2. **Scansiona Cartella**
   ```bash
   dotnet run -- scan-folder ./riviste --title "La Settimana Enigmistica" --periodicity Weekly
   ```

3. **Monitoraggio**
   - L'applicazione mostrerà progress bar per ogni pagina
   - OCR → AI Classification → Database save

4. **Verifica Risultati**
   ```bash
   # Lista giochi estratti
   dotnet run -- list-games --details

   # Export in JSON
   dotnet run -- export-games --output risultati.json --format json
   ```

### Scenario: Analisi Nuovo Tipo di Gioco

1. **Crea file descrizione** (`nuovo_gioco.md`):
   ```markdown
   # Word Search (Cerca Parole)
   
   Gioco dove bisogna trovare parole nascoste in una griglia.
   
   ## Caratteristiche
   - Griglia NxN (tipicamente 10x10 o 15x15)
   - Lista di parole da trovare
   - Parole possono essere orizzontali, verticali, diagonali
   ```

2. **Analizza con AI**
   ```bash
   dotnet run -- analyze-game nuovo_gioco.md
   ```

3. **Conferma Schema**
   - L'AI proporrà uno schema per la tabella `word_search_games`
   - Conferma interattivamente
   - Tabella viene creata automaticamente

---

## 🔥 Troubleshooting

### Errore: "Database connection failed"

**Causa**: PostgreSQL non è avviato o credenziali errate.

**Soluzione**:
```bash
# Verifica container Docker
docker-compose ps

# Se non è running
docker-compose up -d

# Controlla logs
docker-compose logs postgres

# Verifica credenziali in appsettings.Development.json
```

### Errore: "Tesseract not found"

**Causa**: Tesseract non è installato o path errato.

**Soluzione**:
```bash
# Verifica installazione
tesseract --version

# Se non installato, vedi sezione Setup Tesseract OCR

# Verifica path tessdata in appsettings:
"Ocr": {
  "TesseractDataPath": "IL_PATH_CORRETTO_QUI"
}
```

### Errore: "Anthropic API authentication failed"

**Causa**: API key non valida o non configurata.

**Soluzione**:
```bash
# Verifica API key su Anthropic Console
# https://console.anthropic.com/settings/keys

# Ricontrolla configurazione:
dotnet user-secrets list
# oppure controlla appsettings.Development.json

# Testa API key con curl (vedi sezione Verifica Installazione)
```

### Errore: "Language 'ita' not found"

**Causa**: File `ita.traineddata` non installato.

**Soluzione**:
```bash
# Scarica ita.traineddata
# https://github.com/tesseract-ocr/tessdata/raw/main/ita.traineddata

# Copia in tessdata folder
# Windows: C:\Program Files\Tesseract-OCR\tessdata\ita.traineddata
# Linux: /usr/share/tesseract-ocr/4.00/tessdata/ita.traineddata

# Verifica
tesseract --list-langs
# Deve mostrare 'ita'
```

### Errore: "Migration failed" o "Table already exists"

**Causa**: Database in stato inconsistente.

**Soluzione**:
```bash
# Opzione 1: Reset database
docker-compose down -v
docker-compose up -d
dotnet run -- migrate

# Opzione 2: Drop manualmente
psql -h localhost -U dbenigmistica -d dbenigmistica
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
\q
dotnet run -- migrate
```

### Performance Lenta durante OCR

**Causa**: Parametri di processing troppo aggressivi.

**Soluzione**: Riduci parallelismo in `appsettings`:
```json
"Processing": {
  "BatchSize": 2,  // Ridotto da 5
  "MaxDegreeOfParallelism": 2  // Ridotto da 4
}
```

### Errore: "Cannot find dotnet tool 'ef'"

**Causa**: Entity Framework Tools non installato.

**Soluzione**:
```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

---

## 🎯 Checklist Setup Completa

Prima di iniziare ad usare l'applicazione, verifica:

- [ ] .NET 9 SDK installato (`dotnet --version`)
- [ ] Docker Desktop avviato
- [ ] PostgreSQL container running (`docker-compose ps`)
- [ ] Tesseract OCR installato (`tesseract --version`)
- [ ] `ita.traineddata` presente (`tesseract --list-langs`)
- [ ] Account Anthropic creato e API key ottenuta
- [ ] `appsettings.Development.json` configurato (o User Secrets)
- [ ] Connection string con password corretta
- [ ] Anthropic API key configurata
- [ ] Tesseract path configurato correttamente
- [ ] Migrazioni database applicate (`dotnet run -- migrate`)
- [ ] Build successful (`dotnet build`)
- [ ] Test comando help funzionante (`dotnet run -- --help`)

---

## 📞 Supporto

### Risorse

- **GitHub Issues**: [Repository Issues](link-to-your-repo/issues)
- **Anthropic Docs**: [https://docs.anthropic.com](https://docs.anthropic.com)
- **PostgreSQL Docs**: [https://www.postgresql.org/docs/](https://www.postgresql.org/docs/)
- **Tesseract Wiki**: [https://github.com/tesseract-ocr/tesseract/wiki](https://github.com/tesseract-ocr/tesseract/wiki)

### Logs e Debug

Per abilitare logging dettagliato, modifica in `appsettings.Development.json`:

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",  // Aumenta a Trace per ancora più dettagli
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

I log vengono salvati in:
- **Console**: output standard
- **File** (se configurato): `./logs/app-{Date}.log`

---

## 🔐 Best Practices per Produzione

### 1. Secret Management

**NON usare** `appsettings.json` per produzione!

Usa uno di questi metodi:
- **Azure Key Vault** (se su Azure)
- **AWS Secrets Manager** (se su AWS)
- **HashiCorp Vault**
- **Variabili d'ambiente** (minimo)

### 2. Database

- ✅ Usa password forti (16+ caratteri, misti)
- ✅ Abilita SSL per connessioni: `SSL Mode=Require`
- ✅ Crea user dedicato con permessi limitati
- ✅ Backup automatici configurati
- ✅ Separare database dev/staging/prod

### 3. API Keys

- ✅ Rotazione periodica delle chiavi
- ✅ Limiti di rate configurati
- ✅ Monitoring utilizzo e costi
- ✅ API key diverse per dev/prod

### 4. Networking

- ✅ Database NON esposto pubblicamente
- ✅ Firewall rules appropriate
- ✅ VPN per accesso remoto DB

---

## 📝 Note Finali

### File da NON Committare (già in .gitignore)

- ❌ `appsettings.Development.json`
- ❌ `appsettings.Production.json`
- ❌ `.env`
- ❌ Qualsiasi file con password, API key, connection string

### File da Committare

- ✅ `appsettings.json` (template senza secret)
- ✅ `appsettings.Development.json.template`
- ✅ `.env.template`
- ✅ `docker-compose.yml` (con valori placeholder o variabili)
- ✅ Questo file `SETUP_INSTRUCTIONS.md`

### Condivisione con Team

Quando un nuovo sviluppatore clona il repository:

1. Deve seguire questo documento
2. Creare il proprio `appsettings.Development.json` dal template
3. Ottenere le proprie API key Anthropic
4. Configurare il proprio ambiente locale

**NON condividere**:
- La tua API key Anthropic
- Password del database condiviso
- File di configurazione con secret

---

**Buon lavoro con DBEnigmistica! 🎯**

Per domande o problemi, apri una issue su GitHub.
