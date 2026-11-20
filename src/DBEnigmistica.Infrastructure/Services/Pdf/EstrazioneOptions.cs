using Microsoft.Extensions.Options;

namespace DBEnigmistica.Infrastructure.Services.Pdf;

/// <summary>Sezione <c>Estrazione</c> della configurazione.</summary>
public sealed class EstrazioneOptions
{
    public const string SectionName = "Estrazione";
    public const int DpiMinimo = 150;
    public const int DpiMassimo = 300;

    public int Dpi { get; set; } = 200;

    /// <summary>Valore configurato che non è un numero intero; il validatore lo segnala.</summary>
    public string? DpiNonNumerico { get; set; }
}

public sealed class EstrazioneOptionsValidator : IValidateOptions<EstrazioneOptions>
{
    public ValidateOptionsResult Validate(string? name, EstrazioneOptions options) =>
        options.DpiNonNumerico is not null
            ? ValidateOptionsResult.Fail($"Risoluzione non valida: Estrazione:Dpi deve essere un numero intero (trovato '{options.DpiNonNumerico}').")
            : options.Dpi is < EstrazioneOptions.DpiMinimo or > EstrazioneOptions.DpiMassimo
            ? ValidateOptionsResult.Fail(
                $"Risoluzione non valida: Estrazione:Dpi deve essere tra {EstrazioneOptions.DpiMinimo} e {EstrazioneOptions.DpiMassimo} (trovato {options.Dpi}).")
            : ValidateOptionsResult.Success;
}
