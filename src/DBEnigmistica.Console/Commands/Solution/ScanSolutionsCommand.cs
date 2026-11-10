using System.CommandLine;
using DBEnigmistica.Console.Cli;

namespace DBEnigmistica.Console.Commands.Solution;

public class ScanSolutionsCommand : Command
{
    public ScanSolutionsCommand(CliStreams streams) : base("scan-solutions", "Scansione di una pagina per estrarre soluzioni (disponibile dalla fase 4)")
    {
        Options.Add(new Option<int>("--page-id") { Description = "ID della pagina da scansionare" });

        // L'estrazione delle soluzioni arriva in fase 4.
        this.SetNotAvailable(streams, 4);
    }
}
