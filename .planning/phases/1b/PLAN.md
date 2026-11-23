# Fase 1b - Piano delle ondate

Scritto il 20/11/2026 dall'agente di pianificazione (Opus). Aggiornato lo stesso giorno con le decisioni D1b-1 - D1b-9 del proprietario (`CONTEXT.md`, sezione 9; `docs/fase-1b-decisioni.md`). Contesto e vincoli: `CONTEXT.md` nella stessa cartella. Criteri di accettazione: `ROADMAP.md`, fase 1b, numeri 1-8. Le ondate 1b.2 e 1b.3 sono descritte a grandi linee; si dettagliano prima di partire. `.planning/` è tracciato da git.

## 0. Calendario e blocchi

Ordine secondo D1: 1.1 → **1b.1** → 1.2 → 1b.2 → 1.3 → 1.4 con 1b.3. Mai più di 3 agenti insieme. L'ondata 1.1 si è chiusa il 20/11/2026, prima del previsto. Il proprietario ha deciso il 20/11/2026: la 1b.1 parte il 23/11, le ondate seguenti si anticipano, le scadenze esterne non cambiano e il tempo guadagnato diventa margine. Le date della fase 1 qui sotto sono indicative: le fissa il piano della fase 1.

| Ordine | Ondata | Periodo previsto | Agenti | Può partire con `1.1-prova` aperta? |
|---|---|---|---|---|
| 2 | **1b.1** | 23/11 - 6/12/2026 | Opus A, Sonnet B, Sonnet C; poi Haiku D | **Sì.** Nessuna chiamata dal vivo: eseguibile finto di Claude Code e Registrazioni scritte dal progetto |
| 3 | 1.2 | chiusa il 23/11/2026 | (fase 1) | Sì |
| 4 | **1b.2** | 24/11/2026 - 7/12/2026; prototipo pronto verso il 10/1, scadenza 17/1 | Sonnet A, Sonnet B, Sonnet C; poi Haiku | **Codice sì, misura no.** La prima generazione dal vivo richiede `PHASE:1 WAVE:1.1-prova STATUS:complete` e regole approvate |
| 5 | 1.3 | dall'8/12/2026 | (fase 1) | In parte |
| 6 | **1b.3** | dal 25/1/2027 fino al 21/2, accanto alla 1.4 | Sonnet A | Richiede la prova chiusa: la prova 1 usa candidati generati dal vivo |
| - | Prova 1 | 18/1 - 21/2/2027 | proprietario | - |
| - | Decisione G1 | 28/2/2027 | proprietario | - |

Scadenze del proprietario che toccano la 1b: condizioni d'uso dell'abbonamento entro il 6/12/2026; regole v0 approvate e brief corretti entro il 3/1/2027 (`CONTEXT.md`, sezione 7).

Pre-release `v0.2.0-prova.1` a fine 1b.2; correzioni della 1b.3 come `v0.2.0-prova.N` (D6, D1b-8). Revisione Opus `xhigh` a fine di ogni ondata; quella di fine 1b.2 precede la prova 1.

## 1. Regole comuni a tutte le ondate

