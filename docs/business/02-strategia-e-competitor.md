# DBEnigmistica: strategia, competitor, modello di business e rischio legale

Data: 2026-11-10. Autore: analisi strategica (Business Strategist), in parallelo all'analisi di mercato del Trend Researcher.
Legenda: **[FATTO]** = verificato su fonte citata (numero tra parentesi quadre, elenco in fondo); **[VALUTAZIONE]** = giudizio mio, non verificato. Dove un dato non si trova lo dico.
Non sono un avvocato: la sezione legale è una valutazione operativa, non un parere.

## Sintesi (10 righe)

1. I giochi estratti dalle riviste **non si possono ripubblicare** su un sito commerciale senza licenza: il motore di estrazione è legale solo per uso personale o **per conto di chi detiene i diritti**.
2. Il mercato consumer italiano ha già offerte gratuite o quasi: Corriere Giochi (gratis ogni giorno, abbonamento solo giochi a 5 € il primo anno), il gioco quotidiano gratuito della Settimana Enigmistica, app internazionali con oltre 100 milioni di download.
3. Nessuna offerta trovata rende **giocabili online le tipologie classiche italiane** (crittografie, rebus, cruciverba senza schema, sillabici): gli editori vendono la replica PDF (app LaSE, Readly) o giochi in stile NYT.
4. Il motore da solo non è difendibile: chiunque usi lo stesso modello AI lo replica in settimane. Diventa difendibile solo se unito al **lettore interattivo delle tipologie italiane** e a contratti con chi ha gli archivi.
5. **Raccomandazione:** B2B "archivio giocabile" per editori e titolari di archivi di enigmistica italiana; sito pubblico ridotto a vetrina con contenuti originali.
6. Segmento iniziale: editori medi e piccoli e titolari di archivi (Cairo, Edizioni Play Press, Studio Giochi/Il Sole 24 Ore, Biblioteca Enigmistica Italiana, Keesing Italia come possibile partner).
7. Scartati: sito gratuito con pubblicità o freemium (niente contenuti legali, prezzi di riferimento bassissimi, costo di acquisizione alto) e licenza del motore (tecnologia replicabile, pochi acquirenti).
8. Criterio di arresto: al mese 3 nessun interesse concreto da almeno 3 editori su 10, oppure al mese 12 meno di 2 clienti paganti e meno di 15.000 € contrattualizzati.
9. Rischio principale non legale: pochi acquirenti e cicli di vendita lunghi per uno sviluppatore part-time. Va testato **prima** di costruire il prodotto completo.
10. Prima di tutto servono le risposte del proprietario (sezione 9): quali diritti possiede, quante ore ha, se è disposto a vendere a editori.

---

## 1. Mappa dei competitor

### 1.1 Tabella

Prezzi e numeri come riportati dalle fonti, alla data indicata; dove una fonte è vecchia lo segnalo.

