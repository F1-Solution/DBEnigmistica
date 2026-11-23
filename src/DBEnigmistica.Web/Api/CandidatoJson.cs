using System.Text;
using System.Text.Json;
using DBEnigmistica.Application.Creazione;

namespace DBEnigmistica.Web.Api;

/// <summary>Rappresentazione JSON dei candidati nelle risposte delle API del prototipo.</summary>
internal static class CandidatoJson
{
    public static string Scrivi(Action<Utf8JsonWriter> contenuto)
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria))
            contenuto(w);
        return Encoding.UTF8.GetString(memoria.ToArray());
    }

    public static void Candidato(Utf8JsonWriter w, Candidato c, IReadOnlyList<string>? controlli = null)
    {
        w.WriteStartObject();
        w.WriteString("id", c.Id);
        w.WriteString("tipologia", c.Tipologia);
        w.WriteNumber("versioneTipologia", c.VersioneTipologia);
        w.WriteNumber("ordineGenerazione", c.OrdineGenerazione);
        w.WriteString("stato", c.Stato);
        w.WriteString("modo", c.Modo);
        Testo(w, "messaggio", c.Messaggio);
        Testo(w, "versionePrompt", c.VersionePrompt);
        Testo(w, "versioneGeneratore", c.VersioneGeneratore);
        if (c.Seme is { } seme)
            w.WriteNumber("seme", seme);
        else
            w.WriteNull("seme");
        w.WriteNumber("tempoAttivoMs", c.TempoAttivoMs);
        if (c.UltimaAttivitaIl is { } ultima)
            w.WriteString("ultimaAttivitaIl", DateTime.SpecifyKind(ultima, DateTimeKind.Utc));
        else
            w.WriteNull("ultimaAttivitaIl");
        Testo(w, "motivoScarto", c.MotivoScarto);
        Grezzo(w, "brief", c.Brief);
        Grezzo(w, "contenutoGenerato", c.ContenutoGenerato);
        Grezzo(w, "contenutoFinale", c.ContenutoFinale);
        Grezzo(w, "differenze", c.Differenze);
        Grezzo(w, "schemaCaselle", c.SchemaCaselle);
        w.WriteStartArray("controlli");
        foreach (var controllo in controlli ?? [])
            w.WriteStringValue(controllo);
        w.WriteEndArray();
        w.WriteEndObject();
    }

    public static string Tipologie(IReadOnlyList<TipologiaPrototipo> tipologie, Func<TipologiaPrototipo, string?> motivo) => Scrivi(w =>
    {
        w.WriteStartObject();
        w.WriteStartArray("tipologie");
        foreach (var t in tipologie)
        {
            w.WriteStartObject();
            w.WriteString("id", t.Id);
            w.WriteNumber("versione", t.Versione);
            w.WriteString("famiglia", t.Famiglia);
            w.WriteString("nome", t.Nome);
            w.WriteString("modo", t.Modo);
            w.WriteString("statoRegole", t.StatoRegole);
            w.WriteBoolean("regoleApprovate", t.RegoleApprovate);
            w.WriteBoolean("generazioneConsentita", t.GenerazioneConsentita);
            Testo(w, "motivoGenerazioneNonConsentita", motivo(t));
            w.WriteStartArray("campiBrief");
            foreach (var c in t.CampiBrief)
                CampoBrief(w, c);
            w.WriteEndArray();
            w.WriteEndObject();
        }

        w.WriteEndArray();
        w.WriteEndObject();
    });

    private static void CampoBrief(Utf8JsonWriter w, CampoBrief c)
    {
        w.WriteStartObject();
        w.WriteString("nome", c.Nome);
        w.WriteString("etichetta", c.Etichetta);
        w.WriteString("tipo", c.Tipo);
        w.WriteBoolean("obbligatorio", c.Obbligatorio);
        w.WriteStartArray("valori");
        foreach (var v in c.Valori)
            w.WriteStringValue(v);
        w.WriteEndArray();
        if (c.Minimo is { } minimo)
            w.WriteNumber("minimo", minimo);
        else
            w.WriteNull("minimo");
        if (c.Massimo is { } massimo)
            w.WriteNumber("massimo", massimo);
        else
            w.WriteNull("massimo");
        w.WriteEndObject();
    }

    public static string Errori(IReadOnlyList<string> errori) => Scrivi(w =>
    {
        w.WriteStartObject();
        w.WriteStartArray("errori");
        foreach (var errore in errori)
            w.WriteStringValue(errore);
        w.WriteEndArray();
        w.WriteEndObject();
    });

    private static void Testo(Utf8JsonWriter w, string nome, string? valore)
    {
        if (valore is null)
            w.WriteNull(nome);
        else
            w.WriteString(nome, valore);
    }

    private static void Grezzo(Utf8JsonWriter w, string nome, string? json)
    {
        if (json is null)
        {
            w.WriteNull(nome);
            return;
        }

        w.WritePropertyName(nome);
        w.WriteRawValue(json);
    }
}
