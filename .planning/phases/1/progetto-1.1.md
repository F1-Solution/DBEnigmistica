# Fase 1, ondata 1.1 - Documento di progetto (agente A)

Scritto il 20/11/2026 dall'agente A (Opus). Copre i compiti 1-8 dell'agente A in `PLAN.md`, sezione 3. Vincoli: ADR 002, 003, 005, 008-011, decisioni D1-D7 (`docs/fase-1-decisioni.md`), ROADMAP sezione 2.2 e fase 1.

File consegnati:

| File | Contenuto |
|---|---|
| `tipologie/_meta/descrittore.schema.json` | Meta-schema dei descrittori |
| `tipologie/_meta/risposta-pagina.schema.json` | Involucro generico della risposta per pagina |
| `tipologie/_meta/risultato-atteso.schema.json` | Risultato atteso per pagina |
| `tipologie/_meta/prompt-estrazione-v1.md` | Modello del prompt di estrazione v1 |
| `tipologie/_meta/esempi/**` | Esempi validi e non validi, inventati, più uno schema derivato di esempio |

Tutti gli schemi usano JSON Schema 2020-12. Gli esempi usano Tipologie con id `esempio-*`, per non confondersi con i descrittori veri della 1.2.

## 1. Libreria di validazione JSON Schema

**Scelta: `Corvus.Text.Json.Validator` 5.8.12** (Apache-2.0), da aggiungere a `Directory.Packages.props` dall'agente B.

Verifica del 20/11/2026 su nuget.org, GitHub e GitHub Advisory Database:

| Candidato | Versione | Licenza | Manutenzione | Vulnerabilità note | Bozze | Dipendenze (net10.0) | Esito |
|---|---|---|---|---|---|---|---|
| JsonSchema.Net | 9.7.0 (13/11/2026) | Sorgente MIT, **binari NuGet sotto "Open Source Maintenance Fee EULA"** (`OSMFEULA.txt` nel pacchetto) | Attiva | Nessuna | 6, 7, 2019-09, 2020-12 | JsonPointer.Net | **Scartato.** Chi usa i binari in attività con ricavi da 10.000 USD l'anno deve pagare un canone. DBEnigmistica è un prodotto commerciale: non è "MIT o equivalente" (PLAN, sezione 1) |
| NJsonSchema | 11.6.2 (11/11/2026) | MIT | Attiva | Nessuna sulla versione; Newtonsoft.Json < 13.0.1 ha due avvisi HIGH, la dipendenza chiede ≥ 13.0.3 | Pensata per la bozza 4, supporto parziale delle successive; niente 2020-12 completo | Newtonsoft.Json, Namotion.Reflection, NJsonSchema.Annotations | **Scartato.** Niente 2020-12 completo (`if/then/else`, `contains` con `maxContains`, `$defs`), e porta Newtonsoft.Json accanto a System.Text.Json |
| Corvus.Json.Validator | 5.8.12 (19/11/2026) | Apache-2.0 | Attiva | Nessuna | 4, 6, 7, 2019-09, 2020-12 | Roslyn (`Microsoft.CodeAnalysis.CSharp`) e 11 pacchetti di generazione del codice | **Scartato.** Compila il codice a runtime con Roslyn: decine di MB in più nell'immagine chiseled |
| **Corvus.Text.Json.Validator** | **5.8.12 (19/11/2026)** | **Apache-2.0** | Attiva: repository `corvus-dotnet/Corvus.JsonSchema`, ultimo push il 19/11/2026, non archiviato | **Nessuna** (anche per Corvus.Text.Json, Corvus.Regex.Ecma262, NodaTime) | 4, 6, 7, 2019-09, **2020-12** | Corvus.Text.Json 5.8.12 → Corvus.Regex.Ecma262 1.0.15 (Apache-2.0), NodaTime ≥ 3.3.1 (Apache-2.0). Nessuna libreria nativa | **Scelto** |

Motivi della scelta:
- Licenza Apache-2.0 su sorgente e binari, anche per le dipendenze.
- Valutatore a runtime senza generazione di codice e senza Roslyn: adatto agli schemi derivati dai descrittori, che non si conoscono in compilazione.
- Supporto completo della 2020-12, provato sugli schemi di questa ondata (sezione 9).
- I `pattern` seguono ECMA-262 come chiede lo standard, con un timeout di 1 secondo per corrispondenza. Gli schemi usano solo espressioni compatibili.

Righe per B:

```xml
<PackageVersion Include="Corvus.Text.Json.Validator" Version="5.8.12" />
```

Uso previsto dal 1.2 (A): `JsonSchema.FromText(testo)` una volta per schema, poi `Validate(json)`. Da verificare nella 1.2 l'API dei dettagli degli errori, che servono ai messaggi di `types generate` ed `eval draft`. Rischio: il pacchetto esce spesso (62 versioni dal 14/05/2026). Dependabot aggiorna la versione esatta; ogni aggiornamento passa dai test della 1.2.

## 2. Meta-schema dei descrittori

File: `tipologie/_meta/descrittore.schema.json`. Un descrittore sta in `tipologie/<id>/v<n>.json` e può dichiarare `"$schema": "../_meta/descrittore.schema.json"`.

### 2.1 Campi del descrittore

| Campo | Regola |
|---|---|
| `id` | `^[a-z][a-z0-9]*(-[a-z0-9]+)*$`, al massimo 30 caratteri. Uguale alla cartella |
| `versione` | Intero 1-999. Uguale a `<n>` del nome del file |
| `famiglia` | Lista chiusa: `cruciverba`, `gioco-breve`, `crittografia`, `rebus`, `logico` |
| `nome`, `descrizione` | Testi per il prompt e l'interfaccia. `descrizione` dice come si riconosce il Gioco |
| `regole` | Oggetto: `testo`, `versione`, `stato` (`bozza` o `approvata`), `scritte_da`; con `approvata` sono obbligatori `approvate_il` (AAAA-MM-GG) e `approvate_da`, con `bozza` sono vietati |
| `campi` | 1-60 campi |
| `elenchi` | 0-10 elenchi, ognuno con `nome` (al massimo 20 caratteri) e 1-30 campi. Un elenco diventa una tabella figlia |
| `resa` | `{ "modello": ... }`, lista chiusa dell'ADR 002, punto 11 |
| `validatori` | Nomi da lista chiusa, senza ripetizioni |

