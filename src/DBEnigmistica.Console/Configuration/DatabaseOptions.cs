using Microsoft.Extensions.Options;

namespace DBEnigmistica.Console.Configuration;

/// <summary>Sezione <c>ConnectionStrings</c> della configurazione.</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public const string EnvironmentKey = "ConnectionStrings__DefaultConnection";

    public string? DefaultConnection { get; set; }
}

public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options) =>
        string.IsNullOrWhiteSpace(options.DefaultConnection)
            ? ValidateOptionsResult.Fail(
                $"Stringa di connessione al database mancante: imposta {DatabaseOptions.EnvironmentKey} " +
                "(variabile d'ambiente) oppure ConnectionStrings:DefaultConnection nei user-secrets.")
            : ValidateOptionsResult.Success;
}
