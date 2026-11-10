# DBEnigmistica: cinque proposte di interfaccia

Ogni proposta è un file HTML autonomo in questa cartella (`proposta-1.html` … `proposta-5.html`; confronto in `index.html`). Tutte mostrano le stesse quattro schermate, navigabili dalla barra in alto:

1. **Sito**: pagina iniziale con il gioco del giorno gratuito, l'archivio per tipologia e l'abbonamento.
2. **Lettore**: cruciverba 5×5 compilabile (clic o tocco sulla casella, digitazione, frecce, barra spaziatrice per cambiare direzione, Invio per la parola successiva, Backspace) con evidenziazione della parola e della definizione attiva, verifica, aiuto "una lettera" e anagramma con campo soluzione.
3. **Studio**: banco del Motore di creazione con richiesta, bozza generata e modificabile, controlli di revisione, stato (bozza → rivisto → approvato), provenienza e diritti; un secondo elemento della coda mostra la revisione di un Gioco estratto da una Rivista accanto all'immagine della pagina.
4. **Admin**: dashboard di utilizzo (periodo selezionabile, indicatori, grafico per giorno, ripartizione per tipologia), coda di pubblicazione con "Pubblica ora" e impostazioni dell'aspetto del sito.

Parti comuni a tutte: tema chiaro e scuro (segue il dispositivo, poi ricorda la scelta), testo di base da 18 a 20 px, bersagli di tocco di almeno 44 px, focus visibile, collegamento "Vai al contenuto", rispetto di `prefers-reduced-motion`, icone SVG (nessuna emoji). Lo schema del cruciverba e i contenuti sono gli stessi ovunque, così il confronto riguarda solo l'interfaccia.

Contenuti di gioco usati: cruciverba MARE / ORO / LUNA / CASA (orizzontali) e MOLO / RONDA / ERA / FA (verticali); anagramma ATTORE → TEATRO; sciarada PARA + SOLE = PARASOLE; zeppa CASA + U = CAUSA; ditloide "7 G della S"; anagramma estratto ROMA → AMOR. La Rivista «Enigmi d'Autunno» e la firma «G. P., Milano» sono inventate.

I rapporti di contrasto qui sotto sono calcolati con la formula WCAG 2.x sui valori esatti dei token di ciascun file.

---

## 1. Carta: editoriale da carta stampata

**Idea.** La pagina dei giochi di un quotidiano: testata grande in corsivo, data e numero dell'uscita, filetti doppi e colonne separate da un filo, un rosso tipografico come unico richiamo. Niente schede né ombre: la gerarchia sta nella tipografia. Lo Studio e l'Admin usano gli stessi filetti; gli indicatori sono grandi cifre in corsivo separate da linee verticali.

**Palette**

| Ruolo | Chiaro | Scuro |
|---|---|---|
| Sfondo (carta) | `#F4EFE4` | `#161411` |
| Superficie | `#FBF8F1` | `#1F1C18` |
| Inchiostro | `#1A1714` | `#EEE7D9` |
| Testo secondario | `#574F45` | `#BCB19F` |
| Rosso tipografico (richiamo, errori) | `#A3221A` | `#F28C80` / collegamenti `#F5A398` |
| Stato intermedio | `#1F4E79` | `#A9C9EA` |
| Casella attiva / parola attiva | `#F0C24F` / `#E6D8B8` | `#7A5B12` / `#3B3326` |
| Conferma | `#2B6436` | `#8FCB97` |

**Caratteri.** Newsreader (titoli e testo, serif pensato per la lettura su schermo, con corsivo per la testata) e Libre Franklin (etichette, pulsanti, numeri dello schema), entrambi Google Fonts.

**Per chi funziona meglio.** Per il solutore abituale della rivista di carta, che riconosce subito il registro; per un prodotto che vuole presentarsi come "testata" con un'uscita al giorno.

**Punti deboli.** Etichette e pulsanti in maiuscoletto spaziato a 0,8 rem sono meno leggibili per chi ha più di 65 anni. Il serif ha bisogno di schermi nitidi. Il rosso fa sia da richiamo sia da errore, quindi lo stato d'errore si distingue anche per la sottolineatura e il messaggio. Sul desktop la colonna del gioco del giorno resta vuota in basso.

**Contrasto dei testi principali**

