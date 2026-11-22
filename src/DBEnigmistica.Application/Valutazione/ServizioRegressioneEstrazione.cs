using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Parametri di una regressione; con <c>DalVivo</c> le pagine si rendono in PNG e il Fornitore AI è chiamato davvero.</summary>
public sealed record ParametriRegressione(string CartellaSet, string Fornitore, string Modello, int Dpi, bool DalVivo = false);

/// <summary><c>Regressioni</c> elenca le metriche peggiorate rispetto alla Linea di base; è vuoto anche quando la Linea di base manca.</summary>
public sealed record RisultatoRegressione(
    RapportoRegressione Rapporto,
    string Json,
    string Markdown,
    bool ConfrontoDisponibile,
    IReadOnlyList<string> Regressioni)
{
    public int ChiamateAi => Rapporto.ChiamateAi;
}

/// <summary>Misura l'estrazione su un set (riproducendo le Registrazioni o dal vivo) e la confronta con la Linea di base.</summary>
public sealed class ServizioRegressioneEstrazione(RisolutorePagineSet risolutore, IVersionePromptEstrazione versionePrompt)
{
    public const string NomeLineaDiBase = "linea-di-base.json";

    public async Task<RisultatoRegressione> EseguiAsync(
        ParametriRegressione parametri, IFornitoreAiEstrazione fornitore, CancellationToken cancellationToken = default)
    {
        var rapporto = await MisuraAsync(parametri, fornitore, cancellationToken);
        var percorso = Path.Combine(parametri.CartellaSet, NomeLineaDiBase);
        if (!File.Exists(percorso))
            return new RisultatoRegressione(rapporto, rapporto.InJson(), rapporto.InMarkdown(null), ConfrontoDisponibile: false, []);

        var regressioni = LineaDiBase.Leggi(percorso).Confronta(rapporto);
        return new RisultatoRegressione(rapporto, rapporto.InJson(), rapporto.InMarkdown(regressioni), ConfrontoDisponibile: true, regressioni);
    }

    /// <summary>La misura, senza confronto: la usa anche chi registra una Linea di base nuova.</summary>
    public async Task<RapportoRegressione> MisuraAsync(
        ParametriRegressione parametri, IFornitoreAiEstrazione fornitore, CancellationToken cancellationToken = default)
    {
        var set = LettoreSet.Leggi(parametri.CartellaSet);
        var risolte = await risolutore.RisolviAsync(set, parametri.DalVivo ? parametri.Dpi : null, cancellationToken);
        var attesi = LeggiAttesi(set, risolte);
        var descrittori = versionePrompt.Descrittori.ToDictionary(d => d.Id);

        var pagine = new List<PaginaRapporto>();
        var confronti = new List<(string Origine, ConfrontoPagina Confronto)>();
        var chiamate = 0;
        foreach (var risolta in risolte)
        {
            var pagina = risolta.Pagina;
            var atteso = attesi[pagina.Id];
            var risposta = await Chiedi(fornitore, risolta, parametri.Dpi, cancellationToken);
            chiamate += risposta.Chiamate.Count;

            // Una risposta non valida non ha Giochi: tutti gli attesi risultano mancanti.
            var valida = risposta.Esito == EsitoEstrazione.Riuscita && risposta.Json is not null;
            var metadati = risposta.Metadati ?? (risposta.Chiamate.Count > 0 ? MetadatiChiamata.Somma(risposta.Chiamate) : null);
            var confronto = ConfrontoPagina.Calcola(
                atteso.Giochi, valida ? LetturaGiochi.LeggiRisposta(risposta.Json!) : [], descrittori,
                new ConsumoPagina(metadati?.Costo, metadati?.DurataMs));
            pagine.Add(new PaginaRapporto(
                pagina.Id, pagina.Origine, atteso.InBozza, valida, confronto.Attesi, confronto.Estratti, confronto.Abbinati, confronto.Mancanti, confronto.InPiu));
            if (!atteso.InBozza)
                confronti.Add((pagina.Origine, confronto));
        }

        var gruppi = RapportoRegressione.NomiGruppi.ToDictionary(
            nome => nome,
            nome => CalcoloMetriche.Somma([.. confronti.Where(c => nome == "totale" || c.Origine == nome).Select(c => c.Confronto)]));
        return new RapportoRegressione(
            set.Nome, set.Tipo, new CondizioniRegressione(fornitore.IdFornitore, fornitore.Modello, versionePrompt.VersionePrompt, parametri.Dpi),
            chiamate, pagine, gruppi);
    }

    private static Dictionary<string, RisultatoAtteso> LeggiAttesi(SetValutazione set, IReadOnlyList<PaginaRisolta> risolte)
    {
        var attesi = new Dictionary<string, RisultatoAtteso>();
        var diverse = new List<string>();
        foreach (var risolta in risolte)
        {
            var pagina = risolta.Pagina;
            var atteso = LetturaGiochi.LeggiAtteso(Path.Combine(set.Cartella, "attesi", pagina.Id + ".json"), pagina.Id, pagina.Origine);
            if (atteso.Impronta != risolta.Impronta)
                diverse.Add($"{pagina.Id} (impronta attesa {atteso.Impronta}, calcolata {risolta.Impronta})");

            attesi[pagina.Id] = atteso;
        }

        if (diverse.Count > 0)
            throw new SetNonValidoException("L'impronta della pagina è diversa da quella del risultato atteso: " + string.Join("; ", diverse));

        return attesi;
    }

    private async Task<RispostaEstrazionePagina> Chiedi(
        IFornitoreAiEstrazione fornitore, PaginaRisolta risolta, int dpi, CancellationToken cancellationToken)
    {
        // Nella riproduzione l'immagine non serve: conta solo la chiave (impronta e versione del prompt).
        var richiesta = versionePrompt.Modello.CreaRichiesta(risolta.Impronta, risolta.Png ?? [], dpi, risolta.TestoNativo);
        RispostaEstrazionePagina risposta;
        try
        {
            risposta = await fornitore.EstraiPaginaAsync(richiesta, cancellationToken);
        }
        catch (Exception ex) when (ex is RegistrazioneMancanteException or RegistrazioneNonValidaException)
        {
            throw new ValutazioneException($"Pagina {risolta.Pagina.Id}: {ex.Message}", ex);
        }

        if (risposta.Esito is EsitoEstrazione.Riuscita or EsitoEstrazione.RispostaNonValida)
            return risposta;

        throw new ValutazioneException($"Pagina {risolta.Pagina.Id}: il Fornitore AI non ha risposto ({risposta.Esito}): {risposta.Messaggio}");
    }
}
