using Spectre.Console;

namespace DBEnigmistica.Console.Cli;

/// <summary>Scrittori di output e errori dell'esecuzione, più la console Spectre sull'output.</summary>
public sealed class CliStreams
{
    public CliStreams(TextWriter output, TextWriter error)
    {
        Out = output;
        Error = error;
        Console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            Out = new AnsiConsoleOutput(output)
        });
    }

    public TextWriter Out { get; }

    public TextWriter Error { get; }

    public IAnsiConsole Console { get; }
}
