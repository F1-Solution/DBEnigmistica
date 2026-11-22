using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>La soluzione ha le stesse lettere della prima parola, in un ordine diverso.</summary>
public sealed class AnagrammaStesseLettereValidatore : ValidatoreStrutturaleBase
{
    /// <summary>Nome del campo con la prima parola, che non ha un ruolo.</summary>
    public const string CampoPrimaParola = "prima_parola";

    public override NomeValidatore Nome => NomeValidatore.AnagrammaStesseLettere;

    public override IReadOnlyList<RuoloCampo> RuoliRichiesti { get; } = [RuoloCampo.Soluzione];

    protected override void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi)
    {
        var nomeSoluzione = NomeCampo(descrittore, RuoloCampo.Soluzione);
        var prima = TestoNormalizzato.Lettere(LetturaDati.Testo(dati, CampoPrimaParola));
        var soluzione = TestoNormalizzato.Lettere(LetturaDati.Testo(dati, nomeSoluzione));
        if (prima.Length == 0 || soluzione.Length == 0)
            return;
        if (prima == soluzione)
            problemi.Add(new ProblemaValidazione(nomeSoluzione, "La soluzione è uguale alla prima parola: le lettere devono cambiare ordine."));
        else if (!prima.Order().SequenceEqual(soluzione.Order()))
            problemi.Add(new ProblemaValidazione(nomeSoluzione, "La soluzione non ha le stesse lettere della prima parola."));
    }
}
