using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Prototipo;

/// <summary>Candidati e chiamate di creazione nello schema <c>prototipo</c>; le letture non tracciano, così rileggono sempre il database.</summary>
public sealed class CandidatiRepository(EnigmisticaDbContext context) : ICandidatiRepository
{
    /// <summary>Chiave del lucchetto che serializza l'assegnazione dell'ordine di generazione.</summary>
    internal const long ChiaveLucchetto = 4419002;

    private static readonly string[] StatiInRevisione = [StatoCandidato.InRevisione, StatoCandidato.DaRivedere];

    public async Task<Candidato> AggiungiAsync(Candidato candidato, CancellationToken cancellationToken = default)
    {
        var propria = context.Database.CurrentTransaction is null ? await context.Database.BeginTransactionAsync(cancellationToken) : null;
        try
        {
            await context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({ChiaveLucchetto})", cancellationToken);
            var massimo = await context.Candidati.AsNoTracking()
                .Where(c => c.Tipologia == candidato.Tipologia)
                .MaxAsync(c => (int?)c.OrdineGenerazione, cancellationToken) ?? 0;
            var nuovo = candidato with { OrdineGenerazione = massimo + 1 };
            context.Candidati.Add(nuovo);
            await context.SaveChangesAsync(cancellationToken);
            if (propria is not null)
                await propria.CommitAsync(cancellationToken);
            return nuovo;
        }
        catch
        {
            context.ChangeTracker.Clear();
            throw;
        }
        finally
        {
            if (propria is not null)
                await propria.DisposeAsync();
        }
    }

    public async Task BloccaGenerazioneAsync(CancellationToken cancellationToken = default) =>
        await context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({ChiaveLucchetto})", cancellationToken);

    public async Task BloccaAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Database.ExecuteSqlAsync($"SELECT 1 FROM prototipo.candidati WHERE id = {id} FOR UPDATE", cancellationToken);

    public void ScartaModifiche() => context.ChangeTracker.Clear();

    public async Task AggiornaAsync(Candidato candidato, CancellationToken cancellationToken = default)
    {
        var esistente = await context.Candidati.FindAsync([candidato.Id], cancellationToken)
                        ?? throw new InvalidOperationException("Candidato da aggiornare non trovato.");
        context.Entry(esistente).CurrentValues.SetValues(candidato);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<Candidato?> TrovaAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Candidati.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Candidato?> ProssimoDaRivedereAsync(string tipologia, CancellationToken cancellationToken = default) =>
        context.Candidati.AsNoTracking()
            .Where(c => c.Tipologia == tipologia && StatiInRevisione.Contains(c.Stato))
            .OrderBy(c => c.OrdineGenerazione)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> BriefDellaTipologiaAsync(string tipologia, CancellationToken cancellationToken = default) =>
        await context.Candidati.AsNoTracking().Where(c => c.Tipologia == tipologia).Select(c => c.Brief).ToListAsync(cancellationToken);

    public async Task AggiungiChiamateAsync(Guid candidatoId, IReadOnlyList<ChiamataAi> chiamate, CancellationToken cancellationToken = default)
    {
        context.ChiamateAiPrototipo.AddRange(chiamate.Select(c => ChiamataAiPrototipoRiga.Da(candidatoId, c)));
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChiamataAi>> ChiamateAsync(Guid candidatoId, CancellationToken cancellationToken = default)
    {
        var righe = await context.ChiamateAiPrototipo.AsNoTracking()
            .Where(c => c.CandidatoId == candidatoId)
            .OrderBy(c => c.AvviataIl).ThenBy(c => c.Tentativo)
            .ToListAsync(cancellationToken);
        return [.. righe.Select(r => r.ComeChiamata())];
    }

    public Task<int> CancellaRisposteGrezzeAsync(Guid? candidatoId, CancellationToken cancellationToken = default) =>
        context.ChiamateAiPrototipo
            .Where(c => (candidatoId == null || c.CandidatoId == candidatoId) && c.RispostaGrezza != null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.RispostaGrezza, (string?)null), cancellationToken);

    public Task<int> RiparaInGenerazioneAsync(DateTime prima, CancellationToken cancellationToken = default) =>
        context.Candidati
            .Where(c => c.Stato == StatoCandidato.InGenerazione && c.GeneratoIl < prima)
            .ExecuteUpdateAsync(
                s => s.SetProperty(c => c.Stato, StatoCandidato.ErroreGenerazione)
                    .SetProperty(c => c.Messaggio, "Generazione interrotta prima della risposta."),
                cancellationToken);
}

/// <summary>Una sola transazione per caso d'uso; una chiamata annidata partecipa a quella già aperta.</summary>
public sealed class TransazionePrototipo(EnigmisticaDbContext context) : ITransazionePrototipo
{
    public async Task<T> EseguiAsync<T>(Func<Task<T>> operazione, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operazione);
        if (context.Database.CurrentTransaction is not null)
            return await operazione();

        await using var transazione = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var risultato = await operazione();
            await transazione.CommitAsync(cancellationToken);
            return risultato;
        }
        catch
        {
            // Il rollback annulla le righe: il tracciamento non deve riprovarle al prossimo salvataggio.
            context.ChangeTracker.Clear();
            throw;
        }
    }
}
