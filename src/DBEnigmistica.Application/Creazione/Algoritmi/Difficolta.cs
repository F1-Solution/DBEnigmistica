using System.Numerics;

namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>
/// Stima della difficoltà (1-5) come livello minimo di tecniche che risolve il gioco senza ipotesi;
/// 5 = serve la ricerca. Le tecniche sono quelle della sezione 5.5 del documento di progetto.
/// </summary>
public static class StimaDifficolta
{
    public const int Minima = 1;
    public const int Massima = 5;

    public static int Sudoku(IReadOnlyList<int> indizi)
    {
        for (var livello = 1; livello <= 4; livello++)
        {
            if (new RisolutoreLogico(SudokuUnita(), 9, indizi.ToArray(), livello, sudoku: true).Risolve())
            {
                return livello;
            }
        }

        return Massima;
    }

    public static int Futoshiki(int dimensione, IReadOnlyList<int> indizi, IReadOnlyList<SegnoFutoshiki> segni)
    {
        for (var livello = 1; livello <= 4; livello++)
        {
            var solver = new RisolutoreLogico(FutoshikiUnita(dimensione), dimensione, indizi.ToArray(), livello, false)
            {
                Segni = [.. segni.Select(s => ((s.RigaA * dimensione) + s.ColonnaA, (s.RigaB * dimensione) + s.ColonnaB))],
            };
            if (solver.Risolve())
            {
                return livello;
            }
        }

        return Massima;
    }

    public static int Nonogramma(
        int righe,
        int colonne,
        IReadOnlyList<IReadOnlyList<int>> indiziRighe,
        IReadOnlyList<IReadOnlyList<int>> indiziColonne)
    {
        if (SovrapposizioneSemplice(righe, colonne, indiziRighe, indiziColonne))
        {
            return 1;
        }

        var motore = new NonogrammaRisolutore.Motore(righe, colonne, indiziRighe, indiziColonne);
        var stato = motore.Iniziale();
        if (!motore.Valido || !motore.Propaga(stato))
        {
            return Massima;
        }

        if (!stato.Contains(NonogrammaRisolutore.Motore.Ignoto))
        {
            return 2;
        }

        var prove = 0;
        var progresso = true;
        while (progresso && stato.Contains(NonogrammaRisolutore.Motore.Ignoto))
        {
            progresso = false;
            for (var i = 0; i < stato.Length; i++)
            {
                if (stato[i] != NonogrammaRisolutore.Motore.Ignoto)
                {
                    continue;
                }

                foreach (var ipotesi in new[] { NonogrammaRisolutore.Motore.Pieno, NonogrammaRisolutore.Motore.Vuoto })
                {
                    prove++;
                    var copia = (byte[])stato.Clone();
                    copia[i] = ipotesi;
                    if (motore.Propaga(copia))
                    {
                        continue;
                    }

                    stato[i] = ipotesi == NonogrammaRisolutore.Motore.Pieno
                        ? NonogrammaRisolutore.Motore.Vuoto
                        : NonogrammaRisolutore.Motore.Pieno;
                    progresso = motore.Propaga(stato);
                    break;
                }

                if (!progresso)
                {
                    break;
                }
            }
        }

        if (stato.Contains(NonogrammaRisolutore.Motore.Ignoto))
        {
            return Massima;
        }

        return prove <= 10 ? 3 : 4;
    }

