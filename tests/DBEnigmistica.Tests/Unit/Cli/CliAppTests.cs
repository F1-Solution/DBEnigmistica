using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Tests.Unit.Cli;

public class CliAppTests
{
    private static readonly string[] AllCommands =
    [
        "scan-folder", "scan-file", "analyze-game", "analyze-game-text", "list-games", "export-schema",
        "export-games", "export-stats", "migrate", "link-solution", "list-unsolved", "verify-solution",
        "scan-solutions", "export-solutions", "check-ai", "verify-integrity"
    ];

    public static TheoryData<string[], int> DisabledCommands => new()
    {
        { ["scan-file", "x.pdf"], 1 },
        { ["scan-folder", "cartella"], 1 },
        { ["analyze-game", "gioco.md"], 4 },
        { ["analyze-game-text", "descrizione"], 4 },
        { ["scan-solutions", "--page-id", "1"], 4 },
    };

    public static TheoryData<string, int> DisabledCommandsWithoutArguments => new()
    {
        { "scan-file", 1 },
        { "scan-folder", 1 },
        { "analyze-game", 4 },
        { "analyze-game-text", 4 },
        { "scan-solutions", 4 },
    };

    public static TheoryData<string[]> DatabaseCommands => new()
    {
        { ["list-games"] },
        { ["migrate", "--yes"] },
        { ["export-games"] },
        { ["export-schema"] },
        { ["export-stats"] },
        { ["link-solution", "--game-id", "1", "--solution", "abc"] },
        { ["verify-solution", "--solution-id", "1", "--verified", "true"] },
        { ["list-unsolved"] },
        { ["verify-integrity"] },
        { ["export-solutions", "--output", "soluzioni.json"] },
    };

    [Fact]
    public async Task Help_ListsAllCommandsAndExitsWithZero()
    {
        var result = await RunAsync("--help");

        result.ExitCode.Should().Be(0);
        foreach (var command in AllCommands)
            result.Output.Should().Contain(command);
    }

    [Fact]
    public async Task Version_PrintsAssemblyVersionAndExitsWithZero()
    {
        var result = await RunAsync("--version");

        result.ExitCode.Should().Be(0);
        result.Output.Trim().Should().StartWith(typeof(CliApp).Assembly.GetName().Version!.ToString(3));
    }

    [Theory]
    [MemberData(nameof(DisabledCommands))]
    public async Task DisabledCommand_ExitsWithTwoAndSaysFromWhichPhase(string[] args, int phase)
    {
        var result = await RunAsync(args);

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain($"Comando disponibile dalla fase {phase}");
    }

    [Theory]
    [MemberData(nameof(DisabledCommandsWithoutArguments))]
    public async Task DisabledCommand_WithoutArguments_ExitsWithTwoAndSaysFromWhichPhase(string command, int phase)
    {
        var result = await RunAsync(command);

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain($"Comando disponibile dalla fase {phase}");
    }

    [Theory]
    [MemberData(nameof(DatabaseCommands))]
    public async Task DatabaseCommand_WithoutConnectionString_FailsWithClearMessage(string[] args)
    {
        var result = await RunAsync(args);

        result.ExitCode.Should().Be(3);
        result.Error.Should().Contain("ConnectionStrings__DefaultConnection");
        (result.Output + result.Error).Should().NotContain("Service provider not available");
        result.Error.Should().NotContain("   at ", "il messaggio non deve contenere uno stack trace");
    }

    [Fact]
    public void MissingConnectionStringMessage_NamesEnvironmentVariableAndDevelopmentUserSecrets()
    {
        var result = new DatabaseOptionsValidator().Validate(null, new DatabaseOptions());

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ConnectionStrings__DefaultConnection")
            .And.Contain("Development")
            .And.Contain("user-secrets");
    }

    [Fact]
    public async Task DatabaseCommand_WithUnreachableDatabase_FailsWithSingleItalianLineAndNoSecrets()
    {
        const string password = "PASSWORD-SEGNAPOSTO-123";
        var started = DateTime.UtcNow;

        var result = await RunWithConnectionAsync(UnreachableConnection(password), "export-games", "-f", "csv");

        // I tentativi automatici del provider Npgsql portano la durata fra 7 e 21 secondi: il limite blocca solo le attese infinite.
        (DateTime.UtcNow - started).Should().BeLessThan(TimeSpan.FromSeconds(40));
        result.ExitCode.Should().Be(1);
        result.Error.Should().Contain("Database non raggiungibile");
        result.Error.Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(1);
        result.Error.Split('\n').Should().NotContain(line => line.TrimStart() != line && line.TrimStart().StartsWith("at "));
        (result.Output + result.Error).Should().NotContain(password);
    }

    [Fact]
    public async Task DatabaseCommand_WithUnreachableDatabaseAndVerbose_ShowsDetails()
    {
        const string password = "PASSWORD-SEGNAPOSTO-123";

        var result = await RunWithConnectionAsync(UnreachableConnection(password), "export-games", "-f", "csv", "--verbose");

        result.ExitCode.Should().Be(1);
        result.Error.Should().Contain("Database non raggiungibile");
        result.Error.Should().Contain("   at ", "con --verbose compare lo stack trace");
        result.Error.Should().NotContain(password);
    }

    private static string UnreachableConnection(string password) =>
        $"Host=127.0.0.1;Port=1;Database=x;Username=utente;Password={password};Timeout=2;Command Timeout=2";

    private static async Task<(int ExitCode, string Output, string Error)> RunWithConnectionAsync(
        string connectionString,
        params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var code = await CliApp.RunAsync(args, output, error, WithSettings(("ConnectionStrings:DefaultConnection", connectionString)));

        return (code, output.ToString(), error.ToString());
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunAsync(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var code = await CliApp.RunAsync(args, output, error, WithSettings(("ConnectionStrings:DefaultConnection", "")));

        return (code, output.ToString(), error.ToString());
    }

    // Le impostazioni in memoria vengono per ultime e prevalgono su variabili d'ambiente e file.
    private static Action<IConfigurationBuilder> WithSettings(params (string Key, string Value)[] settings) =>
        builder => builder.AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)));
}
