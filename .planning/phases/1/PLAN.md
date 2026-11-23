# Fase 1 - Piano delle ondate

Scritto il 19/11/2026 dall'agente di pianificazione (Opus). Aggiornato lo stesso giorno con le decisioni D1-D7 del proprietario (`docs/fase-1-decisioni.md`, ADR 008-011). Contesto e punto di partenza verificato: `CONTEXT.md` nella stessa cartella. Criteri di accettazione: `ROADMAP.md`, fase 1, numeri 1-8.

## 0. Calendario e blocchi

Ordine e date secondo D1. Ogni ondata dura circa due settimane di calendario, compresi revisione e correzioni. Mai più di 3 agenti insieme.

| Ordine | Ondata | Periodo | Agenti | Può partire senza PDF e condizioni d'uso? |
|---|---|---|---|---|
| 1 | **1.1** | **Chiusa il 20/11/2026** (prevista 23/11 - 6/12) senza la prova reale | Opus A, Sonnet B, Sonnet C; poi Haiku D | Fatta. Resta aperto `1.1-prova`: verifica delle condizioni dell'abbonamento entro il 6/12/2026, prova entro il 13/12/2026 |
| 2 | 1b.1 | 23/11 - 6/12/2026 | (fase 1b) | Sì |
| 3 | **1.2** | 7/12 - 20/12/2026 | Sonnet A, Sonnet B, Sonnet C | **Sì.** Usa il Set sintetico |
| 4 | 1b.2 | 21/12/2026 - 3/1/2027 | (fase 1b) | Sì. Prototipo verso il 10/1, scadenza 17/1, pre-release `v0.2.0-prova.1` |
| 5 | **1.3** | 4/1 - 17/1/2027 | Sonnet A, Sonnet B | **In parte.** Pipeline, coda e riepiloghi sul Set sintetico: sì. Misura delle pagine per finestra: servono PDF veri, condizioni verificate e `1.1-prova` chiusa |
| 6 | **1.4** | 18/1 - 14/2/2027, accanto alla 1b.3 (dal 25/1) | Opus A, Sonnet B | **No.** Servono Set di riferimento con risultati attesi corretti, condizioni dei piani gratuiti verificate, modello locale installato |
| 7 | Revisione e verifica | 15/2 - 21/2/2027 | Opus (xhigh), Opus verifica | No: i criteri 3, 5 e 8 si verificano anche in locale sul Set di riferimento |

Calendario anticipato il 20/11/2026 dal proprietario (`docs/fase-1b-decisioni.md`): la 1.1 si è chiusa prima, le ondate seguenti si anticipano, le scadenze esterne non cambiano e il tempo guadagnato diventa margine. Chiusura della fase il 21/2/2027 con `v0.2.0` (D1, D6). La 1b ha la priorità sulla calibrazione: se la 1b.2 non chiude entro il 10/1, la 1.3 aspetta.

## 1. Regole comuni a tutte le ondate

