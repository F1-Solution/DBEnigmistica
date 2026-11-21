# Fase 0 - Piano dell'ondata 0.2

Progetto di Opus A, 11/11/2026. Contesto e stato verificato: `CONTEXT.md` nella stessa cartella. Criteri di accettazione: `ROADMAP.md`, fase 0, numeri 1-13.

## 0. Ondate

| Ondata | Stato | Contenuto |
|---|---|---|
| 0.1 (A, B, C) | Conclusa, in `main` | Pacchetti centralizzati, OCR e DDL a runtime eliminati, CLI `dbe` su System.CommandLine 2.0 e Generic Host, Testcontainers PostgreSQL 17, immagine Docker in CI |
| **0.2** | **Questo piano** | Sonnet A: modello e migrazioni. Sonnet B: segreti e Fornitore AI. Sonnet C: CI, formato, architettura, copertura |
| 0.3 | Conclusa, in `main` | Haiku A: documenti. Sonnet B: `dbe verify-integrity` nella CLI e test end-to-end in CI. Tag `v0.1.0` |

Regole comuni ai tre agenti:
- Stessa cartella di lavoro, ramo `fase-0-fondamenta`. Nessun commit: li fa il proprietario.
- Ognuno tocca solo i file del proprio elenco (sezione 6). Per un file di un altro agente: scrivi la modifica esatta nel rapporto finale, l'agente principale la inoltra al proprietario del file.
- TDD: prima il test rosso, poi il codice.
- Prima di consegnare: `dotnet format DBEnigmistica.slnx --include <i tuoi file>` e `dotnet build DBEnigmistica.slnx -c Release -warnaserror`.
- Commenti di una frase. Termini del glossario di `CLAUDE.md`.
- Mai aprire `.env` né `src/DBEnigmistica.Console/appsettings.Development.json`.

---

## 1. Progetto del modello (per Sonnet A)

### 1.1 Scelte tecniche, motivate in una riga

| Tema | Scelta | Motivo |
|---|---|---|
| Identificativi delle tabelle nuove | `uuid` generato dall'applicazione con `Guid.CreateVersion7()` | Ordinato nel tempo e nessun `DEFAULT` nel database. |
| Tabelle esistenti | Restano con nomi inglesi e colonne PascalCase, spostate negli schemi `riviste` e `tipologie` | La fase 1 le sostituisce con le tabelle per Tipologia; rinominarle ora è lavoro buttato. |
| Tabelle nuove | Nomi italiani `snake_case`, impostati con `ToTable`/`HasColumnName` | Coerenti con il glossario e con le tabelle generate dalla fase 1. |
| Origine e altri insiemi chiusi | Colonna `text` con `CHECK (... IN (...))` | Si cambiano con una migrazione normale, senza i limiti di `ALTER TYPE`. |
| Enum C# senza valore zero (`Origine` parte da 1) | Il convertitore EF rifiuta i valori non definiti | `default(Origine)` non diventa mai un'origine valida per caso. |
| Date | Solo `timestamptz`; in C# `DateTime` con `Kind = Utc`, troncato al microsecondo | Npgsql rifiuta già i `DateTime` non UTC; il microsecondo è la precisione di PostgreSQL e serve all'impronta. |
| Periodi | `inizio timestamptz NOT NULL`, `fine timestamptz NULL` (esclusa; nulla = senza scadenza), più colonna generata `periodo tstzrange` | L'intervallo semiaperto evita sovrapposizioni di un istante tra periodi contigui. |
| Territorio | Testo: `MONDO` oppure codice ISO 3166-1 alfa-2 (`IT`); colonna generata `territorio_intervallo int4range` | Con `int4range` il vincolo di esclusione usa `&&` nativo: `MONDO` copre tutti i paesi, due paesi diversi non si toccano. |
| Diritti (Soggetti, licenze, Cessioni) | Nello schema `catalogo`, accanto ai Giochi originali; `registro` contiene solo `eventi` | Il vincolo di esclusione e le chiavi esterne restano nello stesso schema; il registro resta un puro storico immutabile. |
| Impronta del registro | Calcolata in C#, codifica a lunghezza prefissata, dati salvati come `text` | L'impronta copre esattamente i byte salvati, senza normalizzazioni del database in mezzo. |
| Concorrenza sull'ultima riga del registro | `pg_advisory_xact_lock` dentro una transazione, più `UNIQUE (impronta_precedente)` | Il lucchetto serializza le aggiunte; il vincolo impedisce una biforcazione della catena anche se qualcuno salta il lucchetto. |
| SQL non esprimibile con EF | Migrazione scritta a mano dopo la baseline | Rigenerare la baseline non cancella trigger, ruoli e vincoli di esclusione. |
| Porte nuove (registro) | Interfacce in Application; Infrastructure riceve il riferimento ad Application | Gli adattatori dipendono verso l'interno (ROADMAP 2.1). |

### 1.2 Tipi del dominio

Namespace fissati (servono ai test di architettura di Sonnet C):

| Namespace | Tipi |
|---|---|
| `DBEnigmistica.Domain.Diritti` | `Origine`, `Soggetto`, `TipoSoggetto`, `LicenzaInEntrata`, `Cessione`, `Periodo`, `Territorio`, `ProvaPubblicoDominio` |
| `DBEnigmistica.Domain.Catalogo` | `GiocoOriginale`, `StatoInedito`, `PrimaPubblicazione` |
| `DBEnigmistica.Domain.Pubblicazione` | `PolicyDiPubblicazione`, `CandidatoPubblicazione`, `EsitoPolicy`, `MotivoBlocco` |
| `DBEnigmistica.Domain.Registro` | `EventoRegistro`, `NuovoEvento`, `ImprontaEvento`, `VerificaCatena`, `EsitoVerifica` |
| `DBEnigmistica.Domain.Entities` (esistente) | `Game` riceve `Origine` |

Regole: oggetti valore immutabili (`record` o `sealed class` con `init`); i metodi restituiscono copie nuove; le invarianti si controllano nel costruttore o nel metodo di fabbrica (`ArgumentException` con messaggio in italiano). `Catalogo` non dipende da `Entities` (test di architettura).

**`Origine`** (enum): `Originale = 1`, `Licenza = 2`, `PubblicoDominio = 3`, `Rivista = 4`. Nel database: `originale`, `licenza`, `pubblico_dominio`, `rivista`.

**`Soggetto`** (entità): `Id` (Guid), `Tipo` (`TipoSoggetto.Persona = 1 | Organizzazione = 2`), `Denominazione` (1-200 caratteri), `IdentificativoFiscale` (opzionale, al più 32), `Note` (opzionale), `CreatoIl` (UTC). Unica anagrafica per titolare, licenziante e cliente.

