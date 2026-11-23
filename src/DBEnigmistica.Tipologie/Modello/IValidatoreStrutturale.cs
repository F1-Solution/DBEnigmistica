using System.Text.Json;

namespace DBEnigmistica.Tipologie.Modello;

/// <summary>Controllo strutturale di un Gioco rispetto al suo descrittore; le implementazioni stanno in Validatori.</summary>
public interface IValidatoreStrutturale
{
    /// <summary>Nome del validatore nell'elenco chiuso del meta-schema.</summary>
    NomeValidatore Nome { get; }

    /// <summary>Ruoli che il descrittore deve avere perché il validatore sia applicabile (controllo 5 del caricatore).</summary>
    IReadOnlyList<RuoloCampo> RuoliRichiesti { get; }

    /// <summary>Verifica <paramref name="dati"/>, l'oggetto <c>dati</c> di un Gioco con le chiavi dei campi e degli elenchi del descrittore.</summary>
    EsitoValidazione Valida(Descrittore descrittore, JsonElement dati);
}

/// <summary>Un'incoerenza trovata: <c>Percorso</c> indica il campo o la voce (per esempio <c>definizioni[3].numero</c>).</summary>
public sealed record ProblemaValidazione(string Percorso, string Messaggio);

/// <summary>Esito di un validatore: valido, oppure con almeno un problema.</summary>
public sealed record EsitoValidazione
{
    /// <summary>Esito senza problemi.</summary>
    public static readonly EsitoValidazione Valido = new([]);

    private EsitoValidazione(IReadOnlyList<ProblemaValidazione> problemi)
    {
        Problemi = problemi;
    }

    /// <summary>True se non ci sono problemi.</summary>
    public bool EValido => Problemi.Count == 0;

    /// <summary>Problemi trovati, nell'ordine di scoperta.</summary>
    public IReadOnlyList<ProblemaValidazione> Problemi { get; }

    /// <summary>Esito con i problemi indicati; ne serve almeno uno.</summary>
    public static EsitoValidazione NonValido(params ProblemaValidazione[] problemi) => NonValido((IReadOnlyList<ProblemaValidazione>)problemi);

    /// <summary>Esito con i problemi indicati; ne serve almeno uno.</summary>
    public static EsitoValidazione NonValido(IReadOnlyList<ProblemaValidazione> problemi)
    {
        ArgumentNullException.ThrowIfNull(problemi);
        if (problemi.Count == 0)
            throw new ArgumentException("Un esito non valido richiede almeno un problema.", nameof(problemi));
        return new EsitoValidazione([.. problemi]);
    }
}
