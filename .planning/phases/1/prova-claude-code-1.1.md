# Prova di Claude Code non interattivo - ondata 1.1

**Stato: IN ATTESA.** La prova reale non è stata eseguita. Il proprietario non ha ancora confermato le condizioni d'uso dell'abbonamento (scadenza 6/12/2026, domanda aperta 13) e nessuna chiamata dal vivo è ammessa prima. Le risposte sotto sono **provvisorie**, prese dalla documentazione ufficiale e da `claude --help` (versione 2.1.329, controllo locale senza rete). Si confermano o si correggono con la prova.

## 1. Procedura per il proprietario

Prerequisiti: condizioni d'uso verificate; Claude Code installato; login fatto con `claude` e poi `/login`; nessuna `ANTHROPIC_API_KEY` nell'ambiente di quella finestra di PowerShell; fuori dalle ore di lavoro degli agenti (la Finestra dell'abbonamento è condivisa). Una sola pagina del Set sintetico.

1. Verifica locale, senza chiamate: `claude auth status --text` (dice se c'è il login e, da v2.1.318, quando finisce). Poi `dotnet run --project src\DBEnigmistica.Console -- check-ai`: deve dire "Configurazione valida."
2. Prepara una cartella vuota e copiaci una pagina PNG del Set sintetico come `pagina.png`; salva uno schema piccolo in `schema.json` (per esempio `{"type":"object","properties":{"titolo":{"type":"string"}},"required":["titolo"]}`) e un `prompt.txt` con: "Leggi pagina.png con lo strumento Read e restituisci il titolo scritto nella pagina."
3. Da quella cartella, in PowerShell, togli la variabile: `Remove-Item Env:ANTHROPIC_API_KEY -ErrorAction SilentlyContinue`.
4. Lancia (il prompt arriva da stdin, come farà l'adattatore):

   ```powershell
   Get-Content prompt.txt -Raw | claude -p --output-format json --json-schema (Get-Content schema.json -Raw) --model claude-sonnet-5-5 --tools Read --allowedTools Read --permission-mode dontAsk --no-session-persistence --strict-mcp-config --disable-slash-commands | Tee-Object uscita.json
   $LASTEXITCODE
   ```

5. Salva `uscita.json`, il codice d'uscita e la durata (`Measure-Command` se vuoi). Poi `claude auth status --text` per vedere se è cambiato qualcosa.
6. Finestra esaurita: **non provocarla apposta**. Se capita, salva testo e codice d'uscita esatti.
7. Non incollare token né il contenuto di `~\.claude\.credentials.json` da nessuna parte. Per il punto "dove sta il login" basta confermare che il file esiste.

Quando la prova è fatta, mandare `uscita.json` all'agente principale: aggiorna questa pagina e l'elenco degli argomenti dell'adattatore (`FornitoreClaudeCode`).

## 2. Domande a cui la prova risponde

| # | Domanda | Risposta provvisoria | Esito prova |
|---|---|---|---|
| 1 | Formato effettivo dell'output e se lo schema si può imporre | Con `--output-format json` esce un solo oggetto JSON (`type`, `subtype`, `is_error`, `result`, `session_id`, `total_cost_usd`, `usage`). Con `--json-schema` il JSON conforme sta nel campo `structured_output`; se manca, la run è finita senza output valido. `format` nello schema non è imposto. | da fare |
| 2 | Come passa l'immagine | Non c'è un'opzione "immagine" in modalità testo. Si scrive il PNG in una cartella dedicata, la si usa come cartella di lavoro, si permette solo `Read` (`--tools Read --allowedTools Read`) e nel prompt si indica il nome del file. In alternativa `--input-format stream-json` accetta blocchi immagine in base64: da valutare solo se `Read` non basta. | da fare |
| 3 | Testo, codice d'uscita e ora di ripresa della finestra esaurita | Il testo è "You've hit your session limit · resets 3:45pm" (o "weekly limit · resets Mon 12:00am"; anche Opus e Sonnet). La documentazione non dice come compare in `-p` né il codice d'uscita. Versioni precedenti scrivevano `Claude AI usage limit reached\|<epoch>`. L'adattatore riconosce entrambi i formati e, senza ora riconoscibile, segnala la finestra esaurita senza istante di ripresa. | da fare (solo se capita) |
| 4 | Dove salva il login su Windows | `%USERPROFILE%\.claude\.credentials.json` (con `CLAUDE_CONFIG_DIR` impostata, in quella cartella). `check-ai` cerca già quel file. | confermare che esiste |
| 5 | Tempi, token e costo | `total_cost_usd` e `usage` sono nell'oggetto JSON; sono stime lato client. | da misurare |
| 6 | L'immagine entra davvero e viene letta? | Atteso sì con `Read`. | da fare |
| 7 | La chiave API nell'ambiente viene preferita al login? | Sì: in modalità `-p` la chiave è sempre usata se presente. Per questo l'adattatore la toglie dal processo figlio, insieme a `ANTHROPIC_AUTH_TOKEN`, `ANTHROPIC_PROFILE`, le variabili di federazione e `CLAUDE_CODE_USE_BEDROCK/VERTEX/FOUNDRY`. | verificare con `/status` |
| 8 | Limiti di dimensione | Stdin: 10 MB. Lo schema passa come argomento: su Windows la riga di comando ha un tetto (circa 32.000 caratteri). | misurare con uno schema reale |

Opzioni usate dall'adattatore (provvisorie): `-p --output-format json --json-schema <schema> --model <modello> --tools Read --allowedTools Read --permission-mode dontAsk --no-session-persistence --strict-mcp-config --disable-slash-commands`, con cartella di lavoro dedicata che contiene `pagina.png`.

Rischi da verificare: le impostazioni utente caricate da Claude Code possono cambiare il comportamento (`--bare` non va bene: non legge il login OAuth); `--permission-mode dontAsk` deve far leggere `pagina.png` senza richieste; un `claude` dentro un `.cmd` o `.bat` è rifiutato dall'adattatore.

## 3. Fonti

- Esecuzione non interattiva, output JSON, `--json-schema`, `structured_output`, stdin 10 MB, `--bare`: https://code.claude.com/docs/en/headless
- Autenticazione, precedenza delle credenziali, `ANTHROPIC_API_KEY` sempre usata con `-p`, posizione di `.credentials.json` su Windows, `claude setup-token`, `claude auth status`: https://code.claude.com/docs/en/authentication
- Messaggi di limite raggiunto ("You've hit your session limit · resets 3:45pm"), errori 429 e 529: https://code.claude.com/docs/en/errors
- Opzioni della riga di comando: `claude --help` (2.1.329, locale) e https://code.claude.com/docs/en/cli-reference

## 4. Nota sul container (ADR 010, punto 6; nessuna costruzione)

Cosa servirebbe nell'immagine dello Studio per usare Claude Code in container, e i rischi. La decisione è rimandata alla fase 2.

- **Eseguibile.** L'immagine pubblicata è `chiseled` (senza shell né gestore di pacchetti). Claude Code nativo va aggiunto come file e deve avviarsi senza shell; da provare. Un'immagine non chiseled allarga la superficie d'attacco (alternativa già scartata da D5).
- **Autenticazione.** Il browser non c'è: serve `CLAUDE_CODE_OAUTH_TOKEN`, ottenuto con `claude setup-token` (dura un anno, solo richieste ai modelli) e passato come segreto di Compose, non in un `.env` tracciato. Il token non va nei log né nell'immagine.
- **Scrittura.** Claude Code salva dati utente in `~/.claude`: serve un volume scrivibile o `CLAUDE_CONFIG_DIR` su una cartella temporanea, e `--no-session-persistence` per non lasciare sessioni. L'adattatore scrive anche una cartella temporanea per pagina.
- **Rete.** Claude Code deve raggiungere Anthropic: l'uscita va aperta solo verso quegli host.
- **Finestra condivisa.** Il container userebbe la stessa Finestra dell'abbonamento del PC: due consumatori sullo stesso limite.
- **Condizioni d'uso.** Un token OAuth dell'abbonamento in un servizio containerizzato è l'uso su cui le condizioni (domanda 13) vanno lette con più attenzione: l'abbonamento è personale e un container lo fa sembrare un servizio. Senza conferma scritta non si fa.
- **Chiusura.** Se il container si ferma a metà (SIGTERM), Claude Code esce con 143 senza registrare il risultato: la pagina resta in coda.
- **Conseguenza.** In fase 1 il container non chiama Claude Code. Se in fase 2 le condizioni non lo permettono, lo Studio in container chiama `dbe` sul PC Windows (opzione C di D5) o usa un altro Fornitore AI.
