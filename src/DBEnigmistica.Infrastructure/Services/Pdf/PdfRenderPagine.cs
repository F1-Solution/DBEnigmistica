using System.Runtime.InteropServices;
using System.Security.Cryptography;
using DBEnigmistica.Application.Acquisizione;
using PDFiumCore;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Exceptions;

namespace DBEnigmistica.Infrastructure.Services.Pdf;

/// <summary>
/// Rendering con PDFium e lettura (testo nativo, impronte) con PdfPig.
/// L'impronta della pagina deriva dal contenuto (testo, immagini, tracciati) e non dal rendering,
/// così non cambia con il sistema operativo, i font installati, la risoluzione o la versione di PDFium.
/// </summary>
public sealed class PdfRenderPagine : IRenderPagine
{
    private const int DpiMinimo = 36;
    private const int DpiMassimo = 600;
    // Una pagina A3 a 300 DPI (circa 17,4 milioni di pixel) è il massimo accettato.
    private const long PixelMassimi = 18_000_000;
    private const int FormatoBgrx = 3;
    private const uint ErroreFormato = 3;
    private const uint ErrorePassword = 4;
    private const uint ErroreSicurezza = 5;

    // PDFium non è thread-safe: ogni chiamata nativa passa da questo blocco.
    private static readonly object Blocco = new();
    private static bool _inizializzata;

    public async Task<InfoPdf> LeggiAsync(string percorsoPdf, CancellationToken cancellationToken = default) =>
        await LeggiAsync(await LeggiFileAsync(percorsoPdf, cancellationToken), cancellationToken);

    public async Task<PaginaRenderizzata> RenderizzaAsync(string percorsoPdf, int numeroPagina, int dpi, CancellationToken cancellationToken = default) =>
        await RenderizzaAsync(await LeggiFileAsync(percorsoPdf, cancellationToken), numeroPagina, dpi, cancellationToken);

    public Task<InfoPdf> LeggiAsync(byte[] pdf, CancellationToken cancellationToken = default) =>
        Task.Run(() => Leggi(pdf, cancellationToken), cancellationToken);

