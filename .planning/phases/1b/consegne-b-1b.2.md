# Consegne dell'agente B all'agente A (ondata 1b.2)

Compilato e coperto da test il 23/11/2026. Namespace: `DBEnigmistica.Application.Creazione`. I tipi sono registrati da `AddCreazione()` (già chiamato dall'host): `RevisioneCandidati` e `TipologieDelPrototipo` sono `Scoped`.

## R4. Salvataggio del tempo

```csharp
Task<Candidato> RevisioneCandidati.SalvaTempoAsync(
    Guid id, long tempoAttivoMs, DateTime ultimaAttivitaIl, CancellationToken cancellationToken = default)
```

Rotta suggerita: `PUT /api/prototipo/candidati/{id:guid}/tempo`, corpo `{ "tempoAttivoMs": <numero>, "ultimaAttivitaIl": "<ISO-8601 UTC>" }`. Risposta 200 con `CandidatoJson.Candidato(w, c, controlli)` come `PUT .../finale`.

Comportamento:
- Salva `TempoAttivoMs` e `UltimaAttivitaIl` (convertito in UTC). Non scrive nulla nel registro e non cambia lo stato.
- Tempo uguale a quello salvato: accettato (invio periodico senza novità), aggiorna solo l'ultima attività.
- Tempo minore di quello salvato: `RichiestaNonValidaException` "Il tempo attivo non può diminuire." (la rotta lo mappa già a 422 con `Esegui`).
- Tempo negativo: `RichiestaNonValidaException` (422), prima di ogni accesso al database.
- Candidato inesistente: `NonTrovatoCreazioneException` (404).
- Candidato che non è il prossimo da rivedere, oppure già concluso con *Accetta* o *Scarta*: `ConflittoCreazioneException` (409). Quindi dopo l'esito il client non deve più inviare nulla.
- Se il candidato era `da_rivedere` il primo salvataggio lo porta a `in_revisione` (come ogni altra azione sul prossimo).
- La rotta deve leggere `tempoAttivoMs` (numero intero) e `ultimaAttivitaIl` (stringa) come `LeggiEsito`; se mancano o non sono del tipo giusto: `RichiestaNonValidaException`.

## R5. Elenco delle Tipologie

```csharp
Task<IReadOnlyList<TipologiaPrototipo>> TipologieDelPrototipo.EseguiAsync(CancellationToken cancellationToken = default)

record TipologiaPrototipo(string Id, int Versione, string Famiglia, string Nome, string Modo,
                          string StatoRegole, bool RegoleApprovate, bool GenerazioneConsentita,
                          IReadOnlyList<CampoBrief> CampiBrief)
record CampoBrief(string Nome, string Etichetta, string Tipo, bool Obbligatorio,
                  IReadOnlyList<string> Valori, int? Minimo = null, int? Massimo = null)
```

Rotta suggerita: `GET /api/prototipo/tipologie`. Le Tipologie sono sette, in ordine di id, ciascuna nell'ultima versione disponibile: `anagramma` (v1), `crittografia-breve` (v0), `cruciverba-classico` (v1), `futoshiki`, `nonogramma`, `rebus`, `sudoku` (v0).

- `Modo`: `ai` (anagramma, crittografia-breve, cruciverba-classico, rebus) oppure `algoritmo` (sudoku, futoshiki, nonogramma).
- `StatoRegole`: `bozza` o `approvata`; `RegoleApprovate` è lo stesso dato come booleano. Oggi tutte in bozza.
- `GenerazioneConsentita` (JSON `generazioneConsentita`) decide se la schermata abilita *Genera*; sostituisce il controllo su `regoleApprovate`. Vale `true` per i giochi con algoritmo, per le Tipologie con regole approvate e per quelle con regole in bozza quando le Registrazioni sono in `riproduci`. Vale `false` per le Tipologie con AI quando l'interruttore dal vivo è spento (anche con regole approvate) e quando si è dal vivo con regole in bozza. Con `false` l'API rifiuterebbe comunque (422).
- `TipologieDelPrototipo` ora ha un secondo parametro facoltativo, `IFornitoreAiCreazione`, che il contenitore fornisce da solo.
- `CampiBrief` nell'ordine in cui compaiono nel modulo. `Tipo`:

| Tipo | Significato | Campi usati |
|---|---|---|
| `testo` | riga di testo | `Massimo` = lunghezza massima (200 per `tema`) |
| `elenco-testi` | 0-8 testi, ciascuno 1-300 caratteri | `Massimo` = 8 (`vincoli`) |
| `intero` | numero | `Minimo`, `Massimo` (`difficolta` 1-5) |
| `scelta` | una voce fra `Valori` | `sottotipo`: `sinonimica`, `perifrastica` |
| `dimensioni-quadrate` | un solo lato: righe = colonne | `Valori` = lati ammessi come testo (`"9"`, `"5"`...) |
| `dimensioni` | righe e colonne indipendenti | `Minimo`, `Massimo` (nonogramma 10-15) |

- Campi per famiglia: `tema` (obbligatorio solo per `gioco-breve`) e `vincoli` assenti per i giochi logici; `sottotipo` (obbligatorio) solo per la crittografia; `dimensioni` solo per cruciverba (quadrate 9-13), sudoku (9), futoshiki (5-7) e nonogramma (10-15); `difficolta` sempre, obbligatoria.
- Il brief da inviare a `POST /candidati` resta quello dello schema `brief.schema.json` (`versione_formato` 1, `tipologia`, `versione_tipologia`, `famiglia`, `tema`, `vincoli`, `difficolta`, `dimensioni` come `{righe, colonne}`, `sottotipo`; i campi non ammessi vanno a `null`, `vincoli` a `[]`). `Versione` e `Famiglia` dell'elenco servono per `versione_tipologia` e `famiglia`.

## Altre modifiche che toccano le rotte di A

- `ICatalogoTipologie` ha ora `ElencaAsync`; `CatalogoInMemoria` dei test è già aggiornato.
- `RevisioneCandidati.ControlliAsync` aggiunge ai controlli esistenti i problemi dei validatori della 1.2, nel formato `<campo>: <messaggio>`. Sono solo informativi: *Accetta* non li considera. Nessuna modifica alle rotte.
- Tipologia `rebus`: il validatore elencato nel descrittore v0 segnala sempre un controllo sul diagramma (vedi il rapporto di B); la schermata lo mostra come un altro controllo.
