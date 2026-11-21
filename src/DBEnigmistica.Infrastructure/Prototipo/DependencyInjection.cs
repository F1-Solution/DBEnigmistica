using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Registro;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Infrastructure.Prototipo;

public static class DependencyInjectionPrototipo
{
    public const string SezioneCartellaTipologie = "Prototipo:CartellaTipologie";
    public const string CartellaTipologiePredefinita = "tipologie";

    /// <summary>
    /// Cartella dei descrittori: senza valore è <c>tipologie</c> accanto all'eseguibile (la pubblicazione la porta con sé);
    /// un valore relativo si risolve dalla cartella corrente.
    /// </summary>
    public static string CartellaTipologie(IConfiguration configuration) =>
        configuration[SezioneCartellaTipologie] is { Length: > 0 } valore
            ? Path.GetFullPath(valore)
            : Path.Combine(AppContext.BaseDirectory, CartellaTipologiePredefinita);

    /// <summary>
    /// Servizi di persistenza del prototipo: il contesto lavora sempre con il ruolo <c>dbe_creazione</c>
    /// (<c>SET ROLE</c> su ogni connessione, stessa stringa di connessione dello Studio).
    /// </summary>
    public static IServiceCollection AddPrototipoInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<EnigmisticaDbContext>(options =>
            options.UseNpgsql(connectionString).AddInterceptors(new RuoloCreazioneInterceptor()));
        services.AddScoped<IRegistroEventi, RegistroEventiRepository>();
        services.AddScoped<ICandidatiRepository, CandidatiRepository>();
        services.AddScoped<ITransazionePrototipo, TransazionePrototipo>();
        services.AddSingleton<ICatalogoTipologie>(_ =>
            new CatalogoTipologieFileSystem(CartellaTipologie(configuration)));
        services.AddSingleton<IFabbricaArchivioRegistrazioniCreazione, FabbricaArchivioRegistrazioniCreazioneFileSystem>();
        return services;
    }
}
