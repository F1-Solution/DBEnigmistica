# Fase 1b, ondata 1b.1 - Documento di progetto (agente A)

Scritto il 20/11/2026 dall'agente A (Opus). Copre i compiti 1-9 dell'agente A in `PLAN.md`, sezione 3. Estende `.planning/phases/1/progetto-1.1.md` (meta-schema, sezione 3.2, sezione 6.2, sezione 7, sezione 9) e non lo cambia. Vincoli: `CLAUDE.md` sezione 5, ADR 002, 005, 010, 011, decisioni D1-D7 e D1b-1 - D1b-9, scelte tecniche di `PLAN.md` sezione 2, con le conferme [A] dell'agente principale del 20/11/2026.

La **Parte 1** contiene i contratti che servono subito a B e a C. La **Parte 2** contiene prompt, descrittori v0, misure ed esempi.

File consegnati:

| File | Contenuto |
|---|---|
| `tipologie/_meta/descrittore.schema.json` | Solo il minimo di `versione`, da 1 a 0 (sezione 7.1) |
| `tipologie/_meta/brief.schema.json` | Formato del brief (sezione 2) |
| `tipologie/_meta/prompt-creazione-v1.md` | Prompt di creazione v1, quattro modelli per famiglia (sezione 6) |
| `tipologie/_meta/esempi/creazione/**` | Brief, schemi derivati, risposte e schema del cruciverba di esempio, validi e non validi (sezione 10) |
| `tipologie/{crittografia-breve,rebus,sudoku,futoshiki,nonogramma}/v0.json` | Descrittori v0 con regole in bozza (sezione 7) |
| `.planning/phases/1b/brief-tipo.md` | Bozze dei brief tipo per il proprietario (sezione 9) |

Id delle Tipologie nuove, proposti e usati in tutti i file: `crittografia-breve`, `rebus`, `sudoku`, `futoshiki`, `nonogramma`. Per anagramma e cruciverba valgono gli id della 1.2 (`anagramma`, `cruciverba-classico`); nella 1b.1 si prova tutto sui descrittori d'esempio della 1.1 (`esempio-anagramma`, `esempio-cruciverba`).

---

# Parte 1. Contratti per B e C

Stato: **provvisori dal 20/11/2026**, finali con l'approvazione dell'agente principale. Un cambio dopo quella data si scrive nella sezione 11 con la data.

## 1. Porta di creazione (compito 1; per B)

### 1.1 Interfaccia

In `src/DBEnigmistica.Application/FornitoreAi/`, accanto alla porta di estrazione, che non cambia.

```csharp
public interface IFornitoreAiCreazione
{
    string IdFornitore { get; }
    string Modello { get; }
    Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default);
}
```

Una chiamata a `CreaAsync` = un candidato. L'adattatore fa al massimo un retry per errore transitorio e al massimo una richiesta di correzione, come l'estrazione. Ogni tentativo è una `ChiamataAi`.

### 1.2 Richiesta

```csharp
public sealed record RichiestaCreazione(
    string Tipologia,          // id del descrittore
    int VersioneTipologia,     // 0 per i descrittori v0
    string BriefJson,          // brief valido per brief.schema.json, con "variante" (sezione 2.3)
    string Prompt,             // prompt compilato (sezione 6), a capo LF
    string VersionePrompt,     // sezione 6.4, per esempio "creazione-v1-5b0e3c9a1f2d"
    string JsonSchema);        // schema della risposta, minimizzato (sezione 3)
```

- `Prompt`, `VersionePrompt` e `JsonSchema` li produce solo il compilatore del prompt (B, compito 3). Nessun altro codice li compone.
- Nessun campo per immagini, testo nativo o file. L'adattatore non abilita strumenti (`--tools` vuoto) e non crea file nella cartella di lavoro.
- Il compilatore rifiuta il carattere U+0000 in tutti gli ingressi.

### 1.3 Impronta della richiesta

`impronta_richiesta` = SHA-256 dei byte `UTF-8(Prompt) ‖ 0x00 ‖ UTF-8(JsonSchema)`, in esadecimale minuscolo (64 cifre). È la conferma [A] "SHA-256 del prompt compilato più lo schema". Il byte 0x00 separa i due testi senza ambiguità, perché nessuno dei due può contenerlo.

- Si calcola dalla richiesta (proprietà derivata), mai passata a mano.
- Prende il posto dell'impronta della pagina in `ChiamataAi.ImprontaPagina` (32 byte), nella chiave delle Registrazioni e nell'evento `chiamata_ai`.
- Due richieste identiche hanno la stessa impronta. Per questo il brief porta `variante` (sezione 2.3): ogni nuovo candidato dello stesso brief cambia il prompt, quindi l'impronta.

### 1.4 Risposta ed esiti

```csharp
public sealed record RispostaCreazione(
    EsitoEstrazione Esito,
    string? Json,                    // risposta valida per JsonSchema, solo con Riuscita
    string? Messaggio,               // errori dello schema o messaggio tecnico; mai testo del prompt
    DateTimeOffset? RipresaFinestra, // solo con FinestraEsaurita, se nota
    IReadOnlyList<ChiamataAi> Chiamate,
    MetadatiChiamata? Metadati = null);
```

- Esiti: gli stessi cinque dell'estrazione. Si riusa l'enum `EsitoEstrazione`, perché `ChiamataAi.Esito` ha quel tipo. Così il contratto dell'estrazione e i suoi test non cambiano.

| Esito | Quando | Effetto sul candidato (sezione 4.2) |
|---|---|---|
| `Riuscita` | Risposta valida per lo schema, al primo tentativo o dopo la correzione | `da_rivedere` |
| `RispostaNonValida` | Anche la correzione non passa lo schema, oppure non contiene JSON | `non_valido_schema` |
| `FinestraEsaurita` | Finestra dell'abbonamento esaurita | `errore_generazione` |
| `ErroreTransitorio` | Errore transitorio anche dopo il retry | `errore_generazione` |
| `ErroreDefinitivo` | Errore che un nuovo tentativo non risolve | `errore_generazione` |

- La risposta di correzione usa lo stesso prompt con in coda gli errori dello schema, come l'estrazione. Il testo della correzione non entra nell'impronta: l'impronta è quella della richiesta originale.
- `Json` è il solo oggetto JSON validato, senza l'involucro di Claude Code.

### 1.5 Riga `ChiamataAi` per la creazione

Stesso record della 1.1, con:

| Campo | Valore per la creazione |
|---|---|
| `Uso` | `"creazione"` (costante nuova `UsoCreazione`, unico cambio a `ChiamataAi.cs`) |
| `ImprontaPagina` | I 32 byte di `impronta_richiesta` |
| `TestoNativoTroncato` | Sempre `false` |
| `Modello` | Il modello di creazione (chiave separata, D1b-6), per esempio `claude-opus-5-5` |
| Tutti gli altri | Come nella 1.1, sezione 7.1 |

Le righe si salvano in `prototipo.chiamate_ai` (sezione 4.4), mai in `riviste.chiamate_ai`.

### 1.6 Registrazioni di creazione

Stessa logica della 1.1, sezione 6.2, con queste differenze.

