# DBEnigmistica - Duello strategico tra le strategie A, B e C

Documento 04 della serie business. Data: 10 novembre 2026.
Destinatari: Product Manager (piano commerciale definitivo) e proprietario (scelta della strada).
Fonti: solo i documenti 01 (ricerca di mercato), 02 (strategia e competitor) e la valutazione tecnica. Nessuna nuova ricerca web.

**Convenzioni**
- **[D1]**: fatto preso dal documento 01. **[D2]**: fatto preso dal documento 02. **[VT]**: fatto preso da `docs/VALUTAZIONE-STATO-ATTUALE.md`.
- **[INF]**: mia inferenza o stima, con le ipotesi dichiarate. Tutti i punteggi, le probabilità, le ore e le soglie delle prove sono [INF].
- Gli stratagemmi compaiono solo dove nominano una mossa concreta. Il punteggio va da 1 (pessimo per il proprietario) a 5 (ottimo).

---

## Sintesi (10 righe)

1. **Vince la Strategia C** (lettore più vetrina prima, editori dopo), 22 punti su 30, contro 17 di B e 15 di A. Vince a condizione di avere date e soglie di uscita fisse, altrimenti diventa un rinvio della scelta.
2. **La Strategia A, quella chiesta dal proprietario, perde come impresa**: i formati italiani classici costano lavoro d'autore per ogni gioco, l'archivio cresce di poche centinaia di giochi l'anno e il prezzo di riferimento è 5 € il primo anno [D2].
3. A si salva solo se il gioco quotidiano è un formato a basso costo di revisione e se i giochi originali si vendono anche in licenza, non solo in abbonamento.
4. **La Strategia B perde per questo proprietario**, non in assoluto: senza contatti con gli editori il ciclo di vendita è una guerra di logoramento che il part-time perde. In più la correzione umana di un archivio assorbe le stesse ore che B prometteva di risparmiare [INF].
5. B torna in testa (circa 20 punti) se il proprietario ha già un contatto diretto con almeno 2 editori o archivi.
6. Il motore di estrazione è il pezzo meno difendibile [D2]. C lo costruisce per ultimo e solo su richiesta di un editore. Il lettore interattivo, che serve a tutte e tre le strade, si costruisce per primo.
7. Equilibrio favorevole al piccolo entrante: formati di nicchia sotto la soglia di interesse dei grandi e ruolo di fornitore, non di rivale, di chi ha già il pubblico (RCS, quotidiani locali, Keesing).
8. Equilibrio sfavorevole: cruciverba generico, "Settimana Enigmistica digitale" in concorrenza aperta con Bresi, white label generico contro Keesing e Amuse Labs, motore venduto da solo.
9. Tre prove decisive entro il mese 4: costo reale di produzione e correzione per gioco, 10 editori contattati con richiesta di demo sul loro PDF, abitudine e prevendita sul gioco quotidiano.
10. Prima di tutto servono le risposte del proprietario (sezione 11): ore, contatti, chi produce i giochi, disponibilità a vendere.

---

## 1. Parametri del duello

```
===========================================================
  STRATEGY DUEL - DBENIGMISTICA
===========================================================
  Tipo di gioco : ingresso in un mercato maturo e di nicchia,
                  con incumbent forti e un bene digitale
                  facilmente replicabile
  Dinamica      : gioco ripetuto (abitudine quotidiana dei
                  giocatori) + contrattazione con pochi
                  acquirenti (editori) + minaccia di imitazione
  Sfidanti      : A "Sito per giocatori"
                  B "Archivio giocabile per editori"
                  C "Ibrido: lettore e vetrina, poi editori"
  Attaccanti    : Bresi (Settimana Enigmistica), RCS/Corriere
                  Giochi (gruppo Cairo), Keesing, app
                  internazionali, sviluppatore imitatore con
                  lo stesso modello AI, editori come acquirenti
  Turni         : 3 attacchi per strategia + verdetto
===========================================================
```

**Punto di partenza tecnico [VT].** Nulla funziona oggi: il progetto compila ma nessun comando gira, la pipeline PDF restituisce pagine bianche e **non esiste alcun lettore web**. Tutte e tre le strategie partono quindi da zero sul lettore. Il patrimonio riutilizzabile è il modello di dominio, il database e il catalogo delle tipologie.

**Vincoli del proprietario** (dalla richiesta e da [D2], sezione 9): una sola persona, tempo parziale, budget ridotto, nessuna rete commerciale nota verso gli editori, contenuti delle riviste non ripubblicabili, giochi originali da far rivedere a una persona, tempi lunghi di vendita verso gli editori.

**Ipotesi di capacità [INF]:** in assenza di risposta del proprietario uso 10 ore a settimana, cioè circa 250 ore in sei mesi. Se le ore sono 5, ogni tempo di questo documento raddoppia.

---

## 2. I giocatori e i loro incentivi

