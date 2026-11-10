# Prompt iniziale di DBEnigmistica

Il testo proviene da `docs/prompt_iniziale` (senza estensione, 143 righe), il prompt con cui il progetto è stato avviato.

Questo documento serve come riferimento per la ripianificazione del progetto: la sezione "Requisiti estratti" elenca ogni richiesta con il rimando alle righe originali.

## Prompt originale

````markdown
Agisci come un senior software engineer specializzato in C#, OCR, analisi del layout di documenti e progettazione di database relazionali.

### OBIETTIVO GENERALE
Devi aiutarmi ad analizzare, progettare e sviluppare una **console application C# (.NET)** che:
1. Scansioni in batch molti file PDF, dove **ogni PDF rappresenta una rivista di enigmistica**.
2. Usi **OCR** e analisi del layout per identificare e estrarre i **giochi enigmistici** presenti nella rivista (es: cruciverba, giochi di logica, rebus, parole crociate, ecc.).
3. Salvi i dati estratti in un **database relazionale**, con:
   - **tabelle specifiche per ogni tipologia di gioco** (con struttura diversa a seconda del gioco),
   - **tabelle per i metadati** delle riviste e dei giochi.

### CONTESTO E COMPORTAMENTO DELL’APPLICAZIONE

- Ogni **PDF = una rivista**.
- Ogni rivista ha:
  - **Titolo** (es. "La Settimana Enigmistica"),
  - **Data di uscita**,
  - **Periodicità** (mensile o settimanale, se possibile da dedurre o specificata nei dati di input),
  - **Lista di pagine** con uno o più giochi su ciascuna.
- L’applicazione deve:
  1. **Caricare i PDF** da una cartella.
  2. Applicare **OCR** alle pagine (puoi assumere l’uso di una libreria OCR come Tesseract, o suggerirne altre compatibili con .NET).
  3. Analizzare il **layout della pagina** per individuare i singoli giochi (titolo del gioco, eventuale didascalia, griglia, testo delle definizioni, ecc.).
  4. **Classificare il tipo di gioco** (es. cruciverba, anagrammi, logica, ecc.), anche in maniera euristica.
  5. **Salvare i dati nel database**.

### REQUISITI DI DATABASE

- Il database deve contenere almeno:

  1. **Tabella RIVISTA** (es. `Magazines`)
     - Id
     - Titolo
     - DataPubblicazione
     - Periodicita (es. "Settimanale", "Mensile")
     - Eventuali altri metadati utili (es. numero, anno, ecc.)

  2. **Tabella PAGINA** (es. `Pages`)
     - Id
     - MagazineId (FK verso `Magazines`)
     - NumeroPagina
     - Eventuali metadati (es. immagine della pagina, testo OCR grezzo)

  3. **Tabella GIOCO GENERALE** (es. `Games`)
     - Id
     - PageId (FK verso `Pages`)
     - TipoGioco (string o enum: es. "Cruciverba", "Rebus", "Logica", ecc.)
     - TitoloGioco (se presente)
     - PosizioneNelLayout (bounding box o coordinate approssimative, opzionale)
     - Altri metadati generali.

- Per **ogni tipologia di gioco**, voglio avere una **tabella specifica dedicata**, ad esempio:
  - `CrosswordGames`
  - `LogicGames`
  - `RebusGames`
  - ecc.

  Queste tabelle avranno **colonne diverse** a seconda del tipo (es. per i cruciverba: numero di righe/colonne, definizioni orizzontali/verticali; per i rebus: testo del rebus, soluzione codificata, ecc.).

- Requisito importante:
  - Quando l’applicazione incontra **un nuovo tipo di gioco non ancora modellato**, deve:
    1. Proporre una **nuova struttura di tabella** (o più tabelle) per rappresentarlo.
    2. Mostrare questa proposta all’utente (in console) in modo leggibile.
    3. Permettere all’utente di **accettare o modificare** la proposta (es. aggiungere/rinominare colonne).
    4. Solo dopo la conferma, creare/aggiornare le tabelle nel database e salvare il gioco.

- Nel file giochi_enigmistici.md trovi le descrizioni di alcuni giochi e alcuni link utili. Usa le definizioni per far creare ad un agente AI le tabelle di database relative

### ASPETTI INTERATTIVI

