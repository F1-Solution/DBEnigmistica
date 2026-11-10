using System.CommandLine;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Console.Commands;

public class ScanFolderCommand : Command
{
    public ScanFolderCommand(CliStreams streams) : base("scan-folder", "Scansione di una cartella di file PDF (disponibile dalla fase 1)")
    {
        Arguments.Add(new Argument<string>("path") { Description = "Percorso della cartella contenente i file PDF", Arity = ArgumentArity.ZeroOrOne });
        Options.Add(new Option<string?>("--title", "-t") { Description = "Titolo della rivista (opzionale)" });
        Options.Add(new Option<string?>("--publisher", "-p") { Description = "Editore (opzionale)" });
        Options.Add(new Option<MagazinePeriodicity>("--periodicity", "-r")
        {
            Description = "Periodicità della rivista",
            DefaultValueFactory = _ => MagazinePeriodicity.Monthly
        });

        // L'estrazione con visione arriva in fase 1.
        this.SetNotAvailable(streams, 1);
    }
}
