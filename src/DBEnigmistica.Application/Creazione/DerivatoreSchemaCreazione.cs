using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Deriva lo JSON Schema della risposta di creazione da descrittore, brief e schema delle caselle (progetto 1b.1, sezione 3).</summary>
public static class DerivatoreSchemaCreazione
{
    public const int LunghezzaMassimaSchema = 24_000;
    public const string MessaggioImmagini = "Tipologia con immagini: la creazione non è ammessa nel prototipo";

    private const string PatternData = "^[0-9]{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$";

    private static readonly JsonSerializerOptions OpzioniTesto = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Schema senza specializzazioni: è quello che entra nella versione del prompt.</summary>
    public static string Generico(DescrittoreTipologia descrittore) => Deriva(descrittore, null, null);

    /// <summary>Schema specializzato dal brief (sottotipo) e, per il cruciverba, dallo schema delle caselle.</summary>
    public static string Deriva(DescrittoreTipologia descrittore, BriefCreazione? brief, SchemaCaselle? caselle)
    {
        ArgumentNullException.ThrowIfNull(descrittore);
        if (descrittore.Campi.Concat(descrittore.Elenchi.SelectMany(e => e.Campi)).Any(c => c.Tipo == DescrittoreTipologia.TipoImmagine))
            throw new InvalidOperationException(MessaggioImmagini);

        var completo = Costruisci(descrittore, brief, caselle, conPrefissi: true);
        if (completo.Length <= LunghezzaMassimaSchema || caselle is null)
            return completo;

        return Costruisci(descrittore, brief, caselle, conPrefissi: false);
    }

    private static string Costruisci(DescrittoreTipologia d, BriefCreazione? brief, SchemaCaselle? caselle, bool conPrefissi)
    {
        var richiede = new List<string>();
        var defs = new JsonObject();
        var haGriglia = false;

        var datiProprieta = new JsonObject();
        foreach (var campo in d.Campi)
        {
            richiede.Add(campo.Nome);
            if (campo.Tipo == "griglia")
                haGriglia = true;
            datiProprieta[campo.Nome] = ValoreCampo(campo, brief, caselle);
        }

        defs["dati"] = null;
        var vociDefs = new List<(string Nome, JsonNode Valore)>();
        foreach (var elenco in d.Elenchi)
        {
            richiede.Add(elenco.Nome);
            var riferimento = "#/$defs/voce_" + elenco.Nome;
            datiProprieta[elenco.Nome] = ArrayElenco(elenco, riferimento, caselle, conPrefissi);
            vociDefs.Add(("voce_" + elenco.Nome, Voce(elenco, brief, caselle)));
        }

        defs["dati"] = Obj(
            ("type", "object"), ("additionalProperties", false), ("required", Array(richiede)), ("properties", datiProprieta));
        foreach (var (nome, valore) in vociDefs)
            defs[nome] = valore;
        if (haGriglia)
            defs["griglia"] = Griglia(d.Resa, caselle);

        var radice = Obj(
            ("$schema", "https://json-schema.org/draft/2020-12/schema"),
            ("$id", $"urn:dbenigmistica:risposta-creazione:{d.Id}:v{d.Versione}"),
            ("type", "object"),
            ("additionalProperties", false),
            ("required", Array(["versione_formato", "tipologia", "versione_tipologia", "titolo", "dati", "avvisi"])),
            ("properties", Obj(
                ("versione_formato", Obj(("const", 1))),
                ("tipologia", Obj(("const", d.Id))),
                ("versione_tipologia", Obj(("const", d.Versione))),
                ("titolo", Nullable(Obj(("type", "string"), ("minLength", 1), ("maxLength", 200)))),
                ("dati", Obj(("$ref", "#/$defs/dati"))),
                ("avvisi", Obj(
                    ("type", "array"), ("maxItems", 10),
                    ("items", Obj(("type", "string"), ("minLength", 1), ("maxLength", 300))))))),
            ("$defs", defs));
        return radice.ToJsonString(OpzioniTesto);
    }