| Coppia | Chiaro | Scuro |
|---|---|---|
| Testo / sfondo | 15,6:1 | 14,9:1 |
| Testo secondario / sfondo | 7,0:1 | 8,7:1 |
| Pulsante primario | 7,5:1 | 7,7:1 |
| Collegamenti / sfondo | 6,5:1 | 9,3:1 |
| Lettera / casella attiva | 10,7:1 | 5,1:1 |
| Errore / sfondo | 6,5:1 | 9,3:1 |

---

## 2. Banco: essenziale da strumento di lavoro

**Idea.** Un'interfaccia da strumento: barra superiore scura fissa, ricerca nell'archivio in primo piano, tabelle con cifre monospaziate, schede con bordo sottile e raggio piccolo, stati scritti in carattere monospaziato. È la proposta più adatta all'uso intensivo dello Studio (coda, bozza e provenienza su tre colonne) e dell'Admin (indicatori in una griglia unica).

**Palette**

| Ruolo | Chiaro | Scuro |
|---|---|---|
| Sfondo | `#F4F5F7` | `#0E1114` |
| Superficie | `#FFFFFF` | `#161A1F` |
| Testo | `#15181C` | `#E6E9ED` |
| Testo secondario | `#4F5761` | `#9FA8B2` |
| Blu inchiostro (richiamo) | `#1E4FD0` | `#86A6FF` / collegamenti `#9DB6FF` |
| Barra superiore | `#15181C` | `#06080A` |
| Casella attiva / parola attiva | `#FFD966` / `#DDE6FB` | `#6A5410` / `#1C2944` |
| Conferma / errore | `#1B7342` / `#C0251C` | `#6FD39A` / `#FF8A80` |

**Caratteri.** IBM Plex Sans (testo e titoli) e IBM Plex Mono (lettere dello schema, cifre, etichette di stato).

**Per chi funziona meglio.** Per il proprietario e per chi lavora nello Studio e nell'Admin ogni giorno; per giocatori esperti che cercano nell'archivio più che sfogliare.

**Punti deboli.** È la proposta con meno personalità: sul sito pubblico ricorda un gestionale più che l'enigmistica. Le etichette monospaziate sono meno leggibili del testo normale. La barra scura fissa occupa spazio verticale sui telefoni piccoli.

**Contrasto dei testi principali**

| Coppia | Chiaro | Scuro |
|---|---|---|
| Testo / sfondo | 16,3:1 | 15,5:1 |
| Testo secondario / sfondo | 6,7:1 | 7,9:1 |
| Pulsante primario | 6,8:1 | 8,0:1 |
| Collegamenti / sfondo | 6,2:1 | 9,5:1 |
| Lettera / casella attiva | 13,0:1 | 6,0:1 |
| Errore / sfondo | 5,5:1 | 8,3:1 |

---

## 3. Salotto: caldo e giocoso

**Idea.** Il tavolino di casa con le tessere: bordi spessi color cacao, ombre piene sfalsate (non sfumate), angoli tondi, tessere di lettere inclinate nell'illustrazione d'apertura. Saluto personale ("Buongiorno!"), serie di giornate di fila, tipologie in una griglia di dimensioni e colori diversi, abbonamento proposto anche come regalo.

**Palette**

| Ruolo | Chiaro | Scuro |
|---|---|---|
| Sfondo (crema) | `#FFF5E8` | `#1D1611` |
| Superficie | `#FFFFFF` | `#2A2019` |
| Testo (cacao) | `#2B1D14` | `#F8EBDC` |
| Testo secondario | `#6A5241` | `#D2BDA7` |
| Terracotta (richiamo) | `#B83F26` | `#F28B6B` / collegamenti `#F7A487` |
| Verde petrolio | `#1C6A66` | `#6FC7BE` |
| Zafferano | `#F2B33D` | `#F2B33D` |
| Casella attiva / parola attiva | `#F2B33D` / `#FCE2AE` | `#7E520A` / `#4A3824` |

**Caratteri.** Fraunces (titoli, lettere dello schema; asse "soft" per un tono morbido) e Figtree (testo e controlli).

**Per chi funziona meglio.** Per il pubblico che gioca per piacere e per abitudine quotidiana, nonni e nipoti, chi apprezza un'accoglienza calorosa; utile per la fidelizzazione (serie, regalo).

**Punti deboli.** Le ombre piene e i bordi spessi appesantiscono le schermate dense dello Studio e dell'Admin. Fraunces a corpo piccolo perde leggibilità, per questo è usato solo da 1,1 rem in su. Il saluto e la serie richiedono un utente riconosciuto. Il tono può sembrare infantile a chi risolve crittografie difficili.

