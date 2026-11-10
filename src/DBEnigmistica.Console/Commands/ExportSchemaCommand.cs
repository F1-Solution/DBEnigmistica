using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace DBEnigmistica.Console.Commands;

public class ExportSchemaCommand : Command
{
    public ExportSchemaCommand() : base("export-schema", "Esporta gli schemi di gioco in formato JSON")
    {
        var outputOption = new Option<string?>(
            aliases: new[] { "--output", "-o" },
            description: "Percorso del file di output (default: schemas.json)");

        var prettyOption = new Option<bool>(
            aliases: new[] { "--pretty", "-p" },
            getDefaultValue: () => true,
            description: "Formattazione JSON leggibile");

        AddOption(outputOption);
        AddOption(prettyOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var output = context.ParseResult.GetValueForOption(outputOption);
            var pretty = context.ParseResult.GetValueForOption(prettyOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not available");
            
            var logger = services.GetRequiredService<ILogger<ExportSchemaCommand>>();
            var schemaRepository = services.GetRequiredService<IGameTypeSchemaRepository>();

            await ExecuteAsync(output, pretty, schemaRepository, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        string? outputPath,
        bool prettyPrint,
        IGameTypeSchemaRepository schemaRepository,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║          DBEnigmistica - Esportazione Schemi             ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            outputPath ??= "schemas.json";

            System.Console.WriteLine("🔍 Recupero schemi dal database...");
            
            var schemas = await schemaRepository.GetAllAsync();
            var schemaList = schemas.ToList();

            if (!schemaList.Any())
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("⚠️  Nessuno schema da esportare.");
                System.Console.ResetColor();
                return;
            }

            System.Console.WriteLine($"📊 Trovati {schemaList.Count} schemi");
            System.Console.WriteLine();

            // Prepare export data
            var exportData = new
            {
                ExportDate = DateTime.UtcNow,
                Version = "1.0",
                TotalSchemas = schemaList.Count,
                Schemas = schemaList.Select(s => new
                {
                    s.Id,
                    s.GameTypeName,
                    s.TableName,
                    s.Description,
                    Schema = JsonSerializer.Deserialize<JsonElement>(s.SchemaJson),
                    s.IsUserValidated,
                    s.CreatedDate,
                    s.Version,
                    s.Notes
                }).OrderBy(s => s.GameTypeName)
            };

            // Serialize to JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = prettyPrint,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(exportData, options);

            // Write to file
            await File.WriteAllTextAsync(outputPath, json, cancellationToken);

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║                  ✅ SUCCESSO                               ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();
            System.Console.WriteLine($"✅ Esportati {schemaList.Count} schemi in: {Path.GetFullPath(outputPath)}");
            
            var fileInfo = new FileInfo(outputPath);
            System.Console.WriteLine($"📦 Dimensione file: {fileInfo.Length:N0} bytes");
            System.Console.WriteLine();

            logger.LogInformation("Exported {Count} schemas to {Path}", schemaList.Count, outputPath);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Errore durante l'esportazione: {ex.Message}");
            System.Console.ResetColor();
            logger.LogError(ex, "Error exporting schemas");
            Environment.Exit(1);
        }
    }
}
