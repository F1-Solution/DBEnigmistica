using System.Globalization;
using System.Text;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Normalizzazione del testo dei Giochi: maiuscole, senza accenti, solo lettere; spazi, apostrofi e trattini separano le parole.</summary>
internal static class TestoNormalizzato
{
    private static readonly char[] Separatori = [' ', '\t', '\r', '\n', '\'', '’', '‘', 'ʼ', '`', '´', '-', '–', '—'];

    /// <summary>Solo le lettere, in maiuscolo e senza accenti; tutto il resto cade.</summary>
    public static string Lettere(string? testo)
    {
        if (string.IsNullOrEmpty(testo))
            return string.Empty;
        var risultato = new StringBuilder(testo.Length);
        foreach (var c in testo.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetter(c))
                risultato.Append(char.ToUpperInvariant(c));
        }

        return risultato.ToString();
    }

    /// <summary>Le parole del testo, normalizzate con <see cref="Lettere"/>; quelle senza lettere si scartano.</summary>
    public static IReadOnlyList<string> Parole(string? testo) =>
        string.IsNullOrEmpty(testo)
            ? []
            : [.. testo.Split(Separatori, StringSplitOptions.RemoveEmptyEntries).Select(Lettere).Where(p => p.Length > 0)];
}
