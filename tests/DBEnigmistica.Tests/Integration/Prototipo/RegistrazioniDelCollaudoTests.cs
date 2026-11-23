using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Tests.Unit.Creazione;
using DBEnigmistica.Web.Composizione;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using static DBEnigmistica.Tests.Integration.Prototipo.StrumentoRegistrazioniCollaudo;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>
/// Il collaudo delle famiglie con AI come lo fa la schermata: l'host nasce dalla sola configurazione in riproduzione
/// e riceve per HTTP i brief tali e quali li invia la schermata. Le Registrazioni sono sintetiche (testi inventati, nessuna chiamata).
/// </summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public sealed class RegistrazioniDelCollaudoTests(PostgresFixture fixture) : PrototipoTestBase(fixture)
{
    private async Task<(Microsoft.AspNetCore.Builder.WebApplication App, HttpClient Client)> AvviaAsync()
    {
        var configurazione = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Prototipo:Porta"] = "0",
            ["Prototipo:CartellaTipologie"] = FileDelRepository.Percorso("tipologie"),
            ["FornitoreAi:Tipo"] = "claude-code",
            ["FornitoreAi:Creazione:Modello"] = ModelloAi,
            ["FornitoreAi:Creazione:Registrazioni:Modalita"] = "riproduci",
            ["FornitoreAi:Creazione:Registrazioni:Cartella"] = CartellaFixture,
            ["FornitoreAi:DalVivoAbilitato"] = "false",
        }).Build();
        var app = PrototipoHost.Crea(configurazione);
        await app.StartAsync();
        return (app, new HttpClient { BaseAddress = new Uri(PrototipoHost.Indirizzi(app).Single()) });
    }

    [Fact]
    public async Task OgniBriefDellaSchermataSiRiproduceInUnCandidatoValidoSenzaControlli()
    {
        var (app, client) = await AvviaAsync();
        await using var _ = app;
        using var __ = client;

        foreach (var codice in Codici)
        {
            using var corpo = new StringContent(Brief(codice), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            var risposta = await client.PostAsync("/api/prototipo/candidati", corpo);

            var testo = await risposta.Content.ReadAsStringAsync();
            risposta.StatusCode.Should().Be(HttpStatusCode.OK, $"{codice}: {testo}");
            var radice = JsonDocument.Parse(testo).RootElement;
            radice.GetProperty("valido"u8).ValueKind.Should().Be(JsonValueKind.String, $"la Registrazione di {codice} deve produrre un candidato valido: {testo}");
            var candidato = radice.GetProperty("candidati"u8).EnumerateArray().Last();
            candidato.GetProperty("stato"u8).GetString().Should().Be(StatoCandidato.DaRivedere, codice);
            candidato.GetProperty("controlli"u8).GetArrayLength().Should().Be(0, codice);
        }
    }

    [Fact]
    public void IBriefDelCollaudoSonoValidiPerLoSchemaDelBrief()
    {
        var validatore = new ValidatoreSchemaJson();

        foreach (var codice in Codici)
            validatore.Valida(Brief(codice), RisorseCreazione.SchemaBrief).Should().BeEmpty(codice);
    }

    [Fact]
    public void IlSemeDiRiproduzioneNonDipendeDalCodiceDelBrief()
    {
        var senza = BriefCreazione.Leggi(Brief("cruci-collaudo")).ConVariante(1);
        var con = senza with { Codice = "cruci-01" };

        con.SemeDiRiproduzione().Should().Be(senza.SemeDiRiproduzione());
    }
}
