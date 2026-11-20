# Decisioni per la fase 1b

Decisioni del proprietario del 20 novembre 2026, sulle nove domande aperte di `.planning/phases/1b/CONTEXT.md`, sezione 9. Sono prese dopo la chiusura dell'ondata 1.1, con la fase 1 ancora in corso. La pianificazione operativa sta in `.planning/phases/1b/CONTEXT.md` e `.planning/phases/1b/PLAN.md`. Il ROADMAP e i documenti di handoff si allineano a queste decisioni.

## Perché

La 1.1 si è chiusa il 20/11/2026, più di due settimane prima del previsto. Il prototipo della 1b misura il tempo di revisione per la soglia G1 del 28 febbraio 2027. Non è un prodotto: non pubblica nulla e gira solo sul PC del proprietario. Le scelte che toccano l'avvio, lo schema e il registro vanno prese prima della 1b.1, che parte il 23/11/2026.

## Decisioni sul prototipo

Tutte e nove le decisioni accolgono la raccomandazione del CONTEXT, sezione 9.

### D1b-1. Come si avvia il prototipo

**Domanda.** Il ROADMAP prevedeva `docker run ... --prototipo`. Il criterio 4 vuole che il server ascolti solo su `127.0.0.1`. Come si avvia il prototipo?

**Decisione.** A. Il prototipo gira nativo su Windows. Il comando `dbe prototipo`, nell'eseguibile `dbe` win-x64 della CI, avvia l'host web su `127.0.0.1`. PostgreSQL resta nel Compose. Claude Code si chiama come nell'estrazione. Il comando arriva nell'ondata 1b.1.

**Perché.** È coerente con D5 e con l'ADR 010. Lascia il criterio 4 com'è e non cambia l'immagine pubblicata.

**Opzioni scartate.**
- *B. Container per la revisione, generazione su Windows.* Il criterio 4 andrebbe riscritto.
- *C. Container con Claude Code dentro.* Va contro D5 e l'ADR 010. Richiede un token nel container prima della verifica delle condizioni d'uso.

**Effetti.** ROADMAP, fase 1b: riga "Avvio locale" e artefatti della pre-release. File 07 dell'handoff.

### D1b-2. Dove stanno i candidati accettati e cosa va nel registro

**Domanda.** Il ROADMAP dice che i candidati accettati restano bozze con origine `originale` e registro parziale. Non dice dove stanno né quali eventi vanno nel registro.

**Decisione.** A. I candidati stanno solo nello schema `prototipo`, con Origine fissata a `originale`. Nel registro in sola aggiunta vanno solo eventi di metadati: la chiamata di creazione, con lo SHA-256 della risposta, e l'esito della revisione. La fase 6 decide se e come importarli nel `catalogo`.

**Perché.** Il `catalogo` resta pulito finché la fase 6 non completa il registro. Il registro lascia comunque una traccia immutabile di generazioni ed esiti. La B anticiperebbe scelte della fase 6. La C perderebbe la traccia del contributo umano fin dall'inizio.

**Opzioni scartate.**
- *B. Copia dei candidati accettati in `catalogo.giochi`.* Anticipa scelte della fase 6.
- *C. Solo schema `prototipo`, nessun evento nel registro.* Perde la traccia.

**Effetti.** ROADMAP, fase 1b: "Risultati consegnati" (schema `prototipo` e registro).

### D1b-3. Quali candidati entrano nella prova 1

**Domanda.** La quota dei candidati accettati dipende da quali candidati arrivano alla revisione. Un candidato che non passa lo schema o i controlli leggeri entra nella prova?

**Decisione.** A. Entrano tutti i candidati validi per lo schema, in ordine di generazione, senza scelta a mano. I candidati non validi per lo schema si rigenerano e si contano a parte nel rapporto.

**Perché.** La prova misura il motore così com'è, senza selezione. È la lettura più prudente di G1. I candidati non validi per lo schema non arrivano comunque al revisore.

**Opzioni scartate.**
- *B. Scarto automatico dei candidati che non passano i controlli leggeri, contati a parte.* Lo scarto sposta la misura dal revisore alla regola di scarto.
- *C. Il proprietario sceglie 5 candidati tra quelli generati.* Rende la quota accettata non confrontabile con le fasi 6 e 7.

**Effetti.** Stati del candidato (ondata 1b.1). Calcolo di `dbe eval review-time` (ondata 1b.2).

### D1b-4. Cruciverba: schema di caselle nere dal programma da subito

**Domanda.** Il ROADMAP prevedeva lo schema di caselle nere solo come contromisura, se i primi 5 candidati avessero più di 10 incroci incoerenti. Con 5 candidati non c'è spazio per ripartire.

**Decisione.** A. Il programma genera uno schema di caselle nere valido, simmetrico e senza parole di una lettera. L'AI riempie parole e definizioni.

