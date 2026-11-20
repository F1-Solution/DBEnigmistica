# 00 - Impostazioni GitHub e ambiente locale

**Stato:** Da fare
**Fase:** 0 (chiusa); le impostazioni servono a tutte le fasi.

## Scopo

Completare le impostazioni manuali su GitHub indicate in `docs/CI.md`. Attivare il controllo dei segreti sul PC. Preparare il database locale e l'accesso a GHCR.

## Scadenza

Nessuna scadenza fissata nella ROADMAP. La voce è "Da fare (proprietario)" nella fase 0.

## Tempo stimato

Non stimato separatamente. La ROADMAP stima circa 7 ore per tutta la fase 0.

## Prerequisiti

- Accesso amministratore al repository `F1-Solution/DBEnigmistica` su GitHub.
- Docker Desktop installato e avviato. I test d'integrazione lo usano.

## Cosa ti serve

- Browser con l'account GitHub del repository.
- PowerShell.
- Un gestore di password per il token della Parte D.

## Procedura

### Parte A - Impostazioni su GitHub

NOTA: La protezione del ramo `main` e dei tag è rimandata (decisione D1, 13/11/2026). Non farla ora.

AVVERTENZA: Non scrivere token, password o chiavi in issue, commenti o file del repository.

1. Apri il repository su GitHub. Vai su *Settings > Actions > General*.
   Risultato: Vedi la sezione *Workflow permissions*.
2. Controlla *Workflow permissions*. Lascia selezionato "Read repository contents and packages permissions".
   Risultato: L'opzione è selezionata. I flussi dichiarano da soli i permessi in più.
3. Se *Actions permissions* limita le azioni, consenti `actions/*` e `docker/*`.
   Risultato: Le due famiglie sono consentite. Se non ci sono limiti, salta questo passo.
4. Vai su *Settings > Advanced Security*. Attiva *Dependabot alerts* e *Dependabot security updates*.
   Risultato: Le due voci mostrano "Enabled".
5. Opzionale: nella stessa pagina attiva *Secret scanning* e *Push protection*, se disponibili.
   Risultato: Le voci mostrano "Enabled", oppure non compaiono.
6. Dopo la prima pubblicazione dell'immagine, apri *Packages > dbenigmistica > Package settings*.
   Risultato: Vedi la visibilità del pacchetto.
7. Controlla che la visibilità sia **Private**.
   Risultato: Il pacchetto è privato. Se la visibilità è pubblica, cambiala in Private.
8. Controlla la sezione *Connected repository*.
   Risultato: Compare `F1-Solution/DBEnigmistica`. Se manca, scegli *Connect repository*.

### Parte B - Controllo dei segreti sul PC (hook gitleaks)

Ogni clone del repository va configurato una volta.

1. Installa gitleaks. Esegui:

```powershell
winget install gitleaks.gitleaks
```

   Risultato: Il comando termina senza errori.

2. Apri PowerShell nella cartella del repository. Esegui:

```powershell
cd C:\Progetti\DBEnigmistica
git config core.hooksPath .githooks
```

   Risultato: Il comando non stampa nulla.

3. Verifica l'impostazione. Esegui:

```powershell
git config core.hooksPath
```

   Risultato: Il comando stampa `.githooks`.

### Parte C - Database locale con Docker

AVVERTENZA: Non stampare, copiare o incollare il contenuto del file `.env` in chat o nel repository.

Le cinque variabili di `.env` sono: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `PGADMIN_DEFAULT_EMAIL`, `PGADMIN_DEFAULT_PASSWORD`. Se una manca, Compose si ferma.

1. Copia il modello nella radice del repository. Esegui:

```powershell
Copy-Item .env.template .env
```

   Risultato: Il file `.env` esiste. Git lo ignora.

2. Apri `.env` con un editor di testo. Imposta tu i valori delle cinque variabili. Salva il file.
   Risultato: Tutte e cinque le variabili hanno un valore.

   NOTA: Questo passo lo fai tu. L'agente non apre questo file.

3. Avvia il database. Esegui:

```powershell
docker compose up -d postgres
```

   Risultato: Il comando termina senza errori.

4. Controlla lo stato. Esegui:

```powershell
docker compose ps
```

   Risultato: Il servizio `postgres` mostra lo stato `healthy`.

ATTENZIONE: Un volume creato con PostgreSQL 16 non si apre con la versione 18 (decisione D3). Esporta prima i dati con `pg_dump`. Solo dopo, se accetti di perdere i dati locali, esegui `docker compose down -v` e poi il passo 3.

### Parte D - Accesso a GHCR (per scaricare l'immagine)

1. Crea un token classico su GitHub con il permesso `read:packages`. Il menu è *Settings del tuo account > Developer settings > Personal access tokens*.
   Risultato: Hai il token. Conservalo nel gestore di password.
2. Esegui il comando. Sostituisci `<utente-github>` con il tuo utente:

```powershell
docker login ghcr.io -u <utente-github>
```

3. Quando il comando chiede la password, incolla il token e premi Invio.
   Risultato: Il comando scrive `Login Succeeded`.

## Verifica finale

- Parte A, passi 4, 7 e 8 sono completati.
- `git config core.hooksPath` stampa `.githooks`.
- `docker compose ps` mostra `postgres` come `healthy`.
- Il login a GHCR riesce.

## Cosa consegnare all'agente principale

Scrivi un messaggio con questi punti, in italiano:

1. Quali passi della Parte A sono fatti, con la data.
2. Esito di `git config core.hooksPath` (il valore stampato).
3. Stato del servizio `postgres` (solo la parola, per esempio `healthy`).
4. Esito del login a GHCR: `Login Succeeded` sì o no.

Non inviare valori di variabili, password o token.

## Cosa sblocca

- Controllo dei segreti in locale prima di ogni commit.
- Test d'integrazione sul PC (Docker).
- Prova dell'immagine pubblicata (file 24).
- Avvio del prototipo con l'eseguibile pre-release (file 07).

---
Fonti: `docs/CI.md` (sezioni "Impostazioni da fare a mano su GitHub" e "Hook locale"); `CLAUDE.md`, sezione 6 "Database locale"; `docs/fase-0-chiusura-decisioni.md` (D1, D3, passo 4); `ROADMAP.md`, fase 0.
