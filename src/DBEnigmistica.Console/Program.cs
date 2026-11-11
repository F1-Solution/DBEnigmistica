using System.Text;
using DBEnigmistica.Console.Cli;

namespace DBEnigmistica.Console;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        System.Console.OutputEncoding = Encoding.UTF8;

        using var cts = new CancellationTokenSource();
        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        return await CliApp.RunAsync(args, System.Console.Out, System.Console.Error, cancellationToken: cts.Token);
    }
}
