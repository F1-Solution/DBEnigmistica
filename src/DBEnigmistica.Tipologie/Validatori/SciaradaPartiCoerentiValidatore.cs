using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Le parti unite danno la parola intera, e il diagramma ne riporta le lettere.</summary>
public sealed class SciaradaPartiCoerentiValidatore : ValidatoreStrutturaleBase
{
    /// <summary>Nome del campo con le parti, che non ha un ruolo.</summary>
    public const string CampoParti = "parti";

    /// <summary>Valore provvisorio: sostituire con <c>NomeValidatore.SciaradaPartiCoerenti</c> quando il modello lo definisce (ottavo membro).</summary>
    public const NomeValidatore NomeProvvisorio = NomeValidatore.SciaradaPartiCoerenti;

    public override NomeValidatore Nome => NomeProvvisorio;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Diagramma, RuoloCampo.Soluzione];

    protected override void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi)
    {
        var nomeDiagramma = NomeCampo(descrittore, RuoloCampo.Diagramma);
        var diagramma = LeggiDiagramma(dati, nomeDiagramma, problemi);
        var parti = TestoNormalizzato.Parole(LetturaDati.Testo(dati, CampoParti));
        var soluzione = TestoNormalizzato.Lettere(LetturaDati.Testo(dati, NomeCampo(descrittore, RuoloCampo.Soluzione)));

        if (parti.Count > 0)
        {
            if (parti.Count < 2)
                problemi.Add(new ProblemaValidazione(CampoParti, "La sciarada ha almeno due parti."));
            if (soluzione.Length > 0 && string.Concat(parti) != soluzione)
                problemi.Add(new ProblemaValidazione(CampoParti, "Le parti, unite in ordine, non danno la soluzione."));
            if (diagramma is not null && !diagramma.Sinistra.SequenceEqual(parti.Select(p => p.Length)))
                problemi.Add(new ProblemaValidazione(nomeDiagramma, "Prima del segno = il diagramma non riporta le lettere delle parti."));
        }
    }

    private static DiagrammaLetto? LeggiDiagramma(JsonElement dati, string nome, List<ProblemaValidazione> problemi)
    {
        var testo = LetturaDati.Testo(dati, nome);
        if (string.IsNullOrWhiteSpace(testo))
            return null;
        if (!DiagrammaLetto.TryLeggi(testo, out var diagramma, out var errore))
        {
            problemi.Add(new ProblemaValidazione(nome, errore));
            return null;
        }

        if (diagramma!.Sinistra.Count < 2)
            problemi.Add(new ProblemaValidazione(nome, "Prima del segno = il diagramma deve riportare almeno due parti."));
        if (diagramma.Sinistra.Sum() != diagramma.Destra.Sum())
            problemi.Add(new ProblemaValidazione(nome, "Le lettere delle parti non sommano a quelle della parola intera."));
        return diagramma;
    }
}
