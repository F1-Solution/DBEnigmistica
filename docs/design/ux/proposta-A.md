# Proposta A - "Il Banco" (una sola coda di lavoro)

Wireframe: `wireframe-A.html`. Convenzioni comuni (passaggio, stime, Gioco di riferimento) in `PROPOSTE-UX.md`.

## 1. Idea di fondo

1. Nello Studio tutto è un elemento di una sola coda: un Gioco da generare, da rivedere o da verificare, uno Schema dinamico da confermare, una Rivista in estrazione, un errore dell'AI.
2. Ogni elemento ha uno stato e una sola azione successiva; la Postazione di revisione passa da un elemento al successivo senza tornare all'elenco, quasi tutta da tastiera.
3. L'Admin è la coda proseguita (In calendario, Pubblicato, Ritirato); il Sito pubblico apre direttamente sul gioco del giorno.

**A chi conviene.** Al proprietario con 5-10 ore a settimana, che apre lo Studio con una domanda sola: "che cosa c'è da fare?". Conviene se la revisione è il collo di bottiglia, come dicono il documento 04 (sezioni 3 e 4) e la prova decisiva 1. Conviene meno ai futuri redattori, che hanno bisogno di orientarsi tra oggetti più che di smaltire una coda.

## 2. Mappa del prodotto

### 2.1 Il modello degli stati (il cuore della proposta)

| Tipo di elemento | Stati in ordine | Chi lo fa avanzare |
|---|---|---|
| Gioco originale (Motore di creazione) | Richiesto → In generazione → Da rivedere → Approvato → In calendario → Pubblicato → (Ritirato) | AI fino a "Da rivedere", poi il proprietario; la pubblicazione avviene da sola alla data |
| Gioco estratto (Motore di estrazione) | In estrazione → Da verificare → Verificato → (Scartato) | AI, poi il proprietario |
| Schema dinamico | Proposto → Confermato / Unito a tipologia esistente / Rimandato | Il proprietario |
| Rivista | Caricata → In estrazione (n/m pagine) → Da verificare (n giochi) → Verificata | Sistema; si chiude da sola quando tutti i suoi Giochi sono verificati |
| Errore | Aperto → Ritentato / Risolto a mano / Ignorato | Il proprietario |

**Regola fissa:** un Gioco con provenienza "Rivista" non può mai entrare in "In calendario". Il comando non compare e la regola è applicata anche lato server. È la traduzione in interfaccia del vincolo "materiale privato, mai ripubblicato".

### 2.2 Le tre superfici

| Superficie | Navigazione principale | Azioni primarie sempre visibili |
|---|---|---|
| **Studio** (desktop, un utente) | Barra laterale con viste salvate della stessa coda: *Da fare* (default), *Da rivedere*, *Da verificare*, *Schemi*, *In corso*, *Errori*, *Tutto*. Sotto: *Calendario* (sola lettura), *Tipologie*, *Impostazioni*. Ricerca globale con Ctrl+K | *Nuovo lotto* (N), *Importa Rivista* (I) |
| **Admin** (web riservato) | Stesso involucro dello Studio, ma con le sole viste *Da programmare*, *In calendario*, *Pubblicati*, *Ritirati*; poi *Aspetto del sito* e *Utilizzo* | *Riempi i giorni vuoti* |
| **Sito pubblico** (mobile prima di tutto) | Barra in basso con 3 voci: *Oggi*, *Archivio*, *Account*. L'indirizzo principale apre già la griglia del gioco del giorno | *Verifica*, *Aiuto* dentro il Lettore |

Studio e Admin sono due installazioni dello stesso involucro: quando lo Studio è collegato al server, le viste Admin compaiono nella stessa barra laterale sotto un separatore. Il proprietario vede un solo strumento, e un redattore futuro riceve solo le viste che gli servono.

### 2.3 Cosa è condiviso

