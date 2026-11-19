using DBEnigmistica.Console.Commands.Solution;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Console.Commands;

public static class HandlerRegistration
{
    /// <summary>Registra i gestori dei comandi: uno scope per esecuzione.</summary>
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<ListGamesHandler>();
        services.AddScoped<CheckAiHandler>();
        services.AddScoped<MigrateHandler>();
        services.AddScoped<VerifyIntegrityHandler>();
        services.AddScoped<ExportGamesHandler>();
        services.AddScoped<ExportSchemaHandler>();
        services.AddScoped<ExportStatsHandler>();
        services.AddScoped<LinkSolutionHandler>();
        services.AddScoped<VerifySolutionHandler>();
        services.AddScoped<ListUnsolvedHandler>();
        services.AddScoped<ExportSolutionsHandler>();
        return services;
    }
}
