using DBEnigmistica.Application.Services.Export;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class ExportGamesCommand : Command
{
    public ExportGamesCommand() : base("export-games", "Esporta giochi estratti in JSON o CSV")
    {
        var formatOption = new Option<string>(
            aliases: new[] { "--format", "-f" },
            getDefaultValue: () => "json",
            description: "Formato export: json o csv");

        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Percorso file di output (default: games.[format])");

        var magazineOption = new Option<int?>(
            aliases: new[] { "--magazine", "-m" },
            description: "Filtra per ID rivista (opzionale)");

        var prettyOption = new Option<bool>(
            aliases: new[] { "--pretty", "-p" },
            getDefaultValue: () => true,
            description: "Formattazione JSON leggibile (solo per JSON)");

        AddOption(formatOption);
        AddOption(outputOption);
        AddOption(magazineOption);
        AddOption(prettyOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var format = context.ParseResult.GetValueForOption(formatOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption);
            var magazineId = context.ParseResult.GetValueForOption(magazineOption);
            var pretty = context.ParseResult.GetValueForOption(prettyOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider
                ?? throw new InvalidOperationException("Service provider not available");

            var logger = services.GetRequiredService<ILogger<ExportGamesCommand>>();
            var exportService = services.GetRequiredService<DataExportService>();

            await ExecuteAsync(format, output, magazineId, pretty, exportService, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        string format,
        string? outputPath,
        int? magazineId,
        bool prettyPrint,
        DataExportService exportService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║           DBEnigmistica - Export Giochi                   ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            format = format.ToLowerInvariant();
            if (format != "json" && format != "csv")
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("✗ Formato non valido. Usa 'json' o 'csv'");
                System.Console.ResetColor();
                Environment.Exit(1);
                return;
            }

            outputPath ??= $"games.{format}";

            System.Console.WriteLine($"📊 Formato: {format.ToUpper()}");
            if (magazineId.HasValue)
                System.Console.WriteLine($"📚 Rivista ID: {magazineId}");
            System.Console.WriteLine($"💾 Output: {outputPath}");
            System.Console.WriteLine();

            System.Console.Write("⏳ Esportazione in corso...");

            string content;
            if (format == "json")
            {
                content = await exportService.ExportGamesAsJsonAsync(magazineId, prettyPrint, cancellationToken);
            }
            else
            {
                content = await exportService.ExportGamesAsCsvAsync(magazineId, cancellationToken);
            }

            await exportService.SaveToFileAsync(content, outputPath, cancellationToken);

            System.Console.WriteLine(" ✓");
            System.Console.WriteLine();

            var fileInfo = new FileInfo(outputPath);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║                  ✅ SUCCESSO                               ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();
            System.Console.WriteLine($"✅ Export completato: {Path.GetFullPath(outputPath)}");
            System.Console.WriteLine($"📦 Dimensione: {fileInfo.Length:N0} bytes");
            System.Console.WriteLine();

            logger.LogInformation("Exported games to {Format} format: {Path}", format, outputPath);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Errore durante l'export: {ex.Message}");
            System.Console.ResetColor();
            logger.LogError(ex, "Error exporting games");
            Environment.Exit(1);
        }
    }
}
