using System.Diagnostics;
using System.Text.Json;

namespace DBEnigmistica.Tests.Fixtures.ClaudeFinto;

/// <summary>Un passo del copione dell'eseguibile finto: cosa stampa, con quale codice esce e quanto attende.</summary>
public sealed record PassoClaudeFinto(string Stdout = "", string? Stderr = null, int Exit = 0, int? SleepMs = null, bool Nipote = false);

/// <summary>Una voce del registro delle invocazioni dell'eseguibile finto.</summary>
public sealed record InvocazioneClaudeFinta(
    string[] Args, string Stdin, int Pid, string Cwd, string[] File, long LunghezzaPng, bool ChiaveApi, bool TokenAuth, bool BaseUrl);

/// <summary>Compila una volta sola l'eseguibile finto di Claude Code e ne prepara copie con un copione proprio, senza rete.</summary>
public static class ClaudeFintoFixture
{
    private static readonly Lazy<string> Compilato = new(Compila, LazyThreadSafetyMode.ExecutionAndPublication);

    public static ClaudeFintoIstanza Crea(params PassoClaudeFinto[] passi)
    {
        var cartella = Path.Combine(Path.GetTempPath(), "dbe-claude-finto-" + Guid.NewGuid().ToString("N"));
        CopiaCartella(Compilato.Value, cartella);
        File.WriteAllText(Path.Combine(cartella, "scenario.json"), JsonSerializer.Serialize(
            passi.Select(p => new Dictionary<string, object?>
            {
                ["stdout"] = p.Stdout,
                ["stderr"] = p.Stderr,
                ["exit"] = p.Exit,
                ["sleepMs"] = p.SleepMs,
                ["nipote"] = p.Nipote ? true : null,
            }).Select(d => d.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value))));
        return new ClaudeFintoIstanza(cartella);
    }

    private static string Compila()
    {
        var radice = Path.Combine(Path.GetTempPath(), "dbe-claude-finto-build-" + Guid.NewGuid().ToString("N"));
        var sorgenti = Path.Combine(radice, "src");
        var uscita = Path.Combine(radice, "out");
        Directory.CreateDirectory(sorgenti);

        var origine = TrovaSorgenti();
        File.Copy(Path.Combine(origine, "ClaudeFinto.csproj.template"), Path.Combine(sorgenti, "ClaudeFinto.csproj"));
        File.Copy(Path.Combine(origine, "Program.cs.txt"), Path.Combine(sorgenti, "Program.cs"));

        var avvio = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argomento in new[] { "build", Path.Combine(sorgenti, "ClaudeFinto.csproj"), "-c", "Release", "-o", uscita, "--nologo", "-v", "q" })
            avvio.ArgumentList.Add(argomento);

        using var processo = Process.Start(avvio)!;
        var testo = processo.StandardOutput.ReadToEndAsync();
        var errori = processo.StandardError.ReadToEndAsync();

        // Una compilazione che non finisce non deve bloccare la suite.
        if (!processo.WaitForExit(TimeSpan.FromMinutes(3)))
        {
            processo.Kill(entireProcessTree: true);
            throw new TimeoutException("Compilazione dell'eseguibile finto oltre i 3 minuti.");
        }

        if (processo.ExitCode != 0)
            throw new InvalidOperationException($"Compilazione dell'eseguibile finto fallita: {testo.Result}{errori.Result}");

        return uscita;
    }

    // I sorgenti dell'eseguibile finto non sono copiati nell'output: si cercano risalendo dal binario di test.
    private static string TrovaSorgenti()
    {
        for (var cartella = new DirectoryInfo(AppContext.BaseDirectory); cartella is not null; cartella = cartella.Parent)
        {
            var candidata = Path.Combine(cartella.FullName, "Fixtures", "ClaudeFinto");
            if (File.Exists(Path.Combine(candidata, "Program.cs.txt")))
                return candidata;
        }

        throw new DirectoryNotFoundException("Sorgenti di ClaudeFinto non trovati.");
    }

    private static void CopiaCartella(string origine, string destinazione)
    {
        Directory.CreateDirectory(destinazione);
        foreach (var file in Directory.GetFiles(origine))
            File.Copy(file, Path.Combine(destinazione, Path.GetFileName(file)));
    }
}

/// <summary>Una copia dell'eseguibile finto con il suo copione e il suo registro; si elimina con Dispose.</summary>
public sealed class ClaudeFintoIstanza(string cartella) : IDisposable
{
    public string Eseguibile { get; } = Path.Combine(cartella, OperatingSystem.IsWindows() ? "ClaudeFinto.exe" : "ClaudeFinto");

    public IReadOnlyList<InvocazioneClaudeFinta> Invocazioni
    {
        get
        {
            var percorso = Path.Combine(cartella, "log.jsonl");
            if (!File.Exists(percorso))
                return [];

            return File.ReadAllLines(percorso).Where(l => l.Length > 0).Select(riga =>
            {
                var radice = JsonDocument.Parse(riga).RootElement;
                return new InvocazioneClaudeFinta(
                    radice.GetProperty("args"u8).EnumerateArray().Select(a => a.GetString()!).ToArray(),
                    radice.GetProperty("stdin"u8).GetString()!,
                    radice.GetProperty("pid"u8).GetInt32(),
                    radice.GetProperty("cwd"u8).GetString()!,
                    radice.GetProperty("file"u8).EnumerateArray().Select(a => a.GetString()!).ToArray(),
                    radice.GetProperty("lunghezzaPng"u8).GetInt64(),
                    radice.GetProperty("chiaveApi"u8).GetBoolean(),
                    radice.GetProperty("tokenAuth"u8).GetBoolean(),
                    radice.GetProperty("baseUrl"u8).GetBoolean());
            }).ToList();
        }
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(cartella, recursive: true);
        }
        catch (IOException)
        {
            // Un processo figlio ancora in chiusura può trattenere un file: la cartella resta in temp.
        }
    }
}
