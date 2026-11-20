# DBEnigmistica - Piano commerciale definitivo

Documento 05 della serie business. Data: 10 novembre 2026. Autore: Product Manager.
Input letti per intero: documenti 01 (ricerca di mercato), 02 (strategia e competitor), 03 (posizionamento e prezzi), 04 (duello strategico), `docs/VALUTAZIONE-STATO-ATTUALE.md`, `docs/PROMPT-INIZIALE.md`, `docs/giochi_enigmistici.md`, più le decisioni del proprietario.
Destinatari: il proprietario (decisione) e il team tecnico (ridisegno delle fasi).

**Convenzioni**
- **[D1]...[D4]**: dato preso dai documenti 01-04, che riportano la fonte originale. **[VT]**: dalla valutazione tecnica.
- **[F1]...**: fonte nuova consultata per questo documento (elenco in fondo).
- **[S]**: stima mia, con ipotesi dichiarata. **[N]**: cercato e non trovato.
- **[P]**: decisione già presa dal proprietario, vincolante.
- Calendario: il **mese 1 (M1) è dicembre 2026**; il mese 24 è novembre 2028. Prezzi B2B IVA esclusa, prezzi ai consumatori IVA inclusa.

---

## Sintesi (10 righe)

1. **Strategia unica, "Bottega d'autore"**: DBEnigmistica è un autore-produttore di giochi enigmistici originali in italiano, creati con assistenza AI e firmati da una persona. Il sito pubblico è vetrina e laboratorio, non il motore di ricavo.
2. **Ricavi in ordine di priorità**: fornitura di giochi a chi li pubblica (a pezzo e "gioco del giorno" in licenza con lettore incorporabile), poi raccolte e libri, poi abbonamento solo oltre una soglia. Esclusi: archivio per editori, pubblicità, licenza del motore.
3. **Il pezzo critico è il Motore di creazione**: entro febbraio 2027 deve portare la revisione di un gioco breve sotto i 20 minuti. Se non ci riesce, la strategia non regge.
4. **MVP** (dicembre 2026-febbraio 2027, sito online a marzo 2027): Motore di creazione e Studio per 3 tipologie, lettore accessibile, sito con gioco del giorno e newsletter, Admin minimo, registro di provenienza e del contributo umano.
5. **Ore** (base 7 a settimana): nel primo trimestre 4 di sviluppo, 2,5 di contenuti e 0,5 di mercato; dal secondo anno 1,5 / 4 / 1,5.
6. **Soglie**: M3 tempo di revisione; M6 300 iscritti e 25% di ritorno settimanale; M9 prima vendita a un compratore; ricavi netti mensili di almeno 200 € a M12, 450 € a M18 e 800 € a M24. Sotto 400 € a M24 ci si ferma.
7. **Budget dei primi 12 mesi**: circa 2.300 € (intervallo 1.100-4.200). La voce che non si taglia è il parere legale.
8. **Prezzi di fornitura stimati** (nessun dato italiano trovato): 20 € un gioco breve, 60 € un cruciverba medio, 120 € uno grande, 49-99 € al mese il gioco del giorno in licenza. I riferimenti USA vanno da 50 a 750 dollari a gioco [F1-F4].
9. **Requisito non negoziabile**: ogni gioco porta origine, diritti, stato di inedito e un registro del contributo umano esportabile. Le pagine delle riviste non escono mai dallo Studio.
10. **Scenario base a M24**: circa 830 € al mese di ricavi netti, quasi tutto margine. È molto meno dei 25-35 mila € l'anno del documento 01, perché qui non ci sono contatti, pubblicità né archivi di terzi.

---

## 1. Da dove partiamo (per chi non ha letto gli altri documenti)

- **Mercato** [D1]: circa 16 milioni di italiani fanno cruciverba almeno una volta l'anno e circa 3,5 milioni ogni settimana. Il bacino è anziano e in lento calo. Il leader su carta (La Settimana Enigmistica) fattura circa 50 milioni di euro, ma il suo digitale è una replica PDF valutata 1,9 su 5. Non c'è un'offerta digitale giocabile di qualità per le tipologie italiane classiche: crittografie, cruciverba senza schema, "varietà".
- **Concorrenza** [D2]: il Corriere regala 7 giochi quotidiani e vende l'abbonamento solo giochi a 5 € il primo anno, poi a 24,99 € l'anno. Keesing e Amuse Labs vendono piattaforme di giochi agli editori, ma non coprono le tipologie italiane.
- **Vincolo legale** [D2]: i giochi delle riviste non si ripubblicano. Per la legge 132/2025 un'opera fatta con l'AI è tutelata solo se è frutto del lavoro intellettuale di una persona.
- **Stato tecnico** [VT]: nulla funziona in produzione. Il progetto .NET compila, ma la pipeline PDF restituisce pagine bianche e non esiste un lettore web. Si possono riusare il modello di dominio, il database e il catalogo delle tipologie.
- **Proprietario** [P]: lavora da solo, 5-10 ore a settimana, senza contatti con editori né autori partner. Vuole un reddito integrativo entro 24 mesi, con soglie di arresto. Mette al centro la creazione di giochi originali e di nuovi format con assistenza AI.

---

## 2. Visione e proposta di valore

**Visione (tre righe)**
Diventare la bottega italiana di riferimento per i giochi enigmistici originali, nuovi e classici, fatti con l'AI come strumento e firmati da un autore.
Giochi che si giocano bene su qualunque schermo, anche con la vista e la pazienza di un solutore di settant'anni.
Venduti a chi vuole offrirli ai propri lettori e giocati da chi li cerca ogni giorno.

**Proposta di valore (una frase)**
Giochi enigmistici italiani originali, firmati e verificati, giocabili subito su qualunque schermo e pronti da pubblicare per chi vuole offrirli ai propri lettori.

---

## 3. La strategia scelta e cosa cambia rispetto ai documenti

### 3.1 La scelta: "Bottega d'autore"

DBEnigmistica **produce giochi originali e li vende come autore e fornitore**. Il prodotto commerciale è il gioco, insieme al lettore che lo rende giocabile. Il software serve a produrre giochi più in fretta e meglio: non si vende da solo.

La logica in quattro passaggi:
1. Il **Motore di creazione** abbassa il costo umano di ogni gioco. È l'ipotesi da validare per prima (soglia di M3).
2. Il **Sito pubblico** mette i giochi davanti ai giocatori e misura quali formati trattengono. Costruisce anche il portafoglio da mostrare ai compratori.
3. I giochi migliori si vendono **più volte**: a pezzo, in licenza quotidiana a siti e newsletter, in raccolte e libri. L'abbonamento arriva solo se il pubblico lo giustifica.
4. I **format originali** sono l'opzione ad alto rendimento: se uno funziona sul sito, si propone in licenza.

Nel documento 04 questa è la "Strategia A modificata" (sezioni 3 e 8.3). A perdeva perché la produzione settimanale divorava le ore. Si salvava con quattro correzioni: gioco quotidiano a basso costo di revisione, autore partner, vendita in licenza, niente abbonamento nel primo anno. Qui il **Motore di creazione prende il posto dell'autore partner**, che il proprietario non ha. Le altre tre correzioni sono adottate. Per questo la prova che conta più di tutte è il tempo di revisione, e la si fa nei primi tre mesi.

### 3.2 Perché non le altre strade

- **Archivio giocabile per editori** (B, consigliata dai documenti 02 e 03): richiede contatti diretti con almeno due editori, che il proprietario non ha. Senza contatti il documento 04 stima il 20-30% di probabilità di un pilota pagato entro il mese 9. In più, correggere archivi altrui consuma le stesse ore che la strada B prometteva di risparmiare. Resta un **listino dormiente**: si riattiva solo se un editore lo chiede di sua iniziativa.
- **Ibrida con decisione al mese 4** (C, vincitrice del documento 04): il suo ramo editori era la digitalizzazione di archivi, ora esclusa. Tengo il suo metodo: lettore prima, motore di estrazione per ultimo, marchio neutro, soglie fisse. Cambia il ramo commerciale: non più digitalizzazione per editori, ma vendita di giochi originali a chi li pubblica.
- **Sito per giocatori con abbonamento come motore di ricavo** (A pura, tesi del documento 01): il pareggio richiede circa 580 abbonati, cioè circa 29.000 utenti attivi al mese [D3]. Non è raggiungibile in 24 mesi con 7 ore a settimana e senza budget di acquisizione.

