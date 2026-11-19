using Microsoft.Extensions.Options;

namespace DBEnigmistica.Console.Configuration;

/// <summary>Sezione <c>FornitoreAi</c> della configurazione, validata solo dai comandi che usano l'AI.</summary>
public sealed class FornitoreAiOptions
{
    public const string SectionName = "FornitoreAi";
    public const string TipoClaudeCode = "claude-code";
    public const string TipoApiAnthropic = "api-anthropic";

    public string? Tipo { get; set; }

    public string? Modello { get; set; }

    public ClaudeCodeOptions ClaudeCode { get; set; } = new();

    public ApiOptions Api { get; set; } = new();
}

public sealed class ClaudeCodeOptions
{
    public string? Eseguibile { get; set; }
}

public sealed class ApiOptions
{
    public string? ChiaveApi { get; set; }
}

/// <summary>Accesso a variabili d'ambiente, file e PATH, sostituibile nei test.</summary>
public interface IAmbienteFornitoreAi
{
    string CartellaUtente { get; }

    string? Variabile(string nome);

    bool FileEsiste(string percorso);

    bool EseguibileNelPath(string nome);
}

public sealed class AmbienteFornitoreAi : IAmbienteFornitoreAi
{
    public string CartellaUtente => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public string? Variabile(string nome) => Environment.GetEnvironmentVariable(nome);

    public bool FileEsiste(string percorso) => File.Exists(percorso);

    public bool EseguibileNelPath(string nome)
    {
        var cartelle = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var estensioni = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries).Prepend(string.Empty)
            : [string.Empty];

        return cartelle.Any(cartella => estensioni.Any(estensione => File.Exists(Path.Combine(cartella, nome + estensione))));
    }
}

public sealed class FornitoreAiOptionsValidator(IAmbienteFornitoreAi ambiente) : IValidateOptions<FornitoreAiOptions>
{
    public const string VariabileTokenClaudeCode = "CLAUDE_CODE_OAUTH_TOKEN";

    public ValidateOptionsResult Validate(string? name, FornitoreAiOptions options)
    {
        var failures = new List<string>();

        switch (options.Tipo?.Trim())
        {
            case FornitoreAiOptions.TipoClaudeCode:
                ValidaClaudeCode(options, failures);
                break;
            case FornitoreAiOptions.TipoApiAnthropic:
                if (string.IsNullOrWhiteSpace(options.Api.ChiaveApi))
                    failures.Add("Chiave API mancante: imposta FornitoreAi__Api__ChiaveApi.");
                break;
            default:
                failures.Add("Fornitore AI non configurato: imposta FornitoreAi__Tipo (claude-code o api-anthropic).");
                break;
        }

        if (string.IsNullOrWhiteSpace(options.Modello))
            failures.Add("Modello del Fornitore AI mancante: imposta FornitoreAi__Modello.");

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private void ValidaClaudeCode(FornitoreAiOptions options, List<string> failures)
    {
        var eseguibile = options.ClaudeCode.Eseguibile?.Trim();
        if (!EseguibileTrovato(eseguibile))
            failures.Add($"Claude Code non trovato: '{eseguibile}' non esiste né è nel PATH. " +
                         "Imposta FornitoreAi__ClaudeCode__Eseguibile con il nome o il percorso di claude.");

        var token = ambiente.Variabile(VariabileTokenClaudeCode);
        var credenziali = Path.Combine(ambiente.CartellaUtente, ".claude", ".credentials.json");
        if (string.IsNullOrWhiteSpace(token) && !ambiente.FileEsiste(credenziali))
            failures.Add($"Claude Code non autenticato: imposta {VariabileTokenClaudeCode} oppure esegui l'accesso con claude.");
    }

    private bool EseguibileTrovato(string? eseguibile)
    {
        if (string.IsNullOrEmpty(eseguibile))
            return false;

        var haPercorso = eseguibile.Contains(Path.DirectorySeparatorChar) || eseguibile.Contains(Path.AltDirectorySeparatorChar);
        return haPercorso ? ambiente.FileEsiste(eseguibile) : ambiente.EseguibileNelPath(eseguibile);
    }
}
