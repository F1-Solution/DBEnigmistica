# Passi per il proprietario

Questa cartella contiene un file per ogni operazione che il proprietario deve fare nel progetto DBEnigmistica, adesso e nelle fasi future. Lo stato del progetto è in `ROADMAP.md`. Se questa cartella e la ROADMAP non coincidono, vince la ROADMAP.

## Come si legge un file

Ogni file descrive una sola operazione. Ha queste parti:

- **Scopo**: a che cosa serve l'operazione.
- **Scadenza** e **Tempo stimato**: quando va fatta, e quanto tempo richiede.
- **Prerequisiti** e **Cosa ti serve**: che cosa deve essere pronto prima.
- **Procedura**: i passi numerati. Ogni passo ha una sola azione. Dopo ogni passo c'è la riga **Risultato**, che dice che cosa devi vedere.
- **Verifica finale**: la prova che l'operazione è riuscita.
- **Cosa consegnare all'agente principale**: che cosa scrivere nel messaggio di ritorno.
- **Cosa sblocca**: quali altre operazioni o fasi dipendono da questa.

Segni usati nei file:

- **AVVERTENZA:** rischio di danno, di perdita di dati o di fuga di un segreto. Leggi prima del passo indicato.
- **ATTENZIONE:** rischio per il risultato dell'operazione.
- **NOTA:** informazione utile.

Gli stati possibili sono:

- **Da fare:** non iniziata.
- **In attesa:** dipende da un'altra operazione o da un lavoro dell'agente. Il motivo è scritto nel file.
- **Fatto:** completata e confermata.
- **Ricorrente:** si ripete a ogni ondata o fase.

Le parti marcate "Da dettagliare prima della fase N" hanno passi generali. I dettagli arrivano dall'agente prima di quella fase.

## Come segnare un'operazione come fatta

1. Nel file, cambia la riga **Stato** in `Fatto`. Aggiungi la data.
2. Nella tabella di questo README, cambia lo stato della stessa riga.
3. Invia il messaggio di consegna all'agente principale, come descritto nel file. L'agente controlla il risultato e registra la data.

## Regole valide per tutti i file

- Non scrivere in chat, in file o nel repository il valore di una password, di un token o di una chiave. Scrivi solo il nome della variabile.
- Non aprire, stampare o copiare i file `.env` e `src\DBEnigmistica.Console\appsettings.Development.json`. Modificare `.env` con un editor è un tuo passo, come indicato nel file 00.
- I PDF delle Riviste, il Set di riferimento e le Registrazioni non entrano mai nel repository, né su GitHub, né in una release. Restano sul PC.
- Una decisione che spetta a te si pone con la skill `grill-with-ui`. L'agente principale la avvia.
- Se un comando è indicato come "disponibile dalla fase N", non esiste ancora. Chiedi all'agente quando usarlo.

## Indice (ordinato per scadenza)

