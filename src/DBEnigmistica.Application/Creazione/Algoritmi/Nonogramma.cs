namespace DBEnigmistica.Application.Creazione.Algoritmi;

/// <summary>Nonogramma: disegno e soluzione in ordine di riga (1 = piena); una linea vuota ha indizio vuoto.</summary>
public sealed record NonogrammaGioco(
    long Seme,
    int Righe,
    int Colonne,
    IReadOnlyList<IReadOnlyList<int>> IndiziRighe,
    IReadOnlyList<IReadOnlyList<int>> IndiziColonne,
    IReadOnlyList<int> Soluzione);

public static class NonogrammaRisolutore
{
    public const int LatoMassimo = 32;

    /// <summary>Indizi (lunghezze dei blocchi) di una linea di caselle piene (valori 1) e vuote (0).</summary>
    public static List<int> CalcolaIndizi(ReadOnlySpan<int> linea)
    {
        var indizi = new List<int>();
        var corrente = 0;
        foreach (var v in linea)
        {
            if (v != 0)
            {
                corrente++;
            }
            else if (corrente > 0)
            {
                indizi.Add(corrente);
                corrente = 0;
            }
        }

        if (corrente > 0)
        {
            indizi.Add(corrente);
        }

        return indizi;
    }

    public static (List<IReadOnlyList<int>> Righe, List<IReadOnlyList<int>> Colonne) CalcolaIndizi(
        IReadOnlyList<int> disegno, int righe, int colonne)
    {
        var ir = new List<IReadOnlyList<int>>();
        var ic = new List<IReadOnlyList<int>>();
        var buffer = new int[Math.Max(righe, colonne)];
        for (var r = 0; r < righe; r++)
        {
            for (var c = 0; c < colonne; c++)
            {
                buffer[c] = disegno[(r * colonne) + c];
            }

            ir.Add(CalcolaIndizi(buffer.AsSpan(0, colonne)));
        }

        for (var c = 0; c < colonne; c++)
        {
            for (var r = 0; r < righe; r++)
            {
                buffer[r] = disegno[(r * colonne) + c];
            }

            ic.Add(CalcolaIndizi(buffer.AsSpan(0, righe)));
        }

        return (ir, ic);
    }

    public static int ContaSoluzioni(
        int righe,
        int colonne,
        IReadOnlyList<IReadOnlyList<int>> indiziRighe,
        IReadOnlyList<IReadOnlyList<int>> indiziColonne,
        int limite = 2) =>
        Risolvi(righe, colonne, indiziRighe, indiziColonne, limite).Count;

    /// <summary>Fino a <paramref name="limite"/> soluzioni (1 = piena, 0 = vuota).</summary>
    public static List<int[]> Risolvi(
        int righe,
        int colonne,
        IReadOnlyList<IReadOnlyList<int>> indiziRighe,
        IReadOnlyList<IReadOnlyList<int>> indiziColonne,
        int limite = 2)
    {
        if (righe is < 1 or > LatoMassimo || colonne is < 1 or > LatoMassimo)
        {
            throw new ArgumentOutOfRangeException(nameof(righe), "Lato del nonogramma fuori intervallo.");
        }

        if (indiziRighe.Count != righe || indiziColonne.Count != colonne)
        {
            throw new ArgumentException("Il numero di indizi non corrisponde alle dimensioni.");
        }

        var soluzioni = new List<int[]>();
        var motore = new Motore(righe, colonne, indiziRighe, indiziColonne);
        if (motore.Valido)
        {
            motore.Cerca(motore.Iniziale(), limite, soluzioni);
        }

        return soluzioni;
    }

    internal sealed class Motore
    {
        internal const byte Ignoto = 0;
        internal const byte Pieno = 1;
        internal const byte Vuoto = 2;

        private readonly int _righe;
        private readonly int _colonne;
        private readonly int[][] _indizi;
        private readonly int[][] _indiziInversi;
        private readonly bool[] _tabAvanti;
        private readonly bool[] _tabIndietro;
        private readonly byte[] _linea;
        private readonly byte[] _lineaInversa;
        private readonly int[] _vuotiPrefissi;
        private readonly int[] _differenze;
        private readonly bool[] _puoVuota;

