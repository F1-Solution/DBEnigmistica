using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class AnalyzeGameCommand : Command
{
    public AnalyzeGameCommand() : base("analyze-game", "Analizza un nuovo tipo di gioco da file Markdown e crea lo schema")
    {
        var fileArgument = new Argument<string>(
            name: "file",
            description: "Percorso del file Markdown contenente la descrizione del gioco");

        var autoApproveOption = new Option<bool>(
            aliases: new[] { "--yes", "-y" },
            description: "Approva automaticamente lo schema proposto senza conferma");

        AddArgument(fileArgument);
        AddOption(autoApproveOption);

        // Le nuove tipologie arrivano in fase 4, senza DDL a runtime.
        this.SetHandler((InvocationContext context) =>
        {
            System.Console.Error.WriteLine("Comando disponibile dalla fase 4");
            context.ExitCode = 2;
        });
    }
}
