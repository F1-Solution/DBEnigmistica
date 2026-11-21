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
