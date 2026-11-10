using DBEnigmistica.Domain.Enums;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class ScanFolderCommand : Command
{
    public ScanFolderCommand() : base("scan-folder", "Scansiona una cartella di file PDF ed estrae i giochi")
    {
        var pathArgument = new Argument<string>(
            name: "path",
            description: "Percorso della cartella contenente i file PDF");

        var titleOption = new Option<string?>(
            aliases: new[] { "--title", "-t" },
            description: "Titolo della rivista (opzionale)");

        var publisherOption = new Option<string?>(
            aliases: new[] { "--publisher", "-p" },
            description: "Editore (opzionale)");

        var periodicityOption = new Option<MagazinePeriodicity>(
            aliases: new[] { "--periodicity", "-r" },
            getDefaultValue: () => MagazinePeriodicity.Monthly,
            description: "Periodicità della rivista");

        AddArgument(pathArgument);
        AddOption(titleOption);
        AddOption(publisherOption);
        AddOption(periodicityOption);

        // L'estrazione con visione arriva in fase 1.
        this.SetHandler((InvocationContext context) =>
        {
            System.Console.Error.WriteLine("Comando disponibile dalla fase 1");
            context.ExitCode = 2;
        });
    }
}
