# Decisioni per la chiusura della fase 0

Decisioni del proprietario del 13 novembre 2026. Chiudono i punti aperti dopo l'ondata 0.3 di `ROADMAP.md`: il criterio 12, il codice d'uscita di `dbe verify-integrity`, la versione di PostgreSQL, il modello predefinito del Fornitore AI, l'accettazione degli ADR e il tag di chiusura. Le modifiche di codice e di configurazione entrano con una PR e il tag `v0.1.1`. Il tag `v0.1.0` resta com'è. La fase 0 si chiude solo con l'approvazione esplicita del proprietario.

## Perché

Il repository è privato su piano gratuito GitHub, che non permette la protezione dei rami né i ruleset. Restano due difetti nella versione `v0.1.0`. Il codice 1 per il registro alterato non distingue una manomissione da un guasto. Compose usa PostgreSQL 16, mentre i test usano la 17.

## Decisioni

### D1. Criterio 12: CI e tag, senza protezione di `main`

**Decisione.** I quattro lavori di `ci.yml` devono essere verdi su ogni commit di `main`. Il tag `v0.1.1` fa pubblicare a `publish.yml` l'immagine `ghcr.io/f1-solution/dbenigmistica:0.1.1`, e `docker run <immagine> --version` stampa `0.1.1`. Un tag su un commit che non discende da `main` fa fallire `publish.yml` prima di pubblicare. La protezione di `main` con controlli obbligatori è rimandata a quando il piano GitHub la consente.

**Opzioni scartate.**
- *GitHub Team a pagamento*: costo ricorrente per una sola protezione. Si riapre se il piano cambia per altri motivi.
- *Repository pubblico*: permetterebbe la protezione dei rami, ma il repository contiene il piano commerciale, che non va esposto. È un'eccezione da tenere annotata.

### D2. Codice d'uscita di `dbe verify-integrity`

**Decisione.** Codice 0 se il registro è integro. Codice 5 se il registro è alterato, con l'id del primo evento non valido su stderr. Codice 1 per gli errori generici, per esempio un database non raggiungibile.

**Opzioni scartate.**
- *Codice 1 per entrambi i casi*: uno script non distinguerebbe una manomissione da un guasto. Un controllo automatico non saprebbe se allarmare o riprovare.

### D3. PostgreSQL 18 ovunque

**Decisione.** I test d'integrazione usano Testcontainers con `postgres:18-alpine`. `docker-compose.yml` usa `postgres:18-alpine`. Un volume locale creato con PostgreSQL 16 non si apre con la 18. Va ricreato con `docker compose down -v` seguito da `docker compose up -d postgres`, che cancella i dati locali. In alternativa va esportato prima con `pg_dump`. Dall'immagine 18 il volume dati si monta su `/var/lib/postgresql`.

**Opzioni scartate.**
- *PostgreSQL 17 solo in Compose*: test e sviluppo locale resterebbero su versioni diverse, e il problema tornerebbe alla prossima versione.
- *Lasciare PostgreSQL 16 in Compose*: lo stesso problema, con una versione più vecchia dei test.

### D4. Modello predefinito del Fornitore AI

**Decisione.** Il modello predefinito resta `claude-sonnet-5-5`, identificativo valido e corrente. L'avviso «da verificare» e la frase «quello configurato oggi è già ritirato» del rischio 5 sono superati. Il confronto tra modelli resta alla linea di base della fase 1.

**Opzioni scartate.**
- *Opus 5.5 come predefinito*: consuma più quota dell'abbonamento, che è condivisa con lo sviluppo (rischio 6 di `ROADMAP.md`).
- *Rimandare la scelta*: la fase 1 misura già i modelli sul set di riferimento. Rimandare non darebbe dati in più.

### D5. ADR 001-006 accettati

**Decisione.** Il proprietario approva gli ADR 001-006 il 13/11/2026, con stato «Accettato». L'ADR 005 viene accettato con il punto 7 aggiornato al codice 5 (D2). L'ADR 006 viene accettato con la conseguenza su PostgreSQL aggiornata a D3 (versione unica 18). L'ADR 004 viene accettato con la conseguenza sul modello aggiornata a D4.

**Opzioni scartate.** Nessuna. Le modifiche di D2, D3 e D4 entrano prima dell'accettazione, perché gli ADR sono ancora proposte.

### D6. Tag `v0.1.0` e chiusura con `v0.1.1`

**Decisione.** Il tag `v0.1.0` (commit 03a773b) resta com'è. È stato pubblicato l'11/11/2026 con tutti i lavori di `publish.yml` verdi. Le modifiche di D2 e D3 entrano con una PR, e la fase 0 si chiude con il tag `v0.1.1`.

**Opzioni scartate.**
- *Spostare `v0.1.0` sul commit nuovo*: un tag pubblicato non si sposta. Chi ha già scaricato l'immagine o l'eseguibile non avrebbe più un riferimento valido.
- *Chiudere su `v0.1.0` senza un'immagine con le correzioni*: la versione chiusa avrebbe il codice 1 per il registro alterato e PostgreSQL 16 in Compose.

## Fatti verificati

- L'eseguibile della Release `v0.1.0` stampa `0.1.0` con `--version`.
- L'immagine `ghcr.io/f1-solution/dbenigmistica` non è stata ancora provata con `docker run`. Serve un login a GHCR del proprietario.
- Il repository è privato su piano gratuito. L'API di protezione dei rami risponde 403.
- Il tag `v0.1.0` è sul commit 03a773b, pubblicato l'11/11/2026.

## Ordine di chiusura

1. PR con le modifiche di codice e di documentazione per D2 e D3. La CI deve essere verde.
2. Unione in `main`.
3. Tag `v0.1.1` sul commit di `main`. `publish.yml` pubblica l'immagine `:0.1.1`.
4. Prova dell'immagine con `docker run ghcr.io/f1-solution/dbenigmistica:0.1.1 --version`, con il login GHCR del proprietario. Deve stampare `0.1.1`.
5. Approvazione del proprietario. Solo dopo, la fase 0 passa a «Fatto» in `ROADMAP.md`.

## Rischi

- **Nessuna protezione di `main`.** Un commit può arrivare su `main` senza CI verde. Contromisura: il proprietario verifica che la CI sia verde prima di unire e prima del tag.
- **Volume locale con PostgreSQL 16.** Con un volume creato dalla 16, il database non si avvia. Contromisura: le istruzioni in `SETUP_INSTRUCTIONS.md`.
- **Immagine non ancora provata.** La prova `docker run` del criterio 12 si fa dopo il tag `v0.1.1`.

## Rimandati

- Protezione di `main` e dei tag (D1), quando il piano GitHub lo consente.

## Discussioni aperte

- Nessuna. Le operazioni che restano sono nell'ordine di chiusura.
