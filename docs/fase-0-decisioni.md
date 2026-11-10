# Decisioni per la fase 0 e la fase 1b

Intervista del 10 novembre 2026, condotta con la pagina grill-with-ui. Raccoglie le sei decisioni del proprietario che servono per chiudere la fase 0 (Fondamenta) e avviare la fase 1b (Prototipo di revisione della creazione) di `ROADMAP.md`. In sintesi: lo Studio gira sul PC del proprietario; non si usano API a pagamento, ma l'abbonamento Claude già attivo per la creazione e, per l'estrazione, il Fornitore AI che supera la soglia di accuratezza al costo minore tra quelli misurati sullo stesso set di pagine; la Prova 1 copre sei famiglie di Giochi con cinque candidati ciascuna.

## Termini

- **Prova 1**: la misura a cronometro del tempo di revisione di Giochi generati dal prototipo del Motore di creazione, da concludere entro il 28 febbraio 2027. Da evitare: test, benchmark.
- **Studio**: l'applicazione privata, a uso esclusivo del proprietario, per acquisizione, revisione, creazione, ricerca e gioco. Da evitare: app desktop, backoffice.
- **Fornitore AI**: il servizio che esegue una chiamata di estrazione o di creazione; il programma lo raggiunge attraverso un'unica interfaccia, così si può cambiare senza toccare il resto. Da evitare: modello, provider.

## Perché

Il proprietario ha chiesto di avviare la fase 0 con l'obiettivo "CI verde su main" e di chiarire tre punti: il gioco breve della prima prova, dove gira lo Studio, il tetto di spesa AI. Sul terzo punto ha risposto con parole sue: "Ho intenzione di usare i piani di abbonamento già inclusi nei piani che ho pagato per la creazione, mentre per l'estrazione vorrei usare piani free anche di altri provider". Sul primo ha allargato il perimetro: "Crittografia breve, Anagramma a tema, Rebus, Cruciverba, Sudoku, Giochi logici vari a tua scelta". Entrambe le risposte cambiano ciò che `ROADMAP.md` prevedeva (chiamate API a consumo con Claude per tutto; due tipologie nella Prova 1), quindi sono state seguite da tre domande di conferma.

## Decisioni vincolanti

### Dove gira lo Studio (Q2)

**Decisione: sul PC Windows del proprietario, con Docker Desktop**, acceso quando lavora e raggiungibile dagli altri dispositivi solo sulla rete di casa. Coincide con la raccomandazione.

Opzioni scartate:
- *Mini server o NAS di casa sempre acceso, raggiungibile da fuori tramite rete privata*: dà un servizio sempre disponibile, ma è un costo e un apparato in più che con 5-10 ore a settimana non serve.
- *Server privato a noleggio (VPS europeo)*: metterebbe le Riviste su macchine di terzi, la posizione meno prudente in attesa del parere legale.

Passare più avanti a una delle due significa spostare container e database, senza cambiare codice. Le pagine vengono comunque inviate a un Fornitore AI per la lettura: questo non dipende da dove gira lo Studio.

### Come chiama l'AI il Motore di creazione (Q4)

**Decisione: tramite Claude Code in modalità non interattiva, con l'abbonamento del proprietario.** Nessun costo aggiuntivo; limiti condivisi con lo sviluppo; solo per lo Studio privato. Coincide con la raccomandazione.

Opzioni scartate:
- *API a consumo con chiave e tetto mensile basso*: indipendente dai limiti dell'abbonamento, ma è una spesa che il proprietario non vuole.
- *Abbonamento per il lavoro interattivo e API solo per i lotti notturni*: resta la via d'uscita se i limiti diventano un ostacolo misurato; l'interfaccia unica verso il Fornitore AI la rende un cambio di configurazione.

### Quale Fornitore AI legge le pagine delle Riviste (Q5)

**Decisione: si parte con Claude tramite l'abbonamento per avere una misura di riferimento, e si provano sullo stesso set di pagine i piani gratuiti di altri fornitori e un modello aperto eseguito sul PC.** Diventa predefinito quello che supera la soglia di accuratezza al costo minore, tra quelli le cui condizioni d'uso il proprietario ha verificato. Coincide con la raccomandazione.

