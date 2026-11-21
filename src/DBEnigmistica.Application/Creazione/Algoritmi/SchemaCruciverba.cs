namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>Definizione numerata di uno schema: casella di partenza e lunghezza della parola.</summary>
public sealed record DefinizioneSchema(int Numero, int Riga, int Colonna, int Lunghezza);

/// <summary>Schema di caselle nere: griglia quadrata in ordine di riga, numerazione italiana standard.</summary>
public sealed record SchemaCruciverba(
    long Seme,
    int Dimensione,
    IReadOnlyList<bool> Nere,
    IReadOnlyList<int> Numeri,
    IReadOnlyList<DefinizioneSchema> Orizzontali,
    IReadOnlyList<DefinizioneSchema> Verticali);

public static class SchemaCruciverbaValidatore
{
    /// <summary>Elenco dei problemi dello schema: simmetria, parole di una lettera, collegamento, numerazione.</summary>
    public static List<string> Valida(SchemaCruciverba schema)
    {
        var n = schema.Dimensione;
        var errori = new List<string>();
        var nere = schema.Nere;
        if (nere.Count != n * n)
        {
            errori.Add("La griglia non è quadrata.");
            return errori;
        }

        for (var i = 0; i < nere.Count; i++)
        {
            if (nere[i] != nere[(n * n) - 1 - i])
            {
                errori.Add("Lo schema non è simmetrico per rotazione di 180 gradi.");
                break;
            }
        }

        var bianche = nere.Count(x => !x);
        if (bianche == 0)
        {
            errori.Add("Lo schema non ha caselle bianche.");
            return errori;
        }

        if (VuoteUnaLettera(nere, n))
        {
            errori.Add("Esiste una casella bianca in una parola di una sola lettera.");
        }

        if (Collegate(nere, n) != bianche)
        {
            errori.Add("Le caselle bianche non sono tutte collegate.");
        }

        var quota = (double)(nere.Count - bianche) / nere.Count;
        if (quota < SchemaCruciverbaGeneratore.QuotaNereMinima || quota > SchemaCruciverbaGeneratore.QuotaNereMassima)
        {
            errori.Add("La quota di caselle nere deve stare tra il 10% e il 22%.");
        }

        if (RunPiuLungo(nere, n) > SchemaCruciverbaGeneratore.LunghezzaMassimaParola)
        {
            errori.Add("Esiste una parola di più di 9 lettere.");
        }

        var atteso = SchemaCruciverbaGeneratore.Numera(nere, n);
        if (!schema.Numeri.SequenceEqual(atteso.Numeri) ||
            !schema.Orizzontali.SequenceEqual(atteso.Orizzontali) ||
            !schema.Verticali.SequenceEqual(atteso.Verticali))
        {
            errori.Add("La numerazione non è quella standard.");
        }

        return errori;
    }

    internal static int RunPiuLungo(IReadOnlyList<bool> nere, int n)
    {
        var massimo = 0;
        for (var a = 0; a < n; a++)
        {
            int orizzontale = 0, verticale = 0;
            for (var b = 0; b < n; b++)
            {
                orizzontale = nere[(a * n) + b] ? 0 : orizzontale + 1;
                verticale = nere[(b * n) + a] ? 0 : verticale + 1;
                massimo = Math.Max(massimo, Math.Max(orizzontale, verticale));
            }
        }

        return massimo;
    }

