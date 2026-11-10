using System.CommandLine;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Console.Commands.Solution;

public class ScanSolutionsCommand : Command
{
    public ScanSolutionsCommand() : base("scan-solutions", "Scansiona una pagina per estrarre soluzioni")
    {
        var pageIdOption = new Option<int>(
            "--page-id",
            "ID della pagina da scansionare"
        ) { IsRequired = true };

        AddOption(pageIdOption);

        this.SetHandler(async (pageId, sp) =>
        {
            var extractionService = sp.GetRequiredService<ISolutionExtractionService>();

            try
            {
                System.Console.WriteLine($"\n🔎 Scansione della pagina #{pageId} per estrarre soluzioni...\n");
                
                var extractedSolutions = (await extractionService.ExtractSolutionsFromPageAsync(pageId)).ToList();

                if (!extractedSolutions.Any())
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine("⚠ Nessuna soluzione estratta da questa pagina.");
                    System.Console.ResetColor();
                    return;
                }

                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("╔════╦════════════════════════╦════════════╦════════════╗");
                System.Console.WriteLine("║ N° ║     Testo Soluzione    ║    Tipo    ║ Confidenza ║");
                System.Console.WriteLine("╠════╬════════════════════════╬════════════╬════════════╣");
                System.Console.ResetColor();

                int index = 1;
                foreach (var solution in extractedSolutions)
                {
                    var text = (solution.SolutionText ?? "N/A").PadRight(22).Substring(0, 22);
                    var type = (solution.GameType?.ToString() ?? "Unknown").PadRight(10).Substring(0, 10);
                    var confidence = $"{solution.ConfidenceScore * 100:F1}%".PadRight(10).Substring(0, 10);

                    System.Console.WriteLine($"║ {index,2} ║ {text} ║ {type} ║ {confidence} ║");

                    // Print additional details
                    if (!string.IsNullOrWhiteSpace(solution.GameReference))
                    {
                        System.Console.WriteLine($"     Riferimento: {solution.GameReference}");
                    }
                    
                    if (solution.PositionX.HasValue && solution.PositionY.HasValue)
                    {
                        System.Console.WriteLine($"     Posizione: ({solution.PositionX}, {solution.PositionY})");
                    }

                    index++;
                }

                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("╚════╩════════════════════════╩════════════╩════════════╝");
                System.Console.ResetColor();

                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✓ Estratte {extractedSolutions.Count} soluzioni");
                System.Console.ResetColor();
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: {ex.Message}");
                System.Console.ResetColor();
            }
        }, pageIdOption, new ServiceProviderBinder());
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
