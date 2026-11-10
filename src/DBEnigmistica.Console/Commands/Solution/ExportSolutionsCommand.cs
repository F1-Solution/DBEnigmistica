using System.CommandLine;
using System.Globalization;
using System.Text.Json;
using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Console.Commands.Solution;

public class ExportSolutionsCommand : Command
{
    public ExportSolutionsCommand() : base("export-solutions", "Esporta soluzioni in JSON o CSV")
    {
        var outputOption = new Option<string>(
            "--output",
            "Percorso file di output (obbligatorio)"
        ) { IsRequired = true };

        var formatOption = new Option<string>(
            "--format",
            getDefaultValue: () => "json",
            "Formato di esportazione: json o csv (default: json)"
        );

        var verifiedOnlyOption = new Option<bool>(
            "--verified-only",
            getDefaultValue: () => false,
            "Esporta solo soluzioni verificate (default: false)"
        );

        var magazineIdOption = new Option<int?>(
            "--magazine-id",
            "ID della rivista da filtrare (opzionale)"
        );

        AddOption(outputOption);
        AddOption(formatOption);
        AddOption(verifiedOnlyOption);
        AddOption(magazineIdOption);

        this.SetHandler(async (outputPath, format, verifiedOnly, magazineId, sp) =>
        {
            var repository = sp.GetRequiredService<IGameSolutionRepository>();

            try
            {
                System.Console.WriteLine($"\n📤 Esportazione soluzioni...");
                
                // Validate format
                if (format.ToLower() != "json" && format.ToLower() != "csv")
                {
                    throw new InvalidOperationException("Formato non supportato. Usare 'json' o 'csv'.");
                }

                // Get solutions
                var solutionsList = verifiedOnly
                    ? (await repository.GetVerifiedSolutionsAsync(magazineId)).ToList()
                    : (await repository.GetAllAsync()).ToList();

                if (magazineId.HasValue && !verifiedOnly)
                {
                    // Filter by magazine - need to check associated game pages
                    solutionsList = solutionsList
                        .Where(s => s.Game?.Page?.MagazineId == magazineId.Value)
                        .ToList();
                }

                System.Console.WriteLine();

                if (!solutionsList.Any())
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine("⚠ Nessuna soluzione trovata con i criteri specificati.");
                    System.Console.ResetColor();
                    return;
                }

                if (format.ToLower() == "json")
                {
                    await ExportToJsonAsync(solutionsList, outputPath);
                }
                else if (format.ToLower() == "csv")
                {
                    await ExportToCsvAsync(solutionsList, outputPath);
                }

                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✓ Esportate {solutionsList.Count} soluzioni");
                System.Console.WriteLine($"✓ File salvato: {outputPath}");
                System.Console.ResetColor();
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: {ex.Message}");
                System.Console.ResetColor();
            }
        }, outputOption, formatOption, verifiedOnlyOption, magazineIdOption, 
           new ServiceProviderBinder());
    }

    private static async Task ExportToJsonAsync(List<DBEnigmistica.Domain.Entities.GameSolution> solutions, string outputPath)
    {
        var exportData = solutions.Select(s => new
        {
            s.Id,
            GameId = s.GameId,
            s.SolutionText,
            SolutionPageId = s.SolutionPageId,
            SolutionType = s.SolutionType.ToString(),
            s.ConfidenceScore,
            s.IsVerified,
            s.PositionX,
            s.PositionY,
            ExtractedDate = s.ExtractedDate.ToString("O"),
            VerifiedDate = s.VerifiedDate?.ToString("O"),
            s.Notes
        }).ToList();

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await System.IO.File.WriteAllTextAsync(outputPath, json);
    }

    private static async Task ExportToCsvAsync(List<DBEnigmistica.Domain.Entities.GameSolution> solutions, string outputPath)
    {
        using var writer = new StreamWriter(outputPath);
        
        // Write header
        await writer.WriteLineAsync("ID,GameID,SolutionText,SolutionPageID,SolutionType,ConfidenceScore,IsVerified,PositionX,PositionY,ExtractedDate,VerifiedDate,Notes");

        // Write data
        foreach (var solution in solutions)
        {
            var line = $"\"{solution.Id}\"," +
                       $"\"{solution.GameId}\"," +
                       $"\"{EscapeCsvValue(solution.SolutionText)}\"," +
                       $"\"{solution.SolutionPageId}\"," +
                       $"\"{solution.SolutionType}\"," +
                       $"\"{solution.ConfidenceScore?.ToString(CultureInfo.InvariantCulture) ?? ""}\"," +
                       $"\"{solution.IsVerified}\"," +
                       $"\"{solution.PositionX}\"," +
                       $"\"{solution.PositionY}\"," +
                       $"\"{solution.ExtractedDate:O}\"," +
                       $"\"{solution.VerifiedDate?.ToString("O") ?? ""}\"," +
                       $"\"{EscapeCsvValue(solution.Notes ?? "")}\"";
            
            await writer.WriteLineAsync(line);
        }

        await writer.FlushAsync();
    }

    private static string EscapeCsvValue(string value)
    {
        return CsvFieldFormatter.EscapeQuotes(value);
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
