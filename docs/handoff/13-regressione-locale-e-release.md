# 13 - Regressione locale sul Set di riferimento e allegato alla release

**Stato:** Da fare (la prima esecuzione segue l'ondata 1.2)
**Fase:** 1 e tutte le fasi successive con una release

## Scopo

Eseguire la regressione sul Set di riferimento, sul PC del proprietario, prima di ogni release. Allegare il rapporto alla release su GitHub. È l'unica eccezione alla regola "test solo con GitHub Actions" (ADR 008).

## Scadenza

- Prima di ogni release.
- Per la release `v0.2.0`: entro il 21 febbraio 2027.

## Tempo stimato

Non stimato separatamente. È parte delle circa 20 ore della fase 1.

## Prerequisiti

- Set di riferimento pronto (file 08).
- Il nome dello script e dei parametri, dato dall'agente. Lo script è `scripts/regressione-locale.ps1`, disponibile dalla ondata 1.2.
- La release esiste su GitHub (file 24).

AVVERTENZA: Il Set di riferimento e le sue Registrazioni non escono dal PC. Non caricarli su GitHub, non allegarli alla release. Allega solo il rapporto.

## Cosa ti serve

- PowerShell.
- Il percorso della cartella del Set di riferimento, per esempio `C:\DBE-Set-di-riferimento`.
- GitHub CLI (`gh`), oppure la pagina della release su GitHub.

## Procedura

1. Apri PowerShell nella cartella del repository. Esegui:

```powershell
cd C:\Progetti\DBEnigmistica
```

   Risultato: Sei nella cartella del repository.

2. Esegui lo script con il parametro della cartella del Set. Il nome del parametro lo dà l'agente.
   Risultato: Lo script termina e scrive un rapporto in una cartella con la versione.

3. Controlla il codice d'uscita. Esegui:

```powershell
$LASTEXITCODE
```

   Risultato: Il codice è `0`. Se è diverso da 0, una metrica è scesa sotto la linea di base. Non fare la release. Avvisa l'agente.

4. Apri il rapporto. Controlla che la data e la versione siano corrette.
   Risultato: Il rapporto è completo.

5. Allega il rapporto alla release. Lo script stampa il comando `gh release upload`. Eseguilo, oppure usa la pagina della release.
   Risultato: Il rapporto è allegato alla release.

   Se usi la pagina: vai su *Releases*, apri la release, scegli *Edit*, trascina il file del rapporto. Salva.

## Verifica finale

- Codice d'uscita `0`.
- Il rapporto è allegato alla release.
- Nessun file del Set è nella release. Controlla la lista degli allegati.

## Cosa consegnare all'agente principale

1. Codice d'uscita.
2. Nome della release a cui è allegato il rapporto.
3. Se una metrica è scesa, il nome della metrica e il valore.

## Cosa sblocca

- La release (file 24). Senza il rapporto la fase non si chiude.
- La chiusura della fase 1.

---
Fonti: `ROADMAP.md`, sezione 1.3 (eccezione alla regola GitHub Actions, ADR 008) e fase 1 ("Dal proprietario" e criterio di accettazione 5); `.planning/phases/1/PLAN.md`, ondata 1.2 (script `scripts/regressione-locale.ps1`) e "Criteri di uscita" della fase 1; `docs/CI.md`, sezione "Creare una versione".
