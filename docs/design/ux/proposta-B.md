# Proposta B - "L'Archivio" (organizzato per oggetti)

Wireframe: `wireframe-B.html`. Convenzioni comuni (passaggio, stime, Gioco di riferimento) in `PROPOSTE-UX.md`.

## 1. Idea di fondo

1. Il prodotto è un archivio di oggetti con un indirizzo ciascuno: **Riviste, Giochi, Tipologie, Pubblicazioni**. Ogni oggetto ha un elenco con filtri e una scheda con schede interne (Contenuto, Prova, Contributo, Provenienza, Storia).
2. Le azioni nascono dall'oggetto ("su questa Rivista: estrai", "su questo Gioco: approva", "su questo giorno: pubblica"), come in un gestionale o in un catalogo di biblioteca.
3. Il Sito pubblico è anch'esso un catalogo: gioco del giorno in evidenza, poi le tipologie e l'archivio.

**A chi conviene.** A chi pensa per collezioni e vuole poter rispondere a domande come "quante crittografie sinonimiche ho del 2024?" o "che cosa ho pubblicato a ottobre?". Conviene ai futuri redattori degli editori, che conoscono già questo modello dai sistemi editoriali, e alla Strategia B del documento 04 (archivio giocabile per editori). Costa più minuti per Gioco al proprietario.

## 2. Mappa del prodotto

### 2.1 Gli oggetti

| Oggetto | Elenco | Scheda (schede interne) | Azioni principali |
|---|---|---|---|
| **Gioco** | Tabella con filtri: tipologia, provenienza (Originale / Rivista), stato, difficoltà, data | Contenuto · Prova (Lettore) · Contributo umano · Provenienza · Pubblicazioni · Storia | Nuovo gioco con AI, Approva, Duplica, Ritira |
| **Rivista** | Copertine o righe: testata, numero, data, stato dell'estrazione | Pagine (miniature) · Giochi · Metadati · Estrazione (registro) | Importa, Estrai di nuovo, Revisione pagina |
| **Tipologia** | Elenco delle circa 30 tipologie, con numero di Giochi e versione dello Schema | Schema (campi) · Esempi · Giochi · Versioni · Proposte in attesa | Conferma proposta, Nuova versione, Unisci |
| **Pubblicazione** (Admin) | Calendario mensile e elenco | Giorno: Gioco, orario, visibilità (gratis/abbonati), statistiche | Programma, Sposta, Ritira |

**Regola fissa:** nella scheda di un Gioco con provenienza "Rivista" la scheda interna *Pubblicazioni* non esiste e il Gioco non compare nel selettore del calendario.

### 2.2 Le tre superfici

| Superficie | Navigazione principale | Note |
|---|---|---|
| **Studio** | Barra laterale: *Giochi*, *Riviste*, *Tipologie*, *Ricerca*. In alto: briciole di pane ("Riviste › Enigmistica Mese n. 41 › Pagina 12"), campo di ricerca, icona *Attività* con le elaborazioni in corso | Ogni scheda ha un indirizzo (collegamento profondo), apribile in una nuova finestra dell'involucro desktop |
| **Admin** | Applicazione separata. Barra laterale: *Pubblicazioni*, *Sito* (aspetto: testata, colori del tema, testi, pagine statiche), *Abbonati*, *Statistiche* | L'Admin legge i Giochi approvati dallo Studio; non modifica il contenuto, lo programma |
| **Sito pubblico** | In alto: logo e *Accedi*. Barra in basso con 4 voci: *Oggi*, *Giochi* (per tipologia), *Archivio*, *Account*. La Home è un catalogo: "Il gioco di oggi" in cima, poi una fila per tipologia | Ogni Gioco pubblicato ha una pagina propria con istruzioni e un pulsante *Gioca* (utile anche per la ricerca sul web) |

### 2.3 Cosa è condiviso

- **Lettore**: stesso componente nella scheda *Prova* dello Studio, nella pagina di gioco del Sito e, in futuro, incorporato dagli editori.
- **Modello degli oggetti e indirizzi**: Studio e Admin usano gli stessi identificativi; un collegamento a un Gioco funziona in entrambi.
- **Catalogo Tipologie e Schema dinamico**: è un oggetto di primo livello, visibile e navigabile.
- **Componenti di elenco e scheda**: tabella con filtri, scheda con schede interne, briciole di pane. Si progettano una volta e valgono per tutti gli oggetti.

## 3. Flussi chiave

### (a) Creare un Gioco originale fino alla pubblicazione

