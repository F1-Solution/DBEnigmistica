using DBEnigmistica.Application.Creazione.Algoritmi;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Algoritmi;

public class StimaDifficoltaTests
{
    private const string SudokuFacile =
        "530070000600195000098000060800060003400803001700020006060000280000419005000080079";

    [Fact]
    public void Sudoku_ConIndiziAbbondanti_EFacile()
    {
        var griglia = SudokuFacile.Select(c => c - '0').ToArray();
        StimaDifficolta.Sudoku(griglia).Should().BeInRange(1, 2);
    }

    [Fact]
    public void Sudoku_ConPochiIndizi_EPiuDifficile()
    {
        var facile = SudokuGeneratore.Genera(5, 40);
        var duro = SudokuGeneratore.Genera(5, 22);
        StimaDifficolta.Sudoku(duro.Indizi).Should().BeGreaterThanOrEqualTo(StimaDifficolta.Sudoku(facile.Indizi));
    }

    [Fact]
    public void Sudoku_StimeSempreNelRangeEDeterministiche()
    {
        foreach (var seme in Enumerable.Range(1, 10))
        {
            var g = SudokuGeneratore.Genera(seme, 24);
            var stima = StimaDifficolta.Sudoku(g.Indizi);
            stima.Should().BeInRange(1, 5);
            StimaDifficolta.Sudoku(g.Indizi).Should().Be(stima);
        }
    }

    [Fact]
    public void Futoshiki_StimeNelRange()
    {
        foreach (var seme in Enumerable.Range(1, 8))
        {
            var g = FutoshikiGeneratore.Genera(seme, 5 + (seme % 2), 1 + (seme % 4));
            StimaDifficolta.Futoshiki(g.Dimensione, g.Indizi, g.Segni).Should().BeInRange(1, 5);
        }
    }

    [Fact]
    public void Futoshiki_SoloIndizi_Livello1()
    {
        var g = FutoshikiGeneratore.Genera(3, 5, 25);
        StimaDifficolta.Futoshiki(5, g.Indizi, []).Should().Be(1);
    }

    [Fact]
    public void Nonogramma_LetteraL_Livello1()
    {
        IReadOnlyList<IReadOnlyList<int>> righe = [new[] { 1 }, new[] { 1 }, new[] { 3 }];
        IReadOnlyList<IReadOnlyList<int>> colonne = [new[] { 3 }, new[] { 1 }, new[] { 1 }];
        StimaDifficolta.Nonogramma(3, 3, righe, colonne).Should().Be(1);
    }

    [Fact]
    public void Nonogramma_NonUnico_RestaAlLivello5()
    {
        IReadOnlyList<IReadOnlyList<int>> uno = [new[] { 1 }, new[] { 1 }];
        StimaDifficolta.Nonogramma(2, 2, uno, uno).Should().Be(5);
    }

    [Fact]
    public void Nonogramma_StimeNelRange()
    {
        foreach (var seme in Enumerable.Range(1, 12))
        {
            var g = NonogrammaGeneratore.Genera(seme, 10, 10, 0.5 + ((seme % 4) * 0.05));
            StimaDifficolta.Nonogramma(g.Righe, g.Colonne, g.IndiziRighe, g.IndiziColonne).Should().BeInRange(1, 5);
        }
    }

    [Fact]
    public void GeneratoriConDifficolta_RestituisconSemeEAvvisoCoerente()
    {
        foreach (var richiesta in new[] { 1, 5 })
        {
            var s = GeneratoriConDifficolta.Sudoku(100, richiesta);
            s.DifficoltaStimata.Should().Be(StimaDifficolta.Sudoku(s.Gioco.Indizi));
            s.Gioco.Seme.Should().BeInRange(100, 149);
            (s.Avvisi.Count == 0).Should().Be(s.DifficoltaStimata == richiesta);
            if (s.Avvisi.Count > 0)
            {
                s.Avvisi.Should().Equal($"Difficoltà richiesta {richiesta}, ottenuta {s.DifficoltaStimata}.");
            }

            var f = GeneratoriConDifficolta.Futoshiki(200, 5, richiesta);
            f.DifficoltaStimata.Should().Be(StimaDifficolta.Futoshiki(5, f.Gioco.Indizi, f.Gioco.Segni));

            var n = GeneratoriConDifficolta.Nonogramma(300, 10, 10, richiesta);
            n.Gioco.Seme.Should().BeInRange(300, 349);
        }
    }

    [Fact]
    public void GeneratoriConDifficolta_StessoSemeBase_StessoRisultato()
    {
        var a = GeneratoriConDifficolta.Sudoku(7, 3);
        var b = GeneratoriConDifficolta.Sudoku(7, 3);
        a.Gioco.Indizi.Should().Equal(b.Gioco.Indizi);
        a.Gioco.Seme.Should().Be(b.Gioco.Seme);
    }

