using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Tests.Unit.Creazione;
using DBEnigmistica.Web.Composizione;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>Le API dell'host del prototipo, su Kestrel vero legato a 127.0.0.1, con Fornitore AI finto e PostgreSQL.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class ApiPrototipoTests(PostgresFixture fixture) : PrototipoTestBase(fixture)
{
    private sealed record Host(WebApplication App, HttpClient Client, FornitoreCreazioneFinto Finto) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.StopAsync();
            await App.DisposeAsync();
        }
    }

    // Le regole del descrittore di prova sono approvate, come lo saranno quelle vere prima di ogni generazione dal vivo.
    private static DescrittoreTipologia Approvato(string id) =>
        Descrittore(id) with { Regole = Descrittore(id).Regole with { Approvata = true, ApprovateIl = "2026-12-01" } };

    private async Task<Host> AvviaAsync(Action<FornitoreCreazioneFinto> prepara)
    {
        var finto = new FornitoreCreazioneFinto();
        prepara(finto);
        var configurazione = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Prototipo:Porta"] = "0",
        }).Build();
        var app = PrototipoHost.Crea(configurazione, servizi =>
        {
            servizi.AddSingleton<IFornitoreAiCreazione>(finto);
            servizi.AddSingleton<ICatalogoTipologie>(new CatalogoInMemoria(Approvato("crittografia-breve"), Approvato("sudoku")));
        });
        await app.StartAsync();
        var client = new HttpClient { BaseAddress = new Uri(PrototipoHost.Indirizzi(app).Single()) };
        return new Host(app, client, finto);
    }

    private static StringContent Corpo(string json) => new(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

    private static async Task<JsonElement> LeggiAsync(HttpResponseMessage risposta) =>
        JsonDocument.Parse(await risposta.Content.ReadAsStringAsync()).RootElement.Clone();

    [Fact]
    public async Task Host_AscoltaSoloSu127001()
    {
        await using var host = await AvviaAsync(_ => { });

        PrototipoHost.Indirizzi(host.App).Should().OnlyContain(a => a.StartsWith("http://127.0.0.1:", StringComparison.Ordinal));
        (await host.Client.GetAsync("/api/prototipo/salute")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Host_NonSiLasciaCambiareIndirizzoDaConfigurazioneOAmbiente()
    {
        var anteriore = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://0.0.0.0:5999");
        try
        {
            var configurazione = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Prototipo:Porta"] = "0",
                ["urls"] = "http://0.0.0.0:5998",
                ["Kestrel:Endpoints:Web:Url"] = "http://0.0.0.0:5997",
            }).Build();
            await using var app = PrototipoHost.Crea(configurazione);
            await app.StartAsync();

            PrototipoHost.Indirizzi(app).Should().OnlyContain(a => a.StartsWith("http://127.0.0.1:", StringComparison.Ordinal));
            await app.StopAsync();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", anteriore);
        }
    }

    [Fact]
    public async Task Host_RifiutaRichiesteConUnHostDiverso()
    {
        await using var host = await AvviaAsync(_ => { });
        using var richiesta = new HttpRequestMessage(HttpMethod.Get, "/api/prototipo/salute");
        richiesta.Headers.Host = "esempio.invalid";

        var risposta = await host.Client.SendAsync(richiesta);

        risposta.StatusCode.Should().Be(HttpStatusCode.MisdirectedRequest);
    }

    [Fact]
    public async Task Api_GeneraModificaAccettaEScarta()
    {
        await using var host = await AvviaAsync(f => f.Riuscita(RispostaCrittografia()).Riuscita(RispostaCrittografia()));

        var genera1 = await LeggiAsync(await host.Client.PostAsync("/api/prototipo/candidati", Corpo(BriefCrittografia())));
        var genera2 = await LeggiAsync(await host.Client.PostAsync("/api/prototipo/candidati", Corpo(BriefCrittografia())));
        var id1 = genera1.GetProperty("valido"u8).GetGuid();
        var id2 = genera2.GetProperty("valido"u8).GetGuid();

        var prossimo = await LeggiAsync(await host.Client.PostAsync("/api/prototipo/tipologie/crittografia-breve/prossimo", Corpo("{}")));
        prossimo.GetProperty("id"u8).GetGuid().Should().Be(id1);
        prossimo.GetProperty("stato"u8).GetString().Should().Be("in_revisione");

        var saltato = await host.Client.PostAsync($"/api/prototipo/candidati/{id2}/accetta", Corpo("{}"));
        saltato.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var modificato = await host.Client.PutAsync(
            $"/api/prototipo/candidati/{id1}/finale", Corpo(RispostaCrittografia("PRANZO DEL SOVRANO", "pranzo = pasto")));
        modificato.StatusCode.Should().Be(HttpStatusCode.OK);
        (await LeggiAsync(modificato)).GetProperty("differenze"u8).GetArrayLength().Should().Be(2);

        var accettato = await LeggiAsync(await host.Client.PostAsync($"/api/prototipo/candidati/{id1}/accetta", Corpo("{\"tempoAttivoMs\":5000}")));
        accettato.GetProperty("stato"u8).GetString().Should().Be("accettato");

        var senzaMotivo = await host.Client.PostAsync($"/api/prototipo/candidati/{id2}/scarta", Corpo("{}"));
        senzaMotivo.StatusCode.Should().Be((HttpStatusCode)422);
        var scartato = await LeggiAsync(await host.Client.PostAsync($"/api/prototipo/candidati/{id2}/scarta", Corpo("{\"motivo\":\"qualita_insufficiente\",\"nota\":\"banale\"}")));
        scartato.GetProperty("stato"u8).GetString().Should().Be("scartato");

        (await host.Client.PostAsync("/api/prototipo/tipologie/crittografia-breve/prossimo", Corpo("{}"))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await VerificaRegistroAsync()).Integra.Should().BeTrue();
    }

    [Fact]
    public async Task Api_ErroriNoti_DiventanoCodiciHttp()
    {
        await using var host = await AvviaAsync(_ => { });

        (await host.Client.PostAsync("/api/prototipo/candidati", Corpo("{}"))).StatusCode.Should().Be((HttpStatusCode)422);
        (await host.Client.PostAsync("/api/prototipo/candidati", Corpo(string.Empty))).StatusCode.Should().Be((HttpStatusCode)422);
        (await host.Client.PostAsync("/api/prototipo/candidati", Corpo("non json"))).StatusCode.Should().Be((HttpStatusCode)422);
        (await host.Client.PostAsync($"/api/prototipo/candidati/{Guid.NewGuid()}/accetta", Corpo("{}"))).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await host.Client.PostAsync("/api/prototipo/candidati", Corpo(FileDelRepository.Esempio("brief", "valido-cruciverba-11x11.json")))).StatusCode
            .Should().Be(HttpStatusCode.NotFound, "la Tipologia non è nel catalogo di prova");
    }

    [Fact]
    public async Task Api_UnErroreImprevisto_NonEsponeDettagliInterni()
    {
        await using var host = await AvviaAsync(_ => { });

        // Il fornitore finto non ha risposte preparate: l'eccezione interna non deve arrivare al client.
        var risposta = await host.Client.PostAsync("/api/prototipo/candidati", Corpo(BriefCrittografia()));

        risposta.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await risposta.Content.ReadAsStringAsync()).Should().NotContain("Queue").And.NotContain("at DBEnigmistica");
    }

    [Fact]
    public async Task Api_CancellaLeRisposteGrezze()
    {
        await using var host = await AvviaAsync(f => f.Riuscita(RispostaCrittografia()));
        await host.Client.PostAsync("/api/prototipo/candidati", Corpo(BriefCrittografia()));

        var risposta = await LeggiAsync(await host.Client.DeleteAsync("/api/prototipo/risposte-grezze"));

        risposta.GetProperty("cancellate"u8).GetInt32().Should().Be(1);
        (await ScalarAsync<long>("SELECT count(*) FROM prototipo.chiamate_ai WHERE risposta_grezza IS NOT NULL")).Should().Be(0);
        (await ScalarAsync<long>("SELECT count(*) FROM prototipo.chiamate_ai WHERE risposta_sha256 IS NOT NULL")).Should().Be(1);
    }

    [Fact]
    public async Task Api_Sudoku_GenerazioneAlgoritmicaSenzaFornitoreAi()
    {
        await using var host = await AvviaAsync(_ => { });

        var risposta = await LeggiAsync(await host.Client.PostAsync("/api/prototipo/candidati", Corpo(FileDelRepository.Esempio("brief", "valido-sudoku.json"))));

        risposta.GetProperty("valido"u8).ValueKind.Should().Be(JsonValueKind.String);
        host.Finto.Richieste.Should().BeEmpty();
    }

    [Fact]
    public async Task Csrf_OriginEsternoESecFetchSiteCrossSite_Rifiutati403()
    {
        await using var host = await AvviaAsync(_ => { });
        var porta = host.Client.BaseAddress!.Port;

        async Task<HttpStatusCode> Invia(string? origin, string? fetchSite, string percorso = "/api/prototipo/risposte-grezze-inesistente")
        {
            using var richiesta = new HttpRequestMessage(HttpMethod.Post, percorso) { Content = Corpo("{}") };
            if (origin is not null)
                richiesta.Headers.Add("Origin", origin);
            if (fetchSite is not null)
                richiesta.Headers.Add("Sec-Fetch-Site", fetchSite);
            return (await host.Client.SendAsync(richiesta)).StatusCode;
        }

        (await Invia("http://evil.example", null)).Should().Be(HttpStatusCode.Forbidden);
        (await Invia($"http://127.0.0.1:{porta + 1}", null)).Should().Be(HttpStatusCode.Forbidden);
        (await Invia(null, "cross-site")).Should().Be(HttpStatusCode.Forbidden);
        (await Invia($"http://127.0.0.1:{porta}", "same-origin", "/api/prototipo/tipologie/crittografia-breve/prossimo")).Should().Be(HttpStatusCode.NoContent);
        (await Invia($"http://localhost:{porta}", null, "/api/prototipo/tipologie/crittografia-breve/prossimo")).Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Csrf_PostEPutSenzaContentTypeJson_Rifiutati415()
    {
        await using var host = await AvviaAsync(_ => { });
        using var testo = new StringContent(BriefCrittografia(), Encoding.UTF8, "text/plain");
        using var testoPut = new StringContent("{}", Encoding.UTF8, "text/plain");

        (await host.Client.PostAsync("/api/prototipo/candidati", testo)).StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        (await host.Client.PutAsync($"/api/prototipo/candidati/{Guid.NewGuid()}/finale", testoPut)).StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        (await host.Client.GetAsync("/api/prototipo/tipologie/crittografia-breve/prossimo")).StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Csrf_PostSenzaCorpo_SenzaContentType_Passa_MaConOriginEsternoSempre403()
    {
        await using var host = await AvviaAsync(_ => { });
        var porta = host.Client.BaseAddress!.Port;
        const string percorso = "/api/prototipo/tipologie/crittografia-breve/prossimo";

        (await host.Client.PostAsync(percorso, null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var esterna = new HttpRequestMessage(HttpMethod.Post, percorso);
        esterna.Headers.Add("Origin", "http://evil.example");
        (await host.Client.SendAsync(esterna)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var altraPorta = new HttpRequestMessage(HttpMethod.Post, percorso);
        altraPorta.Headers.Add("Origin", $"http://127.0.0.1:{porta + 1}");
        (await host.Client.SendAsync(altraPorta)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var siteCross = new HttpRequestMessage(HttpMethod.Post, percorso);
        siteCross.Headers.Add("Sec-Fetch-Site", "cross-site");
        (await host.Client.SendAsync(siteCross)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Api_IControlliDelCruciverbaStannoInUnArrayAParte_EIlContenutoNonCambia()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-cruciverba.v1.json"));
        descrittore = descrittore with { Regole = descrittore.Regole with { Approvata = true, ApprovateIl = "2026-12-01" } };
        var caselle = SchemaCaselle.Leggi(FileDelRepository.Esempio("schema-cruciverba", "valido-9x9.json"));
        var nodo = System.Text.Json.Nodes.JsonNode.Parse(FileDelRepository.Esempio("risposta", "valido-esempio-cruciverba-9x9.json"))!;
        var avvisiOriginali = nodo["avvisi"]!.AsArray().Count;
        var riga = nodo["dati"]!["griglia"]!["celle"]![1]!.GetValue<string>();
        nodo["dati"]!["griglia"]!["celle"]![1] = (riga[0] == 'Z' ? 'Y' : 'Z') + riga[1..];
        var brief = JsonDocument.Parse(FileDelRepository.Esempio("prompt", "esempio-cruciverba-9x9.atteso.json")).RootElement.GetProperty("brief"u8).GetRawText();
        var finto = new FornitoreCreazioneFinto().Riuscita(nodo.ToJsonString());
        var configurazione = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Prototipo:Porta"] = "0",
        }).Build();
        await using var app = PrototipoHost.Crea(configurazione, servizi =>
        {
            servizi.AddSingleton<IFornitoreAiCreazione>(finto);
            servizi.AddSingleton<ICatalogoTipologie>(new CatalogoInMemoria(descrittore));
            servizi.AddSingleton<IGeneratoriAlgoritmici>(new GeneratoriFinti(caselle));
        });
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(PrototipoHost.Indirizzi(app).Single()) };

        var risposta = await LeggiAsync(await client.PostAsync("/api/prototipo/candidati", Corpo(brief)));

        var candidato = risposta.GetProperty("candidati"u8)[0];
        candidato.GetProperty("stato"u8).GetString().Should().Be("da_rivedere");
        candidato.GetProperty("controlli"u8).EnumerateArray().Select(c => c.GetString()).Should().Contain(c => c!.Contains("non coincidono"));
        candidato.GetProperty("contenutoGenerato"u8).GetProperty("avvisi"u8).GetArrayLength().Should().Be(avvisiOriginali);
    }
}