- Chiave: `impronta_richiesta`, versione del prompt, fornitore, modello. Stesse espressioni regolari della 1.1 (la classe `ChiaveRegistrazione` va bene: l'impronta ha la stessa forma).
- Percorso: `<cartella>/<fornitore>/<modello>/<versione_prompt>/<impronta_richiesta>.json`. Nel repository: `tests/DBEnigmistica.Tests/Fixtures/RegistrazioniCreazione/`, solo da brief sintetici scritti dal progetto.
- Nessun `dpi`. In più `uso`, `tipologia` e `versione_tipologia`.

```json
{
  "versione_formato": 1,
  "uso": "creazione",
  "chiave": { "impronta_richiesta": "<hex64>", "versione_prompt": "creazione-v1-5b0e3c9a1f2d", "fornitore": "claude-code", "modello": "claude-opus-5-5" },
  "registrata_il": "2026-12-22T21:04:05.123456Z",
  "tipologia": "crittografia-breve",
  "versione_tipologia": 0,
  "esito": "riuscita",
  "risposta_grezza": "<testo esatto ricevuto>",
  "risposta_sha256": "<hex64>",
  "metadati": { "durata_ms": 41230, "token_ingresso": 2120, "token_uscita": 910, "token_cache_creazione": null, "token_cache_lettura": null, "costo": "0.031200", "costo_valuta": "USD", "costo_tipo": "indicativo" }
}
```

- Si registrano solo `riuscita` e `risposta_non_valida`.
- `Riproduci`: Registrazione mancante → errore esplicito e zero chiamate; SHA-256 diverso → errore esplicito; `tipologia` o `versione_tipologia` diverse dalla richiesta → errore esplicito. Rifà interpretazione e validazione come dal vivo.

### 1.7 Decoratori

Paralleli a quelli dell'estrazione, con lo stesso comportamento: limite per esecuzione (stesso `ContatoreChiamateAi`, seconda richiesta oltre il limite → `LimiteEsecuzioneRaggiuntoException`, codice 4 nella CLI), Registra, Riproduci, registro delle chiamate. Il registro delle chiamate di creazione scrive in `prototipo.chiamate_ai` e l'evento della sezione 4.5.

## 2. Brief (compito 2; per B)

File: `tipologie/_meta/brief.schema.json` (JSON Schema 2020-12). Il brief è un oggetto chiuso. Il programma lo valida prima di compilare il prompt e prima di chiamare un generatore.

### 2.1 Campi

| Campo | Regola |
|---|---|
| `versione_formato` | `1` |
| `codice` | Facoltativo. `^[a-z0-9]+(-[a-z0-9]+)*$`, al massimo 40 caratteri. Il codice del brief tipo, per esempio `crit-sin-01`. Serve al rapporto |
| `tipologia` | Id della Tipologia, stessa espressione del meta-schema |
| `versione_tipologia` | Intero 0-999. Deve essere la versione del descrittore usato (controllo del programma) |
| `famiglia` | Lista chiusa del meta-schema. Deve essere la famiglia del descrittore (controllo del programma) |
| `tema` | `null` o testo 1-200 caratteri |
| `vincoli` | Elenco di 0-8 testi da 1-300 caratteri, senza ripetizioni |
| `difficolta` | Intero 1-5: 1 molto facile, 2 facile, 3 media, 4 difficile, 5 molto difficile |
| `dimensioni` | `null` o `{ "righe": n, "colonne": m }` |
| `sottotipo` | `null`, `sinonimica` o `perifrastica` |
| `variante` | Facoltativo nel file del brief tipo; intero 1-999. Il programma lo scrive sempre nel brief salvato con il candidato (sezione 2.3) |

Tutti i testi sono su una riga: niente caratteri di controllo (U+0000-U+001F, U+007F).

### 2.2 Regole per famiglia e Tipologia

Espresse nello schema con `if`/`then`:

| Condizione | Regola |
|---|---|
| `famiglia` = `crittografia` | `sottotipo` obbligatorio non nullo; `dimensioni` `null` |
| `famiglia` ≠ `crittografia` | `sottotipo` `null` |
| `famiglia` = `gioco-breve` | `tema` non nullo (anagramma a tema); `dimensioni` `null` |
| `famiglia` = `rebus` | `dimensioni` `null` |
| `famiglia` = `cruciverba` | `dimensioni` quadrate da 9x9 a 13x13 (scelta allineata al generatore di C) |
| `famiglia` = `logico` | `tema` `null`; `vincoli` vuoto |
| `tipologia` = `sudoku` | `dimensioni` 9x9 |
| `tipologia` = `futoshiki` | `dimensioni` quadrate 5x5, 6x6 o 7x7 |
| `tipologia` = `nonogramma` | `righe` e `colonne` 10-15 ciascuna, anche diverse |

Controlli del programma, fuori dallo schema: `famiglia` e `versione_tipologia` uguali a quelle del descrittore; per le famiglie con AI esiste un modello di prompt (sezione 6); per `logico` la Tipologia ha un generatore.

### 2.3 Variante

`variante` = 1 + numero dei candidati già salvati con la stessa Tipologia e lo stesso brief, confrontato su tutti i campi tranne `variante`. Contano anche i candidati `non_valido_schema` ed `errore_generazione`. Così ogni candidato, e ogni rigenerazione dopo un candidato non valido, ha un prompt diverso e un'impronta diversa. Per i generatori algoritmici la variante entra solo nel brief salvato.

## 3. Schema della risposta di creazione (compito 3; per B e C)

Lo schema si deriva da un descrittore e da un brief, e per il cruciverba anche dallo schema delle caselle (sezione 5.4). Un solo Gioco, senza riquadro, pagina, confidenza o `stato_tipologia`. Lo stesso schema valida la risposta dell'AI, i dati prodotti dai generatori algoritmici e la versione finale salvata dal revisore.

### 3.1 Involucro

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "urn:dbenigmistica:risposta-creazione:<id>:v<versione>",
  "type": "object",
  "additionalProperties": false,
  "required": ["versione_formato", "tipologia", "versione_tipologia", "titolo", "dati", "avvisi"],
  "properties": {
    "versione_formato": { "const": 1 },
    "tipologia": { "const": "<id>" },
    "versione_tipologia": { "const": <versione> },
    "titolo": { "anyOf": [ { "type": "null" }, { "type": "string", "minLength": 1, "maxLength": 200 } ] },
    "dati": { "$ref": "#/$defs/dati" },
    "avvisi": { "type": "array", "maxItems": 10, "items": { "type": "string", "minLength": 1, "maxLength": 300 } }
  },
  "$defs": { "dati": { }, "...": "griglia e voci degli elenchi" }
}
```

`avvisi`: dubbi dell'autore per chi rivede (AI) o avvisi del generatore (per esempio "difficoltà richiesta non raggiunta"). Non entrano nelle differenze.

### 3.2 Regole di derivazione

Si parte dalle regole 3, 4, 5 e 7 della 1.1, sezione 3.2, con queste differenze:

1. **`dati`**: oggetto chiuso, ogni campo e ogni elenco è una chiave obbligatoria, nell'ordine del descrittore. Un campo non obbligatorio ammette `null`, **tranne** quelli con ruolo `soluzione`: in creazione la soluzione c'è sempre, anche negli elenchi.
2. **Tipi**: come nella 1.1, tranne `immagine` e `griglia`. Un descrittore con un campo `immagine` fa fallire la derivazione con il messaggio "Tipologia con immagini: la creazione non è ammessa nel prototipo" (criterio 8).
3. **`griglia`**: `$ref` a `#/$defs/griglia`, oggetto chiuso `righe`, `colonne`, `celle`, `numeri` come nella 1.1, con l'alfabeto delle celle che dipende dalla resa:
   - resa `griglia` (cruciverba): `^[#A-Z]+$`. In creazione ogni casella bianca ha la sua lettera; niente `.` né `?`.
   - resa `griglia_logica`: `^[.#1-9]+$` e `numeri` con `maxItems: 0`. Il significato dei caratteri per Tipologia è nella sezione 5.
