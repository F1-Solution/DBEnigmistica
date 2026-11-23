using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Il diagramma ha un solo segno =, e i numeri dopo il segno sono le lettere delle parole della soluzione; nel rebus il diagramma è solo l'elenco di queste lunghezze.</summary>
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
        var rebus = descrittore.Famiglia == FamigliaTipologia.Rebus;
        DiagrammaLetto? diagramma;
        string errore;
        if (!(rebus ? DiagrammaLetto.TryLeggiSoluzione(testo, out diagramma, out errore) : DiagrammaLetto.TryLeggi(testo, out diagramma, out errore)))
        {
            problemi.Add(new ProblemaValidazione(nomeDiagramma, errore));
            return;
        }

        var parole = TestoNormalizzato.Parole(LetturaDati.Testo(dati, NomeCampo(descrittore, RuoloCampo.Soluzione)));
        if (parole.Count == 0)
            return;
        var lunghezze = parole.Select(p => p.Length).ToList();
        if (!lunghezze.SequenceEqual(diagramma!.Destra))
            problemi.Add(new ProblemaValidazione(nomeDiagramma, rebus
                ? $"Il diagramma indica {string.Join(' ', diagramma.Destra)} ma le parole della soluzione hanno {string.Join(' ', lunghezze)} lettere."
                : $"Dopo il segno = il diagramma indica {string.Join(' ', diagramma.Destra)} ma le parole della soluzione hanno {string.Join(' ', lunghezze)} lettere."));
    }
}
