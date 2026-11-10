using DBEnigmistica.Domain.Services;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per il riconoscimento ottico dei caratteri (OCR)
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Esegue OCR su un'immagine
    /// </summary>
    Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Esegue OCR su una regione specifica di un'immagine
    /// </summary>
    Task<OcrResult> RecognizeTextInRegionAsync(byte[] imageData, BoundingBox region, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inizializza il motore OCR
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rilascia le risorse del motore OCR
    /// </summary>
    void Dispose();
}