    private static JsonNode ValoreCampo(CampoDescrittore c, BriefCreazione? brief, SchemaCaselle? caselle)
    {
        JsonNode valore;
        if (c.Tipo == "enumerazione" && c.Nome == "sottotipo" && brief?.Sottotipo is { } sottotipo)
            valore = Obj(("const", sottotipo));
        else if (caselle is not null && c.Ruolo == "righe")
            valore = Obj(("const", caselle.Righe));
        else if (caselle is not null && c.Ruolo == "colonne")
            valore = Obj(("const", caselle.Colonne));
        else
            valore = Base(c);

        // In creazione la soluzione c'è sempre: i campi con quel ruolo non ammettono null.
        return c.Obbligatorio || c.Ruolo == "soluzione" ? valore : Nullable(valore);
    }

    private static JsonNode Base(CampoDescrittore c) => c.Tipo switch
    {
        "testo" => Obj(("type", "string"), ("minLength", 1), ("maxLength", c.Max ?? 1)),
        "testo_lungo" => Obj(("type", "string"), ("minLength", 1), ("maxLength", 20000)),
        "intero" => Intero(c.Ruolo),
        "decimale" => Obj(("type", "number"), ("minimum", -99999999.9999), ("maximum", 99999999.9999)),
        "booleano" => Obj(("type", "boolean")),
        "data" => Obj(("type", "string"), ("pattern", PatternData)),
        "enumerazione" => Obj(("type", "string"), ("enum", Array(c.Valori))),
        "griglia" => Obj(("$ref", "#/$defs/griglia")),
        _ => throw new InvalidOperationException($"Tipo di campo non gestito: {c.Tipo}"),
    };

    private static JsonObject Intero(string? ruolo) => ruolo switch
    {
        "righe" or "colonne" => Obj(("type", "integer"), ("minimum", 1), ("maximum", 60)),
        "numero" => Obj(("type", "integer"), ("minimum", 1), ("maximum", 999)),
        _ => Obj(("type", "integer"), ("minimum", int.MinValue), ("maximum", int.MaxValue)),
    };

    private static JsonObject Voce(ElencoDescrittore e, BriefCreazione? brief, SchemaCaselle? caselle)
    {
        var proprieta = new JsonObject();
        foreach (var campo in e.Campi)
            proprieta[campo.Nome] = ValoreCampo(campo, brief, caselle);

        return Obj(
            ("type", "object"), ("additionalProperties", false),
            ("required", Array(e.Campi.Select(c => c.Nome))), ("properties", proprieta));
    }

    private static JsonObject ArrayElenco(ElencoDescrittore e, string riferimento, SchemaCaselle? caselle, bool conPrefissi)
    {
        var voce = Obj(("$ref", riferimento));
        if (caselle is null || !EDefinizioni(e))
            return Obj(("type", "array"), ("minItems", 0), ("maxItems", 500), ("items", voce));

        var n = caselle.Parole.Count;
        if (!conPrefissi)
            return Obj(("type", "array"), ("minItems", n), ("maxItems", n), ("items", voce));

        var prefissi = new JsonArray();
        foreach (var parola in caselle.Parole.OrderBy(p => p.Direzione == SchemaCaselle.Orizzontale ? 0 : 1).ThenBy(p => p.Numero))
            prefissi.Add(VocePrefisso(e, riferimento, parola));

        return Obj(("type", "array"), ("minItems", n), ("maxItems", n), ("prefixItems", prefissi), ("items", false));
    }

    private static JsonObject VocePrefisso(ElencoDescrittore e, string riferimento, ParolaSchema parola)
    {
        var proprieta = new JsonObject();
        foreach (var campo in e.Campi)
        {
            switch (campo.Ruolo)
            {
                case "numero":
                    proprieta[campo.Nome] = Obj(("const", parola.Numero));
                    break;
                case "direzione":
                    proprieta[campo.Nome] = Obj(("const", parola.Direzione));
                    break;
                case "soluzione":
                    proprieta[campo.Nome] = Obj(("type", "string"), ("pattern", $"^[A-Z]{{{parola.Lunghezza}}}$"));
                    break;
            }
        }

        return Obj(("$ref", riferimento), ("properties", proprieta));
    }