- Prima di ogni ondata: leggere `PROGRESS.md` e saltare le ondate con `STATUS:complete`. Verificare che esistano e non siano vuoti `ROADMAP.md`, `CLAUDE.md`, questo `PLAN.md` e `CONTEXT.md`.
- Un ramo per ondata, `fase-1b/ondata-1b.<n>`, creato da `main` aggiornato. Una PR verso `main` a fine ondata (D6). Nessun commit senza richiesta del proprietario.
- Ogni agente tocca solo i file del proprio elenco. Per un file di un altro agente scrive la modifica esatta nel rapporto finale. L'agente principale la inoltra.
- TDD: test rosso, poi codice. Copertura delle righe di Domain e Application almeno 80%.
- Prima di consegnare: `dotnet format DBEnigmistica.slnx --include <file propri>`, `dotnet build DBEnigmistica.slnx -c Release -warnaserror`, test della propria area con Docker attivo.
- Commenti di una frase. Termini del glossario di `CLAUDE.md` e di `docs/fase-1-decisioni.md`. Messaggi all'utente in italiano.
- Mai aprire `.env` né `src/DBEnigmistica.Console/appsettings.Development.json`.
- **Nessuna generazione dal vivo** prima della riga `PHASE:1 WAVE:1.1-prova STATUS:complete` in `PROGRESS.md`. Nessuna generazione dal vivo con regole in bozza.
- **Approvazione delle regole**: il proprietario approva in chat. L'agente scrive data e versione dell'approvazione nel campo `regole` del descrittore, in un commit dedicato che il proprietario chiede. Nessun comando nuovo.
- Il codice di creazione non legge mai `riviste`, nemmeno nei test (tranne il test delle stringhe sentinella, che le scrive per verificarne l'assenza).
- Pacchetti nuovi: licenza verificata (MIT, Apache-2.0, BSD o equivalente), versione esatta in `Directory.Packages.props`, nessuna vulnerabilità nota. Nessun pacchetto `Anthropic*`.
- Fine ondata: CI verde sulla PR (4 lavori), revisione Opus `xhigh` del diff, correzione di CRITICAL e HIGH, gitleaks, approvazione del proprietario, unione. Solo dopo, la riga in `PROGRESS.md`.

## 2. Scelte tecniche

Confermate dal proprietario il 20/11/2026, salvo le righe marcate [A], che l'agente principale conferma all'avvio della 1b.1.

| Tema | Scelta | Fonte |
|---|---|---|
| Porta di creazione | Porta separata `IFornitoreAiCreazione` in `Application/FornitoreAi`, accanto a quella di estrazione, con richiesta (Tipologia, versione, brief, prompt compilato, versione del prompt, JSON Schema) e risposta con i metadati della chiamata. Il contratto di `IFornitoreAiEstrazione` non cambia. Decoratori paralleli: limite per esecuzione, Registra, Riproduci, registro delle chiamate | Confermata 20/11/2026; ROADMAP 2.4 |
| Adattatore | Claude Code non interattivo, stesso avvio del processo dell'estrazione (`ArgumentList`, stdin, timeout, ambiente senza `ANTHROPIC_API_KEY`). Nessuna immagine e nessuno strumento abilitato. Opzioni provvisorie come in `prova-claude-code-1.1.md`; si allineano dopo la prova. Una parte comune con l'adattatore di estrazione si può estrarre solo se i test dell'estrazione restano invariati [A] | D5; ADR 010; D1b-1 |
| Modello | Opus per la creazione, con una chiave di configurazione separata, nuova. Il nome esatto della chiave lo propone B nella 1b.1 | D1b-6 |
| Chiamate di creazione | Riga `ChiamataAi` con `Uso = "creazione"` (valore nuovo). Al posto dell'impronta della pagina: SHA-256 del prompt compilato più lo schema [A]. Risposta grezza nello schema `prototipo`, cancellabile; mai in `riviste.chiamate_ai`. Nel registro evento `chiamata_ai` con i soli metadati e lo SHA-256 della risposta | Confermata 20/11/2026; D7; ADR 011; D1b-2 |
| Esito della revisione | Evento nel registro con i soli metadati dell'esito (candidato, Tipologia, esito, tempo attivo). Nessun testo del candidato. Nome dell'evento nel formato `^[a-z][a-z0-9_]{0,59}$`, proposto da A | D1b-2 |
| Registrazioni | Stesso formato della 1.1 (`progetto-1.1.md`, sezione 6.2), con fornitore, modello, versione del prompt e l'impronta della riga sopra. Nel repository solo Registrazioni di brief sintetici scritti dal progetto | ADR 008 |
| Descrittori v0 | File `tipologie/<id>/v0.json`. Il meta-schema ammette `versione` 0, che significa "prototipo". Il generatore SQL della 1.2 lo salta e non crea tabelle | Confermata 20/11/2026 |
| Rebus v0 | Famiglia `rebus`, resa `esposto_diagramma`: vignette come elenco di descrizioni testuali con le lettere, diagramma, soluzione. Nessun campo `immagine`. In fase 7 il rebus v1 passa a `immagine_diagramma` | Confermata 20/11/2026; criterio 8 |
| Crittografia breve v0 | Sottotipi sinonimica e perifrastica, come enumerazione nel descrittore e scelta nel brief | D1b-5 |
| Cruciverba | Il programma genera lo schema delle caselle nere (9x9-13x13, simmetrico, nessuna parola di una lettera, caselle bianche collegate). Il prompt lo passa all'AI, che riempie parole e definizioni | D1b-4 |
| Sudoku e giochi logici v0 | Famiglia `logico`, resa `griglia_logica` (`righe`, `colonne`, `griglia`). Segni del futoshiki e indizi del nonogramma come elenchi. Il seme del generatore si salva con il candidato [A] | ROADMAP 2.2 |
| Generatori | C# puro in Application, deterministici a parità di seme, nessun riferimento al Fornitore AI (test di architettura). Unicità verificata contando le soluzioni fino a 2 [A] | Criterio 7 |
| Schema `prototipo` | Una migrazione EF. Tabella dei candidati con: Tipologia e versione, brief, Origine `originale` (CHECK), modo (`ai` o `algoritmo`), seme, versione del prompt, posizione nell'ordine di generazione, risposta grezza e SHA-256 (cancellabili), dati generati, dati finali, differenze, tempo attivo, stato, esito, motivo dello scarto. I candidati non validi per lo schema si salvano con uno stato dedicato e si contano a parte; non entrano nella revisione. Nessuna colonna per immagini. Permessi: scrittura a `dbe_creazione`; nessun permesso a `dbe_estrazione`. Colonne esatte nel documento di progetto dell'agente A | Criteri 3 e 8; D1b-2; D1b-3 |
| Ordine dei candidati | La revisione mostra i candidati validi per Tipologia in ordine di generazione. Non esiste un'azione per sceglierli o saltarli | D1b-3 |
| Ruolo del database | I casi d'uso del prototipo lavorano con il ruolo `dbe_creazione`. Il modo (`SET ROLE` o stringa di connessione dedicata) lo propone B; lo approva l'agente principale [A] | Criterio 3; ADR 001 |
| Host web e avvio | Nuovo progetto `DBEnigmistica.Web` (ASP.NET Core, solo le rotte del prototipo). Comando nuovo `dbe prototipo` dell'eseguibile win-x64 della CI: avvia l'host con Kestrel legato in codice a `127.0.0.1`, non sovrascrivibile da configurazione. PostgreSQL resta nel Compose. L'immagine Docker non cambia | D1b-1; criterio 4 |
| Prompt di creazione | Costruito da: descrittore (nome, campi, ruoli), campo `regole` approvato, brief, e per il cruciverba lo schema delle caselle nere. Nient'altro. Versionato come il prompt di estrazione. Il compilatore rifiuta regole con `stato = bozza` fuori dai test | `CLAUDE.md` 5; ADR 002, punto 3 |

## 3. Ondata 1b.1 - Fondamenta del prototipo

**Perché ora.** Usa l'interfaccia del Fornitore AI e l'adattatore della 1.1. Fissa contratti, schema `prototipo` e generatori prima della 1b.2, che costruisce le schermate. Non richiede chiamate dal vivo.

**Ramo:** `fase-1b/ondata-1b.1`, da `main` aggiornato, dal 23/11/2026. Fine prevista il 6/12/2026.

**Come procede con `1.1-prova` aperta.** Nessuna chiamata a Claude Code vero, neppure una di prova. L'adattatore di creazione si prova con l'eseguibile finto (`tests/DBEnigmistica.Tests/Fixtures/ClaudeFinto/`) e con Registrazioni di brief sintetici scritte dal progetto. Le opzioni di Claude Code restano quelle provvisorie della 1.1. La qualità dei prompt non si misura in questa ondata: si misura nella 1b.2, dopo la riga `PHASE:1 WAVE:1.1-prova STATUS:complete` e con regole approvate. Sudoku e giochi logici non usano l'AI, quindi non hanno vincoli.

**Agenti.** A, B e C lavorano in parallelo (3 agenti). D parte dopo che A, B e C hanno consegnato. La revisione Opus `xhigh` chiude l'ondata.

| Agente | Modello | Compito | Motivo del modello |
|---|---|---|---|
| A | Opus | Progetto della creazione: contratti, brief, prompt, descrittori v0, metriche di revisione, bozze dei brief tipo | Ragionamento e prompt, nessun codice C# |
| B | Sonnet | Porta e adattatore di creazione, schema `prototipo`, host web, API, comando `dbe prototipo` | Codice |
| C | Sonnet | Generatori e risolutori di sudoku, futoshiki, nonogramma; schemi di caselle nere del cruciverba | Codice |
| D | Haiku | Documentazione, dopo A, B e C | Documenti da decisioni già prese |

**Scambi tra agenti, con date.**
- 25/11: A consegna a B il contratto provvisorio della porta di creazione, del brief, della tabella dei candidati e degli eventi nel registro.
- 25/11: A consegna a C il formato dei `dati` di sudoku, futoshiki, nonogramma e dello schema del cruciverba, con i livelli di difficoltà.
- 26/11: B aggiunge a `Directory.Packages.props` le righe chieste da C e da A, se servono.
- 2/12: A consegna il documento di progetto finale. B e C si allineano.
- 4/12: A consegna le bozze dei brief tipo e delle regole v0, da inviare al proprietario.
- 5-6/12: PR, CI, revisione Opus `xhigh`, correzioni.

### Agente A (Opus) - Progetto della creazione

File (solo documenti e dati, nessun codice C#):
- `.planning/phases/1b/progetto-1b.1.md` (nuovo);
- `.planning/phases/1b/brief-tipo.md` (nuovo, bozze dei brief per il proprietario);
- `tipologie/_meta/descrittore.schema.json` (solo il minimo di `versione`, da 1 a 0);
- `tipologie/_meta/brief.schema.json` (nuovo);
- `tipologie/_meta/prompt-creazione-v1.md` (nuovo);
- `tipologie/_meta/esempi/creazione/**` (nuovo);
- `tipologie/crittografia-breve/v0.json`, `tipologie/rebus/v0.json`, `tipologie/sudoku/v0.json`, `tipologie/futoshiki/v0.json`, `tipologie/nonogramma/v0.json` (nuovi; gli id sono proposte da confermare nel documento).

Compiti:
1. Contratto della porta di creazione: richiesta, risposta, esiti (gli stessi dell'estrazione), metadati, impronta usata per Registrazioni e chiamate.
2. Formato del brief (tema, vincoli, difficoltà, dimensioni, sottotipo della crittografia) come JSON Schema, con esempi validi e non validi.
3. Derivazione dello JSON Schema della risposta di creazione da un descrittore: un solo Gioco, senza riquadro né pagina, con le regole della sezione 3.2 di `progetto-1.1.md`.
4. Prompt di creazione v1 per crittografia breve (sinonimica e perifrastica), anagramma a tema, rebus con vignette descritte a parole, cruciverba con lo schema delle caselle nere fornito dal programma. Segnaposti solo per descrittore, regole approvate, brief e schema del cruciverba. Versione del prompt nella chiave delle Registrazioni.
5. Descrittori v0 delle cinque Tipologie nuove, con `regole` in bozza. Per cruciverba e anagramma il prompt si prova sui descrittori d'esempio della 1.1; i descrittori v1 arrivano con la 1.2.
6. Contratto della tabella dei candidati (sezione 2, riga "Schema `prototipo`"), stati del candidato (compreso "non valido per lo schema", contato a parte) ed eventi nel registro (chiamata di creazione, esito della revisione), solo metadati.
7. Definizione esatta delle misure per la 1b.2: tempo attivo (pause oltre 2 minuti escluse, ripresa dopo ricaricamento), mediana e quartili, quota accettata sui candidati validi in ordine di generazione, conteggio a parte dei non validi, avviso "misura indicativa" sotto i 10 candidati, confronto con le soglie G1.
8. Livelli di difficoltà di sudoku, futoshiki e nonogramma in funzione delle tecniche di risoluzione necessarie.
9. Bozze di 3-4 brief tipo per ciascuna delle quattro famiglie con AI, scritte dalle regole, da far correggere al proprietario (D1b-7).

Fatto quando:
- Il documento di progetto copre i punti 1-8 ed è approvato dall'agente principale. Nessuna decisione D1-D7 o D1b-1 - D1b-9 contraddetta.
- Gli schemi nuovi sono validi per JSON Schema 2020-12. Gli esempi validi li passano, quelli non validi li falliscono (stessa verifica meccanica della 1.1, sezione 9).
- I cinque descrittori v0 passano il meta-schema modificato. Il rebus v0 usa `esposto_diagramma` e non ha campi `immagine`.
- Il prompt non ha segnaposti per testi di Riviste o esempi di Giochi.
- Le bozze dei brief e delle regole v0 sono pronte per il proprietario.

### Agente B (Sonnet) - Porta di creazione, schema `prototipo`, host web

File:
- `src/DBEnigmistica.Application/FornitoreAi/IFornitoreAiCreazione.cs`, `RichiestaCreazione.cs`, `RispostaCreazione.cs`, `FornitoriAiCreazioneDecoratori.cs` (nuovi); `src/DBEnigmistica.Application/FornitoreAi/ChiamataAi.cs` (solo il valore `creazione` di `Uso`);
- `src/DBEnigmistica.Application/Creazione/**` (nuovo), tranne `Creazione/Algoritmi/**` di C;
- `src/DBEnigmistica.AI/**` (adattatore di creazione, chiave del modello di creazione e loro registrazione; l'adattatore di estrazione solo per estrarre una parte comune, con i suoi test invariati);
- `src/DBEnigmistica.Infrastructure/Data/**`, `src/DBEnigmistica.Infrastructure/Migrations/**` (una migrazione: schema `prototipo` e permessi), `src/DBEnigmistica.Infrastructure/Prototipo/**` (nuovo), `src/DBEnigmistica.Infrastructure/Configuration/DependencyInjection.cs` (solo i propri servizi);
- `src/DBEnigmistica.Web/**` (nuovo progetto);
- `src/DBEnigmistica.Console/Commands/PrototipoCommand.cs` (nuovo), `src/DBEnigmistica.Console/Cli/CliApp.cs` e `src/DBEnigmistica.Console/Commands/HandlerRegistration.cs` (solo la registrazione), `src/DBEnigmistica.Console/appsettings.json` (solo la chiave del modello di creazione);
- `.github/workflows/publish.yml` (solo il controllo che l'archivio `win-x64` contenga l'host del prototipo);
- `DBEnigmistica.slnx`, `Directory.Packages.props`, `tests/DBEnigmistica.Tests/DBEnigmistica.Tests.csproj` (unico proprietario nell'ondata);
- `tests/DBEnigmistica.Tests/Unit/Creazione/**`, `tests/DBEnigmistica.Tests/Integration/Prototipo/**`, `tests/DBEnigmistica.Tests/Architecture/**`, `tests/DBEnigmistica.Tests/Fixtures/ClaudeFinto/**` (estensione per la creazione), `tests/DBEnigmistica.Tests/Fixtures/RegistrazioniCreazione/**` (nuovo).

Compiti:
1. Porta `IFornitoreAiCreazione` e decoratori, con il contratto di A.
2. Adattatore Claude Code per la creazione (sezione 2): niente immagine, niente strumenti, `ANTHROPIC_API_KEY` tolta, retry, al massimo una richiesta di correzione, Finestra esaurita riconosciuta. Modello da una chiave di configurazione separata, nuova, con Opus come valore (D1b-6). `dbe check-ai` resta invariato.
3. Compilatore del prompt di creazione: descrittore, regole approvate, brief, schema del cruciverba da C. Rifiuta regole in bozza fuori dai test.
4. Migrazione dello schema `prototipo` secondo il contratto di A. Permessi ai ruoli. Nessuna colonna per immagini.
5. Casi d'uso: genera candidato (AI o algoritmo, tramite i generatori di C); un candidato non valido per lo schema si salva come tale, si conta a parte e si rigenera; leggi il prossimo candidato valido in ordine di generazione; salva versione finale con differenze; registra esito con motivo; cancella la risposta grezza. Eventi nel registro di soli metadati per la chiamata e per l'esito (D1b-2).
6. Host `DBEnigmistica.Web` con le API di generazione e salvataggio, in ascolto solo su `127.0.0.1`. Nessuna schermata: arrivano nella 1b.2.
7. Comando `dbe prototipo`: avvia l'host, usa la stringa di connessione già prevista (`ConnectionStrings__DefaultConnection`), termina con Ctrl+C. Codice 3 se la configurazione manca.
8. `publish.yml`: controllo che l'archivio `win-x64` contenga l'host del prototipo.
9. Test di architettura: `Web` dipende da Application e dagli adattatori solo tramite la composizione; `Creazione` non dipende da `Acquisizione` né da tipi dello schema `riviste`; `Creazione/Algoritmi` non dipende da `FornitoreAi`.

Test da scrivere prima: con l'eseguibile finto la generazione restituisce un candidato valido; risposta non conforme → una correzione, poi "risposta non valida", candidato contato a parte; Finestra esaurita → esito dedicato; `ANTHROPIC_API_KEY` assente nel figlio; modello di creazione letto dalla chiave separata; limite di 1 chiamata → seconda rifiutata con codice 4; Riproduci senza Registrazione → errore esplicito e zero chiamate; prompt con regole in bozza → rifiutato; **stringhe sentinella** scritte in `riviste` e assenti da ogni prompt compilato (criterio 3); `dbe_creazione` riceve un errore di permesso leggendo `riviste` (criterio 3); host in ascolto solo su `127.0.0.1` (criterio 4); lo schema `prototipo` non ha colonne per immagini (criterio 8); candidati serviti in ordine di generazione, senza salti; differenze di due campi salvate; eventi del registro senza testo del candidato; risposta grezza cancellabile e `verify-integrity` ancora a 0 dopo la cancellazione.

Fatto quando:
- Nessuna chiamata di rete nei test. Test dell'estrazione della 1.1 invariati e verdi.
- Migrazione applicata su Testcontainers e rivista nella PR.
- API provate con test d'integrazione: genera (finto), modifica, accetta, scarta.
- `dbe prototipo` avvia l'host su `127.0.0.1` in un test end-to-end; l'archivio `win-x64` della CI contiene l'host.
- Se il lavoro supera la stima, B consegna in due parti nella stessa ondata: prima porta, adattatore, schema e casi d'uso; poi host e comando.

### Agente C (Sonnet) - Generatori e risolutori

File:
- `src/DBEnigmistica.Application/Creazione/Algoritmi/**` (nuovo);
- `tests/DBEnigmistica.Tests/Unit/Algoritmi/**` (nuovo).

Compiti:
1. Sudoku 9x9: generatore con seme, risolutore che conta le soluzioni fino a 2, rimozione degli indizi finché la soluzione resta unica.
2. Futoshiki 5x5-7x7: stesso risolutore a vincoli più i segni di maggiore e minore; generatore con soluzione unica.
3. Nonogramma 10x10-15x15: generatore del disegno, calcolo degli indizi, risolutore per righe e colonne più ricerca, verifica dell'unicità.
4. Stima della difficoltà dalle tecniche di risoluzione necessarie, con i livelli definiti da A.
5. Conversione nei `dati` dei descrittori v0 di A. Il seme va con il candidato.
6. Generatore di schemi di caselle nere per il cruciverba 9x9-13x13 (simmetria, nessuna parola di una lettera, tutte le caselle bianche collegate, almeno 3 schemi su 5 di 11x11 o più se richiesti dal brief), con la numerazione delle definizioni (D1b-4).
7. Verifica della soluzione unica richiamabile dopo ogni modifica del revisore (serve alla 1b.2).

Test da scrivere prima: risolutori su griglie note con zero, una e più soluzioni; stesso seme → stessa griglia; **test di proprietà: 1.000 sudoku, 1.000 futoshiki e 1.000 nonogrammi generati hanno una e una sola soluzione** (criterio 7, con semi fissi registrati); i generatori non fanno chiamate al Fornitore AI (contatore dell'adattatore finto a 0, criterio 7, più il test di architettura di B); schemi del cruciverba validi su 1.000 semi.

Fatto quando:
- Test di proprietà verdi in CI. Il lavoro "Build e test" non cresce di oltre 3 minuti; altrimenti C ottimizza il risolutore prima di consegnare.
- Copertura di `Creazione/Algoritmi` almeno 80%.

### Agente D (Haiku) - Documentazione, dopo A, B e C

File: `SETUP_INSTRUCTIONS.md` (solo l'avvio del prototipo), `docs/CI.md` (solo il controllo nuovo di `publish.yml`).

Compiti: descrivere `dbe prototipo` sul PC Windows (PostgreSQL nel Compose, indirizzo `127.0.0.1`), la chiave del modello di creazione e il vincolo "nessuna generazione dal vivo prima della prova di Claude Code". Frasi brevi, nessun nome di classe interna.

Fatto quando: i file citano solo comandi, chiavi e comportamenti che esistono nel ramo, verificati dall'agente principale. `ROADMAP.md` e `CLAUDE.md` li aggiorna l'agente principale tramite Haiku, dopo l'unione.

### Criteri di uscita dell'ondata 1b.1

- CI verde (4 lavori) sulla PR di `fase-1b/ondata-1b.1`.
- Criteri 3, 4, 7 e 8 della fase 1b verdi in CI (vedi sezione 7).
- Nessuna chiamata dal vivo nell'ondata. Nessun file con testo di Riviste nel repository.
- Documento di progetto, schemi, prompt e descrittori v0 approvati dall'agente principale. Regole v0 e bozze dei brief inviate al proprietario (scadenza 3/1/2027).
- Modifica del meta-schema (versione 0, saltata dal generatore SQL) comunicata all'ondata 1.2.
- gitleaks pulito. Revisione Opus `xhigh` senza CRITICAL o HIGH aperti.
- Approvazione della PR da parte del proprietario e unione in `main`.
- Riga: `PHASE:1b WAVE:1b.1 STATUS:complete TS:<ISO-8601 UTC>`.

## 4. Ondata 1b.2 - Schermate, cronometro, misura (a grandi linee)

**Ramo:** `fase-1b/ondata-1b.2`, dal 24/11/2026 al 7/12/2026, dopo la 1.2. Prototipo pronto verso il 10/1/2027, scadenza 17/1 (invariate).
I passi dal vivo non sono in questa ondata: vanno nel compito `1b.2-dal-vivo`, con la sua riga in `PROGRESS.md` (D1b.2-2).

| Agente | Modello | Contenuto |
|---|---|---|
| A | Sonnet | Ossatura React e TypeScript con Vite servita dall'host; schermate "Nuovo candidato" e "Revisione" per crittografia, anagramma e cruciverba; cronometro del tempo attivo |
| B | Sonnet | Controlli leggeri visibili al revisore (validatori della 1.2); `dbe eval review-time` con i non validi contati a parte; Playwright; toolchain Node in `ci.yml` (lint, tipi, build, Vitest) |
| C | Sonnet | Schermate di revisione del rebus segnaposto e delle griglie di sudoku, futoshiki e nonogramma, con verifica della soluzione unica dopo ogni modifica |

Poi: Haiku scrive l'ADR 007, limitato all'host ASP.NET Core e all'ossatura React (D1b-9), e aggiorna la guida d'avvio; revisione Opus `xhigh` prima della prova 1.

Passi dal vivo, solo dopo `PHASE:1 WAVE:1.1-prova STATUS:complete` e con le regole approvate: una generazione controllata per famiglia con AI; poi i candidati della prova generati in anticipo con Opus, fuori dalle ore degli agenti, prima del 18/1/2027.

Uscita: criteri 1, 2 e 5 verdi in CI; collaudo del proprietario (circa 1,5 h); pre-release `v0.2.0-prova.1` dalla CI, senza spostare `latest`; riga `PHASE:1b WAVE:1b.2 STATUS:complete TS:<ISO-8601 UTC>`.

## 5. Ondata 1b.3 - Correzioni durante la prova 1 (a grandi linee)

**Ramo:** `fase-1b/ondata-1b.3`, dal 25/1/2027, accanto alla 1.4 (in totale 3 agenti).

- **Sonnet A**: correzioni dagli attriti segnalati nella prima settimana di prova. Nessuna modifica che cambi il modo di misurare (cronometro, stati, ordine dei candidati, calcolo di `review-time`) senza l'approvazione del proprietario, perché romperebbe il confronto tra candidati.
- Revisione Opus `xhigh` delle correzioni.
- Rilasci: `v0.2.0-prova.2`, `v0.2.0-prova.3` e seguenti fino a `v0.2.0`; dopo, `v0.2.1` e seguenti (D1b-8).
- Uscita: riga `PHASE:1b WAVE:1b.3 STATUS:complete TS:<ISO-8601 UTC>`.

## 6. Prova 1, verifica e chiusura

- **Prova 1** (proprietario, 18/1-21/2/2027): 5 candidati validi per famiglia, in ordine di generazione, a cronometro. Sudoku e giochi logici già nella prima metà di gennaio, se pronti.
- **Verifica** (Opus): criteri 1-8 dall'obiettivo; legge il rapporto `review-time` sui 30 candidati.
- **Rapporto G1** (JSON e leggibile) allegato a una release della 1b (criterio 6).
- **Decisione G1** del proprietario il 28/2/2027.
- Riga: `PHASE:1b WAVE:chiusura STATUS:complete TS:<ISO-8601 UTC>`, solo con l'approvazione del proprietario.

## 7. Tracciabilità dai criteri di accettazione alle ondate

| Criterio (ROADMAP, fase 1b) | Ondata e agente | Prova |
|---|---|---|
| 1. Playwright: generare (modello finto), modificare due campi, accettare; differenze e tempo attivo nel database | 1b.1 B (API, differenze, schema); 1b.2 A (schermate, cronometro), 1b.2 B (Playwright) | Test Playwright in CI |
| 2. Cronometro: pause oltre 2 minuti escluse; ripresa dopo ricaricamento | 1b.1 A (definizione); 1b.2 A | Test unitari con orologio simulato |
| 3. Stringhe sentinella in `riviste` assenti dai prompt; `dbe_creazione` con errore di permesso su `riviste` | 1b.1 A (prompt senza segnaposti per Riviste), 1b.1 B | Test d'integrazione su Testcontainers |
| 4. Server in ascolto solo su `127.0.0.1` | 1b.1 B (`dbe prototipo`, D1b-1) | Test d'integrazione sull'indirizzo di ascolto |
| 5. `dbe eval review-time` su 30 candidati sintetici dà i valori attesi | 1b.1 A (definizione delle misure); 1b.2 B | Test con candidati sintetici |
| 6. Proprietario: 5 candidati per famiglia entro il 21/2/2027; rapporto G1 allegato a una release della 1b | 1b.2 (pre-release), 1b.3 (correzioni), prova 1 | Rapporto allegato alla release |
| 7. 1.000 sudoku, futoshiki e nonogrammi con soluzione unica; 0 chiamate al Fornitore AI | 1b.1 C (generatori, test di proprietà), 1b.1 B (test di architettura) | Test di proprietà in CI |
| 8. Rebus solo con descrizioni testuali; nessun campo immagine nello schema `prototipo` | 1b.1 A (descrittore v0 con `esposto_diagramma`), 1b.1 B (schema); 1b.2 C (schermata) | Test sullo schema; revisione del descrittore |

## 8. Aggiornamenti della CI

| Ondata | Aggiunta | Dove |
|---|---|---|
| 1b.1 | Test di permessi e sentinelle, indirizzo di ascolto, test di proprietà dei generatori, avvio di `dbe prototipo` | "Build e test" (nessun lavoro nuovo) |
| 1b.1 | L'archivio `win-x64` contiene l'host del prototipo | `publish.yml`, lavoro "Eseguibile console" |
| 1b.2 | Toolchain Node: lint, controllo dei tipi, build dell'interfaccia, Vitest; Playwright sul prototipo | `ci.yml` |
| 1b.2 | Pre-release `v0.2.0-prova.1` con il rapporto `review-time` | `publish.yml` (nessun cambio: le pre-release non spostano `latest`) |

Nessun lavoro della CI usa l'abbonamento o chiama un Fornitore AI vero. L'immagine Docker non cambia nella fase 1b (D1b-1).

## 9. Mappa dei file per ondata (sintesi)

| Area | 1b.1 | 1b.2 | 1b.3 |
|---|---|---|---|
| `Directory.Packages.props`, `DBEnigmistica.slnx`, `*.Tests.csproj` | B | B | A |
| `tipologie/_meta/**`, `tipologie/<id>/v0.json` | A | - | - |
| `src/DBEnigmistica.Application/FornitoreAi/**` (file di creazione) | B | - | - |
| `src/DBEnigmistica.Application/Creazione/**` | B (tranne `Algoritmi/**`), C (`Algoritmi/**`) | B (controlli), C (verifica dopo le modifiche) | A |
| `src/DBEnigmistica.AI/**` | B | - | - |
| `src/DBEnigmistica.Infrastructure/Data/**`, `Migrations/**`, `Prototipo/**` | B | B (se serve) | A |
| `src/DBEnigmistica.Web/**` | B | A (ossatura, schermate a parole), C (schermate rebus e griglie) in cartelle separate | A |
| `src/DBEnigmistica.Console/**` | B (`dbe prototipo`, chiave del modello) | B (`review-time` in `EvalCommand.cs`) | - |
| `.github/workflows/**` | B (`publish.yml`, controllo dell'archivio) | B (`ci.yml`, Node e Playwright) | - |
| `tests/**` | B (`Creazione`, `Prototipo`, `Architecture`, `Fixtures`), C (`Algoritmi`) | A, B, C nelle proprie cartelle | A |
| `.planning/phases/1b/**` | A (`progetto-1b.1.md`, `brief-tipo.md`) | - | - |
| `docs/CI.md`, `SETUP_INSTRUCTIONS.md` | D (Haiku) | Haiku | Haiku |
| `docs/adr/007-*.md` | - | Haiku (D1b-9) | - |
| `ROADMAP.md`, `CLAUDE.md`, `PROGRESS.md`, `docs/adr/README.md` | agente principale tramite Haiku | idem | idem |

Conflitti con la fase 1: la migrazione EF e l'istantanea del modello (1b.1 B, poi 1.2 B), il meta-schema (1b.1 A, poi generatore della 1.2 A), `Directory.Packages.props`, `DBEnigmistica.slnx`, `ci.yml`. Le ondate sono sequenziali e ognuna parte da `main` aggiornato (D6).

## 10. Rischi del piano

1. **`1.1-prova` non chiusa entro il 13/12/2026** (condizioni d'uso entro il 6/12). La 1b.2 costruisce le schermate con l'eseguibile finto, ma i candidati della prova non si generano. Contromisura: l'agente principale porta la questione al proprietario prima del 21/12; sudoku e giochi logici procedono comunque.
2. **Regole o brief non pronti al 3/1/2027.** Nessuna generazione dal vivo con regole in bozza. Contromisura: promemoria al proprietario alla fine della 1b.1 e della 1.2; la revisione delle famiglie algoritmiche parte per prima.
3. **Agente B sovraccarico.** Segnale: il 2/12 porta, adattatore e schema non sono finiti. Contromisura: B consegna in due parti; l'host e il comando si spostano all'inizio della 1b.2, con l'avviso al proprietario.
4. **Agente C sovraccarico** (tre risolutori più gli schemi del cruciverba). Segnale: il 2/12 il nonogramma non passa i test di proprietà. Contromisura: gli schemi del cruciverba hanno la priorità (servono ai prompt della 1b.2); il nonogramma può chiudersi all'inizio della 1b.2.
5. **Test di proprietà lenti in CI.** Contromisura: risolutore per righe prima della ricerca; semi fissi; se serve, l'agente principale propone al proprietario un lavoro CI separato.
6. **Contesa della Finestra dell'abbonamento** tra prova di Claude Code, generazione dei candidati con Opus, misure della fase 1 e sviluppo. Contromisura: esecuzioni dal vivo solo fuori dalle ore degli agenti e con limite di chiamate.
