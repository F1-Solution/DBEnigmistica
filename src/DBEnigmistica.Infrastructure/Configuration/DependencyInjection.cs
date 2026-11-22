using DBEnigmistica.Application.Acquisizione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Registro;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Prototipo;
using DBEnigmistica.Infrastructure.Repositories;
using DBEnigmistica.Infrastructure.Services.Archivio;
using DBEnigmistica.Infrastructure.Services.Pdf;
using DBEnigmistica.Infrastructure.Services.Registrazioni;
using DBEnigmistica.Infrastructure.Tipologie;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddScoped<IRegistroEventi, RegistroEventiRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Tipologie: SQL applicato da migrate e dati dei Giochi nelle tabelle per Tipologia
        services.AddSingleton<MigratoreTipologie>();
        services.AddSingleton<ArchivioDatiTipologia>();

        // Services - PDF, Archivio e opzioni (validate quando si leggono, così solo i comandi che le usano le richiedono)
        services.AddSingleton<IRenderPagine, PdfRenderPagine>();
        services.AddSingleton<IArchivio, ArchivioFileSystem>();
        services.AddSingleton<IFabbricaArchivioRegistrazioni, FabbricaArchivioRegistrazioniFileSystem>();
        services.AddSingleton<IFabbricaArchivioRegistrazioniCreazione, FabbricaArchivioRegistrazioniCreazioneFileSystem>();
        services.AddSingleton<IValidateOptions<ArchivioOptions>, ArchivioOptionsValidator>();
        services.AddSingleton<IValidateOptions<EstrazioneOptions>, EstrazioneOptionsValidator>();
        services.AddOptions<ArchivioOptions>()
            .Configure(o => o.Cartella = configuration[$"{ArchivioOptions.SectionName}:{nameof(ArchivioOptions.Cartella)}"]);
        services.AddOptions<EstrazioneOptions>()
            .Configure(o =>
            {
                var testo = configuration[$"{EstrazioneOptions.SectionName}:{nameof(EstrazioneOptions.Dpi)}"];
                if (string.IsNullOrWhiteSpace(testo))
                    return;

                if (int.TryParse(testo, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var dpi))
                    o.Dpi = dpi;
                else
                    o.DpiNonNumerico = testo;
            });

        return services;
    }
}
