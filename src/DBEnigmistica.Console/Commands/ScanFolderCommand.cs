using DBEnigmistica.Application.Services.Processing;
using DBEnigmistica.Application.Services.UI;
using DBEnigmistica.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class ScanFolderCommand : Command
{
    public ScanFolderCommand() : base("scan-folder", "Scansiona una cartella di file PDF ed estrae i giochi")
    {
        var pathArgument = new Argument<string>(
            name: "path",
            description: "Percorso della cartella contenente i file PDF");

        var titleOption = new Option<string?>(
            aliases: new[] { "--title", "-t" },
            description: "Titolo della rivista (opzionale)");

        var publisherOption = new Option<string?>(
            aliases: new[] { "--publisher", "-p" },
            description: "Editore (opzionale)");

        var periodicityOption = new Option<MagazinePeriodicity>(
            aliases: new[] { "--periodicity", "-r" },
            getDefaultValue: () => MagazinePeriodicity.Monthly,
            description: "Periodicità della rivista");

        AddArgument(pathArgument);
        AddOption(titleOption);
        AddOption(publisherOption);
        AddOption(periodicityOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var path = context.ParseResult.GetValueForArgument(pathArgument);
            var title = context.ParseResult.GetValueForOption(titleOption);
            var publisher = context.ParseResult.GetValueForOption(publisherOption);
            var periodicity = context.ParseResult.GetValueForOption(periodicityOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not available");
            
            var logger = services.GetRequiredService<ILogger<ScanFolderCommand>>();
            var processingService = services.GetRequiredService<MagazineProcessingService>();

            await ExecuteAsync(path, title, publisher, periodicity, processingService, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        string folderPath,
        string? title,
        string? publisher,
        MagazinePeriodicity periodicity,
        MagazineProcessingService processingService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║         DBEnigmistica - Scansione Cartella PDF           ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            // Validate folder
            if (!Directory.Exists(folderPath))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: La cartella '{folderPath}' non esiste.");
                System.Console.ResetColor();
                Environment.Exit(1);
                return;
            }

            System.Console.WriteLine($"📁 Cartella: {folderPath}");
            if (!string.IsNullOrEmpty(title))
                System.Console.WriteLine($"📖 Rivista: {title}");
            System.Console.WriteLine($"🔄 Periodicità: {periodicity}");
            System.Console.WriteLine();

            // Create progress reporter
            var progressReporter = new ConsoleProgressReporter();

            // Create metadata
            var metadata = new MagazineMetadata
            {
                Title = title,
                Publisher = publisher,
                Periodicity = periodicity
            };

            // Process folder
            var startTime = DateTime.UtcNow;
            var results = await processingService.ProcessFolderAsync(
                folderPath,
                metadata,
                progressReporter,
                cancellationToken);

            var duration = DateTime.UtcNow - startTime;

            // Show summary
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║                    RIEPILOGO                              ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            var successfulMagazines = results.Count(r => r.Success);
            var failedMagazines = results.Count(r => !r.Success);
            var totalPages = results.Sum(r => r.PagesProcessed);
            var totalGames = results.Sum(r => r.GamesExtracted);

            System.Console.WriteLine($"📚 File processati:     {results.Count}");
            System.Console.WriteLine($"✅ Successi:            {successfulMagazines}");
            if (failedMagazines > 0)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"❌ Falliti:             {failedMagazines}");
                System.Console.ResetColor();
            }
            System.Console.WriteLine($"📄 Pagine elaborate:    {totalPages}");
            System.Console.WriteLine($"🎮 Giochi estratti:     {totalGames}");
            System.Console.WriteLine($"⏱️  Tempo impiegato:     {duration:mm\\:ss}");
            System.Console.WriteLine();

            // Show failed magazines if any
            if (failedMagazines > 0)
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("⚠️  File con errori:");
                System.Console.ResetColor();
                foreach (var result in results.Where(r => !r.Success))
                {
                    System.Console.WriteLine($"  • {Path.GetFileName(result.PdfPath)}: {result.ErrorMessage}");
                }
                System.Console.WriteLine();
            }

            logger.LogInformation(
                "Folder processing completed: {Magazines} magazines, {Pages} pages, {Games} games",
                results.Count, totalPages, totalGames);
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("⚠️  Operazione annullata dall'utente.");
            System.Console.ResetColor();
            Environment.Exit(130);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Errore fatale: {ex.Message}");
            System.Console.ResetColor();
            logger.LogError(ex, "Fatal error during folder scan");
            Environment.Exit(1);
        }
    }
}
