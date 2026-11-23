using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DBEnigmistica.Application.Valutazione;

public static class DependencyInjectionValutazione
{
    /// <summary>
    /// Registra la valutazione. Servono già registrati: <c>IRenderPagine</c>, <c>IFabbricaArchivioRegistrazioni</c>,
    /// <c>IInterpreteRispostaGrezza</c>, <c>IValidatoreSchemaJson</c> e <see cref="IGeneratorePdfSet"/>.
    /// </summary>
    public static IServiceCollection AddValutazione(this IServiceCollection services)
    {
        services.TryAddSingleton<IVersionePromptEstrazione>(_ => new VersionePromptCatalogo());
        services.AddScoped<RisolutorePagineSet>();
        services.AddScoped<ServizioRegressioneEstrazione>();
        services.AddScoped<ProduttoreRegistrazioniSintetiche>();
        services.AddScoped<GeneratoreBozzeAttese>();
        return services;
    }
}
