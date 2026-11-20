using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DBEnigmistica.AI.Services;

/// <summary>Classe di un'uscita di Claude Code non interattivo.</summary>
public enum TipoUscitaClaudeCode
{
    Successo,
    FinestraEsaurita,
    Transitorio,
    Definitivo
}

/// <summary>Uscita di Claude Code interpretata: formato provvisorio dalla documentazione, da confermare con la prova della 1.1.</summary>
public sealed record UscitaClaudeCode(
    TipoUscitaClaudeCode Tipo,
    string? Json,
    string? Messaggio,
    DateTimeOffset? Ripresa,
    int? TokenIngresso,
    int? TokenUscita,
    decimal? CostoUsd,
    int? TokenCacheCreazione = null,
    int? TokenCacheLettura = null,
    int? CodiceUscita = null)
{
    private readonly record struct Consumi(int? Ingresso, int? Uscita, int? CacheCreazione, int? CacheLettura, decimal? Costo);

    private static readonly Regex Limite = new(
        @"hit your (?:session|weekly|opus|sonnet)\s+limit|usage limit reached|(?:5-hour|session|weekly) limit reached",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OraDiRipresa = new(
        @"resets\s+(?:(?<giorno>Mon|Tue|Wed|Thu|Fri|Sat|Sun)[a-z]*\s+)?(?<ora>\d{1,2})(?::(?<minuti>\d{2}))?\s*(?<ampm>am|pm)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex InizioEpoch = new(@"\|(?<epoch>\d{9,11})\b", RegexOptions.Compiled);

    private static readonly Regex Autenticazione = new(
        @"not logged in|/login|failed to authenticate|authentication|invalid api key|login expired|login reached|oauth",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Transitorio = new(
        @"\b(?:429|500|502|503|504|529)\b|overloaded|temporarily|rate limit|timed out|timeout|econnreset|etimedout|connection",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Recinto = new(@"^\s*```(?:json)?\s*(?<corpo>.*?)\s*```\s*$", RegexOptions.Singleline | RegexOptions.Compiled);

    public static UscitaClaudeCode Analizza(string stdout, string stderr, int codiceUscita, TimeProvider tempo)
    {
        JsonElement radice;
        try
        {
            using var documento = JsonDocument.Parse(stdout);
            radice = documento.RootElement.Clone();
        }
        catch (JsonException)
        {
            return Fallita(codiceUscita == 0 ? string.Empty : $"{stdout} {stderr}", tempo, default, codiceUscita);
        }

        if (radice.ValueKind != JsonValueKind.Object)
            return Fallita(string.Empty, tempo, default, codiceUscita);

        var consumi = LeggiConsumi(radice);
        var testo = radice.TryGetProperty("result", out var r) && r.ValueKind == JsonValueKind.String ? r.GetString() ?? string.Empty : string.Empty;
        var errore = radice.TryGetProperty("is_error", out var e) && e.ValueKind == JsonValueKind.True;

        if (errore || codiceUscita != 0)
            return Fallita($"{testo} {stderr}", tempo, consumi, codiceUscita);

        if (radice.TryGetProperty("structured_output", out var strutturato) && strutturato.ValueKind != JsonValueKind.Null)
            return Nuova(TipoUscitaClaudeCode.Successo, strutturato.GetRawText(), null, null, consumi, codiceUscita);

        var corpo = Recinto.Match(testo) is { Success: true } m ? m.Groups["corpo"].Value : testo;
        try
        {
            using var _ = JsonDocument.Parse(corpo);
            return Nuova(TipoUscitaClaudeCode.Successo, corpo, null, null, consumi, codiceUscita);
        }
        catch (JsonException)
        {
            return Nuova(TipoUscitaClaudeCode.Successo, null, "La risposta non è un JSON", null, consumi, codiceUscita);
        }
    }

    // Il messaggio salvato è una descrizione fissa con il codice d'uscita: mai testo della pagina o del modello.
    private static UscitaClaudeCode Fallita(string testo, TimeProvider tempo, Consumi consumi, int codiceUscita)
    {
        if (Limite.IsMatch(testo))
        {
            return Nuova(
                TipoUscitaClaudeCode.FinestraEsaurita, null, $"Finestra d'uso di Claude Code esaurita (codice d'uscita {codiceUscita})",
                CalcolaRipresa(testo, tempo), consumi, codiceUscita);
        }

        if (Autenticazione.IsMatch(testo))
        {
            return Nuova(
                TipoUscitaClaudeCode.Definitivo, null, $"Claude Code non autenticato (codice d'uscita {codiceUscita})", null, consumi, codiceUscita);
        }

        return Transitorio.IsMatch(testo)
            ? Nuova(TipoUscitaClaudeCode.Transitorio, null, $"Errore transitorio di Claude Code (codice d'uscita {codiceUscita})", null, consumi, codiceUscita)
            : Nuova(TipoUscitaClaudeCode.Definitivo, null, $"Errore di Claude Code (codice d'uscita {codiceUscita})", null, consumi, codiceUscita);
    }

    private static UscitaClaudeCode Nuova(
        TipoUscitaClaudeCode tipo, string? json, string? messaggio, DateTimeOffset? ripresa, Consumi c, int codiceUscita) =>
        new(tipo, json, messaggio, ripresa, c.Ingresso, c.Uscita, c.Costo, c.CacheCreazione, c.CacheLettura, codiceUscita);

    private static DateTimeOffset? CalcolaRipresa(string testo, TimeProvider tempo)
    {
        if (InizioEpoch.Match(testo) is { Success: true } epoch)
            return DateTimeOffset.FromUnixTimeSeconds(long.Parse(epoch.Groups["epoch"].Value, CultureInfo.InvariantCulture));

        if (OraDiRipresa.Match(testo) is not { Success: true } ora)
            return null;

        var zona = tempo.LocalTimeZone;
        var adesso = TimeZoneInfo.ConvertTime(tempo.GetUtcNow(), zona);
        var ore = int.Parse(ora.Groups["ora"].Value, CultureInfo.InvariantCulture) % 12
                  + (ora.Groups["ampm"].Value.Equals("pm", StringComparison.OrdinalIgnoreCase) ? 12 : 0);
        var minuti = ora.Groups["minuti"].Success ? int.Parse(ora.Groups["minuti"].Value, CultureInfo.InvariantCulture) : 0;

        var candidato = adesso.Date.AddHours(ore).AddMinutes(minuti);
        if (ora.Groups["giorno"].Success)
        {
            var giorno = (DayOfWeek)Array.IndexOf(["sun", "mon", "tue", "wed", "thu", "fri", "sat"], ora.Groups["giorno"].Value[..3].ToLowerInvariant());
            while (candidato.DayOfWeek != giorno || candidato <= adesso.DateTime)
                candidato = candidato.AddDays(1);
        }
        else if (candidato <= adesso.DateTime)
        {
            candidato = candidato.AddDays(1);
        }

        return new DateTimeOffset(candidato, zona.GetUtcOffset(candidato));
    }

    private static Consumi LeggiConsumi(JsonElement radice)
    {
        int? ingresso = null;
        int? uscita = null;
        int? cacheCreazione = null;
        int? cacheLettura = null;
        if (radice.TryGetProperty("usage", out var uso) && uso.ValueKind == JsonValueKind.Object)
        {
            ingresso = Somma(uso, "input_tokens");
            uscita = Somma(uso, "output_tokens");
            cacheCreazione = Somma(uso, "cache_creation_input_tokens");
            cacheLettura = Somma(uso, "cache_read_input_tokens");
        }

        decimal? costo = radice.TryGetProperty("total_cost_usd", out var c) && c.ValueKind == JsonValueKind.Number
                         && c.TryGetDecimal(out var valoreCosto) && valoreCosto >= 0
            ? valoreCosto
            : null;
        return new Consumi(ingresso, uscita, cacheCreazione, cacheLettura, costo);
    }

    private static int? Somma(JsonElement oggetto, params string[] campi)
    {
        long? somma = null;
        foreach (var campo in campi)
        {
            if (oggetto.TryGetProperty(campo, out var valore) && valore.ValueKind == JsonValueKind.Number
                && valore.TryGetInt32(out var numero) && numero >= 0)
                somma = (somma ?? 0) + numero;
        }

        return somma is { } totale and <= int.MaxValue ? (int)totale : null;
    }
}
