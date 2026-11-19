# Integrazione continua e pubblicazione

Test e pubblicazione avvengono solo tramite GitHub Actions. Non serve alcun segreto aggiuntivo: i flussi usano il `GITHUB_TOKEN` automatico. Nessuna immagine e nessun eseguibile si pubblica a mano.

## Flussi

### CI (`.github/workflows/ci.yml`)

- **Quando**: a ogni push su `main`, a ogni pull request e quando lo richiama il flusso Publish. Le esecuzioni superate sullo stesso ramo vengono annullate, tranne quelle dei tag.
- **Build e test** (lavoro "Build e test"):
  1. ripristino dei pacchetti;
  2. controllo del formato con `dotnet format --verify-no-changes`, prima della compilazione (fine riga e spazi, secondo `.editorconfig`);
  3. compilazione in Release con `-warnaserror`: un avviso è un errore;
  4. test con copertura, compresi i test d'integrazione (PostgreSQL 18 con Testcontainers) e i test di architettura;
  5. soglia di copertura: almeno l'80% delle righe di Domain e Application (`.github/scripts/soglia-copertura.py`).
  
  I test end-to-end della CLI (categoria `EndToEnd`) girano in questo stesso passo, contro PostgreSQL 18. I risultati dei test e la copertura compaiono nel riepilogo del lavoro e nell'artefatto `test-results` (14 giorni).
- **Vulnerabilita pacchetti** (lavoro "Vulnerabilita pacchetti"): `dotnet list package --vulnerable --include-transitive`, anche per le dipendenze transitive. Il comando esce con 0 anche quando trova vulnerabilità, quindi lo script controlla il testo del rapporto e fallisce se trova qualcosa.
- **Controllo segreti** (lavoro "Controllo segreti"): gitleaks sull'intera storia git, con i valori oscurati nei log (`--redact`). Si usa il programma da riga di comando, verificato con checksum, perché l'azione ufficiale richiede una licenza nei repository di organizzazione. In locale lo stesso controllo gira come hook `pre-commit` (sezione "Hook locale" più sotto).
- **Build immagine Docker** (lavoro "Build immagine Docker"): costruisce il `Dockerfile` senza pubblicarlo e verifica che `--version` stampi `0.0.0`, la versione predefinita.

### Publish (`.github/workflows/publish.yml`)

- **Quando**: push di un tag `vX.Y.Z` (anche `vX.Y.Z-rc1`) oppure avvio manuale da *Actions > Publish > Run workflow*.
- **Passi**, in ordine:
  1. **CI**: richiama l'intera CI. Se è rossa, la pubblicazione si ferma.
  2. **Commit da main**: verifica che il commit discenda da `origin/main` (`.github/scripts/verifica-tag-da-main.sh`). Un tag su un commit fuori da `main` fa fallire il flusso prima di pubblicare.
  3. **Versione**: il tag deve essere `vX.Y.Z` o `vX.Y.Z-suffisso`. L'avvio manuale produce la versione `0.0.0-manual.<sha>`.
  4. **Immagine console (GHCR)**: costruisce il `Dockerfile` con `VERSION` e lo pubblica su `ghcr.io/f1-solution/dbenigmistica` con i tag `X.Y.Z`, `sha-<7 caratteri>` e `latest`. `latest` non va alle versioni con suffisso e agli avvii manuali da rami diversi da `main`.
  5. **Eseguibile console** (`win-x64` e `linux-x64`): pubblicazione autonoma, con verifica che l'archivio contenga `appsettings.json` e l'eseguibile `dbe` (`dbe.exe` su Windows).
  6. **GitHub Release** (solo da tag): crea la release con note generate, gli archivi `dbenigmistica-console-X.Y.Z-win-x64.zip` e `dbenigmistica-console-X.Y.Z-linux-x64.tar.gz`. Le versioni con suffisso sono marcate come prerelease. L'avvio manuale non crea release.

### Dependabot (`.github/dependabot.yml`)

Ogni settimana propone aggiornamenti per NuGet, GitHub Actions e immagini Docker, raggruppati (pacchetti di test, pacchetti Microsoft, altri aggiornamenti minori e patch). Le pull request passano dalla CI.

## Hook locale

Il file `.githooks/pre-commit` esegue gitleaks sui file in staging prima di ogni commit e blocca il commit se trova segreti. Per attivarlo, una volta per clone:

```bash
git config core.hooksPath .githooks
```

Senza questa impostazione git non esegue l'hook. Se gitleaks non è installato, l'hook blocca il commit e dice come installarlo (`winget install gitleaks.gitleaks` su Windows, `brew install gitleaks` su macOS). Il controllo in CI resta quello autorevole.

