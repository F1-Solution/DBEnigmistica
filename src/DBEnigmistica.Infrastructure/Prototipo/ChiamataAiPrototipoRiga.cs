using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Infrastructure.Prototipo;

/// <summary>Riga di <c>prototipo.chiamate_ai</c>: una <see cref="ChiamataAi"/> di creazione legata al suo candidato.</summary>
public sealed class ChiamataAiPrototipoRiga
{
    public Guid Id { get; set; }

    public Guid CandidatoId { get; set; }

    public DateTime AvviataIl { get; set; }

    public int DurataMs { get; set; }

    public string Uso { get; set; } = ChiamataAi.UsoCreazione;

    public string Fornitore { get; set; } = string.Empty;

    public string Modello { get; set; } = string.Empty;

    public string VersionePrompt { get; set; } = string.Empty;

    public byte[] ImprontaRichiesta { get; set; } = [];

    public int Tentativo { get; set; }

    public bool Correzione { get; set; }

    public string Esito { get; set; } = string.Empty;

    public DateTime? RipresaIl { get; set; }

    public int? CodiceUscita { get; set; }

    public string? MessaggioErrore { get; set; }

    public int? TokenIngresso { get; set; }

    public int? TokenUscita { get; set; }

    public int? TokenCacheCreazione { get; set; }

    public int? TokenCacheLettura { get; set; }

    public decimal? Costo { get; set; }

    public string? CostoValuta { get; set; }

    public string? CostoTipo { get; set; }

    public string? RispostaGrezza { get; set; }

    public byte[]? RispostaSha256 { get; set; }

    public static ChiamataAiPrototipoRiga Da(Guid candidatoId, ChiamataAi c) => new()
    {
        Id = c.Id,
        CandidatoId = candidatoId,
        AvviataIl = c.AvviataIl.UtcDateTime,
        DurataMs = c.DurataMs,
        Uso = c.Uso,
        Fornitore = c.Fornitore,
        Modello = c.Modello,
        VersionePrompt = c.VersionePrompt,
        ImprontaRichiesta = c.ImprontaPagina,
        Tentativo = c.Tentativo,
        Correzione = c.Correzione,
        Esito = c.EsitoTesto,
        RipresaIl = c.RipresaIl?.UtcDateTime,
        CodiceUscita = c.CodiceUscita,
        MessaggioErrore = c.MessaggioErrore,
        TokenIngresso = c.TokenIngresso,
        TokenUscita = c.TokenUscita,
        TokenCacheCreazione = c.TokenCacheCreazione,
        TokenCacheLettura = c.TokenCacheLettura,
        Costo = c.Costo,
        CostoValuta = c.CostoValuta,
        CostoTipo = c.CostoTipo,
        RispostaGrezza = c.RispostaGrezza,
        RispostaSha256 = c.RispostaSha256,
    };

    public ChiamataAi ComeChiamata() => new(
        Id,
        new DateTimeOffset(DateTime.SpecifyKind(AvviataIl, DateTimeKind.Utc)),
        DurataMs,
        Uso,
        Fornitore,
        Modello,
        VersionePrompt,
        ImprontaRichiesta,
        Tentativo,
        Correzione,
        EsitoDaTesto(Esito),
        RipresaIl is { } ripresa ? new DateTimeOffset(DateTime.SpecifyKind(ripresa, DateTimeKind.Utc)) : null,
        CodiceUscita,
        MessaggioErrore,
        TokenIngresso,
        TokenUscita,
        TokenCacheCreazione,
        TokenCacheLettura,
        Costo,
        CostoValuta,
        CostoTipo,
        false,
        RispostaGrezza,
        RispostaSha256);

    private static EsitoEstrazione EsitoDaTesto(string esito) => esito switch
    {
        "riuscita" => EsitoEstrazione.Riuscita,
        "risposta_non_valida" => EsitoEstrazione.RispostaNonValida,
        "finestra_esaurita" => EsitoEstrazione.FinestraEsaurita,
        "errore_transitorio" => EsitoEstrazione.ErroreTransitorio,
        _ => EsitoEstrazione.ErroreDefinitivo,
    };
}
