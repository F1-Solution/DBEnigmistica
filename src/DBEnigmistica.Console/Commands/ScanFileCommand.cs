using System.CommandLine;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Console.Commands;

public class ScanFileCommand : Command
{
    public ScanFileCommand(CliStreams streams) : base("scan-file", "Scansione di un file PDF (disponibile dalla fase 1)")
    {
        Arguments.Add(new Argument<string>("path") { Description = "Percorso del file PDF da scansionare", Arity = ArgumentArity.ZeroOrOne });
        Options.Add(new Option<string?>("--title", "-t") { Description = "Titolo della rivista (opzionale, default: nome file)" });
        Options.Add(new Option<int?>("--issue", "-i") { Description = "Numero della rivista (opzionale)" });
        Options.Add(new Option<DateTime?>("--date", "-d") { Description = "Data di pubblicazione (formato: yyyy-MM-dd)" });
        Options.Add(new Option<MagazinePeriodicity>("--periodicity", "-r")
        {
            Description = "Periodicità della rivista",
            DefaultValueFactory = _ => MagazinePeriodicity.Monthly
        });

        // L'estrazione con visione arriva in fase 1.
        this.SetNotAvailable(streams, 1);
    }
}