4. **Elenchi**: `array` di oggetti chiusi, `minItems` 0, `maxItems` 500, voce in `#/$defs/voce_<elenco>`.
5. **Specializzazione dal brief**: un campo del Gioco di tipo `enumerazione` chiamato `sottotipo` prende `"const": <brief.sottotipo>`.
6. **Specializzazione dallo schema delle caselle** (solo resa `griglia`, sezione 5.4):
   - campi con ruolo `righe` e `colonne`: `const` delle dimensioni; nella griglia `righe` e `colonne` `const`;
   - `celle`: `prefixItems` con un `pattern` per riga, `minItems` = `maxItems` = righe, `items: false`. Ogni casella nera resta `#`, ogni tratto di n bianche diventa `[A-Z]{n}`. Esempio: `....#....` → `^[A-Z]{4}#[A-Z]{4}$`;
   - `numeri`: `const` uguale a `numeri` dello schema delle caselle;
   - elenco con i ruoli `numero`, `direzione`, `definizione`: `prefixItems` con una voce per parola, nell'ordine dello schema delle caselle (orizzontali per numero, poi verticali per numero), `minItems` = `maxItems` = numero di parole, `items: false`. Ogni voce è la voce dell'elenco con `numero` `const`, `direzione` `const` e il campo con ruolo `soluzione` con `pattern` `^[A-Z]{n}$` (n = lunghezza);
   - se lo schema minimizzato supera 24.000 caratteri, si omettono i `prefixItems` dell'elenco e restano `minItems` e `maxItems`. I validatori della 1.2 fanno il resto. Con 13x13 lo schema resta sotto i 20.000 caratteri (stima dall'esempio 9x9 della sezione 10).
7. Solo `anyOf` per i valori nullabili, mai `oneOf`, nessun `format`.
8. Serializzazione: JSON minimizzato, chiavi nell'ordine di costruzione. È il testo che entra nell'impronta (sezione 1.3) e che va a Claude Code con `--json-schema`.

Il programma valida sempre la risposta con lo schema derivato, anche se il fornitore lo impone. Se l'adattatore deve togliere parole chiave non supportate (da scoprire nella prova di Claude Code), toglie solo vincoli (`pattern`, `minLength`, `maxLength`, `minimum`, `maximum`, `prefixItems`, `const` delle specializzazioni), mai chiavi o tipi. La validazione completa resta nel programma.

Controlli dopo lo schema, nel programma: per il cruciverba le lettere di ogni parola nella griglia uguali alla `soluzione` della voce (validatori `lunghezze-coerenti` e `incroci-coerenti` della 1.2); per i giochi logici le verifiche della sezione 5.

Esempi derivati con questo procedimento: `tipologie/_meta/esempi/creazione/risposta/derivato-*.schema.json` (sezione 10).

## 4. Schema `prototipo`, stati ed eventi (compito 6; per B)

### 4.1 Tabella `prototipo.candidati`

Una migrazione EF. Nessuna colonna per immagini (criterio 8). Nomi delle colonne in minuscolo con il trattino basso.

| Colonna | Tipo SQL | Null | Regola |
|---|---|---|---|
| `id` | `uuid` | no | Chiave primaria, UUID versione 7 dal programma |
| `tipologia` | `varchar(30)` | no | Id del descrittore |
| `versione_tipologia` | `integer` | no | `>= 0` |
| `famiglia` | `varchar(20)` | no | `CHECK` sulla lista chiusa del meta-schema |
| `ordine_generazione` | `integer` | no | `>= 1`, progressivo per Tipologia, assegnato alla creazione della riga, compresi i non validi. `UNIQUE (tipologia, ordine_generazione)` |
| `generato_il` | `timestamptz` | no | UTC, creazione della riga |
| `modo` | `varchar(12)` | no | `CHECK (modo IN ('ai', 'algoritmo'))` |
| `origine` | `varchar(12)` | no | `CHECK (origine = 'originale')` |
| `brief` | `jsonb` | no | Brief valido, con `variante` |
| `seme` | `bigint` | sì | Seme del generatore (C usa `long`). Obbligatorio con `modo = 'algoritmo'` e per il cruciverba (seme dello schema delle caselle): `CHECK (modo <> 'algoritmo' OR seme IS NOT NULL)` |
| `versione_generatore` | `varchar(40)` | sì | `^[a-z0-9][a-z0-9.-]{0,39}$`, per esempio `sudoku-1`, `schema-cruciverba-1`. `CHECK ((seme IS NULL) = (versione_generatore IS NULL))` |
| `schema_caselle` | `jsonb` | sì | Schema delle caselle del cruciverba (sezione 5.4); `null` per le altre famiglie |
| `versione_prompt` | `varchar(40)` | sì | `CHECK ((modo = 'ai') = (versione_prompt IS NOT NULL))` |
| `impronta_richiesta` | `bytea` | sì | 32 byte; `CHECK ((modo = 'ai') = (impronta_richiesta IS NOT NULL))`; `CHECK (impronta_richiesta IS NULL OR octet_length(impronta_richiesta) = 32)` |
| `stato` | `varchar(20)` | no | Sezione 4.2 |
| `messaggio` | `varchar(2000)` | sì | Errori dello schema o messaggio tecnico, solo per `non_valido_schema` ed `errore_generazione` |
| `contenuto_generato` | `jsonb` | sì | Risposta validata (involucro della sezione 3.1). È il "dato generato" del PLAN |
| `contenuto_finale` | `jsonb` | sì | Versione salvata dal revisore, stessa forma, valida per lo stesso schema |
| `differenze` | `jsonb` | sì | Sezione 4.3 |
| `tempo_attivo_ms` | `bigint` | no | `>= 0`, 0 alla creazione. Sezione 8.1 |
| `ultima_attivita_il` | `timestamptz` | sì | Ultimo istante di attività ricevuto (sezione 8.1) |
| `revisione_iniziata_il` | `timestamptz` | sì | Prima apertura del candidato in revisione |
| `revisione_conclusa_il` | `timestamptz` | sì | Clic su *Accetta* o *Scarta* |
| `motivo_scarto` | `varchar(30)` | sì | Codice della lista della sezione 4.2; `CHECK ((stato = 'scartato') = (motivo_scarto IS NOT NULL))` |
| `nota_scarto` | `varchar(1000)` | sì | Testo libero del revisore; solo nello schema `prototipo`, mai nel registro |

Altri vincoli:
- `CHECK ((contenuto_generato IS NOT NULL) = (stato IN ('da_rivedere', 'in_revisione', 'accettato', 'scartato')))`.
- `CHECK ((revisione_conclusa_il IS NOT NULL) = (stato IN ('accettato', 'scartato')))`.
- `CHECK (stato <> 'accettato' OR contenuto_finale IS NOT NULL)`.
- Indice `ix_candidati__tipologia_stato_ordine` su (`tipologia`, `stato`, `ordine_generazione`) per "prossimo candidato".

L'esito della revisione è lo stato finale (`accettato` o `scartato`): non c'è una colonna `esito` separata, che sarebbe ridondante. La risposta grezza sta in `prototipo.chiamate_ai` (sezione 4.4), una riga per tentativo, come nell'estrazione.

### 4.2 Stati del candidato

| Stato | Significato | Entra nella revisione | Nel rapporto |
|---|---|---|---|
| `in_generazione` | Riga creata, chiamata o generatore in corso | No | Non conta. All'avvio dell'host un candidato rimasto qui da oltre 10 minuti passa a `errore_generazione` |
| `non_valido_schema` | Risposta non valida per lo schema anche dopo la correzione | No | Contato a parte (`non_validi_schema`) |
| `errore_generazione` | Finestra esaurita o errore del fornitore | No | Contato a parte (`errori_generazione`) |
| `da_rivedere` | Valido, in attesa di revisione | Sì | Valido, non rivisto |
| `in_revisione` | Aperto dal revisore, cronometro avviato | Sì | Valido, non rivisto |
| `accettato` | Esito *Accetta* | Concluso | Rivisto, accettato |
| `scartato` | Esito *Scarta* con motivo | Concluso | Rivisto, scartato |

