using DBEnigmistica.Domain.Registro;

namespace DBEnigmistica.Application.Registro;

/// <summary>Caso d'uso: verifica che la catena delle impronte del registro sia integra.</summary>
public sealed class VerificaIntegritaRegistro(IRegistroEventi registro)
{
    public Task<EsitoVerifica> EseguiAsync(CancellationToken ct = default) =>
        VerificaCatena.VerificaAsync(registro.LeggiInOrdineAsync(ct), ct);
}