- L’applicazione è una **console app** che deve:
  - Mostrare all’utente un riepilogo dei giochi appena riconosciuti in una rivista (tipo, posizione, pagina, ecc.).
  - Per ogni **nuovo tipo di gioco**:
    - Mostrare una proposta di modello di tabella (nome tabella, colonne, tipi).
    - Chiedere conferma o modifiche.
  - Gestire con cura input dell’utente (sì/no, modifica nome colonna, tipo colonna, ecc.).
  - L'applicazione deve accettare parametri da riga di comando che gli consentano di:
    - Scansionare un'intera cartella di PDF
    - Scansionare un singolo file
    - Accettare script da inserire nel database
    - Accettaere file .md di istruzioni di un gioco in particolare da far analizzare ad un agente AI per creare le tabelle di database corrispondenti
    - Accettare un stringa con la descrizione di un gioco da sottoporre ad u agente AI
    - altri comandi che ti vengono in mente e che possano essere utili

### REQUISITI TECNICI

- Linguaggio: **C#**.
- Tipo di progetto: **Console Application .NET (versione recente, es. .NET 10 con System.CommandLine)**.
- Accesso al DB: preferibilmente tramite **Entity Framework Core**, ma puoi proporre una soluzione alternativa se più adatta.
- Database: puoi proporre una soluzione semplice (es. **SQLite** per prototipo) ma con design facilmente migrabile verso SQL Server o altri RDBMS.
- OCR: suggerisci librerie compatibili con .NET per:
  - estrarre testo da PDF,
  - se necessario, convertire PDF in immagini pagina per pagina e poi applicare OCR.
- Agente AI: l'applicazione deve contenere le API per chiamare un agente AI (scegli tu il migliore secondo le recensioni che trovi online per poter creare tabelle di database)
- Architettura:
  - Separare bene:
    - strato di accesso al file system / PDF,
    - strato OCR,
    - strato di analisi del layout e classificazione dei giochi,
    - strato di persistenza dati,
    - strato di interazione con l’utente.

### COSA MI ASPETTO DALLA TUA RISPOSTA

Organizza la risposta in più sezioni chiare:

1. **Domande di chiarimento**: 
   - Prima chiedimi eventuali dettagli mancanti (ad es. se ho preferenze su librerie OCR, DB, o tipologie di giochi principali da supportare).

2. **Progettazione ad alto livello**:
   - Descrivi l’architettura proposta (moduli, responsabilità, pattern principali).
   - Descrivi il workflow principale: dal caricamento dei PDF, all’OCR, al riconoscimento giochi, fino al salvataggio nel DB.

3. **Schema del database**:
   - Proponi le tabelle iniziali (Magazines, Pages, Games, e almeno 2–3 tabelle specifiche per tipologia di gioco come esempio).
   - Spiega come progettare la parte di **evoluzione dinamica dello schema** quando emergono nuovi tipi di gioco.
   - Descrivi come memorizzare i metadati di:
     - pagina (numero, riferimento rivista),
     - rivista (titolo, data uscita, periodicità),
     - collegamento gioco ↔ pagina ↔ rivista.

4. **Design del modello ad oggetti in C#**:
   - Proponi le classi C# corrispondenti (magari con un approccio domain-driven semplificato).
   - Indica eventuali interfacce per astrarre OCR, analisi layout e persistenza.

5. **Flow di interazione con l’utente**:
   - Spiega nel dettaglio come gestire il caso in cui viene trovato un **nuovo tipo di gioco**:
     - come generare automaticamente una proposta di tabella,
     - come mostrarla in console,
     - come recepire le modifiche dell’utente,
     - come applicare le modifiche al DB (migrazioni dinamiche, o altra strategia).

6. **Esempi di codice C#**:
   - Fornisci esempi di:
     - Program.cs con il main flow dell’applicazione.
     - Classi di dominio principali (Magazine, Page, Game, ecc.).
     - Una classe di servizio per il processamento di un PDF.
     - Una classe di servizio per la gestione della proposta/creazione dinamica di tabelle.
   - Il codice non deve essere perfetto o completo al 100%, ma deve dare una base solida e compilabile su cui posso continuare a iterare.

7. **Suggerimenti su librerie e strumenti**:
   - Elenca librerie OCR/PDF consigliate per .NET.
   - Eventuali librerie per l’analisi del layout o per aiutare nella classificazione delle aree della pagina (anche solo come idee/approcci).

Segui un approccio iterativo: prima dammi il design e lo schema, poi passiamo al codice passo per passo.
````

## Requisiti estratti

Le righe indicate sono numeri di riga di `docs/prompt_iniziale`. Il tag "(opzionale)" segna le richieste non obbligatorie o condizionate dal testo.

