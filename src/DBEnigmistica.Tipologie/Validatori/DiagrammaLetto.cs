using System.Text.RegularExpressions;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Diagramma "a b = c d" già letto: le lunghezze prima e dopo il segno =; le parentesi esterne sono facoltative.</summary>
internal sealed partial record DiagrammaLetto(IReadOnlyList<int> Sinistra, IReadOnlyList<int> Destra)
{
    [GeneratedRegex(@"^[\d\s,.'’\-]+$")]
    private static partial Regex LatoValido();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Numeri();

    public static bool TryLeggi(string? testo, out DiagrammaLetto? diagramma, out string errore)
    {
        diagramma = null;
        var t = (testo ?? string.Empty).Trim();
        if (t.StartsWith('(') && t.EndsWith(')'))
            t = t[1..^1].Trim();
        var parti = t.Split('=');
        if (parti.Length != 2)
        {
            errore = "Il diagramma deve avere un solo segno =, per esempio 4 = 4.";
            return false;
        }

        if (!TryLato(parti[0], out var sinistra) || !TryLato(parti[1], out var destra))
        {
            errore = "Il diagramma deve contenere solo numeri di lettere, per esempio 3 2 = 5.";
            return false;
        }

        diagramma = new DiagrammaLetto(sinistra, destra);
        errore = string.Empty;
        return true;
    }

    /// <summary>Diagramma del rebus: solo le lunghezze delle parole della soluzione (per esempio <c>2 5 1 6</c>), senza segno =.</summary>
    public static bool TryLeggiSoluzione(string? testo, out DiagrammaLetto? diagramma, out string errore)
    {
        diagramma = null;
        var t = (testo ?? string.Empty).Trim();
        if (t.StartsWith('(') && t.EndsWith(')'))
            t = t[1..^1].Trim();
        if (t.Contains('=', StringComparison.Ordinal) || !TryLato(t, out var lunghezze))
        {
            errore = "Il diagramma del rebus deve contenere solo le lettere di ogni parola della soluzione, senza segno =, per esempio 2 5 1 6.";
            return false;
        }

        diagramma = new DiagrammaLetto([], lunghezze);
        errore = string.Empty;
        return true;
    }

    private static bool TryLato(string lato, out List<int> numeri)
    {
        numeri = [];
        if (!LatoValido().IsMatch(lato))
            return false;
        foreach (Match m in Numeri().Matches(lato))
        {
            if (!int.TryParse(m.Value, out var n) || n < 1 || n > 999)
                return false;
            numeri.Add(n);
        }

        return numeri.Count > 0;
    }
}
