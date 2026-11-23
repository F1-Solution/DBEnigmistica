using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Valori registrati di un gruppo: i conteggi sono informativi, le metriche si confrontano.</summary>
public sealed record GruppoLineaDiBase(int Pagine, int GiochiAttesi, int GiochiEstratti, IReadOnlyDictionary<string, double?> Metriche);

/// <summary>
/// Linea di base di un set (progetto 1.1, sezione 6.3): una misura registrata contro cui la regressione confronta.
/// Una metrica con valore che scende (o il CER che sale) è una regressione; il confronto è sui 4 decimali, senza tolleranza.
/// </summary>
public sealed record LineaDiBase(
    string Set,
    string RegistrataIl,
    CondizioniRegressione Condizioni,
    IReadOnlyDictionary<string, GruppoLineaDiBase> Gruppi)
{
    public const string DataPredefinita = "2026-11-24T00:00:00Z";

    private static readonly string[] MetricheDiMerito = ["richiamo", "precisione", "tipologia_corretta", "caselle_corrette", "campi_esatti"];
    private static readonly string[] TutteLeMetriche = [.. MetricheDiMerito, "cer"];

    public static LineaDiBase DaRapporto(RapportoRegressione rapporto, string registrataIl = DataPredefinita) => new(
        rapporto.Set,
        registrataIl,
        rapporto.Condizioni,
        RapportoRegressione.NomiGruppi.ToDictionary(
            nome => nome,
            nome => new GruppoLineaDiBase(
                rapporto.Gruppi[nome].Pagine, rapporto.Gruppi[nome].GiochiAttesi, rapporto.Gruppi[nome].GiochiEstratti,
                TutteLeMetriche.ToDictionary(m => m, m => Valore(rapporto.Gruppi[nome], m)))));

    /// <summary>Copia con una metrica cambiata.</summary>
    public LineaDiBase Con(string gruppo, string metrica, double? valore)
    {
        var metriche = new Dictionary<string, double?>(Gruppi[gruppo].Metriche) { [metrica] = valore };
        var gruppi = new Dictionary<string, GruppoLineaDiBase>(Gruppi) { [gruppo] = Gruppi[gruppo] with { Metriche = metriche } };
        return this with { Gruppi = gruppi };
    }

    /// <summary>Le regressioni della misura rispetto alla Linea di base; condizioni o set diversi sono un errore.</summary>
    public IReadOnlyList<string> Confronta(RapportoRegressione rapporto)
    {
        if (!string.Equals(Set, rapporto.Set, StringComparison.Ordinal))
            throw new ValutazioneException($"La Linea di base è del set '{Set}', non di '{rapporto.Set}'.");

        if (Condizioni != rapporto.Condizioni)
        {
            throw new ValutazioneException(
                $"Le condizioni ({Descrivi(rapporto.Condizioni)}) sono diverse da quelle della Linea di base ({Descrivi(Condizioni)}): registra una Linea di base nuova.");
        }

        var regressioni = new List<string>();
        foreach (var nome in RapportoRegressione.NomiGruppi)
        {
            if (!Gruppi.TryGetValue(nome, out var registrato))
                continue;

            foreach (var metrica in TutteLeMetriche)
            {
                if (!registrato.Metriche.TryGetValue(metrica, out var base_) || base_ is not { } riferimento)
                    continue;

                var misurato = Valore(rapporto.Gruppi[nome], metrica);
                if (misurato is not { } valore)
                {
                    regressioni.Add($"{metrica} ({nome}): non applicabile, ma la Linea di base ha {Formatta(riferimento)}");
                    continue;
                }

                var peggiore = metrica == "cer"
                    ? CalcoloMetriche.Arrotonda(valore) > CalcoloMetriche.Arrotonda(riferimento)
                    : CalcoloMetriche.Arrotonda(valore) < CalcoloMetriche.Arrotonda(riferimento);
                if (peggiore)
                    regressioni.Add($"{metrica} ({nome}): {Formatta(valore)} contro {Formatta(riferimento)} della Linea di base");
            }
        }

        return regressioni;
    }

    public string InJson()
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            w.WriteStartObject();
            w.WriteNumber("versione_formato", 1);
            w.WriteString("set", Set);
            w.WriteString("registrata_il", RegistrataIl);
            w.WriteStartObject("condizioni");
            w.WriteString("fornitore", Condizioni.Fornitore);
            w.WriteString("modello", Condizioni.Modello);
            w.WriteString("versione_prompt", Condizioni.VersionePrompt);
            w.WriteNumber("dpi", Condizioni.Dpi);
            w.WriteEndObject();
            w.WriteStartObject("metriche");
            foreach (var nome in RapportoRegressione.NomiGruppi)
            {
                var g = Gruppi[nome];
                w.WriteStartObject(nome);
                w.WriteNumber("pagine", g.Pagine);
                w.WriteNumber("giochi_attesi", g.GiochiAttesi);
                w.WriteNumber("giochi_estratti", g.GiochiEstratti);
                foreach (var metrica in TutteLeMetriche)
                {
                    if (g.Metriche.GetValueOrDefault(metrica) is { } valore)
                        w.WriteNumber(metrica, valore);
                    else
                        w.WriteNull(metrica);
                }

                w.WriteEndObject();
            }

            w.WriteEndObject();
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(memoria.ToArray()) + "\n";
    }

    public static LineaDiBase Leggi(string percorso) => Interpreta(File.ReadAllText(percorso));

    public static LineaDiBase Interpreta(string json)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            var radice = documento.RootElement;
            if (radice.GetProperty("versione_formato").GetInt32() != 1)
                throw new FormatException("versione_formato");

            var c = radice.GetProperty("condizioni");
            var condizioni = new CondizioniRegressione(
                c.GetProperty("fornitore").GetString()!, c.GetProperty("modello").GetString()!,
                c.GetProperty("versione_prompt").GetString()!, c.GetProperty("dpi").GetInt32());

            var gruppi = new Dictionary<string, GruppoLineaDiBase>();
            var metriche = radice.GetProperty("metriche");
            foreach (var nome in RapportoRegressione.NomiGruppi)
            {
                var g = metriche.GetProperty(nome);
                gruppi[nome] = new GruppoLineaDiBase(
                    g.GetProperty("pagine").GetInt32(), g.GetProperty("giochi_attesi").GetInt32(), g.GetProperty("giochi_estratti").GetInt32(),
                    TutteLeMetriche.ToDictionary(
                        m => m,
                        m => g.GetProperty(m) is { ValueKind: JsonValueKind.Number } n ? (double?)n.GetDouble() : null));
            }

            return new LineaDiBase(radice.GetProperty("set").GetString()!, radice.GetProperty("registrata_il").GetString()!, condizioni, gruppi);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new ValutazioneException("linea-di-base.json non è valido: " + ex.Message, ex);
        }
    }

    private static double? Valore(MetricheGruppo g, string metrica) => metrica switch
    {
        "richiamo" => g.Richiamo,
        "precisione" => g.Precisione,
        "tipologia_corretta" => g.TipologiaCorretta,
        "caselle_corrette" => g.CaselleCorrette,
        "campi_esatti" => g.CampiEsatti,
        "cer" => g.Cer,
        _ => throw new ArgumentOutOfRangeException(nameof(metrica), metrica, "Metrica sconosciuta.")
    };

    private static string Descrivi(CondizioniRegressione c) => $"{c.Fornitore}, {c.Modello}, {c.VersionePrompt}, {c.Dpi} DPI";

    private static string Formatta(double valore) => valore.ToString("0.0000", CultureInfo.InvariantCulture);
}
