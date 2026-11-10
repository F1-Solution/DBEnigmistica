using DBEnigmistica.Application.Services.Processing;
using DBEnigmistica.Application.Services.UI;
using DBEnigmistica.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class ScanFileCommand : Command
{
    public ScanFileCommand() : base("scan-file", "Scansiona un singolo file PDF ed estrae i giochi")
    {
        var pathArgument = new Argument<string>(
            name: "path",
            description: "Percorso del file PDF da scansionare");

        var titleOption = new Option<string?>(
            aliases: new[] { "--title", "-t" },
            description: "Titolo della rivista (opzionale, default: nome file)");

        var issueOption = new Option<int?>(
            aliases: new[] { "--issue", "-i" },
            description: "Numero della rivista (opzionale)");

        var dateOption = new Option<DateTime?>(
            aliases: new[] { "--date", "-d" },
            description: "Data di pubblicazione (formato: yyyy-MM-dd)");

        var periodicityOption = new Option<MagazinePeriodicity>(
            aliases: new[] { "--periodicity", "-r" },
            getDefaultValue: () => MagazinePeriodicity.Monthly,
            description: "Periodicità della rivista");

        AddArgument(pathArgument);
        AddOption(titleOption);
        AddOption(issueOption);
        AddOption(dateOption);
        AddOption(periodicityOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var path = context.ParseResult.GetValueForArgument(pathArgument);
            var title = context.ParseResult.GetValueForOption(titleOption);
            var issue = context.ParseResult.GetValueForOption(issueOption);
            var date = context.ParseResult.GetValueForOption(dateOption);
            var periodicity = context.ParseResult.GetValueForOption(periodicityOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not available");
            
            var logger = services.GetRequiredService<ILogger<ScanFileCommand>>();
            var processingService = services.GetRequiredService<MagazineProcessingService>();

            await ExecuteAsync(path, title, issue, date, periodicity, processingService, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        string filePath,
        string? title,
        int? issue,
        DateTime? publicationDate,
        MagazinePeriodicity periodicity,
        MagazineProcessingService processingService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║          DBEnigmistica - Scansione File PDF              ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            // Validate file
            if (!File.Exists(filePath))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: Il file '{filePath}' non esiste.");
                System.Console.ResetColor();
                Environment.Exit(1);
                return;
            }

            if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: Il file deve essere un PDF.");
                System.Console.ResetColor();
                Environment.Exit(1);
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var displayTitle = title ?? fileName;

            System.Console.WriteLine($"📄 File: {Path.GetFileName(filePath)}");
            System.Console.WriteLine($"📖 Rivista: {displayTitle}");
            if (issue.HasValue)
                System.Console.WriteLine($"🔢 Numero: {issue}");
            if (publicationDate.HasValue)
                System.Console.WriteLine($"📅 Data: {publicationDate.Value:dd/MM/yyyy}");
            System.Console.WriteLine($"🔄 Periodicità: {periodicity}");
            System.Console.WriteLine();

            // Create progress reporter
            var progressReporter = new ConsoleProgressReporter();

            // Create metadata
            var metadata = new MagazineMetadata
            {
                Title = displayTitle,
                IssueNumber = issue,
                PublicationDate = publicationDate,
                Periodicity = periodicity
            };

            // Process file
            var result = await processingService.ProcessMagazineAsync(
                filePath,
                metadata,
                progressReporter,
                cancellationToken);

            // Show summary
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║                    RIEPILOGO                              ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            if (result.Success)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("✅ Elaborazione completata con successo!");
                System.Console.ResetColor();
                System.Console.WriteLine();
                System.Console.WriteLine($"🆔 Magazine ID:         {result.MagazineId}");
                System.Console.WriteLine($"📄 Pagine elaborate:    {result.PagesProcessed}/{result.TotalPages}");
                System.Console.WriteLine($"🎮 Giochi estratti:     {result.GamesExtracted}");
                System.Console.WriteLine($"⏱️  Tempo impiegato:     {result.Duration:mm\\:ss}");

                if (result.PageResults.Any(p => !p.Success))
                {
                    var failedPages = result.PageResults.Count(p => !p.Success);
                    System.Console.WriteLine();
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine($"⚠️  {failedPages} pagine con errori");
                    System.Console.ResetColor();
                }
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("❌ Elaborazione fallita!");
                System.Console.WriteLine($"Errore: {result.ErrorMessage}");
                System.Console.ResetColor();
                Environment.Exit(1);
            }

            logger.LogInformation(
                "File processing completed: {Pages} pages, {Games} games",
                result.PagesProcessed, result.GamesExtracted);
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
            logger.LogError(ex, "Fatal error during file scan");
            Environment.Exit(1);
        }
    }
}
