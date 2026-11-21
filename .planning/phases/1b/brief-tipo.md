# Fase 1b - Brief tipo in bozza

Scritto il 20/11/2026 dall'agente A (Opus), ondata 1b.1. Decisione D1b-7: un agente scrive i brief in bozza dalle regole; il proprietario li corregge. Scadenza: 3/1/2027. Tempo previsto: circa 1,5 ore.

## Che cosa sono

Un brief dice al Motore di creazione che gioco si vuole: tema, vincoli, difficoltà e, se serve, dimensioni e sottotipo. Il prompt contiene solo il brief e le regole approvate della Tipologia. Ogni famiglia con AI ha 4 brief. I 5 candidati della prova 1 usano i brief nell'ordine 1, 2, 3, 4, 1: il quinto riusa il primo, e il programma ne fa una variante diversa.

Formato: `tipologie/_meta/brief.schema.json`. Regole di compilazione e significato dei campi: `.planning/phases/1b/progetto-1b.1.md`, sezioni 2 e 6.

Sudoku, futoshiki e nonogramma non hanno brief tipo: si sceglie difficoltà e dimensione nella schermata "Nuovo candidato".

## Come correggerli

Per ogni brief, in chat:
1. Leggi tema, vincoli e difficoltà. Cambia ciò che non ti convince, o scrivi "va bene".
2. Puoi togliere o aggiungere vincoli: al massimo 8, una frase breve ciascuno, su una riga.
3. Difficoltà da 1 (molto facile) a 5 (molto difficile).
4. Non aggiungere esempi di giochi già pubblicati: il prompt non li può contenere (`CLAUDE.md`, sezione 5).

Dopo la correzione l'agente aggiorna questo file e i brief diventano quelli della prova. I brief restano in bozza finché le regole della Tipologia non sono approvate: senza regole approvate la generazione dal vivo non parte.

Gli id `anagramma` e `cruciverba-classico`, con la versione 1, sono quelli dei descrittori dell'ondata 1.2. Se la 1.2 li cambia, l'agente li allinea qui.

## 1. Crittografia breve (`crittografia-breve`, v0)

Sottotipi: 3 candidati sinonimici (brief 1, 3 e il quinto) e 2 perifrastici (brief 2 e 4).

**Brief 1.** Sinonimica, tema in cucina, facile.

```json
{
  "versione_formato": 1,
  "codice": "crit-01",
  "tipologia": "crittografia-breve",
  "versione_tipologia": 0,
  "famiglia": "crittografia",
  "tema": "la cucina e il cibo",
  "vincoli": ["Esposto di al massimo 3 parole", "Soluzione di una o due parole"],
  "difficolta": 2,
  "dimensioni": null,
  "sottotipo": "sinonimica"
}
```

**Brief 2.** Perifrastica, tema geografico, media.

```json
{
  "versione_formato": 1,
  "codice": "crit-02",
  "tipologia": "crittografia-breve",
  "versione_tipologia": 0,
  "famiglia": "crittografia",
  "tema": "le città e i luoghi d'Italia",
  "vincoli": ["La perifrasi indica una città, un fiume o una regione italiana", "Esposto di al massimo 6 parole"],
  "difficolta": 3,
  "dimensioni": null,
  "sottotipo": "perifrastica"
}
```

**Brief 3.** Sinonimica, tema libero, media.

```json
{
  "versione_formato": 1,
  "codice": "crit-03",
  "tipologia": "crittografia-breve",
  "versione_tipologia": 0,
  "famiglia": "crittografia",
  "tema": null,
  "vincoli": ["Esposto fatto di lettere e di una sola parola", "Soluzione che sia un modo di dire o un'espressione comune"],
  "difficolta": 3,
  "dimensioni": null,
  "sottotipo": "sinonimica"
}
```

**Brief 4.** Perifrastica, tema animali, facile.

```json
{
  "versione_formato": 1,
  "codice": "crit-04",
  "tipologia": "crittografia-breve",
  "versione_tipologia": 0,
  "famiglia": "crittografia",
  "tema": "gli animali",
  "vincoli": ["La perifrasi descrive un animale noto", "Esposto di al massimo 5 parole"],
  "difficolta": 2,
  "dimensioni": null,
  "sottotipo": "perifrastica"
}
```

## 2. Anagramma a tema (`anagramma`, v1)

**Brief 1.** Il mare, facile.

```json
{
  "versione_formato": 1,
  "codice": "anag-01",
  "tipologia": "anagramma",
  "versione_tipologia": 1,
  "famiglia": "gioco-breve",
  "tema": "il mare",
  "vincoli": ["Anagramma semplice: una parola diventa un'altra parola", "Parole da 4 a 8 lettere", "Esposto di 2-4 versi"],
  "difficolta": 2,
  "dimensioni": null,
  "sottotipo": null
}
```

**Brief 2.** La montagna, media.

```json
{
  "versione_formato": 1,
  "codice": "anag-02",
  "tipologia": "anagramma",
  "versione_tipologia": 1,
  "famiglia": "gioco-breve",
  "tema": "la montagna",
  "vincoli": ["Anagramma semplice: una parola diventa un'altra parola", "Parole da 5 a 9 lettere", "Esposto di 2-4 versi"],
  "difficolta": 3,
  "dimensioni": null,
  "sottotipo": null
}
```

**Brief 3.** La cucina, facile.

