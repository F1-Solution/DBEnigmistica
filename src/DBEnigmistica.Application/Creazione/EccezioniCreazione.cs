namespace DBEnigmistica.Application.Creazione;

/// <summary>La richiesta non è valida (brief, contenuto, motivo): l'API risponde 422 con gli errori.</summary>
public sealed class RichiestaNonValidaException(IReadOnlyList<string> errori)
    : InvalidOperationException(string.Join("; ", errori))
{
    public IReadOnlyList<string> Errori { get; } = errori;

    public RichiestaNonValidaException(string errore)
        : this([errore])
    {
    }
}

/// <summary>L'operazione non è ammessa nello stato attuale (per esempio un candidato che non è il prossimo): l'API risponde 409.</summary>
public sealed class ConflittoCreazioneException(string messaggio) : InvalidOperationException(messaggio);

/// <summary>Il candidato o la Tipologia non esiste: l'API risponde 404.</summary>
public sealed class NonTrovatoCreazioneException(string messaggio) : InvalidOperationException(messaggio);
