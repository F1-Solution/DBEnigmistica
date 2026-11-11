using DBEnigmistica.AI.Services;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.AI.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fino alla fase 1 l'analizzatore è un segnaposto che non apre connessioni di rete.
        services.AddScoped<IAiGameAnalyzer, FornitoreAiNonDisponibile>();

        return services;
    }
}