    [Fact]
    public void GeneratoriConDifficolta_LivelloFuoriIntervallo_Lancia()
    {
        var act = () => GeneratoriConDifficolta.Sudoku(1, 6);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class DatiGiochiTests
{
    [Fact]
    public void Sudoku_HaIlFormatoDelContratto()
    {
        var g = SudokuGeneratore.Genera(4);
        var dati = DatiGiochi.Sudoku(g, 2);
        dati.Select(p => p.Key).Should().Equal("righe", "colonne", "griglia", "soluzione_griglia", "difficolta");
        var celle = dati["griglia"]!["celle"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
        celle.Should().HaveCount(9).And.OnlyContain(r => r.Length == 9 && r.All(c => c == '.' || (c >= '1' && c <= '9')));
        dati["griglia"]!["numeri"]!.AsArray().Should().BeEmpty();
        dati["soluzione_griglia"]!["celle"]!.AsArray().Select(x => x!.GetValue<string>())
            .Should().OnlyContain(r => r.All(c => c >= '1' && c <= '9'));
        dati["difficolta"]!.GetValue<int>().Should().Be(2);
    }

    [Fact]
    public void Futoshiki_SegniOrdinatiEConCoordinateDa1()
    {
        var gioco = new FutoshikiGioco(
            1, 5, new int[25],
            [
                new SegnoFutoshiki(1, 1, 0, 1),
                new SegnoFutoshiki(0, 1, 0, 0),
                new SegnoFutoshiki(0, 0, 1, 0),
                new SegnoFutoshiki(0, 2, 0, 3),
            ],
            new int[25]);
        var segni = DatiGiochi.Futoshiki(gioco, 3)["segni"]!.AsArray();
        var testo = segni.Select(s => $"{s!["riga"]},{s["colonna"]},{s["verso"]},{s["segno"]}").ToArray();
        testo.Should().Equal(
            "1,1,destra,minore",
            "1,1,sotto,maggiore",
            "1,2,sotto,minore",
            "1,3,destra,maggiore");
    }

    [Fact]
    public void Nonogramma_IndiziEGriglie()
    {
        var g = NonogrammaGeneratore.Genera(8, 10, 10);
        var dati = DatiGiochi.Nonogramma(g, 1);
        dati["griglia"]!["celle"]!.AsArray().Select(x => x!.GetValue<string>()).Should().OnlyContain(r => r == "..........");
        dati["soluzione_griglia"]!["celle"]!.AsArray().Select(x => x!.GetValue<string>())
            .Should().OnlyContain(r => r.All(c => c == '#' || c == '.'));
        dati["indizi_righe"]!.AsArray().Should().HaveCount(10);
        dati["indizi_righe"]![0]!["indice"]!.GetValue<int>().Should().Be(1);
        var vuoto = new NonogrammaGioco(1, 1, 1, [Array.Empty<int>()], [Array.Empty<int>()], [0]);
        DatiGiochi.Nonogramma(vuoto, 1)["indizi_righe"]![0]!["blocchi"]!.GetValue<string>().Should().Be("0");
        DatiGiochi.Nonogramma(g, 1)["indizi_colonne"]![0]!["blocchi"]!.GetValue<string>()
            .Should().Be(string.Join(' ', g.IndiziColonne[0]));
    }

    [Fact]
    public void Schema_HaIlFormatoDelContratto()
    {
        var schema = SchemaCruciverbaGeneratore.Genera(4242, 9);
        var dati = DatiGiochi.Schema(schema);
        dati.Select(p => p.Key).Should().Equal(
            "versione_formato", "righe", "colonne", "celle", "numeri", "parole", "seme", "versione_generatore");
        dati["seme"]!.GetValue<long>().Should().Be(4242);
        dati["versione_generatore"]!.GetValue<string>().Should().Be("schema-cruciverba-1");
        var parole = dati["parole"]!.AsArray();
        parole.Should().HaveCount(schema.Orizzontali.Count + schema.Verticali.Count);
        parole[0]!["direzione"]!.GetValue<string>().Should().Be("orizzontale");
        parole[^1]!["direzione"]!.GetValue<string>().Should().Be("verticale");
        dati["numeri"]!.AsArray().Should().HaveCount(schema.Numeri.Count(x => x != 0));
        dati["celle"]!.AsArray().Select(x => x!.GetValue<string>()).Should().OnlyContain(r => r.Length == 9);
    }

    [Fact]
    public void Validatore_ParolaTroppoLungaEQuotaDiNere()
    {
        var schema = SchemaCruciverbaGeneratore.Genera(9, 13);
        var tutteBianche = new bool[169];
        var (numeri, orizzontali, verticali) = SchemaCruciverbaGeneratore.Numera(tutteBianche, 13);
        var vuoto = schema with { Nere = tutteBianche, Numeri = numeri, Orizzontali = orizzontali, Verticali = verticali };
        var errori = SchemaCruciverbaValidatore.Valida(vuoto);
        errori.Should().Contain(e => e.Contains("più di 9"));
        errori.Should().Contain(e => e.Contains("quota"));
    }
}