Gioco di riferimento: crittografia breve, uno alla volta.

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | *Giochi* → *Nuovo gioco con AI* | Studio, elenco Giochi | 0,2 min |
| 2 | Modulo: tipologia, tema, istruzioni. *Genera* (attesa nella pagina, 20-60 s, o in sottofondo) | Finestra | 1,5 min |
| 3 | Si apre la scheda del Gioco in bozza, scheda interna *Contenuto*: sceglie una delle varianti | Scheda Gioco | 1 min |
| 4 | Modifica i campi | Scheda Gioco › Contenuto | 3 min |
| 5 | Scheda interna *Prova*: gioca nel Lettore | Scheda Gioco › Prova | 2 min |
| 6 | Scheda interna *Contributo umano*: registro precompilato (istruzioni, variante, modifiche) più un campo obbligatorio "Descrivi il tuo contributo" | Scheda Gioco › Contributo | 2 min |
| 7 | Stato → *Approvato*, *Salva* | Scheda Gioco | 0,3 min |
| 8 | Apre l'Admin → *Pubblicazioni* | Admin | 0,5 min |
| 9 | Nel calendario sceglie il giorno | Admin, calendario | 0,5 min |
| 10 | *Aggiungi gioco*: selettore con ricerca tra i Giochi approvati e originali | Finestra | 1 min |
| 11 | *Conferma* (visibilità: gratuito o abbonati) | Admin | 0,3 min |

**Totale: 11 passaggi, circa 13 minuti** (cruciverba 9x9: circa 27).
**Dove si spende il tempo:** correzione e prova come in A, più circa 4 minuti di navigazione tra schede e applicazioni e di scrittura del contributo. In compenso ogni passaggio è esplicito e documentato nella Storia del Gioco.
**Variante a lotti:** dall'elenco Giochi si selezionano più bozze e si apre la prima; *Salva e successivo* toglie il passaggio 1 e mezzo del 2. Con 5 giochi si scende a circa 11 minuti a gioco; la programmazione in Admin si fa una volta per tutti (selezione multipla e *Distribuisci sui giorni liberi*).

### (b) Acquisire una Rivista e rivedere i Giochi estratti

Riferimento: settimanale di 96 pagine e 80 giochi, 1 tipologia nuova (4 giochi).

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | *Riviste* → *Importa* | Elenco Riviste | 0,2 min |
| 2 | Carica il PDF e compila i metadati (con suggerimenti dedotti dal file); dichiarazione "è mia, uso privato" | Finestra | 1 min |
| 3 | Si apre la scheda della Rivista: scheda interna *Estrazione* con avanzamento. Si può lasciare e tornare | Scheda Rivista | 0 |
| 4 | A estrazione (anche parziale) finita: scheda interna *Pagine*, apre pagina 1 | Scheda Rivista › Pagine | 0,2 min |
| 5 | Revisione pagina: immagine con i riquadri dei Giochi trovati, elenco a destra. Per ogni Gioco: seleziona, controlla i campi, *Conferma* | Revisione pagina | circa 1,6 min per Gioco |
| 6 | *Pagina successiva* (96 volte, anche pagine senza giochi) | Revisione pagina | 0,1 min per pagina |
| 7 | Su 4 Giochi compare il segnale "Tipologia sconosciuta: proposta Bifronte" con collegamento | Revisione pagina | - |
| 8 | Va in *Tipologie* → *Proposte in attesa* → scheda della proposta | Tipologie | 0,5 min |
| 9 | Scheda interna *Esempi*: confronta i valori con le immagini | Scheda Tipologia | 3 min |
| 10 | Scheda interna *Schema*: modifica un campo se serve | Scheda Tipologia | 1 min |
| 11 | *Conferma la tipologia* | Scheda Tipologia | 0,3 min |
| 12 | Torna alla Rivista (briciole di pane) e verifica i 4 Giochi rimasti | Revisione pagina | 0,5 min più i Giochi |

**Totale: 5 passaggi di avvio, circa 3 per Gioco, 6 per la tipologia nuova; circa 150 minuti per il numero, cioè 1,8 minuti per Gioco rivisto.**
**Dove si spende il tempo:** sulla revisione uniforme, pagina per pagina: i Giochi sicuri ricevono la stessa attenzione di quelli incerti. La pagina come unità è però il modo naturale di confrontare con la carta, e la Rivista resta navigabile come un oggetto completo.

