using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DBEnigmistica.Application.Creazione.Algoritmi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Seme casuale per i generatori di produzione.</summary>
public sealed class SorgenteSemiCasuale : ISorgenteSemi
{
    public long Prossimo() => Random.Shared.NextInt64();
}

/// <summary>Porta i generatori di <c>Algoritmi</c> nell'involucro di risposta di creazione (progetto 1b.1, sezioni 3.1 e 5).</summary>
public sealed class GeneratoriAlgoritmici : IGeneratoriAlgoritmici
{
    public bool Supporta(string tipologia) => tipologia is "sudoku" or "futoshiki" or "nonogramma";

    public string VersioneGeneratore(string tipologia) => Supporta(tipologia)
        ? DatiGiochi.VersioneGeneratoreLogici
        : throw new InvalidOperationException($"Nessun generatore per la Tipologia {tipologia}.");

    public RisultatoAlgoritmico Genera(string tipologia, BriefCreazione brief, long seme)
    {
        ArgumentNullException.ThrowIfNull(brief);
        var versione = VersioneGeneratore(tipologia);
        JsonObject dati;
        long semeTenuto;
        IReadOnlyList<string> avvisi;
        switch (tipologia)
        {
            case "sudoku":
                var s = GeneratoriConDifficolta.Sudoku(seme, brief.Difficolta);
                (dati, semeTenuto, avvisi) = (DatiGiochi.Sudoku(s.Gioco, s.DifficoltaStimata), s.Gioco.Seme, s.Avvisi);
                break;
            case "futoshiki":
                var f = GeneratoriConDifficolta.Futoshiki(seme, brief.Dimensioni!.Righe, brief.Difficolta);
                (dati, semeTenuto, avvisi) = (DatiGiochi.Futoshiki(f.Gioco, f.DifficoltaStimata), f.Gioco.Seme, f.Avvisi);
                break;
            default:
                var n = GeneratoriConDifficolta.Nonogramma(seme, brief.Dimensioni!.Righe, brief.Dimensioni.Colonne, brief.Difficolta);
                (dati, semeTenuto, avvisi) = (DatiGiochi.Nonogramma(n.Gioco, n.DifficoltaStimata), n.Gioco.Seme, n.Avvisi);
                break;
        }

        var involucro = new JsonObject
        {
            ["versione_formato"] = 1,
            ["tipologia"] = brief.Tipologia,
            ["versione_tipologia"] = brief.VersioneTipologia,
            ["titolo"] = null,
            ["dati"] = dati,
            ["avvisi"] = new JsonArray([.. avvisi.Select(a => (JsonNode?)JsonValue.Create(a))]),
        };
        return new RisultatoAlgoritmico(involucro.ToJsonString(), semeTenuto, versione);
    }

    public SchemaCaselle GeneraSchemaCaselle(int dimensione, long seme)
    {
        var schema = SchemaCruciverbaGeneratore.Genera(seme, dimensione);
        var json = DatiGiochi.Schema(schema).ToJsonString();
        return SchemaCaselle.Leggi(json);
    }
}
