# Decisioni per la fase 1

Decisioni del proprietario del 19 novembre 2026, prese prima di avviare la fase 1: il Motore di estrazione per i cruciverba e i giochi brevi. Arrivano dopo la chiusura della fase 0 con il tag `v0.1.1`. La pianificazione operativa sta in `.planning/phases/1/CONTEXT.md` e `.planning/phases/1/PLAN.md`. Le decisioni D2, D3, D5 e D7 si registrano anche come ADR nuovi. Gli ADR 001-006 non si riscrivono: i nuovi ADR li sostituiscono o li integrano.

## Termini

**Set di riferimento.** Pagine di Riviste con i risultati attesi corretti dal proprietario. Su di esse si misura l'accuratezza e si confrontano i Fornitori AI. Da evitare: dataset, golden set.

**Registrazione.** La risposta del Fornitore AI per una pagina, salvata una volta e riprodotta nei test senza chiamate. Da evitare: cassetta, mock.

**Set sintetico.** Pagine e PDF prodotti dal progetto, senza contenuti di Riviste. Possono stare nel repository del prodotto. Da evitare: dati finti.

**Linea di base.** Le metriche dell'ultima misura accettata. La regressione fallisce se una metrica scende sotto la linea di base. Da evitare: baseline.

**Finestra dell'abbonamento.** Il periodo d'uso dell'abbonamento Claude oltre il quale le chiamate si fermano fino al rinnovo, per esempio il limite delle 5 ore. Da evitare: quota.

**Archivio.** La cartella del PC dove lo Studio conserva le copie delle Riviste e le immagini delle pagine. Da evitare: storage.

## Perché

La fase 0 si è chiusa il 19/11/2026, con più di un mese di anticipo sul 6/12. Il ROADMAP lasciava all'architetto alcune scelte che toccano la custodia delle Riviste: il set di riferimento su GitHub e le risposte grezze nel registro immutabile. Il parere legale arriva entro il 28/2/2027, quindi quelle scelte vanno prese prima. La libreria citata nell'ADR 003 contraddice lo stesso ADR. Il container dello Studio non esiste prima della fase 2. Il ramo unico per fase e le versioni non reggono con le ondate alternate della fase 1 e della 1b.

## Decisioni bloccate

Queste decisioni sono durevoli. Cambiarle richiede una nuova decisione del proprietario.

### D2. Set di riferimento e Registrazioni solo sul PC del proprietario

**Decisione.** Il Set di riferimento, i risultati attesi e le Registrazioni stanno solo sul PC del proprietario, mai su GitHub. La CI su GitHub esegue un Set sintetico prodotto dal progetto. La regressione sul Set di riferimento vero è uno script locale che il proprietario lancia prima di ogni release. Il rapporto si allega alla release. È un'eccezione dichiarata alla regola «test e pubblicazione solo con GitHub Actions». Vale solo per la regressione sul set vero. Build, test e pubblicazione restano su GitHub Actions. La sezione «Conservazione» del ROADMAP cambia: niente repository `DBEnigmistica-refset`. Il criterio 3 si verifica in locale. Questa decisione prende il numero ADR 008.

**Opzioni scartate.**
- *A. Repository `refset` su GitHub con chiave di sola lettura*: mette i contenuti delle Riviste su macchine di terzi prima del parere legale. Va contro la scelta di tenere le Riviste a casa.
- *B. Runner self-hosted sul PC, obbligatorio per `publish.yml`*: costa installazione e manutenzione del runner. Bloccherebbe la pubblicazione a PC spento.
- *D. Scegliere A per ora e spostare dopo*: rischia di dover cancellare dati già finiti su GitHub.

**Rischio.** Lo script può essere dimenticato. Contromisura: la checklist di release richiede il rapporto allegato.

### D3. Rendering con PDFium: PDFiumCore e la libreria nativa di bblanchon

**Decisione.** Il rendering delle pagine usa PDFiumCore 157.0.8100 (Apache-2.0) per i collegamenti e i pacchetti `bblanchon.PDFium.*` 160.0.8165 (Apache-2.0) per la libreria nativa. Il PNG lo codifica il progetto, in circa 80 righe con `System.IO.Compression`, con test. Questa decisione sostituisce il punto 1 dell'ADR 003 e prende il numero ADR 009 (il 007 resta riservato all'interfaccia della fase 2). Due cose si verificano nell'ondata 1.1: il funzionamento nell'immagine chiseled linux-x64 e la resa dei font non incorporati.

**Opzioni scartate.**
- *A. PDFtoImage 5.4.0*: dipende da SkiaSharp. SkiaSharp è stato eliminato in fase 0, e lo stesso ADR 003 lo scarta.
- *C. Solo la libreria nativa, con le funzioni dichiarate a mano*: la manutenzione dei collegamenti resterebbe a carico del progetto.
- *D. Docnet.Core 2.6.0*: fermo dal 2023.

### D5. Estrazione dal vivo su Windows con Claude Code e login OAuth

**Decisione.** In fase 1 l'estrazione dal vivo gira sul PC Windows del proprietario. Usa l'eseguibile `dbe` win-x64 pubblicato dalla CI e Claude Code installato sul PC. Claude Code usa il login OAuth con l'abbonamento: si esegue `claude` e poi `/login`. Non si usa mai una chiave API. Nessuna `ANTHROPIC_API_KEY` deve restare nell'ambiente: l'adattatore la rimuove dall'ambiente del processo figlio. Claude Code la preferirebbe al login e addebiterebbe a consumo. `dbe check-ai` segnala se manca il login. `CLAUDE_CODE_OAUTH_TOKEN`, ottenuto con `claude setup-token` anch'esso via OAuth, serve solo in un container e in fase 1 non si usa. La prova di Claude Code nel container prevista dall'ADR 004 diventa una nota scritta. La decisione sul container passa alla fase 2. La decisione dipende dalla verifica delle condizioni d'uso dell'abbonamento entro il 7/12/2026 (domanda aperta 13). Questa decisione prende il numero ADR 010 e integra l'ADR 004.

