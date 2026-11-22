using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Catalogo;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Derivazione;

/// <summary>JSON Schema della risposta per pagina, derivato dal catalogo dei descrittori (progetto-1.1, sezione 3.2).</summary>
public static class DerivatoreSchemaPagina
{
    private const string UriSchema = "urn:dbenigmistica:risposta-pagina:catalogo";
    private const string TitoloSchema = "Risposta per pagina, derivata dal catalogo dei descrittori";
    private const int MassimoVociElenco = 500;
    private const string DataPattern = "^[0-9]{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$";

    // Definizioni dell'involucro che la derivazione sostituisce con una per Tipologia.
    private static readonly string[] DefinizioniSostituite = ["idTipologia", "giocoNoto"];

    private static readonly JsonSerializerOptions Scrittura = new()
    {
        WriteIndented = true,
        IndentSize = 2,
        NewLine = "\n",
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Schema derivato come testo JSON con rientro di 2 spazi, a capo LF e chiavi nell'ordine di costruzione.</summary>
    public static string Deriva(IEnumerable<Descrittore> descrittori) => Serializza(DerivaNodo(descrittori));

    /// <summary>Testo JSON con rientro di 2 spazi e a capo LF di un nodo.</summary>
    public static string Serializza(JsonNode nodo) => nodo.ToJsonString(Scrittura);

    /// <summary>Schema derivato come albero JSON; un prototipo o una Tipologia con più versioni è un errore.</summary>
    public static JsonObject DerivaNodo(IEnumerable<Descrittore> descrittori)
    {
        ArgumentNullException.ThrowIfNull(descrittori);
        var ordinati = DescrittoriPerCatalogo.Ordina(descrittori);
        var involucro = JsonNode.Parse(RisorseTipologie.Leggi("_meta/risposta-pagina.schema.json"))!.AsObject();

        var radice = new JsonObject();
        foreach (var (chiave, valore) in involucro)
        {
            switch (chiave)
            {
                case "$id":
                    radice[chiave] = UriSchema;
                    break;
                case "title":
                    radice[chiave] = TitoloSchema;
                    break;
                case "description":
                    break;
                case "properties":
                    radice[chiave] = ProprietaConGiochi(valore!.AsObject(), ordinati);
                    break;
                case "$defs":
                    radice[chiave] = DefinizioniConGiochi(valore!.AsObject(), ordinati);
                    break;
                default:
                    radice[chiave] = valore!.DeepClone();
                    break;
            }
        }

        return radice;
    }

    /// <summary>Nome della definizione di una Tipologia: <c>gioco_</c> e l'id con i trattini sostituiti da trattini bassi.</summary>
    public static string NomeDefinizioneGioco(string idTipologia) => "gioco_" + idTipologia.Replace('-', '_');

    private static JsonObject ProprietaConGiochi(JsonObject proprieta, IReadOnlyList<Descrittore> descrittori)
    {
        var risultato = new JsonObject();
        foreach (var (chiave, valore) in proprieta)
        {
            if (chiave != "giochi")
            {
                risultato[chiave] = valore!.DeepClone();
                continue;
            }

            var alternative = new JsonArray();
            foreach (var d in descrittori)
                alternative.Add(Riferimento(NomeDefinizioneGioco(d.Id)));
            alternative.Add(Riferimento("giocoProposto"));

            var giochi = valore!.DeepClone().AsObject();
            giochi["items"] = new JsonObject { ["anyOf"] = alternative };
            risultato[chiave] = giochi;
        }

        return risultato;
    }

    private static JsonObject DefinizioniConGiochi(JsonObject definizioni, IReadOnlyList<Descrittore> descrittori)
    {
        var risultato = new JsonObject();
        foreach (var (chiave, valore) in definizioni)
            if (!DefinizioniSostituite.Contains(chiave))
                risultato[chiave] = valore!.DeepClone();

        foreach (var d in descrittori)
            risultato[NomeDefinizioneGioco(d.Id)] = DefinizioneGioco(d);

        return risultato;
    }

    private static JsonObject DefinizioneGioco(Descrittore d) => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["required"] = Stringhe(["stato_tipologia", "tipologia", "versione_tipologia", "titolo", "riquadro", "dati", "confidenza", "avvisi"]),
        ["properties"] = new JsonObject
        {
            ["stato_tipologia"] = new JsonObject { ["const"] = "nota" },
            ["tipologia"] = new JsonObject { ["const"] = d.Id },
            ["versione_tipologia"] = new JsonObject { ["const"] = d.Versione },
            ["titolo"] = Riferimento("testoONull"),
            ["riquadro"] = Riferimento("riquadro"),
            ["dati"] = Dati(d),
            ["confidenza"] = Confidenza(d),
            ["avvisi"] = Riferimento("avvisi"),
        },
    };

