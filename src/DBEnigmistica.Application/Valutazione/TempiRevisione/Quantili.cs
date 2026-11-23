namespace DBEnigmistica.Application.Valutazione.TempiRevisione;

/// <summary>Quantili con interpolazione lineare (metodo 7 di Hyndman e Fan, come <c>PERCENTILE.INC</c>).</summary>
public static class Quantili
{
    /// <summary>Quantile di ordine <paramref name="p"/> (0-1); <c>null</c> se non ci sono valori.</summary>
    public static double? Calcola(IReadOnlyCollection<double> valori, double p)
    {
        ArgumentNullException.ThrowIfNull(valori);
        if (valori.Count == 0)
            return null;

        var x = valori.Order().ToArray();
        var h = (x.Length - 1) * p;
        var sotto = (int)Math.Floor(h);
        return sotto + 1 >= x.Length ? x[^1] : x[sotto] + ((h - sotto) * (x[sotto + 1] - x[sotto]));
    }
}
