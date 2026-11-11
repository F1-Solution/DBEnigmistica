namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Traccia gli schemi di tabelle create dinamicamente per nuovi tipi di gioco
/// </summary>
public class GameTypeSchema
{
    public int Id { get; set; }

    /// <summary>
    /// Nome del tipo di gioco
    /// </summary>
    public required string GameTypeName { get; set; }

    /// <summary>
    /// Nome della tabella nel database
    /// </summary>
    public required string TableName { get; set; }

    /// <summary>
    /// Schema JSON delle colonne
    /// Format: [{"name": "ColumnName", "type": "string", "nullable": true, "description": "..."}, ...]
    /// </summary>
    public required string SchemaJson { get; set; }

    /// <summary>
    /// Data di creazione dello schema
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Versione dello schema (per gestire evoluzioni future)
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Descrizione del tipo di gioco
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indica se lo schema è stato validato dall'utente
    /// </summary>
    public bool IsUserValidated { get; set; }

    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Notes { get; set; }
}
