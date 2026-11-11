using DBEnigmistica.Domain.Registro;

namespace DBEnigmistica.Application.Registro;

/// <summary>Porta del registro in sola aggiunta: si aggiunge e si legge, mai si modifica.</summary>
public interface IRegistroEventi
{
    Task<EventoRegistro> AggiungiAsync(NuovoEvento evento, CancellationToken ct = default);

    IAsyncEnumerable<EventoRegistro> LeggiInOrdineAsync(CancellationToken ct = default);
}