| Competitor | Categoria | Offerta | Modello di ricavo | Prezzi pubblici | Punti di forza | Debolezze | Minaccia per noi |
|---|---|---|---|---|---|---|---|
| **La Settimana Enigmistica** (editore Bresi srl [5]) | Editore italiano | Rivista cartacea; app "LaSE – Riviste digitali" solo per tablet con replica identica della carta [1][2]; sul sito un gioco quotidiano gratuito con registrazione ("Gioca con noi") [1] | Vendita copie e abbonamenti; il digitale è la replica a pagamento | 1,90 € a numero, 8,80 € per 5, 42 € per 26, 78 € per 52 numeri (fonte 2022) [1]; su App Store in USD: 10,99 $ per 5, 99,99 $ per 52 numeri [2] | Marchio dominante; tra 400.000 copie a settimana e circa 1 milione secondo le stime, non certificate ADS [5][6] | Digitale minimo: niente gioco interattivo delle proprie tipologie, niente smartphone | **Alta** come riferimento per il pubblico. Bassa probabilità di diventare cliente [VALUTAZIONE] |
| **Corriere Giochi** (RCS) | Quotidiano italiano | 7 giochi quotidiani: Wordle italiano "In altre parole", quiz, Apegramma, cruciverba e mini, sudoku, crucipuzzle; archivio e statistiche [3][4] | Giochi del giorno gratuiti; archivio e statistiche per abbonati; leva di fidelizzazione degli abbonati al giornale [3] | Solo giochi: 5 € il primo anno (luglio 2025) [3]; in app 2,49 € al mese e 24,99 € all'anno [4] | Marchio forte, app valutata 4,6 su circa 3.500 valutazioni [4], 18 mesi di sviluppo [3] | Modello NYT: niente rebus, crittografie o tipologie classiche italiane | **Alta** sul consumer: fissa il prezzo di riferimento quasi a zero [VALUTAZIONE] |
| **Il Sole 24 Ore / Studio Giochi** | Quotidiano più fornitore di contenuti | Enigmistica24, supplemento settimanale; Parole24, gioco online dal gennaio 2023, curati da Studio Giochi [7] | Vendita del giornale; Studio Giochi produce contenuti per conto terzi | Non trovati | Competenza autoriale; rapporto con un grande quotidiano | Offerta interattiva limitata | Media. Potenziale **cliente o partner** [VALUTAZIONE] |
| **la Repubblica** | Quotidiano italiano | Rubrica di Stefano Bartezzaghi; "giochi quotidiani su repubblica.it" in un PDF probabilmente del 2022 [8] | Non verificato | Non trovati: il sito non è leggibile dallo strumento | Firma autoriale nota | **Stato attuale non verificato** | Media [VALUTAZIONE] |
| **Cairo Editore** | Editore italiano | Settimanali Enigmistica Più (dal 2018, tiratura dichiarata 120.000) ed Enigmistica Mia [9][10] | Edicola | Offerta digitale non trovata | Distribuzione in edicola; gruppo con un quotidiano (Corriere/RCS) | Nessun digitale trovato | Bassa come concorrente. **Potenziale cliente** [VALUTAZIONE] |
| **Keesing Media Group** (in Italia: Sudoku Srl e Corrado Tedeschi Editore) | Editore internazionale e fornitore B2B | 36 testate italiane, tra cui Settimana Sudoku (oltre 4 milioni di copie l'anno) [11]; "White Label Puzzle Platform" e piattaforma digitale B2B per media [12][13] | Edicola più fornitura e licenza digitale a giornali e media | Non pubblicati | Scala, archivi, piattaforma white label già pronta | Catalogo centrato su sudoku e logica; le tipologie classiche italiane non risultano | **Alta** sul B2B white label. Anche possibile **partner** per l'archivio [VALUTAZIONE] |
| **Readly** | Edicola digitale | 48 testate italiane di enigmistica (seconda categoria dopo Disney) [14] | Abbonamento flat | 9,99 € al mese (fonte 2021) [15] | Canale già usato dagli editori | Solo PDF da sfogliare, non giocabile | Bassa, ma dimostra che gli editori **digitalizzano già in PDF** [VALUTAZIONE] |
| **Siti di soluzioni** (dizy.com, cruciverba.io, fsolver.it) | Indipendenti italiani | Risolutori di definizioni, anagrammi, dizionario inverso [16][17] | Pubblicità da traffico SEO [VALUTAZIONE] | Gratuiti | Traffico: dizy circa 5,82 milioni di visite, cruciverba.io circa 137.000 a settembre 2026 (stime Semrush) [17] | Non sono giochi; traffico volatile (cruciverba.io −43% in un mese) [17] | Bassa: altro bisogno. Dimostrano però la domanda di ricerca legata alla carta |
| **App generiche di cruciverba** (Cruciverba Mobile, Cruciverba (Italiano) di MBEX, CodyCross) | Indipendenti e internazionali | Cruciverba "svedesi" generati; CodyCross con oltre 100 milioni di download nel mondo, anche in italiano [18][19][20] | Gratis con pubblicità e acquisti in app | Cruciverba Mobile Pro 0,99 € [18]; Cruciverba (Italiano) gratis [19] | Distribuzione sugli store | Qualità bassa o ferma: 3,7 su 98 valutazioni; ultimo aggiornamento 2021 [18][19] | Media sul consumer casual |
| **NYT Games** | Riferimento internazionale | Crossword, Wordle, Connections, Spelling Bee e altri; archivio dei cruciverba [21] | Abbonamento ai giochi e bundle | Prezzo attuale **non verificato** (fatturazione ogni 28 giorni o annuale) [21] | Nel 1° trimestre 2025 i giochi hanno portato circa 110.000 dei 250.000 nuovi abbonati netti [22]; 12,78 milioni di abbonati a fine 2025 [23] | Solo inglese | Nessuna diretta. **Prova che archivio e giochi trainano gli abbonamenti** |
| **Guardian Puzzles** | Riferimento internazionale | App con 15 cruciverba a settimana e **oltre 15.000 giochi d'archivio** [24] | Abbonamento all'app; gratis sul sito | 3,49 £ al mese, 32,99 £ all'anno (al lancio) [24] | L'archivio come prodotto | Solo inglese | Nessuna diretta. **Modello da replicare per gli editori italiani** |
| **Figaro Jeux** | Riferimento internazionale | App di giochi di parole | Abbonamento | 4,99 € al mese (scheda senza data) [25] | Marchio | Solo francese | Nessuna |
| **Amuse Labs PuzzleMe** | Piattaforma SaaS per editori | CMS di giochi incorporabile nei siti degli editori [26] | SaaS | Free (uso personale), Plus 99 $ al mese; prezzo editori su richiesta [27] | Clienti editoriali in tutto il mondo | Tipologie italiane classiche non risultano dalle pagine consultate [VALUTAZIONE] | Media sul B2B: è il riferimento di prezzo per lo strumento [VALUTAZIONE] |
| **Crossword Compiler** | Software per autori | Creazione e impaginazione di cruciverba | Licenza | 65 $ per utente singolo, una tantum (pagina italiana, forse datata); 300 $ all'anno multiutente [28] | Standard storico degli autori | Pensato per l'inglese, solo griglie | Media per un futuro Studio pubblico [VALUTAZIONE] |

Contesto di mercato, solo come riferimento (il dimensionamento spetta al Trend Researcher). [FATTO] A marzo 2024 circa 1,24 milioni di italiani hanno letto enigmistica online in 30 giorni, su 29 testate online. In Italia ci sono circa 50 riviste di enigmistica [14].

### 1.2 Lettura della mappa [VALUTAZIONE]

- **Consumer:** il terreno è presidiato da marchi che regalano il gioco del giorno (Corriere, Settimana Enigmistica) e da app gratuite globali. Un marchio nuovo senza catalogo di contenuti propri non ha leve di prezzo.
- **Il buco:** l'enigmistica classica italiana **interattiva**. I grandi editori italiani offrono il PDF (LaSE, Readly) oppure giochi in formato NYT (Corriere). Guardian e NYT dimostrano che l'archivio giocabile vale un abbonamento; nessun editore italiano risulta averlo per le proprie tipologie.
- **Chi può chiudere il buco prima di noi:** Keesing, che ha archivi italiani e una piattaforma white label, e Amuse Labs, che però non si occupa di tipologie italiane. Studio Giochi ha la competenza ma non risulta avere una piattaforma.

---

## 2. Rischio legale sui contenuti

**Avvertenza:** non sono un avvocato. Quanto segue serve a decidere cosa costruire; va confermato con un legale esperto di diritto d'autore prima di pubblicare qualsiasi cosa.

### 2.1 Quadro normativo essenziale [FATTO]

- La protezione riguarda l'**espressione, non l'idea**: tipologie, regole e meccanismi di gioco non sono tutelati in sé [29]. Secondo la Corte di giustizia UE (Infopaq, C-5/08) bastano anche **11 parole** a essere tutelate se esprimono la creazione intellettuale dell'autore [30]. Un cruciverba (schema più definizioni), un rebus (vignetta), una crittografia o una sciarada in versi superano con ogni probabilità questa soglia [VALUTAZIONE].
- **Uso personale (art. 68 LDA):** è libera la riproduzione di singole opere "per uso personale dei lettori, fatta a mano o con mezzi di riproduzione non idonei a spaccio o diffusione dell'opera nel pubblico". Per le fotocopie c'è il limite del 15% di ogni fascicolo [31]. La copia privata digitale "estesa" dell'art. 71-sexies riguarda fonogrammi e videogrammi [32].
- **Estrazione di testo e dati (artt. 70-ter, 70-quater, 70-septies LDA):** è consentita a chi ha accesso legittimo, salvo riserva espressa dei titolari, e le copie si conservano **solo per il tempo necessario** all'estrazione [33]. La legge 132/2025 (in vigore dal 10 ottobre 2025) estende queste regole ai sistemi di AI senza creare nuove eccezioni. Introduce una sanzione penale (multa da 51 a 2.065 €) per chi riproduce o estrae in violazione di queste regole [34][35].
- **Opere con AI (art. 1 LDA modificato dalla L. 132/2025):** sono tutelate le opere "dell'ingegno umano", anche se create con l'ausilio dell'AI, purché siano frutto del lavoro intellettuale dell'autore. Un output con scarso contributo umano può non essere tutelato [35].
- **AI Act, art. 53:** gli obblighi sul diritto d'autore (politica di conformità, rispetto dell'opt-out) ricadono sui **fornitori di modelli** generali, non su chi li usa come DBEnigmistica [36].
- **Banche dati:** il costitutore di una banca dati ha un diritto *sui generis* contro l'estrazione o il reimpiego di parti sostanziali (art. 102-bis LDA). Un archivio di annate di una testata ricade plausibilmente in questa tutela [VALUTAZIONE: non ho potuto verificare il testo, la pagina consultata non era disponibile].
- **Giurisprudenza specifica sui cruciverba:** **non trovata**, né italiana né europea. È un'area senza precedenti noti, quindi l'incertezza è alta.

