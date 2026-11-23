# 07 - Collaudo del prototipo di revisione

**Stato:** In attesa (dipende dal ramo `fase-1b/ondata-1b.2` approvato in PR)
**Fase:** 1b, ondata 1b.2

## Scopo

Verificare che il prototipo si apra, generi e faccia rivedere i Giochi con il cronometro. Il collaudo copre sudoku, futoshiki e nonogramma veri, e le famiglie con AI riprodotte dalle Registrazioni sintetiche. Non fa chiamate a Claude Code.

## Scadenza

- **Data:** 7 dicembre 2026.
- Il collaudo chiude la 1b.2 insieme all'approvazione della PR. La pre-release `v0.2.0-prova.1` esce dalla CI dopo l'unione.

## Tempo stimato

Circa un'ora e mezza.

## Prerequisiti

- Il ramo `fase-1b/ondata-1b.2` è aggiornato e la CI è verde.
- PostgreSQL in esecuzione e configurazione impostata, come in `SETUP_INSTRUCTIONS.md` sezioni 3, 4 e 11.1.
- Node 24 (LTS) installato.
- L'interruttore delle chiamate dal vivo resta spento: `FornitoreAi__DalVivoAbilitato` vale `false`, che è il predefinito. Non lo cambiare.

## Cosa ti serve

- Un browser (Chrome o Edge) sul PC.
- Un file di note per gli attriti.

## Procedura

1. Avvia PostgreSQL dalla radice del repository con `docker compose up -d postgres`.
   Risultato: il container è in esecuzione.

2. Applica le migrazioni con `dotnet run --project src\DBEnigmistica.Console -- migrate`. Rispondi alla richiesta di conferma.
   Risultato: il comando termina senza errori.

3. Costruisci l'interfaccia. Nella cartella `interfaccia`, esegui `npm ci` e poi `npm run build`.
   Risultato: compare la cartella `interfaccia\dist` con `index.html`.

4. Dalla radice del repository, avvia il prototipo con `dotnet run --project src\DBEnigmistica.Console -- prototipo`.
   Risultato: compare la riga "Prototipo in ascolto su http://127.0.0.1:5180 (Ctrl+C per terminare)".

5. Apri `http://127.0.0.1:5180` nel browser.
   Risultato: compare la pagina "Inizio" con l'elenco delle Tipologie.

6. Sudoku. Apri *Nuovo candidato* per il sudoku, scegli la difficoltà e la dimensione 9, poi premi *Genera*.
   Risultato: compare il candidato in revisione con la griglia modificabile.

7. Nel sudoku, togli un indizio fino a lasciare più di una soluzione (per esempio, cancella una casella dato), poi rimettila.
   Risultato: il conflitto non compare come controllo nella lista. Il salvataggio non riesce e compare "Salvataggio non riuscito: " seguito dal motivo, per esempio "Il sudoku ha più di una soluzione.". *Accetta* è disabilitato. Quando rimetti il valore e il salvataggio riesce, *Accetta* torna disponibile.

8. Futoshiki. Ripeti i passi 6 e 7 con la dimensione 5 o 6. Cambia un segno tra due caselle in due modi: prima in modo che il gioco resti valido, poi in modo che rompa l'unicità della soluzione.
   Risultato: con la modifica valida il salvataggio riesce, compare "Modifiche salvate." e la lista dei controlli si aggiorna. Con la modifica che rompe l'unicità il salvataggio non riesce: compare "Salvataggio non riuscito: " seguito dal motivo, per esempio "Il gioco ha più di una soluzione.", e *Accetta* è disabilitato.

9. Nonogramma. Ripeti i passi 6 e 7 con dimensione 10. Cambia una casella in due modi: prima con una modifica che lascia la soluzione unica, poi con una che la rende multipla. Gli indizi si ricalcolano a ogni modifica, quindi dallo schermo non puoi scrivere una griglia che li contraddica.
   Risultato: con la modifica che lascia la soluzione unica il salvataggio riesce, compare "Modifiche salvate." e gli indizi ai margini restano visibili. Con la modifica che rende la soluzione multipla il salvataggio non riesce: compare "Salvataggio non riuscito: " seguito dal motivo, per esempio "Gli indizi hanno più di una soluzione: la soluzione deve essere unica.", e *Accetta* è disabilitato.

10. Cronometro. Mentre lavori, controlla il contatore "Tempo attivo di revisione".
    Risultato: il contatore sale mentre lavori. Lascia la pagina ferma per almeno tre minuti: il contatore non sale durante la pausa.

11. Ricaricamento. Premi F5 a metà di una revisione.
    Risultato: il contatore riparte dal tempo già salvato, non da zero. Le modifiche già salvate restano.

12. Accetta e Scarta. Su un candidato, premi *Accetta*. Su un altro, scegli un motivo dalla lista, scrivi una nota e premi *Scarta*.
    Risultato: l'esito è registrato e si passa al candidato successivo. *Scarta* resta spento finché non scegli il motivo.

13. Famiglie con AI, in riproduzione. Chiudi il prototipo (Ctrl+C) e riavvialo con le variabili della sezione "Avvio in riproduzione" sotto. Poi, per ciascuno dei cinque brief della tabella, apri *Nuovo candidato*, inserisci i valori indicati e premi *Genera*, una sola volta per brief.
    Risultato: compare la scritta "Regole in bozza: la generazione usa le Registrazioni in modalità riproduci, senza l'AI dal vivo", *Genera* è attivo, e compare un candidato in revisione con i valori del brief.
    Un brief diverso da quelli della tabella, o un valore cambiato, non trova la Registrazione: compare il messaggio "Nessuna Registrazione per questo brief: usa uno dei brief del collaudo.". Annota il brief e il messaggio: il collaudo di quella famiglia non è riuscito.

