using DBEnigmistica.Application.Valutazione.TempiRevisione;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Infrastructure.Prototipo;

/// <summary>Legge i candidati dello schema <c>prototipo</c> come l'host: stessa stringa di connessione, ruolo <c>dbe_creazione</c>, nessuna scrittura.</summary>
public sealed class LettoreCandidatiRapporto(IConfiguration configurazione) : ILettoreCandidatiRapporto
{
    public async Task<IReadOnlyList<CandidatoPerRapporto>> LeggiAsync(CancellationToken cancellationToken = default)
    {
        var opzioni = new DbContextOptionsBuilder<EnigmisticaDbContext>()
            .UseNpgsql(configurazione.GetConnectionString("DefaultConnection"))
            .AddInterceptors(new RuoloCreazioneInterceptor())
            .Options;
        await using var contesto = new EnigmisticaDbContext(opzioni);
        return await contesto.Candidati.AsNoTracking()
            .Select(c => new CandidatoPerRapporto(c.Tipologia, c.VersioneTipologia, c.Stato, c.TempoAttivoMs))
            .ToListAsync(cancellationToken);
    }
}