**Contrasto dei testi principali**

| Coppia | Chiaro | Scuro |
|---|---|---|
| Testo / sfondo | 15,1:1 | 15,2:1 |
| Testo secondario / sfondo | 6,7:1 | 9,9:1 |
| Pulsante primario | 5,6:1 | 7,4:1 |
| Collegamenti / sfondo | 6,3:1 | 9,0:1 |
| Lettera / casella attiva | 8,8:1 | 5,8:1 |
| Errore / sfondo | 5,6:1 | 8,8:1 |
| Testo bianco su riquadro terracotta / verde petrolio | 5,6:1 / 6,3:1 | — |

---

## 4. Lente: classico ad alto contrasto per la leggibilità

**Idea.** Ogni scelta subordinata alla lettura: testo di base a 20 px, nero su bianco (e bianco su nero), bordi di 3 px, una sola colonna anche sul desktop, pulsanti da 56 a 68 px, collegamenti sempre sottolineati, focus con contorno nero più alone giallo. Sulla pagina iniziale c'è il comando "Grandezza del testo" con tre livelli; nel Lettore le caselle arrivano a 84 px e la casella attiva è gialla con contorno spesso in entrambi i temi. Numero verde in evidenza per chi preferisce telefonare.

**Palette**

| Ruolo | Chiaro | Scuro |
|---|---|---|
| Sfondo | `#FFFFFF` | `#000000` |
| Testo | `#000000` | `#FFFFFF` |
| Testo secondario | `#333333` | `#DADADA` |
| Richiamo | blu `#0A3A8C` | giallo `#FFD400` / collegamenti `#FFE45C` |
| Casella attiva / parola attiva | `#FFD400` / `#FFF1A6` | `#FFD400` (lettera nera) / `#3A3300` |
| Caselle nere | `#000000` | `#5A5A5A` (grigio, per distinguerle dal fondo) |
| Conferma / errore | `#0B5E1E` / `#A40000` | `#7CFC8A` / `#FF8F8F` |

**Caratteri.** Atkinson Hyperlegible (unico carattere), disegnato per lettori ipovedenti: lettere facilmente distinguibili (I, l, 1; O, 0).

**Per chi funziona meglio.** Per la maggioranza reale del pubblico: giocatori oltre i 65 anni, su telefono, spesso con vista stanca. È anche la base più solida per un'eventuale verifica di accessibilità.

**Punti deboli.** Austera: poca identità di marchio e poco "piacere" visivo. La colonna unica sul desktop spreca spazio nello Studio e nell'Admin. La casella nera grigia del tema scuro va provata con utenti reali.

**Contrasto dei testi principali**

| Coppia | Chiaro | Scuro |
|---|---|---|
| Testo / sfondo | 21,0:1 | 21,0:1 |
| Testo secondario / sfondo | 12,6:1 | 15,0:1 |
| Pulsante primario | 10,5:1 | 14,7:1 |
| Collegamenti / sfondo | 10,5:1 | 16,5:1 |
| Lettera / casella attiva | 14,7:1 | 14,7:1 (lettera nera su giallo) |
| Errore / sfondo | 8,1:1 | 9,6:1 |

Tutte le coppie superano il livello AAA (7:1).

---

## 5. Tascabile: contemporaneo da app

**Idea.** Un'app da tenere in tasca: navigazione a controllo segmentato, superfici bianche arrotondate su fondo caldo, gioco del giorno in un riquadro scuro con lo schema in miniatura, anello dell'obiettivo settimanale, "Riprendi" con barre di avanzamento. Nel Lettore la barra con Verifica / Aiuto / Ricomincia resta ancorata in basso mentre si scorre, a portata di pollice.

**Palette**

| Ruolo | Chiaro | Scuro |
|---|---|---|
| Sfondo | `#F1F0EB` | `#0D0E0D` |
| Superficie | `#FFFFFF` | `#191A19` |
| Testo | `#121212` | `#F1F0EB` |
| Testo secondario | `#55544D` | `#ADABA2` |
| Verde bottiglia (richiamo) | `#17594A` | `#6FD3B0` / collegamenti `#7FDDBB` |
| Arancio (solo elementi non testuali, etichette) | `#FF6B3D` | `#FF6B3D` |
| Casella attiva / parola attiva | `#FFB89E` / `#D9ECE5` | `#8F3A1C` / `#1D3830` |
| Errore | `#B4321A` | `#FF907A` |

