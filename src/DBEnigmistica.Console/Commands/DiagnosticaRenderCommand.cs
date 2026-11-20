using System.CommandLine;
using DBEnigmistica.Application.Acquisizione;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Infrastructure.Services.Pdf;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Console.Commands;

/// <summary>Comando nascosto: rende un PDF sintetico e verifica che le pagine non siano vuote.</summary>
public class DiagnosticaRenderCommand : Command
{
    public DiagnosticaRenderCommand(CommandRunner runner)
        : base("diagnostica-render", "Verifica il rendering delle pagine con un PDF sintetico")
    {
        Hidden = true;
        var salvaPng = new Option<DirectoryInfo?>("--salva-png") { Description = "Cartella in cui salvare i PNG delle pagine" };
        var dpi = new Option<int?>("--dpi") { Description = "Risoluzione; predefinita Estrazione:Dpi" };
        Options.Add(salvaPng);
        Options.Add(dpi);

        this.SetAction((parseResult, ct) => runner.RunAsync<DiagnosticaRenderHandler>(
            HostRequirements.None,
            (handler, token) => handler.ExecuteAsync(parseResult.GetValue(salvaPng), parseResult.GetValue(dpi), token),
            ct));
    }
}

public sealed class DiagnosticaRenderHandler(IRenderPagine render, IOptions<EstrazioneOptions> options, CliStreams streams)
{
    // Una pagina con contenuto ne ha ben più: sotto questa soglia il testo non è stato reso.
    private const long PixelMinimi = 2_000;

    public async Task<int> ExecuteAsync(DirectoryInfo? cartellaPng, int? dpi, CancellationToken cancellationToken)
    {
        var risoluzione = dpi ?? options.Value.Dpi;
        var tipi = Enum.GetValues<TipoPaginaSintetica>();
        var percorso = Path.Combine(Path.GetTempPath(), $"dbe-diagnostica-{Guid.NewGuid():N}.pdf");
        await File.WriteAllBytesAsync(percorso, GeneratorePdfSintetici.Documento(), cancellationToken);
        try
        {
            var info = await render.LeggiAsync(percorso, cancellationToken);
            cartellaPng?.Create();
            var esito = ExitCodes.Success;

            foreach (var pagina in info.Pagine)
            {
                var resa = await render.RenderizzaAsync(percorso, pagina.Numero, risoluzione, cancellationToken);
                var nome = tipi[pagina.Numero - 1];
                var vuota = resa.PixelNonBianchi < PixelMinimi;
                streams.Out.WriteLine($"Pagina {pagina.Numero} ({nome}): {resa.Larghezza}x{resa.Altezza} pixel a {resa.Dpi} DPI, " +
                                      $"{resa.PixelNonBianchi} pixel con contenuto{(vuota ? " - VUOTA" : string.Empty)}");
                if (cartellaPng is not null)
                    await File.WriteAllBytesAsync(Path.Combine(cartellaPng.FullName, $"pagina-{pagina.Numero:D2}-{nome}.png"), resa.Png, cancellationToken);
                if (vuota)
                    esito = ExitCodes.Error;
            }

            streams.Out.WriteLine(esito == ExitCodes.Success ? "Rendering verificato." : "Rendering non riuscito: almeno una pagina è vuota.");
            return esito;
        }
        finally
        {
            File.Delete(percorso);
        }
    }
}
