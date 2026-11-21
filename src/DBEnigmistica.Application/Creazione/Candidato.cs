namespace DBEnigmistica.Application.Creazione;

/// <summary>Stati del candidato (progetto 1b.1, sezione 4.2).</summary>
public static class StatoCandidato
{
    public const string InGenerazione = "in_generazione";
    public const string NonValidoSchema = "non_valido_schema";
    public const string ErroreGenerazione = "errore_generazione";
    public const string DaRivedere = "da_rivedere";
    public const string InRevisione = "in_revisione";
    public const string Accettato = "accettato";
    public const string Scartato = "scartato";

    public static IReadOnlyList<string> Tutti { get; } =
        [InGenerazione, NonValidoSchema, ErroreGenerazione, DaRivedere, InRevisione, Accettato, Scartato];

    /// <summary>Stati in cui il candidato è valido e può entrare nella revisione.</summary>
    public static bool Valido(string stato) => stato is DaRivedere or InRevisione or Accettato or Scartato;
}

/// <summary>Motivi dello scarto: lista chiusa (progetto 1b.1, sezione 4.2).</summary>
public static class MotiviScarto
{
    public static IReadOnlyList<string> Tutti { get; } =
    [
        "soluzione_errata", "regole_non_rispettate", "incroci_incoerenti", "qualita_insufficiente",
        "banale_o_troppo_facile", "contenuto_inadatto", "altro",
    ];
}

/// <summary>Riga di <c>prototipo.candidati</c>: un Gioco originale generato dal prototipo, in attesa o già rivisto.</summary>
public sealed record Candidato
{
    public const string ModoAi = "ai";
    public const string ModoAlgoritmo = "algoritmo";
    public const string OrigineOriginale = "originale";

    public Guid Id { get; init; }

    public string Tipologia { get; init; } = string.Empty;

    public int VersioneTipologia { get; init; }

    public string Famiglia { get; init; } = string.Empty;

    public int OrdineGenerazione { get; init; }

    public DateTime GeneratoIl { get; init; }

    public string Modo { get; init; } = ModoAi;

    public string Origine { get; init; } = OrigineOriginale;

    public string Brief { get; init; } = "{}";

    public long? Seme { get; init; }

    public string? VersioneGeneratore { get; init; }

    public string? SchemaCaselle { get; init; }

    public string? VersionePrompt { get; init; }

    public byte[]? ImprontaRichiesta { get; init; }

    public string Stato { get; init; } = StatoCandidato.InGenerazione;

    public string? Messaggio { get; init; }

    public string? ContenutoGenerato { get; init; }

    public string? ContenutoFinale { get; init; }

    public string? Differenze { get; init; }

    public long TempoAttivoMs { get; init; }

    public DateTime? UltimaAttivitaIl { get; init; }

    public DateTime? RevisioneIniziataIl { get; init; }

    public DateTime? RevisioneConclusaIl { get; init; }

    public string? MotivoScarto { get; init; }

    public string? NotaScarto { get; init; }
}
