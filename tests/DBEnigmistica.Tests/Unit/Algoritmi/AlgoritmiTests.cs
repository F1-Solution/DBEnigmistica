using System.Collections.Concurrent;
using DBEnigmistica.Application.Creazione.Algoritmi;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Algoritmi;

public class GeneratoreCasualeTests
{
    [Fact]
    public void StessoSeme_StessaSequenza_ConValoreRegistrato()
    {
        var a = new GeneratoreCasuale(42);
        var b = new GeneratoreCasuale(42);
        var sequenzaA = Enumerable.Range(0, 20).Select(_ => a.ProssimoU64()).ToArray();
        var sequenzaB = Enumerable.Range(0, 20).Select(_ => b.ProssimoU64()).ToArray();
        sequenzaA.Should().Equal(sequenzaB);
        new GeneratoreCasuale(0).ProssimoU64().Should().Be(0xE220A8397B1DCDAFUL);
    }

    [Fact]
    public void Prossimo_RestaNelloIntervalloEMescolaConservaGliElementi()
    {
        var g = new GeneratoreCasuale(7);
        Enumerable.Range(0, 1000).Select(_ => g.Prossimo(9)).Should().OnlyContain(x => x >= 0 && x < 9);
        g.ProssimoDouble().Should().BeInRange(0, 1);
        var v = Enumerable.Range(0, 10).ToArray();
        g.Mescola<int>(v);
        v.OrderBy(x => x).Should().Equal(Enumerable.Range(0, 10));
        var act = () => g.Prossimo(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class SudokuTests
{
    private const string Esempio =
        "530070000600195000098000060800060003400803001700020006060000280000419005000080079";

    private static int[] Leggi(string s) => s.Select(ch => ch - '0').ToArray();

    [Fact]
    public void Risolutore_GrigliaNota_HaUnaSoluzione()
    {
        SudokuRisolutore.ContaSoluzioni(Leggi(Esempio)).Should().Be(1);
        SudokuRisolutore.Risolvi(Leggi(Esempio))!.Should().Equal(Leggi(
            "534678912672195348198342567859761423426853791713924856961537284287419635345286179"));
    }

    [Fact]
    public void Risolutore_IndiziInConflitto_NessunaSoluzione()
    {
        var g = Leggi(Esempio);
        g[2] = 5;
        SudokuRisolutore.ContaSoluzioni(g).Should().Be(0);
        SudokuRisolutore.Risolvi(g).Should().BeNull();
    }

    [Fact]
    public void Risolutore_SenzaSoluzione_PerCasellaBloccata()
    {
        var g = new int[81];
        for (var c = 0; c < 8; c++)
        {
            g[c] = c + 1;
        }

        g[9 + 8] = 9;
        SudokuRisolutore.ContaSoluzioni(g).Should().Be(0);
    }

    [Fact]
    public void Risolutore_GrigliaVuota_HaMolteSoluzioniEIlLimiteSiRispetta()
    {
        SudokuRisolutore.ContaSoluzioni(new int[81]).Should().Be(2);
        SudokuRisolutore.ContaSoluzioni(new int[81], 5).Should().Be(5);
        SudokuRisolutore.Risolvi(new int[81]).Should().NotBeNull();
    }

    [Fact]
    public void Risolutore_ValoriNonValidi_Lancia()
    {
        var corta = () => SudokuRisolutore.ContaSoluzioni(new int[80]);
        corta.Should().Throw<ArgumentException>();
        var g = new int[81];
        g[0] = 10;
        var fuori = () => SudokuRisolutore.ContaSoluzioni(g);
        fuori.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Generatore_StessoSeme_StessaGriglia()
    {
        var a = SudokuGeneratore.Genera(123);
        var b = SudokuGeneratore.Genera(123);
        a.Indizi.Should().Equal(b.Indizi);
        a.Soluzione.Should().Equal(b.Soluzione);
        a.Seme.Should().Be(123);
        SudokuGeneratore.Genera(124).Indizi.Should().NotEqual(a.Indizi);
    }

    [Fact]
    public void Generatore_ParametriFuoriIntervallo_Lancia()
    {
        var pochi = () => SudokuGeneratore.Genera(1, 10);
        pochi.Should().Throw<ArgumentOutOfRangeException>();
        var troppi = () => SudokuGeneratore.Genera(1, 82);
        troppi.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Proprieta_MilleSudoku_HannoUnaSolaSoluzione()
    {
        var falliti = new ConcurrentBag<long>();
        Parallel.For(1, 1001, seme =>
        {
            var g = SudokuGeneratore.Genera(seme);
            var ok = SudokuRisolutore.ContaSoluzioni(g.Indizi) == 1
                && SudokuRisolutore.Risolvi(g.Indizi)!.SequenceEqual(g.Soluzione)
                && g.Indizi.Count(x => x != 0) >= 17
                && g.Indizi.Zip(g.Soluzione).All(p => p.First == 0 || p.First == p.Second);
            if (!ok)
            {
                falliti.Add(seme);
            }
        });
        falliti.Should().BeEmpty();
    }
}

public class FutoshikiTests
{
    [Fact]
    public void Risolutore_ZeroUnaEMolteSoluzioni()
    {
        // Quadrato 4x4 senza vincoli: molte soluzioni.
        FutoshikiRisolutore.ContaSoluzioni(4, new int[16], []).Should().Be(2);

        // Tre indizi nella prima riga e due nella seconda determinano il quadrato.
        int[] indizi = [1, 2, 3, 4, 2, 1, 4, 3, 0, 0, 0, 0, 0, 0, 0, 0];
        FutoshikiRisolutore.ContaSoluzioni(4, indizi, []).Should().Be(2);
        FutoshikiRisolutore.Risolvi(4, indizi, []).Should().NotBeNull();

        int[] unico = [1, 2, 3, 4, 2, 1, 4, 3, 3, 4, 1, 0, 4, 0, 0, 0];
        FutoshikiRisolutore.ContaSoluzioni(4, unico, []).Should().Be(1);
        FutoshikiRisolutore.Risolvi(4, unico, [])!.Should().Equal(
            1, 2, 3, 4, 2, 1, 4, 3, 3, 4, 1, 2, 4, 3, 2, 1);

        // Due uni nella stessa riga: nessuna soluzione.
        int[] assurdo = [1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        FutoshikiRisolutore.ContaSoluzioni(4, assurdo, []).Should().Be(0);
    }

    [Fact]
    public void Risolutore_ISegniRestringono()
    {
        // 1 in alto a sinistra e segno "A > B" tra (0,1) e (0,0) è impossibile.
        int[] indizi = new int[16];
        indizi[0] = 1;
        var impossibile = new[] { new SegnoFutoshiki(0, 0, 0, 1) };
        FutoshikiRisolutore.ContaSoluzioni(4, indizi, impossibile).Should().Be(0);
    }

    [Fact]
    public void Risolutore_ArgomentiNonValidi_Lancia()
    {
        var piccolo = () => FutoshikiRisolutore.ContaSoluzioni(3, new int[9], []);
        piccolo.Should().Throw<ArgumentOutOfRangeException>();
        var lungo = () => FutoshikiRisolutore.ContaSoluzioni(4, new int[15], []);
        lungo.Should().Throw<ArgumentException>();
        var valore = () => FutoshikiRisolutore.ContaSoluzioni(4, [5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], []);
        valore.Should().Throw<ArgumentException>();
        var segno = () => FutoshikiRisolutore.ContaSoluzioni(4, new int[16], [new SegnoFutoshiki(0, 0, 1, 1)]);
        segno.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Generatore_StessoSeme_StessoGioco()
    {
        var a = FutoshikiGeneratore.Genera(5, 6);
        var b = FutoshikiGeneratore.Genera(5, 6);
        a.Indizi.Should().Equal(b.Indizi);
        a.Segni.Should().Equal(b.Segni);
        a.Soluzione.Should().Equal(b.Soluzione);
        a.Seme.Should().Be(5);
    }

    [Fact]
    public void Generatore_DimensioneFuoriIntervallo_Lancia()
    {
        var act = () => FutoshikiGeneratore.Genera(1, 8);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Proprieta_MilleFutoshiki_HannoUnaSolaSoluzione()
    {
        var falliti = new ConcurrentBag<long>();
        Parallel.For(1, 1001, seme =>
        {
            var dimensione = 5 + (int)(seme % 3);
            var g = FutoshikiGeneratore.Genera(seme, dimensione);
            var ok = FutoshikiRisolutore.ContaSoluzioni(dimensione, g.Indizi, g.Segni) == 1
                && FutoshikiRisolutore.Risolvi(dimensione, g.Indizi, g.Segni)!.SequenceEqual(g.Soluzione);
            if (!ok)
            {
                falliti.Add(seme);
            }
        });
        falliti.Should().BeEmpty();
    }
}

public class NonogrammaTests
{
    private static List<IReadOnlyList<int>> Indizi(params int[][] linee) => [.. linee.Select(l => (IReadOnlyList<int>)l)];

    [Fact]
    public void CalcolaIndizi_DaUnaLinea()
    {
        NonogrammaRisolutore.CalcolaIndizi([1, 1, 0, 1, 0, 0, 1, 1, 1]).Should().Equal(2, 1, 3);
        NonogrammaRisolutore.CalcolaIndizi([0, 0]).Should().BeEmpty();
        var (righe, colonne) = NonogrammaRisolutore.CalcolaIndizi([1, 0, 1, 1], 2, 2);
        righe.Select(x => x.ToArray()).Should().BeEquivalentTo(new[] { new[] { 1 }, new[] { 2 } });
        colonne.Select(x => x.ToArray()).Should().BeEquivalentTo(new[] { new[] { 2 }, new[] { 1 } });
    }

    [Fact]
    public void Risolutore_UnaSoluzione()
    {
        // Lettera "L" 3x3.
        var righe = Indizi([1], [1], [3]);
        var colonne = Indizi([3], [1], [1]);
        var soluzioni = NonogrammaRisolutore.Risolvi(3, 3, righe, colonne);
        soluzioni.Should().HaveCount(1);
        soluzioni[0].Should().Equal(1, 0, 0, 1, 0, 0, 1, 1, 1);
    }

    [Fact]
    public void Risolutore_MoltePossibilita_ScacchieraEUnaDiagonale()
    {
        var uno = Indizi([1], [1]);
        NonogrammaRisolutore.ContaSoluzioni(2, 2, uno, uno).Should().Be(2);
        NonogrammaRisolutore.ContaSoluzioni(2, 2, uno, uno, 1).Should().Be(1);
    }

    [Fact]
    public void Risolutore_NessunaSoluzione()
    {
        // Righe piene ma colonne vuote.
        NonogrammaRisolutore.ContaSoluzioni(2, 2, Indizi([2], [2]), Indizi([], [])).Should().Be(0);
        // Indizio troppo lungo per la linea.
        NonogrammaRisolutore.ContaSoluzioni(2, 2, Indizi([3], []), Indizi([1], [1])).Should().Be(0);
        // Somme diverse.
        NonogrammaRisolutore.ContaSoluzioni(2, 2, Indizi([1], [1]), Indizi([2], [1])).Should().Be(0);
        // Indizio negativo.
        NonogrammaRisolutore.ContaSoluzioni(2, 2, Indizi([-1], []), Indizi([], [])).Should().Be(0);
    }

    [Fact]
    public void Risolutore_ZeroComeLineaVuota_Accettato()
    {
        NonogrammaRisolutore.ContaSoluzioni(2, 2, Indizi([0], [2]), Indizi([1], [1])).Should().Be(1);
    }

    [Fact]
    public void Risolutore_ArgomentiNonValidi_Lancia()
    {
        var grande = () => NonogrammaRisolutore.ContaSoluzioni(40, 2, [], []);
        grande.Should().Throw<ArgumentOutOfRangeException>();
        var conteggio = () => NonogrammaRisolutore.ContaSoluzioni(2, 2, Indizi([1]), Indizi([1], [1]));
        conteggio.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Generatore_StessoSeme_StessoGioco()
    {
        var a = NonogrammaGeneratore.Genera(9, 10, 12);
        var b = NonogrammaGeneratore.Genera(9, 10, 12);
        a.Soluzione.Should().Equal(b.Soluzione);
        a.Seme.Should().Be(9);
        a.Righe.Should().Be(10);
        a.Colonne.Should().Be(12);
    }

    [Fact]
    public void Generatore_ParametriFuoriIntervallo_Lancia()
    {
        var lato = () => NonogrammaGeneratore.Genera(1, 9, 10);
        lato.Should().Throw<ArgumentOutOfRangeException>();
        var densita = () => NonogrammaGeneratore.Genera(1, 10, 10, 0.9);
        densita.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Proprieta_MilleNonogrammi_HannoUnaSolaSoluzione()
    {
        var falliti = new ConcurrentBag<long>();
        Parallel.For(1, 1001, seme =>
        {
            var lato = 10 + (int)(seme % 6);
            var g = NonogrammaGeneratore.Genera(seme, lato, lato);
            var soluzioni = NonogrammaRisolutore.Risolvi(lato, lato, g.IndiziRighe, g.IndiziColonne);
            if (soluzioni.Count != 1 || !soluzioni[0].SequenceEqual(g.Soluzione))
            {
                falliti.Add(seme);
            }
        });
        falliti.Should().BeEmpty();
    }
}

public class SchemaCruciverbaTests
{
    [Fact]
    public void Generatore_StessoSeme_StessoSchema()
    {
        var a = SchemaCruciverbaGeneratore.Genera(77, 11);
        var b = SchemaCruciverbaGeneratore.Genera(77, 11);
        a.Nere.Should().Equal(b.Nere);
        a.Numeri.Should().Equal(b.Numeri);
        a.Seme.Should().Be(77);
    }

    [Fact]
    public void Generatore_DimensioneFuoriIntervallo_Lancia()
    {
        var act = () => SchemaCruciverbaGeneratore.Genera(1, 8);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Numerazione_SchemaNoto()
    {
        bool[] nere = [false, false, false, false, true, false, false, false, false];
        var (numeri, orizzontali, verticali) = SchemaCruciverbaGeneratore.Numera(nere, 3);
        numeri.Should().Equal(1, 0, 2, 0, 0, 0, 3, 0, 0);
        orizzontali.Should().Equal(new DefinizioneSchema(1, 0, 0, 3), new DefinizioneSchema(3, 2, 0, 3));
        verticali.Should().Equal(new DefinizioneSchema(1, 0, 0, 3), new DefinizioneSchema(2, 0, 2, 3));
    }

    [Fact]
    public void Validatore_RiconosceIProblemi()
    {
        var buono = SchemaCruciverbaGeneratore.Genera(3, 9);
        SchemaCruciverbaValidatore.Valida(buono).Should().BeEmpty();

        var nere = buono.Nere.ToArray();
        var primaBianca = Array.IndexOf(nere, false);
        nere[primaBianca] = true;
        SchemaCruciverbaValidatore.Valida(buono with { Nere = nere })
            .Should().Contain(e => e.Contains("simmetrico"));

        SchemaCruciverbaValidatore.Valida(buono with { Nere = new bool[5] })
            .Should().Contain(e => e.Contains("quadrata"));
        SchemaCruciverbaValidatore.Valida(buono with { Nere = Enumerable.Repeat(true, 81).ToArray() })
            .Should().Contain(e => e.Contains("caselle bianche"));
        SchemaCruciverbaValidatore.Valida(buono with { Numeri = new int[81] })
            .Should().Contain(e => e.Contains("numerazione"));

        // Due isole bianche separate da una colonna nera, ciascuna senza parole di una lettera.
        var isole = new bool[81];
        for (var r = 0; r < 9; r++)
        {
            isole[(r * 9) + 4] = true;
        }

        SchemaCruciverbaValidatore.Valida(buono with { Nere = isole }).Should().Contain(e => e.Contains("collegate"));

        // Casella bianca isolata al centro di nere.
        var uno = Enumerable.Repeat(true, 81).ToArray();
        uno[40] = false;
        SchemaCruciverbaValidatore.Valida(buono with { Nere = uno })
            .Should().Contain(e => e.Contains("una sola lettera"));
    }

    [Fact]
    public void Proprieta_MilleSchemi_Validi()
    {
        var falliti = new ConcurrentBag<long>();
        Parallel.For(1, 1001, seme =>
        {
            var dimensione = 9 + (int)(seme % 5);
            var s = SchemaCruciverbaGeneratore.Genera(seme, dimensione);
            if (SchemaCruciverbaValidatore.Valida(s).Count != 0 ||
                s.Nere.Count(x => x) > dimensione * dimensione * 0.22 ||
                s.Nere.Count(x => x) < dimensione * dimensione * 0.10 ||
                s.Orizzontali.Concat(s.Verticali).Any(d => d.Lunghezza < 2 || d.Lunghezza > 9) ||
                s.Orizzontali.Count == 0 || s.Verticali.Count == 0)
            {
                falliti.Add(seme);
            }
        });
        falliti.Should().BeEmpty();
    }
}

public class VerificaUnicitaTests
{
    [Fact]
    public void Sudoku_Esiti()
    {
        var gioco = SudokuGeneratore.Genera(11);
        VerificaUnicita.Sudoku(gioco.Indizi).Should().Be(EsitoUnicita.Unica);
        VerificaUnicita.Sudoku(new int[81]).Should().Be(EsitoUnicita.Multiple);
        VerificaUnicita.Sudoku([.. Enumerable.Repeat(1, 2), .. new int[79]]).Should().Be(EsitoUnicita.Nessuna);
    }

    [Fact]
    public void Futoshiki_Esiti()
    {
        var g = FutoshikiGeneratore.Genera(2, 5);
        VerificaUnicita.Futoshiki(5, g.Indizi, g.Segni).Should().Be(EsitoUnicita.Unica);
        VerificaUnicita.Futoshiki(5, new int[25], []).Should().Be(EsitoUnicita.Multiple);
        VerificaUnicita.Futoshiki(4, [1, 1, .. new int[14]], []).Should().Be(EsitoUnicita.Nessuna);
    }

    [Fact]
    public void Nonogramma_Esiti()
    {
        var g = NonogrammaGeneratore.Genera(2);
        VerificaUnicita.Nonogramma(g.Righe, g.Colonne, g.IndiziRighe, g.IndiziColonne).Should().Be(EsitoUnicita.Unica);
        IReadOnlyList<IReadOnlyList<int>> uno = [new[] { 1 }, new[] { 1 }];
        VerificaUnicita.Nonogramma(2, 2, uno, uno).Should().Be(EsitoUnicita.Multiple);
        VerificaUnicita.Nonogramma(2, 2, uno, [new[] { 2 }, new[] { 1 }]).Should().Be(EsitoUnicita.Nessuna);
    }
}
