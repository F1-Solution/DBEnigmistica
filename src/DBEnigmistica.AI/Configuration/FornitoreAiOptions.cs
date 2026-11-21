using Microsoft.Extensions.Options;

namespace DBEnigmistica.AI.Configuration;

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

    public RegistrazioniOptions Registrazioni { get; set; } = new();

    /// <summary>Impostazioni della creazione: modello proprio (Opus) e Registrazioni proprie.</summary>
    public CreazioneOptions Creazione { get; set; } = new();

    /// <summary>Numero massimo di chiamate reali per esecuzione; vuoto significa nessun limite.</summary>
    public int? LimiteChiamate { get; set; }
}

/// <summary>Sezione <c>FornitoreAi:Creazione</c>: il modello della creazione è separato da quello dell'estrazione (D1b-6).</summary>
public sealed class CreazioneOptions
{
    public string? Modello { get; set; }

    public RegistrazioniOptions Registrazioni { get; set; } = new();
}

/// <summary>Registrazioni delle risposte per pagina: <c>Modalita</c> è <c>registra</c> o <c>riproduci</c>, vuota per spenta.</summary>
public sealed class RegistrazioniOptions
{
    public const string ModalitaRegistra = "registra";
    public const string ModalitaRiproduci = "riproduci";

    public string? Modalita { get; set; }

    public string? Cartella { get; set; }
}

public sealed class ClaudeCodeOptions
{
    public string? Eseguibile { get; set; }

    public int? TimeoutSecondi { get; set; }
}

public sealed class ApiOptions
{
    public string? ChiaveApi { get; set; }
}

/// <summary>Accesso a variabili d'ambiente, file e PATH, sostituibile nei test.</summary>
public interface IAmbienteFornitoreAi
{
    string CartellaUtente { get; }

    /// <summary>Su Windows l'eseguibile di Claude Code deve essere un <c>.exe</c> nativo.</summary>
    bool EWindows => OperatingSystem.IsWindows();

    string? Variabile(string nome);

    bool FileEsiste(string percorso);

    /// <summary>Cerca nel PATH il file esatto <paramref name="nome"/> (su Windows il chiamante aggiunge <c>.exe</c>).</summary>
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
        return cartelle.Any(cartella => File.Exists(Path.Combine(cartella, nome)));
    }
}

public sealed class FornitoreAiOptionsValidator(IAmbienteFornitoreAi ambiente) : IValidateOptions<FornitoreAiOptions>
{
    public const string VariabileTokenClaudeCode = "CLAUDE_CODE_OAUTH_TOKEN";
    public const int TimeoutMassimoSecondi = 3600;

