using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Infrastructure.Services.Ocr;

/// <summary>
/// Wrapper per IOcrService che aggiunge caching automatico
/// </summary>
public class CachedOcrService : IOcrService
{
    private readonly IOcrService _innerOcrService;
    private readonly Cache.OcrCacheService _cacheService;
    private readonly ILogger<CachedOcrService> _logger;
    private readonly bool _cacheEnabled;

    public CachedOcrService(
        IOcrService innerOcrService,
        Cache.OcrCacheService cacheService,
        ILogger<CachedOcrService> logger,
        bool cacheEnabled = true)
    {
        _innerOcrService = innerOcrService;
        _cacheService = cacheService;
        _logger = logger;
        _cacheEnabled = cacheEnabled;
    }

    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData, CancellationToken cancellationToken = default)
    {
        // Prova cache se abilitata
        if (_cacheEnabled && _cacheService.TryGetCachedResult(imageData, out var cachedResult))
        {
            _logger.LogInformation("Using cached OCR result (saved processing time)");
            return cachedResult!;
        }

        // Cache miss o disabilitata - esegui OCR reale
        _logger.LogDebug("Performing OCR (cache miss or disabled)");
        var result = await _innerOcrService.RecognizeTextAsync(imageData, cancellationToken);

        // Salva in cache se abilitata
        if (_cacheEnabled)
        {
            _cacheService.CacheResult(imageData, result);
        }

        return result;
    }

    public async Task<OcrResult> RecognizeTextInRegionAsync(
        byte[] imageData, 
        BoundingBox region, 
        CancellationToken cancellationToken = default)
    {
        // Per region-specific OCR, usiamo il servizio inner direttamente
        // perché la cache per regioni specifiche richiederebbe chiavi più complesse
        _logger.LogDebug("Region-specific OCR (cache bypassed)");
        return await _innerOcrService.RecognizeTextInRegionAsync(imageData, region, cancellationToken);
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return _innerOcrService.InitializeAsync(cancellationToken);
    }

    public void Dispose()
    {
        _innerOcrService.Dispose();
    }
}
