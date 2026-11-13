# ADR 001: superfici, profili e separazione dei dati

- **Stato:** Accettato il 13/11/2026
- **Data:** 11/11/2026

## Contesto

DBEnigmistica ha tre superfici: lo **Studio**, privato e sui dispositivi del proprietario; il **Sito pubblico**, per i giocatori su internet; l'**Admin**, che gestisce il Sito pubblico. Lo Studio contiene Riviste in PDF e Giochi estratti da editori terzi, che non possono mai finire online. Il Sito deve poter essere esposto senza che un errore di configurazione porti con sé l'archivio privato.

Il proprietario ha deciso che lo Studio gira sul PC Windows di casa, con Docker Desktop, raggiungibile solo dalla rete di casa (`docs/fase-0-decisioni.md`, Q2). L'Admin opera sul database pubblico, non su quello dello Studio.

## Decisione

1. **Un solo codice, due profili di esecuzione.** Il profilo `studio` offre Studio e archivio delle Riviste; il profilo `pubblico` offre Sito e Admin. La scelta avviene da configurazione (`DBE_PROFILO=studio|pubblico`, prevista dalla fase 2). Il profilo pubblico non ha la stringa di connessione dello Studio: se la trova, si ferma all'avvio con un errore.
2. **Quattro schemi nello stesso database dello Studio**, con dati separati per contesto: `riviste` (archivio privato, estrazione), `catalogo` (Giochi originali e diritti), `tipologie` (descrittori e migrazioni confermate), `registro` (provenienza, diritti, contributo umano, in sola aggiunta). Il database pubblico è separato e contiene solo edizioni approvate.
3. **Ruoli PostgreSQL per contesto**, di tipo `NOLOGIN`, con permessi minimi: `dbe_estrazione` scrive in `riviste`; `dbe_creazione` scrive in `catalogo` e non ha alcun accesso a `riviste`; `dbe_originalita` legge `riviste` e `catalogo` e restituisce solo punteggi; `dbe_consegna` legge `catalogo`. Gli utenti di accesso arrivano nella fase 2.
4. **Blocco su tre livelli** per i Giochi estratti: la policy di dominio (un Gioco con origine `rivista` non è mai pubblicabile), i vincoli del database (controllo `ck_games_origine_rivista` sulla tabella dei Giochi estratti), e il database pubblico separato, che non riceve mai i Giochi estratti.
5. **Contesti delimitati**: *Acquisizione* (Riviste, Pagine, Giochi estratti), *Catalogo e creazione* (Giochi originali, brief, revisioni), *Tipologie* (descrittori e versioni), *Registro* (provenienza, diritti, contributo umano), *Pubblicazione* (edizioni, calendario, uso).
6. **Confini tra contesti**: i contesti comunicano solo tramite i casi d'uso dell'Application layer, non leggendo le tabelle di un altro contesto.
7. **Dipendenze verso l'interno**: Domain non dipende da nulla; Application dipende solo da Domain; gli adattatori (EF Core e Npgsql, Fornitore AI, PDF, file, web, CLI) dipendono verso l'interno. I test di architettura verificano la regola dalla fase 0.

## Conseguenze

- Il permesso di usare i Giochi estratti come esempi nella creazione (fase 6) è un `GRANT` sul database, non solo una riga di configurazione. Oggi non esiste.
- Lo Studio non è esposto su internet. Un accesso da fuori casa richiederebbe una decisione nuova, con un ADR dedicato.
- Lo schema `riviste` viene scritto da un solo ruolo. Ogni nuovo accesso allo schema richiede una modifica alle migrazioni, quindi è visibile nella revisione.
- Oggi esiste un solo profilo: `DBE_PROFILO` non è ancora nel codice. La separazione per profilo arriva con la fase 2.

## Alternative scartate

- **Studio esposto su internet da un server a noleggio**: porterebbe le Riviste su macchine di terzi, scelta scartata dal proprietario in attesa del parere legale (`docs/fase-0-decisioni.md`, Q2).

## Riferimenti

- `ROADMAP.md`, sezione 1 (CI e pubblicazione) e sezione 2.1 (superfici, profili e dati).
- `CLAUDE.md`, sezione 5 (vincoli di prodotto): blocco su tre livelli, profilo pubblico senza stringa dello Studio.
- `.planning/phases/0/PLAN.md`, sezione 1.6 (ruoli PostgreSQL).
- `docs/fase-0-decisioni.md`, Q2 (dove gira lo Studio).
- Requisiti: RQ-05 (separazione dei dati), OBJ-05, DB-01, DB-02, DB-03 (fase 0 del roadmap).
