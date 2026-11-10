using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Application.Services.Processing;
using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Application.Services.Solution;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Schema Services
        services.AddScoped<ISchemaProposalService, SchemaProposalService>();
        services.AddScoped<ISchemaValidationService, SchemaValidationService>();
        services.AddScoped<DynamicSchemaOrchestrator>();

        // Processing Services
        services.AddScoped<GameExtractionService>();
        services.AddScoped<MagazineProcessingService>();

        // Export Services
        services.AddScoped<DataExportService>();

        // Solution Services
        services.AddScoped<ISolutionMatchingService, SolutionMatchingService>();
        services.AddScoped<ISolutionExtractionService, SolutionExtractionService>();

        return services;
    }
}
