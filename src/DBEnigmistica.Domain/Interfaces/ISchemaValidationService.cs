using DBEnigmistica.Domain.AI;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per validare schemi con l'utente
/// </summary>
public interface ISchemaValidationService
{
    /// <summary>
    /// Mostra uno schema all'utente e chiede conferma/modifiche
    /// </summary>
    Task<(bool Approved, GameTableSchema? ModifiedSchema)> ValidateSchemaWithUserAsync(
        GameTableSchema proposedSchema,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida che lo schema sia corretto e non generi conflitti
    /// </summary>
    Task<(bool IsValid, List<string> Errors)> ValidateSchemaConsistencyAsync(
        GameTableSchema schema,
        CancellationToken cancellationToken = default);
}
