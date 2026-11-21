using System.Text.Json.Nodes;
using DBEnigmistica.Application.Creazione.Algoritmi;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Algoritmi;

/// <summary>Confronto con risolutori ingenui scritti qui e verifica dell'unicità sui <c>dati</c> salvati.</summary>
public class ControlliIndipendentiTests
{
    // ---------- Sudoku ----------

    private static int ForzaSudoku(int[] g, int pos, int limite)
    {
        while (pos < 81 && g[pos] != 0)
        {
            pos++;
        }

        if (pos == 81)
        {
            return 1;
        }

        var totale = 0;
        for (var v = 1; v <= 9 && totale < limite; v++)
        {
            if (!SudokuCompatibile(g, pos, v))
            {
                continue;
            }

            g[pos] = v;
            totale += ForzaSudoku(g, pos + 1, limite - totale);
            g[pos] = 0;
        }

        return totale;
    }

    private static bool SudokuCompatibile(int[] g, int pos, int v)
    {
        int r = pos / 9, c = pos % 9;
        for (var k = 0; k < 9; k++)
        {
            if (g[(r * 9) + k] == v || g[(k * 9) + c] == v ||
                g[((((r / 3) * 3) + (k / 3)) * 9) + ((c / 3) * 3) + (k % 3)] == v)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IndiziCoerenti(int[] g)
    {
        for (var i = 0; i < 81; i++)
        {
            if (g[i] == 0)
            {
                continue;
            }

            var v = g[i];
            g[i] = 0;
            var ok = SudokuCompatibile(g, i, v);
            g[i] = v;
            if (!ok)
            {
                return false;
            }
        }

        return true;
    }

    [Fact]
    public void Sudoku_ConfrontoConForzaBruta()
    {
        var visti = new HashSet<int>();
        for (var seme = 1; seme <= 60; seme++)
        {
            var pieno = SudokuGeneratore.Genera(seme, 81).Soluzione.ToArray();
            var casuale = new Random(seme);
            var daTogliere = 8 + (seme % 14);
            var g = (int[])pieno.Clone();
            foreach (var i in Enumerable.Range(0, 81).OrderBy(_ => casuale.Next()).Take(daTogliere))
            {
                g[i] = 0;
            }

            if (seme % 5 == 0)
            {
                var vuota = Array.FindIndex(g, x => x == 0);
                var piena = Array.FindIndex(g, x => x != 0);
                g[vuota] = g[piena];
            }

            var atteso = IndiziCoerenti(g) ? ForzaSudoku((int[])g.Clone(), 0, 2) : 0;
            SudokuRisolutore.ContaSoluzioni(g, 2).Should().Be(atteso, $"seme {seme}");
            visti.Add(atteso);
        }

        visti.Should().Contain([0, 1, 2]);
    }

    // ---------- Futoshiki ----------

    private static int ForzaFutoshiki(int n, int[] indizi, IReadOnlyList<SegnoFutoshiki> segni)
    {
        // Riempie solo le caselle vuote: gli indizi restano fissi.
        var valori = (int[])indizi.Clone();
        return Cerca(0);

        int Cerca(int pos)
        {
            if (pos == n * n)
            {
                return segni.All(s => valori[(s.RigaA * n) + s.ColonnaA] > valori[(s.RigaB * n) + s.ColonnaB]) ? 1 : 0;
            }

            if (indizi[pos] != 0)
            {
                return Valido(pos, valori[pos]) ? Cerca(pos + 1) : 0;
            }

            var totale = 0;
            for (var v = 1; v <= n && totale < 2; v++)
            {
                if (!Valido(pos, v))
                {
                    continue;
                }

                valori[pos] = v;
                totale += Cerca(pos + 1);
            }

            valori[pos] = 0;
            return totale;
        }

        bool Valido(int pos, int v)
        {
            int r = pos / n, c = pos % n;
            for (var k = 0; k < n; k++)
            {
                if ((k < c && valori[(r * n) + k] == v) || (k < r && valori[(k * n) + c] == v))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Theory]
    [InlineData(4, 80)]
    [InlineData(5, 16)]
    public void Futoshiki_ConfrontoConForzaBruta(int n, int istanze)
    {
        var visti = new HashSet<int>();
        for (var seme = 1; seme <= istanze; seme++)
        {
            var casuale = new Random((n * 1000) + seme);
            var soluzione = QuadratoLatino(n, casuale);
            var indizi = new int[n * n];
            var densitaIndizi = (seme % 4) * 0.08;
            for (var i = 0; i < indizi.Length; i++)
            {
                indizi[i] = casuale.NextDouble() < densitaIndizi ? soluzione[i] : 0;
            }

            var segni = new List<SegnoFutoshiki>();
            var densitaSegni = 0.25 + ((seme % 3) * 0.2);
            for (var r = 0; r < n; r++)
            {
                for (var c = 0; c < n; c++)
                {
                    AggiungiSegno(segni, soluzione, n, r, c, r, c + 1, casuale, densitaSegni);
                    AggiungiSegno(segni, soluzione, n, r, c, r + 1, c, casuale, densitaSegni);
                }
            }

            if (seme % 7 == 0 && segni.Count > 0)
            {
                var s = segni[0];
                segni[0] = new SegnoFutoshiki(s.RigaB, s.ColonnaB, s.RigaA, s.ColonnaA);
            }

            var atteso = Math.Min(ForzaFutoshiki(n, indizi, segni), 2);
            FutoshikiRisolutore.ContaSoluzioni(n, indizi, segni, 2).Should().Be(atteso, $"n {n} seme {seme}");
            visti.Add(atteso);
        }

        visti.Should().Contain([1, 2]);
    }

    private static void AggiungiSegno(
        List<SegnoFutoshiki> segni, int[] sol, int n, int r1, int c1, int r2, int c2, Random casuale, double p)
    {
        if (r2 >= n || c2 >= n || casuale.NextDouble() >= p)
        {
            return;
        }

        var maggiore = sol[(r1 * n) + c1] > sol[(r2 * n) + c2];
        segni.Add(maggiore ? new SegnoFutoshiki(r1, c1, r2, c2) : new SegnoFutoshiki(r2, c2, r1, c1));
    }

    private static int[] QuadratoLatino(int n, Random casuale)
    {
        var righe = Enumerable.Range(0, n).OrderBy(_ => casuale.Next()).ToArray();
        var colonne = Enumerable.Range(0, n).OrderBy(_ => casuale.Next()).ToArray();
        var simboli = Enumerable.Range(1, n).OrderBy(_ => casuale.Next()).ToArray();
        var q = new int[n * n];
        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++)
            {
                q[(r * n) + c] = simboli[(righe[r] + colonne[c]) % n];
            }
        }

        return q;
    }

    // ---------- Nonogramma ----------

    private static List<int> Blocchi(IEnumerable<int> linea)
    {
        var res = new List<int>();
        var corrente = 0;
        foreach (var v in linea.Append(0))
        {
            if (v != 0)
            {
                corrente++;
            }
            else if (corrente > 0)
            {
                res.Add(corrente);
                corrente = 0;
            }
        }

        return res;
    }

    private static int ForzaNonogramma(int righe, int colonne, List<List<int>> ir, List<List<int>> ic)
    {
        var modelli = new List<int[]>[righe];
        for (var r = 0; r < righe; r++)
        {
            modelli[r] = [];
            for (var m = 0; m < 1 << colonne; m++)
            {
                var linea = Enumerable.Range(0, colonne).Select(c => (m >> c) & 1).ToArray();
                if (Blocchi(linea).SequenceEqual(ir[r]))
                {
                    modelli[r].Add(linea);
                }
            }
        }

        var scelti = new int[righe][];
        return Cerca(0);

        int Cerca(int r)
        {
            if (r == righe)
            {
                for (var c = 0; c < colonne; c++)
                {
                    if (!Blocchi(Enumerable.Range(0, righe).Select(k => scelti[k][c])).SequenceEqual(ic[c]))
                    {
                        return 0;
                    }
                }

                return 1;
            }

            var totale = 0;
            foreach (var m in modelli[r])
            {
                scelti[r] = m;
                totale += Cerca(r + 1);
                if (totale >= 2)
                {
                    return 2;
                }
            }

            return totale;
        }
    }

    [Fact]
    public void Nonogramma_ConfrontoConForzaBruta()
    {
        var visti = new HashSet<int>();
        for (var seme = 1; seme <= 90; seme++)
        {
            var righe = 5 + (seme % 2);
            var colonne = 5 + ((seme / 2) % 2);
            var casuale = new Random(seme);
            var disegno = Enumerable.Range(0, righe * colonne).Select(_ => casuale.NextDouble() < 0.55 ? 1 : 0).ToArray();
            var ir = Enumerable.Range(0, righe)
                .Select(r => Blocchi(Enumerable.Range(0, colonne).Select(c => disegno[(r * colonne) + c]))).ToList();
            var ic = Enumerable.Range(0, colonne)
                .Select(c => Blocchi(Enumerable.Range(0, righe).Select(r => disegno[(r * colonne) + c]))).ToList();

            if (seme % 6 == 0)
            {
                ir[0] = [1];
            }

            var atteso = ForzaNonogramma(righe, colonne, ir, ic);
            var risolutore = NonogrammaRisolutore.ContaSoluzioni(
                righe, colonne, [.. ir.Select(x => (IReadOnlyList<int>)x)], [.. ic.Select(x => (IReadOnlyList<int>)x)], 2);
            risolutore.Should().Be(atteso, $"seme {seme}");
            visti.Add(atteso);
        }

        visti.Should().Contain([1, 2]);
    }

    // ---------- Unicità sui dati salvati ----------

    private static JsonObject ViaJson(JsonObject dati) => JsonNode.Parse(dati.ToJsonString())!.AsObject();

    private static int[] LeggiGriglia(JsonNode griglia, Func<char, int> valore) =>
        [.. griglia["celle"]!.AsArray().SelectMany(r => r!.GetValue<string>().Select(valore))];

    [Fact]
    public void Sudoku_UnicitaSuiDatiSalvati()
    {
        for (var seme = 1; seme <= 40; seme++)
        {
            var dati = ViaJson(DatiGiochi.Sudoku(SudokuGeneratore.Genera(seme), 1));
            var indizi = LeggiGriglia(dati["griglia"]!, c => c == '.' ? 0 : c - '0');
            var soluzione = LeggiGriglia(dati["soluzione_griglia"]!, c => c - '0');
            VerificaUnicita.Sudoku(indizi).Should().Be(EsitoUnicita.Unica, $"seme {seme}");
            SudokuRisolutore.Risolvi(indizi)!.Should().Equal(soluzione);
        }
    }

    [Fact]
    public void Futoshiki_UnicitaSuiDatiSalvati()
    {
        for (var seme = 1; seme <= 15; seme++)
        {
            var n = 5 + (seme % 3);
            var dati = ViaJson(DatiGiochi.Futoshiki(FutoshikiGeneratore.Genera(seme, n), 1));
            var indizi = LeggiGriglia(dati["griglia"]!, c => c == '.' ? 0 : c - '0');
            var soluzione = LeggiGriglia(dati["soluzione_griglia"]!, c => c - '0');
            var segni = dati["segni"]!.AsArray().Select(s =>
            {
                var r = s!["riga"]!.GetValue<int>() - 1;
                var c = s["colonna"]!.GetValue<int>() - 1;
                var (r2, c2) = s["verso"]!.GetValue<string>() == "destra" ? (r, c + 1) : (r + 1, c);
                return s["segno"]!.GetValue<string>() == "maggiore"
                    ? new SegnoFutoshiki(r, c, r2, c2)
                    : new SegnoFutoshiki(r2, c2, r, c);
            }).ToList();

            VerificaUnicita.Futoshiki(n, indizi, segni).Should().Be(EsitoUnicita.Unica, $"seme {seme}");
            FutoshikiRisolutore.Risolvi(n, indizi, segni)!.Should().Equal(soluzione);
        }
    }

    [Fact]
    public void Nonogramma_UnicitaSuiDatiSalvati()
    {
        for (var seme = 1; seme <= 40; seme++)
        {
            var lato = 10 + (seme % 6);
            var dati = ViaJson(DatiGiochi.Nonogramma(NonogrammaGeneratore.Genera(seme, lato, lato), 1));
            var disegno = LeggiGriglia(dati["soluzione_griglia"]!, c => c == '#' ? 1 : 0);
            IReadOnlyList<IReadOnlyList<int>> Leggi(string chiave) => [.. dati[chiave]!.AsArray().Select(v =>
                (IReadOnlyList<int>)(v!["blocchi"]!.GetValue<string>() == "0"
                    ? []
                    : v["blocchi"]!.GetValue<string>().Split(' ').Select(int.Parse).ToList()))];

            var righe = Leggi("indizi_righe");
            var colonne = Leggi("indizi_colonne");
            var soluzioni = NonogrammaRisolutore.Risolvi(lato, lato, righe, colonne);
            soluzioni.Should().HaveCount(1, $"seme {seme}");
            soluzioni[0].Should().Equal(disegno);
        }
    }

    [Fact]
    public void Schema_DatiSalvatiRiconducibiliAlloSchemaValido()
    {
        for (var seme = 1; seme <= 60; seme++)
        {
            var n = 9 + (seme % 5);
            var originale = SchemaCruciverbaGeneratore.Genera(seme, n);
            var dati = ViaJson(DatiGiochi.Schema(originale));
            var nere = dati["celle"]!.AsArray().SelectMany(r => r!.GetValue<string>().Select(c => c == '#')).ToArray();
            var (numeri, orizzontali, verticali) = SchemaCruciverbaGeneratore.Numera(nere, n);
            var ricostruito = new SchemaCruciverba(dati["seme"]!.GetValue<long>(), n, nere, numeri, orizzontali, verticali);

            SchemaCruciverbaValidatore.Valida(ricostruito).Should().BeEmpty($"seme {seme}");
            dati["numeri"]!.AsArray().Select(x => x!["numero"]!.GetValue<int>())
                .Should().Equal(numeri.Where(x => x != 0));
            dati["parole"]!.AsArray().Count.Should().Be(orizzontali.Count + verticali.Count);
        }
    }
}
