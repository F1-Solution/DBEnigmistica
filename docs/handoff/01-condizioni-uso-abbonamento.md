# 01 - Verifica delle condizioni d'uso dell'abbonamento

**Stato:** Da fare
**Fase:** 1 (domanda aperta 13 della ROADMAP)

## Scopo

Verificare se l'abbonamento Claude consente l'uso non interattivo di Claude Code sul PC del proprietario. La risposta decide se la prova del file 03 si fa, e come prosegue la fase 1.

## Scadenza

- **6 dicembre 2026** per l'abbonamento. La ROADMAP citava il 7 dicembre; la data è fissata al 6 dicembre dal 20/11/2026 (`docs/fase-1b-decisioni.md`).
- Per i piani gratuiti: prima dell'ondata 1.4, cioè dal 25 gennaio 2027. Vedi il file 09.

## Tempo stimato

Circa 2 ore. La ROADMAP stima queste 2 ore per l'abbonamento e i piani gratuiti insieme.

## Prerequisiti

- Un abbonamento Claude attivo.
- Browser.

## Cosa ti serve

- I termini d'uso del tuo piano Claude.
- La documentazione di Claude Code sull'uso non interattivo: https://code.claude.com/docs/en/headless
- Un file di note fuori dal repository, per annotare le risposte.

NOTA: Le condizioni cambiano nel tempo. Annota la data della verifica e la pagina che hai letto.

## Procedura

1. Apri nel browser i termini d'uso del tuo piano Claude. Cerca i paragrafi su automazione, uso programmatico e strumenti da riga di comando.
   Risultato: Hai la pagina dei termini aperta.
2. Apri la documentazione di Claude Code sull'uso non interattivo: https://code.claude.com/docs/en/headless
   Risultato: Sai se la documentazione descrive l'uso di `claude -p` in uno script.
3. Rispondi alle cinque domande. Per ogni domanda scrivi "Sì", "No" o "Non chiaro". Aggiungi una citazione di massimo due righe dalla pagina.
   - a. Il piano consente di usare Claude Code da riga di comando (`claude -p`) sul mio PC, per uno strumento del mio progetto?
   - b. Il piano consente di elaborare automaticamente molte pagine, in più chiamate?
   - c. Quali limiti si applicano? Scrivi la finestra di 5 ore e la quota settimanale, se indicate.
   - d. Il login con l'abbonamento è ammesso in un container o in un servizio? (serve per il file 15)
   - e. I contenuti inviati sono usati per addestrare i modelli? Quale impostazione va scelta?
   Risultato: Hai cinque risposte con citazioni.
4. Per ogni risposta "Non chiaro", non decidere da solo. Scrivi la domanda nel file di note.
   Risultato: Hai la lista delle domande aperte.
5. Invia la risposta all'agente principale. Vedi la sezione "Cosa consegnare".
   Risultato: L'agente registra l'esito.

Esito possibile:

- Domande a e b sono "Sì": la prova del file 03 si può fare.
- Domanda a o b è "No": non fare la prova. Aspetta la decisione dell'agente principale. Un adattatore API comporta una spesa, e la decisione di spesa è tua.
- Una risposta è "Non chiaro": la prova non si fa senza una tua approvazione scritta (criterio di uscita della ondata 1.1). L'agente ti pone la scelta con la skill `grill-with-ui`.

## Verifica finale

- Hai la data della verifica.
- Hai cinque risposte, ciascuna con una citazione.
- Hai l'indirizzo della pagina dei termini che hai letto.

## Cosa consegnare all'agente principale

Scrivi un messaggio con:

1. Data della verifica.
2. Le cinque risposte (a-e), con "Sì", "No" o "Non chiaro".
3. Una citazione per risposta, massimo due righe.
4. L'indirizzo della pagina dei termini.

Non inviare altri dati personali.

## Cosa sblocca

- File 03: prova reale di Claude Code (con domande a e b a "Sì").
- File 09: verifica dei piani gratuiti, con lo stesso metodo.
- File 15: decisione sul container (fase 2), con la domanda d.

---
Fonti: `ROADMAP.md`, sezione 7 (domanda 13) e fase 1 ("Dal proprietario"); `.planning/phases/1/PLAN.md`, criteri di uscita della 1.1 e rischio 4 del piano; `.planning/phases/1/prova-claude-code-1.1.md`, sezione 3.
