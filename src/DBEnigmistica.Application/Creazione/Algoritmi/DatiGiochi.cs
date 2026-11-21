using System.Text.Json.Nodes;

namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>Conversione dei giochi generati nei <c>dati</c> dei descrittori v0 (sezione 5 del documento di progetto).</summary>
public static class DatiGiochi
{
    public const string VersioneGeneratoreLogici = "logici-1";
    public const string VersioneGeneratoreSchema = "schema-cruciverba-1";

    public static JsonObject Sudoku(SudokuGioco gioco, int difficolta) => new()
    {
        ["righe"] = 9,
        ["colonne"] = 9,
        ["griglia"] = Griglia(9, 9, gioco.Indizi, c => c == 0 ? '.' : (char)('0' + c)),
        ["soluzione_griglia"] = Griglia(9, 9, gioco.Soluzione, c => (char)('0' + c)),
        ["difficolta"] = difficolta,
    };

    public static JsonObject Futoshiki(FutoshikiGioco gioco, int difficolta)
    {
        var n = gioco.Dimensione;
        var segni = new JsonArray();
        foreach (var (riga, colonna, verso, segno) in SegniOrdinati(gioco.Segni))
        {
            segni.Add(new JsonObject
            {
                ["riga"] = riga + 1,
                ["colonna"] = colonna + 1,
                ["verso"] = verso,
                ["segno"] = segno,
            });
        }

        return new JsonObject
        {
            ["righe"] = n,
            ["colonne"] = n,
            ["griglia"] = Griglia(n, n, gioco.Indizi, c => c == 0 ? '.' : (char)('0' + c)),
            ["soluzione_griglia"] = Griglia(n, n, gioco.Soluzione, c => (char)('0' + c)),
            ["difficolta"] = difficolta,
            ["segni"] = segni,
        };
    }

    public static JsonObject Nonogramma(NonogrammaGioco gioco, int difficolta) => new()
    {
        ["righe"] = gioco.Righe,
        ["colonne"] = gioco.Colonne,
        ["griglia"] = Griglia(gioco.Righe, gioco.Colonne, new int[gioco.Righe * gioco.Colonne], _ => '.'),
        ["soluzione_griglia"] = Griglia(gioco.Righe, gioco.Colonne, gioco.Soluzione, c => c == 0 ? '.' : '#'),
        ["difficolta"] = difficolta,
        ["indizi_righe"] = Indizi(gioco.IndiziRighe),
        ["indizi_colonne"] = Indizi(gioco.IndiziColonne),
    };

    /// <summary>Schema delle caselle nere nel formato della sezione 5.4.</summary>
    public static JsonObject Schema(SchemaCruciverba schema)
    {
        var n = schema.Dimensione;
        var celle = new JsonArray();
        for (var r = 0; r < n; r++)
        {
            celle.Add(new string([.. Enumerable.Range(0, n).Select(c => schema.Nere[(r * n) + c] ? '#' : '.')]));
        }

        var numeri = new JsonArray();
        for (var i = 0; i < schema.Numeri.Count; i++)
        {
            if (schema.Numeri[i] != 0)
            {
                numeri.Add(new JsonObject { ["riga"] = (i / n) + 1, ["colonna"] = (i % n) + 1, ["numero"] = schema.Numeri[i] });
            }
        }

        var parole = new JsonArray();
        foreach (var (direzione, elenco) in new[] { ("orizzontale", schema.Orizzontali), ("verticale", schema.Verticali) })
        {
            foreach (var d in elenco)
            {
                parole.Add(new JsonObject
                {
                    ["numero"] = d.Numero,
                    ["direzione"] = direzione,
                    ["riga"] = d.Riga + 1,
                    ["colonna"] = d.Colonna + 1,
                    ["lunghezza"] = d.Lunghezza,
                });
            }
        }

        return new JsonObject
        {
            ["versione_formato"] = 1,
            ["righe"] = n,
            ["colonne"] = n,
            ["celle"] = celle,
            ["numeri"] = numeri,
            ["parole"] = parole,
            ["seme"] = schema.Seme,
            ["versione_generatore"] = VersioneGeneratoreSchema,
        };
    }

    private static JsonObject Griglia(int righe, int colonne, IReadOnlyList<int> valori, Func<int, char> carattere)
    {
        var celle = new JsonArray();
        for (var r = 0; r < righe; r++)
        {
            celle.Add(new string([.. Enumerable.Range(0, colonne).Select(c => carattere(valori[(r * colonne) + c]))]));
        }

        return new JsonObject
        {
            ["righe"] = righe,
            ["colonne"] = colonne,
            ["celle"] = celle,
            ["numeri"] = new JsonArray(),
        };
    }

