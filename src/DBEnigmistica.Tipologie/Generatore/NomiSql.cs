using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Generatore;

/// <summary>Nomi SQL delle tabelle di una Tipologia e quotatura degli identificatori: l'unico punto in cui un nome entra in una query.</summary>
public static partial class NomiSql
{
    /// <summary>Nome della tabella 1:1 (senza schema): <c>gioco_</c> e l'id con i trattini sostituiti da trattini bassi.</summary>
    public static string Tabella(Descrittore d) => "gioco_" + d.Id.Replace('-', '_');

    /// <summary>Nome della tabella figlia di un elenco (senza schema): <c>&lt;base&gt;__&lt;elenco&gt;</c>.</summary>
    public static string Tabella(Descrittore d, ElencoDescrittore elenco) => $"{Tabella(d)}__{elenco.Nome}";

    /// <summary>Nome dello schema.</summary>
    public static string Schema(SchemaTipologia schema) => schema switch
    {
        SchemaTipologia.Riviste => "riviste",
        SchemaTipologia.Catalogo => "catalogo",
        _ => throw new ArgumentOutOfRangeException(nameof(schema), schema, "Schema non previsto."),
    };

    /// <summary>Identificatore fra virgolette; solo minuscole, cifre e trattino basso, altrimenti errore.</summary>
    public static string Quota(string identificatore)
    {
        if (identificatore is null || !Sicuro().IsMatch(identificatore))
            throw new DescrittoreNonGenerabileException($"Identificatore non ammesso nell'SQL: '{identificatore}'.");
        return "\"" + identificatore + "\"";
    }

    /// <summary>Nome qualificato e quotato: <c>schema."tabella"</c>.</summary>
    public static string Qualificato(SchemaTipologia schema, string tabella) => $"{Schema(schema)}.{Quota(tabella)}";

    [GeneratedRegex(@"^[a-z][a-z0-9_]*\z")]
    private static partial Regex Sicuro();
}
