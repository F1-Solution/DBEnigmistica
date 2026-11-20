# 11 - Decisione G1 sul Motore di estrazione e di creazione

**Stato:** In attesa (dipende dal file 10)
**Fase:** 1b (soglia G1)

## Scopo

Decidere se la soglia G1 è superata sulla base della prova 1. Il piano commerciale fissa questa decisione al 28 febbraio 2027.

## Scadenza

**28 febbraio 2027.**

## Tempo stimato

Non stimato nella ROADMAP. La decisione richiede la lettura del rapporto e una risposta.

## Prerequisiti

- File 10 completato. Il rapporto `dbe eval review-time` è disponibile.
- Il rapporto G1 è allegato a una release (file 13).

## Cosa ti serve

- Il rapporto `dbe eval review-time`, in formato leggibile.
- Le soglie G1 (sotto).

## Soglie G1

- Crittografia breve e anagramma: mediana di revisione ≤ 20 minuti.
- Cruciverba classico: mediana di revisione ≤ 60 minuti.
- Almeno il 50% dei candidati accettati.

Con meno di 10 candidati per famiglia, il rapporto mostra "misura indicativa". Rebus, sudoku e giochi logici non hanno soglia G1.

## Procedura

1. Apri il rapporto leggibile. Leggi la mediana e la quota accettata per le tre famiglie con soglia: crittografia breve, anagramma, cruciverba.
   Risultato: Hai i tre valori.

2. Controlla se il rapporto dice "misura indicativa". Se lo dice, annotalo nella decisione.
   Risultato: Sai se la misura è indicativa.

3. Confronta ogni valore con la soglia. Usa le soglie della sezione "Soglie G1".
   Risultato: Hai tre confronti.

4. Scegli l'esito. Decidi tra tre esiti:
   - **continua**: le soglie sono superate.
   - **cambia**: le soglie non sono superate. La ROADMAP prevede una nuova misura entro il 31 marzo 2027, sul prototipo migliorato.
   - **fermati**: come da piano commerciale, si ferma la strategia commerciale. Le fasi 2-5 proseguono solo per uso personale. Le fasi 6-9 si annullano.
   Risultato: Hai un esito.

5. Chiedi all'agente principale di porre la decisione con la skill `grill-with-ui`. Rispondi nella pagina.
   Risultato: La decisione è registrata nel documento di decisione.

## Verifica finale

- La decisione è scritta con data.
- La decisione cita il rapporto e il numero di candidati per famiglia.

## Cosa consegnare all'agente principale

1. Esito: "continua", "cambia" o "fermati".
2. Data della decisione.
3. Una riga di motivazione.

## Cosa sblocca

- Con "continua": il piano prosegue come da ROADMAP, fasi 2-9.
- Con "cambia": nuova misura entro il 31 marzo 2027.
- Con "fermati": il piano commerciale cambia. Il file 23 va rivisto.

---
Fonti: `ROADMAP.md`, fase 1b, "Obiettivo" e "Criteri di accettazione" 6; sezione 5.3 (soglia G1, esito "cambia" e "fermati"); sezione 7, decisione del 28 febbraio 2027 come da piano commerciale; `docs/business/05-piano-commerciale.md`, sezione 8.2 (soglie "continua / cambia / fermati").