### Obiettivo generale (OBJ)

| ID | Requisito | Righe origine |
|---|---|---|
| OBJ-01 | Agire come senior software engineer esperto in C#, OCR, analisi del layout di documenti e progettazione di database relazionali | 1 |
| OBJ-02 | Analizzare, progettare e sviluppare una console application C# (.NET) | 4 |
| OBJ-03 | Scansionare in batch molti file PDF, dove ogni PDF è una rivista di enigmistica | 5 |
| OBJ-04 | Usare OCR e analisi del layout per identificare ed estrarre i giochi enigmistici presenti nella rivista | 6 |
| OBJ-05 | Salvare i dati estratti in un database relazionale | 7 |
| OBJ-06 | Prevedere tabelle specifiche per ogni tipologia di gioco, con struttura diversa a seconda del gioco | 7-8 |
| OBJ-07 | Prevedere tabelle per i metadati delle riviste e dei giochi | 9 |

### Comportamento dell'applicazione (APP)

| ID | Requisito | Righe origine |
|---|---|---|
| APP-01 | Ogni PDF rappresenta una rivista | 13 |
| APP-02 | Ogni rivista ha un titolo (es. "La Settimana Enigmistica") | 15 |
| APP-03 | Ogni rivista ha una data di uscita | 16 |
| APP-04 | Ogni rivista ha una periodicità (mensile o settimanale), se possibile dedotta dai dati o specificata nei dati di input (opzionale) | 17 |
| APP-05 | Ogni rivista ha una lista di pagine, ciascuna con uno o più giochi | 18 |
| APP-06 | L'applicazione carica i PDF da una cartella | 20 |
| APP-07 | L'applicazione applica OCR alle pagine | 21 |
| APP-08 | L'applicazione analizza il layout della pagina per individuare i singoli giochi (titolo, eventuale didascalia, griglia, testo delle definizioni, ecc.) | 22 |
| APP-09 | L'applicazione classifica il tipo di gioco (es. cruciverba, anagrammi, logica), anche in maniera euristica | 23 |
| APP-10 | L'applicazione salva i dati nel database | 24 |

### Database (DB)

| ID | Requisito | Righe origine |
|---|---|---|
| DB-01 | Il database contiene almeno la tabella riviste, la tabella pagine, la tabella giochi generale e le tabelle specifiche per tipologia | 28-30 |
| DB-02 | Tabella riviste (es. `Magazines`) con Id, Titolo, DataPubblicazione e Periodicita (es. "Settimanale", "Mensile") | 30-34 |
| DB-03 | Tabella riviste con altri metadati utili, es. numero e anno (opzionale) | 35 |
| DB-04 | Tabella pagine (es. `Pages`) con Id, MagazineId (FK verso Magazines) e NumeroPagina | 37-40 |
| DB-05 | Tabella pagine con altri metadati, es. immagine della pagina o testo OCR grezzo (opzionale) | 41 |
| DB-06 | Tabella giochi generale (es. `Games`) con Id e PageId (FK verso Pages) | 43-45 |
| DB-07 | Tabella giochi con TipoGioco come stringa o enum (es. "Cruciverba", "Rebus", "Logica") | 46 |
| DB-08 | Tabella giochi con TitoloGioco, se presente | 47 |
| DB-09 | Tabella giochi con PosizioneNelLayout come bounding box o coordinate approssimative (opzionale) | 48 |
| DB-10 | Tabella giochi con altri metadati generali | 49 |
| DB-11 | Per ogni tipologia di gioco, una tabella specifica dedicata (es. `CrosswordGames`, `LogicGames`, `RebusGames`) | 51-55 |
| DB-12 | Le tabelle specifiche hanno colonne diverse a seconda del tipo (es. cruciverbi: righe/colonne, definizioni orizzontali/verticali; rebus: testo, soluzione codificata) | 57 |

### Schema dinamico per nuovi tipi di gioco (DYN)

| ID | Requisito | Righe origine |
|---|---|---|
| DYN-01 | Riconoscere quando si incontra un tipo di gioco non ancora modellato | 60 |
| DYN-02 | Proporre una nuova struttura di tabella (o più tabelle) per rappresentare il nuovo tipo | 61 |
| DYN-03 | Mostrare la proposta all'utente in console in modo leggibile | 62 |
| DYN-04 | Permettere all'utente di accettare o modificare la proposta (es. aggiungere o rinominare colonne) | 63 |
| DYN-05 | Creare o aggiornare le tabelle nel database e salvare il gioco solo dopo la conferma dell'utente | 64 |
| DYN-06 | Usare il file `giochi_enigmistici.md` (descrizioni di alcuni giochi e link utili) per far creare a un agente AI le tabelle di database corrispondenti | 66 |