### 2.2 Valutazione per scenario [VALUTAZIONE]

| Scenario | Rischio | Valutazione operativa |
|---|---|---|
| (a) Archiviare per uso personale giochi di riviste acquistate legalmente | **Basso, ma non nullo** | L'art. 68 copre la riproduzione per uso personale con mezzi "non idonei" alla diffusione. Un database digitale privato in cloud, con invio delle pagine a un'API di terzi, è una zona grigia: il testo pensa a fotocopie e copie a mano. In pratica il rischio di contestazione è trascurabile se il contenuto non esce mai dallo Studio: niente utenti, niente condivisione, niente export pubblico. Lo Studio deve restare monoutente. |
| (b) Ripubblicare quei giochi su un sito commerciale | **Alto: da evitare** | È riproduzione e comunicazione al pubblico di opere protette, più estrazione sostanziale da una banca dati altrui. Nessuna eccezione la copre. Vale anche gratis, anche "rielaborato", anche con soluzioni o definizioni riscritte se schema e scelte restano riconoscibili. Si aggiunge il rischio di marchio con nomi come "La Settimana Enigmistica" o le rubriche note. |
| (c) Usarli come base per addestrare o guidare la generazione di giochi nuovi | **Medio: richiede parere legale** | Il TDM dell'art. 70-quater consente l'analisi solo se c'è accesso legittimo, se non c'è riserva espressa e se le copie non vengono conservate oltre il necessario. Un archivio permanente dei giochi completi **non** è TDM. Le riviste cartacee e i PDF possono contenere riserve espresse. Statistiche aggregate (frequenze di parole, lunghezze, distribuzione delle tipologie) sono difendibili. Usare un gioco specifico come esempio nel prompt per generarne uno simile è a rischio di opera derivata. |
| (d) Pubblicare giochi originali generati con assistenza AI | **Basso, se c'è contributo umano** | Lecito pubblicarli. Per **possederli** (e quindi poterli concedere in licenza o difenderli dalla copia) serve un contributo creativo umano documentato: scelta del tema, revisione e riscrittura delle definizioni, selezione. Va verificato che l'output non ricalchi giochi esistenti, controllando duplicati contro l'archivio privato e i siti pubblici. Un archivio senza autore umano è un patrimonio che chiunque può copiare. |

