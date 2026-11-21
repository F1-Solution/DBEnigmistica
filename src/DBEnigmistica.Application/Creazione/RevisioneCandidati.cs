using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Registro;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Dati di un esito di revisione: il contenuto finale è facoltativo (senza, vale quello già salvato o quello generato).</summary>
public sealed record EsitoRichiesto(
    string Esito,
    string? MotivoScarto = null,
    string? NotaScarto = null,
    string? ContenutoFinale = null,
    long? TempoAttivoMs = null,
    DateTime? UltimaAttivitaIl = null);

/// <summary>Revisione dei candidati in ordine di generazione, senza salti (D1b-3): leggi il prossimo, salva, accetta, scarta.</summary>
public sealed class RevisioneCandidati(
    ICatalogoTipologie catalogo,
    IValidatoreSchemaJson validatore,
    ICandidatiRepository candidati,
    IRegistroEventi registro,
    ITransazionePrototipo transazione,
    TimeProvider? tempo = null)
{
    public const string EsitoAccetta = "accettato";
    public const string EsitoScarta = "scartato";
    public const int MassimoNota = 1000;
    public static readonly TimeSpan AttesaInGenerazione = TimeSpan.FromMinutes(10);

    private readonly TimeProvider _tempo = tempo ?? TimeProvider.System;

    /// <summary>Il prossimo candidato valido della Tipologia; la prima lettura avvia la revisione. <c>null</c> se non ce ne sono.</summary>
    public Task<Candidato?> ProssimoAsync(string tipologia, CancellationToken cancellationToken = default) =>
        transazione.EseguiAsync(
            async () =>
            {
                while (true)
                {
                    var prossimo = await candidati.ProssimoDaRivedereAsync(tipologia, cancellationToken);
                    if (prossimo is null)
                        return null;

                    // Con il lucchetto di riga si rilegge lo stato: un esito concluso nel frattempo non si sovrascrive.
                    await candidati.BloccaAsync(prossimo.Id, cancellationToken);
                    var fresco = await candidati.TrovaAsync(prossimo.Id, cancellationToken);
                    if (fresco is { Stato: StatoCandidato.DaRivedere or StatoCandidato.InRevisione })
                        return await AvviaRevisioneAsync(fresco, cancellationToken);
                }
            },
            cancellationToken);

    /// <summary>Salva la versione finale (valida per lo schema) con le differenze dal generato.</summary>
    public Task<Candidato> SalvaVersioneAsync(Guid id, string contenutoFinale, CancellationToken cancellationToken = default) =>
        transazione.EseguiAsync(
            async () =>
            {
                var c = await CaricaProssimoAsync(id, cancellationToken);
                var descrittore = await DescrittoreAsync(c, cancellationToken);
                ControllaContenuto(c, descrittore, contenutoFinale);
                var (differenze, _) = Differenze.Calcola(c.ContenutoGenerato!, contenutoFinale);
                var aggiornato = c with { ContenutoFinale = contenutoFinale, Differenze = differenze };
                await candidati.AggiornaAsync(aggiornato, cancellationToken);
                return aggiornato;
            },
            cancellationToken);

    /// <summary>Accetta o scarta il prossimo candidato e scrive nel registro l'evento con i soli metadati dell'esito.</summary>
    public Task<Candidato> RegistraEsitoAsync(Guid id, EsitoRichiesto richiesta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(richiesta);
        return transazione.EseguiAsync(() => ConcludiAsync(id, richiesta, cancellationToken), cancellationToken);
    }

    /// <summary>Cancella il testo delle risposte grezze di un candidato (o di tutti): SHA-256 e metadati restano.</summary>
    public Task<int> CancellaRisposteGrezzeAsync(Guid? candidatoId, CancellationToken cancellationToken = default) =>
        candidati.CancellaRisposteGrezzeAsync(candidatoId, cancellationToken);

    /// <summary>All'avvio dell'host: i candidati rimasti in generazione oltre l'attesa passano a errore.</summary>
    public Task<int> RiparaGenerazioniInterrotteAsync(CancellationToken cancellationToken = default) =>
        candidati.RiparaInGenerazioneAsync(_tempo.GetUtcNow().UtcDateTime - AttesaInGenerazione, cancellationToken);

    private async Task<Candidato> ConcludiAsync(Guid id, EsitoRichiesto r, CancellationToken ct)
    {
        if (r.Esito is not (EsitoAccetta or EsitoScarta))
            throw new RichiestaNonValidaException($"Esito sconosciuto: {r.Esito}.");
        if (r.Esito == EsitoScarta && (r.MotivoScarto is null || !MotiviScarto.Tutti.Contains(r.MotivoScarto)))
            throw new RichiestaNonValidaException($"Motivo dello scarto mancante o sconosciuto. Ammessi: {string.Join(", ", MotiviScarto.Tutti)}.");
        if (r.Esito == EsitoAccetta && r.MotivoScarto is not null)
            throw new RichiestaNonValidaException("Un candidato accettato non ha un motivo di scarto.");

        if (r.NotaScarto is { Length: > MassimoNota })
            throw new RichiestaNonValidaException($"La nota dello scarto supera {MassimoNota} caratteri.");

        var c = await CaricaProssimoAsync(id, ct);
        if (r.TempoAttivoMs is { } ms && ms < c.TempoAttivoMs)
            throw new RichiestaNonValidaException("Il tempo attivo non può diminuire.");

        var descrittore = await DescrittoreAsync(c, ct);
        var finale = r.ContenutoFinale ?? c.ContenutoFinale;
        if (r.Esito == EsitoAccetta)
        {
            finale ??= c.ContenutoGenerato;
            ControllaContenuto(c, descrittore, finale!);
        }
        else if (finale is not null && Errori(c, descrittore, finale).Count > 0)
        {
            // Uno scarto non richiede un contenuto valido: quello non valido non si salva.
            finale = c.ContenutoFinale is { } salvato && Errori(c, descrittore, salvato).Count == 0 ? salvato : null;
        }

        var (differenze, numero) = finale is null ? (null, 0) : Differenze.Calcola(c.ContenutoGenerato!, finale);
        var adesso = _tempo.GetUtcNow().UtcDateTime;
        var concluso = c with
        {
            Stato = r.Esito,
            MotivoScarto = r.Esito == EsitoScarta ? r.MotivoScarto : null,
            NotaScarto = r.Esito == EsitoScarta ? r.NotaScarto : null,
            ContenutoFinale = finale,
            Differenze = differenze,
            TempoAttivoMs = r.TempoAttivoMs ?? c.TempoAttivoMs,
            UltimaAttivitaIl = r.UltimaAttivitaIl ?? adesso,
            RevisioneConclusaIl = adesso,
        };
        await candidati.AggiornaAsync(concluso, ct);
        await registro.AggiungiAsync(EventiCreazione.EsitoRevisione(concluso, numero), ct);
        return concluso;
    }

    // Solo il prossimo candidato si può rivedere: nessuna azione sceglie o salta.
    private async Task<Candidato> CaricaProssimoAsync(Guid id, CancellationToken ct)
    {
        // Il lucchetto di riga serializza gli esiti in parallelo: chi arriva dopo rilegge lo stato già concluso e riceve il conflitto.
        await candidati.BloccaAsync(id, ct);
        var c = await candidati.TrovaAsync(id, ct) ?? throw new NonTrovatoCreazioneException("Candidato non trovato.");
        var prossimo = await candidati.ProssimoDaRivedereAsync(c.Tipologia, ct);
        if (prossimo is null || prossimo.Id != id)
            throw new ConflittoCreazioneException("Questo candidato non è il prossimo da rivedere: si rivedono in ordine di generazione.");

        return await AvviaRevisioneAsync(prossimo, ct);
    }

    private async Task<Candidato> AvviaRevisioneAsync(Candidato c, CancellationToken ct)
    {
        if (c.Stato != StatoCandidato.DaRivedere)
            return c;

        var adesso = _tempo.GetUtcNow().UtcDateTime;
        var aperto = c with { Stato = StatoCandidato.InRevisione, RevisioneIniziataIl = adesso, UltimaAttivitaIl = adesso };
        await candidati.AggiornaAsync(aperto, ct);
        return aperto;
    }

    private async Task<DescrittoreTipologia> DescrittoreAsync(Candidato c, CancellationToken ct) =>
        await catalogo.TrovaAsync(c.Tipologia, c.VersioneTipologia, ct)
        ?? throw new NonTrovatoCreazioneException($"Descrittore {c.Tipologia} versione {c.VersioneTipologia} non trovato.");

    private void ControllaContenuto(Candidato c, DescrittoreTipologia d, string contenuto)
    {
        var errori = Errori(c, d, contenuto);
        if (errori.Count > 0)
            throw new RichiestaNonValidaException(errori);
    }

    private List<string> Errori(Candidato c, DescrittoreTipologia d, string contenuto)
    {
        var caselle = c.SchemaCaselle is null ? null : SchemaCaselle.Leggi(c.SchemaCaselle);
        var schema = DerivatoreSchemaCreazione.Deriva(d, BriefCreazione.Leggi(c.Brief), caselle);
        var errori = validatore.Valida(contenuto, schema).ToList();
        if (errori.Count == 0)
            errori.AddRange(ControlliDopoLoSchema(c, d, contenuto));
        return errori;
    }

    // Controlli oltre lo schema: incroci del cruciverba e coerenza e unicità dei giochi logici.
    private static IReadOnlyList<string> ControlliDopoLoSchema(Candidato c, DescrittoreTipologia d, string contenuto)
    {
        if (c.SchemaCaselle is not null)
            return ControlliCruciverba.Verifica(d, SchemaCaselle.Leggi(c.SchemaCaselle), contenuto);

        return d.Famiglia == ModelloPromptCreazione.FamigliaLogico ? ControlliLogici.Verifica(d.Id, contenuto) : [];
    }

    /// <summary>
    /// I controlli oltre lo schema sul contenuto attuale del candidato (finale se c'è, altrimenti generato), per chi rivede:
    /// elenco vuoto se non ce ne sono. Non toccano il contenuto generato.
    /// </summary>
    public async Task<IReadOnlyList<string>> ControlliAsync(Candidato candidato, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidato);
        var contenuto = candidato.ContenutoFinale ?? candidato.ContenutoGenerato;
        if (contenuto is null)
            return [];

        var descrittore = await catalogo.TrovaAsync(candidato.Tipologia, candidato.VersioneTipologia, cancellationToken);
        return descrittore is null ? [] : ControlliDopoLoSchema(candidato, descrittore, contenuto);
    }
}