**Perché.** È più vicino al motore della fase 6, che userà un risolutore. La misura di G1 resta utile anche se l'AI riempie male gli incroci. Costo: un generatore di schemi in più nell'ondata 1b.1, a carico dell'agente C.

**Opzioni scartate.**
- *B. Come nel ROADMAP.* Lo schema arriva solo se scatta il segnale, e con 5 candidati non c'è tempo per ripartire.
- *C. Metà con schema e metà senza.* Divide in due un campione già piccolo.

**Effetti.** Prompt del cruciverba (agente A) e generatore di schemi (agente C), ondata 1b.1.

### D1b-5. Quali crittografie brevi

**Domanda.** "Crittografia breve" copre più giochi (`docs/giochi_enigmistici.md`, sezione 5): pura, sinonimica, perifrastica, sillogistica, a frase. Quali entrano nel descrittore v0 e nella prova?

**Decisione.** B. Crittografia sinonimica e crittografia perifrastica.

**Perché.** Due sottotipi bastano per vedere se l'AI regge la crittografia, senza disperdere i 5 candidati. Il piano commerciale indica la crittografia come la più difficile da generare. Conviene misurarla sui formati più strutturati: esposto, diagramma e soluzione.

**Opzioni scartate.**
- *A. Solo crittografia sinonimica.* Un solo sottotipo misura poco la famiglia.
- *C. Tutti i sottotipi, con quello scelto nel brief.* Disperde i 5 candidati su più sottotipi.

**Effetti.** Descrittore v0 e prompt della crittografia breve (ondata 1b.1).

### D1b-6. Modello per la creazione

**Domanda.** Il modello predefinito è `claude-sonnet-5-5`, scelto per l'estrazione. La creazione fa poche chiamate, circa 30-60 per la prova 1, e la qualità pesa direttamente su G1. Quale modello usare?

**Decisione.** B. Opus per la creazione, con una chiave di configurazione separata e nuova, distinta da `FornitoreAi:Modello`.

**Perché.** Le chiamate sono poche e la Finestra dell'abbonamento regge. È la stessa logica del ROADMAP, sezione 2.3, punto 5, per i descrittori.

**Opzioni scartate.**
- *A. Lo stesso modello dell'estrazione.* Qualità più bassa su un lavoro che pesa su G1.
- *C. Prova di 2-3 candidati per famiglia con entrambi i modelli, prima della prova 1.* Consuma Finestra e ore di revisione prima della prova.

**Effetti.** Configurazione del Fornitore AI per la creazione (ondata 1b.1).

### D1b-7. Chi scrive i brief tipo

**Domanda.** Il ROADMAP assegna al proprietario 3-4 brief per ciascuna delle quattro famiglie con AI, circa 3 ore. Chi scrive i brief?

**Decisione.** B. Un agente Opus scrive in bozza i brief dalle regole approvate. Il proprietario li corregge, in circa 1-1,5 ore. Nel piano si conta 1,5 ore. Scelta entro il 13/12/2026; brief corretti entro il 3/1/2027.

**Perché.** Risparmia circa 1,5 ore su un calendario con circa un'ora di margine. I brief restano del proprietario dopo la correzione. Con brief liberi cambierebbero da candidato a candidato, e la misura sarebbe meno confrontabile.

**Opzioni scartate.**
- *A. Il proprietario li scrive.* Circa 3 ore.
- *C. Brief liberi, scritti al momento della generazione.* La misura cambia da candidato a candidato.

**Effetti.** Ore del proprietario nella fase 1b (costruzione da 7 a 5,5 ore). File 06 dell'handoff.

### D1b-8. Numerazione delle correzioni della 1b.3

**Domanda.** È la domanda q8 della fase 1 (`.planning/phases/1/CONTEXT.md`, sezione 8). La decisione D6 dice `v0.2.1` e seguenti. Ma prima di `v0.2.0` quelle versioni sposterebbero `latest`.

**Decisione.** A. Le correzioni escono come `v0.2.0-prova.2`, `v0.2.0-prova.3` e così via, senza spostare `latest`, fino a `v0.2.0`. Dopo, `v0.2.1` e seguenti. Questa decisione chiude la domanda q8 per la fase 1 e per la 1b.

**Perché.** Non sposta `latest` e rispetta lo spirito di D6. La numerazione si chiude una volta sola, per entrambe le fasi.

**Opzioni scartate.**
- *B. `v0.2.1` e seguenti come in D6.* Sposta `latest` prima della chiusura della fase 1.

**Effetti.** ROADMAP, sezione 1.2 ("Rami e versioni") e tabella delle stime. File 24 dell'handoff.

### D1b-9. ADR dell'interfaccia (007) nella 1b.2

**Domanda.** Il numero 007 è riservato all'ADR dell'interfaccia, previsto in fase 2. La 1b anticipa l'host web e l'ossatura React. Quando si scrive l'ADR?

