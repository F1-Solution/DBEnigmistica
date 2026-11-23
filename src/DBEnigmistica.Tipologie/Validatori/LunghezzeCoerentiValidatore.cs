using System.Text.Json;
using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Soluzioni e diagrammi delle definizioni hanno tante lettere quante le caselle della parola.</summary>
public sealed partial class LunghezzeCoerentiValidatore : ValidatoreStrutturaleBase
{
    public override NomeValidatore Nome => NomeValidatore.LunghezzeCoerenti;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Griglia, RuoloCampo.Numero, RuoloCampo.Direzione, RuoloCampo.Soluzione];

    /// <summary>Diagramma in coda alla definizione, per esempio (5) o (3,2): numeri di una o due cifre, così un anno tra parentesi non vale.</summary>
    [GeneratedRegex(@"\((\d{1,2}(?:[\s,.'’\-]+\d{1,2})*)\)\s*$")]
    private static partial Regex DiagrammaInCoda();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Numeri();

    protected override void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi)
    {
        var nomeGriglia = NomeCampo(descrittore, RuoloCampo.Griglia);
        if (LetturaDati.Elemento(dati, nomeGriglia) is not { } valore
            || !GrigliaLetta.TryLeggi(valore, verificaForma: true, out var griglia, out _))
        {
            problemi.Add(new ProblemaValidazione(nomeGriglia, "Griglia non leggibile: la controlla il validatore delle dimensioni."));
            return;
        }

        var parole = NumerazioneGriglia.Parole(griglia!);
        foreach (var d in DefinizioniLette.Leggi(descrittore, dati, out _))
        {
            var parola = parole.FirstOrDefault(p => p.Numero == d.Numero && p.Direzione == d.Direzione);
            if (parola is null)
                continue;
            var lettere = TestoNormalizzato.Lettere(d.Soluzione).Length;
            if (lettere > 0 && lettere != parola.Lunghezza)
                problemi.Add(new ProblemaValidazione(d.PercorsoSoluzione, $"La soluzione ha {lettere} lettere ma la parola {parola.Numero} {parola.Direzione} ha {parola.Lunghezza} caselle."));
            var diagramma = DiagrammaInCoda().Match(d.Testo ?? string.Empty);
            if (diagramma.Success)
            {
                var somma = Numeri().Matches(diagramma.Groups[1].Value).Sum(m => int.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture));
                if (somma != parola.Lunghezza)
                    problemi.Add(new ProblemaValidazione(d.PercorsoTesto, $"Il diagramma della definizione indica {somma} lettere ma la parola ha {parola.Lunghezza} caselle."));
            }
        }
    }
}
