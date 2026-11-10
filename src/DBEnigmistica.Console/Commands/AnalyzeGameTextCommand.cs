using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class AnalyzeGameTextCommand : Command
{
    public AnalyzeGameTextCommand() : base("analyze-game-text", "Analizza un nuovo tipo di gioco da descrizione testuale e crea lo schema")
    {
        var descriptionArgument = new Argument<string>(
            name: "description",
            description: "Descrizione testuale del tipo di gioco");

        var nameOption = new Option<string?>(
            aliases: new[] { "--name", "-n" },
            description: "Nome del tipo di gioco (opzionale)");

        AddArgument(descriptionArgument);
        AddOption(nameOption);

        // Le nuove tipologie arrivano in fase 4, senza DDL a runtime.
        this.SetHandler((InvocationContext context) =>
        {
            System.Console.Error.WriteLine("Comando disponibile dalla fase 4");
            context.ExitCode = 2;
        });
    }
}