        public Motore(
            int righe, int colonne, IReadOnlyList<IReadOnlyList<int>> ir, IReadOnlyList<IReadOnlyList<int>> ic)
        {
            _righe = righe;
            _colonne = colonne;
            _indizi = new int[righe + colonne][];
            Valido = true;
            for (var i = 0; i < righe + colonne; i++)
            {
                var lunghezza = i < righe ? colonne : righe;
                var sorgente = i < righe ? ir[i] : ic[i - righe];
                _indizi[i] = [.. sorgente.Where(x => x > 0)];
                if (sorgente.Any(x => x < 0) || _indizi[i].Sum() + _indizi[i].Length - 1 > lunghezza)
                {
                    Valido = false;
                }
            }

            _indiziInversi = [.. _indizi.Select(x => x.Reverse().ToArray())];
            var max = Math.Max(righe, colonne);
            var maxBlocchi = _indizi.Max(x => x.Length);
            var dimTab = (max + 1) * (maxBlocchi + 1);
            _tabAvanti = new bool[dimTab];
            _tabIndietro = new bool[dimTab];
            _linea = new byte[max];
            _lineaInversa = new byte[max];
            _vuotiPrefissi = new int[max + 1];
            _differenze = new int[max + 1];
            _puoVuota = new bool[max];
        }

        public bool Valido { get; }

        public byte[] Iniziale() => new byte[_righe * _colonne];

        public void Cerca(byte[] stato, int limite, List<int[]> soluzioni)
        {
            if (!Propaga(stato))
            {
                return;
            }

            var ignoto = Array.IndexOf(stato, Ignoto);
            if (ignoto < 0)
            {
                soluzioni.Add([.. stato.Select(x => x == Pieno ? 1 : 0)]);
                return;
            }

            foreach (var valore in new[] { Pieno, Vuoto })
            {
                var copia = (byte[])stato.Clone();
                copia[ignoto] = valore;
                Cerca(copia, limite, soluzioni);
                if (soluzioni.Count >= limite)
                {
                    return;
                }
            }
        }

        internal bool Propaga(byte[] stato)
        {
            var totale = _righe + _colonne;
            var inCoda = new bool[totale];
            var coda = new Queue<int>(totale);
            for (var i = 0; i < totale; i++)
            {
                coda.Enqueue(i);
                inCoda[i] = true;
            }

            while (coda.Count > 0)
            {
                var l = coda.Dequeue();
                inCoda[l] = false;
                var riga = l < _righe;
                var lunghezza = riga ? _colonne : _righe;
                var fisso = riga ? l : l - _righe;

                for (var k = 0; k < lunghezza; k++)
                {
                    _linea[k] = stato[riga ? (fisso * _colonne) + k : (k * _colonne) + fisso];
                }

                if (!RisolviLinea(l, lunghezza))
                {
                    return false;
                }

                for (var k = 0; k < lunghezza; k++)
                {
                    var i = riga ? (fisso * _colonne) + k : (k * _colonne) + fisso;
                    if (stato[i] == _linea[k])
                    {
                        continue;
                    }

                    stato[i] = _linea[k];
                    var incrocio = riga ? _righe + k : k;
                    if (!inCoda[incrocio])
                    {
                        inCoda[incrocio] = true;
                        coda.Enqueue(incrocio);
                    }
                }
            }

            return true;
        }

