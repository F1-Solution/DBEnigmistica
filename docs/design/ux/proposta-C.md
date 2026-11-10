# Proposta C - "I Percorsi" (procedure guidate passo passo)

Wireframe: `wireframe-C.html`. Convenzioni comuni (passaggio, stime, Gioco di riferimento) in `PROPOSTE-UX.md`.

## 1. Idea di fondo

1. Il prodotto offre poche procedure con un inizio e una fine: **Crea, Acquisisci, Pubblica, Cerca e gioca** e, sul Sito, **Gioca oggi**. Ogni procedura ha passi numerati, *Indietro*, *Salva ed esci* e un riepilogo finale.
2. Non si deve sapere dove stanno le cose: si sceglie che cosa fare e la procedura conduce. Ogni procedura si sospende e si riprende dal passo in cui era, anche dopo giorni.
3. Il Sito pubblico è un solo percorso quotidiano, lineare e a caratteri grandi: gioca, finisci, fatti ricordare domani, e dopo qualche giorno l'offerta di abbonamento.

**A chi conviene.** A chi usa lo strumento di rado e a intervalli lunghi (il proprietario a 5 ore a settimana che ogni volta deve ricordarsi come si fa) e a chi lo usa per la prima volta (redattori degli editori, demo a un editore). Sul Sito conviene ai giocatori meno esperti di tecnologia. Costa più passaggi a chi conosce già lo strumento.

## 2. Mappa del prodotto

### 2.1 I percorsi

| Percorso | Superficie | Passi | Si riprende? |
|---|---|---|---|
| **Crea giochi** | Studio | 1 Tipologia e tema · 2 Scegli una proposta · 3 Rifinisci · 4 Provalo · 5 Il tuo contributo · 6 Riepilogo e approvazione | Sì, da ogni passo |
| **Acquisisci una Rivista** | Studio | 1 Carica il PDF · 2 Controlla i dati · 3 Lettura in corso · 4 Rivedi le pagine · 5 Nuove tipologie · 6 Riepilogo | Sì; il passo 3 si chiude e avvisa quando ha finito |
| **Cerca e gioca** | Studio | 1 Cosa cerchi · 2 Scegli · 3 Gioca | Non serve |
| **Pubblica** | Admin (anche come seguito di *Crea giochi*) | 1 Scegli i giochi · 2 Scegli i giorni · 3 Anteprima sul Sito · 4 Conferma | Sì |
| **Cambia l'aspetto del sito** | Admin | 1 Cosa cambiare · 2 Modifica con anteprima · 3 Pubblica la modifica | Sì, con bozza |
| **Gioca oggi** | Sito | Benvenuto (solo la prima volta) · Gioco · Fine · Promemoria · (dopo 3-7 giorni) Abbonamento | Sì, i progressi restano |

Il passo *Nuove tipologie* di *Acquisisci* è a sua volta un piccolo percorso in 3 tappe (Guarda gli esempi · Controlla i campi · Conferma), il punto in cui lo Schema dinamico viene spiegato a chi non lo conosce.

**Regola fissa:** il percorso *Pubblica* elenca solo Giochi originali approvati. I Giochi estratti dalle Riviste finiscono nel percorso *Acquisisci* con il riepilogo "Salvati nel tuo archivio privato".

### 2.2 Le tre superfici

| Superficie | Navigazione principale | Note |
|---|---|---|
| **Studio** | Home "Che cosa vuoi fare?" con 3 grandi scelte (*Crea giochi*, *Acquisisci una Rivista*, *Cerca e gioca*), sotto *Da riprendere* (percorsi sospesi) e *In corso* (letture e generazioni in sottofondo). Menu secondario *Strumenti*: Tipologie, Impostazioni, Registro attività | Dentro un percorso la navigazione principale sparisce: restano solo l'indicatore dei passi, *Indietro*, *Salva ed esci* |
| **Admin** | Home con 3 scelte: *Pubblica giochi*, *Cambia l'aspetto del sito*, *Guarda come va* (dashboard di utilizzo). In cima un avviso se ci sono giorni scoperti | La dashboard è l'unica vista non procedurale |
| **Sito pubblico** | Nessuna barra di navigazione: l'indirizzo principale porta al percorso *Gioca oggi*. Un menu *Altro* in alto a destra con Archivio, Abbonamento, Come si gioca, Aiuto. Caratteri grandi per default | Un solo pulsante principale per schermata |

### 2.3 Cosa è condiviso

- **Lettore**: lo stesso componente nei passi *Provalo*, *Rivedi le pagine*, *Gioca* e sul Sito.
- **Motore dei percorsi**: indicatore dei passi, salvataggio automatico a ogni passo, ripresa, riepilogo. Si costruisce una volta e serve a tutti i percorsi.
- **Registro del contributo umano**: si compila durante *Crea giochi* e si conferma esplicitamente al passo 5.
- **Catalogo Tipologie e Schema dinamico**.

