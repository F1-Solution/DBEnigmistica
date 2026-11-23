using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DBEnigmistica.Application.Valutazione.TempiRevisione;

public sealed record TipologiaNelRapporto(string Tipologia, IReadOnlyList<int> Versioni);

public sealed record MinutiRapporto(double? Mediana, double? Q1, double? Q3, double? Minimo, double? Massimo);

public sealed record SogliaG1(int MinutiMassimi, double QuotaMinima, bool? TempoEntroSoglia, bool? QuotaEntroSoglia);

public sealed record GruppoRapporto(
    string Gruppo,
    IReadOnlyList<TipologiaNelRapporto> Tipologie,
    int Generati,
    int Validi,
    int NonValidiSchema,
    int ErroriGenerazione,
    double? QuotaNonValidi,
    int Rivisti,
    int NonRivisti,
    int Accettati,
    int Scartati,
    double? QuotaAccettata,
    MinutiRapporto Minuti,
    bool MisuraIndicativa,
    SogliaG1? SogliaG1);

/// <summary>Rapporto <c>review-time</c> (progetto 1b.1, sezione 8.2): confronta con le soglie di G1, non decide.</summary>
public sealed record RapportoTempiRevisione(DateTimeOffset GeneratoIl, IReadOnlyList<GruppoRapporto> Gruppi)
{
    public string InJson()
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            w.WriteStartObject();
            w.WriteNumber("versione_formato", 1);
            w.WriteString("generato_il", GeneratoIl.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
            w.WriteStartArray("gruppi");
            foreach (var g in Gruppi)
                ScriviGruppo(w, g);
            w.WriteEndArray();
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(memoria.ToArray()) + "\n";
    }

    public string InMarkdown()
    {
        var s = new StringBuilder();
        s.Append("# Tempi di revisione\n\n");
        s.Append("Generato il ").Append(GeneratoIl.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(" UTC. ");
        s.Append("Il rapporto mette a confronto con le soglie di G1, non decide.\n\n");
        s.Append("| Gruppo | Generati | Validi | Non validi | Errori | Quota non validi | Rivisti | Non rivisti | Accettati | Scartati | Quota accettata | Mediana (min) | Q1 | Q3 | Min | Max | Soglia G1 | Note |\n");
        s.Append("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|\n");
        foreach (var g in Gruppi)
        {
            s.Append("| ").Append(g.Gruppo).Append(" | ").Append(g.Generati).Append(" | ").Append(g.Validi).Append(" | ")
                .Append(g.NonValidiSchema).Append(" | ").Append(g.ErroriGenerazione).Append(" | ").Append(Testo(g.QuotaNonValidi)).Append(" | ")
                .Append(g.Rivisti).Append(" | ").Append(g.NonRivisti).Append(" | ")
                .Append(g.Accettati).Append(" | ").Append(g.Scartati).Append(" | ").Append(Testo(g.QuotaAccettata)).Append(" | ")
                .Append(Testo(g.Minuti.Mediana)).Append(" | ").Append(Testo(g.Minuti.Q1)).Append(" | ").Append(Testo(g.Minuti.Q3)).Append(" | ")
                .Append(Testo(g.Minuti.Minimo)).Append(" | ").Append(Testo(g.Minuti.Massimo)).Append(" | ").Append(TestoSoglia(g.SogliaG1))
                .Append(" | ").Append(g.MisuraIndicativa ? "misura indicativa" : string.Empty).Append(" |\n");
        }

        return s.ToString();
    }

    private static void ScriviGruppo(Utf8JsonWriter w, GruppoRapporto g)
    {
        w.WriteStartObject();
        w.WriteString("gruppo", g.Gruppo);
        w.WriteStartArray("tipologie");
        foreach (var t in g.Tipologie)
        {
            w.WriteStartObject();
            w.WriteString("tipologia", t.Tipologia);
            w.WriteStartArray("versioni");
            foreach (var v in t.Versioni)
                w.WriteNumberValue(v);
            w.WriteEndArray();
            w.WriteEndObject();
        }

        w.WriteEndArray();
        w.WriteNumber("generati", g.Generati);
        w.WriteNumber("validi", g.Validi);
        w.WriteNumber("non_validi_schema", g.NonValidiSchema);
        w.WriteNumber("errori_generazione", g.ErroriGenerazione);
        Numero(w, "quota_non_validi", g.QuotaNonValidi);
        w.WriteNumber("rivisti", g.Rivisti);
        w.WriteNumber("non_rivisti", g.NonRivisti);
        w.WriteNumber("accettati", g.Accettati);
        w.WriteNumber("scartati", g.Scartati);
        Numero(w, "quota_accettata", g.QuotaAccettata);
        w.WriteStartObject("minuti");
        Numero(w, "mediana", g.Minuti.Mediana);
        Numero(w, "q1", g.Minuti.Q1);
        Numero(w, "q3", g.Minuti.Q3);
        Numero(w, "minimo", g.Minuti.Minimo);
        Numero(w, "massimo", g.Minuti.Massimo);
        w.WriteEndObject();
        w.WriteBoolean("misura_indicativa", g.MisuraIndicativa);
        if (g.SogliaG1 is { } s)
        {
            w.WriteStartObject("soglia_g1");
            w.WriteNumber("minuti_massimi", s.MinutiMassimi);
            w.WriteNumber("quota_minima", s.QuotaMinima);
            Booleano(w, "tempo_entro_soglia", s.TempoEntroSoglia);
            Booleano(w, "quota_entro_soglia", s.QuotaEntroSoglia);
            w.WriteEndObject();
        }
        else
        {
            w.WriteNull("soglia_g1");
        }

        w.WriteEndObject();
    }

    private static void Numero(Utf8JsonWriter w, string nome, double? valore)
    {
        if (valore is { } v)
            w.WriteNumber(nome, v);
        else
            w.WriteNull(nome);
    }

    private static void Booleano(Utf8JsonWriter w, string nome, bool? valore)
    {
        if (valore is { } v)
            w.WriteBoolean(nome, v);
        else
            w.WriteNull(nome);
    }

    private static string Testo(double? valore) =>
        valore is { } v ? v.ToString("0.####", CultureInfo.InvariantCulture).Replace('.', ',') : "-";

    private static string TestoSoglia(SogliaG1? soglia) => soglia is null
        ? "-"
        : $"tempo ≤ {soglia.MinutiMassimi} min: {SiNo(soglia.TempoEntroSoglia)}; quota ≥ {Testo(soglia.QuotaMinima)}: {SiNo(soglia.QuotaEntroSoglia)}";

    private static string SiNo(bool? valore) => valore switch { true => "sì", false => "no", null => "n.d." };
}
