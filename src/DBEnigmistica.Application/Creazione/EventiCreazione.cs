using System.Globalization;
using System.Text;
using System.Text.Json;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Domain.Registro;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Eventi del registro della creazione: solo metadati, mai testo di prompt, brief, risposte o candidati (progetto 1b.1, sezione 4.6).</summary>
public static class EventiCreazione
{
    public const string AutoreMotore = "motore-creazione";
    public const string AutoreProprietario = "proprietario";

    public static NuovoEvento ChiamataAi(ChiamataAi c, Candidato candidato)
    {
        ArgumentNullException.ThrowIfNull(c);
        ArgumentNullException.ThrowIfNull(candidato);
        var dati = Json(w =>
        {
            w.WriteNumber("versione", 1);
            w.WriteString("uso", c.Uso);
            w.WriteString("fornitore", c.Fornitore);
            w.WriteString("modello", c.Modello);
            w.WriteString("versione_prompt", c.VersionePrompt);
            w.WriteString("impronta_richiesta", Convert.ToHexStringLower(c.ImprontaPagina));
            w.WriteString("tipologia", candidato.Tipologia);
            w.WriteNumber("versione_tipologia", candidato.VersioneTipologia);
            w.WriteString("candidato_id", candidato.Id.ToString("D"));
            w.WriteNumber("tentativo", c.Tentativo);
            w.WriteBoolean("correzione", c.Correzione);
            w.WriteString("esito", c.EsitoTesto);
            w.WriteNumber("durata_ms", c.DurataMs);
            Numero(w, "token_ingresso", c.TokenIngresso);
            Numero(w, "token_uscita", c.TokenUscita);
            Numero(w, "token_cache_creazione", c.TokenCacheCreazione);
            Numero(w, "token_cache_lettura", c.TokenCacheLettura);
            Testo(w, "costo", c.Costo?.ToString("F6", CultureInfo.InvariantCulture));
            Testo(w, "costo_valuta", c.CostoValuta);
            Testo(w, "costo_tipo", c.CostoTipo);
            Testo(w, "risposta_sha256", c.RispostaSha256 is null ? null : Convert.ToHexStringLower(c.RispostaSha256));
        });
        var avvenuto = c.AvviataIl.UtcDateTime.AddMilliseconds(c.DurataMs);
        return new NuovoEvento(avvenuto, "chiamata_ai", "chiamata_ai", c.Id.ToString("D"), AutoreMotore, dati);
    }

    public static NuovoEvento EsitoRevisione(Candidato c, int campiModificati)
    {
        ArgumentNullException.ThrowIfNull(c);
        var dati = Json(w =>
        {
            w.WriteNumber("versione", 1);
            w.WriteString("candidato_id", c.Id.ToString("D"));
            w.WriteString("tipologia", c.Tipologia);
            w.WriteNumber("versione_tipologia", c.VersioneTipologia);
            w.WriteString("modo", c.Modo);
            w.WriteNumber("ordine_generazione", c.OrdineGenerazione);
            w.WriteString("esito", c.Stato);
            Testo(w, "motivo_scarto", c.MotivoScarto);
            w.WriteNumber("tempo_attivo_ms", c.TempoAttivoMs);
            w.WriteNumber("campi_modificati", campiModificati);
            Testo(w, "contenuto_finale_sha256", c.ContenutoFinale is null ? null : Jcs.Sha256Esadecimale(c.ContenutoFinale));
            Testo(w, "versione_prompt", c.VersionePrompt);
            if (c.Seme is { } seme)
                w.WriteNumber("seme", seme);
            else
                w.WriteNull("seme");
            Testo(w, "versione_generatore", c.VersioneGeneratore);
        });
        return new NuovoEvento(c.RevisioneConclusaIl!.Value, "esito_revisione", "candidato_prototipo", c.Id.ToString("D"), AutoreProprietario, dati);
    }

    private static string Json(Action<Utf8JsonWriter> scrivi)
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria))
        {
            w.WriteStartObject();
            scrivi(w);
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(memoria.ToArray());
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
}
