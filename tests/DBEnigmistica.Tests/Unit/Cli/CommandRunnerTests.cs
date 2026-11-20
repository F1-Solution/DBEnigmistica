using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Console.Cli;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Cli;

public class CommandRunnerTests
{
    [Fact]
    public async Task LimiteDellEsecuzioneRaggiunto_EsceConQuattroEDiceIlLimite()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var runner = new CommandRunner(new CliStreams(output, error));

        var codice = await runner.RunAsync<CliStreams>(
            HostRequirements.None,
            (_, _) => throw new LimiteEsecuzioneRaggiuntoException(),
            CancellationToken.None);

        codice.Should().Be(ExitCodes.LimitReached).And.Be(4);
        error.ToString().Should().Contain("Limite dell'esecuzione raggiunto");
    }

    [Fact]
    public async Task ErroreGenerico_RestaAUno()
    {
        var error = new StringWriter();
        var runner = new CommandRunner(new CliStreams(new StringWriter(), error));

        var codice = await runner.RunAsync<CliStreams>(
            HostRequirements.None, (_, _) => throw new InvalidOperationException("boom"), CancellationToken.None);

        codice.Should().Be(ExitCodes.Error);
    }
}
