using System.CommandLine;
using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Console.Cli;
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
public sealed class CheckAiHandler(IOptions<FornitoreAiOptions> options, IAmbienteFornitoreAi ambiente, CliStreams streams)
{
    public int Execute()
    {
        var value = options.Value;
        streams.Out.WriteLine($"Fornitore AI: {value.Tipo}");
        streams.Out.WriteLine($"Modello: {value.Modello}");

        if (value.Tipo?.Trim() == FornitoreAiOptions.TipoClaudeCode && !string.IsNullOrWhiteSpace(ambiente.Variabile("ANTHROPIC_API_KEY")))
            streams.Out.WriteLine("Avviso: ANTHROPIC_API_KEY è presente nell'ambiente: la chiave non verrà passata a Claude Code.");

        streams.Out.WriteLine("Configurazione valida.");
        return ExitCodes.Success;
    }
}