Nessuna proprietà fuori elenco è ammessa (`additionalProperties: false` a ogni livello). Un `default`, un `sql` o un tipo SQL scritto a mano fanno fallire la validazione.

### 2.2 Campo

| Proprietà | Regola |
|---|---|
| `nome` | `^[a-z][a-z0-9_]{0,40}$` (ADR 002, punto 5), escluse le parole riservate di PostgreSQL (riservate e "riservate, ammesse come funzione o tipo"), le colonne di sistema (`oid`, `tableoid`, `xmin`, `xmax`, `cmin`, `cmax`, `ctid`) e i nomi del generatore (`id`, `gioco_id`, `posizione`) |
| `tipo` | `testo`, `testo_lungo`, `intero`, `decimale`, `booleano`, `data`, `enumerazione`, `griglia`, `immagine` |
| `max` | 1-4000. Obbligatorio con `testo`, vietato con gli altri tipi |
| `valori` | 1-50 valori `^[a-z0-9_]{1,40}$`, senza ripetizioni. Obbligatorio con `enumerazione`, vietato con gli altri tipi |
| `obbligatorio` | Booleano, assente vale `false`. Solo questo produce `NOT NULL` |
| `indice` | Ammesso a `true` solo con `intero`, `testo`, `enumerazione`, `data` (ADR 002, punto 6) |
| `ruolo` | Facoltativo, lista chiusa (sezione 2.3) |
| `etichetta`, `descrizione` | Testi per il prompt e l'interfaccia |

Il tipo `griglia` non è ammesso nei campi di un elenco.

### 2.3 Ruoli, resa e validatori

Ruoli e tipo richiesto:

| Ruolo | Tipo | Significato |
|---|---|---|
| `righe`, `colonne` | `intero` | Dimensioni della griglia (1-60) |
| `griglia` | `griglia` | Caselle e numeri |
| `numero` | `intero` | Numero della definizione (1-999) |
| `direzione` | `enumerazione` | Orizzontale o verticale |
| `definizione`, `esposto` | `testo` o `testo_lungo` | Testi misurati con il CER |
| `diagramma`, `soluzione` | `testo` | Diagramma e soluzione stampati |
| `immagine` | `immagine` | Riquadro di un'immagine |

Ruoli richiesti dal modello di resa (controllati dal meta-schema con `contains`):

| Modello | Campi del Gioco | Elenco |
|---|---|---|
| `griglia` | `righe`, `colonne`, `griglia` | Almeno un elenco con `numero`, `direzione`, `definizione` |
| `esposto_diagramma` | `esposto`, `soluzione` | - |
| `immagine_diagramma` | `immagine`, `soluzione` | - |
| `griglia_logica` | `righe`, `colonne`, `griglia` | - |

Validatori ammessi: `griglia-coerente-con-dimensioni`, `numerazione-coerente-con-griglia`, `lunghezze-coerenti`, `incroci-coerenti`, `diagramma-coerente-con-soluzione`, `anagramma-stesse-lettere`, `cambio-di-una-lettera`. La 1.2 (B) li implementa. Un nome nuovo richiede un cambio del meta-schema e del codice nella stessa PR.

### 2.4 Controlli fuori da JSON Schema

JSON Schema non li esprime. Li fa il caricatore della 1.2 (A), dopo la validazione, con un messaggio per ognuno:

1. Nomi unici tra i `campi` del Gioco, tra gli `elenchi`, e tra i `campi` di ogni elenco. Un elenco e un campo del Gioco non possono avere lo stesso nome.
2. Ogni ruolo al massimo una volta nei campi del Gioco e una volta in ogni elenco.
3. `id` uguale alla cartella e `versione` uguale al nome del file.
4. Identificatori composti dal generatore entro 63 byte (sezione 3.1). Con i limiti del meta-schema (id 30, elenco 20) le tabelle restano entro 58 byte; i vincoli si accorciano con la regola della sezione 3.1.
5. Validatori applicabili: per esempio `numerazione-coerente-con-griglia` richiede il ruolo `griglia` e un elenco con `numero` e `direzione`. Ogni validatore dichiara i ruoli che gli servono.
6. Nessun file `v<n>.json` con lo stesso `id` e una `versione` già registrata con contenuto diverso nelle colonne (sezione 2.5).

### 2.5 Versioni

- `regole.versione` sale quando cambia il testo delle regole o la sua approvazione. Il file resta lo stesso: le tabelle non cambiano.
- `versione` sale quando cambiano `campi`, `elenchi`, `resa` o `validatori`. Si crea un file nuovo `v<n+1>.json` e il vecchio resta.
- In fase 1 il generatore produce solo la creazione delle tabelle di una versione. Il passaggio di dati tra versioni si progetta in fase 4, con `dbe types apply`.
- Ogni cambio di regole o struttura cambia la versione del prompt (sezione 5.3), quindi invalida le Registrazioni.

## 3. Regole di traduzione

### 3.1 Descrittore → SQL

File prodotti: `tipologie/<id>/v<n>.riviste.sql` e `tipologie/<id>/v<n>.catalogo.sql`. Stesse strutture nei due schemi; cambiano solo il riferimento al nucleo e il tipo della chiave.

**Nomi.**
- `<base>` = `gioco_` + `id` con `-` sostituito da `_`. Esempio: `cruciverba-classico` → `gioco_cruciverba_classico`.
- Tabella 1:1: `<schema>."<base>"`.
- Tabella figlia di un elenco: `<schema>."<base>__<elenco>"` (doppio trattino basso: un id non contiene `--`, quindi il nome non è ambiguo).
- Vincoli e indici: `pk_<tabella>`, `fk_<tabella>__gioco`, `ck_<tabella>__<colonna>`, `ix_<tabella>__<colonna>`.
- Un nome oltre 63 byte diventa: primi 54 caratteri + `_` + primi 8 caratteri esadecimali dello SHA-256 (UTF-8) del nome intero. È deterministico.
- Tutti gli identificatori sono quotati con `"`. Lo schema è sempre esplicito (`riviste.` o `catalogo.`) e viene da una costante, mai dal descrittore.