Passaggi ammessi: `in_generazione` → `non_valido_schema` | `errore_generazione` | `da_rivedere`; `da_rivedere` → `in_revisione`; `in_revisione` → `accettato` | `scartato`. Nessun altro. Gli stati finali non cambiano più.

Generazione (D1b-3): *Genera* crea un candidato. Se è `non_valido_schema`, il caso d'uso rigenera da solo con `variante` + 1, fino a 3 candidati per clic. Poi si ferma con un messaggio. `errore_generazione` non rigenera.

Ordine (D1b-3): il prossimo candidato di una Tipologia è il primo in `in_revisione` o `da_rivedere` per `ordine_generazione` crescente. *Accetta*, *Scarta* e il salvataggio funzionano solo su quel candidato; su un altro l'API risponde 409. Non esiste un'azione per scegliere o saltare.

Motivi dello scarto (lista chiusa): `soluzione_errata`, `regole_non_rispettate`, `incroci_incoerenti`, `qualita_insufficiente`, `banale_o_troppo_facile`, `contenuto_inadatto`, `altro`.

Salvataggio: *Accetta* richiede un `contenuto_finale` valido per lo schema della richiesta (sezione 3), altrimenti 422 con gli errori. *Scarta* salva il contenuto corrente solo se è valido; altrimenti `contenuto_finale` e `differenze` restano `null`.

### 4.3 Differenze

`differenze` = array di `{ "percorso": "<JSON Pointer RFC 6901>", "prima": <valore>, "dopo": <valore> }`, tra `contenuto_generato` e `contenuto_finale`, solo su `/titolo` e `/dati/**`.

- Confronto ricorsivo; un oggetto si confronta chiave per chiave, un array indice per indice.
- Foglia diversa → una voce. Voce di un elenco in più o in meno → una voce con l'oggetto intero e `null` dall'altra parte.
- `celle` della griglia: una voce per riga cambiata (`/dati/griglia/celle/3`), con le due stringhe.
- Ordine delle voci: quello della visita, chiavi nell'ordine del contenuto generato.
- `campi_modificati` (nel registro) = numero di voci.

Esempio (criterio 1, due campi modificati):

```json
[
  { "percorso": "/dati/esposto", "prima": "CENA SOVRANO", "dopo": "PRANZO DEL SOVRANO" },
  { "percorso": "/dati/spiegazione", "prima": "cena = pasto; sovrano = re", "dopo": "pranzo = pasto; sovrano = re" }
]
```

### 4.4 Tabella `prototipo.chiamate_ai`

Stesse colonne di `riviste.chiamate_ai` (1.1, sezione 7.1), con queste differenze:

| Colonna | Differenza |
|---|---|
| `uso` | `CHECK (uso = 'creazione')` |
| `impronta_pagina` | Rinominata `impronta_richiesta` (`bytea`, 32 byte) |
| `testo_nativo_troncato` | Assente |
| `candidato_id` | Nuova: `uuid NOT NULL`, chiave esterna verso `prototipo.candidati (id)` |

Indici: `ix_chiamate_ai__candidato_id`, `ix_chiamate_ai__avviata_il`. Cancellare la risposta grezza = `UPDATE ... SET risposta_grezza = NULL`: restano SHA-256 e metadati, e `verify-integrity` resta a 0.

### 4.5 Permessi

- `dbe_creazione`: `USAGE` sullo schema `prototipo`; `SELECT`, `INSERT`, `UPDATE` su `prototipo.candidati` e `prototipo.chiamate_ai`. Nessun `DELETE`: un candidato non si cancella.
- `dbe_estrazione`: nessun permesso sullo schema `prototipo`.
- Restano i permessi esistenti di `dbe_creazione` (nessuno su `riviste`, criterio 3).

### 4.6 Eventi nel registro

Solo metadati (D1b-2). Nome nel formato `^[a-z][a-z0-9_]{0,59}$`. `dati` compatto (senza spazi), chiavi nell'ordine scritto, nessuna chiave in più, valori sconosciuti `null`, esadecimale minuscolo. **Mai** nel registro: prompt, brief, risposta grezza, contenuto del candidato, `nota_scarto`, `messaggio`.

**Evento `chiamata_ai`, uso `creazione`**: uno per `ChiamataAi`.

| Campo di `NuovoEvento` | Valore |
|---|---|
| `avvenutoIl` | `avviata_il + durata_ms`, UTC |
| `tipo` | `chiamata_ai` |
| `entita` | `chiamata_ai` |
| `entitaId` | `id` della riga, formato `D` minuscolo |
| `autore` | `motore-creazione` |

```json
{"versione":1,"uso":"creazione","fornitore":"claude-code","modello":"claude-opus-5-5","versione_prompt":"creazione-v1-5b0e3c9a1f2d","impronta_richiesta":"<hex64>","tipologia":"crittografia-breve","versione_tipologia":0,"candidato_id":"<uuid>","tentativo":1,"correzione":false,"esito":"riuscita","durata_ms":41230,"token_ingresso":2120,"token_uscita":910,"token_cache_creazione":null,"token_cache_lettura":null,"costo":"0.031200","costo_valuta":"USD","costo_tipo":"indicativo","risposta_sha256":"<hex64>"}
```

**Evento `esito_revisione`**: uno per candidato, al clic su *Accetta* o *Scarta*. Vale anche per i candidati algoritmici, che non hanno chiamate.

| Campo di `NuovoEvento` | Valore |
|---|---|
| `avvenutoIl` | `revisione_conclusa_il`, UTC |
| `tipo` | `esito_revisione` |
| `entita` | `candidato_prototipo` |
| `entitaId` | `id` del candidato, formato `D` minuscolo |
| `autore` | `proprietario` |

```json
{"versione":1,"candidato_id":"<uuid>","tipologia":"crittografia-breve","versione_tipologia":0,"modo":"ai","ordine_generazione":3,"esito":"scartato","motivo_scarto":"soluzione_errata","tempo_attivo_ms":734000,"campi_modificati":2,"contenuto_finale_sha256":"<hex64>","versione_prompt":"creazione-v1-5b0e3c9a1f2d","seme":null,"versione_generatore":null}
```

- `esito`: `accettato` o `scartato`. `motivo_scarto`: codice della lista, `null` con `accettato`.
- `contenuto_finale_sha256`: SHA-256 della serializzazione canonica RFC 8785 (JCS) di `contenuto_finale`; `null` se manca. Serve alla fase 6 per collegare l'esito alla versione salvata senza mettere testo nel registro.
- `seme` come numero JSON (intero a 64 bit con segno); `null` se assente.

## 5. Formati per C: giochi logici, schema del cruciverba, difficoltà (compito 8)

Coordinate sempre da 1, riga poi colonna, origine in alto a sinistra. Ogni generatore riceve seme e dimensioni e produce l'involucro della sezione 3.1 con `titolo` `null`. Stesso seme e stesse dimensioni → stesso contenuto, byte per byte. Il candidato salva il seme del tentativo che ha prodotto la griglia, e la `versione_generatore`.

### 5.1 Sudoku (`sudoku`, v0)

```json
{
  "righe": 9,
  "colonne": 9,
  "griglia": { "righe": 9, "colonne": 9, "celle": ["5..1.....", "..."], "numeri": [] },
  "soluzione_griglia": { "righe": 9, "colonne": 9, "celle": ["583167294", "..."], "numeri": [] },
  "difficolta": 2
}
```