    internal static bool VuoteUnaLettera(IReadOnlyList<bool> nere, int n)
    {
        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++)
            {
                if (nere[(r * n) + c])
                {
                    continue;
                }

                var orizzontale = (c > 0 && !nere[(r * n) + c - 1]) || (c + 1 < n && !nere[(r * n) + c + 1]);
                var verticale = (r > 0 && !nere[((r - 1) * n) + c]) || (r + 1 < n && !nere[((r + 1) * n) + c]);
                if (!orizzontale || !verticale)
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal static int Collegate(IReadOnlyList<bool> nere, int n)
    {
        var inizio = -1;
        for (var i = 0; i < nere.Count && inizio < 0; i++)
        {
            if (!nere[i])
            {
                inizio = i;
            }
        }

        if (inizio < 0)
        {
            return 0;
        }

        var visto = new bool[nere.Count];
        var pila = new Stack<int>();
        pila.Push(inizio);
        visto[inizio] = true;
        var conta = 0;
        while (pila.Count > 0)
        {
            var i = pila.Pop();
            conta++;
            int r = i / n, c = i % n;
            foreach (var (dr, dc) in new[] { (0, 1), (1, 0), (0, -1), (-1, 0) })
            {
                int rr = r + dr, cc = c + dc;
                if (rr < 0 || rr >= n || cc < 0 || cc >= n)
                {
                    continue;
                }

                var j = (rr * n) + cc;
                if (!nere[j] && !visto[j])
                {
                    visto[j] = true;
                    pila.Push(j);
                }
            }
        }

        return conta;
    }
}

public static class SchemaCruciverbaGeneratore
{
    public const int DimensioneMinima = 9;
    public const int DimensioneMassima = 13;
    public const double QuotaNereMinima = 0.10;
    public const double QuotaNereMassima = 0.22;
    public const int LunghezzaMassimaParola = 9;
    private const int TentativiMassimi = 10_000;

    /// <summary>
    /// Genera uno schema simmetrico a 180 gradi, senza parole di una lettera e con le bianche collegate:
    /// piazza nere casuali, poi annerisce le caselle fuori regola finché lo schema è valido.
    /// </summary>
    public static SchemaCruciverba Genera(long seme, int dimensione = 11)
    {
        if (dimensione is < DimensioneMinima or > DimensioneMassima)
        {
            throw new ArgumentOutOfRangeException(nameof(dimensione), "Lo schema ha lato da 9 a 13.");
        }

        var casuale = new GeneratoreCasuale(seme);
        var n = dimensione;
        var totale = n * n;
        for (var tentativo = 0; tentativo < TentativiMassimi; tentativo++)
        {
            var nere = new bool[totale];
            var daPiazzare = (int)Math.Round(totale * (0.05 + (casuale.ProssimoDouble() * 0.07)));
            for (var k = 0; k < daPiazzare; k++)
            {
                Annerisci(nere, casuale.Prossimo(totale));
            }

            Ripara(nere, n, casuale);
            var contate = nere.Count(x => x);
            if (contate > totale * QuotaNereMassima || contate < totale * QuotaNereMinima)
            {
                continue;
            }

            var (numeri, orizzontali, verticali) = Numera(nere, n);
            return new SchemaCruciverba(seme, n, nere, numeri, orizzontali, verticali);
        }

        throw new InvalidOperationException("Impossibile generare uno schema valido per questo seme.");
    }

    private static void Annerisci(bool[] nere, int i)
    {
        nere[i] = true;
        nere[nere.Length - 1 - i] = true;
    }

    private static void Ripara(bool[] nere, int n, GeneratoreCasuale casuale)
    {
        bool cambiato;
        do
        {
            cambiato = false;
            for (var r = 0; r < n && !cambiato; r++)
            {
                for (var c = 0; c < n; c++)
                {
                    if (nere[(r * n) + c] || !EIsolata(nere, n, r, c))
                    {
                        continue;
                    }

                    Annerisci(nere, (r * n) + c);
                    cambiato = true;
                }
            }

            if (!cambiato)
            {
                cambiato = SpezzaParoleLunghe(nere, n, casuale);
            }

            if (!cambiato)
            {
                cambiato = TieniSoloComponentePiuGrande(nere, n);
            }
        }
        while (cambiato);
    }

