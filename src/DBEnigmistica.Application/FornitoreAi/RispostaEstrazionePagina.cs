namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Esito di una chiamata di estrazione.</summary>
public enum EsitoEstrazione
{
    Riuscita,
    RispostaNonValida,
    FinestraEsaurita,
    ErroreTransitorio,
    ErroreDefinitivo
}

/// <summary>Risposta strutturata del Fornitore AI per una pagina, con una <see cref="ChiamataAi"/> per ogni tentativo fatto.</summary>
public sealed record RispostaEstrazionePagina(
    EsitoEstrazione Esito,
    string? Json,
    string? Messaggio,
    DateTimeOffset? RipresaFinestra,
    IReadOnlyList<ChiamataAi> Chiamate,
    MetadatiChiamata? Metadati = null);
