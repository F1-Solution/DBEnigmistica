using System.CommandLine;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Generatore;

namespace DBEnigmistica.Console.Commands;

/// <summary>Gestione delle Tipologie: <c>types generate [--check]</c> scrive o controlla l'SQL registrato nei file <c>tipologie/</c>.</summary>
public class TypesCommand : Command
{
    public TypesCommand(CommandRunner runner)
        : base("types", "Gestisce le Tipologie dei Giochi")
    {
        Subcommands.Add(new TypesGenerateCommand(runner));
    }
}

public class TypesGenerateCommand : Command
{
    public TypesGenerateCommand(CommandRunner runner)
        : base("generate", "Genera l'SQL delle Tipologie dai descrittori; con --check confronta senza scrivere")
    {
        var check = new Option<bool>("--check") { Description = "Non scrive: esce con un codice diverso da 0 se l'SQL registrato è diverso o manca" };
        var cartella = new Option<DirectoryInfo?>("--cartella") { Description = "Cartella tipologie (predefinita: quella del repository)" };
        Options.Add(check);
        Options.Add(cartella);

        this.SetAction((parseResult, ct) => runner.RunAsync<TypesGenerateHandler>(
            HostRequirements.None,
            (handler, _) => Task.FromResult(handler.Execute(parseResult.GetValue(check), parseResult.GetValue(cartella)?.FullName)),
            ct));
    }
}

public sealed class TypesGenerateHandler(CliStreams streams)
{
    public int Execute(bool soloControllo, string? cartella)
    {
        var radice = cartella ?? TrovaCartellaTipologie();
        if (radice is null || !Directory.Exists(radice))
        {
            streams.Error.WriteLine("Cartella tipologie non trovata: usa --cartella.");
            return ExitCodes.Error;
        }

        CatalogoTipologie catalogo;
        try
        {
            catalogo = new CaricatoreDescrittori().CaricaCartella(radice);
        }
        catch (DescrittoreNonValidoException ex)
        {
            streams.Error.WriteLine($"Descrittore non valido ({ex.Origine}):");
            foreach (var errore in ex.Errori)
                streams.Error.WriteLine($"  - {errore}");
            return ExitCodes.Error;
        }

        EsitoAllineamento esito;
        try
        {
            esito = AllineamentoSql.Allinea(catalogo.Descrittori, radice, scrivi: !soloControllo);
        }
        catch (DescrittoreNonGenerabileException ex)
        {
            streams.Error.WriteLine(ex.Message);
            return ExitCodes.Error;
        }

        return soloControllo ? RiferisciControllo(esito) : RiferisciGenerazione(esito);
    }

    private int RiferisciControllo(EsitoAllineamento esito)
    {
        foreach (var file in esito.Differenti)
            streams.Error.WriteLine($"SQL diverso da quello generato: {file}");
        foreach (var file in esito.Mancanti)
            streams.Error.WriteLine($"SQL mancante: {file}");
        foreach (var file in esito.Orfani)
            streams.Error.WriteLine($"SQL senza descrittore con SQL: {file}");

        if (!esito.EAllineato)
        {
            streams.Error.WriteLine("Esegui 'dbe types generate' e rivedi i file prodotti.");
            return ExitCodes.Error;
        }

        streams.Out.WriteLine($"SQL delle Tipologie allineato: {esito.Invariati} file.");
        return ExitCodes.Success;
    }

    private int RiferisciGenerazione(EsitoAllineamento esito)
    {
        foreach (var file in esito.Scritti)
            streams.Out.WriteLine($"Scritto {file}");
        foreach (var file in esito.Orfani)
            streams.Error.WriteLine($"Attenzione, SQL senza descrittore con SQL (non cancellato): {file}");

        streams.Out.WriteLine($"File scritti: {esito.Scritti.Count}; invariati: {esito.Invariati}.");
        return ExitCodes.Success;
    }

    // Risale dalla cartella corrente e da quella dell'eseguibile fino a trovare tipologie/_meta.
    private static string? TrovaCartellaTipologie()
    {
        foreach (var partenza in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var cartella = new DirectoryInfo(partenza); cartella is not null; cartella = cartella.Parent)
            {
                var candidata = Path.Combine(cartella.FullName, "tipologie");
                if (Directory.Exists(Path.Combine(candidata, "_meta")))
                    return candidata;
            }
        }

        return null;
    }
}
