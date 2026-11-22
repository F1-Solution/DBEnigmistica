using System.Text.Json;
using DBEnigmistica.Tests.Unit.Creazione;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tests.Unit.Tipologie.Validatori;

/// <summary>Descrittori letti dai file reali e dati inventati per i test dei validatori.</summary>
internal static class DatiDiProva
{
    public const string Numeri = """[{"riga":1,"colonna":1,"numero":1},{"riga":1,"colonna":2,"numero":2},{"riga":2,"colonna":1,"numero":3},{"riga":2,"colonna":3,"numero":4},{"riga":3,"colonna":2,"numero":5}]""";

    public const string Definizioni = """
        [
          {"numero":1,"direzione":"orizzontale","testo":"Sillaba (2)","soluzione":"CA"},
          {"numero":1,"direzione":"verticale","testo":"Due lettere","soluzione":"CA"},
          {"numero":2,"direzione":"verticale","testo":"Ama (3)","soluzione":"AMA"},
          {"numero":3,"direzione":"orizzontale","testo":"Un verbo","soluzione":"AMO"},
          {"numero":4,"direzione":"verticale","testo":"Congiunzione","soluzione":"OR"},
          {"numero":5,"direzione":"orizzontale","testo":"Breve","soluzione":"AR"}
        ]
        """;

    public static Descrittore Descrittore(string id, int versione = 1)
    {
        using var documento = JsonDocument.Parse(FileDelRepository.Leggi("tipologie", id, $"v{versione}.json"));
        var r = documento.RootElement;
        return new Descrittore(
            id,
            versione,
            Valore<FamigliaTipologia>(r, "famiglia"u8),
            "prova",
            "prova",
            new RegoleTipologia("prova", 1, StatoRegole.Bozza),
            [.. r.GetProperty("campi"u8).EnumerateArray().Select(Campo)],
            [.. r.GetProperty("elenchi"u8).EnumerateArray().Select(e => new ElencoDescrittore(
                e.GetProperty("nome"u8).GetString()!,
                [.. e.GetProperty("campi"u8).EnumerateArray().Select(Campo)]))],
            new ResaDescrittore(NomiModello.TryLeggi<ModelloResa>(r.GetProperty("resa"u8).GetProperty("modello"u8).GetString(), out var resa) ? resa : default),
            []);
    }

    /// <summary>Cruciverba 3x3 di lettere inventate: CA#, AMO, #AR.</summary>
    public static JsonElement Cruciverba(
        string celle = """["CA#","AMO","#AR"]""",
        string numeri = Numeri,
        string definizioni = Definizioni,
        int righe = 3,
        int colonne = 3,
        int grigliaRighe = 3,
        int grigliaColonne = 3) =>
        Json($$"""
            {"righe":{{righe}},"colonne":{{colonne}},
             "griglia":{"righe":{{grigliaRighe}},"colonne":{{grigliaColonne}},"celle":{{celle}},"numeri":{{numeri}}},
             "autore":null,"definizioni":{{definizioni}}}
            """);

    /// <summary>Gioco breve con diagramma, prima parola, soluzione e parti; ogni valore può essere null.</summary>
    public static JsonElement Breve(string? diagramma, string? prima, string? soluzione, string? parti = null) =>
        Json($$"""
            {"esposto":"testo inventato","diagramma":{{Testo(diagramma)}},"prima_parola":{{Testo(prima)}},"parti":{{Testo(parti)}},"soluzione":{{Testo(soluzione)}},"autore":null}
            """);

    public static JsonElement Json(string json)
    {
        using var documento = JsonDocument.Parse(json);
        return documento.RootElement.Clone();
    }

    private static string Testo(string? valore) => valore is null ? "null" : JsonSerializer.Serialize(valore);

    private static CampoDescrittore Campo(JsonElement e) => new(
        e.GetProperty("nome"u8).GetString()!,
        Valore<TipoCampo>(e, "tipo"u8),
        Ruolo: e.TryGetProperty("ruolo"u8, out var ruolo) && NomiModello.TryLeggi<RuoloCampo>(ruolo.GetString(), out var valore) ? valore : null);

    private static T Valore<T>(JsonElement e, ReadOnlySpan<byte> nome) where T : struct, Enum =>
        NomiModello.TryLeggi<T>(e.GetProperty(nome).GetString(), out var valore) ? valore : throw new InvalidOperationException("Valore non riconosciuto.");
}
