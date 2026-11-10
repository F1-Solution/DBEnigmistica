using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class MigrateCommand : Command
{
    public MigrateCommand() : base("migrate", "Applica le migrazioni pending al database")
    {
        this.SetHandler(async (InvocationContext context) =>
        {
            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not available");
            
            var logger = services.GetRequiredService<ILogger<MigrateCommand>>();
            var dbContext = services.GetRequiredService<EnigmisticaDbContext>();

            await ExecuteAsync(dbContext, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        EnigmisticaDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║          DBEnigmistica - Migrazioni Database             ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            System.Console.WriteLine("🔍 Verifica migrazioni pending...");
            
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingList = pendingMigrations.ToList();

            if (!pendingList.Any())
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("✅ Database già aggiornato. Nessuna migrazione da applicare.");
                System.Console.ResetColor();
                
                var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
                var appliedList = appliedMigrations.ToList();
                
                if (appliedList.Any())
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine($"📋 Migrazioni applicate: {appliedList.Count}");
                    System.Console.WriteLine($"   Ultima: {appliedList.Last()}");
                }
                
                return;
            }

            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"⚠️  Trovate {pendingList.Count} migrazioni da applicare:");
            System.Console.ResetColor();
            
            foreach (var migration in pendingList)
            {
                System.Console.WriteLine($"   • {migration}");
            }
            
            System.Console.WriteLine();
            System.Console.Write("Procedere con l'applicazione? [s/N]: ");
            
            var response = System.Console.ReadLine()?.Trim().ToLowerInvariant();
            
            if (response != "s" && response != "sì" && response != "si" && response != "y" && response != "yes")
            {
                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("⚠️  Operazione annullata dall'utente.");
                System.Console.ResetColor();
                return;
            }

            System.Console.WriteLine();
            System.Console.WriteLine("⏳ Applicazione migrazioni in corso...");

            await dbContext.Database.MigrateAsync(cancellationToken);

            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║                  ✅ SUCCESSO                               ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();
            System.Console.WriteLine($"✅ {pendingList.Count} migrazioni applicate con successo!");
            System.Console.WriteLine();

            logger.LogInformation("Applied {Count} migrations successfully", pendingList.Count);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║                   ❌ ERRORE                                ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();
            System.Console.WriteLine($"✗ Errore durante l'applicazione delle migrazioni:");
            System.Console.WriteLine($"  {ex.Message}");
            System.Console.WriteLine();
            
            if (ex.InnerException != null)
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.WriteLine("Dettagli tecnici:");
                System.Console.WriteLine($"  {ex.InnerException.Message}");
                System.Console.ResetColor();
                System.Console.WriteLine();
            }

            logger.LogError(ex, "Error applying migrations");
            Environment.Exit(1);
        }
    }
}
