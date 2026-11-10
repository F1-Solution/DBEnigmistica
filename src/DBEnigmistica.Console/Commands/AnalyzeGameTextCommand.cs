using System.CommandLine;
using DBEnigmistica.Console.Cli;

namespace DBEnigmistica.Console.Commands;

public class AnalyzeGameTextCommand : Command
{
    public AnalyzeGameTextCommand(CliStreams streams)
        : base("analyze-game-text", "Analisi di una nuova Tipologia da descrizione testuale (disponibile dalla fase 4)")
    {
        Arguments.Add(new Argument<string>("description") { Description = "Descrizione testuale della Tipologia", Arity = ArgumentArity.ZeroOrOne });
        Options.Add(new Option<string?>("--name", "-n") { Description = "Nome del tipo di gioco (opzionale)" });

        // Le nuove tipologie arrivano in fase 4, senza DDL a runtime.
        this.SetNotAvailable(streams, 4);
    }
}
