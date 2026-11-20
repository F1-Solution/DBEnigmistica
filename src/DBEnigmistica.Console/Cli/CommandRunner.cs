using System.Net.Sockets;
using DBEnigmistica.Application.FornitoreAi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DBEnigmistica.Console.Cli;

/// <summary>
/// Costruisce e avvia il host di un comando, risolve il gestore in uno scope per esecuzione e ne restituisce il codice d'uscita.
/// </summary>
public sealed class CommandRunner(CliStreams streams, Action<IConfigurationBuilder>? configure = null, bool verbose = false)
{
    public async Task<int> RunAsync<THandler>(
        HostRequirements requirements,
        Func<THandler, CancellationToken, Task<int>> run,
        CancellationToken cancellationToken)
        where THandler : notnull
    {
        IHost? host = null;
        try
        {
            host = CliHost.Build(requirements, streams, configure, verbose);
            await host.StartAsync(cancellationToken);

            await using var scope = host.Services.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            return await run(handler, cancellationToken);
        }
        catch (Exception ex) when (TryGetValidationFailures(ex, out var failures))
        {
            foreach (var failure in failures)
                streams.Error.WriteLine(failure);

            return ExitCodes.Configuration;
        }
        catch (Exception ex) when (Flatten(ex).OfType<LimiteEsecuzioneRaggiuntoException>().Any())
        {
            streams.Error.WriteLine("Limite dell'esecuzione raggiunto");
            return ExitCodes.LimitReached;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            streams.Error.WriteLine("Operazione annullata.");
            return ExitCodes.Cancelled;
        }
        catch (Exception ex)
        {
            if (verbose)
                host?.Services.GetService<ILogger<CommandRunner>>()?.LogError(ex, "Comando terminato con un errore");

            streams.Error.WriteLine(IsDatabaseUnreachable(ex)
                ? $"Database non raggiungibile: {ex.GetBaseException().Message}"
                : $"Errore: {ex.Message}");
            return ExitCodes.Error;
        }
        finally
        {
            await StopAsync(host);
        }
    }

    // Gli errori inviati dal server (PostgresException) non sono problemi di raggiungibilità.
    private static bool IsDatabaseUnreachable(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is PostgresException)
                return false;

            if (current is NpgsqlException or SocketException)
                return true;
        }

        return false;
    }

    private static async Task StopAsync(IHost? host)
    {
        if (host is null)
            return;

        try
        {
            await host.StopAsync(CancellationToken.None);
        }
        finally
        {
            host.Dispose();
        }
    }

    private static bool TryGetValidationFailures(Exception ex, out IReadOnlyList<string> failures)
    {
        var found = Flatten(ex).OfType<OptionsValidationException>().SelectMany(e => e.Failures).Distinct().ToList();
        failures = found;
        return found.Count > 0;
    }

    private static IEnumerable<Exception> Flatten(Exception ex) =>
        ex is AggregateException aggregate ? aggregate.Flatten().InnerExceptions : [ex];
}
