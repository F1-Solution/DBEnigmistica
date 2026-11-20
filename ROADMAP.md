# DBEnigmistica - Roadmap

Data: 10 novembre 2026. Sostituisce `docs/plan.md`, che resta solo come documento storico.
Fonti lette per intero: `docs/PROMPT-INIZIALE.md`, `docs/plan.md`, `docs/VALUTAZIONE-STATO-ATTUALE.md` (di seguito "valutazione"), `docs/business/05-piano-commerciale.md` (di seguito "piano commerciale"), `docs/design/ux/PROPOSTE-UX.md`, `docs/design/ux/proposta-A.md`, `docs/design/ui/PROPOSTE-UI.md`, `docs/giochi_enigmistici.md`, `docs/fase-0-decisioni.md`.
Vincoli: le 12 decisioni del proprietario del 10/11/2026 e le 6 decisioni dello stesso giorno raccolte in `docs/fase-0-decisioni.md` (dove gira lo Studio, Fornitore AI, prova 1). Dove contraddicono un documento, vincono le decisioni; tra le due serie vince la seconda.
Convenzioni: **[P]** = decisione del proprietario; **[A]** = scelta dell'architetto, motivata in una riga; **[S]** = stima.

---

## Sintesi

1. Un solo prodotto web con due profili di esecuzione: **studio** (privato: Studio e archivio delle Riviste) e **pubblico** (Sito e Admin). Interfaccia in React e TypeScript con API ASP.NET Core [P]; involucro desktop dello Studio con Tauri [P]; la riga di comando `dbe` resta per i batch.
2. Ordine [P]: fondamenta, poi **Motore di estrazione e Studio** (fasi 1-5), poi **Motore di creazione** (fasi 6-7), infine **Sito pubblico e Admin** (fasi 8-9). Accanto all'estrazione, la **fase 1b** (confermata) costruisce un prototipo minimo di creazione solo per misurare il tempo di revisione entro la soglia G1 del 28 febbraio 2027, su sei famiglie di Giochi con 5 candidati ciascuna [P].
3. Tipologie in sequenza in ogni componente: cruciverba classico e giochi brevi, poi crittografie e giochi a chiave, poi rebus. Sudoku e giochi logici entrano nel catalogo dalla fase 1b (generati e verificati da un algoritmo, senza AI) e nell'estrazione con la fase 5. Ogni tipologia è un **descrittore** da cui si generano la migrazione delle tabelle dedicate, lo schema dell'output dell'AI e la resa nel Lettore.
4. Lettura delle pagine: immagine della pagina, testo nativo del PDF e una chiamata per pagina al **Fornitore AI** con visione e output strutturato. Si parte con Claude tramite l'abbonamento del proprietario come misura di riferimento; diventa predefinito il fornitore che supera la soglia di accuratezza al costo minore, tra quelli con condizioni d'uso verificate [P]. L'accuratezza si misura su un Set di riferimento fornito dal proprietario. Niente Tesseract.
5. **Nessuna API a consumo come scelta predefinita** [P]: il programma raggiunge l'AI attraverso un'unica interfaccia verso il Fornitore AI; la creazione usa Claude Code in modalità non interattiva con l'abbonamento, solo nello Studio privato. I limiti dell'abbonamento sono condivisi con lo sviluppo; il Sito pubblico non dipende dall'abbonamento. L'adattatore API resta possibile, non predefinito.
6. I Giochi estratti non vanno mai online, con un blocco su tre livelli: domain policy, vincoli del database e un database pubblico separato. Nella creazione si usa solo la conoscenza di formato (regole delle tipologie approvate dal proprietario). L'uso come esempi richiede un parere legale favorevole, da ricevere entro il 28 febbraio 2027, e un interruttore [P].
7. Fase 0 **conclusa il 19/11/2026**, ondate 0.1-0.3 concluse: CI verde su `main` (build e test, vulnerabilità dei pacchetti, controllo dei segreti, immagine Docker); OCR, layout e DDL a runtime eliminati; CLI `dbe` su System.CommandLine e test su PostgreSQL vero; modello di provenienza, diritti e registro in sola aggiunta; Fornitore AI configurato senza API a consumo. Decisioni del 13/11/2026: codice 5 per il registro alterato, PostgreSQL 18, ADR accettati, chiusura con tag `v0.1.1` dopo la PR. Chiusura sul tag `v0.1.1`, approvata dal proprietario il 19/11/2026, con l'immagine `0.1.1` verificata. Restano le domande al legale (entro il 15/12/2026).
8. Ore richieste al proprietario [S]: circa 129,5 di guida, prova e collaudo fino al lancio (di cui 14 di prova 1), più circa 15 di contenuti per riempire la coda. Sono distribuite su 11 fasi (0-9 più 1b) e 38 ondate di 2-3 agenti.
9. Date del piano commerciale tenute: parere legale e soglia **G1 al 28 febbraio 2027** (regge con 1-2 ore di margine sulla prova 1, vedi fase 1b). Date non raggiungibili con l'ordine scelto, che va rispettato: Sito online il **1 settembre 2027** invece di marzo, quindi da G2 in poi soglie spostate di 3-6 mesi (sezione 5).
10. A 24 mesi (30 novembre 2028) la soglia realistica è 450 € netti al mese; gli 800 € slittano al 31 maggio 2029.
11. Restano aperte decisioni non bloccanti per l'avvio. Le prime servono entro il 6 dicembre 2026 (verifica delle condizioni d'uso dell'abbonamento) e il 7 dicembre 2026 (set di PDF di prova) (sezione "Domande aperte").

---

## Tabella delle fasi

| N | Nome | Obiettivo in una riga | Dipende da | Stato | Cosa serve dal proprietario |
|---|---|---|---|---|---|
| - | Analisi preliminare | Valutazione del codice, piano commerciale, proposte UX e UI | - | **Fatto** | - |
| 0 | Fondamenta | Codice verde su .NET 10, CLI funzionante, segreti fuori dai file, CI eseguita, modello di provenienza e diritti | - | **Conclusa** (19/11/2026, tag `v0.1.1`) | Impostazioni manuali su GitHub (GHCR; protezione di `main` e dei tag rimandata, `docs/fase-0-chiusura-decisioni.md`, D1); **domande al legale entro il 15 dicembre 2026** (circa 7 h) |
| 1 | Estrazione: cruciverba e giochi brevi | Dalla riga di comando una Rivista PDF diventa Giochi salvati nelle tabelle dedicate, con accuratezza misurata e Fornitore AI scelto per misura | 0 | **In corso**: ondata 1.1 chiusa 20/11/2026; prova reale `1.1-prova` aperta | PDF di prova, correzione dei risultati attesi, regole delle tipologie da approvare, verifica delle condizioni d'uso dei Fornitori AI, scelta del fornitore predefinito, login OAuth di Claude Code sul PC, conferma delle soglie di accuratezza (circa 20 h) |
| 1b | Prototipo di revisione della creazione | Generazione (AI per i giochi a parole, algoritmo per sudoku e giochi logici) e schermata di revisione con cronometro per sei famiglie, per misurare G1 entro il 28 febbraio 2027 | 0 | In preparazione (ondata 1b.1) | Regole e brief, **prova 1** (30 candidati a cronometro, 5 per famiglia), decisione G1 (circa 19,5 h) |
| 2 | Fondamenta dell'interfaccia | Studio in container sul PC Windows del proprietario (Docker Desktop, solo rete di casa): accesso con passkey, involucro "Banco", Lettore di base | 1, 1b | Da iniziare | Certificato locale sui dispositivi di casa, passkey (circa 6 h) |
| 3 | Studio: acquisizione e revisione | Importare una Rivista e verificarne i Giochi da tastiera, con ricerca, schede e gioco | 2 | Da iniziare | Uso reale su almeno 2 numeri e segnalazione degli attriti (circa 10 h) |
| 4 | Crittografie, tipologie nuove e soluzioni | Estrarre crittografie e giochi a chiave; flusso "tipologia nuova, proposta, conferma o modifica"; soluzioni collegate | 3 | Da iniziare | Pagine di crittografie e di soluzioni, risultati attesi, revisione delle prime tipologie proposte (circa 11 h) |
| 5 | Rebus, involucro desktop e completamento dell'estrazione | Estrarre i rebus, i sudoku e i giochi logici, involucro desktop Tauri dello Studio, comandi batch accessori | 4 | Da iniziare | Pagine di rebus, sudoku e giochi logici, installazione dell'involucro (circa 9 h) |
| 6 | Creazione v1: cruciverba e giochi brevi | Generazione a lotti con validatori, revisione cronometrata, registro del contributo umano completo | 5, 1b | Da iniziare | Scelta del lessico, brief, conferma dei tempi di G1 sul motore completo (circa 14 h) |
| 7 | Creazione v2: crittografie, rebus e laboratorio dei format | Crittografie e rebus originali, nuovi format, dossier di paternità | 6 | Da iniziare | 2-3 idee di format, prova sulle crittografie, scelta del gioco del giorno (circa 13 h) |
| 8 | Sito pubblico e Admin | Sito accessibile con il gioco del giorno, calendario, newsletter, misurazione, blocco di pubblicazione | 7 e parere legale | Da iniziare | Marchio e dominio, parere legale, privacy, server, gruppo di prova over 65, 60 giochi in coda (circa 20 h più 15 h di contenuti) |
| 9 | Crescita commerciale | Esportazioni per editori e KDP, Lettore incorporabile, aspetto del sito modificabile, dashboard completa | 8 | Da iniziare | Priorità tra KDP e Lettore incorporabile, prove con il primo cliente (circa 9 h) |

### Stime per fase [S]

Regola usata: durata = ore del proprietario / ore disponibili per lo sviluppo nel periodo, arrotondata per eccesso, più una settimana di margine per i cicli di correzione degli agenti. Festività a ore ridotte. Le date partono da lunedì 16 novembre 2026. Le fasi 1 e 1b procedono in parallelo con questo ordine delle ondate [P, D1]: 1.1 chiusa il 20 nov 2026, poi 1b.1 dal 23 nov, 1.2, 1b.2, 1.3, e 1.4 accanto alla 1b.3. La 1.1 si è chiusa più di due settimane prima del 6 dic previsto: le ondate successive si anticipano di altrettanto, e il margine va a G1. Le scadenze esterne non cambiano: prototipo pronto verso il 10 gen 2027, scadenza il 17 gen, prova 1 dal 18 gen, G1 il 28 feb 2027. Così non lavorano mai più di 3 agenti insieme. Tra il 18 gennaio e il 21 febbraio 2027 circa 3 ore a settimana vanno alla prova 1, quindi la fase 2 avanza più lentamente.

| Fase | Ondate | Ore del proprietario | Periodo stimato | Rilascio |
|---|---|---|---|---|
| 0 | 3 | 7 | 16 nov - 6 dic 2026 | `v0.1.0`; chiusura `v0.1.1` |
| 1 | 4 | 20 | 23 nov 2026 - 21 feb 2027 (festività) | `v0.2.0` |
| 1b | 3 | 5,5 di costruzione + 14 di prova 1 | costruzione dal 23 nov 2026 al 17 gen 2027 (prototipo pronto verso il 10 gen); prova 1 18 gen - 21 feb 2027; **G1 il 28 feb 2027** | `v0.2.0-prova.1` (pre-release); correzioni `v0.2.0-prova.2`, `-prova.3` e così via fino a `v0.2.0`, poi `v0.2.1` e seguenti |
| 2 | 3 | 6 | 25 gen - 21 feb 2027 | `v0.3.0` |
| 3 | 4 | 10 | 22 feb - 14 mar 2027 | `v0.4.0` |
| 4 | 4 | 11 | 15 mar - 11 apr 2027 (Pasqua) | `v0.5.0` |
| 5 | 3 | 9 | 12 apr - 2 mag 2027 | `v0.6.0` |
| 6 | 5 | 14 | 3 - 30 mag 2027 | `v0.7.0` |
| 7 | 4 | 13 | 31 mag - 27 giu 2027 | `v0.8.0` |
| 8 | 5 | 20 (+15 di contenuti) | 28 giu - 8 ago 2027, agosto di margine; online il **1 set 2027** | `v1.0.0` |
| 9 | 4 | 9 | set - nov 2027 (prima ondata: esportazione KDP) | `v1.1.0`... |
| **Totale fino al lancio** | **38** | **circa 129,5** (di cui 14 di prova 1) | | |

Le date delle fasi 2-9 non sono ricalcolate: la chiusura della fase 1 al 21 feb 2027 le sposta. Da decidere con il proprietario.

---

## 1. Modo di lavorare

### 1.1 Agenti e modelli [P]

- Un **agente principale** orchestra. Per ogni ondata lancia 2-3 agenti in parallelo, ciascuno nel proprio ramo o worktree e con un compito che non tocca i file degli altri.
- Modelli: **Haiku** per la documentazione (README, guide, testi d'aiuto, ADR scritti da decisioni già prese); **Sonnet** per il codice; **Opus** per i ragionamenti complessi (progetto dei descrittori, metriche di accuratezza, prompt, sicurezza del generatore di migrazioni, algoritmi di riempimento); **Opus con sforzo massimo** per la revisione del codice a fine di ogni ondata.
- Le fasi sono **verticali e ampie**: ogni fase consegna qualcosa che il proprietario usa, non uno strato tecnico. Il limite vero è il tempo del proprietario, quindi ogni fase gli chiede **una sola sessione di decisione all'inizio e una di collaudo alla fine**, più i compiti indicati.

### 1.2 Ciclo di una fase

1. **Preparazione** (agente principale, Opus): `.planning/phases/<fase>/CONTEXT.md` e `PLAN.md` derivati da questa roadmap. Il proprietario li approva (decisioni della fase). Nessuna esecuzione se mancano `ROADMAP.md`, `CLAUDE.md`, `CONTEXT.md` o `PLAN.md`.
2. **Ondate**: ogni ondata si chiude con la CI verde e la revisione di Opus (sforzo massimo). Le osservazioni CRITICAL e HIGH vanno corrette prima dell'ondata successiva. Ogni ondata aggiunge a `PROGRESS.md` una riga `PHASE:<n> WAVE:<n.m> STATUS:<complete|failed> TS:<ISO-8601 UTC>`. Alla ripresa non si rieseguono le ondate già complete.
3. **Verifica**: un agente Opus controlla i criteri di accettazione partendo dall'obiettivo, esegue i comandi elencati e scrive l'esito.
4. **Collaudo e chiusura**: il proprietario prova il risultato, approva la fusione in `main` e il tag di rilascio. Senza questa approvazione la fase non si chiude.

**Rami e versioni** [P, D6]: un ramo per ondata, con una pull request verso `main`. Il prototipo della 1b esce come pre-release `v0.2.0-prova.1`, senza spostare `latest`. La fase 1 si chiude con `v0.2.0`. Le correzioni della 1b.3 escono come `v0.2.0-prova.2`, `-prova.3` e così via, senza spostare `latest`, fino a `v0.2.0`; poi come `v0.2.1` e seguenti (D1b-8, che chiude la domanda q8 della fase 1).

### 1.3 CI e pubblicazione [P]

- Test e pubblicazione **solo tramite GitHub Actions** sul repository `github.com/F1-Solution/DBEnigmistica`. Eccezione dichiarata, limitata alla regressione sul Set di riferimento vero: è uno script locale che il proprietario lancia prima di ogni release, con il rapporto allegato alla release (ADR 008). La protezione del ramo `main` (CI verde e approvazione del proprietario) è rimandata: il piano gratuito dei repository privati non la consente (`docs/fase-0-chiusura-decisioni.md`, D1, 13/11/2026).
- Base già predisposta (vedi `docs/CI.md`): `ci.yml` eseguito per la prima volta e verde su `main` il 10/11/2026; `publish.yml` parte con il tag `v0.1.0`. `ci.yml` gira su push a `main`, pull request e chiamata da `publish`: build e test con copertura, vulnerabilità dei pacchetti, gitleaks. `publish.yml` gira su tag `vX.Y.Z` o avvio manuale: riesegue la CI, pubblica `ghcr.io/f1-solution/dbenigmistica` con i tag `X.Y.Z`, `sha-<commit>` e `latest`, e crea la GitHub Release con gli eseguibili. Dependabot è configurato. GHCR resta privato. Una pre-release (`vX.Y.Z-prova.N`) non sposta `latest`.
- Una sola immagine per tutto [A]: profilo web scelto con `DBE_PROFILO=studio|pubblico`, riga di comando con `docker run ... dbe <comando>`. Così c'è un solo artefatto da tenere aggiornato. Gli artefatti in più (involucro desktop, pacchetto del Lettore) si aggiungono a `publish.yml` come lavori a sé, seguendo `docs/CI.md`.
- I controlli si accumulano: ogni fase dice solo quelli che **aggiunge**.

---

## 2. Architettura di riferimento

### 2.1 Superfici, profili e dati

```
           PC Windows del proprietario (rete di casa)                Internet
 ┌─────────────────────────────────────────────────────┐   ┌───────────────────────────────┐
 │ Involucro desktop / browser                          │   │ Browser dei giocatori         │
 │        │                                             │   │        │                      │
 │  [Profilo "studio"] Studio + API + lavori in coda    │   │  [Profilo "pubblico"]          │
 │        │            + riga di comando dbe            │   │   Sito + Admin + API pubblica │
 │  PostgreSQL "dbe_studio"                             │   │  PostgreSQL "dbe_pubblico"     │
 │   schema riviste    (archivio privato, estrazione)   │   │   solo edizioni pubblicate,    │
 │   schema catalogo   (Giochi originali, creazione)    │──▶│   calendario, eventi d'uso,    │
 │   schema tipologie  (descrittori, migrazioni)        │   │   aspetto del sito             │
 │   schema registro   (sola aggiunta)                  │   │   CHECK: origine originale     │
 │  Archivio: PDF, immagini delle pagine, ritagli       │   └───────────────────────────────┘
 └─────────────────────────────────────────────────────┘
       Consegna firmata Studio → Pubblico: solo Giochi approvati che superano la policy di pubblicazione
```

- **Due profili, un codice** [P]. Il profilo pubblico non ha la stringa di connessione dello Studio: se la trova, all'avvio si ferma con un errore. L'Admin gira nel profilo pubblico, con lo stesso involucro e la stessa libreria di componenti dello Studio [A]: deve operare sul database pubblico, e lo Studio resta comunque raggiungibile solo dalla rete di casa [P].
- **Archivi separati** [P]. Schemi distinti con un ruolo PostgreSQL per contesto [A]: `dbe_estrazione` (scrive in `riviste`), `dbe_creazione` (scrive in `catalogo` e **non può leggere** `riviste`), `dbe_originalita` (legge entrambi e restituisce solo punteggi di somiglianza), `dbe_consegna` (legge `catalogo`). L'interruttore che permette di usare i giochi estratti come esempi è anche un permesso del database, non solo una riga di configurazione.
- **Contesti delimitati** [A]: *Acquisizione* (Riviste, Pagine, Giochi estratti), *Catalogo e creazione* (Giochi originali, brief, revisioni), *Tipologie* (descrittori e versioni), *Registro* (provenienza, diritti, contributo umano), *Pubblicazione* (edizioni, calendario, uso). I contesti comunicano solo tramite i casi d'uso dell'Application layer, mai leggendo direttamente le tabelle di un altro contesto.
- **Dipendenze** [A]: Domain non dipende da niente. Application dipende solo da Domain. Gli adattatori (EF Core e Npgsql, Fornitore AI, PDF, file, web, CLI) dipendono verso l'interno. La regola è verificata dai test di architettura dalla fase 0.

### 2.2 Il descrittore di tipologia [P, struttura A]

Ogni tipologia è un file versionato (`tipologie/<id>/v<n>.json`, nel repository per le tipologie di base e nel database per quelle confermate dallo Studio). Da qui derivano tre cose: la **migrazione SQL** delle tabelle dedicate, lo **JSON Schema** dell'output vincolato dell'AI e la **resa** nel Lettore. Schema minimo:

```json
{
  "id": "cruciverba-classico", "versione": 1, "famiglia": "cruciverba",
  "regole": "Testo delle regole del gioco per l'AI e per la guida del Sito",
  "campi": [
    { "nome": "righe", "tipo": "intero", "obbligatorio": true, "ruolo": "dimensione" },
    { "nome": "griglia", "tipo": "griglia", "obbligatorio": true, "ruolo": "griglia" }
  ],
  "elenchi": [
    { "nome": "definizioni", "campi": [
      { "nome": "numero", "tipo": "intero", "ruolo": "numero" },
      { "nome": "direzione", "tipo": "enumerazione", "valori": ["orizzontale", "verticale"], "ruolo": "direzione" },
      { "nome": "testo", "tipo": "testo", "max": 300, "ruolo": "definizione" },
      { "nome": "soluzione", "tipo": "testo", "max": 40, "ruolo": "soluzione" } ] }
  ],
  "resa": { "modello": "griglia" },
  "validatori": ["numerazione-coerente-con-griglia", "lunghezze-coerenti"]
}
```

- **Regole approvate** [P]: il campo `regole` è scritto in bozza da un agente e **approvato dal proprietario**; l'approvazione si registra con data e versione. È l'unica conoscenza che il Motore di creazione riceve sulle tipologie finché l'interruttore degli esempi è spento.
- **Tipi ammessi (lista chiusa)**: `testo` (con `max`), `testo_lungo`, `intero`, `decimale`, `booleano`, `data`, `enumerazione`, `griglia`, `immagine` (riferimento a un ritaglio), più gli `elenchi`, che diventano tabelle figlie. Non esistono `DEFAULT` liberi, gli identificatori sono validati con un'espressione regolare e poi quotati. Il generatore non accetta stringhe SQL: anche i tipi di dato SQL e gli indici si scelgono solo da un elenco fisso (regola fissata nell'ondata 0.2).
- **Tabelle**: `giochi` (generale: pagina, tipologia, versione, titolo, riquadro, confidenza, origine) più `<schema>.gioco_<tipologia>` 1:1 e le tabelle figlie. Le stesse strutture sono generate sia in `riviste` sia in `catalogo`.
- **Modelli di resa del Lettore**: `griglia` (cruciverba, cruciverba crittografato), `esposto_diagramma` (crittografie, giochi brevi), `immagine_diagramma` (rebus), `griglia_logica` (sudoku e giochi logici: valori da un insieme chiuso, indizi su caselle, bordi o margini, regioni) [A: i giochi logici non stanno nel modello del cruciverba, che ha caselle nere e definizioni]. Un format nuovo riusa uno di questi modelli e cambia solo i dati.
- **Migrazioni**: per le tipologie di base il generatore le produce in fase di sviluppo e le registra nel repository; entrano tramite pull request. Per quelle confermate nello Studio vengono salvate come *in attesa* e si applicano solo con un comando esplicito: azione "Applica migrazione" con l'SQL mostrato, oppure `dbe types apply <id>`. Nessun DDL viene eseguito al volo [P].

