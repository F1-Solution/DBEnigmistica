using DBEnigmistica.Domain.Catalogo;
using DBEnigmistica.Domain.Diritti;

namespace DBEnigmistica.Domain.Pubblicazione;

public enum MotivoBlocco
{
    OrigineRivista,
    OrigineNonValida,
    ProvaLicenzaMancante,
    LicenzaNonValidaAllIstante,
    ProvaPubblicoDominioMancante,
    CessioneEsclusivaInConflitto,
    PromessoComeInedito,
    NonApprovato
}

/// <summary>Il Gioco com'è al momento della valutazione.</summary>
public sealed record CandidatoPubblicazione(
    Origine Origine,
    IReadOnlyList<LicenzaInEntrata> Licenze,
    ProvaPubblicoDominio? ProvaPubblicoDominio,
    IReadOnlyList<Cessione> Cessioni,
    StatoInedito StatoInedito,
    bool Approvato,
    DateTime Istante,
    Territorio TerritorioDiPubblicazione);

public sealed record EsitoPolicy(IReadOnlyList<MotivoBlocco> Motivi)
{
    public bool Pubblicabile => Motivi.Count == 0;
}

/// <summary>Decide se un Gioco si può pubblicare e raccoglie tutti i motivi di blocco.</summary>
public static class PolicyDiPubblicazione
{
    public static EsitoPolicy Valuta(CandidatoPubblicazione c)
    {
        var motivi = new List<MotivoBlocco>();

        // Si pubblica solo un'origine dell'elenco chiuso: ogni altro valore, definito o no, blocca.
        if (c.Origine == Origine.Rivista)
            motivi.Add(MotivoBlocco.OrigineRivista);
        else if (c.Origine is not (Origine.Originale or Origine.Licenza or Origine.PubblicoDominio))
            motivi.Add(MotivoBlocco.OrigineNonValida);

        if (c.Origine == Origine.Licenza)
            AggiungiMotivoLicenza(c, motivi);

        if (c.Origine == Origine.PubblicoDominio && c.ProvaPubblicoDominio is null)
            motivi.Add(MotivoBlocco.ProvaPubblicoDominioMancante);

        if (c.Cessioni.Any(x => x.Esclusiva && x.Periodo.Contiene(c.Istante)
                                && x.Territorio.SiSovrappone(c.TerritorioDiPubblicazione)))
            motivi.Add(MotivoBlocco.CessioneEsclusivaInConflitto);

        if (c.StatoInedito.EInedito && c.Cessioni.Any(x => x.ComeInedito && !x.Periodo.TerminatoPrima(c.Istante)))
            motivi.Add(MotivoBlocco.PromessoComeInedito);

        if (!c.Approvato)
            motivi.Add(MotivoBlocco.NonApprovato);

        return new EsitoPolicy(motivi);
    }

    private static void AggiungiMotivoLicenza(CandidatoPubblicazione c, List<MotivoBlocco> motivi)
    {
        if (c.Licenze.Count == 0)
            motivi.Add(MotivoBlocco.ProvaLicenzaMancante);
        else if (!c.Licenze.Any(l => l.Periodo.Contiene(c.Istante)))
            motivi.Add(MotivoBlocco.LicenzaNonValidaAllIstante);
    }
}