| Giocatore | Cosa vuole (incentivo) | Mosse credibili | Mosse non credibili | Fonte |
|---|---|---|---|---|
| **Bresi (La Settimana Enigmistica)** | Proteggere circa 50 mln di ricavi da carta con margine del 26%; le vendite calano e sono sostenute dagli aumenti di prezzo | Ignorare un piccolo entrante. Diffida per marchio o somiglianza se un sito ricorda la testata o le sue rubriche. Migliorare l'app LaSE | Lanciare un digitale gratuito e ottimo delle proprie "varietà": cannibalizzerebbe la carta. Non lo esclude, ma non lo farebbe per reazione a un entrante piccolo [INF] | [D1] [D2] |
| **RCS / Corriere Giochi** (gruppo Cairo, che pubblica anche Enigmistica Più e Mia) | Fidelizzare gli abbonati al giornale con giochi quotidiani; ha già squadra, app e 18 mesi di investimento | Aggiungere un formato che un indipendente ha dimostrato funzionare (rebus o crittografia del giorno). Comprare contenuti invece di produrli se costa meno | Comprare il lettore o il motore di un indipendente: ha già una piattaforma propria [INF] | [D2] |
| **Keesing** (36 testate italiane, piattaforma white label B2B) | Vendere la propria piattaforma e i propri contenuti a media ed editori | Ignorare le tipologie italiane classiche (mercato piccolo). Fare da partner o canale. Copiare un'idea solo se il mercato si dimostra grande | Investire per primo sulle crittografie italiane [INF] | [D2] |
| **App internazionali** (CodyCross, Easybrain, FgCos) | Volumi globali, pubblicità e acquisti in app | Restare sul cruciverba generico localizzato | Entrare su rebus e crittografie italiane: richiedono autori italiani e non scalano [INF] | [D1] [D2] |
| **Sviluppatore imitatore** con lo stesso modello AI | Lo stesso spazio, a costo quasi nullo | Ricostruire il motore "in settimane" [D2]. Inondare il web di giochi AI non rivisti. Offrire agli editori lo stesso servizio a prezzo più basso | Produrre giochi con revisione umana di qualità ogni giorno: costa lo stesso lavoro che costa al proprietario [INF] | [D2] |
| **Editori medi e piccoli e archivi** (acquirenti di B) | Ricavi digitali senza rischio; oggi vendono già il PDF su Readly e LaSE | Aspettare. Chiedere una demo gratuita. Pilota a basso prezzo se c'è un caso dimostrato | Firmare in fretta con un fornitore sconosciuto e part-time [INF] | [D2] |
| **Giocatori** | Divertimento gratuito, abitudine quotidiana; i solutori abituali pagano già circa 100 € l'anno di carta | Giocare gratis; pagare poco per l'archivio se l'abitudine è forte | Pagare più di 25-30 € l'anno a un marchio sconosciuto [INF] | [D1] |

**Lettura [INF].** Due fatti strutturali governano tutto il duello.
1. **Il mercato delle tipologie italiane classiche è troppo piccolo per interessare i grandi.** Questo protegge il piccolo entrante, ma limita anche quanto può guadagnare. Vale per A, B e C.
2. **Il contenuto originale costa lavoro umano per ogni pezzo** e non si automatizza senza perdere tutela e qualità [D1] [D2]. Chi lo produce ha l'unico vantaggio non copiabile. Chi non lo produce compete sul prezzo con l'imitatore.

---

## 3. Duello 1: Strategia A "Sito per giocatori" sotto attacco

```
-----------------------------------------------------------
  ROUND 1/3 - L'attacco del prezzo zero
-----------------------------------------------------------
  ATTACCANTE : RCS/Corriere Giochi + Bresi
  Mossa      : gioco del giorno gratuito di marchi noti,
               abbonamento solo giochi a 5 euro il 1o anno
  Concetto   : ancoraggio di prezzo / concorrenza alla
               Bertrand sul gioco gratuito
  Effetto    : il prezzo massimo credibile di A e' 20-30
               euro l'anno, con conversione 1-3%
  Danno      : GRAVE
-----------------------------------------------------------
```

Fatti: Corriere a 5 € il primo anno, poi 24,99 € l'anno; gioco quotidiano gratuito della Settimana Enigmistica [D2]; 9% di italiani paga per le news online [D1]. Scenario base del documento 01: servono 50.000 utenti attivi al mese per circa 17.000 € netti da abbonamenti [D1].

Inferenza: 50.000 utenti mensili per un marchio nuovo, senza budget di acquisizione, sono il 4% di tutta l'audience online del genere. Il caso "Parole" (1,1 milioni di visite al mese) non è un riferimento valido: è un clone di Wordle, i cui contenuti sono una lista di parole e non costano nulla a ogni gioco. I formati di A costano invece lavoro d'autore ogni giorno. Lo scenario realistico per A pura senza un gioco virale è quello prudente: **circa 2-3.000 € l'anno** [D1].

```
-----------------------------------------------------------
  ROUND 2/3 - L'attacco del costo di produzione
-----------------------------------------------------------
  ATTACCANTE : il calendario del proprietario (e l'imitatore)
  Mossa      : obbligo di 7-15 giochi nuovi a settimana
               rivisti da una persona, per sempre
  Concetto   : guerra di logoramento; costi marginali
               asimmetrici (l'imitatore pubblica AI non
               rivista a costo zero)
  Effetto    : la produzione assorbe le ore che servono
               per lettore, SEO e pagamenti
  Danno      : GRAVE
-----------------------------------------------------------
```

