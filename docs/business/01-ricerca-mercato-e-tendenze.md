# DBEnigmistica - Ricerca di mercato e tendenze

Documento 01 della serie business. Data della ricerca: 10 novembre 2026.
Ambito: domanda, tendenze e dimensione del mercato. Competitor e modello di business sono trattati a parte (Business Strategist).
Destinatari successivi: Pricing Analyst, Product Manager.

**Convenzioni**
- **[V]**: dato verificato, con fonte riportata tra parentesi quadre (es. [F1]). L'elenco completo delle fonti è in fondo.
- **[V2]**: dato con fonte secondaria (aggregatore, stima di uno strumento terzo, sintesi giornalistica di un report non consultato direttamente). Va usato con cautela.
- **[S]**: stima mia, con il calcolo o l'ipotesi esplicita.
- **[N]**: dato cercato e non trovato.

---

## Sintesi (10 righe)

1. La domanda c'è ed è ampia: nel 2024 il 31,5% degli italiani di 14 anni e più ha fatto cruciverba (circa 16 milioni di persone, stima mia), il 6,7% almeno una volta a settimana (ISTAT, 2026).
2. È però in lento calo strutturale (40,3% nel 2000) e sbilanciata verso gli anziani: la pratica settimanale va dal 4,1% dei 14-17enni al 13,1% degli over 65.
3. La carta regge meglio del resto dell'edicola: l'editore della Settimana Enigmistica fattura circa 50 milioni l'anno con il 26% di utile, ma le vendite calano e sono sostenute dagli aumenti di prezzo.
4. Il digitale italiano dei giochi di parole è giovane e frammentato: il Corriere ha lanciato una sezione Giochi solo a luglio 2025 (5 euro il primo anno), e l'app digitale ufficiale della Settimana Enigmistica ha voto 1,9/5.
5. I segnali di domanda online sono forti: un clone italiano di Wordle stimato a circa 1,1 milioni di visite al mese, e un sito di soluzioni per cruciverba stimato a 5,8 milioni di visite al mese, il 95% da mobile.
6. Il vuoto più netto è l'enigmistica tipicamente italiana: rebus, crittografie e giochi della "varietà" da settimanale non hanno un'offerta digitale giocabile di qualità (nessuna app di rebus italiano classico tra i primi risultati del Play Store).
7. I rischi principali sono quattro: propensione a pagare bassa (9% paga per news online), concorrenza degli editori, pubblicità concentrata sulle grandi piattaforme e diritto d'autore (contenuti delle riviste non usabili, giochi solo AI non tutelabili).
8. Verdetto: l'opportunità esiste come nicchia, non come mercato di massa. Per uno sviluppatore singolo il valore realistico a 24 mesi è dell'ordine di decine di migliaia di euro l'anno (scenario base circa 25-35 mila euro). Lo scenario ottimista arriva a qualche centinaio di migliaia di euro, ma richiede licenze B2B o un forte successo di un gioco virale.

---

## 1. Dimensione e andamento del mercato

### 1.1 Praticanti in Italia (dato ufficiale più solido)

Indagine ISTAT "I cittadini e il tempo libero" 2024, pubblicata il 3 settembre 2026 [F1]:

