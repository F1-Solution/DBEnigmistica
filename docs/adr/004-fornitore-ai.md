# ADR 004: Fornitore AI dietro un'unica interfaccia, Claude Code predefinito

- **Stato:** Proposto, in attesa dell'approvazione del proprietario
- **Data:** 11/11/2026

## Contesto

Il programma usa modelli con visione per leggere le Riviste (ADR 003) e per creare Giochi. Il proprietario ha deciso che non si usano API a consumo come scelta predefinita (`ROADMAP.md`, sintesi, punto 5). Lo sviluppo e la creazione consumano gli stessi limiti dell'abbonamento. Il fornitore migliore per l'estrazione non è ancora noto: va scelto con una misura sul set di riferimento.

Il codice precedente usava il pacchetto non ufficiale `Anthropic.SDK` e una chiave API obbligatoria con la sezione `Anthropic`. Nella fase 0 quel pacchetto è stato eliminato e la configurazione è stata sostituita dalla sezione `FornitoreAi`, validata solo dai comandi che usano l'AI.

## Decisione

1. **Una sola interfaccia nell'Application layer**, con due usi: estrazione (pagina in Giochi) e creazione (brief e regole in candidato). Gli adattatori stanno in Infrastructure e si scelgono da configurazione.
2. **Adattatori previsti** (`ROADMAP.md`, sezione 2.4):
   - *Claude Code non interattivo* con l'abbonamento del proprietario: predefinito per la creazione e riferimento per l'estrazione. Solo nello Studio sul PC del proprietario.
   - *Piani gratuiti di altri fornitori* e *modello aperto in locale*: candidati per l'estrazione, confrontati sullo stesso set.
   - *API a consumo*: possibile, non predefinita. Si attiva solo se un limite viene misurato e il proprietario accetta la spesa.
3. **Configurazione** nella sezione `FornitoreAi`:
   - `FornitoreAi:Tipo`: `claude-code` (predefinito) oppure `api-anthropic`.
   - `FornitoreAi:Modello`: il modello, configurabile. Nel file `appsettings.json` è `claude-sonnet-5-5`.
   - `FornitoreAi:ClaudeCode:Eseguibile`: nome o percorso di `claude`, con predefinito `claude`.
   - `FornitoreAi:Api:ChiaveApi`: richiesta solo con `api-anthropic`, da variabile d'ambiente o da user-secrets.
   - `CLAUDE_CODE_OAUTH_TOKEN`: token di Claude Code, in alternativa all'accesso con `claude`.
4. **Validazione all'avvio dei comandi che usano l'AI**, prima di qualunque chiamata di rete. Con `claude-code` si controllano l'eseguibile e l'autenticazione (variabile di token o credenziali salvate dall'accesso). Con `api-anthropic` si controlla che la chiave sia presente. In entrambi i casi si controlla che il modello sia impostato. L'errore dice quale variabile impostare, senza stampare valori.
5. **`dbe check-ai`** verifica la configurazione senza chiamate di rete e riporta il fornitore e il modello. Non stampa mai la chiave.
6. **Regola di scelta per l'estrazione** (`ROADMAP.md`, sezione 2.4): diventa predefinito il fornitore che supera le soglie di accuratezza al costo minore, tra quelli le cui condizioni d'uso il proprietario ha verificato. I piani gratuiti ricevono pagine delle Riviste solo dopo questa verifica.
7. **Il profilo pubblico non registra alcun adattatore**: il Sito non dipende dall'abbonamento.

## Conseguenze

- Cambiare fornitore non tocca l'Application layer né il dominio. Il confronto sullo stesso set di pagine resta ripetibile.
- Oggi nessun adattatore è implementato. Il segnaposto `FornitoreAiNonDisponibile` lancia l'errore "Il Fornitore AI è disponibile dalla fase 1". Con la configurazione valida, `dbe check-ai` può quindi dare esito positivo senza che una chiamata reale sia mai stata provata.
- Con `api-anthropic`, la validazione verifica solo la presenza della chiave, non la sua validità. Un controllo di rete, se servisse, è una decisione separata.
- La condivisione dei limiti dell'abbonamento è un vincolo di pianificazione: estrazione e creazione girano in orari diversi dal lavoro degli agenti, e la fase 1 misura quante pagine si leggono per finestra.
- L'esecuzione in container dello Studio richiede l'eseguibile di Claude Code nell'immagine e il token passato come segreto. Il punto è da verificare nell'ondata 1.1: se le condizioni d'uso lo sconsigliano, lo Studio chiamerà Claude Code installato su Windows.
- Il modello predefinito è un valore da verificare. `ROADMAP.md` (sezione 8, rischio 5) segnala che il modello configurato potrebbe essere già ritirato: va controllato prima della fase 1.

## Alternative scartate

- **API a consumo come predefinita**: scartata dal proprietario, anche con un tetto mensile basso: è una spesa che non vuole (`docs/fase-0-decisioni.md`, Q3 e Q4).
- **Pacchetto non ufficiale `Anthropic.SDK`**: eliminato. L'accesso passa dall'interfaccia unica.
- **Un solo fornitore collegato in modo fisso**: cambiare fornitore richiederebbe modifiche in tutto il codice.
- **Solo piani gratuiti**: limiti di volume e possibile uso dei dati per addestramento. Restano candidati, dopo la verifica.
- **Solo un modello aperto in locale**: la qualità su griglie e rebus è incerta. Resta un candidato da misurare.

## Riferimenti

- `ROADMAP.md`, sintesi punti 4-5, sezione 2.4 (il Fornitore AI), sezione 8, rischio 5 (dipendenza dal Fornitore AI).
- `docs/fase-0-decisioni.md`, Q4 e Q5 (Claude Code non interattivo; scelta del fornitore predefinito).
- `docs/ondata-0-2-decisioni-2.md`, q4 (comando `dbe check-ai`).
- `ROADMAP.md`, sezione 3, fase 0, criteri 6 e 7 (nessun pacchetto di fornitore nella fase 0; test di configurazione).
- Requisiti: AI-01, TEC-10 (riformulato in fase 1).