### 3.3 Cosa tengo e cosa cambio di ciascun documento

| Documento | Cosa sosteneva | Cosa tengo | Cosa cambio e perché |
|---|---|---|---|
| **01 Ricerca di mercato** | Nicchia di giocatori; posizionamento "varietà italiana giocabile"; abbonamento più licenze B2B; scenario base 25-35 mila € l'anno a 24 mesi con 50.000 utenti attivi al mese | Il vuoto sulle tipologie italiane, l'accessibilità per gli over 65, il gioco del giorno come aggancio, gli aiuti e la verifica, il rischio diritti | L'abbonamento scende al 4° posto tra le linee di ricavo. Lo scenario base del documento 01 non è un obiettivo: il mio scenario base a M24 vale circa 10 mila € l'anno. La "licenza B2B dei contenuti originali", che lì era una nota, diventa la linea principale |
| **02 Strategia e competitor** | B2B "archivio giocabile" per editori; vetrina non monetizzata; criterio di arresto: 3 editori interessati su 10 al mese 3 e 15.000 € contrattualizzati al mese 12 | Analisi legale, campo provenienza e licenza, Studio monoutente, "mai ripubblicare giochi estratti", niente licenza del motore, Keesing e Amuse Labs come riferimenti | Il servizio per editori diventa dormiente. Il motore di estrazione torna a essere uno strumento privato, all'ultimo posto. La vetrina diventa laboratorio e catalogo. Le soglie sono più basse (sezione 8), perché si parte senza contatti |
| **03 Prezzi** | Linea giocatori a 3,49 € al mese e 29,99 € l'anno, fondatori a 19,99 €; linea editori con avvio, canone e prezzo per pagina; 460 ore l'anno; 11 giochi a settimana | Prezzi consumer e costi unitari (AI, Stripe, hosting); prezzo fondatori; principio "annuale come opzione predefinita"; costo per gioco (12 minuti per un gioco breve, 45 per un cruciverba) come obiettivo | Aggiungo il **listino fornitore**, che mancava (sezione 5.2). Sospendo il listino editori. Le ore scendono a circa 320 l'anno. La soglia di attivazione dell'abbonamento passa a 1.000 iscritti alla newsletter e 300 giochi in archivio. Il questionario sul prezzo scende da 150 a 60 risposte, perché il pubblico iniziale è piccolo |
| **04 Duello** | Vince C con soglie al mese 4 (500 iscritti, 30% di ritorno, 1 pilota); 10 contatti con editori nei mesi 1-4 | Lettore per primo, estrazione per ultima, formato quotidiano a basso costo, marchio neutro, "ogni prova costa meno di un mese e può chiudere una strada" | Soglie spostate: il sito va online a marzo 2027, quindi la soglia sul pubblico è a fine maggio (M6), più bassa (300 iscritti, 25%) ma con tre mesi di sito online invece di uno. I contatti partono solo dopo che il motore ha superato la soglia di M3, come chiede il proprietario |

### 3.4 Numeri contraddittori tra i documenti e scelta

| Tema | Valori in conflitto | Scelta |
|---|---|---|
| Ore disponibili | 460 l'anno, cioè 10 a settimana [D3]; 250 in sei mesi [D4] | **7 ore a settimana**, circa 320 l'anno su 46 settimane. Sezione 11 con i casi a 5 e 10 ore |
| Tempo di revisione per gioco | 12 minuti breve, 45 cruciverba [D3]; 20-45 minuti [D4] | Obiettivo 15-20 minuti per un gioco breve e 45 per un cruciverba medio; la prova 1 sostituisce le stime con misure |
| Giochi nuovi a settimana | 7-15 [D1]; 11 [D3]; 5-10 [D4] | **7 brevi** (uno al giorno) più **1 cruciverba medio ogni 2 settimane** sul sito, più la produzione inedita per la vendita |
| Prezzo per la prevendita ai giocatori | "sotto i 25 €" [D4]; fondatori 19,99 € [D3] | 19,99 € il primo anno, come nel documento 03 |
| Valutazione dell'app Corriere Giochi | 4,7 su 29 voti, negozio USA [D1]; 4,6 su circa 3.500 voti, negozio italiano [D2][D3] | 4,6, dato del negozio italiano |
| Prezzo della Settimana Enigmistica | 2 € (2024) [D1]; 1,90 € (fonte 2022) [D2] | 2 €, il dato più recente |
| Copie della Settimana Enigmistica | oltre 400.000 [D1]; da 400.000 a 1 milione [D2] | "Oltre 400.000, non certificate". Il numero non cambia nessuna decisione |
| Tipologie di partenza | 5 [D2]; 2 [D4] | 3 (sezione 7) |

---

## 4. Segmenti e priorità

| Priorità | Segmento | Chi sono | Cosa cercano | Cosa comprano da noi | Da quando |
|---|---|---|---|---|---|
| **1** | **Compratori di contenuti** | Testate online locali, newsletter, piccoli settimanali e riviste di nicchia, riviste di enigmistica che accettano collaboratori esterni; per i format logici indipendenti dalla lingua, anche siti e pubblicazioni estere | Giochi inediti, corretti, a prezzo prevedibile e senza rischio legale; un modo semplice per farli giocare ai propri lettori | Giochi a pezzo o a pacchetto; gioco del giorno in licenza con lettore incorporabile | Proposte da M6; prima vendita attesa entro M9 |
| **2** | **Solutori su schermo** | Adulti e anziani italiani che giocano su smartphone e tablet; donne in maggioranza nel digitale [D1] | Gioco del giorno gratuito, leggibile, con aiuti e verifica, senza pubblicità invadente | Per ora nulla: portano attenzione e dati. Poi raccolte e, oltre soglia, l'abbonamento | Da M4 (marzo 2027) |
| **2** | **Acquirenti di giochi stampati** | Solutori che preferiscono la carta, figli e nipoti che fanno un regalo, animatori di RSA e centri anziani | Raccolte a caratteri grandi, stampabili o in libro | Raccolte PDF, libri stampati su Amazon | Prima raccolta PDF ad aprile-maggio 2027; primo libro per il Natale 2027 |
| 3 | Appassionati di enigmistica classica | Associazione Rebussistica Italiana (meno di 200 soci), lettori delle riviste specialistiche, Biblioteca Enigmistica Italiana [D1] | Qualità, rispetto delle regole | Nulla: danno credibilità e fanno da revisori severi | Da M4, come gruppo di prova |
| Dormiente | Editori con archivi da digitalizzare | Elenco nel documento 02 | Archivio giocabile | Listino del documento 03 sezione 3.2, invariato | Solo su richiesta spontanea |

---

## 5. Linee di ricavo

### 5.1 Ordine di priorità

| # | Linea | Perché in questa posizione | Attivazione | Prezzo di lancio |
|---|---|---|---|---|
| **L1** | **Fornitura di giochi originali**: a pezzo, a pacchetto o su commissione; cessione esclusiva o licenza non esclusiva | È la linea che il proprietario ha aperto. Ogni gioco si paga una volta per tutte, non serve un pubblico proprio e non serve un abbonato alla volta | Campionario a M6, prima vendita entro M9 | Sezione 5.2 |
| **L2** | **Gioco del giorno in licenza** con lettore incorporabile nel sito o nella newsletter del cliente | Ricavo ricorrente, la stessa produzione serve più clienti (licenza non esclusiva) e il lettore diventa un vantaggio | M10 (settembre 2027) | Micro 49 € al mese (fino a 5.000 partite al mese); Piccolo 99 € al mese (fino a 20.000); annuale anticipato -15%; 30 giorni di prova gratuita |
| **L3** | **Raccolte e libri**: PDF stampabili e libri stampati su Amazon KDP | Non richiede contatti e sfrutta giochi già pubblicati. È anche la prima prova che qualcuno paga per questi giochi | Primo PDF ad aprile-maggio 2027; primo libro a ottobre 2027 | PDF da 50 giochi a 4,99 € [D3]; libro da 100 giochi a caratteri grandi a 9,99 € |
| L4 | **Abbonamento "Solutore"** sul sito | Rende solo con molti iscritti; il pareggio a costo pieno vuole circa 580 abbonati [D3] | Non prima di M13 e solo con almeno 1.000 iscritti alla newsletter e 300 giochi in archivio | Prezzi del documento 03: 3,49 € al mese, 29,99 € l'anno, fondatori 19,99 € il primo anno |
| L5 | **Licenza per strutture** (RSA, centri anziani, scuole): stampa illimitata per uso interno | Pubblico coerente con l'accessibilità, ma va raggiunto struttura per struttura | Dopo M18, se L3 dimostra domanda dagli animatori | 79 € l'anno [D3] |
| - | Pubblicità | Esclusa: con il traffico atteso vale 75-225 € al mese e peggiora l'esperienza degli anziani [D3] | Da rivalutare oltre 300.000 pagine viste al mese | - |
| - | Licenza del motore; archivio per editori | Esclusa / dormiente (sezione 3.2) | - | - |

