using System.Globalization;

namespace DBEnigmistica.Application.Services.Export;

/// <summary>
/// Formatta i campi CSV neutralizzando le formule dei fogli di calcolo
/// </summary>
public static class CsvFieldFormatter
{
    /// <summary>Prima riga di ogni file CSV: Excel usa la virgola come separatore e rispetta le virgolette.</summary>
    public const string SeparatorDirective = "sep=,";

    private static readonly char[] FormulaPrefixes = ['=', '+', '-', '@', '\t', '\r'];

    /// <summary>
    /// Antepone un apostrofo ai valori che un foglio di calcolo leggerebbe come formula, giudicando il primo carattere non spazio.
    /// </summary>
    public static string Neutralize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var first = value.AsSpan().TrimStart(' ');

        return !first.IsEmpty && Array.IndexOf(FormulaPrefixes, first[0]) >= 0 ? "'" + value : value;
    }

    /// <summary>
    /// Neutralizza le formule e raddoppia le virgolette, senza racchiudere il campo tra virgolette.
    /// </summary>
    public static string NeutralizeAndDoubleQuotes(string? value) => Neutralize(value).Replace("\"", "\"\"");

    /// <summary>
    /// Campo di testo CSV: neutralizzato e sempre racchiuso tra virgolette.
    /// </summary>
    public static string Format(string? value) => $"\"{NeutralizeAndDoubleQuotes(value)}\"";

    /// <summary>
    /// Campo numerico CSV con cultura invariante (punto decimale); stringa vuota se nullo.
    /// </summary>
    public static string Number(IFormattable? value) =>
        value?.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>
    /// Campo data CSV in formato ISO 8601 ("O") con cultura invariante; stringa vuota se nullo.
    /// </summary>
    public static string Date(DateTime? value) =>
        value?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
}