    // Livello 1: sovrapposizione dei blocchi, linee vuote o piene e chiusura dei blocchi completi.
    private static bool SovrapposizioneSemplice(
        int righe, int colonne, IReadOnlyList<IReadOnlyList<int>> ir, IReadOnlyList<IReadOnlyList<int>> ic)
    {
        var stato = new byte[righe * colonne];
        var cambiato = true;
        while (cambiato)
        {
            cambiato = false;
            for (var l = 0; l < righe + colonne; l++)
            {
                var riga = l < righe;
                var n = riga ? colonne : righe;
                var fisso = riga ? l : l - righe;
                var blocchi = (riga ? ir[l] : ic[l - righe]).Where(x => x > 0).ToArray();
                int Indice(int k) => riga ? (fisso * colonne) + k : (k * colonne) + fisso;

                var piene = 0;
                for (var k = 0; k < n; k++)
                {
                    piene += stato[Indice(k)] == 1 ? 1 : 0;
                }

                if (blocchi.Length == 0 || piene == blocchi.Sum())
                {
                    for (var k = 0; k < n; k++)
                    {
                        if (stato[Indice(k)] == 0)
                        {
                            stato[Indice(k)] = 2;
                            cambiato = true;
                        }
                    }

                    continue;
                }

                var sinistra = 0;
                var inizi = new int[blocchi.Length];
                for (var j = 0; j < blocchi.Length; j++)
                {
                    inizi[j] = sinistra;
                    sinistra += blocchi[j] + 1;
                }

                var destra = n;
                for (var j = blocchi.Length - 1; j >= 0; j--)
                {
                    destra -= blocchi[j];
                    for (var k = destra; k < inizi[j] + blocchi[j]; k++)
                    {
                        if (stato[Indice(k)] == 0)
                        {
                            stato[Indice(k)] = 1;
                            cambiato = true;
                        }
                    }

                    destra--;
                }
            }
        }

        return stato.All(x => x != 0)
            && NonogrammaRisolutore.ContaSoluzioni(righe, colonne, ir, ic, 2) == 1;
    }

    private static int[][] SudokuUnita()
    {
        var unita = new List<int[]>();
        for (var a = 0; a < 9; a++)
        {
            unita.Add([.. Enumerable.Range(0, 9).Select(b => (a * 9) + b)]);
        }

        for (var a = 0; a < 9; a++)
        {
            unita.Add([.. Enumerable.Range(0, 9).Select(b => (b * 9) + a)]);
        }

        for (var a = 0; a < 9; a++)
        {
            unita.Add([.. Enumerable.Range(0, 9).Select(b => ((((a / 3) * 3) + (b / 3)) * 9) + ((a % 3) * 3) + (b % 3))]);
        }

        return [.. unita];
    }

    private static int[][] FutoshikiUnita(int n)
    {
        var unita = new List<int[]>();
        for (var a = 0; a < n; a++)
        {
            unita.Add([.. Enumerable.Range(0, n).Select(b => (a * n) + b)]);
        }

        for (var a = 0; a < n; a++)
        {
            unita.Add([.. Enumerable.Range(0, n).Select(b => (b * n) + a)]);
        }

        return [.. unita];
    }

    /// <summary>
    /// Risolutore a candidati (bit 1..cifre). Tecniche per livello: 1 singolo nascosto (futoshiki: anche
    /// nudo e limiti dei segni al 2); 2 singolo nudo; 3 coppie, intersezioni; 4 terne, X-wing.
    /// </summary>
    private sealed class RisolutoreLogico
    {
        private readonly int[][] _unita;
        private readonly int _cifre;
        private readonly int[] _cand;
        private readonly int[] _valori;
        private readonly int _livello;
        private readonly bool _sudoku;
        private readonly List<int>[] _pari;

        public RisolutoreLogico(int[][] unita, int cifre, int[] indizi, int livello, bool sudoku)
        {
            _unita = unita;
            _cifre = cifre;
            _livello = livello;
            _sudoku = sudoku;
            _valori = new int[indizi.Length];
            _cand = new int[indizi.Length];
            _pari = new List<int>[indizi.Length];
            var tutti = ((1 << (cifre + 1)) - 1) & ~1;
            Array.Fill(_cand, tutti);
            for (var i = 0; i < indizi.Length; i++)
            {
                _pari[i] = [.. unita.Where(u => u.Contains(i)).SelectMany(u => u).Where(j => j != i).Distinct()];
            }

            Iniziali = indizi;
        }