Fatti: nello scenario base servono 7-15 giochi nuovi a settimana rivisti da una persona; il collo di bottiglia è la produzione, non il software [D1]. Il rebus richiede illustrazioni originali [D1]. I giochi senza contributo umano rilevante non sono tutelati (L. 132/2025) e quindi si possono copiare [D1] [D2].

Inferenza sul tempo: creare e rivedere bene una crittografia o un cruciverba piccolo richiede 20-45 minuti; un rebus con illustrazione e verifica richiede di più. Con 10 giochi a settimana servono 4-8 ore, cioè quasi tutte le 10 ore disponibili. Resta poco o nulla per costruire il prodotto.

Inferenza sull'archivio: con 5-10 giochi a settimana l'archivio cresce di 250-500 giochi l'anno. Il Guardian vende un archivio di oltre 15.000 giochi [D2]. Un abbonamento "per l'archivio" non ha valore credibile prima di due o tre anni.

```
-----------------------------------------------------------
  ROUND 3/3 - L'attacco dell'imitazione
-----------------------------------------------------------
  ATTACCANTE : RCS/Corriere Giochi
  Stratagemma: n. 12 "Prendere la capra di passaggio"
  Mossa      : se una "crittografia del giorno" di A
               diventa popolare, il Corriere aggiunge
               l'ottavo gioco con i propri autori
  Concetto   : vantaggio del secondo arrivato; i formati
               di gioco non sono tutelati
  Danno      : MEDIO (alto solo se A ha successo)
-----------------------------------------------------------
```

Fatti: le regole e le tipologie di gioco non sono tutelate, solo l'espressione [D2]; il Corriere ha già importato formati altrui (Wordle, Spelling Bee) [D1].

Inferenza: è una minaccia che scatta solo dopo il successo. Il proprietario può trasformarla in vendita: proporsi come fornitore del formato prima che il Corriere lo copi (stratagemma n. 30, "scambiare i ruoli di ospite e padrone di casa").

**Dove cede A.** Cede sul modello di ricavo, non sul prodotto. Il gioco quotidiano italiano originale ha senso; l'abbonamento all'archivio come motore economico no. Cede anche sulla capacità: A è l'unica strategia che obbliga a produrre ogni settimana per sempre, a prescindere dai ricavi.

**Cosa succede se i grandi reagiscono.** Bresi non ha incentivo a reagire finché A resta piccola e lontana dalle sue "varietà". Se A punta esplicitamente a essere "la Settimana Enigmistica giocabile", rischia una diffida su marchio e somiglianza: è lo stratagemma n. 13 al contrario, "battere l'erba e spaventare il serpente". Il Corriere reagisce solo imitando un formato che ha già funzionato.

**Cosa salverebbe A [INF].**
1. Un gioco quotidiano con **basso costo di revisione** per pezzo, per esempio crittografia breve, anagramma a tema o catena di parole, invece di rebus illustrati ogni giorno.
2. **Un autore partner** (un enigmista pagato a gioco o a quota dei ricavi) che tolga la produzione dal tempo del proprietario.
3. **Vendere i giochi originali anche in licenza** a quotidiani locali e siti, ipotesi già indicata dal documento 01, così ogni gioco viene pagato più volte.
4. Rinunciare all'abbonamento nei primi 12 mesi e misurare solo abitudine e prevendita.

Con queste quattro modifiche A smette di essere A e diventa il lato consumer di C.

---

## 4. Duello 2: Strategia B "Archivio giocabile per editori" sotto attacco

```
-----------------------------------------------------------
  ROUND 1/3 - L'attacco dell'attesa
-----------------------------------------------------------
  ATTACCANTE : gli editori acquirenti
  Stratagemma: n. 4 "Aspettare riposati il nemico stanco"
  Mossa      : "interessante, ci risentiamo dopo l'estate"
  Concetto   : guerra di logoramento; opzione esterna
               dell'acquirente (non fare nulla) quasi gratuita
  Effetto    : il primo euro slitta oltre il mese 9-12
  Danno      : GRAVE (per un part-time senza contatti)
-----------------------------------------------------------
```

Fatti: il documento 02 indica come rischio principale "pochi acquirenti e cicli di vendita lunghi" e fissa il primo pilota tra i mesi 6 e 9 [D2]. Gli editori vendono già il PDF su Readly e LaSE [D2]. Il proprietario non ha esperienza di vendita B2B documentata [D2].

Inferenza: per un editore la scelta "non fare nulla" non costa nulla e non comporta rischi. Un fornitore part-time e sconosciuto aumenta il rischio percepito, e chi decide in un editore medio ha altre priorità. Senza un contatto caldo, la probabilità di un pilota pagato entro il mese 9 è bassa, nell'ordine del 20-30%. Il conto alla rovescia gioca contro il proprietario, che perde motivazione mentre l'editore non perde niente.

```
-----------------------------------------------------------
  ROUND 2/3 - L'attacco del lavoro nascosto
-----------------------------------------------------------
  ATTACCANTE : la realta' della correzione
  Mossa      : il pilota da "un'annata o 50 numeri" va
               verificato gioco per gioco
  Concetto   : costi marginali non nulli; il servizio
               scala con le ore, non con il software
  Effetto    : un solo cliente satura il proprietario
  Danno      : GRAVE
-----------------------------------------------------------
```

