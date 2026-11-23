using System.Text.Json;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Web.Api;

/// <summary>API del prototipo: elenca le Tipologie, genera, rivedi, salva, salva il tempo, accetta, scarta.</summary>
public static class PrototipoEndpoints
{
    public const string Radice = "/api/prototipo";

    public static IEndpointRouteBuilder MapPrototipo(this IEndpointRouteBuilder app)
    {
        var gruppo = app.MapGroup(Radice);

        gruppo.MapGet("/salute", () => Results.Text("{\"stato\":\"ok\"}", "application/json"));

        gruppo.MapPost("/candidati", (HttpContext http, GeneraCandidato genera, RevisioneCandidati revisione) => Esegui(http, async ct =>
        {
            var esito = await genera.EseguiAsync(await Corpo(http), ct);
            var controlli = new Dictionary<Guid, IReadOnlyList<string>>();
            foreach (var c in esito.Candidati)
                controlli[c.Id] = await revisione.ControlliAsync(c, ct);
            return Json(200, CandidatoJson.Scrivi(w =>
            {
                w.WriteStartObject();
                w.WriteStartArray("candidati");
                foreach (var c in esito.Candidati)
                    CandidatoJson.Candidato(w, c, controlli[c.Id]);
                w.WriteEndArray();
                if (esito.Valido is { } valido)
                    w.WriteString("valido", valido.Id);
                else
                    w.WriteNull("valido");
                if (esito.Messaggio is { } messaggio)
                    w.WriteString("messaggio", messaggio);
                else
                    w.WriteNull("messaggio");
                w.WriteEndObject();
            }));
        }));

        // La lettura del prossimo avvia la revisione (cambia stato): è un POST.
        gruppo.MapPost("/tipologie/{tipologia}/prossimo", (HttpContext http, string tipologia, RevisioneCandidati revisione) => Esegui(http, async ct =>
        {
            var c = await revisione.ProssimoAsync(tipologia, ct);
            if (c is null)
                return Results.NoContent();

            var controlli = await revisione.ControlliAsync(c, ct);
            return Json(200, CandidatoJson.Scrivi(w => CandidatoJson.Candidato(w, c, controlli)));
        }));

        gruppo.MapPut("/candidati/{id:guid}/finale", (HttpContext http, Guid id, RevisioneCandidati revisione) => Esegui(http, async ct =>
        {
            var c = await revisione.SalvaVersioneAsync(id, await Corpo(http), ct);
            var controlli = await revisione.ControlliAsync(c, ct);
            return Json(200, CandidatoJson.Scrivi(w => CandidatoJson.Candidato(w, c, controlli)));
        }));

        gruppo.MapPut("/candidati/{id:guid}/tempo", (HttpContext http, Guid id, RevisioneCandidati revisione) => Esegui(http, async ct =>
        {
            var (tempo, ultima) = LeggiTempo(await Corpo(http));
            var c = await revisione.SalvaTempoAsync(id, tempo, ultima, ct);
            return Json(200, CandidatoJson.Scrivi(w => CandidatoJson.Candidato(w, c)));
        }));

        gruppo.MapGet("/tipologie", (HttpContext http, TipologieDelPrototipo tipologie, IFornitoreAiCreazione fornitore) => Esegui(http, async ct =>
            Json(200, CandidatoJson.Tipologie(await tipologie.EseguiAsync(ct), t => MotivoNonConsentita(t, fornitore)))));

        gruppo.MapPost("/candidati/{id:guid}/accetta", (HttpContext http, Guid id, RevisioneCandidati revisione) => Esegui(http, async ct =>
        {
            var richiesta = LeggiEsito(await Corpo(http, facoltativo: true), RevisioneCandidati.EsitoAccetta);
            var c = await revisione.RegistraEsitoAsync(id, richiesta, ct);
            return Json(200, CandidatoJson.Scrivi(w => CandidatoJson.Candidato(w, c)));
        }));

        gruppo.MapPost("/candidati/{id:guid}/scarta", (HttpContext http, Guid id, RevisioneCandidati revisione) => Esegui(http, async ct =>
        {
            var richiesta = LeggiEsito(await Corpo(http, facoltativo: true), RevisioneCandidati.EsitoScarta);
            var c = await revisione.RegistraEsitoAsync(id, richiesta, ct);
            return Json(200, CandidatoJson.Scrivi(w => CandidatoJson.Candidato(w, c)));
        }));

        gruppo.MapDelete("/risposte-grezze", (HttpContext http, RevisioneCandidati revisione) => Esegui(http, async ct =>
            Json(200, $"{{\"cancellate\":{await revisione.CancellaRisposteGrezzeAsync(null, ct)}}}")));

        gruppo.MapDelete("/candidati/{id:guid}/risposta-grezza", (HttpContext http, Guid id, RevisioneCandidati revisione) => Esegui(http, async ct =>
            Json(200, $"{{\"cancellate\":{await revisione.CancellaRisposteGrezzeAsync(id, ct)}}}")));

        return app;
    }

    public const string MessaggioRegoleInBozza =
        "Regole in bozza: la generazione con l'AI non è disponibile finché il proprietario non approva le regole di questa Tipologia.";

    public const string MessaggioRegistrazioneMancante = "Nessuna Registrazione per questo brief: usa uno dei brief del collaudo.";

