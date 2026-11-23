# Decisioni dell'ondata 1.2: nome delle Tabelle per Tipologia (D7) e interruttore delle chiamate dal vivo (D8)

Decisioni del proprietario del 23 novembre 2026, prese con l'intervista `grill-with-ui`. In entrambi i casi il proprietario ha accettato la raccomandazione A. D7 cambia il nome delle Tabelle per Tipologia prima che la v1 entri in `main`. D8 aggiunge un interruttore che blocca le chiamate dal vivo finché il proprietario non dà il via. Le due decisioni non cambiano il piano di fase 1 oltre a questo. La numerazione D7 e D8 vale solo per questo documento: la D7 di `docs/fase-1-decisioni.md` è un'altra decisione.

## Termini

- **Tabella per Tipologia**: la tabella generata come SQL rivisto dal descrittore e applicata solo con `dbe migrate`. Da evitare: «tabella dinamica» e «DDL a runtime».
- **Versione del descrittore**: il numero intero di una Tipologia, v1, v2 e così via. Da evitare: «revisione» e «release».

## Perché

Oggi il nome della Tabella per Tipologia è `gioco_<id_con_underscore>`, senza versione. Una v2 della stessa Tipologia genererebbe la stessa `CREATE TABLE`. Allora `dbe migrate` fallirebbe con l'errore 42P07, e non esiste un percorso basato su `ALTER`. Dopo l'unione in `main` l'SQL della v1 non si può cambiare, perché il suo checksum è registrato.

## Decisioni bloccate

### D7. Nome versionato delle Tabelle per Tipologia

**Decisione.** Il nome include la versione: `gioco_<id_con_underscore>_v<n>`. Esempi: `gioco_cruciverba_classico_v1` e `gioco_cruciverba_classico_v1__definizioni`, sia in `riviste` sia in `catalogo`. Una v2 è una tabella nuova accanto alla v1. I dati passano dalla v1 alla v2 con una migrazione esplicita e rivista. Il Gioco sa già quale tabella leggere, grazie a `versione_tipologia`. Il limite di 63 byte include il suffisso `_v<n>`. La guardia provvisoria che rifiuta due versioni della stessa Tipologia si rimuove.

**Opzioni scartate.**
- *B: nome senza versione, con la v2 come SQL di `ALTER` generato e rivisto.* Serve un generatore di differenze. È un lavoro grande e rischioso per una sola v2 prevista.
- *C: nome senza versione e decisione rimandata.* Il problema arriverebbe quando non si può più rimediare senza toccare checksum già registrati.

## Scelte di routine

### D8. Interruttore per le chiamate dal vivo

**Decisione.** La chiave `FornitoreAi:DalVivoAbilitato` vale `false` per impostazione predefinita. Con `false`, `dbe eval extraction --live` e la generazione dal vivo di `dbe prototipo` si rifiutano di partire. Il messaggio rimanda alla prova `1.1-prova`. `--replay` e `check-ai` non cambiano. Il ramo va realizzato in questa ondata.

**Opzioni scartate.**
- *B: farlo nella 1.3.* Il ramo resterebbe in `main` senza protezione per settimane.
- *C: nessuna guardia.* Tutto dipenderebbe dalla disciplina di chi lancia i comandi, agenti compresi.

## Fatti verificati

- `.planning/phases/1/PLAN.md`, riga 48: le Tabelle per Tipologia sono `riviste.gioco_<id_con_underscore>` e `catalogo.gioco_<...>`. Il testo attuale non ha la versione: si aggiorna con questa decisione.
- `.planning/phases/1/progetto-1.1.md`, riga 129: il nome `<base>` è `gioco_` più `id`, senza versione. Riga 143: la tabella dei ritagli arriverà con una versione nuova del descrittore.
- `ROADMAP.md`, riga 144: cita la forma senza versione `<schema>.gioco_<tipologia>`.
- `.planning/phases/1/preparazione-1.2.md`, sezione 7, D2 (21/11/2026): «le `v0` non si applicano mai».
- Nessuna Tabella per Tipologia è mai stata applicata in `main`. Il codice di `dbe migrate` (`MigratoreTipologie` e la tabella `tipologie.migrazioni_applicate`) non esiste in `main`. È nei commit della 1.2.
- `docs/adr/002-descrittore-di-tipologia-e-schema-dinamico.md` non cita il nome delle tabelle con la forma `gioco_<...>`.

## Rischi

- **Migrazione dei dati v1 → v2 scritta a mano.** Ogni v2 richiede una migrazione dei dati rivista. Un errore qui sposta dati sbagliati. Contromisura: revisione prima dell'applicazione, con `dbe migrate` e un comando esplicito.
- **Nomi più lunghi.** Il suffisso `_v<n>` consuma byte del limite di 63. Un id di Tipologia lungo cade prima nella regola dell'hash. Contromisura: test di generazione sui nomi più lunghi del catalogo.

## Rimandato

- Formato della migrazione dei dati v1 → v2, da definire alla prima v2.
- Destino della v1 dopo la migrazione: conservarla o eliminarla. Si decide alla prima v2.

## Discussioni aperte

Nessuna.
