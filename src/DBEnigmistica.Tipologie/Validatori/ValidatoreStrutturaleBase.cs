using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Base dei validatori: controlla che i dati siano un oggetto e che il descrittore abbia i ruoli richiesti, poi delega.</summary>
public abstract class ValidatoreStrutturaleBase : IValidatoreStrutturale
{
    public abstract NomeValidatore Nome { get; }

    public abstract IReadOnlyList<RuoloCampo> RuoliRichiesti { get; }

    public EsitoValidazione Valida(Descrittore descrittore, JsonElement dati)
    {
        ArgumentNullException.ThrowIfNull(descrittore);
        var problemi = new List<ProblemaValidazione>();
        if (dati.ValueKind != JsonValueKind.Object)
            problemi.Add(new ProblemaValidazione("dati", "I dati del Gioco devono essere un oggetto."));
        foreach (var ruolo in RuoliRichiesti)
        {
            if (descrittore.CampoConRuolo(ruolo) is null && descrittore.CampoDiElencoConRuolo(ruolo) is null)
                problemi.Add(new ProblemaValidazione("descrittore", $"Il descrittore non ha il ruolo '{NomiModello.Nome(ruolo)}' richiesto dal validatore."));
        }

        if (problemi.Count == 0)
            Controlla(descrittore, dati, problemi);
        return problemi.Count == 0 ? EsitoValidazione.Valido : EsitoValidazione.NonValido(problemi);
    }

    /// <summary>Esegue il controllo su dati già verificati come oggetto; aggiunge i problemi trovati.</summary>
    protected abstract void Controlla(Descrittore descrittore, JsonElement dati, List<ProblemaValidazione> problemi);

    /// <summary>Nome del campo del Gioco o di un elenco che ha quel ruolo.</summary>
    protected static string NomeCampo(Descrittore descrittore, RuoloCampo ruolo) =>
        (descrittore.CampoConRuolo(ruolo) ?? descrittore.CampoDiElencoConRuolo(ruolo))!.Nome;
}
