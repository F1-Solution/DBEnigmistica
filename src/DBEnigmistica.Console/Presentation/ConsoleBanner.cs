using Spectre.Console;

namespace DBEnigmistica.Console.Presentation;

public static class ConsoleBanner
{
    public static void Write(IAnsiConsole console, string title)
    {
        console.Write(new Rule($"[cyan]DBEnigmistica - {Markup.Escape(title)}[/]").LeftJustified());
        console.WriteLine();
    }

    public static void Success(IAnsiConsole console, string message) =>
        console.MarkupLineInterpolated($"[green]✓ {message}[/]");

    public static void Warning(IAnsiConsole console, string message) =>
        console.MarkupLineInterpolated($"[yellow]⚠ {message}[/]");
}
