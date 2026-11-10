# DBEnigmistica - Tre proposte di esperienza d'uso

Data: 10 novembre 2026. Destinatario: il proprietario, che sceglie una proposta. Ambito: architettura dell'informazione, flussi e struttura delle schermate. Colori e stile visivo sono del lavoro parallelo dell'UI Designer e qui non compaiono.

| Proposta | Documento | Wireframe |
|---|---|---|
| A - Il Banco | `proposta-A.md` | `wireframe-A.html` (10 schermate) |
| B - L'Archivio | `proposta-B.md` | `wireframe-B.html` (11 schermate) |
| C - I Percorsi | `proposta-C.md` | `wireframe-C.html` (10 schermate) |

I wireframe sono file HTML autonomi in scala di grigi: si aprono nel browser, l'indice a sinistra porta a ogni schermata, i pulsanti con bordo spesso seguono il flusso. Ogni file contiene schermate desktop (Studio, Admin) e mobile (Sito pubblico) e una schermata con gli stati.

## Convenzioni

- **Passaggio**: una decisione o un'azione dell'utente che cambia schermata o stato. Scrivere dentro un campo non conta come passaggio.
- **Tempi**: stime mie, non misure. Valgono come confronto tra le proposte; i valori assoluti vanno sostituiti con quelli della prova decisiva 1 del documento `docs/business/04-duello-strategico.md` (costo reale per gioco).
- **Giochi di riferimento**: (a) crittografia breve creata con il Motore di creazione, il formato quotidiano proposto dal documento 04; (b) settimanale di 96 pagine con 80 Giochi, di cui 4 di una tipologia nuova.
- **Regola comune alle tre proposte**: un Gioco con provenienza "Rivista" non può mai essere pubblicato. In ogni proposta il comando non esiste nell'interfaccia ed è bloccato anche lato server.
- **Lettore comune**: in tutte e tre le proposte il Lettore è un solo componente, con modalità griglia e modalità elenco, vista parola ingrandita su mobile e verifica che funziona anche senza rete.

## Sintesi delle tre proposte

**A - Il Banco.** Nello Studio tutto è un elemento di una sola coda con uno stato e un'azione successiva: Giochi da rivedere o verificare, Schemi da confermare, Riviste in estrazione, errori. Una Postazione di revisione passa da un elemento al successivo, quasi tutta da tastiera, e il registro del contributo umano si compila dalle azioni. L'Admin è la coda proseguita; il Sito apre direttamente sul gioco del giorno.

**B - L'Archivio.** Il prodotto è un archivio di oggetti (Riviste, Giochi, Tipologie, Pubblicazioni) con elenco filtrabile, scheda e indirizzo propri. Le azioni nascono dall'oggetto. Studio e Admin sono applicazioni separate; il Sito è un catalogo con il gioco del giorno in evidenza.

**C - I Percorsi.** Il prodotto offre poche procedure guidate con passi numerati, riepilogo e ripresa: Crea, Acquisisci, Pubblica, Cerca e gioca. La Home dello Studio chiede "Che cosa vuoi fare?". Il Sito è un solo percorso quotidiano lineare, a caratteri grandi, con l'offerta di abbonamento dopo qualche giorno di gioco.

## Confronto sui flussi

### Passaggi e tempo stimato

| Flusso | A - Il Banco | B - L'Archivio | C - I Percorsi |
|---|---|---|---|
| **(a) Creare e pubblicare un Gioco originale** (crittografia breve) | 6 passaggi per Gioco + 2 per lotto | 11 passaggi (9 in modalità a lotti) | 9 passaggi (7 ripetendo le impostazioni) |
| Tempo per Gioco rivisto | **circa 8 min** | circa 13 min (11 a lotti) | circa 11 min (9,5 ripetendo) |
| Stesso flusso con cruciverba 9x9 | circa 20 min | circa 27 min | circa 24 min |
| **(b) Acquisire una Rivista** (80 Giochi, 1 tipologia nuova) | 2 di avvio + 1-2 per Gioco + 4 per lo Schema | 5 di avvio + circa 3 per Gioco + 6 per lo Schema | 6 passi di percorso + circa 2 per Gioco + 3 per lo Schema |
| Tempo per Gioco rivisto | **circa 1,0 min** | circa 1,8 min | circa 1,4 min |
| Tempo per numero | circa 82 min | circa 150 min | circa 115 min |
| **(c) Giocatore mobile**: dal link alla prima lettera | **1** (circa 5 s) | 3 (circa 20 s) | 2 la prima volta, poi 1 (circa 15 s, poi 5 s) |
| Attivare il promemoria | 2 | 3 (serve un account) | 2 |
| Giorno dopo, fino al gioco | 1 | 2-3 | 1 |
| Abbonarsi | 4 | 5 | 4 |
| **(d) Cercare un Gioco e giocarlo nello Studio** | **3 (circa 15 s)** | 4 (circa 30 s), ricerca più potente | 4 (circa 30 s) |

