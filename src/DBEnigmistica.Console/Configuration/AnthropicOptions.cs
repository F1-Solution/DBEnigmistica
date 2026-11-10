using Microsoft.Extensions.Options;

namespace DBEnigmistica.Console.Configuration;

/// <summary>Sezione <c>Anthropic</c> della configurazione, validata solo dai comandi che usano l'AI.</summary>
public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";
    public const string ApiKeyEnvironmentKey = "Anthropic__ApiKey";

    public string? ApiKey { get; set; }

    public string? Model { get; set; }

    public int MaxTokens { get; set; }
}

public sealed class AnthropicOptionsValidator : IValidateOptions<AnthropicOptions>
{
    public ValidateOptionsResult Validate(string? name, AnthropicOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            failures.Add($"Chiave API Anthropic mancante: imposta {AnthropicOptions.ApiKeyEnvironmentKey} " +
                         "(variabile d'ambiente) oppure Anthropic:ApiKey nei user-secrets.");

        if (string.IsNullOrWhiteSpace(options.Model))
            failures.Add("Modello Anthropic mancante: imposta Anthropic__Model.");

        if (options.MaxTokens <= 0)
            failures.Add("Anthropic__MaxTokens deve essere maggiore di zero.");

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