**Riferimento al nucleo** (costanti del generatore, da allineare se la 1.2 B rinomina il nucleo):

| Schema | Tabella | Colonna | Tipo di `gioco_id` |
|---|---|---|---|
| `riviste` | `riviste."games"` | `"Id"` | `integer` |
| `catalogo` | `catalogo."giochi"` | `"id"` | `uuid` |

**Colonne.** Tabella 1:1: `gioco_id`, poi i campi nell'ordine del descrittore. Tabella figlia: `gioco_id`, `posizione` (`integer NOT NULL`, indice della voce nell'elenco da 0), poi i campi nell'ordine dell'elenco. Tipi SQL dalla tabella dell'ADR 002, punto 4. `NOT NULL` solo con `obbligatorio: true`. `enumerazione` produce `CHECK ("<col>" IN ('v1', 'v2'))` con i valori nell'ordine del descrittore. `immagine` è `uuid` senza chiave esterna in fase 1: la tabella dei ritagli non esiste ancora (va aggiunta quando nasce, con una versione nuova del descrittore).

**Vincoli, sempre dentro `CREATE TABLE`.**
- 1:1: `PRIMARY KEY ("gioco_id")`, che vale anche come `UNIQUE (gioco_id)` dell'ADR 002; `FOREIGN KEY ("gioco_id") REFERENCES <nucleo> ON DELETE CASCADE`.
- Figlia: `PRIMARY KEY ("gioco_id", "posizione")`, che vale anche come indice btree sulla chiave esterna; `FOREIGN KEY ("gioco_id") REFERENCES <schema>."<base>" ("gioco_id") ON DELETE CASCADE`; `CHECK ("posizione" >= 0)`.
- Mai `DEFAULT`, identity, sequenze, trigger, funzioni, `GRANT`, `ALTER`, `DROP`, `BEGIN`/`COMMIT`, `SET`.

**Indici.** Solo `CREATE INDEX "<nome>" ON <schema>."<tabella>" ("<col>");` per i campi con `indice: true`, dopo tutte le tabelle.

**Ordine deterministico.** Riga di intestazione `-- Generato da dbe types generate: <id> v<n>, schema <schema>. Non modificare a mano.`; riga vuota; tabella 1:1; tabelle figlie nell'ordine degli elenchi; indici della 1:1 poi delle figlie, nell'ordine dei campi. Parole chiave maiuscole, rientro di 4 spazi, una colonna per riga, `", "` tra i valori dei `CHECK`, una riga vuota tra le istruzioni, UTF-8 senza BOM, a capo LF, un a capo finale. Il checksum di `tipologie.migrazioni_applicate` è lo SHA-256 dei byte del file.

Esempio, dal descrittore `esempi/descrittore/valido-esempio-cruciverba.v1.json`, schema `riviste` (prodotto dallo script di riferimento della sezione 9):

```sql
-- Generato da dbe types generate: esempio-cruciverba v1, schema riviste. Non modificare a mano.

CREATE TABLE riviste."gioco_esempio_cruciverba" (
    "gioco_id" integer NOT NULL,
    "righe" integer NOT NULL,
    "colonne" integer NOT NULL,
    "griglia" jsonb NOT NULL,
    "autore" varchar(100),
    CONSTRAINT "pk_gioco_esempio_cruciverba" PRIMARY KEY ("gioco_id"),
    CONSTRAINT "fk_gioco_esempio_cruciverba__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."games" ("Id") ON DELETE CASCADE
);

CREATE TABLE riviste."gioco_esempio_cruciverba__definizioni" (
    "gioco_id" integer NOT NULL,
    "posizione" integer NOT NULL,
    "numero" integer NOT NULL,
    "direzione" text NOT NULL,
    "testo" varchar(300) NOT NULL,
    "soluzione" varchar(40),
    CONSTRAINT "pk_gioco_esempio_cruciverba__definizioni" PRIMARY KEY ("gioco_id", "posizione"),
    CONSTRAINT "fk_gioco_esempio_cruciverba__definizioni__gioco" FOREIGN KEY ("gioco_id") REFERENCES riviste."gioco_esempio_cruciverba" ("gioco_id") ON DELETE CASCADE,
    CONSTRAINT "ck_gioco_esempio_cruciverba__definizioni__posizione" CHECK ("posizione" >= 0),
    CONSTRAINT "ck_gioco_esempio_cruciverba__definizioni__direzione" CHECK ("direzione" IN ('orizzontale', 'verticale'))
);

CREATE INDEX "ix_gioco_esempio_cruciverba__autore" ON riviste."gioco_esempio_cruciverba" ("autore");
```

`dbe migrate` esegue ogni file in una transazione, dopo le migrazioni EF, con `SET LOCAL lock_timeout` e `SET LOCAL statement_timeout` impostati dal programma prima del file (sezione 8). Il valore di `griglia` in `jsonb` è l'oggetto della sezione 3.2, senza modifiche.

### 3.2 Descrittore → JSON Schema della risposta per pagina

L'involucro è `risposta-pagina.schema.json`. Lo schema inviato al Fornitore AI si deriva dal catalogo dei descrittori così:

1. Si copia l'involucro e si sostituisce `giochi.items` con `anyOf` di un `gioco_<id_con_underscore>` per Tipologia, in ordine di `id`, seguito da `giocoProposto`.
2. `gioco_<id>`: oggetto chiuso con tutte le chiavi obbligatorie; `stato_tipologia` `const "nota"`, `tipologia` `const <id>`, `versione_tipologia` `const <versione>`; `titolo`, `riquadro`, `avvisi` come nell'involucro.
3. `dati`: oggetto chiuso. **Ogni** campo e ogni elenco è una chiave obbligatoria; un campo non obbligatorio ammette `null` (`anyOf: [null, valore]`). Così lo schema regge anche le modalità "strict" dei fornitori che vogliono tutte le chiavi presenti.
4. Valore per tipo: `testo` → stringa 1..`max`; `testo_lungo` → stringa 1..20000; `intero` → intero a 32 bit (ruoli `righe`/`colonne` 1-60, `numero` 1-999); `decimale` → numero entro ±99999999,9999; `booleano` → booleano; `data` → stringa `AAAA-MM-GG`; `enumerazione` → `enum` dei valori; `griglia` → `$ref` a `griglia`; `immagine` → `{ "riquadro": riquadro }`.
5. Elenco → `array` (al massimo 500 voci) di oggetti chiusi costruiti con la regola 3, nello stesso ordine dei campi.
6. `confidenza` → oggetto chiuso con `gioco`, `tipologia`, `campi` (una chiave obbligatoria per campo) ed `elenchi` (una chiave obbligatoria per elenco), tutti numeri 0-1.
7. Si usano solo `anyOf` con discriminanti `const`, mai `oneOf`, e nessun `format`: i fornitori con output vincolato li supportano meglio.

