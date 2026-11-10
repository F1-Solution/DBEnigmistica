using System.CommandLine;
using DBEnigmistica.Console.Cli;

namespace DBEnigmistica.Console.Commands;

public static class NotAvailableAction
{
    /// <summary>Collega al comando il messaggio "disponibile dalla fase N" su stderr e il codice d'uscita 2.</summary>
    public static void SetNotAvailable(this Command command, CliStreams streams, int phase) =>
        command.SetAction(_ =>
        {
            streams.Error.WriteLine($"Comando disponibile dalla fase {phase}");
            return ExitCodes.NotAvailable;
        });
}
