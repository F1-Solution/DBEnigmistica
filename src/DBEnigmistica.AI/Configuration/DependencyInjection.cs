using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.FornitoreAi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.AI.Configuration;

public static class DependencyInjection
{
    /// <summary>Registra il Fornitore AI; richiede <c>IOptions&lt;FornitoreAiOptions&gt;</c>, che registra il chiamante.</summary>
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fino alla fase 4 l'analizzatore di schemi è un segnaposto che non apre connessioni di rete.
        services.AddScoped<IAiGameAnalyzer, FornitoreAiNonDisponibile>();

        services.TryAddSingleton<IValidatoreSchemaJson, ValidatoreSchemaJson>();
        services.TryAddSingleton<RegistroChiamateAiInMemoria>();
        services.TryAddSingleton<IRegistroChiamateAi>(p => p.GetRequiredService<RegistroChiamateAiInMemoria>());
        services.TryAddSingleton<ContatoreChiamateAi>();
        services.TryAddSingleton<IInterpreteRispostaGrezza, InterpreteRispostaClaudeCode>();
        services.TryAddSingleton<IFornitoreAiEstrazione>(p => CreaFornitore(
            p.GetRequiredService<IOptions<FornitoreAiOptions>>().Value,
            p.GetRequiredService<IValidatoreSchemaJson>(),
            p.GetRequiredService<IRegistroChiamateAi>(),
            p.GetRequiredService<ContatoreChiamateAi>(),
            p.GetRequiredService<IFabbricaArchivioRegistrazioni>(),
            p.GetRequiredService<IInterpreteRispostaGrezza>()));

        return services;
    }

    // Catena dal vivo: limite e contatore, registro delle chiamate, Registrazioni, adattatore. In riproduzione c'è solo la riproduzione.
    private static IFornitoreAiEstrazione CreaFornitore(
        FornitoreAiOptions opzioni,
        IValidatoreSchemaJson validatore,
        IRegistroChiamateAi registro,
        ContatoreChiamateAi contatore,
        IFabbricaArchivioRegistrazioni fabbrica,
        IInterpreteRispostaGrezza interprete)
    {
        if (opzioni.Tipo?.Trim() != FornitoreAiOptions.TipoClaudeCode)
            throw ErroreConfigurazione("Il Fornitore AI api-anthropic non è disponibile in fase 1: imposta FornitoreAi__Tipo=claude-code.");

        var modello = opzioni.Modello ?? string.Empty;
        var modalita = opzioni.Registrazioni.Modalita?.Trim();
        IArchivioRegistrazioni? archivio = null;
        if (!string.IsNullOrEmpty(modalita))
        {
            if (modalita is not (RegistrazioniOptions.ModalitaRegistra or RegistrazioniOptions.ModalitaRiproduci))
                throw ErroreConfigurazione("Modalità delle Registrazioni non valida: imposta FornitoreAi__Registrazioni__Modalita (registra o riproduci).");

            if (string.IsNullOrWhiteSpace(opzioni.Registrazioni.Cartella))
                throw ErroreConfigurazione("Cartella delle Registrazioni mancante: imposta FornitoreAi__Registrazioni__Cartella.");

            archivio = fabbrica.Apri(opzioni.Registrazioni.Cartella);
        }

        if (modalita == RegistrazioniOptions.ModalitaRiproduci)
            return new FornitoreAiRiproduci(FornitoreClaudeCode.Id, modello, archivio!, interprete, validatore);

        var impostazioni = new ImpostazioniClaudeCode(
            opzioni.ClaudeCode.Eseguibile ?? "claude",
            modello,
            TimeSpan.FromSeconds(opzioni.ClaudeCode.TimeoutSecondi ?? 300));
        IFornitoreAiEstrazione fornitore = new FornitoreClaudeCode(impostazioni, validatore);
        if (modalita == RegistrazioniOptions.ModalitaRegistra)
            fornitore = new FornitoreAiRegistra(fornitore, archivio!);

        return new FornitoreAiConLimite(new FornitoreAiConRegistroChiamate(fornitore, registro), opzioni.LimiteChiamate, contatore);
    }

    private static OptionsValidationException ErroreConfigurazione(string messaggio) =>
        new(Options.DefaultName, typeof(FornitoreAiOptions), [messaggio]);
}
