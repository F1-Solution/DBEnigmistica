using DBEnigmistica.Console.Cli;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>Il comando <c>dbe prototipo</c> avvia l'host su 127.0.0.1 e termina con Ctrl+C; senza configurazione esce con il codice 3.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "EndToEnd")]
public class PrototipoEndToEndTests(PostgresFixture fixture) : IntegrationTestBase(fixture)
{
    private Dictionary<string, string?> Configurazione(string cartellaRegistrazioni) => new()
    {
        ["ConnectionStrings:DefaultConnection"] = ConnectionString,
        ["Prototipo:Porta"] = "0",
        ["Prototipo:CartellaTipologie"] = FileDelRepository.Percorso("tipologie"),
        ["FornitoreAi:Tipo"] = "claude-code",
        ["FornitoreAi:Creazione:Modello"] = "claude-opus-5-5",
        ["FornitoreAi:Creazione:Registrazioni:Modalita"] = "riproduci",
        ["FornitoreAi:Creazione:Registrazioni:Cartella"] = cartellaRegistrazioni,
    };

    [Fact]
    public async Task DbePrototipo_AvviaLHostSu127001ETerminaConCtrlC()
    {
        var uscita = new StringWriter();
        var errori = new StringWriter();
        using var cts = new CancellationTokenSource();
        var configurazione = Configurazione(Path.Combine(Path.GetTempPath(), "dbe-reg-e2e-" + Guid.NewGuid().ToString("N")));
        var comando = Task.Run(() => CliApp.RunAsync(["prototipo"], uscita, errori, b => b.AddInMemoryCollection(configurazione), cts.Token));

        string? indirizzo = null;
        for (var i = 0; i < 300 && indirizzo is null && !comando.IsCompleted; i++)
        {
            await Task.Delay(100);
            indirizzo = uscita.ToString().Split('\n').Select(r => r.Trim()).FirstOrDefault(r => r.StartsWith("Prototipo in ascolto su ", StringComparison.Ordinal))
                ?.Split(' ')[4];
        }

        indirizzo.Should().NotBeNull($"l'host deve partire: {errori}");
        indirizzo.Should().StartWith("http://127.0.0.1:");
        using (var client = new HttpClient())
            (await client.GetAsync(indirizzo + "/api/prototipo/salute")).IsSuccessStatusCode.Should().BeTrue();

        await cts.CancelAsync();
        (await comando.WaitAsync(TimeSpan.FromSeconds(30))).Should().Be(ExitCodes.Success, errori.ToString());
    }

    [Fact]
    public async Task DbePrototipo_SenzaConfigurazione_EsceConCodice3()
    {
        var errori = new StringWriter();
        var configurazione = Configurazione("x");
        configurazione["ConnectionStrings:DefaultConnection"] = string.Empty;

        var codice = await CliApp.RunAsync(["prototipo"], new StringWriter(), errori, b => b.AddInMemoryCollection(configurazione));

        codice.Should().Be(ExitCodes.Configuration, errori.ToString());
    }

    [Fact]
    public async Task DbePrototipo_SenzaModelloDiCreazione_EsceConCodice3ENominaLaChiave()
    {
        var errori = new StringWriter();
        var configurazione = Configurazione("x");
        configurazione["FornitoreAi:Creazione:Modello"] = string.Empty;

        var codice = await CliApp.RunAsync(["prototipo"], new StringWriter(), errori, b => b.AddInMemoryCollection(configurazione));

        codice.Should().Be(ExitCodes.Configuration);
        errori.ToString().Should().Contain("FornitoreAi__Creazione__Modello");
    }

    [Fact]
    public async Task DbePrototipo_ConUnLoginCheNonPuoFareSetRole_EsceConCodice3ConMessaggioChiaro()
    {
        var login = "dbe_login_" + Guid.NewGuid().ToString("N")[..10];
        await ExecuteAsync($"CREATE ROLE {login} LOGIN PASSWORD 'pw'");
        try
        {
            var errori = new StringWriter();
            var configurazione = Configurazione("x");
            configurazione["ConnectionStrings:DefaultConnection"] =
                new Npgsql.NpgsqlConnectionStringBuilder(ConnectionString) { Username = login, Password = "pw" }.ConnectionString;

            var codice = await CliApp.RunAsync(["prototipo"], new StringWriter(), errori, b => b.AddInMemoryCollection(configurazione));

            codice.Should().Be(ExitCodes.Configuration, errori.ToString());
            errori.ToString().Should().Contain("SET ROLE dbe_creazione").And.Contain("superutente");
        }
        finally
        {
            Npgsql.NpgsqlConnection.ClearAllPools();
            await ExecuteAsync($"DROP ROLE IF EXISTS {login}");
        }
    }

    [Fact]
    public async Task DbePrototipo_SenzaIlPrototipoNellaPubblicazione_RispondeNonIncluso_ConCodice2_AncheSenzaConfigurazione()
    {
        var errori = new StringWriter();
        var gestore = new DBEnigmistica.Console.Commands.PrototipoHandler(
            new ConfigurationBuilder().Build(), new CliStreams(new StringWriter(), errori), "DBEnigmistica.Web.Inesistente, DBEnigmistica.Web");

        var codice = await gestore.ExecuteAsync(CancellationToken.None);

        codice.Should().Be(ExitCodes.NotAvailable);
        errori.ToString().Should().Contain("win-x64").And.Contain("linux-x64").And.Contain("non è incluso");
    }
}