Fatti: il documento 02 prevede un pilota di un'annata o 50 numeri, soglie di accuratezza del 90% sui cruciverba e dell'80% sulle altre tipologie, e la correzione umana inclusa nel servizio [D2].

Inferenza, con ipotesi esplicite: un numero di settimanale contiene 60-100 giochi. Con un'accuratezza dell'80-90% bisogna comunque **verificarli tutti**, perché non si sa quali sono sbagliati. Con 2 minuti di verifica a gioco più la correzione degli errori servono 3-5 ore a numero. Un pilota di 50 numeri richiede 150-250 ore, cioè **15-25 settimane** a 10 ore a settimana. Un contratto ricorrente su un settimanale richiede circa 150-250 ore l'anno. Il prezzo per numero dovrebbe superare i 100-150 € per pagare il tempo a 30 € l'ora, e l'editore deve vedere un ricavo digitale che lo giustifichi.

Conclusione: B non elimina il collo di bottiglia della revisione umana che affligge A. Lo sposta su contenuti altrui, pagati ma non scalabili. Con 10 ore a settimana il tetto è **uno o due clienti**.

```
-----------------------------------------------------------
  ROUND 3/3 - L'attacco dell'imitatore e del gigante
-----------------------------------------------------------
  ATTACCANTE : sviluppatore imitatore + Keesing
  Stratagemma: n. 24 "Chiedere il passaggio per conquistare Guo"
  Mossa      : Keesing accetta una "partnership", vede il
               lettore e il metodo, poi estende la propria
               piattaforma; l'imitatore offre lo stesso
               servizio a meta' prezzo all'editore successivo
  Concetto   : hold-up; concorrenza alla Bertrand con
               pochi acquirenti (potere di monopsonio)
  Danno      : MEDIO
-----------------------------------------------------------
```

Fatti: il motore si replica in settimane con lo stesso modello [D2]; Keesing ha archivi italiani e una piattaforma white label [D2]. Il documento 02 propone contratti "con dati di proprietà del cliente ma lettore e strumenti nostri" [D2].

Inferenza: se i dati corretti sono del cliente, il costo per cambiare fornitore è solo il lettore, che è il pezzo "medio" in difendibilità. Il fossato dichiarato dal documento 02 ("contratti e dati strutturati") è più sottile di quanto appaia. Il rischio Keesing è però contenuto dalla dimensione del mercato: le tipologie italiane classiche sono poco interessanti per un gruppo internazionale. Il pericolo reale è l'imitatore locale.

**Dove cede B.** Sul tempo al primo euro, sulla dipendenza da pochi decisori e sul lavoro di correzione. Regge invece benissimo sul piano legale, perché lavora sui contenuti di chi ha i diritti, ed è l'unica strada che usa il motore come prodotto.

**Cosa succede se i grandi reagiscono.** Bresi non è un cliente e non ha motivo di reagire. RCS e Cairo, appartenenti allo stesso gruppo, hanno già una squadra giochi interna: per loro conviene fare in casa, quindi sono acquirenti meno probabili di quanto suggerisca la lista del documento 02 [INF]. Keesing risponde solo se il mercato si dimostra grande, e allora vince lei.

**Cosa salverebbe B [INF].** Un contatto diretto con almeno due editori o archivi; prezzi a quota dei ricavi digitali invece che a forfait, per togliere all'editore il rischio di non fare nulla; un pilota piccolo (5-10 numeri, non 50); dati corretti concessi al cliente in licenza, non ceduti.

---

## 5. Duello 3: Strategia C "Ibrida" sotto attacco

**Definizione di C usata qui.** Mesi 1-4: lettore web per 2 tipologie e vetrina pubblica con un gioco quotidiano originale a basso costo di revisione. In parallelo, 10 contatti con editori e archivi. Il motore di estrazione si costruisce **solo** dopo che un editore chiede una demo sul proprio PDF. La prima demo si fa a mano nello Studio, dichiarandolo. Al mese 4 c'è un cancello di decisione con soglie numeriche.

```
-----------------------------------------------------------
  ROUND 1/3 - L'attacco della dispersione
-----------------------------------------------------------
  ATTACCANTE : il calendario del proprietario
  Mossa      : due mercati, due messaggi, mezze ore su
               ciascuno: nessuno dei due prodotti finito
  Concetto   : costo opportunita'; il documento 02 indica
               la dispersione come rischio n. 4
  Danno      : MEDIO
-----------------------------------------------------------
```

Fatti: il documento 02 classifica "capacità del singolo e dispersione tra sito, Studio, amministrazione e motore" come probabilità alta e impatto alto [D2].

Difesa [INF]: in C il lavoro comune alle due strade è il lettore, circa il 60% delle ore dei primi quattro mesi. Le parti divergenti sono piccole: la produzione di contenuti è limitata a un gioco al giorno di un solo formato, la vendita a 2 ore a settimana di contatti. Il motore, la parte più costosa di B, è rinviato. La dispersione resta il rischio principale di C e si controlla solo con il cancello del mese 4.