### 2.3 Cosa fare in pratica

**Chiaramente da evitare**
- Pubblicare, anche in parte, contenuti estratti da riviste di terzi; usarne i nomi o le rubriche come marchi.
- Offrire agli utenti di caricare PDF di riviste altrui e giocarli: diventerebbe un servizio di diffusione, con responsabilità della piattaforma.
- Usare singoli giochi protetti come esempi nel prompt per produrre giochi destinati alla pubblicazione.

**Richiede un parere legale** (stima di costo: una consulenza mirata, qualche centinaio di euro [VALUTAZIONE])
- Lo Studio privato con invio delle pagine a un'API AI in cloud (perimetro dell'art. 68).
- Uso di statistiche e TDM sull'archivio privato per guidare la generazione.
- Pubblico dominio delle annate storiche. Per le opere anonime il termine di 70 anni decorre dalla pubblicazione; per le opere firmate decorre dalla morte dell'autore. Una rivista è un'opera collettiva con illustratori e autori diversi, quindi va fatta una verifica testata per testata.
- Il contratto tipo con gli editori: chi è titolare dei dati strutturati e dei giochi corretti dopo la digitalizzazione.

**Strade percorribili**
1. **Lavorare per chi detiene i diritti** (la strada raccomandata). L'editore fornisce il PDF, noi restituiamo i giochi strutturati e giocabili, con licenza scritta. Il motore diventa pienamente legale.
2. **Originali con contributo umano documentato**: giochi creati nello Studio con AI come assistente, registrando chi ha fatto cosa.
3. **Contenuti degli utenti** con licenza esplicita nei termini d'uso, garanzia di originalità da parte dell'autore e procedura di rimozione su segnalazione.
4. **Licenze** da autori indipendenti o associazioni (compenso a gioco o quota dei ricavi).
5. **Pubblico dominio** solo dopo verifica legale, per singola annata.

Requisito tecnico che discende da tutto questo: ogni gioco nel database deve avere un campo **provenienza e licenza** (privato, originale, licenziato, utente, pubblico dominio), e il sito pubblico deve poter pubblicare solo i giochi con provenienza ammessa.

---

## 3. Modelli di business a confronto

CAC = costo di acquisizione di un cliente. Tutte le stime di costo e sforzo sono **[VALUTAZIONE]**; i prezzi finali li fissa il Pricing Analyst.

| Modello | A chi si vende | Perché comprerebbe | CAC atteso | Sforzo per sviluppatore singolo | Rischio | Giudizio |
|---|---|---|---|---|---|---|
| **A. Sito gratuito con pubblicità** | Inserzionisti, tramite il traffico dei giocatori | Il giocatore non paga nulla | Gratis in denaro via SEO, ma servono 12-24 mesi di contenuti; a pagamento non regge | Alto: serve un gioco nuovo ogni giorno, più SEO, più gestione delle pubblicità | Alto: senza contenuti propri il sito è vuoto o illegale; le pubblicità rendono poco senza centinaia di migliaia di pagine viste al mese | **Scartato come motore di ricavo** |
| **B. Freemium con abbonamento** | Appassionati italiani | Archivio, tipologie classiche, nessuna pubblicità | Alto: il riferimento è Corriere a 5 € il primo anno o 24,99 € all'anno [3][4]; con conversioni da gratis a pagante tipiche dell'1-3%, il CAC supera facilmente il valore annuo dell'abbonato | Molto alto: contenuti quotidiani, app mobile, pagamenti, assistenza | Alto | **Scartato ora**; possibile dopo, se il catalogo originale cresce |
| **C. Vendita di pacchetti di giochi** (PDF o raccolte digitali) | Appassionati, scuole, RSA e animazione anziani | Contenuti nuovi e stampabili | Medio | Medio: creazione di contenuti più un negozio | Basso, ma ricavi bassi | **Complementare**, non strategia |
| **D1. Editori: digitalizzazione dell'archivio in "archivio giocabile"** | Editori di enigmistica, quotidiani con rubriche di giochi, biblioteche e archivi (BEI) | Trasforma un archivio PDF (oggi venduto come replica o fermo) in un prodotto interattivo da abbonamento, come Guardian e NYT [22][24] | Alto per contratto (vendita diretta del fondatore, qualche settimana per contratto), ma con pochi contratti utili | Medio: il motore va comunque costruito; serve un lettore incorporabile | Medio: pochi acquirenti, cicli lunghi | **Raccomandato** |
| **D2. Strumento di creazione e impaginazione per autori** | Autori, piccoli editori, scuole | Le tipologie italiane non sono supportate dagli strumenti esistenti (Crossword Compiler, PuzzleMe) | Medio-basso, tramite la comunità enigmistica | Alto: editor per ogni tipologia, impaginazione di stampa | Medio: mercato piccolo | **Fase 2**: nasce dallo Studio usato per correggere i giochi dei clienti D1 |
| **D3. Giochi in white label per siti di giornali** | Quotidiani locali e nazionali | Fidelizzazione degli abbonati, come dichiarato da Corriere [3] | Alto | Alto: servono contenuti quotidiani più la piattaforma | Alto: Keesing e Amuse Labs sono già lì [12][27] | **Solo come estensione di D1** (giocare il proprio archivio sul proprio sito), non come fornitura di contenuti |
| **E. Licenza del motore di estrazione** | Editori, aziende di software | Automatizzare la digitalizzazione | Alto | Basso come tecnologia, alto come supporto | Alto: si replica con lo stesso modello AI, ci sono pochi acquirenti e il valore cala a ogni nuovo modello | **Scartato** |

