using System.Text.Json.Nodes;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.Creazione.Algoritmi;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>Controlli dei giochi logici dopo lo schema: coerenza con la soluzione e soluzione unica (progetto 1b.1, sezione 5).</summary>
public class ControlliLogiciTests
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

    // Un sudoku minimo: togliere un indizio qualsiasi dà più soluzioni.
    internal static JsonNode SudokuMinimo() => Involucro("sudoku", DatiGiochi.Sudoku(SudokuGeneratore.Genera(11, 17), 3));

    internal static JsonNode Nonogramma() => Involucro("nonogramma", DatiGiochi.Nonogramma(NonogrammaGeneratore.Genera(5, 10, 10), 2));

    internal static JsonNode Futoshiki() => Involucro("futoshiki", DatiGiochi.Futoshiki(FutoshikiGeneratore.Genera(9, 5), 2));

    private static string Riga(JsonNode contenuto, string griglia, int indice) =>
        contenuto["dati"]![griglia]!["celle"]![indice]!.GetValue<string>();

    private static void ImpostaRiga(JsonNode contenuto, string griglia, int indice, string valore) =>
        contenuto["dati"]![griglia]!["celle"]![indice] = valore;

    [Fact]
    public void ContenutiGeneratiDaiGeneratori_PassanoITreControlli()
    {
        ControlliLogici.Verifica("sudoku", SudokuMinimo().ToJsonString()).Should().BeEmpty();
        ControlliLogici.Verifica("nonogramma", Nonogramma().ToJsonString()).Should().BeEmpty();
        ControlliLogici.Verifica("futoshiki", Futoshiki().ToJsonString()).Should().BeEmpty();
    }

    [Fact]
    public void Sudoku_ConUnIndizioToltoHaDueSoluzioni()
    {
        var contenuto = SudokuMinimo();
        // Cerca il primo indizio di tutta la griglia e lo toglie.
        for (var r = 0; r < 9; r++)
        {
            var celle = Riga(contenuto, "griglia", r).ToCharArray();
            var p = Array.FindIndex(celle, c => c != '.');
            if (p < 0)
                continue;
            celle[p] = '.';
            ImpostaRiga(contenuto, "griglia", r, new string(celle));
            break;
        }

        ControlliLogici.Verifica("sudoku", contenuto.ToJsonString()).Should().ContainSingle().Which.Should().Contain("più di una soluzione");
    }

    [Fact]
    public void Sudoku_ConUnIndizioCheContraddiceLaSoluzione()
    {
        var contenuto = SudokuMinimo();
        for (var r = 0; r < 9; r++)
        {
            var celle = Riga(contenuto, "griglia", r).ToCharArray();
            var p = Array.FindIndex(celle, c => c != '.');
            if (p < 0)
                continue;
            celle[p] = celle[p] == '9' ? '8' : (char)(celle[p] + 1);
            ImpostaRiga(contenuto, "griglia", r, new string(celle));
            break;
        }

        ControlliLogici.Verifica("sudoku", contenuto.ToJsonString()).Should().Contain(e => e.Contains("contraddice la soluzione"));
    }

    [Fact]
    public void Sudoku_ConUnaSoluzioneNonValida()
    {
        var contenuto = SudokuMinimo();
        ImpostaRiga(contenuto, "soluzione_griglia", 1, Riga(contenuto, "soluzione_griglia", 0));

        ControlliLogici.Verifica("sudoku", contenuto.ToJsonString()).Should().NotBeEmpty();
    }

    [Fact]
    public void Nonogramma_ConIndiziCheNonCorrispondonoAlDisegno()
    {
        var contenuto = Nonogramma();
        contenuto["dati"]!["indizi_righe"]![0]!["blocchi"] = "10 10";

        var errori = ControlliLogici.Verifica("nonogramma", contenuto.ToJsonString());

        errori.Should().ContainSingle().Which.Should().Contain("indizi delle righe non corrispondono");
    }

    [Fact]
    public void Nonogramma_ConIndiziNonNumerici_OFuoriOrdine_DaErrore()
    {
        var nonNumerico = Nonogramma();
        nonNumerico["dati"]!["indizi_colonne"]![0]!["blocchi"] = "x";
        var fuoriOrdine = Nonogramma();
        fuoriOrdine["dati"]!["indizi_righe"]![0]!["indice"] = 2;

        ControlliLogici.Verifica("nonogramma", nonNumerico.ToJsonString()).Should().NotBeEmpty();
        ControlliLogici.Verifica("nonogramma", fuoriOrdine.ToJsonString()).Should().NotBeEmpty();
    }

    [Fact]
    public void Futoshiki_ConUnSegnoCheContraddiceLaSoluzione_OFuoriGriglia()
    {
        var contraddice = Futoshiki();
        var segno = contraddice["dati"]!["segni"]![0]!;
        segno["segno"] = segno["segno"]!.GetValue<string>() == "maggiore" ? "minore" : "maggiore";
        var fuori = Futoshiki();
        fuori["dati"]!["segni"]![0]!["colonna"] = 5;
        fuori["dati"]!["segni"]![0]!["verso"] = "destra";

        ControlliLogici.Verifica("futoshiki", contraddice.ToJsonString()).Should().Contain(e => e.Contains("contraddice"));
        ControlliLogici.Verifica("futoshiki", fuori.ToJsonString()).Should().Contain(e => e.Contains("esce dalla griglia"));
    }

    [Fact]
    public void Futoshiki_SenzaSegniNeIndiziSufficienti_HaPiuSoluzioni()
    {
        var contenuto = Futoshiki();
        contenuto["dati"]!["segni"] = new JsonArray();
        for (var r = 0; r < 5; r++)
            ImpostaRiga(contenuto, "griglia", r, ".....");

        ControlliLogici.Verifica("futoshiki", contenuto.ToJsonString()).Should().Contain(e => e.Contains("più di una soluzione"));
    }

    [Fact]
    public void ContenutoSenzaLaFormaAttesa_DaUnErroreNonUnaEccezione()
    {
        ControlliLogici.Verifica("sudoku", "{\"dati\":{}}").Should().ContainSingle();
        ControlliLogici.Verifica("rebus", "{\"dati\":{}}").Should().BeEmpty("le altre Tipologie non hanno controlli");
    }
}
