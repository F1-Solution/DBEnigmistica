namespace DBEnigmistica.Tipologie.Caricamento;

/// <summary>Descrittore che non supera il meta-schema o i controlli della sezione 2.4; <c>Errori</c> ha un messaggio per ogni problema.</summary>
public sealed class DescrittoreNonValidoException : Exception
{
    public DescrittoreNonValidoException(string origine, IReadOnlyList<string> errori)
        : base($"Descrittore non valido ({origine}): {string.Join("; ", errori)}")
    {
        Origine = origine;
        Errori = errori;
    }

    /// <summary>File o nome da cui viene il descrittore.</summary>
    public string Origine { get; }

    /// <summary>Un messaggio per ogni problema, in italiano.</summary>
    public IReadOnlyList<string> Errori { get; }
}
