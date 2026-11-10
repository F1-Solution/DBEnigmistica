using System.CommandLine;
using System.Globalization;
using System.Text.Json;
using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands.Solution;

public class ExportSolutionsCommand : Command
{
    public ExportSolutionsCommand(CommandRunner runner) : base("export-solutions", "Esporta soluzioni in JSON o CSV")
    {
        var outputOption = new Option<string>("--output") { Description = "Percorso file di output (obbligatorio)", Required = true };
        var formatOption = new Option<string>("--format")
        {
            Description = "Formato di esportazione: json o csv (default: json)",
            DefaultValueFactory = _ => "json"
        };
        var verifiedOnlyOption = new Option<bool>("--verified-only") { Description = "Esporta solo soluzioni verificate (default: false)" };
        var magazineIdOption = new Option<int?>("--magazine-id") { Description = "ID della rivista da filtrare (opzionale)" };

        Options.Add(outputOption);
        Options.Add(formatOption);
        Options.Add(verifiedOnlyOption);
        Options.Add(magazineIdOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<ExportSolutionsHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetRequiredValue(outputOption),
                parseResult.GetRequiredValue(formatOption),
                parseResult.GetValue(verifiedOnlyOption),
                parseResult.GetValue(magazineIdOption),
                token),
            ct));
    }
}

public sealed class ExportSolutionsHandler(IGameSolutionRepository repository, IAnsiConsole console, CliStreams streams)
{
    public async Task<int> ExecuteAsync(
        string outputPath,
        string format,
        bool verifiedOnly,
        int? magazineId,
        CancellationToken cancellationToken)
    {
        format = format.ToLowerInvariant();
        if (format is not ("json" or "csv"))
        {
            streams.Error.WriteLine("Formato non supportato: usa 'json' o 'csv'.");
            return ExitCodes.Error;
        }

        var solutions = await LoadAsync(verifiedOnly, magazineId);
        if (solutions.Count == 0)
        {
            ConsoleBanner.Warning(console, "Nessuna soluzione trovata con i criteri specificati.");
            return ExitCodes.Success;
        }

        if (format == "json")
            await ExportToJsonAsync(solutions, outputPath, cancellationToken);
        else
            await ExportToCsvAsync(solutions, outputPath, cancellationToken);

        ConsoleBanner.Success(console, $"Esportate {solutions.Count} soluzioni");
        ConsoleBanner.Success(console, $"File salvato: {outputPath}");
        return ExitCodes.Success;
    }

    private async Task<List<GameSolution>> LoadAsync(bool verifiedOnly, int? magazineId)
    {
        if (verifiedOnly)
            return (await repository.GetVerifiedSolutionsAsync(magazineId)).ToList();

        var all = (await repository.GetAllAsync()).ToList();
        return magazineId.HasValue
            ? all.Where(s => s.Game?.Page?.MagazineId == magazineId.Value).ToList()
            : all;
    }

    private static async Task ExportToJsonAsync(List<GameSolution> solutions, string outputPath, CancellationToken cancellationToken)
    {
        var data = solutions.Select(s => new
        {
            s.Id,
            s.GameId,
            s.SolutionText,
            s.SolutionPageId,
            SolutionType = s.SolutionType.ToString(),
            s.ConfidenceScore,
            s.IsVerified,
            s.PositionX,
            s.PositionY,
            ExtractedDate = s.ExtractedDate.ToString("O"),
            VerifiedDate = s.VerifiedDate?.ToString("O"),
            s.Notes
        }).ToList();

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputPath, json, cancellationToken);
    }

    private static async Task ExportToCsvAsync(List<GameSolution> solutions, string outputPath, CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(outputPath);
        await writer.WriteLineAsync(CsvFieldFormatter.SeparatorDirective);
        await writer.WriteLineAsync("ID,GameID,SolutionText,SolutionPageID,SolutionType,ConfidenceScore,IsVerified,PositionX,PositionY,ExtractedDate,VerifiedDate,Notes");

        foreach (var s in solutions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = $"{CsvFieldFormatter.Number(s.Id)}," +
                       $"{CsvFieldFormatter.Number(s.GameId)}," +
                       $"{CsvFieldFormatter.Format(s.SolutionText)}," +
                       $"{CsvFieldFormatter.Number(s.SolutionPageId)}," +
                       $"{CsvFieldFormatter.Format(s.SolutionType.ToString())}," +
                       $"{CsvFieldFormatter.Number(s.ConfidenceScore)}," +
                       $"{s.IsVerified}," +
                       $"{CsvFieldFormatter.Number(s.PositionX)}," +
                       $"{CsvFieldFormatter.Number(s.PositionY)}," +
                       $"{CsvFieldFormatter.Date(s.ExtractedDate)}," +
                       $"{CsvFieldFormatter.Date(s.VerifiedDate)}," +
                       $"{CsvFieldFormatter.Format(s.Notes)}";

            await writer.WriteLineAsync(line);
        }
    }
}
