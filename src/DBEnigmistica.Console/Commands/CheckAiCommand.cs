using System.CommandLine;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Configuration;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Console.Commands;

public class CheckAiCommand : Command
{
    public CheckAiCommand(CommandRunner runner)
        : base("check-ai", "Verifica la configurazione del Fornitore AI, senza chiamate di rete")
    {
        this.SetAction((_, ct) => runner.RunAsync<CheckAiHandler>(
            HostRequirements.Ai,
            (handler, _) => Task.FromResult(handler.Execute()),
            ct));
    }
}

/// <summary>La validazione avviene all'avvio del host: qui si riporta solo l'esito, senza mai stampare la chiave.</summary>
public sealed class CheckAiHandler(IOptions<FornitoreAiOptions> options, CliStreams streams)
{
    public int Execute()
    {
        var value = options.Value;
        streams.Out.WriteLine($"Fornitore AI: {value.Tipo}");
        streams.Out.WriteLine($"Modello: {value.Modello}");
        streams.Out.WriteLine("Configurazione valida.");
        return ExitCodes.Success;
    }
}
