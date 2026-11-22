using System.Text;
using System.Text.RegularExpressions;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Normalizzazione N del progetto 1.1 (sezione 4.3) e distanza di Levenshtein sui caratteri Unicode.</summary>
public static partial class NormalizzazioneTesto
{
    public static string Normalizza(string testo)
    {
        ArgumentNullException.ThrowIfNull(testo);
        var s = testo.Normalize(NormalizationForm.FormC);
        s = ApostrofiETipografici().Replace(s, "'");
        s = VirgoletteDoppie().Replace(s, "\"");
        s = Trattini().Replace(s, "-");
        s = s.Replace("…", "...", StringComparison.Ordinal);
        s = Spazi().Replace(s, " ");
        s = SpaziAttornoApostrofo().Replace(s, "'");
        return s.Trim(' ').ToLowerInvariant();
    }

    /// <summary>Numero di caratteri Unicode (rune) dopo la normalizzazione.</summary>
    public static int Lunghezza(string testo) => Normalizza(testo).EnumerateRunes().Count();

    /// <summary>Distanza di modifica con peso 1 fra i due testi normalizzati.</summary>
    public static int Distanza(string a, string b)
    {
        var x = Normalizza(a).EnumerateRunes().Select(r => r.Value).ToArray();
        var y = Normalizza(b).EnumerateRunes().Select(r => r.Value).ToArray();
        var precedente = Enumerable.Range(0, y.Length + 1).ToArray();
        var corrente = new int[y.Length + 1];
        for (var i = 1; i <= x.Length; i++)
        {
            corrente[0] = i;
            for (var j = 1; j <= y.Length; j++)
            {
                var sostituzione = precedente[j - 1] + (x[i - 1] == y[j - 1] ? 0 : 1);
                corrente[j] = Math.Min(sostituzione, Math.Min(precedente[j] + 1, corrente[j - 1] + 1));
            }

            (precedente, corrente) = (corrente, precedente);
        }

        return precedente[y.Length];
    }

    [GeneratedRegex("[’‘‛ʼ´`]")]
    private static partial Regex ApostrofiETipografici();

    [GeneratedRegex("[“”„«»]")]
    private static partial Regex VirgoletteDoppie();

    [GeneratedRegex("[–—−]")]
    private static partial Regex Trattini();

    [GeneratedRegex(@"[\s\p{Zs}]+")]
    private static partial Regex Spazi();

    [GeneratedRegex(" ?' ?")]
    private static partial Regex SpaziAttornoApostrofo();
}