    private static bool EDefinizioni(ElencoDescrittore e) =>
        new[] { "numero", "direzione", "definizione" }.All(r => e.Campi.Any(c => c.Ruolo == r));

    private static JsonObject Griglia(string resa, SchemaCaselle? caselle)
    {
        var logica = resa == "griglia_logica";
        var alfabeto = logica ? "^[.#1-9]+$" : "^[#A-Z]+$";
        JsonNode righe;
        JsonNode colonne;
        JsonNode celle;
        JsonNode numeri;
        if (caselle is not null)
        {
            righe = Obj(("const", caselle.Righe));
            colonne = Obj(("const", caselle.Colonne));
            var prefissi = new JsonArray();
            foreach (var riga in caselle.Celle)
                prefissi.Add(Obj(("type", "string"), ("pattern", PatternRiga(riga))));
            celle = Obj(
                ("type", "array"), ("minItems", caselle.Righe), ("maxItems", caselle.Righe),
                ("prefixItems", prefissi), ("items", false));
            var elenco = new JsonArray();
            foreach (var n in caselle.Numeri)
                elenco.Add(Obj(("riga", n.Riga), ("colonna", n.Colonna), ("numero", n.Numero)));
            numeri = Obj(("const", elenco));
        }
        else
        {
            righe = Obj(("type", "integer"), ("minimum", 1), ("maximum", 60));
            colonne = Obj(("type", "integer"), ("minimum", 1), ("maximum", 60));
            celle = Obj(
                ("type", "array"), ("minItems", 1), ("maxItems", 60),
                ("items", Obj(("type", "string"), ("minLength", 1), ("maxLength", 60), ("pattern", alfabeto))));
            numeri = Obj(
                ("type", "array"), ("maxItems", logica ? 0 : 1000),
                ("items", Obj(
                    ("type", "object"), ("additionalProperties", false),
                    ("required", Array(["riga", "colonna", "numero"])),
                    ("properties", Obj(
                        ("riga", Obj(("type", "integer"), ("minimum", 1), ("maximum", 60))),
                        ("colonna", Obj(("type", "integer"), ("minimum", 1), ("maximum", 60))),
                        ("numero", Obj(("type", "integer"), ("minimum", 1), ("maximum", 999))))))));
        }

        return Obj(
            ("type", "object"), ("additionalProperties", false),
            ("required", Array(["righe", "colonne", "celle", "numeri"])),
            ("properties", Obj(("righe", righe), ("colonne", colonne), ("celle", celle), ("numeri", numeri))));
    }

    // Ogni casella nera resta '#', ogni tratto di n bianche diventa [A-Z]{n}.
    private static string PatternRiga(string riga)
    {
        var testo = new System.Text.StringBuilder("^");
        var conta = 0;
        foreach (var carattere in riga)
        {
            if (carattere == '#')
            {
                Chiudi(testo, ref conta);
                testo.Append('#');
            }
            else
            {
                conta++;
            }
        }

        Chiudi(testo, ref conta);
        return testo.Append('$').ToString();

        static void Chiudi(System.Text.StringBuilder t, ref int n)
        {
            if (n > 0)
                t.Append($"[A-Z]{{{n}}}");
            n = 0;
        }
    }

    private static JsonObject Nullable(JsonNode valore) =>
        Obj(("anyOf", new JsonArray(Obj(("type", "null")), valore)));

    private static JsonArray Array(IEnumerable<string> valori)
    {
        var a = new JsonArray();
        foreach (var v in valori)
            a.Add(v);
        return a;
    }

    private static JsonObject Obj(params (string Nome, object? Valore)[] proprieta)
    {
        var o = new JsonObject();
        foreach (var (nome, valore) in proprieta)
        {
            o[nome] = valore switch
            {
                null => null,
                JsonNode nodo => nodo,
                string s => JsonValue.Create(s),
                bool b => JsonValue.Create(b),
                int i => JsonValue.Create(i),
                double x => JsonValue.Create(x),
                _ => throw new InvalidOperationException("Valore JSON non gestito"),
            };
        }

        return o;
    }
}