- `griglia` (ruolo `griglia`): lo schema da giocare. `.` casella vuota, `1`-`9` indizio.
- `soluzione_griglia`: la soluzione completa, solo cifre `1`-`9`.
- `difficolta`: livello stimato (sezione 5.5), 1-5.
- Verifiche del programma: ogni indizio uguale alla soluzione; soluzione valida per righe, colonne e riquadri 3x3; una e una sola soluzione (conteggio fino a 2).

### 5.2 Futoshiki (`futoshiki`, v0)

```json
{
  "righe": 5,
  "colonne": 5,
  "griglia": { "righe": 5, "colonne": 5, "celle": ["...4.", "....."], "numeri": [] },
  "soluzione_griglia": { "righe": 5, "colonne": 5, "celle": ["21345", "..."], "numeri": [] },
  "difficolta": 3,
  "segni": [ { "riga": 1, "colonna": 2, "verso": "destra", "segno": "maggiore" } ]
}
```

- Lato n da 5 a 7; cifre da `1` a n.
- `segni`: elenco di segni tra caselle adiacenti. `verso` `destra` confronta (riga, colonna) con (riga, colonna + 1); `sotto` con (riga + 1, colonna). `segno` `maggiore`: la prima casella è maggiore della seconda; `minore`: è minore. Ordine dell'elenco: per riga, poi colonna, poi `destra` prima di `sotto`.
- Corrispondenza con `SegnoFutoshiki(RigaA, ColonnaA, RigaB, ColonnaB)` di C: `maggiore` → A = (riga, colonna), B = la vicina; `minore` → A = la vicina, B = (riga, colonna). Coordinate da 1 nei `dati`, da 0 o da 1 nel codice a scelta di C.
- Verifiche: segni tra caselle adiacenti e dentro la griglia; soluzione quadrato latino che rispetta i segni; soluzione unica.

### 5.3 Nonogramma (`nonogramma`, v0)

```json
{
  "righe": 10,
  "colonne": 10,
  "griglia": { "righe": 10, "colonne": 10, "celle": ["..........", "..."], "numeri": [] },
  "soluzione_griglia": { "righe": 10, "colonne": 10, "celle": ["..####....", "..."], "numeri": [] },
  "difficolta": 2,
  "indizi_righe": [ { "indice": 1, "blocchi": "4" } ],
  "indizi_colonne": [ { "indice": 1, "blocchi": "0" } ]
}
```

- `griglia` (ruolo `griglia`): la griglia di partenza mostrata al giocatore, tutta `.`. Resta per uniformità con gli altri giochi logici: il ruolo `griglia` è sempre "la griglia all'inizio del gioco".
- `soluzione_griglia`: il disegno, `#` casella piena, `.` vuota.
- `indizi_righe` e `indizi_colonne`: una voce per linea, `indice` da 1, in ordine. `blocchi`: lunghezze dei blocchi pieni nell'ordine, separate da uno spazio; `"0"` per una linea vuota. Corrisponde a un indizio vuoto nel codice di C.
- Verifiche: indizi uguali a quelli calcolati dal disegno; soluzione unica.

### 5.4 Schema delle caselle del cruciverba (per C e B)

Prodotto dal generatore di C, passato al compilatore del prompt (sezione 6.3) e alla derivazione dello schema (sezione 3.2, regola 6). Si salva nel candidato (`schema_caselle`).

```json
{
  "versione_formato": 1,
  "righe": 9,
  "colonne": 9,
  "celle": ["....#....", "..."],
  "numeri": [ { "riga": 1, "colonna": 1, "numero": 1 } ],
  "parole": [ { "numero": 1, "direzione": "orizzontale", "riga": 1, "colonna": 1, "lunghezza": 4 } ],
  "seme": 4242,
  "versione_generatore": "schema-cruciverba-1"
}
```

- `celle`: `#` nera, `.` bianca.
- Regole dello schema (D1b-4): simmetria centrale (la casella (r, c) è nera se e solo se lo è (righe + 1 - r, colonne + 1 - c)); ogni tratto orizzontale e verticale di bianche è lungo almeno 2, cioè nessuna parola di una lettera e ogni casella bianca sta in una parola orizzontale e in una verticale; tutte le bianche collegate per lato; parole lunghe al massimo 9 lettere; caselle nere tra il 10% e il 22% del totale.
- Numerazione: si scorrono le caselle per riga, dall'alto, e in ogni riga da sinistra. Una casella bianca riceve il numero successivo se inizia una parola orizzontale (a sinistra bordo o nera, a destra bianca) o verticale (sopra bordo o nera, sotto bianca). Una casella che inizia due parole ha un solo numero.
- `numeri`: ogni casella numerata, nell'ordine della numerazione. `parole`: prima tutte le orizzontali per numero crescente, poi tutte le verticali per numero crescente.
- Dimensioni quadrate da 9 a 13 (brief). Se il brief chiede 11 o più, lo schema è di quella dimensione: il vincolo "almeno 3 candidati su 5 di 11x11 o più" si ottiene dai brief (sezione 9).
- Un esempio 9x9 valido: `tipologie/_meta/esempi/creazione/schema-cruciverba/valido-9x9.json`.

### 5.5 Livelli di difficoltà dei giochi logici

Regola comune: il risolutore logico applica le tecniche in ordine di livello, ripartendo dalla più semplice dopo ogni progresso. La **difficoltà stimata** è il livello della tecnica più difficile che è servita. Livello 5: le tecniche dei livelli 1-4 non bastano e serve un'ipotesi con ritorno indietro (ricerca). L'unicità si verifica sempre a parte, con il conteggio fino a 2.

Generazione per una difficoltà richiesta: il generatore prova i semi `seme_base`, `seme_base + 1`, ... fino a 50 tentativi. Tiene il primo con difficoltà stimata uguale a quella del brief. Se nessuno la raggiunge, tiene quello più vicino (a parità, il primo) e aggiunge in `avvisi` "Difficoltà richiesta N, ottenuta M.". Il candidato salva il seme del tentativo tenuto.

**Sudoku**

| Livello | Tecniche ammesse (in aggiunta alle precedenti) |
|---|---|
| 1 | Singolo nascosto (un numero può stare in una sola casella di una riga, colonna o riquadro) |
| 2 | Singolo nudo (una casella ammette un solo numero) |
| 3 | Intersezioni riquadro-linea (pointing e claiming); coppie nude e nascoste |
| 4 | Terne e quaterne nude e nascoste; X-wing; Swordfish; XY-wing |
| 5 | Serve la ricerca |

**Futoshiki** (candidati per casella da 1 a n)

| Livello | Tecniche ammesse (in aggiunta alle precedenti) |
|---|---|
| 1 | Singolo nudo e singolo nascosto in riga o colonna; esclusione dei valori già presenti |
| 2 | Limiti dei segni: con a > b, a non può valere il minimo dei candidati di b, e b non può valere il massimo di a (propagati fino a stabilità) |
| 3 | Catene di segni (a > b > c: a almeno 3, c al massimo n - 2); coppie nude e nascoste in riga o colonna |
| 4 | Terne nude e nascoste; limiti dei segni incrociati con i sottoinsiemi |
| 5 | Serve la ricerca |

**Nonogramma**

| Livello | Tecniche ammesse (in aggiunta alle precedenti) |
|---|---|
| 1 | Sovrapposizione semplice (caselle piene comuni alla disposizione più a sinistra e più a destra di ogni blocco); linee vuote o piene; chiusura dei blocchi completi |
| 2 | Risolutore per linea completo: intersezione di tutte le disposizioni compatibili di una linea, ripetuto su righe e colonne fino a stabilità |
| 3 | Prova su una casella: si assume un valore, si propaga con il livello 2; se nasce una contraddizione, vale l'altro. Al massimo 10 prove in tutto |
| 4 | Come il livello 3, senza limite di prove |
| 5 | Serve la ricerca (più caselle assunte insieme) |

