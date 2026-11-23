using System.CommandLine;
using System.Text.Json;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using DBEnigmistica.Domain.Interfaces;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands.Solution;

public class ListUnsolvedCommand : Command
{
    public ListUnsolvedCommand(CommandRunner runner) : base("list-unsolved", "Elenca i giochi senza soluzione")
    {
        var magazineIdOption = new Option<int?>("--magazine-id") { Description = "ID della rivista da filtrare (opzionale)" };
        var tipologiaOption = new Option<string?>("--tipologia") { Description = "Tipologia da filtrare, per esempio cruciverba-classico (opzionale)" };
        var exportOption = new Option<string?>("--export") { Description = "Percorso file per esportare in JSON (opzionale)" };

        Options.Add(magazineIdOption);
        Options.Add(tipologiaOption);
        Options.Add(exportOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<ListUnsolvedHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetValue(magazineIdOption),
                parseResult.GetValue(tipologiaOption),
                parseResult.GetValue(exportOption),
                token),
            ct));
    }
}

public sealed class ListUnsolvedHandler(IGameSolutionRepository repository, IAnsiConsole console)
{
    public async Task<int> ExecuteAsync(
        int? magazineId,
        string? tipologia,
        string? exportPath,
        CancellationToken cancellationToken)
    {
        var games = (await repository.GetUnsolvedGamesAsync(magazineId))
            .Where(g => tipologia is null || g.Tipologia == tipologia)
            .ToList();

        if (games.Count == 0)
        {
            ConsoleBanner.Warning(console, "Nessun gioco senza soluzione trovato.");
            return ExitCodes.Success;
        }

        var table = new Table().AddColumn("ID").AddColumn("Titolo").AddColumn("Tipologia").AddColumn("Pagina");
        foreach (var game in games)
            table.AddRow(game.Id.ToString(), Markup.Escape(game.Title ?? "N/A"), Markup.Escape(game.Tipologia), game.PageId.ToString());

        console.Write(table);
        ConsoleBanner.Success(console, $"Trovati {games.Count} giochi senza soluzione");

        if (!string.IsNullOrWhiteSpace(exportPath))
            await ExportAsync(games, exportPath, cancellationToken);

        return ExitCodes.Success;
    }

    private async Task ExportAsync(
        IEnumerable<Domain.Entities.Game> games,
        string exportPath,
        CancellationToken cancellationToken)
    {
        var data = games.Select(g => new
        {
            g.Id,
            g.Title,
            g.Tipologia,
            g.PageId,
            g.RiquadroX,
            g.RiquadroY
        }).ToList();

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(exportPath, json, cancellationToken);

        ConsoleBanner.Success(console, $"Dati esportati in: {exportPath}");
    }
}
