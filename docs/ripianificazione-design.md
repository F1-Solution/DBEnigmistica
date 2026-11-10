# Ripianificazione DBEnigmistica: documento di sintesi del design

Sintesi di un'intervista di progettazione condotta con lo strumento grill-with-ui (sessione `C-Progetti-DBEnigmistica/20261110-110220`, creata il 2026-11-10). Il documento riporta ogni domanda, la risposta data dal proprietario, la raccomandazione che l'accompagnava e le ragioni delle scelte. Le 17 domande risultano tutte risposte: 9 sono decisioni durabili (difficili da invertire), 8 sono scelte ordinarie. Nessuna domanda è stata rimandata.

Le sigle `q1`-`q17` indicano le domande nell'ordine dell'intervista. "Raccomandazione" indica la proposta degli agenti di strategia e progettazione che accompagnava ogni domanda.

---

## 1. Sintesi

Il progetto DBEnigmistica, fermo da mesi, viene ripianificato da console application a prodotto con interfaccia, senza ripartire da zero. Il proprietario definisce tre superfici: lo **Studio**, privato, per acquisizione, revisione, ricerca e gioco, raggiungibile dai suoi dispositivi come container; il **Sito pubblico**, aperto a tutti, dove i Giochi si giocano a schermo; l'**Admin**, riservato al proprietario, per pubblicare nuovi Giochi, modificare l'aspetto del sito e vedere una dashboard di utilizzo. Lo Studio e il Sito pubblico condividono la stessa base web, installabile come app (PWA), con in più un involucro desktop per lo Studio. Il motore resta in C#/.NET; l'interfaccia è in React e TypeScript, l'involucro desktop è Tauri, e il Lettore (il componente che rende un Gioco giocabile) è distribuibile come componente incorporabile. Il contenuto pubblicato sono Giochi originali, creati o assistiti dall'AI e rivisti dal proprietario; i Giochi delle Riviste restano nell'archivio privato e possono essere aggiunti in futuro solo su licenza. L'uso dell'archivio come esempio per il Motore di creazione è subordinato a un parere legale favorevole. L'ordine di costruzione è: prima Motore di estrazione e Studio, poi Motore di creazione, Sito pubblico per ultimo. Sul piano commerciale il proprietario sceglie un reddito integrativo entro 24 mesi, con soglie fisse e l'impegno a fermare la parte commerciale se non vengono raggiunte. Le stime usano 10 ore a settimana; il proprietario indica 5-10 ore. Il repository va pubblicato su GitHub dopo aver spostato le password di sviluppo in variabili d'ambiente, e l'immagine Docker va su GitHub Container Registry in forma privata. Le decisioni sulla grafica sono la proposta A (combinazione Carta e Banco, con le regole di leggibilità di Lente) e la coda unica di lavoro per Studio e Admin.

---

## 2. Termini

Ogni termine ha la definizione concordata nell'intervista e l'elenco delle parole da evitare in documenti, codice e interfaccia.

- **Rivista**: Un singolo PDF che rappresenta un numero di una pubblicazione di enigmistica.
  - Da evitare: documento, file.
- **Gioco**: Un singolo enigma estratto da una pagina di una Rivista (cruciverba, rebus, anagramma, ecc.).
  - Da evitare: puzzle, quiz.
- **Schema dinamico**: La struttura delle tabelle specifiche di una tipologia di Gioco, proposta dall'AI e confermata dall'utente prima di essere creata.
  - Da evitare: migrazione automatica.
- **Studio**: L'applicazione privata, a tuo uso esclusivo, per acquisizione, ricerca e gioco.
  - Da evitare: app desktop, backoffice.
- **Sito pubblico**: La web UI aperta a tutti dove i Giochi si giocano a schermo.
  - Da evitare: portale, frontend.
- **Admin**: L'area riservata del Sito pubblico: pubblicazione di nuovi Giochi, modifica dell'aspetto del sito, dashboard di utilizzo.
  - Da evitare: pannello di controllo.
- **Lettore**: Il componente web che rende un Gioco giocabile a schermo (griglia compilabile, verifica della soluzione); serve allo Studio, al Sito pubblico e a un eventuale editore.
  - Da evitare: player, viewer.
