# 03 - Prova reale di Claude Code non interattivo (ondata 1.1-prova)

**Stato:** In attesa (dipende dal file 01)
**Fase:** 1

## Scopo

Verificare sul PC Windows del proprietario che Claude Code legge una pagina PNG e restituisce il risultato in JSON, con il login dell'abbonamento. La prova conferma o corregge le risposte provvisorie del documento `.planning/phases/1/prova-claude-code-1.1.md`.

## Scadenza

**6 dicembre 2026.** Stessa data del file 01 (`docs/fase-1b-decisioni.md`).

## Tempo stimato

Non stimato nella ROADMAP. La prova è una sessione sola, fuori dagli orari di lavoro degli agenti.

## Prerequisiti

- File 01 completato, con le domande a e b a "Sì". Oppure una tua approvazione scritta.
- Claude Code installato sul PC.
- L'agente principale ha confermato la fonte della pagina di prova (passo 9). Il Set sintetico non è ancora nel repository.
- Nessun agente sta lavorando sull'abbonamento. La finestra è condivisa.

AVVERTENZA: Usa solo la pagina di prova prodotta dal progetto. Non usare pagine di Riviste reali in questa prova.

AVVERTENZA: Per la prova dal vivo imposta `FornitoreAi__DalVivoAbilitato` a `true` solo dopo il via. Con `false`, il predefinito, i comandi dal vivo non partono.

AVVERTENZA: Non aprire, non stampare e non incollare il file `~\.claude\.credentials.json`. Basta sapere che esiste (passo 6).

## Cosa ti serve

- PowerShell.
- Il tuo account Claude dell'abbonamento.
- Una cartella di lavoro fuori dal repository: `%USERPROFILE%\DBE-prova-1.1`.

## Procedura

### Parte 1 - Login e controlli

1. Apri PowerShell. Esegui:

```powershell
Get-Command claude
```

   Risultato: Il comando mostra il percorso di `claude`. Se mostra un errore, installa Claude Code e fermati qui.

2. Esegui `claude`. Nella sessione scrivi `/login`. Segui la pagina nel browser con l'account dell'abbonamento.
   Risultato: Claude Code conferma l'accesso.

3. Chiudi Claude Code. Scrivi `/exit`.
   Risultato: Torni al prompt di PowerShell.

4. Esegui:

```powershell
claude auth status --text
```

   Risultato: Il testo indica che c'è un accesso, e la scadenza se disponibile. Copia solo questa riga di stato. Non copiare token.

5. Controlla che non ci sia una chiave API nella finestra. Esegui:

```powershell
[bool]$env:ANTHROPIC_API_KEY
```

   Risultato: Il comando stampa `False`.

   AVVERTENZA: Se stampa `True`, Claude Code usa la chiave invece del login. Esegui `Remove-Item Env:ANTHROPIC_API_KEY -ErrorAction SilentlyContinue`. Poi esegui:

```powershell
[bool][Environment]::GetEnvironmentVariable('ANTHROPIC_API_KEY','User')
```

   Se stampa `True`, rimuovi la variabile da *Impostazioni > Sistema > Informazioni > Impostazioni di sistema avanzate > Variabili d'ambiente*. Poi apri una nuova finestra di PowerShell e ripeti il passo 5.

6. Controlla che il file di accesso esista. Esegui:

```powershell
Test-Path "$env:USERPROFILE\.claude\.credentials.json"
```

   Risultato: Il comando stampa `True`. Non aprire il file.

7. Controlla la configurazione del progetto. Esegui:

```powershell
cd C:\Progetti\DBEnigmistica
dotnet run --project src\DBEnigmistica.Console -- check-ai
```

   Risultato: Il comando stampa `Configurazione valida.`. Se esce con codice 3, il login manca. Torna al passo 2.

### Parte 2 - Preparazione della prova

8. Crea la cartella di lavoro. Esegui:

```powershell
New-Item -ItemType Directory -Path "$env:USERPROFILE\DBE-prova-1.1"
```

   Risultato: La cartella esiste.

9. Crea la pagina di prova. Esegui:

