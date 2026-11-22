using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DBEnigmistica.Application.Valutazione;

public sealed record CondizioniRegressione(string Fornitore, string Modello, string VersionePrompt, int Dpi);

public sealed record PaginaRapporto(string Id, string Origine, bool InBozza, bool RispostaValida, int Attesi, int Estratti, int Abbinati, int Mancanti, int InPiu);

/// <summary>Il rapporto di una regressione: nessuna data e chiavi in ordine fisso, così due esecuzioni sugli stessi dati sono identiche.</summary>
public sealed record RapportoRegressione(
    string Set,
    TipoSet Tipo,
    CondizioniRegressione Condizioni,
    int ChiamateAi,
    IReadOnlyList<PaginaRapporto> Pagine,
    IReadOnlyDictionary<string, MetricheGruppo> Gruppi)
{
    public static readonly string[] NomiGruppi = ["digitale", "scansione", "totale"];

    public string InJson()
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            w.WriteStartObject();
            w.WriteNumber("versione_formato", 1);
            w.WriteString("set", Set);
            w.WriteString("tipo", Tipo == TipoSet.Sintetico ? "sintetico" : "riferimento");
            w.WriteStartObject("condizioni");
            w.WriteString("fornitore", Condizioni.Fornitore);
            w.WriteString("modello", Condizioni.Modello);
            w.WriteString("versione_prompt", Condizioni.VersionePrompt);
            w.WriteNumber("dpi", Condizioni.Dpi);
            w.WriteEndObject();
            w.WriteNumber("chiamate_ai", ChiamateAi);
            w.WriteStartArray("pagine");
            foreach (var p in Pagine)
            {
                w.WriteStartObject();
                w.WriteString("id", p.Id);
                w.WriteString("origine", p.Origine);
                w.WriteBoolean("in_bozza", p.InBozza);
                w.WriteBoolean("risposta_valida", p.RispostaValida);
                w.WriteNumber("giochi_attesi", p.Attesi);
                w.WriteNumber("giochi_estratti", p.Estratti);
                w.WriteNumber("abbinati", p.Abbinati);
                w.WriteNumber("mancanti", p.Mancanti);
                w.WriteNumber("in_piu", p.InPiu);
                w.WriteEndObject();
            }

            w.WriteEndArray();
            w.WriteStartObject("metriche");
            foreach (var nome in NomiGruppi)
                ScriviGruppo(w, nome, Gruppi[nome]);

            w.WriteEndObject();
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(memoria.ToArray()) + "\n";
    }

    /// <summary>Il rapporto in Markdown; <paramref name="regressioni"/> è <c>null</c> se la Linea di base manca.</summary>
    public string InMarkdown(IReadOnlyList<string>? regressioni)
    {
        var s = new StringBuilder();
        s.Append("# Rapporto della regressione sull'estrazione\n\n");
        s.Append(CultureInfo.InvariantCulture, $"- Set: {Set} ({(Tipo == TipoSet.Sintetico ? "sintetico" : "riferimento")})\n");
        s.Append(CultureInfo.InvariantCulture, $"- Fornitore: {Condizioni.Fornitore}, modello: {Condizioni.Modello}\n");
        s.Append(CultureInfo.InvariantCulture, $"- Versione del prompt: {Condizioni.VersionePrompt}, risoluzione: {Condizioni.Dpi} DPI\n");
        s.Append(CultureInfo.InvariantCulture, $"- Chiamate AI: {ChiamateAi}\n");
        s.Append(regressioni is null ? "- Linea di base assente: il confronto manca.\n\n" : "- Confronto con la Linea di base: eseguito\n\n");

        s.Append("## Metriche per gruppo\n\n| Gruppo | Pagine | Attesi | Estratti | Abbinati | Mancanti | In più | Richiamo | Precisione | Tipologia corretta | Caselle corrette | CER | Campi esatti |\n|---|---|---|---|---|---|---|---|---|---|---|---|---|\n");
        foreach (var nome in NomiGruppi)
        {
            var g = Gruppi[nome];
            s.Append(CultureInfo.InvariantCulture,
                $"| {nome} | {g.Pagine} | {g.GiochiAttesi} | {g.GiochiEstratti} | {g.Abbinati} | {g.Mancanti} | {g.InPiu} | {Valore(g.Richiamo)} | {Valore(g.Precisione)} | {Valore(g.TipologiaCorretta)} | {Valore(g.CaselleCorrette)} | {Valore(g.Cer)} | {Valore(g.CampiEsatti)} |\n");
        }

        s.Append("\n## Costo e tempo medi per pagina\n\n| Gruppo | Costo medio | Pagine con costo | Tempo medio (ms) | Pagine con tempo |\n|---|---|---|---|---|\n");
        foreach (var nome in NomiGruppi)
        {
            var g = Gruppi[nome];
            s.Append(CultureInfo.InvariantCulture, $"| {nome} | {Valore(g.CostoMedio)} | {g.PagineConCosto} | {Valore(g.TempoMedioMs)} | {g.PagineConTempo} |\n");
        }

        s.Append("\n## Pagine\n\n| Pagina | Origine | Stato | Risposta | Attesi | Estratti | Abbinati |\n|---|---|---|---|---|---|---|\n");
        foreach (var p in Pagine)
        {
            s.Append(CultureInfo.InvariantCulture,
                $"| {p.Id} | {p.Origine} | {(p.InBozza ? "bozza (fuori dalle metriche)" : "corretto")} | {(p.RispostaValida ? "valida" : "non valida")} | {p.Attesi} | {p.Estratti} | {p.Abbinati} |\n");
        }

        if (regressioni is not null)
        {
            s.Append("\n## Confronto con la Linea di base\n\n");
            if (regressioni.Count == 0)
            {
                s.Append("Nessuna regressione.\n");
            }
            else
            {
                s.Append("Regressione: una metrica è peggiorata.\n\n");
                foreach (var r in regressioni)
                    s.Append("- ").Append(r).Append('\n');
            }
        }

        return s.ToString();
    }

    private static string Valore(double? valore) => valore is { } v ? v.ToString("0.0000", CultureInfo.InvariantCulture) : "n/a";

    private static void ScriviGruppo(Utf8JsonWriter w, string nome, MetricheGruppo g)
    {
        w.WriteStartObject(nome);
        w.WriteNumber("pagine", g.Pagine);
        w.WriteNumber("giochi_attesi", g.GiochiAttesi);
        w.WriteNumber("giochi_estratti", g.GiochiEstratti);
        w.WriteNumber("abbinati", g.Abbinati);
        w.WriteNumber("mancanti", g.Mancanti);
        w.WriteNumber("in_piu", g.InPiu);
        Numero(w, "richiamo", g.Richiamo);
        Numero(w, "precisione", g.Precisione);
        Numero(w, "tipologia_corretta", g.TipologiaCorretta);
        Numero(w, "caselle_corrette", g.CaselleCorrette);
        Numero(w, "cer", g.Cer);
        Numero(w, "campi_esatti", g.CampiEsatti);
        Numero(w, "costo_medio_pagina", g.CostoMedio);
        w.WriteNumber("pagine_con_costo", g.PagineConCosto);
        Numero(w, "tempo_medio_pagina_ms", g.TempoMedioMs);
        w.WriteNumber("pagine_con_tempo", g.PagineConTempo);
        w.WriteEndObject();
    }

    private static void Numero(Utf8JsonWriter w, string nome, double? valore)
    {
        if (valore is { } v)
            w.WriteNumber(nome, v);
        else
            w.WriteNull(nome);
    }
}