---

## 4. Vantaggio competitivo: cosa è difendibile [VALUTAZIONE]

| Elemento | Difficile da copiare? | Perché |
|---|---|---|
| Motore di estrazione multi-tipologia | **No, da solo** | Si basa su un modello generale (Claude con visione) accessibile a tutti. Lo stato attuale è uno scheletro non funzionante (vedi `docs/VALUTAZIONE-STATO-ATTUALE.md`). Un concorrente con lo stesso modello lo ricostruisce in settimane, e ogni nuova versione del modello riduce il vantaggio. |
| Catalogo strutturato delle ~30 tipologie italiane (schemi JSON) | **Poco** | Conoscenza pubblica (le regole delle tipologie non sono tutelate); serve tempo ma non è una barriera. |
| Set di riferimento e qualità misurata sulle riviste italiane | **Medio** | Mesi di correzioni su pagine reali sono un vantaggio di esecuzione, non un fossato. |
| **Lettore interattivo delle tipologie italiane classiche** (crittografie, senza schema, sillabici, rebus con verifica) | **Medio** | Nessun concorrente trovato lo offre. È lavoro di prodotto specifico per un mercato piccolo, poco interessante per Keesing o Amuse Labs. |
| **Contratti e archivi degli editori clienti, più i dati strutturati corretti** | **Sì: è l'unico fossato vero** | Chi digitalizza per primo l'archivio di una testata crea dati corretti a mano, un'integrazione e un rapporto. Cambiare fornitore costa all'editore. |
| Strumento di creazione (Studio) | **Medio, nel tempo** | Diventa difendibile solo con una comunità di autori che ci pubblica. |

Conclusione: il valore non sta nel motore ma nella **combinazione**: estrazione, correzione umana nello Studio, lettore interattivo italiano, contratti con chi ha i diritti. Il fossato si costruisce con i clienti, non con il codice.

---

## 5. Raccomandazione strategica

### 5.1 La scelta

**Strategia di ingresso:** servizio B2B "archivio giocabile" per i titolari di diritti dell'enigmistica italiana. Il motore digitalizza i loro PDF, lo Studio serve a correggerli, il lettore incorporabile li rende giocabili sul loro sito o app. Il sito pubblico resta una **vetrina** con un piccolo catalogo di giochi originali, per dimostrare il lettore. Non è un prodotto da monetizzare.

**Segmento iniziale (beachhead):** editori medi e piccoli e archivi con annate in PDF e senza un prodotto interattivo. Nomi da contattare: Cairo Editore (Enigmistica Più e Mia), Edizioni Play Press, Studio Giochi e Il Sole 24 Ore, Biblioteca Enigmistica Italiana di Modena, rubriche di quotidiani (Repubblica, Corriere per il proprio archivio). Keesing Italia va contattata come possibile partner o cliente per le tipologie che la sua piattaforma non copre. La Settimana Enigmistica **non** è il primo bersaglio: politica digitale molto chiusa [1][6].

**Proposta di valore in una frase:** "Trasformiamo l'archivio PDF della vostra testata in cruciverba, crittografie e rebus giocabili online, pronti da pubblicare sul vostro sito o nella vostra app, senza riscrivere nulla a mano."

**Perché questa e non altre:** è l'unica opzione in cui il motore è legale (lavora sui contenuti del cliente). Sfrutta il buco trovato (tipologie italiane interattive) e non chiede di produrre un gioco al giorno. Pochi contratti bastano per un'attività part-time.

**Cosa si sceglie di NON fare (compromessi espliciti):**
- niente pubblicità né abbonamenti consumer nei primi 12 mesi;
- 5 tipologie, non 30: cruciverba classico, cruciverba senza schema, crittografia, rebus (immagine più frase risolutiva), anagramma o sciarada;
- niente app mobile nativa: lettore web incorporabile e responsive;
- niente licenza del motore;
- niente pubblicazione di giochi estratti da riviste di terzi, mai;
- l'area di amministrazione del sito si limita alla pubblicazione; personalizzazione e dashboard avanzate vengono dopo.

### 5.2 Sequenza dei primi 12 mesi

