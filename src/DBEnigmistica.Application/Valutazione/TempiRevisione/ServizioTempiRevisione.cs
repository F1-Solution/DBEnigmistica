namespace DBEnigmistica.Application.Valutazione.TempiRevisione;

/// <summary>Legge i candidati e produce il rapporto <c>review-time</c> con JSON e Markdown.</summary>
public sealed class ServizioTempiRevisione(TimeProvider? tempo = null)
{
    private readonly TimeProvider _tempo = tempo ?? TimeProvider.System;

    public async Task<RapportoTempiRevisione> EseguiAsync(ILettoreCandidatiRapporto lettore, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lettore);
        var candidati = await lettore.LeggiAsync(cancellationToken);
        return CalcolatoreTempiRevisione.Calcola(candidati, _tempo.GetUtcNow());
    }
}
