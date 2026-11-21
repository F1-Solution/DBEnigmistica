using System.Numerics;

namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>Sudoku 9x9: griglie di 81 valori in ordine di riga, 0 = casella vuota.</summary>
public sealed record SudokuGioco(long Seme, IReadOnlyList<int> Indizi, IReadOnlyList<int> Soluzione);

public static class SudokuRisolutore
{
    public const int Lato = 9;
    public const int Caselle = 81;

    /// <summary>Conta le soluzioni fermandosi a <paramref name="limite"/>; indizi in conflitto danno 0.</summary>
    public static int ContaSoluzioni(IReadOnlyList<int> griglia, int limite = 2)
    {
        var stato = Stato.Da(griglia);
        return stato is null ? 0 : stato.Conta(limite, null);
    }

    /// <summary>Prima soluzione trovata oppure null.</summary>
    public static int[]? Risolvi(IReadOnlyList<int> griglia)
    {
        var stato = Stato.Da(griglia);
        if (stato is null)
        {
            return null;
        }

        return stato.Conta(1, null) == 1 ? stato.Prima : null;
    }

    internal static int[] GrigliaCompleta(GeneratoreCasuale casuale)
    {
        var stato = Stato.Da(new int[Caselle])!;
        stato.Conta(1, casuale);
        return stato.Prima!;
    }

    private sealed class Stato
    {
        private readonly int[] _g = new int[Caselle];
        private readonly int[] _righe = new int[Lato];
        private readonly int[] _colonne = new int[Lato];
        private readonly int[] _blocchi = new int[Lato];

        public int[]? Prima { get; private set; }

        public static Stato? Da(IReadOnlyList<int> griglia)
        {
            if (griglia.Count != Caselle)
            {
                throw new ArgumentException("La griglia del sudoku ha 81 caselle.", nameof(griglia));
            }

            var s = new Stato();
            for (var i = 0; i < Caselle; i++)
            {
                var v = griglia[i];
                if (v is < 0 or > 9)
                {
                    throw new ArgumentException("I valori del sudoku vanno da 0 a 9.", nameof(griglia));
                }

                if (v != 0 && !s.Metti(i, v))
                {
                    return null;
                }
            }

            return s;
        }

        private bool Metti(int i, int v)
        {
            int r = i / Lato, c = i % Lato, b = (r / 3 * 3) + (c / 3), bit = 1 << v;
            if (((_righe[r] | _colonne[c] | _blocchi[b]) & bit) != 0)
            {
                return false;
            }

            _righe[r] |= bit;
            _colonne[c] |= bit;
            _blocchi[b] |= bit;
            _g[i] = v;
            return true;
        }

        private void Togli(int i)
        {
            int r = i / Lato, c = i % Lato, b = (r / 3 * 3) + (c / 3), bit = ~(1 << _g[i]);
            _righe[r] &= bit;
            _colonne[c] &= bit;
            _blocchi[b] &= bit;
            _g[i] = 0;
        }

        public int Conta(int limite, GeneratoreCasuale? casuale)
        {
            var migliore = -1;
            var candidatiMigliori = 0;
            var minimo = 10;
            for (var i = 0; i < Caselle; i++)
            {
                if (_g[i] != 0)
                {
                    continue;
                }

                int r = i / Lato, c = i % Lato;
                var candidati = ~(_righe[r] | _colonne[c] | _blocchi[(r / 3 * 3) + (c / 3)]) & 0x3FE;
                var n = BitOperations.PopCount((uint)candidati);
                if (n == 0)
                {
                    return 0;
                }

                if (n < minimo)
                {
                    minimo = n;
                    migliore = i;
                    candidatiMigliori = candidati;
                    if (n == 1)
                    {
                        break;
                    }
                }
            }

            if (migliore < 0)
            {
                Prima ??= (int[])_g.Clone();
                return 1;
            }

            Span<int> valori = stackalloc int[9];
            var k = 0;
            for (var m = candidatiMigliori; m != 0; m &= m - 1)
            {
                valori[k++] = BitOperations.TrailingZeroCount(m);
            }

            var scelti = valori[..k];
            casuale?.Mescola(scelti);

            var totale = 0;
            foreach (var v in scelti)
            {
                Metti(migliore, v);
                totale += Conta(limite - totale, casuale);
                Togli(migliore);
                if (totale >= limite)
                {
                    break;
                }
            }

            return totale;
        }
    }
}

public static class SudokuGeneratore
{
    public const int IndiziMinimiAssoluti = 17;

    /// <summary>
    /// Genera un sudoku a soluzione unica: toglie indizi in ordine casuale finché ne restano
    /// <paramref name="indiziDesiderati"/> o non se ne può togliere altri senza perdere l'unicità.
    /// </summary>
    public static SudokuGioco Genera(long seme, int indiziDesiderati = 30)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(indiziDesiderati, IndiziMinimiAssoluti);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(indiziDesiderati, SudokuRisolutore.Caselle);

        var casuale = new GeneratoreCasuale(seme);
        var soluzione = SudokuRisolutore.GrigliaCompleta(casuale);
        var indizi = (int[])soluzione.Clone();

        var ordine = Enumerable.Range(0, SudokuRisolutore.Caselle).ToArray();
        casuale.Mescola<int>(ordine);

        var rimasti = SudokuRisolutore.Caselle;
        foreach (var i in ordine)
        {
            if (rimasti <= indiziDesiderati)
            {
                break;
            }

            var valore = indizi[i];
            indizi[i] = 0;
            if (SudokuRisolutore.ContaSoluzioni(indizi, 2) == 1)
            {
                rimasti--;
            }
            else
            {
                indizi[i] = valore;
            }
        }

        return new SudokuGioco(seme, indizi, soluzione);
    }
}