    public Task<PaginaRenderizzata> RenderizzaAsync(byte[] pdf, int numeroPagina, int dpi, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dpi, DpiMinimo);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dpi, DpiMassimo);
        ArgumentOutOfRangeException.ThrowIfLessThan(numeroPagina, 1);
        return Task.Run(() => Renderizza(pdf, numeroPagina, dpi), cancellationToken);
    }

    private static async Task<byte[]> LeggiFileAsync(string percorsoPdf, CancellationToken cancellationToken)
    {
        if (!File.Exists(percorsoPdf))
            throw new FileNotFoundException($"File PDF non trovato: {percorsoPdf}", percorsoPdf);

        return await File.ReadAllBytesAsync(percorsoPdf, cancellationToken);
    }

    private static InfoPdf Leggi(byte[] pdf, CancellationToken cancellationToken)
    {
        // PDFium classifica per primo i PDF cifrati o danneggiati.
        lock (Blocco)
        {
            using var documento = ApriDocumento(pdf);
        }

        try
        {
            using var documentoPig = PdfDocument.Open(pdf);
            var pagine = new List<InfoPagina>(documentoPig.NumberOfPages);
            foreach (var pagina in documentoPig.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                pagine.Add(new InfoPagina(pagina.Number, pagina.Width, pagina.Height, ImprontaPagina(pagina), pagina.Text));
            }

            return new InfoPdf(SHA256.HashData(pdf), pagine);
        }
        catch (PdfDocumentEncryptedException ex)
        {
            throw Cifrato(ex);
        }
        // Qualsiasi altro errore di PdfPig rende il PDF danneggiato: senza testo e impronta della pagina non c'è una chiave stabile per le Registrazioni.
        catch (Exception ex) when (ex is not (OperationCanceledException or OutOfMemoryException))
        {
            throw Danneggiato(ex);
        }
    }

    private static PaginaRenderizzata Renderizza(byte[] pdf, int numeroPagina, int dpi)
    {
        lock (Blocco)
        {
            using var documento = ApriDocumento(pdf);
            var totale = fpdfview.FPDF_GetPageCount(documento.Documento);
            if (numeroPagina > totale)
                throw new ArgumentOutOfRangeException(nameof(numeroPagina), numeroPagina, $"Il PDF ha {totale} pagine.");

            var pagina = fpdfview.FPDF_LoadPage(documento.Documento, numeroPagina - 1)
                         ?? throw Danneggiato(null, $"Impossibile leggere la pagina {numeroPagina}.");
            try
            {
                var larghezza = Pixel(fpdfview.FPDF_GetPageWidthF(pagina), dpi);
                var altezza = Pixel(fpdfview.FPDF_GetPageHeightF(pagina), dpi);
                if ((long)larghezza * altezza > PixelMassimi)
                    throw new InvalidOperationException($"Pagina {numeroPagina} troppo grande a {dpi} DPI ({larghezza}x{altezza} pixel).");

                return DisegnaPagina(pagina, numeroPagina, dpi, larghezza, altezza);
            }
            finally
            {
                fpdfview.FPDF_ClosePage(pagina);
            }
        }
    }

    private static PaginaRenderizzata DisegnaPagina(FpdfPageT pagina, int numeroPagina, int dpi, int larghezza, int altezza)
    {
        var stride = larghezza * 4;
        var pixel = new byte[(long)stride * altezza];
        var fissati = GCHandle.Alloc(pixel, GCHandleType.Pinned);
        try
        {
            var bitmap = fpdfview.FPDFBitmapCreateEx(larghezza, altezza, FormatoBgrx, fissati.AddrOfPinnedObject(), stride)
                         ?? throw new InvalidOperationException("PDFium non ha creato la bitmap.");
            try
            {
                fpdfview.FPDFBitmapFillRect(bitmap, 0, 0, larghezza, altezza, 0xFFFFFFFF);
                fpdfview.FPDF_RenderPageBitmap(bitmap, pagina, 0, 0, larghezza, altezza, 0, 0);
            }
            finally
            {
                fpdfview.FPDFBitmapDestroy(bitmap);
            }
        }
        finally
        {
            fissati.Free();
        }

        return new PaginaRenderizzata(numeroPagina, larghezza, altezza, dpi, PngEncoder.CodificaBgrx(pixel, larghezza, altezza, stride), ContaPixelNonBianchi(pixel));
    }

    // Conta i pixel con un canale sotto 240: l'antialiasing quasi bianco non conta.
    private static long ContaPixelNonBianchi(byte[] bgrx)
    {
        long conteggio = 0;
        for (var i = 0; i < bgrx.Length; i += 4)
        {
            if (bgrx[i] < 240 || bgrx[i + 1] < 240 || bgrx[i + 2] < 240)
                conteggio++;
        }

        return conteggio;
    }

    private static int Pixel(float punti, int dpi) => (int)Math.Round(punti * dpi / 72.0, MidpointRounding.AwayFromZero);

    private sealed class DocumentoPdfium : IDisposable
    {
        private readonly GCHandle _dati;

        public DocumentoPdfium(FpdfDocumentT documento, GCHandle dati)
        {
            Documento = documento;
            _dati = dati;
        }

        public FpdfDocumentT Documento { get; }

        public void Dispose()
        {
            fpdfview.FPDF_CloseDocument(Documento);
            _dati.Free();
        }
    }

    private static DocumentoPdfium ApriDocumento(byte[] pdf)
    {
        if (!_inizializzata)
        {
            fpdfview.FPDF_InitLibrary();
            _inizializzata = true;
        }

        var dati = GCHandle.Alloc(pdf, GCHandleType.Pinned);
        var documento = fpdfview.FPDF_LoadMemDocument64(dati.AddrOfPinnedObject(), (ulong)pdf.Length, null);
        if (documento is null)
        {
            var errore = fpdfview.FPDF_GetLastError();
            dati.Free();
            throw errore switch
            {
                ErrorePassword or ErroreSicurezza => Cifrato(null),
                ErroreFormato => Danneggiato(null),
                _ => Danneggiato(null, $"Impossibile aprire il PDF (codice PDFium {errore}).")
            };
        }

        return new DocumentoPdfium(documento, dati);
    }

    private static PdfNonLeggibileException Cifrato(Exception? inner) =>
        new(MotivoPdfNonLeggibile.Cifrato, "Il PDF è cifrato e richiede una password: rimuovi la protezione e riprova.", inner);

    private static PdfNonLeggibileException Danneggiato(Exception? inner, string? dettaglio = null) =>
        new(MotivoPdfNonLeggibile.Danneggiato, dettaglio ?? "Il PDF è danneggiato o non è un PDF valido.", inner);

    // Serializzazione canonica del contenuto della pagina, indipendente dal rendering.
    private static byte[] ImprontaPagina(Page pagina)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var memoria = new MemoryStream();
        using (var scrittore = new BinaryWriter(memoria, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            scrittore.Write(Centesimi(pagina.Width));
            scrittore.Write(Centesimi(pagina.Height));

            foreach (var lettera in pagina.Letters)
            {
                scrittore.Write(lettera.Value);
                scrittore.Write(lettera.FontName ?? string.Empty);
                scrittore.Write(Centesimi(lettera.StartBaseLine.X));
                scrittore.Write(Centesimi(lettera.StartBaseLine.Y));
                scrittore.Write(Centesimi(lettera.FontSize));
            }

            foreach (var immagine in pagina.GetImages())
            {
                scrittore.Write(Centesimi(immagine.BoundingBox.Left));
                scrittore.Write(Centesimi(immagine.BoundingBox.Bottom));
                scrittore.Write(Centesimi(immagine.BoundingBox.Right));
                scrittore.Write(Centesimi(immagine.BoundingBox.Top));
                scrittore.Write(immagine.RawMemory.Length);
                hash.AppendData(memoria.ToArray());
                memoria.SetLength(0);
                hash.AppendData(immagine.RawMemory.Span);
            }

            foreach (var tracciato in pagina.Paths)
            {
                var riquadro = tracciato.GetBoundingRectangle();
                scrittore.Write(tracciato.IsFilled);
                scrittore.Write(tracciato.IsStroked);
                scrittore.Write(tracciato.Count);
                if (riquadro is { } r)
                {
                    scrittore.Write(Centesimi(r.Left));
                    scrittore.Write(Centesimi(r.Bottom));
                    scrittore.Write(Centesimi(r.Right));
                    scrittore.Write(Centesimi(r.Top));
                }
            }
        }

        hash.AppendData(memoria.ToArray());
        return hash.GetHashAndReset();
    }

    private static long Centesimi(double valore) => (long)Math.Round(valore * 100, MidpointRounding.AwayFromZero);
}