| Mesi | Mossa | Risultato atteso |
|---|---|---|
| 1-2 | Completare la pipeline per 5 tipologie (vedi la sezione 7 della valutazione tecnica); set di riferimento di 20-30 pagine; campo provenienza e licenza nel database | Accuratezza e costo per pagina misurati |
| 1-2 | Parere legale breve sui punti della sezione 2.3 | Perimetro dello Studio e del TDM confermato |
| 2-3 | **10 colloqui di scoperta** con editori e archivi, prima di costruire il lettore: quali archivi hanno, in che formato, cosa vendono in digitale, chi decide | Lista delle esigenze; almeno 3 interessati |
| 3-5 | Lettore web incorporabile per le 5 tipologie; vetrina pubblica con 50-100 giochi originali creati nello Studio (contributo umano documentato) | Demo pubblica legale |
| 4-6 | Demo privata a ogni editore interessato **sul suo stesso PDF**: 1 numero digitalizzato e giocabile | Proposta di pilota |
| 6-9 | 1-2 piloti a pagamento: un'annata o 50 numeri, consegnati come dati strutturati più lettore incorporabile o export verso la loro piattaforma | Primo ricavo; caso d'uso documentato |
| 9-12 | Trasformare il pilota in un contratto ricorrente (nuovi numeri ogni settimana più licenza del lettore); 2° e 3° cliente; dallo Studio nasce lo strumento di correzione e creazione per i clienti | Ricavo ricorrente; base per la fase 2 (D2) |

### 5.3 Criteri per continuare o fermarsi

| Cancello | Quando | Continuare se | Altrimenti |
|---|---|---|---|
| G1 Tecnico | Fine mese 2 | Sul set di riferimento: almeno il 90% di griglie e definizioni dei cruciverba corrette, almeno l'80% sulle altre 4 tipologie. Costo AI per pagina compatibile con un prezzo per numero di qualche euro [VALUTAZIONE: soglia da confermare con il Pricing Analyst] | Ridurre a 2-3 tipologie oppure tenere lo Studio come strumento personale |
| G2 Domanda | Fine mese 3 | Almeno 3 dei 10 interlocutori chiedono una demo sul proprio archivio | **Fermarsi sul B2B.** Valutare D2 (strumento per autori) o il progetto come hobby |
| G3 Pilota | Fine mese 7 | Almeno 1 pilota firmato a pagamento o lettera d'intenti | Ultimo tentativo con 10 nuovi contatti, poi stop |
| G4 Ricavo | Fine mese 12 | Almeno 2 clienti paganti e almeno 15.000 € contrattualizzati, oppure 1 contratto ricorrente annuale | Stop commerciale; lo Studio resta un progetto personale |

### 5.4 Le due alternative scartate

1. **Sito consumer gratuito con pubblicità o freemium.** Non ha contenuti legali in quantità: i giochi estratti non sono pubblicabili e produrre un gioco originale al giorno è un lavoro a tempo pieno. Si scontra con offerte gratuite di marchi forti: Corriere gratis ogni giorno e 5 € il primo anno [3], gioco quotidiano gratuito della Settimana Enigmistica [1], CodyCross [20]. Il CAC per un abbonato supera il valore annuo. Resta un'opzione per dopo, se gli utenti della vetrina crescono senza spesa.
2. **Licenza del motore di estrazione.** La tecnologia è replicabile con lo stesso modello, il valore cala a ogni nuova versione dell'AI e gli acquirenti sono una decina. Venduto senza il lettore e il servizio di correzione è una commodity.

---

## 6. SWOT sintetica

| | |
|---|---|
| **Punti di forza** | Struttura della soluzione e modello di dominio riutilizzabili. Catalogo delle tipologie italiane già descritto (`docs/giochi_enigmistici.md`). La pipeline con visione AI elimina OCR e analisi di layout. Costi fissi bassi. |
| **Punti deboli** | Nulla in produzione: pipeline, salvataggio e lettore da costruire. Sviluppatore singolo part-time senza esperienza di vendita B2B documentata. Nessun contenuto proprio pubblicabile. Nessun marchio. |
| **Opportunità** | Gli editori italiani hanno archivi in PDF (48 testate su Readly [14]) ma nessun prodotto interattivo trovato per le tipologie classiche. NYT e Guardian dimostrano il valore dell'archivio giocabile [22][24]. Corriere dimostra che i quotidiani italiani investono nei giochi [3]. |
| **Minacce** | Keesing con archivi italiani e piattaforma white label [11][12]. Mercato consumer con prezzi di riferimento quasi a zero. Incertezza legale su TDM e opere con AI [34][35]. Dipendenza da un solo fornitore di AI. |

**Mosse derivate:** forza più opportunità = archivio giocabile per editori (la strategia scelta). Debolezza più minaccia = proporsi a Keesing come partner sulle tipologie che non copre, invece di sfidarla.

## 7. I cinque rischi principali e la contromisura

