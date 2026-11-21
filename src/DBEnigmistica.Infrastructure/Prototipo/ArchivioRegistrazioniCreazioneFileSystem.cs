using System.Globalization;
using System.Text;
using System.Text.Json;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Infrastructure.Prototipo;

/// <summary>Fabbrica dell'archivio delle Registrazioni di creazione su file system.</summary>
public sealed class FabbricaArchivioRegistrazioniCreazioneFileSystem : IFabbricaArchivioRegistrazioniCreazione
{
    public IArchivioRegistrazioniCreazione Apri(string cartella) => new ArchivioRegistrazioniCreazioneFileSystem(cartella);
}

/// <summary>Registrazioni come <c>&lt;fornitore&gt;/&lt;modello&gt;/&lt;versione_prompt&gt;/&lt;impronta_richiesta&gt;.json</c> (progetto 1b.1, sezione 1.6).</summary>
public sealed class ArchivioRegistrazioniCreazioneFileSystem(string cartella) : IArchivioRegistrazioniCreazione
{
    private const int VersioneFormato = 1;
    private const string FormatoData = "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'";

    public async Task SalvaAsync(RegistrazioneCreazione registrazione, CancellationToken cancellationToken = default)
    {
        var percorso = Percorso(registrazione.Chiave);
        Directory.CreateDirectory(Path.GetDirectoryName(percorso)!);
        var temporaneo = percorso + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporaneo, Serializza(registrazione), new UTF8Encoding(false), cancellationToken);
            File.Move(temporaneo, percorso, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaneo))
                File.Delete(temporaneo);
        }
    }

    public async Task<RegistrazioneCreazione?> CaricaAsync(ChiaveRegistrazione chiave, CancellationToken cancellationToken = default)
    {
        var percorso = Percorso(chiave);
        if (!File.Exists(percorso))
            return null;

        try
        {
            return Leggi(await File.ReadAllTextAsync(percorso, Encoding.UTF8, cancellationToken), chiave);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException
                                       or ArgumentException or InvalidCastException or OverflowException or IOException)
        {
            throw new RegistrazioneNonValidaException($"{chiave}: il file non è leggibile ({ex.GetType().Name}).", ex);
        }
    }

    private string Percorso(ChiaveRegistrazione c) =>
        Path.Combine(cartella, c.Fornitore, c.Modello, c.VersionePrompt, c.ImprontaPagina + ".json");

    private static string Serializza(RegistrazioneCreazione r)
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            w.WriteStartObject();
            w.WriteNumber("versione_formato", VersioneFormato);
            w.WriteString("uso", ChiamataAi.UsoCreazione);
            w.WriteStartObject("chiave");
            w.WriteString("impronta_richiesta", r.Chiave.ImprontaPagina);
            w.WriteString("versione_prompt", r.Chiave.VersionePrompt);
            w.WriteString("fornitore", r.Chiave.Fornitore);
            w.WriteString("modello", r.Chiave.Modello);
            w.WriteEndObject();
            w.WriteString("registrata_il", r.RegistrataIl.UtcDateTime.ToString(FormatoData, CultureInfo.InvariantCulture));
            w.WriteString("tipologia", r.Tipologia);
            w.WriteNumber("versione_tipologia", r.VersioneTipologia);
            w.WriteString("esito", r.Esito == EsitoEstrazione.Riuscita ? "riuscita" : "risposta_non_valida");
            w.WriteString("risposta_grezza", r.RispostaGrezza);
            w.WriteString("risposta_sha256", r.RispostaSha256);
            ScriviMetadati(w, r.Metadati);
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(memoria.ToArray()) + "\n";
    }

    private static void ScriviMetadati(Utf8JsonWriter w, MetadatiChiamata m)
    {
        w.WriteStartObject("metadati");
        w.WriteNumber("durata_ms", m.DurataMs);
        Numero(w, "token_ingresso", m.TokenIngresso);
        Numero(w, "token_uscita", m.TokenUscita);
        Numero(w, "token_cache_creazione", m.TokenCacheCreazione);
        Numero(w, "token_cache_lettura", m.TokenCacheLettura);
        Testo(w, "costo", m.Costo?.ToString("F6", CultureInfo.InvariantCulture));
        Testo(w, "costo_valuta", m.CostoValuta);
        Testo(w, "costo_tipo", m.CostoTipo);
        w.WriteEndObject();
    }

    private static void Numero(Utf8JsonWriter w, string nome, int? valore)
    {
        if (valore is { } v)
            w.WriteNumber(nome, v);
        else
            w.WriteNull(nome);
    }

    private static void Testo(Utf8JsonWriter w, string nome, string? valore)
    {
        if (valore is null)
            w.WriteNull(nome);
        else
            w.WriteString(nome, valore);
    }

    private static RegistrazioneCreazione Leggi(string testo, ChiaveRegistrazione atteso)
    {
        using var documento = JsonDocument.Parse(testo);
        var radice = documento.RootElement;
        if (radice.GetProperty("versione_formato").GetInt32() != VersioneFormato || radice.GetProperty("uso").GetString() != ChiamataAi.UsoCreazione)
            throw new FormatException("versione_formato");

        var k = radice.GetProperty("chiave");
        var chiave = new ChiaveRegistrazione(
            k.GetProperty("impronta_richiesta").GetString()!,
            k.GetProperty("versione_prompt").GetString()!,
            k.GetProperty("fornitore").GetString()!,
            k.GetProperty("modello").GetString()!);
        if (chiave != atteso)
            throw new FormatException("chiave");

        var esito = radice.GetProperty("esito").GetString() switch
        {
            "riuscita" => EsitoEstrazione.Riuscita,
            "risposta_non_valida" => EsitoEstrazione.RispostaNonValida,
            _ => throw new FormatException("esito"),
        };
        var m = radice.GetProperty("metadati");
        var metadati = new MetadatiChiamata(
            m.GetProperty("durata_ms").GetInt32(),
            IntOpzionale(m, "token_ingresso"),
            IntOpzionale(m, "token_uscita"),
            IntOpzionale(m, "token_cache_creazione"),
            IntOpzionale(m, "token_cache_lettura"),
            TestoOpzionale(m, "costo") is { } costo ? decimal.Parse(costo, CultureInfo.InvariantCulture) : null,
            TestoOpzionale(m, "costo_valuta"),
            TestoOpzionale(m, "costo_tipo"));

        return new RegistrazioneCreazione(
            chiave,
            DateTimeOffset.Parse(radice.GetProperty("registrata_il").GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
            radice.GetProperty("tipologia").GetString()!,
            radice.GetProperty("versione_tipologia").GetInt32(),
            esito,
            radice.GetProperty("risposta_grezza").GetString()!,
            radice.GetProperty("risposta_sha256").GetString()!,
            metadati);
    }

    private static int? IntOpzionale(JsonElement e, string nome) =>
        e.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;

    private static string? TestoOpzionale(JsonElement e, string nome) =>
        e.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
