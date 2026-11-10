using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace DBEnigmistica.Infrastructure.Services.Pdf;

/// <summary>
/// Implementazione del servizio PDF usando PdfPig (testo nativo e conteggio pagine)
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<PdfPageData>> ExtractPagesAsync(string pdfFilePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting pages from PDF: {PdfFilePath}", pdfFilePath);
        EnsureFileExists(pdfFilePath);

        var pages = new List<PdfPageData>();

        await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfFilePath);

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                pages.Add(ToPageData(page));
            }
        }, cancellationToken);

        _logger.LogInformation("Extracted {PageCount} pages from PDF", pages.Count);
        return pages;
    }

    public async Task<PdfPageData?> ExtractPageAsync(string pdfFilePath, int pageNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting page {PageNumber} from PDF: {PdfFilePath}", pageNumber, pdfFilePath);
        EnsureFileExists(pdfFilePath);

        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfFilePath);

            if (pageNumber < 1 || pageNumber > document.NumberOfPages)
            {
                _logger.LogWarning("Page number {PageNumber} out of range (1-{MaxPages})", pageNumber, document.NumberOfPages);
                return null;
            }

            return ToPageData(document.GetPage(pageNumber));
        }, cancellationToken);
    }

    public async Task<int> GetPageCountAsync(string pdfFilePath, CancellationToken cancellationToken = default)
    {
        EnsureFileExists(pdfFilePath);

        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(pdfFilePath);
            return document.NumberOfPages;
        }, cancellationToken);
    }

    public Task<byte[]> RenderPageAsync(string pdfFilePath, int pageNumber, int dpi, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("disponibile dalla fase 1");
    }

    private static void EnsureFileExists(string pdfFilePath)
    {
        if (!File.Exists(pdfFilePath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfFilePath}");
        }
    }

    private static PdfPageData ToPageData(Page page) => new()
    {
        PageNumber = page.Number,
        Width = (int)page.Width,
        Height = (int)page.Height,
        NativeText = page.Text
    };
}
