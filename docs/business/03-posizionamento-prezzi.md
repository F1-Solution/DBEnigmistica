# DBEnigmistica - Posizionamento di prezzo

Documento 03 della serie business. Data: 10 novembre 2026. Autore: Pricing Analyst.
Input letti per intero: `01-ricerca-mercato-e-tendenze.md` (Trend Researcher) e `02-strategia-e-competitor.md` (Business Strategist).
Destinatario successivo: Product Manager (piano commerciale).

**Convenzioni** (allineate al documento 01)
- **[V]**: dato verificato su fonte primaria, con riferimento [P1], [P2]... all'elenco delle fonti in fondo.
- **[V2]**: fonte secondaria (aggregatore, blog di terzi, sintesi di ricerca) oppure dato ripreso dai documenti 01 e 02.
- **[S]**: stima mia, con ipotesi o calcolo esplicito.
- **[N]**: cercato e non trovato.
- Prezzi B2C **IVA inclusa**; prezzi B2B **IVA esclusa**. Cambio usato: 1 euro = 1,12 dollari [V2][P10].

---

## Sintesi (10 righe)

1. Il costo AI è trascurabile: circa **0,06 euro per pagina** acquisita con Sonnet 5.5 in batch (intervallo 0,02-0,20). Il vero costo è il **tempo umano** di correzione e di revisione.
2. Una pagina corretta a mano costa circa 4,75 euro (8 minuti a 35 euro l'ora); un gioco originale rivisto costa da 7 a 29 euro. Il prezzo va costruito sui minuti, non sui token.
3. **Linea giocatori**: base gratuita con il gioco del giorno; piano "Solutore" a **3,49 euro al mese o 29,99 euro all'anno**. Prezzo fondatori 19,99 euro il primo anno, prova di 14 giorni senza carta.
4. Pareggio giocatori: 54 abbonati sui soli costi vivi, **circa 580 abbonati** a costo pieno (tempo valorizzato), cioè circa 29.000 utenti attivi al mese con conversione al 2%.
5. **Linea editori**: canone del lettore più prezzo per pagina, in tre fasce. Il Medio costa 3.500 euro di avvio, 299 euro al mese e 1,90 euro a pagina corretta dalla redazione. Pilota a 2.900 euro.
6. Pareggio editori: **1 cliente Medio** a costo pieno nel primo anno, **2 clienti Medi** a regime (solo canone e nuove pagine).
7. Sensibilità: sui giocatori il prezzo conta poco (con elasticità -1 il profitto varia di meno del 30% tra -30% e +30%); conta la conversione, che se si dimezza raddoppia gli utenti necessari. Sugli editori il -30% porta il primo anno in perdita.
8. **La linea editori ha l'economia migliore** per uno sviluppatore singolo: pareggio con 1-2 contratti invece di circa 580 abbonati, circa 50 euro l'ora già nel primo anno, nessuna produzione quotidiana di contenuti.
9. Il sito pubblico resta aperto a tutti e gratuito (come chiede il proprietario). Il piano a pagamento per i giocatori ha già i prezzi fissati, ma si attiva solo oltre la soglia di 5.000 utenti attivi al mese e 200 giochi in archivio.
10. Ipotesi più fragili: minuti di correzione per pagina, presenza di personale dell'editore per la correzione in autonomia, conversione dei giocatori, valore dell'ora del proprietario.

---

## 1. Struttura dei costi unitari

### 1.1 Ipotesi trasversali [S]

| Ipotesi | Valore | Motivo |
|---|---|---|
| Valore dell'ora del proprietario | **35 euro l'ora** | Costo-opportunità di uno sviluppatore freelance in Italia, prudente. Serve a confrontare le due linee: a 0 euro l'ora entrambe pareggiano subito in cassa. |
| Ore disponibili | **circa 460 ore l'anno** (10 a settimana per 46 settimane) | Il documento 02 (sezione 9, domanda 3) chiede questo dato al proprietario; finché non risponde uso questa ipotesi. |
| Sviluppo del prodotto | escluso dai costi di linea | Il motore, lo Studio e il lettore servono a entrambe le linee: è un costo comune e non cambia la scelta. |
| Pagine di gioco per numero di rivista | 40 | Ordine di grandezza di un settimanale di enigmistica. |

### 1.2 Costo AI per pagina acquisita

Modello di fascia media: **Claude Sonnet 5.5**. Listino [V][P1]:
- standard: 2 dollari per milione di token in input, 10 in output;
- **in batch (sconto del 50%)**: 1 dollaro in input, 5 in output.

Il prompt caching si somma allo sconto batch [V][P1], ma sul prompt di sistema fa risparmiare meno di 0,003 euro a pagina, quindi lo ignoro.

Token dell'immagine [V][P2]: costo pari a ⌈larghezza/28⌉ × ⌈altezza/28⌉. Sui modelli 4.7 e successivi il tetto è 2.576 pixel sul lato lungo e **4.784 token** per immagine. Una pagina A4 a 200-300 dpi supera il tetto e viene ridotta, quindi costa **circa 4.780 token**. Nota [V][P1]: dai modelli 4.7 il tokenizer produce circa il 30% di token in più a parità di testo, e le stime di output qui sotto ne tengono conto.

| Voce [S] | Basso | **Centrale** | Alto |
|---|---|---|---|
| Token immagine | 4.780 | 4.780 | 4.780 |
| Prompt di testo (istruzioni più schema JSON delle tipologie) | 2.000 | 3.000 | 4.200 |
| Token in output (JSON di griglie, definizioni, soluzioni, più ragionamento) | 3.000 | 8.000 | 20.000 |
| Passaggi per pagina (rielaborazioni, verifica) | 1 | 1,5 | 2 |
| Costo in batch, in dollari | 0,022 | 0,072 | 0,218 |
| **Costo in batch, in euro** | **0,02** | **0,06** | **0,20** |
| Senza batch (correzione interattiva nello Studio) | 0,04 | 0,13 | 0,39 |
| Opus 5.5 in batch (2 e 10 dollari) [V][P1], per le pagine difficili | 0,04 | 0,13 | 0,39 |

Per numero di 40 pagine: **circa 2,6 euro** (intervallo 0,8-7,8). Il criterio G1 del documento 02 ("costo AI per pagina compatibile con un prezzo per numero di qualche euro") è quindi rispettato **per la sola AI**. Non lo è per il servizio completo (vedi 1.3).

### 1.3 Costo per pagina digitalizzata (AI più correzione umana) [S]

| Scenario | Minuti umani per pagina | Costo del tempo | AI | **Costo pieno per pagina** |
|---|---|---|---|---|
| Servizio completo, correggiamo noi | 8 (intervallo 4-15) | 4,67 | 0,07 | **4,74** (2,40-8,82) |
| Correzione a cura della redazione nello Studio; noi facciamo controllo a campione | 1 | 0,58 | 0,07 | **0,65** |

L'AI pesa l'1,5% del costo del servizio completo. **Ogni minuto di correzione risparmiato vale circa 8 volte il costo AI dell'intera pagina.** Questa è la variabile da misurare al cancello G1, insieme all'accuratezza.

### 1.4 Costo per gioco originale prodotto con AI e revisione umana [S]

La generazione del testo usa Sonnet 5.5, con circa 3 tentativi per ogni gioco pubblicato. L'illustrazione dei rebus richiede un modello di immagini diverso da Claude: **il listino non è stato verificato** e uso un intervallo di 0,02-0,20 euro per immagine.

| Tipo di gioco | AI | Minuti di revisione umana | **Costo pieno** |
|---|---|---|---|
| Gioco breve del giorno (crittografia, anagramma, parola del giorno) | 0,10 | 12 | **7,10** |
| Cruciverba o "varietà" premium, con definizioni riscritte | 0,30 | 45 | **26,55** |
| Rebus illustrato | 0,10 + 0,02-0,20 | 50 | **circa 29,40** |

Piano editoriale minimo per la linea giocatori (coerente con i 7-15 giochi a settimana del documento 01):
- composizione: 7 giochi brevi, 3 premium e 1 rebus a settimana;
- costo: **circa 159 euro a settimana**, cioè **circa 8.250 euro l'anno**, di cui meno di 100 euro di AI;
- tempo: **233 ore l'anno**, pari alla metà del tempo disponibile.

La revisione umana non è facoltativa: per la L. 132/2025 un gioco senza contributo umano non è tutelabile (documento 02, sezione 2).

### 1.5 Hosting e costi fissi in denaro [S, su listini V]

| Voce | Costo annuo | Base |
|---|---|---|
| Server cloud | circa 180-240 euro | Hetzner da 5,49 euro al mese IVA esclusa [V][P5]. Ho stimato circa 15-20 euro al mese con backup, archiviazione e un secondo ambiente. |
| Dominio, posta transazionale | circa 40 euro | Stima |
| Commercialista e adempimenti (se si apre la partita IVA) | circa 800 euro | Stima, non verificata |
| Parere legale (una tantum) | circa 500 euro | Documento 02: "qualche centinaio di euro" |
| Account sviluppatore Apple (solo con app nativa) | 99 dollari l'anno | Non serve al lancio (web) |

### 1.6 Commissioni di pagamento, store e IVA: netto per abbonato

Listini: Stripe Italia applica 1,5% + 0,25 euro sulle carte standard SEE, 2,8% + 0,25 euro sulle premium e lo 0,7% di Billing sugli abbonamenti [V][P3]. Paddle, che fa da rivenditore e gestisce l'IVA, applica 5% + 0,50 [V][P4]. App Store (Small Business Program) e Google Play trattengono il 15% sugli abbonamenti [V2][P6][P7]. IVA ordinaria al 22%.

| Prezzo lordo | Stripe, IVA 22% | Paddle (gestisce l'IVA) | App Store o Play, 15% | Stripe in regime forfettario (senza IVA, **da verificare**) |
|---|---|---|---|---|
| **29,99 euro all'anno** | **23,67 (79%)** | 22,58 (75%) | 20,89 (70%) | 29,08 (97%) |
| 19,99 euro all'anno (fondatori) | 15,70 (79%) | 14,88 (74%) | 13,93 (70%) | 19,10 (96%) |
| **3,49 euro al mese** | **2,53 (73%)** | 2,19 (63%) | 2,43 (70%) | 3,16 (91%) |
| 4,99 euro una tantum (raccolta stampabile) | 3,77 (76%) | 3,32 (67%) | 3,48 (70%) | 4,67 (94%) |

Letture:
- La quota fissa di 0,25 euro di Stripe penalizza il mensile: **l'annuale va spinto**.
- **Canale consigliato al lancio: web con Stripe**, perché il pubblico è quasi tutto italiano e l'IVA nazionale è semplice da gestire. Paddle costa circa 4 punti in più sull'annuale: conviene solo se il proprietario non vuole gestire l'IVA. Le app native toglierebbero altri 9 punti: non al lancio, come già nel documento 02.
- Il regime forfettario (soglia di 85.000 euro, confermata per il 2026 [V2][P8]) alzerebbe il netto di circa 18 punti, ma non è chiaro come si applichi alle vendite di servizi elettronici a consumatori di altri paesi UE **[N]**. È una decisione da prendere con il commercialista. Nei calcoli uso l'IVA al 22%, che è il caso prudente.

### 1.7 Costi fissi annui per linea [S]

| | Linea giocatori | Linea editori |
|---|---|---|
| Costi vivi in denaro | circa 1.200 euro (server, posta, commercialista, AI per i contenuti) | circa 1.000 euro (server, commercialista) |
| Tempo fisso | 333 ore: 233 di contenuti e 100 di manutenzione, assistenza e comunità | 180 ore: 80 di vendita e colloqui, 50 di vetrina con 50-100 giochi originali, 50 di manutenzione |
| Tempo valorizzato a 35 euro l'ora | 11.650 euro | 6.300 euro |
| **Costo fisso pieno** | **12.850 euro all'anno** | **7.300 euro all'anno** |
| Quota delle 460 ore disponibili | 72% | 39% |

Fare entrambe le linee a pieno regime richiede circa 700 ore (333 più 374 del caso base editori): **non entrano nel tempo di uno sviluppatore part-time**. È un vincolo di capacità, non di mercato.

---

## 2. Linea giocatori

### 2.1 Prezzi di riferimento

| Offerta | Prezzo | Equivalente annuo in euro | Tipo e fonte |
|---|---|---|---|
| Corriere Giochi (RCS), app | 2,49 euro al mese; 24,99 euro all'anno | 24,99 | [V][P9], scheda App Store Italia, 10/11/2026 |
| Corriere Giochi, offerta di lancio | 5 euro il primo anno (luglio 2025) | 5 | [V2], documento 02 [3] |
| La Settimana Enigmistica, replica digitale (app LaSE) | 1,90 euro a numero; 78 euro per 52 numeri | 78 | [V2], Trend Online 27/06/2022 (documento 02 [1]); su App Store 99,99 dollari per 52 numeri |
| La Settimana Enigmistica, carta | 2 euro a numero | circa 104 | [V2], documento 01 [F3] |
| NYT Games | circa 50 dollari l'anno | circa 45 | [V2], Nieman Lab, settembre 2025 (documento 01 [F19]). Prezzo attuale **[N]**: la pagina di aiuto del NYT rimanda alla pagina di abbonamento [P11] |
| Guardian Puzzles | 3,49 sterline al mese; 32,99 sterline all'anno | n.d. | [V2], prezzo di lancio 2020 (documento 02 [24]) |
| Figaro Jeux | 4,99 euro al mese | circa 60 (se mensile per 12 mesi) | [V2], scheda senza data (documento 02 [25]) |
| Readly (edicola PDF) | 9,99 euro al mese | circa 120 | [V2], fonte del 2021 (documento 02 [15]) |
| App di cruciverba generiche | gratuite con pubblicità; Pro a 0,99 euro una tantum | circa 1 | [V2], documento 02 [18][19] |

Lettura:
- L'ancora digitale italiana è Corriere a **24,99 euro**, sostenuta da un marchio di informazione e già scontata al lancio.
- L'ancora di chi gioca su carta va da **78 a 104 euro l'anno**.
- DBEnigmistica non ha marchio, ma offre tipologie che Corriere non ha (crittografie, rebus, cruciverba senza schema, "varietà").

### 2.2 Livelli e prezzi consigliati (IVA inclusa)

| Livello | Prezzo | Contenuto |
|---|---|---|
| **Gratuito** | 0 | Gioco del giorno (rebus o crittografia, condivisibile); ultimi 7 giorni giocabili; 1 cruciverba a settimana; 3 aiuti al giorno; **accessibilità completa** (caratteri grandi, contrasto, tablet), perché non si mette dietro pagamento l'accessibilità per un pubblico anziano; senza registrazione obbligatoria |
| **Solutore, mensile** | **3,49 euro al mese** | Archivio completo, tutte le tipologie, 3 giochi premium e 1 rebus a settimana, aiuti e verifica della casella illimitati, stampa in PDF, statistiche complete |
| **Solutore, annuale** | **29,99 euro all'anno** (equivale a 8,6 mesi: il 28% in meno del mensile) | Come il mensile |
| **Fondatori** (prezzo di lancio) | **19,99 euro per il primo anno**, rinnovo a 29,99 dichiarato al momento dell'acquisto | Offerta valida solo per chi si iscrive nei primi 90 giorni dal lancio del piano; scadenza fissa |
| **Regalo** | 29,99 euro, 12 mesi, senza rinnovo automatico | Per figli e nipoti che regalano al solutore di carta; da attivare a dicembre |
| **Prova gratuita** | 14 giorni, senza carta | Il valore (archivio e tipologie) si scopre giocando; il pubblico anziano diffida della carta chiesta subito |
| Raccolta stampabile (fase 2, mese 9 e oltre) | 4,99 euro una tantum (50 giochi in PDF) | Per chi non vuole abbonarsi |
| Licenza strutture (fase 2): RSA, centri anziani, scuole | 79 euro all'anno IVA esclusa | Stampa illimitata per uso interno |

Esclusi al lancio:
- **pubblicità**: con 5.000 utenti attivi al mese e un RPM di 1-3 euro (documento 01) varrebbe 75-225 euro al mese, e peggiorerebbe l'esperienza degli over 65; da rivalutare oltre 300.000 pagine viste al mese;
- piano famiglia;
- sconti permanenti;
- coupon.

### 2.3 Perché 29,99 euro e non 24,99

1. È il **20% sopra Corriere** su un'offerta che Corriere non copre (tipologie italiane classiche), e meno del 40% della replica PDF della Settimana Enigmistica (78 euro).
2. Con elasticità vicina a -1 (sezione 4.1) i ricavi sono quasi uguali tra 24,99 e 34,99 euro. A parità di ricavi, il prezzo più alto dà **meno abbonati da assistere** e una quota fissa delle commissioni più bassa. Il cliente atteso, che oggi spende circa 100 euro l'anno di carta, è probabilmente meno sensibile al prezzo di quanto dica l'elasticità -1: il rischio sta più nell'abbassare il prezzo che nell'alzarlo.
3. Il prezzo fondatori a 19,99 euro sta **sotto** Corriere solo per il primo anno, con scadenza dichiarata. È uno strumento di lancio, non un riposizionamento.

### 2.4 Punto di pareggio della linea giocatori [S]

Contributo per abbonato all'anno = netto Stripe meno 1,50 euro di costi variabili (server, posta, circa 2 minuti di assistenza).

| Prezzo | Contributo per abbonato | Pareggio sui costi vivi (1.200 euro) | **Pareggio a costo pieno (12.850 euro)** | Utenti attivi al mese necessari (conversione al 2%) |
|---|---|---|---|---|
| 29,99 euro (listino) | 22,17 | **54 abbonati** | **580 abbonati** | **circa 29.000** |
| 19,99 euro (fondatori) | 14,20 | 85 abbonati | 905 abbonati | circa 45.000 |

Confronto con gli scenari del documento 01 a 24 mesi:
- Prudente (50 abbonati): non copre nemmeno i costi vivi.
- Base (1.000 abbonati): profitto di **circa 9.300 euro** dopo il tempo valorizzato.
- Il pareggio a costo pieno richiede circa **60% dello scenario base**, e quindi verosimilmente **18-24 mesi** di crescita organica.

Nel primo anno (stima: 10.000 utenti attivi al mese al mese 12 e 200 fondatori) il contributo è di circa 2.840 euro: copre i costi vivi, ma vale **meno di 5 euro per ciascuna delle circa 340 ore** dedicate.

---

## 3. Linea editori

### 3.1 Riferimenti: fornitori di giochi in white label

| Fornitore | Prezzo pubblico | Tipo e fonte |
|---|---|---|
| Amuse Labs PuzzleMe | Personal gratis (1.000 partite al mese); **Plus 99 dollari al mese** (2.000 partite); **Professional 249 dollari** (5.000 partite); **Enterprise da 500 dollari** (illimitate). Il pagamento annuale fa risparmiare fino al 10%. Nessun listino separato per gli editori; sconti per piccoli editori su richiesta | [V][P12], 10/11/2026 |
| Playgent | Individual 19 dollari al mese, Team 99 dollari al mese, Enterprise su preventivo; partite illimitate; AI a consumo | [V][P13], 10/11/2026. Un'altra pagina dello stesso sito riporta 249, 1.499 e 4.999 dollari [V2]: i listini sono incoerenti |
| INN News Crossword | da 49 a 999 dollari al mese secondo il numero di iscritti; 250 dollari di attivazione | [V2][P14], la pagina ha restituito errore 403 e il dato viene da una sintesi di ricerca |
| ParkerSyndication | giochi gratuiti da incorporare, con il 50% della pubblicità all'editore | [V2][P14] |
| Keesing (piattaforma white label) | **[N]**, non pubblicato | Documento 02 |
| **Digitalizzazione di archivi in giochi giocabili** | **[N]**: nessun fornitore con prezzi pubblici trovato | Il prezzo per pagina che segue è una stima mia, basata sul costo e sul valore |

Lettura:
- Il **canone del lettore** ha un'ancora chiara, da 90 a 450 euro al mese per la fascia piccola e media.
- La **digitalizzazione** non ha concorrenti con listino: lì il prezzo si fissa sul costo (sezione 1.3) e sul valore (sezione 3.4).

### 3.2 Modello consigliato: avvio una tantum, canone del lettore e prezzo per pagina (IVA esclusa)

Si fa pagare a pagina, non a gioco: il costo cresce con le pagine, l'editore conta le pagine prima di firmare, e "che cosa è un gioco" sarebbe fonte di contestazioni. Il canone si misura in partite al mese, che è la metrica di valore usata da Amuse Labs.

| | **Piccolo** | **Medio** (segmento iniziale) | **Grande** |
|---|---|---|---|
| Profilo | 1 testata, fino a 20.000 partite al mese, 2 utenti dello Studio | fino a 3 testate, fino a 100.000 partite al mese, 5 utenti | testate illimitate, oltre 100.000 partite al mese, 15 utenti, livello di servizio garantito |
| **Avvio** (una tantum) | **900 euro** | **3.500 euro** | **da 7.500 euro** |
| **Canone lettore più Studio** | **99 euro al mese** | **299 euro al mese** | **da 790 euro al mese** |
| Superamento della soglia di partite | passaggio alla fascia superiore al rinnovo, nessun sovrapprezzo a sorpresa | come Piccolo | su contratto |

| Digitalizzazione, per pagina | Fino a 4.999 pagine all'anno | 5.000-19.999 | 20.000 e oltre |
|---|---|---|---|
| **Correzione a cura della redazione nello Studio** (modalità predefinita) | **1,90 euro** | 1,50 euro | 1,20 euro |
| **Servizio completo** (correggiamo noi; accuratezza garantita al 99% delle caselle) | **7,90 euro**, senza sconti di volume, con un tetto di 1.500 pagine a trimestre | | |

| Formula commerciale | Prezzo e condizioni |
|---|---|
| Demo su un numero del proprio archivio | Gratuita. Costo per noi: circa 3 ore e 3 euro di AI; è il costo di acquisizione |
| **Pilota di 90 giorni** (solo fasce Media e Grande) | **2.900 euro**: avvio ridotto, fino a 300 pagine in servizio completo, lettore per 3 mesi, rapporto di accuratezza. **Scalato dall'avvio** se l'editore firma l'annuale entro 30 giorni. Margine circa 28% |
| Canone annuale pagato in anticipo | -15% (Piccolo 1.010 euro, Medio 3.050 euro all'anno) |
| Impegno biennale | -20% sul canone e prezzi per pagina bloccati per 24 mesi |
| Archivi culturali senza scopo di lucro (es. Biblioteca Enigmistica Italiana) | -30% per 12 mesi, motivato dal valore di caso di riferimento pubblico; scade e non si rinnova automaticamente |
| Altri sconti | Nessuno. Le alternative preferite sono pagamento dilazionato in 3 rate oppure 100 pagine di servizio completo in omaggio |
| **Quota sui ricavi** | **Non al lancio** (vedi sotto) |

Perché niente quota sui ricavi al lancio: lo sviluppatore non controlla il marketing dell'editore, e una quota su un prodotto che non vende è una quota di zero.

Unica eccezione, in **fase 2**: "archivio ospitato" sul sito DBEnigmistica, per i piccoli editori senza piattaforma propria. Lì il canale di vendita è nostro: all'editore va il 40% dei ricavi netti del piano Solutore, ripartito in proporzione alle partite sui suoi giochi [S]. Si attiva oltre 300 abbonati paganti. È il ponte tra le due linee: porta nel piano per i giocatori **contenuti in licenza** che oggi non si possono pubblicare.

### 3.3 Economia per cliente tipo, primo anno [S]

| | Piccolo, correzione della redazione | Medio, correzione della redazione |
|---|---|---|
| Avvio | 900 | 3.500 |
| Pagine | 1.000 × 1,90 = 1.900 | 4.000 × 1,90 = 7.600 |
| Canone annuale anticipato | 1.010 | 3.050 |
| **Ricavo, anno 1** | **3.810** | **14.150** |
| Costo dell'avvio (12 ore / 40 ore) | 420 | 1.400 |
| Costo delle pagine (0,65 euro a pagina) | 650 | 2.600 |
| Assistenza (1 ora / 2 ore al mese) più server | 540 | 1.080 |
| **Contributo, anno 1** | **2.200 (58%)** | **9.070 (64%)** |
| Ore dedicate | 41 | 131 |
| Ricavo, anno 2 (500 / 1.500 nuove pagine più canone) | 1.960 | 5.900 |
| **Contributo, anno 2** | **1.095** | **3.845** |

Il Medio in **servizio completo** sulle stesse 4.000 pagine (a 7,90 euro):
- ricavo di circa 38.150 euro, costo di circa 21.400 euro, contributo del 44%;
- richiede però **circa 600 ore**, più del tempo disponibile in un anno.

Per questo il servizio completo resta un'opzione a tetto, non la modalità predefinita.

### 3.4 Verifica sul valore per l'editore [S, confidenza bassa]

Ipotesi: un settimanale Medio con circa 120.000 copie (tiratura dichiarata di Enigmistica Più nel 2018, documento 02 [9]) converte lo 0,5-1% degli acquirenti a un archivio giocabile da 24,99 euro.
- Valore per l'editore: 600-1.200 abbonati, circa **11.800-23.700 euro netti all'anno**.
- Nostro prezzo su 5 anni: 14.150 + 4 × 5.900 = 37.750 euro, contro un valore di 59.000-118.500 euro. Quota di valore catturata **32-64%**.

È nella fascia del "mercato nuovo senza alternative" (30-50%), ma sopra se la conversione è bassa. **Il prezzo del primo anno del Medio è al tetto**: non va alzato, e va difeso con il caso del pilota.

Per il Piccolo (circa 20.000 copie, 100-200 abbonati, 2.000-4.000 euro all'anno) la quota catturata su 2 anni è del 72-144%: **la fascia Piccolo regge solo per chi ha già un canale digitale**. Agli altri si propone l'archivio ospitato in fase 2.

### 3.5 Punto di pareggio della linea editori [S]

| Misura | Valore |
|---|---|
| Sui costi vivi (circa 1.000 euro all'anno) | **1 pilota** a 2.900 euro |
| A costo pieno, anno 1 (7.300 euro) | **1 cliente Medio** (0,8 Medi) oppure 4 Piccoli |
| A costo pieno, a regime (anno 2 e successivi, solo canone e nuove pagine) | **2 clienti Medi** (7.300 / 3.845 = 1,9) |
| Caso base, anno 1: 1 Medio più 1 Piccolo | ricavo 17.960 euro, contributo 11.270, **profitto 3.970 (22%)**, 352 ore, circa **50 euro l'ora** in denaro |

Coerenza con il documento 02: il cancello G4 (almeno 2 clienti e almeno 15.000 euro contrattualizzati al mese 12) corrisponde quasi esattamente al caso base qui sopra.

---

## 4. Analisi di sensibilità

### 4.1 Giocatori: prezzo da -30% a +30%, a 24 mesi con 50.000 utenti attivi al mese

Ipotesi: elasticità della conversione al prezzo pari a -1 [S] (una variazione di prezzo del 10% fa variare la conversione del 10% in senso opposto); conversione di riferimento al 2%; costo fisso pieno di 12.850 euro.

| Prezzo annuo | Conversione | Abbonati | Ricavo netto | Contributo | Profitto | Margine sul ricavo netto | Abbonati per il pareggio | Utenti attivi al mese per il pareggio |
|---|---|---|---|---|---|---|---|---|
| 20,99 (-30%) | 2,6% | 1.300 | 21.437 | 19.487 | 6.637 | 31% | 857 | 32.960 |
| 23,99 (-20%) | 2,4% | 1.200 | 22.668 | 20.868 | 8.018 | 35% | 739 | 30.790 |
| 26,99 (-10%) | 2,2% | 1.100 | 23.408 | 21.758 | 8.908 | 38% | 650 | 29.550 |
| **29,99** | **2,0%** | **1.000** | **23.670** | **22.170** | **9.320** | **39%** | **580** | **29.000** |
| 32,99 (+10%) | 1,8% | 900 | 23.463 | 22.113 | 9.263 | 39% | 523 | 29.060 |
| 35,99 (+20%) | 1,6% | 800 | 22.768 | 21.568 | 8.718 | 38% | 477 | 29.810 |
| 38,99 (+30%) | 1,4% | 700 | 21.595 | 20.545 | 7.695 | 36% | 438 | 31.290 |
| 20,99 (-30%), conversione invariata | 2,0% | 1.000 | 16.490 | 14.990 | 2.140 | 13% | 857 | 42.850 |
| 38,99 (+30%), conversione invariata | 2,0% | 1.000 | 30.850 | 29.350 | 16.500 | 53% | 438 | 21.900 |
| **29,99, conversione dimezzata** | **1,0%** | **500** | **11.835** | **11.085** | **-1.765** | **-15%** | **580** | **58.000** |

### 4.2 Editori: prezzo da -30% a +30% (caso base: 1 Medio più 1 Piccolo, anno 1)

Ipotesi: numero di clienti invariato nell'intervallo [S], perché la decisione B2B è binaria e legata al budget; costi variabili invariati (6.690 euro); costo fisso pieno di 7.300 euro.

| Variazione di listino | Ricavo | Contributo | Profitto | Margine | Pareggio, in clienti Medi (anno 1) |
|---|---|---|---|---|---|
| -30% | 12.572 | 5.882 | **-1.418** | -11% | 1,5 |
| -20% | 14.368 | 7.678 | 378 | 3% | 1,2 |
| -10% | 16.164 | 9.474 | 2.174 | 13% | 0,95 |
| **Listino** | **17.960** | **11.270** | **3.970** | **22%** | **0,8** |
| +10% | 19.756 | 13.066 | 5.766 | 29% | 0,7 |
| +20% | 21.552 | 14.862 | 7.562 | 35% | 0,6 |
| +30% | 23.348 | 16.658 | 9.358 | 40% | 0,55 |
| **Conversione dimezzata: firma solo il Medio** | 14.150 | 9.070 | 1.770 | 13% | 0,8 |
| **Conversione dimezzata: firma solo il Piccolo** | 3.810 | 2.200 | **-5.100** | -134% | n.a. |

### 4.3 Variabile nascosta: minuti di correzione nel servizio completo (prezzo 7,90 euro a pagina)

| Minuti per pagina | 4 | **8** | 12 | 15 |
|---|---|---|---|---|
| Costo per pagina | 2,40 | **4,74** | 7,07 | 8,82 |
| Margine | 70% | **40%** | 10% | **-12%** |

Nella correzione a cura della redazione, a 1,90 euro: il margine è del 66% con 1 minuto di controllo a campione e del 35% con 2 minuti.

### 4.4 Che cosa dice la sensibilità

- **Giocatori**: tra -30% e +30% il profitto resta tra 6.600 e 9.300 euro. Il prezzo è una leva debole, **la conversione è la leva forte**: dimezzata, porta in perdita e raddoppia gli utenti necessari (58.000). Se l'elasticità reale è più bassa di -1 (solutori abituali), il +30% vale circa 7.000 euro l'anno in più: va misurato (esperimento 1).
- **Editori**: la linea è più sensibile al prezzo perché i costi sono ore. Il -30% porta in perdita il primo anno. **Non si scende sotto il listino** se non con le formule previste in 3.2.
- Il rischio vero degli editori **non è il prezzo**, ma il numero di contratti (con il solo Piccolo si perdono 5.100 euro) e i minuti per pagina (oltre 12 minuti il servizio completo non rende).

---

## 5. Raccomandazione

### 5.1 Prezzi da adottare al lancio

**Linea giocatori** (IVA inclusa, web con Stripe):

| Voce | Prezzo |
|---|---|
| Gratuito | 0 euro: gioco del giorno, 7 giorni di archivio, 1 cruciverba a settimana, accessibilità completa |
| Solutore mensile | **3,49 euro** |
| Solutore annuale | **29,99 euro** |
| Fondatori (90 giorni dal lancio del piano) | **19,99 euro il primo anno**, poi 29,99 |
| Regalo 12 mesi | 29,99 euro |
| Prova | 14 giorni senza carta |

**Linea editori** (IVA esclusa):

| Voce | Piccolo | Medio | Grande |
|---|---|---|---|
| Avvio | 900 | 3.500 | da 7.500 |
| Canone lettore più Studio, al mese | 99 | 299 | da 790 |
| Pagina, correzione della redazione | 1,90 / 1,50 / 1,20 secondo il volume | | |
| Pagina, servizio completo | 7,90 (con tetto) | | |
| Pilota di 90 giorni | non previsto | 2.900, scalato dall'avvio | 2.900, scalato dall'avvio |
| Annuale anticipato / biennale | -15% / -20% sul canone | | |

### 5.2 Quale linea ha l'economia migliore

**La linea editori**, per quattro motivi:

1. **Pareggio con 1-2 contratti** invece di circa 580 abbonati, cioè circa 29.000 utenti attivi al mese, che il documento 01 colloca nello scenario base a 18-24 mesi.
2. **Rendimento orario immediato**: circa 50 euro l'ora nel caso base del primo anno, contro meno di 5 euro l'ora per i giocatori nel primo anno (sezione 2.4). I giocatori arrivano a circa 57 euro l'ora solo a 24 mesi, e solo nello scenario base.
3. **Costo fisso in tempo più basso** (180 ore contro 333): niente produzione quotidiana di giochi. Resta spazio per lo sviluppo del prodotto, che serve a entrambe.
4. **Contenuti legali per costruzione**: l'editore porta i diritti, mentre i giocatori richiedono di produrre ogni gioco da zero (documento 02, sezione 2).

Che cosa si perde:
- il **tetto di crescita** dei giocatori (scenario ottimista del documento 01, oltre 200.000 euro l'anno);
- la **prevedibilità**: i ricavi B2B sono a scatti, con cicli di vendita lunghi e pochi acquirenti.

Come si riconcilia con la richiesta del proprietario di un sito pubblico aperto a tutti:
- **dal lancio** il sito è pubblico, gratuito, con il gioco del giorno: fa da vetrina della linea editori e da canale di acquisizione per i giocatori;
- il **piano Solutore si attiva** ai prezzi sopra quando si superano 5.000 utenti attivi al mese e 200 giochi in archivio. Sotto questa soglia l'1-2% di conversione vale meno di 100 abbonati: non ripaga gestione dei pagamenti, IVA e assistenza;
- in fase 2, l'**archivio ospitato** (sezione 3.2) porta nel piano Solutore contenuti degli editori in licenza.

### 5.3 Tre esperimenti di prezzo nei primi sei mesi

| # | Esperimento | Quando | Come | Regola di decisione |
|---|---|---|---|---|
| 1 | **Disponibilità a pagare dei giocatori** (prima di attivare i pagamenti) | Mesi 3-6, sulla vetrina | Questionario Van Westendorp agli utenti registrati (almeno 150 risposte). In parallelo una "porta finta": il pulsante "Diventa Solutore" mostra a caso 24,99 o 34,99 euro e misura i clic, poi lista d'attesa | Se i clic a 34,99 sono almeno il 75% di quelli a 24,99, si lancia a 34,99 (elasticità sotto 1). Se il prezzo "troppo caro" mediano del Van Westendorp è sotto 25 euro, si lancia a 24,99. Altrimenti si resta a 29,99 |
| 2 | **Menu di servizio per gli editori** | Mesi 4-6, in ogni proposta dopo la demo | Ogni proposta presenta entrambe le modalità: correzione della redazione a 1,90 euro e servizio completo a 7,90 euro. Si registrano la scelta e le obiezioni | Se almeno 2 editori su 3 scelgono il servizio completo, la redazione non ha personale: si alza il servizio completo a 8,90 euro e si investe nello Studio per scendere sotto 5 minuti a pagina. Se scelgono la correzione propria, si conferma il modello |
| 3 | **Prezzo e struttura del pilota** | Mesi 4-6, prove in sequenza | Prime 3 proposte a 2.900 euro (300 pagine, scalato dall'avvio). Se nessuna è accettata, le 3 successive a 1.900 euro per 150 pagine, alle stesse condizioni | Si adotta la versione con il tasso di accettazione più alto. Se nessun pilota viene firmato entro il mese 7, vale il cancello G3 del documento 02, non un ulteriore sconto |

Da fare dopo il mese 6, quando il piano Solutore è attivo: prova di 14 giorni senza carta contro 7 giorni con carta, e offerta regalo a dicembre.

### 5.4 Governo dei prezzi

- **Sconti**: ogni sconto ha una motivazione scritta nel contratto e una scadenza. Sono ammessi solo quelli della sezione 3.2 e il prezzo fondatori. Il proprietario è l'unico che approva: il limite pratico è "nessuno sconto non previsto dal listino".
- **Revisione**: trimestrale. Indicatori da guardare:
  - conversione da gratuito a pagante;
  - rapporto tra ricavo effettivo e listino, con obiettivo oltre l'85%;
  - minuti di correzione per pagina;
  - ore per cliente;
  - disdette dopo variazioni di prezzo, con obiettivo sotto il 5%.

---

## 6. Ipotesi più fragili

| # | Ipotesi | Valore usato | Che cosa succede se è sbagliata | Come verificarla |
|---|---|---|---|---|
| 1 | Minuti di correzione per pagina | 8 (servizio completo), 1 (controllo a campione) | Oltre 12 minuti il servizio completo non rende; oltre 2 minuti il margine sulla correzione della redazione si dimezza | Cancello G1: misurare i minuti, non solo l'accuratezza |
| 2 | Gli editori hanno personale per correggere nello Studio | Modalità predefinita | Senza personale, il Medio passa a circa 600 ore l'anno e satura il tempo disponibile | Esperimento 2 |
| 3 | Disponibilità a pagare degli editori per la digitalizzazione | 1,90 euro a pagina, 299 euro al mese | Nessun listino pubblico di confronto; quota di valore già al 32-64% | Colloqui G2 ed esperimento 3 |
| 4 | Conversione dei giocatori e utenti attivi al mese | 2% su 50.000 a 24 mesi (documento 01, scenario base) | Con conversione dimezzata servono 58.000 utenti attivi al mese per pareggiare | Esperimento 1 e dati della vetrina |
| 5 | Elasticità -1 | Simmetrica | Se è più bassa conviene il prezzo più alto (fino a +7.000 euro l'anno) | Esperimento 1 |
| 6 | Valore dell'ora (35 euro) e ore disponibili (460) | | Cambiano il pareggio a costo pieno, non quello in cassa | Risposta del proprietario (documento 02, sezione 9) |
| 7 | Regime IVA | IVA al 22% | Con il forfettario il netto sale di circa 18 punti | Commercialista |

## 7. Note per il Product Manager

- Il cancello G1 del documento 02 va riformulato in termini misurabili: **costo AI fino a 0,25 euro per pagina e correzione fino a 2 minuti per pagina** nella modalità a cura della redazione.
- Requisiti di prodotto che derivano dai prezzi:
  - contatore di partite al mese per lettore incorporato (base del canone);
  - contatore di pagine elaborate per cliente;
  - registro dei minuti di correzione nello Studio;
  - campo provenienza e licenza (già richiesto dal documento 02);
  - pagamento web con abbonamento annuale come opzione predefinita.
- I costi di produzione dei giochi originali (233 ore l'anno) valgono **solo se si attiva il piano Solutore**. Per la vetrina bastano 50-100 giochi una tantum e un gioco del giorno breve.

---

## Fonti

Tutte consultate il 10/11/2026, salvo diversa indicazione.

- [P1] Anthropic, "Pricing", documentazione della piattaforma Claude: listino di Sonnet 5.5 (2 e 10 dollari per milione di token), Batch API con sconto del 50% (1 e 5 dollari), Opus 5.5 in batch (2 e 10 dollari), moltiplicatori del caching, nota sul tokenizer. https://platform.claude.com/docs/en/about-claude/pricing (reindirizzata da https://docs.anthropic.com/en/docs/about-claude/pricing)
- [P2] Anthropic, "Vision", sezione "Resolution and token cost": formula ⌈w/28⌉×⌈h/28⌉, tetto di 2.576 pixel e 4.784 token per i modelli 4.7 e successivi. https://platform.claude.com/docs/en/build-with-claude/vision
- [P3] Stripe, "Tariffe" (Italia): 1,5% + 0,25 euro carte SEE standard, 2,8% + 0,25 euro premium, 0,7% Billing. https://stripe.com/it/pricing
- [P4] Paddle, "Pricing": 5% + 50 centesimi per transazione, rivenditore che gestisce IVA e imposte. https://www.paddle.com/pricing
- [P5] Hetzner, "Cloud": server condivisi da 5,49 euro al mese IVA esclusa. https://www.hetzner.com/cloud
- [P6] Apple App Store Small Business Program, commissione del 15% (fonti secondarie): Appbot, https://appbot.co/blog/app-developers-apple-google-small-business-programs/ ; Adapty, https://adapty.io/blog/app-store-small-business-program/
- [P7] Google Play, commissioni sugli abbonamenti 2026, 10% più 5% di billing (fonti secondarie): Adapty, https://adapty.io/blog/markdown/google-play-billing-changes-subscriptions-fees.md ; pagina di supporto Google, https://support.google.com/googleplay/android-developer/answer/16954621
- [P8] Regime forfettario 2026, soglia di 85.000 euro (fonti secondarie): QuickFisco, https://quickfisco.it/?p=35586 ; PMI.it, https://www.pmi.it/economia/mercati/479129/flat-tax-regime-forfettario-soglie.html
- [P9] Apple App Store Italia, "Corriere Giochi" (RCS MediaGroup): 2,49 euro mensile, 24,99 euro annuale, valutazione 4,6 su circa 3.500 voti. https://apps.apple.com/it/app/corriere-giochi/id6744399739
- [P10] Banca d'Italia, cambi di riferimento del 2/11/2026 (dollaro USA 1,1216; dato tramite sintesi di ricerca). https://www.bancaditalia.it/compiti/operazioni-cambi/cambio/cambi_rif_20261102
- [P11] NYT Help Center, "New York Times Games Subscription" (nessun prezzo pubblicato, rimanda a nytimes.com/subscription/games). https://thenewyorktimeshelpcenter.helpjuice.com/360052272251-New-York-Times-Games-Subscription
- [P12] Amuse Labs, "Plans and Pricing". https://amuselabs.com/pricing
- [P13] Playgent, "Pricing". https://playgent.com/pricing ; pagina editori con listino diverso: https://www.playgent.com/publishers
- [P14] INN, "News Games" (errore 403, dati tramite sintesi di ricerca) e ParkerSyndication (sintesi di ricerca). https://inn.org/resource/news-games/ ; https://lifestyle.kbew98country.com/story/327095/guinness-world-records-puzzle-master-opens-parkersyndication-18-free-daily-games-for-publishers/
- Fonti riprese dai documenti 01 e 02, con le loro date: Corriere 5 euro il primo anno (Il Post, 20/07/2025); Settimana Enigmistica 1,90 euro e 78 euro (Trend Online, 27/06/2022) e App Store LaSE; NYT Games circa 50 dollari (Nieman Lab, settembre 2025); Guardian 3,49 e 32,99 sterline (Digiday e TapSmart); Figaro Jeux 4,99 euro (App Store, senza data); Readly 9,99 euro (Sky TG24, 10/03/2021); tiratura di Enigmistica Più (Cairo Book 2018). Gli URL sono negli elenchi delle fonti di quei documenti.

**Cercati e non trovati [N]**:
- prezzo attuale di NYT Games;
- listino attuale in euro della Settimana Enigmistica digitale;
- listini B2B di Keesing;
- prezzi pubblici di servizi di digitalizzazione di archivi di enigmistica in giochi giocabili;
- listino di un modello di generazione di immagini per i rebus (usato l'intervallo 0,02-0,20 euro);
- trattamento IVA del regime forfettario sui servizi elettronici venduti a consumatori di altri paesi UE.
