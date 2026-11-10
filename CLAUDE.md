# DBEnigmistica - istruzioni di progetto

Leggi questo file all'inizio di ogni sessione. Lo stato reale è in `ROADMAP.md`: se i due documenti divergono, vince il roadmap.

## 1. Il progetto

- DBEnigmistica è un prodotto per l'enigmistica digitale in italiano, costruito attorno a Riviste e Giochi.
- Tre superfici: **Studio** (privato, sui dispositivi del proprietario), **Sito pubblico** (per i giocatori), **Admin** (gestione del Sito).
- Tre componenti: **Motore di estrazione** (legge le Riviste in PDF), **Motore di creazione** (genera Giochi originali), **Lettore** (fa giocare a schermo).

## 2. Glossario

Usa sempre questi termini, con la maiuscola iniziale.

- **Rivista**: un numero di una pubblicazione periodica in PDF, da cui il Motore di estrazione legge i Giochi.
- **Gioco**: un'unità di gioco, per esempio un cruciverba o un anagramma. Distingui "Gioco estratto" da "Gioco originale".
- **Tipologia**: il tipo di gioco, definito da un descrittore versionato (per esempio `cruciverba-classico`). Non usare "tipo" o "categoria" al suo posto.
- **Schema dinamico**: il modo in cui una Tipologia ottiene tabelle proprie, generate come migrazioni da rivedere. Non è DDL eseguito a runtime: quello del vecchio codice va eliminato.
- **Studio**: la superficie privata con archivio delle Riviste, estrazione e revisione (profilo `studio`).
- **Sito pubblico**: la superficie per i giocatori su internet, con i soli Giochi approvati (profilo `pubblico`).
- **Admin**: la gestione del Sito pubblico, nello stesso profilo pubblico.
- **Lettore**: il componente che fa giocare a schermo. Usato in Studio e Sito, poi incorporabile.
- **Motore di estrazione**: la pipeline che trasforma una Rivista in Giochi estratti con Claude e visione.
- **Motore di creazione**: la pipeline che genera Giochi originali.

## 3. Documenti di riferimento

