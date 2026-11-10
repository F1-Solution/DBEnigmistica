using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DBEnigmistica.Application.Services.Schema;

/// <summary>
/// Servizio orchestratore per la gestione completa dello schema dinamico
/// </summary>
public class DynamicSchemaOrchestrator
{
    private readonly ILogger<DynamicSchemaOrchestrator> _logger;
    private readonly ISchemaProposalService _proposalService;
    private readonly ISchemaValidationService _validationService;
    private readonly IDynamicMigrationService _migrationService;
    private readonly IGameTypeSchemaRepository _schemaRepository;

    public DynamicSchemaOrchestrator(
        ILogger<DynamicSchemaOrchestrator> logger,
        ISchemaProposalService proposalService,
        ISchemaValidationService validationService,
        IDynamicMigrationService migrationService,
        IGameTypeSchemaRepository schemaRepository)
    {
        _logger = logger;
        _proposalService = proposalService;
        _validationService = validationService;
        _migrationService = migrationService;
        _schemaRepository = schemaRepository;
    }

    /// <summary>
    /// Workflow completo: proposta → validazione → creazione → salvataggio metadata
    /// </summary>
    public async Task<bool> CreateNewGameTypeSchemaAsync(
        string gameTypeName,
        string gameDescription,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting schema creation workflow for: {GameTypeName}", gameTypeName);

        try
        {
            // 1. Verifica se il tipo esiste già
            var existing = await _schemaRepository.GetByGameTypeNameAsync(gameTypeName);
            if (existing != null)
            {
                _logger.LogWarning("Game type schema already exists: {GameTypeName}", gameTypeName);
                Console.WriteLine($"⚠ Lo schema per '{gameTypeName}' esiste già.");
                return false;
            }

            // 2. Genera proposta schema con AI
            Console.WriteLine($"🤖 Generazione schema per '{gameTypeName}' in corso...");
            var proposedSchema = await _proposalService.GenerateSchemaProposalAsync(
                gameTypeName,
                gameDescription,
                cancellationToken);

            // 3. Valida consistenza dello schema
            var (isValid, errors) = await _validationService.ValidateSchemaConsistencyAsync(
                proposedSchema,
                cancellationToken);

            if (!isValid)
            {
                _logger.LogError("Schema validation failed: {Errors}", string.Join(", ", errors));
                Console.WriteLine("❌ Schema non valido:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"   • {error}");
                }
                return false;
            }

            // 4. Validazione con utente
            var (approved, modifiedSchema) = await _validationService.ValidateSchemaWithUserAsync(
                proposedSchema,
                cancellationToken);

            if (!approved || modifiedSchema == null)
            {
                _logger.LogInformation("Schema rejected by user");
                Console.WriteLine("❌ Creazione schema annullata.");
                return false;
            }

            // 5. Crea la tabella nel database
            Console.WriteLine("📊 Creazione tabella nel database...");
            var created = await _migrationService.CreateTableAsync(modifiedSchema, cancellationToken);

            if (!created)
            {
                Console.WriteLine("❌ Errore durante la creazione della tabella.");
                return false;
            }

            // 6. Salva i metadata dello schema
            var schemaEntity = new GameTypeSchema
            {
                GameTypeName = modifiedSchema.GameTypeName,
                TableName = modifiedSchema.TableName,
                SchemaJson = JsonSerializer.Serialize(modifiedSchema, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                }),
                CreatedDate = DateTime.UtcNow,
                Version = 1,
                Description = modifiedSchema.Description,
                IsUserValidated = true
            };

            await _schemaRepository.AddAsync(schemaEntity);

            _logger.LogInformation("Schema created and saved successfully: {TableName}", modifiedSchema.TableName);
            Console.WriteLine($"✅ Schema creato con successo: {modifiedSchema.TableName}");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create schema for: {GameTypeName}", gameTypeName);
            Console.WriteLine($"❌ Errore: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Crea schema analizzando un file markdown
    /// </summary>
    public async Task<bool> CreateSchemaFromMarkdownAsync(
        string markdownFilePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating schema from markdown file: {FilePath}", markdownFilePath);

        try
        {
            // 1. Genera schema da markdown
            Console.WriteLine($"📄 Analisi file markdown: {Path.GetFileName(markdownFilePath)}");
            var proposedSchema = await _proposalService.GenerateSchemaFromMarkdownAsync(
                markdownFilePath,
                cancellationToken);

            // 2. Verifica se esiste già
            var existing = await _schemaRepository.GetByGameTypeNameAsync(proposedSchema.GameTypeName);
            if (existing != null)
            {
                Console.WriteLine($"⚠ Lo schema per '{proposedSchema.GameTypeName}' esiste già.");
                return false;
            }

            // 3. Valida e procedi con il workflow normale
            return await ContinueSchemaCreationWorkflowAsync(proposedSchema, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create schema from markdown: {FilePath}", markdownFilePath);
            Console.WriteLine($"❌ Errore: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ottiene tutti gli schemi esistenti
    /// </summary>
    public async Task<IEnumerable<GameTypeSchema>> GetAllSchemasAsync()
    {
        return await _schemaRepository.GetAllAsync();
    }

    /// <summary>
    /// Verifica se uno schema esiste per un tipo di gioco
    /// </summary>
    public async Task<bool> SchemaExistsForGameTypeAsync(string gameTypeName)
    {
        var schema = await _schemaRepository.GetByGameTypeNameAsync(gameTypeName);
        return schema != null;
    }

    private async Task<bool> ContinueSchemaCreationWorkflowAsync(
        GameTableSchema proposedSchema,
        CancellationToken cancellationToken)
    {
        // Valida consistenza
        var (isValid, errors) = await _validationService.ValidateSchemaConsistencyAsync(
            proposedSchema,
            cancellationToken);

        if (!isValid)
        {
            Console.WriteLine("❌ Schema non valido:");
            foreach (var error in errors)
            {
                Console.WriteLine($"   • {error}");
            }
            return false;
        }

        // Validazione utente
        var (approved, modifiedSchema) = await _validationService.ValidateSchemaWithUserAsync(
            proposedSchema,
            cancellationToken);

        if (!approved || modifiedSchema == null)
        {
            Console.WriteLine("❌ Creazione schema annullata.");
            return false;
        }

        // Crea tabella
        Console.WriteLine("📊 Creazione tabella nel database...");
        var created = await _migrationService.CreateTableAsync(modifiedSchema, cancellationToken);

        if (!created)
        {
            Console.WriteLine("❌ Errore durante la creazione della tabella.");
            return false;
        }

        // Salva metadata
        var schemaEntity = new GameTypeSchema
        {
            GameTypeName = modifiedSchema.GameTypeName,
            TableName = modifiedSchema.TableName,
            SchemaJson = JsonSerializer.Serialize(modifiedSchema, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            }),
            CreatedDate = DateTime.UtcNow,
            Version = 1,
            Description = modifiedSchema.Description,
            IsUserValidated = true
        };

        await _schemaRepository.AddAsync(schemaEntity);

        Console.WriteLine($"✅ Schema creato con successo: {modifiedSchema.TableName}");
        return true;
    }
}
