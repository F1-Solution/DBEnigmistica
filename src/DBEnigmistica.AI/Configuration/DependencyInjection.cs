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
        // Claude API Client
        services.AddSingleton<ClaudeApiClient>();

        // AI Services
        services.AddScoped<IAiGameAnalyzer, ClaudeGameAnalyzer>();

        return services;
    }
}
