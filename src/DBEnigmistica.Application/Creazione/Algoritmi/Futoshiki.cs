using System.Numerics;

namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>Segno del futoshiki tra due caselle adiacenti: il valore in A è maggiore di quello in B.</summary>
public sealed record SegnoFutoshiki(int RigaA, int ColonnaA, int RigaB, int ColonnaB);

/// <summary>Futoshiki: quadrato latino di lato <paramref name="Dimensione"/>, indizi in ordine di riga con 0 = vuota.</summary>
public sealed record FutoshikiGioco(
    long Seme,
    int Dimensione,
    IReadOnlyList<int> Indizi,
    IReadOnlyList<SegnoFutoshiki> Segni,
    IReadOnlyList<int> Soluzione);

public static class FutoshikiRisolutore
{
    public const int DimensioneMinima = 4;
    public const int DimensioneMassima = 9;

    /// <summary>Conta le soluzioni fermandosi a <paramref name="limite"/>.</summary>
    public static int ContaSoluzioni(
        int dimensione, IReadOnlyList<int> indizi, IReadOnlyList<SegnoFutoshiki> segni, int limite = 2)
    {
        var ricerca = Ricerca.Crea(dimensione, indizi, segni);
        return ricerca is null ? 0 : ricerca.Conta(limite, null);
    }

    /// <summary>Prima soluzione trovata oppure null.</summary>
    public static int[]? Risolvi(int dimensione, IReadOnlyList<int> indizi, IReadOnlyList<SegnoFutoshiki> segni)
    {
        var ricerca = Ricerca.Crea(dimensione, indizi, segni);
        return ricerca is not null && ricerca.Conta(1, null) == 1 ? ricerca.Prima : null;
    }

    internal static int[] QuadratoCasuale(int dimensione, GeneratoreCasuale casuale)
    {
        var ricerca = Ricerca.Crea(dimensione, new int[dimensione * dimensione], [])!;
        ricerca.Conta(1, casuale);
        return ricerca.Prima!;
    }

    private sealed class Ricerca
    {
        private readonly int _n;
        private readonly ushort[] _iniziale;
        private readonly (int A, int B)[] _segni;

        private Ricerca(int n, ushort[] iniziale, (int A, int B)[] segni)
        {
            _n = n;
            _iniziale = iniziale;
            _segni = segni;
        }

        public int[]? Prima { get; private set; }

        public static Ricerca? Crea(int n, IReadOnlyList<int> indizi, IReadOnlyList<SegnoFutoshiki> segni)
        {
            if (n is < DimensioneMinima or > DimensioneMassima)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "Il futoshiki ha lato da 4 a 9.");
            }

            if (indizi.Count != n * n)
            {
                throw new ArgumentException("Gli indizi devono essere lato x lato.", nameof(indizi));
            }

            var tutti = (ushort)((1 << n) - 1);
            var dom = new ushort[n * n];
            for (var i = 0; i < dom.Length; i++)
            {
                var v = indizi[i];
                if (v < 0 || v > n)
                {
                    throw new ArgumentException("Valore fuori intervallo.", nameof(indizi));
                }

                dom[i] = v == 0 ? tutti : (ushort)(1 << (v - 1));
            }

            var coppie = new (int, int)[segni.Count];
            for (var k = 0; k < coppie.Length; k++)
            {
                var s = segni[k];
                if (!Dentro(s.RigaA, s.ColonnaA, n) || !Dentro(s.RigaB, s.ColonnaB, n) ||
                    Math.Abs(s.RigaA - s.RigaB) + Math.Abs(s.ColonnaA - s.ColonnaB) != 1)
                {
                    throw new ArgumentException("Un segno deve unire due caselle adiacenti.", nameof(segni));
                }

                coppie[k] = ((s.RigaA * n) + s.ColonnaA, (s.RigaB * n) + s.ColonnaB);
            }

