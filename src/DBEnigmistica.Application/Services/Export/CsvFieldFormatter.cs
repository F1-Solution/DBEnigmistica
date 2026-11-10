namespace DBEnigmistica.Application.Services.Export;

/// <summary>
/// Formatta i campi CSV neutralizzando le formule dei fogli di calcolo
/// </summary>
public static class CsvFieldFormatter
{
    private static readonly char[] FormulaPrefixes = ['=', '+', '-', '@', '\t', '\r'];

    /// <summary>
    /// Neutralizza i valori che un foglio di calcolo leggerebbe come formula (apostrofo iniziale).
    /// </summary>
    public static string Neutralize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Array.IndexOf(FormulaPrefixes, value[0]) >= 0 ? "'" + value : value;
    }

    /// <summary>
    /// Neutralizza le formule e raddoppia le virgolette, senza racchiudere il campo tra virgolette.
    /// </summary>
    public static string EscapeQuotes(string? value) => Neutralize(value).Replace("\"", "\"\"");

    /// <summary>
    /// Neutralizza le formule e quota il campo solo se contiene virgole, virgolette o a capo.
    /// </summary>
    public static string Format(string? value)
    {
        var safe = Neutralize(value);

        return safe.IndexOfAny([',', '"', '\n', '\r']) >= 0
            ? $"\"{safe.Replace("\"", "\"\"")}\""
            : safe;
    }
}
