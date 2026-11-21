# 06 - Regole e brief per il prototipo di creazione (fase 1b)

**Stato:** In attesa (l'agente prepara le regole e la bozza dei brief nella ondata 1b.1)
**Fase:** 1b

## Scopo

Approvare le regole delle Tipologie del prototipo. Scrivere i brief tipo per la prova 1. Il prototipo misura il tempo di revisione per la soglia G1.

## Scadenza

**3 gennaio 2027** per i brief corretti (decisione D1b-7). Il prototipo è pronto verso il 10 gennaio 2027. Il collaudo (file 07) deve chiudersi entro il 17 gennaio.

## Tempo stimato

- Regole delle Tipologie: circa 2,5 ore.
- Brief tipo: circa 1,5 ore, per correggere la bozza scritta da un agente Opus (3-4 brief per ciascuna delle quattro famiglie generate con l'AI).

## Prerequisiti

- Il file 05 è completato. Le regole di `cruciverba-classico` e `anagramma` sono già approvate.
- L'agente principale ti invia le regole delle Tipologie nuove.

NOTA: Il numero esatto delle Tipologie va confermato con l'agente. Le sei famiglie della prova sono crittografia breve, anagramma a tema, rebus, cruciverba classico, sudoku e giochi logici. I giochi logici sono due famiglie, futoshiki e nonogramma. Se le regole sono sette, il lavoro cresce di circa un'ora.

## Cosa ti serve

- Il messaggio dell'agente con le regole delle Tipologie.
- Un file di note fuori dal repository, per i brief.

## Procedura

### Parte 1 - Regole delle Tipologie

1. Leggi il testo delle regole di ogni Tipologia nuova (crittografia breve, rebus, sudoku, futoshiki, nonogramma).
   Risultato: Hai letto tutte le regole.

2. Controlla che le regole descrivano il gioco come lo conosci. Annota ogni correzione.
   Risultato: Hai una lista di correzioni, anche vuota.

3. Rispondi con una riga per Tipologia: "approvata" oppure "modifica:" con il testo corretto. Indica anche la data nel formato `AAAA-MM-GG`.
   Risultato: L'agente riceve la risposta. Poi scrive la data e la versione nel campo `regole` del descrittore, con un commit dedicato su tua richiesta.

### Parte 2 - Brief tipo

Un brief ha tre campi: **tema**, **vincoli**, **difficoltà**. La difficoltà va da 1 a 5.

4. Leggi la bozza dei brief per crittografia breve, anagramma a tema, rebus e cruciverba classico. L'agente la prepara dalle regole approvate.
   Risultato: Hai letto tutte le bozze.

5. Correggi ogni brief che non va: cambia tema, vincoli o difficoltà. Togli un brief se non serve. Aggiungine uno solo se una famiglia ne ha meno di tre.
   Risultato: Ogni famiglia ha tre o quattro brief corretti.

6. Per il cruciverba, controlla che i vincoli indichino la dimensione della griglia, per esempio "11x11".
   Risultato: Ogni brief di cruciverba ha una dimensione.

7. Invia i brief corretti all'agente principale.
   Risultato: L'agente li inserisce nella schermata "Nuovo candidato".

## Verifica finale

- Tutte le Tipologie nuove hanno la regola approvata o modificata, con data.
- Ogni famiglia generata con l'AI ha almeno tre brief.
- Il cruciverba ha brief con dimensione indicata.

## Cosa consegnare all'agente principale

1. Una riga per Tipologia: "approvata" o "modifica:" con il testo, e la data.
2. Tutti i brief, nel formato tema, vincoli, difficoltà.

## Cosa sblocca

- File 07: collaudo del prototipo.
- File 10: prova 1. Senza brief la prova non parte.

---
Fonti: `ROADMAP.md`, fase 1b, "Dal proprietario (circa 19,5 h)" e "Obiettivo" (sei famiglie); `docs/fase-1b-decisioni.md`, D1b-7; sezione 7 (domanda 12, chiusa l'11/11/2026); `.planning/phases/1/PLAN.md`, ondata 1.2 (regole in bozza per le Tipologie); `ROADMAP.md`, fase 1b, "Risultati consegnati" (campi del brief).