            return new Ricerca(n, dom, coppie);
        }

        private static bool Dentro(int r, int c, int n) => r >= 0 && r < n && c >= 0 && c < n;

        public int Conta(int limite, GeneratoreCasuale? casuale) => Cerca((ushort[])_iniziale.Clone(), limite, casuale);

        private int Cerca(ushort[] d, int limite, GeneratoreCasuale? casuale)
        {
            if (!Propaga(d))
            {
                return 0;
            }

            var scelta = -1;
            var minimo = int.MaxValue;
            for (var i = 0; i < d.Length; i++)
            {
                var n = BitOperations.PopCount(d[i]);
                if (n > 1 && n < minimo)
                {
                    minimo = n;
                    scelta = i;
                }
            }

            if (scelta < 0)
            {
                Prima ??= d.Select(m => BitOperations.TrailingZeroCount(m) + 1).ToArray();
                return 1;
            }

            var valori = new int[DimensioneMassima];
            var k = 0;
            for (var m = (int)d[scelta]; m != 0; m &= m - 1)
            {
                valori[k++] = BitOperations.TrailingZeroCount(m);
            }

            var scelti = valori.AsSpan(0, k);
            casuale?.Mescola(scelti);

            var totale = 0;
            foreach (var bit in scelti)
            {
                var copia = (ushort[])d.Clone();
                copia[scelta] = (ushort)(1 << bit);
                totale += Cerca(copia, limite - totale, casuale);
                if (totale >= limite)
                {
                    break;
                }
            }

            return totale;
        }

        private bool Propaga(ushort[] d)
        {
            bool cambiato;
            do
            {
                cambiato = false;

                foreach (var (a, b) in _segni)
                {
                    var minB = BitOperations.TrailingZeroCount((int)d[b]);
                    var nuovoA = (ushort)(d[a] & ~((1 << (minB + 1)) - 1));
                    var maxA = 31 - BitOperations.LeadingZeroCount((uint)nuovoA);
                    var nuovoB = (ushort)(d[b] & ((1 << Math.Max(maxA, 0)) - 1));
                    if (nuovoA == 0 || nuovoB == 0)
                    {
                        return false;
                    }

                    if (nuovoA != d[a] || nuovoB != d[b])
                    {
                        d[a] = nuovoA;
                        d[b] = nuovoB;
                        cambiato = true;
                    }
                }

                for (var linea = 0; linea < 2 * _n; linea++)
                {
                    var esito = Linea(d, linea);
                    if (esito < 0)
                    {
                        return false;
                    }

                    cambiato |= esito > 0;
                }
            }
            while (cambiato);

            return true;
        }

        // Restituisce -1 se la riga o colonna è impossibile, 1 se ha ristretto i domini, 0 altrimenti.
        private int Linea(ushort[] d, int linea)
        {
            var n = _n;
            var riga = linea < n;
            var fisso = riga ? linea : linea - n;
            var cambiato = false;

            int Indice(int k) => riga ? (fisso * n) + k : (k * n) + fisso;

            var visti = 0;
            for (var k = 0; k < n; k++)
            {
                var m = d[Indice(k)];
                if (BitOperations.PopCount(m) == 1)
                {
                    if ((visti & m) != 0)
                    {
                        return -1;
                    }

                    visti |= m;
                }
            }

            for (var k = 0; k < n; k++)
            {
                var i = Indice(k);
                if (BitOperations.PopCount(d[i]) > 1)
                {
                    var nuovo = (ushort)(d[i] & ~visti);
                    if (nuovo == 0)
                    {
                        return -1;
                    }

                    if (nuovo != d[i])
                    {
                        d[i] = nuovo;
                        cambiato = true;
                    }
                }
            }

            // Singoli nascosti con maschere: "una volta" e "almeno due volte" per valore.
            int unaVolta = 0, duePiu = 0;
            for (var k = 0; k < n; k++)
            {
                var m = d[Indice(k)];
                duePiu |= unaVolta & m;
                unaVolta |= m;
            }

            if (unaVolta != (1 << n) - 1)
            {
                return -1;
            }

            var unici = unaVolta & ~duePiu;
            if (unici != 0)
            {
                for (var k = 0; k < n; k++)
                {
                    var i = Indice(k);
                    var soloQui = d[i] & unici;
                    if (soloQui == 0 || soloQui == d[i])
                    {
                        continue;
                    }

                    if ((soloQui & (soloQui - 1)) != 0)
                    {
                        return -1;
                    }

                    d[i] = (ushort)soloQui;
                    cambiato = true;
                }
            }

            return cambiato ? 1 : 0;
        }
    }
}

public static class FutoshikiGeneratore
{
    /// <summary>
    /// Genera un futoshiki a soluzione unica: parte da tutti i segni e da <paramref name="indiziFissi"/> indizi, ne aggiunge finché è unico
    /// e poi toglie segni in ordine casuale finché resta unico.
    /// </summary>
    public static FutoshikiGioco Genera(long seme, int dimensione = 5, int indiziFissi = 4)
    {
        if (dimensione is < 5 or > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(dimensione), "Il futoshiki generato ha lato da 5 a 7.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(indiziFissi);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(indiziFissi, dimensione * dimensione);
        var casuale = new GeneratoreCasuale(seme);
        var soluzione = FutoshikiRisolutore.QuadratoCasuale(dimensione, casuale);

        var segni = TuttiISegni(dimensione, soluzione);
        var indizi = new int[dimensione * dimensione];

        var ordineIndizi = Enumerable.Range(0, indizi.Length).ToArray();
        casuale.Mescola<int>(ordineIndizi);
        var prossimo = 0;
        for (; prossimo < indiziFissi; prossimo++)
        {
            indizi[ordineIndizi[prossimo]] = soluzione[ordineIndizi[prossimo]];
        }

        while (FutoshikiRisolutore.ContaSoluzioni(dimensione, indizi, segni, 2) != 1)
        {
            var i = ordineIndizi[prossimo++];
            indizi[i] = soluzione[i];
        }

        var ordineSegni = Enumerable.Range(0, segni.Count).ToArray();
        casuale.Mescola<int>(ordineSegni);
        var tolti = new bool[segni.Count];
        foreach (var k in ordineSegni)
        {
            tolti[k] = true;
            var rimasti = segni.Where((_, j) => !tolti[j]).ToList();
            if (FutoshikiRisolutore.ContaSoluzioni(dimensione, indizi, rimasti, 2) != 1)
            {
                tolti[k] = false;
            }
        }

        return new FutoshikiGioco(
            seme, dimensione, indizi, segni.Where((_, j) => !tolti[j]).ToList(), soluzione);
    }

    private static List<SegnoFutoshiki> TuttiISegni(int n, int[] soluzione)
    {
        var segni = new List<SegnoFutoshiki>();
        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++)
            {
                var v = soluzione[(r * n) + c];
                if (c + 1 < n)
                {
                    segni.Add(Segno(r, c, r, c + 1, v, soluzione[(r * n) + c + 1]));
                }

                if (r + 1 < n)
                {
                    segni.Add(Segno(r, c, r + 1, c, v, soluzione[((r + 1) * n) + c]));
                }
            }
        }

        return segni;
    }

    private static SegnoFutoshiki Segno(int r1, int c1, int r2, int c2, int v1, int v2) =>
        v1 > v2 ? new SegnoFutoshiki(r1, c1, r2, c2) : new SegnoFutoshiki(r2, c2, r1, c1);
}