Nota su L3 [F5]: dal 10 giugno 2025 KDP paga il 50% invece del 60% sui libri stampati con prezzo sotto una soglia, che negli USA è 9,99 dollari. La soglia per amazon.it **non è stata verificata [N]**: il prezzo del libro va fissato appena sopra la soglia italiana. KDP chiede di dichiarare i contenuti generati con l'AI. Secondo fonti secondarie la dichiarazione non riguarda i contenuti solo assistiti, ma la regola va letta sulle linee guida ufficiali prima di pubblicare.

### 5.2 Linea L1: quanto vengono pagati gli autori di giochi (tensione 4)

**Dati trovati (tutti esteri, tutti di cruciverba o sudoku)**

| Committente | Compenso | Condizioni | Data | Tipo |
|---|---|---|---|---|
| New York Times, cruciverba dal lunedì al sabato (15x15) | 500 dollari per i primi due pubblicati, poi 750 dollari | Il NYT scarta il 97% delle proposte | Tariffe dal 2019, confermate da un articolo del 25/8/2026 | [F1] |
| New York Times, cruciverba della domenica (21x21) | 1.500 dollari, poi 2.250 dollari | Gli autori cedono i diritti, anche sulle raccolte in libro | Come sopra | [F1] [F6] |
| Spyscape (museo di New York, cruciverba settimanale online, 15x15) | 200 dollari a gioco, alla pubblicazione | Cessione di tutti i diritti | Scheda per autori del 2024 | [F2] |
| Artisanal Sudoku (pubblicazione indipendente) | 50 dollari a gioco | Gioco inedito; l'autore può ripubblicarlo con il proprio nome | Da novembre 2024 | [F3] |
| La maggior parte delle testate USA | "meno di 100 dollari" a cruciverba | - | 2012, dato vecchio | [F4] |
| Best Crosswords (sito) | Compenso dopo un periodo di prova, importo non pubblicato | L'autore resta proprietario; licenza non esclusiva | Senza data | [F7] |
| Compensi degli autori italiani di enigmistica | **[N]** | Nessuna fonte pubblica trovata (né editori, né associazioni, né autori) | - | [N] |
| Listini di syndication per piccoli giornali | **[N]**, solo su preventivo | Esempio: INN News Crossword da 49 a 999 dollari al mese [D3] | - | [N] |
| Acquisto di un format di successo | Il NYT ha comprato Wordle per una cifra "nell'ordine del milione di dollari" | Caso unico, un format virale | 31/1/2022 | [F8] |

**Stima per l'Italia [S]**
Ipotesi:
- il mercato italiano paga tra il 30 e il 50% dei committenti USA di fascia media (50-200 dollari): è più piccolo e ha una disponibilità a pagare più bassa [D1];
- la licenza non esclusiva vale circa la metà della cessione esclusiva;
- il prezzo deve coprire almeno il doppio del costo del tempo, valorizzato a 35 € l'ora come nel documento 03.

| Prodotto (licenza non esclusiva, inedito) | Intervallo stimato | **Prezzo di lancio** | Costo del tempo [D3] | Cessione esclusiva |
|---|---|---|---|---|
| Gioco breve: crittografia, anagramma, ditloide, indovinello | 15-30 € | **20 €** | 7-12 € (12-20 minuti) | il doppio |
| Cruciverba medio (11x11-13x13) o gioco di logica | 40-80 € | **60 €** | circa 26 € (45 minuti) | il doppio |
| Cruciverba grande, senza schema, "varietà" premium | 80-150 € | **120 €** | 35-50 € | il doppio |
| Pacchetto di 30 giochi brevi a tema | 300-600 € | **450 €** | 210-360 € | il doppio |
| Format originale: regolamento, 30 esemplari, licenza d'uso per 12 mesi | 1.000-3.000 € | **1.500 €**, più la fornitura successiva a listino | 15-25 ore | da negoziare |

Confidenza **bassa**. Il prezzo vero esce dalla prova 5 (sezione 9). Due regole commerciali valgono da subito:
- ciò che si vende come **inedito** non si pubblica prima sul sito;
- un gioco ceduto in **esclusiva** esce per sempre dal catalogo.

### 5.3 Scenari di ricavo a M24 [S]

Ricavi netti mensili, cioè dopo IVA e commissioni.

| Linea | Prudente | **Base** | Ottimista |
|---|---|---|---|
| L1 Fornitura | 0 € | 250 € (circa 3 cruciverba medi e 5 giochi brevi al mese) | 600 € |
| L2 Gioco del giorno in licenza | 0 € | 250 € (3 clienti tra Micro e Piccolo) | 800 € (6-8 clienti) |
| L3 Raccolte e libri | 60 € | 180 € (circa 60 copie al mese su tutto il catalogo) | 400 € |
| L4 Abbonamento | 0 € (mai attivato) | 150 € (circa 75 abbonati) | 500 € |
| **Ricavi netti** | **60 €** | **830 €** | **2.300 €** |
| Costi vivi mensili (anno 2) | circa 110 € | circa 150 € | circa 250 € |
| **Margine** | circa -50 € | **circa 680 €** | circa 2.050 € |
| Esito alla soglia di M24 (sezione 8) | fermarsi | continuare, di poco | continuare e investire |

Lo scenario ottimista non comprende la vendita di un format di successo: è un'opzione, non un piano. Con 7 ore a settimana, cioè circa 30 al mese, il caso base vale circa 23 € l'ora di ricavo netto.

---

## 6. Portafoglio prodotto

| Componente | Che cos'è | Perché commerciale | Priorità |
|---|---|---|---|
| **Motore di creazione** | Genera giochi candidati per tipologia con assistenza AI e li controlla con validatori automatici (griglia coerente, parole esistenti, soluzione unica dove serve). Contiene un **laboratorio dei format**: si descrive un nuovo tipo di gioco, l'AI propone lo schema, l'autore lo corregge e lo conferma. È il flusso "nuovo tipo, proposta, conferma" del prompt iniziale, applicato alla creazione invece che all'estrazione | È il fattore di costo dell'intera strategia: ogni minuto di revisione risparmiato vale più di tutto il costo AI di un gioco [D3]. I format originali sono l'unico contenuto che nessun concorrente ha | **MVP** (3 tipologie); laboratorio dei format completo nella seconda ondata |
| **Studio** | Applicazione web privata e monoutente del proprietario: coda di revisione, modifica, registro del contributo umano, prova di gioco, catalogo e diritti. In seguito: libreria di riferimento delle riviste, ricerca, involucro desktop | È dove nasce il valore d'autore, cioè il contributo umano che rende i giochi tutelabili e vendibili | **MVP**: revisione, registro, catalogo. **Seconda ondata**: involucro desktop, ricerca. **Dopo**: libreria delle riviste |
| **Lettore** | Componente che rende giocabile ogni tipologia: verifica, aiuti progressivi, caratteri grandi, stampa. Lo stesso componente gira sul sito pubblico e, incorporato, nei siti dei clienti | Serve a tutte le linee di ricavo. Un lettore incorporabile trasforma un gioco venduto in un servizio ricorrente (L2) | **MVP** sul sito; **seconda ondata** la versione incorporabile, ma l'architettura deve prevederla da subito |
| **Sito pubblico** | Sito aperto a tutti con gioco del giorno, archivio recente, guide alle tipologie, iscrizione alla newsletter, condivisione del risultato | Laboratorio (quali formati trattengono), portafoglio per i compratori, canale per raccolte e abbonamento | **MVP** |
| **Admin** | Area del sito per programmare e pubblicare i giochi, cambiare l'aspetto e vedere la dashboard d'uso | Senza programmazione anticipata il gioco del giorno consuma ore ogni giorno; senza dashboard non si possono verificare le soglie | **MVP**: calendario, pubblicazione, dashboard essenziale. **Seconda ondata**: aspetto del sito, dashboard per cliente. **Dopo**: rapporti avanzati |
| **Motore di estrazione** | Acquisizione delle pagine delle riviste del proprietario per farne una libreria privata di riferimento (formati, difficoltà, stile) | Nessun ricavo diretto, perché i giochi estratti non si pubblicano [P]. Può dare al Motore di creazione statistiche aggregate (lunghezze, tipologie, difficoltà), ma solo dopo il parere legale [D2] | **Dopo** (T6, marzo-maggio 2028), solo se il parere legale lo consente. Se il proprietario lo vuole prima per piacere personale, è tempo fuori dalle 7 ore commerciali |

