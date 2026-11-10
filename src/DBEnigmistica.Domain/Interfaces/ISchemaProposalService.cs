using DBEnigmistica.Domain.AI;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per generare proposte di schema da parte dell'AI
/// </summary>
public interface ISchemaProposalService
{
    /// <summary>
    /// Genera una proposta di schema per un nuovo tipo di gioco
    /// </summary>
    Task<GameTableSchema> GenerateSchemaProposalAsync(
        string gameTypeName,
        string gameDescription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera una proposta di schema analizzando un file markdown
    /// </summary>
    Task<GameTableSchema> GenerateSchemaFromMarkdownAsync(
        string markdownFilePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera una proposta di schema analizzando testo markdown
    /// </summary>
    Task<GameTableSchema> GenerateSchemaFromMarkdownContentAsync(
        string markdownContent,
        CancellationToken cancellationToken = default);
}
