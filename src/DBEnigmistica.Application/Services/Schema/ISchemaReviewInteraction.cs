using DBEnigmistica.Domain.AI;

namespace DBEnigmistica.Application.Services.Schema;

/// <summary>
/// Dialogo con l'utente per la revisione di uno schema; la resa a schermo sta nel progetto dell'interfaccia.
/// </summary>
public interface ISchemaReviewInteraction
{
    void ShowProposal(GameTableSchema schema);

    /// <summary>Chiede se approvare lo schema; restituisce il testo digitato, o null a fine input.</summary>
    string? AskApproval();

    void ShowEditHelp();

    /// <summary>Chiede un comando di modifica; restituisce il testo digitato, o null a fine input.</summary>
    string? AskEditCommand();

    void ShowEditResult(string message);
}
