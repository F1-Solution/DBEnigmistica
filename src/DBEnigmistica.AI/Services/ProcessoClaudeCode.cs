using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DBEnigmistica.AI.Services;

/// <summary>Avvio del processo di Claude Code e utilità comuni agli adattatori di estrazione e di creazione.</summary>
internal static class ProcessoClaudeCode
{
    public const int RigaComandoMassimaWindows = 32_000;
    public const int RigaComandoMassimaAltri = 120_000;

    // Variabili che cambierebbero autenticazione o fornitore e quindi addebiterebbero a consumo (D5).
    private static readonly string[] VariabiliDaTogliere =
    [
        "ANTHROPIC_API_KEY",
        "ANTHROPIC_AUTH_TOKEN",
        "ANTHROPIC_BASE_URL",
        "ANTHROPIC_PROFILE",
        "ANTHROPIC_FEDERATION_RULE_ID",
        "ANTHROPIC_ORGANIZATION_ID",
        "CLAUDE_CODE_USE_BEDROCK",
        "CLAUDE_CODE_USE_VERTEX",
        "CLAUDE_CODE_USE_FOUNDRY",
    ];

    public static async Task<(UscitaClaudeCode Uscita, string? Grezza)> EseguiAsync(
        string eseguibile,
        IReadOnlyList<string> argomenti,
        string cartella,
        string prompt,
        TimeSpan timeout,
        TimeProvider tempo,
        CancellationToken ct)
    {
        var avvio = new ProcessStartInfo(eseguibile.Trim())
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = cartella,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        foreach (var argomento in argomenti)
            avvio.ArgumentList.Add(argomento);

        foreach (var variabile in VariabiliDaTogliere)
            avvio.Environment.Remove(variabile);

        Process processo;
        try
        {
            processo = Process.Start(avvio) ?? throw new InvalidOperationException("Avvio di Claude Code non riuscito");
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            return (new UscitaClaudeCode(TipoUscitaClaudeCode.Definitivo, null, $"Claude Code non avviabile: {ex.Message}", null, null, null, null), null);
        }

        using (processo)
        {
            var uscitaStandard = processo.StandardOutput.ReadToEndAsync();
            var erroriStandard = processo.StandardError.ReadToEndAsync();
            Osserva(uscitaStandard);
            Osserva(erroriStandard);
            using var scadenza = CancellationTokenSource.CreateLinkedTokenSource(ct);
            scadenza.CancelAfter(timeout);

            try
            {
                await ScriviStdinAsync(processo, prompt, scadenza.Token);
                await processo.WaitForExitAsync(scadenza.Token);

                // Un nipote che tiene aperta la pipe non deve bloccare oltre il timeout.
                var testo = await uscitaStandard.WaitAsync(scadenza.Token);
                var errori = await erroriStandard.WaitAsync(scadenza.Token);
                return (UscitaClaudeCode.Analizza(testo, errori, processo.ExitCode, tempo), testo);
            }
            catch (OperationCanceledException)
            {
                Chiudi(processo);
                if (ct.IsCancellationRequested)
                    throw;

                return (new UscitaClaudeCode(TipoUscitaClaudeCode.Transitorio, null, "Timeout di Claude Code", null, null, null, null), null);
            }
        }
    }

    public static string Minimizza(string json)
    {
        using var documento = JsonDocument.Parse(json);
        using var memoria = new MemoryStream();
        using (var scrittore = new Utf8JsonWriter(memoria, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
            documento.RootElement.WriteTo(scrittore);

        return Encoding.UTF8.GetString(memoria.ToArray());
    }

    // Stima per eccesso della riga di comando: virgolette e barre rovesciate contano doppio, ogni argomento ha le virgolette.
    public static int LunghezzaRigaComando(string eseguibile, IEnumerable<string> argomenti) =>
        Lunghezza(eseguibile) + argomenti.Sum(a => 1 + Lunghezza(a));

    private static int Lunghezza(string valore) =>
        valore.Length + 2 + valore.Count(c => c is '"' or '\\');

    private static async Task ScriviStdinAsync(Process processo, string prompt, CancellationToken ct)
    {
        try
        {
            await processo.StandardInput.WriteAsync(prompt.AsMemory(), ct);
            processo.StandardInput.Close();
        }
        catch (IOException)
        {
            // Il processo può uscire senza leggere tutto lo stdin: l'esito si legge dall'uscita.
        }
    }

    private static void Chiudi(Process processo)
    {
        try
        {
            processo.Kill(entireProcessTree: true);
            processo.WaitForExit(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            // Il processo era già terminato.
        }
    }

    // Una lettura abbandonata dopo un timeout non deve lasciare eccezioni non osservate.
    private static void Osserva(Task<string> lettura) =>
        lettura.ContinueWith(t => _ = t.Exception, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
}