    private static JsonArray Indizi(IReadOnlyList<IReadOnlyList<int>> linee)
    {
        var elenco = new JsonArray();
        for (var i = 0; i < linee.Count; i++)
        {
            elenco.Add(new JsonObject
            {
                ["indice"] = i + 1,
                ["blocchi"] = linee[i].Count == 0 ? "0" : string.Join(' ', linee[i]),
            });
        }

        return elenco;
    }

    // Ordine del documento: per riga, poi colonna, poi "destra" prima di "sotto". Coordinate da 0.
    private static IEnumerable<(int Riga, int Colonna, string Verso, string Segno)> SegniOrdinati(
        IReadOnlyList<SegnoFutoshiki> segni)
    {
        return segni
            .Select(s =>
            {
                var orizzontale = s.RigaA == s.RigaB;
                var primaE_A = orizzontale ? s.ColonnaA < s.ColonnaB : s.RigaA < s.RigaB;
                var riga = primaE_A ? s.RigaA : s.RigaB;
                var colonna = primaE_A ? s.ColonnaA : s.ColonnaB;
                return (riga, colonna, verso: orizzontale ? "destra" : "sotto", segno: primaE_A ? "maggiore" : "minore");
            })
            .OrderBy(x => x.riga)
            .ThenBy(x => x.colonna)
            .ThenBy(x => x.verso == "destra" ? 0 : 1);
    }
}

/// <summary>Risultato di una generazione con difficoltà richiesta.</summary>
public sealed record RisultatoGenerazione<T>(T Gioco, int DifficoltaStimata, IReadOnlyList<string> Avvisi);

/// <summary>Prova fino a 50 semi consecutivi e tiene il primo con la difficoltà richiesta, altrimenti il più vicino.</summary>
public static class GeneratoriConDifficolta
{
    public const int TentativiMassimi = 50;

    public static RisultatoGenerazione<SudokuGioco> Sudoku(long semeBase, int difficolta)
    {
        Controlla(difficolta);
        int[] indizi = [0, 38, 32, 28, 25, 22];
        return Cerca(semeBase, difficolta, seme => SudokuGeneratore.Genera(seme, indizi[difficolta]),
            g => StimaDifficolta.Sudoku(g.Indizi));
    }

    public static RisultatoGenerazione<FutoshikiGioco> Futoshiki(long semeBase, int dimensione, int difficolta)
    {
        Controlla(difficolta);
        int[] fissi = [0, dimensione, 4, 3, 2, 1];
        return Cerca(semeBase, difficolta, seme => FutoshikiGeneratore.Genera(seme, dimensione, fissi[difficolta]),
            g => StimaDifficolta.Futoshiki(g.Dimensione, g.Indizi, g.Segni));
    }

    public static RisultatoGenerazione<NonogrammaGioco> Nonogramma(long semeBase, int righe, int colonne, int difficolta)
    {
        Controlla(difficolta);
        double[] densita = [0, 0.7, 0.65, 0.6, 0.55, 0.5];
        return Cerca(semeBase, difficolta, seme => NonogrammaGeneratore.Genera(seme, righe, colonne, densita[difficolta]),
            g => StimaDifficolta.Nonogramma(g.Righe, g.Colonne, g.IndiziRighe, g.IndiziColonne));
    }

    private static void Controlla(int difficolta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(difficolta, StimaDifficolta.Minima);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(difficolta, StimaDifficolta.Massima);
    }

    private static RisultatoGenerazione<T> Cerca<T>(long semeBase, int richiesta, Func<long, T> genera, Func<T, int> stima)
    {
        T? migliore = default;
        var distanzaMigliore = int.MaxValue;
        var stimaMigliore = 0;
        for (var i = 0; i < TentativiMassimi; i++)
        {
            var gioco = genera(semeBase + i);
            var ottenuta = stima(gioco);
            var distanza = Math.Abs(ottenuta - richiesta);
            if (distanza == 0)
            {
                return new RisultatoGenerazione<T>(gioco, ottenuta, []);
            }

            if (distanza < distanzaMigliore)
            {
                (migliore, distanzaMigliore, stimaMigliore) = (gioco, distanza, ottenuta);
            }
        }

        return new RisultatoGenerazione<T>(
            migliore!, stimaMigliore, [$"Difficoltà richiesta {richiesta}, ottenuta {stimaMigliore}."]);
    }
}
