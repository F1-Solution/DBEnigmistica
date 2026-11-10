using System.CommandLine;
using System.Text.Json;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Console.Commands.Solution;

public class ListUnsolvedCommand : Command
{
    public ListUnsolvedCommand() : base("list-unsolved", "Elenca i giochi senza soluzione")
    {
        var magazineIdOption = new Option<int?>(
            "--magazine-id",
            "ID della rivista da filtrare (opzionale)"
        );

        var gameTypeOption = new Option<GameType?>(
            "--game-type",
            "Tipo di gioco da filtrare (opzionale)"
        );

        var exportOption = new Option<string?>(
            "--export",
            "Percorso file per esportare in JSON (opzionale)"
        );

        AddOption(magazineIdOption);
        AddOption(gameTypeOption);
        AddOption(exportOption);

        this.SetHandler(async (magazineId, gameType, exportPath, sp) =>
        {
            var repository = sp.GetRequiredService<IGameSolutionRepository>();

            try
            {
                System.Console.WriteLine($"\n🔍 Ricerca giochi senza soluzione...");
                
                var unsolvedGames = await repository.GetUnsolvedGamesAsync(magazineId);

                // Filter by game type if specified
                var filteredGames = gameType.HasValue 
                    ? unsolvedGames.Where(g => g.GameType == gameType.Value).ToList()
                    : unsolvedGames.ToList();

                System.Console.WriteLine();

                if (!filteredGames.Any())
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine("⚠ Nessun gioco senza soluzione trovato.");
                    System.Console.ResetColor();
                    return;
                }

                // Display table
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("╔════════╦════════════════════╦══════════════╦═══════════╦════════╗");
                System.Console.WriteLine("║   ID   ║      Titolo        ║     Tipo     ║  Rivista  ║ Pagina ║");
                System.Console.WriteLine("╠════════╬════════════════════╬══════════════╬═══════════╬════════╣");
                System.Console.ResetColor();

                foreach (var game in filteredGames)
                {
                    var title = (game.Title ?? "N/A").PadRight(18).Substring(0, 18);
                    var type = game.GameType.ToString().PadRight(12).Substring(0, 12);
                    var magazineIdDisplay = game.PageId.ToString().PadRight(9).Substring(0, 9);
                    System.Console.WriteLine($"║ {game.Id,6} ║ {title} ║ {type} ║ {magazineIdDisplay} ║ {game.PageId,6} ║");
                }

                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("╚════════╩════════════════════╩══════════════╩═══════════╩════════╝");
                System.Console.ResetColor();

                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✓ Trovati {filteredGames.Count} giochi senza soluzione");
                System.Console.ResetColor();

                // Export to JSON if requested
                if (!string.IsNullOrWhiteSpace(exportPath))
                {
                    var exportData = filteredGames.Select(g => new
                    {
                        g.Id,
                        g.Title,
                        Type = g.GameType.ToString(),
                        g.PageId,
                        g.PositionX,
                        g.PositionY
                    }).ToList();

                    var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                    
                    await System.IO.File.WriteAllTextAsync(exportPath, json);

                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine($"✓ Dati esportati in: {exportPath}");
                    System.Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: {ex.Message}");
                System.Console.ResetColor();
            }
        }, magazineIdOption, gameTypeOption, exportOption, 
           new ServiceProviderBinder());
    }

    private class ServiceProviderBinder : System.CommandLine.Binding.BinderBase<IServiceProvider>
    {
        protected override IServiceProvider GetBoundValue(System.CommandLine.Binding.BindingContext bindingContext)
        {
            return bindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not found");
        }
    }
}
