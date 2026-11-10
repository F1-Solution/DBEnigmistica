using System.CommandLine;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands;

public class MigrateCommand : Command
{
    public MigrateCommand(CommandRunner runner) : base("migrate", "Applica le migrazioni pending al database")
    {
        var yesOption = new Option<bool>("--yes", "-y") { Description = "Applica senza chiedere conferma" };
        Options.Add(yesOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<MigrateHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(parseResult.GetValue(yesOption), token),
            ct));
    }
}

public sealed class MigrateHandler(
    EnigmisticaDbContext dbContext,
    IAnsiConsole console,
    CliStreams streams,
    ILogger<MigrateHandler> logger)
{
    public async Task<int> ExecuteAsync(bool assumeYes, CancellationToken cancellationToken)
    {
        ConsoleBanner.Write(console, "Migrazioni database");

        var pending = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            await WriteUpToDateAsync(cancellationToken);
            return ExitCodes.Success;
        }

        console.MarkupLineInterpolated($"[yellow]Trovate {pending.Count} migrazioni da applicare:[/]");
        foreach (var migration in pending)
            console.MarkupLineInterpolated($"   - {migration}");
        console.WriteLine();

        if (!assumeYes)
        {
            var confirmed = Confirm();
            if (confirmed is null)
                return ExitCodes.Error;

            if (confirmed == false)
            {
                ConsoleBanner.Warning(console, "Operazione annullata dall'utente.");
                return ExitCodes.Success;
            }
        }

        console.MarkupLine("Applicazione migrazioni in corso...");
        await dbContext.Database.MigrateAsync(cancellationToken);

        ConsoleBanner.Success(console, $"{pending.Count} migrazioni applicate con successo.");
        logger.LogInformation("Applied {Count} migrations successfully", pending.Count);
        return ExitCodes.Success;
    }

    private async Task WriteUpToDateAsync(CancellationToken cancellationToken)
    {
        ConsoleBanner.Success(console, "Database già aggiornato. Nessuna migrazione da applicare.");

        var applied = (await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
        if (applied.Count == 0)
            return;

        console.MarkupLineInterpolated($"Migrazioni applicate: {applied.Count}");
        console.MarkupLineInterpolated($"   Ultima: {applied[^1]}");
    }

    // Restituisce null se non c'è un terminale interattivo su cui chiedere.
    private bool? Confirm()
    {
        if (!console.Profile.Capabilities.Interactive)
        {
            streams.Error.WriteLine("Conferma non possibile senza terminale interattivo: usa --yes per applicare le migrazioni.");
            return null;
        }

        return console.Prompt(new ConfirmationPrompt("Procedere con l'applicazione?")
        {
            DefaultValue = false,
            Yes = 's',
            No = 'n'
        });
    }
}
