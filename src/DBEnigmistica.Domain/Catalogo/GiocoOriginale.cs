using System.Text.RegularExpressions;
using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Pubblicazione;

namespace DBEnigmistica.Domain.Catalogo;

/// <summary>Gioco originale, con licenza o di pubblico dominio: radice dell'aggregato del Catalogo.</summary>
public sealed partial class GiocoOriginale
{
    private const string Inedito = "inedito";
    private const string Pubblicato = "pubblicato";

    private readonly List<LicenzaInEntrata> _licenze = [];
    private readonly List<Cessione> _cessioni = [];

    // Campi persistiti come colonne piatte; le prove si ricostruiscono dalle proprietà pubbliche.
    private string? _provaAutore;
    private int? _provaAnno;
    private string? _provaMotivo;
    private string _statoInedito = Inedito;
    private DateTime? _primaPubblicazioneIl;
    private string? _primaPubblicazioneLuogo;

    private GiocoOriginale()
    {
    }

    public Guid Id { get; private init; }

    public string Tipologia { get; private init; } = string.Empty;

    public int VersioneTipologia { get; private init; }

    public string? Titolo { get; private init; }

    public Origine Origine { get; private init; }

    public Guid? TitolareId { get; private set; }

    public DateTime CreatoIl { get; private init; }

    public IReadOnlyList<LicenzaInEntrata> Licenze => _licenze;

    public IReadOnlyList<Cessione> Cessioni => _cessioni;

    public ProvaPubblicoDominio? ProvaPubblicoDominio =>
        _provaAutore is null ? null : new ProvaPubblicoDominio(_provaAutore, _provaAnno!.Value, _provaMotivo!);

    public StatoInedito StatoInedito =>
        _statoInedito == Inedito
            ? StatoInedito.Inedito
            : StatoInedito.Pubblicato(new PrimaPubblicazione(_primaPubblicazioneIl!.Value, _primaPubblicazioneLuogo!));

    public static GiocoOriginale Crea(
        string tipologia, int versioneTipologia, Origine origine, DateTime creatoIl,
        string? titolo = null, Guid? titolareId = null)
    {
        if (origine is not (Origine.Originale or Origine.Licenza or Origine.PubblicoDominio))
            throw new ArgumentException(
                "Un Gioco originale non può avere origine Rivista né un'origine sconosciuta.", nameof(origine));
        if (tipologia is null || !FormatoTipologia().IsMatch(tipologia))
            throw new ArgumentException("La Tipologia deve essere un identificativo come cruciverba-classico.", nameof(tipologia));
        if (versioneTipologia < 1)
            throw new ArgumentException("La versione della Tipologia parte da 1.", nameof(versioneTipologia));

        return new GiocoOriginale
        {
            Id = Guid.CreateVersion7(),
            Tipologia = tipologia,
            VersioneTipologia = versioneTipologia,
            Titolo = Controlli.TestoFacoltativo(titolo, nameof(titolo), 200),
            Origine = origine,
            TitolareId = titolareId is null ? null : Controlli.Id(titolareId.Value, nameof(titolareId)),
            CreatoIl = Controlli.Utc(creatoIl, nameof(creatoIl))
        };
    }

    public void AssegnaTitolare(Guid titolareId) => TitolareId = Controlli.Id(titolareId, nameof(titolareId));

    public void RegistraProvaPubblicoDominio(ProvaPubblicoDominio prova)
    {
        ArgumentNullException.ThrowIfNull(prova);
        if (Origine != Origine.PubblicoDominio)
            throw new InvalidOperationException("La prova di pubblico dominio vale solo per i Giochi di pubblico dominio.");
        (_provaAutore, _provaAnno, _provaMotivo) = (prova.Autore, prova.Anno, prova.Motivo);
    }

    public void AggiungiLicenza(Guid licenzianteId, string estremiAccordo, Periodo periodo) =>
        _licenze.Add(LicenzaInEntrata.Crea(Id, licenzianteId, estremiAccordo, periodo));

    public void AggiungiCessione(
        Guid clienteId, DateTime stipulataIl, Periodo periodo, bool esclusiva, Territorio territorio, bool comeInedito)
    {
        var cessione = Cessione.Crea(Id, clienteId, stipulataIl, periodo, esclusiva, territorio, comeInedito);
        if (_cessioni.Any(cessione.InConflittoEsclusivoCon))
            throw new InvalidOperationException(
                "Esiste già una Cessione esclusiva che si sovrappone per periodo e territorio.");
        _cessioni.Add(cessione);
    }

    public void RegistraPrimaPubblicazione(PrimaPubblicazione prima)
    {
        var nuovo = StatoInedito.Pubblica(prima);
        (_statoInedito, _primaPubblicazioneIl, _primaPubblicazioneLuogo) =
            (Pubblicato, nuovo.Prima!.Il, nuovo.Prima.Luogo);
    }

    /// <summary>Fotografia del Gioco per la PolicyDiPubblicazione.</summary>
    public CandidatoPubblicazione ComeCandidato(DateTime istante, bool approvato, Territorio territorioDiPubblicazione) =>
        new(Origine, _licenze.ToArray(), ProvaPubblicoDominio, _cessioni.ToArray(),
            StatoInedito, approvato, Controlli.Utc(istante, nameof(istante)), territorioDiPubblicazione);

    [GeneratedRegex("^[a-z][a-z0-9-]{1,60}$")]
    private static partial Regex FormatoTipologia();
}