**`Periodo`** (valore): `Inizio` (UTC), `Fine` (UTC, opzionale, esclusa). Invariante: `Fine > Inizio`. Metodi: `Contiene(DateTime istante)`, `SiSovrappone(Periodo altro)`, `TerminatoPrima(DateTime istante)`.

**`Territorio`** (valore): `Territorio.Mondo` oppure `Territorio.Paese("IT")` (due lettere maiuscole A-Z). `Codice` restituisce `MONDO` o il codice. `SiSovrappone(Territorio)`: vero se uno dei due è `Mondo` o se i codici coincidono.

**`ProvaPubblicoDominio`** (valore): `Autore` (non vuoto), `Anno` (intero, da -3000 all'anno corrente), `Motivo` (non vuoto, al più 1000).

**`LicenzaInEntrata`** (entità): `Id`, `GiocoId`, `LicenzianteId` (Soggetto), `EstremiAccordo` (1-500), `Periodo`.

**`Cessione`** (entità): `Id`, `GiocoId`, `ClienteId` (Soggetto), `StipulataIl` (UTC), `Periodo`, `Esclusiva` (bool), `Territorio`, `ComeInedito` (bool: il Gioco è stato promesso o venduto come inedito, RQ-03). Vedi la sezione 7, punto D1.

**`PrimaPubblicazione`** (valore): `Il` (UTC), `Luogo` (1-300 caratteri, per esempio "Sito pubblico" o "Rivista X n. 12").

**`StatoInedito`** (valore, due stati): `StatoInedito.Inedito` oppure `StatoInedito.Pubblicato(PrimaPubblicazione)`. `Pubblica(PrimaPubblicazione)` da inedito restituisce il nuovo stato; da pubblicato lancia `InvalidOperationException` ("Il Gioco è già stato pubblicato il ... in ...: lo stato di inedito non torna indietro").

**`GiocoOriginale`** (radice dell'aggregato, tabella `catalogo.giochi`):
- `Id`, `Tipologia` (identificativo del descrittore, per esempio `cruciverba-classico`, regex `^[a-z][a-z0-9-]{1,60}$`), `VersioneTipologia` (≥ 1), `Titolo` (opzionale, al più 200), `CreatoIl` (UTC).
- `required Origine Origine` in `{Originale, Licenza, PubblicoDominio}`: `Rivista` lancia eccezione alla creazione.
- `TitolareId` (Soggetto, opzionale al salvataggio).
- `ProvaPubblicoDominio?` (opzionale al salvataggio, decisione 5).
- `Licenze` e `Cessioni` (collezioni in sola lettura).
- `StatoInedito` (alla creazione sempre `Inedito`).
- Metodi: `Crea(...)`, `AssegnaTitolare(Guid)`, `RegistraProvaPubblicoDominio(...)`, `AggiungiLicenza(...)`, `AggiungiCessione(...)` (rifiuta subito una seconda Cessione esclusiva sovrapposta per periodo e territorio; il database resta l'ultima difesa), `RegistraPrimaPubblicazione(PrimaPubblicazione)`.
- Le prove mancanti non impediscono il salvataggio (decisione 5).

**`Game`** (Gioco estratto, esistente): aggiungere `public required Origine Origine { get; init; }`. Il dominio accetta solo `Origine.Rivista` per un Gioco estratto; il database lo impone con un `CHECK`. Aggiornare tutti i punti che costruiscono un `Game`: `GameExtractionService.cs:61`, `RepositoryIntegrationTests.cs:179`, `IntegrationTestBase.NewGame`, `GameSolutionServiceTests.cs:22`, `:51`, `CsvExportTests.cs:58-113`.

### 1.3 `PolicyDiPubblicazione`

Funzione pura: `static EsitoPolicy Valuta(CandidatoPubblicazione c)`. Nessun accesso a database, orologio o configurazione.

`CandidatoPubblicazione` (record): `Origine`, `TitolareId?`, `Licenze` (lista), `ProvaPubblicoDominio?`, `Cessioni` (lista), `StatoInedito`, `Approvato` (bool, fornito dal chiamante; l'approvazione con impronta arriva in fase 6), `Istante` (UTC), `TerritorioDiPubblicazione` (per il Sito pubblico: `Territorio.Mondo`).

`EsitoPolicy`: `Pubblicabile` (bool) e `Motivi` (lista di `MotivoBlocco`, vuota se pubblicabile). La policy raccoglie **tutti** i motivi, non solo il primo.

| # | Condizione | `MotivoBlocco` |
|---|---|---|
| 1 | `Origine == Rivista` (qualunque sia il resto) | `OrigineRivista` |
| 2 | `Origine == Licenza` e nessuna licenza con licenziante, estremi e periodo | `ProvaLicenzaMancante` |
| 3 | `Origine == Licenza`, licenze presenti ma nessuna con `Periodo.Contiene(Istante)` | `LicenzaNonValidaAllIstante` |
| 4 | `Origine == PubblicoDominio` e prova assente | `ProvaPubblicoDominioMancante` |
| 5 | `Origine` in `{Originale, Licenza}` e `TitolareId` nullo | `TitolareMancante` (sezione 7, D2) |
| 6 | Esiste una Cessione `Esclusiva` con `Periodo.Contiene(Istante)` e territorio che si sovrappone a `TerritorioDiPubblicazione` | `CessioneEsclusivaInConflitto` (sezione 7, D3) |
| 7 | `StatoInedito` è inedito ed esiste una Cessione `ComeInedito` non terminata prima di `Istante` | `PromessoComeInedito` (sezione 7, D1) |
| 8 | `Approvato == false` | `NonApprovato` |
| - | Nessuna delle precedenti | `Pubblicabile = true` |

Il "registro completo" di RQ-04 entra nella policy in fase 6. Test a tabella (`[Theory]` con `MemberData`): almeno un caso per riga, la combinazione `Rivista` + tutto il resto valido, casi di confine sui periodi (istante uguale a `Inizio`: contenuto; uguale a `Fine`: escluso). Copertura dei rami al 100% (criterio 10).

### 1.4 Schemi, tabelle e colonne

Estensione: `btree_gist` (`modelBuilder.HasPostgresExtension("btree_gist")`), serve al vincolo di esclusione su `uuid`. È un'estensione fidata: basta il permesso `CREATE` sul database.

**Schema `riviste`** (contesto Acquisizione). Le tabelle esistenti si spostano qui con nome e colonne invariati: `magazines`, `pages`, `games`, `game_solutions`, `crossword_games`, `rebus_games`, `logic_games`, `anagram_games`.
- `games.origine text NOT NULL CHECK (origine = 'rivista')`, nessun default.
- `games.SolutionPageId`: chiave esterna `ON DELETE NO ACTION` (EF `DeleteBehavior.NoAction`). **Non `RESTRICT`**: `NO ACTION` controlla a fine istruzione, così cancellare una Rivista con i suoi Giochi e le sue pagine-soluzione interne funziona ancora.
- `game_solutions.SolutionPageId`: da `SET NULL` a `NO ACTION`, stessa regola della decisione 3 (sezione 7, nota N1).

**Schema `tipologie`**: `game_type_schemas` spostata qui con nome e colonne invariati. I descrittori versionati arrivano in fase 1.

**Schema `catalogo`** (contesto Catalogo e creazione):

`catalogo.soggetti`
| Colonna | Tipo | Vincoli |
|---|---|---|
| `id` | uuid | PK |
| `tipo` | text | NOT NULL, `CHECK (tipo IN ('persona','organizzazione'))` |
| `denominazione` | varchar(200) | NOT NULL, `CHECK (btrim(denominazione) <> '')` |
| `identificativo_fiscale` | varchar(32) | NULL |
| `note` | text | NULL |
| `creato_il` | timestamptz | NOT NULL |

`catalogo.giochi`
| Colonna | Tipo | Vincoli |
|---|---|---|
| `id` | uuid | PK |
| `tipologia` | varchar(61) | NOT NULL, `CHECK (tipologia ~ '^[a-z][a-z0-9-]{1,60}$')` |
| `versione_tipologia` | integer | NOT NULL, `CHECK (> 0)` |
| `titolo` | varchar(200) | NULL |
| `origine` | text | **NOT NULL, nessun default**, `CHECK (origine IN ('originale','licenza','pubblico_dominio'))` |
| `titolare_id` | uuid | NULL, FK `soggetti(id)` `ON DELETE NO ACTION` |
| `pd_autore` | varchar(200) | NULL |
| `pd_anno` | integer | NULL |
| `pd_motivo` | varchar(1000) | NULL |
| `stato_inedito` | text | NOT NULL, `CHECK (stato_inedito IN ('inedito','pubblicato'))` |
| `prima_pubblicazione_il` | timestamptz | NULL |
| `prima_pubblicazione_luogo` | varchar(300) | NULL |
| `creato_il` | timestamptz | NOT NULL |

Vincoli di tabella:
- `ck_giochi_pd_tutti_o_nessuno`: i tre `pd_*` tutti nulli o tutti valorizzati.
- `ck_giochi_pd_solo_pubblico_dominio`: `pd_autore IS NULL OR origine = 'pubblico_dominio'`.
- `ck_giochi_stato_inedito`: (`inedito` e i due campi di prima pubblicazione nulli) oppure (`pubblicato` e i due campi valorizzati).
- Trigger `tr_giochi_inedito_senza_ritorno` (`BEFORE UPDATE`, funzione `catalogo.inedito_senza_ritorno()`): se `OLD.stato_inedito = 'pubblicato'`, rifiuta `NEW.stato_inedito <> 'pubblicato'` e ogni modifica di `prima_pubblicazione_il` o `prima_pubblicazione_luogo`. `RAISE EXCEPTION ... USING ERRCODE = 'check_violation'`.
- Nessun `DEFAULT` sulla tabella: lo stato iniziale `inedito` lo scrive il dominio.

`catalogo.licenze_in_entrata`
| Colonna | Tipo | Vincoli |
|---|---|---|
| `id` | uuid | PK |
| `gioco_id` | uuid | NOT NULL, FK `giochi(id)` `ON DELETE CASCADE` |
| `licenziante_id` | uuid | NOT NULL, FK `soggetti(id)` `NO ACTION` |
| `estremi_accordo` | varchar(500) | NOT NULL, non vuoto |
| `inizio` | timestamptz | NOT NULL |
| `fine` | timestamptz | NULL, `CHECK (fine IS NULL OR fine > inizio)` |

`catalogo.cessioni`
| Colonna | Tipo | Vincoli |
|---|---|---|
| `id` | uuid | PK |
| `gioco_id` | uuid | NOT NULL, FK `giochi(id)` `ON DELETE NO ACTION` (un Gioco ceduto non si cancella) |
| `cliente_id` | uuid | NOT NULL, FK `soggetti(id)` `NO ACTION` |
| `stipulata_il` | timestamptz | NOT NULL |
| `inizio` | timestamptz | NOT NULL |
| `fine` | timestamptz | NULL, `CHECK (fine IS NULL OR fine > inizio)` |
| `esclusiva` | boolean | NOT NULL |
| `territorio` | varchar(5) | NOT NULL, `CHECK (territorio ~ '^([A-Z]{2}|MONDO)$')` |
| `come_inedito` | boolean | NOT NULL |
| `periodo` | tstzrange | `GENERATED ALWAYS AS (tstzrange(inizio, fine, '[)')) STORED` |
| `territorio_intervallo` | int4range | `GENERATED ALWAYS AS (CASE WHEN territorio = 'MONDO' THEN int4range(0, 676) ELSE int4range((ascii(substr(territorio,1,1)) - 65) * 26 + ascii(substr(territorio,2,1)) - 65, (ascii(substr(territorio,1,1)) - 65) * 26 + ascii(substr(territorio,2,1)) - 64) END) STORED` |

Vincolo di esclusione (migrazione a mano):
```sql
ALTER TABLE catalogo.cessioni ADD CONSTRAINT ex_cessioni_esclusive_sovrapposte
  EXCLUDE USING gist (gioco_id WITH =, periodo WITH &&, territorio_intervallo WITH &&)
  WHERE (esclusiva);
```
Le colonne generate non si mappano in EF. Errore atteso nei test: SQLSTATE `23P01` (`exclusion_violation`).

**Schema `registro`** (contesto Registro):

`registro.eventi`
| Colonna | Tipo | Vincoli |
|---|---|---|
| `id` | bigint | `GENERATED ALWAYS AS IDENTITY`, PK (solo ordine di lettura, non entra nell'impronta) |
| `avvenuto_il` | timestamptz | NOT NULL |
| `tipo` | varchar(60) | NOT NULL, `CHECK (tipo ~ '^[a-z][a-z0-9_]{0,59}$')` |
| `entita` | varchar(60) | NOT NULL, stessa regex (per esempio `gioco_originale`, `cessione`) |
| `entita_id` | varchar(100) | NOT NULL |
| `autore` | varchar(200) | NOT NULL, non vuoto |
| `dati` | text | NOT NULL, `CHECK (dati IS JSON OBJECT)` (PostgreSQL 16 o superiore) |
| `impronta_precedente` | bytea | NOT NULL, `UNIQUE`, `CHECK (octet_length(...) = 32)` |
| `impronta` | bytea | NOT NULL, `UNIQUE`, `CHECK (octet_length(...) = 32)` |

Trigger (migrazione a mano):
- `tr_eventi_no_update` `BEFORE UPDATE FOR EACH ROW` e `tr_eventi_no_delete` `BEFORE DELETE FOR EACH ROW`: `RAISE EXCEPTION 'registro.eventi è in sola aggiunta' USING ERRCODE = 'insufficient_privilege'`.
- `tr_eventi_no_truncate` `BEFORE TRUNCATE FOR EACH STATEMENT`: stesso errore.
- `REVOKE UPDATE, DELETE, TRUNCATE ON registro.eventi FROM PUBLIC` e da tutti i ruoli di contesto.

### 1.5 Impronta concatenata

- Prima riga: `impronta_precedente` = 32 byte a zero.
- `impronta = SHA-256(impronta_precedente || contenuto)`, dove `contenuto` è la concatenazione, in quest'ordine, dei campi `"v1"`, `avvenuto_il`, `tipo`, `entita`, `entita_id`, `autore`, `dati`. Ogni campo si codifica come `<numero di byte UTF-8 in decimale>:<byte UTF-8>,` (netstring): nessuna ambiguità senza limitare i caratteri.
- `avvenuto_il` in formato `yyyy-MM-ddTHH:mm:ss.ffffffZ`, invariante, dopo il troncamento al microsecondo. Il valore salvato è lo stesso valore troncato.
- `dati`: JSON prodotto da `System.Text.Json` e salvato come testo identico; la verifica rilegge il testo, non lo riformatta.
- Il campo `"v1"` permette di cambiare formato in futuro senza ambiguità.
- `ImprontaEvento.Calcola(byte[] precedente, NuovoEvento e)` e `VerificaCatena.Verifica(IEnumerable<EventoRegistro> inOrdineDiId)` sono funzioni pure del dominio. `EsitoVerifica`: `Integra` (bool), `EventiVerificati`, `PrimoIdNonValido?`, `Motivo?` ("impronta precedente non coincide" oppure "impronta ricalcolata diversa").

Aggiunta (Infrastructure, `RegistroEventiRepository : IRegistroEventi`):
1. Usa la transazione corrente del `DbContext` o ne apre una. Senza transazione il lucchetto di sessione cadrebbe a fine istruzione.
2. `SELECT pg_advisory_xact_lock(4419001)`: costante unica, dichiarata in una sola classe.
3. Legge `impronta` dell'ultima riga (`ORDER BY id DESC LIMIT 1`).
4. Calcola l'impronta e inserisce. `UNIQUE (impronta_precedente)` fa fallire un inserimento concorrente che abbia saltato il lucchetto.

Limite noto: la catena non rivela la cancellazione delle ultime righe fatta da un superutente. Trigger e permessi la impediscono; un'ancora esterna (per esempio l'impronta di testa nel backup firmato) va valutata in fase 6 con RQ-12.

### 1.6 Ruoli PostgreSQL

Creati nella migrazione a mano, `NOLOGIN` (ruoli di gruppo; gli utenti di accesso arrivano in fase 2). `CREATE ROLE` vale per tutto il cluster: va protetto con `IF NOT EXISTS` in un blocco `DO`, e la `Down` non elimina i ruoli.

| Ruolo | `riviste` | `catalogo` | `tipologie` | `registro.eventi` |
|---|---|---|---|---|
| `dbe_estrazione` | USAGE; SELECT, INSERT, UPDATE, DELETE | nessuno | USAGE; SELECT | USAGE; SELECT, INSERT |
| `dbe_creazione` | **nessuno (nemmeno USAGE)** | USAGE; SELECT, INSERT, UPDATE, DELETE | USAGE; SELECT | USAGE; SELECT, INSERT |
| `dbe_originalita` | USAGE; SELECT | USAGE; SELECT | USAGE; SELECT | nessuno |
| `dbe_consegna` | nessuno | USAGE; SELECT | USAGE; SELECT | USAGE; SELECT, INSERT |

- `REVOKE ALL ON SCHEMA riviste, catalogo, tipologie, registro FROM PUBLIC`.
- `GRANT USAGE` sulle sequenze delle tabelle con identità a chi ha `INSERT`.
- `ALTER DEFAULT PRIVILEGES IN SCHEMA <schema> GRANT ...` con la stessa tabella, così le future tabelle per Tipologia ereditano i permessi.
- L'interruttore degli esempi (fase 6) sarà un `GRANT SELECT ON riviste...` a `dbe_creazione`: oggi non esiste.
- L'applicazione continua a collegarsi con l'utente di `ConnectionStrings__DefaultConnection`. I ruoli si provano nei test con `SET ROLE` (l'utente di Testcontainers è superutente).

### 1.7 Baseline delle migrazioni

1. Eliminare `20260131223647_InitialCreate*`, `20260131235606_AddGameSolutions*` e `EnigmisticaDbContextModelSnapshot.cs`.
2. Aggiornare `EnigmisticaDbContext`, poi generare `Baseline` con `dotnet ef migrations add Baseline` (variabile `ConnectionStrings__DefaultConnection` impostata a una stringa qualsiasi: `add` non si collega).
3. Aggiungere a mano la migrazione successiva `VincoliRuoliERegistro` con `migrationBuilder.Sql(...)`: funzioni e trigger, colonne generate e vincolo di esclusione, `CHECK` non generati da EF, ruoli e permessi. `Down` simmetrica, tranne i ruoli.
4. Eseguire `dotnet format` sui file generati (EF scrive con CRLF su Windows).
5. Nessun database è mai stato migrato: non serve un percorso di aggiornamento.

### 1.8 Date in UTC

- Nessuna colonna `timestamp without time zone`, `date` o `time` negli schemi `riviste`, `catalogo`, `tipologie`, `registro`.
- Il dominio rifiuta i `DateTime` con `Kind` diverso da `Utc` (`ArgumentException`).
- Test d'integrazione (criterio 13): interroga `information_schema.columns` e verifica zero colonne di quei tipi; salva e rilegge un valore e verifica `Kind == Utc` e lo stesso istante.

### 1.9 Pagina-soluzione in uso (decisione 3)

- Il database rifiuta già (chiave esterna `NO ACTION`). Il messaggio lo aggiunge l'applicazione.
- `MagazineRepository.DeleteAsync` (e ogni futura cancellazione di pagina) prima di cancellare cerca i Giochi **non** cancellati dalla stessa operazione che usano una delle pagine come pagina-soluzione (`games.SolutionPageId` e `game_solutions.SolutionPageId`). Se ne trova, lancia `PaginaSoluzioneInUsoException` (Domain), con messaggio: "La pagina N della Rivista «Titolo» è pagina-soluzione di: Gioco 12 «Titolo» (Rivista «...» n. ...), ...". Nessuna cancellazione parziale.
- Test d'integrazione: (a) Rivista A con pagina usata da un Gioco della Rivista B: cancellazione rifiutata, messaggio con l'id del Gioco, righe intatte; (b) stessa cosa con `DELETE` SQL diretto: SQLSTATE `23503`; (c) Rivista con pagina-soluzione usata solo dai propri Giochi: cancellazione riuscita.

### 1.10 Regola per il futuro generatore di migrazioni per Tipologia

Da riportare nell'ADR dell'ondata 0.3. Il generatore (fase 1) non accetta stringhe SQL da descrittori, AI o configurazione.

| Tipo del descrittore | Tipo SQL (unico ammesso) |
|---|---|
| `testo` con `max` (1-4000, obbligatorio) | `varchar(max)` |
| `testo_lungo` | `text` |
| `intero` | `integer` |
| `decimale` | `numeric(12,4)` |
| `booleano` | `boolean` |
| `data` | `timestamptz` |
| `enumerazione` | `text` con `CHECK (col IN (...))`; valori con regex `^[a-z0-9_]{1,40}$` |
| `griglia` | `jsonb` |
| `immagine` | `uuid` con chiave esterna verso la tabella dei ritagli (fase 1) |

- Identificatori: regex `^[a-z][a-z0-9_]{0,40}$`, non nella lista delle parole riservate di PostgreSQL, sempre quotati.
- Indici ammessi, e solo questi: chiave primaria; `UNIQUE (gioco_id)` per la tabella 1:1; btree sulle chiavi esterne delle tabelle figlie (`elenchi`); btree su un campo con `"indice": true` e tipo in `{intero, testo, enumerazione, data}`.
- `NOT NULL` solo da `obbligatorio: true`. Nessun `DEFAULT`, nessun trigger, nessuna funzione.
- Permessi: ereditati dai privilegi predefiniti dello schema (sezione 1.6), mai `GRANT` generati.
- Il risultato è un file di migrazione rivisto e registrato; si applica solo con `dbe migrate` o `dbe types apply <id>`. Nessun DDL a runtime.

### 1.11 `dbe verify-integrity`: divisione tra 0.2 e 0.3

| Pezzo | Ondata |
|---|---|
| Tabella, trigger, permessi, impronta, aggiunta con lucchetto | 0.2 (Sonnet A) |
| `ImprontaEvento`, `VerificaCatena` (dominio, test unitari) | 0.2 (Sonnet A) |
| Caso d'uso `VerificaIntegritaRegistro` in Application e lettura ordinata in Infrastructure | 0.2 (Sonnet A) |
| Test d'integrazione: alterazione da superutente (`SET session_replication_role = replica`, `UPDATE` di `dati`), il caso d'uso riporta l'id alterato | 0.2 (Sonnet A) |
| Comando `dbe verify-integrity` (Console), codice d'uscita diverso da 0, test end-to-end in CI | 0.3 (Sonnet B) |

---

## 2. Pacchetto Sonnet A: modello e migrazioni

**File e cartelle che può toccare**
- `src/DBEnigmistica.Domain/**`
- `src/DBEnigmistica.Application/**` (compreso `Configuration/DependencyInjection.cs`)
- `src/DBEnigmistica.Infrastructure/**` (compresi `DBEnigmistica.Infrastructure.csproj`, che riceve il riferimento ad Application, `Configuration/DependencyInjection.cs`, `Migrations/**`)
- `tests/DBEnigmistica.Tests/Integration/**`
- `tests/DBEnigmistica.Tests/Unit/Domain/**` (nuova)
- `tests/DBEnigmistica.Tests/Unit/Application/**` (nuova)
- Test esistenti che costruiscono un `Game`: `tests/DBEnigmistica.Tests/Unit/Services/CsvExportTests.cs`, `Unit/Services/GameExtractionServiceTests.cs`, `Unit/Cli/GameSolutionServiceTests.cs`

**Primo passo, entro poco**: aggiungere `Domain/Diritti/Origine.cs` e la proprietà `Game.Origine`, e aggiornare i punti di costruzione elencati in 1.2. Poi segnalarlo all'agente principale: Sonnet C ne ha bisogno per i propri test.

**Test da scrivere prima (TDD)**
- Unitari, dominio: `Periodo`, `Territorio`, `StatoInedito` (anche il non-ritorno), `GiocoOriginale` (Origine `Rivista` rifiutata, seconda Cessione esclusiva sovrapposta rifiutata, salvataggio senza prove consentito), `PolicyDiPubblicazione` (tabella 1.3, rami al 100%), `ImprontaEvento` (vettore fisso: input noto, impronta esadecimale attesa scritta nel test), `VerificaCatena` (catena integra, impronta alterata, anello mancante, catena vuota).
- Integrazione (PostgreSQL 17):
  - Gioco originale e Gioco estratto senza origine (`INSERT` SQL senza la colonna): `23502`. `column_default` nullo per le due colonne `origine`.
  - Gioco estratto con origine diversa da `rivista`: `23514`.
  - Due Cessioni esclusive sovrapposte (stesso paese; `MONDO` e `IT`): `23P01`. Non sovrapposte (`IT` e `FR`; periodi contigui) o non esclusive: accettate.
  - `UPDATE` e `DELETE` e `TRUNCATE` su `registro.eventi`: rifiutati.
  - Inedito: da `pubblicato` a `inedito` rifiutato; modifica della data di prima pubblicazione rifiutata.
  - Due aggiunte concorrenti al registro (due contesti, `Task.WhenAll`): catena lineare, verifica integra.
  - Alterazione da superutente: verifica non integra con l'id giusto.
  - Ruoli: con `SET ROLE dbe_creazione` una `SELECT` su `riviste.games` dà `42501`; `dbe_estrazione` non può fare `UPDATE` su `registro.eventi`.
  - Date (1.8) e pagina-soluzione (1.9).
- Aggiornare i test d'integrazione esistenti ai nomi qualificati (`riviste.magazines`, ...).

**Criteri coperti**: 9 (tranne il comando CLI), 10, 13; parte di 4 e 11.

**Verifica**
```
dotnet build DBEnigmistica.slnx -c Release -warnaserror
dotnet test DBEnigmistica.slnx -c Release --nologo
```
Con Docker attivo. Tutti verdi, nessun test saltato.

---

## 3. Pacchetto Sonnet B: segreti e Fornitore AI

**File e cartelle che può toccare**
- `src/DBEnigmistica.AI/**` (compreso il csproj: via `PackageReference Include="Anthropic.SDK"`)
- `src/DBEnigmistica.Console/**` (compresi `Cli/CliHost.cs`, `Cli/CliApp.cs`, `Program.cs`, `appsettings.json`, `appsettings.Development.json.template`, `Configuration/**`, `Commands/**`)
- `tests/DBEnigmistica.Tests/Unit/Cli/CliAppTests.cs`, `tests/DBEnigmistica.Tests/Unit/Cli/FornitoreAi*.cs` (nuovi)
- `.githooks/**` (nuova), `.gitleaks.toml` (nuovo, solo se serve una eccezione motivata)
- `.env.template`

**Lavoro**
1. Eliminare `ClaudeApiClient.cs` e `ClaudeGameAnalyzer.cs`. In `AddAiServices` registrare `FornitoreAiNonDisponibile : IAiGameAnalyzer`, che lancia `InvalidOperationException("Il Fornitore AI è disponibile dalla fase 1")`. Così `GameExtractionService` e `SchemaProposalService` restano risolvibili senza toccare Application. Restano `Models/` e `Prompts/`.
2. Nuova configurazione, sezione `FornitoreAi` (sostituisce `Anthropic`; `AnthropicOptions.cs` va eliminato):

   | Chiave | Variabile d'ambiente | Valore predefinito in `appsettings.json` |
   |---|---|---|
   | `FornitoreAi:Tipo` | `FornitoreAi__Tipo` | `claude-code` (ammessi: `claude-code`, `api-anthropic`) |
   | `FornitoreAi:Modello` | `FornitoreAi__Modello` | `claude-sonnet-5-5` |
   | `FornitoreAi:ClaudeCode:Eseguibile` | `FornitoreAi__ClaudeCode__Eseguibile` | `claude` |
   | `FornitoreAi:Api:ChiaveApi` | `FornitoreAi__Api__ChiaveApi` | assente (solo variabile d'ambiente o user-secrets) |

   Autenticazione di Claude Code: variabile `CLAUDE_CODE_OAUTH_TOKEN` non vuota, oppure file `.claude/.credentials.json` nella cartella dell'utente.
3. `FornitoreAiOptionsValidator` (Console), con accesso a variabili d'ambiente, file e `PATH` dietro un'interfaccia sostituibile nei test. Messaggi:
   - Tipo vuoto o sconosciuto: "Fornitore AI non configurato: imposta FornitoreAi__Tipo (claude-code o api-anthropic)."
   - `claude-code`, eseguibile non trovato (percorso assoluto inesistente o nome assente dal `PATH`): "Claude Code non trovato: ..."
   - `claude-code`, non autenticato: "Claude Code non autenticato: imposta CLAUDE_CODE_OAUTH_TOKEN oppure esegui l'accesso con claude."
   - `api-anthropic`, chiave vuota: "Chiave API mancante: imposta FornitoreAi__Api__ChiaveApi."
   - Modello vuoto: "Modello del Fornitore AI mancante: imposta FornitoreAi__Modello."
4. Nuovo comando `dbe check-ai` ("Verifica la configurazione del Fornitore AI, senza chiamate di rete"), con `HostRequirements.Ai`: stampa fornitore e modello, codice 0 se valido, `ExitCodes.Configuration` (3) se no. È il comando che usa l'AI nel test del criterio 7 (sezione 7, D4).
5. Hook gitleaks: `.githooks/pre-commit` (sh, funziona anche con Git per Windows) che esegue `gitleaks git --pre-commit --staged --redact --no-banner`. Se `gitleaks` manca, il commit si ferma con un messaggio che spiega come installarlo. Attivazione: `git config core.hooksPath .githooks`. La guida la scrive l'ondata 0.3: B riporta nel rapporto i comandi esatti.
6. `.env.template`: togliere la riga su `Anthropic__ApiKey`; aggiungere commenti, senza valori, su `FornitoreAi__Tipo` e `CLAUDE_CODE_OAUTH_TOKEN`. `appsettings.Development.json.template`: sezione `FornitoreAi` senza segnaposti di chiave.

**Test da scrivere prima (TDD)**
- Validatore: un test per ogni messaggio del punto 3; caso valido per `claude-code` (eseguibile finto e token) e per `api-anthropic`.
- `dbe check-ai` via `CliApp.RunAsync`: senza `FornitoreAi:Tipo`, con eseguibile mancante, con chiave vuota per `api-anthropic` → codice 3 e messaggio esplicito su stderr. Nessun client HTTP registrato nel contenitore (asserzione sul `ServiceCollection`).
- `list-games` (e `--help`) senza alcuna configurazione AI: nessun errore di configurazione AI. Sostituisce `EmptyApiKey_DoesNotBlockHostForCommandsThatDoNotUseAi` e `EmptyApiKey_BlocksHostForCommandsThatUseAi`.
- Il contenitore si costruisce con `ValidateOnBuild = true` (nessun servizio irrisolvibile dopo l'eliminazione dell'adattatore).

**Richieste ad altri agenti**
- A Sonnet C: togliere la riga `Anthropic.SDK` da `Directory.Packages.props` quando B ha tolto il riferimento dal csproj.
- A Sonnet C: nel commento del `Dockerfile` sostituire `Anthropic__ApiKey` con `FornitoreAi__Tipo` e `CLAUDE_CODE_OAUTH_TOKEN`.
- All'ondata 0.3: `docs/CI.md`, `CLAUDE.md` e README citano ancora `Anthropic__ApiKey`.

**Criteri coperti**: 6 (per la parte `src/`), 7.

**Verifica**
```
dotnet build DBEnigmistica.slnx -c Release -warnaserror
dotnet test DBEnigmistica.slnx -c Release --nologo --filter "FullyQualifiedName~Cli|FullyQualifiedName~FornitoreAi"
git grep -niE "Anthropic\.SDK|Anthropic__ApiKey" -- src .env.template
gitleaks git --redact --no-banner .
```
L'ultima ricerca deve essere vuota per `src/` e `.env.template`.

---

## 4. Pacchetto Sonnet C: CI, formato, architettura, copertura

**File e cartelle che può toccare**
- `.github/workflows/ci.yml`, `.github/workflows/publish.yml`, `.github/scripts/**` (nuova)
- `Dockerfile`, `.dockerignore`
- `Directory.Packages.props` (unico proprietario)
- `tests/DBEnigmistica.Tests/DBEnigmistica.Tests.csproj` (unico proprietario)
- `tests/DBEnigmistica.Tests/Architecture/**` (nuova)
- `tests/DBEnigmistica.Tests/Unit/Services/DataExportServiceTests.cs` e `SchemaProposalServiceTests.cs` (nuovi)
- `.editorconfig`, `.gitattributes` (nuovi)
- Passata finale di `dotnet format` su tutta la soluzione (solo spazi e fine riga), **solo dopo** che l'agente principale conferma che A e B hanno consegnato.

**Lavoro**
1. `.editorconfig` minimo: `root = true`; per `*.cs`: `indent_style = space`, `indent_size = 4`, `end_of_line = lf`, `charset = utf-8`, `insert_final_newline = true`, `trim_trailing_whitespace = true`. Nessuna regola di stile con severità `warning` o `error` (eviterebbe ondate di errori). `.gitattributes`: `* text=auto eol=lf`, più `*.sln`/`*.slnx` e `*.ps1` con `eol=crlf` se servono. La rinormalizzazione dell'indice (`git add --renormalize .`) la fa l'agente principale al momento del commit.
2. `ci.yml`, lavoro "Build e test":
   - compilazione con `-warnaserror`;
   - passo `dotnet format "$SOLUTION" --verify-no-changes --no-restore` (prima della compilazione, per un errore rapido);
   - passo "Soglia di copertura": legge `coverage.cobertura.xml`, somma righe coperte e totali dei soli pacchetti `DBEnigmistica.Domain` e `DBEnigmistica.Application`, fallisce sotto l'80% e scrive il valore nel riepilogo. Python già presente sul runner, nessun pacchetto nuovo.
   - I test di architettura girano con gli altri: nessun passo in più.
3. Test di architettura con `NetArchTest.Rules` (aggiungerlo a `Directory.Packages.props` e al csproj dei test):
   - Domain non dipende da `DBEnigmistica.Application`, `.Infrastructure`, `.AI`, `.Console`, `Microsoft.EntityFrameworkCore`, `Npgsql`.
   - Application non dipende da `Microsoft.EntityFrameworkCore`, `Npgsql`, `Spectre.Console`, `System.CommandLine`, `Anthropic`, `DBEnigmistica.Infrastructure`, `.AI`, `.Console`.
   - `DBEnigmistica.Domain.Catalogo` non dipende da `DBEnigmistica.Domain.Entities` (la creazione non vede le Riviste).
   - Nessun progetto referenzia un pacchetto il cui nome inizia con `Anthropic` (lettura di `Directory.Packages.props` nel test, criterio 6).
4. Copertura: test unitari per `DataExportService` (JSON e CSV, Rivista inesistente, elenco vuoto, statistiche) e `SchemaProposalService` (con `Mock<IAiGameAnalyzer>`). Per costruire un `Game` usa `Origine = Origine.Rivista`, disponibile dopo il primo passo di A.
5. `Directory.Packages.props`: togliere `Anthropic.SDK` **solo dopo** che `git grep -n "Anthropic.SDK" src` è vuoto (lavoro di B). Se alla fine non lo è, lasciarla e segnalarlo.
6. `Dockerfile`: commento sulle variabili aggiornato ai nomi della sezione 3. Nel build stage, dopo `test -f /app/appsettings.json`, un controllo che fallisce se nella pubblicazione ci sono librerie native (`*.so`, `*.so.*`, `*.dylib`, cartelle `runtimes/*/native`), con messaggio che indica i file. La fase 1 lo sostituirà con un elenco ammesso (PDFium).
7. `ci.yml`, lavoro "Build immagine Docker": `load: true`, tag locale, poi `docker run --rm <tag> --version` che deve stampare `0.0.0`.
8. `publish.yml`: nuovo lavoro `tag-da-main` (checkout con `fetch-depth: 0`), eseguito solo per `github.ref_type == 'tag'`; lancia `.github/scripts/verifica-tag-da-main.sh "$GITHUB_SHA" origin/main`, che usa `git merge-base --is-ancestor` e fallisce con un messaggio chiaro. `version` dipende da `ci` e da `tag-da-main` (con `if: always()` e controllo dell'esito, perché il lavoro saltato negli avvii manuali non blocchi). Così nessuna immagine e nessuna release partono da un tag fuori da `main`.
9. Verificare che `grep -n "cp -n" Dockerfile .github/workflows/publish.yml` resti vuoto (criterio 2: oggi lo è già).

**Test da scrivere prima (TDD)**
- Test di architettura (3).
- Test di `DataExportService` e `SchemaProposalService` (4).
- Script del tag: prova locale in una cartella temporanea (repository con `main` e un ramo laterale): esito 0 per un commit di `main`, diverso da 0 per uno del ramo. I comandi della prova vanno nel rapporto.

**Criteri coperti**: 1 (CI), 2, 6 (`Directory.Packages.props`), 8, 11, 12 (parte automatica: controllo del tag, `--version`, immagine senza librerie native).

**Verifica**
```
dotnet format DBEnigmistica.slnx --verify-no-changes
dotnet build DBEnigmistica.slnx -c Release -warnaserror
dotnet test DBEnigmistica.slnx -c Release --nologo --collect:"XPlat Code Coverage"
docker build -t dbe:locale . && docker run --rm dbe:locale --version
bash .github/scripts/verifica-tag-da-main.sh <sha> main
```
La soglia di copertura si controlla con lo stesso script Python del passo di CI, eseguito in locale. La verifica sul runner avviene dopo il push del proprietario.

---

## 5. Ordine di esecuzione

1. Le tre consegne partono insieme.
2. A completa il primo passo (`Origine`, `Game.Origine`) e lo segnala. C scrive i test che costruiscono `Game` solo dopo.
3. B finisce prima di quanto C tocchi la riga `Anthropic.SDK`.
4. A e B consegnano. L'agente principale lo conferma a C.
5. C fa la passata finale di `dotnet format`, la verifica completa e consegna.
6. Revisione del codice (Opus, sforzo `xhigh`), gitleaks, correzione dei problemi CRITICAL e HIGH.
7. Il proprietario approva; commit; push; CI verde su GitHub. Solo allora la riga `PHASE:0 WAVE:0.2 STATUS:complete` in `PROGRESS.md`.

---

## 6. Mappa dei file

| File o cartella | Proprietario | Gli altri chiedono così |
|---|---|---|
| `src/DBEnigmistica.Domain/**` | A | - |
| `src/DBEnigmistica.Application/**` (anche la registrazione dei servizi) | A | B non la tocca: usa l'adattatore segnaposto in AI |
| `src/DBEnigmistica.Infrastructure/**`, `Migrations/**` | A | - |
| `src/DBEnigmistica.AI/**` | B | - |
| `src/DBEnigmistica.Console/**` (`Program.cs`, `CliHost.cs`, `appsettings*.json*`) | B | A non ne ha bisogno: registra i servizi in `AddInfrastructure` e `AddApplicationServices` |
| `Directory.Packages.props` | C | A o B scrivono la riga esatta nel rapporto |
| `tests/DBEnigmistica.Tests/DBEnigmistica.Tests.csproj` | C | come sopra |
| `tests/.../Integration/**`, `Unit/Domain/**`, `Unit/Application/**` | A | - |
| `tests/.../Unit/Services/CsvExportTests.cs`, `GameExtractionServiceTests.cs`, `Unit/Cli/GameSolutionServiceTests.cs` | A | - |
| `tests/.../Unit/Cli/CliAppTests.cs`, `Unit/Cli/FornitoreAi*.cs` | B | - |
| `tests/.../Unit/Services/DataExportServiceTests.cs`, `SchemaProposalServiceTests.cs`, `Architecture/**` | C | - |
| `.github/**`, `Dockerfile`, `.dockerignore`, `.editorconfig`, `.gitattributes` | C | B indica i nomi delle variabili (già fissati nella sezione 3) |
| `.githooks/**`, `.gitleaks.toml`, `.env.template` | B | - |
| `docker-compose.yml` | nessuno | sezione 7, D5 |
| `ROADMAP.md`, `CLAUDE.md`, `README.md`, `SETUP_INSTRUCTIONS.md`, `docs/**` | nessuno (ondata 0.3) | annotare nel rapporto |
| `PROGRESS.md` | agente principale | - |
| Tutti i `*.cs` (solo spazi e fine riga) | C, passata finale | dopo la consegna di A e B |

---

## 7. Rischi, dubbi e punti per il proprietario

### Decisioni che spettano al proprietario (da porre con `grill-with-ui`)

- **D1. Cessione "come inedito".** RQ-03 dice che un Gioco "promesso o venduto come inedito" non si pubblica sul Sito. Il piano aggiunge alla Cessione il campo `come_inedito` e il motivo `PromessoComeInedito`. Alternativa: bloccare ogni Gioco con una Cessione qualsiasi, attiva o futura.
- **D2. Titolare obbligatorio per pubblicare.** Il piano blocca un Gioco `originale` o `licenza` senza titolare. Per il pubblico dominio non si chiede un titolare. Estende la logica della decisione 5.
- **D3. Esclusiva e Sito pubblico.** Il piano tratta il Sito come pubblicazione nel territorio `MONDO`: una Cessione esclusiva attiva, in qualunque paese, blocca la pubblicazione sul Sito per la sua durata.
- **D4. Comando `dbe check-ai`.** Il criterio 7 chiede un comando che usa l'AI, ma oggi tutti rispondono "disponibile dalla fase N". Il piano aggiunge un comando pubblico di sola verifica. Alternativa: anticipare la validazione dentro `scan-file`, che però darebbe un errore di configurazione invece del messaggio "disponibile dalla fase 1".
- **D5. PostgreSQL 16 nel compose.** I test usano 17. Passare il compose a 17 rende illeggibile un volume dati creato con la 16 (serve un `pg_upgrade` o un volume nuovo). Il piano non lo cambia: le funzioni usate (`IS JSON`, `btree_gist`) esistono già nella 16. Deciso il 13/11/2026: PostgreSQL 18 per test e Compose (`docs/fase-0-chiusura-decisioni.md`).

### Scelte fatte nel piano, da confermare in revisione

- **D6. Un territorio per Cessione** (`MONDO` o un paese). Più paesi = più Cessioni. Regioni come "UE" non esistono.
- **N1.** `game_solutions.SolutionPageId` passa da `SET NULL` a `NO ACTION`, per coerenza con la decisione 3.
- **N2.** Le tabelle dei diritti stanno in `catalogo`, non in `registro`. Il registro conserva la storia delle modifiche.
- **N3.** "Autenticato" per Claude Code si verifica senza rete (token o file di credenziali). Un token scaduto passa la verifica e fallisce solo alla prima chiamata, in fase 1.

### Rischi concreti

1. **Formato.** 267 errori WHITESPACE in 18 file; fine riga misti (60 CRLF, 87 LF, 1 misto) e `core.autocrlf=true`. Senza `.editorconfig` e `.gitattributes`, Windows e il runner Linux chiedono fine riga diverse. Tocca a C, con la passata finale dopo A e B.
2. **Copertura.** Domain e Application sono al 65,7% (360 su 548), serve l'80%. Stima dopo l'ondata: circa 85%, se A copre bene il nuovo dominio e C copre `DataExportService` e `SchemaProposalService`. Le entità di dettaglio mai usate pesano circa 40 righe scoperte.
3. **Avvisi.** Oggi 0 avvisi in Release: `-warnaserror` non rompe nulla. Possono emergere avvisi nullable nel codice nuovo (A e B) e, in CI, avvisi NuGet di vulnerabilità (`NU1901`-`NU1904`) che diventano errori appena esce un avviso nuovo. È voluto, ma può bloccare la CI senza modifiche al codice.
4. **Cartella condivisa.** `required Origine` su `Game` rompe la compilazione di ogni test che costruisce un `Game` finché A non ha aggiornato i punti elencati; la riga `Anthropic.SDK` in `Directory.Packages.props` non si può togliere prima che B abbia tolto il riferimento. L'ordine della sezione 5 lo gestisce.
5. **Ruoli e migrazioni.** `CREATE ROLE` richiede `CREATEROLE` all'utente che migra. Oggi è il superutente del compose; un utente di migrazione con meno permessi (fase 2) avrà bisogno di quel privilegio o di ruoli creati a parte.
6. **NetArchTest.Rules.** L'ultima versione (1.3.x) è del 2021 e punta a `netstandard2.0`. Se dà problemi con .NET 10, il ripiego è `TngTech.ArchUnitNET`; C lo segnala invece di cambiare da solo.
7. **`.planning/` è ignorato da git.** CONTEXT e PLAN non arrivano nel repository né alla revisione. Se devono restare, va tolto `.planning/` da `.gitignore` (decisione dell'agente principale o del proprietario).
