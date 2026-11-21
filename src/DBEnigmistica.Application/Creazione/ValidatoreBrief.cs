using System.Reflection;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Risorse incorporate della creazione: schema del brief e modelli del prompt.</summary>
public static class RisorseCreazione
{
    public static string SchemaBrief => Leggi("brief.schema.json");

    public static string PromptCreazione => Leggi("prompt-creazione-v1.md");

    private static string Leggi(string nome)
    {
        using var flusso = typeof(RisorseCreazione).Assembly.GetManifestResourceStream(nome)
                           ?? throw new InvalidOperationException($"Risorsa incorporata mancante: {nome}.");
        using var lettore = new StreamReader(flusso);
        return lettore.ReadToEnd();
    }
}

/// <summary>Valida un brief con lo schema e con i controlli del programma (progetto 1b.1, sezione 2.2).</summary>
public sealed class ValidatoreBrief(IValidatoreSchemaJson validatore, Func<string, bool> haGeneratoreAlgoritmico)
{
    private readonly Lazy<string> _schema = new(() => RisorseCreazione.SchemaBrief);

    /// <summary>Restituisce gli errori; elenco vuoto se il brief è valido per il descrittore.</summary>
    public IReadOnlyList<string> Valida(string briefJson, DescrittoreTipologia descrittore, bool haModelloPrompt)
    {
        ArgumentNullException.ThrowIfNull(briefJson);
        ArgumentNullException.ThrowIfNull(descrittore);
        var errori = validatore.Valida(briefJson, _schema.Value);
        if (errori.Count > 0)
            return errori;

        var brief = BriefCreazione.Leggi(briefJson);
        var problemi = new List<string>();
        if (brief.Tipologia != descrittore.Id)
            problemi.Add($"La Tipologia del brief ({brief.Tipologia}) non è quella del descrittore ({descrittore.Id}).");
        if (brief.VersioneTipologia != descrittore.Versione)
            problemi.Add($"La versione del brief ({brief.VersioneTipologia}) non è quella del descrittore ({descrittore.Versione}).");
        if (brief.Famiglia != descrittore.Famiglia)
            problemi.Add($"La famiglia del brief ({brief.Famiglia}) non è quella del descrittore ({descrittore.Famiglia}).");

        if (descrittore.Famiglia == ModelloPromptCreazione.FamigliaLogico)
        {
            if (!haGeneratoreAlgoritmico(descrittore.Id))
                problemi.Add($"La Tipologia {descrittore.Id} non ha un generatore.");
        }
        else if (!haModelloPrompt)
        {
            problemi.Add($"Non esiste un modello di prompt per la famiglia {descrittore.Famiglia}.");
        }

        return problemi;
    }
}
