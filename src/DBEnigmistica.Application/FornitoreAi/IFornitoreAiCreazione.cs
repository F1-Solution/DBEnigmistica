namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Porta del Fornitore AI per la creazione: una chiamata produce un candidato.</summary>
public interface IFornitoreAiCreazione
{
    string IdFornitore { get; }

    string Modello { get; }

    Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default);
}