### Interazione con l'utente (UX)

| ID | Requisito | Righe origine |
|---|---|---|
| UX-01 | Mostrare all'utente un riepilogo dei giochi appena riconosciuti in una rivista (tipo, posizione, pagina, ecc.) | 70-71 |
| UX-02 | Per ogni nuovo tipo di gioco, mostrare una proposta di modello di tabella (nome tabella, colonne, tipi) | 72-73 |
| UX-03 | Per ogni nuovo tipo di gioco, chiedere conferma o modifiche | 73-74 |
| UX-04 | Gestire con cura l'input dell'utente (sì/no, modifica nome colonna, tipo colonna, ecc.) | 75 |

### Comandi da riga di comando (CLI)

| ID | Requisito | Righe origine |
|---|---|---|
| CLI-01 | L'applicazione accetta parametri da riga di comando | 76 |
| CLI-02 | Parametro per scansionare un'intera cartella di PDF | 77 |
| CLI-03 | Parametro per scansionare un singolo file | 78 |
| CLI-04 | Parametro per accettare script da inserire nel database | 79 |
| CLI-05 | Parametro per accettare un file `.md` di istruzioni di un gioco da far analizzare a un agente AI | 80 |
| CLI-06 | Parametro per accettare una stringa con la descrizione di un gioco da sottoporre a un agente AI | 81 |
| CLI-07 | Altri comandi utili che vengono in mente (opzionale, richiesta aperta) | 82 |

### Requisiti tecnici e architettura (TEC)

| ID | Requisito | Righe origine |
|---|---|---|
| TEC-01 | Linguaggio C# | 86 |
| TEC-02 | Tipo di progetto: console application .NET di versione recente (es. .NET 10) con System.CommandLine | 87 |
| TEC-03 | Accesso al database preferibilmente tramite Entity Framework Core, oppure una alternativa più adatta | 88 |
| TEC-04 | Database: soluzione semplice per il prototipo (es. SQLite) con design facilmente migrabile verso SQL Server o altri RDBMS | 89 |
| TEC-05 | Suggerire librerie compatibili con .NET per estrarre testo dai PDF | 90-91 |
| TEC-06 | Se necessario, convertire i PDF in immagini pagina per pagina e applicare OCR, con librerie .NET suggerite (opzionale) | 91-92 |
| TEC-07 | Separare l'architettura in strati distinti | 94-95 |
| TEC-08 | Strato di accesso al file system e ai PDF | 96 |
| TEC-09 | Strato OCR | 97 |
| TEC-10 | Strato di analisi del layout e classificazione dei giochi | 98 |
| TEC-11 | Strato di persistenza dei dati | 99 |
| TEC-12 | Strato di interazione con l'utente | 100 |

### Agente AI (AI)

| ID | Requisito | Righe origine |
|---|---|---|
| AI-01 | L'applicazione contiene le API per chiamare un agente AI | 93 |
| AI-02 | Scegliere l'agente AI migliore in base alle recensioni trovate online, per creare tabelle di database | 93 |
| AI-03 | L'agente AI analizza il file `.md` di istruzioni di un gioco e crea le tabelle di database corrispondenti | 80 |
| AI-04 | L'agente AI crea tabelle di database a partire dalla descrizione testuale di un gioco | 81 |

### Consegne attese dalla risposta (OUT)