---

## 7. Definizione dell'MVP

**Obiettivo dell'MVP**: rendere possibili le prove 1-4 (sezione 9). Non deve generare ricavi.

**Tipologie di partenza (3)**

| Tipologia | Ruolo | Perché |
|---|---|---|
| **Gioco breve del giorno**: crittografia breve **oppure** anagramma a tema, da scegliere con la prova 1 | Aggancio quotidiano, condivisibile | Basso costo di revisione [D3][D4]. La crittografia è un vuoto di mercato, ma generarne una buona con l'AI è difficile: l'anagramma a tema è la riserva se la crittografia supera i 20 minuti di revisione |
| **Cruciverba classico piccolo-medio** (9x9-13x13) | Prodotto riconoscibile, il più richiesto dai compratori | Il riempimento della griglia si fa con algoritmi noti; l'AI aiuta sulle definizioni; il costo stimato è di 45 minuti [D3] |
| **Un format originale** a uscita settimanale, scelto dal proprietario tra 2-3 idee | Prova della linea "nuovi format", la novità chiesta dal proprietario | Senza un format nell'MVP l'ipotesi più ambiziosa non viene misurata. Uscita settimanale, non quotidiana, per contenere le ore |

**Escluse dall'MVP**:
- **rebus**: le illustrazioni costano e i diritti sulle immagini generate dall'AI sono ancora da chiarire con il legale;
- **cruciverba senza schema**: è distintivo, quindi va nella seconda ondata;
- **sudoku e giochi logici numerici**: mercato saturo; utili solo per i libri, seconda ondata;
- **giochi in versi**: richiedono molto lavoro d'autore.

**Dentro l'MVP**
1. Motore di creazione per le 3 tipologie: generazione a lotti, validatori, tetto di spesa.
2. Studio: coda di revisione, modifica campo per campo, registro del contributo umano, catalogo con origine, diritti e stato di inedito, cronometro di revisione.
3. Lettore web responsive e accessibile: verifica, aiuti, salvataggio dei progressi senza account, stampa a caratteri grandi.
4. Sito pubblico: gioco del giorno, ultimi 7 giorni, format della settimana, 5-10 pagine guida sulle tipologie, newsletter con doppia conferma, condivisione del risultato senza svelare la soluzione.
5. Admin: calendario con programmazione di almeno 14 giorni, pubblicazione e ritiro, dashboard essenziale.
6. Misurazione senza cookie di profilazione.
7. Blocco di pubblicazione: online va solo ciò che è originale, approvato e ha diritti ammessi.

**Fuori dall'MVP**: pagamenti (la prima raccolta PDF si vende su un negozio esterno già pronto), account utente, lettore incorporabile, esportazioni per editori, aspetto modificabile, involucro desktop, motore di estrazione, app native.

**Criterio di "MVP pronto"** (fine febbraio 2027):
- 60 giochi approvati in coda, cioè circa 6 settimane di gioco del giorno più i format;
- il sito si apre e si gioca su smartphone, tablet e desktop;
- un utente di oltre 65 anni del gruppo di prova completa un gioco senza aiuto esterno;
- il registro del contributo umano è completo per ogni gioco pubblicato.

---

## 8. Roadmap a 24 mesi per trimestri, con soglie

### 8.1 Trimestri e traguardi

| Trimestre | Periodo | Obiettivo unico | Traguardi |
|---|---|---|---|
| **T1** | dic 2026 - feb 2027 | **Il motore funziona?** | Fondamenta tecniche (sezione 14.3); Motore di creazione e Studio per 3 tipologie; prova 1; scelta del gioco del giorno; parere legale richiesto e ricevuto; marchio neutro e dominio; 60 giochi in coda. **Soglia G1** |
| **T2** | mar - mag 2027 | **I giocatori tornano?** | Sito online a inizio marzo; gioco del giorno ogni giorno; newsletter; guide; prima raccolta PDF ad aprile; questionario sul prezzo; campionario per i compratori e prime 10 proposte. **Soglia G2** |
| **T3** | giu - ago 2027 | **Qualcuno compra i giochi?** | Esportazioni (PDF di stampa, ipuz, JSON) e dossier di paternità; 20 proposte inviate in totale; prima vendita; seconda tipologia di format o cruciverba senza schema. **Soglia G3** |
| **T4** | set - nov 2027 | **Ricavo ricorrente** | Lettore incorporabile con chiave per cliente e contatore di partite; lancio di L2 (primo cliente); primo libro KDP per Natale; aspetto del sito modificabile dall'Admin; involucro desktop dello Studio. **Soglia G4** |
| T5 | dic 2027 - feb 2028 | Consolidare L1 e L2 | 2-3 clienti L2; catalogo di 300 giochi; decisione sull'abbonamento (L4) se le soglie di attivazione sono raggiunte; secondo libro |
| T6 | mar - mag 2028 | Abbonamento o più fornitura | Pagamenti e L4 se attivato; altrimenti più ore su L1 e L2. Motore di estrazione privato, solo se il parere legale lo consente. **Soglia G5** |
| T7 | giu - ago 2028 | Format originali in licenza | Proposta di 1-2 format anche a pubblicazioni estere (logica indipendente dalla lingua); valutazione di L5 |
| T8 | set - nov 2028 | Decisione finale | Catalogo libri per il Natale 2028; bilancio dei 24 mesi. **Soglia G6** |

### 8.2 Soglie "continua / cambia / fermati"

**Ricavi netti mensili** = ricavi incassati al netto di IVA e commissioni, media degli ultimi 3 mesi. Uso i ricavi e non il margine perché nel primo anno le spese una tantum, come il parere legale, falserebbero il confronto. Il budget si controlla a parte (sezione 11).

