using DBEnigmistica.Application.Registro;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Domain.Registro;
using DBEnigmistica.Infrastructure.Repositories;
using DBEnigmistica.Tests.Integration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DBEnigmistica.Tests.EndToEnd;

/// <summary>La CLI vera, dall'ingresso in processo, contro PostgreSQL 18 (criteri 5 e 9 della fase 0).</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "EndToEnd")]
public class CliEndToEndTests : IntegrationTestBase
{
    public CliEndToEndTests(PostgresFixture fixture) : base(fixture)
    {
    }

    // La stringa di connessione passa dalla configurazione in memoria, non dall'ambiente condiviso dal processo.
    private Task<(int Code, string Output, string Error)> CliAsync(params string[] args) =>
        CliConAsync(args, []);

    private async Task<(int Code, string Output, string Error)> CliConAsync(
        string[] args, params (string Chiave, string Valore)[] impostazioni)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var coppie = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
        };
        foreach (var (chiave, valore) in impostazioni)
            coppie[chiave] = valore;

        var code = await CliApp.RunAsync(args, output, error, builder => builder.AddInMemoryCollection(coppie));
        return (code, output.ToString(), error.ToString());
    }

    private async Task<long> AggiungiEventoAsync(string entitaId)
    {
        await using var context = NewContext();
        var evento = new NuovoEvento(DateTime.UtcNow, "gioco_creato", "gioco_originale", entitaId, "Alessio", "{\"k\":1}");
        return (await new RegistroEventiRepository(context).AggiungiAsync(evento)).Id;
    }

    [Fact]
    public async Task Help_ListsTheCommandsIncludingVerifyIntegrity()
    {
        var result = await CliAsync("--help");

        result.Code.Should().Be(0);
        result.Output.Should().Contain("verify-integrity").And.Contain("list-games").And.Contain("migrate");
    }

    [Fact]
    public async Task Migrate_then_ListGames_on_an_empty_database_succeed_without_container_errors()
    {
        var migrate = await CliAsync("migrate", "--yes");
        var list = await CliAsync("list-games");

        migrate.Code.Should().Be(0, migrate.Error);
        migrate.Output.Should().Contain("Nessuna migrazione da applicare");
        list.Code.Should().Be(0, list.Error);
        (migrate.Output + migrate.Error + list.Output + list.Error).Should().NotContain("Service provider not available");
    }

    [Fact]
    public async Task VerifyIntegrity_on_an_empty_registry_is_intact()
    {
        var result = await CliAsync("verify-integrity");

        result.Code.Should().Be(0, result.Error);
        result.Output.Should().Contain("0 eventi verificati");
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public async Task VerifyIntegrity_on_a_registry_with_events_is_intact()
    {
        await AggiungiEventoAsync("1");
        await AggiungiEventoAsync("2");
        await AggiungiEventoAsync("3");

        var result = await CliAsync("verify-integrity");

        result.Code.Should().Be(0, result.Error);
        result.Output.Should().Contain("3 eventi verificati");
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public async Task VerifyIntegrity_after_a_superuser_alteration_fails_and_names_the_altered_id()
    {
        await AggiungiEventoAsync("1");
        var alterato = await AggiungiEventoAsync("2");
        await AggiungiEventoAsync("3");
        await AlteraAsync(alterato);

        var result = await CliAsync("verify-integrity");

        result.Code.Should().Be(5);
        result.Code.Should().Be(ExitCodes.RegistryTampered);
        result.Error.Should().Contain($"id {alterato}").And.Contain(VerificaCatena.MotivoRicalcolata);
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public async Task VerifyIntegrity_with_an_unreachable_database_exits_with_one_not_five()
    {
        var result = await CliConAsync(
            ["verify-integrity"],
            ("ConnectionStrings:DefaultConnection",
                "Host=127.0.0.1;Port=1;Database=x;Username=x;Password=x;Timeout=3;Pooling=false"));

        result.Code.Should().Be(1, result.Error);
        result.Code.Should().Be(ExitCodes.Error);
        result.Error.Should().Contain("Database non raggiungibile").And.NotContain("same key");
    }

    [Fact]
    public async Task Command_not_yet_available_exits_with_two_and_names_the_phase()
    {
        var result = await CliAsync("scan-file", "x.pdf");

        result.Code.Should().Be(2);
        result.Error.Should().Contain("Comando disponibile dalla fase 1");
    }

    [Fact]
    public async Task Commands_without_AI_start_with_no_AI_provider_configuration()
    {
        foreach (var args in new[] { new[] { "list-games" }, ["verify-integrity"] })
        {
            var result = await CliConAsync(
                args,
                ("FornitoreAi:Tipo", ""),
                ("FornitoreAi:Modello", ""),
                ("FornitoreAi:ClaudeCode:Eseguibile", ""));
            result.Code.Should().Be(0, $"{args[0]}: {result.Error}");
            result.Error.Should().NotContain("Fornitore AI");
        }
    }

    private async Task AlteraAsync(long id)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SET session_replication_role = replica; UPDATE registro.eventi SET dati = '{\"k\":999}' WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        (await command.ExecuteNonQueryAsync()).Should().Be(1);
    }
}
