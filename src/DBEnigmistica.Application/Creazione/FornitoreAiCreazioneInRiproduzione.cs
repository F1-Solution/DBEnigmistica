using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Segna il Fornitore AI di creazione che riproduce Registrazioni: nessuna chiamata, quindi le regole in bozza sono ammesse.</summary>
public interface IFornitoreAiInRiproduzione
{
}

/// <summary>Fornitore che riproduce Registrazioni e lo dichiara: lo usa l'avvio quando le Registrazioni sono in modalità <c>riproduci</c>.</summary>
public sealed class FornitoreAiCreazioneInRiproduzione(IFornitoreAiCreazione interno) : IFornitoreAiCreazione, IFornitoreAiInRiproduzione
{
    public string IdFornitore => interno.IdFornitore;

    public string Modello => interno.Modello;

    public Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default) =>
        interno.CreaAsync(richiesta, cancellationToken);
}
