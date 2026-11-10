using DBEnigmistica.Domain.Services;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per la lettura di file PDF
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Estrae dimensioni e testo nativo di tutte le pagine
    /// </summary>
    Task<IEnumerable<PdfPageData>> ExtractPagesAsync(string pdfFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Estrae dimensioni e testo nativo di una singola pagina
    /// </summary>
    Task<PdfPageData?> ExtractPageAsync(string pdfFilePath, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ottiene il numero totale di pagine in un PDF
    /// </summary>
    Task<int> GetPageCountAsync(string pdfFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rende una pagina come immagine PNG
    /// </summary>
    Task<byte[]> RenderPageAsync(string pdfFilePath, int pageNumber, int dpi, CancellationToken cancellationToken = default);
}
