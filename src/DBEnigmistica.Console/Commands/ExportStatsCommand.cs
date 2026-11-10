using DBEnigmistica.Application.Services.Export;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class ExportStatsCommand : Command
{
    public ExportStatsCommand() : base("export-stats", "Esporta statistiche riviste in JSON")
    {
        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            getDefaultValue: () => "magazine-stats.json",
            description: "Percorso file di output");

        var prettyOption = new Option<bool>(
            aliases: new[] { "--pretty", "-p" },
            getDefaultValue: () => true,
            description: "Formattazione JSON leggibile");

        AddOption(outputOption);
        AddOption(prettyOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var pretty = context.ParseResult.GetValueForOption(prettyOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider
                ?? throw new InvalidOperationException("Service provider not available");

            var logger = services.GetRequiredService<ILogger<ExportStatsCommand>>();
            var exportService = services.GetRequiredService<DataExportService>();

            await ExecuteAsync(output, pretty, exportService, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        string outputPath,
        bool prettyPrint,
        DataExportService exportService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║        DBEnigmistica - Export Statistiche                 ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            System.Console.WriteLine($"💾 Output: {outputPath}");
            System.Console.WriteLine();

            System.Console.Write("⏳ Generazione statistiche...");

            var content = await exportService.ExportMagazineStatsAsJsonAsync(prettyPrint, cancellationToken);
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
            System.Console.WriteLine($"✅ Statistiche esportate: {Path.GetFullPath(outputPath)}");
            System.Console.WriteLine($"📦 Dimensione: {fileInfo.Length:N0} bytes");
            System.Console.WriteLine();

            logger.LogInformation("Exported magazine statistics to: {Path}", outputPath);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Errore durante l'export: {ex.Message}");
            System.Console.ResetColor();
            logger.LogError(ex, "Error exporting statistics");
            Environment.Exit(1);
        }
    }
}