### 2.3 Lettura delle pagine [P, dettagli A]

1. Rendering della pagina con PDFiumCore e la libreria nativa `bblanchon.PDFium` a una risoluzione configurabile (150-200 DPI); il PNG lo codifica il progetto (ADR 009, che sostituisce il punto 1 dell'ADR 003). Testo nativo con PdfPig quando esiste. Le impronte SHA-256 di PDF e pagina servono all'idempotenza. L'Archivio (`Archivio:Cartella`) conserva la copia della Rivista con nome uguale alla sua impronta, le immagini delle pagine in cartelle per impronta e il testo nativo nel database; la cartella va nel backup, e lo spazio delle Riviste è occupato due volte (`docs/fase-1-decisioni.md`, D3 e D4).
2. **Una chiamata per pagina** al Fornitore AI configurato (sezione 2.4; riferimento iniziale: Claude tramite l'abbonamento, modello configurabile) con: immagine, testo nativo, catalogo dei descrittori e uno JSON Schema per l'output strutturato, verificato anche lato programma perché non tutti i fornitori lo impongono. La risposta contiene l'elenco dei Giochi: tipologia nota oppure "nuova tipologia proposta", titolo, riquadro normalizzato tra 0 e 1, dati conformi al descrittore, confidenza per campo e avvisi.
3. I validatori strutturali del descrittore (per esempio la numerazione coerente con la griglia) abbassano la confidenza dei Giochi incoerenti. Una seconda chiamata sul ritaglio ingrandito del solo Gioco che non passa la validazione resta **disattivata**: si accende solo se il Set di riferimento dimostra che migliora l'accuratezza [A].
4. Le cartelle passano da una coda che rispetta i limiti del fornitore: quando la finestra dell'abbonamento o la quota del piano gratuito si esaurisce, la coda si ferma e riprende da sola. Ogni chiamata registra nel registro fornitore, modello, durata, esito, token e costo quando il fornitore li riporta, più lo SHA-256 della risposta. La risposta grezza va nella tabella `riviste.chiamate_ai`, che si può svuotare (ADR 011). Un limite di pagine per esecuzione interrompe il lavoro; il tetto di spesa vale solo con l'adattatore API.
5. Il modello più capace del fornitore di riferimento (oggi Opus) solo per progettare descrittori di tipologie nuove: sono chiamate rare e conta la qualità.

### 2.4 Il Fornitore AI [P, dettagli A]

- **Due porte** nell'Application layer, una per l'estrazione (pagina → Giochi) e una per la creazione (brief e regole → candidato), confermate il 20/11/2026 (`docs/fase-1b-decisioni.md`). Gli adattatori stanno in Infrastructure e si scelgono da configurazione, così cambiare fornitore non tocca il resto.
- **Adattatori previsti.** *Claude Code non interattivo* con l'abbonamento del proprietario: predefinito per la creazione e riferimento per l'estrazione, utilizzabile **solo nello Studio sul PC del proprietario**. *Piani gratuiti di altri fornitori* e *modello aperto in locale*: candidati per l'estrazione, confrontati sul Set di riferimento. *API a consumo* (SDK ufficiale, prompt caching, Batch API, tetto di spesa): possibile ma non predefinito; si attiva solo se un ostacolo dei limiti viene misurato e il proprietario accetta la spesa.
- **Regola di scelta per l'estrazione** [P]: diventa predefinito il fornitore che supera le soglie di accuratezza al costo minore, tra quelli le cui condizioni d'uso il proprietario ha verificato. I piani gratuiti ricevono pagine delle Riviste solo dopo questa verifica [A: è la posizione meno prudente, ed è tra le domande al legale].
- **Limiti condivisi.** L'abbonamento serve anche allo sviluppo del progetto: estrazione e creazione si pianificano in orari diversi dal lavoro degli agenti, e la fase 1 misura quante pagine si leggono per finestra.
- **Sito pubblico.** Il profilo pubblico non registra alcun adattatore del Fornitore AI e non dipende dall'abbonamento: serve solo edizioni già approvate nello Studio.
- **Esecuzione in fase 1** [P, ADR 010]: l'estrazione dal vivo gira sul PC Windows del proprietario con l'eseguibile `dbe` win-x64 pubblicato dalla CI e con Claude Code installato sul PC. Claude Code usa il login OAuth con l'abbonamento (`claude`, poi `/login`). Non si usa mai una chiave API: nessuna `ANTHROPIC_API_KEY` resta nell'ambiente, perché l'adattatore la rimuove dal processo figlio. `dbe check-ai` segnala se manca il login. `CLAUDE_CODE_OAUTH_TOKEN` (da `claude setup-token`) serve solo in un container, quindi in fase 1 non si usa.
- **Container, rimandato alla fase 2** [P, ADR 010]: la prova di Claude Code nel container diventa una nota scritta. La scelta dipende dalla verifica delle condizioni d'uso dell'abbonamento (domanda aperta 13).

---

## 3. Le fasi

### Fase 0 - Fondamenta

**Obiettivo.** Un codice che compila senza avvisi bloccanti, ha i test verdi su PostgreSQL vero e una CLI funzionante. I segreti stanno fuori dai file, la CI è attiva e il dominio contiene già il modello di provenienza, diritti e registro, su cui si appoggia tutto il resto.

**Perché qui.** All'avvio della fase nessun comando funzionava (`ScanFileCommand.cs:50`), il repository non aveva commit e nessun test toccava un database vero. Costruire sopra senza CI significherebbe ripetere l'errore del vecchio piano, che dichiarava "completate" funzioni mai eseguite. Il modello di provenienza va per primo perché ogni Gioco, estratto o creato, ne ha bisogno dal primo salvataggio [P].

**Stato al 19/11/2026.**

| Voce | Stato |
|---|---|
| Passaggio a `net10.0` (con EF Core e Npgsql 10) | **Fatto** |
| Password di sviluppo fuori dai file tracciati (`docker-compose.yml` legge `.env`, `EnigmisticaDbContextFactory.cs` legge l'ambiente) | **Fatto** |
| Primo commit e push; `ci.yml` eseguito su GitHub e verde su `main` (Build e test, Vulnerabilita pacchetti, Controllo segreti) | **Fatto** |
| Ondata 0.1-A, unita in `main`: eliminati OCR, analisi del layout, cache, creazione di tabelle a runtime e servizi segnaposto delle soluzioni; pacchetti centralizzati in `Directory.Packages.props`; dipendenze vulnerabili `Cryptography.Xml` e `Pkcs` 9.0.0 risolte; `appsettings.json` copiato dal csproj; healthcheck del compose corretto; esportazione CSV con neutralizzazione delle formule | **Fatto** |
| Ondate 0.1-B e 0.1-C: riga di comando su System.CommandLine 2.0.15 e Generic Host con eseguibile `dbe`, senza `Environment.Exit`, interfaccia di console fuori da Application; 35 test d'integrazione su PostgreSQL 17 con Testcontainers, database in memoria eliminato; lavoro di CI che costruisce l'immagine Docker; 121 test superati in locale, più quelli aggiunti dopo la revisione (124 con questi) | **Fatto**, unito in `main` |
| Pacchetto non ufficiale `Anthropic.SDK` eliminato; validazione della configurazione del Fornitore AI al posto della chiave API | **Fatto** (ondata 0.2) |
| Ondata 0.2: provenienza e diritti, registro in sola aggiunta, schemi e ruoli, baseline delle migrazioni, hook gitleaks, estensioni della CI | **Fatto** |
| Ondata 0.3: `CLAUDE.md`, ADR, README e SETUP riscritti (compresa la sezione superata sulle password di `SETUP_INSTRUCTIONS.md`), test end-to-end, tag `v0.1.0` | **Fatto** |
| Chiusura: decisioni del 13/11/2026 (`docs/fase-0-chiusura-decisioni.md`): codice 5 per il registro alterato, PostgreSQL 18, tag `v0.1.1` | **Fatto** (19/11/2026, approvata dal proprietario; immagine `0.1.1` verificata) |
| Impostazioni manuali su GitHub (`docs/CI.md`): la protezione di `main` e dei tag è rimandata, perché il piano gratuito dei repository privati non la consente (`docs/fase-0-chiusura-decisioni.md`, D1, 13/11/2026); domande al legale | Da fare (proprietario) |

Il numero di avvisi della build era 52 prima dell'ondata 0.1. Misurato l'11/11/2026: 0 avvisi (`.planning/phases/0/CONTEXT.md`). La CI compila con `-warnaserror`, come chiede il criterio 1 (verificato in `ci.yml`).

**Riuso, riscrittura, eliminazione.**

| Elemento | Destino | Riferimento |
|---|---|---|
| Struttura a 5 progetti e direzione delle dipendenze | Tenere; aggiungere `DBEnigmistica.Web` (host ASP.NET Core, in fase 2) e `DBEnigmistica.Tipologie` (descrittori e generatore, in fase 1) | `DBEnigmistica.slnx` |
| Repository | Tenere, con semantica unica di `Add` e `SaveChanges` (causa del bug dei metadati non salvati) | `src/DBEnigmistica.Infrastructure/Repositories/*`; `DynamicSchemaOrchestrator.cs:115`, `:238` |
| Entità `Magazine`, `Page`, `Game`, `GameSolution` | Rifattorizzare: tipologia come dato invece dell'enum `GameType`, origine e diritti, riquadro normalizzato | `src/DBEnigmistica.Domain/Entities/`, `Enums/GameType.cs` |
| Migrazioni EF del 31/01/2026 | Rigenerare una baseline nuova: non sono mai state applicate | `src/DBEnigmistica.Infrastructure/Migrations/20260131*` |
| Cablaggio della CLI | Riscrivere su System.CommandLine 2.0 stabile e Generic Host; servizi risolti dal contenitore | `src/DBEnigmistica.Console/Program.cs:100-104` |
| `Environment.Exit` negli handler (23 occorrenze) | Eliminare: codici di uscita restituiti dagli handler | es. `ScanFileCommand.cs:85` |
| Configurazione | Options pattern con `ValidateOnStart`, user-secrets davvero collegati, `appsettings.json` copiato in output, cartella base = cartella dell'eseguibile | `Program.cs:13-18`, `:14`; `appsettings.json:14`, `:18`; `Infrastructure/Configuration/DependencyInjection.cs:44` |
| Chiave API vuota accettata; pacchetto non ufficiale `Anthropic.SDK` | Validare la configurazione del Fornitore AI, non la chiave: errore chiaro all'avvio dei comandi che usano l'AI se il fornitore configurato non è utilizzabile; nessuna chiave richiesta con il fornitore predefinito. Eliminare `Anthropic.SDK`: l'accesso all'AI passa dall'interfaccia della sezione 2.4 (fase 1) | `ClaudeApiClient.cs:26-27`; `Console/Configuration/AnthropicOptions.cs`; `Directory.Packages.props` |
| UI di console dentro Application | Spostare in Console (Spectre.Console) | `DynamicSchemaOrchestrator.cs:51`, `SchemaValidationService.cs`, `Services/UI/ConsoleProgressReporter.cs` |
| DDL a runtime (SQL injection) | **Eliminare subito il percorso**: `analyze-game` resta disattivato fino alla fase 4 | `DynamicMigrationService.cs:82`, `:104`, `:157`, `:186` |
| `TesseractOcrService`, cache OCR, `BasicLayoutAnalysisService`, SkiaSharp, Tesseract | **Eliminare** (sostituiti dalla visione in fase 1) | `TesseractOcrService.cs:26`, `:126-145`; `BasicLayoutAnalysisService.cs:62`, `:75-76`, `:146`; `Services/Ocr/CachedOcrService.cs`, `Services/Cache/OcrCacheService.cs` |
| Stub delle soluzioni | Eliminare i servizi; tenere l'entità e il collegamento manuale | `SolutionExtractionService.cs:41-44`, `SolutionMatchingService.cs:36-38` |
| Test | Riscrivere: `DynamicMigrationServiceTests` eliminato insieme al codice; `SchemaValidationServiceTests` riallineato (messaggi in italiano, controllo delle parole riservate); `RepositoryIntegrationTests` spostato su Testcontainers PostgreSQL | `tests/.../DynamicMigrationServiceTests.cs:18`; `SchemaValidationService.cs:72-118`; `RepositoryIntegrationTests.cs:19` |
| Export JSON/CSV | Tenere (in fase 5 legge le tabelle per tipologia) | `DataExportService.cs:196` |
| `docker-compose.yml` | Tenere (già parametrizzato); healthcheck con `$${POSTGRES_USER}` invece del nome fisso; servizio applicativo in fase 2 | `docker-compose.yml:16` |
| `ci.yml`, `publish.yml`, `Dependabot`, `Dockerfile`, `docs/CI.md` | Tenere ed eseguire; togliere da `Dockerfile` e `publish.yml` la copia manuale di `appsettings.json` quando il csproj la fa da solo; togliere `Ocr__TesseractDataPath` dagli esempi | `Dockerfile`, `.github/workflows/publish.yml`, `docs/CI.md` |
| README, SETUP_INSTRUCTIONS, `docs/plan.md` | Riscrivere README e SETUP dallo stato reale, compresa la sezione superata sulle password; segnare `docs/plan.md` come superato | `SETUP_INSTRUCTIONS.md:82-83`, `:142` e seguenti |

**Risultati consegnati.**
- Soluzione su .NET 10 con gestione centralizzata dei pacchetti (`Directory.Packages.props`), pacchetti aggiornati, nessuna vulnerabilità nota.
- Eseguibile `dbe` su System.CommandLine 2.0 con i comandi esistenti funzionanti dove il codice lo consente (`list-games`, `migrate`, `export-*`, `link-solution`). Gli altri rispondono "disponibile dalla fase N" con codice d'uscita 2.
- Modello di provenienza e diritti nel dominio: `Origine` (originale, licenza, pubblico dominio, rivista) **senza valore predefinito**; `Diritti` con titolare, licenze e cessioni (cliente, periodo, esclusiva, territorio); `StatoInedito`; `PolicyDiPubblicazione` come funzione pura del dominio.
- Registro in sola aggiunta: tabella `registro.eventi` con impronta concatenata (ogni riga contiene lo SHA-256 della precedente) e trigger che rifiutano `UPDATE` e `DELETE`. Comando `dbe verify-integrity`.
- Schemi `riviste`, `catalogo`, `tipologie`, `registro` e ruoli PostgreSQL per contesto, creati dalle migrazioni.
- `CLAUDE.md` del progetto (convenzioni per gli agenti), ADR 001-006 (sezione 2), README e SETUP riscritti.

**Criteri di accettazione.**
1. `dotnet build DBEnigmistica.slnx -c Release -warnaserror` termina con codice 0. `dotnet list package --vulnerable --include-transitive` non riporta nulla (nessuna traccia di `System.Security.Cryptography.Xml` né `Pkcs` 9.0.0). Il lavoro "Vulnerabilita pacchetti" è verde su GitHub.
2. `dotnet publish src/DBEnigmistica.Console -c Release -o out` produce `out/appsettings.json` senza copie manuali; `grep -n "cp -n" Dockerfile .github/workflows/publish.yml` non trova nulla.
3. `docker compose up -d postgres` con un `POSTGRES_USER` diverso da `dbenigmistica` arriva allo stato `healthy`. `SETUP_INSTRUCTIONS.md` non contiene più la sezione "Modifica Password" né password d'esempio nelle stringhe di connessione.
4. `dotnet test` dà 0 falliti e 0 saltati senza motivo. Almeno 10 test della categoria `Integration` girano contro PostgreSQL 18 tramite Testcontainers e nessun test usa `UseInMemoryDatabase`.
5. `dbe --help` elenca i comandi. `dbe list-games` contro un database vuoto termina con codice 0 e senza `Service provider not available` (test end-to-end della CLI in CI).
6. `grep -rn "Environment.Exit" src/` e `grep -rniE "tesseract|skiasharp|Anthropic\.SDK" src/ Directory.Packages.props` non trovano nulla. Nella fase 0 non entra alcun pacchetto di un fornitore AI: gli adattatori arrivano con la fase 1, e il test di architettura del criterio 8 li confina in Infrastructure.
7. gitleaks sull'intero albero non trova nulla. Un test verifica che un comando che usa l'AI, senza Fornitore AI configurato o con un fornitore incompleto (per Claude Code: eseguibile non trovato o non autenticato; per l'adattatore API, se scelto: chiave vuota), termina con un messaggio esplicito e codice diverso da 0, prima di qualunque chiamata di rete. Un secondo test verifica che i comandi senza AI partono senza alcuna chiave.
8. Test di architettura (NetArchTest): Domain non referenzia altri progetti; Application non referenzia EF Core, Npgsql, Spectre, System.CommandLine né l'SDK AI.
9. Test d'integrazione: salvare un Gioco senza origine fallisce (`NOT NULL`, nessun default); due cessioni esclusive sovrapposte sullo stesso Gioco falliscono (vincolo di esclusione); `UPDATE` e `DELETE` su `registro.eventi` falliscono; dopo un'alterazione fatta da superutente nel test, `dbe verify-integrity` termina con codice diverso da 0 (codice 5).
10. Test a tabella della `PolicyDiPubblicazione` con il 100% dei rami coperto: un Gioco con origine `rivista` non è mai pubblicabile, qualunque sia il resto.
11. Copertura delle righe di Domain e Application di almeno l'80%.
12. I quattro lavori di `ci.yml` (Build e test, Vulnerabilita pacchetti, Controllo segreti, Build immagine Docker) sono verdi su GitHub su ogni commit di `main`. Il tag `v0.1.1` fa pubblicare a `publish.yml` `ghcr.io/f1-solution/dbenigmistica:0.1.1`, e `docker run <immagine> --version` stampa `0.1.1`. Un tag su un commit che non discende da `main` fa fallire `publish.yml` prima di pubblicare. La protezione di `main` con controlli obbligatori è rimandata a quando il piano GitHub la consente. Immagine `0.1.1` verificata dal proprietario il 19/11/2026.
13. Test d'integrazione: tutte le colonne di data e ora sono `timestamptz` e i valori salvati sono in UTC; la cancellazione di una pagina usata come pagina-soluzione da un Gioco di un'altra Rivista segue il comportamento deciso nell'ondata 0.2 (oggi il database la rifiuta).

**Requisiti coperti.** OBJ-02 (in parte), OBJ-05, OBJ-07, APP-01, DB-01, DB-02, DB-03, DB-04, DB-06, DB-07, DB-08, DB-10, CLI-01, TEC-01, TEC-02, TEC-03, TEC-07, TEC-11, TEC-12 (CLI), OUT-03, OUT-07, OUT-08, OUT-09, OUT-10, OUT-11, OUT-15, OUT-16. Piano commerciale: RQ-01, RQ-02 (campi e vincolo d'esclusiva), RQ-03 (campo), RQ-04 (policy e test), RQ-05 (separazione), RQ-12 (infrastruttura del registro).

**CI aggiunta e artefatti.** `ci.yml` eseguito per la prima volta e verde su `main` il 10/11/2026; l'ondata 0.1-C aggiunge il lavoro che costruisce l'immagine Docker; `publish.yml` si esegue per la prima volta con il tag `v0.1.0`, con il controllo che il commit del tag discenda da `main`. Si aggiungono a `ci.yml`: build con `-warnaserror`, test d'integrazione con Testcontainers, soglia di copertura, `dotnet format --verify-no-changes`, test di architettura, test end-to-end della CLI. Gitleaks anche come hook pre-commit locale. Artefatti di `publish.yml`: immagine `ghcr.io/f1-solution/dbenigmistica` (per ora solo CLI), eseguibili `win-x64` e `linux-x64`, rapporto di copertura.

**Ondate.**

| Ondata | Agenti | Contenuto |
|---|---|---|
| 0.1 (**conclusa**, unita in `main`) | Sonnet A, Sonnet B, Sonnet C | A: pacchetti centralizzati e aggiornati, eliminazione delle dipendenze vulnerabili `Cryptography.Xml` e `Pkcs` 9.0.0, eliminazione di Tesseract, layout, cache e stub delle soluzioni, disattivazione del DDL a runtime, `appsettings.json` copiato da csproj. B: CLI su System.CommandLine 2.0 e Generic Host, Options pattern, user-secrets, via `Environment.Exit`, Spectre.Console, UI fuori da Application. C: Testcontainers, riscrittura dei 15 test rossi, healthcheck del compose |
| 0.2 (**conclusa**) | Opus A (progetto), poi Sonnet A; Sonnet B; Sonnet C | A: modello di provenienza, diritti, inedito, policy, registro in sola aggiunta, schemi e ruoli, baseline delle migrazioni; date sempre in UTC; decisione sulla cancellazione di una pagina usata come pagina-soluzione da un Gioco di un'altra Rivista (oggi il database la rifiuta); regola per il futuro generatore di migrazioni per tipologia: tipi di dato e indici solo da un elenco fisso. B: gitleaks pre-commit, validazione della configurazione del Fornitore AI al posto della chiave API, eliminazione di `Anthropic.SDK`, `.env.template` ripulito da Tesseract. C: estensioni di `ci.yml` (test di architettura, soglia di copertura, formato), pulizia di `Dockerfile` e `publish.yml`, protezione della pubblicazione da tag su commit non discendenti da `main`, verifica dell'immagine Docker senza librerie native (vedi sotto) |
| 0.3 (**conclusa**) | Haiku A, Sonnet B | A: `CLAUDE.md`, ADR 001-006, README e SETUP riscritti (via la sezione sulle password), `docs/CI.md` aggiornato, avviso "superato" su `docs/plan.md`. B: test end-to-end della CLI in CI e `dbe verify-integrity` |
| Revisione | Opus (sforzo massimo) | Diff completo della fase, con attenzione a segreti, vincoli del registro e direzione delle dipendenze |

**Immagine Docker e librerie native.** Il rilievo decade per Tesseract e SkiaSharp: sono stati eliminati nell'ondata 0.1-A e il `Dockerfile` usa già l'immagine `runtime:10.0-noble-chiseled`, senza pacchetti di sistema. Nell'ondata 0.2 il `Dockerfile` ha ricevuto il controllo che rifiuta le librerie native. Il tema torna nella fase 1 per due motivi: PDFium (PDFiumCore con `bblanchon.PDFium`, ADR 009) ha una libreria nativa propria, da provare sull'immagine chiseled anche con PDF a font non incorporati; la scelta del container per Claude Code è rimandata alla fase 2 (sezione 2.4, ADR 010).

**Rischi e segnali precoci.**
- *Il primo commit include un segreto.* **Superato**: primo commit fatto con il controllo dei segreti verde.
- *Migrazione a System.CommandLine 2.0 più lunga del previsto.* **Superato**: migrazione fatta nell'ondata 0.1-B.
- *Le ondate 0.1-B e 0.1-C restano fuori da `main` mentre parte la 0.2.* **Superato**: 0.1-B e 0.1-C sono unite in `main` prima della 0.2.

**Dal proprietario (circa 7 h).** Fatti: repository privato e approvazione del primo commit; ADR 001-006 approvati il 13/11/2026; chiusura della fase 0 approvata il 19/11/2026, con immagine `0.1.1` verificata. Restano: impostazioni manuali di `docs/CI.md` (permessi dei flussi, GHCR privato, Dependabot; protezione di `main` e dei tag rimandata, `docs/fase-0-chiusura-decisioni.md`, D1).
**Parere legale, azione con scadenza.** Scegliere il legale e **inviare le domande scritte entro il 15 dicembre 2026**: quelle della sezione 12 del piano commerciale, più l'estrazione privata dalle Riviste acquistate, la conferma che lo Studio sul PC di casa è una collocazione adeguata, l'invio delle pagine delle Riviste a un Fornitore AI esterno (Claude con l'abbonamento, piani gratuiti di altri fornitori che possono usare i dati per addestramento), la tutela di sudoku e giochi logici generati da un algoritmo e l'uso dei Giochi estratti come esempi per la creazione. **Risposta attesa entro il 28 febbraio 2027**, la data della prova 0 del piano commerciale. Il parere serve a decidere l'interruttore degli esempi (fase 6), le immagini dei rebus (fase 7) e il lancio del Sito (fase 8). Se non arriva entro il 30 aprile 2027, il lancio resta fermo.

---

### Fase 1 - Motore di estrazione: cruciverba e giochi brevi

**Obiettivo.** Dalla riga di comando, un PDF o una cartella di Riviste diventano Giochi salvati nelle tabelle dedicate del cruciverba classico e dei giochi brevi (anagramma, sciarada, cambio di lettera). L'accuratezza è misurata su un Set di riferimento.

**Stato al 20/11/2026.** L'ondata 1.1 è conclusa con l'approvazione del proprietario, ma senza la prova reale di Claude Code. È unita in `main` (PR #31, commit 716134e) con CI verde. Sono consegnati il rendering delle pagine con PDFium, l'Archivio delle Riviste, l'adattatore Claude Code con Registrazioni e limite di chiamate, i meta-schemi delle Tipologie e `check-ai` con il controllo del login OAuth.

**Aperto.** Il task `1.1-prova` (`.planning/phases/1/prova-claude-code-1.1.md`, in attesa) richiede prima la verifica delle condizioni d'uso dell'abbonamento da parte del proprietario, entro il 06/12/2026 (domanda 13). Finché `PROGRESS.md` non contiene `PHASE:1 WAVE:1.1-prova STATUS:complete`, non si fanno estrazioni né generazioni dal vivo. La prossima ondata è la 1b.1.

**Perché qui.** È il primo componente scelto dal proprietario [P] e quello dove la valutazione ha trovato il guasto più grave: pagine bianche, OCR mai inizializzato, dati scartati. La riga di comando basta a dimostrare la pipeline prima di investire nell'interfaccia. Cruciverba e giochi brevi danno subito un risultato usabile e coprono i due modelli di resa più frequenti.

**Riuso, riscrittura, eliminazione.**

| Elemento | Destino | Riferimento |
|---|---|---|
| `PdfService` | Riscrivere: rendering reale con PDFiumCore e `bblanchon.PDFium` (ADR 009), testo nativo con PdfPig | `PdfService.cs:102-111` |
| `ClaudeApiClient` | Riscrivere dietro l'interfaccia unica del Fornitore AI (sezione 2.4): adattatore predefinito Claude Code non interattivo con l'abbonamento (immagini, output strutturato verificato, retry, gestione della finestra esaurita); adattatori per i piani gratuiti scelti e per un modello aperto locale. L'adattatore API (SDK ufficiale, prompt caching, Batch API) resta possibile, non predefinito | `ClaudeApiClient.cs:29`, `:50-53`, `:124-144` |
| `PromptTemplates`, `ClaudeGameAnalyzer` | Riscrivere: prompt costruiti dai descrittori, niente campi cablati | `PromptTemplates.cs:18`; `ClaudeGameAnalyzer.cs:26`, `:176-214` |
| `MagazineProcessingService` | Rifattorizzare: tenere il ciclo per pagina e gli errori per pagina; aggiungere idempotenza, ripresa, persistenza dei dati, testo nativo nel database, immagini delle pagine nell'Archivio, cartelle ricorsive su richiesta | `MagazineProcessingService.cs:180-187`, `:213-214`, `:279` |
| `GameExtractionService` | Riscrivere sul nuovo contratto: niente più tipi scartati né dati persi | `GameExtractionService.cs:44-45`, `:76-90` |
| Entità `CrosswordGame`, `AnagramGame` | Sostituire con i descrittori `cruciverba-classico`, `anagramma`, `sciarada`, `cambio-di-lettera` | `Domain/Entities/CrosswordGame.cs`, `AnagramGame.cs` |
| Entità `LogicGame` | Eliminare: i giochi logici non sono tra le tipologie di partenza dell'estrazione; tornano come descrittori (sudoku e giochi logici nel catalogo dalla fase 1b, estrazione in fase 5) | `Domain/Entities/LogicGame.cs` |

**Risultati consegnati.**
- Progetto `DBEnigmistica.Tipologie`: meta-schema dei descrittori, descrittori v1 delle 4 tipologie, generatore di SQL (lista chiusa di tipi, identificatori quotati) e derivazione dello JSON Schema.
- `dbe types generate` (rigenera i file SQL; in CI verifica che coincidano con quelli registrati) e `dbe migrate` (applica le migrazioni EF del nucleo e quelle delle tipologie approvate, con checksum).
- Scrittore e lettore parametrizzati delle tabelle per tipologia: Npgsql con parametri, identificatori presi solo dal descrittore validato.
- Pipeline per pagina: rendering, testo nativo, chiamata al Fornitore AI, validatori strutturali, persistenza in una transazione per pagina. Una riscansione salta le pagine già elaborate (impronta SHA-256).
- **Archivio** (`Archivio:Cartella`): copia della Rivista con nome uguale alla sua impronta SHA-256; immagini delle pagine in cartelle per impronta; testo nativo nel database (`docs/fase-1-decisioni.md`, D4).
- `dbe scan-file` e `dbe scan-folder [--recursive]` (coda che si ferma e riprende con i limiti del fornitore; `--batch` solo con l'adattatore API) con un riepilogo per Gioco (tipologia, pagina, riquadro, confidenza, avvisi) e uno per Rivista. Metadati della Rivista (testata, numero, anno, data, periodicità) proposti dal modello a partire dalla copertina e dal nome del file, sovrascrivibili da opzioni.
- Registro delle chiamate AI (fornitore, modello, durata, token e costo quando disponibili, SHA-256 della risposta) nel `registro`; la risposta grezza va nella tabella `riviste.chiamate_ai`, che si può svuotare (ADR 011); limite di pagine per esecuzione e, solo con l'adattatore API, tetto di spesa; `dbe costs` riepiloga chiamate, tempi e costi per Rivista, pagina e fornitore.
- **Set di riferimento** e banco di prova: `dbe eval extraction --set <percorso> [--fornitore <id>]` produce un rapporto JSON e uno leggibile.
- **Confronto dei Fornitori AI** [P]: sullo stesso set, Claude con l'abbonamento come riferimento, i piani gratuiti di altri fornitori con condizioni d'uso verificate e un modello aperto eseguito sul PC. Rapporto con metriche, costo e **pagine lette per finestra dell'abbonamento**; il fornitore predefinito si registra in configurazione e in un ADR.
- Modalità *registra e riproduci*: la risposta del modello per ogni pagina del Set di riferimento viene salvata una volta, come Registrazione sul PC del proprietario, e riprodotta in locale senza costi. In CI, le Registrazioni del Set sintetico.

**Set di riferimento e misura di accuratezza [P, metriche A].**
- Composizione per questa fase: 15-20 pagine con cruciverba e giochi brevi, di cui almeno 5 da scansioni. Vengono da almeno 4 Riviste diverse fornite dal proprietario. Le fasi 4 e 5 aggiungono crittografie, soluzioni, rebus, sudoku e giochi logici. Lo stesso set serve al confronto dei Fornitori AI.
- Risultato atteso, solo sul PC del proprietario: un JSON per pagina conforme ai descrittori. Lo prepara in bozza un agente Opus, il proprietario lo corregge con lo strumento `dbe eval draft`.
- Conservazione [P, ADR 008]: il Set di riferimento, i risultati attesi e le Registrazioni stanno solo sul PC del proprietario, mai su GitHub. Non esiste un repository `DBEnigmistica-refset`. Le pagine delle Riviste non entrano mai nel repository del prodotto (RQ-05).
- Metriche: **richiamo e precisione dei Giochi** (un Gioco atteso è trovato se tipologia uguale e IoU dei riquadri ≥ 0,5); **accuratezza della tipologia**; **accuratezza delle caselle** del cruciverba (nera o bianca, più il numero); **CER** (tasso d'errore sui caratteri dopo normalizzazione di maiuscole, spazi e apostrofi) su definizioni ed esposti; **campi esatti** per i giochi brevi; costo e tempo medi per pagina.
- Soglie iniziali, da confermare dal proprietario dopo la prima misura di riferimento:

| Metrica | PDF digitali | Scansioni |
|---|---|---|
| Richiamo dei Giochi | ≥ 97% | ≥ 93% |
| Precisione dei Giochi | ≥ 95% | ≥ 90% |
| Tipologia corretta | ≥ 97% | ≥ 93% |
| Caselle del cruciverba corrette | ≥ 99,5% | ≥ 98% |
| CER di definizioni ed esposti | ≤ 1% | ≤ 3% |
| Campi esatti dei giochi brevi | ≥ 95% | ≥ 90% |

**Criteri di accettazione.**
1. `dbe types generate --check` termina con codice 0 (SQL registrato uguale a quello generato). Un fuzz test con 10.000 descrittori casuali, inclusi nomi e tipi con payload d'iniezione, li rifiuta o li quota tutti. Applicati su PostgreSQL, nessuno esegue istruzioni estranee: il test conta le tabelle prima e dopo.
2. `dbe migrate` su un database vuoto crea il nucleo e le tabelle delle 4 tipologie in `riviste` e in `catalogo`; una seconda esecuzione non fa nulla. squawk non riporta errori sulle migrazioni generate.
3. *Verificato in locale* (ADR 008): `dbe scan-file <pdf del Set di riferimento> --replay` salva esattamente i Giochi attesi. Una seconda esecuzione fa 0 chiamate al modello (contatore nel log) e non crea duplicati. In CI lo stesso test usa il Set sintetico.
4. Interrompendo `scan-folder` a metà e rilanciandolo, si riprende dalle pagine mancanti: test con un estrattore finto che conta le chiamate.
5. `dbe eval extraction --live` sul Set di riferimento rispetta le soglie confermate (verificato in locale). Il rapporto è allegato alla release.
6. Ogni chiamata AI ha una riga nel registro con fornitore, modello e durata, più token e costo quando il fornitore li riporta. Con un limite di 1 pagina per esecuzione il comando si ferma dopo la prima pagina con codice 4 e il messaggio "limite dell'esecuzione raggiunto". Con l'adattatore API e un tetto di 0,01 € si ferma con codice 4 e il messaggio "tetto di spesa raggiunto". Con un estrattore finto che segnala la finestra esaurita, la coda si ferma e riprende senza perdere pagine.
7. Con il set riprodotto, `Page.ImagePath`, il testo nativo e il riquadro normalizzato di ogni Gioco sono valorizzati. Nessun Gioco si perde perché ha una tipologia non prevista: finisce come "nuova tipologia proposta", in attesa della fase 4.
8. Il rapporto di confronto dei Fornitori AI sullo stesso set è allegato alla release, con le pagine lette per finestra dell'abbonamento. Il fornitore predefinito supera le soglie confermate ed è il meno costoso tra quelli con condizioni d'uso verificate dal proprietario; se nessuno le supera, resta Claude con l'abbonamento e la decisione si registra nell'ADR.

**Requisiti coperti.** OBJ-03, OBJ-04 (con visione), OBJ-05, OBJ-06, APP-01, APP-02, APP-03, APP-04, APP-05, APP-06, APP-09, APP-10, DB-05, DB-09, DB-10, DB-11, DB-12 (cruciverba e brevi), UX-01 (riga di comando), CLI-02, CLI-03, CLI-07 (`eval`, `costs`), TEC-05, TEC-06 (rendering), TEC-08, TEC-10 (riformulato), AI-01, OUT-04, OUT-05, OUT-14 (generatore), OUT-17, OUT-20. Piano commerciale: RQ-07 (registro delle chiamate AI, riusato in fase 6), RQ-20, RQ-22 (validatori strutturali), RQ-23 (limite per esecuzione; tetto di spesa solo con l'adattatore API).

**CI aggiunta e artefatti.** Controllo di coerenza dei descrittori e degli SQL generati, fuzz test del generatore, squawk, applicazione delle migrazioni su Testcontainers, **test riprodotti sul Set sintetico**: la regressione fallisce se una metrica scende sotto la linea di base. La regressione sul Set di riferimento vero non sta in CI: è uno script locale che il proprietario lancia prima di ogni release, con il rapporto allegato alla release (eccezione dichiarata alla regola sulle GitHub Actions, ADR 008). La misura dal vivo (`dbe eval extraction --live`) gira sul PC del proprietario e non in CI [A: l'abbonamento non va usato da runner di terzi e le pagine delle Riviste restano a casa]; il rapporto si allega alla release. Un workflow `eval-live.yml` si aggiunge solo se si attiva l'adattatore API. Test di avvio del rendering PDFium (`bblanchon.PDFium`) nell'immagine chiseled. Artefatti: immagine con `scan-*`, rapporto di accuratezza in JSON e in versione leggibile.

**Ondate.**

| Ondata | Agenti | Contenuto |
|---|---|---|
| 1.1 (**fatta** 20/11/2026; `1.1-prova` aperta) | Opus A, Sonnet B, Sonnet C | A: meta-schema dei descrittori, regole di traduzione verso SQL, JSON Schema e resa; definizione delle metriche e del formato del risultato atteso; prompt di estrazione. B: adattatore PDF (PDFiumCore e `bblanchon.PDFium`, PdfPig, PNG, impronte, Archivio), con prova del PDFium nell'immagine chiseled e della resa dei font non incorporati. C: interfaccia del Fornitore AI e adattatore Claude Code non interattivo (output strutturato verificato, retry, finestra esaurita, registrazione e riproduzione, limite per esecuzione); prova di Claude Code sul PC Windows con login OAuth, nota scritta sul container |
| 1.2 | Sonnet A, Sonnet B, Sonnet C | A: generatore SQL, `types generate`, `migrate` con le migrazioni delle tipologie, scrittore e lettore parametrizzati, fuzz test. B: descrittori delle 4 tipologie e validatori strutturali. C: `eval draft` ed `eval extraction`, calcolo delle metriche |
| 1.3 | Sonnet A, Sonnet B | A: pipeline per pagina (idempotenza, ripresa, persistenza, metadati della Rivista). B: coda delle cartelle nei limiti del fornitore, riepiloghi in console, `costs`, misura delle pagine per finestra dell'abbonamento |
| 1.4 (accanto alla 1b.3) | Opus A, Sonnet B | A: analisi degli errori sulla prima misura reale e messa a punto di prompt e DPI; confronto dei Fornitori AI sul set (Claude con l'abbonamento, piani gratuiti, modello aperto locale). B: adattatori dei piani gratuiti scelti e del modello locale, correzioni e nuova linea di base |
| Revisione | Opus (sforzo massimo) | Sicurezza del generatore e dello scrittore, segreti del Fornitore AI, limiti per esecuzione, determinismo dei test riprodotti |

**Rischi e segnali precoci.**
- *Griglie dei cruciverba lette male dalle scansioni.* Segnale: caselle corrette sotto il 98% alla prima misura. Contromisure, in ordine: DPI più alti; attivazione della seconda chiamata sul ritaglio; contrassegno "da verificare" su tutte le griglie delle scansioni.
- *Troppe poche pagine per finestra dell'abbonamento.* Segnale: la misura dell'ondata 1.3 mostra che una Rivista di 96 pagine richiede più di due finestre. Contromisure, in ordine: estrazione nelle ore senza sviluppo (notte, fine settimana); fornitore gratuito o locale per le pagine senza griglie; adattatore API solo se l'ostacolo è misurato e il proprietario accetta la spesa.
- *Fornitori gratuiti o locali sotto soglia.* Segnale: il confronto dell'ondata 1.4. Contromisura: resta predefinito Claude con l'abbonamento e si ripete il confronto quando cambiano i modelli disponibili.
- *Risultati attesi costosi da preparare.* Segnale: più di 30 minuti per pagina nella prima sessione. Contromisura: si parte con 10 pagine e il set cresce nelle fasi successive.

**Dal proprietario (circa 20 h).** Fornire i PDF (almeno 4 testate, incluse scansioni); correggere i risultati attesi di 15-20 pagine (8-10 h); **approvare le regole** delle 4 tipologie, scritte in bozza dagli agenti (1 h); **verificare le condizioni d'uso** dell'abbonamento per l'uso non interattivo e dei piani gratuiti candidati, compreso l'uso dei dati per addestramento (circa 2 h); installare il modello aperto locale, se la scheda grafica del PC lo consente (circa 1 h); scegliere il fornitore predefinito sul rapporto di confronto; eseguire `/login` di Claude Code sul PC; confermare le soglie di accuratezza dopo la linea di base; tenere sul PC il Set di riferimento e lanciare la regressione locale prima di ogni release, allegandone il rapporto.

---

### Fase 1b - Prototipo di revisione della creazione (confermata)

**Obiettivo.** Misurare, entro il 21 febbraio 2027, i minuti mediani di revisione e la quota di candidati accettati per **sei famiglie di Giochi, 5 candidati ciascuna** [P]: **crittografia breve** (sottotipi sinonimica e perifrastica), **anagramma a tema**, **rebus**, **cruciverba classico** (9x9-13x13, almeno 3 candidati su 5 di 11x11 o più, la misura "medio" del piano commerciale), **sudoku** e **giochi logici** (futoshiki e nonogramma, confermati dal proprietario l'11/11/2026). Il proprietario decide la soglia **G1 il 28 febbraio 2027**, come da piano commerciale [P]. Con 5 candidati per famiglia la misura è **indicativa**: la soglia dei 20 minuti per i giochi a parole (60 per il cruciverba) va confermata con la misura ripetuta della fase 6 (anagramma e cruciverba) e della fase 7 (crittografia).

**Giochi logici scelti** [P, confermati l'11/11/2026; proposti dall'architetto]. *Futoshiki* (5x5-7x7): riusa il risolutore a vincoli del sudoku (quadrato latino più i segni di maggiore e minore tra caselle), la soluzione unica si verifica in millisecondi e la griglia piccola si legge bene da telefono. *Nonogramma* (Hanjie, 10x10-15x15): meccanica diversa, con indizi ai margini e un disegno come premio, e soluzione unica verificabile con un risolutore per righe e colonne più ricerca. Scartati: kakuro (generarlo con soluzione unica costa di più e chiede calcoli), hashi (resa a ponti, non a caselle), griglie logiche a indizi testuali (richiedono testo, quindi AI). Entrambi sono descritti in `docs/giochi_enigmistici.md` (sezione "Puzzles logici numerici").

**Perché qui.** Il proprietario ha confermato le date del piano commerciale [P]. La soglia G1 è la prova di cui dipende tutta la strategia commerciale e richiede circa 14 ore di revisione a cronometro tra gennaio e febbraio. Il prototipo deve quindi essere pronto verso il 10 gennaio 2027, con scadenza il 17, accanto all'estrazione e non dopo. Non dipende dall'estrazione: usa solo la conoscenza di formato, cioè le regole delle tipologie approvate dal proprietario [P].

**Perimetro (volutamente minimo) [P].** Per crittografia breve, anagramma, rebus e cruciverba una richiesta al Fornitore AI di creazione per candidato (Claude Code con l'abbonamento; il modello Opus ha una chiave di configurazione separata da `FornitoreAi:Modello`, D1b-6). Il cruciverba riceve lo schema di caselle nere generato dal programma (D1b-4). Sudoku e giochi logici si generano con un algoritmo e si verificano automaticamente, **senza AI**. Una schermata di revisione con cronometro. Il rebus usa **immagini segnaposto descritte a parole**: le immagini vere restano legate al parere legale. Niente lotti, validatori completi, registro completo, pubblicazione o autenticazione: il prototipo gira solo su `localhost` del PC del proprietario.

**Riuso, riscrittura, eliminazione.** Riusa l'adattatore Claude Code della fase 1 (ondata 1.1), dietro una porta di creazione separata che salva le risposte grezze nello schema `prototipo`, non in `riviste.chiamate_ai`, i descrittori `cruciverba-classico` e `anagramma` della fase 1 e descrittori di prototipo (versione 0, che il generatore SQL della 1.2 salta) per crittografia breve, rebus, sudoku, futoshiki e nonogramma, che le fasi 4 e 5 completano in v1. Non c'è codice esistente da toccare. Il prototipo **anticipa** due pezzi che le fasi successive estendono, non buttano: l'host `DBEnigmistica.Web`, con le sole rotte del prototipo, che la fase 2 completa con profili e passkey, e l'ossatura React e TypeScript (Vite), che diventa l'involucro dello Studio nella fase 2. Cronometro e registrazione delle differenze passano nella Postazione della fase 6.

**Risultati consegnati.**
- Schermata "Nuovo candidato": tipologia, brief (tema, vincoli, difficoltà), *Genera*. Per i giochi a parole e il rebus una chiamata al Fornitore AI con output strutturato conforme al descrittore; il prompt contiene solo il brief e le regole approvate. Per sudoku e giochi logici il generatore algoritmico, con difficoltà stimata dalle tecniche di risoluzione necessarie.
- Schermata "Revisione": candidato modificabile campo per campo (griglia e definizioni del cruciverba; esposto, diagramma e soluzione di crittografia e anagramma; descrizione a parole delle vignette, lettere, diagramma e soluzione del rebus; griglia e indizi di sudoku e giochi logici). Controlli automatici leggeri visibili al revisore: incroci della griglia coerenti, lunghezze corrette; per sudoku e giochi logici la soluzione unica, ricontrollata dopo ogni modifica. Cronometro del tempo attivo, che esclude le pause oltre 2 minuti. Esiti *Accetta* o *Scarta* con motivo. Differenze tra la risposta dell'AI e la versione finale salvate.
- Schema `prototipo` nel database dello Studio con candidati, risposta grezza, versione finale, differenze, tempo attivo ed esito. Nel registro in sola aggiunta vanno solo metadati: la chiamata di creazione con lo SHA-256 della risposta, e l'esito della revisione (D1b-2). I candidati accettati restano bozze con origine `originale` e registro parziale: non sono pubblicabili finché la fase 6 non completa il registro.
- `dbe eval review-time`: mediana, quartili e quota accettata per tipologia, più il confronto con le soglie G1 (crittografia breve e anagramma ≤ 20 minuti, cruciverba ≤ 60 minuti, almeno il 50% accettato), con l'avviso "misura indicativa" sotto i 10 candidati. Si contano tutti i candidati validi per lo schema, in ordine di generazione, senza scelta a mano; quelli non validi si rigenerano e si contano a parte nel rapporto (D1b-3). Per rebus, sudoku e giochi logici il rapporto dà i tempi senza soglia G1.
- Avvio locale: `dbe prototipo` dall'eseguibile `dbe` win-x64 della CI, sul PC Windows del proprietario. Il server ascolta su `127.0.0.1`; PostgreSQL gira nel Compose (`docker compose up -d postgres`). Il comando arriva nella ondata 1b.1 (D1b-1).

**Criteri di accettazione.**
1. Playwright: generare (modello finto), modificare due campi, accettare. Nel database risultano le differenze dei due campi e un tempo attivo coerente con l'orologio simulato.
2. Test unitari del cronometro: le pause oltre 2 minuti sono escluse; ripresa dopo il ricaricamento della pagina.
3. Un test semina stringhe sentinella nello schema `riviste` e verifica che nessun prompt del prototipo le contenga. Il ruolo `dbe_creazione` riceve un errore di permesso su `riviste`.
4. Il server ascolta solo su `127.0.0.1` (test d'integrazione sull'indirizzo di ascolto).
5. `dbe eval review-time` su 30 candidati sintetici produce i valori attesi del test.
6. Il proprietario ha rivisto 5 candidati per ciascuna delle sei famiglie entro il 21 febbraio 2027; il rapporto G1 è allegato a una release della 1b (`v0.2.0-prova.1` o successive).
7. Test di proprietà: 1.000 sudoku, futoshiki e nonogrammi generati hanno tutti una e una sola soluzione secondo il risolutore, e la loro generazione fa 0 chiamate al Fornitore AI (contatore dell'adattatore finto).
8. I candidati rebus contengono solo descrizioni testuali delle vignette: lo schema `prototipo` non ha campi per immagini (il diagramma è `esposto_diagramma`, non `immagine_diagramma`) e nessuna immagine viene generata né importata.

**Requisiti coperti.** Piano commerciale: prova 1, soglia G1; anticipo parziale di RQ-06, RQ-08, RQ-09, RQ-11.

**CI aggiunta e artefatti.** Toolchain Node in `ci.yml` (lint, controllo dei tipi e build dell'interfaccia; test dei componenti con Vitest), Playwright sul prototipo, test del cronometro e dei permessi, test di proprietà dei generatori di sudoku e giochi logici. Artefatti: pre-release `v0.2.0-prova.1` con l'eseguibile `dbe` win-x64 della CI (senza spostare `latest`), rapporto `review-time` in JSON e versione leggibile.

**Ondate** (alternate a quelle della fase 1).

| Ondata | Agenti | Contenuto |
|---|---|---|
| 1b.1 (dopo la 1.1) | Opus A, Sonnet B, Sonnet C | A: prompt di generazione per crittografia breve, anagramma, rebus (con vignette descritte a parole) e cruciverba, dal brief e dalle regole approvate; descrittori v0; bozze dei brief tipo per le famiglie con AI, da correggere dal proprietario (D1b-7). B: host web minimo, comando `dbe prototipo`, schema `prototipo`, API di generazione e salvataggio. C: generatori algoritmici e risolutori di sudoku, futoshiki e nonogramma, con verifica della soluzione unica e stima della difficoltà; generatore dello schema di caselle nere del cruciverba (D1b-4) |
| 1b.2 (dopo la 1.2) | Sonnet A, Sonnet B, Sonnet C | A: ossatura React e TypeScript, schermate di generazione e revisione, cronometro; ADR 007 per host e ossatura (D1b-9). B: controlli leggeri, `eval review-time`, Playwright. C: schermate di revisione del rebus segnaposto e delle griglie di sudoku e giochi logici |
| 1b.3 (durante la prova) | Sonnet A | Correzioni dagli attriti segnalati nella prima settimana di prova |
| Revisione | Opus (sforzo massimo) | A fine 1b.2, prima che inizi la prova 1 |

**Rischi e segnali precoci.**
- *Cruciverba generato da una sola richiesta poco coerente*: il tempo misurato sarebbe pessimista rispetto al motore completo della fase 6, che userà un risolutore. Segnale: più di 10 incroci incoerenti per griglia nei primi 5 candidati. Contromisura, da subito (D1b-4): il programma genera lo schema di caselle nere valido, e la richiesta chiede solo le parole e le definizioni. Il dato resta comunque una misura del lavoro di revisione.
- *Prototipo non pronto il 17 gennaio.* Segnale: la 1b.2 non chiude entro il 10 gennaio. Contromisura: le ondate della fase 1 cedono il posto alla 1b, perché G1 ha la priorità sulla calibrazione dell'estrazione.
- *G1 a "cambia"*: si segue il piano commerciale (un mese in più e nuova misura entro il 31 marzo 2027). La nuova misura si fa sul prototipo migliorato, senza anticipare la fase 6.
- *Misura indicativa*: con 5 candidati una mediana dipende da uno o due candidati anomali. Segnale: quartili molto distanti nel rapporto. Contromisura: G1 si decide sulle tre famiglie con soglia (crittografia, anagramma, cruciverba) lette insieme; la conferma arriva dalle misure ripetute delle fasi 6 e 7.
- *Prototipo più largo* (sei descrittori, un generatore algoritmico, rebus segnaposto). Segnale: lo stesso del rischio *Prototipo non pronto il 17 gennaio*, oppure le ore di costruzione del proprietario superano le 5,5. Contromisura, oltre a quella di quel rischio: sudoku e giochi logici si rivedono appena pronti, già nella prima metà di gennaio; se serve, il rebus (che non ha soglia G1) esce dalla prova di febbraio e si misura entro il 31 marzo 2027, e l'ondata 1.4 passa dopo il 28 febbraio.
- *Contesa dei limiti dell'abbonamento* tra prova 1, estrazione della fase 1 e sviluppo. Segnale: generazioni interrotte per finestra esaurita durante le sessioni di prova. Contromisura: i candidati a parole si generano in anticipo, fuori dalle ore di sviluppo, e la sessione di revisione non aspetta il Fornitore AI.

**Dal proprietario (circa 19,5 h).** Costruzione (circa 5,5 h): confermare i giochi logici scelti (domanda aperta 12); approvare in chat le regole delle sei tipologie (circa 2,5 h), con data e versione scritte dall'agente nel campo `regole` del descrittore, in un commit dedicato; correggere i brief tipo delle quattro famiglie generate con l'AI, scritti in bozza da un agente Opus (circa 1,5 h, D1b-7); prova e collaudo del prototipo (circa 1,5 h). **Prova 1** (circa 14 h): 5 candidati per famiglia a cronometro tra il 18 gennaio e il 21 febbraio 2027. Alle soglie G1 servono circa 100 minuti per la crittografia breve, 100 per l'anagramma, 300 per il cruciverba, 150-225 per il rebus (30-45 minuti l'uno) e circa 50 per sudoku e giochi logici insieme, cioè 12-13 ore, più circa un'ora per brief, note e pause. **Decisione G1 il 28 febbraio 2027.**

**Tiene la data del 28 febbraio 2027?** Sì, con poco margine [S]. Dal 16 novembre al 17 gennaio ci sono circa 48 ore di guida (7 settimane piene e 2 di festività a metà ritmo); servono circa 5 ore per chiudere la fase 0, circa 15 per la parte della fase 1 prima del 17 gennaio e 5,5 per la costruzione della 1b. Le ore di guida avanzano (circa 22), ma il vincolo vero è la finestra della prova: dal 18 gennaio al 21 febbraio le 3 ore settimanali di contenuti danno 15 ore contro le 14 della prova 1, quindi il margine è di circa un'ora. Parte delle ore avanzate copre la revisione anticipata di sudoku e giochi logici a gennaio, che riporta il margine a circa due ore. Se il margine si esaurisce, non si sposta G1: si applicano in ordine le contromisure del rischio "Prototipo più largo" (rebus fuori dalla prova di febbraio, ondata 1.4 dopo il 28 febbraio).

---

### Fase 2 - Fondamenta dell'interfaccia e Studio in container

**Obiettivo.** Lo Studio gira come container sul PC Windows del proprietario con Docker Desktop [P], acceso quando lavora e raggiungibile dagli altri dispositivi solo sulla rete di casa, con accesso tramite passkey. Niente accesso da fuori casa né rete privata per ora. Ha l'involucro "Banco" con la coda in sola lettura e un Lettore di base che fa giocare da tastiera un cruciverba estratto.

**Perché qui.** Arriva dopo la fase 1 perché l'interfaccia mostra dati reali fin dal primo giorno, e dopo la fase 1b perché ne eredita host e ossatura. Prima di Studio e Sito servono un'autenticazione forte, i due profili e un Lettore unico. Tecnologia decisa [P]: **React e TypeScript** per l'interfaccia, **API ASP.NET Core** per il motore, Lettore distribuibile anche come componente incorporabile; Blazor scartato. **Decisione rinviata** [P, ADR 010]: se Claude Code gira nel container dello Studio con un token dell'abbonamento; si decide in questa fase, dopo la verifica delle condizioni d'uso.

**Riuso, riscrittura, eliminazione.** Si estendono l'host `DBEnigmistica.Web` e l'ossatura React della fase 1b; le schermate del prototipo restano dietro il comando `dbe prototipo` fino alla fase 6, che le sostituisce con la Postazione. Si riusano i casi d'uso dell'Application layer e i repository, esposti tramite API HTTP. La CLI resta un host separato sulla stessa Application.

**Risultati consegnati.**
- Host `DBEnigmistica.Web` (ASP.NET Core, API minime con OpenAPI) con i profili `studio` e `pubblico`. Il profilo pubblico non espone le rotte `/api/studio/*` e rifiuta di avviarsi con la stringa di connessione dello Studio.
- Autenticazione con passkey di ASP.NET Core Identity (.NET 10) e un codice di recupero stampabile. Un solo utente nel profilo studio (RQ-35). **La passkey serve ancora** [A]: la rete di casa non è un confine fidato (ospiti, dispositivi di altri, oggetti connessi), e lo Studio custodisce le Riviste e può avviare chiamate sull'abbonamento del proprietario; nel profilo pubblico serve comunque per l'Admin. Il costo vero è l'HTTPS sulla rete di casa, che le passkey richiedono: lo Studio risponde in HTTPS con un certificato locale [A: Caddy con autorità di certificazione interna, la cui radice si installa una volta sui dispositivi del proprietario]. Dal PC stesso (`localhost`) le passkey funzionano anche senza certificato.
- Interfaccia React e TypeScript (Vite): libreria di componenti accessibili senza stile [A: React Aria, per griglie e controlli con ARIA già corretti]; client dell'API generato dall'OpenAPI, così un cambiamento dell'API rompe la compilazione dell'interfaccia; test dei componenti con Vitest e Testing Library; test end-to-end con Playwright per .NET [A: restano nella suite .NET]. Ogni schermata parla solo con l'API HTTP. Le schede hanno un indirizzo proprio dal primo giorno.
- Token di design derivati da `PROPOSTE-UI.md`: identità **Banco** per Studio e Admin; **Carta con le regole di Lente** per Lettore e Sito. Tema chiaro e scuro, testo di base 18-20 px, aree di tocco di almeno 44 px, focus visibile.
- Involucro "Banco" (proposta A): barra laterale con le viste della coda, coda in sola lettura sui dati della fase 1, gestore delle scorciatoie da tastiera disattivabili e rimappabili (WCAG 2.1.4).
- **Lettore v0** come pacchetto TypeScript a sé (`packages/lettore`), con due uscite fin da subito [P]: componente React per Studio e Sito e **web component** (elemento personalizzato) per l'incorporamento della fase 9 (RQ-19). Resa `griglia` dal descrittore, griglia `role="grid"` con fuoco mobile, modalità elenco, verifica offline tramite impronta della soluzione per parola, modi *gioco*, *anteprima* e *verifica affiancata*. Budget di peso: al massimo 80 KB compressi per il web component.
- `deploy/studio/compose.yml` (applicazione, PostgreSQL, volume dati, HTTPS locale), script di backup (`pg_dump`, volume e cartella dell'Archivio) con ripristino provato, guida all'installazione su Docker Desktop per Windows con l'avvio insieme al PC e l'accesso dalla rete di casa.

**Criteri di accettazione.**
1. `docker compose -f deploy/studio/compose.yml up -d` porta `/health` a 200 entro 60 secondi su una macchina pulita (test in CI con l'immagine appena costruita).
2. Qualunque chiamata `/api/*` senza sessione restituisce 401. L'accesso con passkey funziona in Playwright tramite un autenticatore virtuale WebAuthn.
3. Avviato con `DBE_PROFILO=pubblico`, `/api/studio/games` restituisce 404. Con la stringa di connessione dello Studio presente, il processo termina con codice diverso da 0 e un messaggio esplicito.
4. Playwright: un cruciverba del set riprodotto si completa solo da tastiera (frecce, lettere, spazio per cambiare direzione, Tab per la definizione successiva) e la verifica dà "Tutto giusto". Funziona anche a rete disattivata dopo il caricamento.
5. axe-core (Deque.AxeCore.Playwright) non trova violazioni gravi o critiche nel Lettore e nell'involucro.
6. `backup` seguito da `restore` su un database nuovo riproduce gli stessi conteggi e la stessa impronta del registro (test in CI).
7. L'immagine è su GHCR con l'SBOM; Trivy non trova vulnerabilità alte o critiche correggibili.
8. Il web component del Lettore, inserito in una pagina HTML statica senza React, mostra e verifica un cruciverba (Playwright) e pesa al massimo 80 KB compressi (controllo di dimensione in CI).

**Requisiti coperti.** OBJ-02 (prodotto web), TEC-12 (web). Piano commerciale: RQ-05 (profili separati), RQ-14 (base), RQ-16 (verifica e progressi locali, base), RQ-19 (architettura pronta), RQ-35.

**CI aggiunta e artefatti.** Playwright end-to-end contro il compose (lint, tipi e Vitest sono già entrati con la fase 1b), budget di peso del Lettore, axe, controllo delle differenze dell'OpenAPI (un cambiamento incompatibile fa fallire la CI se non dichiarato), build multi-stadio, SBOM, Trivy, test di backup e ripristino. Artefatti: immagine applicativa completa (profili web più CLI), pacchetto del Lettore come artefatto interno.

**Ondate.**

| Ondata | Agenti | Contenuto |
|---|---|---|
| 2.1 | Opus A, Sonnet B, Sonnet C | A: nuovo ADR che sostituisce il 007 per l'interfaccia, con API, profili e passkey (il 007 è scritto nella 1b.2 e limitato all'host e all'ossatura; React e TypeScript con API ASP.NET Core, Tauri); convenzioni dell'API, token di design da `PROPOSTE-UI.md`. B: host web esteso dal prototipo con profili, passkey, OpenAPI. C: ossatura React estesa dal prototipo, client generato, instradamento con indirizzi delle schede, testi in italiano |
| 2.2 | Sonnet A, Sonnet B, Sonnet C | A: Lettore v0 (resa griglia, tastiera, ARIA, modalità elenco, verifica offline). B: involucro Banco, coda in sola lettura, gestore delle scorciatoie. C: compose dello Studio con HTTPS locale, backup e ripristino, pubblicazione dell'immagine, SBOM, Trivy |
| 2.3 | Sonnet A, Haiku B | A: Playwright e axe in CI, autenticatore virtuale. B: guida all'installazione dello Studio su Docker Desktop e all'accesso dalla rete di casa |
| Revisione | Opus (sforzo massimo) | Autenticazione, separazione dei profili, accessibilità del Lettore |

**Rischi e segnali precoci.**
- *Accesso dai dispositivi di casa complicato* (certificato locale, passkey). Segnale: il collaudo richiede più di un'ora. Contromisura [A]: il certificato si installa una volta per dispositivo; un dispositivo che non lo accetta usa lo Studio solo dal PC. Lo Studio non è mai esposto su internet; l'accesso da fuori casa (rete privata come Tailscale) si riconsidera solo su richiesta del proprietario, senza cambiare codice.
- *PC spento o in sospensione durante un'estrazione lunga.* Segnale: lavori interrotti nella coda. Contromisura: la coda riprende dopo il riavvio (fase 3) e la guida indica come evitare la sospensione durante i lavori.
- *La griglia ARIA non funziona con i lettori di schermo.* Segnale: la prova manuale con NVDA dell'ondata 2.3 fallisce. Contromisura: la modalità elenco è sempre disponibile.

**Dal proprietario (circa 6 h).** Dove gira lo Studio è deciso (PC Windows con Docker Desktop, solo rete di casa) [P]; installare il certificato locale sui dispositivi di casa; registrare le passkey; collaudo. Il collaudo cade durante la prova 1, quindi si tiene breve.

---

### Fase 3 - Studio: acquisizione e revisione (cruciverba e giochi brevi)

**Obiettivo.** Il proprietario importa una Rivista dallo Studio, segue l'estrazione in sottofondo e verifica i Giochi da tastiera nella Postazione, accanto al ritaglio della pagina. Può poi cercarli, aprirne la scheda e giocarli.

**Perché qui.** Chiude il primo componente scelto dal proprietario [P] con un prodotto che si usa davvero. La coda, la macchina a stati e la Postazione servono di nuovo al Motore di creazione (fase 6): costruirle qui sui Giochi estratti abbassa il rischio di quella fase.

**Riuso, riscrittura, eliminazione.** Si riusano la pipeline della fase 1 (ora lanciata da lavori in coda), il Lettore v0 e l'involucro della fase 2. La CLI `scan-*` resta per i batch e scrive nella stessa coda.

**Risultati consegnati.**
- *Importa Rivista* (tasto I o trascinamento), con la dichiarazione "è mia, uso privato" alla prima importazione e i metadati proposti da confermare. Doppione riconosciuto per impronta o per testata e numero: *Apri* oppure *Importa come nuova versione*.
- Lavori in sottofondo in una coda su PostgreSQL (`FOR UPDATE SKIP LOCKED`) [A: nessuna dipendenza in più; il carico è di un solo utente]. Avanzamento ("In estrazione 34/96"), sospensione, annullamento e ripresa dopo il riavvio del container.
- Macchina a stati della proposta A per Rivista, Gioco estratto ed Errore. Viste *Da fare*, *Da verificare*, *In corso*, *Errori*, *Tutto*; ordinamento per confidenza crescente.
- **Postazione di verifica**: ritaglio a sinistra, campi a destra, fuoco sul primo campo incerto (segnalato con la parola "incerto", non solo con il colore). Tasti Invio, E, S, J e K; precaricamento dei 2 elementi successivi; tempo medio per Gioco visibile; controllo a campione ogni 20 conferme (attivabile). Ogni modifica è salvata e registrata.
- **Schede con indirizzo proprio** (proposta B) per Rivista, Gioco e Tipologia, aperte con Maiusc+Invio. Riepilogo dei Giochi di una Rivista (UX-01).
- **Ricerca** con Ctrl+K e filtri `tipo:`, `rivista:`, `stato:`, `anno:`, su PostgreSQL con ricerca testuale in italiano, `unaccent` e `pg_trgm`.
- Lettore: resa `esposto_diagramma` per i giochi brevi; i Giochi estratti si giocano solo nello Studio.
- Errori dell'AI come elementi della coda, con *Ritenta*, *Ritenta con altre istruzioni* e *Fai a mano*.

**Criteri di accettazione.**
1. Playwright: l'importazione di un PDF di prova (estrattore riprodotto) porta i primi Giochi in "Da verificare" prima della fine dell'estrazione. 10 Giochi si verificano solo da tastiera.
2. Il passaggio all'elemento successivo nella Postazione ha un p95 sotto i 300 ms (misurato in Playwright con 50 elementi).
3. Fermando il container a metà estrazione e riavviandolo, l'estrazione riprende senza nuove chiamate per le pagine già fatte (contatore dell'estrattore finto).
4. `tipo:cruciverba anno:2025` restituisce esattamente i Giochi attesi del set di prova; la ricerca su 10.000 Giochi sintetici risponde in meno di 200 ms al p95 (test d'integrazione).
5. Nessun comando di pubblicazione o di calendario compare per un Gioco con origine `rivista`. La chiamata diretta all'API di consegna restituisce 422 con il motivo (test d'integrazione e Playwright).
6. Una seconda importazione dello stesso file mostra l'avviso di doppione e non crea record.
7. axe senza violazioni gravi o critiche su coda, Postazione e schede.

**Requisiti coperti.** APP-04 (conferma della periodicità), UX-01 (Studio), TEC-12. Piano commerciale: RQ-04 (blocco lato server per i Giochi estratti), RQ-05.

**CI aggiunta e artefatti.** Suite Playwright del Banco (flussi da tastiera), budget di prestazioni della Postazione, test d'integrazione della ricerca, test di ripresa dei lavori. Artefatto: release `v0.4.0` dello Studio.

**Ondate.**

| Ondata | Agenti | Contenuto |
|---|---|---|
| 3.1 | Sonnet A, Sonnet B, Sonnet C | A: coda dei lavori e API di importazione, avanzamento, ripresa. B: macchina a stati, viste della coda, gestione degli errori. C: indice di ricerca e API di ricerca |
| 3.2 | Sonnet A, Sonnet B, Sonnet C | A: Postazione di verifica (ritagli, fuoco sui campi incerti, precaricamento, campione). B: schede di Rivista, Gioco e Tipologia; tavolozza Ctrl+K. C: resa `esposto_diagramma` nel Lettore |
| 3.3 | Sonnet A, Haiku B | A: Playwright, budget di prestazioni, correzioni dal collaudo. B: guida d'uso dello Studio (tasti, viste) |
| Revisione | Opus (sforzo massimo) | Concorrenza della coda, coerenza degli stati, blocco della pubblicazione |

**Rischi e segnali precoci.**
- *La coda diventa un muro* con 80 Giochi per Rivista (rischio principale della proposta A). Segnale: il collaudo del proprietario si ferma prima della fine del primo numero. Contromisura: le Riviste in verifica hanno una vista propria e non contano come urgenti in *Da fare*.
- *Conferme distratte.* Segnale: il controllo a campione trova errori lasciati passare. Contromisura: campione attivo di default per le prime due Riviste.

**Dal proprietario (circa 10 h).** Importare e verificare almeno 2 numeri reali; misurare i minuti per Gioco verificato (obiettivo indicativo della proposta A: circa 1 minuto); elencare gli attriti; decidere se le scorciatoie a un tasto sono attive di default.

---

### Fase 4 - Crittografie e giochi a chiave, tipologie nuove e soluzioni

**Obiettivo.** Estrarre crittografie e giochi a chiave. Rendere operativo, nello Studio e dalla riga di comando, il flusso "tipologia nuova, proposta, conferma o modifica dell'utente" con migrazioni applicate solo su comando esplicito. Collegare le soluzioni ai Giochi.

**Perché qui.** Le crittografie sono la seconda tipologia per ordine [P] e usano la resa `esposto_diagramma` già pronta. Il flusso delle tipologie nuove serve ora perché la fase 1 accumula "nuove tipologie proposte" senza destinazione. Le soluzioni servono perché una crittografia senza soluzione non si può verificare nel Lettore.

**Interpretazione** [A]: "giochi a chiave" = giochi la cui soluzione nasce da una chiave o da un diagramma: crittografie (pura, sinonimica, perifrastica, a frase) e cruciverba crittografato (un numero per lettera). Il proprietario la conferma o la corregge prima della fase.

**Riuso, riscrittura, eliminazione.**

| Elemento | Destino | Riferimento |
|---|---|---|
| Interazione s/n/m di conferma dello schema | Rifattorizzare in Console (Spectre.Console) e completare `modify-column`; si rivalida dopo ogni modifica | `SchemaValidationService.cs:52-53`, `:181`, `:254` |
| `SchemaProposalService`, `DynamicSchemaOrchestrator` | Riscrivere: producono descrittori, non DDL | `src/DBEnigmistica.Application/Services/Schema/` |
| Comandi `analyze-game`, `analyze-game-text` | Rifattorizzare: il file `.md` diviso per gioco e l'opzione `--yes` rispettata | `AnalyzeGameCommand.cs:17-41` |
| Entità `GameSolution` e `link-solution` | Tenere ed estendere con il collegamento proposto dal modello | `Domain/Entities/GameSolution.cs` |

**Risultati consegnati.**
- Descrittori `crittografia` (con varianti come enumerazione), `crittografia-a-frase` e `cruciverba-crittografato`, con i relativi validatori: lunghezze del diagramma coerenti con la soluzione, numeri della griglia coerenti con le lettere.
- **Flusso tipologia nuova nello Studio.** L'elemento "Schema da confermare: X, n Giochi in attesa" entra nella coda. La prima volta si apre la procedura guidata in 3 tappe (proposta C: esempi, campi, conferma); dalle volte successive la scheda compatta (proposta A). Si possono modificare nome, tipo (dalla lista chiusa), obbligatorietà e ruolo nel Lettore, con nuova validazione a ogni modifica. Alla fine si sceglie tra *Conferma*, *Unisci a tipologia esistente* e *Rimanda*.
- La conferma crea il descrittore v1 e la migrazione **in attesa**. L'applicazione avviene solo con "Applica migrazione", che mostra l'SQL e chiede conferma, oppure con `dbe types apply <id>`. Dopo l'applicazione i Giochi in attesa vengono rimappati e tornano in "Da verificare".
- **Versioni dei descrittori**: le modifiche generano migrazioni `ALTER` (nuove colonne ammettono valori nulli; rinomina solo con mappatura esplicita; eliminazione di colonne vietata senza una seconda conferma). I Giochi con la versione precedente vengono segnalati.
- Riga di comando: `dbe analyze-game <file.md>` divide il file per sezione di gioco e produce una proposta per gioco (con `docs/giochi_enigmistici.md` circa 30); `dbe analyze-game-text "<descrizione>"`; conferma interattiva in console oppure `--yes`.
- **Soluzioni**: le pagine di soluzioni sono classificate ed estratte; il collegamento ai Giochi (stessa Rivista o numero successivo) è proposto con una confidenza e confermato nella Postazione. Il Lettore abilita la verifica quando la soluzione è collegata.
- Set di riferimento esteso con crittografie, cruciverba crittografati e pagine di soluzioni.

**Criteri di accettazione.**
1. `dbe analyze-game docs/giochi_enigmistici.md --dry-run` produce una proposta per ciascuna sezione di gioco italiana del file (numero atteso fissato nel test) e nessuna migrazione.
2. Una proposta con nomi o tipi ostili (`x"; DROP TABLE`, tipo `TEXT; --`) viene rifiutata con un messaggio e senza effetti sul database.
3. Playwright: procedura in 3 tappe, poi migrazione **in attesa** e verifica che la tabella non esiste ancora; dopo "Applica migrazione" la tabella esiste e i Giochi in attesa sono salvati e di nuovo in "Da verificare".
4. Il passaggio da v1 a v2 di un descrittore genera solo `ALTER` ammessi (squawk senza errori). I Giochi v1 restano leggibili.
5. Sul Set di riferimento: soglie della fase 1 rispettate per le crittografie (CER dell'esposto; diagramma esatto ≥ 97% sui digitali); collegamento delle soluzioni con precisione ≥ 90% e richiamo ≥ 85%.
6. `dbe analyze-game-text "..." --yes` non chiede input e termina con codice 0 (test end-to-end della CLI).

**Requisiti coperti.** DB-11, DB-12 (crittografie), DYN-01, DYN-02, DYN-03, DYN-04, DYN-05, DYN-06, UX-02, UX-03, UX-04, CLI-05, CLI-06, AI-03, AI-04, OUT-06, OUT-12, OUT-13, OUT-14, OUT-18. Piano commerciale: RQ-20 (versioni), base tecnica di RQ-21.

**CI aggiunta e artefatti.** Test di proprietà (FsCheck) sul passaggio tra versioni dei descrittori, Playwright del flusso delle tipologie, metriche delle crittografie e delle soluzioni nei test riprodotti.

**Ondate.**

| Ondata | Agenti | Contenuto |
|---|---|---|
| 4.1 | Opus A, Sonnet B, Sonnet C | A: prompt di proposta delle tipologie (Opus come modello di progetto), regole di evoluzione dei descrittori. B: descrittori e validatori delle crittografie e del cruciverba crittografato. C: estrazione delle pagine di soluzioni |
| 4.2 | Sonnet A, Sonnet B, Sonnet C | A: flusso nello Studio (3 tappe e scheda compatta, Applica migrazione). B: versioni e generatore di `ALTER`. C: `analyze-game` e `analyze-game-text` con divisione del markdown e conferma in console |
| 4.3 | Sonnet A, Sonnet B, Haiku C | A: collegamento delle soluzioni nella Postazione, verifica nel Lettore. B: Playwright e test riprodotti. C: guida "Come si conferma una tipologia" |
| Revisione | Opus (sforzo massimo) | Sicurezza delle proposte e delle migrazioni, coerenza delle versioni |

**Rischi e segnali precoci.**
- *Proposte di tipologie troppo diverse tra loro per giochi simili* (proliferazione). Segnale: più di 3 tipologie nuove per Rivista. Contromisura: il prompt riceve il catalogo e deve proporre prima *Unisci*.
- *Soluzioni nel numero successivo non disponibile.* Segnale: richiamo del collegamento sotto l'85%. Contromisura: stato "soluzione mancante" e collegamento manuale.

**Dal proprietario (circa 11 h).** Confermare l'interpretazione di "giochi a chiave"; fornire pagine di crittografie e di soluzioni (anche del numero successivo); correggere i risultati attesi; rivedere le prime 3-5 tipologie proposte dalle Riviste e dal file `.md`.

---

### Fase 5 - Rebus, involucro desktop e completamento dell'estrazione

**Obiettivo.** Estrarre i rebus con i ritagli delle vignette, e i sudoku e i giochi logici già presenti nel catalogo dalla fase 1b. Consegnare l'involucro desktop dello Studio. Completare i comandi batch utili (script SQL, esportazioni, rielaborazione, integrità, costi).

**Perché qui.** Il rebus è l'ultima tipologia per ordine [P] e richiede la resa `immagine_diagramma`. L'involucro desktop serve a un uso quotidiano dello Studio prima che inizi la creazione. Chiudere qui il Motore di estrazione lascia le fasi 6-7 libere di concentrarsi sulla creazione. Sudoku e giochi logici entrano qui [A]: le quattro tipologie di partenza restano quelle decise, e qui l'estrazione di griglie numeriche costa poco perché il risolutore della fase 1b fa da validatore (una griglia letta male non ha una soluzione unica). La resa `griglia_logica` del Lettore nasce qui per giocarli nello Studio e serve poi alla creazione (fase 6) e al Sito (fase 8).

**Riuso, riscrittura, eliminazione.**

| Elemento | Destino | Riferimento |
|---|---|---|
| Entità `RebusGame` | Sostituire con il descrittore `rebus` | `Domain/Entities/RebusGame.cs` |
| `ExecuteSqlAsync` mai esposto | Rifattorizzare in `dbe sql run` con protezioni | `DynamicMigrationService.cs:114` |
| `DataExportService` ed `export-*` | Tenere; leggere le tabelle per tipologia e i descrittori | `DataExportService.cs:196` |

**Risultati consegnati.**
- Descrittore `rebus` (vignetta come ritaglio, lettere sulle figure, diagramma, prima lettura, soluzione) con validatore del diagramma, estrazione e resa `immagine_diagramma`. I ritagli restano nel volume privato.
- Descrittori v1 di `sudoku`, `futoshiki` e `nonogramma` (dai v0 della fase 1b), estrazione dalle Riviste con il risolutore come validatore, resa `griglia_logica` nel Lettore (tastiera, ARIA, verifica offline).
- **Involucro desktop con Tauri** [P]: ospita l'indirizzo dello Studio (con l'interfaccia React già costruita), notifica di sistema a fine lavoro, trascinamento dei PDF, memoria dell'indirizzo dello Studio nella rete di casa. Installatore Windows pubblicato come asset della release; firma del codice rimandata [A: per un solo utente non serve un certificato a pagamento].
- Comandi: `dbe sql run <file> [--dry-run]` (solo profilo studio, in transazione, rollback in prova, schema `registro` escluso, esecuzione registrata); `dbe export` aggiornato; `dbe reprocess --page <id>`; `dbe stats`; `dbe backup` e `dbe restore`.
- Set di riferimento esteso con i rebus.

**Criteri di accettazione.**
1. Sul Set di riferimento dei rebus: richiamo ≥ 95%, diagramma esatto ≥ 95%, lettere sulle figure corrette ≥ 90% (soglie da confermare dopo la linea di base).
2. `dbe sql run --dry-run` di uno script con `INSERT` lascia i conteggi invariati. Uno script che tocca `registro` viene rifiutato. Con `DBE_PROFILO=pubblico` il comando non esiste.
3. Il workflow di release produce l'eseguibile dell'involucro. Un test di avvio senza interfaccia apre l'indirizzo configurato e termina con codice 0.
4. `dbe export --format csv` dei Giochi di una Rivista del set produce un CSV valido (rilettura e confronto dei conteggi).
5. Sul Set di riferimento di sudoku e giochi logici: indizi esatti ≥ 99% sui digitali; ogni griglia estratta con un indizio sbagliato viene segnalata dal risolutore (nessuna soluzione o più di una) e va in "Da verificare". Playwright: un sudoku e un futoshiki si completano solo da tastiera nel Lettore.

**Requisiti coperti.** DB-11, DB-12 (rebus), CLI-04, CLI-07. Piano commerciale: RQ-36. RQ-37 anticipato per decisione del proprietario (vedi matrice).

**CI aggiunta e artefatti.** Lavoro di build Tauri su runner Windows (toolchain Rust) aggiunto a `publish.yml`, test dei comandi batch, metriche dei rebus nei test riprodotti. Artefatti: eseguibile dell'involucro nella GitHub Release; release `v0.6.0`, Motore di estrazione completo.

**Ondate.** 5.1: Sonnet A (descrittore, estrazione e resa del rebus), Sonnet B (involucro desktop), Sonnet C (`sql run`, `export`, `reprocess`, `stats`, `backup`). 5.2: Sonnet A (test riprodotti e correzioni), Haiku B (aggiornamento delle guide e dell'aiuto in linea della CLI), Sonnet C (descrittori v1, estrazione e resa `griglia_logica` di sudoku e giochi logici). 5.3: Opus A (verifica finale del Motore di estrazione rispetto ai requisiti OBJ, APP, DB, DYN, CLI). Revisione: Opus (sforzo massimo).

**Rischi e segnali precoci.** *Rebus con soluzioni ambigue o vignette complesse.* Segnale: lettere sulle figure corrette sotto l'80%. Contromisura: i rebus entrano sempre in "Da verificare" con il ritaglio grande accanto. *Involucro instabile.* Segnale: problemi della WebView di sistema sulla macchina del proprietario. Contromisura: lo Studio installato come PWA dal browser finché il problema non è risolto.

**Dal proprietario (circa 9 h).** Pagine di rebus e risultati attesi; installazione e prova dell'involucro; collaudo del Motore di estrazione completo.

---

### Fase 6 - Motore di creazione v1: cruciverba e giochi brevi

**Obiettivo.** Creare Giochi originali di cruciverba classico e giochi brevi: brief umano, generazione a lotti, validatori, revisione cronometrata nella Postazione e registro del contributo umano completo fino all'approvazione con impronta e marca temporale. Confermare sul motore completo i tempi misurati per G1 nella fase 1b.

**Perché qui.** Il Motore di creazione viene dopo l'estrazione per decisione del proprietario [P] e **usa l'archivio come base nel senso ammesso oggi** [P]: descrittori con le **regole approvate dal proprietario**, validatori, rese del Lettore, coda e Postazione costruiti nelle fasi 1-5, più il controllo di originalità. I giochi estratti **non** entrano nei prompt, né come esempi né come statistiche: l'interruttore resta spento finché il parere legale non è favorevole. Gli archivi restano separati anche dopo.

**Riuso, riscrittura, eliminazione.** Si riusano descrittori, validatori strutturali, interfaccia del Fornitore AI (registro delle chiamate, limiti per esecuzione), coda, Postazione, Lettore e il codice della fase 1b (generazione, generatori algoritmici, cronometro, differenze). I candidati accettati nella prova 1 entrano nella coda editoriale come bozze da completare nel registro. Le schermate del prototipo si eliminano. Nulla di legacy da eliminare.

**Risultati consegnati.**
- **Ricerca preliminare** (obbligatoria): algoritmi open source di riempimento delle griglie e lessici italiani con licenza compatibile con un uso commerciale. Il proprietario sceglie il lessico.
- Lessico italiano con frequenze e una lista di parole vietate; servizio di lessico.
- **Brief obbligatorio** prima di ogni lotto: tema, idea, vincoli, difficoltà voluta (1-5), pubblico (RQ-06).
- Generazione a lotti con 3 varianti per Gioco, tramite il Fornitore AI di creazione (Claude Code con l'abbonamento, lotti pianificati in orari diversi dallo sviluppo), con limite di chiamate per lotto e costo registrato per Gioco approvato, zero con l'abbonamento (RQ-23).
- **Sudoku e giochi logici** nella coda editoriale: generatori algoritmici della fase 1b, soluzione unica garantita dal risolutore, difficoltà stimata, nessuna chiamata al Fornitore AI. Quale atto umano basta per *Approva* su un gioco generato da algoritmo si decide con il parere legale.
- **Validatori** (RQ-22): riempimento della griglia con un risolutore a vincoli sul lessico, simmetria e connessione della griglia, numerazione, parole presenti nel lessico; per i giochi brevi, verifica deterministica del meccanismo e unicità della soluzione nel lessico dove possibile.
- Coda editoriale con gli stati Richiesto, In generazione, Da rivedere, Approvato, Venduto e Ritirato (In calendario e Pubblicato si aggiungono in fase 8) (RQ-25).
- Postazione di revisione: anteprima nel Lettore con cronometro, modifiche campo per campo con prima e dopo, scarti con motivo, difficoltà assegnata (RQ-08, RQ-09, RQ-24).
- **Registro del contributo umano**: brief, chiamate AI, scelte, modifiche, tempo, approvazione con SHA-256 della versione approvata e marca temporale RFC 3161. *Approva* resta bloccato se manca un atto creativo umano oltre all'approvazione (RQ-07, RQ-12).
- Controllo di originalità contro il proprio catalogo, con soglia e blocco (RQ-10, prima parte).
- Costruttore dei prompt che usa solo brief, regole approvate (versione registrata nel registro) e lessico.
- Interruttore `Creazione:EsempiDaArchivio`, spento di default. Si può accendere solo se è configurato il riferimento a un parere legale favorevole e il ruolo `dbe_creazione` ha ottenuto il permesso di lettura su una vista limitata di `riviste` tramite una migrazione dedicata. Anche acceso, i Giochi estratti restano nel loro archivio e ogni prompt che li usa lo dichiara nel registro (RQ-11).

**Criteri di accettazione.**
1. Con l'interruttore spento, un test d'integrazione semina nell'archivio delle Riviste stringhe sentinella e verifica che nessun prompt registrato ne contenga una; il ruolo `dbe_creazione` riceve un errore di permesso su `riviste`. Una tipologia con regole non approvate non si può generare (errore 422).
2. Su 100 griglie generate (9x9-13x13), il 100% supera i validatori di coerenza e tutte le parole sono nel lessico (test con un generatore finto per le definizioni).
3. Senza brief la generazione non parte (errore 422). Senza atto creativo umano *Approva* è disabilitato e l'API restituisce 422.
4. L'approvazione scrive nel registro l'impronta della versione approvata e una marca temporale verificabile con `dbe verify-integrity --timestamps`. Il ripristino da backup conserva la catena delle impronte.
5. `dbe eval review-time` sul motore completo (almeno 10 candidati per anagramma e cruciverba) produce mediane non peggiori di quelle della prova 1 e conferma le soglie di G1 (20 minuti per l'anagramma, 60 per il cruciverba), che la prova 1 con 5 candidati dava solo come indicative. Se sono peggiori, il rapporto lo segnala e la fase non si chiude senza una decisione del proprietario. La crittografia breve si conferma nella prova della fase 7.
6. I candidati accettati nella prova 1 sono nella coda editoriale come bozze; nessuno risulta approvato senza registro completo.

**Requisiti coperti.** Piano commerciale: RQ-06, RQ-07, RQ-08, RQ-09, RQ-10 (catalogo), RQ-11, RQ-12, RQ-22, RQ-23, RQ-24 (assegnazione), RQ-25; conferma di G1.

**CI aggiunta e artefatti.** Test di proprietà sui validatori e sul riempimento, test delle sentinelle e dei permessi, test di immutabilità del registro e delle marche temporali (con un TSA finto in CI), test del blocco di approvazione. Workflow `eval-generation.yml` (manuale) con quota di validità e costo per candidato. Artefatti: rapporto di conferma dei tempi; release `v0.7.0`.

**Ondate.**

| Ondata | Agenti | Contenuto |
|---|---|---|
| 6.1 | Opus A, Sonnet B | A: ricerca su algoritmi di riempimento e lessici (licenze), progetto del risolutore e dei validatori, prompt per definizioni ed esposti. B: servizio di lessico e brief |
| 6.2 | Sonnet A, Sonnet B, Sonnet C | A: riempimento delle griglie e validatori del cruciverba. B: generatori e validatori dei giochi brevi, lotti e limiti per lotto, ingresso di sudoku e giochi logici nella coda editoriale. C: stati della coda editoriale e Postazione di revisione (cronometro, differenze, scarti) |
| 6.3 | Opus A, Sonnet B | A: progetto del registro completo (blocco senza atto creativo, marca temporale, catena delle impronte). B: implementazione, originalità contro il catalogo, interruttore e permessi |
| 6.4 | Sonnet A, Sonnet B | A: test end-to-end del flusso dal brief all'approvazione, migrazione dei candidati della prova 1. B: `eval-generation`, rimozione delle schermate del prototipo |
| 6.5 | Proprietario e Sonnet A | Misura di conferma sul motore completo, correzioni dai riscontri |
| Revisione | Opus (sforzo massimo) | Integrità del registro, permessi, correttezza del risolutore |

**Rischi e segnali precoci.**
- *L'AI è debole sui giochi di parole italiani* (rischio 1 del piano commerciale). Segnale: quota accettata sotto il 30% nei primi 20 candidati. Contromisura: prima i formati più strutturati (cambio di lettera, anagramma), istruzioni più vincolanti, validatori più severi.
- *Riempimento delle griglie lento.* Segnale: più di 30 secondi per un 13x13. Contromisura: griglie modello predefinite e un lessico filtrato per frequenza.
- *Licenza del lessico incompatibile.* Segnale: nessun lessico adeguato nella ricerca dell'ondata 6.1. Contromisura: lessico costruito da fonti con licenza permissiva e verificato dal proprietario.

**Dal proprietario (circa 14 h).** Scegliere il lessico e il servizio di marca temporale, alla luce del parere legale; decidere l'interruttore degli esempi (resta spento se il parere non è favorevole); scrivere i brief; rivedere a cronometro 10 candidati per tipologia sul motore completo (circa 5 h); completare il registro dei candidati della prova 1 da tenere.

---

### Fase 7 - Motore di creazione v2: crittografie, rebus e laboratorio dei format

**Obiettivo.** Creare crittografie, giochi a chiave e rebus originali. Aprire il laboratorio dei format: descrizione di un gioco nuovo, schema proposto dall'AI, modifica e conferma dell'autore, validatori e resa. Esportare il dossier di paternità.

**Perché qui.** Completa le quattro tipologie anche nella creazione [P]. Porta un format originale, prerequisito dell'MVP commerciale, prima del Sito. Riusa il flusso delle tipologie della fase 4 nel contesto della creazione.

**Riuso, riscrittura, eliminazione.** Si riusano i descrittori di crittografie e rebus della fase 4-5, il flusso delle tipologie della fase 4 e il registro della fase 6.

**Risultati consegnati.**
- Creazione di crittografie e cruciverba crittografati con validatori: lunghezze del diagramma, vincoli dell'esposto per la crittografia pura, unicità della chiave nel cruciverba crittografato.
- Creazione di rebus [A]: immagini **da una libreria con licenza o di pubblico dominio**, composte dal proprietario, al posto delle vignette segnaposto descritte a parole della prova 1. La generazione di immagini con l'AI resta dietro un interruttore che dipende dal parere legale (rischio 2d del piano commerciale).
- **Laboratorio dei format** (RQ-21): stesso flusso della fase 4 avviato da una descrizione nello Studio. Il format nuovo sceglie un modello di resa esistente e dichiara i validatori generici che usa.
- **Dossier di paternità** in PDF e JSON per Gioco (RQ-13).
- Controllo di originalità anche contro l'archivio delle Riviste, tramite il ruolo `dbe_originalita`, che restituisce solo punteggi (RQ-10, seconda parte).

**Criteri di accettazione.**
1. Un format di prova descritto nel laboratorio arriva fino a un Gioco giocabile nel Lettore senza codice nuovo. Il test end-to-end usa un modello finto.
2. Il dossier JSON di un Gioco approvato rispetta lo schema pubblicato e contiene i punti 1-11 della sezione 12 del piano commerciale. Il PDF si genera in modo deterministico (test di snapshot sul testo estratto).
3. Un Gioco creato e quasi identico a uno dell'archivio (dati seminati) viene bloccato dal controllo di originalità con un motivo. Il ruolo `dbe_originalita` non può restituire testo dell'archivio (test sui permessi delle funzioni).
4. Validatori delle crittografie: 100% dei casi di prova scritti dal proprietario classificati correttamente.

**Requisiti coperti.** Piano commerciale: RQ-10 (libreria), RQ-13, RQ-21. Format originale dell'MVP.

**CI aggiunta e artefatti.** Test di snapshot del dossier, test del laboratorio dei format, test del ruolo di originalità. Artefatti: esempio di dossier; release `v0.8.0`.

**Ondate.** 7.1: Opus A (prompt e validatori delle crittografie, regole del laboratorio), Sonnet B (composizione dei rebus da libreria), Sonnet C (dossier di paternità). 7.2: Sonnet A (generatori delle crittografie), Sonnet B (laboratorio dei format nello Studio), Sonnet C (originalità contro l'archivio). 7.3: Sonnet A (end-to-end e correzioni), Haiku B (guide per l'autore). 7.4: proprietario (prova sulle crittografie, almeno 10 candidati: conferma la soglia dei 20 minuti che la prova 1 dava come indicativa). Revisione: Opus (sforzo massimo).

**Rischi e segnali precoci.** *Crittografie generate di qualità insufficiente.* Segnale: mediana di revisione oltre i 20 minuti. Contromisura: l'anagramma a tema diventa il gioco del giorno, come prevede il piano commerciale. *Tempo d'autore sui rebus troppo alto.* Segnale: più di 45 minuti per un rebus. Contromisura: i rebus diventano un'uscita settimanale e non quotidiana.

**Dal proprietario (circa 13 h).** Proporre 2-3 idee di format; prova sulle crittografie; scegliere il gioco del giorno; scegliere la libreria di immagini per i rebus.

---

### Fase 8 - Sito pubblico e Admin

**Obiettivo.** Mettere online il Sito: gioco del giorno, ultimi 7 giorni, format della settimana, guide, newsletter, condivisione e stampa, accessibile per un pubblico sopra i 65 anni da telefono. Consegnare l'Admin con calendario, pubblicazione automatica, segnalazioni e dashboard essenziale. Il blocco di pubblicazione è attivo lato server.

**Perché qui.** Il Sito è l'ultima superficie per decisione del proprietario [P]. Arriva quando esistono Giochi originali approvati e un registro completo, che sono le condizioni per pubblicare. Non va online prima del parere legale (prova 0 del piano commerciale).

**Riuso, riscrittura, eliminazione.** Si riusano il Lettore (tutte e tre le rese), l'involucro Banco per l'Admin, la policy di pubblicazione della fase 0 e il profilo pubblico della fase 2.

**Risultati consegnati.**
- Profilo pubblico in produzione [A: server Hetzner come nel piano commerciale, Caddy per HTTPS, PostgreSQL proprio, backup notturni cifrati con ripristino provato].
- **Consegna Studio → Pubblico** firmata: si trasferisce solo un'*edizione* (istantanea JSON resa dal descrittore, impronte della soluzione, autore o pseudonimo, data). La policy è controllata in partenza e all'arrivo. Il database pubblico ha `CHECK (origine IN ('originale'))`, allargabile in futuro a licenza e pubblico dominio [P].
- **Sito** (proposta C, identità Carta con le regole di Lente): l'indirizzo principale apre il Lettore sul gioco del giorno; "Come si gioca" solo la prima volta; scelta tra griglia ed elenco; testo di base 20 px e comando "Grandezza del testo"; un solo pulsante principale per schermata; barra in basso con *Oggi* e *Archivio* (proposta A); ultimi 7 giorni; format della settimana; 5-10 guide alle tipologie; stampa A4 a caratteri grandi con le soluzioni su una pagina separata; condivisione del risultato senza svelare la soluzione; progressi salvati sul dispositivo; verifica anche offline.
- **Admin** (involucro Banco): viste *Da programmare*, *In calendario*, *Pubblicati*, *Ritirati*; calendario di almeno 14 giorni con pubblicazione automatica alle 7:00 (fuso di Roma); avviso se la coda copre meno di 7 giorni; *Riempi i giorni vuoti*; segnalazioni dei giocatori con correzione o ritiro registrati (lo stato torna allo Studio); dashboard con la metrica guida e le 5 metriche di supporto.
- **Misurazione senza cookie di profilazione** [A]: eventi anonimi (partita iniziata o completata, tempo, aiuti, abbandono, fonte del traffico). Il ritorno settimanale si calcola **sul dispositivo**: il Lettore invia solo il dato "giocato anche la settimana scorsa: sì o no", senza identificativi. Da confermare con il legale.
- **Nessuna dipendenza dall'abbonamento** [P]: il profilo pubblico non chiama alcun Fornitore AI e serve solo edizioni già approvate nello Studio.
- Newsletter con doppia conferma e cancellazione in un clic, tramite un fornitore europeo via API. Il Sito conserva solo lo stato dell'iscrizione.
- Privacy, termini d'uso e procedura di rimozione dal parere legale; marchio neutro e dominio.

**Criteri di accettazione.**
1. **Blocco di pubblicazione**: un test prova a pubblicare un Gioco estratto per ogni via (servizio dello Studio, API di consegna con firma valida, inserimento diretto con il ruolo applicativo del database pubblico) e ogni tentativo fallisce. Il test è obbligatorio in CI.
2. axe su tutte le pagine del Sito e del Lettore senza violazioni (WCAG 2.2 AA) in tema chiaro e scuro. Con zoom al 200% e larghezza 320 px nessuno scorrimento orizzontale (Playwright); aree di tocco di almeno 44x44 px (controllo automatico sulle dimensioni).
3. Lighthouse CI in emulazione mobile: accessibilità ≥ 95 e prestazioni ≥ 85 sulla pagina *Oggi*.
4. Playwright su emulazione iPhone SE e Android piccolo: dal link alla prima lettera con 1 tocco; partita completata; condivisione senza lettere della soluzione (controllo sul testo generato); stampa con le soluzioni su una pagina separata.
5. Un'edizione programmata viene pubblicata da sola all'ora prevista (test con orologio simulato). Con meno di 7 giorni coperti compare l'avviso nell'Admin.
6. Nessun cookie impostato senza consenso sulle pagine del Sito (ispezione in Playwright). Gli eventi non contengono IP né identificativi (test sullo schema degli eventi).
7. Ripristino del backup del database pubblico su un server vuoto in meno di 30 minuti (esercitazione documentata e ripetuta in CI con dati sintetici).
8. ZAP in modalità base senza avvisi alti contro l'ambiente di prova.
9. Avviato con `DBE_PROFILO=pubblico`, il contenitore dei servizi non registra alcun adattatore del Fornitore AI e l'immagine pubblica non richiede credenziali dell'abbonamento (test d'integrazione).

**Requisiti coperti.** Piano commerciale: RQ-03 (applicato), RQ-04, RQ-14, RQ-15, RQ-16, RQ-17, RQ-18, RQ-26, RQ-27, RQ-28 (sito), RQ-29 (essenziale), RQ-30; prova 2 (gruppo di prova), prova 3.

**CI aggiunta e artefatti.** Suite Playwright mobile, axe su tutte le pagine, Lighthouse CI con budget, test del blocco su tre livelli, ZAP, esercitazione di ripristino, **deploy del profilo pubblico solo da CI** con un ambiente protetto (approvazione del proprietario) e ritorno alla versione precedente in un comando. Artefatti: release `v1.0.0`, Sito in produzione.

**Ondate.**

| Ondata | Agenti | Contenuto |
|---|---|---|
| 8.1 | Opus A, Sonnet B, Sonnet C | A: modello delle edizioni, protocollo di consegna firmata, modello degli eventi senza identificativi. B: infrastruttura pubblica (server, Caddy, backup, deploy da CI). C: consegna e blocco su tre livelli |
| 8.2 | Sonnet A, Sonnet B, Sonnet C | A: percorso *Gioca oggi*, archivio di 7 giorni, format della settimana. B: Admin (viste, calendario, pubblicazione automatica, segnalazioni). C: misurazione e dashboard essenziale |
| 8.3 | Sonnet A, Sonnet B, Haiku C | A: stampa a caratteri grandi, condivisione, offline. B: newsletter. C: guide alle tipologie (testi da rivedere con il proprietario), pagine di privacy e termini dal parere legale |
| 8.4 | Sonnet A, Sonnet B | A: accessibilità (axe, zoom, Lighthouse) e correzioni. B: ZAP, esercitazione di ripristino |
| 8.5 | Proprietario e Sonnet A | Prova 2 con il gruppo di prova, correzioni, lancio |
| Revisione | Opus (sforzo massimo) | Firma della consegna, blocco di pubblicazione, privacy, sicurezza dell'Admin |

**Rischi e segnali precoci.**
- *Il pubblico over 65 non completa il gioco senza aiuto.* Segnale: meno dell'80% di completamento nel gruppo di prova. Contromisura: modalità elenco proposta per prima, guida iniziale più lunga.
- *Parere legale in ritardo.* Segnale: nessuna risposta entro il 30 aprile 2027. Contromisura: si costruisce tutto, ma il deploy pubblico resta fermo.
- *Coda sotto i 60 giochi al lancio.* Segnale: meno di 40 approvati al 1 agosto 2027. Contromisura: lancio con un gioco breve al giorno e il format ogni 2 settimane.

**Dal proprietario (circa 20 h più 15 h di contenuti).** Marchio neutro e dominio; parere legale ricevuto; privacy e termini; account del server e del fornitore di newsletter; gruppo di prova di 20-30 persone (prova 2); approvazione di 60 Giochi in coda; decisione sul lancio (**1 settembre 2027**).

---

### Fase 9 - Crescita commerciale

**Obiettivo.** Abilitare le linee di ricavo della seconda ondata: esportazione degli interni per KDP e per gli editori, Lettore incorporabile con chiave per cliente, aspetto del sito modificabile dall'Admin, dashboard completa e per cliente, gestione delle vendite e delle cessioni.

**Perché qui.** Servono un Sito online e un catalogo; l'ordine interno segue le date ridatate delle linee L1-L3 (sezione "Soglie").

**Risultati consegnati.**
- Esportazione degli interni di un libro KDP (margini, abbondanze, caratteri grandi, indice delle soluzioni) (RQ-33).
- Esportazione per editori: PDF vettoriale (A4 e formati rivista), ipuz per i cruciverba, JSON, testo delle definizioni, soluzioni in un file separato, dossier allegato (RQ-32).
- Lettore incorporabile (web component o iframe) con chiave per cliente, domini autorizzati, colori del cliente e contatore di partite per cliente (RQ-19, RQ-28 per canale).
- Aspetto del sito (colori, caratteri, pagine) modificabile dall'Admin senza un rilascio, entro limiti di contrasto verificati (RQ-31).
- Dashboard completa e per cliente (RQ-29); confronto tra difficoltà assegnata e dati di gioco (RQ-24, seconda parte).
- Gestione delle vendite: cessioni e licenze dallo Studio, blocco della seconda esclusiva, ritiro dal catalogo dei Giochi ceduti in esclusiva (RQ-02, seconda parte).
- I pagamenti (RQ-34) restano fuori fino all'attivazione di L4; si aggiunge allora una fase dedicata.

**Criteri di accettazione.** L'ipuz esportato passa la validazione dello schema ipuz. Il PDF KDP ha le dimensioni e le abbondanze previste (controllo automatico delle misure). Il Lettore incorporato in un dominio non autorizzato non si avvia, mentre su un dominio autorizzato conta le partite del cliente. Un cambio di colore che scende sotto un contrasto di 4,5:1 viene rifiutato. Una seconda esclusiva sullo stesso Gioco viene rifiutata dall'interfaccia, dall'API e dal database.

**Requisiti coperti.** Piano commerciale: RQ-02, RQ-19, RQ-24, RQ-28, RQ-29, RQ-31, RQ-32, RQ-33.

**CI aggiunta e artefatti.** Validazione ipuz, controlli sulle misure dei PDF, test d'incorporamento su un dominio di prova, controllo del contrasto dei temi. Artefatti: pacchetto del Lettore incorporabile versionato; release `v1.x`.

**Ondate.** 9.1: Sonnet A (KDP), Sonnet B (esportazioni per editori), Sonnet C (vendite e cessioni). 9.2: Opus A (sicurezza dell'incorporamento: chiavi, domini, limiti), Sonnet B (Lettore incorporabile), Sonnet C (aspetto modificabile). 9.3: Sonnet A (dashboard per cliente, confronto delle difficoltà), Haiku B (guida d'integrazione per i clienti). 9.4: Sonnet A (correzioni dal primo cliente). Revisione: Opus (sforzo massimo) a ogni ondata.

**Rischi e segnali precoci.** *Nessun cliente per il Lettore incorporabile.* Segnale: G3 a "cambia" o "fermati". Contromisura: si rimanda l'ondata 9.2 e si dà priorità a KDP e alle esportazioni.

**Dal proprietario (circa 9 h).** Ordine di priorità tra KDP e Lettore incorporabile; prove con il primo cliente; scelte di stile per i libri.

---

## 4. Matrice di tracciabilità dei requisiti di `PROMPT-INIZIALE.md`

Destino: numero di fase, oppure **Superato** con il motivo. Gli 84 requisiti hanno tutti un destino.

| ID | Destino | Nota |
|---|---|---|
| OBJ-01 | **Superato** | Era il ruolo chiesto all'assistente; sostituito dal modello di lavoro con agenti e modelli assegnati (sezione 1) |
| OBJ-02 | 0, 2 | Riformulato [P]: la console resta per i batch; il prodotto è web con due profili più l'involucro desktop |
| OBJ-03 | 1 | `scan-folder` in coda, nei limiti del Fornitore AI |
| OBJ-04 | 1 | **Superato in parte**: OCR e analisi del layout sostituiti dalla visione del Fornitore AI e dal testo nativo [P] |
| OBJ-05 | 0, 1 | |
| OBJ-06 | 1, 4, 5 | Tabelle dedicate generate dai descrittori, per tipologia in sequenza |
| OBJ-07 | 0 | |
| APP-01 | 0, 1 | |
| APP-02 | 1 | Proposto dal modello, confermabile |
| APP-03 | 1 | |
| APP-04 | 1, 3 | Dedotta dal modello in fase 1, confermata nello Studio in fase 3 |
| APP-05 | 1 | |
| APP-06 | 1 | Con opzione ricorsiva |
| APP-07 | **Superato** | OCR sostituito da immagine della pagina più testo nativo più visione (decisione 6); copertura funzionale in fase 1 |
| APP-08 | **Superato** | L'analisi del layout euristica è sostituita dalla segmentazione del modello; i riquadri arrivano dalla risposta strutturata (fase 1) |
| APP-09 | 1, 4 | Classificazione fatta dal modello; le tipologie sconosciute diventano proposte in fase 4 |
| APP-10 | 1 | |
| DB-01 | 0, 1 | |
| DB-02 | 0 | |
| DB-03 | 0 | Numero, anno, testata |
| DB-04 | 0 | |
| DB-05 | 1 | Immagine della pagina, testo nativo, trascrizione del modello |
| DB-06 | 0 | |
| DB-07 | 0 | Riformulato: la tipologia è un dato (tabella dei descrittori) e non un enum chiuso |
| DB-08 | 0 | |
| DB-09 | 1 | Riquadro in coordinate normalizzate da 0 a 1 rispetto alla pagina |
| DB-10 | 0, 1 | Confidenza, modello, costo, versione del descrittore |
| DB-11 | 1, 4, 5 | `LogicGames` diventa i descrittori di sudoku e giochi logici (fase 5) |
| DB-12 | 1, 4, 5 | |
| DYN-01 | 4 | La "nuova tipologia proposta" nasce in fase 1 e viene gestita in fase 4 |
| DYN-02 | 4 | Proposta di un descrittore, da cui deriva la struttura delle tabelle |
| DYN-03 | 4 | Nello Studio (procedura guidata) e in console |
| DYN-04 | 4 | |
| DYN-05 | 4 | Le tabelle si creano solo con un comando esplicito, dopo la conferma [P] |
| DYN-06 | 4 | Il file è diviso per gioco: una proposta per gioco |
| UX-01 | 1, 3 | Riepilogo in console (fase 1) e nello Studio (fase 3) |
| UX-02 | 4 | |
| UX-03 | 4 | |
| UX-04 | 4 | Rivalidazione dopo ogni modifica |
| CLI-01 | 0 | |
| CLI-02 | 1 | |
| CLI-03 | 1 | |
| CLI-04 | 5 | `dbe sql run` con prova, transazione e schema `registro` escluso |
| CLI-05 | 4 | |
| CLI-06 | 4 | |
| CLI-07 | 1, 5 | `eval`, `costs`, `reprocess`, `stats`, `backup`, `verify-integrity`, `export` |
| TEC-01 | 0 | |
| TEC-02 | 0 | .NET 10, System.CommandLine 2.0 stabile |
| TEC-03 | 0 | EF Core per il nucleo; Npgsql con parametri per le tabelle per tipologia |
| TEC-04 | **Superato** | PostgreSQL da subito (decisione 9); un prototipo SQLite non serve e la portabilità verso altri RDBMS non è un obiettivo |
| TEC-05 | 1 | PdfPig per il testo nativo |
| TEC-06 | 1 | Il rendering della pagina resta (PDFiumCore con `bblanchon.PDFium`, ADR 009); la parte OCR è **superata** |
| TEC-07 | 0 | Verificato dai test di architettura |
| TEC-08 | 1 | Adattatore per PDF e file |
| TEC-09 | **Superato** | Lo strato OCR è sostituito dallo strato di lettura della pagina (immagine più testo nativo), fase 1 |
| TEC-10 | 1 | Riformulato: un estrattore con visione fa segmentazione e classificazione |
| TEC-11 | 0 | |
| TEC-12 | 0, 2 | CLI e interfaccia web |
| AI-01 | 1 | Interfaccia unica verso il Fornitore AI; adattatore predefinito Claude Code non interattivo con l'abbonamento; adattatore API possibile, non predefinito |
| AI-02 | **Superato** | Il Fornitore AI non si sceglie dalle recensioni ma dalla misura sul Set di riferimento (fase 1): Claude con l'abbonamento come riferimento, piani gratuiti e modello aperto locale a confronto [P] |
| AI-03 | 4 | |
| AI-04 | 4 | |
| OUT-01 | **Superato** | Richiesta sul formato della risposta di un assistente; questa roadmap è organizzata in sezioni |
| OUT-02 | **Superato** | Le domande di chiarimento sono raccolte nella sezione "Domande aperte" |
| OUT-03 | 0 | ADR e test di architettura |
| OUT-04 | 1 | Pipeline reale e sezione 2.3 |
| OUT-05 | 0, 1 | |
| OUT-06 | 4 | Versioni dei descrittori e migrazioni generate |
| OUT-07 | 0 | |
| OUT-08 | 0 | |
| OUT-09 | 0 | |
| OUT-10 | 0 | |
| OUT-11 | 0, 1 | Interfacce di lettura della pagina, estrazione e persistenza; quelle di OCR e layout sono **superate** |
| OUT-12 | 4 | |
| OUT-13 | 4 | |
| OUT-14 | 1, 4 | Migrazioni generate e applicate con un comando esplicito invece di migrazioni dinamiche |
| OUT-15 | 0 | `Program.cs` reale e funzionante al posto dell'esempio |
| OUT-16 | 0 | |
| OUT-17 | 1 | |
| OUT-18 | 4 | |
| OUT-19 | **Superato** | Sostituito da un requisito più severo: codice funzionante, CI verde e criteri di accettazione verificati |
| OUT-20 | 1 | PDFiumCore con `bblanchon.PDFium` (ADR 009) e PdfPig; le librerie OCR sono **superate** |
| OUT-21 | **Superato** | La visione del modello sostituisce le librerie di layout (decisione 6) |
| OUT-22 | **Superato** | Assorbito dal metodo a fasi e ondate con collaudo del proprietario |

### 4.1 Requisiti di prodotto del piano commerciale (sezione 14) per fase

| RQ | Fase | RQ | Fase | RQ | Fase |
|---|---|---|---|---|---|
| RQ-01 | 0 | RQ-14 | 2 (base), 8 | RQ-27 | 8 |
| RQ-02 | 0 (campi, vincolo), 9 (vendite) | RQ-15 | 8 (base in 2) | RQ-28 | 8 (sito), 9 (per cliente) |
| RQ-03 | 0 (campo), 8 (applicato) | RQ-16 | 2 (base), 8 | RQ-29 | 8 (essenziale), 9 (completa) |
| RQ-04 | 0 (policy), 3, 8 | RQ-17 | 8 | RQ-30 | 8 |
| RQ-05 | 0, 2, 3 | RQ-18 | 8 | RQ-31 | 9 |
| RQ-06 | 6 (anticipo in 1b) | RQ-19 | 2 (architettura), 9 | RQ-32 | 9 |
| RQ-07 | 1 (chiamate di estrazione), 6 | RQ-20 | 1, 4 | RQ-33 | 9 |
| RQ-08 | 6 | RQ-21 | 4 (base), 7 | RQ-34 | Dopo la roadmap: fase dedicata quando si attiva L4 |
| RQ-09 | 6 | RQ-22 | 1 (strutturali), 6 | RQ-35 | 2 |
| RQ-10 | 6 (catalogo), 7 (libreria) | RQ-23 | 1 (estrazione), 6 | RQ-36 | 5 |
| RQ-11 | 6 | RQ-24 | 6, 9 | RQ-37 | 1-5: **anticipato per decisione del proprietario**. Resta valido il vincolo legale sull'uso dei giochi estratti nella creazione (interruttore della fase 6) |
| RQ-12 | 0 (infrastruttura), 6 | RQ-25 | 6, 8 | | |
| RQ-13 | 7 | RQ-26 | 8 | | |

---

## 5. Soglie commerciali ridatate

**Sono stime [S].** Ipotesi: 7 ore a settimana del proprietario, partenza il 16 novembre 2026, nessuna fase che sfora più del margine di una settimana. Metriche e soglie sono quelle della sezione 8.2 del piano commerciale [P]. Si tengono le date del piano commerciale dove sono raggiungibili; dove non lo sono, la tabella lo dice e propone la data realistica.

### 5.1 Perché alcune date non sono raggiungibili

- Il piano commerciale metteva il Sito online a inizio marzo 2027, dopo il solo Motore di creazione. Con l'ordine scelto [P], prima del Sito ci sono anche le fasi 1-5 (estrazione e Studio) e le fasi 6-7.
- Ore del proprietario necessarie dal 16 novembre 2026 al lancio: circa 129,5 di guida, prova e collaudo, 15 di contenuti per la coda iniziale e circa 25 di legale, marchio e prove con il gruppo, per un totale di circa 169,5 ore. A 7 ore a settimana sono almeno 25 settimane di solo lavoro del proprietario; con i margini di fase e le festività la data realistica resta il **1 settembre 2027**, usando circa una settimana del margine di agosto. Lanciare a inizio marzo 2027 richiederebbe circa 11 ore a settimana **e** saltare le fasi 1-5, cosa che contraddice la decisione 2.
- G1 invece **si tiene**: il prototipo della fase 1b non dipende dall'estrazione e la prova 1 (circa 14 ore con sei famiglie) entra nelle 7 ore di gennaio e febbraio (circa 3 ore a settimana), rallentando la fase 2. Il margine è di 1-2 ore; le contromisure sono nella fase 1b.

### 5.2 Come si dividono le 7 ore

| Periodo | Guida degli agenti e collaudo | Contenuti (prova 1, creazione, revisione) | Legale, marchio, mercato |
|---|---|---|---|
| 16 nov 2026 - 17 gen 2027 (fasi 0, 1, 1b) | 6 | 0 | 1 |
| 18 gen - 21 feb 2027 (prova 1, fasi 1 e 2) | 3 | 3 | 1 |
| 22 feb - 30 mag 2027 (fasi 3-6) | 6 | 0 | 1 |
| 31 mag - 31 ago 2027 (fasi 7-8) | 4 | 2,5 | 0,5 |
| set - nov 2027 (fase 9, sito online) | 2 | 3,5 | 1,5 |
| Dal 2028 | 1,5 | 4 | 1,5 |

### 5.3 Soglie

| Soglia | Data nel piano commerciale | **Data in questa roadmap** | Esito | Metrica e soglia "continua" (invariate) |
|---|---|---|---|---|
| **G1 Motore** | 28 feb 2027 | **28 feb 2027** | Tenuta (fase 1b), misura indicativa con 5 candidati per famiglia, confermata nelle fasi 6 e 7 | Gioco breve ≤ 20 min, cruciverba medio ≤ 60 min, almeno il 50% accettato. Se "cambia": nuova misura entro il 31 mar 2027, come nel piano |
| MVP pronto | fine feb 2027 | fine ago 2027 | **Non raggiungibile**: +6 mesi | 60 giochi in coda, Sito giocabile, prova over 65, registro completo |
| Sito online | inizio mar 2027 | **1 set 2027** | **Non raggiungibile**: +6 mesi | - |
| **G2 Pubblico** | 31 mag 2027 | **30 nov 2027** | **Non raggiungibile**: +6 mesi, perché servono 3 mesi di Sito online | Almeno 300 iscritti, 25% di ritorno, 25 raccolte vendute |
| **G3 Compratori** | 31 ago 2027 | **30 nov 2027** | **Non raggiungibile**: +3 mesi. Il campionario richiede il dossier (fase 7, fine giugno) e una demo pubblica (settembre) | Almeno 20 proposte, 3 risposte interessate, 1 vendita |
| **G4 Ricavo** | 30 nov 2027 | **31 mag 2028** | **Non raggiungibile**: +6 mesi. A fine novembre 2027 il Sito avrebbe 3 mesi di vita e nessuna linea di ricavo ricorrente | Almeno 200 € netti al mese e 1 compratore pagante |
| **G5 Crescita** | 31 mag 2028 | **30 nov 2028** (= 24 mesi) | **Non raggiungibile**: +6 mesi. Il piano assumeva 15 mesi di Sito online a quella data, qui sarebbero 9 | Almeno 450 € netti al mese |
| **G6 Esito** | 30 nov 2028 | **31 mag 2029** (30 mesi) | **Non raggiungibile**: +6 mesi. Il piano assumeva 21 mesi di mercato, qui sarebbero 15 | Almeno 800 € netti al mese; sotto i 400 € ci si ferma |

Se G1 è "fermati" il 28 febbraio 2027, si applica la regola del piano commerciale: niente più strategia commerciale. Le fasi 2-5 proseguono solo se il proprietario vuole lo Studio per uso personale; le fasi 6-9 si annullano.

### 5.4 Prove e linee di ricavo

| Elemento | Piano commerciale | Data in questa roadmap | Esito |
|---|---|---|---|
| Prova 0, parere legale | gen-feb 2027 | Domande inviate entro il **15 dic 2026**, risposta entro il **28 feb 2027** | Tenuta (anticipato l'invio) |
| Prova 1, costo di creazione | feb 2027 | 18 gen - 21 feb 2027 (fase 1b), 6 famiglie con 5 candidati ciascuna, senza spesa API | Tenuta |
| Prova 2, gradimento del formato | fine feb 2027 | lug-ago 2027 (fase 8) | Spostata, +5 mesi |
| Prova 3, abitudine | mar-mag 2027 | set-nov 2027 | Spostata, +6 mesi |
| Prova 4, raccolta PDF e questionario | apr-mag 2027 | ott-nov 2027 | Spostata, +6 mesi |
| Prova 5, compratori | prime 10 proposte a mag 2027, 20 ad ago 2027 | 10 entro il 30 set 2027, 20 entro il 31 ott 2027 | Spostata, +4 e +2 mesi |
| L1 prima vendita | entro ago 2027 | entro nov 2027 | Spostata, +3 mesi |
| L2 gioco del giorno in licenza | set 2027 | gen 2028 (dopo il Lettore incorporabile della fase 9) | Spostata, +4 mesi |
| L3 primo PDF | apr-mag 2027 | ott 2027 | Spostata, +6 mesi |
| L3 primo libro KDP | ott 2027 | metà nov 2027, in tempo per Natale 2027 (esportazione KDP come prima ondata della fase 9, 100 giochi approvati entro metà ottobre) | Spostata, +1 mese |
| L4 abbonamento | non prima di dic 2027 | non prima di giu 2028, con 1.000 iscritti e 300 giochi | Spostata, +6 mesi |
| L5 licenza per strutture | dopo mag 2028 | dopo nov 2028 | Spostata, +6 mesi |

**Effetto sull'obiettivo dei 24 mesi.** A 24 mesi (30 novembre 2028) la soglia realistica è 450 € netti al mese; gli 800 € arrivano a 30 mesi. Si può tenere G6 al 30 novembre 2028 solo abbassandone la soglia, perché a quella data il Sito avrebbe 15 mesi di mercato invece dei 21 previsti. Lo decide il proprietario a G2 (domanda aperta 5).

**Varianti di ore.** Con 5 ore a settimana G1 resta a fine febbraio solo se la prova 1 ha la precedenza su tutto: le fasi 1 e 2 slittano di circa 3 settimane e il Sito va online a inizio dicembre 2027. Con 10 ore il Sito va online a inizio luglio 2027 e G2 cade al 30 settembre 2027.

---

## 6. Cosa cambia rispetto al vecchio piano

| Tema | Vecchio piano (`docs/plan.md`) | Nuova roadmap |
|---|---|---|
| Stato dichiarato | 10 fasi "completate" | Nessuna fase completata; fase 0 conclusa il 19/11/2026 con l'approvazione del proprietario, con la CI verde su `main`. Lo stato si dimostra solo con la CI e i criteri di accettazione |
| Prodotto | Console application | Un prodotto web con due profili (studio e pubblico), involucro desktop e riga di comando per i batch |
| Lettura delle pagine | Tesseract più analisi del layout euristica più classificazione su testo | Immagine più testo nativo più una chiamata per pagina al Fornitore AI con visione e output strutturato; fornitore scelto per misura, senza API a consumo come scelta predefinita |
| Accuratezza | Mai misurata ("test con PDF reali da fare su richiesta") | Set di riferimento con risultati attesi, metriche e soglie; regressione locale prima di ogni release e Set sintetico in CI (ADR 008) |
| Schema per tipologia | DDL generato al volo da stringhe dell'AI (SQL injection) | Descrittori con lista chiusa di tipi; migrazioni generate, riviste e applicate con un comando esplicito |
| Dati estratti | Scartati dopo l'estrazione | Salvati nelle tabelle dedicate; la risposta grezza in una tabella cancellabile, i metadati nel registro (ADR 011) |
| Tipologie | Enum chiuso di 4 tipi (cruciverba, rebus, logica, anagramma) | Dato versionato; 4 tipologie di partenza in sequenza (cruciverba e brevi, crittografie, rebus); sudoku e giochi logici nel catalogo dalla fase 1b ed estratti in fase 5; altri giochi logici tramite il flusso delle tipologie nuove |
| Contenuti e diritti | Assenti | Origine obbligatoria, diritti, inedito, registro in sola aggiunta, blocco di pubblicazione su tre livelli |
| Creazione di giochi | Assente | Prototipo di misura subito (fase 1b, soglia G1 a febbraio 2027), poi Motore di creazione con validatori e registro del contributo umano (legge 132/2025); solo regole di formato approvate, esempi dalle Riviste solo dopo un parere legale favorevole |
| Tecnologia dell'interfaccia | Nessuna | React e TypeScript con API ASP.NET Core; Lettore anche come web component incorporabile; involucro desktop Tauri |
| Sito, Admin, Lettore | "Rimandati, oltre scope" | Fasi 2, 8 e 9, con requisiti di accessibilità misurabili |
| Soluzioni | Stub "completati" | Estrazione e collegamento in fase 4, verifica nel Lettore |
| Cache OCR | In memoria, inutile in una CLI | Idempotenza con impronte persistite |
| Test | 24 test, 15 rossi, solo database in memoria | Testcontainers PostgreSQL, test di architettura, test riprodotti, Playwright, axe, Lighthouse |
| CI e rilascio | Nessuna; repository senza commit | GitHub Actions obbligatorie, immagine su GHCR privato, release per fase, deploy pubblico solo da CI |
| Dimensione delle fasi | Strati tecnici orizzontali | Fasi verticali con un risultato usabile, ondate di 2-3 agenti, modelli assegnati |
| Ruolo del proprietario | Non previsto | Ore stimate e decisioni per fase; collaudo obbligatorio prima della chiusura |

---

## 7. Domande aperte

Decisioni del 13/11/2026 per la chiusura della fase 0, con `docs/fase-0-chiusura-decisioni.md`: protezione di `main` rimandata (piano GitHub), codice 5 per il registro alterato, PostgreSQL 18, modello predefinito `claude-sonnet-5-5` confermato, ADR 001-006 accettati, chiusura con `v0.1.1`. Decisioni già chiuse dal proprietario il 10/11/2026 e recepite: interfaccia React e TypeScript con API ASP.NET Core e involucro Tauri; prototipo di creazione confermato (fase 1b) e date del piano commerciale tenute dove possibile; Giochi estratti nella creazione solo dopo un parere legale favorevole, dietro interruttore. Chiuse nella stessa data con `docs/fase-0-decisioni.md`: lo Studio gira sul PC Windows del proprietario con Docker Desktop, solo sulla rete di casa (era la domanda 1); nessuna API a consumo come scelta predefinita, AI raggiunta tramite un'unica interfaccia verso il Fornitore AI; prova 1 con sei famiglie e 5 candidati ciascuna (era la domanda 2). Chiusa l'11/11/2026, con `docs/ondata-0-2-decisioni.md`: giochi logici della prova 1 confermati, cioè sudoku, futoshiki e nonogramma (era la domanda 12). I numeri delle domande restanti non cambiano. Decisioni del 19/11/2026 per la fase 1, con `docs/fase-1-decisioni.md`: Set di riferimento solo sul PC (ADR 008); rendering con PDFiumCore e `bblanchon.PDFium` (ADR 009); estrazione dal vivo su Windows con login OAuth, con il container rimandato alla fase 2 (ADR 010); risposta grezza in una tabella cancellabile (ADR 011); Archivio, ondate e rami e versioni (D1, D4, D6). Decisioni del 20/11/2026 per la fase 1b, con `docs/fase-1b-decisioni.md`: prototipo avviato con `dbe prototipo` su Windows (D1b-1); candidati solo nello schema `prototipo` (D1b-2); tutti i candidati validi, senza scelta a mano (D1b-3); schema di caselle nere dal programma (D1b-4); crittografie sinonimica e perifrastica (D1b-5); Opus per la creazione (D1b-6); brief in bozza scritti da un agente (D1b-7); numerazione `v0.2.0-prova.N` (D1b-8, che chiude la domanda q8 della fase 1); ADR 007 nella 1b.2 (D1b-9). Verifica delle condizioni d'uso entro il 6/12/2026. Nessuna domanda numerata si chiude per intero: la 13 resta aperta sulle condizioni d'uso.

| # | Domanda | Opzioni e raccomandazione | Entro | Blocca |
|---|---|---|---|---|
| 3 | **Cosa significa "giochi a chiave"?** | Interpretazione proposta: crittografie più cruciverba crittografato | 14 mar 2027 | Fase 4 |
| 4 | **"Nessun contatto esterno" esclude anche le proposte a freddo ai compratori?** | Interpretazione proposta: come nel piano commerciale, si parte senza contatti né partner e le proposte a freddo restano. Altrimenti G3 va riscritta e L1-L2 dipendono solo da richieste spontanee | Prima della fase 8 | G3 |
| 5 | **G6 a 30 mesi o soglia più bassa a 24 mesi?** Le date da G2 in poi non sono raggiungibili (sezione 5.3) | Raccomandato accettare G6 al 31 maggio 2029 con 800 €; in alternativa G6 al 30 novembre 2028 con 450 € | 30 nov 2027 (G2) | Soglie finali |
| 6 | Soglie di accuratezza dell'estrazione | Valori iniziali nella fase 1, da confermare dopo la linea di base | Fine fase 1 | Chiusura della fase 1 |
| 7 | Lessico italiano e sua licenza | Scelta dopo la ricerca dell'ondata 6.1 | Fase 6 | Validatori |
| 8 | Marca temporale: qualificata eIDAS o RFC 3161 semplice? | Dal parere legale (sezione 12 del piano commerciale) | Fase 6 | Approvazione nel registro |
| 9 | Immagini dei rebus creati: solo libreria con licenza, o anche generate dall'AI? | Raccomandata solo libreria finché il legale non si esprime | Fase 7 | Rebus creati |
| 10 | Fornitore della newsletter e del server pubblico | Fornitore europeo con doppia conferma; Hetzner come da piano commerciale | Fase 8 | Lancio |
| 11 | Domande ancora aperte del piano commerciale (sezione 16): soglia di reddito, cessione in esclusiva, posizione fiscale, firma o pseudonimo, idee di format, budget | Restano del proprietario; la posizione fiscale va chiarita prima della prima vendita (nov 2027) | Varie | Fasi 7-9 |
| 13 | **Condizioni d'uso dei Fornitori AI** | Il proprietario verifica: l'abbonamento Claude per l'uso non interattivo di Claude Code, anche in container; i piani gratuiti candidati (uso dei dati per addestramento, limiti di volume). L'invio di pagine di Riviste a piani gratuiti è anche tra le domande al legale | Abbonamento: 6 dic 2026; piani gratuiti: prima dell'ondata 1.4 (accanto alla 1b.3, dalla prova 1) | Fasi 1 e 1b; scelta del fornitore predefinito; container (fase 2, ADR 010) |
| 14 | **Set di PDF di prova** | Almeno 4 testate, con scansioni; idealmente 20-30 pagine di tipologie diverse | 7 dic 2026 | Fase 1 (Set di riferimento) |

---

## 8. Rischi trasversali

| # | Rischio | Segnale precoce | Contromisura |
|---|---|---|---|
| 1 | **Il tempo del proprietario non basta**: circa 129,5 ore di guida e collaudo prima del lancio, più contenuti e legale | Due fasi di fila oltre le ore stimate del 50%, oppure un mese sotto le 4 ore a settimana | Una sola sessione di decisione e una di collaudo per fase; si taglia prima l'ampiezza delle fasi 5, 7 e 9, mai la CI o il registro |
| 2 | **La misura di G1 sul prototipo non vale per il motore completo**: un cruciverba ottenuto da una sola richiesta all'AI può dare tempi peggiori di quello col risolutore, mentre un gioco breve senza validatori completi può darli migliori. In più la prova 1 e la fase 1 competono per le stesse ore tra gennaio e febbraio, e con 5 candidati per famiglia la misura è solo indicativa | Prototipo non pronto al 17 gennaio; più di 10 incroci incoerenti per griglia; quartili molto distanti; tempi delle fasi 6 e 7 peggiori di quelli della prova 1 | Priorità della fase 1b sulla calibrazione dell'estrazione; schema delle caselle nere generato dal programma (D1b-4); misure di conferma in fase 6 (anagramma, cruciverba) e 7 (crittografia) con decisione esplicita del proprietario |
| 3 | **Accuratezza o capacità dell'estrazione insufficienti sulle pagine reali** (scansioni, griglie, rebus) | Linea di base della fase 1 sotto le soglie dopo l'ondata di messa a punto, oppure troppe poche pagine per finestra dell'abbonamento | DPI più alti, seconda chiamata sul ritaglio, fornitore gratuito o locale dove basta; adattatore API solo se l'ostacolo è misurato e il proprietario accetta la spesa; in ultima istanza tutte le griglie delle scansioni passano per la verifica manuale |
| 4 | **Legale**: uso delle Riviste, invio delle pagine a Fornitori AI esterni, tutela dei giochi creati (anche di quelli generati da algoritmo), immagini dei rebus, privacy della misurazione | Domande non inviate entro il 15 dic 2026; parere non arrivato entro il 28 feb 2027 (ultima tolleranza il 30 apr 2027 per il lancio); un parere che boccia l'estrazione o l'invio delle pagine a un fornitore esterno | Domande inviate in fase 0; archivi separati e permessi del database; interruttori spenti di default; modello aperto locale come ripiego per l'estrazione; lancio subordinato al parere |
| 5 | **Dipendenza dal Fornitore AI** (condizioni dell'abbonamento, piani gratuiti ritirati o ridotti, modelli ritirati). Il modello predefinito è `claude-sonnet-5-5`, confermato il 13/11/2026 (`docs/fase-0-chiusura-decisioni.md`, D4) | Errori di modello inesistente, cambi delle condizioni d'uso, quota gratuita ridotta | Interfaccia unica e adattatori in configurazione, confronto dei fornitori ripetibile sul set, test riprodotti indipendenti dal fornitore |
| 6 | **Contesa dei limiti dell'abbonamento** tra sviluppo, creazione ed estrazione (il 10/11/2026 un agente si è già fermato per il limite delle 5 ore) | Ondate ferme per limite raggiunto; estrazioni o lotti di creazione interrotti per finestra esaurita | Estrazione e creazione pianificate in orari diversi dal lavoro degli agenti; misura delle pagine per finestra in fase 1; code che si fermano e riprendono da sole; adattatore API solo se l'ostacolo è misurato |
