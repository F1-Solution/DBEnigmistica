using System.CommandLine;
using System.Text.Json;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Presentation;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace DBEnigmistica.Console.Commands;

public class ListGamesCommand : Command
{
    public ListGamesCommand(CommandRunner runner) : base("list-games", "Lista tutte le tipologie di giochi nel database")
    {
        var detailsOption = new Option<bool>("--details", "-d") { Description = "Mostra dettagli completi degli schemi" };
        Options.Add(detailsOption);

        this.SetAction((parseResult, ct) => runner.RunAsync<ListGamesHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(parseResult.GetValue(detailsOption), token),
            ct));
    }
}

public sealed class ListGamesHandler(
    IGameTypeSchemaRepository schemaRepository,
    IAnsiConsole console,
    ILogger<ListGamesHandler> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<int> ExecuteAsync(bool showDetails, CancellationToken cancellationToken)
    {
        ConsoleBanner.Write(console, "Tipologie di giochi");

        var schemas = (await schemaRepository.GetAllAsync()).OrderBy(s => s.CreatedDate).ToList();
        if (schemas.Count == 0)
        {
            ConsoleBanner.Warning(console, "Nessuna tipologia di gioco trovata nel database.");
            return ExitCodes.Success;
        }

        console.MarkupLineInterpolated($"Trovate {schemas.Count} tipologie di giochi:");
        console.WriteLine();

        foreach (var schema in schemas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteSchema(schema, showDetails);
        }

        logger.LogInformation("Listed {Count} game type schemas", schemas.Count);
        return ExitCodes.Success;
    }

    private void WriteSchema(GameTypeSchema schema, bool showDetails)
    {
        var validation = schema.IsUserValidated ? "" : " [yellow][[non validato]][/]";
        console.MarkupLine($"[green]{Markup.Escape(schema.GameTypeName)}[/]{validation}");
        console.MarkupLineInterpolated($"   Tabella: {schema.TableName}");
        console.MarkupLineInterpolated($"   Creata: {schema.CreatedDate:dd/MM/yyyy HH:mm}");

        if (!string.IsNullOrEmpty(schema.Description))
            console.MarkupLineInterpolated($"   Descrizione: {schema.Description}");

        if (showDetails)
            WriteColumns(schema.SchemaJson);

        console.WriteLine();
    }

    private void WriteColumns(string schemaJson)
    {
        SchemaDetails? details;
        try
        {
            details = JsonSerializer.Deserialize<SchemaDetails>(schemaJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Schema JSON non valido");
            console.MarkupLineInterpolated($"[red]   Errore nel parsing dello schema: {ex.Message}[/]");
            return;
        }

        if (details?.Columns is not { Count: > 0 })
            return;

        console.MarkupLineInterpolated($"   Colonne ({details.Columns.Count}):");
        foreach (var column in details.Columns)
        {
            var required = column.IsRequired ? "obbligatorio" : "opzionale";
            console.MarkupLineInterpolated($"      - {column.ColumnName} ({column.DataType}) [[{required}]]");
            if (!string.IsNullOrEmpty(column.Description))
                console.MarkupLineInterpolated($"[grey]        {column.Description}[/]");
        }
    }

    private sealed class SchemaDetails
    {
        public List<ColumnDetails> Columns { get; set; } = [];
    }

    private sealed class ColumnDetails
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? Description { get; set; }
    }
}
