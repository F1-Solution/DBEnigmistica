using DBEnigmistica.Application.Registro;
using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Application.Services.Solutions;
using DBEnigmistica.Application.Valutazione;
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

        // Registro
        services.AddScoped<VerificaIntegritaRegistro>();

        // Valutazione
        services.AddValutazione();

        // Export Services
        services.AddScoped<DataExportService>();

        return services;
    }
}