```
-----------------------------------------------------------
  ROUND 2/3 - L'attacco del conflitto di canale
-----------------------------------------------------------
  ATTACCANTE : gli editori acquirenti
  Mossa      : "Ci chiedete di diventare nostri fornitori
               mentre gestite un sito che ci fa concorrenza?"
  Concetto   : conflitto di canale; credibilita' dell'impegno
  Danno      : MEDIO
-----------------------------------------------------------
```

Inferenza: è l'attacco più serio contro C. La difesa ha tre parti.
1. La vetrina usa solo formati che gli editori non pubblicano in digitale (crittografie, giochi classici) [D2].
2. La vetrina si presenta come dimostrazione del lettore, con un marchio neutro.
3. Il gioco quotidiano stesso si offre in licenza agli editori: lo stratagemma n. 17, "gettare un mattone per ottenere una giada". Il gioco gratuito è il mattone, il contratto è la giada.

Se un editore pone comunque l'esclusiva come condizione, il proprietario può spegnere la vetrina: è lo stratagemma n. 21, "la cicala lascia la pelle". Il lettore resta e cambia solo dove viene pubblicato.

```
-----------------------------------------------------------
  ROUND 3/3 - L'attacco del rinvio
-----------------------------------------------------------
  ATTACCANTE : il proprietario stesso
  Mossa      : "tengo aperte entrambe le strade" diventa
               "non scelgo mai"
  Concetto   : valore d'opzione contro costo dell'attesa;
               un'opzione vale solo se ha una scadenza
  Danno      : GRAVE se non ci sono soglie, LIEVE se ci sono
-----------------------------------------------------------
```

Difesa [INF]: il cancello del mese 4 (sezione 9) con soglie numeriche e una scelta binaria. Senza quel cancello C perde contro B, perché B almeno impone una scadenza commerciale.

**Dove cede C.** Rende meno di B nel caso in cui il proprietario abbia già contatti con gli editori: in quel caso il lavoro consumer è un costo inutile. E rende meno di una A ben fatta se un formato quotidiano diventa virale, perché metà delle ore sono andate agli editori.

**Perché regge meglio delle altre due [INF].** Costruisce prima il pezzo che serve in ogni scenario (il lettore) e per ultimo il pezzo più replicabile (il motore). È lo stratagemma n. 8, "riparare apertamente le strade e marciare di nascosto su Chencang": in pubblico c'è un sito di giochi, in sostanza si costruisce la demo per gli editori. Raccoglie poi due segnali di mercato indipendenti con lo stesso lavoro. Infine è la più reversibile: dopo sei mesi entrambe le strade restano aperte e nessuna ora è stata spesa su un componente utile a una sola di esse, salvo le circa 40-60 ore di contenuti.

---

## 6. Teoria dei giochi: dove c'è un equilibrio favorevole al piccolo entrante

### 6.1 I sottogiochi

| Sottogioco | Struttura | Equilibrio | Favorevole al proprietario? |
|---|---|---|---|
| **Entrante contro Bresi** | Gioco d'ingresso sequenziale: l'entrante entra, l'incumbent accetta o combatte | Se l'entrante resta piccolo e su formati diversi dalle "varietà", la risposta migliore di Bresi è ignorarlo: combattere costa e rischia di cannibalizzare la carta. È la logica della "judo economics": l'entrante limita da sé la propria scala per non provocare l'incumbent | **Sì**, finché il proprietario non imita testata, marchio o "varietà" in modo riconoscibile |
| **Entrante contro RCS/Corriere** | Imitazione dopo il successo; scelta di RCS tra fare e comprare | Se un formato funziona, RCS lo copia (costo basso) oppure lo compra. Compra solo se il fornitore offre qualità e continuità che le costerebbe di più produrre | **Sì, come fornitore**; **no, come rivale** |
| **Entrante contro Keesing** | Partner potenziale con potere contrattuale schiacciante | Keesing collabora solo se il proprietario copre tipologie che lei non vuole fare; se il mercato cresce, la sua mossa migliore è integrarle | **Sì, ma instabile**: usare Keesing come canale, mai come unico cliente |
| **Entrante contro imitatore AI** | Concorrenza alla Bertrand su un bene replicabile | Sul motore e sui giochi AI non rivisti il prezzo tende a zero. Sui giochi con revisione umana e sui contratti già firmati vince chi arriva prima | **No** sul motore, **sì** su contenuto rivisto e primo contratto |
| **Proprietario contro editore acquirente** | Contrattazione con opzione esterna quasi gratuita (non fare nulla) | Il dominio dell'attesa: la strategia dominante dell'editore è aspettare, a meno che il pilota costi zero e mostri un ricavo. Il proprietario deve cambiare i payoff: demo sul PDF dell'editore, pilota piccolo, prezzo a quota dei ricavi | **No** con l'offerta standard, **sì** con un pilota senza rischio per l'editore |
| **Proprietario contro giocatori** | Gioco ripetuto di abitudine quotidiana | L'abitudine si forma su un gioco breve, quotidiano e condivisibile. Pagano gli abituali, non gli occasionali [D1] | **Sì**, se il costo di produzione per gioco è basso |

### 6.2 Controllo dell'equilibrio di Nash

