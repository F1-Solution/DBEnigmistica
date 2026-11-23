using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Fornitore di creazione che rifiuta ogni chiamata: la generazione con AI si ferma con questo messaggio prima di creare un candidato.</summary>
public interface IFornitoreAiDisattivato
{
    string Messaggio { get; }
}

/// <summary>Fornitore usato con l'interruttore delle chiamate dal vivo spento: nessuna chiamata parte mai (D8, D1b.2-3).</summary>
public sealed class FornitoreAiCreazioneDisattivato(string messaggio) : IFornitoreAiCreazione, IFornitoreAiDisattivato
{
    public string IdFornitore => "disattivato";

    public string Modello => string.Empty;

    public string Messaggio { get; } = messaggio;

    public Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(Messaggio);
}