- **Motore di creazione**: Il componente che genera Giochi originali (e nuovi format) con assistenza AI, da rivedere e approvare da una persona prima della pubblicazione.
  - Da evitare: generatore automatico.
- **Motore di estrazione**: Il componente che legge le Riviste in PDF e ne ricava i Giochi in forma strutturata.
  - Da evitare: OCR, scanner.

---

## 3. Perché

Il progetto nasce come console application: il prompt iniziale chiedeva una tabella dedicata per ogni tipologia di Gioco e un'acquisizione da PDF, senza alcuna interfaccia grafica. La nuova richiesta, espressa nell'intent, è: "Riprendere il progetto fermo da mesi senza ricominciare da zero: nuova valutazione, proposte UI/UX, fasi ripianificate, repo GitHub e test/pubblicazione tramite CI." Il cambio di forma del prodotto è il motivo per cui le domande sono state necessarie.

Le parole del proprietario erano ambigue su quattro punti, ed è da queste che nasce l'intervista.

- **Che cosa deve fare l'interfaccia (q1).** La domanda era il punto di partenza: il prodotto cambia forma, e da questa risposta dipendono schermate, stack frontend e quasi tutte le fasi. Nella risposta libera il proprietario descrive tre superfici e lo scopo commerciale del lavoro.
- **Chi la usa e dove gira (q2).** Il proprietario ha usato i termini "applicazione desktop" e "pubblicazione tramite CI" senza specificarne il significato. Il corpo della domanda osserva che l'applicazione desktop, a uso esclusivo, andava raggiunta anche da altri dispositivi come container; due scelte tecniche diverse. La risposta libera chiarisce lo Studio e aggiunge un Sito pubblico per tutti.
- **Come sono fatti i PDF (q3).** Il proprietario non sapeva ancora la proporzione tra PDF digitali e scansioni. La risposta è "Misto", con un set di prova in arrivo. Senza quel dato la strategia di lettura delle pagine non si poteva fissare, e il costo per pagina restava incerto.
- **Quante risorse esterne ci sono (q9).** La risposta alla domanda "su quali risorse esterne puoi contare" è "nessuna". Il corpo della domanda precisa che questa informazione "sposta di più il verdetto" commerciale. Nella stessa risposta il proprietario sottolinea l'importanza dei Giochi originali e dei format originali.

---

## 4. Decisioni vincolanti

