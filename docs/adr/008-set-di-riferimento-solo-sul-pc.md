# ADR 008: Set di riferimento e Registrazioni solo sul PC del proprietario

- **Stato:** Accettato il 19/11/2026
- **Data:** 19/11/2026
- **Fonte:** `docs/fase-1-decisioni.md`, decisione D2

## Contesto

Il Motore di estrazione si misura su un Set di riferimento: Riviste reali con i risultati attesi corretti dal proprietario (ADR 003, punto 6). Una Registrazione è la risposta del Fornitore AI per una pagina, salvata una volta e riprodotta nei test senza chiamate. Anche le Registrazioni derivano dalle Riviste.

Il ROADMAP prevedeva un repository GitHub per il Set di riferimento. Il parere legale sulle Riviste arriva entro il 28/2/2027, quindi il contenuto non può ancora andare su macchine di terzi.

## Decisione

1. Il Set di riferimento, i risultati attesi e le Registrazioni stanno solo sul PC del proprietario, mai su GitHub.
2. La CI su GitHub esegue i test sul Set sintetico, prodotto dal progetto senza contenuti di Riviste.
3. La regressione sul Set di riferimento è uno script locale. Il proprietario lo lancia prima di ogni release e allega il rapporto alla release.
4. È un'eccezione dichiarata alla regola «test e pubblicazione solo con GitHub Actions». Vale solo per la regressione sul Set di riferimento. Build, test e pubblicazione restano su GitHub Actions.
5. Non si crea il repository `DBEnigmistica-refset`. La sezione «Conservazione» del ROADMAP cambia di conseguenza, e il criterio 3 della fase 1 si verifica in locale.

## Alternative scartate

- **Repository `refset` su GitHub con chiave di sola lettura**: metterebbe i contenuti delle Riviste su macchine di terzi prima del parere legale.
- **Runner self-hosted sul PC, obbligatorio per `publish.yml`**: costa installazione e manutenzione, e bloccherebbe la pubblicazione a PC spento.
- **Scegliere il repository per ora e spostare i dati dopo**: rischia di dover cancellare dati già finiti su GitHub.

## Conseguenze

- La regressione sul Set di riferimento non gira in CI. Un peggioramento si vede solo quando il proprietario lancia lo script.
- Lo script può essere dimenticato. Contromisura: la checklist di release richiede il rapporto allegato.
- Il Set sintetico non misura l'accuratezza sulle Riviste reali. Le soglie si confermano solo con il Set di riferimento, a fine fase 1.
- La custodia del Set di riferimento e delle Registrazioni è responsabilità del proprietario, sul suo PC.

## Riferimenti

- `docs/fase-1-decisioni.md`, D2 e la sezione «Termini».
- `ROADMAP.md`, sezione «Conservazione» e criterio 3 della fase 1 (da aggiornare).
- ADR 003, punto 6 (misura su un set di riferimento).
- Regola di test e pubblicazione: `CLAUDE.md`, sezione 4.
