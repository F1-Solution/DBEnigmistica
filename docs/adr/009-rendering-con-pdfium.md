# ADR 009: rendering delle pagine con PDFium, PNG codificato dal progetto

- **Stato:** Accettato il 19/11/2026
- **Data:** 19/11/2026
- **Sostituisce:** ADR 003, punto 1 (solo la libreria di rendering). Gli altri punti dell'ADR 003 restano validi.
- **Fonte:** `docs/fase-1-decisioni.md`, decisione D3

## Contesto

L'ADR 003 sceglie PDFium per il rendering delle pagine, ma cita il pacchetto PDFtoImage. Quel pacchetto dipende da SkiaSharp, che è stato eliminato in fase 0 e che lo stesso ADR 003 scarta. La scelta della libreria va quindi corretta prima della fase 1.

## Decisione

1. Il rendering usa **PDFiumCore 157.0.8100** (Apache-2.0) per i collegamenti e **`bblanchon.PDFium.*` 160.0.8165** (Apache-2.0) per la libreria nativa.
2. Il PNG lo codifica il progetto, in circa 80 righe con `System.IO.Compression`, coperte da test.
3. La risoluzione resta tra 150 e 200 DPI, come nell'ADR 003.
4. Due verifiche si fanno nell'ondata 1.1:
   - il rendering funziona nell'immagine chiseled linux-x64;
   - i font non incorporati vengono resi in modo leggibile.

## Alternative scartate

- **PDFtoImage 5.4.0**: dipende da SkiaSharp, eliminato in fase 0.
- **Solo la libreria nativa, con le funzioni dichiarate a mano**: la manutenzione dei collegamenti resterebbe a carico del progetto.
- **Docnet.Core 2.6.0**: il progetto è fermo dal 2023.

## Conseguenze

- Il progetto possiede il codice di codifica PNG, e per questo deve testarlo.
- PDFiumCore e i pacchetti `bblanchon.PDFium.*` vanno aggiornati insieme. Una versione nuova va verificata con le stesse due prove.
- Il controllo del `Dockerfile` sulle librerie native, previsto dall'ADR 003, va sostituito con un elenco ammesso. Resta una verifica di build nell'immagine chiseled.
- Se una delle due verifiche dell'ondata 1.1 fallisce, la decisione si riapre con un nuovo ADR.

## Riferimenti

- `docs/fase-1-decisioni.md`, D3 e la sezione «Rischi» (PDFium e font).
- ADR 003, punto 1 (sostituito) e conseguenze sulle librerie native.
- Requisiti: TEC-05, TEC-06.
