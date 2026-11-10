using System.CommandLine;
using System.Text.Json;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands;

public class ExportSchemaCommand : Command
{
    public ExportSchemaCommand(CommandRunner runner) : base("export-schema", "Esporta gli schemi di gioco in formato JSON")
    {
        var outputOption = new Option<string?>("--output", "-o") { Description = "Percorso del file di output (default: schemas.json)" };
        var prettyOption = new Option<bool>("--pretty", "-p")
        {
            Description = "Formattazione JSON leggibile",
            DefaultValueFactory = _ => true
        };

        Options.Add(outputOption);
        Options.Add(prettyOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<ExportSchemaHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetValue(outputOption),
                parseResult.GetValue(prettyOption),
                token),
            ct));
    }
}

public sealed class ExportSchemaHandler(
    IGameTypeSchemaRepository schemaRepository,
    IAnsiConsole console,
    ILogger<ExportSchemaHandler> logger)
{
    public async Task<int> ExecuteAsync(string? outputPath, bool prettyPrint, CancellationToken cancellationToken)
    {
        ConsoleBanner.Write(console, "Esportazione schemi");
        outputPath ??= "schemas.json";

        var schemas = (await schemaRepository.GetAllAsync()).ToList();
        if (schemas.Count == 0)
        {
            ConsoleBanner.Warning(console, "Nessuno schema da esportare.");
            return ExitCodes.Success;
        }

        var exportData = new
        {
            ExportDate = DateTime.UtcNow,
            Version = "1.0",
            TotalSchemas = schemas.Count,
            Schemas = schemas.Select(s => new
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

        var options = new JsonSerializerOptions
        {
            WriteIndented = prettyPrint,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(exportData, options), cancellationToken);

        ConsoleBanner.Success(console, $"Esportati {schemas.Count} schemi in: {Path.GetFullPath(outputPath)}");
        console.MarkupLineInterpolated($"Dimensione file: {new FileInfo(outputPath).Length:N0} byte");

        logger.LogInformation("Exported {Count} schemas to {Path}", schemas.Count, outputPath);
        return ExitCodes.Success;
    }
}