Esempio completo: `esempi/risposta-pagina/derivato-esempio.schema.json`, derivato dai due descrittori di esempio.

Il programma valida sempre la risposta con lo schema derivato, anche se il fornitore lo impone. Se l'adattatore deve togliere parole chiave non supportate dal fornitore (da scoprire nella prova di C), toglie solo vincoli (`minimum`, `maxLength`, `pattern`), mai chiavi o tipi, e la validazione completa resta lato programma.

Controlli dopo lo schema (pipeline 1.3, prima dei validatori strutturali):
- Riquadro dentro la pagina: `x + larghezza ≤ 1` e `y + altezza ≤ 1` con tolleranza 0,001. Oltre la tolleranza il riquadro si taglia al bordo e si aggiunge un avviso. Si arrotonda a 5 decimali per `numeric(6,5)`.
- Chiavi di `confidenza.campi` ed `elenchi` uguali ai nomi del descrittore (già imposto dallo schema derivato).
- `griglia`: `righe` e `colonne` uguali ai campi con quei ruoli, `celle` con `righe` stringhe di `colonne` caratteri (validatore `griglia-coerente-con-dimensioni`).

### 3.3 Descrittore → modello di resa

Il Lettore riceve `resa.modello` e la mappa dei ruoli: per ogni ruolo, il nome del campo o dell'elenco che lo porta. La mappa si calcola dal descrittore, non si scrive a mano.

| Modello | Dati che usa |
|---|---|
| `griglia` | `righe`, `colonne`, `griglia`; l'elenco con `numero`, `direzione`, `definizione` (e `soluzione` se c'è), raggruppato per direzione e ordinato per numero |
| `esposto_diagramma` | `esposto`, `diagramma` se c'è, `soluzione` |
| `immagine_diagramma` | `immagine`, `diagramma` se c'è, `soluzione` |
| `griglia_logica` | `righe`, `colonne`, `griglia` |

I campi senza ruolo (per esempio `autore`) si mostrano come dettagli, con `etichetta`. La forma della mappa per il Lettore si fissa in fase 2.

## 4. Risultato atteso e metriche

### 4.1 Formato del risultato atteso

Un file per pagina, conforme a `risultato-atteso.schema.json`:
- `pagina`: `id` (uguale al nome del file), `impronta` (esadecimale minuscolo, la stessa della chiave delle Registrazioni), `origine` (`digitale` o `scansione`: è il gruppo delle metriche).
- `revisione`: `stato` `bozza` o `corretto`. `bozza_da` indica la Registrazione da cui `dbe eval draft` ha prodotto la bozza. Con `corretto` sono obbligatori `corretto_da` e `corretto_il`; con `bozza` valgono `null`.
- `contenuto_pagina`: stessa lista della risposta.
- `giochi`: per ogni Gioco atteso `id` (`g01`, `g02`...), `stato_tipologia`, riquadro, titolo. Con `nota`: `tipologia`, `versione_tipologia`, `dati` (stessa forma della risposta derivata, validati anche con lo schema derivato) e `campi_non_valutati` facoltativo. Con `proposta`: solo `famiglia`.
- `note`: testo libero o `null`.

Le pagine con `bozza` restano fuori dalle metriche e il rapporto le elenca. Una pagina senza Giochi ha `giochi: []` e misura la precisione.

### 4.2 Abbinamento dei Giochi

1. Per ogni pagina si calcolano tutte le coppie (atteso, estratto) con la stessa **chiave di tipologia** e IoU ≥ 0,5. Chiave: `nota:<id>` per le Tipologie note (la versione non conta; una versione diversa dà un avviso nel rapporto), `proposta` per tutte le proposte.
2. IoU = area dell'intersezione / area dell'unione dei due riquadri normalizzati. L'IoU non cambia con la scala degli assi, quindi i riquadri normalizzati bastano.
3. Abbinamento goloso deterministico: coppie ordinate per IoU decrescente, poi indice dell'atteso crescente, poi indice dell'estratto crescente. Si prende una coppia se nessuno dei due è già abbinato.
4. Abbinati = veri positivi. Attesi senza coppia = mancanti. Estratti senza coppia = in più.

### 4.3 Metriche

Tutte le metriche sono micro-medie: si sommano numeratori e denominatori su tutte le pagine del gruppo. Con denominatore 0 la metrica vale `null` ("non applicabile"), mai 100%.

| Metrica | Numeratore / denominatore |
|---|---|
| Richiamo dei Giochi | abbinati / attesi |
| Precisione dei Giochi | abbinati / estratti |
| Tipologia corretta | Su un secondo abbinamento con lo stesso algoritmo ma **senza** la chiave di tipologia (solo IoU ≥ 0,5): coppie con la stessa chiave di tipologia / coppie |
| Caselle del cruciverba corrette | Per le coppie della sezione 4.2 la cui Tipologia ha il ruolo `griglia`: caselle corrette / caselle attese |
| CER di definizioni ed esposti | Somma delle distanze di modifica / somma delle lunghezze attese, sui campi con ruolo `definizione` o `esposto` delle coppie della sezione 4.2 |
| Campi esatti dei giochi brevi | Per le coppie di famiglia `gioco-breve`: campi uguali / campi confrontati |
| Costo e tempo medi per pagina | Media sulle pagine con il valore noto; il rapporto indica anche quante pagine non l'hanno |