**Caratteri.** Bricolage Grotesque (titoli, lettere dello schema) e Instrument Sans (testo e controlli).

**Per chi funziona meglio.** Per chi gioca solo dal telefono e ha familiarità con le app; per un pubblico più giovane da conquistare; per un'eventuale app installabile.

**Punti deboli.** Lo stile segue le mode e invecchierà prima delle altre. I controlli segmentati e le icone sono meno ovvi per chi usa poco lo smartphone. La barra ancorata può coprire una riga di definizioni sui telefoni piccoli. Il saluto personale richiede l'accesso.

**Contrasto dei testi principali**

| Coppia | Chiaro | Scuro |
|---|---|---|
| Testo / sfondo | 16,4:1 | 17,0:1 |
| Testo secondario / sfondo | 6,7:1 | 8,4:1 |
| Pulsante primario | 8,2:1 | 10,7:1 |
| Collegamenti / sfondo | 7,2:1 | 11,9:1 |
| Lettera / casella attiva | 11,2:1 | 6,6:1 |
| Errore / sfondo | 5,4:1 | 8,8:1 |

---

## Tabella di confronto

Punteggi da 1 (debole) a 5 (forte), giudizio del progettista.

| | Carta | Banco | Salotto | Lente | Tascabile |
|---|---|---|---|---|---|
| Carattere | editoriale | strumento | caldo, giocoso | alta leggibilità | app |
| Testo di base | 19 px | 18 px | 19 px | 20 px (regolabile) | 18 px |
| Contrasto minimo dei testi principali | 5,1:1 | 5,5:1 | 5,6:1 | 8,1:1 | 5,4:1 |
| Leggibilità per chi ha più di 65 anni | 3 | 3 | 4 | 5 | 3 |
| Uso da telefono | 4 | 3 | 4 | 5 | 5 |
| Legame con la tradizione su carta | 5 | 2 | 3 | 3 | 2 |
| Identità e piacere visivo | 5 | 2 | 5 | 2 | 4 |
| Adatto a Studio e Admin | 3 | 5 | 2 | 3 | 4 |
| Durata nel tempo dello stile | 5 | 4 | 3 | 5 | 2 |

## Raccomandazione

Raccomando una **combinazione**, non una proposta sola.

- **Sito pubblico e Lettore: Carta, con le regole di Lente.** Carta dà a DBEnigmistica un'identità riconoscibile e legata alla tradizione su carta senza imitare nessuna testata. Lente porta ciò che il pubblico reale richiede: testo da 20 px, comando "Grandezza del testo", pulsanti da 56 px, collegamenti sottolineati, focus con alone, caselle grandi e casella attiva gialla. In pratica: tipografia, testata e filetti di Carta; misure, contrasti e comportamenti di Lente; etichette in maiuscoletto sostituite da testo normale in grassetto.
- **Studio e Admin: Banco.** Sono strumenti di lavoro per il proprietario, usati a lungo al desktop: tabelle dense, cifre monospaziate e tre colonne (coda, bozza, provenienza) fanno risparmiare tempo. Tema e caratteri possono restare quelli del sito, cambia solo la densità.

Se si vuole una sola proposta, scegliere **Lente**: è l'unica che supera AAA su tutte le coppie principali in entrambi i temi ed è pensata per la maggioranza dei giocatori (oltre 65 anni, su telefono). Le altre si possono rendere accessibili; Lente lo è già.

Da **Tascabile** vale la pena prendere la barra degli aiuti ancorata in basso nel Lettore su telefono; da **Salotto** la serie di giornate e l'abbonamento da regalare, come funzioni e non come stile.

## Cosa resta da verificare

- Prove con utenti reali oltre i 65 anni, soprattutto sul cambio di direzione nello schema (tocco ripetuto sulla stessa casella o barra spaziatrice).
- Lettori di schermo (NVDA, VoiceOver): la struttura c'è (etichette per ogni casella, definizione attiva annunciata, messaggi in regioni `status`), ma non è stata provata con un lettore vero.
- Tastiere virtuali di iOS e Android: la digitazione passa per l'evento `input` ed è stata simulata, non provata su un telefono.
- Senza connessione i caratteri Google non si caricano e le pagine usano i caratteri di sistema indicati come alternativa.
