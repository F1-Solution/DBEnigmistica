# Decisioni per l'ondata 0.2 (provenienza, diritti e giochi logici)

Intervista dell'11 novembre 2026, condotta con la pagina grill-with-ui. Serve ad avviare l'ondata 0.2 della fase 0 (Fondamenta) di `ROADMAP.md`. Raccoglie le sei risposte del proprietario su tre temi: il modello di provenienza e diritti dei Giochi, la conferma dei giochi logici della Prova 1 e il modo di preparare la fase (documenti di pianificazione, cancellazione delle pagine-soluzione). Due risposte sono vincolanti per le tabelle della baseline; le altre quattro sono scelte ordinarie.

## Termini

- **Origine**: da dove viene un Gioco: originale, licenza, pubblico dominio, rivista. Obbligatoria, senza valore predefinito. Da evitare: fonte, provenienza generica.
- **Titolare**: chi detiene i diritti su un Gioco. Da evitare: autore, proprietario del Gioco.
- **Cessione**: il trasferimento a un cliente del diritto d'uso di un Gioco, con periodo, esclusiva e territorio. Da evitare: vendita, licenza in uscita.
- **Registro**: l'elenco in sola aggiunta degli eventi su un Gioco, compreso il contributo umano. Da evitare: log, storico.
- **Soggetto**: una persona o un'organizzazione dell'anagrafica unica, usata come Titolare, licenziante o cliente di una Cessione. Da evitare: contatto, utente, anagrafica cliente.

## Perché

Il proprietario ha chiesto di avviare l'ondata 0.2 con l'obiettivo "ondata 0.2 unita in main con CI verde". Ha chiesto anche di ricevere le domande che servono per due punti: il modello di provenienza e diritti dei Giochi, e la conferma dei giochi logici della Prova 1. Il modello di provenienza e diritti fissa la forma delle tabelle della baseline, che poi è costoso cambiare. La conferma dei giochi logici sblocca l'ondata 1b.1.

## Decisioni vincolanti

### Chi è il Titolare e chi è il cliente (q2)

**Decisione: un'anagrafica unica dei Soggetti, persona o organizzazione, usata per Titolare, licenziante e cliente di una Cessione.**

Opzioni scartate:
- *Testo libero su ogni Gioco e su ogni Cessione*: perde. Lo stesso cliente scritto in due modi diventa due clienti, e le ricerche su cosa è stato ceduto a chi non sono affidabili.
- *Titolare sempre il proprietario, sottinteso; anagrafica solo per i clienti*: perde. Non regge i Giochi su licenza e di pubblico dominio, che il modello prevede.

Costo accettato: una tabella in più e un passaggio in più quando si inserisce un Soggetto nuovo.

### Lo stato di inedito (q5)

**Decisione: due stati, senza ritorno. Un Gioco è inedito finché non ha una prima pubblicazione; di quella prima pubblicazione si salvano data e luogo. Il passaggio da inedito a pubblicato si protegge con un vincolo.** Lo stato entra nella policy di pubblicazione e nelle Cessioni. Dipende da q2.

Opzioni scartate:
- *Tre stati (inedito, pubblicato in proprio sul Sito, pubblicato da terzi)*: perde. Il caso in più si ricava già dal luogo della prima pubblicazione.
- *Nessun campo, calcolato ogni volta dagli eventi di pubblicazione e dalle Cessioni*: perde. La policy dipenderebbe da dati di altri contesti, contro la regola dei contesti delimitati.

## Scelte ordinarie

- **Giochi logici della Prova 1 (q1)**: futoshiki e nonogramma, come proposto in `ROADMAP.md`. Il nonogramma mette alla prova il modello di resa griglia_logica prima che ci si costruisca sopra; il futoshiki riusa quasi tutto il sudoku. Chiude la domanda aperta 12.
- **Documenti di pianificazione (q3)**: un agente Opus scrive `CONTEXT.md` e `PLAN.md` della fase 0 da `ROADMAP.md` e da queste risposte, prima che parta l'ondata. Il progetto del modello dei dati va comunque scritto prima dell'implementazione, e `PLAN.md` è il posto giusto per farlo.
- **Pagina-soluzione usata da un altro Gioco (q4)**: la cancellazione viene rifiutata. Prima si scollega la soluzione a mano, poi si cancella la pagina. Una soluzione collegata è lavoro di revisione, e perderla in silenzio è un danno.
- **Prove dell'Origine (q6)**: le prove per licenza (licenziante, estremi dell'accordo, periodo) e per pubblico dominio (autore, anno, motivo) sono obbligatorie alla pubblicazione, non al salvataggio. Il Gioco si può salvare anche incompleto, ma la policy lo blocca finché le prove mancano. L'Origine in sé resta obbligatoria al salvataggio.

## Fatti verificati

- La cartella `.planning` non esiste nel repository. L'ondata 0.1 è stata eseguita senza `CONTEXT.md` e `PLAN.md`.
- Il database rifiuta oggi la cancellazione di una pagina usata come pagina-soluzione da un Gioco di un'altra Rivista.
- La domanda aperta 12 di `ROADMAP.md` (riga 962: "Confermi i giochi logici della prova 1?", scadenza 7 dicembre 2026, blocca l'ondata 1b.1) è chiusa da q1.
- `docs/ondata-0-2-decisioni.md` non esisteva prima di questa intervista.

## Rischi

- **Secondo risolutore per il nonogramma**: il nonogramma richiede un risolutore non banale, oltre a quello del sudoku. È il costo della scelta di q1 rispetto al solo futoshiki.
- **Giochi in archivio con prove dell'Origine incomplete**: per le scelte di q6 esisteranno Giochi salvati senza prove complete. Vanno tenuti visibili in un elenco, altrimenti restano invisibili fino alla pubblicazione.
- **Passaggio manuale in più**: per cancellare una Rivista le cui pagine sono usate come soluzione serve un passaggio manuale in più (q4). Il messaggio di rifiuto deve dire quali Giochi usano la pagina.

## Rimandati

Nessuna domanda è stata rimandata.

## Discussioni aperte

- **Rappresentazione del territorio di una Cessione**: dettaglio tecnico lasciato al progetto dell'ondata. Va deciso in `.planning\phases\0\PLAN.md`.
- **Forma dell'impronta del Registro**: dettaglio tecnico lasciato al progetto dell'ondata. Va deciso in `.planning\phases\0\PLAN.md`.
- **Aggiornamento di `ROADMAP.md`**: chiudere la domanda aperta 12 con la scelta di q1 (futoshiki e nonogramma).
