using DBEnigmistica.Application.Services.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class AnalyzeGameTextCommand : Command
{
    public AnalyzeGameTextCommand() : base("analyze-game-text", "Analizza un nuovo tipo di gioco da descrizione testuale e crea lo schema")
    {
        var descriptionArgument = new Argument<string>(
            name: "description",
            description: "Descrizione testuale del tipo di gioco");

        var nameOption = new Option<string?>(
            aliases: new[] { "--name", "-n" },
            description: "Nome del tipo di gioco (opzionale)");

        AddArgument(descriptionArgument);
        AddOption(nameOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var description = context.ParseResult.GetValueForArgument(descriptionArgument);
            var name = context.ParseResult.GetValueForOption(nameOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not available");
            
            var logger = services.GetRequiredService<ILogger<AnalyzeGameTextCommand>>();
            var orchestrator = services.GetRequiredService<DynamicSchemaOrchestrator>();

            await ExecuteAsync(description, name, orchestrator, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        string description,
        string? gameName,
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

            if (string.IsNullOrWhiteSpace(description))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: La descrizione non può essere vuota.");
                System.Console.ResetColor();
                Environment.Exit(1);
                return;
            }

            if (!string.IsNullOrEmpty(gameName))
                System.Console.WriteLine($"🎮 Nome gioco: {gameName}");
            
            System.Console.WriteLine($"📝 Descrizione: {description}");
            System.Console.WriteLine();
            System.Console.WriteLine("🤖 Generazione schema con AI...");
            System.Console.WriteLine();

            // Create schema from description
            var success = await orchestrator.CreateNewGameTypeSchemaAsync(
                gameName ?? "custom_game",
                description,
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

                logger.LogInformation("Game schema created successfully from description");
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
                System.Console.WriteLine("║                   ❌ FALLITO                               ║");
                System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
                System.Console.ResetColor();
                System.Console.WriteLine();

                logger.LogError("Failed to create game schema from description");
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