Il livello si salva in `dati.difficolta`. Nel brief la difficoltà è quella voluta; nei dati è quella stimata.

---

# Parte 2. Prompt, descrittori, misure, esempi

## 6. Prompt di creazione v1 (compito 4; per B)

File: `tipologie/_meta/prompt-creazione-v1.md`.

### 6.1 Struttura

- Quattro modelli, uno per famiglia con AI: `crittografia` (crittografia breve, sinonimica e perifrastica), `gioco-breve` (anagramma a tema), `rebus` (vignette descritte a parole), `cruciverba` (schema delle caselle dal programma, D1b-4). Il compilatore sceglie il modello dalla `famiglia` del descrittore. Per `logico` non c'è modello: chiamarlo è un errore.
- Ogni modello sta tra `<!-- inizio-modello:<famiglia> -->` e `<!-- fine-modello:<famiglia> -->`. Si tolgono un a capo iniziale e uno finale, come nell'estrazione.
- Lo JSON Schema non è nel testo del prompt: va a Claude Code con `--json-schema` (sezione 3.2, regola 8) ed entra nell'impronta.

### 6.2 Segnaposti

| Segnaposto | Modelli | Valore |
|---|---|---|
| `{{descrittore}}` | tutti | Sezione 6.3 |
| `{{regole}}` | tutti | Sezione 6.3. Solo regole `approvata`, tranne nei test |
| `{{brief}}` | tutti | Sezione 6.3 |
| `{{schema_cruciverba}}` | solo `cruciverba` | Sezione 6.3 |

Un segnaposto sconosciuto, mancante o presente in un modello che non lo ammette fa fallire il caricamento. **Non esistono segnaposti per testi di Riviste, Giochi estratti o esempi** (criterio 3): il compilatore non ha ingressi da cui riceverli.

### 6.3 Testo dei segnaposti

Righe separate da LF, nessun a capo finale. Il modello fornisce le righe vuote intorno.

**`{{descrittore}}`**

```
Tipologia `<id>`, versione <versione>: <nome>.
Famiglia: <famiglia>. Resa: <resa.modello>.
Descrizione: <descrizione>
Campi di `dati`, in quest'ordine:
- `<nome>` (<etichetta o nome>): <tipo leggibile>[, obbligatorio][. <descrizione>]
Elenco `<nome>` (<etichetta o nome>)[. <descrizione>] Ogni voce ha:
- `<nome>` (<etichetta o nome>): ...
```

- Tipo leggibile come nella 1.1, sezione 5.2, tranne `griglia` → "griglia" (il formato lo descrive il modello del cruciverba). `immagine` non si presenta: la derivazione fallisce prima.
- ", obbligatorio" se il campo è obbligatorio **o** ha ruolo `soluzione` (sezione 3.2, regola 1).
- Il campo `regole` del descrittore non entra qui: entra in `{{regole}}`.

**`{{regole}}`**

```
<regole.testo>
Versione delle regole: <regole.versione>, approvate il <approvate_il>.
```

Nei test, con regole in bozza, la seconda riga è `Versione delle regole: <versione>, bozza non approvata (solo test).` Fuori dai test il compilatore rifiuta la bozza con un errore esplicito (ADR 002, punto 3).

**`{{brief}}`**

```
<<<BRIEF
Tema: <tema, oppure "libero">
Sottotipo: <sottotipo>                        (riga solo se non null)
Dimensioni: <righe> righe e <colonne> colonne (riga solo se non null)
Difficoltà: <n> su 5 (<molto facile | facile | media | difficile | molto difficile>)
Vincoli:                                      (oppure "Vincoli: nessuno")
- <vincolo>
Variante: <variante>
BRIEF>>>
```

`codice` non entra nel prompt. Nei valori le sequenze `<<<BRIEF` e `BRIEF>>>` diventano `<<BRIEF` e `BRIEF>>`, ripetendo finché servono, come per il testo nativo dell'estrazione. Lo schema del brief vieta già gli a capo e i caratteri di controllo.

**`{{schema_cruciverba}}`**

