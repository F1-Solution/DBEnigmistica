using DBEnigmistica.Domain.Services;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per l'analisi del layout di una pagina
/// </summary>
public interface ILayoutAnalysisService
{
    /// <summary>
    /// Analizza il layout di un'immagine e identifica le regioni
    /// </summary>
    Task<IEnumerable<PageRegion>> AnalyzeLayoutAsync(byte[] imageData, OcrResult ocrResult, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Identifica potenziali griglie/schemi nella pagina
    /// </summary>
    Task<IEnumerable<PageRegion>> DetectGridsAsync(byte[] imageData, CancellationToken cancellationToken = default);
}
