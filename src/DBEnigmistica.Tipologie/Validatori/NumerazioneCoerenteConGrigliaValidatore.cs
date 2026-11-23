using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>I numeri della griglia e le definizioni corrispondono alle parole della griglia, numerate in ordine di lettura.</summary>
public sealed class NumerazioneCoerenteConGrigliaValidatore : ValidatoreStrutturaleBase
{
    public override NomeValidatore Nome => NomeValidatore.NumerazioneCoerenteConGriglia;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Griglia, RuoloCampo.Numero, RuoloCampo.Direzione];

    protected override void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi)
    {
        var nomeGriglia = NomeCampo(descrittore, RuoloCampo.Griglia);
        if (LetturaDati.Elemento(dati, nomeGriglia) is not { } valore
            || !GrigliaLetta.TryLeggi(valore, verificaForma: true, out var griglia, out var errore))
        {
            problemi.Add(new ProblemaValidazione(nomeGriglia, "Griglia non leggibile: la controlla il validatore delle dimensioni."));
            return;
        }

        var parole = NumerazioneGriglia.Parole(griglia!);
        ControllaNumeri(griglia!, parole, nomeGriglia, problemi);
        ControllaDefinizioni(DefinizioniLette.Leggi(descrittore, dati, out var nomeElenco), parole, nomeElenco, problemi);
    }

    private static void ControllaNumeri(GrigliaLetta griglia, IReadOnlyList<ParolaInGriglia> parole, string nomeGriglia, List<ProblemaValidazione> problemi)
    {
        var attesi = parole.GroupBy(p => (p.Riga, p.Colonna)).ToDictionary(g => g.Key, g => g.First().Numero);
        var presenti = new HashSet<(int, int)>();
        for (var i = 0; i < griglia.Numeri.Count; i++)
        {
            var n = griglia.Numeri[i];
            var percorso = $"{nomeGriglia}.numeri[{i}]";
            if (!attesi.TryGetValue((n.Riga, n.Colonna), out var atteso))
                problemi.Add(new ProblemaValidazione(percorso, $"Riga {n.Riga}, colonna {n.Colonna}: nessuna parola inizia in questa casella."));
            else if (atteso != n.Numero)
                problemi.Add(new ProblemaValidazione(percorso, $"Riga {n.Riga}, colonna {n.Colonna}: il numero è {n.Numero} invece di {atteso}."));
            else if (!presenti.Add((n.Riga, n.Colonna)))
                problemi.Add(new ProblemaValidazione(percorso, $"Riga {n.Riga}, colonna {n.Colonna}: il numero è ripetuto."));
        }

        foreach (var (posizione, numero) in attesi.Where(a => !presenti.Contains(a.Key)))
            problemi.Add(new ProblemaValidazione($"{nomeGriglia}.numeri", $"Manca il numero {numero} alla riga {posizione.Riga}, colonna {posizione.Colonna}."));
    }

    private static void ControllaDefinizioni(IReadOnlyList<DefinizioneLetta> definizioni, IReadOnlyList<ParolaInGriglia> parole, string nomeElenco, List<ProblemaValidazione> problemi)
    {
        var viste = new HashSet<(int, string)>();
        foreach (var d in definizioni)
        {
            if (d.Numero is not { } numero || d.Direzione is not { } direzione)
                problemi.Add(new ProblemaValidazione(d.Percorso, "La definizione deve avere numero e direzione."));
            else if (!parole.Any(p => p.Numero == numero && p.Direzione == direzione))
                problemi.Add(new ProblemaValidazione(d.Percorso, $"Nessuna parola {direzione} ha il numero {numero}."));
            else if (!viste.Add((numero, direzione)))
                problemi.Add(new ProblemaValidazione(d.Percorso, $"La definizione {numero} {direzione} è ripetuta."));
        }

        foreach (var p in parole.Where(p => !viste.Contains((p.Numero, p.Direzione))))
            problemi.Add(new ProblemaValidazione(nomeElenco, $"Manca la definizione {p.Numero} {p.Direzione}."));
    }
}