- **Lettore**: un solo componente con tre modi: *gioco* (Sito, Studio), *anteprima* (Postazione di revisione, cronometrata) e *verifica affiancata* (accanto all'immagine della pagina). È lo stesso componente che un editore potrà incorporare.
- **Modello degli stati e Registro del contributo umano**: ogni passaggio di stato scrive una riga nel registro del Gioco.
- **Catalogo delle Tipologie e Schema dinamico**: lo stesso nei due motori.
- **Ricerca**: un indice unico che lo Studio interroga per intero e il Sito interroga solo sui Giochi pubblicati.

## 3. Flussi chiave

Passaggio = una decisione o un'azione dell'utente che cambia schermata o stato (scrivere dentro un campo non conta). I tempi sono stime da confermare con la prova decisiva 1 del documento 04.

### (a) Creare un Gioco originale fino alla pubblicazione

Gioco di riferimento: crittografia breve, creata in un lotto di 5.

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| L1 | *Nuovo lotto* (N): tipologia, tema, quantità, istruzioni scritte dal proprietario | Finestra sopra la coda | 1 min per lotto, 0,2 per gioco |
| L2 | *Genera*: il lotto entra in coda come "In generazione 0/5" e il proprietario può fare altro | Coda | 0 |
| 1 | Apre il primo "Da rivedere" (Invio). La Postazione passa poi da sola al successivo | Postazione di revisione | 0,1 min |
| 2 | Sceglie una delle 3 varianti proposte (tasti 1/2/3) | Postazione | 1 min |
| 3 | Corregge esposto, soluzione e diagramma sul posto; ogni modifica viene registrata | Postazione | 3 min |
| 4 | Prova il gioco nel Lettore in modo *anteprima* (P), con cronometro | Postazione, pannello di sinistra | 2 min |
| 5 | *Approva* (A): il registro del contributo è già compilato (istruzioni scritte, variante scelta, modifiche, prova svolta); una nota libera è facoltativa | Postazione, pannello in basso | 1 min |
| 6 | Il Gioco riceve il primo giorno libero del calendario; Invio accetta, D apre il selettore di data | Postazione | 0,3 min |
| - | Alla data il Sito lo pubblica da solo. L'Admin segnala solo le eccezioni (giorni vuoti nei prossimi 7) | Admin | 0 |

**Totale: 6 passaggi per gioco più 2 per lotto, circa 8 minuti** (cruciverba 9x9: circa 20 minuti, perché il passaggio 3 si allunga).
**Dove si spende il tempo:** correzione (40%) e prova (25%). Navigazione e burocrazia legale scendono quasi a zero.
**Contributo umano:** il registro si compila con le azioni (istruzioni scritte, scelta, modifiche puntuali con prima e dopo, prova, approvazione con data e ora). *Approva* è bloccato se il registro non contiene almeno un atto creativo umano oltre alla sola approvazione; in quel caso la Postazione chiede di scrivere la nota.

### (b) Acquisire una Rivista e rivedere i Giochi estratti

Riferimento: settimanale di 96 pagine e 80 giochi, con 1 tipologia nuova (4 giochi).

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | *Importa Rivista* (I) o trascinamento del PDF sulla coda. La prima volta: dichiarazione "è mia, uso privato" | Coda | 0,3 min |
| 2 | Conferma i metadati proposti (testata, numero, data, periodicità dedotta) | Finestra breve | 0,5 min |
| - | L'estrazione lavora in sottofondo ("In estrazione 34/96 pagine"). I Giochi entrano in "Da verificare" pagina per pagina: si comincia senza aspettare la fine | Coda, vista *In corso* | 0 |
| 3 | Per ogni Gioco, Postazione di verifica: ritaglio della pagina a sinistra, dati strutturati a destra, fuoco già sul primo campo con bassa confidenza. Invio conferma e passa al successivo; E modifica; S scarta | Postazione di verifica | 0,5 min con alta confidenza, 2-3 min con bassa; media circa 0,95 |
| 4 | La tipologia nuova compare come un solo elemento: "Schema da confermare: Bifronte, 4 Giochi in attesa". Lo apre | Coda, vista *Schemi* | 0,2 min |
| 5 | Confronta i campi proposti con i valori di 3 esempi affiancati alle immagini | Scheda dello schema | 3 min |
| 6 | Modifica un campo (nome, tipo, obbligatorio) se serve | Scheda dello schema | 1 min |
| 7 | *Conferma* (oppure *Unisci a tipologia esistente* o *Rimanda*). I 4 Giochi tornano in "Da verificare" | Scheda dello schema | 0,3 min |
| - | La Rivista passa a "Verificata" da sola quando l'ultimo Gioco è verificato | Coda | 0 |

**Totale: 2 passaggi di avvio, 1-2 per Gioco, 4 per lo schema; circa 82 minuti per il numero, cioè 1,0 minuto per Gioco rivisto.**
**Dove si spende il tempo:** sui Giochi a bassa confidenza. La coda ordina per confidenza crescente, così l'attenzione va dove serve e i Giochi sicuri si confermano in serie.
**Rischio da presidiare:** il ritmo "Invio, Invio, Invio" porta a confermare senza guardare. Contromisure: ogni 20 conferme un controllo a campione con un errore inserito apposta (opzionale), e una misura visibile del tempo medio per Gioco.

### (c) Il giocatore da mobile

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| G1-1 | Apre il link (condivisione, newsletter, ricerca). La pagina *Oggi* è già il Lettore: definizione attiva in grande sopra la griglia. Tocca una casella e scrive | Sito, *Oggi* | 5 s alla prima lettera |
| G1-2 | *Verifica* in fondo alla griglia. Esito in testo: "Tutto giusto" oppure "3 lettere da rivedere", evidenziate | Lettore | - |
| G1-3 | Schermata di fine: tempo, "Domani alle 7 c'è un nuovo gioco". *Ricordamelo* | Sito | - |
| G1-4 | Sceglie email (un campo) o "Aggiungi alla schermata Home" (istruzione illustrata) | Foglio dal basso | 30 s |
| G2-1 | Il giorno dopo apre l'email o l'icona: è di nuovo sul gioco, con "2 giorni di fila" in testo | Sito, *Oggi* | 5 s |
| S-1 | Dopo 7 giorni, oppure toccando un giorno chiuso nell'*Archivio* (oltre gli ultimi 7): *Abbonati* | Archivio | - |
| S-2 | Sceglie il piano: annuale in evidenza, mensile sotto, prezzo totale scritto per intero | Sito | - |
| S-3 | Paga: portafoglio del telefono o carta; l'account si crea con l'email del pagamento | Sito | - |
| S-4 | Conferma e *Torna al gioco* | Sito | - |

**Passaggi:** giorno 1 = 1 per iniziare e 2 per il promemoria; giorno 2 = 1; abbonamento = 4.
**Dove si spende il tempo:** nel gioco stesso. Il percorso è il più corto possibile; il prezzo è che chi non conosce il prodotto non vede altre tipologie se non apre l'Archivio.

### (d) Cercare un Gioco nell'archivio e giocarlo nello Studio

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | Ctrl+K (o il campo *Cerca* in alto) | Ovunque nello Studio | 1 s |
| 2 | Scrive "crittografia mare" oppure usa i filtri testuali `tipo:`, `rivista:`, `stato:`, `anno:`. Risultati mentre scrive, con stato e provenienza | Tavolozza di ricerca | 10 s |
| 3 | Invio sul risultato: si apre il Lettore in modo *gioco*. Maiusc+Invio apre invece la scheda dell'elemento | Lettore | 3 s |

**Totale: 3 passaggi, circa 15 secondi.** I Giochi estratti da Riviste si giocano solo qui, mai sul Sito.

## 5. Stati da non dimenticare

| Stato | Dove | Comportamento previsto |
|---|---|---|
| **Vuoto** | Coda *Da fare* vuota | Testo "Niente da fare. Il calendario è coperto fino al 18 novembre" più le due azioni primarie. Se il calendario ha buchi, il vuoto diventa "3 giorni scoperti la prossima settimana: *Nuovo lotto*" |
| Vuoto | Primo avvio dello Studio | Coda con 3 elementi guida veri (importa una Rivista di prova, crea un lotto di 1, conferma una tipologia); si chiudono come elementi normali |
| Vuoto | Sito, *Archivio* senza giochi passati | "Il primo gioco è di oggi" con collegamento a *Oggi* |
| **Caricamento** | Coda e Postazione | Righe segnaposto della stessa altezza (nessuno spostamento del layout). La Postazione precarica i 2 elementi successivi, così il passaggio al successivo è istantaneo |
| Caricamento | Lettore sul Sito | La griglia vuota compare subito con le dimensioni giuste; le definizioni arrivano dopo |
| **Errore dell'AI** | Generazione o estrazione fallita, risposta non valida, limite di costo raggiunto | L'elemento diventa "Errore" e resta in coda con causa in chiaro ("La risposta non rispetta lo schema della crittografia"), *Ritenta*, *Ritenta con altre istruzioni*, *Fai a mano*. Gli altri elementi del lotto proseguono. Un contatore *Errori* nella barra laterale; nessuna finestra modale |
| Errore dell'AI | Confidenza bassa ma risposta valida | Non è un errore: il Gioco entra in "Da verificare" con i campi incerti evidenziati in testo ("incerto") e non solo con il colore |
| **Elaborazione lunga** | Estrazione di una Rivista, lotto di generazione | Elemento in coda con avanzamento in pagine o giochi, tempo residuo stimato, *Sospendi* e *Annulla*. Il lavoro già fatto resta. Notifica di sistema del desktop alla fine. Lo Studio si può chiudere: l'elaborazione riprende alla riapertura |
| **Conflitto** | Lo stesso Gioco aperto in Studio e in Admin, o in due finestre | Blocco leggero: la seconda finestra apre in sola lettura con "In revisione nello Studio dalle 21:14" e *Prendi il controllo* |
| Conflitto | Giorno di calendario già occupato | All'approvazione la Postazione propone il primo giorno libero; non sovrascrive mai |
| Conflitto | Schema dinamico confermato mentre altri Giochi della stessa tipologia sono in verifica con lo schema vecchio | I Giochi coinvolti tornano in "Da verificare" con il campo nuovo evidenziato |
| Conflitto | Rivista già importata (stesso file o stessa testata e numero) | All'importazione: "Questa Rivista c'è già (verificata il 3 novembre)". *Apri* o *Importa come nuova versione* |
| **Offline (Lettore)** | Sito | Il gioco del giorno e i progressi restano in locale; la verifica funziona senza rete perché il Lettore riceve un'impronta della soluzione per parola, non la soluzione in chiaro. Avviso discreto "Sei senza connessione: i progressi sono salvati su questo telefono". Alla riconnessione sincronizza; se esistono progressi su due dispositivi tiene il più avanzato e lo dice |
| Offline | Studio | La coda locale resta consultabile; generazione ed estrazione si mettono in attesa con "Riprende quando torna la rete" |

## 6. Accessibilità

| Tema | Scelta in questa proposta |
|---|---|
| **Tastiera** | Lo Studio è progettato da tastiera: J/K elemento precedente e successivo, Invio conferma, A approva, E modifica, S scarta, P prova, Ctrl+K cerca, ? elenco tasti. Le scorciatoie a un solo carattere si possono disattivare o rimappare, come richiede WCAG 2.1.4, e agiscono solo quando il fuoco non è in un campo di testo. Ordine di tabulazione uguale all'ordine visivo; collegamento "Salta alla coda" |
| Tastiera nel Lettore | Frecce per muoversi, lettere per scrivere, Backspace per cancellare e tornare indietro, Tab e Maiusc+Tab per passare alla definizione successiva o precedente, Spazio per cambiare direzione |
| **Lettore di schermo, griglia** | La griglia è un `role="grid"` con fuoco mobile (roving tabindex). Annuncio a ogni spostamento: "7 orizzontale, 6 lettere: Fiume di Roma. Lettera 2 di 6, vuota, incrocia 3 verticale". La definizione attiva sta in una regione `aria-live="polite"`. È sempre disponibile la **modalità elenco**: un campo di testo per parola con la definizione come etichetta, che con lettori di schermo e testo molto ingrandito funziona meglio della griglia |
| Lettore di schermo, Studio | La coda è una tabella con intestazioni e stato in testo. Gli avanzamenti in sottofondo sono annunciati in una regione `role="status"` al massimo ogni 30 secondi, mai a ogni pagina |
| **Bersagli** | Sito: almeno 44x44 px per ogni controllo, caselle comprese. Sopra i 9 colonne la griglia non entra in 375 px con caselle da 44: il Lettore mostra la griglia ridotta per orientarsi e una **vista parola** ingrandita per scrivere. Studio: almeno 24x24 px (WCAG 2.2), 32 px per le azioni frequenti |
| **Testo ingrandibile** | Sito: testo base 18 px, zoom mai bloccato, ricomposizione fino al 400% senza scorrimento orizzontale; un controllo "A A" nel Lettore porta il testo a 150% e 200%. Studio: unità relative ovunque; la Postazione passa a una colonna sotto i 900 px effettivi |
| Altro | Nessun limite di tempo (il cronometro si può nascondere). Esito della verifica in testo e icona, non solo colore. Movimento ridotto rispettato |

## 7. Valutazione

**Punti di forza**
- Minimo tempo per Gioco rivisto: navigazione quasi nulla, revisione ordinata per confidenza, registro del contributo compilato dalle azioni.
- Una sola domanda all'avvio dello Studio ("che cosa c'è da fare?"): adatto a sessioni brevi e discontinue.
- Le elaborazioni lunghe non bloccano mai: tutto è un elemento con il suo stato.
- Il vincolo "Rivista mai pubblicata" è uno stato impossibile, non una regola da ricordare.
- Sito pubblico più corto delle tre proposte: 1 tocco dal link alla prima lettera.

**Punti deboli**
- Gli oggetti (una Rivista intera, una Tipologia, la storia di un Gioco) si vedono solo come schede secondarie: chi vuole "sfogliare" trova lo strumento povero.
- Per un redattore esterno la coda è opaca senza formazione; servono viste filtrate per ruolo.
- Il ritmo veloce espone a conferme distratte (vedi contromisure nel flusso b).
- Il Sito mostra poco del catalogo: scoprire altre tipologie richiede l'Archivio.

**Costo di realizzazione relativo: medio.** Servono la macchina a stati (che serve comunque, anche per il registro del contributo), la Postazione con precaricamento e scorciatoie, e i lavori in sottofondo ripristinabili. Le schermate sono poche (circa 8 nello Studio).

**Rischio principale.** Coda che cresce più in fretta di quanto il proprietario la smaltisca (una Rivista intera ne aggiunge 80): la vista *Da fare* diventa un muro e scoraggia. Mitigazione: *Da fare* mostra per default solo ciò che sblocca il calendario dei prossimi 7 giorni; le Riviste in verifica hanno una vista propria e non contano come urgenti.