        public (int A, int B)[] Segni { get; init; } = [];

        private int[] Iniziali { get; }

        public bool Risolve()
        {
            for (var i = 0; i < Iniziali.Length; i++)
            {
                if (Iniziali[i] != 0)
                {
                    Metti(i, Iniziali[i]);
                }
            }

            var progresso = true;
            while (progresso)
            {
                progresso = Tecniche();
            }

            return _valori.All(v => v != 0);
        }

        private void Metti(int i, int v)
        {
            _valori[i] = v;
            _cand[i] = 0;
            foreach (var j in _pari[i])
            {
                _cand[j] &= ~(1 << v);
            }
        }

        private bool Tecniche()
        {
            if (SingoloNascosto() || (!_sudoku && SingoloNudo()))
            {
                return true;
            }

            if (_livello >= 2 && (_sudoku ? SingoloNudo() : Limiti()))
            {
                return true;
            }

            if (_livello >= 3 && (SottoinsiemiNudi(2) || SottoinsiemiNascosti(2) || (_sudoku && Intersezioni())))
            {
                return true;
            }

            return _livello >= 4 && (SottoinsiemiNudi(3) || SottoinsiemiNascosti(3) || (_sudoku && XWing()));
        }

        private bool SingoloNascosto()
        {
            foreach (var u in _unita)
            {
                for (var v = 1; v <= _cifre; v++)
                {
                    var dove = -1;
                    var conta = 0;
                    foreach (var i in u)
                    {
                        if ((_cand[i] & (1 << v)) != 0)
                        {
                            conta++;
                            dove = i;
                        }
                    }

                    if (conta == 1)
                    {
                        Metti(dove, v);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool SingoloNudo()
        {
            for (var i = 0; i < _cand.Length; i++)
            {
                if (_valori[i] == 0 && BitOperations.PopCount((uint)_cand[i]) == 1)
                {
                    Metti(i, BitOperations.TrailingZeroCount(_cand[i]));
                    return true;
                }
            }

            return false;
        }

        // Futoshiki: con a > b, a non può valere il minimo di b e b non il massimo di a.
        private bool Limiti()
        {
            var cambiato = false;
            foreach (var (a, b) in Segni)
            {
                var domA = _valori[a] != 0 ? 1 << _valori[a] : _cand[a];
                var domB = _valori[b] != 0 ? 1 << _valori[b] : _cand[b];
                if (domA == 0 || domB == 0)
                {
                    continue;
                }

                var minB = BitOperations.TrailingZeroCount(domB);
                var maxA = 31 - BitOperations.LeadingZeroCount((uint)domA);
                if (_valori[a] == 0)
                {
                    var nuovo = _cand[a] & ~((1 << (minB + 1)) - 1);
                    cambiato |= nuovo != _cand[a];
                    _cand[a] = nuovo;
                }

                if (_valori[b] == 0)
                {
                    var nuovo = _cand[b] & ((1 << maxA) - 1);
                    cambiato |= nuovo != _cand[b];
                    _cand[b] = nuovo;
                }
            }

            return cambiato;
        }

        private bool SottoinsiemiNudi(int k)
        {
            foreach (var u in _unita)
            {
                var celle = u.Where(i => _valori[i] == 0 && BitOperations.PopCount((uint)_cand[i]) <= k).ToArray();
                foreach (var gruppo in Combinazioni(celle, k))
                {
                    var unione = gruppo.Aggregate(0, (m, i) => m | _cand[i]);
                    if (BitOperations.PopCount((uint)unione) != k)
                    {
                        continue;
                    }

                    var cambiato = false;
                    foreach (var i in u)
                    {
                        if (_valori[i] == 0 && !gruppo.Contains(i) && (_cand[i] & unione) != 0)
                        {
                            _cand[i] &= ~unione;
                            cambiato = true;
                        }
                    }

                    if (cambiato)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool SottoinsiemiNascosti(int k)
        {
            var cifre = Enumerable.Range(1, _cifre).ToArray();
            foreach (var u in _unita)
            {
                foreach (var gruppo in Combinazioni(cifre, k))
                {
                    var maschera = gruppo.Aggregate(0, (m, v) => m | (1 << v));
                    var celle = u.Where(i => _valori[i] == 0 && (_cand[i] & maschera) != 0).ToArray();
                    if (celle.Length != k || gruppo.Any(v => !celle.Any(i => (_cand[i] & (1 << v)) != 0)))
                    {
                        continue;
                    }

                    var cambiato = false;
                    foreach (var i in celle)
                    {
                        var nuovo = _cand[i] & maschera;
                        cambiato |= nuovo != _cand[i];
                        _cand[i] = nuovo;
                    }

                    if (cambiato)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Sudoku: pointing (riquadro verso linea) e claiming (linea verso riquadro).
        private bool Intersezioni()
        {
            for (var v = 1; v <= 9; v++)
            {
                for (var a = 0; a < 27; a++)
                {
                    for (var b = 0; b < 27; b++)
                    {
                        if (a / 9 == b / 9 || (a / 9 != 2 && b / 9 != 2))
                        {
                            continue;
                        }

                        var (origine, bersaglio) = (_unita[a], _unita[b]);
                        var inOrigine = origine.Where(i => (_cand[i] & (1 << v)) != 0).ToArray();
                        if (inOrigine.Length < 2 || !inOrigine.All(i => bersaglio.Contains(i)))
                        {
                            continue;
                        }

                        var cambiato = false;
                        foreach (var i in bersaglio.Where(i => !origine.Contains(i) && (_cand[i] & (1 << v)) != 0))
                        {
                            _cand[i] &= ~(1 << v);
                            cambiato = true;
                        }

                        if (cambiato)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool XWing()
        {
            for (var v = 1; v <= 9; v++)
            {
                for (var offset = 0; offset <= 9; offset += 9)
                {
                    var linee = Enumerable.Range(0, 9)
                        .Select(a => (Linea: a, Pos: _unita[offset + a].Where(i => (_cand[i] & (1 << v)) != 0).ToArray()))
                        .Where(x => x.Pos.Length == 2)
                        .ToArray();
                    for (var x = 0; x < linee.Length; x++)
                    {
                        for (var y = x + 1; y < linee.Length; y++)
                        {
                            var trasversali = (offset == 0 ? Enumerable.Range(0, 9).Select(b => 9 + b) : Enumerable.Range(0, 9))
                                .Select(idx => _unita[idx]).ToArray();
                            var colonne = linee[x].Pos.Select(i => offset == 0 ? i % 9 : i / 9).ToArray();
                            var altre = linee[y].Pos.Select(i => offset == 0 ? i % 9 : i / 9).ToArray();
                            if (!colonne.SequenceEqual(altre))
                            {
                                continue;
                            }

                            var cambiato = false;
                            foreach (var c in colonne)
                            {
                                foreach (var i in trasversali[c].Where(i =>
                                    (_cand[i] & (1 << v)) != 0 &&
                                    (offset == 0 ? i / 9 : i % 9) != linee[x].Linea &&
                                    (offset == 0 ? i / 9 : i % 9) != linee[y].Linea))
                                {
                                    _cand[i] &= ~(1 << v);
                                    cambiato = true;
                                }
                            }

                            if (cambiato)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static IEnumerable<int[]> Combinazioni(int[] elementi, int k)
        {
            var indici = new int[k];
            IEnumerable<int[]> Ricorri(int inizio, int profondita)
            {
                if (profondita == k)
                {
                    yield return (int[])indici.Clone();
                    yield break;
                }

                for (var i = inizio; i < elementi.Length; i++)
                {
                    indici[profondita] = elementi[i];
                    foreach (var c in Ricorri(i + 1, profondita + 1))
                    {
                        yield return c;
                    }
                }
            }

            return Ricorri(0, 0);
        }
    }
}