```
Righe: <righe>. Colonne: <colonne>.
Caselle, una riga di testo per riga dello schema (`#` nera, `.` bianca):
<celle[0]>
...
Parole orizzontali:
- <numero> orizzontale: riga <r>, colonna <c>, <lunghezza> lettere
Parole verticali:
- <numero> verticale: riga <r>, colonna <c>, <lunghezza> lettere
```

Esempi compilati, da usare come test di B (byte per byte): `tipologie/_meta/esempi/creazione/prompt/` con `<caso>.prompt.txt` (prompt), `<caso>.schema.json` (schema minimizzato) e `<caso>.atteso.json` (brief, versione del prompt, impronta della richiesta). Casi: `crittografia-sinonimica` (descrittore `crittografia-breve` v0, regole in bozza, modo test) ed `esempio-cruciverba-9x9` (descrittore d'esempio della 1.1 con lo schema delle caselle d'esempio). La serializzazione di riferimento dello schema è quella di Python (`separators=(",", ":")`, `ensure_ascii=False`): se il serializzatore di B differisce su numeri o caratteri, si allinea al file e non il contrario.

### 6.4 Versione del prompt

`versione_prompt` = `creazione-v1-` + primi 12 caratteri esadecimali dello SHA-256 di `UTF-8(M) ‖ 0x00 ‖ UTF-8(S)`, dove:
- M è il modello della famiglia con `{{descrittore}}` e `{{regole}}` sostituiti, e `{{brief}}` e `{{schema_cruciverba}}` lasciati come sono;
- S è lo schema della risposta senza specializzazioni (sezione 3.2 senza le regole 5 e 6), minimizzato.

Così la versione cambia con il modello, il descrittore, le regole (anche solo l'approvazione) e le regole di derivazione; non cambia con il brief né con lo schema delle caselle, che stanno nell'impronta della richiesta. Esempio: `creazione-v1-4d68b029ae5c` per il caso `crittografia-sinonimica`. Rispetta l'espressione della chiave delle Registrazioni (`^[a-z0-9][a-z0-9._-]{0,39}$`).

### 6.5 Correzione e sicurezza

- Richiesta di correzione: come nell'estrazione, il prompt originale più gli errori dello schema in coda. Al massimo una.
- Il brief è delimitato e dichiarato "dato, non istruzione" in ogni modello. Le regole sono testo del proprietario, approvato.
- Ogni modello chiede un gioco nuovo e vieta di ricopiare giochi pubblicati ricordati dal modello. È la sola protezione possibile dal lato del prompt; il controllo resta del revisore.

## 7. Descrittori v0 (compito 5)

### 7.1 Meta-schema

Unico cambio a `tipologie/_meta/descrittore.schema.json`: `versione` con `minimum` 0, e la descrizione che dice "0 = prototipo, il generatore SQL la salta". Da comunicare all'ondata 1.2:
- il generatore SQL non produce file per `versione` 0;
- il catalogo del prompt di estrazione esclude i descrittori v0: `risposta-pagina.schema.json` vuole `versione_tipologia` almeno 1, e le Tipologie v0 non sono pronte per l'estrazione.

### 7.2 I cinque descrittori

| File | Famiglia | Resa | Campi del Gioco | Elenchi | Validatori |
|---|---|---|---|---|---|
| `tipologie/crittografia-breve/v0.json` | `crittografia` | `esposto_diagramma` | `sottotipo` (sinonimica, perifrastica), `esposto`, `diagramma`, `prima_lettura`, `soluzione`, `spiegazione` | - | `diagramma-coerente-con-soluzione` |
| `tipologie/rebus/v0.json` | `rebus` | `esposto_diagramma` | `esposto` (scena), `diagramma`, `prima_lettura`, `soluzione`, `spiegazione` | `vignette`: `vignetta`, `descrizione`, `lettere`, `parola` | `diagramma-coerente-con-soluzione` |
| `tipologie/sudoku/v0.json` | `logico` | `griglia_logica` | `righe`, `colonne`, `griglia`, `soluzione_griglia`, `difficolta` | - | `griglia-coerente-con-dimensioni` |
| `tipologie/futoshiki/v0.json` | `logico` | `griglia_logica` | come il sudoku | `segni`: `riga`, `colonna`, `verso`, `segno` | `griglia-coerente-con-dimensioni` |
| `tipologie/nonogramma/v0.json` | `logico` | `griglia_logica` | come il sudoku | `indizi_righe`, `indizi_colonne`: `indice`, `blocchi` | `griglia-coerente-con-dimensioni` |

- Nessun campo di tipo `immagine` e nessuna resa `immagine_diagramma` (criterio 8). Il rebus descrive le figure a parole nell'elenco `vignette`.
- `spiegazione` e `prima_lettura` sono campi senza ruolo: aiutano chi rivede e si mostrano come dettagli.
- Regole in bozza, `versione` 1, `scritte_da` "agente A, ondata 1b.1". Il proprietario le approva in chat entro il 3/1/2027; l'agente scrive `stato`, `approvate_il` e `approvate_da` in un commit dedicato che il proprietario chiede. Il file resta `v0.json`: cambia solo `regole.versione` se cambia il testo.
- La verifica della soluzione unica dei giochi logici non è un validatore del descrittore (la lista chiusa non lo prevede): la fa il codice di C, scelto dall'id della Tipologia. Il nome `soluzione-unica` entra nella lista con i descrittori v1 della fase 5.

## 8. Misure della revisione (compito 7; per la 1b.2)

### 8.1 Tempo attivo

**Attività.** Istanti in cui il revisore usa la schermata "Revisione" di un candidato: apertura del candidato; `pointerdown`, `keydown`, `input`, `wheel`, `scroll`; `pointermove` (al massimo uno ogni 5 secondi); ritorno del fuoco alla finestra; clic su *Accetta* o *Scarta*, che chiude la misura. Orologio: `Date.now()` del browser, in millisecondi. Prototipo e host girano sullo stesso PC, quindi l'orologio è lo stesso del server.

**Calcolo.** Con t0 < t1 < ... < tn gli istanti di attività del candidato, anche attraverso i ricaricamenti:

tempo_attivo = Σ per i da 1 a n di d(i), con d(i) = t(i) - t(i-1) se 0 ≤ t(i) - t(i-1) ≤ 120.000 ms, altrimenti 0.

- Una pausa oltre 2 minuti è esclusa per intero: conta 0, non 2 minuti. Esattamente 120.000 ms conta.
- Un intervallo negativo (orologio spostato indietro) conta 0.
- È una funzione pura degli istanti: si prova con un orologio simulato (criterio 2).

**Salvataggio e ripresa.** Il client invia al server `tempo_attivo_ms` e l'istante dell'ultima attività ogni 15 secondi se sono cambiati, e sempre con *Accetta* e *Scarta*. Il server salva `tempo_attivo_ms` e `ultima_attivita_il` e rifiuta un `tempo_attivo_ms` minore di quello salvato. Dopo un ricaricamento il client legge i due valori e riparte: l'istante salvato è t(i-1) per la prima attività nuova, con la stessa regola dei 2 minuti. Si perdono al massimo 15 secondi se il browser si chiude di colpo.

**Esempio per i test** (istanti in secondi): 0 (apertura), 60, 150, ricaricamento, 400, 410 (*Accetta*). Intervalli 60, 90, 250, 10. Tempo attivo 60 + 90 + 0 + 10 = 160 secondi.

### 8.2 Rapporto `dbe eval review-time`

**Gruppi della prova**, in quest'ordine fisso:

| Gruppo | Tipologie | Soglia G1 |
|---|---|---|
| `crittografia_breve` | `crittografia-breve` | ≤ 20 minuti, quota ≥ 50% |
| `anagramma` | `anagramma` | ≤ 20 minuti, quota ≥ 50% |
| `cruciverba` | `cruciverba-classico` | ≤ 60 minuti, quota ≥ 50% |
| `rebus` | `rebus` | Nessuna |
| `sudoku` | `sudoku` | Nessuna |
| `giochi_logici` | `futoshiki`, `nonogramma` | Nessuna |

Le versioni della Tipologia non separano i gruppi; il rapporto le elenca.

**Conteggi per gruppo.**
- `validi` = candidati in `da_rivedere`, `in_revisione`, `accettato`, `scartato`.
- `rivisti` = `accettati` + `scartati`. `non_rivisti` = `validi` - `rivisti`. Per D1b-3 i rivisti sono i primi validi in ordine di generazione.
- `non_validi_schema` ed `errori_generazione`: contati a parte, mai nei tempi né nella quota.
- `generati` = `validi` + `non_validi_schema`. `quota_non_validi` = `non_validi_schema` / `generati`.

**Misure sui rivisti** (accettati e scartati insieme: anche uno scarto costa tempo di revisione):
- minuti = `tempo_attivo_ms` / 60.000, in ordine crescente x(0) ≤ ... ≤ x(n-1);
- quantile Q(p) con interpolazione lineare (metodo 7 di Hyndman e Fan, come `PERCENTILE.INC`): h = (n - 1) p, Q(p) = x(⌊h⌋) + (h - ⌊h⌋) (x(⌊h⌋+1) - x(⌊h⌋)); con n = 1 tutti i quantili valgono x(0);
- `mediana` = Q(0,5), `q1` = Q(0,25), `q3` = Q(0,75), più `minimo` e `massimo`;
- `quota_accettata` = `accettati` / `rivisti`;
- con denominatore 0 il valore è `null`, mai 0 o 100%.

**Avvisi e soglie.**
- `misura_indicativa` = `true` se `rivisti` < 10. Il testo leggibile scrive "misura indicativa".
- Gruppi con soglia: `tempo_entro_soglia` = `mediana` ≤ soglia; `quota_entro_soglia` = `quota_accettata` ≥ 0,5; `null` se la misura manca. Il rapporto mette a confronto, non decide: G1 resta del proprietario (28/2/2027).
- Gruppi senza soglia: `soglia_g1` = `null`; tempi e quota si riportano.

**Arrotondamento e forma.** Calcolo in doppia precisione; nel rapporto 4 decimali, arrotondamento lontano da zero, come nella 1.1. Due esecuzioni sugli stessi dati danno lo stesso JSON, salvo `generato_il`.

```json
{
  "versione_formato": 1,
  "generato_il": "2027-02-21T18:00:00Z",
  "gruppi": [
    {
      "gruppo": "crittografia_breve",
      "tipologie": [ { "tipologia": "crittografia-breve", "versioni": [0] } ],
      "generati": 7, "validi": 6, "non_validi_schema": 1, "errori_generazione": 0, "quota_non_validi": 0.1429,
      "rivisti": 5, "non_rivisti": 1, "accettati": 3, "scartati": 2, "quota_accettata": 0.6,
      "minuti": { "mediana": 14.5, "q1": 10.25, "q3": 19.0, "minimo": 8.0, "massimo": 31.2 },
      "misura_indicativa": true,
      "soglia_g1": { "minuti_massimi": 20, "quota_minima": 0.5, "tempo_entro_soglia": true, "quota_entro_soglia": true }
    }
  ]
}
```

I 30 candidati sintetici del criterio 5 (1b.2, B) coprono almeno: un gruppo con 1 rivisto, uno con 0 rivisti, una pausa oltre 2 minuti, un non valido e un errore di generazione, una mediana con interpolazione.

## 9. Brief tipo (compito 9)

File: `.planning/phases/1b/brief-tipo.md`. Per ognuna delle quattro famiglie con AI: 4 brief in bozza, scritti dalle regole v0 (crittografia, rebus) e dai descrittori d'esempio della 1.1 (anagramma, cruciverba), in JSON valido per `brief.schema.json`. Il proprietario li corregge entro il 3/1/2027 (D1b-7).

- I 5 candidati di una famiglia usano i brief nell'ordine 1, 2, 3, 4, 1. Il quinto candidato riusa il brief 1 con `variante` 2.
- Cruciverba: i brief 1, 2 e 3 sono di 11x11 o più, quindi almeno 3 candidati su 5 sono di 11x11 o più (ROADMAP, fase 1b).
- Sudoku e giochi logici non hanno brief tipo: il brief si compila nella schermata con difficoltà e dimensioni.

## 10. Esempi e verifica meccanica

Esempi in `tipologie/_meta/esempi/creazione/`, tutti inventati dal progetto:

| Cartella | Validi | Non validi (motivo) |
|---|---|---|
| `brief/` | crittografia sinonimica, anagramma a tema, rebus, cruciverba 11x11, sudoku, nonogramma 10x15 | crittografia senza sottotipo; sottotipo nel rebus; sudoku 8x8; giochi logici con tema; cruciverba 11x13; difficoltà 6; anagramma senza tema; vincolo con a capo; proprietà `esempi` estranea |
| `risposta/` | crittografia sinonimica, rebus, anagramma (descrittore d'esempio), cruciverba 9x9 (descrittore d'esempio, lettere casuali), sudoku, futoshiki 5x5, nonogramma 10x10 | sottotipo diverso dal brief; soluzione `null`; riquadro estraneo; rebus con chiave `immagine`; anagramma senza `avvisi`; cruciverba con una nera spostata; cruciverba con una definizione in meno; sudoku con una lettera; futoshiki con verso `sopra`; nonogramma con numeri nella griglia |
| `risposta/derivato-*.schema.json` | Sette schemi derivati con le regole della sezione 3.2: crittografia breve specializzata su `sinonimica`, rebus, anagramma d'esempio, cruciverba d'esempio specializzato sullo schema 9x9, sudoku, futoshiki, nonogramma | - |
| `schema-cruciverba/` | `valido-9x9.json` | - |
| `prompt/` | Due prompt compilati con schema e impronta (sezione 6.3) | - |

I giochi logici d'esempio hanno soluzione unica, verificata dallo script di riferimento. Il campo `difficolta` degli esempi è indicativo: non viene dal risolutore a tecniche di C.

Verifica eseguita il 20/11/2026 fuori dal repository (cartella temporanea dell'agente), come nella 1.1, sezione 9:

1. Python `jsonschema` 4.25.1 con `Draft202012Validator`. `check_schema` conferma che `brief.schema.json`, il meta-schema modificato e i sette schemi derivati sono conformi alla bozza 2020-12. Tutti i validi passano, tutti i non validi falliscono, con il primo errore uguale a quello voluto. I cinque descrittori v0 passano il meta-schema; una `versione` -1 è rifiutata. Gli esempi della 1.1 (descrittori, risultato atteso, risposta per pagina con involucro e schema derivato) danno gli stessi esiti di prima.
2. Lo schema delle caselle d'esempio rispetta le regole della sezione 5.4 (simmetria, parole di almeno 2 lettere, bianche collegate, numerazione ricalcolata uguale, nere al 12%).
3. Il compilatore di riferimento trova nei quattro modelli solo i segnaposti ammessi, e rifiuta le regole in bozza fuori dai test.
4. Dimensione dello schema del cruciverba specializzato, minimizzato: 8.215 caratteri per il 9x9 d'esempio, 14.176 per un 13x13 con 62 parole. Resta sotto la soglia di 24.000 della sezione 3.2, regola 6.
5. Programma .NET 10 con `Corvus.Text.Json.Validator` 5.8.12: sezione 11, punto 9.

Lo script di riferimento (derivazione, compilatore del prompt, generatori degli esempi) resta fuori dal repository, come nella 1.1. Il codice vero è C# di B e C, con test sugli esempi di questa cartella.

## 11. Scostamenti e punti aperti

1. **Risposta grezza in `prototipo.chiamate_ai`, non nei candidati.** Il PLAN elenca "risposta grezza e SHA-256" tra le colonne dei candidati. Con la correzione un candidato può avere due risposte; una riga per tentativo, come nell'estrazione, le conserva entrambe e usa lo stesso record `ChiamataAi`. La cancellazione resta un `UPDATE` sulla tabella delle chiamate.
2. **Nessuna colonna `esito`.** L'esito è lo stato finale (`accettato`, `scartato`); una colonna in più sarebbe ridondante. Il motivo dello scarto ha la sua colonna.
3. **`variante` nel brief.** Conseguenza dell'impronta confermata [A]: senza, due candidati dello stesso brief avrebbero la stessa impronta, e Registra e Riproduci li confonderebbero.
4. **Cruciverba quadrato.** Il brief ammette solo 9x9-13x13 quadrati, come il generatore di C (`dimensione`). Il ROADMAP dice "9x9-13x13": compatibile.
5. **Regole dello schema delle caselle più strette di D1b-4.** "Nessuna parola di una lettera" si legge come "ogni tratto di bianche lungo almeno 2", quindi ogni bianca è incrociata. In più: parole al massimo di 9 lettere e nere tra il 10% e il 22%, per aiutare l'AI a riempire gli incroci. Il validatore di C controlla già simmetria, parole di una lettera, collegamento e numerazione; **lunghezza massima e quota di nere sono nuove per C**.
6. **Alfabeto delle griglie logiche.** Il tipo `griglia` della 1.1 ammette `#.?A-Z`; i giochi logici in creazione usano `.#1-9` (sezione 3.2, regola 3). Non tocca l'estrazione oggi; la fase 5, che estrae sudoku e giochi logici, dovrà estendere `risposta-pagina.schema.json` con una versione nuova.
7. **Rebus nel prototipo.** `esposto` del rebus contiene la descrizione della scena, non un testo da stampare: è la lettura più vicina al criterio 8 con la resa `esposto_diagramma`. In fase 7 il rebus v1 passa a `immagine_diagramma`.
8. **Autore dell'evento `esito_revisione`.** Il prototipo non ha autenticazione: l'autore è la costante `proprietario`. La fase 6 la sostituisce con l'utente.
9. **Seconda verifica con Corvus.** Eseguita il 20/11/2026 con un programma .NET 10 e `Corvus.Text.Json.Validator` 5.8.12, fuori dal repository: 55 coppie schema-esempio (meta-schema con esempi 1.1 e descrittori v0, risultato atteso, schema derivato della 1.1, brief, sette schemi di creazione). Stessi esiti di Python, 0 errori. Il validatore del codice regge quindi `prefixItems`, `const` e `if`/`then` degli schemi nuovi.
10. Nessuna scelta di questo documento contraddice D1-D7 o D1b-1 - D1b-9. Il ruolo del database (`SET ROLE` o stringa di connessione dedicata) resta una proposta di B, da approvare dall'agente principale. Il nome della chiave del modello di creazione lo propone B (D1b-6).
