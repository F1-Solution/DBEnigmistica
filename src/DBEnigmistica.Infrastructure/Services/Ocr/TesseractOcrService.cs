using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace DBEnigmistica.Infrastructure.Services.Ocr;

/// <summary>
/// Implementazione del servizio OCR usando Tesseract
/// </summary>
public class TesseractOcrService : IOcrService
{
    private readonly ILogger<TesseractOcrService> _logger;
    private readonly string _tessDataPath;
    private readonly string _language;
    private TesseractEngine? _engine;
    private bool _disposed;

    public TesseractOcrService(ILogger<TesseractOcrService> logger, string tessDataPath = "./tessdata", string language = "ita")
    {
        _logger = logger;
        _tessDataPath = tessDataPath;
        _language = language;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_engine != null)
        {
            _logger.LogDebug("OCR engine already initialized");
            return;
        }

        await Task.Run(() =>
        {
            _logger.LogInformation("Initializing Tesseract OCR engine with language: {Language}, data path: {TessDataPath}", _language, _tessDataPath);

            if (!Directory.Exists(_tessDataPath))
            {
                _logger.LogWarning("Tessdata directory not found: {TessDataPath}. Creating directory.", _tessDataPath);
                Directory.CreateDirectory(_tessDataPath);
            }

            try
            {
                _engine = new TesseractEngine(_tessDataPath, _language, EngineMode.Default);
                _logger.LogInformation("Tesseract OCR engine initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Tesseract OCR engine");
                throw;
            }
        }, cancellationToken);
    }

    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                using var pix = Pix.LoadFromMemory(imageData);
                using var page = _engine!.Process(pix);

                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                var words = new List<OcrWord>();
                using var iterator = page.GetIterator();
                iterator.Begin();

                do
                {
                    if (iterator.TryGetBoundingBox(PageIteratorLevel.Word, out var bounds))
                    {
                        var wordText = iterator.GetText(PageIteratorLevel.Word);
                        var wordConfidence = iterator.GetConfidence(PageIteratorLevel.Word);

                        if (!string.IsNullOrWhiteSpace(wordText))
                        {
                            words.Add(new OcrWord
                            {
                                Text = wordText,
                                Confidence = wordConfidence / 100f,
                                BoundingBox = new BoundingBox
                                {
                                    X = bounds.X1,
                                    Y = bounds.Y1,
                                    Width = bounds.X2 - bounds.X1,
                                    Height = bounds.Y2 - bounds.Y1
                                }
                            });
                        }
                    }
                } while (iterator.Next(PageIteratorLevel.Word));

                _logger.LogDebug("OCR completed. Text length: {TextLength}, Words: {WordCount}, Confidence: {Confidence:P}",
                    text.Length, words.Count, confidence);

                return new OcrResult
                {
                    Text = text,
                    Confidence = confidence,
                    Words = words
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR recognition failed");
                throw;
            }
        }, cancellationToken);
    }

    public async Task<OcrResult> RecognizeTextInRegionAsync(byte[] imageData, BoundingBox region, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        return await Task.Run(() =>
        {
            try
            {
                // Per ora usiamo l'immagine completa e filtriamo le parole nella regione
                // In una implementazione completa, dovremmo croppare l'immagine prima
                _logger.LogWarning("Region-based OCR not fully optimized - processing full image and filtering results");
                
                using var pix = Pix.LoadFromMemory(imageData);
                using var page = _engine!.Process(pix);

                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                // Filtra solo il testo che cade nella regione specificata
                // Per ora restituiamo tutto - da migliorare
                _logger.LogDebug("OCR in region completed. Text length: {TextLength}, Confidence: {Confidence:P}",
                    text.Length, confidence);

                return new OcrResult
                {
                    Text = text,
                    Confidence = confidence,
                    Words = []
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR recognition in region failed");
                throw;
            }
        }, cancellationToken);
    }

    private void EnsureInitialized()
    {
        if (_engine == null)
        {
            throw new InvalidOperationException("OCR engine not initialized. Call InitializeAsync first.");
        }

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TesseractOcrService));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _engine?.Dispose();
        _engine = null;
        _disposed = true;
        
        _logger.LogInformation("Tesseract OCR engine disposed");
    }
}
