namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Calcola le parole di una griglia e la loro numerazione in ordine di lettura.</summary>
internal static class NumerazioneGriglia
{
    /// <summary>Una casella bianca inizia una parola se non ha bianca prima e ne ha una dopo, in orizzontale o in verticale (minimo 2 lettere).</summary>
    public static IReadOnlyList<ParolaInGriglia> Parole(GrigliaLetta g)
    {
        var parole = new List<ParolaInGriglia>();
        var numero = 0;
        for (var r = 0; r < g.Celle.Count; r++)
        {
            for (var c = 0; c < g.Celle[r].Length; c++)
            {
                if (!g.Bianca(r, c))
                    continue;
                var orizzontale = !g.Bianca(r, c - 1) && g.Bianca(r, c + 1);
                var verticale = !g.Bianca(r - 1, c) && g.Bianca(r + 1, c);
                if (!orizzontale && !verticale)
                    continue;
                numero++;
                if (orizzontale)
                    parole.Add(new ParolaInGriglia(numero, ParolaInGriglia.Orizzontale, r + 1, c + 1, Lunghezza(g, r, c, 0, 1)));
                if (verticale)
                    parole.Add(new ParolaInGriglia(numero, ParolaInGriglia.Verticale, r + 1, c + 1, Lunghezza(g, r, c, 1, 0)));
            }
        }

        return parole;
    }

    private static int Lunghezza(GrigliaLetta g, int riga, int colonna, int dr, int dc)
    {
        var n = 0;
        while (g.Bianca(riga + (n * dr), colonna + (n * dc)))
            n++;
        return n;
    }
}