| Indicatore (popolazione 14+) | Valore | Tipo |
|---|---|---|
| Fa cruciverba (almeno una volta nell'anno) | 31,5% | [V] [F1] |
| Fa cruciverba nel 2000 | 40,3% | [V] [F1] |
| Fa cruciverba almeno una volta a settimana | 6,7% (attività di gioco con la frequenza settimanale più alta) | [V] [F1] |
| Donne / uomini che fanno cruciverba | 34,7% / 28,0% | [V] [F1] |
| Pratica settimanale, 14-17 anni / 65+ | 4,1% / 13,1% | [V] [F1] |
| Partecipazione per età (annuale) | "circa una persona su tre in ogni fascia" | [V] [F1] |
| Popolazione 14+ che gioca ad almeno un'attività | 52,9% = 27,51 milioni | [V] [F1] |

Nota: la categoria ISTAT è "cruciverba o simili" e include probabilmente altri giochi enigmistici, ma l'indagine non scorpora rebus, sudoku o logica. **[N]**

Conversioni in valori assoluti **[S]**:
- popolazione 14+ ≈ 27,51 mln / 0,529 ≈ **52,0 milioni**;
- chi fa cruciverba nell'anno ≈ 52,0 × 31,5% ≈ **16,4 milioni**;
- chi li fa ogni settimana ≈ 52,0 × 6,7% ≈ **3,5 milioni**. Questo è il nucleo di giocatori abituali.

Lettura: l'abitudine annuale è trasversale per età. L'intensità invece è anziana: i giovani provano, gli over 65 giocano ogni settimana. In 24 anni il bacino ha perso circa 9 punti, cioè circa un quinto in termini relativi.

### 1.2 Edicola: diffusione e tirature

- **La Settimana Enigmistica (Bresi srl)** [F3, ItaliaOggi, 3/11/2025]:
  - ricavi da vendite quasi 50 milioni di euro nel 2024 (49,7 milioni nel 2023);
  - utile netto 13,3 milioni nel 2024 (margine 26,6%) e 14,9 milioni nel 2023 (30%);
  - circa 300 milioni di ricavi cumulati tra il 2019 e il 2024; **[V]**
  - "oltre 400 mila copie ogni settimana" a 2 euro, dato **non certificato** **[V2]**. La testata non aderisce ad ADS e l'editore non pubblica le tirature; nel 2021 ItaliaOggi citava stime non ufficiali di circa 500 mila copie [F4];
  - nel 2024 c'è stato un "leggero calo" delle vendite, compensato dall'aumento del prezzo di copertina da giugno 2024; per il 2025 l'editore prevede un "fisiologico calo delle vendite" [F3]. **[V]**
  - altri titoli Bresi: Il Mese enigmistica (2,80 euro), Mondo Sudoku (1,80), I Sudoku (1), La Vacanza enigmistica (5) [F3]. **[V]**
- **Numero di testate**: in edicola ci sono circa 50 testate di enigmistica, circa 20 di autodefiniti e circa 10 di puzzle e rebus [F5, AGI, 24/6/2024]. **[V]**
- **Keesing Italia** (filiale del maggiore gruppo europeo del puzzle): fatturato 2024 di circa 3,27 milioni di euro, utile di circa 1,1 milioni [F34]. **[V2]** (aggregatore di bilanci)
- **Tirature e diffusione delle altre testate** (Cairo, Mondadori/Focus, Keesing, Corrierino ecc.): **[N]**. Non esistono dati certificati pubblici.
- **Fatturato totale dell'enigmistica in edicola**: **[N]**. Stima d'ordine **[S]**, confidenza bassa: tra 80 e 150 milioni di euro l'anno. Ipotesi: Bresi, leader di gran lunga, vale circa 50 milioni; le altre circa 75 testate valgono complessivamente da 0,6 a 2 volte Bresi.

**Contesto edicola (in declino strutturale)**:
- 10.720 edicole esclusive o prevalenti a gennaio 2025 (SNAG) [F25] **[V]**;
- -2.667 punti vendita (-16%) tra il 2019 e il 2023 [F26] **[V]**;
- oltre 4.900 comuni senza edicola (UNCEM, 2026) **[V2]**;
- vendite dei quotidiani da circa 4 milioni di copie al giorno nel 2013 a meno di 2 milioni nel 2026 [F27] **[V2]**; -27% in edicola tra il 2022 e il 2025 [F37] **[V2]**.

L'enigmistica soffre meno della stampa d'informazione, ma perde canale fisico: meno edicole significa meno occasioni d'acquisto, soprattutto nei piccoli centri.

### 1.3 Digitale in Italia

- **Audience online del genere**: Audiweb, marzo 2024, 29 testate online di enigmistica (cruciverba, rebus, crittografie, sudoku) [F5]:
  - 1.242.000 utenti, di cui 681 mila donne e 561 mila uomini;
  - area più attiva: Sud e Isole (429 mila). **[V]**
- **Edicola digitale**: Readly ha 48 testate enigmistiche italiane, seconda categoria dopo Disney (132). Nel 2023 cruciverba e sudoku hanno registrato il tempo di lettura più alto, 28 minuti [F5]. **[V]**
- **Spesa B2C in contenuti digitali in Italia** (Osservatorio Digital Content, PoliMi) [F23, ItaliaOggi, 17/2/2026]:
  - circa 4 miliardi di euro nel 2025, +3%;
  - gaming 42%, editoria e informazione 5%, con le news a +6%. **[V]**
  - Ne derivo circa 1,7 miliardi per il gaming e circa 0,2 miliardi per l'editoria **[S]** (quote applicate al totale).
- **Valore del mercato digitale dei giochi di parole e logica in Italia**: **[N]**. Nessuna fonte pubblica lo isola.

### 1.4 Riferimento USA ed Europa

- **New York Times Games**:
  - oltre 11,2 miliardi di partite nel 2025 [F17] **[V2]** (cifra comunicata dal NYT e ripresa da NBC e AP);
  - Wordle: 4,2 miliardi di partite nel 2025, contro 5,3 nel 2024 e 4,8 nel 2023 [F16] **[V2]**. Il gioco simbolo è **in calo**;
  - abbonati totali NYT: 12,8 milioni a fine 2025 [F18] **[V]**;
  - nel primo trimestre 2025 i Giochi avrebbero portato 110 mila dei 250 mila nuovi abbonati netti [F20] **[V2]** (analisi di terzi);
  - prezzo dell'abbonamento solo giochi: circa 50 dollari l'anno, 120 dollari per il piano famiglia [F19] **[V2]**;
  - da fine agosto 2025 Mini Crossword, Tiles e Letter Boxed sono passati dietro paywall, mentre Wordle e Spelling Bee restano gratuiti [F19] **[V2]**.
- **Regno Unito**:
  - Guardian Puzzles app a 3,49 sterline al mese (dal 2020); Telegraph solo giochi a 2,99 sterline al mese [F21] **[V]**;
  - "oltre 1,7 milioni di lettori" usano l'hub puzzle del Guardian [F38] **[V2]**.
- **Mercato globale "games & puzzles"**: le stime dei report a pagamento divergono da 16 a 28 miliardi di dollari per il 2023 [F36]. **Non le uso per il dimensionamento**: perimetri diversi e metodologia non verificabile.
- **App di parole (USA)**: Sensor Tower Q4 2025 indica Wordscapes a circa 517 mila dollari di ricavi nell'ultima settimana di dicembre [F32] **[V2]**. È un dato da "testa del mercato", non replicabile da un indipendente.

---

## 2. Tendenze

| Tendenza | Evidenza | Implicazione per DBEnigmistica |
|---|---|---|
| **Gioco quotidiano stile Wordle** | Cloni italiani di Wordle online da anni [F6]. "Parole" stimato a 1,12 milioni di visite a settembre 2026, 96% Italia [F8] **[V2]**. Wordle globale però in calo dal 2024 [F16]. | Il formato "un gioco al giorno, condivisibile" funziona anche in italiano. Wordle in sé è saturo e protetto: servono formati propri. |
| **Giochi come leva editoriale** | Corriere della Sera, luglio 2025: sezione Giochi e app, 7 giochi, 5 euro il primo anno; obiettivo dichiarato l'engagement degli abbonati, più che l'acquisizione [F6] **[V]**. Altri quotidiani hanno valutato di importare Wordle e hanno rinunciato per problemi di copyright [F6]. Il Post già nel 2022 indicava i giochi come leva sulla disponibilità a pagare [F35]. | Gli editori italiani si muovono adesso: la finestra per un indipendente è aperta, ma si restringe. Gli editori sono anche potenziali clienti B2B (licenza di giochi). |
| **App mobile di cruciverba** | Il Play Store italiano è dominato da sviluppatori internazionali o seriali (FgCos, Easybrain, Fanatee/CodyCross, Fugo) [F15]. La migliore app italiana di cruciverba "classici" ha 546 mila installazioni e 4,8/5, ma ricavi stimati sotto i 10 mila dollari al mese [F11] **[V2]**. | Il cruciverba generico su mobile è affollato e poco remunerativo per installazione. Non è lì il vantaggio. |
| **AI per generare enigmi** | Amuse Labs (PuzzleMe): circa il 35% dei puzzle gratuiti creati sulla sua piattaforma usa l'AI [F29] **[V2]** (dato aziendale). Critiche diffuse sulla qualità (definizioni deboli, incroci scorretti) [F30]. Legge 132/2025: tutela d'autore solo per opere frutto di lavoro intellettuale umano, anche se assistito da AI [F31] **[V]**. | L'AI abbassa il costo di produzione per tutti, quindi il contenuto generico perde valore. Il valore resta nella revisione umana e nella qualità. Senza un contributo umano documentabile, i giochi rischiano di non essere tutelati e di poter essere copiati. |
| **Invecchiamento del pubblico dell'edicola** | Cruciverba dal 40,3% al 31,5% della popolazione 14+ tra il 2000 e il 2024. Pratica settimanale al 13,1% tra gli over 65 contro il 4,1% tra i 14-17enni [F1] **[V]**. Età media dei lettori della Settimana Enigmistica: **[N]**. | Il pubblico pagante attuale è anziano: servono accessibilità (caratteri grandi, tablet) e semplicità. Il pubblico futuro va conquistato con formati brevi e social. |
| **Passaggio al digitale** | Sito di soluzioni per cruciverba (dizy.com): 5,82 milioni di visite a settembre 2026, 97% Italia, 95% da mobile; le query principali sono definizioni di cruciverba [F9] **[V2]**. | Molti solutori su carta usano già lo smartphone come aiuto. Il passaggio "carta + telefono" è in corso e la domanda di aiuti e suggerimenti è enorme. |
| **Pubblicità digitale** | In Italia nel 2025: internet 6,2 miliardi (53% del mercato pubblicitario). Nel 2026 l'83% della raccolta online andrà ai grandi operatori internazionali [F24] **[V2]**. Mobile game advertising globale oltre 12 miliardi di dollari nel 2025 [F33] **[V2]**. eCPM dei casual game in calo su base annua secondo TopOn H1 2025 **[V2]**. | La pubblicità cresce in totale, ma la quota per i piccoli editori si riduce. Non è una base solida per il modello di ricavo. |

---

## 3. Pubblico

| Dimensione | Evidenza | Tipo |
|---|---|---|
| Genere | Le donne fanno più cruciverba (34,7% contro 28,0%) [F1]. Audience online del genere a maggioranza femminile: 681 mila contro 561 mila [F5]. | [V] |
| Età | Partecipazione annuale simile in tutte le fasce (circa 1 su 3); pratica settimanale concentrata negli over 65 (13,1%) [F1]. | [V] |
| Territorio | Online il Sud e le Isole sono l'area più attiva (429 mila utenti) [F5]. Nel complesso del gioco non a premi prevale invece il Nord (57,6% contro 45,9% del Mezzogiorno) [F1]. | [V] |
| Istruzione | Chi ha una laurea gioca di più (63,3% contro 40,1%) [F1]. Il Corriere punta su un "target evoluto" [F6]. | [V] |
| Dispositivi | Tra i videogiocatori online lo smartphone domina (64,4%; 74,3% tra le donne); tablet 16,1% (22,2% tra le donne) [F1]. Il dato riguarda i videogiochi, non l'enigmistica. Traffico dei siti di soluzioni al 95% da mobile [F9]. | [V] / [V2] |
| Carta contro digitale per l'enigmistica | Quota di chi gioca su carta o su schermo: **[N]**. | [N] |
| Quando si gioca | Momenti della giornata e durata delle sessioni per l'enigmistica: **[N]**. Readly: 28 minuti di lettura per cruciverba e sudoku [F5]. | [N] / [V] |
| Disponibilità a pagare | Il 9% degli italiani paga per news online, nella fascia bassa tra i paesi europei [F22]. Prezzi di riferimento: Settimana Enigmistica 2 euro a settimana (circa 104 euro l'anno su carta) [F3]; Corriere Giochi 5 euro il primo anno [F6], poi circa 2,49 euro al mese o 24,99 euro l'anno sull'App Store italiano [F7] **[V2]**; NYT Games circa 50 dollari l'anno [F19]. | [V] / [V2] |
| Pagamenti in-app | Tra i videogiocatori online di 11 anni e più, il 41% gioca a titoli con acquisti in-game e circa metà di loro spende [F1]. Il dato riguarda i videogiochi in generale. | [V] |

**Inferenza [S]**: il solutore abituale paga già circa 100 euro l'anno per la carta. Il segmento digitale giovane, invece, è abituato al gratuito. La disponibilità a pagare più credibile è quella di chi oggi compra in edicola e cerca una versione digitale migliore, non quella dei giocatori occasionali di Wordle.

---

## 4. Segnali di domanda

### 4.1 Volumi di ricerca e traffico (Semrush, dati settembre 2026, stime di terze parti) [V2]

| Parola chiave o sito | Valore | Fonte |
|---|---|---|
| "wordle" (Italia) | 368.000 ricerche al mese | [F8] |
| "wordle italiano" | 40.500 ricerche al mese | [F8] |
| "parole" (in senso di gioco) | 40.500 ricerche al mese | [F8] |
| "parole gioco", "wordle ita" | 6.600 ciascuna | [F8] |
| Sito "Parole" (clone italiano di Wordle) | 1,12 milioni di visite al mese (1,65 milioni ad agosto) | [F8] |
| dizy.com (soluzioni di cruciverba) | 5,82 milioni di visite al mese, 97% Italia, 95% mobile | [F9] |
| Singole definizioni di cruciverba come query | fino a 90.500 ricerche al mese (es. "sono famose le sue danze norvegesi") | [F9] [F10] |
| cruciverba.io | 137 mila visite a settembre 2026 (462 mila a luglio) | [F10] |
| "cruciverba", "rebus", "crittografia", "settimana enigmistica" come parole chiave secche | **[N]**: volumi non pubblicamente disponibili senza accesso a Keyword Planner o Semrush | - |

Lettura **[S]**: le singole definizioni con decine di migliaia di ricerche al mese indicano che milioni di persone risolvono cruciverba su carta (o in app) e cercano aiuto online. È una domanda di **aiuto alla soluzione** più che di nuovi giochi. Resta comunque un pubblico intercettabile via SEO e un indizio di quanto sia mobile.

### 4.2 App italiane (Play Store e App Store, rilevazione al 10/11/2026)

| App | Installazioni / valutazione | Fonte |
|---|---|---|
| Cruciverba classici (FgCos Games) | 546 mila+ installazioni, 4,79/5 su 11 mila voti, ricavi stimati sotto i 10 mila dollari al mese (54% acquisti in-app, 46% pubblicità) | [F11] [V2] |
| Cruciverba - Enigmistica (Digital Taps) | 52 mila+ installazioni, 67 voti, solo pubblicità | [F12] [V2] |
| Giochi di Parole - Cruciverba (Esogames) | 47,5 mila+ installazioni | [F13] [V2] |
| LaSE - Riviste digitali (Bresi, Settimana Enigmistica) | iOS 1,9/5 su 184 voti; Play Store 2,2 | [F14] [F15] [V2] |
| Corriere Giochi (RCS) | iOS 4,7/5 su 29 voti (storefront USA); dati italiani **[N]** | [F7] |

Un dato pesa più degli altri: **l'app digitale ufficiale del leader di mercato ha una valutazione molto bassa**. È una domanda insoddisfatta di "Settimana Enigmistica giocabile bene su schermo".

### 4.3 Comunità

- **Associazione Rebussistica Italiana (ARI)**: meno di 200 tesserati (2023) [F28] **[V]**. Le riviste specialistiche (Penombra, Sibilla, Labirinto, Leonardo) hanno tirature molto basse e si ricevono solo in abbonamento [F28]. **[V]**
- Esistono il Congresso nazionale di enigmistica e la Biblioteca Enigmistica Italiana di Modena (Enignet) [F5] [F28]. **[V]**
- Dimensione delle comunità social (gruppi Facebook, subreddit, Telegram): **[N]**, non verificata.

L'enigmistica "classica" organizzata è una nicchia di poche centinaia di appassionati: utile per qualità e credibilità, irrilevante come mercato.

---

## 5. Spazi non coperti

| Spazio | Evidenza | Valutazione [S] |
|---|---|---|
| **Rebus italiano classico in digitale** | La ricerca "rebus" sul Play Store italiano restituisce solo rebus inglesi o picture puzzle, nessuna app di rebus italiano classico tra i primi risultati [F15]. Le app storiche (iRebus, Treccani Rebus) sono vecchie e solo iOS **[V2]**. | Vuoto reale. Barriera: il rebus richiede illustrazioni originali, costose e oggi in parte producibili con AI più revisione. |
| **Crittografie, giochi in versi, enigmistica "classica"** | Nessuna offerta digitale giocabile trovata. Pubblico ristretto (ARI sotto i 200 soci) [F28]. | Vuoto reale ma piccolo. Da usare come contenuto premium e distintivo, non come prodotto principale. |
| **"Varietà" da settimanale** (bersaglio, pista cifrata, aguzzate la vista, ditloidi, domino, schede senza schema ecc.) | L'offerta ufficiale digitale è un PDF da annotare con voto 1,9/5 [F14]. Nessuna app indipendente trovata che renda giocabili e verificabili queste tipologie. | Spazio più interessante: risponde al pubblico pagante attuale. È il punto in cui il motore di DBEnigmistica (30+ tipologie strutturate) è un vantaggio tecnico. |
| **Aiuti progressivi e verifica** | La domanda di soluzioni è enorme [F9]. | Funzione da includere: suggerimento per lettera o parola e verifica della casella. È un modo per intercettare la domanda SEO. |
| **Accessibilità per gli over 65** | Pubblico settimanale concentrato negli over 65 [F1]; tablet relativamente più usato dalle donne [F1]. | Caratteri grandi, alto contrasto, tablet in orizzontale e stampa a casa sono differenzianti, a basso costo. |
| **Gioco quotidiano italiano originale e condivisibile** | Il Corriere ha "In altre parole" (clone di Wordle), "L'Apegramma" (clone di Spelling Bee), cruciverba e sudoku [F7]. Nessun formato quotidiano basato su rebus o crittografia. | Possibile "gioco gancio" virale: un rebus o una crittografia del giorno, condivisibile come Wordle. |
| **Licenza B2B a quotidiani locali e siti** | Il Corriere ha impiegato circa 18 mesi per lanciare i Giochi [F6]; altri editori hanno rinunciato per il copyright [F6]. | Un catalogo di giochi originali in italiano, strutturati e incorporabili via widget, potrebbe essere venduto a editori minori. Da approfondire con il Business Strategist. |

---

## 6. Rischi di tendenza

| Rischio | Evidenza | Gravità [S] |
|---|---|---|
| **Declino strutturale del bacino** | Cruciverba da 40,3% a 31,5% in 24 anni [F1]; edicole in calo [F25]. | Media: è lento, ma il pubblico pagante invecchia. |
| **Dominanza dei grandi editori** | Bresi ha 50 milioni di ricavi e un marchio storico [F3]; RCS è già entrato nel digitale [F6]. Se Bresi lanciasse un'app di qualità, occuperebbe lo spazio "varietà". | Alta sul segmento "varietà"; bassa su rebus e crittografie. |
| **Saturazione del cruciverba mobile** | Sviluppatori seriali internazionali localizzano in italiano [F15]. | Alta: evitare di competere sul cruciverba generico. |
| **Dipendenza dagli store** | Le commissioni degli store e l'IVA al 22% riducono il netto. Le commissioni (15% per i piccoli sviluppatori) sono note ma **non riverificate in questa ricerca**. | Media: un web-first riduce il rischio, ma lo scoprimento su mobile passa dagli store. |
| **Pubblicità in calo per i piccoli** | 83% della raccolta online ai grandi operatori internazionali nel 2026 [F24]; eCPM dei casual game in calo (TopOn) **[V2]**. | Alta, se il modello dipende dalle impression. |
| **Bassa propensione a pagare** | 9% paga per news online [F22]; Corriere Giochi a 5 euro il primo anno [F6] fissa un'ancora di prezzo bassa. | Alta: limita il prezzo massimo dell'abbonamento. |
| **Diritto d'autore** | I contenuti delle riviste sono protetti (premessa del progetto). Giochi generati da AI senza contributo umano rilevante potrebbero non essere tutelati (L. 132/2025) [F31]. Il NYT ha fatto rimuovere cloni di Wordle [F6]. | Alta: il sito pubblico deve usare solo giochi originali rivisti o contenuti in licenza, e documentare il lavoro umano di revisione. |
| **Qualità dei giochi generati da AI** | Critiche alla qualità dei cruciverba generati [F30]. | Media: il pubblico esperto dell'enigmistica è severo; un errore in un rebus o in una crittografia rovina la reputazione. |
| **Effetto moda** | Wordle in calo da 5,3 a 4,2 miliardi di partite in un anno [F16]. | Media per i formati virali; bassa per l'enigmistica tradizionale, che è abitudine. |

---

## 7. Verdetto

**L'opportunità esiste, ma è una nicchia.** Il bacino italiano è grande (circa 16 milioni di praticanti annuali e circa 3,5 milioni settimanali, stima su dati ISTAT) e ha già dimostrato di pagare: circa 50 milioni l'anno al solo leader di carta. L'offerta digitale di qualità sull'enigmistica tipicamente italiana è scarsa o mal valutata. Pesano però quattro fattori: propensione a pagare bassa, editori grandi che si stanno muovendo, pubblicità poco remunerativa per i piccoli e vincolo del diritto d'autore. Per uno sviluppatore singolo l'opportunità è realisticamente un **business di nicchia redditizio come progetto laterale**, non una startup da venture capital.

Il posizionamento con più senso, sulla base dei dati:
- una "varietà enigmistica italiana giocabile bene su schermo", con aiuti, verifica e accessibilità;
- un gioco quotidiano gratuito originale (rebus o crittografia del giorno) come motore di acquisizione;
- un abbonamento per l'archivio e le tipologie premium;
- una possibile licenza B2B dei contenuti originali.

### Tre scenari a 24 mesi dal lancio pubblico [S]

Tutti i valori sono stime mie. Le ipotesi sono esplicite, così Pricing Analyst e Product Manager possono sostituirle.

**Ipotesi comuni**
- Il ricavo netto per abbonato è circa il 65-70% del prezzo lordo: IVA al 22% più commissioni di pagamento o dello store.
- Ricavo pubblicitario (RPM, ricavo per mille pagine viste) tra 1 e 3 euro, ipotesi non verificata.
- Il modello è freemium con un gioco quotidiano gratuito.
- Il riferimento di audience è 1,24 milioni di utenti online del genere (Audiweb 2024) [F5]. Il caso "Parole" mostra circa 1,1 milioni di visite al mese ottenibili da un progetto indipendente con un solo gioco [F8].

| | Prudente | Base | Ottimista |
|---|---|---|---|
| Utenti attivi mensili (MAU) a 24 mesi | 5.000 (0,4% dell'audience Audiweb del genere) | 50.000 (4%) | 250.000 (20%), con un gioco quotidiano che diventa virale |
| Conversione da MAU ad abbonato pagante | 1% | 2% | 3% |
| Prezzo abbonamento | 20 euro l'anno | 25 euro l'anno (allineato al Corriere a regime) | 30 euro l'anno, con tipologie premium |
| Abbonati paganti | 50 | 1.000 | 7.500 |
| Ricavi netti da abbonamenti | circa 0,7 mila euro | circa 17 mila euro | circa 150 mila euro |
| Pubblicità (circa 15 pagine viste per MAU al mese) | circa 1-2 mila euro | circa 9-27 mila euro | circa 45-135 mila euro |
| Licenze B2B | 0 | 0-5 mila euro (1 o 2 editori locali) | 30-80 mila euro (5-10 editori o siti) |
| **Totale annuo indicativo** | **circa 2-3 mila euro** | **circa 25-50 mila euro** (valore centrale 25-35 mila) | **circa 225-365 mila euro** |
| Condizioni perché accada | Lancio senza distribuzione, solo SEO, contenuti limitati | SEO sulle tipologie scoperte, gioco quotidiano con buona ritenzione, produzione regolare di giochi originali revisionati | Gioco gancio virale e almeno un accordo B2B o di distribuzione con un editore; nessun lancio digitale forte da parte di Bresi |

Nota sul costo nascosto **[S]**: il collo di bottiglia non è il software ma la **produzione di contenuti originali di qualità** (giochi revisionati ogni giorno). Nello scenario base servono indicativamente 7-15 giochi nuovi a settimana, revisionati da un essere umano. La pipeline AI più revisione del progetto è quindi il fattore critico di fattibilità, più della dimensione del mercato.

---

## 8. Cosa non sono riuscito a verificare

- Tirature e diffusione certificate di La Settimana Enigmistica e delle altre testate di enigmistica (non aderiscono ad ADS o non pubblicano).
- Fatturato complessivo del mercato italiano dell'enigmistica in edicola (ho solo Bresi e Keesing Italia).
- Valore del mercato digitale dei giochi di parole e logica in Italia ed Europa (solo report globali a pagamento con stime divergenti).
- Quota di gioco su carta rispetto al digitale e dispositivi usati per l'enigmistica (i dati ISTAT sui dispositivi riguardano i videogiochi).
- Età media e profilo dei lettori della Settimana Enigmistica.
- Volumi di ricerca delle parole chiave secche "cruciverba", "rebus", "crittografia", "settimana enigmistica" (servono Keyword Planner o Semrush con account).
- Download dell'app Corriere Giochi e numero di abbonati ai giochi del Corriere.
- Download Android dell'app della Settimana Enigmistica.
- Numero di abbonati solo giochi del NYT (cifra precisa non pubblicata in modo univoco).
- Dimensione delle comunità social italiane dell'enigmistica.
- Commissioni attuali di App Store e Google Play per i piccoli sviluppatori (note, non riverificate qui).
- RPM pubblicitari realistici per un sito italiano di giochi (nessuna fonte affidabile trovata).

---

## Fonti

Data di accesso per tutte le fonti: 10 novembre 2026, salvo diversa indicazione. Accanto a ogni fonte è indicata la data di pubblicazione, se nota.

- [F1] ISTAT, "Giochi e videogiochi - Anno 2024", Statistiche Today, 3/9/2026. https://www.istat.it/wp-content/uploads/2026/09/Stat-today_Giochi-e-videogiochi_Anno-2024.pdf
- [F2] ISTAT, podcast "Dati alla mano - Quanto giochiamo?", trascrizione, settembre 2026. https://www.istat.it/wp-content/uploads/2026/09/giochi.pdf
- [F3] ItaliaOggi (C. Plazzotta), "La Settimana Enigmistica e la regola degli utili: sono il 25% dei ricavi", 3/11/2025. https://www.italiaoggi.it/marketing-e-media/media/la-settimana-enigmistica-e-la-regola-degli-utili-sono-il-25-dei-ricavi-xf17ub6z
- [F4] Il Post, "Storia della Settimana Enigmistica", 13/3/2016, e Il Post/Charlie, 3/10/2021 (assenza di certificazione ADS e stime di circa 500 mila copie; consultate tramite sintesi di ricerca). https://www.ilpost.it/2016/03/13/storia-settimana-enigmistica/ ; https://www.ilpost.it/charlie/ledipeo-enciclopedico/
- [F5] AGI (F. Di Chio), "Boom di siti e riviste di rebus e cruciverba", 24/6/2024 (dati Audiweb marzo 2024, Readly 2023). https://www.agi.it/cronaca/news/2024-06-24/boom-siti-riviste-rebus-cruciverba-26895218/
- [F6] Il Post/Charlie, "Il Corriere della Sera si prende Wordle, una specie", 20/7/2025. https://www.ilpost.it/charlie/il-corriere-della-sera-si-prende-wordle-una-specie/
- [F7] Apple App Store, "Corriere Giochi" (RCS MediaGroup), scheda consultata il 10/11/2026; prezzi italiani da sintesi di ricerca della scheda IT. https://apps.apple.com/app/id6744399739 ; https://apps.apple.com/it/app/corriere-giochi/id6744399739
- [F8] Semrush, panoramica di pietroppeter.github.io (gioco "Parole"), dati settembre 2026, aggiornata l'11/10/2026. https://es.semrush.com/website/pietroppeter.github.io/overview
- [F9] Semrush, panoramica di dizy.com, dati settembre 2026. https://www.semrush.com/website/dizy.com/overview/
- [F10] Semrush, panoramica di cruciverba.io, dati settembre 2026. https://www.semrush.com/website/cruciverba.io/overview/
- [F11] AppGoblin, "Cruciverba classici" (FgCos Games). https://appgoblin.info/apps/com.fgcos.cruciverba_autodefiniti
- [F12] AppGoblin, "Cruciverba - Enigmistica" (Digital Taps). https://appgoblin.info/apps/it.turutu.enigmisticacruciverba
- [F13] AppGoblin, "Giochi di Parole - Cruciverba" (Esogames), da sintesi di ricerca. https://appgoblin.info/apps/com.esogames.giochidiparole
- [F14] MWM, scheda "LaSE - Riviste digitali" (Bresi srl, iOS). https://mwm.ai/apps/lase-riviste-digitali/6443897131
- [F15] Google Play Italia, ricerche "rebus", "enigmistica" e "cruciverba", osservazione diretta del 10/11/2026. https://play.google.com/store/search?q=rebus&c=apps&hl=it&gl=IT ; https://play.google.com/store/search?q=enigmistica&c=apps&hl=it&gl=IT ; https://play.google.com/store/search?q=cruciverba&c=apps&hl=it&gl=IT
- [F16] A Media Operator, "The New York Times Wants the Wordle TV Show to 'Re-Energize' Lapsed Players", 3/8/2026. https://www.amediaoperator.com/analysis/wordle-tv-show-new-york-times-games-jonathan-knight/
- [F17] NBC New York/TODAY, "Hardest Wordle and Connections games of 2025" (11,2 miliardi di partite NYT Games 2025), dicembre 2025. https://www.nbcnewyork.com/entertainment/entertainment-news/hardest-wordle-connections-games-2025/6436464/
- [F18] The New York Times Company, 2025 Annual Report, marzo 2026 (dato tramite sintesi di ricerca). https://s23.q4cdn.com/152113917/files/doc_events/2026/Mar/The-New-York-Times-Company-2025-Annual-Report.pdf
- [F19] Nieman Lab, "The New York Times launches a family subscription with separate Wordles for everyone", settembre 2025. https://www.niemanlab.org/2025/09/the-new-york-times-launches-a-family-subscription-with-separate-wordles-for-everyone/
- [F20] NeoAds (Substack), "NYT hit 11.6M subscribers by rethinking news as a product", 2025 (analisi di terzi). https://neoads.substack.com/p/nyt-just-hit-116m-subscribers-by
- [F21] Press Gazette, "New York Times is not the only publisher betting on games to solve subscription growth puzzle", 2/2/2022, aggiornato 7/11/2023. https://pressgazette.co.uk/comment-analysis/new-york-times-is-not-the-only-publisher-betting-on-games-to-solve-subscription-growth-puzzle/
- [F22] Reuters Institute, Digital News Report 2025 - Italy, giugno 2025. https://reutersinstitute.politics.ox.ac.uk/digital-news-report/2025/italy
- [F23] ItaliaOggi (A. Secchi), "Contenuti digitali, la spesa degli italiani a 4 miliardi di euro nel 2025", 17/2/2026 (Osservatorio Digital Content, PoliMi). https://www.italiaoggi.it/marketing-e-media/media/contenuti-digitali-la-spesa-degli-italiani-a-4-miliardi-di-euro-nel-2025-vdlqa07p
- [F24] Osservatorio Internet Media, PoliMi, "Mercato pubblicitario italiano 2025", novembre 2025, e CorCom (tramite sintesi di ricerca). https://brand-news.it/wp-content/uploads/2025/11/Mercato-pubblicitario-italiano-2025-OSS-INTERNET-MEDIA.pdf ; https://www.corrierecomunicazioni.it/digital-economy/ladvertising-online-in-volata-10-anche-grazie-alla-genai
- [F25] ItaliaOggi, "Edicole, cala ancora il numero in Italia: adesso sono 10.720", 2025. https://www.italiaoggi.it/marketing-e-media/media/edicole-cala-ancora-il-numero-in-italia-adesso-sono-10720-qc2bj72n
- [F26] Fortune Italia, "Edicole, è crisi profonda: in quattro anni ne sono sparite 2.700 in Italia". https://www.fortuneita.com/?p=327082
- [F27] Il Fatto Quotidiano, articolo sulla crisi delle edicole, 27/1/2026 (tramite sintesi di ricerca). https://www.ilfattoquotidiano.it/2026/01/27/crisi-edicole-stampa-democrazia-notizie/8269705/
- [F28] Il Post, "I rebus, per solutori abili ma anche inabili", 28/2/2023. https://www.ilpost.it/2023/02/28/andreoli-rebus/
- [F29] Amuse Labs, "Reflections about AI and PuzzleMe" (dato aziendale). https://amuselabs.com/resources/business/reflections-about-ai-and-puzzleme/
- [F30] PuzzCulture, "The Conundrum of Computers, Crosswords, and AI", 9/1/2025. https://puzzculture.com/2025/01/09/the-conundrum-of-computers-crosswords-and-ai
- [F31] Legge 23 settembre 2025 n. 132, art. 25 (modifica dell'art. 1 della L. 633/1941), in vigore dal 10/10/2025; commento Iusletter, 22/10/2025. https://iusletter.com/dalla-redazione/diritto-dellimpresa/diritto-dautore/diritto-dautore-e-opere-create-con-lai/ ; https://www.lavoripubblici.it/news/intelligenza-artificiale-pubblicata-gazzetta-ufficiale-legge-ai-132-2025-36444
- [F32] Sensor Tower, "2025 Q4 unified top 5 word puzzle games revenue US" (post generato automaticamente). https://sensortower.com/blog/2025-q4-unified-top-5-word-puzzle-games-revenue-us-603697f4241bc16eb8570d37
- [F33] WN Hub, "In 2025, mobile game advertising revenue exceeded $12 billion" (sintesi di dati Sensor Tower). https://wnhub.io/news/analytics/item-51349
- [F34] Aziende.it, scheda Keesing Italia S.r.l. (aggregatore di bilanci). https://www.aziende.it/keesing-italia-s-r-l
- [F35] Il Post/Charlie, "47 orizzontale", 16/1/2022. https://www.ilpost.it/charlie/47-orizzontale/
- [F36] Persistence Market Research via GlobeNewswire, 24/1/2024, e 360iResearch via GlobeNewswire, 9/8/2024 (stime globali "games and puzzles" divergenti). https://rss.globenewswire.com/news-release/2024/01/24/2815898/0/en/Games-and-Puzzles-Market-to-Hit-US-50-3-Billion-Mark-by-2030-According-to-Persistence-Market-Research.html ; https://rss.globenewswire.com/fr/news-release/2024/08/09/2927585/28124/en/Games-Puzzles-Market-by-Games-Type-Board-Games-Card-Games-Puzzles-Licensing-Licensed-Non-Licensed-Age-Group-Distribution-Game-Theme-Global-Forecast-2024-2030.html
- [F37] Mediastorm #108 (L. Simi), "Quanto vendono i quotidiani italiani?" (tramite sintesi di ricerca). https://leliosimi.substack.com/p/mediastorm-108-quanto-vendono-i-quotidiani
- [F38] inkl / The Guardian, "Words with friends: how our puzzles app can bring crossword fans together" (tramite sintesi di ricerca). https://www.inkl.com/news/words-with-friends-how-our-puzzles-app-can-bring-crossword-fans-together
