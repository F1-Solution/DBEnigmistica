using Serilog.Core;
using Serilog.Events;

namespace DBEnigmistica.Console.Cli;

/// <summary>Scrive i log di Serilog sullo scrittore d'errore dell'esecuzione invece che su System.Console.</summary>
internal sealed class ErrorWriterSink(TextWriter writer) : ILogEventSink
{
    private readonly object _gate = new();

    public void Emit(LogEvent logEvent)
    {
        lock (_gate)
        {
            writer.WriteLine($"[{logEvent.Level.ToString().ToUpperInvariant()}] {logEvent.RenderMessage()}");

            if (logEvent.Exception is not null)
                writer.WriteLine(logEvent.Exception);
        }
    }
}
