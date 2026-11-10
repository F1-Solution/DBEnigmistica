using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Repositories;
using DBEnigmistica.Infrastructure.Services.Pdf;
using DBEnigmistica.Infrastructure.Services.Ocr;
using DBEnigmistica.Infrastructure.Services.LayoutAnalysis;
using DBEnigmistica.Infrastructure.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace DBEnigmistica.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<EnigmisticaDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IMagazineRepository, MagazineRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameTypeSchemaRepository, GameTypeSchemaRepository>();
        services.AddScoped<IGameSolutionRepository, GameSolutionRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Services - PDF
        services.AddScoped<IPdfService, PdfService>();

        // Services - Caching
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024 * 1024 * 100; // 100MB size limit
        });
        services.AddSingleton<OcrCacheService>();

        // Services - OCR (with caching)
        var tessDataPath = configuration["Ocr:TessdataPath"] ?? "./tessdata";
        var ocrLanguage = configuration["Ocr:Language"] ?? "ita";
        var cacheEnabled = bool.Parse(configuration["Ocr:CacheEnabled"] ?? "true");
        
        services.AddSingleton<TesseractOcrService>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TesseractOcrService>>();
            return new TesseractOcrService(logger, tessDataPath, ocrLanguage);
        });

        services.AddSingleton<IOcrService>(sp =>
        {
            var innerService = sp.GetRequiredService<TesseractOcrService>();
            var cacheService = sp.GetRequiredService<OcrCacheService>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CachedOcrService>>();
            return new CachedOcrService(innerService, cacheService, logger, cacheEnabled);
        });

        // Services - Layout Analysis
        services.AddScoped<ILayoutAnalysisService, BasicLayoutAnalysisService>();

        // Services - Dynamic Schema
        services.AddScoped<IDynamicMigrationService, Services.Database.DynamicMigrationService>();

        return services;
    }
}
