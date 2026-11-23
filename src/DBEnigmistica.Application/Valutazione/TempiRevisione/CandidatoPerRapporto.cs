using System.Text.Json;

namespace DBEnigmistica.Application.Valutazione.TempiRevisione;

/// <summary>Il minimo di un candidato che serve al rapporto: Tipologia, versione, stato e tempo attivo.</summary>
public sealed record CandidatoPerRapporto(string Tipologia, int Versione, string Stato, long TempoAttivoMs);

/// <summary>Lettura dei candidati dello schema <c>prototipo</c> per il rapporto, in sola lettura.</summary>
public interface ILettoreCandidatiRapporto
{
    Task<IReadOnlyList<CandidatoPerRapporto>> LeggiAsync(CancellationToken cancellationToken = default);
}

/// <summary>Candidati da un file JSON (set sintetico): il tempo è <c>tempo_attivo_ms</c> oppure si calcola dagli <c>istanti_ms</c>.</summary>
public static class LettoreCandidatiDaFile
{
    public static IReadOnlyList<CandidatoPerRapporto> Leggi(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            using var documento = JsonDocument.Parse(json);
            var radice = documento.RootElement;
            if (radice.GetProperty("versione_formato").GetInt32() != 1)
                throw new FormatException("Formato dei candidati sconosciuto: serve versione_formato 1.");

            return
            [
                .. radice.GetProperty("candidati").EnumerateArray().Select(c => Candidato(c)),
            ];
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new FormatException($"File dei candidati non leggibile: {ex.Message}", ex);
        }
    }

    private static CandidatoPerRapporto Candidato(JsonElement c)
    {
        var tempo = c.TryGetProperty("istanti_ms", out var istanti)
            ? CalcoloTempoAttivo.Da([.. istanti.EnumerateArray().Select(i => i.GetInt64())])
            : c.GetProperty("tempo_attivo_ms").GetInt64();
        return tempo < 0
            ? throw new FormatException("Il tempo attivo di un candidato è negativo.")
            : new CandidatoPerRapporto(
                c.GetProperty("tipologia").GetString()!,
                c.GetProperty("versione_tipologia").GetInt32(),
                c.GetProperty("stato").GetString()!,
                tempo);
    }
}
