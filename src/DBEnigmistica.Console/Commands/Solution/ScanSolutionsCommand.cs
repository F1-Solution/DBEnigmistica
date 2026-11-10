using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands.Solution;

public class ScanSolutionsCommand : Command
{
    public ScanSolutionsCommand() : base("scan-solutions", "Scansiona una pagina per estrarre soluzioni")
    {
        var pageIdOption = new Option<int>(
            "--page-id",
            "ID della pagina da scansionare"
        ) { IsRequired = true };

        AddOption(pageIdOption);

        // L'estrazione delle soluzioni arriva in fase 4.
        this.SetHandler((InvocationContext context) =>
        {
            System.Console.Error.WriteLine("Comando disponibile dalla fase 4");
            context.ExitCode = 2;
        });
    }
}