## 3. Flussi chiave

### (a) Creare un Gioco originale fino alla pubblicazione

Gioco di riferimento: crittografia breve, uno alla volta, con *Crea un altro con le stesse impostazioni* disponibile alla fine.

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | Home → *Crea giochi* | Studio, Home | 0,1 min |
| 2 | Passo 1: tipologia, tema, istruzioni. *Avanti* (la generazione parte) | Percorso, passo 1 | 1,5 min |
| 3 | Passo 2: sceglie una delle 3 proposte affiancate | Passo 2 | 1 min |
| 4 | Passo 3: rifinisce (modifiche sul posto) | Passo 3 | 3 min |
| 5 | Passo 4: lo prova nel Lettore | Passo 4 | 2 min |
| 6 | Passo 5: legge il registro precompilato, aggiunge una frase e spunta "Confermo che questo è il mio contributo" | Passo 5 | 1,5 min |
| 7 | Passo 6: riepilogo, *Approva* | Passo 6 | 0,5 min |
| 8 | "Vuoi programmarlo ora?" → *Sì*: il percorso *Pubblica* si apre con il Gioco già scelto e il primo giorno libero proposto | Pubblica, passo 2 | 0,5 min |
| 9 | Anteprima di come apparirà sul Sito e *Conferma* | Pubblica, passi 3-4 | 0,5 min |

**Totale: 9 passaggi, circa 11 minuti** (cruciverba 9x9: circa 24). Con *Crea un altro con le stesse impostazioni* i passaggi 1-2 si saltano dal secondo Gioco in poi: circa 9,5 minuti.
**Dove si spende il tempo:** come in A sulla correzione e sulla prova; in più circa 2 minuti di conferme e riepiloghi. È il flusso che documenta meglio il contributo umano, perché lo fa leggere e confermare esplicitamente ogni volta, ed è il più facile da spiegare a un legale o a un editore.

### (b) Acquisire una Rivista e rivedere i Giochi estratti

Riferimento: settimanale di 96 pagine e 80 giochi, 1 tipologia nuova (4 giochi).

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | Home → *Acquisisci una Rivista* | Studio | 0,1 min |
| 2 | Passo 1: carica il PDF, dichiarazione "è mia, uso privato" | Passo 1 | 0,3 min |
| 3 | Passo 2: controlla i dati dedotti (testata, numero, data, periodicità) | Passo 2 | 0,5 min |
| 4 | Passo 3: "Sto leggendo la Rivista: puoi chiudere, ti avviso io". *Salva ed esci* oppure *Comincia a rivedere le pagine pronte* | Passo 3 | 0 |
| 5 | Ripresa da *Da riprendere* in Home (o dalla notifica) | Home | 0,1 min |
| 6 | Passo 4: pagina per pagina, immagine a sinistra e Giochi a destra. Per ogni Gioco *Conferma* o *Correggi*. Le pagine con tutti i Giochi ad alta confidenza offrono *Conferma tutta la pagina* dopo un'occhiata; le pagine senza giochi si saltano da sole | Passo 4 | circa 1,3 min per Gioco |
| 7 | Passo 5, tappa 1: esempi della tipologia nuova affiancati alle immagini | Passo 5 | 3 min |
| 8 | Tappa 2: controlla i campi proposti, ne modifica uno | Passo 5 | 1,5 min |
| 9 | Tappa 3: *Conferma la tipologia* e verifica dei 4 Giochi | Passo 5 | 0,3 min più i Giochi |
| 10 | Passo 6: riepilogo (80 verificati, 2 scartati, 1 tipologia nuova) e *Chiudi* | Passo 6 | 0,3 min |

**Totale: 6 passi del percorso, 2 passaggi per Gioco in media, 3 per la tipologia nuova; circa 115 minuti per il numero, cioè 1,4 minuti per Gioco rivisto.**
**Dove si spende il tempo:** sulla revisione pagina per pagina, alleggerita da *Conferma tutta la pagina*. La tipologia nuova si affronta tutta insieme alla fine, quando il proprietario ha già visto la Rivista, con meno salti di contesto che in B.
**Limite:** la tipologia nuova blocca la chiusura del percorso; la si può *Rimandare*, ma allora il percorso resta tra quelli da riprendere.

