using System.Text.RegularExpressions;

namespace DBEnigmistica.Domain.Diritti;

/// <summary>Il mondo intero oppure un paese (ISO 3166-1 alfa-2 maiuscolo).</summary>
public sealed partial record Territorio
{
    private const string CodiceMondo = "MONDO";

    private Territorio(string codice) => Codice = codice;

    public static Territorio Mondo { get; } = new(CodiceMondo);

    public string Codice { get; }

    public static Territorio Paese(string codice)
    {
        if (codice is null || !CodicePaese().IsMatch(codice))
            throw new ArgumentException("Il paese deve essere un codice di due lettere maiuscole, per esempio IT.", nameof(codice));
        return new Territorio(codice);
    }

    /// <summary>Legge il testo salvato: MONDO oppure un codice paese.</summary>
    public static Territorio Da(string codice) => codice == CodiceMondo ? Mondo : Paese(codice);

    public bool SiSovrappone(Territorio altro) => Codice == CodiceMondo || altro.Codice == CodiceMondo || Codice == altro.Codice;

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex CodicePaese();
}
