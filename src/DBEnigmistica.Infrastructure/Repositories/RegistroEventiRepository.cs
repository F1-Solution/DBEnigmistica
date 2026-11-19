using DBEnigmistica.Application.Registro;
using DBEnigmistica.Domain.Registro;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Repositories;

/// <summary>Registro in sola aggiunta: le aggiunte sono serializzate da un lucchetto di transazione.</summary>
public sealed class RegistroEventiRepository(EnigmisticaDbContext context) : IRegistroEventi
{
    /// <summary>Chiave unica del lucchetto sull'ultima riga del registro.</summary>
    internal const long ChiaveLucchetto = 4419001;

    /// <summary>Va chiamato dentro la transazione del caso d'uso, così l'evento e la modifica che documenta riescono o falliscono insieme.</summary>
    public async Task<EventoRegistro> AggiungiAsync(NuovoEvento evento, CancellationToken ct = default)
    {
        // Il lucchetto vale quanto la transazione: senza una esterna se ne apre una.
        var propria = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(ct)
            : null;
        EventoRegistro? riga = null;
        try
        {
            await context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({ChiaveLucchetto})", ct);

            var precedente = await context.EventiRegistro.AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Select(x => x.Impronta)
                .FirstOrDefaultAsync(ct) ?? ImprontaEvento.Iniziale;

            riga = EventoRegistro.Da(0, evento, precedente, ImprontaEvento.Calcola(precedente, evento));
            context.EventiRegistro.Add(riga);
            await context.SaveChangesAsync(ct);

            if (propria is not null)
                await propria.CommitAsync(ct);
            return riga;
        }
        catch
        {
            // Una riga rimasta Added sarebbe reinserita, senza lucchetto, dal prossimo SaveChanges.
            if (riga is not null)
                context.Entry(riga).State = EntityState.Detached;
            throw;
        }
        finally
        {
            if (propria is not null)
                await propria.DisposeAsync();
        }
    }

    public IAsyncEnumerable<EventoRegistro> LeggiInOrdineAsync(CancellationToken ct = default) =>
        context.EventiRegistro.AsNoTracking().OrderBy(x => x.Id).AsAsyncEnumerable();
}
