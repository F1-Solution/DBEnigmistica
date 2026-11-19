# ADR 003: lettura delle pagine con visione, senza OCR

- **Stato:** Accettato il 13/11/2026
- **Data:** 11/11/2026

## Contesto

Il Motore di estrazione deve trasformare le pagine di una Rivista in Giochi. Il codice precedente usava Tesseract OCR, un'analisi del layout euristica e una classificazione sul testo. La valutazione del 10/11/2026 (`docs/VALUTAZIONE-STATO-ATTUALE.md`) ha trovato pagine bianche e un OCR mai inizializzato: il risultato non era utilizzabile. Tesseract, SkiaSharp e l'analisi del layout sono stati eliminati nell'ondata 0.1-A (`ROADMAP.md`, sezione 3, fase 0). Le griglie dei cruciverba, i rebus e la disposizione delle definizioni non si leggono bene con l'OCR.

## Decisione

1. **Rendering della pagina** con PDFium (pacchetto PDFtoImage) a una risoluzione configurabile, tra 150 e 200 DPI. Quando il PDF ha testo nativo, si estrae con PdfPig e si passa insieme all'immagine.
2. **Impronte SHA-256** del PDF e di ogni pagina, per rendere l'elaborazione idempotente: una riscansione salta le pagine già lette.
3. **Una chiamata per pagina** al Fornitore AI (ADR 004), con: l'immagine, il testo nativo, il catalogo dei descrittori e uno JSON Schema per l'output strutturato. La risposta contiene l'elenco dei Giochi: Tipologia nota oppure "nuova tipologia proposta", titolo, riquadro normalizzato tra 0 e 1, dati conformi al descrittore, confidenza per campo e avvisi.
4. **Il programma verifica la risposta** anche quando il fornitore non impone lo schema. I validatori strutturali del descrittore (per esempio la numerazione coerente con la griglia) abbassano la confidenza dei Giochi incoerenti.
5. **Seconda chiamata sul ritaglio ingrandito** del solo Gioco che non passa la validazione: resta disattivata. Si accende solo se il set di riferimento dimostra un miglioramento dell'accuratezza.
6. **Misura su un set di riferimento** fornito dal proprietario, con le metriche della fase 1 (richiamo, precisione, tipologia, caselle, CER, campi esatti). Le soglie si confermano dopo la prima misura.
7. **Una transazione per pagina.** Ogni pagina viene salvata in una transazione: un errore su una pagina non annulla le altre, e una ripresa riparte dalle pagine mancanti.
8. **Coda delle cartelle** nei limiti del fornitore: quando la finestra dell'abbonamento o la quota del piano gratuito si esaurisce, la coda si ferma e riprende da sola. Ogni chiamata registra fornitore, modello, durata, esito e risposta grezza, più token e costo quando il fornitore li riporta.
9. **Modello più capace per i descrittori**: solo per progettare descrittori di Tipologie nuove, chiamate rare in cui conta la qualità. L'estrazione usa il modello configurato (ADR 004).

## Conseguenze

- La pagina passa al fornitore come immagine: il costo e i tempi dipendono dal numero di pagine e dalla finestra dell'abbonamento. Il limite per esecuzione e la coda che si ferma e riprende sono previsti nella fase 1.
- PDFium è una libreria nativa. L'immagine Docker finale (`runtime:10.0-noble-chiseled`) oggi non ammette librerie native: il `Dockerfile` fallisce la build se trova file `.so`, `.dylib` o `runtimes/*/native`. La fase 1 deve sostituire quel controllo con un elenco ammesso e provare il rendering nell'immagine chiseled, anche con PDF a font non incorporati.
- Un rendering a 150-200 DPI occupa molta memoria per pagina. Il limite di pagine per esecuzione, previsto nella fase 1, contiene il consumo di una Rivista intera.
- Le griglie delle scansioni sono il punto più debole. La soglia di caselle corrette è più alta per le scansioni (98%) che per i PDF digitali (99,5%). Se la misura è sotto soglia, le griglie delle scansioni passano dalla verifica manuale.

## Alternative scartate

- **Tesseract OCR con analisi del layout euristica**: pagine bianche, OCR mai inizializzato, dati scartati (valutazione del 10/11/2026). Eliminato.
- **SkiaSharp per il rendering**: sostituito da PDFium, eliminato nella fase 0.
- **Solo testo nativo del PDF, senza immagine**: non legge le scansioni e non descrive la disposizione di griglie e rebus. Il testo nativo resta un ingresso affiancato all'immagine, non sostitutivo.
- **Seconda chiamata sul ritaglio come comportamento predefinito**: raddoppia il costo per pagina. Resta disattivata finché la misura non la giustifica.

## Riferimenti

- `ROADMAP.md`, sezione 2.3 (lettura delle pagine) e sezione 3, fase 0 (eliminazione di OCR e layout) e fase 1 (perché qui, rischi).
- `docs/VALUTAZIONE-STATO-ATTUALE.md`: pagine bianche e OCR mai inizializzato.
- `Dockerfile`: controllo sulle librerie native nella pubblicazione.
- Requisiti: OBJ-03, OBJ-04 (con visione), TEC-05, TEC-06 (rendering), AI-01.
