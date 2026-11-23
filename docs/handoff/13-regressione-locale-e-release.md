# 13 - Regressione locale sul Set di riferimento e allegato alla release

**Stato:** Da fare. Prima prova entro il 27/11/2026 sul Set sintetico; rilancio dopo il ricalcolo del 2/12/2026, senza nuova approvazione.
**Fase:** 1 e tutte le fasi successive con una release

## Scopo

Eseguire la regressione sul Set di riferimento, sul PC del proprietario, prima di ogni release. Allegare il rapporto alla release su GitHub. È l'unica eccezione alla regola "test solo con GitHub Actions" (ADR 008). Finché il Set di riferimento non è pronto, la prova usa il Set sintetico del repository.

## Scadenza

- Prima prova: entro il 27/11/2026, sul Set sintetico.
- Rilancio: entro il 6/12/2026, dopo il ricalcolo del 2/12.
- Prima di ogni release.
- Per la release `v0.2.0`: entro il 21 febbraio 2027.

## Tempo stimato

Circa 15 minuti la prima volta, con questa guida. Il rilancio richiede circa 5 minuti.

## Prerequisiti

- Lo script `scripts/regressione-locale.ps1`, presente nel repository dalla ondata 1.2 (dopo l'unione in `main`).
- Per la prova del 27/11: il Set sintetico, già nel repository in `tests\DBEnigmistica.Tests\Fixtures\SetSintetico`. Non serve altro.
- Per una release: il Set di riferimento (file 08) e la release esiste su GitHub (file 24).

AVVERTENZA: Il Set di riferimento e le sue Registrazioni non escono dal PC. Non caricarli su GitHub, non allegarli alla release. Allega solo il rapporto.

## Cosa ti serve

- PowerShell.
- Per una release: il percorso della cartella del Set di riferimento, per esempio `C:\DBE-Set-di-riferimento`.
- GitHub CLI (`gh`), oppure la pagina della release su GitHub.

## Procedura

1. Apri PowerShell e vai nella cartella del repository:

```powershell
cd C:\Progetti\DBEnigmistica
```

   Risultato: Sei nella cartella del repository.

2. Lancia lo script. Per la prova, usa il Set sintetico:

```powershell
.\scripts\regressione-locale.ps1 -Set tests\DBEnigmistica.Tests\Fixtures\SetSintetico
```

   Per una release, indica il Set di riferimento e il tag:

```powershell
.\scripts\regressione-locale.ps1 -Set C:\DBE-Set-di-riferimento -Tag v0.2.0
```

   Parametri:
   - `-Set` (obbligatorio): cartella del set, con il file `set.json`.
   - `-Tag`: tag della release. Serve solo al comando `gh release upload` che lo script stampa. Se lo ometti, il comando contiene il segnaposto `<tag-della-release>`, da sostituire.
   - `-Uscita`: cartella dei rapporti. Predefinita: `rapporti-regressione` nella cartella corrente.
   - `-Dbe`: eseguibile `dbe`. Se lo ometti, usa `dbe` dal PATH o, in mancanza, `dotnet run` sul progetto del repository.

   Non cambiare `-Fornitore`, `-Modello` e `-Dpi`: i valori predefiniti sono quelli giusti.

   Risultato: Lo script termina in pochi minuti e scrive `rapporto.json` e `rapporto.md` in `rapporti-regressione\<versione del prompt>\<nome del set>\`, anche se la regressione fallisce.

3. Controlla il codice d'uscita, subito dopo lo script:

```powershell
$LASTEXITCODE
```

   Risultato: Il codice è `0`: nessuna metrica è scesa sotto la Linea di base, e puoi fare la release.
   Se il codice è `1`: una metrica è scesa, oppure il set non si è letto. Non fare la release. Leggi le metriche peggiorate stampate dallo script e mandami il rapporto, usando il percorso stampato dallo script.
   Se manca `set.json`, lo script esce con `1` senza rapporto: segnala il percorso del set all'agente.

4. Apri `rapporto.md` nella cartella indicata. Controlla che l'intestazione riporti il set, la versione del prompt e la risoluzione corretti.
   Risultato: Il rapporto è completo.

5. Allega il rapporto alla release, solo con codice `0`. Lo script stampa il comando `gh release upload` con i due file. Lo script non lo esegue: eseguilo tu, dopo aver controllato il tag. Per la prova del 27/11 non c'è release: salta questo passo e manda all'agente il codice e il rapporto.
   La cartella `rapporti-regressione\` è ignorata da git, quindi i rapporti non finiscono nei commit.
   Risultato: Il rapporto è allegato alla release.

   Se usi la pagina: vai su *Releases*, apri la release, scegli *Edit*, trascina il file del rapporto. Salva.

## Verifica finale

- Codice d'uscita `0`.
- Il rapporto riporta il set, la versione del prompt e la risoluzione corretti.
- Per una release: il rapporto è allegato, e nessun file del Set è nella lista degli allegati.

## Cosa consegnare all'agente principale

1. Codice d'uscita.
2. Per la prova: la cartella del rapporto. Per una release: il nome della release a cui è allegato il rapporto.
3. Se il codice è `1`: l'elenco delle metriche peggiorate stampato dallo script.

## Cosa sblocca

- La release (file 24). Senza il rapporto la fase non si chiude.
- La chiusura della fase 1, dopo il rilancio entro il 6/12/2026.

---
Fonti: `ROADMAP.md`, sezione 1.3 (eccezione alla regola GitHub Actions, ADR 008) e fase 1 ("Dal proprietario" e criterio di accettazione 5); `.planning/phases/1/preparazione-1.2.md`, sezioni 1.3 (D6) e 7; `scripts/regressione-locale.ps1` (parametri e codici d'uscita); `docs/CI.md`, sezione "Creare una versione".