14. Rebus. Apri il candidato rebus del brief `rebus-collaudo`.
    Risultato: le vignette sono descritte a parole. Non ci sono immagini né campi per file.

15. Senza riproduzione. Chiudi il prototipo, riavvialo con la configurazione normale (senza le variabili di riproduzione) e apri *Nuovo candidato* per una famiglia con AI.
    Risultato: *Genera* è disattivato e accanto al pulsante compare il messaggio "Le chiamate dal vivo sono disattivate. Si abilitano con FornitoreAi:DalVivoAbilitato=true solo dopo la prova 1.1-prova (PROGRESS.md).". Nessuna chiamata parte. Sudoku e giochi logici continuano a funzionare.
    Con una Tipologia in bozza e le chiamate dal vivo attive, il messaggio è "Regole in bozza: la generazione con l'AI non è disponibile finché il proprietario non approva le regole di questa Tipologia.".

## Avvio in riproduzione (passo 13)

Imposta queste variabili d'ambiente (nomi, nessun segreto) prima di `dbe prototipo`:

- `ConnectionStrings__DefaultConnection`: la stessa del resto del collaudo.
- `FornitoreAi__Tipo=claude-code`
- `FornitoreAi__Creazione__Modello=claude-opus-5-5`
- `FornitoreAi__Creazione__Registrazioni__Modalita=riproduci`
- `FornitoreAi__Creazione__Registrazioni__Cartella=<percorso assoluto di tests\DBEnigmistica.Tests\Fixtures\RegistrazioniCreazione>`
- `FornitoreAi__DalVivoAbilitato` resta `false`.

Il modello deve essere esattamente `claude-opus-5-5`. Con un brief diverso dai cinque della tabella, la Registrazione manca e la generazione fallisce.

| Brief | Tipologia | Campi da inserire | Candidato atteso |
|---|---|---|---|
| `crit-sin-collaudo` | `crittografia-breve`, sottotipo `sinonimica` | Tema `il mercato`; Vincoli `Esposto di al massimo 2 parole`; Difficoltà 2 (Facile) | esposto `CENA SOVRANO`, soluzione `PASTORE` |
| `crit-per-collaudo` | `crittografia-breve`, sottotipo `perifrastica` | Tema vuoto; Vincoli vuoti; Difficoltà 3 (Media) | esposto `TERZA CIFRA E RIFIUTO`, soluzione `TRENO` |
| `anag-collaudo` | `anagramma` | Tema `il mare`; Vincoli `Anagramma semplice: una parola diventa un'altra parola` e `Esposto di 2-4 versi` (due righe); Difficoltà 2 (Facile) | parola `mare`, soluzione `rame` |
| `rebus-collaudo` | `rebus` | Tema vuoto; Vincoli `Una sola vignetta` e `Al massimo 3 figure` (due righe); Difficoltà 2 (Facile) | soluzione `STRANA IDEA`, due vignette descritte a parole |
| `cruci-collaudo` | `cruciverba-classico` | Tema vuoto; Vincoli `Definizioni brevi, al massimo 8 parole`; Difficoltà 2 (Facile); Dimensioni quadrata 9 x 9 | griglia 9x9 con definizioni inventate |

Ogni tentativo crea un candidato nel database e aumenta la variante del brief. Un secondo tentativo con lo stesso brief non trova più la Registrazione. Per ripartire da zero, l'unico comando documentato è quello che svuota il database locale (sezione 3 di `SETUP_INSTRUCTIONS.md`, opzione A): `docker compose down -v`, poi `docker compose up -d postgres`, poi `dotnet run --project src\DBEnigmistica.Console -- migrate`.

**ATTENZIONE: `docker compose down -v` cancella TUTTI i dati locali del database, non solo i candidati del collaudo: candidati, revisioni e ogni altro dato.** L'Archivio delle Riviste è una cartella sul disco (`Archivio__Cartella`), non sta nel volume di Docker, quindi questo comando non la cancella. Le Registrazioni nella loro cartella restano. Se vuoi cancellare solo i candidati del collaudo, chiedi all'agente principale: non esiste un comando per farlo.

I testi dei candidati sono inventati e fissi: il collaudo prova il flusso, non la qualità dei testi. I dettagli dei brief sono in `.planning/phases/1b/brief-collaudo-1b.2.md`.

16. Annota ogni problema nel file di note: schermata, passo, cosa ti aspettavi, cosa è successo.
    Risultato: hai la lista dei problemi per l'agente principale.

17. Ferma il prototipo con Ctrl+C.
    Risultato: il programma termina con codice 0.

## Verifica finale

- Sudoku, futoshiki e nonogramma si generano e si modificano. I controlli si aggiornano.
- Il cronometro conta, si ferma nelle pause lunghe e riprende dopo il ricaricamento.
- *Accetta* e *Scarta* registrano l'esito. *Scarta* richiede un motivo.
- Le famiglie con AI rifiutano la generazione senza riproduzione e non fanno chiamate.
- La lista dei problemi è consegnata all'agente principale.