- **A pura**: l'equilibrio (A piccola su formati di nicchia, Bresi ignora, Corriere ignora) è stabile ma **povero**: nessuno ha incentivo a deviare perché non c'è nulla da prendere. Se A cresce, il Corriere devia (imita) e l'equilibrio si rompe a sfavore di A.
- **B pura**: l'equilibrio più probabile è (proprietario offre, editore aspetta): stabile e **sfavorevole**. Si sposta solo cambiando i payoff dell'editore.
- **C**: non esiste un equilibrio unico, ed è il punto. C sceglie dopo aver visto le mosse degli altri, con un'informazione che A e B non hanno. In termini di teoria dei giochi compra l'informazione con circa 60 ore di lavoro in più rispetto alla strada più corta.

### 6.3 Mosse da evitare in ogni strategia

- Usare nomi, grafica o rubriche che ricordino La Settimana Enigmistica: invita la sola reazione credibile di Bresi, quella legale [D2].
- Competere sul cruciverba generico mobile: mercato già occupato dalle app internazionali [D1].
- Vendere il motore da solo: è la merce che l'imitatore offre a prezzo zero [D2].
- Pubblicare anche un solo gioco estratto da riviste di terzi: rischio alto e ora anche penale per l'estrazione in violazione [D2].

---

## 7. Vincoli reali del proprietario, pesati

