using System.Net;
using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.Configuration;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Infrastructure.Prototipo;
using DBEnigmistica.Web.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Web.Composizione;

/// <summary>
/// Composizione dell'host del prototipo: è l'unico punto di <c>Web</c> che conosce Infrastruttura e adattatori.
/// Kestrel ascolta solo su 127.0.0.1, fissato qui nel codice e non sovrascrivibile da configurazione, URL o variabili d'ambiente.
/// </summary>
public static class PrototipoHost
{
    public const string SezionePorta = "Prototipo:Porta";
    public const int PortaPredefinita = 5180;

    /// <summary>Costruisce l'applicazione senza avviarla. <paramref name="sostituzioni"/> serve ai test per sostituire servizi.</summary>
    public static WebApplication Crea(
        IConfiguration configurazione,
        Action<IServiceCollection>? sostituzioni = null,
        string? cartellaInterfaccia = null)
    {
        ArgumentNullException.ThrowIfNull(configurazione);
        var porta = int.TryParse(configurazione[SezionePorta], out var p) && p is >= 0 and <= 65535 ? p : PortaPredefinita;

        // Il costruttore vuoto non legge configurazione né URL dell'ambiente: l'indirizzo è solo quello scritto qui.
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.WebHost.UseKestrel(o =>
        {
            o.Listen(IPAddress.Loopback, porta);
            o.AddServerHeader = false;
            o.Limits.MaxRequestBodySize = 1_048_576;
        });
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddSimpleConsole();

        var servizi = builder.Services;
        servizi.AddRoutingCore();
        servizi.AddSingleton<IRegistroChiamateAi, RegistroChiamateAiNullo>();
        servizi.Configure<FornitoreAiOptions>(configurazione.GetSection(FornitoreAiOptions.SectionName));
        servizi.AddSingleton<IAmbienteFornitoreAi, AmbienteFornitoreAi>();
        servizi.AddPrototipoInfrastructure(configurazione);
        servizi.AddAiServices(configurazione);
        servizi.AddCreazione();
        sostituzioni?.Invoke(servizi);

        var app = builder.Build();
        app.Use(SoloLocale);
        app.Use(IntestazioniDiSicurezza);
        var cartella = cartellaInterfaccia ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        if (Directory.Exists(cartella))
            app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(cartella)) });
        app.Use((contesto, successivo) => RispostaInterfaccia(contesto, successivo, cartella));
        app.UseRouting();
        app.MapPrototipo();
        return app;
    }

    /// <summary>Gli indirizzi su cui l'applicazione avviata ascolta.</summary>
    public static IReadOnlyList<string> Indirizzi(WebApplication app) =>
        [.. app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses ?? []];

    // Le pagine non caricano nulla da fuori e non si incorniciano (CSP, nosniff, nessun referrer).
    private static Task IntestazioniDiSicurezza(HttpContext contesto, RequestDelegate successivo)
    {
        var intestazioni = contesto.Response.Headers;
        intestazioni["Content-Security-Policy"] = "default-src 'self'; base-uri 'none'; form-action 'self'; object-src 'none'; frame-ancestors 'none'";
        intestazioni["X-Content-Type-Options"] = "nosniff";
        intestazioni["Referrer-Policy"] = "no-referrer";
        return successivo(contesto);
    }

    // Ritorno a index.html per i percorsi dell'interfaccia (solo GET, non /api, non file con estensione).
    private static async Task RispostaInterfaccia(HttpContext contesto, RequestDelegate successivo, string cartella)
    {
        var percorso = contesto.Request.Path;
        var daInterfaccia = HttpMethods.IsGet(contesto.Request.Method)
            && !percorso.StartsWithSegments("/api")
            && !Path.HasExtension(percorso.Value);
        if (!daInterfaccia)
        {
            await successivo(contesto);
            return;
        }

        var indice = Path.Combine(cartella, "index.html");
        contesto.Response.ContentType = "text/html; charset=utf-8";
        if (File.Exists(indice))
        {
            await contesto.Response.SendFileAsync(indice, contesto.RequestAborted);
            return;
        }

        contesto.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await contesto.Response.WriteAsync(
            "<!doctype html><meta charset=\"utf-8\"><title>Interfaccia non costruita</title>"
            + "<p>L'interfaccia non è stata costruita. Esegui <code>npm run build</code> nella cartella <code>interfaccia</code> e riavvia il prototipo.</p>",
            contesto.RequestAborted);
    }

    // Difesa locale: rebinding DNS (Host), richieste di altri siti (Origin, Sec-Fetch-Site) e POST/PUT "semplici" (Content-Type).
    private static async Task SoloLocale(HttpContext contesto, RequestDelegate successivo)
    {
        var richiesta = contesto.Request;
        if (richiesta.Host.Host is not ("127.0.0.1" or "localhost"))
        {
            contesto.Response.StatusCode = StatusCodes.Status421MisdirectedRequest;
            return;
        }

        // Origini ammesse: l'indirizzo su cui l'host ascolta davvero (con la forma senza porta se è la 80).
        var porta = contesto.Connection.LocalPort;
        var consentiti = new List<string> { $"http://127.0.0.1:{porta}", $"http://localhost:{porta}" };
        if (porta == 80)
            consentiti.AddRange(["http://127.0.0.1", "http://localhost"]);
        var origine = richiesta.Headers.Origin.ToString();
        if ((origine.Length > 0 && !consentiti.Contains(origine, StringComparer.OrdinalIgnoreCase))
            || string.Equals(richiesta.Headers["Sec-Fetch-Site"].ToString(), "cross-site", StringComparison.OrdinalIgnoreCase))
        {
            contesto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        // Un corpo (anche a pezzi) deve essere JSON; le POST senza corpo (prossimo, accetta, scarta) non portano Content-Type.
        var haCorpo = richiesta.ContentLength is > 0 || richiesta.Headers.ContainsKey("Transfer-Encoding");
        if (haCorpo && (HttpMethods.IsPost(richiesta.Method) || HttpMethods.IsPut(richiesta.Method))
            && !(richiesta.ContentType ?? string.Empty).StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
        {
            contesto.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            return;
        }

        await successivo(contesto);
    }
}

/// <summary>Registro delle chiamate che non conserva nulla in memoria: le righe vanno nello schema <c>prototipo</c> con il candidato.</summary>
public sealed class RegistroChiamateAiNullo : IRegistroChiamateAi
{
    public Task RegistraAsync(ChiamataAi chiamata, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