### (c) Il giocatore da mobile

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| G1-1 | Apre il link: "Il gioco di oggi: crittografia. Circa 5 minuti." Un solo pulsante grande: *Gioca* | Sito, Benvenuto | - |
| G1-2 | Solo la prima volta: "Come si gioca" in 3 frasi e un'immagine, *Ho capito* (o *Salta*) | Sito | circa 15 s alla prima lettera |
| G1-3 | Gioca. *Verifica* sempre visibile | Lettore | - |
| G1-4 | Fine: "Bravo, l'hai risolta in 6 minuti". Domanda: "Vuoi che te lo ricordiamo domani?" *Sì, ricordamelo* | Sito | - |
| G1-5 | Sceglie come: email (un campo) o icona sulla schermata Home (istruzioni illustrate passo per passo) | Sito | 30 s |
| G2-1 | Il giorno dopo apre il promemoria: va diretto al gioco (il Benvenuto non c'è più) | Sito | 5 s |
| S-1 | Dopo 3-7 giorni di gioco, alla fine del gioco: "Ti piace? Con l'abbonamento hai tutti i giochi passati e 2 giochi in più al giorno". *Scopri l'abbonamento* (oppure *Non ora*, che lo nasconde per 7 giorni) | Sito | - |
| S-2 | Una sola offerta spiegata a parole, prezzo annuale intero e "puoi disdire quando vuoi" | Sito | - |
| S-3 | Paga (portafoglio del telefono o carta, account creato con l'email del promemoria) | Sito | - |
| S-4 | Riepilogo leggibile ("Hai pagato 19 € per un anno, rinnovo il 10 novembre 2027") e *Torna al gioco* | Sito | - |

**Passaggi:** giorno 1 = 2 per iniziare (1 dal secondo giorno) e 2 per il promemoria; giorno 2 = 1; abbonamento = 4.
**Dove si spende il tempo:** il primo giorno nella spiegazione, che però riduce l'abbandono di chi non conosce la tipologia. L'offerta arriva quando il giocatore ha già un'abitudine, nel momento di soddisfazione, ed è rinviabile con un tocco.

### (d) Cercare un Gioco nell'archivio e giocarlo nello Studio

| # | Passaggio | Dove | Tempo stimato |
|---|---|---|---|
| 1 | Home → *Cerca e gioca* (oppure il campo di ricerca in Home, che salta al passo 2) | Studio | 2 s |
| 2 | Passo 1 "Che cosa cerchi?": testo libero e 3 domande facoltative (tipologia, Rivista o originali, periodo) | Percorso | 15 s |
| 3 | Passo 2: risultati con anteprima della griglia; sceglie | Percorso | 10 s |
| 4 | Passo 3: *Gioca* a schermo intero; all'uscita "Torna ai risultati" | Lettore | 3 s |

**Totale: 4 passaggi, circa 30 secondi.**

## 5. Stati da non dimenticare

| Stato | Dove | Comportamento previsto |
|---|---|---|
| **Vuoto** | Home dello Studio al primo avvio | Le 3 scelte più un quarto riquadro *Prova con un esempio*, che esegue *Acquisisci* su un PDF dimostrativo e *Crea* su un Gioco già preparato |
| Vuoto | *Da riprendere* senza elementi | La sezione non compare (nessun riquadro vuoto) |
| Vuoto | Passo 2 di *Cerca e gioca* senza risultati | "Nessun Gioco trovato" più le domande del passo 1 già compilate e modificabili sulla stessa schermata |
| **Caricamento** | Passaggio da un passo al successivo | Il passo nuovo si mostra subito con segnaposto; il pulsante *Avanti* diventa "Sto preparando..." e non si può premere due volte |
| **Errore dell'AI** | *Crea giochi*, passo 2 | Il passo 2 mostra "Non sono riuscito a generare proposte: (causa)". Tre uscite: *Riprova*, *Torna al passo 1 e cambia le istruzioni*, *Scrivilo a mano* (passa al passo 3 con campi vuoti). Nessun dato perso |
| Errore dell'AI | *Acquisisci*, passo 3 | Le pagine non lette diventano un elenco al passo 4 ("5 pagine non lette: *Riprova* · *Inserisci a mano* · *Salta*") |
| **Elaborazione lunga** | *Acquisisci*, passo 3; generazione di più Giochi | Il passo 3 mostra avanzamento, tempo stimato e invita a chiudere. Il percorso finisce in *In corso* in Home con la barra di avanzamento; notifica del desktop alla fine. Non serve tenere la finestra aperta |
| **Conflitto** | Stesso percorso aperto in due finestre | La seconda apre "Questo percorso è aperto in un'altra finestra": *Continua qui* (l'altra va in sola lettura) |
| Conflitto | Giorno scelto in *Pubblica* già occupato | Al passo 2 i giorni occupati non sono selezionabili, con il motivo in testo |
| Conflitto | Percorso ripreso dopo che i dati sono cambiati (per esempio tipologia confermata da un altro percorso) | Alla ripresa: "Da quando hai lasciato questo percorso: la tipologia Bifronte è stata confermata". Il passo 5 diventa già completato |
| Conflitto | Rivista già acquisita | Al passo 1: "Questa Rivista c'è già". *Aprila* o *Acquisisci di nuovo* |
| **Offline (Lettore)** | Sito | Come nelle altre proposte: progressi in locale, verifica tramite impronta della soluzione, sincronizzazione. Il passo "Promemoria" offline mette la scelta in attesa e la invia alla riconnessione, dicendolo |
| Offline | Studio | I percorsi già aperti restano consultabili; i passi che chiedono l'AI mostrano "Questo passo richiede la connessione" e propongono *Salva ed esci* |
| **Abbandono a metà** (specifico di questa proposta) | Ogni percorso | Salvataggio automatico a ogni passo; *Salva ed esci* sempre visibile; alla chiusura della finestra nessuna domanda, il percorso è già salvato. I percorsi sospesi da più di 30 giorni vengono proposti per l'archiviazione, mai cancellati senza chiedere |

## 6. Accessibilità

| Tema | Scelta in questa proposta |
|---|---|
| **Tastiera** | Ogni passo è una pagina corta: Tab segue l'ordine di lettura, Invio sul pulsante principale avanza, Alt+freccia sinistra torna indietro, Esc apre *Salva ed esci*. Nessuna scorciatoia a un solo carattere, quindi nessun conflitto con i lettori di schermo |
| Tastiera nel Lettore | Uguale nelle tre proposte (vedi proposta A) |
| **Lettore di schermo, griglia** | Base comune: `role="grid"`, annuncio di definizione, posizione e incroci, **modalità elenco**. In questa proposta il Benvenuto del Sito chiede, la prima volta e con un controllo esplicito, se si preferisce "griglia" o "elenco di parole"; la scelta resta memorizzata sul dispositivo |
| Lettore di schermo, percorsi | Indicatore dei passi come elenco ordinato con `aria-current="step"`; a ogni cambio di passo il fuoco va al titolo h1 del passo ("Passo 4 di 6: Rivedi le pagine"). Errori dei moduli con riepilogo in cima collegato ai campi |
| **Bersagli** | Sito: 48x48 px per i pulsanti principali, 44x44 per le caselle, vista parola ingrandita per le griglie larghe; un solo pulsante principale per schermata, distanziato dagli altri. Studio: 24x24 px minimi, pulsanti dei passi alti 40 px |
| **Testo ingrandibile** | Sito: testo base 20 px (il più grande delle tre proposte), zoom libero, ricomposizione al 400%; la struttura a una colonna regge bene l'ingrandimento. Studio: unità relative; affiancamento immagine e dati che passa a una colonna sotto i 900 px |
| Altro | Nessun limite di tempo; esiti in testo; movimento ridotto; il messaggio di fine non usa animazioni obbligatorie |

## 7. Valutazione

**Punti di forza**
- Si impara da soli: ogni procedura spiega il proprio passo. Ottimo dopo una pausa di due settimane, per una demo a un editore e per i futuri redattori.
- Il contributo umano è documentato nel modo più esplicito e difendibile: letto e confermato a ogni Gioco.
- Lo Schema dinamico è introdotto con una procedura dedicata, comprensibile anche a chi non conosce il concetto di schema.
- Sito pubblico più adatto ai giocatori anziani: una scelta per schermata, testo grande, offerta di abbonamento al momento giusto e rinviabile.

**Punti deboli**
- A regime è lento: chi conosce lo strumento attraversa riepiloghi e conferme che non gli servono (circa 3 minuti in più per Gioco creato rispetto ad A).
- Le procedure sono rigide: qualcosa che non rientra in un percorso (correggere un solo campo di un Gioco già pubblicato, rinominare una Tipologia) richiede un percorso nuovo o un'uscita verso *Strumenti*.
- Ogni nuova funzione richiede un nuovo percorso, quindi più lavoro di progettazione nel tempo.
- Nel Sito chi vuole esplorare trova poco: l'Archivio è nascosto in *Altro*.

**Costo di realizzazione relativo: alto.** Servono un motore dei percorsi con salvataggio a ogni passo e ripresa anche dopo giorni, percorsi che sopravvivono a elaborazioni lunghe in sottofondo, la gestione dei dati cambiati nel frattempo e un percorso per ogni caso d'uso. Il numero di schermate è il più alto delle tre (ogni passo è una schermata).

**Rischio principale.** Le procedure fotografano il modo di lavorare di oggi. Quando cambia (lotti di 20 Giochi, editori che portano 50 numeri, un autore partner) i percorsi diventano gabbie e il proprietario li aggira usando gli *Strumenti*, che non sono stati progettati come superficie principale. Mitigazione: tenere i percorsi corti e far nascere ogni percorso da azioni che esistono anche fuori dal percorso.
