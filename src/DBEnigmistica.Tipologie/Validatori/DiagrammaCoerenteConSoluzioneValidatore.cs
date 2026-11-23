using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Il diagramma ha un solo segno =, e i numeri dopo il segno sono le lettere delle parole della soluzione.</summary>
public sealed class DiagrammaCoerenteConSoluzioneValidatore : ValidatoreStrutturaleBase
{
    public override NomeValidatore Nome => NomeValidatore.DiagrammaCoerenteConSoluzione;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Diagramma, RuoloCampo.Soluzione];

    protected override void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi)
    {
        var nomeDiagramma = NomeCampo(descrittore, RuoloCampo.Diagramma);
        var testo = LetturaDati.Testo(dati, nomeDiagramma);
        if (string.IsNullOrWhiteSpace(testo))
            return;
        if (!DiagrammaLetto.TryLeggi(testo, out var diagramma, out var errore))
        {
            problemi.Add(new ProblemaValidazione(nomeDiagramma, errore));
            return;
        }

        var parole = TestoNormalizzato.Parole(LetturaDati.Testo(dati, NomeCampo(descrittore, RuoloCampo.Soluzione)));
        if (parole.Count == 0)
            return;
        var lunghezze = parole.Select(p => p.Length).ToList();
        if (!lunghezze.SequenceEqual(diagramma!.Destra))
            problemi.Add(new ProblemaValidazione(nomeDiagramma, $"Dopo il segno = il diagramma indica {string.Join(' ', diagramma.Destra)} ma le parole della soluzione hanno {string.Join(' ', lunghezze)} lettere."));
    }
}