Opzioni scartate come scelta unica:
- *Solo piani gratuiti di altri fornitori*: costo zero, ma limiti di volume e possibilità che le pagine vengano usate per addestramento; qualità su cruciverba e crittografie italiane non nota.
- *Solo Claude con l'abbonamento*: qualità nota, ma consuma gli stessi limiti dello sviluppo.
- *Solo un modello aperto in locale*: le pagine non escono da casa, ma richiede una scheda grafica adeguata e la qualità su griglie e rebus è incerta.

## Scelte ordinarie

- **Tipologie della Prova 1 (Q1)**: crittografia breve, anagramma a tema, rebus, cruciverba, sudoku e giochi logici a scelta dell'agente.
- **Tetto di spesa AI (Q3)**: nessuna spesa API. Abbonamenti già pagati per la creazione; piani gratuiti, anche di altri fornitori, per l'estrazione. Precisata dalle decisioni Q4 e Q5.
- **Scaglionamento delle sei famiglie (Q6)**: tutte e sei nel prototipo della Prova 1, con 5 candidati ciascuna. Il proprietario ha scelto diversamente dalla raccomandazione, che proponeva tre passi (prima crittografia, anagramma e cruciverba con 10 candidati ciascuno; poi sudoku e giochi logici; il rebus per ultimo) per avere campioni più grandi sulle tipologie a parole.

## Fatti verificati

- Un abbonamento Claude copre l'uso tramite le applicazioni di Anthropic e Claude Code, non le chiamate API fatte da un programma.- I limiti dell'abbonamento sono gli stessi che consuma lo sviluppo del progetto: il 10 novembre 2026 un agente si è fermato per il limite delle 5 ore.
- Sudoku e giochi logici a griglia si generano con un algoritmo e si verificano automaticamente (soluzione unica garantita dal programma): non richiedono AI e il tempo di revisione è quasi nullo.
- La fase 1b in `ROADMAP.md` stimava circa 19 ore del proprietario con due sole tipologie.

## Rischi

- **Claude Code non interattivo (ipotesi da provare all'inizio della fase 1)**: lo Studio, girando solo sul PC del proprietario, potrebbe far lavorare Claude Code in modalità non interattiva con l'abbonamento; non vale per il Sito pubblico.
- **Contesa dei limiti dell'abbonamento** tra sviluppo, creazione ed estrazione. Contromisura: generare ed estrarre in orari diversi dal lavoro degli agenti; passare alle API a consumo solo se l'ostacolo viene misurato.
- **Condizioni d'uso**: quelle del piano Claude per l'uso non interattivo e quelle dei piani gratuiti di altri fornitori (uso dei dati per addestramento) non sono state verificate dall'agente. Spetta al proprietario verificarle; per le pagine di Riviste protette l'invio a piani gratuiti è la posizione meno prudente e va incluso nelle domande al legale.
- **Misura indicativa**: con 5 candidati per tipologia il tempo di revisione misurato nella Prova 1 è indicativo e non conclusivo. Per crittografia, anagramma e cruciverba la soglia dei 20 minuti va confermata con la misura ripetuta prevista nella fase 6.
- **Prototipo più largo**: sei descrizioni di tipologia, un generatore algoritmico per sudoku e giochi logici, immagini segnaposto per il rebus. Le ore del proprietario per la fase 1b salgono rispetto alle 19 stimate.
- **Qualità dell'estrazione con fornitori gratuiti o locali**: ignota finché non viene misurata sul set di riferimento.

## Rimandati

Nessuna domanda è stata rimandata.

## Discussioni aperte

- **Quali giochi logici oltre al sudoku**: l'agente sceglierà uno o due formati a griglia con soluzione unica verificabile tra quelli di `docs/giochi_enigmistici.md` e li proporrà al proprietario quando si pianifica la fase 1b.
- **Immagini dei rebus**: nella Prova 1 si usano immagini segnaposto descritte a parole. La produzione delle immagini vere resta legata al parere legale.
- **Aggiornamento di `ROADMAP.md`**: il piano va allineato a queste decisioni (Fornitore AI dietro un'unica interfaccia, niente API a consumo come scelta predefinita, sei famiglie nella fase 1b, ore del proprietario ricalcolate).
- **Set di PDF di prova**: resta da fornire, serve prima della fase 1.