| # | Rischio | Probabilità / impatto [VALUTAZIONE] | Contromisura |
|---|---|---|---|
| 1 | Pubblicazione accidentale di contenuti di terzi, con diffida, causa o sanzione penale per il TDM (L. 132/2025) [34] | Media / alto | Campo provenienza e licenza obbligatorio; il sito pubblica solo provenienze ammesse; Studio monoutente; parere legale prima del lancio; procedura di rimozione. |
| 2 | Pochi acquirenti e cicli di vendita lunghi | Alta / alto | Colloqui **prima** di costruire il lettore (cancello G2); demo sul PDF del cliente; pilota a prezzo basso con conversione in contratto ricorrente. |
| 3 | Qualità di estrazione insufficiente su rebus e crittografie | Media / medio | 5 tipologie; set di riferimento; correzione umana nello Studio inclusa nel servizio e nel prezzo. |
| 4 | Capacità del singolo sviluppatore e dispersione tra sito, Studio, amministrazione e motore | Alta / alto | Un solo obiettivo per trimestre (tabella 5.2); vetrina minima; niente app native né dashboard prima di G3. |
| 5 | Mossa di un incumbent (Keesing estende il white label alle tipologie italiane, Corriere o Settimana Enigmistica fanno l'archivio interattivo in proprio) | Media / alto | Velocità sui primi 2-3 clienti; partnership con Keesing invece di concorrenza; contratti pluriennali con dati di proprietà del cliente ma lettore e strumenti nostri. |

Rischio aggiuntivo da monitorare: costo e disponibilità dell'API AI. Contromisure: limiti di spesa per esecuzione, Batch API, interfaccia astratta per poter cambiare modello (già previsto nella valutazione tecnica, sezione 6.2).

## 8. Note per il Pricing Analyst e il Product Manager

- Prezzi B2B di riferimento pubblici **non trovati** per Keesing e per il piano editori di Amuse Labs [27]. Gli unici ancoraggi sono Amuse Labs Plus a 99 $ al mese [27] e Crossword Compiler a 65 $ una tantum o 300 $ all'anno [28].
- Ancoraggi consumer: Corriere Giochi 2,49 € al mese e 24,99 € all'anno [4]; Guardian 3,49 £ e 32,99 £ [24]; Figaro Jeux 4,99 € al mese [25]; replica della Settimana Enigmistica 78 € per 52 numeri (fonte 2022) [1].
- Il prezzo NYT Games attuale non è verificato [21].

## 9. Domande a cui deve rispondere il proprietario

1. Quali riviste possiede, in che formato e con quali diritti? Ha contatti personali con editori, autori o associazioni enigmistiche (ARI, BEI)?
2. L'obiettivo è un reddito (quanto, entro quando) o un progetto personale che, se possibile, si ripaga?
3. Quante ore a settimana sono realistiche per 12 mesi, e quante di queste è disposto a dedicare a vendita e colloqui invece che a sviluppo?
4. Qual è il budget annuo per API AI, hosting e un parere legale (ordine di grandezza: qualche centinaio di euro per il parere)?
5. È disposto a lavorare per conto di editori (B2B) invece di costruire un proprio marchio per giocatori?
6. Crea giochi lui stesso, o conosce autori che lo farebbero? Serve per la vetrina e per l'opzione D2.
7. Quali 5 tipologie considera irrinunciabili? La proposta qui è: cruciverba classico, senza schema, crittografia, rebus, anagramma o sciarada.
8. Accetta i criteri di arresto della sezione 5.3?

---

## Fonti

Tutte consultate il 2026-11-10 salvo diversa indicazione. Tra parentesi la data del contenuto quando nota.

1. Trend Online, "La Settimana Enigmistica Online: dove e come giocare" (27/06/2022): https://www.trend-online.com/giochi-console/settimana-enigmistica-online/
2. App Store, "LaSE – Riviste digitali" (scheda e acquisti in app; dati tramite risultati di ricerca, la pagina ha restituito errore 429): https://apps.apple.com/app/id6443897131
3. Il Post, Charlie, "Il Corriere della Sera si prende Wordle, una specie" (20/07/2025): https://www.ilpost.it/charlie/il-corriere-della-sera-si-prende-wordle-una-specie/
4. App Store, "Corriere Giochi", RCS MediaGroup (versione 1.0.0 del 10/07/2025): https://apps.apple.com/it/app/corriere-giochi/id6744399739
5. ItaliaOggi, "La Settimana Enigmistica e la regola degli utili sono il 25% dei ricavi": https://www.italiaoggi.it/marketing-e-media/media/la-settimana-enigmistica-e-la-regola-degli-utili-sono-il-25-dei-ricavi-xf17ub6z ; ItaliaOggi, "La Settimana Enigmistica, ecco perché è nel cuore dei lettori": https://www.italiaoggi.it/marketing-e-media/media/la-settimana-enigmistica-ecco-perche-e-nel-cuore-dei-lettori-ycyrcey3
6. Il Foglio, "Novant'anni di Settimana enigmistica" (22/01/2022): https://www.ilfoglio.it/cultura/2022/01/22/news/novantanni-di-settimana-enigmistica--148581
7. Studio Giochi, "Parole24 è online" (gennaio 2023): https://studiogiochi.com/en/2023/01/parole24-e-online/ ; Enigmistica24 (03/11/2023): https://www.studiogiochi.com/files/studiogiochi.com/2023/11/Enigmistica24_3_11_2023.pdf
8. Rubbettino, PDF con la rubrica "Lessico & Nuvole" di la Repubblica (2022 circa): https://www.store.rubbettinoeditore.it/wp-content/uploads/2022/09/moro-ilvenerdi.pdf
9. Cairo Communication, Cairo Book luglio 2018: https://cairocommunication.it/sites/default/files/cairo_book/CAIRO_BOOK_luglio18.pdf
10. Cairo Communication, Cairo Book 2019: https://cairocommunication.it/sites/default/files/cairo_book/CAIROBOOK_ANNO_2019.pdf
11. Keesing, "Takeovers in France and Italy, a new president/MD in North America" (09/11/2022): https://www.keesing.com/?p=47
12. Keesing, "Keesing and BNR launch the Daily News Puzzle": https://keesing.com/?p=4150
13. Keesing, "Keesing strengthens Nordic leadership with major Finnish acquisitions": https://www.keesing.com/major-finnish-acquisitions/
14. AGI, "Boom di siti e riviste di rebus e cruciverba" (24/06/2024): https://www.agi.it/cronaca/news/2024-06-24/boom-siti-riviste-rebus-cruciverba-26895218/
15. Sky TG24, "Readly, oltre cinquemila riviste in un unico abbonamento" (10/03/2021): https://tg24.sky.it/tecnologia/now/2021/03/10/recensione-readly-riviste-abbonamento
16. Dizy (home page, © 2013-2026 Prometheo): https://www.dizy.com/
17. Semrush, panoramica di cruciverba.io (dati di settembre 2026, aggiornati l'11/10/2026): https://it.semrush.com/website/cruciverba.io/overview
18. App Store, "Cruciverba Mobile" (versione 2.9.8 del 14/03/2024): https://apps.apple.com/it/app/cruciverba-mobile/id550982960
19. App Store, "Cruciverba (Italiano)", MBEX LTD EOOD (versione 1.3.1 del 31/03/2021): https://apps.apple.com/app/id1323173041
20. AppGoblin, CodyCross (Fanatee): https://appgoblin.info/apps/com.fanatee.cody ; Google Play, scheda evento: https://play.google.com/store/apps/eventdetails/4828146346764547170
21. NYT Help Center, "New York Times Games Subscription": https://thenewyorktimeshelpcenter.helpjuice.com/360052272251-New-York-Times-Games-Subscription
22. Pelcro, "The New York Times delivers strong Q1 2025 results as bundle strategy gains momentum": https://www.get.pelcro.com/blog/the-new-york-times-delivers-strong-q1-2025-results-as-bundle-strategy-gains-momentum
23. TheWrap, "The New York Times Q4 2025 earnings": https://www.thewrap.com/media-platforms/journalism/the-new-york-times-q4-2025-earnings
24. Digiday, "To drive daily habits and customer retention, publishers turn to puzzles": https://digiday.com/media/drive-daily-habits-customer-retention-publishers-turn-puzzles ; TapSmart, "Great crossword apps": https://www.tapsmart.com/games/great-crossword-apps/
25. App Store, "Figaro Jeux" (scheda senza data): https://apps.apple.com/FR/app/id1465863879
26. Amuse Labs, "A Modern CMS for Games" (soluzioni per editori): https://amuselabs.com/solutions/publishers/
27. Amuse Labs, "Plans and Pricing": https://amuselabs.com/pricing ; FAQ: https://amuselabs.com/faqs/
28. Crossword Compiler, ordini: https://crossword-compiler.com/ordering.html ; pagina italiana: https://crossword-compiler.com/it/ordering_once.html
29. Università di Roma La Sapienza (DICEA), "Diritto d'autore e Open Access": https://www.dicea.uniroma1.it/sites/default/files/Diritto%20d%27Autore%20e%20Open%20Access.pdf
30. Pinsent Masons Out-Law, "Eleven-word snippets can infringe copyright, rules ECJ" (2009): https://www.pinsentmasons.com/en-gb/out-law/news/eleven-word-snippets-can-infringe-copyright-rules-ecj ; testo della sentenza C-5/08: https://www.ippt.eu/sites/ippt/files/2009/IPPT20090716_ECJ_Infopaq_v_DDF.pdf
31. Brocardi, art. 68 L. 633/1941 (aggiornato al 09/08/2026): https://www.brocardi.it/legge-diritto-autore/titolo-i/capo-v/sezione-i/art68.html
32. Brocardi, art. 71-sexies L. 633/1941: https://brocardi.it/legge-diritto-autore/titolo-i/capo-v/sezione-ii/art71sexies.html
33. Brocardi, art. 70-quater L. 633/1941: https://www.brocardi.it/legge-diritto-autore/titolo-i/capo-v/sezione-i/art70quater.html
34. Hogan Lovells, "Copyright provisions in the new Italian AI law" (08/10/2025): https://www.hlc.com/en/publications/copyright-provisions-in-the-new-italian-ailaw-reinforcing-human-authorship-and-text-and-data-mining
35. Legance, newsletter sulla legge italiana sull'intelligenza artificiale (settembre 2025): https://www.legance.it/wp-content/uploads/2025/09/Newsletter-ITA_Legge_sullIntelligenza_Artificiale.pdf ; Communia, "Italy updates its copyright law to address AI" (01/10/2025): https://communia-association.org/2025/10/01/italy-updates-its-copyright-law-to-address-ai/
36. IAPP, "The EU AI Act and copyrights compliance": https://iapp.org/news/a/the-eu-ai-act-and-copyrights-compliance ; Commissione UE, consultazione su AI e copyright: https://digital-strategy.ec.europa.eu/en/faqs/stakeholder-consultation-ai-and-copyright-compliance

**Dati cercati e non trovati:** prezzo attuale di NYT Games; offerta giochi attuale di la Repubblica; offerta digitale di Cairo Editore; listini B2B di Keesing e del piano editori di Amuse Labs; giurisprudenza italiana o europea sui cruciverba; testo vigente dell'art. 102-bis LDA (la pagina consultata non era disponibile); prezzi attuali della Settimana Enigmistica digitale in euro (le fonti risalgono al 2022 o sono in dollari).