| ID | Titolo | Fase | Scadenza | Tempo stimato | Stato | Blocca |
|---|---|---|---|---|---|---|
| 01 | [Verifica delle condizioni d'uso dell'abbonamento](01-condizioni-uso-abbonamento.md) | 1 | 6 dic 2026 | circa 2 h (con i piani gratuiti) | Da fare | Prova 03, decisione sul container (15), piani gratuiti (09) |
| 02 | [Set di PDF di prova](02-set-pdf-di-prova.md) | 1 | 7 dic 2026 | incluso nelle circa 20 h della fase 1 | Da fare | Risultati attesi (08), Set di riferimento |
| 03 | [Login e prova reale di Claude Code](03-prova-claude-code.md) | 1 | 6 dic 2026 (dopo 01) | non stimato nella ROADMAP | In attesa (01) | Estrazione dal vivo; riga `1.1-prova` in `PROGRESS.md` |
| 04 | [Domande al legale](04-domande-legale.md) | 0 | Invio 15 dic 2026; risposta 28 feb 2027 | non stimato separatamente | Da fare | Interruttore degli esempi (19), immagini dei rebus (20), lancio (21) |
| 05 | [Regole delle 4 Tipologie della fase 1](05-regole-tipologie-fase-1.md) | 1 | 20 dic 2026 | circa 1 h | In attesa (ondata 1.2) | Ondata 1b.2 |
| 06 | [Regole e brief del prototipo](06-regole-e-brief-fase-1b.md) | 1b | 3 gen 2027 (brief); prototipo pronto verso il 10 gen | circa 4 h (regole 2,5 h, brief 1,5 h) | In attesa (ondata 1b.1) | Collaudo (07), prova 1 (10) |
| 07 | [Collaudo del prototipo](07-collaudo-prototipo.md) | 1b | 17 gen 2027 | circa 1,5 h | In attesa (ondata 1b.2) | Prova 1 (10) |
| 24 | [Approvare una PR, unirla e creare un tag](24-approvare-pr-e-tag.md) | tutte | Pre-release del prototipo prima del 18 gen 2027; poi fine di ogni ondata | circa 15 min per PR | Ricorrente | Pre-release, release di ogni fase |
| 10 | [Prova 1: revisione a cronometro](10-prova-1-revisione-cronometro.md) | 1b | 18 gen - 21 feb 2027 | circa 14 h | In attesa (06, 07) | Decisione G1 (11) |
| 08 | [Correzione dei risultati attesi](08-risultati-attesi-set-di-riferimento.md) | 1 | 25 gen 2027 | circa 8-10 h | Da fare | Ondata 1.4, scelta del fornitore (12) |
| 09 | [Modello aperto locale e piani gratuiti](09-modello-locale-e-piani-gratuiti.md) | 1 | 25 gen 2027 | circa 1 h (il modello) | Da fare | Ondata 1.4 |
| 12 | [Scelta del Fornitore AI e soglie](12-scelta-fornitore-e-soglie.md) | 1 | 21 feb 2027 | non stimato separatamente | In attesa (ondata 1.4) | Chiusura della fase 1 |
| 13 | [Regressione locale e allegato alla release](13-regressione-locale-e-release.md) | 1 | Prima di ogni release; `v0.2.0` entro 21 feb 2027 | non stimato separatamente | Da fare | Release `v0.2.0` |
| 14 | [Fase 2: certificato, passkey e collaudo](14-fase-2-certificato-e-passkey.md) | 2 | 21 feb 2027 | circa 6 h (fase 2) | Da fare | Fase 3 |
| 15 | [Fase 2: decisione sul container](15-fase-2-decisione-container.md) | 2 | 21 feb 2027 | incluso nelle 6 h della fase 2 | Da fare (dipende da 01, domanda d) | Configurazione del container |
| 11 | [Decisione G1](11-decisione-g1.md) | 1b | 28 feb 2027 | non stimato | In attesa (10) | Fasi 2-9 (esito "continua"), piano commerciale |
| 16 | [Fase 3: importazione e misura](16-fase-3-importazione-e-misura.md) | 3 | 14 mar 2027 | circa 10 h | Da fare | Fase 4 |
| 17 | [Fase 4: giochi a chiave e soluzioni](17-fase-4-giochi-a-chiave-e-soluzioni.md) | 4 | 14 mar 2027 (interpretazione); 11 apr 2027 (fase) | circa 11 h | Da fare | Fase 5 |
| 18 | [Fase 5: rebus, sudoku e involucro](18-fase-5-rebus-e-involucro-desktop.md) | 5 | 2 mag 2027 | circa 9 h (la ROADMAP dice 8 h nel testo) | Da fare | Release `v0.6.0`, fase 6 |
| 19 | [Fase 6: lessico, marca temporale, interruttore](19-fase-6-lessico-marca-e-interruttore.md) | 6 | 30 mag 2027 | circa 14 h | Da fare | Fase 7 |
| 20 | [Fase 7: format e gioco del giorno](20-fase-7-format-e-gioco-del-giorno.md) | 7 | 27 giu 2027 | circa 13 h | Da fare | Fase 8 |
| 23 | [Decisioni strategiche](23-decisioni-strategiche.md) | varie | 28 giu 2027 (format, firma); 30 nov 2027 (G6) | non stimato | Da fare | Soglie finali, fasi 7-9 |
| 21 | [Fase 8: Sito, Admin e lancio](21-fase-8-sito-e-lancio.md) | 8 | 1 set 2027 (lancio) | circa 20 h, più 15 h di contenuti | Da fare | Fase 9, release `v1.0.0` |
| 22 | [Fase 9: crescita commerciale](22-fase-9-crescita-commerciale.md) | 9 | nov 2027 | circa 9 h | Da fare | Esportazioni e Lettore incorporabile |
| 00 | [Impostazioni GitHub e ambiente locale](00-impostazioni-github-e-ambiente.md) | 0 | Nessuna scadenza fissata | non stimato (fase 0: circa 7 h) | Da fare | Hook dei segreti, database locale, GHCR |

## Già chiuso (non serve fare nulla)

- Domanda 12 della ROADMAP, giochi logici: sudoku, futoshiki e nonogramma confermati l'11/11/2026.
- Fase 0 chiusa il 19/11/2026. Immagine `0.1.1` verificata.
- Ondata 1.1 chiusa il 20/11/2026. Resta aperta la prova reale (file 03).
- ADR 001-006 accettati il 13/11/2026. ADR 008-011 accettati il 19/11/2026.
- Decisione sulla protezione di `main` e dei tag: rimandata (D1). Nessuna azione ora.

## Nomi usati nei file

- **Rivista**: un numero di una pubblicazione periodica in PDF.
- **Gioco**: un'unità di gioco, per esempio un cruciverba o un anagramma.
- **Tipologia**: il tipo di gioco, definito da un descrittore versionato.
- **Studio**: la superficie privata, sul PC del proprietario.
- **Sito pubblico** e **Admin**: la superficie per i giocatori e la sua gestione.
- **Lettore**: il componente che fa giocare a schermo.
- **Fornitore AI**: il servizio che legge le pagine o genera i candidati (per esempio Claude Code).
- **Archivio**: la cartella delle Riviste e delle pagine, sul PC.
- **Set di riferimento**: le Riviste e i risultati attesi usati per misurare l'accuratezza.
- **Registrazione**: la risposta salvata del Fornitore AI per una pagina.
- **Postazione**: la schermata di verifica dei Giochi.
- **Motore di estrazione** e **Motore di creazione**: le due pipeline del prodotto.

## Fonti

- `ROADMAP.md`: fasi, "Dal proprietario", tabella delle stime, sezioni 5 e 7.
- `CLAUDE.md`: sezioni 6 (comandi e segreti) e 7 (stato).
- `docs/CI.md`: impostazioni GitHub, hook, pubblicazione.
- `docs/fase-0-chiusura-decisioni.md`.
- `docs/business/05-piano-commerciale.md`: sezioni 12 e 16.
- `.planning/phases/1/PLAN.md`, `CONTEXT.md` e `prova-claude-code-1.1.md`.
