using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>
/// Campo del brief ammesso per una Tipologia. <c>Tipo</c>: <c>testo</c> (<c>Massimo</c> = lunghezza), <c>elenco-testi</c> (<c>Massimo</c> = voci),
/// <c>intero</c> (<c>Minimo</c>, <c>Massimo</c>), <c>scelta</c> (<c>Valori</c>), <c>dimensioni-quadrate</c> (<c>Valori</c> = lati ammessi),
/// <c>dimensioni</c> (righe e colonne tra <c>Minimo</c> e <c>Massimo</c>).
/// </summary>
public sealed record CampoBrief(
    string Nome,
    string Etichetta,
    string Tipo,
    bool Obbligatorio,
    IReadOnlyList<string> Valori,
    int? Minimo = null,
    int? Massimo = null);

/// <summary>Una Tipologia del prototipo con l'esito delle regole e i campi del brief che la schermata mostra.</summary>
public sealed record TipologiaPrototipo(
    string Id,
    int Versione,
    string Famiglia,
    string Nome,
    string Modo,
    string StatoRegole,
    bool RegoleApprovate,
    bool GenerazioneConsentita,
    IReadOnlyList<CampoBrief> CampiBrief);

/// <summary>Elenca le Tipologie del prototipo, nell'ultima versione, con i campi del brief ammessi per famiglia (progetto 1b.1, sezione 2.2).</summary>
public sealed class TipologieDelPrototipo(ICatalogoTipologie catalogo, IFornitoreAiCreazione? fornitore = null)
{
    /// <summary>Le sette Tipologie che il prototipo sa generare e rivedere, in ordine di id.</summary>
    public static IReadOnlyList<string> Ids { get; } =
        ["anagramma", "crittografia-breve", "cruciverba-classico", "futoshiki", "nonogramma", "rebus", "sudoku"];

    public async Task<IReadOnlyList<TipologiaPrototipo>> EseguiAsync(CancellationToken cancellationToken = default)
    {
        var tutti = await catalogo.ElencaAsync(cancellationToken);
        return
        [
            .. Ids
                .Select(id => tutti.Where(d => d.Id == id).MaxBy(d => d.Versione))
                .OfType<DescrittoreTipologia>()
                .Select(d => Descrivi(d, fornitore)),
        ];
    }

    private static TipologiaPrototipo Descrivi(DescrittoreTipologia d, IFornitoreAiCreazione? fornitore) => new(
        d.Id,
        d.Versione,
        d.Famiglia,
        d.Nome,
        d.Famiglia == ModelloPromptCreazione.FamigliaLogico ? Candidato.ModoAlgoritmo : Candidato.ModoAi,
        d.Regole.Approvata ? "approvata" : "bozza",
        d.Regole.Approvata,
        GenerazioneConsentita(d, fornitore),
        CampiBrief(d));

    // Algoritmi sempre; con AI solo se il fornitore non è spento e le regole sono approvate o le Registrazioni si riproducono.
    private static bool GenerazioneConsentita(DescrittoreTipologia d, IFornitoreAiCreazione? fornitore) =>
        d.Famiglia == ModelloPromptCreazione.FamigliaLogico
        || (fornitore is not IFornitoreAiDisattivato && (d.Regole.Approvata || fornitore is IFornitoreAiInRiproduzione));

    private static List<CampoBrief> CampiBrief(DescrittoreTipologia d)
    {
        var logico = d.Famiglia == ModelloPromptCreazione.FamigliaLogico;
        var campi = new List<CampoBrief>();
        if (!logico)
            campi.Add(new CampoBrief("tema", "Tema", "testo", d.Famiglia == "gioco-breve", [], Massimo: 200));
        if (d.Famiglia == "crittografia")
            campi.Add(new CampoBrief("sottotipo", "Sottotipo", "scelta", true, ["sinonimica", "perifrastica"]));
        if (!logico)
            campi.Add(new CampoBrief("vincoli", "Vincoli", "elenco-testi", false, [], Massimo: 8));
        campi.Add(new CampoBrief("difficolta", "Difficoltà", "intero", true, [], 1, 5));
        if (Dimensioni(d) is { } dimensioni)
            campi.Add(dimensioni);
        return campi;
    }

    private static CampoBrief? Dimensioni(DescrittoreTipologia d) => d.Id switch
    {
        "sudoku" => Quadrate(9),
        "futoshiki" => Quadrate(5, 6, 7),
        "nonogramma" => new CampoBrief("dimensioni", "Dimensioni", "dimensioni", true, [], 10, 15),
        _ when d.Famiglia == "cruciverba" => Quadrate(9, 10, 11, 12, 13),
        _ => null,
    };

    private static CampoBrief Quadrate(params int[] lati) =>
        new("dimensioni", "Dimensioni", "dimensioni-quadrate", true, [.. lati.Select(l => l.ToString(System.Globalization.CultureInfo.InvariantCulture))]);
}