### Che cosa significa per la settimana del proprietario

Settimana tipo: 7 crittografie per il gioco quotidiano e 1 numero di Rivista.

| | A | B | C |
|---|---|---|---|
| Creazione (7 x tempo per Gioco) | 56 min | 77-91 min | 67-77 min |
| Acquisizione (1 numero) | 82 min | 150 min | 115 min |
| **Totale** | **circa 2 ore e 20** | **circa 4 ore** | **circa 3 ore e 10** |
| Quota di una settimana da 5 ore | 47% | 80% | 63% |
| Quota di una settimana da 10 ore | 23% | 40% | 32% |

Con 5 ore a settimana B lascia meno di un'ora al prodotto; A ne lascia quasi tre.

### Altri criteri

| Criterio | A - Il Banco | B - L'Archivio | C - I Percorsi |
|---|---|---|---|
| Costo di realizzazione relativo | Medio | **Basso** | Alto |
| Facilità di apprendimento | Media (va imparata la coda e i tasti) | Alta (modello noto) | **Molto alta** |
| Velocità a regime | **Molto alta** | Bassa | Media |
| Ripresa dopo una pausa di due settimane | Alta (la coda dice che cosa fare) | Media | **Alta** |
| Adatto ai futuri redattori di editori | Medio (viste per ruolo da aggiungere) | **Alto** | Alto per chi inizia, basso per chi lavora a volume |
| Documentazione del contributo umano | Automatica, con blocco se manca un atto creativo | Scheda dedicata, testo obbligatorio | **Esplicita e confermata a ogni Gioco** |
| Sito per giocatori over 65 da mobile | Alta (1 tocco) | Media (catalogo da attraversare) | **Molto alta** (una scelta per schermata, testo 20 px, guida la prima volta) |
| Ricerca e inventario | Buona (tavolozza) | **Ottima** (faccette, viste salvate) | Sufficiente |
| Funziona anche senza Motore di estrazione (Strategia C del documento 04, che lo rinvia) | Sì: la coda ha solo meno tipi di elementi | Sì: Riviste resta vuoto | Sì: un percorso in meno |
| Rischio principale | Coda che cresce più in fretta di quanto si smaltisca; conferme distratte | Tempo perso in navigazione: si mangia la produzione | Percorsi rigidi che diventano gabbie quando il lavoro cambia |

## Raccomandazione

**Raccomando la proposta A, Il Banco, come struttura di Studio e Admin, con il Sito pubblico preso dalla proposta C e le schede degli oggetti prese dalla proposta B.**

**Perché A.** Il vincolo che governa il prodotto è il tempo di revisione del proprietario: il documento 04 lo indica come causa di sconfitta della Strategia A commerciale e come voce decisiva della prova 1 (gioco quotidiano sotto i 30 minuti, correzione sotto le 3 ore per numero). A è l'unica proposta che riduce la navigazione quasi a zero e concentra l'attenzione dove serve, ordinando per scadenza del calendario e per confidenza. Sulla settimana tipo libera circa 1 ora e 40 rispetto a B e circa 50 minuti rispetto a C. Il registro del contributo che si compila dalle azioni toglie burocrazia senza togliere prova legale. Inoltre la macchina a stati su cui A si regge serve comunque, a qualunque proposta, per il registro e per la regola "Rivista mai pubblicata": il costo medio di A è in buona parte un costo che le altre due rimandano.

