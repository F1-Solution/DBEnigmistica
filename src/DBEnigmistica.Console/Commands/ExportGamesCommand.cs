using System.CommandLine;
using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands;

public class ExportGamesCommand : Command
{
    public ExportGamesCommand(CommandRunner runner) : base("export-games", "Esporta giochi estratti in JSON o CSV")
    {
        var formatOption = new Option<string>("--format", "-f")
        {
            Description = "Formato export: json o csv",
            DefaultValueFactory = _ => "json"
        };
        var outputOption = new Option<string?>("--output", "-o") { Description = "Percorso file di output (default: games.[format])" };
        var magazineOption = new Option<int?>("--magazine", "-m") { Description = "Filtra per ID rivista (opzionale)" };
        var prettyOption = new Option<bool>("--pretty", "-p")
        {
            Description = "Formattazione JSON leggibile (solo per JSON)",
            DefaultValueFactory = _ => true
        };

        Options.Add(formatOption);
        Options.Add(outputOption);
        Options.Add(magazineOption);
        Options.Add(prettyOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<ExportGamesHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetRequiredValue(formatOption),
                parseResult.GetValue(outputOption),
                parseResult.GetValue(magazineOption),
                parseResult.GetValue(prettyOption),
                token),
            ct));
    }
}

public sealed class ExportGamesHandler(
    DataExportService exportService,
    IAnsiConsole console,
    CliStreams streams,
    ILogger<ExportGamesHandler> logger)
{
    public async Task<int> ExecuteAsync(
        string format,
        string? outputPath,
        int? magazineId,
        bool prettyPrint,
        CancellationToken cancellationToken)
    {
        ConsoleBanner.Write(console, "Export giochi");

        format = format.ToLowerInvariant();
        if (format is not ("json" or "csv"))
        {
            streams.Error.WriteLine("Formato non valido: usa 'json' o 'csv'.");
            return ExitCodes.Error;
        }

        outputPath ??= $"games.{format}";
        console.MarkupLineInterpolated($"Formato: {format.ToUpperInvariant()}");
        if (magazineId.HasValue)
            console.MarkupLineInterpolated($"Rivista ID: {magazineId}");
        console.MarkupLineInterpolated($"Output: {outputPath}");

        var content = format == "json"
            ? await exportService.ExportGamesAsJsonAsync(magazineId, prettyPrint, cancellationToken)
            : await exportService.ExportGamesAsCsvAsync(magazineId, cancellationToken);

        await exportService.SaveToFileAsync(content, outputPath, cancellationToken);

        ConsoleBanner.Success(console, $"Export completato: {Path.GetFullPath(outputPath)}");
        console.MarkupLineInterpolated($"Dimensione: {new FileInfo(outputPath).Length:N0} byte");

        logger.LogInformation("Exported games to {Format} format: {Path}", format, outputPath);
        return ExitCodes.Success;
    }
}
