using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using SkiaSharp;

namespace DBEnigmistica.Infrastructure.Services.Pdf;

/// <summary>
/// Implementazione del servizio PDF usando PdfPig
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;
    private const int DefaultDpi = 300;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<PdfPageData>> ExtractPagesAsync(string pdfFilePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting pages from PDF: {PdfFilePath}", pdfFilePath);

        if (!File.Exists(pdfFilePath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
        }

        var pages = new List<PdfPageData>();

        await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfFilePath);
            
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var pageData = ConvertPageToImage(page);
                pages.Add(pageData);
                
                _logger.LogDebug("Extracted page {PageNumber}/{TotalPages}", page.Number, document.NumberOfPages);
            }
        }, cancellationToken);

        _logger.LogInformation("Extracted {PageCount} pages from PDF", pages.Count);
        return pages;
    }

    public async Task<PdfPageData?> ExtractPageAsync(string pdfFilePath, int pageNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting page {PageNumber} from PDF: {PdfFilePath}", pageNumber, pdfFilePath);

        if (!File.Exists(pdfFilePath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
        }

        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfFilePath);
            
            if (pageNumber < 1 || pageNumber > document.NumberOfPages)
            {
                _logger.LogWarning("Page number {PageNumber} out of range (1-{MaxPages})", pageNumber, document.NumberOfPages);
                return null;
            }

            var page = document.GetPage(pageNumber);
            return ConvertPageToImage(page);
        }, cancellationToken);
    }

    public async Task<int> GetPageCountAsync(string pdfFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfFilePath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
        }

        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfFilePath);
            return document.NumberOfPages;
        }, cancellationToken);
    }

    private PdfPageData ConvertPageToImage(Page page)
    {
        // Ottieni le dimensioni della pagina
        var width = (int)page.Width;
        var height = (int)page.Height;

        // Calcola dimensioni scalate basate su DPI
        var scaleFactor = DefaultDpi / 72.0; // PDF usa 72 DPI di default
        var scaledWidth = (int)(width * scaleFactor);
        var scaledHeight = (int)(height * scaleFactor);

        // Per ora creiamo un'immagine bianca con le dimensioni corrette
        // In una implementazione completa, dovremmo renderizzare il PDF
        // PdfPig non supporta rendering nativo, quindi servirebbe una libreria aggiuntiva
        using var surface = SKSurface.Create(new SKImageInfo(scaledWidth, scaledHeight));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        // TODO: Implementare rendering reale del PDF
        // Per ora restituiamo un'immagine placeholder
        _logger.LogWarning("PDF rendering not fully implemented - returning placeholder image");

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return new PdfPageData
        {
            PageNumber = page.Number,
            ImageData = data.ToArray(),
            Width = scaledWidth,
            Height = scaledHeight,
            ImageFormat = "PNG"
        };
    }
}