| ID | Requisito | Righe origine |
|---|---|---|
| OUT-01 | Organizzare la risposta in più sezioni chiare | 104 |
| OUT-02 | Porre per prime le domande di chiarimento su eventuali dettagli mancanti (es. preferenze su librerie OCR, DB, tipologie di giochi principali) | 106-107 |
| OUT-03 | Descrivere l'architettura proposta (moduli, responsabilità, pattern principali) | 109-110 |
| OUT-04 | Descrivere il workflow principale: caricamento dei PDF, OCR, riconoscimento dei giochi, salvataggio nel DB | 111 |
| OUT-05 | Proporre le tabelle iniziali (Magazines, Pages, Games) e almeno 2-3 tabelle specifiche per tipologia come esempio | 113-114 |
| OUT-06 | Spiegare come progettare l'evoluzione dinamica dello schema quando emergono nuovi tipi di gioco | 115 |
| OUT-07 | Descrivere come memorizzare i metadati di pagina (numero, riferimento alla rivista) | 117 |
| OUT-08 | Descrivere come memorizzare i metadati di rivista (titolo, data di uscita, periodicità) | 118 |
| OUT-09 | Descrivere come memorizzare il collegamento gioco, pagina e rivista | 119 |
| OUT-10 | Proporre le classi C# corrispondenti, con un approccio domain-driven semplificato | 121-122 |
| OUT-11 | Indicare le interfacce per astrarre OCR, analisi del layout e persistenza | 123 |
| OUT-12 | Spiegare come generare automaticamente una proposta di tabella per un nuovo tipo di gioco | 126-127 |
| OUT-13 | Spiegare come mostrare la proposta in console e come recepire le modifiche dell'utente | 128-129 |
| OUT-14 | Spiegare come applicare le modifiche al DB (migrazioni dinamiche o altra strategia) | 130 |
| OUT-15 | Fornire esempio di `Program.cs` con il main flow dell'applicazione | 134 |
| OUT-16 | Fornire esempi delle classi di dominio principali (Magazine, Page, Game, ecc.) | 135 |
| OUT-17 | Fornire esempio di classe di servizio per il processamento di un PDF | 136 |
| OUT-18 | Fornire esempio di classe di servizio per la gestione della proposta e della creazione dinamica delle tabelle | 137 |
| OUT-19 | Il codice non deve essere perfetto né completo al 100%, ma deve essere una base solida e compilabile su cui iterare | 138 |
| OUT-20 | Elencare le librerie OCR/PDF consigliate per .NET | 141 |
| OUT-21 | Elencare librerie o approcci per l'analisi del layout o la classificazione delle aree della pagina (anche solo come idee) | 142 |
| OUT-22 | Seguire un approccio iterativo: prima design e schema, poi codice passo per passo | 143 |

## Punti ambigui o lasciati aperti

- **Libreria OCR e percorso PDF** (righe 21, 90-92): nessuna libreria è scelta (Tesseract è solo un esempio). Non è chiaro se il percorso principale sia l'estrazione diretta del testo dal PDF o l'OCR sulle immagini di pagina.
- **Database** (righe 88-89): EF Core è "preferibile" e SQLite è indicato come "es." per il prototipo. Il motore definitivo non è scelto.
- **Fornitore dell'agente AI** (riga 93): il criterio è "le recensioni online", senza indicare fornitore, modello, costi, limiti o trattamento dei dati inviati.
- **Periodicità** (righe 17, 34): non è detto come dedurla (dal titolo, dalla data, dal numero) né se il valore debba essere sempre specificato in input.
- **Data di uscita** (righe 16, 33): la granularità (giorno, mese, numero di uscita) non è definita.
- **Classificazione euristica e "nuovo tipo"** (righe 23, 60): non sono definiti le regole, le soglie di confidenza né il criterio con cui si decide che un gioco non è ancora modellato.
- **Posizione nel layout** (riga 48): l'unità (pixel, punti PDF, percentuale) e il sistema di coordinate non sono specificati.
- **Campi "altri metadati"** (righe 35, 41, 49): sono indicati solo come "eventuali" o "altri", senza un elenco.
- **Script da inserire nel database** (riga 79): non è chiaro se si tratti di script SQL, di migrazioni o di codice C#, né come vadano eseguiti.
- **Evoluzione dello schema** (riga 130): "migrazioni dinamiche o altra strategia" lascia la scelta aperta.
- **Percorso di `giochi_enigmistici.md`** (righe 66, 80): il testo non indica il percorso del file.
- **Versione .NET e System.CommandLine** (riga 87): sono indicati come esempi ("es."), non come vincoli.
- **Ordine delle consegne** (righe 106 e 109-143): si chiedono prima le domande di chiarimento e nello stesso prompt design e codice. Il testo finale (riga 143) suggerisce un approccio iterativo, ma non dice se le domande debbano bloccare il resto.
- **Lingua dei nomi di tabelle e colonne** (righe 30, 37, 43): il prompt usa sia nomi italiani (RIVISTA, PAGINA, GIOCO GENERALE) sia inglesi (Magazines, Pages, Games) senza fissare la scelta.
- **Consegna dei rebus e dei tipi** (riga 57): gli esempi di colonne sono solo per cruciverba e rebus; per gli altri tipi (logica, anagrammi) la struttura non è indicata.
