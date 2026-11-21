namespace DBEnigmistica.AI.Configuration;

/// <summary>Valida la configurazione della creazione con le stesse regole dell'estrazione, ma sul modello e sulle Registrazioni di creazione.</summary>
public static class ValidatoreOpzioniCreazione
{
    public static IReadOnlyList<string> Valida(FornitoreAiOptions opzioni, IAmbienteFornitoreAi ambiente)
    {
        ArgumentNullException.ThrowIfNull(opzioni);
        var copia = new FornitoreAiOptions
        {
            Tipo = opzioni.Tipo,
            Modello = opzioni.Creazione.Modello,
            ClaudeCode = opzioni.ClaudeCode,
            Registrazioni = opzioni.Creazione.Registrazioni,
            LimiteChiamate = opzioni.LimiteChiamate,
        };

        var risultato = new FornitoreAiOptionsValidator(ambiente).Validate(null, copia);
        return risultato.Failures?
            .Select(f => f
                .Replace("FornitoreAi__Registrazioni__", "FornitoreAi__Creazione__Registrazioni__", StringComparison.Ordinal)
                .Replace("FornitoreAi__Modello", "FornitoreAi__Creazione__Modello", StringComparison.Ordinal))
            .ToList() ?? [];
    }
}
