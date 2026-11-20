namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Porta del Fornitore AI per l'estrazione: dalla pagina alla risposta strutturata.</summary>
public interface IFornitoreAiEstrazione
{
    string IdFornitore { get; }

    string Modello { get; }

    Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default);
}