```json
{
  "versione_formato": 1,
  "codice": "anag-03",
  "tipologia": "anagramma",
  "versione_tipologia": 1,
  "famiglia": "gioco-breve",
  "tema": "la cucina italiana",
  "vincoli": ["Almeno una delle due parole è un ingrediente o un piatto", "Parole da 4 a 8 lettere", "Esposto in prosa, al massimo 3 frasi"],
  "difficolta": 2,
  "dimensioni": null,
  "sottotipo": null
}
```

**Brief 4.** Le stagioni, media.

```json
{
  "versione_formato": 1,
  "codice": "anag-04",
  "tipologia": "anagramma",
  "versione_tipologia": 1,
  "famiglia": "gioco-breve",
  "tema": "le stagioni e il tempo atmosferico",
  "vincoli": ["Anagramma semplice: una parola diventa un'altra parola", "Parole da 5 a 9 lettere", "Esposto di 2-4 versi"],
  "difficolta": 3,
  "dimensioni": null,
  "sottotipo": null
}
```

## 3. Rebus (`rebus`, v0)

Le vignette sono descritte a parole. Nessuna immagine (criterio 8).

**Brief 1.** Tema libero, una vignetta, facile.

```json
{
  "versione_formato": 1,
  "codice": "rebus-01",
  "tipologia": "rebus",
  "versione_tipologia": 0,
  "famiglia": "rebus",
  "tema": null,
  "vincoli": ["Una sola vignetta", "Da 2 a 3 figure", "Soluzione di 2 o 3 parole"],
  "difficolta": 2,
  "dimensioni": null,
  "sottotipo": null
}
```

**Brief 2.** La scuola, due vignette, media.

```json
{
  "versione_formato": 1,
  "codice": "rebus-02",
  "tipologia": "rebus",
  "versione_tipologia": 0,
  "famiglia": "rebus",
  "tema": "la scuola",
  "vincoli": ["Due vignette", "Da 3 a 4 figure in tutto", "Almeno una figura che compie un'azione"],
  "difficolta": 3,
  "dimensioni": null,
  "sottotipo": null
}
```

**Brief 3.** Modo di dire, media.

```json
{
  "versione_formato": 1,
  "codice": "rebus-03",
  "tipologia": "rebus",
  "versione_tipologia": 0,
  "famiglia": "rebus",
  "tema": null,
  "vincoli": ["La soluzione è un modo di dire o un proverbio italiano noto", "Una o due vignette", "Al massimo 4 figure"],
  "difficolta": 3,
  "dimensioni": null,
  "sottotipo": null
}
```

**Brief 4.** Il mare, facile.

```json
{
  "versione_formato": 1,
  "codice": "rebus-04",
  "tipologia": "rebus",
  "versione_tipologia": 0,
  "famiglia": "rebus",
  "tema": "il mare e la spiaggia",
  "vincoli": ["Una sola vignetta ambientata in spiaggia", "Da 2 a 3 figure", "Soluzione di 2 o 3 parole"],
  "difficolta": 2,
  "dimensioni": null,
  "sottotipo": null
}
```

## 4. Cruciverba classico (`cruciverba-classico`, v1)

Lo schema delle caselle nere lo genera il programma (D1b-4). I brief 1, 2 e 3 sono di 11x11 o più: con l'ordine 1, 2, 3, 4, 1 almeno 3 candidati su 5 sono di 11x11 o più (in questo caso 4 su 5).

**Brief 1.** 11x11, tema libero, facile.

```json
{
  "versione_formato": 1,
  "codice": "cruci-01",
  "tipologia": "cruciverba-classico",
  "versione_tipologia": 1,
  "famiglia": "cruciverba",
  "tema": null,
  "vincoli": ["Definizioni di al massimo 8 parole", "Nomi propri solo se molto noti", "Nessuna sigla poco nota"],
  "difficolta": 2,
  "dimensioni": { "righe": 11, "colonne": 11 },
  "sottotipo": null
}
```

**Brief 2.** 13x13, tema cinema italiano, media.

```json
{
  "versione_formato": 1,
  "codice": "cruci-02",
  "tipologia": "cruciverba-classico",
  "versione_tipologia": 1,
  "famiglia": "cruciverba",
  "tema": "il cinema italiano",
  "vincoli": ["Le parole più lunghe sono legate al tema", "Definizioni di al massimo 8 parole", "Nomi propri solo se molto noti"],
  "difficolta": 3,
  "dimensioni": { "righe": 13, "colonne": 13 },
  "sottotipo": null
}
```

**Brief 3.** 11x11, tema cucina, media.

```json
{
  "versione_formato": 1,
  "codice": "cruci-03",
  "tipologia": "cruciverba-classico",
  "versione_tipologia": 1,
  "famiglia": "cruciverba",
  "tema": "la cucina italiana",
  "vincoli": ["Alcune parole lunghe legate al tema", "Definizioni di al massimo 8 parole", "Nessuna parola straniera, salvo quelle entrate nell'uso"],
  "difficolta": 3,
  "dimensioni": { "righe": 11, "colonne": 11 },
  "sottotipo": null
}
```

**Brief 4.** 9x9, tema libero, molto facile.

```json
{
  "versione_formato": 1,
  "codice": "cruci-04",
  "tipologia": "cruciverba-classico",
  "versione_tipologia": 1,
  "famiglia": "cruciverba",
  "tema": null,
  "vincoli": ["Definizioni dirette, al massimo 6 parole", "Solo parole di uso comune"],
  "difficolta": 1,
  "dimensioni": { "righe": 9, "colonne": 9 },
  "sottotipo": null
}
```
