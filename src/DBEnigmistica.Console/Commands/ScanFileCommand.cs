using DBEnigmistica.Domain.Enums;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace DBEnigmistica.Console.Commands;

public class ScanFileCommand : Command
{
    public ScanFileCommand() : base("scan-file", "Scansiona un singolo file PDF ed estrae i giochi")
    {
        var pathArgument = new Argument<string>(
            name: "path",
            description: "Percorso del file PDF da scansionare");

        var titleOption = new Option<string?>(
            aliases: new[] { "--title", "-t" },
            description: "Titolo della rivista (opzionale, default: nome file)");

        var issueOption = new Option<int?>(
            aliases: new[] { "--issue", "-i" },
            description: "Numero della rivista (opzionale)");

        var dateOption = new Option<DateTime?>(
            aliases: new[] { "--date", "-d" },
            description: "Data di pubblicazione (formato: yyyy-MM-dd)");

        var periodicityOption = new Option<MagazinePeriodicity>(
            aliases: new[] { "--periodicity", "-r" },
            getDefaultValue: () => MagazinePeriodicity.Monthly,
            description: "Periodicità della rivista");

        AddArgument(pathArgument);
        AddOption(titleOption);
        AddOption(issueOption);
        AddOption(dateOption);
        AddOption(periodicityOption);

        // L'estrazione con visione arriva in fase 1.
        this.SetHandler((InvocationContext context) =>
        {
            System.Console.Error.WriteLine("Comando disponibile dalla fase 1");
            context.ExitCode = 2;
        });
    }
}
