# 12 - Scelta del Fornitore AI predefinito e conferma delle soglie di accuratezza

**Stato:** In attesa (dipende dalla ondata 1.4)
**Fase:** 1

## Scopo

Scegliere il Fornitore AI predefinito per l'estrazione, sul rapporto di confronto. Confermare le soglie di accuratezza dopo la prima misura di riferimento.

## Scadenza

**21 febbraio 2027**, fine della fase 1.

## Tempo stimato

Non stimato separatamente. È parte delle circa 20 ore della fase 1.

## Prerequisiti

- Ondata 1.4 completata. Il rapporto di confronto dei Fornitori AI è pronto.
- File 01 e file 09: condizioni verificate per ogni fornitore candidato.

## Cosa ti serve

- Il rapporto di confronto (metriche, costo e pagine lette per finestra dell'abbonamento).
- Le soglie della tabella sotto.

## Regola di scelta

Il Fornitore AI predefinito è quello che supera le soglie, al costo più basso. Deve avere condizioni d'uso che tu hai verificato.

Se nessun fornitore supera le soglie, resta Claude con l'abbonamento. L'agente registra la decisione in un ADR.

## Soglie di accuratezza iniziali

Le soglie sono da confermare dopo la prima misura di riferimento.

| Metrica | PDF digitali | Scansioni |
|---|---|---|
| Richiamo dei Giochi | ≥ 97% | ≥ 93% |
| Precisione dei Giochi | ≥ 95% | ≥ 90% |
| Tipologia corretta | ≥ 97% | ≥ 93% |
| Caselle del cruciverba corrette | ≥ 99,5% | ≥ 98% |
| CER di definizioni ed esposti | ≤ 1% | ≤ 3% |
| Campi esatti dei giochi brevi | ≥ 95% | ≥ 90% |

## Procedura

1. Apri il rapporto di confronto. Leggi le metriche di ogni fornitore sul Set di riferimento.
   Risultato: Hai una tabella per fornitore.

2. Confronta ogni fornitore con la tabella delle soglie. Segna quali soglie supera.
   Risultato: Sai quali fornitori superano tutte le soglie.

3. Per i fornitori che superano le soglie, confronta il costo. Confronta anche le pagine lette per finestra dell'abbonamento.
   Risultato: Hai il fornitore meno costoso tra quelli idonei.

4. Verifica che le condizioni del fornitore scelto siano verificate (file 01 o file 09).
   Risultato: Hai la conferma delle condizioni.

5. Decidi se confermare le soglie della tabella, o proporre una modifica. Se proponi una modifica, scrivi la soglia nuova e il motivo.
   Risultato: Hai la decisione sulle soglie.

6. Comunica la scelta all'agente principale. L'agente la registra in configurazione e in un ADR.
   Risultato: La scelta è registrata.

## Verifica finale

- Il fornitore scelto supera le soglie confermate.
- Le condizioni del fornitore sono verificate.
- Le soglie confermate sono scritte con data.

## Cosa consegnare all'agente principale

1. Il fornitore predefinito scelto.
2. Le soglie confermate, oppure le modifiche con motivo.
3. Data della decisione.

## Cosa sblocca

- Chiusura della fase 1.
- Approvazione del tag `v0.2.0` (file 24).

---
Fonti: `ROADMAP.md`, sezione 2.4 (regola di scelta per l'estrazione), fase 1 ("Set di riferimento e misura di accuratezza", tabella delle soglie) e "Dal proprietario"; sezione 7 (domanda 6); `.planning/phases/1/PLAN.md`, criterio di accettazione 8 della fase 1.