**Opzioni scartate.**
- *B. Claude Code in un'immagine non chiseled, con il token in `.env`*: allarga l'immagine pubblicata. Metterebbe un token in un container prima della verifica delle condizioni d'uso.
- *C. Servizio su Windows chiamato dal container*: anticipa lavoro della fase 2.

### D7. Risposta grezza del Fornitore AI fuori dal registro immutabile

**Decisione.** La risposta grezza del Fornitore AI va in una tabella `riviste.chiamate_ai`, che si può cancellare. Nel registro in sola aggiunta (ADR 005) finiscono solo i metadati della chiamata: fornitore, modello, durata, token e costo se disponibili. Va anche lo SHA-256 della risposta. Questa decisione prende il numero ADR 011, integra l'ADR 005 e corregge il ROADMAP della fase 1, che oggi dice «risposta grezza nel registro».

**Opzioni scartate.**
- *B. Risposta grezza nel registro*: impossibile cancellarla se il legale lo chiedesse, perché il registro vieta UPDATE, DELETE e TRUNCATE.
- *C. Nessuna risposta conservata*: si perde il materiale per l'analisi degli errori dell'ondata 1.4.

**Rischio.** La prova del contenuto regge solo finché la tabella non viene svuotata.

## Scelte ordinarie

**D1. Ordine delle ondate.** Si parte subito. L'ondata 1.1 inizia il 23/11/2026. L'ordine è quello alternato del ROADMAP: 1.1, 1b.1, 1.2, 1b.2, 1.3, poi 1.4 accanto alla 1b.3. Al massimo tre agenti lavorano insieme. Si dà priorità alla 1b sulla calibrazione. Il prototipo della 1b è pronto verso il 10/1/2027, con scadenza il 17/1. La chiusura della fase 1 slitta al 21/2/2027, invece del 24/1.

**Opzioni scartate.**
- *B. 1b.1 e 1b.2 di fila*: ferma l'estrazione per settimane.
- *C. Prima 1.1-1.3, poi la 1b*: mette a rischio la scadenza del 17/1.
- *D. Aspettare il 7/12*: spreca il tempo guadagnato.

**D4. Archivio.** La copia della Rivista va nell'Archivio con nome uguale alla sua impronta SHA-256. Le immagini delle pagine sono PNG in cartelle per impronta. La cartella radice è configurabile con la chiave `Archivio:Cartella`. Il testo nativo va nel database. Costo: lo spazio delle Riviste è occupato due volte. La cartella va inclusa nel backup.

**Opzioni scartate.**
- *Solo riferimento al PDF originale*: si rompe se il file si sposta.
- *Immagini nel database*: appesantisce backup e migrazioni.

**D6. Rami e versioni.** Un ramo per ondata, con una PR verso `main`. Il prototipo della 1b si pubblica come `v0.2.0-prova.1`, come pre-release, senza spostare `latest`. La fase 1 si chiude con `v0.2.0`. Le correzioni della 1b.3 escono come `v0.2.1` e seguenti. Va recepito nel ROADMAP, nella sezione 1.2 e nella tabella delle stime.

**Opzioni scartate.**
- *Ramo per fase*: conflitti tra rami lunghi.
- *Rinumerare a `v0.3.0`*: sposta le versioni di tutte le fasi.

## Fatti verificati

- I file `ClaudeApiClient.cs`, `ClaudeGameAnalyzer.cs` e `MagazineProcessingService.cs`, citati dal ROADMAP della fase 1, sono stati eliminati in fase 0.
- `PdfService.cs` rende «disponibile dalla fase 1». Il servizio implementa `IPdfService`, che è in Domain.
- Restano `PromptTemplates.cs` e le entità `CrosswordGame`, `AnagramGame`, `LogicGame` e `RebusGame`. Il ROADMAP non cita `RebusGame`.
- Le porte del Fornitore AI e del PDF stanno ancora in Domain. L'ADR 004 le vuole in Application. La configurazione del Fornitore AI sta nel progetto Console. Il piano della fase 1 sistema entrambe le cose.
- PDFtoImage 5.4.0 dipende da SkiaSharp.
- Git LFS gratuito offre 10 GiB di spazio e 10 GiB di traffico al mese.
- Il token predefinito di GitHub Actions non legge un altro repository privato. Per questo D2 evita un repository `refset`.
- `publish.yml` accetta tag di pre-release senza spostare `latest`.

## Rischi

- **Claude Code non interattivo non ancora provato.** Non si conoscono ancora output strutturato, immagini e segnale di finestra esaurita. Contromisura: la prova nella 1.1.
- **PDFium e font.** Da verificare il funzionamento nell'immagine chiseled e la resa dei font non incorporati. Contromisura: la verifica nella 1.1.
- **Condizioni d'uso dell'abbonamento.** Non ancora verificate. Da esse dipendono D5 e l'estrazione dal vivo. Scadenza: 7/12/2026.
- **Chiusura della fase 1.** Slitta al 21/2/2027.
- **Regressione locale.** Il controllo sul Set di riferimento vero dipende dalla disciplina del proprietario. Lo script può essere dimenticato.

## Rimandate

- **Container per Claude Code.** La scelta passa alla fase 2 (ADR 010).
- **Soglie di accuratezza.** La conferma avviene a fine fase 1, dopo la linea di base.
- **Libreria di validazione JSON Schema.** La scelta avviene nell'ondata 1.1, dopo la verifica delle licenze.

## Discussioni aperte

- Nessuna.