    // Annerisce una casella interna alla prima parola troppo lunga, lasciando pezzi di almeno 2.
    private static bool SpezzaParoleLunghe(bool[] nere, int n, GeneratoreCasuale casuale)
    {
        for (var a = 0; a < n; a++)
        {
            foreach (var orizzontale in new[] { true, false })
            {
                var inizio = 0;
                for (var b = 0; b <= n; b++)
                {
                    var bianca = b < n && !nere[orizzontale ? (a * n) + b : (b * n) + a];
                    if (bianca)
                    {
                        continue;
                    }

                    var lunghezza = b - inizio;
                    if (lunghezza > LunghezzaMassimaParola)
                    {
                        var posizione = inizio + 2 + casuale.Prossimo(lunghezza - 4);
                        Annerisci(nere, orizzontale ? (a * n) + posizione : (posizione * n) + a);
                        return true;
                    }

                    inizio = b + 1;
                }
            }
        }

        return false;
    }

    private static bool EIsolata(bool[] nere, int n, int r, int c)
    {
        var orizzontale = (c > 0 && !nere[(r * n) + c - 1]) || (c + 1 < n && !nere[(r * n) + c + 1]);
        var verticale = (r > 0 && !nere[((r - 1) * n) + c]) || (r + 1 < n && !nere[((r + 1) * n) + c]);
        return !orizzontale || !verticale;
    }

    private static bool TieniSoloComponentePiuGrande(bool[] nere, int n)
    {
        var componente = new int[nere.Length];
        var dimensioni = new List<int> { 0 };
        for (var i = 0; i < nere.Length; i++)
        {
            if (nere[i] || componente[i] != 0)
            {
                continue;
            }

            var id = dimensioni.Count;
            dimensioni.Add(0);
            var pila = new Stack<int>();
            pila.Push(i);
            componente[i] = id;
            while (pila.Count > 0)
            {
                var corrente = pila.Pop();
                dimensioni[id]++;
                int r = corrente / n, c = corrente % n;
                foreach (var (dr, dc) in new[] { (0, 1), (1, 0), (0, -1), (-1, 0) })
                {
                    int rr = r + dr, cc = c + dc;
                    if (rr < 0 || rr >= n || cc < 0 || cc >= n)
                    {
                        continue;
                    }

                    var j = (rr * n) + cc;
                    if (!nere[j] && componente[j] == 0)
                    {
                        componente[j] = id;
                        pila.Push(j);
                    }
                }
            }
        }

        if (dimensioni.Count <= 2)
        {
            return false;
        }

        var migliore = 1;
        for (var id = 2; id < dimensioni.Count; id++)
        {
            if (dimensioni[id] > dimensioni[migliore])
            {
                migliore = id;
            }
        }

        for (var i = 0; i < nere.Length; i++)
        {
            if (!nere[i] && componente[i] != migliore)
            {
                Annerisci(nere, i);
            }
        }

        return true;
    }

    /// <summary>Numerazione standard: da sinistra a destra e dall'alto in basso, una casella per parola iniziata.</summary>
    public static (int[] Numeri, List<DefinizioneSchema> Orizzontali, List<DefinizioneSchema> Verticali) Numera(
        IReadOnlyList<bool> nere, int n)
    {
        var numeri = new int[n * n];
        var orizzontali = new List<DefinizioneSchema>();
        var verticali = new List<DefinizioneSchema>();
        var prossimo = 1;
        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++)
            {
                if (nere[(r * n) + c])
                {
                    continue;
                }

                var inizioO = (c == 0 || nere[(r * n) + c - 1]) && c + 1 < n && !nere[(r * n) + c + 1];
                var inizioV = (r == 0 || nere[((r - 1) * n) + c]) && r + 1 < n && !nere[((r + 1) * n) + c];
                if (!inizioO && !inizioV)
                {
                    continue;
                }

                numeri[(r * n) + c] = prossimo;
                if (inizioO)
                {
                    var l = 0;
                    while (c + l < n && !nere[(r * n) + c + l])
                    {
                        l++;
                    }

                    orizzontali.Add(new DefinizioneSchema(prossimo, r, c, l));
                }

                if (inizioV)
                {
                    var l = 0;
                    while (r + l < n && !nere[((r + l) * n) + c])
                    {
                        l++;
                    }

                    verticali.Add(new DefinizioneSchema(prossimo, r, c, l));
                }

                prossimo++;
            }
        }

        return (numeri, orizzontali, verticali);
    }
}
