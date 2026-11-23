# 05 - Approvazione delle regole delle quattro Tipologie della fase 1

**Stato:** In attesa. Le bozze arrivano entro il 27/11/2026 dalla ondata 1.2.
**Fase:** 1

## Scopo

Approvare il testo delle regole di quattro Tipologie: `cruciverba-classico`, `anagramma`, `sciarada` e `cambio-di-lettera`. Le regole sono il testo che descrive il gioco all'AI. L'approvazione conta per il Motore di creazione.

## Scadenza

Le bozze arrivano entro il 27/11/2026. L'approvazione di tutte e 4 le regole è entro il **4 dicembre 2026**. La ondata 1b.2 aspetta almeno le regole di `cruciverba-classico` e `anagramma`.

## Tempo stimato

Circa 1 ora (ROADMAP, fase 1).

## Prerequisiti

- L'agente principale ti invia il testo delle regole, con la versione.
- Conosci le regole dei giochi. Non serve competenza tecnica.

## Cosa ti serve

- Il messaggio dell'agente con il testo delle regole.
- Un editor di testo, se vuoi scrivere le modifiche tu.

## Procedura

1. Leggi il testo delle regole della prima Tipologia, `cruciverba-classico`.
   Risultato: Hai letto il testo.

2. Controlla che le regole descrivano il gioco come lo conosci. Annota ogni errore o parola da cambiare.
   Risultato: Hai una lista di correzioni, anche vuota.

3. Ripeti i passi 1 e 2 per `anagramma`, `sciarada` e `cambio-di-lettera`.
   Risultato: Hai una lista di correzioni per ogni Tipologia.

4. Rispondi all'agente con una riga per Tipologia. Scrivi "approvata" oppure "modifica:" seguito dal testo corretto.
   Risultato: Hai una risposta per ogni Tipologia.

5. Indica il valore da scrivere nel campo `approvate_da`. Per esempio, il tuo nome e cognome.
   Risultato: L'agente sa quale valore inserire.

6. Indica la data di approvazione. Usa il formato `AAAA-MM-GG`.
   Risultato: La data è scritta nella risposta.

NOTA: Ogni modifica delle regole alza la versione delle regole. Cambia anche l'impronta del prompt. Le risposte registrate del modello diventano non valide (vedi `progetto-1.1.md`, sezione 5.3). Questo accade anche dopo l'approvazione. Dopo ogni approvazione l'agente ricalcola da solo le Registrazioni del Set sintetico: tu non esegui nulla.

## Verifica finale

- Hai approvato o modificato tutte e quattro le Tipologie.
- L'agente conferma che, nel file di ogni descrittore, `stato` è `approvata`.
- L'agente conferma che `approvate_il` è nel formato `AAAA-MM-GG`.

## Cosa consegnare all'agente principale

Scrivi un messaggio con:

1. Una riga per Tipologia: "approvata" oppure "modifica:" con il testo corretto.
2. Il valore per `approvate_da`.
3. La data di approvazione, nel formato `AAAA-MM-GG`.

## Cosa sblocca

- Ondata 1b.2: descrittori v1 di `cruciverba-classico` e `anagramma`, con le regole approvate.
- Ondata 1.2: nessuna dipendenza. Le tabelle delle Tipologie si applicano con SQL rivisto e unito in `main` (decisione D2): lo stato delle regole conta solo per la creazione.

---
Fonti: `ROADMAP.md`, fase 1, "Dal proprietario" (approvare le regole, circa 1 ora) e sezione 2.2 (regole approvate); `.planning/phases/1/PLAN.md`, ondata 1.2 e tabella delle date; `.planning/phases/1/progetto-1.1.md`, sezioni 2 (campo `regole`) e 5.3 (versione del prompt).