        // Aggiorna _linea con le deduzioni; false se la linea è contraddittoria.
        private bool RisolviLinea(int l, int n)
        {
            var blocchi = _indizi[l];
            var k = blocchi.Length;
            var stride = k + 1;

            for (var i = 0; i < n; i++)
            {
                _lineaInversa[i] = _linea[n - 1 - i];
            }

            Prefissi(_linea, n, blocchi, _tabAvanti, stride);
            Prefissi(_lineaInversa, n, _indiziInversi[l], _tabIndietro, stride);
            if (!_tabAvanti[(n * stride) + k])
            {
                return false;
            }

            _vuotiPrefissi[0] = 0;
            for (var i = 0; i < n; i++)
            {
                _vuotiPrefissi[i + 1] = _vuotiPrefissi[i] + (_linea[i] == Vuoto ? 1 : 0);
                _differenze[i] = 0;
                _puoVuota[i] = false;
            }

            _differenze[n] = 0;

            for (var i = 0; i < n; i++)
            {
                if (_linea[i] == Pieno)
                {
                    continue;
                }

                for (var j = 0; j <= k; j++)
                {
                    if (_tabAvanti[(i * stride) + j] && _tabIndietro[((n - i - 1) * stride) + (k - j)])
                    {
                        _puoVuota[i] = true;
                        break;
                    }
                }
            }

            for (var j = 1; j <= k; j++)
            {
                var len = blocchi[j - 1];
                for (var s = 0; s + len <= n; s++)
                {
                    var e = s + len;
                    if (_vuotiPrefissi[e] - _vuotiPrefissi[s] != 0)
                    {
                        continue;
                    }

                    var sinistra = s == 0 ? j == 1 : _linea[s - 1] != Pieno && _tabAvanti[((s - 1) * stride) + (j - 1)];
                    if (!sinistra)
                    {
                        continue;
                    }

                    var destra = e == n ? j == k : _linea[e] != Pieno && _tabIndietro[((n - e - 1) * stride) + (k - j)];
                    if (destra)
                    {
                        _differenze[s]++;
                        _differenze[e]--;
                    }
                }
            }

            var piena = 0;
            for (var i = 0; i < n; i++)
            {
                piena += _differenze[i];
                var puoPiena = piena > 0;
                if (!puoPiena && !_puoVuota[i])
                {
                    return false;
                }

                if (_linea[i] == Ignoto)
                {
                    if (puoPiena && !_puoVuota[i])
                    {
                        _linea[i] = Pieno;
                    }
                    else if (!puoPiena && _puoVuota[i])
                    {
                        _linea[i] = Vuoto;
                    }
                }
                else if (_linea[i] == Pieno ? !puoPiena : !_puoVuota[i])
                {
                    return false;
                }
            }

            return true;
        }

        // tab[i, j]: le prime i caselle possono contenere esattamente i primi j blocchi.
        private void Prefissi(byte[] celle, int n, int[] blocchi, bool[] tab, int stride)
        {
            var k = blocchi.Length;
            var vuoti = new int[n + 1];
            for (var i = 0; i < n; i++)
            {
                vuoti[i + 1] = vuoti[i] + (celle[i] == Vuoto ? 1 : 0);
            }

            for (var j = 0; j <= k; j++)
            {
                tab[j] = j == 0;
            }

            for (var i = 1; i <= n; i++)
            {
                for (var j = 0; j <= k; j++)
                {
                    var ok = celle[i - 1] != Pieno && tab[((i - 1) * stride) + j];
                    if (!ok && j >= 1)
                    {
                        var s = i - blocchi[j - 1];
                        if (s >= 0 && vuoti[i] - vuoti[s] == 0)
                        {
                            ok = s == 0 ? j == 1 : celle[s - 1] != Pieno && tab[((s - 1) * stride) + (j - 1)];
                        }
                    }

                    tab[(i * stride) + j] = ok;
                }
            }
        }
    }
}

public static class NonogrammaGeneratore
{
    private const int TentativiRiparazione = 40;

    /// <summary>
    /// Genera un nonogramma a soluzione unica: disegno casuale con densità data, riparato cambiando
    /// una casella dove due soluzioni differiscono, finché la soluzione è unica.
    /// </summary>
    public static NonogrammaGioco Genera(long seme, int righe = 10, int colonne = 10, double densita = 0.6)
    {
        if (righe is < 10 or > 15 || colonne is < 10 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(righe), "Il nonogramma generato ha lato da 10 a 15.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(densita, 0.3);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(densita, 0.8);

        var casuale = new GeneratoreCasuale(seme);
        while (true)
        {
            var disegno = new int[righe * colonne];
            for (var i = 0; i < disegno.Length; i++)
            {
                disegno[i] = casuale.ProssimoDouble() < densita ? 1 : 0;
            }

            for (var tentativo = 0; tentativo < TentativiRiparazione; tentativo++)
            {
                var (ir, ic) = NonogrammaRisolutore.CalcolaIndizi(disegno, righe, colonne);
                var soluzioni = NonogrammaRisolutore.Risolvi(righe, colonne, ir, ic, 2);
                if (soluzioni.Count == 1)
                {
                    return new NonogrammaGioco(seme, righe, colonne, ir, ic, disegno);
                }

                var altra = soluzioni.First(s => !s.AsSpan().SequenceEqual(disegno));
                var diverse = Enumerable.Range(0, disegno.Length).Where(i => altra[i] != disegno[i]).ToArray();
                var scelta = diverse[casuale.Prossimo(diverse.Length)];
                disegno[scelta] = 1 - disegno[scelta];
            }
        }
    }
}
