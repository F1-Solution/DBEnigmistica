using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Tests.Unit.Creazione;
using DBEnigmistica.Web.Composizione;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>D8 e D1b.2-3 su HTTP: l'host nasce dalla sola configurazione con l'interruttore spento, senza servizi sostituiti.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public sealed class InterruttoreSpentoHostTests(PostgresFixture fixture) : PrototipoTestBase(fixture)
{
    private static StringContent Corpo(string json) => new(json, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

    [Fact]
    public async Task ConInterruttoreSpento_FamigliaConAi_Da422ConIlMessaggioDiD8_ESudokuDa200()
    {
        var configurazione = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Prototipo:Porta"] = "0",
            ["Prototipo:CartellaTipologie"] = FileDelRepository.Percorso("tipologie"),
            ["FornitoreAi:DalVivoAbilitato"] = "false",
        }).Build();
        await using var app = PrototipoHost.Crea(configurazione);
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(PrototipoHost.Indirizzi(app).Single()) };

        var ai = await client.PostAsync("/api/prototipo/candidati", Corpo(BriefCrittografia()));
        var sudoku = await client.PostAsync("/api/prototipo/candidati", Corpo(FileDelRepository.Esempio("brief", "valido-sudoku.json")));

        ai.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        JsonDocument.Parse(await ai.Content.ReadAsStringAsync()).RootElement.GetProperty("errori"u8).EnumerateArray()
            .Select(e => e.GetString()).Should().Contain(FornitoreAiOptions.MessaggioDalVivoDisattivato);
        sudoku.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
