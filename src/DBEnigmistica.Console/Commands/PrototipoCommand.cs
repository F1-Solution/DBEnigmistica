using System.CommandLine;
using System.Reflection;
using DBEnigmistica.Console.Cli;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Console.Commands;

public class PrototipoCommand : Command
{
    public PrototipoCommand(CommandRunner runner)
        : base("prototipo", "Avvia il prototipo di revisione dei Giochi originali, in ascolto solo su 127.0.0.1 (Ctrl+C per terminare)")
    {
        this.SetAction((_, ct) => runner.RunAsync<PrototipoHandler>(
            HostRequirements.None,
            (handler, token) => handler.ExecuteAsync(token),
            ct));
    }
}

/// <summary>
/// Avvia l'host del prototipo. L'host sta in <c>DBEnigmistica.Web</c>, che non c'è nell'immagine Docker (solo runtime):
/// si carica per nome e, se manca, il comando risponde come "non disponibile" (codice 2) anche senza configurazione.
/// </summary>
public sealed class PrototipoHandler(IConfiguration configurazione, CliStreams streams, string? tipoAvvio = null)
{
    private const string TipoAvvio = "DBEnigmistica.Web.Composizione.AvvioPrototipo, DBEnigmistica.Web";

    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var avvio = Type.GetType(tipoAvvio ?? TipoAvvio)?.GetMethod("EseguiAsync", BindingFlags.Public | BindingFlags.Static);
        if (avvio is null)
        {
            streams.Error.WriteLine("Il prototipo non è incluso in questa pubblicazione (per esempio l'immagine Docker): usa l'eseguibile degli archivi win-x64 o linux-x64.");
            return ExitCodes.NotAvailable;
        }

        var esegui = avvio.CreateDelegate<Func<IConfiguration, TextWriter, CancellationToken, Task<int>>>();
        return await esegui(configurazione, streams.Out, cancellationToken);
    }
}
