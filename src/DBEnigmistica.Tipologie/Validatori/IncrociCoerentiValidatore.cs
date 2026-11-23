using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Le soluzioni messe nella griglia concordano con le lettere stampate e tra loro nelle caselle di incrocio.</summary>
public sealed class IncrociCoerentiValidatore : ValidatoreStrutturaleBase
{
    public override NomeValidatore Nome => NomeValidatore.IncrociCoerenti;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Griglia, RuoloCampo.Numero, RuoloCampo.Direzione, RuoloCampo.Soluzione];

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
        var scritte = new Dictionary<(int Riga, int Colonna), char>();
        foreach (var d in DefinizioniLette.Leggi(descrittore, dati, out _))
        {
            var parola = parole.FirstOrDefault(p => p.Numero == d.Numero && p.Direzione == d.Direzione);
            var lettere = TestoNormalizzato.Lettere(d.Soluzione);
            if (parola is null || lettere.Length != parola.Lunghezza)
                continue;
            var verticale = parola.Direzione == ParolaInGriglia.Verticale;
            for (var k = 0; k < lettere.Length; k++)
            {
                var posizione = (Riga: parola.Riga + (verticale ? k : 0), Colonna: parola.Colonna + (verticale ? 0 : k));
                var stampata = griglia!.Celle[posizione.Riga - 1][posizione.Colonna - 1];
                if (stampata is >= 'A' and <= 'Z' && stampata != lettere[k])
                    problemi.Add(new ProblemaValidazione(d.PercorsoSoluzione, $"Riga {posizione.Riga}, colonna {posizione.Colonna}: la griglia ha {stampata} ma la soluzione ha {lettere[k]}."));
                else if (scritte.TryGetValue(posizione, out var altra) && altra != lettere[k])
                    problemi.Add(new ProblemaValidazione(d.PercorsoSoluzione, $"Riga {posizione.Riga}, colonna {posizione.Colonna}: un'altra soluzione ha {altra} ma questa ha {lettere[k]}."));
                else
                    scritte[posizione] = lettere[k];
            }
        }
    }
}
