# 08 - Correzione dei risultati attesi del Set di riferimento

**Stato:** Da fare (dipende dal file 02 e dall'ondata 1.2)
**Fase:** 1

## Scopo

Correggere i risultati attesi di 15-20 pagine del Set di riferimento. Ogni pagina ha un file JSON con i Giochi che la pagina contiene. Le misure di accuratezza confrontano il Motore di estrazione con questi file.

## Scadenza

**25 gennaio 2027**. La ondata 1.4 non parte senza i risultati attesi corretti (PLAN di fase 1).

## Tempo stimato

Circa 8-10 ore per 15-20 pagine (ROADMAP, fase 1). Se una pagina richiede più di 30 minuti, avvisa l'agente: è un segnale di rischio.

## Prerequisiti

- File 02 completato: PDF nella cartella del Set di riferimento.
- Ondata 1.2 completata. Il comando `dbe eval draft` è disponibile dalla fase 1, ondata 1.2.

## Cosa ti serve

- La cartella del Set di riferimento, per esempio `C:\DBE-Set-di-riferimento`.
- Un visualizzatore PDF.
- Le regole approvate del file 05.
- Il nome esatto del comando e dei parametri, dato dall'agente principale.

AVVERTENZA: Il Set di riferimento e i risultati attesi restano sul PC. Non copiarli nel repository. Non caricarli su GitHub.

## Procedura

1. Chiedi all'agente principale il comando esatto per creare le bozze, con i parametri. Il comando è `dbe eval draft`.
   Risultato: Hai il comando scritto.

2. Esegui il comando dalla cartella del repository. Usa la cartella del Set di riferimento.
   Risultato: Il comando crea una bozza JSON per pagina.

3. Apri il PDF della prima pagina. Apri la bozza JSON della stessa pagina.
   Risultato: Hai la pagina e la bozza una accanto all'altra.

4. Controlla ogni Gioco della pagina. Controlla la Tipologia, il titolo, il riquadro e i campi.
   Risultato: Ogni Gioco della pagina è nella bozza, con la Tipologia giusta.

5. Per il cruciverba, controlla le caselle nere e bianche e la numerazione. Controlla le definizioni lettera per lettera.
   Risultato: La griglia nella bozza coincide con la pagina.

6. Correggi la bozza. Salva il file JSON.
   Risultato: Il file è corretto. Il formato resta quello dell'agente.

7. Ripeti i passi 3-6 per tutte le pagine del Set.
   Risultato: Ogni pagina ha un file corretto.

8. Annota il tempo per pagina e le difficoltà.
   Risultato: Hai una lista di tempi e problemi.

## Verifica finale

- Ogni pagina del Set ha un file JSON.
- Il comando di controllo dell'agente non segnala errori di formato (chiedi il comando all'agente).
- Nessun file del Set è nel repository. Controlla con `git status --short`.

## Cosa consegnare all'agente principale

1. Numero di pagine corrette.
2. Tempo medio per pagina, in minuti.
3. Lista dei problemi: pagina, descrizione breve.
4. Esito del controllo di formato (sì o no).

## Cosa sblocca

- Misura di accuratezza di `dbe eval extraction` (ondate 1.2 e 1.4).
- Scelta del Fornitore AI predefinito (file 12).

---
Fonti: `ROADMAP.md`, fase 1, "Dal proprietario" (correggere i risultati attesi, circa 8-10 h), "Set di riferimento e misura di accuratezza" (conservazione, risultati attesi) e "Rischi" (più di 30 minuti per pagina); `.planning/phases/1/PLAN.md`, ondata 1.4 (blocco); `docs/adr/008-set-di-riferimento-solo-sul-pc.md`.
