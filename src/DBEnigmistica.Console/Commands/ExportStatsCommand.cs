using System.CommandLine;
using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands;

public class ExportStatsCommand : Command
{
    public ExportStatsCommand(CommandRunner runner) : base("export-stats", "Esporta statistiche riviste in JSON")
    {
        var outputOption = new Option<string>("--output", "-o")
        {
            Description = "Percorso file di output",
            DefaultValueFactory = _ => "magazine-stats.json"
        };
        var prettyOption = new Option<bool>("--pretty", "-p")
        {
            Description = "Formattazione JSON leggibile",
            DefaultValueFactory = _ => true
        };

        Options.Add(outputOption);
        Options.Add(prettyOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<ExportStatsHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetRequiredValue(outputOption),
                parseResult.GetValue(prettyOption),
                token),
            ct));
    }
}

public sealed class ExportStatsHandler(
    DataExportService exportService,
    IAnsiConsole console,
    ILogger<ExportStatsHandler> logger)
{
    public async Task<int> ExecuteAsync(string outputPath, bool prettyPrint, CancellationToken cancellationToken)
    {
        ConsoleBanner.Write(console, "Export statistiche");
        console.MarkupLineInterpolated($"Output: {outputPath}");

        var content = await exportService.ExportMagazineStatsAsJsonAsync(prettyPrint, cancellationToken);
        await exportService.SaveToFileAsync(content, outputPath, cancellationToken);

        ConsoleBanner.Success(console, $"Statistiche esportate: {Path.GetFullPath(outputPath)}");
        console.MarkupLineInterpolated($"Dimensione: {new FileInfo(outputPath).Length:N0} byte");

        logger.LogInformation("Exported magazine statistics to: {Path}", outputPath);
        return ExitCodes.Success;
    }
}
