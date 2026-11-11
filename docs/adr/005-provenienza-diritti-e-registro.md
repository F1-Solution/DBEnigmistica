# ADR 005: provenienza, diritti e registro in sola aggiunta

- **Stato:** Proposto, in attesa dell'approvazione del proprietario
- **Data:** 11/11/2026

## Contesto

Ogni Gioco deve dichiarare da dove viene (Origine), chi ne detiene i diritti (Titolare), a chi è stato ceduto l'uso, e in quali casi può andare sul Sito pubblico. Il piano commerciale chiede che la prova di questi dati sia conservata e non modificabile (RQ-01 a RQ-05, RQ-12). Il modello serve dal primo salvataggio, perché i Giochi estratti e quelli originali ne hanno bisogno fin dall'inizio.

Il proprietario ha preso decisioni vincolanti l'11/11/2026 (`docs/ondata-0-2-decisioni.md`, `docs/ondata-0-2-decisioni-2.md`, `.planning/phases/0/CONTEXT.md`). Le principali: un'anagrafica unica dei Soggetti; due stati senza ritorno per l'inedito; prove dell'Origine obbligatorie alla pubblicazione, non al salvataggio; una Cessione blocca il Sito solo se dichiara il Gioco come inedito o se è esclusiva; l'esclusiva blocca il Sito qualunque sia il territorio; il Titolare mancante non blocca la pubblicazione.

## Decisione

1. **Origine obbligatoria**, senza valore predefinito. I valori sono `originale`, `licenza`, `pubblico_dominio` e `rivista`. Un Gioco di licenza o di pubblico dominio si salva anche senza prove complete; la policy lo blocca finché le prove mancano.
2. **Soggetto**: un'anagrafica unica, persona o organizzazione, usata come Titolare, licenziante o cliente di una Cessione. Nessun testo libero su ogni Gioco.
3. **Licenza in entrata** e **Cessione**: la Cessione ha cliente, periodo (inizio incluso, fine esclusa), territorio (`MONDO` oppure un codice ISO 3166-1 alfa-2), flag di esclusiva e flag "come inedito". Il database impedisce due Cessioni esclusive sovrapposte sullo stesso Gioco, con un vincolo di esclusione su periodo e territorio.
4. **Stato di inedito a due stati senza ritorno**: un Gioco è inedito finché non ha una prima pubblicazione, di cui si salvano data e luogo. Un trigger impedisce di tornare indietro.
5. **Policy di pubblicazione** come funzione pura del dominio, senza accessi al database o all'orologio. Raccoglie tutti i motivi di blocco, non solo il primo. Un Gioco con origine `rivista` non è mai pubblicabile. Un Gioco ceduto come inedito non è pubblicabile finché la Cessione è in corso. Un'esclusiva in corso blocca la pubblicazione sul Sito qualunque sia il suo territorio, perché il Sito vale come pubblicazione su `MONDO`.
6. **Registro in sola aggiunta** (`registro.eventi`): un trigger rifiuta `UPDATE`, `DELETE` e `TRUNCATE`. Ogni riga contiene lo SHA-256 della precedente, calcolato su un contenuto codificato a lunghezza prefissata. L'aggiunta avviene con un lucchetto di transazione sull'ultima riga, più un vincolo di unicità sull'impronta precedente, che impedisce una biforcazione della catena anche se il lucchetto viene saltato.
7. **Verifica** della catena con `dbe verify-integrity` (ondata 0.3): codice d'uscita 0 se integra, 1 se alterata, con l'id del primo evento non valido e il motivo.

## Conseguenze

- Un Gioco salvato senza prove complete va tenuto visibile in un elenco di lavoro, altrimenti resterebbe invisibile fino alla pubblicazione.
- Un'indicazione "come inedito" dimenticata in una Cessione lascia il Gioco pubblicabile. È un costo accettato dal proprietario.
- Un'esclusiva anche solo all'estero blocca il Sito. L'interpretazione dell'esclusiva per paese va confermata dal legale.
- Il Titolare mancante è trattato come proprietario: un Titolare sottinteso è più debole in una contestazione, ma è stato scelto così dal proprietario (`docs/ondata-0-2-decisioni-2.md`, q3).
- **Limite noto**: la catena non rivela la cancellazione delle ultime righe fatta da un superutente. Trigger e permessi la impediscono, ma non la rilevano se avviene. Un'ancora esterna (per esempio l'impronta di testa nel backup firmato) va valutata in fase 6, insieme a RQ-12.
- L'impronta è legata al formato v1 del contenuto. Un cambio di formato richiede un nuovo valore di versione e non deve riscrivere le righe esistenti.

## Alternative scartate

- **Testo libero per Titolare e cliente**: lo stesso cliente scritto in due modi diventa due clienti, e le ricerche su cosa è stato ceduto a chi non sono affidabili.
- **Tre stati per l'inedito** (inedito, pubblicato in proprio, pubblicato da terzi): il caso in più si ricava dal luogo della prima pubblicazione.
- **Stato di inedito calcolato ogni volta** dagli eventi e dalle Cessioni: la policy dipenderebbe da dati di altri contesti, contro la regola dei contesti delimitati.
- **Prove dell'Origine obbligatorie al salvataggio**: scartate dal proprietario. Il Gioco si salva incompleto e la policy lo blocca.
- **Titolare obbligatorio o bloccante**: la raccomandazione iniziale del piano era di bloccare la pubblicazione senza Titolare. Il proprietario ha scelto diversamente (`docs/ondata-0-2-decisioni-2.md`, q3).
- **Cancellare la pagina scollegando in silenzio le soluzioni**: scartato; la cancellazione viene rifiutata (q4).

## Riferimenti

- `.planning/phases/0/PLAN.md`, sezioni 1.2 (tipi del dominio), 1.3 (policy di pubblicazione), 1.5 (impronta concatenata), 1.8 (date in UTC), 1.9 (pagina-soluzione in uso), 1.11 (`verify-integrity`).
- `docs/ondata-0-2-decisioni.md` e `docs/ondata-0-2-decisioni-2.md`.
- `ROADMAP.md`, sezione 3, fase 0 (modello di provenienza e diritti; criteri 9, 10, 13).
- Requisiti: RQ-01, RQ-02, RQ-03, RQ-04, RQ-05, RQ-12 (piano commerciale).
