using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Registro;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DBEnigmistica.Application.Creazione;

public static class DependencyInjectionCreazione
{
    /// <summary>
    /// Registra i casi d'uso della creazione. Servono già registrati: <see cref="ICatalogoTipologie"/>, <see cref="ICandidatiRepository"/>,
    /// <see cref="ITransazionePrototipo"/>, <see cref="IRegistroEventi"/>, <see cref="IFornitoreAiCreazione"/> e <see cref="IValidatoreSchemaJson"/>.
    /// Le regole in bozza non sono mai ammesse: solo i test le abilitano costruendo i casi d'uso a mano.
    /// </summary>
    public static IServiceCollection AddCreazione(this IServiceCollection services)
    {
        services.TryAddSingleton<IGeneratoriAlgoritmici, GeneratoriAlgoritmici>();
        services.TryAddSingleton<ISorgenteSemi, SorgenteSemiCasuale>();
        services.TryAddSingleton(_ => ModelloPromptCreazione.Carica(RisorseCreazione.PromptCreazione));
        services.TryAddSingleton(p => new ValidatoreBrief(
            p.GetRequiredService<IValidatoreSchemaJson>(), p.GetRequiredService<IGeneratoriAlgoritmici>().Supporta));
        services.AddScoped<GeneraCandidato>();
        services.AddScoped<RevisioneCandidati>();
        return services;
    }
}
