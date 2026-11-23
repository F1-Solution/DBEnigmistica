using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Catalogo;
using DBEnigmistica.Tipologie.Derivazione;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Modello del prompt di estrazione, con la sua versione e lo schema derivato: il punto unico da cui la valutazione li legge.</summary>
public interface IVersionePromptEstrazione
{
    ModelloPromptEstrazione Modello { get; }

    /// <summary>I descrittori del catalogo di estrazione: ultima versione di ogni id, senza prototipi.</summary>
    IReadOnlyList<Descrittore> Descrittori { get; }

    string VersionePrompt => Modello.VersionePrompt;

    string JsonSchema => Modello.JsonSchema;
}

/// <summary>Versione del prompt dalla sezione 5.3: modello, catalogo e schema derivato dai descrittori incorporati.</summary>
public sealed class VersionePromptCatalogo : IVersionePromptEstrazione
{
    public VersionePromptCatalogo()
        : this(new CaricatoreDescrittori().CaricaRisorse().PerEstrazione(), RisorseValutazione.PromptEstrazione)
    {
    }

    public VersionePromptCatalogo(IReadOnlyList<Descrittore> descrittori, string modelloMarkdown)
    {
        Descrittori = descrittori;
        Modello = ModelloPromptEstrazione.Carica(modelloMarkdown, FormattatoreCatalogo.Formatta(descrittori), DerivatoreSchemaPagina.Deriva(descrittori));
    }

    public ModelloPromptEstrazione Modello { get; }

    public IReadOnlyList<Descrittore> Descrittori { get; }

    public string VersionePrompt => Modello.VersionePrompt;

    public string JsonSchema => Modello.JsonSchema;
}

/// <summary>Risorse incorporate della valutazione.</summary>
public static class RisorseValutazione
{
    public static string PromptEstrazione => Leggi("prompt-estrazione-v1.md");

    public static string SchemaRisultatoAtteso => Leggi("risultato-atteso.schema.json");

    private static string Leggi(string nome)
    {
        using var flusso = typeof(RisorseValutazione).Assembly.GetManifestResourceStream(nome)
                           ?? throw new InvalidOperationException($"Risorsa incorporata mancante: {nome}.");
        using var lettore = new StreamReader(flusso);
        return lettore.ReadToEnd();
    }
}
