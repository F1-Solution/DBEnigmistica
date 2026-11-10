using System.CommandLine;
using DBEnigmistica.Application.Services.Solutions;
using DBEnigmistica.Console.Cli;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands.Solution;

public class VerifySolutionCommand : Command
{
    public VerifySolutionCommand(CommandRunner runner) : base("verify-solution", "Verifica o rifiuta una soluzione")
    {
        var solutionIdOption = new Option<int>("--solution-id") { Description = "ID della soluzione da verificare", Required = true };
        var verifiedOption = new Option<bool>("--verified") { Description = "Indica se la soluzione è corretta (true/false)", Required = true };
        var notesOption = new Option<string?>("--notes") { Description = "Note sulla verifica (opzionale)" };

        Options.Add(solutionIdOption);
        Options.Add(verifiedOption);
        Options.Add(notesOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<VerifySolutionHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetRequiredValue(solutionIdOption),
                parseResult.GetRequiredValue(verifiedOption),
                parseResult.GetValue(notesOption),
                token),
            ct));
    }
}

public sealed class VerifySolutionHandler(IGameSolutionService solutionService, IAnsiConsole console, CliStreams streams)
{
    public async Task<int> ExecuteAsync(int solutionId, bool verified, string? notes, CancellationToken cancellationToken)
    {
        try
        {
            await solutionService.VerifyAsync(solutionId, verified, notes, cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            streams.Error.WriteLine($"Errore: {ex.Message}");
            return ExitCodes.Error;
        }

        console.MarkupLine(verified ? "[green]✓ Approvata[/]" : "[yellow]✗ Rifiutata[/]");
        console.MarkupLineInterpolated($"  ID soluzione: #{solutionId}");
        console.MarkupLineInterpolated($"  Stato: {(verified ? "Verificata" : "Non verificata")}");
        if (!string.IsNullOrWhiteSpace(notes))
            console.MarkupLineInterpolated($"  Note: {notes}");

        return ExitCodes.Success;
    }
}