**Caselle.** Se `righe` o `colonne` differiscono, tutte le caselle attese contano come errate. Altrimenti una casella è corretta se coincidono la categoria (nera `#` o bianca, cioè `.` o lettera) e il numero stampato in quella casella (o l'assenza di numero). `?` nell'estratto è sempre errata; `?` nell'atteso esclude la casella dal conteggio. Le lettere non si confrontano in fase 1.

**CER.**
- Normalizzazione N, in ordine: Unicode NFC; `’ ‘ ‛ ʼ ´` e accento grave → `'`; `“ ” „ « »` → `"`; `– — −` → `-`; `…` → `...`; ogni spazio Unicode (anche U+00A0 e U+202F) → spazio; spazi consecutivi → uno; spazi prima e dopo `'` tolti; spazi iniziali e finali tolti; minuscole con le regole invarianti.
- Distanza di Levenshtein con peso 1, contata sui caratteri Unicode (rune), dopo N. Lunghezza = numero di rune dopo N.
- Voci degli elenchi allineate per (`numero`, `direzione`) quando l'elenco ha quei ruoli, altrimenti per posizione. Chiavi ripetute: nell'ordine in cui compaiono.
- Voce attesa senza voce estratta: errori += lunghezza attesa. Voce estratta in più: errori += sua lunghezza (il denominatore non cambia). Atteso `null` ed estratto `null`: non conta. Atteso `null` ed estratto testo: errori += lunghezza estratta.

**Campi esatti.** Si confrontano i campi del Gioco (non gli elenchi). Testi uguali dopo N; altri tipi per uguaglianza del valore JSON; `null` uguale a `null`. I `campi_non_valutati` dell'atteso restano fuori. Valgono anche per caselle e CER.

**Gruppi.** `digitale`, `scansione` e `totale`, dall'`origine` della pagina. Le soglie della ROADMAP si confrontano per gruppo.

**Arrotondamento.** Calcolo in doppia precisione; nel rapporto e nella Linea di base 4 decimali, arrotondamento lontano da zero. Il rapporto di due esecuzioni sugli stessi dati è identico byte per byte (chiavi in ordine fisso, nessuna data tranne quella dell'intestazione, che il test esclude).

## 5. Prompt di estrazione v1

File: `tipologie/_meta/prompt-estrazione-v1.md`. Il programma usa il testo tra `<!-- inizio-modello -->` e `<!-- fine-modello -->`.

### 5.1 Segnaposti

| Segnaposto | Valore |
|---|---|
| `{{catalogo_tipologie}}` | Catalogo dei descrittori, formato della sezione 5.2 |
| `{{schema_risposta}}` | Schema derivato (sezione 3.2), JSON con rientro di 2 spazi, chiavi nell'ordine di costruzione |
| `{{testo_nativo}}` | Testo nativo della pagina, oppure stringa vuota |

Il testo nativo si prepara così: a capo normalizzati in LF; le sequenze `<<<TESTO` e `TESTO>>>` sostituite con `<<TESTO` e `TESTO>>`, così la pagina non chiude il blocco; troncato a 50.000 caratteri con un avviso nella riga di `chiamate_ai` (non nel prompt). Un segnaposto sconosciuto, o assente, fa fallire il caricamento del modello.

Il prompt va su stdin del processo di Claude Code (PLAN, sezione 2). L'immagine passa con il meccanismo che C fissa nella prova. Se il fornitore accetta uno schema separato, lo schema si passa anche lì; il testo del prompt non cambia.

### 5.2 Formato del catalogo

Per ogni descrittore, in ordine di `id`:

```
### <id> (versione <versione>): <nome>
Famiglia: <famiglia>. Resa: <resa.modello>.
Come si riconosce: <descrizione>
Regole: <regole.testo>
Campi:
- `<nome>` (<etichetta o nome>): <tipo leggibile>[, obbligatorio][. <descrizione>]
Elenco `<nome>` (<etichetta o nome>), una voce per riga stampata:
- `<nome>` ...
```

Tipo leggibile: `testo` → "testo, al massimo N caratteri"; `testo_lungo` → "testo lungo"; `intero` → "numero intero"; `decimale` → "numero decimale"; `booleano` → "vero o falso"; `data` → "data AAAA-MM-GG"; `enumerazione` → "uno tra: v1, v2"; `griglia` → "griglia, nel formato descritto sopra"; `immagine` → "riquadro dell'immagine". Una riga vuota tra due Tipologie. Le regole entrano anche se in bozza: per l'estrazione bastano; l'approvazione conta per il Motore di creazione (ADR 002, punto 3).

### 5.3 Versione del prompt

`versione_prompt` = `estrazione-v1-` + primi 12 caratteri esadecimali dello SHA-256 (UTF-8, LF) del modello con catalogo e schema sostituiti e `{{testo_nativo}}` lasciato com'è. Esempio: `estrazione-v1-3fa29c0b71de`. Il numero dopo `v` è quello del file; l'impronta cattura descrittori, regole e schema. Entra nella chiave delle Registrazioni e in `chiamate_ai`.

## 6. Registrazioni, Linea di base, Set sintetico

### 6.1 Struttura di un set

Uguale per il Set sintetico (nel repository) e per il Set di riferimento (solo sul PC, D2):

```
<set>/
  set.json                     elenco delle pagine
  pagine/<id>.png              immagine della pagina (o PDF di origine, vedi 6.4)
  attesi/<id>.json             risultato atteso, sezione 4.1
  registrazioni/<fornitore>/<modello>/<versione_prompt>/<impronta>.json
  linea-di-base.json
```

`set.json`: `{ "versione_formato": 1, "nome": "<nome>", "tipo": "sintetico" | "riferimento", "pagine": [ { "id": "s01", "origine": "digitale", "pdf": "pdf/sintetico.pdf" | null, "numero_pagina": 1 | null } ] }`. Il lettore della 1.2 (C) rifiuta un `id` fuori da `^[a-z0-9][a-z0-9-]{0,63}$` e qualsiasi percorso con `..`, assoluto o fuori dalla cartella del set.

### 6.2 Registrazione

Chiave (PLAN, sezione 2): impronta della pagina, versione del prompt, id del fornitore, modello. Percorso: `registrazioni/<fornitore>/<modello>/<versione_prompt>/<impronta>.json`, con `fornitore` `^[a-z][a-z0-9-]{0,39}$`, `modello` `^[a-z0-9]([a-z0-9._-]{0,98}[a-z0-9])?$`, `impronta` `^[0-9a-f]{64}$`. Ogni componente si valida prima di comporre il percorso.

Impronta della pagina (scelta di B, accettata dall'agente principale il 20/11/2026): SHA-256 di una serializzazione canonica del contenuto della pagina letto con PdfPig. Entrano dimensioni della pagina, lettere (testo, font, posizione, corpo), byte e riquadro delle immagini, riquadri dei tracciati, con coordinate arrotondate ai centesimi. Non dipende dal PNG né dal DPI, quindi è identica su Windows e in CI. Limite noto: la stessa pagina in due PDF diversi ha la stessa impronta, e così due pagine bianche uguali. La 1.3 ne tiene conto quando salta le pagine già elaborate.

Contenuto (UTF-8, LF, chiavi in quest'ordine):

```json
{
  "versione_formato": 1,
  "chiave": { "impronta_pagina": "<hex64>", "versione_prompt": "estrazione-v1-3fa29c0b71de", "fornitore": "claude-code", "modello": "claude-sonnet-5-5" },
  "registrata_il": "2026-12-20T21:04:05.123456Z",
  "dpi": 200,
  "esito": "riuscita",
  "risposta_grezza": "<testo esatto ricevuto>",
  "risposta_sha256": "<hex64>",
  "metadati": { "durata_ms": 41230, "token_ingresso": 5120, "token_uscita": 2210, "token_cache_creazione": null, "token_cache_lettura": null, "costo": "0.031200", "costo_valuta": "USD", "costo_tipo": "indicativo" }
}
```

- Si registrano solo gli esiti `riuscita` e `risposta_non_valida` (hanno un testo). Gli errori non si registrano.
- `Riproduci` legge il file, controlla lo SHA-256, restituisce `risposta_grezza` e i metadati come se fossero arrivati dal fornitore. Manca il file: errore esplicito, zero chiamate. SHA-256 diverso: errore esplicito.
- `Riproduci` rifiuta una Registrazione con `dpi` diverso da quello richiesto, con un messaggio che lo dice: il DPI resta nella Registrazione perché l'impronta non lo contiene.
- Le Registrazioni di Riviste vere contengono testo delle Riviste: stanno solo sul PC (D2).

### 6.3 Linea di base

`linea-di-base.json`:

```json
{
  "versione_formato": 1,
  "set": "sintetico",
  "registrata_il": "2026-12-20T21:04:05Z",
  "condizioni": { "fornitore": "finto", "modello": "finto-1", "versione_prompt": "estrazione-v1-3fa29c0b71de", "dpi": 200 },
  "metriche": {
    "digitale":  { "pagine": 4, "giochi_attesi": 6, "giochi_estratti": 6, "richiamo": 1.0, "precisione": 1.0, "tipologia_corretta": 1.0, "caselle_corrette": 1.0, "cer": 0.0, "campi_esatti": 1.0 },
    "scansione": { "...": "stessa forma" },
    "totale":    { "...": "stessa forma" }
  }
}
```

Regressione (`eval extraction --replay`): fallisce se, in un gruppo, una metrica con valore nella Linea di base scende (`richiamo`, `precisione`, `tipologia_corretta`, `caselle_corrette`, `campi_esatti`) o sale (`cer`), confrontando i valori a 4 decimali, senza tolleranza: la riproduzione è deterministica. Una metrica con valore nella Linea di base e `null` nella misura fallisce. Condizioni diverse da quelle della Linea di base: errore che chiede di registrarne una nuova. La Linea di base si aggiorna solo con un comando esplicito e una PR (Set sintetico) o sul PC (Set di riferimento).

### 6.4 Contenuto minimo del Set sintetico

Pagine generate dal generatore di PDF sintetici dell'agente B (PdfPig), con testi inventati dal progetto. Risultati attesi e Registrazioni scritti a mano dalla 1.2 (C), coerenti con le Tipologie vere della 1.2.

| Pagina | Origine | Contenuto | Cosa prova |
|---|---|---|---|
| s01 | digitale | Cruciverba 7x7 con definizioni, font incorporato | Caselle, numeri, CER delle definizioni |
| s02 | digitale | Anagramma, sciarada e cambio di lettera, font standard non incorporato (Helvetica) | Tre Giochi per pagina, campi esatti, font standard |
| s03 | digitale | Due giochi brevi con un font TrueType non incorporato e non standard | Resa del font sostitutivo, testo nativo presente |
| s04 | scansione | Cruciverba 5x5 solo immagine, nessun testo nativo | Gruppo scansioni, pagina senza testo nativo |
| s05 | digitale | Pagina di testo redazionale, nessun Gioco | Precisione: zero Giochi attesi |
| s06 | digitale | Un anagramma e un gioco di Tipologia non prevista | Proposta (criterio 7) |

La Registrazione di almeno una pagina contiene un errore voluto (una casella e una definizione sbagliate) e una contiene un Gioco in più, così la Linea di base sintetica non è tutta al 100% e la regressione si vede.

## 7. Contratto delle chiamate AI (D7, ADR 011)

Ogni tentativo verso il Fornitore AI è una chiamata: una riga in `riviste.chiamate_ai` e un evento nel registro. Il retry di un errore transitorio e la richiesta di correzione sono chiamate separate. La riproduzione non fa chiamate e non scrive righe.

### 7.1 Riga di `riviste.chiamate_ai`

Persistenza nella 1.3 (migrazione EF, A). Nella 1.1 C implementa lo stesso contratto in memoria.

| Colonna | Tipo SQL | Null | Regola |
|---|---|---|---|
| `id` | `uuid` | no | Chiave primaria, generata dal programma (UUID versione 7) |
| `avviata_il` | `timestamptz` | no | UTC, inizio della chiamata |
| `durata_ms` | `integer` | no | `>= 0`, dal lancio alla risposta o all'errore |
| `uso` | `varchar(20)` | no | `CHECK (uso IN ('estrazione'))`; la 1b aggiunge `creazione` nel proprio schema |
| `fornitore` | `varchar(40)` | no | `^[a-z][a-z0-9-]{0,39}$`, per esempio `claude-code`, `finto` |
| `modello` | `varchar(100)` | no | Modello configurato, per esempio `claude-sonnet-5-5` |
| `versione_prompt` | `varchar(40)` | no | Sezione 5.3 |
| `impronta_pagina` | `bytea` | no | `CHECK (octet_length(impronta_pagina) = 32)`; collega la chiamata a `riviste.pages.impronta` senza chiave esterna, perché la chiamata si salva anche quando la pagina fallisce |
| `tentativo` | `integer` | no | `>= 1`, numero del tentativo per la stessa pagina nella stessa esecuzione |
| `correzione` | `boolean` | no | `true` per la richiesta di correzione dopo una risposta non valida |
| `esito` | `varchar(30)` | no | `CHECK` tra `riuscita`, `risposta_non_valida`, `finestra_esaurita`, `errore_transitorio`, `errore_definitivo` (gli esiti di `IFornitoreAiEstrazione`) |
| `ripresa_il` | `timestamptz` | sì | Solo con `finestra_esaurita`, se il fornitore indica l'ora |
| `codice_uscita` | `integer` | sì | Codice d'uscita del processo, per gli adattatori a riga di comando |
| `messaggio_errore` | `varchar(1000)` | sì | Messaggio tecnico, senza testo della pagina; troncato a 1000 caratteri |
| `token_ingresso` | `integer` | sì | `>= 0`, se riportato |
| `token_uscita` | `integer` | sì | `>= 0`, se riportato |
| `token_cache_creazione` | `integer` | sì | `>= 0`, se riportato |
| `token_cache_lettura` | `integer` | sì | `>= 0`, se riportato |
| `costo` | `numeric(12,6)` | sì | `>= 0`, se riportato |
| `costo_valuta` | `varchar(3)` | sì | `^[A-Z]{3}$`; `CHECK ((costo IS NULL) = (costo_valuta IS NULL))` |
| `costo_tipo` | `varchar(12)` | sì | `addebitato` (API a consumo) o `indicativo` (abbonamento: il valore riportato non si paga); `CHECK ((costo IS NULL) = (costo_tipo IS NULL))`. Il tetto di spesa somma solo `addebitato` |
| `testo_nativo_troncato` | `boolean` | no | `true` se il testo nativo superava 50.000 caratteri |
| `risposta_grezza` | `text` | sì | Testo esatto ricevuto, senza tagli; `null` se non è arrivato testo |
| `risposta_sha256` | `bytea` | sì | SHA-256 dei byte UTF-8 (senza BOM) di `risposta_grezza`; `CHECK (risposta_grezza IS NULL OR octet_length(risposta_sha256) = 32)` |

Indici: `ix_chiamate_ai__impronta_pagina` (btree) e `ix_chiamate_ai__avviata_il` (btree), per `dbe costs` e per l'analisi della 1.4. Permessi: `dbe_estrazione` con `SELECT`, `INSERT`, `UPDATE`, `DELETE` (D7: la tabella si può svuotare). Svuotare il testo (`UPDATE ... SET risposta_grezza = NULL`) lascia lo SHA-256 e i metadati.

In memoria (C, 1.1): un record con gli stessi campi e gli stessi nomi in PascalCase italiano (`Id`, `AvviataIl`, `DurataMs`, ...). `RispostaSha256` e `ImprontaPagina` come `byte[]` di 32 byte. `costo` come `decimal?`.

### 7.2 Evento `chiamata_ai` nel registro

| Campo di `NuovoEvento` | Valore |
|---|---|
| `avvenutoIl` | `avviata_il + durata_ms`, in UTC |
| `tipo` | `chiamata_ai` |
| `entita` | `chiamata_ai` |
| `entitaId` | `id` della riga, formato `D` minuscolo (36 caratteri) |
| `autore` | `motore-estrazione` |
| `dati` | JSON sotto |

`dati`: oggetto JSON compatto (senza spazi), chiavi in quest'ordine, nessuna chiave in più:

```json
{"versione":1,"uso":"estrazione","fornitore":"claude-code","modello":"claude-sonnet-5-5","versione_prompt":"estrazione-v1-3fa29c0b71de","impronta_pagina":"<hex64>","tentativo":1,"correzione":false,"esito":"riuscita","durata_ms":41230,"token_ingresso":5120,"token_uscita":2210,"token_cache_creazione":null,"token_cache_lettura":null,"costo":"0.031200","costo_valuta":"USD","costo_tipo":"indicativo","risposta_sha256":"<hex64>"}
```

- Interi come numeri JSON; `costo` come stringa decimale con il punto e 6 decimali, per non perdere precisione; valori sconosciuti come `null`.
- Esadecimale minuscolo per le impronte; `risposta_sha256` `null` se non c'è testo.
- **Mai** nel registro: `risposta_grezza`, `messaggio_errore`, testo nativo, prompt, nome del file della Rivista.
- L'ordine fisso serve all'impronta concatenata del registro: lo stesso evento produce sempre gli stessi byte.

Test della 1.3 (già nel PLAN): i `dati` contengono lo SHA-256 e non il testo; `DELETE` su `riviste.chiamate_ai` con `dbe_estrazione` riesce e `verify-integrity` resta a 0.

## 8. Squawk sulle migrazioni delle Tipologie

Squawk (ultima versione al 20/11/2026: 2.73.0) gira solo sui file `tipologie/**/v*.sql` generati, non sulle migrazioni EF. Configurazione proposta (`.squawk.toml` nella radice, 1.2 A):

```toml
pg_version = "18"
assume_in_transaction = true
excluded_rules = [
  "prefer-text-field",
  "prefer-bigint-over-int",
  "require-lock-timeout",
  "require-statement-timeout",
]
```

| Regola esclusa | Motivo |
|---|---|
| `prefer-text-field` | L'ADR 002, punto 4, fissa `varchar(max)` per `testo`. La regola protegge dal blocco quando si cambia la lunghezza di una colonna esistente; qui le tabelle sono nuove e un cambio di lunghezza è una versione nuova del descrittore |
| `prefer-bigint-over-int` | L'ADR 002 fissa `integer` per `intero`, e `gioco_id` in `riviste` deve avere lo stesso tipo di `riviste.games."Id"` (`integer`). I valori sono piccoli: dimensioni, numeri di definizione, posizioni |
| `require-lock-timeout`, `require-statement-timeout` | Il generatore non scrive `SET` (sezione 3.1). `dbe migrate` imposta `SET LOCAL lock_timeout = '5s'` e `SET LOCAL statement_timeout = '60s'` nella transazione, prima di ogni file. È il caso "connessione già configurata" previsto dalla documentazione di squawk. Un test d'integrazione della 1.2 lo verifica |

Regole che **non** si escludono, perché l'SQL generato le rispetta già:
- `require-concurrent-index-creation`: squawk la salta per gli indici su tabelle create nella stessa transazione, che con `assume_in_transaction` è sempre il caso.
- `prefer-robust-stmts`: soddisfatta da `assume_in_transaction`; il file non contiene `BEGIN`/`COMMIT` (anche `transaction-nesting` resta attiva).
- `adding-foreign-key-constraint`, `constraint-missing-not-valid`, `disallowed-unique-constraint`: i vincoli stanno dentro `CREATE TABLE`, su tabelle senza righe.
- `require-table-schema`, `identifier-too-long`, `ban-char-field`, `prefer-timestamptz`, `prefer-identity`, `adding-serial-primary-key-field`: rispettate per costruzione.

Le regole opt-in (`ban-new-write-restriction` e simili) restano spente. Versione di squawk fissata con checksum nella CI (PLAN, sezione 9).

Prova del 20/11/2026 con squawk 2.73.0 sull'SQL dell'esempio della sezione 3.1, schemi `riviste` e `catalogo`:
- Senza `assume_in_transaction` e senza esclusioni scattano anche `prefer-robust-stmts` e `require-concurrent-index-creation`.
- Con `assume_in_transaction` e senza esclusioni scattano solo le quattro regole escluse sopra.
- Con la configurazione proposta: 0 problemi, codice d'uscita 0.

Se una regola nuova scatta sulle Tipologie vere della 1.2, si corregge il generatore; un'esclusione nuova richiede un motivo scritto qui.

## 9. Esempi e validazione meccanica

Esempi in `tipologie/_meta/esempi/`, tutti inventati:

| Cartella | Validi | Non validi (motivo) |
|---|---|---|
| `descrittore/` | `valido-esempio-cruciverba.v1.json` (regole in bozza), `valido-esempio-anagramma.v1.json` (regole approvate) | identificatore con payload d'iniezione; tipo `varchar(100)`; nome `select`; resa `griglia` senza il ruolo `colonne`; regole approvate senza data; proprietà `default` |
| `risultato-atteso/` | `valido-s01.json` (corretto, digitale), `valido-s06.json` (bozza, scansione, con proposta e `campi_non_valutati`) | corretto senza data; origine `fotocopia`; riquadro con `x` negativo |
| `risposta-pagina/` | `valido-s01-cruciverba.json`, `valido-s06-proposta.json` (con metadati della Rivista e una proposta) | confidenza 1,5; riquadro senza `altezza`; `non-valido-solo-derivato-dati.json`: valido per l'involucro, non valido per lo schema derivato (campo estraneo e definizione senza numero) |
| `risposta-pagina/derivato-esempio.schema.json` | Schema derivato dai due descrittori di esempio con le regole della sezione 3.2 | - |

Validazione eseguita il 20/11/2026, fuori dal repository (cartella temporanea dell'agente), con due validatori indipendenti:
1. Programma .NET 10 con `Corvus.Text.Json.Validator` 5.8.12 (`JsonSchema.FromText`, `Validate`). Esito: tutti i validi passano, tutti i non validi falliscono, per i quattro schemi (meta-schema, risultato atteso, involucro, schema derivato).
2. Python `jsonschema` 4.25.1 con `Draft202012Validator`: `check_schema` conferma che i quattro schemi sono conformi al meta-schema 2020-12; stessi esiti del punto 1, e il primo errore di ogni esempio non valido è quello voluto.

Lo schema derivato e l'SQL della sezione 3.1 sono prodotti da uno script Python di riferimento (circa 100 righe), anch'esso fuori dal repository. Serve solo a controllare le regole: il generatore vero è C# nella 1.2 (A), con test snapshot.

## 10. Scostamenti e punti da decidere

1. **Nome dell'evento.** Il PLAN e l'ADR 011 scrivono `chiamata-ai`. `NuovoEvento` accetta solo `^[a-z][a-z0-9_]{0,59}$` e gli eventi esistenti usano il trattino basso (`gioco_creato`). Il contratto usa `chiamata_ai`. Solo un nome: D7 non cambia. PLAN allineato il 20/11/2026.
2. **Impronta della pagina.** Chiuso il 20/11/2026: vale la scelta di B (contenuto della pagina letto con PdfPig, sezione 6.2), stabile tra Windows e CI e indipendente dal DPI. Limite noto: pagine uguali in PDF diversi, o pagine bianche uguali, hanno la stessa impronta; la 1.3 ne tiene conto nel salto delle pagine. Il DPI resta nella Registrazione.
3. **Metadati in più nell'evento.** Oltre all'elenco di D7 (fornitore, modello, durata, esito, token, costo, SHA-256) l'evento porta `versione_prompt`, `impronta_pagina`, `tentativo`, `correzione` e `uso`. Sono metadati senza testo delle Riviste, quindi nel perimetro di D7. Se il proprietario volesse l'elenco stretto, si tolgono senza altre modifiche.
4. **Dati dei Giochi proposti.** Per una proposta la risposta porta `trascrizione` (testo visibile). Il nucleo della 1.2 non ha una colonna per conservarla. Proposta per la 1.2 (B): `riviste.games.trascrizione_proposta text NULL`, valorizzata solo con `stato_tipologia = proposta`. Senza, il Gioco non si perde (criterio 7) ma il testo sì. Accettato: aggiunto al PLAN, ondata 1.2 B, il 20/11/2026.
5. **Chiave `immagine` senza chiave esterna** in fase 1 (sezione 3.1): la tabella dei ritagli non esiste. Nessuna delle 4 Tipologie della fase 1 usa `immagine`.
6. **`regole` come oggetto.** Lo schema minimo della ROADMAP 2.2 mostra `regole` come stringa; l'ADR 002, punto 3, chiede data e versione dell'approvazione. Il meta-schema usa un oggetto. Nessun conflitto con D1-D7.
7. Nessuna scelta di questo documento contraddice D1-D7. Nessuna decisione nuova del proprietario è necessaria per chiudere l'ondata; i punti 1, 2 e 4 sono chiusi.
