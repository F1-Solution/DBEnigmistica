using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Application.Services.Solutions;

/// <summary>
/// Collegamento manuale e verifica delle soluzioni dei Giochi.
/// </summary>
public interface IGameSolutionService
{
    /// <summary>Associa una soluzione a un Gioco; la soluzione nasce già verificata.</summary>
    /// <exception cref="KeyNotFoundException">Il Gioco non esiste.</exception>
    Task<GameSolution> LinkAsync(
        int gameId,
        string solutionText,
        int? pageId,
        SolutionType solutionType,
        CancellationToken cancellationToken = default);

    /// <summary>Approva o rifiuta una soluzione esistente.</summary>
    /// <exception cref="KeyNotFoundException">La soluzione non esiste.</exception>
    Task<GameSolution> VerifyAsync(
        int solutionId,
        bool verified,
        string? notes,
        CancellationToken cancellationToken = default);
}