    private static JsonObject Dati(Descrittore d)
    {
        var nomi = d.Campi.Select(c => c.Nome).Concat(d.Elenchi.Select(e => e.Nome)).ToList();
        var proprieta = new JsonObject();
        foreach (var campo in d.Campi)
            proprieta[campo.Nome] = ValoreCampo(campo);
        foreach (var elenco in d.Elenchi)
            proprieta[elenco.Nome] = Elenco(elenco);
        return OggettoChiuso(nomi, proprieta);
    }

    private static JsonObject Elenco(ElencoDescrittore elenco)
    {
        var proprieta = new JsonObject();
        foreach (var campo in elenco.Campi)
            proprieta[campo.Nome] = ValoreCampo(campo);

        return new JsonObject
        {
            ["type"] = "array",
            ["maxItems"] = MassimoVociElenco,
            ["items"] = OggettoChiuso(elenco.Campi.Select(c => c.Nome), proprieta),
        };
    }

    private static JsonObject Confidenza(Descrittore d)
    {
        var campi = new JsonObject();
        foreach (var campo in d.Campi)
            campi[campo.Nome] = Riferimento("probabilita");
        var elenchi = new JsonObject();
        foreach (var elenco in d.Elenchi)
            elenchi[elenco.Nome] = Riferimento("probabilita");

        return OggettoChiuso(
            ["gioco", "tipologia", "campi", "elenchi"],
            new JsonObject
            {
                ["gioco"] = Riferimento("probabilita"),
                ["tipologia"] = Riferimento("probabilita"),
                ["campi"] = OggettoChiuso(d.Campi.Select(c => c.Nome), campi),
                ["elenchi"] = OggettoChiuso(d.Elenchi.Select(e => e.Nome), elenchi),
            });
    }

    // Un campo non obbligatorio ammette null, così ogni chiave è sempre presente (modalità "strict" dei fornitori).
    private static JsonNode ValoreCampo(CampoDescrittore c)
    {
        var valore = Valore(c);
        return c.Obbligatorio
            ? valore
            : new JsonObject { ["anyOf"] = new JsonArray(new JsonObject { ["type"] = "null" }, valore) };
    }

    private static JsonObject Valore(CampoDescrittore c) => c.Tipo switch
    {
        TipoCampo.Testo => new JsonObject { ["type"] = "string", ["minLength"] = 1, ["maxLength"] = c.Max },
        TipoCampo.TestoLungo => new JsonObject { ["type"] = "string", ["minLength"] = 1, ["maxLength"] = 20000 },
        TipoCampo.Intero => Intero(c.Ruolo),
        TipoCampo.Decimale => new JsonObject { ["type"] = "number", ["minimum"] = -99999999.9999m, ["maximum"] = 99999999.9999m },
        TipoCampo.Booleano => new JsonObject { ["type"] = "boolean" },
        TipoCampo.Data => new JsonObject { ["type"] = "string", ["pattern"] = DataPattern },
        TipoCampo.Enumerazione => new JsonObject { ["type"] = "string", ["enum"] = Stringhe(c.Valori ?? []) },
        TipoCampo.Griglia => Riferimento("griglia"),
        TipoCampo.Immagine => OggettoChiuso(["riquadro"], new JsonObject { ["riquadro"] = Riferimento("riquadro") }),
        _ => throw new ArgumentOutOfRangeException(nameof(c), c.Tipo, "Tipo di campo non previsto."),
    };

    private static JsonObject Intero(RuoloCampo? ruolo) => ruolo switch
    {
        RuoloCampo.Righe or RuoloCampo.Colonne => new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 60 },
        RuoloCampo.Numero => new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 999 },
        _ => new JsonObject { ["type"] = "integer", ["minimum"] = int.MinValue, ["maximum"] = int.MaxValue },
    };

    private static JsonObject OggettoChiuso(IEnumerable<string> obbligatori, JsonObject proprieta) => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["required"] = Stringhe(obbligatori),
        ["properties"] = proprieta,
    };

    private static JsonObject Riferimento(string definizione) =>
        new() { ["$ref"] = string.Create(CultureInfo.InvariantCulture, $"#/$defs/{definizione}") };

    private static JsonArray Stringhe(IEnumerable<string> valori)
    {
        var elenco = new JsonArray();
        foreach (var valore in valori)
            elenco.Add(valore);
        return elenco;
    }
}