## Configurazione del Fornitore AI nelle esecuzioni

Nel container la configurazione arriva solo da variabili d'ambiente (gli user-secrets valgono solo in Development, con `dotnet run`); l'immagine non contiene segreti. Le chiavi sono quelle della sezione `FornitoreAi` (`docs/adr/004-fornitore-ai.md`):

- `FornitoreAi__Tipo`: `claude-code` (predefinito) oppure `api-anthropic`.
- `FornitoreAi__Modello`: modello usato dal fornitore.
- `FornitoreAi__ClaudeCode__Eseguibile`: eseguibile di Claude Code.
- `FornitoreAi__Api__ChiaveApi`: solo con `api-anthropic`.
- `CLAUDE_CODE_OAUTH_TOKEN`: token di Claude Code, in alternativa all'accesso con `claude`.

Per la connessione al database: `ConnectionStrings__DefaultConnection`.

```
docker run --rm \
  -e ConnectionStrings__DefaultConnection="<stringa di connessione>" \
  ghcr.io/f1-solution/dbenigmistica:latest --help
```

L'immagine oggi non contiene Claude Code. Con il fornitore predefinito, i comandi che usano l'AI nel container falliscono la validazione finché la fase 1 non porta l'eseguibile nel container (`docs/adr/004-fornitore-ai.md`).

Per scaricare l'immagine serve un account con accesso al pacchetto (`docker login ghcr.io` con un token che ha `read:packages`).

## Creare una versione

Dal ramo `main` aggiornato e con la CI verde:

```
git tag -a v1.0.0 -m "Versione 1.0.0"
git push origin v1.0.0
```

Il tag fa partire Publish. Se un tag è stato creato per errore, eliminarlo (`git push origin :refs/tags/v1.0.0`) prima che la pubblicazione termini, e cancellare l'eventuale release.

## Impostazioni da fare a mano su GitHub

1. **Permessi dei flussi** (*Settings > Actions > General > Workflow permissions*): lasciare "Read repository contents and packages permissions". I flussi dichiarano da soli i permessi in più (`packages: write`, `contents: write`). Se l'organizzazione limita le azioni consentite (*Allowed actions*), consentire `actions/*` e `docker/*` (le azioni sono fissate per SHA).
2. **Protezione del ramo `main`** (*Settings > Branches* oppure *Rules > Rulesets*): richiedere la pull request, richiedere i controlli di stato **Build e test**, **Vulnerabilita pacchetti**, **Controllo segreti** e **Build immagine Docker** (compaiono nell'elenco dopo la prima esecuzione), con ramo aggiornato, e vietare il push diretto e i force push. Rendere obbligatorio un controllo solo dopo che è diventato verde, altrimenti blocca anche le correzioni. Nei repository privati di un'organizzazione serve un piano GitHub che supporti la protezione dei rami. Il repository è privato su piano gratuito, che non la consente (l'API risponde 403): la protezione è rimandata finché il piano non la permette (decisione D1 del 13/11/2026, `docs/fase-0-chiusura-decisioni.md`).
3. **Protezione dei tag** (*Rules > Rulesets > New tag ruleset*, destinazione `v*`): limitare chi può creare tag di versione, perché un tag avvia la pubblicazione. Anche questa protezione è rimandata con quella di `main`, perché i ruleset richiedono lo stesso piano.
4. **Pacchetto GHCR**: dopo la prima pubblicazione, aprire *Packages > dbenigmistica > Package settings*. Verificare che la visibilità sia **Private** e che il pacchetto risulti collegato al repository (lo fa l'etichetta `org.opencontainers.image.source`); in caso contrario usare *Connect repository*. Per far scaricare l'immagine ad altri, aggiungere persone, team o il repository in *Manage access*. Se un'organizzazione impedisce la creazione di pacchetti dai flussi, abilitarla in *Organization settings > Packages*.
5. **Dependabot** (*Settings > Advanced Security*): attivare Dependabot alerts e security updates (gli aggiornamenti pianificati partono già dal file di configurazione).
6. **Secret scanning** (opzionale, *Settings > Advanced Security*): attivare secret scanning e push protection, se disponibili, in aggiunta a gitleaks.

## Aggiungere altri artefatti

Per un'immagine: aggiungere un `Dockerfile.<nome>` e un lavoro analogo a `container`. Per un eseguibile: aggiungere il progetto alla matrice del lavoro `binaries`. Il lavoro `release` raccoglie gli archivi `console-*`: va esteso con il nuovo prefisso. Ogni fase che aggiunge artefatti (involucro desktop, pacchetto del Lettore) lo dichiara in `ROADMAP.md`.
