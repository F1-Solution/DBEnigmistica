using DBEnigmistica.Application.Registro;
using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Application.Services.Processing;
using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Application.Services.Solutions;
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

        // Solution Services
        services.AddScoped<IGameSolutionService, GameSolutionService>();

        // Processing Services
        services.AddScoped<GameExtractionService>();

        // Registro
        services.AddScoped<VerificaIntegritaRegistro>();

        // Export Services
        services.AddScoped<DataExportService>();

        return services;
    }
}
