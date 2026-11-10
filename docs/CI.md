# Integrazione continua e pubblicazione

Test e pubblicazione avvengono solo tramite GitHub Actions. Nessun segreto aggiuntivo e' necessario: si usa solo il `GITHUB_TOKEN` automatico.

## Flussi

### CI (`.github/workflows/ci.yml`)

- **Quando**: a ogni push su `main`, a ogni pull request e quando lo richiama il flusso Publish. Le esecuzioni superate sullo stesso ramo vengono annullate.
- **Build e test**: ripristino, compilazione in Release (gli avvisi restano visibili), test con copertura. Risultati e copertura sono nel riepilogo del lavoro e nell'artefatto `test-results` (14 giorni).
- **Vulnerabilita pacchetti**: `dotnet list package --vulnerable --include-transitive`; il lavoro fallisce se trova qualcosa.
- **Controllo segreti**: gitleaks sull'intera storia git, con valori oscurati nei log. Si usa il programma da riga di comando (verificato con checksum) perche' l'azione ufficiale richiede una licenza a pagamento nei repository di organizzazione.
- **Stato noto**: i test falliscono (15 su 24) e vanno risanati nella fase 0 di `ROADMAP.md`; nulla e' mascherato. Il controllo vulnerabilita non segnala pacchetti.

### Publish (`.github/workflows/publish.yml`)

- **Quando**: push di un tag `vX.Y.Z` (anche `vX.Y.Z-rc1`) oppure avvio manuale da *Actions > Publish > Run workflow*.
- **Passi**: esegue prima CI per intero; solo se e' verde procede.
- **Immagine**: costruisce `Dockerfile` e la pubblica su `ghcr.io/f1-solution/dbenigmistica` con i tag `X.Y.Z`, `sha-<7 caratteri>` e `latest`. `latest` non va alle versioni con suffisso (prerelease) ne' agli avvii manuali da rami diversi da `main`. Un avvio manuale produce la versione `0.0.0-manual.<sha>`.
- **Release** (solo da tag): crea la GitHub Release con note generate automaticamente e gli eseguibili console autonomi `dbenigmistica-console-X.Y.Z-win-x64.zip` e `...-linux-x64.tar.gz`. Le versioni con suffisso sono marcate come prerelease.

### Dependabot (`.github/dependabot.yml`)

Ogni settimana propone aggiornamenti per NuGet, GitHub Actions e immagini Docker, raggruppati (pacchetti di test, pacchetti Microsoft, altri aggiornamenti minori/patch). Le pull request passano dalla CI.

## Creare una versione

Dal ramo `main` aggiornato e con la CI verde:

```
git tag -a v1.0.0 -m "Versione 1.0.0"
git push origin v1.0.0
```

Il tag fa partire Publish. Se un tag e' stato creato per errore, eliminarlo (`git push origin :refs/tags/v1.0.0`) prima che la pubblicazione termini e cancellare l'eventuale release.

## Eseguire l'immagine

La configurazione arriva da variabili d'ambiente; l'immagine non contiene segreti.

```
docker run --rm \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Anthropic__ApiKey="..." \
  -e Ocr__TesseractDataPath=/tessdata -v ./tessdata:/tessdata:ro \
  ghcr.io/f1-solution/dbenigmistica:latest --help
```

Per scaricarla serve un account con accesso al pacchetto (`docker login ghcr.io` con un token che ha `read:packages`).

## Impostazioni da fare a mano su GitHub

1. **Permessi dei flussi** (*Settings > Actions > General > Workflow permissions*): lasciare "Read repository contents and packages permissions"; i flussi dichiarano da soli i permessi in piu' (`packages: write`, `contents: write`). Se l'organizzazione limita le azioni consentite (*Allowed actions*), consentire `actions/*` e `docker/*` (le azioni sono fissate per SHA).
2. **Protezione del ramo `main`** (*Settings > Branches* oppure *Rules > Rulesets*): richiedere la pull request, richiedere i controlli di stato **Build e test**, **Vulnerabilita pacchetti** e **Controllo segreti** (compaiono nell'elenco dopo la prima esecuzione) con ramo aggiornato, vietare il push diretto e i force push. Rendere obbligatorio un controllo solo dopo che e' diventato verde, altrimenti blocca anche le correzioni. Nei repository privati di un'organizzazione serve un piano GitHub che supporti la protezione dei rami.
3. **Protezione dei tag** (*Rules > Rulesets > New tag ruleset*, destinazione `v*`): limitare chi puo' creare tag di versione, perche' un tag avvia la pubblicazione.
4. **Pacchetto GHCR**: dopo la prima pubblicazione aprire *Packages > dbenigmistica > Package settings*. Verificare che la visibilita' sia **Private** e che il pacchetto risulti collegato al repository (lo fa l'etichetta `org.opencontainers.image.source`); in caso contrario usare *Connect repository*. Per far scaricare l'immagine ad altri, aggiungere persone, team o il repository in *Manage access*. Se un'organizzazione impedisce la creazione di pacchetti da flussi, abilitarla in *Organization settings > Packages*.
5. **Dependabot** (*Settings > Advanced Security*): attivare Dependabot alerts e security updates (gli aggiornamenti pianificati partono gia' dal file di configurazione).
6. **Secret scanning** (opzionale, *Settings > Advanced Security*): attivare secret scanning e push protection, se disponibili, in aggiunta a gitleaks.

## Aggiungere altri artefatti

Futuri artefatti (API web, interfaccia web, involucro desktop): per un'immagine, aggiungere un `Dockerfile.<nome>` e un lavoro analogo a `container`; per un eseguibile, aggiungere il progetto alla fase di pubblicazione di `binaries`. Il lavoro `release` raccoglie gli archivi `console-*` e va esteso con il nuovo prefisso.