    public ValidateOptionsResult Validate(string? name, FornitoreAiOptions options)
    {
        var failures = new List<string>();
        var riproduzione = options.Registrazioni.Modalita?.Trim() == RegistrazioniOptions.ModalitaRiproduci;

        switch (options.Tipo?.Trim())
        {
            case FornitoreAiOptions.TipoClaudeCode:
                // Riproducendo non si chiama Claude Code: eseguibile e login non servono.
                if (!riproduzione)
                    ValidaClaudeCode(options, failures);
                ValidaLimiti(options, failures);
                break;
            case FornitoreAiOptions.TipoApiAnthropic:
                failures.Add("Il Fornitore AI api-anthropic non è disponibile in fase 1: imposta FornitoreAi__Tipo=claude-code.");
                break;
            default:
                failures.Add("Fornitore AI non configurato: imposta FornitoreAi__Tipo (claude-code o api-anthropic).");
                break;
        }

        if (string.IsNullOrWhiteSpace(options.Modello))
            failures.Add("Modello del Fornitore AI mancante: imposta FornitoreAi__Modello.");

        ValidaRegistrazioni(options.Registrazioni, failures);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidaLimiti(FornitoreAiOptions options, List<string> failures)
    {
        if (options.ClaudeCode.TimeoutSecondi is { } timeout and (<= 0 or > TimeoutMassimoSecondi))
            failures.Add($"Timeout non valido: FornitoreAi__ClaudeCode__TimeoutSecondi deve essere tra 1 e {TimeoutMassimoSecondi} (trovato {timeout}).");

        if (options.LimiteChiamate is < 1)
            failures.Add($"Limite non valido: FornitoreAi__LimiteChiamate deve essere almeno 1 oppure assente (trovato {options.LimiteChiamate}).");
    }

    private static void ValidaRegistrazioni(RegistrazioniOptions registrazioni, List<string> failures)
    {
        var modalita = registrazioni.Modalita?.Trim();
        if (string.IsNullOrEmpty(modalita))
            return;

        if (modalita is not (RegistrazioniOptions.ModalitaRegistra or RegistrazioniOptions.ModalitaRiproduci))
            failures.Add("Modalità delle Registrazioni non valida: imposta FornitoreAi__Registrazioni__Modalita (registra o riproduci).");
        else if (string.IsNullOrWhiteSpace(registrazioni.Cartella))
            failures.Add("Cartella delle Registrazioni mancante: imposta FornitoreAi__Registrazioni__Cartella.");
    }

    private void ValidaClaudeCode(FornitoreAiOptions options, List<string> failures)
    {
        var eseguibile = options.ClaudeCode.Eseguibile?.Trim();
        if (ambiente.EWindows && !string.IsNullOrEmpty(eseguibile) && !EstensioneAmmessaSuWindows(eseguibile))
        {
            failures.Add($"Serve l'eseguibile nativo di Claude Code (.exe), non '{eseguibile}': " +
                         "imposta FornitoreAi__ClaudeCode__Eseguibile con il percorso di claude.exe.");
        }
        else if (!EseguibileTrovato(eseguibile))
        {
            failures.Add($"Claude Code non trovato: '{eseguibile}' non esiste né è nel PATH. " +
                         "Imposta FornitoreAi__ClaudeCode__Eseguibile con il nome o il percorso di claude.");
        }

        var token = ambiente.Variabile(VariabileTokenClaudeCode);
        var credenziali = Path.Combine(ambiente.CartellaUtente, ".claude", ".credentials.json");
        if (string.IsNullOrWhiteSpace(token) && !ambiente.FileEsiste(credenziali))
            failures.Add("Claude Code non autenticato: esegui claude e poi /login");
    }

    // Su Windows un nome senza estensione si risolve in .exe; qualsiasi altra estensione (.cmd, .bat, ...) è rifiutata.
    private bool EstensioneAmmessaSuWindows(string eseguibile)
    {
        var nomeFile = NomeFile(eseguibile);
        if (nomeFile.EndsWith('.') || nomeFile.EndsWith(' '))
            return false;

        var haPercorso = HaPercorso(eseguibile);
        var estensione = Path.GetExtension(nomeFile);
        return estensione.Equals(".exe", StringComparison.OrdinalIgnoreCase) || (!haPercorso && estensione.Length == 0);
    }

    private bool EseguibileTrovato(string? eseguibile)
    {
        if (string.IsNullOrEmpty(eseguibile))
            return false;

        if (HaPercorso(eseguibile))
            return ambiente.FileEsiste(eseguibile);

        var nome = ambiente.EWindows && Path.GetExtension(NomeFile(eseguibile)).Length == 0 ? eseguibile + ".exe" : eseguibile;
        return ambiente.EseguibileNelPath(nome);
    }

    // Con le regole Windows sia '\' sia '/' separano i percorsi, indipendentemente dal sistema che esegue il codice.
    private bool HaPercorso(string valore) =>
        ambiente.EWindows
            ? valore.AsSpan().IndexOfAny('\\', '/') >= 0
            : valore.Contains(Path.DirectorySeparatorChar) || valore.Contains(Path.AltDirectorySeparatorChar);

    private string NomeFile(string valore) =>
        ambiente.EWindows ? valore[(valore.AsSpan().LastIndexOfAny('\\', '/') + 1)..] : Path.GetFileName(valore);
}
