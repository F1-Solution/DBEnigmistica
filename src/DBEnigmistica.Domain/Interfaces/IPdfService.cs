using DBEnigmistica.Domain.Services;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per l'estrazione di pagine da file PDF
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Estrae tutte le pagine da un file PDF come immagini
    /// </summary>
    Task<IEnumerable<PdfPageData>> ExtractPagesAsync(string pdfFilePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Estrae una singola pagina da un file PDF
    /// </summary>
    Task<PdfPageData?> ExtractPageAsync(string pdfFilePath, int pageNumber, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ottiene il numero totale di pagine in un PDF
    /// </summary>
    Task<int> GetPageCountAsync(string pdfFilePath, CancellationToken cancellationToken = default);
}
