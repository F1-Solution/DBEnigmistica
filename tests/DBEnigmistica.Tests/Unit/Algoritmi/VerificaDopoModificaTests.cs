using System.Diagnostics;
using System.Text.Json.Nodes;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.Creazione.Algoritmi;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Algoritmi;

/// <summary>Correttezza della verifica dopo una modifica del revisore (sempre attiva) e suoi tempi (categoria Temporizzato).</summary>
public class VerificaDopoModificaTests
{
    private static JsonNode Involucro(string tipologia, JsonObject dati) => new JsonObject
    {
        ["versione_formato"] = 1,
        ["tipologia"] = tipologia,
        ["versione_tipologia"] = 0,
        ["titolo"] = null,
        ["dati"] = dati,
        ["avvisi"] = new JsonArray(),
    };

    private static JsonNode SudokuConDueSoluzioni()
    {
        var contenuto = Involucro("sudoku", DatiGiochi.Sudoku(SudokuGeneratore.Genera(11, 17), 3));
        var celle = contenuto["dati"]!["griglia"]!["celle"]!.AsArray();
        for (var r = 0; r < 9; r++)
        {
            var riga = celle[r]!.GetValue<string>().ToCharArray();
            var p = Array.FindIndex(riga, c => c != '.');
            if (p < 0)
                continue;
            riga[p] = '.';
            celle[r] = new string(riga);
            break;
        }

        return contenuto;
    }

    // Senza segni il futoshiki generato ha quasi sempre più soluzioni: il controllo atteso è quello dell'unicità.
    private static JsonNode FutoshikiSenzaSegni()
    {
        var contenuto = Involucro("futoshiki", DatiGiochi.Futoshiki(FutoshikiGeneratore.Genera(9, 7), 2));
        contenuto["dati"]!["segni"]!.AsArray().Clear();
        return contenuto;
    }

    // Caso costoso: il generatore produce schemi minimi, quindi la modifica è un indizio in più dalla soluzione; il risolutore dimostra l'unicità per intero.
    private static JsonNode FutoshikiUnicoDopoUnaModifica()
    {
        var contenuto = Involucro("futoshiki", DatiGiochi.Futoshiki(FutoshikiGeneratore.Genera(9, 7), 2));
        var celle = contenuto["dati"]!["griglia"]!["celle"]!.AsArray();
        var soluzione = contenuto["dati"]!["soluzione_griglia"]!["celle"]!.AsArray();
        for (var r = 0; r < 7; r++)
        {
            var riga = celle[r]!.GetValue<string>().ToCharArray();
            var p = Array.FindIndex(riga, c => c == '.');
            if (p < 0)
                continue;
            riga[p] = soluzione[r]!.GetValue<string>()[p];
            celle[r] = new string(riga);
            return contenuto;
        }

        throw new InvalidOperationException("Il futoshiki generato non ha caselle vuote da compilare.");
    }

    private static JsonNode NonogrammaConCasellaCambiata()
    {
        var contenuto = Involucro("nonogramma", DatiGiochi.Nonogramma(NonogrammaGeneratore.Genera(5, 15, 15), 2));
        var celle = contenuto["dati"]!["soluzione_griglia"]!["celle"]!.AsArray();
        var riga = celle[0]!.GetValue<string>().ToCharArray();
        riga[0] = riga[0] == '#' ? '.' : '#';
        celle[0] = new string(riga);
        return contenuto;
    }

    private static IReadOnlyList<string> Verifica(string tipologia, JsonNode contenuto) =>
        ControlliLogici.Verifica(tipologia, contenuto.ToJsonString());

    // Mediana di 5 esecuzioni dopo una di riscaldamento: regge il rumore del coverage e dei runner condivisi.
    private static long MedianaMs(string tipologia, JsonNode contenuto)
    {
        var json = contenuto.ToJsonString();
        ControlliLogici.Verifica(tipologia, json);
        var tempi = new long[5];
        for (var i = 0; i < tempi.Length; i++)
        {
            var cronometro = Stopwatch.StartNew();
            ControlliLogici.Verifica(tipologia, json);
            tempi[i] = cronometro.ElapsedMilliseconds;
        }

        Array.Sort(tempi);
        return tempi[tempi.Length / 2];
    }

    [Fact]
    public void Sudoku_ConUnIndizioInMeno_SegnalaPiuSoluzioni()
    {
        Verifica("sudoku", SudokuConDueSoluzioni()).Should().ContainSingle().Which.Should().Contain("più di una soluzione");
    }

    [Fact]
    public void Futoshiki_SenzaSegni_SegnalaPiuSoluzioni()
    {
        Verifica("futoshiki", FutoshikiSenzaSegni()).Should().ContainSingle().Which.Should().Contain("più di una soluzione");
    }

    [Fact]
    public void Nonogramma_ConUnaCasellaCambiata_SegnalaGliIndiziNonCoerenti()
    {
        Verifica("nonogramma", NonogrammaConCasellaCambiata()).Should().Contain(e => e.Contains("indizi"));
    }

    [Fact]
    [Trait("Category", "Temporizzato")]
    public void Sudoku_ConDueSoluzioni_SiVerificaEntro200Ms() =>
        MedianaMs("sudoku", SudokuConDueSoluzioni()).Should().BeLessThan(200);

    [Fact]
    public void Futoshiki7x7_ConUnIndizioInPiu_RestaUnicoSenzaControlli() =>
        Verifica("futoshiki", FutoshikiUnicoDopoUnaModifica()).Should().BeEmpty();

    [Fact]
    [Trait("Category", "Temporizzato")]
    public void Futoshiki7x7_UnicoDopoUnaModifica_SiVerificaEntro500Ms() =>
        MedianaMs("futoshiki", FutoshikiUnicoDopoUnaModifica()).Should().BeLessThan(500);

    [Fact]
    [Trait("Category", "Temporizzato")]
    public void Nonogramma15x15_UnicoDaDimostrare_SiVerificaEntro500Ms() =>
        MedianaMs("nonogramma", Involucro("nonogramma", DatiGiochi.Nonogramma(NonogrammaGeneratore.Genera(5, 15, 15), 2))).Should().BeLessThan(500);

    [Fact]
    [Trait("Category", "Temporizzato")]
    public void Nonogramma15x15_ConUnaCasellaCambiata_SiVerificaEntro500Ms() =>
        MedianaMs("nonogramma", NonogrammaConCasellaCambiata()).Should().BeLessThan(500);
}