### (c) Il giocatore da mobile

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| G1-1 | Apre il link: Home catalogo, "Il gioco di oggi" in cima | Sito, Home | - |
| G1-2 | Tocca la scheda del gioco: pagina con istruzioni brevi e difficoltà | Pagina gioco | - |
| G1-3 | *Gioca*: si apre il Lettore | Lettore | circa 20 s alla prima lettera |
| G1-4 | Fine: esito, "Altri giochi gratuiti", "Crea un account per non perdere la serie" | Sito | - |
| G1-5/7 | Promemoria: *Crea account* → email (link magico, senza password) → conferma dall'email | Sito, email | 1-2 min |
| G2-1/3 | Il giorno dopo: link nell'email → Home → scheda → *Gioca* (oppure *Oggi* nella barra) | Sito | circa 15 s |
| S-1 | In *Archivio*, un gioco oltre gli ultimi 7 giorni: *Sblocca con l'abbonamento* | Archivio | - |
| S-2 | Confronta i piani | Pagina piani | - |
| S-3 | Accede (è già registrato) | Sito | - |
| S-4 | Paga | Sito | - |
| S-5 | Conferma e ritorno al gioco sbloccato | Sito | - |

**Passaggi:** giorno 1 = 3 per iniziare e 3 per il promemoria; giorno 2 = 3 (2 dalla barra); abbonamento = 5.
**Dove si spende il tempo:** nella scelta. Per un giocatore abituale over 65 la Home catalogo è un ostacolo ogni mattina; per un giocatore curioso è il motivo per restare e scoprire altre tipologie. Il Sito ha più pagine da indicizzare per la ricerca sul web.

### (d) Cercare un Gioco nell'archivio e giocarlo nello Studio

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | *Ricerca* nella barra laterale | Studio | 2 s |
| 2 | Testo libero più filtri a faccette (tipologia, Rivista, anno, stato, provenienza, difficoltà) con il numero di risultati per faccetta | Ricerca | 15 s |
| 3 | Apre la scheda del Gioco | Scheda Gioco | 3 s |
| 4 | Scheda interna *Prova* (o *Gioca a schermo intero*) | Lettore | 3 s |

**Totale: 4 passaggi, circa 25-30 secondi.** È la ricerca più potente delle tre: le faccette rispondono anche a domande di inventario, e la ricerca si salva come vista ("Crittografie 2024 non ancora provate").

## 5. Stati da non dimenticare

| Stato | Dove | Comportamento previsto |
|---|---|---|
| **Vuoto** | Elenco Riviste, Giochi, Tipologie al primo avvio | Ogni elenco spiega che cosa conterrà e offre l'azione per riempirlo (*Importa la prima Rivista*, *Crea il primo Gioco*). Tipologie arriva già popolato con il catalogo iniziale |
| Vuoto | Ricerca senza risultati | "Nessun Gioco con questi filtri" più l'elenco dei filtri attivi, ciascuno rimovibile, e il numero di risultati che si otterrebbe togliendolo |
| Vuoto | Admin, giorno senza Pubblicazione | Il giorno nel calendario dice "Vuoto" in testo; in cima al mese "4 giorni vuoti" con *Distribuisci* |
| **Caricamento** | Elenchi e schede | Righe e schede segnaposto; le schede interne pesanti (Prova, Pagine) si caricano solo quando aperte |
| **Errore dell'AI** | Generazione | La scheda della bozza si apre comunque, con il riquadro "La generazione non è riuscita: (causa)". *Riprova*, *Modifica le istruzioni*, *Compila a mano*. Le istruzioni scritte non si perdono |
| Errore dell'AI | Estrazione | Nella scheda Rivista, scheda interna *Estrazione*: elenco delle pagine non riuscite con causa e *Riprova queste pagine*. Nelle miniature le pagine fallite sono marcate in testo ("non letta") |
| **Elaborazione lunga** | Estrazione, lotti | Icona *Attività* nella barra in alto con il numero di lavori; il pannello elenca ogni lavoro con avanzamento, *Sospendi*, *Annulla* e collegamento all'oggetto. Notifica del desktop alla fine. Se l'utente apre la scheda della Rivista durante l'estrazione, le pagine già lette sono già rivedibili |
| **Conflitto** | Stessa scheda aperta in due finestre o in Studio e Admin | Salvataggio con controllo di versione: se l'oggetto è cambiato, "Questo Gioco è stato modificato alle 21:14 in un'altra finestra" con confronto campo per campo e scelta |
| Conflitto | Due Pubblicazioni nello stesso giorno e orario | Il calendario lo impedisce e propone lo slot successivo |
| Conflitto | Nuova versione di uno Schema con Giochi esistenti | La scheda Tipologia mostra "38 Giochi sulla versione 1": *Migra* (con anteprima dei campi mancanti) o *Lascia sulla versione 1* |
| Conflitto | Rivista già importata | Avviso nella finestra di importazione con collegamento alla scheda esistente |
| **Offline (Lettore)** | Sito | Come nelle altre proposte: progressi in locale, verifica tramite impronta della soluzione, sincronizzazione alla riconnessione. In più la Home catalogo offline mostra solo i giochi già scaricati, con "Disponibile senza rete" scritto sulla scheda |
| Offline | Studio | Elenchi e schede in sola lettura dalla copia locale; le azioni che richiedono il server sono disattivate con la spiegazione accanto |

