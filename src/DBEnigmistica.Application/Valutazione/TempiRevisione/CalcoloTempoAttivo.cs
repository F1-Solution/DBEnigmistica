namespace DBEnigmistica.Application.Valutazione.TempiRevisione;

/// <summary>Tempo attivo di un candidato dagli istanti di attività (progetto 1b.1, sezione 8.1): una pausa oltre due minuti non conta.</summary>
public static class CalcoloTempoAttivo
{
    /// <summary>Intervallo massimo tra due attività consecutive che conta come tempo attivo.</summary>
    public const long PausaMassimaMs = 120_000;

    /// <summary>Somma degli intervalli tra istanti consecutivi (millisecondi); un intervallo negativo o oltre la pausa massima conta zero.</summary>
    public static long Da(IReadOnlyList<long> istantiMs)
    {
        ArgumentNullException.ThrowIfNull(istantiMs);
        long totale = 0;
        for (var i = 1; i < istantiMs.Count; i++)
        {
            var intervallo = istantiMs[i] - istantiMs[i - 1];
            if (intervallo is >= 0 and <= PausaMassimaMs)
                totale += intervallo;
        }

        return totale;
    }
}
