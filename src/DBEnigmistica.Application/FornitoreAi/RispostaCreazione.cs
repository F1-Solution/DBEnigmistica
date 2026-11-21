namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Risposta del Fornitore AI per un candidato, con una <see cref="ChiamataAi"/> per ogni tentativo.</summary>
public sealed record RispostaCreazione(
    EsitoEstrazione Esito,
    string? Json,
    string? Messaggio,
    DateTimeOffset? RipresaFinestra,
    IReadOnlyList<ChiamataAi> Chiamate,
    MetadatiChiamata? Metadati = null);
