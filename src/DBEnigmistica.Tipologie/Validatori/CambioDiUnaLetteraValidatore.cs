using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>La soluzione differisce dalla prima parola per una sola lettera, nella stessa posizione.</summary>
public sealed class CambioDiUnaLetteraValidatore : ValidatoreStrutturaleBase
{
    public override NomeValidatore Nome => NomeValidatore.CambioDiUnaLettera;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Soluzione];

    protected override void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi)
    {
        var nomeSoluzione = NomeCampo(descrittore, RuoloCampo.Soluzione);
        var prima = TestoNormalizzato.Lettere(LetturaDati.Testo(dati, AnagrammaStesseLettereValidatore.CampoPrimaParola));
        var soluzione = TestoNormalizzato.Lettere(LetturaDati.Testo(dati, nomeSoluzione));
        if (prima.Length == 0 || soluzione.Length == 0)
            return;
        if (prima.Length != soluzione.Length)
        {
            problemi.Add(new ProblemaValidazione(nomeSoluzione, "La soluzione e la prima parola devono avere lo stesso numero di lettere."));
            return;
        }

        var diverse = prima.Where((lettera, i) => lettera != soluzione[i]).Count();
        if (diverse != 1)
            problemi.Add(new ProblemaValidazione(nomeSoluzione, $"La soluzione differisce dalla prima parola per {diverse} lettere invece che per una."));
    }
}