**Che cosa prenderei da C**
1. **Tutto il Sito pubblico**: il percorso "Gioca oggi" con "Come si gioca" solo la prima volta, la scelta tra griglia ed elenco di parole, testo base 20 px, un solo pulsante principale per schermata, l'offerta di abbonamento a fine gioco dopo 3-7 giorni e rinviabile, il riepilogo del pagamento in linguaggio comune. È la soluzione migliore per giocatori over 65 da mobile e costa poco, perché il Sito ha poche schermate. Da A terrei solo la barra in basso con *Oggi* e *Archivio*, più facile da trovare del menu *Altro*.
2. **Lo Schema dinamico guidato la prima volta**: la procedura in 3 tappe (esempi, campi, conferma) si apre al primo Schema proposto; dal secondo in poi si usa la scheda compatta di A.
3. **"Prova con un esempio"** al primo avvio e la regola "ogni errore offre un'uscita che non perde il lavoro".

**Che cosa prenderei da B**
1. **Le schede degli oggetti come livello secondario**: dalla coda, Maiusc+Invio apre la scheda di Gioco, Rivista o Tipologia con un indirizzo proprio (Storia, Provenienza, Pubblicazioni, versioni dello Schema). Senza questo livello A non risponde a "fammi vedere tutta la Rivista 41" e non regge i redattori futuri.
2. **Le faccette nella ricerca** come seconda modalità della tavolozza Ctrl+K e le viste salvate.
3. **Le versioni dello Schema** con migrazione esplicita dei Giochi esistenti.

**Costo della combinazione: medio**, circa quello di A più il Sito di C (poche schermate) e due tipi di scheda di B (componenti standard).

**Rischio della combinazione e come presidiarlo.** Il rischio resta quello di A: la coda diventa un muro quando entra una Rivista intera, e il ritmo veloce porta a conferme distratte. Presidi: la vista *Da fare* mostra solo ciò che serve al calendario dei prossimi 7 giorni; le Riviste in verifica hanno una vista propria; la Postazione mostra il tempo medio per Gioco e, a scelta, un controllo a campione ogni 20 conferme.

**Quando sceglierei un'altra proposta**
- **B**, se il proprietario sceglie la Strategia B del documento 04 (contatti diretti con almeno 2 editori) e i redattori degli editori diventano gli utenti principali entro un anno.
- **C**, se lo Studio sarà usato soprattutto da persone diverse dal proprietario e saltuariamente (per esempio un autore partner pagato a gioco), oppure se servono soprattutto demo guidate agli editori.

## Decisioni che restano al proprietario

1. Accetti che Studio e Admin abbiano lo stesso involucro (A) o li vuoi separati (B)?
2. Scorciatoie a un solo tasto nello Studio: attive per default (più veloci) o da attivare (più sicure)?
3. Registro del contributo: basta il registro automatico con blocco se manca un atto creativo (A) o vuoi una frase scritta e una conferma per ogni Gioco (C)? La risposta dipende dal parere legale sulla L. 132/2025 previsto dal documento 04.
4. Promemoria per i giocatori: email, icona sulla schermata Home o entrambi? Notifiche web no, perché su molti telefoni richiedono passaggi che un giocatore anziano non completa.
5. Dopo quanti giorni di gioco proporre l'abbonamento (3, 5 o 7)?

## Fonti e metodo

- Requisiti: `docs/PROMPT-INIZIALE.md` (UX-01..04 riepilogo dei giochi e proposta di schema, DYN-01..05 Schema dinamico, CLI-02..06 scansione di cartella e file, descrizione di un gioco a un agente AI: qui coperti da *Importa Rivista* e da "nuovo format di gioco" nel Motore di creazione).
- Contesto commerciale: `docs/business/04-duello-strategico.md` (tempo di revisione come collo di bottiglia, Strategia C con Lettore prima e Motore di estrazione rinviato, prova decisiva 1, divieto di ripubblicare le Riviste).
- Stato tecnico: `docs/VALUTAZIONE-STATO-ATTUALE.md` (nessun lettore web esistente; Schema dinamico da riprogettare).
- Tipologie: `docs/giochi_enigmistici.md`.
- Linee guida UX dalla skill ui-ux-pro-max (bersagli di 44 px su mobile e 24 px su web secondo WCAG 2.2, navigazione da tastiera e collegamento "salta", testo base di almeno 16 px e ricomposizione senza scorrimento orizzontale, colore mai unico portatore di significato, regioni live per gli aggiornamenti, ripristino dagli errori, salvataggio automatico dei moduli lunghi, barra in basso con al massimo 5 voci, collegamenti profondi). La ricerca nella base dati della skill per "code di revisione" non ha dato risultati: le scelte sulla coda di A sono mie.
