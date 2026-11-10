using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace DBEnigmistica.Infrastructure.Services.Cache;

/// <summary>
/// Cache service per risultati OCR con chiave basata su hash dell'immagine
/// </summary>
public class OcrCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OcrCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(24);

    public OcrCacheService(IMemoryCache cache, ILogger<OcrCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Genera chiave cache dall'hash SHA256 dell'immagine
    /// </summary>
    private string GenerateCacheKey(byte[] imageData)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(imageData);
        return $"ocr:{Convert.ToHexString(hash)}";
    }

    /// <summary>
    /// Prova a recuperare risultato OCR dalla cache
    /// </summary>
    public bool TryGetCachedResult(byte[] imageData, out OcrResult? result)
    {
        var key = GenerateCacheKey(imageData);
        
        if (_cache.TryGetValue(key, out OcrResult? cachedResult))
        {
            _logger.LogDebug("OCR cache HIT for key: {Key}", key);
            result = cachedResult;
            return true;
        }

        _logger.LogDebug("OCR cache MISS for key: {Key}", key);
        result = null;
        return false;
    }

    /// <summary>
    /// Salva risultato OCR in cache
    /// </summary>
    public void CacheResult(byte[] imageData, OcrResult result, TimeSpan? expiration = null)
    {
        var key = GenerateCacheKey(imageData);
        var expirationTime = expiration ?? _defaultExpiration;

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expirationTime)
            .SetSize(EstimateSize(result));

        _cache.Set(key, result, cacheOptions);
        
        _logger.LogDebug("Cached OCR result with key: {Key}, expires in {Minutes} minutes", 
            key, expirationTime.TotalMinutes);
    }

    /// <summary>
    /// Invalida cache per una specifica immagine
    /// </summary>
    public void InvalidateCache(byte[] imageData)
    {
        var key = GenerateCacheKey(imageData);
        _cache.Remove(key);
        _logger.LogDebug("Invalidated OCR cache for key: {Key}", key);
    }

    /// <summary>
    /// Pulisce tutta la cache OCR
    /// </summary>
    public void ClearAll()
    {
        // MemoryCache non ha un metodo Clear() built-in
        // In production si potrebbe usare un distributed cache come Redis
        _logger.LogWarning("ClearAll called but MemoryCache doesn't support clearing all entries");
    }

    /// <summary>
    /// Stima dimensione approssimativa del risultato per il size limit
    /// </summary>
    private long EstimateSize(OcrResult result)
    {
        // Stima approssimativa: lunghezza testo + numero di parole * 100 bytes
        long size = result.Text?.Length ?? 0;
        size += result.Words.Count() * 100; // Approssimazione per BoundingBox e metadata
        return size;
    }

    /// <summary>
    /// Statistiche cache (per diagnostica)
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        // MemoryCache non espone statistiche built-in
        // In un'implementazione reale si potrebbero tracciare manualmente
        return new CacheStatistics
        {
            Message = "Statistics not available with MemoryCache. Consider using Redis for production."
        };
    }
}

public class CacheStatistics
{
    public int? TotalEntries { get; set; }
    public int? HitCount { get; set; }
    public int? MissCount { get; set; }
    public long? TotalSize { get; set; }
    public string? Message { get; set; }
}
