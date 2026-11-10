using System.CommandLine;
using DBEnigmistica.Console.Cli;

namespace DBEnigmistica.Console.Commands;

public class AnalyzeGameCommand : Command
{
    public AnalyzeGameCommand(CliStreams streams)
        : base("analyze-game", "Analisi di una nuova Tipologia da file Markdown (disponibile dalla fase 4)")
    {
        Arguments.Add(new Argument<string>("file") { Description = "Percorso del file Markdown contenente la descrizione del gioco", Arity = ArgumentArity.ZeroOrOne });
        Options.Add(new Option<bool>("--yes", "-y") { Description = "Approva automaticamente lo schema proposto senza conferma" });

        // Le nuove tipologie arrivano in fase 4, senza DDL a runtime.
        this.SetNotAvailable(streams, 4);
    }
}
