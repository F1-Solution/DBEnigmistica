namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Riga di <c>riviste.chiamate_ai</c> (progetto 1.1, sezione 7.1): un tentativo verso il Fornitore AI.</summary>
public sealed record ChiamataAi(
    Guid Id,
    DateTimeOffset AvviataIl,
    int DurataMs,
    string Uso,
    string Fornitore,
    string Modello,
    string VersionePrompt,
    byte[] ImprontaPagina,
    int Tentativo,
    bool Correzione,
    EsitoEstrazione Esito,
    DateTimeOffset? RipresaIl,
    int? CodiceUscita,
    string? MessaggioErrore,
    int? TokenIngresso,
    int? TokenUscita,
    int? TokenCacheCreazione,
    int? TokenCacheLettura,
    decimal? Costo,
    string? CostoValuta,
    string? CostoTipo,
    bool TestoNativoTroncato,
    string? RispostaGrezza,
    byte[]? RispostaSha256)
{
    public const string UsoEstrazione = "estrazione";
    public const string UsoCreazione = "creazione";
    public const string CostoAddebitato = "addebitato";
    public const string CostoIndicativo = "indicativo";
    public const int MassimoMessaggioErrore = 1000;

    /// <summary>Converte l'impronta esadecimale della pagina nei 32 byte della colonna <c>impronta_pagina</c>.</summary>
    public static byte[] ImprontaDaEsadecimale(string impronta)
    {
        byte[] valore;
        try
        {
            valore = Convert.FromHexString(impronta);
        }
        catch (FormatException)
        {
            throw new ArgumentException("L'impronta della pagina non è esadecimale", nameof(impronta));
        }

        return valore.Length == 32
            ? valore
            : throw new ArgumentException("L'impronta della pagina deve essere di 32 byte (64 cifre esadecimali)", nameof(impronta));
    }

    /// <summary>Valore dell'esito nel registro e nella colonna <c>esito</c> (snake_case).</summary>
    public string EsitoTesto => Esito switch
    {
        EsitoEstrazione.Riuscita => "riuscita",
        EsitoEstrazione.RispostaNonValida => "risposta_non_valida",
        EsitoEstrazione.FinestraEsaurita => "finestra_esaurita",
        EsitoEstrazione.ErroreTransitorio => "errore_transitorio",
        _ => "errore_definitivo",
    };
}