- `ROADMAP.md`: fasi, stato, decisioni, domande aperte. È la fonte di verità. Leggi la sezione della fase in corso prima di lavorare.
- `docs\PROMPT-INIZIALE.md`: requisiti originali con identificativi (OBJ-, APP-, DB-, CLI-, TEC-, OUT-, UX-, AI-, RQ-). Usali per tracciare il lavoro.
- `docs\VALUTAZIONE-STATO-ATTUALE.md`: analisi del codice del 10/11/2026. Spiega perché un pezzo va riscritto. Non è la guida al lavoro.
- `docs\business\05-piano-commerciale.md`: vincoli commerciali, soglie G1-G6 e date. Gli altri file di `docs\business\` sono contesto.
- `docs\design\ui\PROPOSTE-UI.md` e `docs\design\ux\PROPOSTE-UX.md`: proposte. Le scelte adottate sono in ROADMAP, fase 2 (identità Banco per Studio e Admin, Carta con le regole di Lente per Lettore e Sito). I file HTML sono anteprime.
- `docs\CI.md`: flussi GitHub Actions, impostazioni manuali del repository, creazione dei tag.
- `docs\ripianificazione-design.md`: decisioni dell'intervista di ripianificazione. Oggi non esiste. Se manca, non ricostruire le decisioni a memoria: segnalalo al proprietario.
- `docs\plan.md` e `docs\copilot_memory.md`: storici e superati. Non usarli per decidere.
- `docs\giochi_enigmistici.md`: materiale di riferimento sui giochi, usato per il roadmap.

## 4. Regole di lavoro (obblighi del proprietario)

**Orchestrazione**
- L'agente principale non scrive codice né documenti. Lancia agenti secondari e ne coordina il lavoro.
- Non più di 2-3 agenti in parallelo. Ogni agente ha un compito che non tocca i file degli altri.

**Modello per agente**
- Haiku: documentazione (README, guide, testi d'aiuto, ADR da decisioni già prese).
- Sonnet: scrittura di codice.
- Opus: ragionamenti complessi (architettura, pianificazione, analisi, descrittori, prompt).
- Opus con effort `xhigh`: revisione del codice a fine di ogni ondata.

**Decisioni del proprietario**
- Ogni dubbio o decisione che spetta al proprietario si pone con la skill `grill-with-ui`, per tutta la durata del progetto.
- La skill `grilling` (grill-me) si usa solo se il proprietario la chiede espressamente.

**Fine di ogni round di lavoro.** L'agente principale mostra, in quest'ordine:
1. La tabella degli stati delle fasi.
2. Cosa si aspetta dal proprietario, e in particolare cosa è bloccato da lui.
3. Se servono operazioni manuali, una guida passo passo.
4. Il prompt suggerito per il passo successivo.

**Bug.** Prima di toccare il codice: trova tutti i punti d'uso con ricerca, elenca file e riga, fatti approvare la lista. Poi correggi tutto in un solo passaggio e verifica con build e test.

**Test e pubblicazione**
- Test e pubblicazione avvengono solo con GitHub Actions. Niente pubblicazioni manuali di immagini o eseguibili.
- Una fase non è conclusa se la CI non è verde.
- Una fase non si chiude senza l'approvazione esplicita del proprietario. Prima della chiusura esegui la verifica dei criteri di accettazione.

**Prima di ogni commit**
- Esegui la revisione del codice e risolvi i problemi CRITICAL e HIGH.
- Nessun segreto nei file tracciati. Controlla con gitleaks.
- Committa solo se il proprietario lo chiede.
- Messaggio: una o due frasi brevi, nel formato `tipo: descrizione`, con tipo tra feat, fix, refactor, docs, test, chore, perf, ci. Nessuna riga di attribuzione.
- Descrivi il comportamento osservabile, non nomi di classi, metodi o file interni.

## 5. Vincoli di prodotto (non negoziabili)

- I Giochi estratti dalle Riviste non vanno mai pubblicati. Stanno nell'archivio separato dello Studio, con blocco su tre livelli: policy di dominio, vincoli del database, database pubblico separato.
- Ogni Gioco ha origine, diritti e registro del contributo umano. Il registro è in sola aggiunta: non si modifica né si cancella.
- Il Motore di creazione usa solo regole di formato, cioè le regole delle Tipologie approvate dal proprietario. Usare i Giochi estratti come esempi richiede un parere legale favorevole e un interruttore (ROADMAP, sezione 2.2).
- Tabelle per Tipologia: generate come migrazioni da rivedere, mai DDL eseguito al volo. Le migrazioni confermate nello Studio si applicano solo con un comando esplicito.
- Il profilo pubblico non ha la stringa di connessione dello Studio né espone `/api/studio/*`.
- Il pubblico del Sito è spesso over 65 e usa il telefono: accessibilità come requisito. Testo di base 18-20 px, aree di tocco di almeno 44 px, focus visibile, nessuna violazione gravi o critiche con axe-core, scorciatoie disattivabili e rimappabili.

## 6. Tecnologia e comandi

**Stack**
- .NET 10 (`net10.0`), EF Core, PostgreSQL. Soluzione `DBEnigmistica.slnx`.
- Progetti in `src\`: Domain, Application, Infrastructure, AI, Console. Test in `tests\DBEnigmistica.Tests` (cartelle `Unit` e `Integration`).
- Dalla fase 2: interfaccia React e TypeScript con API ASP.NET Core. Dalla fase 5: involucro desktop Tauri. Non ancora presenti.
- Estrazione con modelli Claude con visione. Il modello è configurabile. Il default nel codice (`claude-3-5-sonnet`) è ritirato: non usarlo.

**Compilare e testare** (comandi di `.github\workflows\ci.yml`)
- `dotnet restore DBEnigmistica.slnx`
- `dotnet build DBEnigmistica.slnx --configuration Release --no-restore`
- `dotnet test DBEnigmistica.slnx --configuration Release --no-build`
- I test d'integrazione (dalla fase 0) usano Testcontainers: serve Docker in esecuzione.

**Database locale** (`docker-compose.yml`)
- Crea `.env` nella radice copiando `.env.template` e imposta tu i valori (`.env` è ignorato da git). Variabili: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, e per pgAdmin `PGADMIN_DEFAULT_EMAIL`, `PGADMIN_DEFAULT_PASSWORD`. Se una manca, Compose si ferma.
- Avvio: `docker compose up -d postgres`.

**Console**
- Aiuto: `dotnet run --project src\DBEnigmistica.Console -- --help`. Oggi i comandi non partono: vedi stato.
- Variabili d'ambiente: `ConnectionStrings__DefaultConnection` (stringa di connessione) e `Anthropic__ApiKey` (chiave API, letta come `Anthropic:ApiKey`).

**Segreti**
- `src\DBEnigmistica.Console\appsettings.Development.json` e `.env`, se esistono, contengono segreti e sono ignorati da git. Non aprirli, non stamparli, non copiarli. Cita solo i nomi delle chiavi, mai i valori.
- `.env.template` contiene ancora `ANTHROPIC_API_KEY` e `TESSERACT_DATA_PATH`: vanno rimossi in fase 0, perché Tesseract è eliminato.

## 7. Stato attuale

- Fase 0 (Fondamenta) in corso: `net10.0` e password fuori dai file tracciati sono fatti. CI e Dockerfile sono predisposti ma mai eseguiti su GitHub. Al 10/11/2026 non ci sono commit.
- Oggi 15 test su 24 falliscono per difetti noti, da risanare nella fase 0. La CLI non parte ("Service provider not available"). Il Motore di estrazione non produce risultati. Compose usa PostgreSQL 16, i test della fase 0 usano 17.

## 8. Protocollo di avanzamento

- Prima di eseguire una fase verifica che esistano e non siano vuoti `ROADMAP.md`, `CLAUDE.md`, `.planning\phases\<fase>\CONTEXT.md` e `.planning\phases\<fase>\PLAN.md`. Se ne manca uno, fermati ed elencalo.
- Prima di ogni ondata leggi `PROGRESS.md` nella radice del repo. Salta le ondate con riga `STATUS:complete`. Non rieseguire mai un'ondata completa.
- Dopo ogni ondata aggiungi una riga: `PHASE:<id> WAVE:<id> STATUS:<complete|failed> TS:<ISO-8601 UTC>`.
- Se `PROGRESS.md` non esiste, crealo prima della prima ondata. Oggi non esiste.
- Dopo un'interruzione (per esempio un limite di utilizzo): riporta l'ultima ondata completa e quelle restanti, poi riprendi da lì.
