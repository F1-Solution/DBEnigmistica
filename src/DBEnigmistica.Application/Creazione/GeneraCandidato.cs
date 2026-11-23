using System.Text.Json;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Registro;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Esito di un clic su Genera: i candidati creati, l'ultimo dei quali è valido a meno che la generazione si sia fermata.</summary>
public sealed record EsitoGenerazione(IReadOnlyList<Candidato> Candidati, string? Messaggio)
{
    public Candidato? Valido => Candidati.LastOrDefault(c => c.Stato == StatoCandidato.DaRivedere);
}

/// <summary>Genera un candidato, con l'AI o con un generatore algoritmico, e lo salva (progetto 1b.1, sezione 4.2).</summary>
public sealed class GeneraCandidato(
    ICatalogoTipologie catalogo,
    ValidatoreBrief validatoreBrief,
    ModelloPromptCreazione modello,
    IFornitoreAiCreazione fornitore,
    IValidatoreSchemaJson validatoreSchema,
    IGeneratoriAlgoritmici generatori,
    ISorgenteSemi semi,
    ICandidatiRepository candidati,
    IRegistroEventi registro,
    ITransazionePrototipo transazione,
    TimeProvider? tempo = null,
    bool consentiRegoleInBozza = false)
{
    /// <summary>Candidati non validi per lo schema rigenerati da soli per ogni clic, compreso il primo tentativo.</summary>
    public const int TentativiMassimi = 3;

    private readonly TimeProvider _tempo = tempo ?? TimeProvider.System;

    public async Task<EsitoGenerazione> EseguiAsync(string briefJson, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(briefJson);
        var (descrittore, brief) = await PreparaAsync(briefJson, cancellationToken);
        if (descrittore.Famiglia != ModelloPromptCreazione.FamigliaLogico && fornitore is IFornitoreAiDisattivato disattivato)
            throw new RichiestaNonValidaException(disattivato.Messaggio);

        var creati = new List<Candidato>();
        for (var tentativo = 1; tentativo <= TentativiMassimi; tentativo++)
        {
            var candidato = await UnoAsync(descrittore, brief, cancellationToken);
            creati.Add(candidato);
            if (candidato.Stato != StatoCandidato.NonValidoSchema)
                return new EsitoGenerazione(creati, null);
        }

        return new EsitoGenerazione(
            creati, $"Dopo {TentativiMassimi} tentativi nessun candidato è valido per lo schema: riprova o cambia il brief.");
    }

    private async Task<(DescrittoreTipologia Descrittore, BriefCreazione Brief)> PreparaAsync(string briefJson, CancellationToken ct)
    {
        BriefCreazione brief;
        try
        {
            using var _ = JsonDocument.Parse(briefJson);
            var schemaErrori = validatoreSchema.Valida(briefJson, RisorseCreazione.SchemaBrief);
            if (schemaErrori.Count > 0)
                throw new RichiestaNonValidaException(schemaErrori);
            brief = BriefCreazione.Leggi(briefJson);
        }
        catch (JsonException ex)
        {
            throw new RichiestaNonValidaException($"Il brief non è un JSON valido: {ex.Message}");
        }

        var descrittore = await catalogo.TrovaAsync(brief.Tipologia, brief.VersioneTipologia, ct)
                          ?? throw new NonTrovatoCreazioneException($"Tipologia {brief.Tipologia} versione {brief.VersioneTipologia} non trovata.");
        var errori = validatoreBrief.Valida(briefJson, descrittore, modello.HaModello(descrittore.Famiglia));
        if (errori.Count > 0)
            throw new RichiestaNonValidaException(errori);

        return (descrittore, brief);
    }

    private async Task<Candidato> UnoAsync(DescrittoreTipologia descrittore, BriefCreazione briefBase, CancellationToken ct)
    {
        var algoritmico = descrittore.Famiglia == ModelloPromptCreazione.FamigliaLogico;
        if (!algoritmico && fornitore is ILimiteChiamateAi { LimiteRaggiunto: true })
            throw new LimiteEsecuzioneRaggiuntoException();

        // Variante, schema delle caselle, prompt e riga del candidato nascono sotto lo stesso lucchetto: due clic non condividono la variante.
        var preparato = await transazione.EseguiAsync(() => CreaRigaAsync(descrittore, briefBase, algoritmico, ct), ct);
        var salvato = preparato.Candidato;

        try
        {
            return algoritmico
                ? await GeneraAlgoritmoAsync(salvato, descrittore, preparato.Brief, ct)
                : await GeneraAiAsync(salvato, preparato.Richiesta!, ct);
        }
        catch (Exception ex)
        {
            await SegnaErroreAsync(salvato, ex);
            throw;
        }
    }

    private sealed record RigaPreparata(Candidato Candidato, BriefCreazione Brief, SchemaCaselle? Caselle, RichiestaCreazione? Richiesta);

    private async Task<RigaPreparata> CreaRigaAsync(DescrittoreTipologia descrittore, BriefCreazione briefBase, bool algoritmico, CancellationToken ct)
    {
        await candidati.BloccaGenerazioneAsync(ct);
        var esistenti = await candidati.BriefDellaTipologiaAsync(descrittore.Id, ct);
        var chiave = briefBase.SenzaVarianteJson();
        var variante = 1 + esistenti.Count(b => BriefCreazione.Leggi(b).SenzaVarianteJson() == chiave);
        var brief = briefBase.ConVariante(variante);

        // In riproduzione il seme dipende dal brief: lo stesso brief dà lo stesso schema, quindi la stessa richiesta e la stessa Registrazione.
        var seme = !algoritmico && fornitore is IFornitoreAiInRiproduzione ? brief.SemeDiRiproduzione() : semi.Prossimo();
        SchemaCaselle? caselle = null;
        if (descrittore.Famiglia == "cruciverba")
            caselle = generatori.GeneraSchemaCaselle(brief.Dimensioni!.Righe, seme);

        RichiestaCreazione? richiesta = null;
        if (!algoritmico)
        {
            try
            {
                richiesta = modello.Compila(descrittore, brief, caselle, consentiRegoleInBozza || fornitore is IFornitoreAiInRiproduzione);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                throw new RichiestaNonValidaException(ex.Message);
            }
        }

        var nuovo = new Candidato
        {
            Id = Guid.CreateVersion7(),
            Tipologia = descrittore.Id,
            VersioneTipologia = descrittore.Versione,
            Famiglia = descrittore.Famiglia,
            GeneratoIl = _tempo.GetUtcNow().UtcDateTime,
            Modo = algoritmico ? Candidato.ModoAlgoritmo : Candidato.ModoAi,
            Brief = brief.ToJson(),
            Seme = algoritmico ? seme : caselle?.Seme,
            VersioneGeneratore = algoritmico ? generatori.VersioneGeneratore(descrittore.Id) : caselle?.VersioneGeneratore,
            SchemaCaselle = caselle?.ToJson(),
            VersionePrompt = richiesta?.VersionePrompt,
            ImprontaRichiesta = richiesta?.ImprontaByte(),
            Stato = StatoCandidato.InGenerazione,
        };
        return new RigaPreparata(await candidati.AggiungiAsync(nuovo, ct), brief, caselle, richiesta);
    }

    // Il candidato non deve restare in generazione, nemmeno se la richiesta è stata annullata: l'errore si salva senza il token annullato.
    private async Task SegnaErroreAsync(Candidato c, Exception ex)
    {
        var messaggio = ex is OperationCanceledException ? "Generazione annullata." : Limita(ex.Message);
        try
        {
            candidati.ScartaModifiche();
            await candidati.AggiornaAsync(c with { Stato = StatoCandidato.ErroreGenerazione, Messaggio = messaggio }, CancellationToken.None);
        }
        catch (Exception)
        {
            // Se anche il salvataggio dell'errore fallisce (database non raggiungibile) resta l'eccezione originale;
            // il candidato rimasto in generazione è riparato al prossimo avvio dell'host.
        }
    }

    private async Task<Candidato> GeneraAlgoritmoAsync(Candidato c, DescrittoreTipologia d, BriefCreazione brief, CancellationToken ct)
    {
        var risultato = generatori.Genera(d.Id, brief, c.Seme!.Value);
        var schema = DerivatoreSchemaCreazione.Deriva(d, brief, null);
        var errori = validatoreSchema.Valida(risultato.ContenutoJson, schema);
        var aggiornato = errori.Count == 0
            ? c with { Seme = risultato.Seme, VersioneGeneratore = risultato.VersioneGeneratore, Stato = StatoCandidato.DaRivedere, ContenutoGenerato = risultato.ContenutoJson }
            : c with { Seme = risultato.Seme, VersioneGeneratore = risultato.VersioneGeneratore, Stato = StatoCandidato.NonValidoSchema, Messaggio = Limita(string.Join("; ", errori)) };
        await candidati.AggiornaAsync(aggiornato, ct);
        return aggiornato;
    }

    private async Task<Candidato> GeneraAiAsync(
        Candidato c, RichiestaCreazione richiesta, CancellationToken ct)
    {
        var risposta = await fornitore.CreaAsync(richiesta, ct);
        var stato = risposta.Esito switch
        {
            EsitoEstrazione.Riuscita => StatoCandidato.DaRivedere,
            EsitoEstrazione.RispostaNonValida => StatoCandidato.NonValidoSchema,
            _ => StatoCandidato.ErroreGenerazione,
        };

        // Il contenuto resta esattamente quello del modello: gli incroci sbagliati di un cruciverba valido per lo schema non lo scartano (D1b-3)
        // e si calcolano alla lettura (`RevisioneCandidati.ControlliAsync`), mentre l'accettazione li rifiuta.
        var contenuto = stato == StatoCandidato.DaRivedere ? risposta.Json : null;

        var aggiornato = c with
        {
            Stato = stato,
            ContenutoGenerato = contenuto,
            Messaggio = stato == StatoCandidato.DaRivedere ? null : Limita(risposta.Messaggio),
        };

        // La risposta dell'AI c'è già: righe, eventi e stato si salvano anche se la richiesta è stata annullata nel frattempo.
        await transazione.EseguiAsync(
            async () =>
            {
                await candidati.AggiungiChiamateAsync(c.Id, risposta.Chiamate, CancellationToken.None);
                foreach (var chiamata in risposta.Chiamate)
                    await registro.AggiungiAsync(EventiCreazione.ChiamataAi(chiamata, c), CancellationToken.None);
                await candidati.AggiornaAsync(aggiornato, CancellationToken.None);
                return true;
            },
            CancellationToken.None);
        return aggiornato;
    }

    private static string? Limita(string? messaggio) =>
        messaggio is { Length: > 2000 } ? messaggio[..2000] : messaggio;
}
