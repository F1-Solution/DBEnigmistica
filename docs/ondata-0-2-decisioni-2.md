# Decisioni per l'ondata 0.2, secondo giro (pubblicazione, esclusive e Titolare)

Secondo giro dell'intervista dell'11 novembre 2026, condotto con la pagina grill-with-ui. Nasce dai punti D1-D4 di `.planning\phases\0\PLAN.md`, sezione 7, emersi dal progetto del modello di provenienza e diritti. Il primo giro, dello stesso giorno, è in `docs/ondata-0-2-decisioni.md`: i termini usati qui (Origine, Titolare, Cessione, Registro, Soggetto) hanno lo stesso significato.

## Perché

Il progetto del modello di provenienza e diritti ha fatto emergere quattro scelte che spettano al proprietario prima di scrivere il codice. Due riguardano quando un Gioco può andare sul Sito pubblico, una il caso del Titolare mancante, una un comando di verifica dell'AI. Le prime due sono vincolanti per la policy di pubblicazione e per le Cessioni.

## Decisioni vincolanti

### Quando una Cessione blocca il Sito pubblico (q1)

**Decisione: un Gioco ceduto si può pubblicare sul Sito pubblico. Solo una Cessione che dichiara il Gioco come inedito lo blocca. L'indicazione è obbligatoria in ogni Cessione.**

Il piano commerciale (RQ-03) dice che un Gioco promesso o venduto come inedito non va sul Sito pubblico. La decisione applica questa regola solo ai casi in cui la promessa c'è.

Opzioni scartate:
- *Qualsiasi Cessione, attiva o futura, blocca il Sito pubblico*: perde. Impedisce di riusare sul Sito pubblico Giochi ceduti senza promesse di inedito, ad esempio in uso non esclusivo.

Costo accettato: un'indicazione in più da compilare a ogni Cessione. Se la si dimentica, il Gioco resta pubblicabile.

### Esclusiva su un solo paese (q2)

**Decisione: il Sito pubblico vale come pubblicazione nel mondo intero. Una Cessione con esclusiva attiva blocca il Sito pubblico per la durata dell'esclusiva, qualunque sia il territorio.**

Il Sito è raggiungibile da ovunque. Un Sito in italiano si legge anche dal paese dell'esclusiva, e un cliente potrebbe contestarlo.

Opzioni scartate:
- *Blocca solo un'esclusiva sul mondo intero o sull'Italia*: perde. Recupera i Giochi ceduti per un solo paese estero, ma si regge su un'interpretazione che va confermata dal legale.

Costo accettato: un Gioco ceduto in esclusiva anche solo all'estero resta fuori dal Sito pubblico.

## Scelte ordinarie

- **Titolare mancante (q3)**: il piano prevedeva che la policy blocchi la pubblicazione finché il Titolare manca. Il proprietario ha scelto l'opzione B, diversa dalla raccomandazione A: la decisione prevale sul piano. Quindi la policy di pubblicazione non blocca un Gioco originale o su licenza senza Titolare, e il Titolare mancante è il proprietario. La raccomandazione A puntava al Registro, che deve poter dimostrare chi detiene i diritti; un Titolare sottinteso è un dato non scritto.
- **Comando di verifica dell'AI (q4)**: si aggiunge il comando `dbe check-ai`, di sola verifica. Serve a provare il criterio di accettazione 7: dice subito se Claude Code è trovato e autenticato, senza chiamate di rete, prima di un'estrazione lunga. Scartate: far confluire la verifica in `scan-file`, che confonderebbe il messaggio di un comando non ancora disponibile; rimandare il criterio alla fase 1, che lascerebbe la fase 0 con un criterio non soddisfatto.

## Fatti verificati

- Oggi nessun comando usa l'AI. Tutti rispondono "disponibile dalla fase N".
- Il criterio di accettazione 7 chiede di provare che un comando che usa l'AI si ferma con un messaggio chiaro se il Fornitore AI non è configurato.
- Il piano commerciale (RQ-03) dice che un Gioco promesso o venduto come inedito non va sul Sito pubblico.
- Il piano prevede un territorio per ogni Cessione: il mondo intero oppure un solo paese.
- La policy chiede i dati di Titolare e prove alla pubblicazione, non al salvataggio. Per il pubblico dominio il Titolare non esiste e non viene chiesto.

## Rischi

- **Indicazione "come inedito" dimenticata**: il Gioco resta pubblicabile anche se la Cessione promette l'inedito (q1).
- **Esclusiva all'estero**: Giochi ceduti in esclusiva anche solo all'estero restano fuori dal Sito pubblico (q2).
- **Titolare sottinteso**: un Titolare non scritto è debole in una contestazione (q3).
- **Comando in più**: `dbe check-ai` è un comando in più da mantenere (q4).

## Rimandati

Nessuna domanda è stata rimandata.

## Discussioni aperte

- **Interpretazione dell'esclusiva per paese**: conviene farla confermare al legale. La decisione q2 la applica in modo prudente, ma la lettura non è ancora verificata.
- **Versione di PostgreSQL**: `docker-compose.yml` usa PostgreSQL 16, mentre i test della fase 0 usano 17. Il punto è D5 del piano, non deciso qui.
