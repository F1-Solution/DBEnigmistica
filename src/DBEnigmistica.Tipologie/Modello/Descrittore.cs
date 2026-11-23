namespace DBEnigmistica.Tipologie.Modello;

/// <summary>Descrittore di una Tipologia, già validato contro il meta-schema (tipologie/_meta/descrittore.schema.json).</summary>
public sealed record Descrittore(
    string Id,
    int Versione,
    FamigliaTipologia Famiglia,
    string Nome,
    string Descrizione,
    RegoleTipologia Regole,
    IReadOnlyList<CampoDescrittore> Campi,
    IReadOnlyList<ElencoDescrittore> Elenchi,
    ResaDescrittore Resa,
    IReadOnlyList<NomeValidatore> Validatori)
{
    /// <summary>True se il descrittore è un prototipo (versione 0): niente SQL, niente tabelle.</summary>
    public bool EPrototipo => Versione == 0;

    /// <summary>Campo del Gioco con quel ruolo, o null.</summary>
    public CampoDescrittore? CampoConRuolo(RuoloCampo ruolo) => Campi.FirstOrDefault(c => c.Ruolo == ruolo);

    /// <summary>Elenco che contiene un campo con quel ruolo, o null.</summary>
    public ElencoDescrittore? ElencoConRuolo(RuoloCampo ruolo) => Elenchi.FirstOrDefault(e => e.Campi.Any(c => c.Ruolo == ruolo));

    /// <summary>Primo campo con quel ruolo fra i campi degli elenchi, o null.</summary>
    public CampoDescrittore? CampoDiElencoConRuolo(RuoloCampo ruolo) =>
        Elenchi.SelectMany(e => e.Campi).FirstOrDefault(c => c.Ruolo == ruolo);
}

/// <summary>Regole del gioco, bozza di un agente o approvate dal proprietario (ADR 002, punto 3).</summary>
public sealed record RegoleTipologia(
    string Testo,
    int Versione,
    StatoRegole Stato,
    string? ScrittaDa = null,
    DateOnly? ApprovateIl = null,
    string? ApprovateDa = null);

/// <summary>Campo di un Gioco o di una voce di elenco; <c>Max</c> vale solo per i testi, <c>Valori</c> solo per le enumerazioni.</summary>
public sealed record CampoDescrittore(
    string Nome,
    TipoCampo Tipo,
    string? Etichetta = null,
    string? Descrizione = null,
    bool Obbligatorio = false,
    int? Max = null,
    IReadOnlyList<string>? Valori = null,
    bool Indice = false,
    RuoloCampo? Ruolo = null);

/// <summary>Lista ripetuta di voci, che diventa una tabella figlia.</summary>
public sealed record ElencoDescrittore(
    string Nome,
    IReadOnlyList<CampoDescrittore> Campi,
    string? Etichetta = null,
    string? Descrizione = null);

/// <summary>Modello di resa del Lettore.</summary>
public sealed record ResaDescrittore(ModelloResa Modello);