**Decisione.** A. L'ADR 007 si scrive nell'ondata 1b.2. Copre solo l'host ASP.NET Core e l'ossatura React, con le scelte già prese il 10/11/2026. Quando la fase 2 estende l'interfaccia, lo sostituisce con un ADR nuovo: un ADR non si riscrive.

**Perché.** Il codice che la fase 2 eredita nasce nella 1b. L'ADR deve esistere quando nasce. Le scelte sono già del proprietario, quindi l'ADR le registra soltanto.

**Opzioni scartate.**
- *B. ADR 007 in fase 2, come previsto.* La 1b documenterebbe l'anticipo solo nel piano.

**Effetti.** Ondata 1b.2 (agente A). Sezione 2.1 della fase 2.

## Altre decisioni del 20/11/2026

### Calendario delle ondate

**Domanda.** La 1.1 si chiude il 20/11 invece del 6/12. Come cambia il calendario?

**Decisione.** La 1b.1 parte il 23/11/2026. Le ondate successive si anticipano di circa due settimane. Le scadenze esterne restano: prototipo pronto verso il 10/1/2027, scadenza il 17/1, prova 1 dal 18/1 al 21/2, G1 il 28/2/2027. Le due settimane sono margine.

**Opzioni scartate.**
- *Anticipare anche le scadenze esterne.*
- *Tenere le date delle ondate.* Si perderebbe il tempo guadagnato.

### Scadenza della verifica delle condizioni d'uso

**Domanda.** Il ROADMAP cita il 7 dicembre, il PLAN di fase 1 il 6 dicembre. Quale data vale?

**Decisione.** Il 6/12/2026. Il set di PDF di prova resta al 7/12/2026 (domanda 14).

**Opzioni scartate.**
- *Il 7/12/2026, come nel ROADMAP.*

### Approvazione delle regole delle Tipologie

**Domanda.** Come si registra l'approvazione delle regole?

**Decisione.** Il proprietario approva in chat. L'agente scrive la data e la versione nel campo `regole` del descrittore, in un commit dedicato che il proprietario chiede. Non serve un comando nuovo.

**Opzioni scartate.**
- *Un comando `dbe types approve`.*
- *Un file separato per le approvazioni.*

### Prova 1 nel piano commerciale

**Domanda.** Il piano commerciale (sezione 9) descrive la prova 1 con 15 candidati per 3 tipologie e 20-40 € di AI. Come si allinea al ROADMAP?

**Decisione.** La prova 1 misura sei famiglie con 5 candidati ciascuna, senza spesa API: i candidati si generano con l'abbonamento Claude. Le ore sono 14, come nella fase 1b. Il piano commerciale riporta una nota datata 20/11/2026.

**Opzioni scartate.** Non registrate dal proprietario. La base è il ROADMAP, sezione 5.4.

### Ore della fase 5

**Decisione.** Le ore del proprietario per la fase 5 sono 9, come nella tabella delle stime. Il testo della fase che diceva 8 si corregge a 9.

**Opzioni scartate.** Non registrate dal proprietario.

### Ore della fase 1b

**Decisione.** La costruzione scende da 7 a 5,5 ore, perché i brief corretti costano 1,5 ore invece di 3 (D1b-7). Il totale fino al lancio passa da circa 131 a circa 129,5 ore.

**Opzioni scartate.** Non registrate dal proprietario. Il calcolo deriva da D1b-7.

## Scelte tecniche confermate

- **Descrittori v0.** La versione 0 identifica il prototipo. Il meta-schema ammette la versione 0, e il generatore SQL della 1.2 la salta.
- **Rebus.** Il prototipo usa il campo `esposto_diagramma` al posto di `immagine_diagramma`. Quest'ultimo richiederebbe un campo `immagine`, vietato dal criterio 8.
- **Porta di creazione.** Una porta separata dal Fornitore AI di estrazione, con le risposte grezze nello schema `prototipo` e non in `riviste.chiamate_ai`.

## Fatti verificati

- `.github/workflows/publish.yml` e `docs/CI.md` citano già il win-x64. Resta da verificare, nella 1b.1, che l'eseguibile `dbe` sia allegato alla pre-release.
- Le date delle fasi 2-9 non sono ricalcolate (ROADMAP, stime).

## Rischi

- **Porta separata e sezione 2.4 del ROADMAP.** La sezione 2.4 prevede "un'unica interfaccia" per estrazione e creazione. La porta separata la contraddice. La 2.4 non è ancora riallineata, fuori dalla fase 1b.
- **Ondate della fase 1 nei documenti di handoff.** La data di fine della 1.2 (28/12/2026, file 05) non è ricalcolata. Le ondate si anticipano, ma la nuova data sta nel PLAN, che non è ancora aggiornato.

## Rimandate

- Nessuna.

## Discussioni aperte

- Nessuna sul prototipo. Vedi il rischio sulla sezione 2.4.
