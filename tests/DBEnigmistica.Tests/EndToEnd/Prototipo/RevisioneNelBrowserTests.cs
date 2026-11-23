using System.Text.Json;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Tests.Integration;
using DBEnigmistica.Tests.Integration.Prototipo;
using DBEnigmistica.Web.Composizione;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace DBEnigmistica.Tests.EndToEnd.Prototipo;

/// <summary>
/// Criterio 1 della fase 1b nel browser vero: genera con il modello finto, modifica due campi, accetta.
/// L'orologio del browser è simulato e nessuna chiamata AI parte (Fornitore AI finto).
/// </summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "EndToEnd")]
public sealed class RevisioneNelBrowserTests(PostgresFixture fixture, BrowserFixture browser) : PrototipoTestBase(fixture), IClassFixture<BrowserFixture>
{
    // Vicino all'ora vera: il server rifiuta un'ultima attività oltre un giorno nel futuro.
    private static readonly DateTime Inizio = DateTime.UtcNow.AddHours(-1).Date.AddHours(DateTime.UtcNow.AddHours(-1).Hour);

    private static DescrittoreTipologia Approvato(string id) =>
        Descrittore(id) with { Regole = Descrittore(id).Regole with { Approvata = true, ApprovateIl = "2026-12-01" } };

    private async Task<WebApplication> AvviaHostAsync()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        var configurazione = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Prototipo:Porta"] = "0",
        }).Build();
        var app = PrototipoHost.Crea(
            configurazione,
            servizi =>
            {
                servizi.AddSingleton<IFornitoreAiCreazione>(finto);
                servizi.AddSingleton<ICatalogoTipologie>(new CatalogoInMemoria(Approvato("crittografia-breve")));
            },
            FattoConBrowserAttribute.CartellaInterfaccia);
        await app.StartAsync();
        return app;
    }

    [FattoConBrowser]
    public async Task GeneraModificaDueCampiEAccetta_ConTempoAttivoCoerenteConLOrologioSimulato()
    {
        await using var app = await AvviaHostAsync();
        var indirizzo = PrototipoHost.Indirizzi(app).Single();
        await using var contesto = await browser.Browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = indirizzo });
        var pagina = await contesto.NewPageAsync();
        var richiesteEsterne = new List<string>();
        pagina.Request += (_, r) =>
        {
            if (!r.Url.StartsWith(indirizzo, StringComparison.Ordinal) && !r.Url.StartsWith("data:", StringComparison.Ordinal))
                richiesteEsterne.Add(r.Url);
        };

        // Orologio fermo: il tempo passa solo quando il test lo fa avanzare.
        await contesto.Clock.InstallAsync(new ClockInstallOptions { TimeDate = Inizio });
        await contesto.Clock.PauseAtAsync(Inizio.AddSeconds(1));

        await pagina.GotoAsync("/#/nuovo/crittografia-breve");
        await pagina.GetByLabel("Sottotipo").SelectOptionAsync("sinonimica");
        await pagina.RunAndWaitForResponseAsync(
            () => pagina.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Genera", Exact = true }).ClickAsync(),
            r => r.Url.EndsWith("/api/prototipo/candidati", StringComparison.Ordinal) && r.Request.Method == "POST");

        await pagina.EvaluateAsync("location.hash = '#/revisione/crittografia-breve'");
        var esposto = pagina.GetByLabel("Esposto", new PageGetByLabelOptions { Exact = true });
        await Assertions.Expect(esposto).ToHaveValueAsync("CENA SOVRANO");

        // Istanti di attività: apertura a 0 s, primo campo a 60 s, secondo a 90 s, Accetta a 100 s.
        await contesto.Clock.RunForAsync(60_000);
        await ModificaAsync(pagina, esposto, "PRANZO DEL SOVRANO");
        await contesto.Clock.RunForAsync(30_000);
        await ModificaAsync(pagina, pagina.GetByLabel("Spiegazione", new PageGetByLabelOptions { Exact = true }), "pranzo = pasto; sovrano = re");
        await contesto.Clock.RunForAsync(10_000);
        var accetta = await pagina.RunAndWaitForResponseAsync(
            () => pagina.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accetta", Exact = true }).ClickAsync(),
            r => r.Url.EndsWith("/accetta", StringComparison.Ordinal) && r.Request.Method == "POST",
            new PageRunAndWaitForResponseOptions { Timeout = 60_000 });
        accetta.Status.Should().Be(200, await accetta.TextAsync());
        await Assertions.Expect(pagina.GetByText("Candidato accettato.")).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 30_000 });

        await using var s = Servizi();
        var c = await s.Contesto.Candidati.AsNoTracking().SingleAsync();
        c.Stato.Should().Be(StatoCandidato.Accettato);
        c.TempoAttivoMs.Should().Be(100_000);
        c.UltimaAttivitaIl.Should().Be(Inizio.AddSeconds(101));
        var differenze = JsonDocument.Parse(c.Differenze!).RootElement;
        differenze.EnumerateArray().Select(d => d.GetProperty("percorso"u8).GetString())
            .Should().Equal("/dati/esposto", "/dati/spiegazione");
        differenze[0].GetProperty("dopo"u8).GetString().Should().Be("PRANZO DEL SOVRANO");
        var evento = (await s.Registro.LeggiInOrdineAsync().ToListAsync()).Single(e => e.Tipo == "esito_revisione");
        var dati = JsonDocument.Parse(evento.Dati).RootElement;
        dati.GetProperty("campi_modificati"u8).GetInt32().Should().Be(2);
        dati.GetProperty("tempo_attivo_ms"u8).GetInt64().Should().Be(100_000);
        richiesteEsterne.Should().BeEmpty("le pagine non caricano nulla da fuori (CSP)");
    }

    // Un campo si modifica e si attende il salvataggio sul server prima di far avanzare l'orologio.
    private static Task ModificaAsync(IPage pagina, ILocator campo, string testo) =>
        pagina.RunAndWaitForResponseAsync(
            () => campo.FillAsync(testo),
            r => r.Url.EndsWith("/finale", StringComparison.Ordinal) && r.Request.Method == "PUT");
}
