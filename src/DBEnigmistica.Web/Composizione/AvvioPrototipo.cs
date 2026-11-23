using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Prototipo;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DBEnigmistica.Web.Composizione;

/// <summary>Punto d'ingresso di <c>dbe prototipo</c>: valida la configurazione, avvia l'host e attende la chiusura.</summary>
public static class AvvioPrototipo
{
    /// <summary>
    /// Restituisce 0 alla chiusura con Ctrl+C. La configurazione mancante lancia <see cref="OptionsValidationException"/>
    /// (l'eseguibile la riporta con il codice 3).
    /// </summary>
    public static async Task<int> EseguiAsync(IConfiguration configurazione, TextWriter output, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configurazione);
        ArgumentNullException.ThrowIfNull(output);
        var errori = Verifica(configurazione, new AmbienteFornitoreAi());
        if (errori.Count > 0)
            throw new OptionsValidationException(Options.DefaultName, typeof(FornitoreAiOptions), errori);

        await using var app = PrototipoHost.Crea(configurazione);
        await VerificaRuoloAsync(app, cancellationToken);
        await app.StartAsync(cancellationToken);
        try
        {
            await using (var scope = app.Services.CreateAsyncScope())
            {
                var riparati = await scope.ServiceProvider.GetRequiredService<RevisioneCandidati>().RiparaGenerazioniInterrotteAsync(cancellationToken);
                if (riparati > 0)
                    await output.WriteLineAsync($"Generazioni interrotte portate a errore: {riparati}.");
            }

            foreach (var indirizzo in PrototipoHost.Indirizzi(app))
                await output.WriteLineAsync($"Prototipo in ascolto su {indirizzo} (Ctrl+C per terminare).");

            await app.WaitForShutdownAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Ctrl+C è la chiusura normale.
        }
        finally
        {
            await app.StopAsync(CancellationToken.None);
        }

        return 0;
    }

    public const string MessaggioRuolo =
        "Il login del database non può usare il ruolo dbe_creazione: deve poter eseguire SET ROLE dbe_creazione " +
        "(superutente oppure membro del ruolo). Cambia l'utente in ConnectionStrings__DefaultConnection.";

    /// <summary>Prima di ascoltare verifica che le connessioni lavorino davvero come <c>dbe_creazione</c>.</summary>
    internal static async Task VerificaRuoloAsync(WebApplication app, CancellationToken cancellationToken)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var contesto = scope.ServiceProvider.GetRequiredService<EnigmisticaDbContext>();
        string? utente;
        try
        {
            utente = (await contesto.Database.SqlQuery<string>($"SELECT current_user AS \"Value\"").ToListAsync(cancellationToken)).SingleOrDefault();
        }
        catch (Exception ex) when (ex.GetBaseException() is PostgresException { SqlState: "42501" or "22023" or "0LP01" })
        {
            throw new OptionsValidationException(Options.DefaultName, typeof(FornitoreAiOptions), [MessaggioRuolo]);
        }

        if (utente != RuoloCreazioneInterceptor.Ruolo)
            throw new OptionsValidationException(Options.DefaultName, typeof(FornitoreAiOptions), [MessaggioRuolo]);
    }

    /// <summary>Errori di configurazione: stringa di connessione, modello e Fornitore AI di creazione, cartella dei descrittori.</summary>
    public static IReadOnlyList<string> Verifica(IConfiguration configurazione, IAmbienteFornitoreAi ambiente)
    {
        var errori = new List<string>();
        if (string.IsNullOrWhiteSpace(configurazione.GetConnectionString("DefaultConnection")))
            errori.Add("Stringa di connessione mancante: imposta ConnectionStrings__DefaultConnection.");

        var opzioni = new FornitoreAiOptions();
        configurazione.GetSection(FornitoreAiOptions.SectionName).Bind(opzioni);
        errori.AddRange(ValidatoreOpzioniCreazione.Valida(opzioni, ambiente));

        // Fuori dalla riproduzione delle Registrazioni la generazione chiama il Fornitore AI dal vivo.
        var riproduce = opzioni.Creazione.Registrazioni.Modalita?.Trim() == RegistrazioniOptions.ModalitaRiproduci;
        if (!opzioni.DalVivoAbilitato && !riproduce)
            errori.Add(FornitoreAiOptions.MessaggioDalVivoDisattivato);

        var cartella = DependencyInjectionPrototipo.CartellaTipologie(configurazione);
        if (!Directory.Exists(cartella))
            errori.Add($"Cartella dei descrittori non trovata: '{cartella}'. Serve la cartella tipologie accanto all'eseguibile, oppure imposta Prototipo__CartellaTipologie.");

        return errori;
    }
}
