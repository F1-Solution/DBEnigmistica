using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class ListGamesCommand : Command
{
    public ListGamesCommand() : base("list-games", "Lista tutte le tipologie di giochi nel database")
    {
        var detailsOption = new Option<bool>(
            aliases: new[] { "--details", "-d" },
            description: "Mostra dettagli completi degli schemi");

        AddOption(detailsOption);

        this.SetHandler(async (InvocationContext context) =>
        {
            var showDetails = context.ParseResult.GetValueForOption(detailsOption);

            var services = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not available");
            
            var logger = services.GetRequiredService<ILogger<ListGamesCommand>>();
            var schemaRepository = services.GetRequiredService<IGameTypeSchemaRepository>();

            await ExecuteAsync(showDetails, schemaRepository, logger, context.GetCancellationToken());
        });
    }

    private static async Task ExecuteAsync(
        bool showDetails,
        IGameTypeSchemaRepository schemaRepository,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║         DBEnigmistica - Tipologie di Giochi              ║");
            System.Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            var schemas = await schemaRepository.GetAllAsync();
            var schemaList = schemas.ToList();

            if (!schemaList.Any())
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("⚠️  Nessuna tipologia di gioco trovata nel database.");
                System.Console.ResetColor();
                System.Console.WriteLine();
                System.Console.WriteLine("Suggerimento: Usa 'analyze-game' o 'analyze-game-text' per creare nuovi tipi.");
                return;
            }

            System.Console.WriteLine($"📊 Trovate {schemaList.Count} tipologie di giochi:");
            System.Console.WriteLine();

            foreach (var schema in schemaList.OrderBy(s => s.CreatedDate))
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Write("🎮 ");
                System.Console.ResetColor();
                System.Console.Write($"{schema.GameTypeName}");
                
                if (!schema.IsUserValidated)
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.Write(" [non validato]");
                    System.Console.ResetColor();
                }
                System.Console.WriteLine();

                System.Console.WriteLine($"   📋 Tabella: {schema.TableName}");
                System.Console.WriteLine($"   📅 Creata: {schema.CreatedDate:dd/MM/yyyy HH:mm}");

                if (!string.IsNullOrEmpty(schema.Description))
                {
                    System.Console.WriteLine($"   📝 Descrizione: {schema.Description}");
                }

                if (showDetails)
                {
                    // Parse schema JSON to show columns
                    try
                    {
                        var schemaJson = System.Text.Json.JsonSerializer.Deserialize<SchemaDetails>(schema.SchemaJson);
                        if (schemaJson?.Columns != null && schemaJson.Columns.Any())
                        {
                            System.Console.WriteLine($"   🔢 Colonne ({schemaJson.Columns.Count}):");
                            foreach (var column in schemaJson.Columns)
                            {
                                var required = column.IsRequired ? "[obbligatorio]" : "[opzionale]";
                                System.Console.WriteLine($"      • {column.ColumnName} ({column.DataType}) {required}");
                                if (!string.IsNullOrEmpty(column.Description))
                                {
                                    System.Console.ForegroundColor = ConsoleColor.DarkGray;
                                    System.Console.WriteLine($"        {column.Description}");
                                    System.Console.ResetColor();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.WriteLine($"   ⚠️  Errore nel parsing dello schema: {ex.Message}");
                        System.Console.ResetColor();
                    }
                }

                System.Console.WriteLine();
            }

            logger.LogInformation("Listed {Count} game type schemas", schemaList.Count);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ Errore: {ex.Message}");
            System.Console.ResetColor();
            logger.LogError(ex, "Error listing game types");
            Environment.Exit(1);
        }
    }

    // Helper class for deserializing schema JSON
    private class SchemaDetails
    {
        public string TableName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ColumnDetails> Columns { get; set; } = new();
    }

    private class ColumnDetails
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? Description { get; set; }
    }
}
