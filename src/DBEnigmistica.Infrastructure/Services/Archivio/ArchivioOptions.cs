using Microsoft.Extensions.Options;

namespace DBEnigmistica.Infrastructure.Services.Archivio;

/// <summary>Sezione <c>Archivio</c> della configurazione.</summary>
public sealed class ArchivioOptions
{
    public const string SectionName = "Archivio";
    public const string EnvironmentKey = "Archivio__Cartella";

    public string? Cartella { get; set; }
}

public sealed class ArchivioOptionsValidator : IValidateOptions<ArchivioOptions>
{
    public ValidateOptionsResult Validate(string? name, ArchivioOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Cartella))
        {
            return ValidateOptionsResult.Fail(
                $"Cartella dell'Archivio mancante: imposta la variabile d'ambiente {ArchivioOptions.EnvironmentKey} " +
                "oppure la chiave Archivio:Cartella nella configurazione.");
        }

        return Path.IsPathFullyQualified(options.Cartella)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Archivio:Cartella deve essere un percorso assoluto.");
    }
}