    // Perché Genera è disattivato: prima l'interruttore delle chiamate dal vivo (D8), poi le regole in bozza.
    private static string? MotivoNonConsentita(TipologiaPrototipo tipologia, IFornitoreAiCreazione fornitore)
    {
        if (tipologia.GenerazioneConsentita)
            return null;
        if (fornitore is IFornitoreAiDisattivato disattivato)
            return disattivato.Messaggio;
        return tipologia.RegoleApprovate ? "La generazione non è consentita per questa Tipologia." : MessaggioRegoleInBozza;
    }

    // Corpo della richiesta come testo UTF-8; senza corpo è un errore, salvo dove è facoltativo.
    private static async Task<string> Corpo(HttpContext http, bool facoltativo = false)
    {
        using var lettore = new StreamReader(http.Request.Body, System.Text.Encoding.UTF8);
        var testo = await lettore.ReadToEndAsync(http.RequestAborted);
        if (string.IsNullOrWhiteSpace(testo))
            return facoltativo ? "{}" : throw new RichiestaNonValidaException("Il corpo della richiesta è vuoto.");

        return testo;
    }

    private static EsitoRichiesto LeggiEsito(string corpo, string esito)
    {
        try
        {
            using var documento = JsonDocument.Parse(corpo);
            var r = documento.RootElement;
            if (r.ValueKind != JsonValueKind.Object)
                throw new RichiestaNonValidaException("Il corpo deve essere un oggetto JSON.");

            return new EsitoRichiesto(
                esito,
                Testo(r, "motivo"),
                Testo(r, "nota"),
                r.TryGetProperty("contenutoFinale", out var finale) && finale.ValueKind == JsonValueKind.Object ? finale.GetRawText() : null,
                r.TryGetProperty("tempoAttivoMs", out var ms) && ms.ValueKind != JsonValueKind.Null ? LeggiTempoAttivo(ms) : null,
                r.TryGetProperty("ultimaAttivitaIl", out var ultima) && ultima.ValueKind != JsonValueKind.Null ? LeggiData(ultima) : null);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException and not RichiestaNonValidaException)
        {
            throw new RichiestaNonValidaException("Il corpo non è un JSON valido.");
        }
    }

    private static readonly System.Text.RegularExpressions.Regex FusoOrario =
        new(@"(Z|[+-]\d{2}:\d{2})$", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private const string MessaggioTempo = "Servono tempoAttivoMs (numero intero non negativo) e ultimaAttivitaIl (data e ora ISO 8601 con fuso orario, non oltre un giorno nel futuro).";

    // La data deve avere il fuso orario e non essere più di un giorno nel futuro; altrimenti 422.
    private static DateTime LeggiData(JsonElement e)
    {
        var testo = e.ValueKind == JsonValueKind.String ? e.GetString() : null;
        if (testo is null || !FusoOrario.IsMatch(testo)
            || !DateTimeOffset.TryParse(testo, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var data)
            || data > DateTimeOffset.UtcNow.AddDays(1))
            throw new RichiestaNonValidaException(MessaggioTempo);

        return data.UtcDateTime;
    }

    private static long LeggiTempoAttivo(JsonElement e) =>
        e.ValueKind == JsonValueKind.Number && e.TryGetInt64(out var ms) && ms >= 0 ? ms : throw new RichiestaNonValidaException(MessaggioTempo);

    private static (long TempoAttivoMs, DateTime UltimaAttivitaIl) LeggiTempo(string corpo)
    {
        try
        {
            using var documento = JsonDocument.Parse(corpo);
            var r = documento.RootElement;
            if (r.ValueKind == JsonValueKind.Object
                && r.TryGetProperty("tempoAttivoMs", out var ms)
                && r.TryGetProperty("ultimaAttivitaIl", out var ultima))
                return (LeggiTempoAttivo(ms), LeggiData(ultima));
        }
        catch (JsonException)
        {
            // Il corpo non è JSON: vale il messaggio sotto.
        }

        throw new RichiestaNonValidaException(MessaggioTempo);
    }

    private static string? Testo(JsonElement e, string nome) =>
        e.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static IResult Json(int stato, string json) => Results.Text(json, "application/json", System.Text.Encoding.UTF8, stato);

    // Gli errori noti diventano risposte chiare; gli altri si registrano e si riportano senza dettagli interni.
    private static async Task<IResult> Esegui(HttpContext http, Func<CancellationToken, Task<IResult>> operazione)
    {
        try
        {
            // L'annullamento della richiesta HTTP (pagina chiusa) non interrompe la generazione: vale solo la chiusura dell'host.
            return await operazione(http.RequestServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);
        }
        catch (RichiestaNonValidaException ex)
        {
            return Json(422, CandidatoJson.Errori(ex.Errori));
        }
        catch (RegistrazioneMancanteException)
        {
            return Json(422, CandidatoJson.Errori([MessaggioRegistrazioneMancante]));
        }
        catch (ConflittoCreazioneException ex)
        {
            return Json(409, CandidatoJson.Errori([ex.Message]));
        }
        catch (NonTrovatoCreazioneException ex)
        {
            return Json(404, CandidatoJson.Errori([ex.Message]));
        }
        catch (LimiteEsecuzioneRaggiuntoException)
        {
            return Json(429, CandidatoJson.Errori(["Limite delle chiamate dell'esecuzione raggiunto."]));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Prototipo").LogError(ex, "Errore nell'API del prototipo");
            return Json(500, CandidatoJson.Errori(["Errore interno del prototipo: i dettagli sono nel registro dell'host."]));
        }
    }
}