| Soglia | Data | Metrica | **Continua** | **Cambia** | **Fermati** |
|---|---|---|---|---|---|
| **G1 Motore** | 28 feb 2027 (M3) | Minuti mediani di revisione e quota di candidati accettati (prova 1) | Gioco breve ≤ 20 minuti, cruciverba medio ≤ 60 minuti, almeno il 50% dei candidati accettato | Gioco breve tra 21 e 35 minuti: una sola tipologia quotidiana, format ogni 2 settimane, un mese in più di lavoro sul motore e nuova misura a fine marzo | Gioco breve oltre i 35 minuti anche dopo il mese in più: il motore non batte il lavoro a mano. Si ferma la strategia commerciale e lo Studio resta personale |
| **G2 Pubblico** | 31 mag 2027 (M6) | Iscritti, ritorno settimanale, vendite della raccolta PDF | Almeno 300 iscritti, almeno il 25% di ritorno, almeno 25 raccolte vendute | 100-299 iscritti oppure ritorno tra 15% e 24%: si cambiano il formato del giorno o i canali e si rimisura a fine agosto | Meno di 100 iscritti **e** meno di 10 vendite: si chiude il lato consumer (L3 e L4), il sito resta vetrina e le ore passano su L1 |
| **G3 Compratori** | 31 ago 2027 (M9) | Proposte, risposte, vendite | Almeno 20 proposte, almeno 3 risposte interessate, almeno 1 vendita pagata, anche piccola | Interesse senza vendita: prezzo, formato o segmento da rivedere e altre 20 proposte entro novembre | Meno di 3 risposte su 20: L1 e L2 restano aperte solo alle richieste spontanee. Se G2 era "fermati", si applica G4 in anticipo |
| **G4 Ricavo** | 30 nov 2027 (M12) | Ricavi netti mensili; compratori paganti | Almeno 200 € al mese e almeno 1 compratore pagante (L1 o L2) | 80-199 € al mese: si tiene solo la linea più redditizia | Meno di 80 € al mese e nessun compratore pagante: **stop commerciale**. Il progetto resta personale |
| **G5 Crescita** | 31 mag 2028 (M18) | Ricavi netti mensili | Almeno 450 € | 200-449 €: si concentra tutto sulla linea con più ricavo per ora | Meno di 200 €: stop commerciale |
| **G6 Esito** | 30 nov 2028 (M24) | Ricavi netti mensili | Almeno 800 € (circa 9.600 € l'anno): si continua e si valuta se aumentare le ore o cercare un autore partner pagato con i ricavi | 400-799 €: si resta in manutenzione con la sola linea migliore e il minimo di ore | Meno di 400 €: stop commerciale |

**Confronto con le soglie precedenti.**
- Il documento 02 chiedeva 15.000 € contrattualizzati al mese 12. Il documento 04 chiedeva 500 iscritti e un pilota al mese 4.
- Le mie soglie sono più basse perché il proprietario parte senza contatti, con meno ore e senza servizio agli editori.
- Sono più severe in un punto: fermano tutto già a M3 se il motore non funziona, perché senza motore la strategia non ha basi.

---

## 9. Prove decisive dei primi sei mesi

| # | Prova | Periodo | Ore | Euro | Misura | Soglia |
|---|---|---|---|---|---|---|
| 0 | **Parere legale** (prerequisito) | gen - feb 2027 | 3 | 500-1.000 € | Risposte scritte alle domande della sezione 12, rischio 2 | Nessun sito online prima del parere |
| 1 | **Costo di creazione**: il proprietario genera e rivede 5 candidati per ciascuna delle 6 famiglie, cronometrando | feb 2027 | 14 | 0 € di API (abbonamento Claude) | Minuti mediani di revisione, quota accettata, errori trovati da un secondo controllo | G1 |
| 2 | **Gradimento del formato**: gruppo di 20-30 persone, almeno metà sopra i 55 anni, gioca alla cieca 2-3 candidati per il gioco del giorno e il format | fine feb 2027 | 6 | 0 € | Completamento, tempo, voto da 1 a 5, "lo rigiocheresti domani?" | Si sceglie il formato con il completamento più alto e un voto di almeno 3,5 |
| 3 | **Abitudine**: sito online con gioco del giorno, condivisione e newsletter | mar - mag 2027 | 20 (canali e comunità) | 0-150 € di promozione | Iscritti, ritorno settimanale, partite completate | G2 |
| 4 | **Disponibilità a pagare dei solutori**: raccolta PDF da 50 giochi a 4,99 € e questionario sul prezzo (almeno 60 risposte) | apr - mag 2027 | 8 | 0-30 € | Vendite, conversione dalla newsletter, prezzo "troppo caro" mediano | Almeno 25 vendite; il questionario fissa il prezzo di L4 (regola del documento 03, esperimento 1) |
| 5 | **Disponibilità a pagare dei compratori**: campionario di 20 giochi, listino, dossier di paternità e demo; prime 10 proposte | mag 2027 (poi fino ad agosto) | 12 entro M6 | 0 € | Risposte, richieste di prezzo, obiezioni | Entro M6: almeno 2 risposte su 10. Poi G3 |
| | **Totale nei primi 6 mesi** | | **circa 63 ore** (dentro le ore di contenuti e mercato) | **500-1.180 €** | | |

*Aggiornato il 20/11/2026 per allinearsi al ROADMAP: la prova 1 misura sei famiglie con 5 candidati ciascuna, senza spesa API (abbonamento Claude). Le ore passano da 15 a 14, come nella fase 1b (`docs/fase-1b-decisioni.md`).*

---

## 10. Metriche

**Metrica guida**: **partite completate a settimana sui giochi DBEnigmistica, su tutti i canali** (sito più lettori incorporati nei siti dei clienti).
Misura insieme il valore per i giocatori e quello che si vende ai compratori, che pagano per far giocare i propri lettori.
Obiettivi [S]: 800 a settimana a M6, 2.500 a M12, 8.000 a M24.

| # | Metrica di supporto | Perché | Obiettivo |
|---|---|---|---|
| 1 | Minuti mediani di revisione per un gioco breve | Determina la capacità produttiva (tensione 2) | ≤ 20 a M3; ≤ 15 a M12 |
| 2 | Quota di candidati del motore accettati dopo la revisione | Qualità del motore e costo AI per gioco pubblicato | ≥ 50% a M3; ≥ 65% a M12 |
| 3 | Ritorno settimanale (giocatori di una settimana che giocano anche la successiva) | Abitudine, cioè il valore del gioco del giorno | ≥ 25% a M6; ≥ 35% a M12 |
| 4 | Iscritti alla newsletter | Unico canale di proprietà; condiziona L3 e L4 | 300 a M6; 1.000 a M12; 3.000 a M24 |
| 5 | Ricavi netti mensili (media su 3 mesi) | Obiettivo del proprietario | 200 € a M12; 450 € a M18; 800 € a M24 |

---

## 11. Ore settimanali (tensione 2) e budget

### 11.1 Come si dividono 5-10 ore

Lo sviluppo è delegato in gran parte ad agenti AI. Al proprietario restano specifiche, revisione del codice, prove e rilascio: stimo circa 1 ora sua ogni 3-5 ore di lavoro degli agenti [S].
La revisione dei giochi invece non si delega, perché è proprio il contributo umano che li rende tutelabili.

**Piano base: 7 ore a settimana.**

| Periodo | Sviluppo (guida degli agenti) | Contenuti (creazione e revisione) | Mercato e contatti | Totale |
|---|---|---|---|---|
| T1 (dic-feb) | 4 | 2,5 | 0,5 | 7 |
| T2 (mar-mag) | 3 | 3 | 1 | 7 |
| T3-T4 (giu-nov 2027) | 2 | 3,5 | 1,5 | 7 |
| Anno 2 | 1,5 | 4 | 1,5 | 7 |

**Verifica di capacità** (anno 2, 4 ore di contenuti = 240 minuti):
- gioco del giorno: 7 × 15 minuti = 105 minuti;
- restano 135 minuti: circa 2 cruciverba medi a settimana (inediti per la vendita o format), oppure il lavoro su raccolte e pacchetti.

Se G1 misura 20 minuti invece di 15, il gioco del giorno sale a 140 minuti e resta un solo cruciverba a settimana.

**Varianti**

| Ore a settimana | T1 | T2 | T3-T4 | Anno 2 | Cosa si perde |
|---|---|---|---|---|---|
| **5** | 3 / 1,5 / 0,5 | 2 / 2 / 1 | 1,5 / 2,5 / 1 | 1 / 3 / 1 | MVP pronto a fine marzo invece che a fine febbraio, con tutte le soglie spostate di un mese; un solo cruciverba ogni 2 settimane; L2 slitta a T5 |
| **10** | 5 / 4 / 1 | 4 / 4 / 2 | 3 / 5 / 2 | 2 / 5,5 / 2,5 | Nessuna perdita: si anticipano il lettore incorporabile (T3) e la seconda tipologia |

Regola: **quando le ore scendono si taglia prima lo sviluppo nuovo, poi i contatti, e per ultima la cadenza del gioco del giorno**. Il pubblico perdona una funzione in meno, non un giorno senza gioco.

### 11.2 Budget dei primi 12 mesi [S salvo dove indicato]

| Voce | Intervallo | Valore centrale | Base |
|---|---|---|---|
| AI per la creazione dei giochi (API) | 100-250 € | 150 € | 0,10-0,30 € a gioco con circa 3 tentativi [D3]; circa 800 giochi generati, scarti compresi |
| Strumenti di sviluppo con agenti AI | 240-1.200 € | 480 € | 20-100 € al mese, listino non verificato in questo documento |
| Hosting | 180-240 € | 200 € | [D3], server Hetzner |
| Dominio, posta, newsletter | 40-150 € | 80 € | [D3]; piano gratuito della newsletter fino a qualche migliaio di iscritti |
| **Parere legale**, termini d'uso, privacy, contratto tipo di fornitura | 500-1.000 € | 800 € | "Qualche centinaio di euro" per il solo parere [D2][D3], più il contratto |
| Ricerca di anteriorità del marchio e marca temporale | 0-200 € | 100 € | Per il marchio neutro e la data certa del registro |
| Promozione (solo nelle prove) | 0-300 € | 150 € | Piccoli test sui social, prova 3 |
| Commercialista e posizione fiscale | 0-800 € | 300 € | 800 € l'anno con partita IVA [D3]; dipende dalla domanda aperta 3 |
| Copie d'autore dei libri, materiali | 30-100 € | 50 € | - |
| **Totale** | **circa 1.100-4.200 €** | **circa 2.300 €** | |

Costi ricorrenti dal secondo anno: circa 1.300-1.800 € l'anno, cioè 110-150 € al mese.

---

## 12. Legge 132/2025: cosa deve registrare il prodotto per ogni gioco (tensione 3)

**Contesto** [D1][D2]. La legge tutela le opere "dell'ingegno umano" anche se create con l'ausilio dell'AI, purché siano frutto del lavoro intellettuale dell'autore. Un output con scarso contributo umano può non essere tutelato. In quel caso non si può né vendere in esclusiva né difendere dalla copia.

La legge **non dice cosa registrare**: il registro qui sotto è una scelta prudente a fini di prova [S], da confermare con il legale.

| # | Cosa registrare | Perché |
|---|---|---|
| 1 | **Autore umano** (nome o pseudonimo collegato a una persona) e ruolo | Identifica il titolare |
| 2 | **Brief creativo umano**, scritto prima della generazione: tema, idea, vincoli, difficoltà voluta, pubblico | Prova che l'idea e le scelte di fondo sono umane |
| 3 | **Ogni chiamata AI**: modello e versione, istruzioni inviate, parametri, risposta grezza, data e ora, costo | Separa ciò che ha fatto la macchina |
| 4 | **Selezione**: candidati generati, scartati e motivo dello scarto | La selezione è parte del lavoro d'autore |
| 5 | **Modifiche umane campo per campo**: differenze tra la risposta AI e la versione finale (definizioni riscritte, parole cambiate, griglia modificata, soluzione o esposto riformulati) | È il cuore della prova del contributo |
| 6 | **Tempo umano** dedicato, con cronometro automatico e correzione manuale | Indica la sostanza del lavoro; serve anche alla metrica 1 |
| 7 | **Controllo di originalità**: similarità con la libreria privata delle riviste, con il proprio catalogo e, dove possibile, con fonti pubbliche; esito e soglia | Esclude giochi che ricalcano opere altrui (rischio di opera derivata [D2]) |
| 8 | **Dichiarazione di prompt pulito**: nessun gioco protetto di terzi usato come esempio nelle istruzioni (controllo automatico più conferma) | Il documento 02 classifica questo uso come rischio di opera derivata |
| 9 | **Approvazione finale** con impronta SHA-256 della versione approvata e marca temporale; registro in sola aggiunta, non modificabile | Data certa e integrità |
| 10 | **Origine e diritti**: origine (originale, licenza, pubblico dominio, privato di riferimento), titolare, licenze e cessioni concesse (a chi, quando, esclusiva o no, territorio, durata), stato di inedito | Impedisce di vendere due volte in esclusiva e di pubblicare ciò che si è promesso inedito |
| 11 | **Dossier di paternità** esportabile in PDF e JSON per ogni gioco | Lo chiederanno i compratori, che vogliono garanzie di originalità, e serve per dichiarare il contenuto AI su KDP |

Da valutare con il legale: se basta una marca temporale qualificata secondo il regolamento eIDAS o serve un deposito presso un ente. Va valutato anche se pubblicare con un nome o con uno pseudonimo.

---

## 13. Piano di ingresso sul mercato senza contatti

### 13.1 Canali concreti

| Canale | Per chi | Come | Costo |
|---|---|---|---|
| **Condivisione del risultato** del gioco del giorno (testo e simboli, senza svelare la soluzione) | Solutori | Pensata per WhatsApp e i gruppi di famiglia: è il meccanismo che ha fatto crescere Wordle [D1] | 0 € |
| **Newsletter** con il gioco del giorno o della settimana | Solutori | Unico canale di proprietà; base per L3 e L4 | 0 € fino al piano gratuito |
| **Ricerca organica**: guide alle tipologie (come si risolve una crittografia), giochi stampabili a caratteri grandi, "giochi per anziani da stampare" | Solutori, animatori | 10 pagine stabili, poi una al mese | Solo ore |
| **Gruppi e pagine social di enigmistica** (dimensione [N]) e gruppi di animatori sociali | Solutori, animatori | Un gioco a settimana, nel rispetto delle regole del gruppo, senza vendere | Solo ore |
| **Amazon KDP** | Acquirenti di libri | È un canale di scoperta: chi cerca "enigmistica caratteri grandi" trova il libro | 0 € (royalty [F5]) |
| **Comunità enigmistica classica** (ARI, Enignet, Congresso) | Appassionati | Gruppo di prova severo e credibilità; nessuna vendita | 0 € |
| **Proposte a freddo a compratori**: testate online locali (le associazioni di categoria della stampa online locale servono a trovarle), newsletter italiane, piccoli settimanali, riviste che accettano collaboratori; per i format logici, pubblicazioni estere che pagano gli autori [F2][F3] | Compratori | Campionario, listino, dossier, demo; 5 proposte a settimana, sollecito dopo 10 giorni | Solo ore |

### 13.2 Le prime 10 azioni

| # | Azione | Quando | Uscita |
|---|---|---|---|
| 1 | Scegliere un **marchio neutro**, lontano da "Settimana" e dai nomi delle testate [D4]; ricerca di anteriorità; registrare il dominio | Dicembre 2026, settimane 1-2 | Nome e dominio |
| 2 | Inviare al legale le domande scritte (sezione 12 e rischio 2) | Gennaio 2027 | Parere entro fine febbraio |
| 3 | Eseguire le prove 1 e 2 e scegliere il gioco del giorno e il format | Febbraio 2027 | Decisione G1 |
| 4 | Riempire la coda: 60 giochi approvati prima del lancio | Febbraio 2027 | 6 settimane di riserva |
| 5 | Mettere online il sito e invitare di persona 30 conoscenti, almeno metà sopra i 55 anni, come primo gruppo | Inizio marzo 2027 | Primi iscritti e prime osservazioni |
| 6 | Pubblicare 10 pagine guida e 5 giochi stampabili a caratteri grandi | Marzo 2027 | Base per la ricerca organica |
| 7 | Presentare il gioco della settimana in 5 gruppi o comunità e alla comunità enigmistica classica | Marzo-aprile 2027 | Traffico e revisori severi |
| 8 | Pubblicare la prima raccolta PDF a 4,99 € e il questionario sul prezzo | Aprile 2027 | Prova 4 |
| 9 | Preparare la cartella del fornitore (20 giochi inediti, listino, dossier, demo) e un elenco di 40 compratori potenziali | Aprile-maggio 2027 | Pronti per le proposte |
| 10 | Inviare 5 proposte a settimana con sollecito, tracciate in un semplice registro dei contatti | Da maggio 2027 | 10 entro M6, 20 entro M9 (G3) |

---

## 14. Requisiti di prodotto per il team tecnico

### 14.1 Requisiti

Priorità: **MVP** (entro febbraio 2027), **2** (seconda ondata, marzo-novembre 2027), **D** (dopo).

**A. Origine, diritti e separazione**

| ID | Requisito | Priorità |
|---|---|---|
| RQ-01 | Ogni gioco ha un'**origine obbligatoria** senza valore predefinito: originale, licenza, pubblico dominio, privato di riferimento | MVP |
| RQ-02 | Ogni gioco ha **titolare e diritti**: licenze e cessioni concesse (cliente, data, esclusiva o no, territorio, durata); il sistema impedisce una seconda cessione esclusiva | MVP (campi); 2 (gestione delle vendite) |
| RQ-03 | Stato di **inedito**: un gioco promesso o venduto come inedito non si può pubblicare sul sito | MVP |
| RQ-04 | **Blocco di pubblicazione** applicato sul server: sito, esportazioni e lettore incorporato servono solo giochi originali, in licenza o di pubblico dominio verificato, approvati e con registro completo. Un test automatico lo dimostra | MVP |
| RQ-05 | I **materiali delle riviste** stanno in un archivio separato, irraggiungibile dal sito pubblico e dall'Admin | MVP (separazione); D (libreria) |

**B. Registro del contributo umano**

| ID | Requisito | Priorità |
|---|---|---|
| RQ-06 | Brief creativo umano obbligatorio prima della generazione | MVP |
| RQ-07 | Registrazione di ogni chiamata AI: modello, versione, istruzioni, parametri, risposta grezza, data, costo | MVP |
| RQ-08 | Versioni con differenze campo per campo tra risposta AI e finale; candidati scartati con motivo | MVP |
| RQ-09 | Cronometro del tempo umano per gioco, con correzione manuale | MVP |
| RQ-10 | Controllo di originalità contro la libreria privata e il proprio catalogo, con soglia e blocco | MVP (contro il catalogo); 2 (contro la libreria) |
| RQ-11 | Il Motore di creazione non può usare giochi della libreria privata come esempi nelle istruzioni per giochi pubblicabili; ammesse solo specifiche di formato e statistiche aggregate | MVP |
| RQ-12 | Approvazione con impronta SHA-256 e marca temporale; registro in sola aggiunta, con salvataggio e ripristino provati | MVP |
| RQ-13 | Esportazione del dossier di paternità in PDF e JSON | 2 |

**C. Lettore e accessibilità**

| ID | Requisito | Priorità |
|---|---|---|
| RQ-14 | Lettore web responsive per smartphone, tablet orizzontale e desktop, senza installazione | MVP |
| RQ-15 | **Accessibilità per un pubblico anziano**: conformità WCAG 2.2 livello AA; testo predefinito di almeno 18 px e ingrandibile fino al 200% senza rompere la griglia; alto contrasto; aree di tocco di almeno 44 × 44 px; nessun limite di tempo; uso completo da tastiera; istruzioni in italiano semplice | MVP |
| RQ-16 | Verifica di casella e parola, aiuti progressivi, progressi salvati sul dispositivo senza account | MVP |
| RQ-17 | Stampa A4 a caratteri grandi, con soluzioni su una pagina separata | MVP |
| RQ-18 | Condivisione del risultato senza svelare la soluzione, adatta a WhatsApp | MVP |
| RQ-19 | **Lettore incorporabile** (iframe o web component) con chiave per cliente, domini autorizzati, colori del cliente e contatore di partite per cliente (base del canone L2) | 2 (architettura pronta già nell'MVP) |

**D. Motore di creazione**

| ID | Requisito | Priorità |
|---|---|---|
| RQ-20 | La tipologia è un **dato**: schema versionato per tipologia (modello `game_types` con payload JSONB della valutazione tecnica); un nuovo format richiede solo schema, validatore e rappresentazione nel lettore | MVP |
| RQ-21 | **Laboratorio dei format**: descrizione del nuovo gioco, schema proposto dall'AI, modifica e conferma dell'autore; nessuna creazione di tabelle a runtime | 2 |
| RQ-22 | Validatori automatici per tipologia: griglia coerente, parole presenti nel dizionario, diagramma corretto, soluzione unica dove applicabile | MVP |
| RQ-23 | Generazione a lotti con tetto di spesa mensile e costo registrato per gioco pubblicato | MVP |
| RQ-24 | Livello di difficoltà da 1 a 5 assegnato dall'autore e confrontato con i dati di gioco | MVP (assegnazione); 2 (confronto) |
| RQ-25 | Coda editoriale con stati: bozza, in revisione, approvato, programmato, pubblicato, venduto, ritirato | MVP |

**E. Sito pubblico, Admin e misurazione**

| ID | Requisito | Priorità |
|---|---|---|
| RQ-26 | Calendario con programmazione di almeno 14 giorni e pubblicazione automatica del gioco del giorno; avviso se la coda copre meno di 7 giorni | MVP |
| RQ-27 | Newsletter con doppia conferma e cancellazione in un clic; dati personali ridotti al minimo (GDPR) | MVP |
| RQ-28 | **Misurazione dell'uso** senza cookie di profilazione: partite iniziate e completate, tempo, aiuti usati, abbandoni per gioco, ritorno settimanale, fonte del traffico; separata per canale (sito o cliente L2) | MVP (sito); 2 (per cliente) |
| RQ-29 | Dashboard dell'Admin con la metrica guida e le 5 di supporto (sezione 10) | MVP (essenziale); 2 (completa) |
| RQ-30 | Segnalazione di un errore da parte dei giocatori; correzione o ritiro tracciati nel registro | MVP |
| RQ-31 | Aspetto del sito (colori, caratteri, pagine) modificabile dall'Admin senza rilasciare nuovo codice | 2 |

**F. Esportazione e vendita**

| ID | Requisito | Priorità |
|---|---|---|
| RQ-32 | **Esportazione per editori**: PDF di stampa vettoriale (A4 e formati rivista), ipuz (formato aperto per i cruciverba), JSON, testo delle definizioni; soluzioni in un file separato; dossier allegato | 2 (entro agosto 2027) |
| RQ-33 | Esportazione degli interni di un libro KDP: margini e abbondanze di stampa, caratteri grandi, indice delle soluzioni | 2 (entro settembre 2027) |
| RQ-34 | Niente pagamenti propri prima di L4: le raccolte si vendono su un negozio esterno. Con L4 si usano pagamenti web con l'annuale come opzione predefinita [D3] | D |

**G. Studio**

| ID | Requisito | Priorità |
|---|---|---|
| RQ-35 | Studio monoutente con autenticazione forte, non esposto su internet senza protezione | MVP |
| RQ-36 | Involucro desktop dello Studio | 2 |
| RQ-37 | Motore di estrazione e libreria delle riviste solo per uso privato, attivati solo dopo il parere legale favorevole | D |

### 14.2 Requisiti che cambiano rispetto al prompt iniziale

- Il **flusso "nuovo tipo, proposta, conferma"** del prompt iniziale resta, ma diventa il laboratorio dei format del Motore di creazione (RQ-21) invece che un passo della scansione.
- La creazione di tabelle a runtime si abbandona, come raccomandato dalla valutazione tecnica [VT, sezione 6.3].
- La **console a riga di comando** non serve all'MVP, salvo per compiti di manutenzione.
- La **pipeline di estrazione** è rinviata a T6 (marzo-maggio 2028).

### 14.3 Ordine suggerito per le fasi tecniche

1. **Fondamenta**: primo commit, passaggio a .NET 10, modello `game_types` con payload JSONB, origine, diritti e registro (RQ-01-12), test su PostgreSQL reale [VT, sezione 7].
2. **Motore di creazione v0 e Studio di revisione** per le 3 tipologie (RQ-20, 22-25, 35).
3. **Lettore accessibile, Sito pubblico, Admin minimo, misurazione** (RQ-14-18, 26-30).
4. **Esportazioni, dossier, lettore incorporabile** (RQ-13, 19, 32-33).
5. **Aspetto del sito, dashboard per cliente, laboratorio dei format, involucro desktop** (RQ-21, 29, 31, 36).
6. **Pagamenti e abbonamento** se attivati (RQ-34); **estrazione privata** se il parere lo consente (RQ-37).

Raccomandazione, non vincolo: un solo codice con tre superfici (Studio, Sito, Admin) e permessi separati, per contenere il lavoro di manutenzione di una persona sola.

---

## 15. Rischi principali e contromisure

| # | Rischio | Probabilità / impatto [S] | Contromisura | Segnale d'allarme |
|---|---|---|---|---|
| 1 | **Il motore non abbassa il tempo di revisione**: l'AI è debole sui giochi di parole italiani, soprattutto sulle crittografie | Media / altissimo | Soglia G1 a M3; validatori automatici; anagramma a tema come riserva; partire dai formati più strutturati | Mediana sopra i 20 minuti alla prova 1 |
| 2 | **Legale**: (a) un gioco somiglia troppo a un gioco protetto; (b) giochi non tutelabili per scarso contributo umano; (c) uso delle riviste oltre l'uso personale o l'estrazione consentita (sanzioni della L. 132/2025 [D2]); (d) diritti sulle immagini AI per i rebus; (e) obblighi di dichiarazione su KDP | Media / alto | Parere legale prima del lancio (domande scritte su a-e); registro della sezione 12; prompt senza giochi protetti (RQ-11); controllo di originalità (RQ-10); riviste separate (RQ-05); rebus esclusi fino al chiarimento; procedura di rimozione nei termini d'uso | Una diffida, oppure un parere che boccia lo Studio in cloud |
| 3 | **Nessun compratore** risponde, perché il proprietario parte senza contatti | Alta / alto | Campionario e demo pronti prima delle proposte; prezzi bassi e prova gratuita su L2; segmenti piccoli e numerosi (testate locali, newsletter) invece dei grandi editori; format logici anche all'estero; soglia G3 | Meno di 2 risposte sulle prime 10 proposte |
| 4 | **Dispersione e tempo**: tre superfici, due motori, cinque linee di ricavo | Alta / alto | Un obiettivo per trimestre (sezione 8.1); regola dei tagli (sezione 11.1); estrazione rinviata; niente app native | Due trimestri di fila con il traguardo mancato |
| 5 | **Giochi copiati** da siti terzi o da imitatori che usano lo stesso modello AI | Media / medio | Registro con data certa; una voce trappola riconoscibile nei cruciverba; termini d'uso; il valore sta nella revisione e nel marchio, non nel singolo gioco | Giochi propri trovati altrove |
| 6 | **Un errore in un gioco pubblicato** compromette la reputazione presso il pubblico esperto | Media / medio | Validatori; secondo controllo prima della programmazione; segnalazione dai giocatori e correzione rapida (RQ-30) | Più di 1 errore segnalato ogni 50 giochi |
| 7 | **Reazione di un grande** (il Corriere copia un format, Bresi contesta il marchio) | Bassa / medio | Marchio neutro; nessuna imitazione delle "varietà" né delle rubriche note [D4]; proporsi come fornitore prima di essere copiati | Diffida, oppure un formato simile su un sito noto |
| 8 | **Fisco e burocrazia**: vendite senza un regime adatto, IVA sulle vendite a consumatori di altri paesi UE [D3] | Media / medio | Commercialista prima della prima vendita (domanda aperta 3); negozi esterni che gestiscono l'IVA per le raccolte | Prima vendita senza un regime deciso |
| 9 | **Dipendenza dal fornitore AI** (prezzi, modelli ritirati) | Media / basso | Interfaccia astratta [VT]; tetto di spesa (RQ-23); il costo AI pesa poco rispetto al tempo umano [D3] | Costo per gioco pubblicato sopra 1 € |
| 10 | **Calo di motivazione** prima dei risultati | Media / alto | Soglie fisse con date; traguardi trimestrali piccoli e visibili; la regola "fermati" rende accettabile smettere | Ore effettive sotto 4 a settimana per un mese |

---

## 16. Domande aperte per il proprietario

1. **Soglia di reddito**: confermi 800 € netti al mese a novembre 2028 come "continua" e 400 € come "fermati"? Se il tuo numero è un altro, va scritto ora, perché tutte le soglie intermedie ne derivano.
2. **Cessione dei diritti**: sei disposto a vendere giochi in esclusiva, a un prezzo doppio, sapendo che escono per sempre dal tuo catalogo? Oppure solo licenze non esclusive?
3. **Posizione fiscale e vincoli**: puoi svolgere un'attività commerciale (partita IVA o prestazioni occasionali) e ci sono vincoli contrattuali con altre attività che svolgi? Va chiarito prima della prima vendita, prevista ad aprile 2027.
4. **Firma**: pubblichi con il tuo nome o con uno pseudonimo? Lo pseudonimo va bene, ma deve essere collegato a te nel registro.
5. **Format originali**: hai già 2-3 idee di nuovi giochi da mettere nella prova 2? Se no, il primo trimestre comprende anche un'ora a settimana per idearli.
6. **Budget**: confermi circa 2.300 € per i primi 12 mesi, con un tetto massimo di 4.200 €? Vale anche se il tuo abbonamento agli strumenti di sviluppo con agenti AI è già pagato per altri motivi.

---

## Fonti nuove di questo documento

Tutte consultate il 10/11/2026. Le fonti dei documenti 01-04 sono negli elenchi di quei documenti.

- [F1] Daily Hampshire Gazette, "A rare feat: Florence crossword fan cracks the New York Times with first published puzzle", 25/8/2026 (compensi NYT e 97% di proposte scartate). https://gazettenet.com/2026/08/25/a-rare-feat-florence-crossword-fan-cracks-the-new-york-times-with-first-published-puzzle/ ; conferma delle stesse tariffe in Berkeleyside, 11/7/2021: https://www.berkeleyside.org/2021/07/11/meet-the-bay-area-word-nerds-that-make-new-york-times-crosswords ; cronologia delle tariffe 1993-2019 in Communicrossings: https://communicrossings.com/node/136
- [F2] Spyscape, "Crossword Spec Sheet" (200 dollari a gioco, cessione di tutti i diritti), documento del 27/9/2024. https://asset-cache.spyscape.com/5c0e988989879695ede9d753/6956e2eb5eaa5f8c985f15ac_Spyscape%20Crossword%20Spec%20Sheet.pdf
- [F3] Artisanal Sudoku, "How to Submit a Puzzle" (50 dollari a gioco da novembre 2024). https://artisanalsudoku.substack.com/p/how-to-submit-a-puzzle
- [F4] Marketplace, "Gridlock: Crossword constructors puzzled by pay", 11/12/2012 ("most puzzles pay under $100"). https://www.marketplace.org/2012/12/11/gridlock-crossword-constructors-puzzled-pay ; The Awl, "Solving The Broken Crossword Puzzle Economy" (media indipendente oltre 200 dollari a gioco). https://www.theawl.com/?p=73112
- [F5] Amazon KDP, pagina di aiuto sulle royalty dei libri stampati (50% sotto soglia dal 10/6/2025). https://kdp.amazon.com/help/topic/G201834330 ; sulla dichiarazione dei contenuti AI, solo fonti secondarie, per esempio: https://www.builtwritten.com/blog/can-you-publish-an-ai-written-book-on-kdp
- [F6] xwordinfo, "Value" (aumenti NYT dal 1/1/2017 per autori con almeno 10 giochi pubblicati). https://www.xwordinfo.com/Value
- [F7] Best Crosswords, "Crossword Constructor Program" (compenso dopo il periodo di prova, l'autore resta proprietario, licenza non esclusiva). https://www.bestcrosswords.com/bestcrosswords/guestconstructor/GuestConstructorProgram.page
- [F8] Nieman Lab, "The New York Times buys Wordle for a price in the low seven figures", 31/1/2022. https://www.niemanlab.org/2022/01/the-new-york-times-buys-wordle-for-a-price-in-the-low-seven-figures/

**Cercati e non trovati [N]**:
- compensi degli autori italiani di enigmistica (editori, riviste, associazioni);
- listini di syndication di giochi per piccoli giornali (solo su preventivo: Daily Sudoku, Andrews McMeel, Roko);
- compensi dei fornitori europei di giochi logici, Keesing compresa;
- soglia di prezzo per la royalty del 60% su amazon.it;
- testo ufficiale di KDP sulla dichiarazione dei contenuti AI;
- dimensione delle comunità social italiane di enigmistica.
