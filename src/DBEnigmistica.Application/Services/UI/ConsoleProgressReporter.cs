using DBEnigmistica.Application.Services.Processing;

namespace DBEnigmistica.Application.Services.UI;

public class ConsoleProgressReporter : IProgress<ProcessingProgress>
{
    private readonly object _lock = new();
    private int _lastPercentage = -1;
    private string _lastStage = string.Empty;

    public void Report(ProcessingProgress value)
    {
        lock (_lock)
        {
            // Clear previous line if updating same stage
            if (value.Stage == _lastStage && value.PercentComplete > _lastPercentage)
            {
                System.Console.Write("\r");
            }
            else if (value.Stage != _lastStage)
            {
                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine($"╔═══ {value.Stage} ═══");
                System.Console.ResetColor();
                _lastStage = value.Stage;
            }

            // Progress bar
            if (value.TotalPages > 0 || value.TotalFiles > 0)
            {
                var progressBar = CreateProgressBar(value.PercentComplete);
                System.Console.Write($"║ {progressBar} {value.PercentComplete,3}% │ {value.Message}");
            }
            else
            {
                System.Console.Write($"║ {value.Message}");
            }

            _lastPercentage = value.PercentComplete;

            // Add newline if complete
            if (value.PercentComplete == 100 || value.Stage == "Completato")
            {
                System.Console.WriteLine();
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("╚═══════════════════════════════════════════════════════════");
                System.Console.ResetColor();
            }
        }
    }

    private string CreateProgressBar(int percentage)
    {
        const int barLength = 30;
        var filled = (int)(barLength * (percentage / 100.0));
        var empty = barLength - filled;

        var bar = new string('█', filled) + new string('░', empty);
        return $"[{bar}]";
    }

    public void ReportError(string message)
    {
        lock (_lock)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ ERRORE: {message}");
            System.Console.ResetColor();
        }
    }

    public void ReportSuccess(string message)
    {
        lock (_lock)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ {message}");
            System.Console.ResetColor();
        }
    }

    public void ReportWarning(string message)
    {
        lock (_lock)
        {
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"⚠ {message}");
            System.Console.ResetColor();
        }
    }

    public void Clear()
    {
        _lastPercentage = -1;
        _lastStage = string.Empty;
    }
}
