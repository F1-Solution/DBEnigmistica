using DBEnigmistica.Application.Services.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class AnalyzeGameCommand : Command
{
    public AnalyzeGameCommand() : base("analyze-game", "Analizza un nuovo tipo di gioco da file Markdown e crea lo schema")
    {
        var fileArgument = new Argument<string>(
            name: "file",
            description: "Percorso del file Markdown contenente la descrizione del gioco");

        var autoApproveOption = new Option<bool>(
            aliases: new[] { "--yes", "-y" },
            description: "Approva automaticamente lo schema proposto senza conferma");

        AddArgument(fileArgument);
        AddOption(autoApproveOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var file = context.ParseResult.GetValueForArgument(fileArgument);
            var autoApprove = context.ParseResult.GetValueForOption(autoApproveOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not available");
            
            var logger = services.GetRequiredService<ILogger<AnalyzeGameCommand>>();
            var orchestrator = services.GetRequiredService<DynamicSchemaOrchestrator>();

            await ExecuteAsync(file, autoApprove, orchestrator, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        string filePath,
        bool autoApprove,
        DynamicSchemaOrchestrator orchestrator,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║       DBEnigmistica - Analisi Nuovo Tipo di Gioco        ║");
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

            System.Console.WriteLine($"📄 File: {Path.GetFileName(filePath)}");
            System.Console.WriteLine();

            // Read markdown content
            var markdownContent = await File.ReadAllTextAsync(filePath, cancellationToken);

            if (string.IsNullOrWhiteSpace(markdownContent))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: Il file è vuoto.");
                System.Console.ResetColor();
                Environment.Exit(1);
                return;
            }

            System.Console.WriteLine("🤖 Analisi del contenuto con AI...");
            System.Console.WriteLine();

            // Create schema from markdown
            var success = await orchestrator.CreateSchemaFromMarkdownAsync(
                filePath,
                cancellationToken);

            System.Console.WriteLine();

            if (success)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                System.Console.WriteLine("║                  ✅ SUCCESSO                               ║");
                System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                System.Console.ResetColor();
                System.Console.WriteLine();
                System.Console.WriteLine("📊 Schema creato con successo!");
                System.Console.WriteLine();

                logger.LogInformation("Game schema created successfully from markdown");
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                System.Console.WriteLine("║                   ❌ FALLITO                               ║");
                System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                System.Console.ResetColor();
                System.Console.WriteLine();

                logger.LogError("Failed to create game schema from markdown");
                Environment.Exit(1);
            }
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
            logger.LogError(ex, "Fatal error during game analysis");
            Environment.Exit(1);
        }
    }
}