## 6. Accessibilità

| Tema | Scelta in questa proposta |
|---|---|
| **Tastiera** | Modello web classico: Tab e Maiusc+Tab, schede interne con il modello ARIA *tabs* (frecce sinistra e destra), tabelle con intestazioni ordinabili (`aria-sort`), briciole di pane in un `nav`. Poche scorciatoie, tutte con modificatore (Alt+S salva, Alt+N nuovo, Ctrl+K cerca), quindi nessun conflitto con i lettori di schermo |
| Tastiera nel Lettore | Uguale nelle tre proposte (vedi proposta A) |
| **Lettore di schermo, griglia** | Base comune: `role="grid"` con fuoco mobile, annuncio di definizione, posizione e incroci, **modalità elenco** sempre disponibile. In questa proposta la pagina di gioco del Sito ha anche le istruzioni in testo prima del Lettore, con un titolo h2 raggiungibile |
| Lettore di schermo, Studio | Ogni scheda ha un h1 con il nome dell'oggetto; al cambio di scheda interna il fuoco va sul suo titolo. La revisione pagina ha l'elenco dei Giochi come alternativa all'immagine con riquadri, che non si usa da tastiera |
| **Bersagli** | Sito: 44x44 px minimi; le schede del catalogo sono interamente toccabili; vista parola ingrandita per le griglie larghe. Studio: 24x24 px minimi, righe di tabella alte almeno 32 px |
| **Testo ingrandibile** | Sito: 18 px di base, zoom libero, ricomposizione al 400%. Il catalogo passa da file orizzontali a una colonna verticale quando il testo è ingrandito, perché lo scorrimento orizzontale è difficile da usare per molti giocatori anziani. Studio: le tabelle passano a schede impilate sotto i 700 px effettivi |
| Altro | Nessun limite di tempo; esiti in testo; movimento ridotto; nessun carosello automatico in Home |

## 7. Valutazione

**Punti di forza**
- Modello mentale familiare: chiunque abbia usato un gestionale, un CMS o una biblioteca digitale lo capisce senza istruzioni. È il più adatto ai redattori esterni.
- Ricerca e inventario migliori: faccette, viste salvate, collegamenti profondi a ogni oggetto.
- La Rivista resta un oggetto intero e navigabile pagina per pagina, come la carta.
- Le Tipologie sono visibili e governabili (versioni, esempi, migrazioni): lo Schema dinamico non è nascosto.
- Sito più ricco da esplorare e da indicizzare; più adatto a un archivio grande, se un giorno ci sarà.

**Punti deboli**
- Il tempo per Gioco rivisto è il più alto delle tre (circa 1,8 minuti in estrazione, 13 in creazione): il proprietario paga in navigazione la flessibilità.
- Nessuna risposta immediata a "che cosa devo fare adesso?": i lavori in attesa sono sparsi tra oggetti diversi.
- Creazione e pubblicazione sono separate tra Studio e Admin: due applicazioni da aprire per un solo Gioco.
- Per i giocatori anziani la Home catalogo aggiunge 2 tocchi e una scelta ogni mattina.

**Costo di realizzazione relativo: basso.** Sono elenchi e schede standard (tabelle con filtri, moduli, schede interne) che le librerie di componenti offrono già; ogni oggetto in più segue lo stesso schema. Il pezzo meno banale è la revisione pagina con i riquadri.

**Rischio principale.** Lo Studio diventa un gestionale ben ordinato in cui il proprietario passa il tempo a navigare invece che a rivedere: con 5 ore a settimana, la differenza di circa 70 minuti per numero di Rivista e di 5 minuti per Gioco creato si mangia la produzione. Mitigazione parziale: una vista "Da fare" trasversale presa dalla proposta A.