Sono le nove domande con flag durabile. Per ciascuna sono riportate la decisione (testo completo dell'opzione scelta o della risposta libera), la coincidenza con la raccomandazione, le opzioni scartate con la ragione della raccomandazione o della discussione.

### 4.1 q1: A cosa deve servire l'interfaccia grafica?

- **Decisione**: risposta libera, nessuna delle quattro opzioni. Testo completo: "Tre superfici: (1) Studio privato per acquisizione, revisione, ricerca e gioco; (2) Sito pubblico aperto a tutti con i Giochi giocabili; (3) Admin del sito per acquisire/pubblicare nuovi Giochi, modificare la UI web e vedere una dashboard di utilizzo. Scopo commerciale: valutare fattibilità e strategia con Trend Researcher + Business Strategist, poi Pricing Analyst, poi Product Manager (e Strategy Duel Agent). Lo Studio potrebbe diventare pubblico in futuro, senza la parte di acquisizione."
- **Coincide con la raccomandazione?** No. La raccomandazione era A: banco di lavoro per l'acquisizione, senza gioco a schermo.
- **Opzioni non scelte e perché.**
  - A (banco per l'acquisizione più consultazione): la parte Studio della risposta ne mantiene il contenuto (acquisizione, revisione, ricerca), ma la risposta aggiunge le altre due superfici. La raccomandazione motivava A con la revisione pagina-a-fianco-del-dato, che resta nello Studio.
  - B (sola consultazione, acquisizione da riga di comando): non scelta; la risposta prevede l'acquisizione nello Studio.
  - C (come A, più i Giochi giocabili a schermo): il gioco a schermo compare nella risposta, nel Sito pubblico. La raccomandazione lo rinviava, perché "è un prodotto a parte" da fare "finché i dati non sono affidabili". Il proprietario non ha accolto il rinvio.
  - D (nessuna UI web o desktop, una TUI): non scelta; la risposta prevede interfacce web.

### 4.2 q2: Chi lo usa e dove gira?

- **Decisione**: risposta libera. Testo completo: "Studio solo per me, raggiungibile dai miei dispositivi, come container; immagine su GitHub Container Registry purché privata. Sito pubblico per tutti, con Admin solo per me."
- **Coincide con la raccomandazione?** In parte. Per lo Studio la risposta coincide con B (container raggiungibile dai dispositivi, immagine su GitHub Container Registry). La risposta aggiunge due elementi che le opzioni non prevedevano: un Sito pubblico aperto a tutti e un Admin riservato al proprietario.
- **Opzioni non scelte e perché.**
  - A (solo tu, sul tuo PC, pacchetto o immagine in locale): non scelta, perché la risposta richiede accesso dai dispositivi del proprietario.
  - C (più utenti via internet, hosting cloud, login, deploy automatico): non scelta come opzione unica. La raccomandazione indicava che, se servissero più utenti, si sarebbe aggiunta l'autenticazione senza rifare la pipeline. Il thread dice che l'hosting del Sito pubblico si decide dopo la valutazione commerciale (vedi sezione 9).
- **Note.** Il thread registra che il registry può essere privato: un'immagine pubblicata da un repository privato resta visibile solo a chi ha accesso all'organizzazione F1-Solution. La quota di spazio del piano dell'organizzazione è da verificare.

### 4.3 q4: Quali Giochi finiscono sul Sito pubblico?

- **Decisione**: opzione C. Testo completo: "Misto: originali da subito, contenuti su licenza quando e se arrivano accordi; più eventuali opere di pubblico dominio."
- **Coincide con la raccomandazione?** Sì.
- **Opzioni non scelte e perché.**
  - A (solo Giochi originali, archivio delle Riviste privato): non scelta. La raccomandazione preferisce C perché le licenze si possono aggiungere dopo senza cambiare architettura, purché ogni Gioco porti con sé origine e diritti. A non prevede contenuti su licenza né opere di pubblico dominio.
  - B (solo Giochi delle Riviste su licenza): non scelta. La raccomandazione osserva che gli originali permettono di partire "senza dipendere da nessuno"; B dipende da un accordo con l'editore o l'autore.
  - D (Giochi estratti pubblicati così come sono): non scelta. La raccomandazione lo sconsiglia: espone a contestazioni per violazione del diritto d'autore e "può far chiudere il sito".
- **Conseguenza indicata.** Serve una fase per la creazione di Giochi originali (editor e generazione assistita). Il corpo della domanda ricorda che archiviare i Giochi delle Riviste per uso personale nello Studio è cosa diversa dal ripubblicarli su un sito commerciale.

### 4.4 q5: Com'è fatto lo Studio rispetto al Sito pubblico?

- **Decisione**: opzione B. Testo completo: "Come A, più un involucro desktop (finestra nativa, avvio dal menu Start, accesso ai file locali) attorno alla stessa interfaccia." La parte "Come A" è: "Un solo prodotto web con due profili: lo Studio è la stessa applicazione avviata in privato con acquisizione e revisione attive; il Sito pubblico è la stessa base senza acquisizione, più l'Admin. Installabile come app (PWA) sui tuoi dispositivi."
- **Coincide con la raccomandazione?** No. La raccomandazione era A, cioè lo stesso prodotto web con due profili, senza involucro desktop.
- **Opzioni non scelte e perché.**
  - A (solo PWA, senza involucro desktop): non scelta. La raccomandazione motivava A con il fatto che il gioco a schermo, scritto una volta e usato sia nello Studio sia nel Sito pubblico, "dimezza il lavoro" e rende semplice aprire lo Studio senza acquisizione. Il suo unico costo dichiarato era "rinuncia all'integrazione nativa con Windows", e la raccomandazione osservava che l'involucro di B "si può aggiungere dopo senza rifare nulla". Il proprietario ha scelto di averlo fin da subito.
  - C (Studio come applicazione desktop nativa Windows separata, es. Avalonia/WPF, e Sito pubblico come progetto web distinto): non scelta. La raccomandazione osservava che "raddoppia interfacce, test e CI".
- **Nota.** La risposta non dice se l'involucro desktop è indicato come Tauri. La decisione sul framework dell'interfaccia (sezione 4.9) indica Tauri per l'involucro desktop.

### 4.5 q6: Come si memorizzano i dati specifici di ogni tipologia di Gioco?

- **Decisione**: opzione B. Testo completo: "Tabelle dedicate per tipologia come nel prompt iniziale, ma generate come migrazioni da rivedere e applicare con un comando esplicito (niente DDL eseguito al volo)."
- **Coincide con la raccomandazione?** No. La raccomandazione era A: tipologia come schema JSON versionato, approvato dal proprietario, con i dati del Gioco in una colonna JSONB validata contro lo schema, e viste SQL per tipologia quando servono.
- **Stato del problema.** Il corpo della domanda riporta che il meccanismo attuale è esposto a SQL injection e non registra le tabelle che crea; `docs/giochi_enigmistici.md` descrive circa 30 tipologie.
- **Opzioni non scelte e perché.**
  - A (schema JSON in JSONB): non scelta. La raccomandazione lo preferisce perché lo schema serve a tre scopi con un solo artefatto (vincolare l'output dell'AI, validare i dati in ingresso, descrivere al frontend cosa disegnare) ed "elimina alla radice il rischio di SQL generato a runtime". Il costo dichiarato è perdere le colonne tipizzate in SQL puro, recuperabili con viste, e il legame con PostgreSQL. Il proprietario non ha dato una motivazione esplicita; il thread riporta che la scelta "resta fedele al prompt iniziale ed elimina il DDL eseguito al volo".
  - C (tabelle create a runtime, con il SQL generato reso sicuro): non scelta. La scelta del proprietario elimina il DDL eseguito al volo, quindi C non è stata accolta.
- **Conseguenza indicata dal thread.** Ogni tipologia avrà una descrizione strutturata dei suoi campi, perché serve all'AI per l'output vincolato e al Lettore per sapere cosa disegnare. La migrazione verrà generata da quella descrizione.

### 4.6 q7: Qual è l'obiettivo commerciale?

- **Decisione**: opzione B. Testo completo: "Un reddito integrativo entro 24 mesi: accetto di fissare soglie e di fermare la parte commerciale se non vengono raggiunte."
- **Coincide con la raccomandazione?** Sì.
- **Opzioni non scelte e perché.**
  - A (progetto personale che si ripaga le spese, "prima di tutto lo voglio per me"): non scelta. La raccomandazione osserva che A "va benissimo se la priorità è il piacere di costruirlo", ma allora il piano commerciale "si riduce a poco".
  - C (prodotto da cedere o dare in licenza a un editore): non scelta. La raccomandazione osserva che C "dipende da terzi che oggi non conosci".
- **Ragione della scelta indicata dalla raccomandazione.** Con soglie fisse al mese 4 e al mese 6 il proprietario scopre presto e con poca spesa (circa 80 ore e al massimo 200 €) se la strada regge; in caso contrario resta comunque con lo Studio e il Lettore per uso personale.
- **Contesto.** Il corpo della domanda riporta che gli agenti divergono: la ricerca di mercato vede una nicchia per un sito rivolto ai giocatori (scenario base 25-50 mila € l'anno a 24 mesi, stima); lo stratega raccomanda un servizio per editori; il duello strategico fa vincere una via ibrida, ma solo con soglie di uscita fisse.

### 4.7 q10: In che ordine si costruisce?

- **Decisione**: opzione B. Testo completo: "Prima il Motore di estrazione e lo Studio, come nel prompt iniziale: l'archivio delle Riviste fa da base di esempi per il Motore di creazione, che viene dopo, e il Sito pubblico per ultimo."
- **Coincide con la raccomandazione?** No. La raccomandazione era A: "Prima il Lettore e il Motore di creazione su poche tipologie, con il Sito pubblico in versione vetrina; poi lo Studio completo; il Motore di estrazione per ultimo."
- **Argomenti della raccomandazione scartata.** Il Lettore serve a qualunque strada, e il Motore di creazione è ciò che il proprietario vuole vendere. Metterli per primi dà in poche settimane qualcosa da mostrare e da cronometrare (quanto costa davvero rivedere un Gioco generato), che la raccomandazione indica come la prova decisiva. Il costo dichiarato era che per i primi mesi le Riviste non entrano nel sistema. La raccomandazione diceva che B "rimanda di mesi la verifica commerciale".
- **Opzione C non scelta.** "Una fetta sottile di tutto: una sola tipologia portata da PDF a Studio a Lettore a Sito pubblico, poi si allarga." La raccomandazione la scartava perché, con 5-10 ore a settimana, una fetta completa richiede troppo prima di vedere un risultato.
- **Conseguenze registrate nel thread.** Il piano commerciale (docs/business/05) metteva prima la creazione e fissava a febbraio 2027 la prova sul tempo di revisione. Con questo ordine quelle date slittano della durata delle fasi di estrazione, e le soglie commerciali "vanno ridatate". Usare l'archivio come base di esempi per il Motore di creazione è il punto su cui gli agenti chiedono un parere legale.

### 4.8 q15: Con quale tecnologia si costruisce l'interfaccia?

- **Decisione**: opzione A. Testo completo: "React e TypeScript per l'interfaccia, API ASP.NET Core per il motore; Lettore distribuibile anche come componente incorporabile; involucro desktop con Tauri."
- **Coincide con la raccomandazione?** Sì.
- **Opzioni non scelte e perché.**
  - B (Blazor, tutto in C#, con involucro MAUI Blazor Hybrid o Photino): non scelta. La raccomandazione osserva che il Lettore deve partire in fretta su telefoni non recenti, funzionare senza rete e potersi incorporare in siti di terzi, requisiti in cui Blazor "scarica un runtime pesante o richiede una connessione sempre attiva". B resta la scelta giusta "se per te conta poter leggere e modificare tutto in C#".
  - C (pagine generate dal server con Razor, JavaScript solo nel Lettore): non scelta. La raccomandazione non la discute esplicitamente.
- **Contesto.** Il motore resta in C#/.NET: dominio, EF Core e repository si tengono. I cinque mockup sono già HTML e JavaScript e si riusano (vedi sezione 6). Il costo dichiarato è avere due linguaggi nel progetto; lo sviluppo è delegato agli agenti, che lavorano bene in entrambi.

### 4.9 q17: Il Motore di creazione può usare i Giochi estratti dalle Riviste?

- **Decisione**: opzione B. Testo completo: "Anche come esempi, ma solo dopo un parere legale favorevole: fino ad allora vale A, e il sistema tiene separati i due archivi." Il regime provvisorio A è: "Solo come conoscenza di formato: dalle Riviste si ricavano le regole delle tipologie (struttura, lunghezze, livelli di difficoltà), scritte e approvate da me; il Motore di creazione non riceve mai testi o schemi dei Giochi estratti."
- **Coincide con la raccomandazione?** Sì.
- **Opzioni non scelte e perché.**
  - A: non scartata, ma vale come regime provvisorio fino al parere legale. La raccomandazione la indica come partenza sicura, perché le regole di formato "sono idee e non opere protette".
  - C (esempi da subito): non scelta. La raccomandazione osserva che espone "proprio i Giochi che vuoi vendere a una contestazione sulla loro origine".
- **Conseguenze indicate.** Il parere costa "qualche centinaio di euro" ed è già nel budget del piano commerciale (dato della raccomandazione). In ogni caso la separazione tra archivio delle Riviste e catalogo pubblicabile va costruita. Lo stato del parere (richiesto o no) non risulta da state.json.

---

## 5. Scelte ordinarie

Sono le otto domande con flag non durabile. Ciascuna ha un punto di sintesi.

- **q3 (quantità e forma dei PDF).** Risposta libera: "Misto: la maggior parte PDF digitali nativi, alcuni da scansionare. Un set di PDF di prova arriverà appena possibile." Coincide con la raccomandazione D (misto, da verificare). Il thread prevede che per i PDF nativi si usi il testo già presente più l'immagine della pagina, e per le scansioni la sola visione; il set di prova (idealmente 20-30 pagine con tipologie diverse) serve prima della fase di estrazione e non blocca la pianificazione.
- **q8 (tempo settimanale per 12 mesi).** Risposta: B, "5-10 ore a settimana". La raccomandazione non indicava una lettera: chiedeva la fascia reale. Il corpo ricorda che tutte le stime assumono 10 ore a settimana e che con 5 i tempi raddoppiano; con meno di 5 ore il duello strategico conclude che nessuna strategia regge come impresa.
- **q9 (risorse esterne).** Risposta libera: "Nessuna risorsa esterna: parto da solo, con le riviste che possiedo. Se esiste un motore che crea giochi, valuterò di collaborare con editori, case editrici o siti che remunerano gli autori. Dare grande importanza alla creazione di giochi originali e anche di format originali per nuovi tipi di giochi logici o enigmistici." Coincide con la raccomandazione D, l'ipotesi prudente finora usata. Il thread però ne trae che il Motore di creazione diventa il prodotto principale e il Motore di estrazione il fornitore di esempi e di stile (uso interno, nulla ripubblicato), e apre una quarta linea di ricavo, vendere giochi originali come autore, passata al Product Manager (vedi sezione 9).
- **q11 (tipologie di partenza).** Risposta: tutte e quattro le opzioni. A: "Cruciverba classico (griglia con definizioni)."; B: "Crittografia e giochi brevi in versi o a chiave (testo corto, soluzione a parole)."; C: "Anagramma, sciarada, cambio di lettera e simili (giochi di parole brevi)."; D: "Rebus (richiede illustrazioni originali)." La raccomandazione indicava A e C; quella per il rebus diceva di "metterli nel secondo giro". Il thread li sequenzia dentro ogni componente (prima cruciverba e giochi brevi, poi crittografie, poi rebus con le sue immagini).
- **q12 (primo commit e push).** Risposta: opzione B. Testo completo: "Prima sposto le password di sviluppo in variabili d'ambiente, poi commit e push." Coincide con la raccomandazione. La raccomandazione diceva che una password finita nella cronologia git "ci resta anche dopo averla tolta"; il rischio della opzione A (commit subito) era basso, perché sono credenziali di un database locale. Il corpo indica le posizioni: `docker-compose.yml` righe 9 e 26, `EnigmisticaDbContextFactory.cs` riga 17. Dopo il push la CI può partire.
- **q13 (proposta di UI).** Risposta: opzione A. Testo completo: "Combinazione: identità di Carta (pagina di giornale, righe e colonne, rosso tipografico su avorio) con le regole di leggibilità di Lente per Sito pubblico e Lettore; Banco (strumento di lavoro denso, tabelle, tre colonne) per Studio e Admin." Coincide con la raccomandazione. La raccomandazione osservava che il Sito pubblico e gli strumenti di lavoro hanno pubblici opposti; il costo è mantenere due registri visivi su una base comune di componenti. B (solo Lente) era la scelta "più sicura" se si preferisce un solo stile.
- **q14 (proposta di UX).** Risposta: opzione A. Testo completo: "Il Banco per Studio e Admin (un'unica coda di elementi con stato e azione successiva, revisione quasi tutta da tastiera), con il Sito pubblico e la procedura guidata per lo Schema dinamico presi da I Percorsi e le schede con indirizzo proprio prese da L'Archivio." Coincide con la raccomandazione. Stime dell'agente per una settimana tipo (7 Giochi creati e 1 Rivista rivista): Banco circa 2 h 20, Archivio circa 4 h, Percorsi circa 3 h 10; sono stime, non misure. Rischio dichiarato: la coda unica può confondersi quando Giochi creati e Giochi estratti si mescolano, da gestire con filtri.
- **q16 (soglie commerciali).** Risposta: opzione B. Testo completo: "Stesse date del piano commerciale: accanto all'estrazione si costruisce subito un prototipo minimo del Motore di creazione solo per misurare il tempo di revisione." Coincide con la raccomandazione. Un prototipo minimo (una richiesta all'AI, una schermata di revisione con cronometro, due tipologie) serve a dire presto se la parte commerciale regge. L'opzione A (date spostate in avanti) non è stata scelta: la raccomandazione la diceva più lineare ma con un primo segnale dal mercato spostato di molti mesi. Vedi sezione 9 per il rapporto con q10.

---

## 6. Fatti verificati

Sono ricavati dalle descrizioni nei corpi delle domande, nelle opzioni e nelle discussioni, non da una domanda diretta.

**Codice esistente**
- Il codice esistente è uno scheletro: il modello dati e i repository si tengono; l'estrazione e lo Schema dinamico vanno riscritti (corpo di q10).
- Il codice attuale per l'estrazione usa Tesseract e un'analisi del layout euristica, con un modello Claude del 2024. I modelli attuali leggono direttamente le pagine come immagini, griglie comprese (corpo di q3).
- Il meccanismo di creazione delle tabelle per tipologia è oggi esposto a SQL injection e non registra le tabelle che crea (corpo di q6).
- La cronologia git locale è descritta come assente, con origin collegato al repository privato e vuoto `F1-Solution/DBEnigmistica` (corpo di q12).
- Nei file tracciabili ci sono password di sviluppo locali in chiaro, in `docker-compose.yml` (righe 9 e 26) e in `EnigmisticaDbContextFactory.cs` (riga 17). La chiave Anthropic vera è in un file ignorato da git (corpo di q12).
- PostgreSQL è già in `docker-compose` (rationale di q2).
- Il motore è in C#/.NET e il dominio, EF Core e i repository si tengono (corpo e rationale di q15).

**Design**
- Esistono cinque mockup in `docs/design/ui/` (apri `index.html`). Ognuno usa lo stesso cruciverba e lo stesso anagramma, cambia solo l'interfaccia, e mostra Sito pubblico, Lettore, Studio e Admin, in chiaro e scuro. Il confronto completo è in `PROPOSTE-UI.md` (corpo di q13).
- Esistono tre modelli di organizzazione UX in `docs/design/ux/`, ciascuno con wireframe navigabile (corpo di q14).
- I mockup sono già HTML e JavaScript e si riusano nel nuovo progetto (rationale di q15).

**Archivio e dati**
- `docs/giochi_enigmistici.md` descrive circa trenta tipologie di Gioco. Ogni tipologia costa un Lettore dedicato, una tabella, un prompt di creazione e regole di verifica (corpo di q11 e q6).
- Gli agenti indicano come spazio libero l'enigmistica classica italiana, non il cruciverba generico (corpo di q11).

**Piano commerciale e stime**
- Il piano commerciale (docs/business/05) metteva prima la creazione e fissava a febbraio 2027 la prova sul tempo di revisione (thread di q10).
- Le soglie del piano sono: febbraio 2027, revisione di un Gioco breve entro 20 minuti; maggio 2027, 300 iscritti; agosto 2027, prima vendita; poi 200, 450 e 800 € netti al mese a novembre 2027, maggio 2028 e novembre 2028. Sotto 400 € a novembre 2028 la parte commerciale si ferma (corpo di q16).
- Lo scenario di mercato base per un sito rivolto ai giocatori è 25-50 mila € l'anno a 24 mesi, stima (corpo di q7).
- Tutte le stime degli agenti assumono 10 ore a settimana; con 5 i tempi raddoppiano (corpo di q8).
- Le soglie di uscita dell'obiettivo scelto sono al mese 4 e al mese 6, con una spesa stimata di circa 80 ore e al massimo 200 € (rationale di q7).
- Il parere legale sull'uso dell'archivio come esempi costa "qualche centinaio di euro" ed è già nel budget del piano commerciale (rationale di q17).
- Gli agenti di strategia coinvolti sono Trend Researcher, Business Strategist, Pricing Analyst, Product Manager e Strategy Duel Agent (risposta libera di q1).

**Registro e hosting**
- Un'immagine Docker pubblicata da un repository privato nasce privata e resta visibile solo a chi ha accesso all'organizzazione F1-Solution (thread di q2).
- Il Sito pubblico richiede un hosting raggiungibile da internet (thread di q2).

---

## 7. Rischi

- **Qualità dell'estrazione senza revisione (q1).** Senza una schermata di revisione pagina-a-fianco-del-dato gli errori restano nel database e nessuno li vede.
- **Fase frontend vera (q1).** Il costo dell'interfaccia è una fase frontend vera; il gioco a schermo è "un prodotto a parte", che la raccomandazione consigliava di rimandare.
- **Accesso da fuori rete (q2).** L'accesso alla rete domestica da fuori va protetto a parte, con VPN o tunnel.
- **Quota del registry (q2).** La quota di spazio del piano dell'organizzazione è da verificare.
- **Contestazioni sul diritto d'autore (q4).** Pubblicare Giochi estratti dalle Riviste espone a contestazioni e può far chiudere il sito.
- **Duplicazione di interfacce (q5).** Lo Studio come applicazione desktop separata raddoppia interfacce, test e CI. Scegliendo l'involucro desktop si rinuncia all'integrazione nativa con Windows nella forma di A, ma l'involucro è di B.
- **Perdita di colonne tipizzate (q6).** Con le tabelle dedicate, la comodità di interrogare colonne tipizzate in SQL puro è mantenuta, ma l'approccio scartato (schema JSON con viste) avrebbe recuperato le viste e legato il sistema a PostgreSQL. Il rischio di SQL generato a runtime è eliminato dal divieto di DDL al volo.
- **Costo di mantenere due linguaggi (q15).** Il progetto ha due linguaggi, TypeScript e C#, con il costo che ne deriva.
- **Ritardo della verifica commerciale (q10, q16).** Con l'ordine scelto, la verifica commerciale slitta della durata delle fasi di estrazione; le date del piano commerciale vanno ridatate.
- **Confusione nella coda unica (q14).** La coda unica può diventare confusa quando Giochi creati e Giochi estratti si mescolano; va gestita con filtri.
- **Due registri visivi (q13).** Mantenere due registri visivi (Carta e Banco) su una base comune costa di più di uno solo.
- **Rebus (q11).** Il rebus richiede la produzione di immagini, un problema a sé. La crittografia richiede revisione esperta.
- **Password nella cronologia git (q12).** Una password finita nella cronologia resta anche dopo la rimozione.
- **Origine dei Giochi (q17).** Usare i Giochi estratti come esempi per il Motore di creazione espone i Giochi che si vogliono vendere a una contestazione sulla loro origine, da cui la scelta di aspettare il parere.
- **Costo di produzione dei Giochi originali (q9, q14).** La prova decisiva è cronometrare quanto costa davvero produrre e rivedere un Gioco originale.
- **Scelta della coda unica (q14).** Costa più di B da realizzare.

---

## 8. Rimandati

Nessuna domanda è stata rimandata. Tutte e 17 hanno stato `answered`.

---

## 9. Discussioni aperte

Sono i punti emersi nelle discussioni o nelle note finali senza una decisione.

1. **Hosting del Sito pubblico.** Non è stato deciso. Il thread di q2 dice che lo si decide dopo la valutazione commerciale.
2. **Come si protegge l'Admin.** La risposta di q2 non dice come l'accesso all'Admin è protetto. La raccomandazione di q2 aveva indicato l'autenticazione come aggiunta futura.
3. **Date commerciali.** Il thread di q10 dice che le soglie commerciali "vanno ridatate". La risposta di q16 sceglie invece di mantenere le stesse date, con un prototipo minimo del Motore di creazione. Il rapporto tra i due punti non è chiarito in state.json.
4. **Parere legale sugli esempi.** Il parere sull'uso dell'archivio come base di esempi per il Motore di creazione è subordinato a q17. state.json non dice se è già stato richiesto.
5. **Baricentro sul Motore di creazione.** Il thread di q9 indica il Motore di creazione come prodotto principale e il Motore di estrazione come fornitore di esempi e di stile. La risposta di q9 non seleziona alcuna risorsa esterna. Il punto non è stato confermato dal proprietario.
6. **Quarta linea di ricavo.** Vendere giochi originali come autore è stato passato al Product Manager. Non è deciso.
7. **Collaborazioni future.** La risposta di q9 apre a collaborazioni con editori, case editrici o siti che remunerano gli autori, "se esiste un motore che crea giochi". Non è deciso.
8. **Format originali.** La risposta di q9 dà importanza alla creazione di format originali per nuovi tipi di giochi logici o enigmistici. Nessun format è stato scelto.
9. **Studio pubblico in futuro.** La risposta di q1 dice che lo Studio potrebbe diventare pubblico senza la parte di acquisizione. Non è deciso.
10. **Ore per editori e autori.** La raccomandazione di q8 chiedeva quante delle ore disponibili servono per parlare con editori o autori invece che sul prodotto. La risposta indica solo la fascia 5-10 ore.
11. **Quantità del set di prova.** Il set di PDF di prova (idealmente 20-30 pagine con tipologie diverse) "arriverà appena possibile" (q3). Non c'è una data.
12. **Dettagli commerciali.** Restano aperte le domande di dettaglio commerciale lasciate dal Product Manager: esclusiva o licenza, partita IVA, nome d'autore, budget. Non bloccano le fasi e restano elencate come aperte in ROADMAP.md e nel piano commerciale (nota finale della sessione).
