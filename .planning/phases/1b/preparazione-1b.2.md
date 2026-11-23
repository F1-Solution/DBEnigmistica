# Fase 1b, ondata 1b.2 - Preparazione all'avvio

Scritto il 23/11/2026 dall'agente di pianificazione (Opus), sul ramo `fase-1b/ondata-1b.2` creato da `main` al merge 1633bae (ondata 1.2 unita). Base: `PLAN.md` sezioni 4, 7, 8 e 9; `CONTEXT.md`; `progetto-1b.1.md` sezioni 4 e 8; `docs/fase-1b-decisioni.md`; `docs/ondata-1-2-decisioni-d7.md`; `preparazione-1.2.md`. Nessun codice scritto. Le proposte marcate [A] le conferma l'agente principale. Le domande per il proprietario sono nella sezione 6.

## 1. Prerequisiti

### 1.1 Presenti in `main` (verificati nel codice)

| Serve alla 1b.2 | Dove | Chi lo usa |
|---|---|---|
| Host del prototipo legato in codice a `127.0.0.1`, porta predefinita 5180 (`Prototipo:Porta`) | `src/DBEnigmistica.Web/Composizione/PrototipoHost.cs` (`CreateEmptyBuilder`, `Listen(IPAddress.Loopback, ...)`) | A |
| Filtro locale: `Host` solo `127.0.0.1` o `localhost`, `Origin` uguale all'indirizzo dell'host, `Sec-Fetch-Site: cross-site` rifiutato, corpo solo `application/json` | `PrototipoHost.cs`, metodo `SoloLocale` | A (le schermate devono passarlo) |
| API: `POST /api/prototipo/candidati`, `POST /tipologie/{id}/prossimo`, `PUT /candidati/{id}/finale`, `POST /candidati/{id}/accetta` e `/scarta`, `DELETE` delle risposte grezze, `GET /salute` | `src/DBEnigmistica.Web/Api/PrototipoEndpoints.cs`, `CandidatoJson.cs` | A, C |
| Ogni risposta con un candidato porta già l'elenco `controlli` | `PrototipoEndpoints.cs`, righe 27-29, 55-56, 62-63 | A, B, C |
| Controlli oltre lo schema: lettere della griglia contro le soluzioni del cruciverba; coerenza e soluzione unica dei giochi logici | `Application/Creazione/ControlliCruciverba.cs`, `ControlliLogici.cs`, `RevisioneCandidati.cs:173-195` | B, C |
| Tempo attivo e ultima attività accettati con *Accetta* e *Scarta*; il tempo non può diminuire | `RevisioneCandidati.cs:6-13, 94-95, 119-120` | A |
| Ordine di generazione senza salti, 409 su un candidato diverso dal prossimo | `RevisioneCandidati.cs`; `progetto-1b.1.md` 4.2 | A, C |
| Differenze tra generato e finale (JSON Pointer) | `Application/Creazione/Differenze.cs` | A (criterio 1) |
| Schema `prototipo`, stati, motivi dello scarto, eventi `chiamata_ai` ed `esito_revisione` | Migrazione della 1b.1; `Infrastructure/Prototipo/CandidatiRepository.cs`; `Application/Creazione/EventiCreazione.cs` | B (`review-time` legge i candidati) |
| Definizione di tempo attivo e del rapporto `review-time` (gruppi, quantili metodo 7, avvisi, forma JSON) | `progetto-1b.1.md` 8.1 e 8.2 | A (cronometro), B (rapporto) |
| Generatori e risolutori di sudoku, futoshiki, nonogramma; schemi del cruciverba; verifica dell'unicità | `Application/Creazione/Algoritmi/**` | C |
| Validatori strutturali della 1.2: `griglia-coerente-con-dimensioni`, `numerazione-coerente-con-griglia`, `lunghezze-coerenti`, `incroci-coerenti`, `diagramma-coerente-con-soluzione`, `anagramma-stesse-lettere`, `cambio-di-lettera`, `sciarada-parti-coerenti` | `src/DBEnigmistica.Tipologie/Validatori/**`, `CatalogoValidatori.Tutti`, interfaccia `Modello/IValidatoreStrutturale.cs` | B |
| Descrittori v1 di `cruciverba-classico` e `anagramma` (più `sciarada`, `cambio-di-lettera`) e v0 delle cinque Tipologie del prototipo | `tipologie/*/v1.json`, `tipologie/*/v0.json` | A, B, C |
| Interruttore `FornitoreAi:DalVivoAbilitato`, predefinito `false` | `AI/Configuration/FornitoreAiOptions.cs:30`; `AI/Configuration/DependencyInjection.cs:73, 118`; `Web/Composizione/AvvioPrototipo.cs:95` | B (vedi R6) |
| Comando `eval` con `extraction`, `draft` e `sintetico aggiorna-chiavi` | `Console/Commands/EvalCommand.cs` | B (aggiunge `review-time`) |
| Archivio `win-x64` controllato: contiene `DBEnigmistica.Web.dll` | `.github/workflows/publish.yml:175`; `ci.yml:94-101` | A (aggiunge l'interfaccia) |
| Test del prototipo da estendere | `tests/DBEnigmistica.Tests/Integration/Prototipo/**`, `Unit/Creazione/**`, `Architecture/PrototipoArchitetturaTests.cs` | A, B, C |

### 1.2 Mancanti (li crea la 1b.2)

- Nessun file Node nel repository: niente `package.json`, React, Vite, TypeScript, ESLint, Vitest. `.gitignore` non esclude `node_modules` né la cartella di build.
- L'host non serve file statici: nessun `UseStaticFiles`, nessun ritorno a `index.html`, nessuna intestazione `Content-Security-Policy`.
- Manca un'API per salvare il tempo attivo ogni 15 secondi (`progetto-1b.1.md` 8.1). Oggi il tempo arriva solo con *Accetta* e *Scarta*: un ricaricamento perde tutto il tempo precedente.
- Manca un'API che elenchi le Tipologie del prototipo e i campi del brief. La schermata "Nuovo candidato" non può costruire il modulo.
- I validatori della 1.2 non sono collegati alla revisione. `ControlliDopoLoSchema` non controlla anagramma, crittografia e rebus (`diagramma-coerente-con-soluzione`, `anagramma-stesse-lettere`), né lunghezze e numerazione del cruciverba.
- `dbe eval review-time`: non esiste.
- Playwright: nessun pacchetto in `Directory.Packages.props`, nessun browser in CI.
- `ci.yml` e `publish.yml` non hanno passi Node. `dependabot.yml` non ha l'ecosistema `npm`. Il lavoro "Vulnerabilita pacchetti" non controlla i pacchetti npm.
- ADR 007: non esiste (`docs/adr/README.md:26`).

### 1.3 Blocchi del proprietario

- **Nessuno blocca l'avvio.** La 1b.2 non fa chiamate dal vivo: modello finto, Registrazioni di brief sintetici, generatori algoritmici.
- **Tutte le regole sono in bozza** (`regole.stato = bozza` in tutti i nove descrittori). Il compilatore del prompt le rifiuta fuori dai test (`ModelloPromptCreazione.cs:70-71`). Nessuna generazione AI vera finché il proprietario non approva: le 4 regole v1 entro il 4/12 (D3 della 1.2), le 5 regole v0 entro il 3/1/2027. Sudoku e giochi logici non usano il prompt: non ne dipendono.
- **`1.1-prova`**: condizioni d'uso entro il 6/12, prova entro il 13/12. Senza la riga `PHASE:1 WAVE:1.1-prova STATUS:complete` nessun passo dal vivo (sezione 4, "Passi dal vivo").
- **Prova dello script locale** (27/11) e brief corretti (3/1): non servono alla 1b.2.

## 2. Riconciliazione

### 2.1 Con quello che 1b.1 e 1.2 hanno consegnato

| # | Punto | Proposta | |
|---|---|---|---|
| R1 | Due lettori dei descrittori: `Creazione/DescrittoreTipologia.cs`, letto dal file system (`Infrastructure/Prototipo/CatalogoTipologieFileSystem.cs`, file copiati da `Console.csproj:16`), e il caricatore di `DBEnigmistica.Tipologie` con meta-schema e risorse incorporate. La 1.2 (R1) rimandava l'unificazione alla 1b.2 | **Non si unificano nella 1b.2.** Si collegano solo i validatori (R2). Il rischio di toccare il caricamento a sei settimane dalla prova è più alto del beneficio. L'unificazione passa alla fase 2, che estende l'host. Il test della 1.2 che legge ogni `v*.json` con entrambi i lettori resta e protegge l'allineamento | [A] |
| R2 | "Controlli leggeri visibili al revisore (validatori della 1.2)" | B scrive in `Application/Creazione/` un adattatore che, dato il descrittore, carica il `Descrittore` di Tipologie dalle risorse incorporate (`RisorseTipologie`, `CaricatoreDescrittori`) ed esegue i validatori elencati nel campo `validatori`. I problemi diventano voci di `controlli`. Restano i controlli esistenti del cruciverba e dei giochi logici. Application referenzia già Tipologie (`Application.csproj:5`): nessun riferimento nuovo | [A] |
| R3 | I controlli leggeri non bloccano | Restano solo informazioni per il revisore, anche su *Accetta* per i validatori nuovi. Oggi *Accetta* rifiuta un contenuto con errori dei controlli esistenti (`ControllaContenuto` usa `Errori`, che include `ControlliDopoLoSchema`). I validatori nuovi entrano solo in `ControlliAsync`, non in `Errori`. Così non cambia il comportamento della 1b.1 e la misura non dipende da una regola di scarto (D1b-3, opzione B scartata) | [A] |
| R4 | Salvataggio periodico del tempo | Nuovo caso d'uso `SalvaTempoAsync(id, tempoAttivoMs, ultimaAttivitaIl)` in `RevisioneCandidati` (B) e nuova rotta `PUT /api/prototipo/candidati/{id}/tempo` (A). Stessa regola del tempo che non diminuisce; 409 se il candidato non è il prossimo | [A] |
| R5 | Modulo del brief | Nuova rotta `GET /api/prototipo/tipologie` (A) con id, versione, famiglia, modo (`ai` o `algoritmo`), stato delle regole e campi del brief ammessi per la famiglia. B espone il caso d'uso in `Creazione/`. La schermata mostra "regole in bozza" e disattiva *Genera* per le Tipologie AI con regole in bozza | [A] |
| R6 | Interruttore D8 e avvio dell'host. Con `DalVivoAbilitato = false` e senza Registrazioni in `riproduci`, `dbe prototipo` non parte affatto (`AvvioPrototipo.cs:95`). Così anche sudoku e giochi logici, che non usano l'AI, non si possono rivedere prima di `1.1-prova` | Decisa: Q3 = A (D1b.2-3) | [P] |
| R7 | ADR 007: il ROADMAP (tabella delle ondate 1b) lo assegna ad A, il `PLAN.md` (sezione 4) a Haiku | Lo scrive Haiku, come vuole `CLAUDE.md` per gli ADR da decisioni già prese. A consegna i fatti nel rapporto (versioni, cartelle, come l'host serve l'interfaccia) | [A] |
| R8 | `PLAN.md` sezione 8: "`publish.yml` (nessun cambio)" | Serve un cambio: l'archivio `win-x64` deve contenere l'interfaccia costruita. Si aggiunge la costruzione Node prima di `dotnet publish` e il controllo `test -f .../wwwroot/index.html`. Le pre-release non spostano `latest`: questo non cambia | [A] |
| R9 | Playwright: il ROADMAP, fase 2, prevede "Playwright per .NET, restano nella suite .NET" e axe con `Deque.AxeCore.Playwright` | Nella 1b.2 Playwright per .NET (`Microsoft.Playwright`) in `tests/DBEnigmistica.Tests/EndToEnd/Prototipo/`, solo Chromium. Niente axe: l'accessibilità non si applica al prototipo (`CONTEXT.md`, sezione 4) | [A] |
| R10 | Accessibilità | `CONTEXT.md`, sezione 4: i vincoli del Sito (18-20 px, tocco 44 px, axe-core) non si applicano al prototipo. Minimo di buon senso, senza criterio di uscita: testo di base 16 px, fuoco visibile, tutto usabile da tastiera, etichette sui campi. Nessuna scorciatoia da tastiera | [A] |
| R11 | Identità visiva | "Involucro Banco" è fuori dalla fase 1b (`CONTEXT.md`, sezione 2). Stile neutro con variabili CSS in un solo file, senza caratteri esterni né CDN. La fase 2 sostituisce le variabili con i token di `PROPOSTE-UI.md` | [A] |
| R12 | Scelte che la fase 2 eredita | Nessuna libreria di componenti (React Aria arriva con la fase 2). Client dell'API scritto a mano e tipizzato (il client generato dall'OpenAPI è della fase 2). Vitest con Testing Library, come nel ROADMAP, fase 2 | [A] |
| R13 | Dove sta l'interfaccia | Cartella `interfaccia/` nella radice, con il proprio `package.json`. La fase 2 la può trasformare in workspace accanto a `packages/lettore` senza spostarla | [A] |
| R14 | Come l'host trova l'interfaccia | `npm run build` scrive in `interfaccia/dist/`. `DBEnigmistica.Web.csproj` copia `interfaccia/dist/**` in `wwwroot/` dell'output e della pubblicazione. Nessun pacchetto nuovo (niente file incorporati). Con `-p:RichiediInterfaccia=true` (CI e `publish.yml`) la build fallisce se manca `index.html`. Senza, l'host risponde su `/` con una pagina che dice di eseguire `npm run build`. L'immagine Docker non cambia: `IncludePrototipo=false` esclude già Web | [A] |
| R15 | Sicurezza delle pagine servite | `Content-Security-Policy: default-src 'self'; frame-ancestors 'none'`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`. Il filtro `SoloLocale` non si allenta. In sviluppo il proxy di Vite toglie l'intestazione `Origin` verso l'host, così il filtro resta invariato | [A] |
| R16 | Toolchain e pacchetti npm | npm con `package-lock.json` tracciato e `npm ci` in CI. Node LTS 24, fissato in `.nvmrc` e in `actions/setup-node` per SHA. Versioni esatte in `package.json` (nessun `^` né `~`), ultima stabile pubblicata da almeno due settimane. Licenze MIT, Apache-2.0, BSD o ISC. `npm audit --audit-level=high` nel lavoro "Vulnerabilita pacchetti". Ecosistema `npm` in `dependabot.yml` | [A] |
| R17 | Lavori della CI | Restano 4 lavori. I passi Node (lint, tipi, Vitest, build) vanno in "Build e test" prima della compilazione .NET; Playwright gira con gli altri test. Se il lavoro supera i 20 minuti di `timeout-minutes`, B lo porta a 30 e lo segnala | [A] |
| R18 | Rapporto `review-time` e pre-release | `dbe eval review-time --output <cartella>` scrive JSON e Markdown. In CI gira sui 30 candidati sintetici (criterio 5) e il rapporto si pubblica come artefatto. Il rapporto della prova vera lo allega il proprietario alla release (handoff 13), come per la regressione locale | [A] |
| R19 | Ruolo del database di `review-time` | Solo lettura dello schema `prototipo`, con la stessa stringa di connessione e il ruolo `dbe_creazione` usato dall'host (`RuoloCreazioneInterceptor`) | [A] |

### 2.2 Proprietà dei file condivisi

| File | Proprietario | Gli altri |
|---|---|---|
| `DBEnigmistica.slnx`, `Directory.Packages.props`, `tests/DBEnigmistica.Tests/DBEnigmistica.Tests.csproj` | B (`PLAN.md`, sezione 9) | Chiedono le righe nel rapporto |
| `src/DBEnigmistica.Web/**` (`.csproj`, `Composizione/**`, `Api/**`) | A | C non tocca Web in C#; chiede le rotte ad A |
| `src/DBEnigmistica.Application/Creazione/**` (tranne `Algoritmi/**`) | B | A chiede i casi d'uso di R4 e R5 a B |
| `src/DBEnigmistica.Application/Creazione/Algoritmi/**` | C | - |
| `interfaccia/` configurazione (`package.json`, `package-lock.json`, `vite.config.ts`, `tsconfig*.json`, `eslint.config.js`, `.nvmrc`, `index.html`) | A | C chiede le dipendenze ad A |
| `interfaccia/src/comune/**` (client dell'API, cronometro, campi, layout, instradamento) | A | C usa, chiede modifiche ad A |
| `interfaccia/src/schermate/parole/**` (crittografia, anagramma, cruciverba, "Nuovo candidato") | A | - |
| `interfaccia/src/schermate/griglie/**`, `interfaccia/src/schermate/rebus/**` | C | - |
| `.github/workflows/ci.yml`, `publish.yml`, `.github/dependabot.yml` | B | A consegna il testo dei passi Node entro il 26/11 |
| `Console/Commands/EvalCommand.cs` | B | - |
| `.gitignore` | A (solo `node_modules/` e `interfaccia/dist/`) | - |

## 3. Analisi d'impatto

Non c'è codice vecchio da riscrivere. I punti che cambiano comportamento esistente:

| File | Righe | Modifica | Agente |
|---|---|---|---|
| `src/DBEnigmistica.Application/Creazione/RevisioneCandidati.cs` | 173-195 | `ControlliAsync` aggiunge i validatori della 1.2 (R2); `Errori` no (R3). Nuovo `SalvaTempoAsync` (R4) | B |
| `src/DBEnigmistica.Web/Composizione/PrototipoHost.cs` | 33-57 | File statici da `wwwroot`, ritorno a `index.html` fuori da `/api`, intestazioni di R15 | A |
| `src/DBEnigmistica.Web/Api/PrototipoEndpoints.cs` | 18-90 | Rotte `PUT .../tempo` e `GET /tipologie` | A |
| `src/DBEnigmistica.Web/DBEnigmistica.Web.csproj` | 1-22 | Copia di `interfaccia/dist/**` e controllo `RichiediInterfaccia` (R14) | A |
| `src/DBEnigmistica.Web/Composizione/AvvioPrototipo.cs` | 83-103 | Solo se Q3 = A: l'interruttore spento non blocca più l'avvio | B (vedi brief) |
| `src/DBEnigmistica.AI/Configuration/DependencyInjection.cs` | 116-119 | Solo se Q3 = A: con l'interruttore spento la creazione AI si registra come fornitore che rifiuta, non come errore all'avvio | B |
| `src/DBEnigmistica.Console/Commands/EvalCommand.cs` | - | Sottocomando `review-time` | B |
| `.github/workflows/ci.yml` | 25-150, 207-248 | Passi Node, Playwright, `npm audit`, `RichiediInterfaccia=true` nella pubblicazione di controllo | B |
| `.github/workflows/publish.yml` | 144-180 | Node prima di `dotnet publish`; controllo di `wwwroot/index.html` | B |
| Test esistenti toccati | `Integration/Prototipo/ApiPrototipoTests.cs`, `PrototipoEndToEndTests.cs`, `Unit/Creazione/InterruttoreDalVivoPrototipoTests.cs` (se Q3 = A) | Estendere, non riscrivere | A, B |

`AvvioPrototipo.cs` è in Web ma lo modifica B, perché il cambio segue l'interruttore: A non lo tocca. È l'unica eccezione alla tabella 2.2.

## 4. Brief degli agenti

Regole comuni, valide per tutti (`CLAUDE.md`, `PLAN.md` sezione 1):
- Ramo `fase-1b/ondata-1b.2`. Nessun commit senza richiesta del proprietario.
- TDD: prima il test rosso, poi il codice. Copertura delle righe di Domain e Application almeno 80%.
- Prima di consegnare: `dotnet format DBEnigmistica.slnx --include <file propri>`, `dotnet build DBEnigmistica.slnx -c Release -warnaserror`, test della propria area con Docker attivo. Per l'interfaccia: `npm run lint`, `npm run typecheck`, `npm test`, `npm run build` in `interfaccia/`.
- **Nessuna chiamata AI dal vivo.** Solo eseguibile finto, Registrazioni di brief sintetici e generatori algoritmici. `DalVivoAbilitato` resta `false` in ogni test e in CI.
- **Nessun testo di Riviste** nel repository o nei test. Solo testi inventati dal progetto.
- Nessuna modifica che cambi il modo di misurare oltre a `progetto-1b.1.md` 8.1 e 8.2 senza l'approvazione dell'agente principale.
- Mai aprire `.env` né `appsettings.Development.json`. Nessun segreto nei file.
- Ogni agente tocca solo i propri file (sezione 2.2). Per un file altrui scrive la modifica esatta nel rapporto.
- Commenti di una frase. Termini del glossario. Testi a schermo in italiano.

### Agente A (Sonnet) - Ossatura, schermate dei giochi a parole, cronometro

**Obiettivo.** Il prototipo si apre nel browser da `dbe prototipo`, genera e fa rivedere crittografia breve, anagramma e cruciverba con il cronometro del tempo attivo.

**File esclusivi.**
- `interfaccia/**`, tranne `interfaccia/src/schermate/griglie/**` e `interfaccia/src/schermate/rebus/**` (di C).
- `src/DBEnigmistica.Web/**`, tranne `Composizione/AvvioPrototipo.cs` (di B, sezione 3).
- `.gitignore` (solo le due righe di R13-R14).
- `tests/DBEnigmistica.Tests/Integration/Prototipo/ApiPrototipoTests.cs` (solo i casi nuovi), `Integration/Prototipo/HostInterfacciaTests.cs` (nuovo).

**Compiti.**
1. Entro il 25/11: ossatura Vite con React e TypeScript, ESLint, Vitest e Testing Library (R12, R13, R16). Proxy di sviluppo verso `127.0.0.1:5180` senza `Origin` (R15).
2. Entro il 25/11: host che serve `wwwroot` con le intestazioni di R15 e il ritorno a `index.html` (R14). Le rotte `/api/**` non cambiano.
3. Entro il 26/11: testo esatto dei passi Node per `ci.yml` e `publish.yml`, consegnato a B. Componenti comuni (client dell'API, campi modificabili, elenco dei `controlli`, pulsanti *Accetta* e *Scarta* con motivo e nota, cronometro) consegnati a C.
4. Rotte `GET /api/prototipo/tipologie` e `PUT /api/prototipo/candidati/{id}/tempo`, sopra i casi d'uso di B (R4, R5), consegnati da B il 26/11.
5. Schermata "Nuovo candidato": Tipologia, brief (tema, vincoli, difficoltà, dimensioni, sottotipo, variante), *Genera*. Mostra i candidati non validi e il messaggio dopo 3 tentativi (`progetto-1b.1.md` 4.2). Per le regole in bozza mostra l'avviso e disattiva *Genera* (R5).
6. Schermata "Revisione" per crittografia breve, anagramma e cruciverba: il prossimo candidato della Tipologia, campi modificabili, griglia del cruciverba con lo schema delle caselle fisso, definizioni, `controlli` aggiornati a ogni salvataggio, *Accetta*, *Scarta* con motivo dalla lista chiusa e nota.
7. Cronometro come funzione pura degli istanti (`progetto-1b.1.md` 8.1): eventi elencati, `pointermove` al massimo ogni 5 secondi, invio ogni 15 secondi se cambiato e sempre con l'esito, ripresa dopo ricaricamento dai valori salvati.
8. Rapporto per l'ADR 007 (R7): versioni, cartelle, come l'host serve l'interfaccia, cosa eredita la fase 2.

**Test da scrivere prima.**
- Vitest, con orologio simulato: l'esempio di 8.1 dà 160 secondi; 120.000 ms contano; 120.001 ms contano 0; un intervallo negativo conta 0; ripresa dopo ricaricamento (criterio 2).
- Vitest: invio del tempo ogni 15 secondi solo se cambiato; nessun invio dopo l'esito.
- .NET: l'host serve `index.html` su `/` e su un percorso dell'interfaccia; `/api/...` sconosciuto dà 404, non `index.html`; le intestazioni di R15 ci sono; una richiesta con `Origin` estraneo è ancora rifiutata.
- .NET: `PUT .../tempo` salva; un tempo minore dà 422; un candidato diverso dal prossimo dà 409.

**Fatto quando.** Criterio 2 verde in CI. Le schermate a parole funzionano con il modello finto. Nessuna chiamata a domini esterni dalle pagine (CSP).

### Agente B (Sonnet) - Controlli leggeri, `review-time`, Playwright, CI

**Obiettivo.** Il revisore vede i controlli della 1.2. La misura della prova si calcola. Il flusso completo è provato in CI.

**File esclusivi.**
- `src/DBEnigmistica.Application/Creazione/**`, tranne `Algoritmi/**`.
- `src/DBEnigmistica.Application/Valutazione/TempiRevisione/**` (nuovo), `src/DBEnigmistica.Console/Commands/EvalCommand.cs`, `Console/Cli/CliApp.cs` e `Commands/HandlerRegistration.cs` (solo se serve la registrazione).
- `src/DBEnigmistica.Infrastructure/Prototipo/**` (lettura per `review-time`).
- `src/DBEnigmistica.Web/Composizione/AvvioPrototipo.cs` e `src/DBEnigmistica.AI/Configuration/**`, solo se Q3 = A.
- `DBEnigmistica.slnx`, `Directory.Packages.props`, `tests/DBEnigmistica.Tests/DBEnigmistica.Tests.csproj`.
- `.github/workflows/ci.yml`, `.github/workflows/publish.yml`, `.github/dependabot.yml`, `.github/scripts/**`.
- `tests/DBEnigmistica.Tests/Unit/Creazione/**`, `Unit/Valutazione/TempiRevisione/**`, `Integration/Prototipo/PrototipoIntegrationTests.cs`, `EndToEnd/Prototipo/**` (nuovo), `Fixtures/CandidatiSintetici/**` (nuovo).

**Compiti.**
1. Entro il 26/11: casi d'uso di R4 (`SalvaTempoAsync`) e R5 (elenco delle Tipologie con i campi del brief), consegnati ad A.
2. Controlli leggeri (R2, R3): adattatore verso i validatori della 1.2, problemi con percorso e messaggio in italiano, solo in `ControlliAsync`.
3. Q3, secondo la risposta del proprietario.
4. `dbe eval review-time [--output <cartella>]`: gruppi, conteggi, quantili, avvisi, soglie e forma di `progetto-1b.1.md` 8.2. JSON e Markdown deterministici. Codice 0 anche con soglie mancate: il rapporto confronta, non decide.
5. 30 candidati sintetici (criterio 5) con i casi minimi di 8.2: un gruppo con 1 rivisto, uno con 0, una pausa oltre 2 minuti, un non valido, un errore di generazione, una mediana con interpolazione. Valori attesi calcolati a mano nel test.
6. Playwright per .NET (R9): generare con il modello finto, modificare due campi, accettare; nel database le differenze dei due campi e un tempo attivo coerente con l'orologio simulato (criterio 1). Orologio del browser simulato con l'API `Clock` di Playwright.
7. CI (R16, R17, R18): passi Node di A, `RichiediInterfaccia=true`, installazione di Chromium con le dipendenze, `npm audit`, rapporto `review-time` sintetico come artefatto, ecosistema `npm` in Dependabot. `publish.yml` come R8.

**Test da scrivere prima.**
- Un anagramma con lettere diverse dà un controllo; uno corretto no. Una crittografia con diagramma incoerente dà un controllo. Un controllo nuovo non impedisce *Accetta*.
- `review-time` sui 30 candidati sintetici dà i valori attesi; due esecuzioni danno lo stesso JSON salvo `generato_il`; un gruppo senza rivisti dà `null`, non 0.
- Il test Playwright del criterio 1.
- Se Q3 = A: con l'interruttore spento l'host parte, sudoku si genera, una Tipologia AI risponde con il messaggio di D8 e zero chiamate.

**Fatto quando.** Criteri 1 e 5 verdi in CI. `publish.yml` produce un archivio `win-x64` con `wwwroot/index.html` (provato con `workflow_dispatch` sul ramo, se il proprietario lo consente, altrimenti con il passo equivalente in `ci.yml`). I 4 lavori della CI verdi.

### Agente C (Sonnet) - Schermate di griglie e rebus

**Obiettivo.** Sudoku, futoshiki, nonogramma e rebus si generano e si rivedono, con la soluzione unica ricontrollata dopo ogni modifica.

**File esclusivi.**
- `interfaccia/src/schermate/griglie/**`, `interfaccia/src/schermate/rebus/**` (con i propri test Vitest accanto).
- `src/DBEnigmistica.Application/Creazione/Algoritmi/**` (solo se serve per la verifica dopo le modifiche).
- `tests/DBEnigmistica.Tests/Unit/Algoritmi/**`.

**Compiti.**
1. Revisione di sudoku 9x9, futoshiki 5x5-7x7 (segni tra caselle), nonogramma 10x10-15x15 (indizi ai margini). Formati di `progetto-1b.1.md` 5.1-5.3.
2. Dopo ogni modifica salva con `PUT .../finale` e mostra i `controlli` restituiti (coerenza e soluzione unica). Nessun risolutore nel browser: la verifica resta nel server.
3. "Nuovo candidato" per i giochi logici: difficoltà e dimensioni, senza tema (il brief lo vieta).
4. Rebus: elenco delle vignette descritte a parole con lettere e parola, esposto, diagramma, soluzione. Nessun campo e nessun controllo per immagini (criterio 8).
5. Usa i componenti comuni di A, compreso il cronometro. Ogni modifica chiesta ai componenti passa dal rapporto.

**Test da scrivere prima.**
- Vitest: la griglia si modifica da tastiera e manda il contenuto nella forma del descrittore; un futoshiki con un segno cambiato mostra il controllo restituito; il rebus non ha elementi `img` né campi file.
- .NET, se C tocca `Algoritmi/**`: la verifica di un sudoku modificato con due soluzioni dà il controllo atteso entro 200 ms.

**Fatto quando.** Le schermate funzionano con i generatori veri. Il lavoro "Build e test" non cresce di oltre 3 minuti per i test di C.

### Agente D (Haiku) - Documentazione, dopo A, B e C (4/12)

**File esclusivi.** `docs/adr/007-host-web-e-ossatura-react.md` (nuovo; solo host ASP.NET Core e ossatura React, D1b-9; i fatti dal rapporto di A), `SETUP_INSTRUCTIONS.md` (avvio del prototipo, interfaccia nel browser), `docs/CI.md` (passi Node, Playwright, `npm audit`, controllo dell'archivio), `docs/handoff/07-collaudo-prototipo.md` (passi del collaudo, sezione 5), `README.md` (`eval review-time`). L'indice `docs/adr/README.md`, `ROADMAP.md`, `CLAUDE.md` e `PROGRESS.md` li aggiorna l'agente principale tramite Haiku, dopo l'unione.

**Compiti.** Descrivere solo comandi, chiavi, rotte e comportamenti che esistono nel ramo. Frasi brevi, nessun nome di classe interna.

**Fatto quando.** L'agente principale verifica ogni comando citato contro il ramo.

### Revisione finale (Opus `xhigh`, 5/12)

Sul diff completo, con attenzione a:
- filtro `SoloLocale` invariato e CSP effettiva;
- nessuna risorsa esterna nelle pagine e nessun testo di Riviste;
- cronometro e salvataggio del tempo (monotonia, 409, ripresa);
- `review-time` (quantili, `null`, determinismo);
- controlli leggeri senza effetto sull'esito (R3);
- interruttore D8 dopo Q3;
- pacchetti npm (licenze, versioni esatte, `npm audit`) e passi CI fissati per SHA.

Poi gitleaks. Vanno corretti tutti i CRITICAL e gli HIGH.

### Passi dal vivo (fuori dall'ondata, vedi Q2)

Solo dopo `PHASE:1 WAVE:1.1-prova STATUS:complete` e con le regole della Tipologia approvate:
1. Una generazione controllata per famiglia con AI, con `DalVivoAbilitato = true`, limite di 1 chiamata, fuori dalle ore degli agenti.
2. I candidati della prova generati in anticipo con Opus, prima del 18/1/2027: anagramma e cruciverba dopo il 4/12, crittografia breve e rebus dopo il 3/1.

### Criteri di uscita dell'ondata 1b.2

- CI verde (4 lavori) sulla PR di `fase-1b/ondata-1b.2`.
- Criteri 1, 2 e 5 della fase 1b verdi in CI. I criteri 3, 4, 7 e 8 restano verdi.
- Nessuna chiamata dal vivo nell'ondata.
- ADR 007 scritto. gitleaks pulito. Revisione Opus `xhigh` senza CRITICAL o HIGH aperti.
- Collaudo del proprietario (circa 1,5 h, vedi Q4) e approvazione della PR. Unione in `main`.
- Pre-release `v0.2.0-prova.1` dalla CI, senza spostare `latest` (vedi Q4).
- Riga: `PHASE:1b WAVE:1b.2 STATUS:complete TS:<ISO-8601 UTC>`.

## 5. Date

La 1.2 si è chiusa il 23/11/2026 (`PROGRESS.md`), due settimane prima del previsto (6/12). Il `PLAN.md` dava la 1b.2 dal 21/12. La proposta segue lo stesso schema di due settimane della 1.2. Le scadenze esterne non cambiano.

| Data | Evento |
|---|---|
| 24/11 | Avvio di A, B, C |
| 25/11 | A: ossatura e host che serve l'interfaccia |
| 26/11 | A consegna i passi Node a B e i componenti comuni a C. B consegna i casi d'uso di tempo e Tipologie ad A |
| 27/11 | Proprietario: prova dello script locale (1.2, D6). Regole v1 in bozza già inviate |
| 2/12 | B: `review-time` e Playwright verdi. C: griglie e rebus |
| 3/12 | Fine di A, B, C |
| 4/12 | Haiku D. PR e CI. Proprietario: approvazione delle 4 regole v1 |
| 5/12 | Revisione Opus `xhigh` e correzioni |
| 6/12 | Proprietario: verifica delle condizioni d'uso |
| 7/12 | Collaudo del proprietario (circa 1,5 h), approvazione, unione, pre-release, riga in `PROGRESS.md` |

Proposta (Q1): **1b.2 dal 24/11 al 7/12/2026.** Seguono, come indicazione: 1.3 dall'8/12 al 21/12; `1.1-prova` entro il 13/12; passi dal vivo dal 14/12 per anagramma e cruciverba, dal 4/1/2027 per crittografia e rebus; candidati della prova pronti entro il 17/1; 1.4 e 1b.3 dal 18/1 (la 1b.3 dal 25/1, invariata).

Rischio: la 1b.2 ha tre consegne incrociate il 26/11. Se A slitta, C slitta di altrettanto. Contromisura: C parte dai test Vitest e dai formati, che non dipendono dai componenti.

Scadenze esterne invariate:
- condizioni d'uso: 6/12;
- `1.1-prova`: 13/12;
- regole v0 e brief: 3/1;
- prototipo: verso il 10/1, scadenza 17/1;
- prova 1: dal 18/1 al 21/2;
- G1: 28/2/2027.

Il tempo guadagnato resta margine, soprattutto per i passi dal vivo.

## 6. Domande per il proprietario

**Q1. Date della 1b.2.** *Decisa: A (D1b.2-1).* Il piano la metteva dal 21/12/2026 al 3/1/2027. La 1.2 si è chiusa il 23/11.
- A. Dal 24/11 al 7/12/2026, due settimane come la 1.2. Poi la 1.3 dall'8/12.
- B. Dal 24/11 al 14/12/2026, tre settimane, con più margine sulle schermate.
- C. Date del piano (21/12-3/1), con la 1.3 prima.

Raccomandazione: A. La 1b.2 non dipende da chiamate dal vivo, e il margine serve dopo, ai passi dal vivo prima del 17/1.

**Q2. Passi dal vivo e chiusura dell'ondata.** *Decisa: A (D1b.2-2).* La generazione controllata per famiglia richiede `1.1-prova` chiusa e regole approvate, che arrivano dopo il 7/12 (fino al 3/1 per crittografia e rebus).
- A. L'ondata si chiude senza i passi dal vivo. Diventano un compito a parte, `1b.2-dal-vivo`, con la sua riga in `PROGRESS.md`, come `1.1-prova`.
- B. L'ondata resta aperta finché non è fatta almeno una generazione dal vivo per famiglia.
- C. Nessuna generazione controllata: si generano subito i candidati della prova.

Raccomandazione: A. Separa il codice, pronto il 7/12, dalle attese esterne; il controllo per famiglia evita di sprecare candidati della prova.

**Q3. Interruttore delle chiamate dal vivo e avvio del prototipo.** *Decisa: A (D1b.2-3).* Con `FornitoreAi:DalVivoAbilitato = false` oggi `dbe prototipo` non parte affatto. Così non si possono rivedere neanche sudoku e giochi logici, che non usano l'AI, prima di `1.1-prova`.
- A. L'host parte sempre. Con l'interruttore spento solo *Genera* per crittografia, anagramma, rebus e cruciverba si rifiuta, con il messaggio di D8 e zero chiamate. Sudoku, giochi logici e revisione funzionano.
- B. Resta com'è. Per usare il prototipo prima di `1.1-prova` si avvia con le Registrazioni in modalità riproduci.
- C. Un secondo interruttore solo per il prototipo.

Raccomandazione: A. Rispetta lo scopo di D8 (nessuna chiamata dal vivo) e permette la revisione anticipata di sudoku e giochi logici prevista dal ROADMAP.

**Q4. Collaudo e pre-release `v0.2.0-prova.1`.** *Decisa: A (D1b.2-4).* Il collaudo (circa 1,5 h) è un criterio di uscita. Al 7/12 le famiglie con AI non si possono generare dal vivo.
- A. Collaudo il 7/12 su sudoku, futoshiki e nonogramma veri e sulle famiglie con AI riprodotte dalle Registrazioni sintetiche. Pre-release subito dopo l'unione. Le correzioni dopo i passi dal vivo escono come `v0.2.0-prova.2`.
- B. Collaudo e pre-release dopo i passi dal vivo, verso il 4-10/1/2027.
- C. Due collaudi: uno breve il 7/12 senza pre-release, uno completo a gennaio con la pre-release.

Raccomandazione: A. La numerazione `v0.2.0-prova.N` (D1b-8) regge le correzioni, e a gennaio il tempo del proprietario va ai brief e alle regole.

## 7. Decisioni del proprietario (23/11/2026)

Tutte e quattro con l'opzione A. Dettagli e opzioni scartate in [`docs/ondata-1b-2-decisioni.md`](../../../docs/ondata-1b-2-decisioni.md).

- **D1b.2-1.** 1b.2 dal 24/11 al 7/12/2026; 1.3 dall'8/12. Le scadenze esterne restano.
- **D1b.2-2.** Passi dal vivo fuori dall'ondata, nel compito `1b.2-dal-vivo` con la sua riga in `PROGRESS.md`.
- **D1b.2-3.** `dbe prototipo` parte sempre; con l'interruttore spento rifiuta solo le *Genera* con AI, senza chiamate.
- **D1b.2-4.** Collaudo del proprietario il 7/12 su sudoku, futoshiki, nonogramma e famiglie con AI riprodotte; pre-release `v0.2.0-prova.1` dalla CI.
