using System.CommandLine;
using DBEnigmistica.Application.Services.Solutions;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using DBEnigmistica.Domain.Enums;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands.Solution;

public class LinkSolutionCommand : Command
{
    public LinkSolutionCommand(CommandRunner runner) : base("link-solution", "Associa una soluzione a un gioco")
    {
        var gameIdOption = new Option<int>("--game-id") { Description = "ID del gioco a cui associare la soluzione", Required = true };
        var solutionOption = new Option<string>("--solution") { Description = "Testo della soluzione", Required = true };
        var pageIdOption = new Option<int?>("--page-id") { Description = "ID della pagina contenente la soluzione (opzionale)" };
        var typeOption = new Option<SolutionType>("--type")
        {
            Description = "Tipo di soluzione: InSamePage, InSameIssue, InNextIssue, External",
            DefaultValueFactory = _ => SolutionType.InSameIssue
        };

        Options.Add(gameIdOption);
        Options.Add(solutionOption);
        Options.Add(pageIdOption);
        Options.Add(typeOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<LinkSolutionHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetRequiredValue(gameIdOption),
                parseResult.GetRequiredValue(solutionOption),
                parseResult.GetValue(pageIdOption),
                parseResult.GetRequiredValue(typeOption),
                token),
            ct));
    }
}

public sealed class LinkSolutionHandler(IGameSolutionService solutionService, IAnsiConsole console, CliStreams streams)
{
    public async Task<int> ExecuteAsync(
        int gameId,
        string solution,
        int? pageId,
        SolutionType solutionType,
        CancellationToken cancellationToken)
    {
        try
        {
            var linked = await solutionService.LinkAsync(gameId, solution, pageId, solutionType, cancellationToken);

            ConsoleBanner.Success(console, $"Soluzione associata con successo (ID: {linked.Id})");
            console.MarkupLineInterpolated($"  Gioco: #{gameId}");
            console.MarkupLineInterpolated($"  Soluzione: {solution}");
            console.MarkupLineInterpolated($"  Tipo: {solutionType}");
            if (pageId.HasValue)
                console.MarkupLineInterpolated($"  Pagina: #{pageId}");

            return ExitCodes.Success;
        }
        catch (KeyNotFoundException ex)
        {
            streams.Error.WriteLine($"Errore: {ex.Message}");
            return ExitCodes.Error;
        }
    }
}