- Prima di ogni ondata: leggere `PROGRESS.md` e saltare le ondate con `STATUS:complete`. Verificare che esistano e non siano vuoti `ROADMAP.md`, `CLAUDE.md`, questo `PLAN.md` e `CONTEXT.md`.
- Un ramo per ondata, `fase-1/ondata-1.<n>`, creato da `main` aggiornato. Una PR verso `main` a fine ondata (D6). Nessun commit senza richiesta del proprietario.
- Ogni agente tocca solo i file del proprio elenco. Per un file di un altro agente scrive la modifica esatta nel rapporto finale. L'agente principale la inoltra al proprietario del file.
- TDD: test rosso, poi codice. Copertura delle righe di Domain, Application e Tipologie almeno 80%.
- Prima di consegnare: `dotnet format DBEnigmistica.slnx --include <file propri>`, `dotnet build DBEnigmistica.slnx -c Release -warnaserror`, test della propria area con Docker attivo.
- Commenti di una frase. Termini del glossario di `CLAUDE.md` e di `docs/fase-1-decisioni.md` (Set di riferimento, Set sintetico, Registrazione, Linea di base, Finestra dell'abbonamento, Archivio). Messaggi all'utente in italiano.
- Mai aprire `.env` né `src/DBEnigmistica.Console/appsettings.Development.json`.
- Mai pagine, PDF, risultati attesi o Registrazioni di Riviste nel repository. Nel repository e in CI solo il Set sintetico, prodotto dal progetto (D2, ADR 008).
- Pacchetti nuovi: licenza verificata (MIT, Apache-2.0, BSD o equivalente), versione esatta in `Directory.Packages.props`, nessuna vulnerabilità nota. Nessun pacchetto `Anthropic*`: l'adattatore API non si costruisce in fase 1.
- Fine ondata: CI verde sulla PR, revisione Opus `xhigh` del diff, correzione di CRITICAL e HIGH, gitleaks, approvazione del proprietario, unione. Solo dopo, la riga in `PROGRESS.md`.

## 2. Scelte tecniche

| Tema | Scelta | Fonte |
|---|---|---|
| Porte | Le porte del PDF e del Fornitore AI si spostano da `Domain/Interfaces` ad Application. Nuove porte: `Application/Acquisizione/IRenderPagine`, `IArchivio`; `Application/FornitoreAi/IFornitoreAiEstrazione`, `IRegistroChiamateAi`. `IPdfService` si sostituisce con le nuove porte. `IAiGameAnalyzer` si sposta in `Application/FornitoreAi` e resta solo per `SchemaProposalService` (fino alla fase 4) e `GameExtractionService` (eliminato nella 1.2). I tipi di `Domain/AI` restano dove sono. Un test di architettura vieta le due porte in Domain | ADR 001 punto 7; ADR 004 punto 1 |
| Adattatori AI | Nel progetto `DBEnigmistica.AI`, che riceve il riferimento ad Application. `FornitoreAiOptions`, il validatore e `IAmbienteFornitoreAi` si spostano da Console ad AI. Console registra solo i comandi e chiama le estensioni di AI | ADR 004 |
| Claude Code | Eseguibile nativo avviato con `System.Diagnostics.Process`, argomenti come `ArgumentList` (mai una stringa di shell, mai `cmd.exe` per un `.cmd`). Prompt via stdin. Immagine in un file temporaneo in una cartella dedicata, con la sola lettura di quella cartella. `ANTHROPIC_API_KEY` tolta da `ProcessStartInfo.Environment`. Output JSON da stdout, validato contro lo JSON Schema. Timeout con chiusura dell'albero dei processi. Opzioni esatte (formato dell'output, schema, modo di passare l'immagine) fissate dalla prova della 1.1 | D5; ADR 010 |
| Login | Su Windows il login OAuth (`claude`, poi `/login`). `check-ai` segnala il login mancante senza chiamate di rete e avvisa se trova `ANTHROPIC_API_KEY` nell'ambiente. `CLAUDE_CODE_OAUTH_TOKEN` non si usa in fase 1 | D5; ADR 010 |
| Validazione JSON Schema | Libreria scelta dall'agente A nella 1.1, entro il 25/11, dopo la verifica di licenza, manutenzione e vulnerabilità. Candidati da verificare: JsonSchema.Net, NJsonSchema, Corvus.JsonSchema. Esito nel documento di progetto | `docs/fase-1-decisioni.md`, "Rimandate" |
| Rendering | PDFiumCore 157.0.8100 per i collegamenti e `bblanchon.PDFium.Linux` e `bblanchon.PDFium.Win32` 160.0.8165 per la libreria nativa. PNG codificato dal progetto con `System.IO.Compression.ZLibStream` e CRC-32 scritto a mano, output deterministico. DPI predefinito 200, chiave `Estrazione:Dpi` (150-300) | D3; ADR 009 |
| Archivio | Radice `Archivio:Cartella`, obbligatoria per i comandi che estraggono. Rivista copiata come `riviste/<sha256>.pdf`. Pagine come `pagine/<sha256-pdf>/<nnn>.png`. Testo nativo in una colonna di `riviste.pages`. `Page.ImagePath` relativo alla radice. Scrittura atomica (file temporaneo e rinomina) | D4 |
| Chiamate AI | Tabella `riviste.chiamate_ai` (fornitore, modello, durata, esito, token, costo, impronta della pagina, versione del prompt, risposta grezza, SHA-256 della risposta), cancellabile dal ruolo `dbe_estrazione`. Evento `chiamata_ai` nel registro con i soli metadati e lo SHA-256, mai il testo. Il nome usa il trattino basso solo per il formato dei tipi di evento del registro (`^[a-z][a-z0-9_]{0,59}$`); l'ADR 011 non cambia. Contratto e porta nella 1.1, persistenza nella 1.3 | D7; ADR 011 |
| Registra e riproduci | Decoratore di `IFornitoreAiEstrazione`. `Registra` salva la risposta per (impronta della pagina, versione del prompt, id del fornitore, modello). `Riproduci` la restituisce e fallisce se manca, con zero chiamate. Cartella delle Registrazioni configurabile: nel repository solo quelle del Set sintetico; quelle vere stanno sul PC del proprietario | D2; ADR 008 |
| Regressione | In CI: `eval extraction --replay` sul Set sintetico contro la sua Linea di base. In locale: script PowerShell `scripts/regressione-locale.ps1` sul Set di riferimento, prima di ogni release. Scrive il rapporto da allegare alla release e stampa il comando `gh release upload` | D2; ADR 008 |
| Tabelle per Tipologia | `riviste.gioco_<id_con_underscore>_v<n>` e `catalogo.gioco_<id_con_underscore>_v<n>` (D7, `docs/ondata-1-2-decisioni-d7.md`), 1:1 con `riviste.games` / `catalogo.giochi`, tabelle figlie per gli `elenchi`. SQL in `tipologie/<id>/v<n>.<schema>.sql`, applicato da `dbe migrate` dopo le migrazioni EF, con tabella `tipologie.migrazioni_applicate` (id, versione, schema, checksum SHA-256) | ADR 002 punti 1, 10; criterio 2 |
| Nucleo `riviste` | `games` riceve `tipologia`, `versione_tipologia`, `stato_tipologia` (`nota` o `proposta`), riquadro `numeric(6,5)` x, y, larghezza, altezza con `CHECK` 0-1, `confidenza`, `avvisi jsonb`. Via `GameType`, `crossword_games`, `anagram_games`, `logic_games`, `rebus_games` e le loro entità. `magazines.impronta` e `pages.impronta` `bytea` UNIQUE | Criterio 7 |
| Coda | Tabella `riviste.lavori` (Rivista, pagina, stato, tentativi, prossimo tentativo). `scan-folder` mette in coda e un ciclo elabora. Finestra esaurita: attesa fino all'ora segnalata dal fornitore o a un intervallo configurato. Ctrl+C lascia le pagine non fatte in coda | Criteri 4 e 6 |

## 3. Ondata 1.1 - Fondamenta del Motore di estrazione

**Stato: chiusa il 20/11/2026** con l'approvazione del proprietario, unita in `main` (PR #31), CI verde, senza la prova reale di Claude Code (`PROGRESS.md`: `PHASE:1 WAVE:1.1 STATUS:complete`). Il compito `1.1-prova` resta aperto (`prova-claude-code-1.1.md`): il proprietario verifica le condizioni d'uso dell'abbonamento entro il 6/12/2026 e la prova si fa entro il 13/12/2026. Fino alla riga `PHASE:1 WAVE:1.1-prova STATUS:complete`, nessuna estrazione o generazione dal vivo. Il testo sotto resta come registro del piano.

**Perché prima.** Sblocca la 1b.1 (interfaccia del Fornitore AI e adattatore Claude Code). Chiude le due verifiche di PDFium (ADR 009) e la prova di Claude Code (ADR 010). Fissa il formato dei descrittori, del risultato atteso, del prompt e delle Registrazioni.

**Ramo:** `fase-1/ondata-1.1`, da `main` aggiornato, dal 23/11/2026.

**Agenti.** A, B e C lavorano in parallelo (3 agenti). D parte dopo che A, B e C hanno consegnato. La revisione Opus `xhigh` chiude l'ondata.

| Agente | Modello | Compito | Motivo del modello |
|---|---|---|---|
| A | Opus | Progetto: meta-schemi, metriche, prompt, Registrazioni, contratto delle chiamate AI, scelta della libreria JSON Schema | Ragionamento e analisi, nessun codice C# |
| B | Sonnet | PDF, PNG, Archivio, Docker e CI | Codice |
| C | Sonnet | Fornitore AI, adattatore Claude Code, configurazione, prova reale | Codice |
| D | Haiku | Documentazione per il proprietario | Documenti da decisioni già prese |

**Scambi tra agenti, con date.**
- 25/11: A consegna la libreria JSON Schema scelta, con licenza e versione. B la aggiunge a `Directory.Packages.props`.
- 25/11: C chiede a B le righe di `Directory.Packages.props` che gli servono (per esempio `Microsoft.Extensions.Options`). B le aggiunge lo stesso giorno.
- 27/11: A consegna il contratto provvisorio dell'output per pagina e della riga di `chiamate_ai`. C lo usa e lo allinea alla versione finale a fine ondata.

### Agente A (Opus) - Progetto

File (solo documenti e dati, nessun codice C#): `.planning/phases/1/progetto-1.1.md` (nuovo), `tipologie/_meta/descrittore.schema.json` (nuovo), `tipologie/_meta/risultato-atteso.schema.json` (nuovo), `tipologie/_meta/risposta-pagina.schema.json` (nuovo), `tipologie/_meta/prompt-estrazione-v1.md` (nuovo), `tipologie/_meta/esempi/**` (nuovo).

Compiti:
1. Libreria di validazione JSON Schema: confronto dei candidati per licenza, manutenzione, vulnerabilità note, supporto della bozza dello schema usata, dipendenze. Scelta motivata entro il 25/11.
2. Meta-schema dei descrittori come JSON Schema (ADR 002, ROADMAP 2.2): campi, `elenchi`, tipi a lista chiusa, `ruolo`, `indice`, `resa`, `validatori` (nomi da un elenco chiuso), `regole` con data e versione di approvazione.
3. Regole di traduzione: descrittore → SQL (tabelle in `riviste` e `catalogo`, nomi, vincoli, indici ammessi, ordine deterministico delle istruzioni). Descrittore → JSON Schema della risposta per pagina (elenco di Giochi con tipologia nota o proposta, titolo, riquadro, dati, confidenza per campo, avvisi). Descrittore → modello di resa.
4. Formato del risultato atteso per pagina e definizione esatta delle metriche: abbinamento dei Giochi con IoU ≥ 0,5 e stessa tipologia, algoritmo di abbinamento, normalizzazione del CER, caselle del cruciverba, campi esatti, aggregazione per digitali e scansioni.
5. Prompt di estrazione v1, costruito dai descrittori e versionato. La versione entra nella chiave delle Registrazioni.
6. Formato delle Registrazioni e della Linea di base. Contenuto minimo del Set sintetico (4-6 pagine: griglia digitale, giochi brevi, font non incorporato, pagina solo immagine).
7. Contratto della riga di `riviste.chiamate_ai` e dei `dati` dell'evento `chiamata_ai` nel registro: soli metadati e SHA-256 (D7).
8. Squawk: elenco delle regole da disattivare per le tabelle nuove create nella stessa migrazione, con motivo.

Fatto quando:
- Il documento di progetto copre i punti 1-8 ed è approvato dall'agente principale.
- I tre JSON Schema sono validi. Gli esempi di descrittore, risultato atteso e risposta per pagina li passano. Un esempio non valido per ciascuno li fallisce.
- La libreria JSON Schema è scelta con licenza, versione esatta e motivo.

### Agente B (Sonnet) - PDF, PNG, Archivio, immagine Docker

File:
- `src/DBEnigmistica.Application/Acquisizione/IRenderPagine.cs`, `IArchivio.cs`, `PaginaRenderizzata.cs` (nuovi);
- `src/DBEnigmistica.Domain/Interfaces/IPdfService.cs` (eliminazione);
- `src/DBEnigmistica.Infrastructure/Services/Pdf/**`, `src/DBEnigmistica.Infrastructure/Services/Archivio/**` (nuovo), `src/DBEnigmistica.Infrastructure/Configuration/DependencyInjection.cs` (solo i propri servizi e le opzioni `Archivio` ed `Estrazione`);
- `src/DBEnigmistica.Console/Commands/DiagnosticaRenderCommand.cs` (nuovo, comando nascosto), `src/DBEnigmistica.Console/Cli/CliApp.cs` e `src/DBEnigmistica.Console/Commands/HandlerRegistration.cs` (solo la registrazione del comando);
- `Directory.Packages.props`, `src/DBEnigmistica.Infrastructure/DBEnigmistica.Infrastructure.csproj`, `tests/DBEnigmistica.Tests/DBEnigmistica.Tests.csproj` (unico proprietario nell'ondata);
- `Dockerfile`, `.github/workflows/ci.yml` (lavoro "Build immagine Docker"), `.github/workflows/publish.yml` (lavoro "Eseguibile console"), `.gitignore` (solo se serve un'eccezione per PDF sintetici);
- `tests/DBEnigmistica.Tests/Unit/Pdf/**`, `tests/DBEnigmistica.Tests/Unit/Archivio/**`, `tests/DBEnigmistica.Tests/Fixtures/PdfSintetici/**` (nuovi).

Compiti:
1. Pacchetti: PDFiumCore 157.0.8100, `bblanchon.PDFium.Linux` e `bblanchon.PDFium.Win32` 160.0.8165 (D3). Verificare che PDFiumCore non porti una propria copia nativa. Se la porta, escluderla e scriverlo nel rapporto.
2. Rendering di una pagina a DPI configurabile con PDFium. Dimensioni in pixel. Errori chiari per PDF cifrati o danneggiati.
3. Codificatore PNG del progetto (circa 80 righe): firma, IHDR, IDAT con `ZLibStream`, IEND, CRC-32 scritto a mano. Output identico byte per byte per lo stesso input.
4. Testo nativo con PdfPig dietro la porta. `IPdfService` sostituita dalle nuove porte, `PdfService` adattato o sostituito.
5. Impronte SHA-256 del file e di ogni pagina. Scelta dell'impronta della pagina motivata nel rapporto.
6. Archivio su file system (D4): opzioni `Archivio:Cartella` con validatore, copia della Rivista per impronta, PNG per impronta, scrittura atomica, nessun percorso costruito da input non validato.
7. Generatore di PDF sintetici per i test, con PdfPig: pagina con griglia e definizioni scritte dal progetto, pagina con un font standard non incorporato, pagina con un font TrueType non incorporato e non standard, pagina solo immagine. Preferenza: PDF generati al volo, nessun PDF nel repository.
8. Comando nascosto `dbe diagnostica-render`: genera in memoria il PDF sintetico, rende le pagine e verifica che l'area del testo non sia vuota. Codice 0 se riesce. Con un'opzione salva i PNG in una cartella.
9. `Dockerfile`: elenco ammesso al posto del divieto. Ammessa solo la libreria nativa di PDFium per linux-x64. Qualsiasi altro file nativo fa fallire la build. Pubblicazione per `linux-x64`, così le librerie degli altri sistemi non entrano. Commento su `CLAUDE_CODE_OAUTH_TOKEN` aggiornato (non usato in fase 1).
10. `ci.yml`, lavoro "Build immagine Docker": passo nuovo che esegue `dbe diagnostica-render` nell'immagine chiseled e carica i PNG come artefatto.
11. `publish.yml`, lavoro "Eseguibile console": controllo che l'archivio `win-x64` contenga `pdfium.dll` e quello `linux-x64` `libpdfium.so`.

Test da scrivere prima: rendering di un PDF sintetico a 150 e 200 DPI con dimensioni attese; PNG valido (firma, CRC di ogni blocco, dati decompressi uguali ai pixel); stesso PNG per due codifiche dello stesso input; stessa impronta per due rendering della stessa pagina; impronte diverse per pagine diverse; testo nativo presente e assente; pagine con font non incorporati rese senza eccezioni e con pixel non bianchi nell'area del testo; archivio idempotente (la seconda scrittura non duplica); percorso con `..` o assoluto rifiutato; `Archivio:Cartella` mancante → errore di configurazione.

Fatto quando:
- Test verdi su Windows (in locale) e su Linux (CI).
- La build dell'immagine fallisce se si aggiunge un file nativo fuori elenco (provato una volta in locale, esito nel rapporto).
- `dbe diagnostica-render` termina con 0 nell'immagine chiseled linux-x64, in CI. Verifica 1 dell'ADR 009 chiusa.
- I PNG dei font non incorporati, resi nell'immagine chiseled e su Windows, sono leggibili a vista. L'agente principale li confronta dall'artefatto della CI. Verifica 2 dell'ADR 009 chiusa.
- Se una delle due verifiche fallisce: B si ferma, scrive il rapporto e l'agente principale porta la questione al proprietario (nuovo ADR, ADR 009).

### Agente C (Sonnet) - Fornitore AI e adattatore Claude Code

File:
- `src/DBEnigmistica.Application/FornitoreAi/**` (nuovo);
- `src/DBEnigmistica.Domain/Interfaces/IAiGameAnalyzer.cs` (spostamento in Application);
- `src/DBEnigmistica.Application/Services/Processing/GameExtractionService.cs` e `src/DBEnigmistica.Application/Services/Schema/SchemaProposalService.cs` (solo le direttive `using`);
- `src/DBEnigmistica.AI/**` (tutto il progetto, compreso il `.csproj` e il riferimento ad Application);
- `src/DBEnigmistica.Console/Configuration/FornitoreAiOptions.cs` (spostamento in AI), `src/DBEnigmistica.Console/Commands/CheckAiCommand.cs`, `src/DBEnigmistica.Console/Cli/CliHost.cs`, `src/DBEnigmistica.Console/Cli/HostRequirements.cs`;
- `tests/DBEnigmistica.Tests/Unit/FornitoreAi/**` (nuovo), `tests/DBEnigmistica.Tests/Unit/Cli/FornitoreAi*.cs`, `tests/DBEnigmistica.Tests/Unit/Services/GameExtractionServiceTests.cs` e `SchemaProposalServiceTests.cs` (solo `using`), `tests/DBEnigmistica.Tests/Architecture/**`, `tests/DBEnigmistica.Tests/Fixtures/ClaudeFinto/**` (nuovo);
- `.planning/phases/1/prova-claude-code-1.1.md` (nuovo, rapporto della prova e nota sul container).

Compiti:
1. `IFornitoreAiEstrazione`: dalla pagina (immagine, testo nativo, catalogo dei descrittori, JSON Schema, versione del prompt) alla risposta strutturata con i metadati della chiamata. Esiti: riuscita, risposta non valida, finestra esaurita (con istante di ripresa se noto), errore transitorio, errore definitivo.
2. Adattatore Claude Code non interattivo (sezione 2): timeout, retry con attesa crescente solo sugli errori transitori, al massimo una richiesta di correzione se la risposta non passa lo schema, riconoscimento della finestra esaurita.
3. Ambiente del processo figlio: `ANTHROPIC_API_KEY` sempre tolta (D5). Le altre variabili che cambiano autenticazione o fornitore, trovate nella prova, si tolgono con lo stesso meccanismo.
4. Decoratori: `Registra`, `Riproduci` (D2), `LimitePerEsecuzione` (codice 4, "limite dell'esecuzione raggiunto"), contatore delle chiamate. Adattatore finto configurabile per i test.
5. `IRegistroChiamateAi` con il contratto di A (D7): implementazione in memoria. La persistenza in `riviste.chiamate_ai` e l'evento nel registro arrivano nella 1.3.
6. Spostamento di `IAiGameAnalyzer` in Application e della configurazione del Fornitore AI in AI. Test di architettura: nessuna porta del PDF o del Fornitore AI in `Domain/Interfaces`; pacchetti dei fornitori solo in `DBEnigmistica.AI`; Application non dipende da AI.
7. `check-ai` (D5): se manca il login dice "Claude Code non autenticato: esegui claude e poi /login" con codice 3. Se trova `ANTHROPIC_API_KEY` stampa un avviso: la chiave non verrà passata a Claude Code. Nessuna chiamata di rete. Il resto del comportamento non cambia.
8. Prova reale su Windows, **solo dopo** che il proprietario ha confermato le condizioni dell'abbonamento e fatto il login: una pagina del Set sintetico con Claude Code installato. Il rapporto risponde a: formato effettivo dell'output e se si può imporre lo schema; come passa l'immagine; testo, codice d'uscita e ora di ripresa della finestra esaurita (osservati o, se non provocabili, dalla documentazione, con la fonte); dove Claude Code salva il login su Windows; tempi, token e costo riportati. Una sola pagina, fuori dalle ore degli agenti.
9. Nota scritta su Claude Code nel container (ADR 010, punto 6): cosa servirebbe nell'immagine e quali rischi. Nessuna costruzione.

Test da scrivere prima: con un eseguibile finto (programma di test che stampa risposte preparate) l'adattatore restituisce la risposta valida; risposta non conforme → una richiesta di correzione, poi "risposta non valida"; messaggio di finestra esaurita → esito dedicato con l'ora di ripresa; timeout → errore transitorio, retry e processo chiuso; argomenti passati come elenco, mai in una stringa di shell; `ANTHROPIC_API_KEY` presente nel processo padre e assente nel figlio; riproduzione senza Registrazione → errore esplicito e zero chiamate; limite di 1 pagina → seconda chiamata rifiutata con codice 4; `check-ai` senza login → codice 3 e messaggio esatto; `check-ai` con la chiave nell'ambiente → avviso.

Fatto quando:
- Nessuna chiamata di rete nei test. `dbe check-ai` con login presente si comporta come prima.
- Test di architettura verdi con le porte in Application.
- Rapporto della prova reale consegnato, con le risposte del punto 8. Se le condizioni non sono verificate entro il 6/12, vedi "Criteri di uscita".
- Nota sul container consegnata.

### Agente D (Haiku) - Documentazione, dopo A, B e C

File: `README.md`, `SETUP_INSTRUCTIONS.md`, `docs/CI.md`, `CLAUDE.md` (sezione 6, solo le righe su Archivio e login).

Compiti: login di Claude Code su Windows (`claude`, poi `/login`) e divieto di `ANTHROPIC_API_KEY`; chiave `Archivio:Cartella` e inclusione della cartella nel backup; comando `dbe diagnostica-render` e passo nuovo della CI; elenco ammesso delle librerie native. Frasi brevi, nessun nome di classe interna.

Fatto quando: i quattro file citano solo comandi, chiavi e comportamenti che esistono nel ramo, verificati dall'agente principale.

### Criteri di uscita dell'ondata 1.1
- CI verde (4 lavori) sulla PR di `fase-1/ondata-1.1`. Il lavoro Docker rende il PDF sintetico nell'immagine chiseled.
- Le due verifiche dell'ADR 009 chiuse con esito positivo, oppure ADR riaperto con il proprietario.
- Documento di progetto, meta-schemi e libreria JSON Schema approvati dall'agente principale. Nessuna decisione D1-D7 contraddetta.
- Prova reale di Claude Code fatta, con rapporto. Se le condizioni dell'abbonamento non sono verificate entro il 6/12, l'ondata si chiude senza la prova solo con l'approvazione del proprietario. La prova resta allora un compito aperto, con la riga `PHASE:1 WAVE:1.1-prova STATUS:complete` quando si fa. Nessuna estrazione o generazione dal vivo prima di quella riga.
- Revisione Opus `xhigh` senza CRITICAL o HIGH aperti.
- Riga: `PHASE:1 WAVE:1.1 STATUS:complete TS:<ISO-8601 UTC>`.

## 4. Ondata 1.2 - Tipologie, generatore, misure

**Ramo:** `fase-1/ondata-1.2`, dal 7/12/2026 (fine prevista il 20/12), dopo l'unione della 1b.1.

**Stato: chiusa il 23/11/2026** con l'approvazione del proprietario, unita in `main` (PR #33) con CI verde. Le regole delle 4 Tipologie restano da approvare entro il 4/12/2026.

### Sonnet A - Generatore SQL, migrazioni delle Tipologie, scrittore e lettore

File: `src/DBEnigmistica.Tipologie/**` (nuovo progetto, tranne `Validatori/**` e `Descrittori/**` di B), `DBEnigmistica.slnx`, `Directory.Packages.props`, `tests/.../DBEnigmistica.Tests.csproj` (unico proprietario nell'ondata); `src/DBEnigmistica.Infrastructure/Tipologie/**` (scrittore, lettore, applicazione delle migrazioni); `src/DBEnigmistica.Console/Commands/MigrateCommand.cs`, `TypesCommand.cs` (nuovo); `.github/workflows/ci.yml` (passi `types generate --check` e squawk); `.github/scripts/soglia-copertura.py`; `tests/.../Unit/Tipologie/Generatore/**`, `tests/.../Integration/Tipologie/**`.

Compiti: caricamento e validazione dei descrittori contro il meta-schema, con la libreria scelta nella 1.1. Dalla 1b.1 il meta-schema ammette `versione` 0 per i descrittori di prototipo (`tipologie/<id>/v0.json`, decisione del 20/11/2026, `.planning/phases/1b/PLAN.md`, sezione 2): il caricatore li accetta, ma il generatore SQL e `dbe types generate [--check]` li saltano e non producono file SQL né tabelle; `dbe migrate` non li applica. Generatore SQL deterministico (lista chiusa, identificatori validati e quotati, nessun DEFAULT, trigger, funzione o GRANT); derivazione dello JSON Schema; `dbe types generate [--check]` (codice diverso da 0 se l'SQL registrato differisce); `dbe migrate` che applica EF e poi le Tipologie con checksum, idempotente; scrittore e lettore Npgsql parametrizzati; fuzz test.

Test da scrivere prima: descrittore valido → SQL atteso (snapshot); descrittore `v0.json` valido → nessun file SQL generato, `types generate --check` a 0 senza SQL registrato per esso, nessuna tabella creata da `migrate`; identificatori con payload d'iniezione, parole riservate, tipi fuori lista → rifiutati; fuzz con 10.000 descrittori casuali (seme fisso registrato) → rifiutati o quotati, e applicati su PostgreSQL non cambiano il numero di tabelle fuori da quelle attese; `migrate` due volte → la seconda non fa nulla; checksum alterato → errore; scrittore con valori ostili salvati come dati.

### Sonnet B - Descrittori, validatori, nucleo `riviste`

File: `tipologie/cruciverba-classico/**`, `tipologie/anagramma/**`, `tipologie/sciarada/**`, `tipologie/cambio-di-lettera/**`; `src/DBEnigmistica.Tipologie/Validatori/**`, `src/DBEnigmistica.Tipologie/Descrittori/**`; `src/DBEnigmistica.Domain/Entities/**`, `src/DBEnigmistica.Domain/Enums/GameType.cs` (eliminazione); `src/DBEnigmistica.Infrastructure/Data/**`, `src/DBEnigmistica.Infrastructure/Migrations/**`, `src/DBEnigmistica.Infrastructure/Repositories/**`; `src/DBEnigmistica.Application/Services/Processing/GameExtractionService.cs` (eliminazione o adattamento minimo), `src/DBEnigmistica.Application/Services/Export/**`, `src/DBEnigmistica.AI/Prompts/PromptTemplates.cs` e `src/DBEnigmistica.AI/Models/Claude*Response.cs` (eliminazione); test esistenti che costruiscono `Game`.

Compiti: descrittori v1 delle 4 Tipologie con `regole` in bozza (da approvare dal proprietario); validatori strutturali (numerazione coerente con la griglia, lunghezze coerenti, incroci; per i giochi brevi coerenza tra esposto e soluzione dove la regola lo permette); migrazione EF del nucleo (sezione 2, riga "Nucleo"), con in più la colonna `riviste.games.trascrizione_proposta text NULL`, valorizzata solo con `stato_tipologia = proposta` (`progetto-1.1.md`, sezione 10); eliminazione delle entità, delle tabelle di dettaglio e dell'enum; aggiornamento di repository ed esportazioni. Prima di toccare il codice, elenco di tutti i punti d'uso di `Game`, `GameType` e delle entità di dettaglio, approvato dall'agente principale.

Test da scrivere prima: ogni validatore con casi validi e non validi; descrittori conformi al meta-schema; riquadro fuori da 0-1 rifiutato dal database; Giochi con `stato_tipologia = proposta` salvabili senza tabella dedicata; esportazioni CSV e JSON invariate nel contenuto per i campi rimasti.

### Sonnet C - Valutazione, Set sintetico, regressione locale

File: `src/DBEnigmistica.Application/Valutazione/**` (nuovo); `src/DBEnigmistica.Console/Commands/EvalCommand.cs` (nuovo, con `draft` ed `extraction`); `tests/.../Unit/Valutazione/**`, `tests/.../Fixtures/SetSintetico/**` (nuovo); `scripts/regressione-locale.ps1` (nuovo); `.github/workflows/ci.yml` (solo il passo di regressione sul Set sintetico).

Compiti:
1. Lettura di un set (cartella con pagine, risultati attesi, Registrazioni, Linea di base), uguale per il Set sintetico e per il Set di riferimento (D2).
2. Calcolo delle metriche come definite nella 1.1. Rapporto JSON e Markdown.
3. `eval extraction --replay` (in CI) e `--live`. Confronto con la Linea di base: fallisce se una metrica scende.
4. `eval draft`: bozze dei risultati attesi da una Registrazione e ciclo di correzione semplice da riga di comando (file JSON per pagina, controllo contro lo schema).
5. Set sintetico di 4-6 pagine prodotte con il generatore della 1.1, risultati attesi e Registrazioni scritti dal progetto, Linea di base sintetica.
6. Passo CI: `eval extraction --replay` sul Set sintetico contro la sua Linea di base.
7. Script `scripts/regressione-locale.ps1` per il PC del proprietario (D2): legge la cartella del Set di riferimento da un parametro, esegue la regressione riprodotta, scrive il rapporto in una cartella con la versione, stampa il comando `gh release upload`. Codice diverso da 0 se una metrica scende sotto la Linea di base. Nessun dato del Set di riferimento esce dal PC.

Test da scrivere prima: metriche su casi costruiti a mano (Gioco mancante, in più, tipologia sbagliata, IoU sotto 0,5, CER con maiuscole e apostrofi, caselle); rapporto identico su due esecuzioni; regressione sotto la Linea di base → codice diverso da 0; set senza Registrazione per una pagina → errore esplicito e zero chiamate.

### Criteri di uscita dell'ondata 1.2
- `dbe types generate --check` e `dbe migrate` doppio verdi in CI. Squawk in CI senza errori sulle migrazioni generate.
- I descrittori `v0.json` della 1b.1 sono caricati e validati, ma non generano SQL né tabelle (test dedicato).
- Regressione riprodotta sul Set sintetico verde in CI.
- Script locale provato dal proprietario su una cartella di prova (anche il Set sintetico), con rapporto prodotto.
- Regole delle 4 Tipologie inviate al proprietario per l'approvazione (circa 1 h). La 1b.2 aspetta almeno quelle di `cruciverba-classico` e `anagramma`.
- Haiku aggiorna `docs/CI.md` con la checklist di release (rapporto della regressione locale allegato, D2).
- Revisione Opus `xhigh` con attenzione al generatore e allo scrittore.
- Riga: `PHASE:1 WAVE:1.2 STATUS:complete TS:<ISO-8601 UTC>`.

## 5. Ondata 1.3 - Pipeline e coda

**Ramo:** `fase-1/ondata-1.3`, dal 4/1/2027 (fine prevista il 17/1), dopo la 1b.2.

### Sonnet A - Pipeline per pagina e chiamate AI

File: `src/DBEnigmistica.Application/Acquisizione/**` (tranne `Coda/**`); `src/DBEnigmistica.Infrastructure/Acquisizione/**` (persistenza delle pagine e delle chiamate AI); `src/DBEnigmistica.Infrastructure/Data/**` e `Migrations/**` (una sola migrazione, con le tabelle della coda indicate da B); `src/DBEnigmistica.Console/Commands/ScanFileCommand.cs`; `.github/workflows/ci.yml` (solo il passo end-to-end); test `Unit/Acquisizione/**`, `Integration/Acquisizione/**` (tranne `Coda`), `EndToEnd/ScanFile*`.

Compiti: per ogni pagina rendering, testo nativo, Archivio, chiamata, verifica dello schema, validatori, persistenza in una transazione per pagina (Giochi, dati per Tipologia, riga di `riviste.chiamate_ai`, evento `chiamata_ai` nel registro con metadati e SHA-256, D7); salto delle pagine con impronta già elaborata; Gioco di tipologia non prevista salvato come proposta; metadati della Rivista dalla copertina e dal nome del file, sovrascrivibili con le opzioni; `scan-file` con `--replay`, `--limite-pagine`, `--dpi`; `Archivio:Cartella` richiesta all'avvio di `scan-*`.

Test da scrivere prima: `scan-file <pdf sintetico> --replay` salva esattamente i Giochi attesi e la seconda esecuzione fa 0 chiamate e 0 duplicati (criterio 3); errore su una pagina non annulla le altre; `ImagePath`, testo nativo e riquadro valorizzati (criterio 7); tipologia sconosciuta → proposta, nessun Gioco perso; i `dati` dell'evento `chiamata_ai` contengono lo SHA-256 e non il testo della risposta; `DELETE` su `riviste.chiamate_ai` riuscito con il ruolo `dbe_estrazione` e `verify-integrity` ancora a 0 dopo la cancellazione.

### Sonnet B - Coda, riepiloghi, costi, misura della finestra

File: `src/DBEnigmistica.Application/Acquisizione/Coda/**`; `src/DBEnigmistica.Console/Commands/ScanFolderCommand.cs`, `CostsCommand.cs` (nuovo); `src/DBEnigmistica.Console/Presentation/Riepiloghi*.cs` (nuovi); `scripts/misura-finestra.ps1` (nuovo); test `Unit/Coda/**`, `Integration/Coda/**`.

Compiti: coda persistente (sezione 2), `scan-folder [--recursive]`, ripresa dopo interruzione, attesa e ripresa con finestra esaurita, limite per esecuzione (codice 4), tetto di spesa solo con l'adattatore API (che non esiste: test con un fornitore finto che riporta costi); riepiloghi per Gioco e per Rivista; `dbe costs` per Rivista, pagina e fornitore; script di misura delle pagine per finestra, da lanciare sul PC.

Test da scrivere prima: interruzione a metà con estrattore finto che conta le chiamate → ripresa dalle pagine mancanti (criterio 4); finestra esaurita simulata → attesa e ripresa senza perdere pagine; limite di 1 pagina → codice 4 e messaggio esatto; tetto di 0,01 € con fornitore finto a costo → codice 4 "tetto di spesa raggiunto" (criterio 6).

### Criteri di uscita dell'ondata 1.3
- Criteri 3, 4, 6, 7 verdi in CI sul Set sintetico.
- Criterio 3 sul Set di riferimento verificato in locale con lo script, se le Registrazioni vere esistono (D2).
- Misura delle pagine per finestra su pagine vere, se PDF e condizioni sono arrivati, con il segnale del rischio "più di due finestre per 96 pagine".
- Riga: `PHASE:1 WAVE:1.3 STATUS:complete TS:<ISO-8601 UTC>`.

## 6. Ondata 1.4 - Misura reale e confronto dei Fornitori AI

**Ramo:** `fase-1/ondata-1.4`, dal 18/1/2027 al 14/2, accanto alla 1b.3, che parte il 25/1 (in totale 3 agenti).

Bloccata finché non ci sono: Set di riferimento con risultati attesi corretti dal proprietario, condizioni dei piani gratuiti verificate, modello locale installato (se possibile). Le esecuzioni dal vivo girano sul PC del proprietario, fuori dalle sessioni della prova 1.

- **Opus A** - File: `.planning/phases/1/analisi-1.4.md`, `tipologie/_meta/prompt-estrazione-v<n>.md`, configurazione DPI. Prima misura dal vivo con Claude (`eval extraction --live`) sul Set di riferimento; analisi degli errori con le risposte grezze di `riviste.chiamate_ai`; nuove versioni di prompt e DPI; decisione motivata sulla seconda chiamata sul ritaglio; confronto dei fornitori sullo stesso set con costo e pagine per finestra; bozza dell'ADR del fornitore predefinito (da scrivere poi in Haiku).
- **Sonnet B** - File: `src/DBEnigmistica.AI/Adattatori/<fornitore>/**` (nuovi), registrazione in configurazione, test con risposte del Set sintetico. Adattatori dei piani gratuiti scelti e del modello locale; correzioni da A; nuova Linea di base registrata sul PC.
- Uscita: rapporto di accuratezza e rapporto di confronto (JSON e leggibile), prodotti sul PC e pronti da allegare alla release; riga `PHASE:1 WAVE:1.4 STATUS:complete TS:<ISO-8601 UTC>`.

## 7. Revisione e verifica di fine fase

- **Revisione** (Opus `xhigh`) sul diff completo della fase: sicurezza del generatore e dello scrittore, invocazione di Claude Code (argomenti, stdin, file temporanei, permessi, ambiente senza `ANTHROPIC_API_KEY`), segreti del Fornitore AI, limiti per esecuzione, determinismo dei test riprodotti, assenza di contenuti delle Riviste nel repository e nel registro.
- **Verifica** (Opus): esegue i comandi dei criteri 1-8 in CI. Per i criteri 3, 5 e 8 sul Set di riferimento, legge i rapporti prodotti sul PC dal proprietario.
- **Proprietario**: conferma delle soglie, scelta del fornitore predefinito, collaudo, regressione locale, approvazione del tag `v0.2.0` e allegato dei rapporti alla release (D2, D6).
- Riga: `PHASE:1 WAVE:revisione STATUS:complete TS:<ISO-8601 UTC>`.

## 8. Mappa verso i criteri di accettazione

| Criterio | Ondata e agente | Prova |
|---|---|---|
| 1. `types generate --check`; fuzz 10.000 descrittori, nessuna istruzione estranea | 1.1 A (regole), 1.2 A | Test unitari e d'integrazione; passo CI |
| 2. `migrate` su database vuoto crea nucleo e 4 Tipologie in `riviste` e `catalogo`; idempotente; squawk pulito | 1.2 A, 1.2 B | Test d'integrazione; passo squawk in CI |
| 3. `scan-file --replay` salva i Giochi attesi; seconda esecuzione 0 chiamate, 0 duplicati | 1.1 C (riproduzione), 1.2 C (Set sintetico, script locale), 1.3 A | In CI sul Set sintetico; in locale sul Set di riferimento con `scripts/regressione-locale.ps1` (D2) |
| 4. Ripresa di `scan-folder` dalle pagine mancanti | 1.3 B | Test con estrattore finto |
| 5. `eval extraction --live` rispetta le soglie confermate; rapporto in release | 1.2 C (strumento), 1.4 A/B (misura) | Esecuzione sul PC del proprietario; rapporto allegato |
| 6. Riga di registro per ogni chiamata; limite 1 pagina → codice 4; tetto 0,01 € → codice 4; finestra esaurita → ripresa | 1.1 A (contratto), 1.1 C (porta, limite), 1.3 A (registro, `chiamate_ai`), 1.3 B (coda, tetto) | Test unitari e d'integrazione; nel registro metadati e SHA-256, la risposta in `riviste.chiamate_ai` (D7) |
| 7. `ImagePath`, testo nativo, riquadro normalizzato valorizzati; nessun Gioco perso per tipologia imprevista | 1.1 B (render, Archivio), 1.2 B (nucleo), 1.3 A | Test d'integrazione sul Set sintetico |
| 8. Rapporto di confronto con pagine per finestra; fornitore predefinito registrato in configurazione e ADR | 1.3 B (misura), 1.4 A/B | Rapporto allegato; ADR nuovo (Haiku) |

## 9. Aggiornamenti della CI

| Ondata | Aggiunta | Dove |
|---|---|---|
| 1.1 | Elenco ammesso delle librerie native (solo PDFium linux-x64) | `Dockerfile` |
| 1.1 | `dbe diagnostica-render` nell'immagine chiseled, PNG come artefatto | `ci.yml`, lavoro "Build immagine Docker" |
| 1.1 | Libreria nativa di PDFium presente negli archivi `win-x64` e `linux-x64` | `publish.yml`, lavoro "Eseguibile console" |
| 1.1 | Test di architettura su porte e pacchetti dei fornitori | "Build e test" (nessun passo nuovo) |
| 1.2 | `dbe types generate --check`; fuzz test; migrazioni delle Tipologie su Testcontainers; squawk con versione fissata e checksum, regole escluse motivate | "Build e test" o lavoro nuovo "Migrazioni" |
| 1.2 | Soglia di copertura estesa a `DBEnigmistica.Tipologie` | `.github/scripts/soglia-copertura.py` |
| 1.2 | Regressione riprodotta sul Set sintetico contro la sua Linea di base | "Build e test" |
| 1.3 | Test end-to-end di `scan-file --replay` sul Set sintetico | `EndToEnd` |
| fine fase | Immagine con `scan-*`; rapporti di accuratezza, confronto e regressione locale allegati alla release dal proprietario | `publish.yml` (immagine e archivi); allegati a mano (D2) |

Nessun lavoro della CI usa l'abbonamento, legge il Set di riferimento o invia pagine a un Fornitore AI. La regressione sul Set di riferimento è l'unica eccezione alla regola "solo GitHub Actions" (ADR 008).

## 10. Dove si intrecciano le ondate della fase 1b

| Momento | Ondata 1b | Cosa prende dalla fase 1 | Vincolo |
|---|---|---|---|
| Dopo la 1.1 (23/11-6/12) | 1b.1 | `IFornitoreAi*`, adattatore Claude Code, meta-schema dei descrittori | La 1b.1 aggiunge una porta di creazione separata, senza cambiare il contratto di estrazione. Nuovo host `DBEnigmistica.Web`, comando `dbe prototipo` e schema `prototipo`: migrazione EF propria. Il meta-schema ammette `versione` 0 (vedi 1.2 A). Generazione dal vivo solo dopo la riga `PHASE:1 WAVE:1.1-prova STATUS:complete` |
| Dopo la 1.2 (21/12) | 1b.2 | Descrittori v1 e regole approvate di `cruciverba-classico` e `anagramma` | Se le regole non sono approvate, la 1b.2 parte con le bozze e le aggiorna. Pre-release `v0.2.0-prova.1` senza spostare `latest` (D6) |
| Durante la prova 1 (18/1-21/2) | 1b.3 (1 agente) accanto alla 1.4 (2 agenti) | - | Al massimo 3 agenti. Le esecuzioni dal vivo della 1.4 fuori dalle sessioni di prova. Correzioni come `v0.2.0-prova.N` fino a `v0.2.0`, poi `v0.2.1` e seguenti (D1b-8, chiude q8) |
| Priorità | La 1b vince sulla calibrazione | - | Se la 1b.2 non chiude entro il 10/1, la 1.3 aspetta |

## 11. Mappa dei file per ondata (sintesi)

| Area | 1.1 | 1.2 | 1.3 | 1.4 |
|---|---|---|---|---|
| `Directory.Packages.props`, `*.Tests.csproj` | B (righe di C e A su richiesta) | A | A | B |
| `DBEnigmistica.slnx` | - | A | - | - |
| `Dockerfile` | B | - | - | - |
| `.github/workflows/ci.yml` | B (lavoro Docker) | A (squawk, generate), C (regressione sintetica) | A (end-to-end) | - |
| `.github/workflows/publish.yml` | B (controllo librerie native) | - | - | - |
| `.github/scripts/**` | - | A | - | - |
| `scripts/**` | - | C (`regressione-locale.ps1`) | B (`misura-finestra.ps1`) | - |
| `src/DBEnigmistica.AI/**` | C | B (eliminazione di `PromptTemplates.cs` e `Models/Claude*Response.cs`) | - | B |
| `src/DBEnigmistica.Application/FornitoreAi/**` | C | - | - | - |
| `src/DBEnigmistica.Application/Services/**` | C (solo `using` in `Processing` e `Schema`) | B (`Processing`, `Export`) | - | - |
| `src/DBEnigmistica.Application/Acquisizione/**` | B (porte PDF e Archivio) | - | A, B (`Coda/**`) | - |
| `src/DBEnigmistica.Application/Valutazione/**` | - | C | - | - |
| `src/DBEnigmistica.Tipologie/**` | - | A (B: `Validatori/**`, `Descrittori/**`) | - | - |
| `tipologie/**` | A (`_meta/**`) | B (descrittori) | - | A (`_meta/prompt-*`) |
| `src/DBEnigmistica.Domain/Interfaces/**` | B (`IPdfService`), C (`IAiGameAnalyzer`) | - | - | - |
| `src/DBEnigmistica.Domain/Entities/**`, `Enums/**`, `Infrastructure/Data/**`, `Migrations/**`, `Repositories/**` | - | B | A (unica migrazione: `chiamate_ai`, coda) | - |
| `src/DBEnigmistica.Infrastructure/Services/**`, `Configuration/**` | B | A (`Tipologie/**`) | A (`Acquisizione/**`) | - |
| `src/DBEnigmistica.Console/**` | B (`CliApp.cs`, `HandlerRegistration.cs`, `diagnostica-render`), C (configurazione AI, `check-ai`, `CliHost.cs`, `HostRequirements.cs`) | A (`migrate`, `types`), C (`eval`) | A (`scan-file`), B (`scan-folder`, `costs`, riepiloghi) | - |
| `tests/**` | B (`Unit/Pdf`, `Unit/Archivio`, `Fixtures/PdfSintetici`), C (`Unit/FornitoreAi`, `Unit/Cli/FornitoreAi*`, `Architecture`, `Fixtures/ClaudeFinto`) | A (`Tipologie`), B (test di `Game`), C (`Valutazione`, `Fixtures/SetSintetico`) | A (`Acquisizione`, `EndToEnd`), B (`Coda`) | B (adattatori) |
| `.planning/phases/1/**` | A (`progetto-1.1.md`), C (`prova-claude-code-1.1.md`) | - | - | A (`analisi-1.4.md`) |
| `README.md`, `SETUP_INSTRUCTIONS.md`, `docs/CI.md`, `CLAUDE.md` | D (Haiku), dopo A-C | Haiku (`docs/CI.md`, checklist di release) | Haiku | Haiku (ADR del fornitore) |
| `ROADMAP.md`, `PROGRESS.md`, `docs/adr/**` | agente principale tramite Haiku | idem | idem | idem |

Nell'ondata 1.1 il conflitto possibile è uno solo: `Directory.Packages.props`. Lo tocca solo B, con le righe chieste da A e C (sezione 3, "Scambi tra agenti"). Nell'ondata 1.3 A e B toccano entrambi lo schema `riviste`: una sola migrazione EF, scritta da A, con le tabelle di B indicate da B nel rapporto.

## 12. Rischi del piano

1. **Claude Code non si presta all'uso da programma** (output non strutturato, immagini non leggibili in modalità non interattiva, finestra non riconoscibile). Segnale: prova della 1.1. Contromisura: prompt che chiede solo JSON con verifica lato programma. Se l'immagine non passa, si ferma la fase e si riporta al proprietario: l'alternativa è l'adattatore API, che è una sua decisione di spesa.
2. **PDFium nell'immagine chiseled** manca di librerie di sistema o rende male i font non incorporati. Segnale: passo CI della 1.1 e confronto dei PNG. Contromisura: nuovo ADR con il proprietario (ADR 009). In fase 1 l'estrazione gira comunque su Windows (ADR 010), quindi la 1.2 non si ferma.
3. **Collegamenti 157 e libreria nativa 160 non compatibili.** Segnale: test di rendering della 1.1. Contromisura: allineare le versioni e riportarlo al proprietario, perché le versioni sono fissate da D3.
4. **Condizioni dell'abbonamento non verificate entro il 6/12.** Già avvenuto in parte: la 1.1 si è chiusa il 20/11/2026 senza la prova, con l'approvazione del proprietario. Segnale ora: nessuna riga `1.1-prova` al 13/12/2026. Contromisura: l'agente principale porta la questione al proprietario (ADR 010, punto 7). Nessuna esecuzione dal vivo prima della prova.
5. **Rifattorizzazione del nucleo più larga del previsto** (1.2 B). Segnale: elenco dei punti d'uso oltre 30 file. Contromisura: dividere B in due consegne sequenziali nella stessa ondata.
6. **Contesa della Finestra dell'abbonamento** tra prova reale della 1.1, misura della 1.3, 1b e sviluppo. Contromisura: prove reali solo fuori dagli orari degli agenti e con limite di pagine.
7. **Set di riferimento in ritardo.** Segnale: niente PDF il 7/12. Contromisura: le ondate 1.1-1.3 procedono sul Set sintetico; la 1.4 slitta, e con lei la chiusura.
8. **Regressione locale dimenticata.** Contromisura: checklist di release in `docs/CI.md`; la verifica di fine fase legge il rapporto.
