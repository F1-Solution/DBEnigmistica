using System.Net;
using System.Text;
using System.Net.Http.Headers;
using DBEnigmistica.Web.Composizione;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>L'host del prototipo serve l'interfaccia costruita, con intestazioni di sicurezza e senza allentare il filtro locale.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class HostInterfacciaTests(PostgresFixture fixture) : PrototipoTestBase(fixture)
{
    private const string PaginaProva = "<!doctype html><title>Prova</title><div id=\"radice\"></div>";

    private sealed record Host(WebApplication App, HttpClient Client, string Cartella) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.StopAsync();
            await App.DisposeAsync();
            if (Directory.Exists(Cartella))
                Directory.Delete(Cartella, true);
        }
    }

    private async Task<Host> AvviaAsync(bool conInterfaccia = true)
    {
        var cartella = Path.Combine(Path.GetTempPath(), "dbe-ui-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(cartella);
        if (conInterfaccia)
        {
            await File.WriteAllTextAsync(Path.Combine(cartella, "index.html"), PaginaProva);
            Directory.CreateDirectory(Path.Combine(cartella, "assets"));
            await File.WriteAllTextAsync(Path.Combine(cartella, "assets", "app.js"), "console.log(1);");
        }

        var configurazione = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Prototipo:Porta"] = "0",
        }).Build();
        var app = PrototipoHost.Crea(configurazione, cartellaInterfaccia: cartella);
        await app.StartAsync();
        return new Host(app, new HttpClient { BaseAddress = new Uri(PrototipoHost.Indirizzi(app).Single()) }, cartella);
    }

    [Fact]
    public async Task Radice_ServeIndexHtml()
    {
        await using var host = await AvviaAsync();

        var risposta = await host.Client.GetAsync("/");

        risposta.StatusCode.Should().Be(HttpStatusCode.OK);
        risposta.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
        (await risposta.Content.ReadAsStringAsync()).Should().Be(PaginaProva);
    }

    [Fact]
    public async Task PercorsoDellInterfaccia_TornaAIndexHtml()
    {
        await using var host = await AvviaAsync();

        var risposta = await host.Client.GetAsync("/revisione/crittografia-breve");

        risposta.StatusCode.Should().Be(HttpStatusCode.OK);
        (await risposta.Content.ReadAsStringAsync()).Should().Be(PaginaProva);
    }

    [Fact]
    public async Task FileStatico_SiServeConIlSuoTipo()
    {
        await using var host = await AvviaAsync();

        var risposta = await host.Client.GetAsync("/assets/app.js");

        risposta.StatusCode.Should().Be(HttpStatusCode.OK);
        risposta.Content.Headers.ContentType!.MediaType.Should().Contain("javascript");
    }

    [Fact]
    public async Task FileStaticoMancante_DaIndexHtmlSoloSeNonHaEstensione_Altrimenti404()
    {
        await using var host = await AvviaAsync();

        (await host.Client.GetAsync("/assets/mancante.js")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiSconosciuta_DaNotFoundENonIndexHtml()
    {
        await using var host = await AvviaAsync();

        var risposta = await host.Client.GetAsync("/api/prototipo/inesistente");

        risposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await risposta.Content.ReadAsStringAsync()).Should().NotContain("<title>");
        (await host.Client.GetAsync("/api/altro")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Risposte_PortanoLeIntestazioniDiSicurezza()
    {
        await using var host = await AvviaAsync();

        foreach (var percorso in new[] { "/", "/assets/app.js", "/api/prototipo/salute", "/api/prototipo/inesistente" })
        {
            var risposta = await host.Client.GetAsync(percorso);
            risposta.Headers.GetValues("Content-Security-Policy").Should().ContainSingle()
                .Which.Should().Be("default-src 'self'; base-uri 'none'; form-action 'self'; object-src 'none'; frame-ancestors 'none'", percorso);
            risposta.Headers.GetValues("X-Content-Type-Options").Should().ContainSingle().Which.Should().Be("nosniff");
            risposta.Headers.GetValues("Referrer-Policy").Should().ContainSingle().Which.Should().Be("no-referrer");
        }
    }

    [Fact]
    public async Task SenzaInterfaccia_LaRadiceDiceDiEseguireNpmRunBuild()
    {
        await using var host = await AvviaAsync(conInterfaccia: false);

        var risposta = await host.Client.GetAsync("/");

        risposta.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await risposta.Content.ReadAsStringAsync()).Should().Contain("npm run build");
        (await host.Client.GetAsync("/api/prototipo/salute")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FiltroLocale_ResistePerLeRichiesteDellInterfaccia()
    {
        await using var host = await AvviaAsync();

        using var origineEstranea = new HttpRequestMessage(HttpMethod.Get, "/");
        origineEstranea.Headers.Add("Origin", "http://esempio.invalid");
        (await host.Client.SendAsync(origineEstranea)).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using var altroHost = new HttpRequestMessage(HttpMethod.Get, "/");
        altroHost.Headers.Host = "esempio.invalid";
        (await host.Client.SendAsync(altroHost)).StatusCode.Should().Be(HttpStatusCode.MisdirectedRequest);

        using var cross = new HttpRequestMessage(HttpMethod.Put, "/api/prototipo/candidati/" + Guid.NewGuid() + "/tempo")
        {
            Content = new StringContent("{}", Encoding.UTF8, new MediaTypeHeaderValue("application/json")),
        };
        cross.Headers.Add("Sec-Fetch-Site", "cross-site");
        (await host.Client.SendAsync(cross)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MetodiDiversiDaGetSuPercorsoDellInterfaccia_NonDannoIndexHtml()
    {
        await using var host = await AvviaAsync();

        var risposta = await host.Client.PostAsync("/revisione/x", new StringContent("{}", Encoding.UTF8, new MediaTypeHeaderValue("application/json")));

        risposta.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }
}
