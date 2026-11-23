using System.Text.Json;
using System.Text.RegularExpressions;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Errore della valutazione, con un messaggio per l'utente.</summary>
public class ValutazioneException(string message, Exception? inner = null) : InvalidOperationException(message, inner);

/// <summary>Il set (set.json o un suo file) non è conforme al formato.</summary>
public sealed class SetNonValidoException(string message, Exception? inner = null) : ValutazioneException(message, inner);

public enum TipoSet
{
    Sintetico,
    Riferimento
}

/// <summary>Una pagina del set: <c>Pdf</c> è un percorso relativo alla cartella del set, oppure <c>null</c> se la produce il generatore.</summary>
public sealed record PaginaSet(string Id, string Origine, string? Pdf, int? NumeroPagina);

public sealed record SetValutazione(string Cartella, string Nome, TipoSet Tipo, string? Generatore, IReadOnlyList<PaginaSet> Pagine);

/// <summary>Legge set.json (progetto 1.1, sezione 6.1) rifiutando id fuori forma e percorsi che escono dalla cartella del set.</summary>
public static partial class LettoreSet
{
    public const string NomeFile = "set.json";
    private const int MassimoPagine = 1000;

    public static SetValutazione Leggi(string cartella)
    {
        var percorso = Path.Combine(cartella, NomeFile);
        if (!File.Exists(percorso))
            throw new SetNonValidoException($"Manca set.json nella cartella del set: {cartella}");

        return Interpreta(cartella, File.ReadAllText(percorso));
    }

    public static SetValutazione Interpreta(string cartella, string json)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            return Costruisci(cartella, documento.RootElement);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException and not ValutazioneException or FormatException or KeyNotFoundException)
        {
            throw new SetNonValidoException("set.json non è valido: " + ex.Message, ex);
        }
    }

    /// <summary>Percorso completo del PDF di una pagina, oppure <c>null</c> se lo produce il generatore.</summary>
    public static string? PercorsoPdf(SetValutazione set, PaginaSet pagina) =>
        pagina.Pdf is null ? null : Path.GetFullPath(Path.Combine(set.Cartella, pagina.Pdf));

    private static SetValutazione Costruisci(string cartella, JsonElement radice)
    {
        if (radice.ValueKind != JsonValueKind.Object)
            throw new SetNonValidoException("set.json deve essere un oggetto.");

        if (!radice.TryGetProperty("versione_formato", out var versione) || versione.ValueKind != JsonValueKind.Number || versione.GetInt32() != 1)
            throw new SetNonValidoException("set.json: versione_formato deve essere 1.");

        var nome = Testo(radice, "nome") ?? throw new SetNonValidoException("set.json: manca il nome del set.");
        var tipo = Testo(radice, "tipo") switch
        {
            "sintetico" => TipoSet.Sintetico,
            "riferimento" => TipoSet.Riferimento,
            _ => throw new SetNonValidoException("set.json: il tipo deve essere sintetico o riferimento.")
        };

        string? generatore = null;
        if (radice.TryGetProperty("generatore", out var gen))
        {
            if (tipo != TipoSet.Sintetico)
                throw new SetNonValidoException("set.json: il generatore è ammesso solo con tipo sintetico.");

            generatore = gen.ValueKind == JsonValueKind.String && NomeGeneratore().IsMatch(gen.GetString()!)
                ? gen.GetString()
                : throw new SetNonValidoException("set.json: nome del generatore non valido.");
        }

        if (!radice.TryGetProperty("pagine", out var elenco) || elenco.ValueKind != JsonValueKind.Array || elenco.GetArrayLength() is 0 or > MassimoPagine)
            throw new SetNonValidoException($"set.json: pagine deve essere un elenco da 1 a {MassimoPagine} voci.");

        var pagine = new List<PaginaSet>();
        foreach (var voce in elenco.EnumerateArray())
        {
            var pagina = LeggiPagina(voce, generatore);
            if (pagine.Any(p => p.Id == pagina.Id))
                throw new SetNonValidoException($"set.json: l'id {pagina.Id} è ripetuto.");

            pagine.Add(pagina);
        }

        return new SetValutazione(cartella, nome, tipo, generatore, pagine);
    }

    private static PaginaSet LeggiPagina(JsonElement voce, string? generatore)
    {
        var id = Testo(voce, "id");
        if (id is null || !IdPagina().IsMatch(id))
            throw new SetNonValidoException($"set.json: id di pagina non valido: '{id}'.");

        var origine = Testo(voce, "origine");
        if (origine is not ("digitale" or "scansione"))
            throw new SetNonValidoException($"set.json: pagina {id}: l'origine deve essere digitale o scansione.");

        int? numero = voce.TryGetProperty("numero_pagina", out var n) && n.ValueKind == JsonValueKind.Number ? n.GetInt32() : null;
        if (numero is < 1)
            throw new SetNonValidoException($"set.json: pagina {id}: numero_pagina deve partire da 1.");

        string? pdf = null;
        if (voce.TryGetProperty("pdf", out var p) && p.ValueKind != JsonValueKind.Null)
        {
            pdf = p.ValueKind == JsonValueKind.String ? p.GetString() : null;
            if (pdf is null || !PercorsoInterno(pdf))
                throw new SetNonValidoException($"set.json: pagina {id}: il percorso del PDF deve essere relativo e interno alla cartella del set.");
        }

        if (pdf is null && generatore is null)
            throw new SetNonValidoException($"set.json: pagina {id}: serve un PDF oppure un generatore.");

        if (numero is null)
            throw new SetNonValidoException($"set.json: pagina {id}: manca numero_pagina.");

        return new PaginaSet(id, origine, pdf, numero);
    }

    // Rifiuta vuoto, assoluto, con unità o condivisione di rete, e ogni segmento "..".
    private static bool PercorsoInterno(string percorso)
    {
        if (string.IsNullOrWhiteSpace(percorso) || percorso.Contains(':', StringComparison.Ordinal) || percorso.Contains('\0', StringComparison.Ordinal))
            return false;

        if (percorso.StartsWith('/') || percorso.StartsWith('\\') || Path.IsPathRooted(percorso))
            return false;

        return !percorso.Split('/', '\\').Any(segmento => segmento == "..");
    }

    private static string? Testo(JsonElement oggetto, string nome) =>
        oggetto.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{0,63}$")]
    private static partial Regex IdPagina();

    [GeneratedRegex("^[a-z][a-z0-9-]{0,39}$")]
    private static partial Regex NomeGeneratore();
}