```powershell
cd C:\Progetti\DBEnigmistica
dotnet run --project src\DBEnigmistica.Console -- diagnostica-render --salva-png "$env:USERPROFILE\DBE-prova-1.1\render"
```

   Risultato: La cartella `render` contiene file PNG.

   NOTA: Se il comando dice che la cartella non esiste, crea `render` con `New-Item -ItemType Directory` nella stessa posizione. Poi ripeti il comando.

   ATTENZIONE: Questo comando sostituisce la pagina del Set sintetico, che non esiste ancora nel repository. Fermati se l'agente principale non ha confermato questo passo.

10. Apri i PNG della cartella `render` con Foto. Scegli il PNG con un titolo leggibile. Copialo come `pagina.png`. Esegui, sostituendo `<nome-file>`:

```powershell
Copy-Item "$env:USERPROFILE\DBE-prova-1.1\render\<nome-file>.png" "$env:USERPROFILE\DBE-prova-1.1\pagina.png"
```

    Risultato: Il file `pagina.png` esiste. Il titolo è leggibile. Se non c'è un titolo leggibile, fermati e avvisa l'agente.

11. Vai nella cartella di prova. Crea lo schema. Esegui:

```powershell
Set-Location "$env:USERPROFILE\DBE-prova-1.1"
Set-Content -Path schema.json -Value '{"type":"object","properties":{"titolo":{"type":"string"}},"required":["titolo"]}' -Encoding utf8
```

    Risultato: Il file `schema.json` esiste.

12. Crea il prompt. Esegui:

```powershell
Set-Content -Path prompt.txt -Value 'Leggi pagina.png con lo strumento Read e restituisci il titolo scritto nella pagina.' -Encoding utf8
```

    Risultato: Il file `prompt.txt` esiste.

13. Togli la chiave API dalla finestra. Esegui:

```powershell
Remove-Item Env:ANTHROPIC_API_KEY -ErrorAction SilentlyContinue
```

    Risultato: Il comando non stampa nulla.

### Parte 3 - Esecuzione

14. Annota l'ora di inizio.

15. Esegui la prova. Il comando va scritto su una riga:

```powershell
Get-Content prompt.txt -Raw | claude -p --output-format json --json-schema (Get-Content schema.json -Raw) --model claude-sonnet-5-5 --tools Read --allowedTools Read --permission-mode dontAsk --no-session-persistence --strict-mcp-config --disable-slash-commands | Tee-Object uscita.json
$LASTEXITCODE
```

    Risultato: Il comando scrive `uscita.json` e stampa il codice d'uscita.

16. Annota l'ora di fine e il codice d'uscita.

17. Esegui `claude auth status --text`. Confronta la riga con quella del passo 4.
    Risultato: Sai se lo stato dell'accesso è cambiato.

ATTENZIONE: Se compare un messaggio di limite raggiunto (per esempio "session limit" o "resets"), non ripetere il comando. Salva il testo esatto e il codice d'uscita. Non provocare il limite apposta.

## Verifica finale

- `uscita.json` esiste e non è vuoto.
- Apri `uscita.json` con il Blocco note. Cerca la parola `structured_output`.
  Risultato: Il campo esiste e contiene il titolo. Se manca, annotalo: è un risultato della prova.
- Cerca la parola `token` nel file. Risultato: Non compare. Se compare, non inviare il file: avvisa l'agente.

## Cosa consegnare all'agente principale

Invia questi elementi:

1. Il file `uscita.json`.
2. Il codice d'uscita.
3. L'ora di inizio e l'ora di fine.
4. La riga di stato di `claude auth status --text` prima e dopo il passo 17.
5. Il testo esatto di un eventuale messaggio di limite.
6. Se il titolo è stato letto correttamente (sì o no).

Non inviare `.credentials.json`, token o chiavi.

## Cosa sblocca

- Il rapporto della prova 1.1-prova. L'agente aggiorna il documento della prova.
- L'agente scrive `PHASE:1 WAVE:1.1-prova STATUS:complete` in `PROGRESS.md`. Non scrivere questa riga tu.
- Fino a questa riga non si fanno estrazioni né generazioni dal vivo (file 10).

---
Fonti: `.planning/phases/1/prova-claude-code-1.1.md`, sezione 1 (procedura) e sezione 2 (domande della prova); `.planning/phases/1/PLAN.md`, criteri di uscita della 1.1; `ROADMAP.md`, fase 1, "Dal proprietario" (login OAuth) e sezione 7 (domanda 13); `src/DBEnigmistica.Console/Commands/DiagnosticaRenderCommand.cs` (opzione `--salva-png`).