| Vincolo | Peso su A | Peso su B | Peso su C |
|---|---|---|---|
| Una sola persona | Alto: produzione e prodotto insieme | Alto: vendita, correzione e prodotto insieme | Medio: lettore comune, il resto si attiva dopo |
| Tempo parziale (ipotesi 10 ore a settimana) | Alto: la produzione settimanale consuma quasi tutte le ore | Alto: un pilota di 50 numeri vale 15-25 settimane | Medio |
| Nessuna rete commerciale verso gli editori | Nullo | **Decisivo**: è la differenza tra B vincente e B perdente | Medio: 10 contatti a freddo come prova |
| Contenuti delle riviste non ripubblicabili | Alto: tutto deve essere originale | Nullo: si lavora sui contenuti del cliente | Medio: originali solo per la vetrina |
| Giochi originali rivisti da una persona | **Decisivo**: è il costo principale | Basso (ma la correzione dell'archivio è lo stesso tipo di lavoro) | Medio: un formato, cadenza bassa |
| Tempi lunghi di vendita verso gli editori | Nullo | **Decisivo** | Medio: si vende in parallelo, senza aspettare |
| Budget ridotto | Medio: niente acquisizione a pagamento | Basso | Basso |

**Lettura [INF].** A è uccisa dal vincolo di produzione, B dal vincolo della rete commerciale. C è l'unica strategia in cui nessun vincolo è decisivo, ma nessun vantaggio è massimo.

---

## 8. Verdetto e punteggi

### 8.1 Tabella dei punteggi

Scala da 1 a 5, dove 5 è il migliore per il proprietario. Rischio legale e dipendenza: 5 = rischio o dipendenza bassi. Tutti i valori sono [INF], motivati nelle righe sotto.

| Criterio | A - Sito per giocatori | B - Archivio per editori | C - Ibrida | Motivazione sintetica |
|---|:-:|:-:|:-:|---|
| Tempo al primo euro significativo (circa 1.000 € cumulati) | 2 | 2 | 3 | A: abbonamenti trascurabili prima dei mesi 12-18. B: pilota tra i mesi 6-9 secondo [D2], 9-15 senza contatti. C: due canali, vince il primo che paga (licenza del gioco quotidiano o pilota) |
| Capitale e tempo richiesti | 2 | 3 | 4 | A: produzione settimanale senza fine. B: motore, Studio, lettore e ore di vendita e correzione. C: lettore e un formato; motore solo su richiesta |
| Difendibilità | 2 | 3 | 3 | A: formati non tutelati, imitabili da RCS e dall'imitatore. B: contratto e integrazione, ma dati del cliente e motore replicabile. C: lettore e catalogo rivisto, più eventuali contratti |
| Rischio legale | 3 | 5 | 4 | A: alto volume di pubblicazione, rischio di somiglianza con giochi esistenti e di scarsa tutela dei giochi AI. B: lavora per chi ha i diritti. C: pubblica poco e solo originali |
| Dipendenza da terzi | 3 | 1 | 3 | A: Google per il traffico, fornitore AI. B: pochi decisori, Keesing, fornitore AI. C: nessun terzo singolo decisivo |
| Reversibilità (costo di cambiare strada dopo sei mesi) | 3 | 2 | 5 | A: il lettore resta, i contenuti servono come demo. B: motore e Studio non servono a un sito per giocatori. C: progettata per scegliere al mese 4-6 |
| **Totale su 30** | **15** | **17** | **22** | |
| Ricavo potenziale a 24 mesi (solo indicativo, non sommato) | 2-3 mila € l'anno (prudente), 25-35 mila nel base con 50.000 utenti mensili [D1] | 15 mila € contrattualizzati al mese 12 se supera G4 [D2], tetto di 1-2 clienti con 10 ore a settimana [INF] | Somma parziale dei due, con tetto dato dalle ore | |

**Variante condizionale [INF].** Se il proprietario ha già un contatto diretto con almeno 2 editori o archivi, B sale a circa 20 punti: tempo al primo euro 4, dipendenza 2. A quel punto B e C si equivalgono e conviene B, perché è più semplice.

```
===========================================================
  VERDETTO DEL DUELLO
===========================================================
  Vincitore : Strategia C (ibrida), 22/30
  Secondo   : Strategia B, 17/30 (20/30 con contatti caldi)
  Terzo     : Strategia A, 15/30
  Nash      : A e B convergono su equilibri stabili ma
              sfavorevoli (A piccola e povera; editore che
              aspetta). C non si ferma su un equilibrio:
              compra informazione e sceglie al mese 4-6.
  Condizioni: cancello al mese 4 con soglie numeriche;
              motore costruito solo su richiesta di un
              editore; un solo formato quotidiano a basso
              costo di revisione; marchio neutro.
===========================================================
```

### 8.2 Perché la strategia chiesta dal proprietario perde

A perde perché unisce il costo più alto (produrre ogni settimana per sempre) al ricavo più conteso (abbonamenti contro offerte gratuite di marchi noti). Il prodotto di A non è sbagliato: il documento 01 conferma il vuoto sulle tipologie italiane giocabili [D1]. È sbagliato il modo in cui A dovrebbe pagarsi. La salvano le quattro modifiche della sezione 3, che di fatto la trasformano nel lato consumer di C.

### 8.3 Condizioni alle quali il verdetto cambia

| Se il proprietario... | allora vince | Perché |
|---|---|---|
| ha contatti diretti con almeno 2 editori o archivi | **B** | Il vincolo decisivo di B sparisce |
| ha un autore partner che produce 5 o più giochi a settimana | **A modificata** (in pratica C con più peso sul consumer) | Il vincolo decisivo di A sparisce |
| non vuole vendere a editori in nessun caso | **A modificata**, con obiettivo dichiarato di progetto personale che si ripaga | B e C perdono metà del loro valore |
| ha meno di 5 ore a settimana | **Nessuna come impresa**: solo un gioco quotidiano come hobby | Ogni tempo di questo documento raddoppia |

---

## 9. Sequenza consigliata per i primi sei mesi (Strategia C)

Ore indicative con l'ipotesi di 10 ore a settimana [INF].

| Periodo | Mossa | Ore | Uscita attesa |
|---|---|---|---|
| Settimane 1-2 | Risposte del proprietario alla sezione 11. Prova decisiva 1 (costo di produzione). Richiesta di parere legale breve sui punti della sezione 2.3 del documento 02 | 15 | Ore a gioco misurate; decisione sul formato quotidiano |
| Mesi 1-2 | Lettore web per 2 tipologie: una quotidiana a basso costo di revisione (proposta: crittografia breve) e una di base (proposta: cruciverba classico). Campo provenienza e licenza nel database [D2]. Nessun motore | 70 | Lettore funzionante su desktop e mobile, con verifica, aiuti e caratteri grandi [D1] |
| Mesi 1-2, in parallelo | Prova decisiva 2: 10 contatti con editori e archivi, chiedendo un colloquio, non vendendo | 15 | Lista di chi risponde, chi decide, quali archivi e formati hanno |
| Mese 3 | Vetrina pubblica con marchio neutro, 30-40 giochi originali in coda, iscrizione a newsletter o notifica. Prova decisiva 3 al via | 40 | Gioco del giorno online |
| Mesi 3-4 | Demo a ogni editore interessato **sul suo PDF**: un numero inserito a mano nello Studio e giocabile nel lettore, dichiarando che è fatto a mano | 30 | Richiesta di pilota o rifiuto motivato |
| **Fine mese 4** | **Cancello di decisione** (vedi sotto) | 5 | Una sola strada |
| Mesi 5-6, ramo editori | Motore di estrazione solo per le 2 tipologie e solo sui PDF dell'editore; pilota di 5-10 numeri, a pagamento o a quota dei ricavi | 50 | Primo contratto o lettera d'intenti |
| Mesi 5-6, ramo giocatori | Prevendita "socio fondatore" annuale; licenza del gioco quotidiano a 3-5 quotidiani locali o siti | 50 | Primi euro da giocatori o licenze |
| Fine mese 6 | Secondo cancello: continuare, cambiare ramo o fermarsi (stratagemma n. 36, "la ritirata") | 5 | Decisione scritta |

**Cancello del mese 4 [INF, soglie da confermare con Product Manager e Pricing Analyst].**
- **Ramo editori** se almeno 1 editore ha chiesto un pilota o una proposta economica dopo la demo sul proprio PDF.
- **Ramo giocatori** se il gioco quotidiano ha almeno 500 iscritti e almeno il 30% torna ogni settimana, e se il costo misurato resta sotto le 3 ore a settimana.
- **Entrambi i segnali**: priorità agli editori, perché pagano prima; la vetrina resta come demo.
- **Nessuno dei due**: fermarsi come impresa e tenere il gioco quotidiano come hobby.

---

## 10. Prove decisive (economiche, da fare presto)

| # | Prova | Quando | Costo | Come si misura | Soglia di superamento [INF] | Se fallisce |
|---|---|---|---|---|---|---|
| 1 | **Costo reale per gioco**: il proprietario crea e rivede 5 giochi originali per tipologia candidata, cronometrando; corregge a mano 1 numero di una rivista che possiede (uso personale, nulla pubblicato) | Settimane 1-2 | Circa 10 ore, 0 € | Minuti per gioco originale; ore per numero corretto | Gioco quotidiano sotto i 30 minuti a pezzo; correzione sotto le 3 ore a numero | Sopra i 30 minuti: A e il lato consumer di C richiedono un autore partner. Sopra le 5 ore a numero: B non regge a forfait, serve un prezzo a ore |
| 2 | **Domanda degli editori**: 10 contatti con editori e archivi (lista del documento 02 esclusa Bresi, con RCS e Cairo trattati come un solo gruppo) per un colloquio di 30 minuti, poi una demo sul loro PDF | Mesi 1-4 | Circa 30 ore, 0 € | Risposte, colloqui, richieste di demo, richieste di prezzo | Almeno 3 colloqui entro il mese 2; almeno 1 richiesta di pilota o di prezzo entro il mese 4 | Meno di 3 colloqui: B è chiusa per questo proprietario, salvo un partner commerciale. Equivale al cancello G2 del documento 02, anticipato |
| 3 | **Abitudine e disponibilità a pagare dei giocatori**: gioco quotidiano gratuito, iscrizione per ricevere il gioco, poi prevendita "socio fondatore" a prezzo annuale sotto i 25 € | Mesi 3-6 | Circa 40 ore più 0-200 € di promozione | Iscritti, ritorno settimanale, numero di prevendite pagate | Almeno 500 iscritti e il 30% di ritorno settimanale al mese 4; almeno 50 prevendite al mese 6 | Il lato consumer resta un hobby o una demo; il ricavo deve venire dalle licenze o dagli editori |

Le soglie sono mie e prudenti. Il Product Manager le deve trasformare in obiettivi del piano. Il principio è che **ogni prova costa meno di un mese di lavoro e può chiudere una strada da sola**.

---

## 11. Decisioni che spettano solo al proprietario

1. **Qual è l'obiettivo?**
   - (a) un reddito integrativo di almeno X € l'anno entro 24 mesi;
   - (b) un progetto personale che si ripaga le spese;
   - (c) un prodotto da vendere o cedere a un editore.
   Con (a) conviene C con priorità agli editori; con (b) basta A modificata; con (c) conviene B.
2. **Quante ore a settimana, per 12 mesi, e quante di queste per vendere e parlare con le persone?**
   - (a) meno di 5;
   - (b) 5-10;
   - (c) più di 10.
   Con (a) nessuna strategia regge come impresa.
3. **Hai già contatti diretti con editori, archivi, autori o associazioni (ARI, BEI, Studio Giochi)?**
   - (a) sì, con almeno 2 decisori;
   - (b) conoscenze indirette;
   - (c) nessuno.
   Con (a) il verdetto passa a B.
4. **Sei disposto a lavorare per conto di editori, sotto il loro marchio?**
   - (a) sì;
   - (b) solo tramite un partner (Keesing, Studio Giochi);
   - (c) no, voglio un marchio mio per i giocatori.
   Con (c) restano solo A modificata e il lato consumer di C.
5. **Chi produce i giochi originali?**
   - (a) tu;
   - (b) un autore pagato a gioco;
   - (c) un autore a quota dei ricavi;
   - (d) una comunità di autori con licenza nei termini d'uso.
   Con (a) il formato quotidiano deve costare meno di 30 minuti a gioco.
6. **Quale formato come gioco quotidiano?**
   - (a) crittografia breve (basso costo, vuoto di mercato);
   - (b) rebus (gancio più forte, illustrazione costosa);
   - (c) cruciverba (familiare, ma affollato).
7. **Quale marchio per la vetrina?**
   - (a) neutro e tecnico, come demo del lettore, compatibile con gli editori;
   - (b) forte e rivolto ai giocatori, in conflitto con il ruolo di fornitore.
8. **Budget per i primi sei mesi (AI, hosting, parere legale, eventuale autore)?**
   - (a) meno di 500 €;
   - (b) 500-2.000 €;
   - (c) più di 2.000 €.
9. **Nei contratti con gli editori, i dati corretti sono**
   - (a) ceduti al cliente;
   - (b) concessi in licenza al cliente finché dura il contratto;
   - (c) condivisi.
   Con (a) il fossato si riduce al solo lettore.
10. **Accetti i cancelli del mese 4 e del mese 6, compreso lo stop come impresa?**
    - (a) sì, così come sono;
    - (b) sì, con soglie diverse da indicare;
    - (c) no.
    Con (c) C degenera in un rinvio della scelta e conviene scegliere subito B o A modificata.

---

## 12. Cosa resta incerto

- Tutte le ore per gioco e per numero sono stime: la prova decisiva 1 le sostituisce con misure in due settimane.
- La probabilità di un pilota pagato senza contatti (20-30% entro il mese 9) è una stima mia, senza dati di riferimento sul settore.
- Il ricavo per licenza a un quotidiano locale non è noto: il documento 02 non ha trovato listini B2B pubblici di Keesing né del piano editori di Amuse Labs [D2].
- La reazione di Bresi è dedotta dagli incentivi economici (margini sulla carta, politica digitale chiusa), non da dichiarazioni.
