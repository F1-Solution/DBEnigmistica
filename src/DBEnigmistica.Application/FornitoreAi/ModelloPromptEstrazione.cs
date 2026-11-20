using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Prompt di una pagina con il testo nativo già inserito e l'indicazione di un eventuale troncamento.</summary>
public sealed record PromptPagina(string Testo, bool TestoNativoTroncato);

/// <summary>Modello del prompt di estrazione (progetto 1.1, sezione 5): sostituzione dei segnaposti e versione del prompt.</summary>
public sealed partial class ModelloPromptEstrazione
{
    public const int MassimoTestoNativo = 50_000;
    public const string SegnapostoCatalogo = "catalogo_tipologie";
    public const string SegnapostoSchema = "schema_risposta";
    public const string SegnapostoTestoNativo = "testo_nativo";

    private const string Inizio = "<!-- inizio-modello -->";
    private const string Fine = "<!-- fine-modello -->";

    // Parti del modello tra un segnaposto del testo nativo e il successivo, con catalogo e schema già inseriti.
    private readonly string[] _parti;

    private ModelloPromptEstrazione(string[] parti, string versionePrompt, string jsonSchema)
    {
        _parti = parti;
        VersionePrompt = versionePrompt;
        JsonSchema = jsonSchema;
    }

    public string VersionePrompt { get; }

    public string JsonSchema { get; }

    /// <summary>Costruisce il modello dal file Markdown; un segnaposto sconosciuto o assente fa fallire il caricamento.</summary>
    public static ModelloPromptEstrazione Carica(string markdown, string catalogoTipologie, string schemaRisposta, string prefissoVersione = "estrazione-v1")
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(catalogoTipologie);
        ArgumentNullException.ThrowIfNull(schemaRisposta);

        var testo = ConLf(markdown);
        var da = testo.IndexOf(Inizio, StringComparison.Ordinal);
        var a = da < 0 ? -1 : testo.IndexOf(Fine, da + Inizio.Length, StringComparison.Ordinal);
        if (da < 0 || a < 0)
            throw new FormatException("Modello del prompt non valido: mancano i marcatori inizio-modello e fine-modello.");

        var modello = testo[(da + Inizio.Length)..a];
        modello = modello.StartsWith('\n') ? modello[1..] : modello;
        modello = modello.EndsWith('\n') ? modello[..^1] : modello;

        var catalogo = ConLf(catalogoTipologie);
        var schema = ConLf(schemaRisposta);
        var parti = new List<string>();
        var corrente = new StringBuilder();
        var visti = new HashSet<string>();
        var ultimo = 0;
        foreach (Match m in Segnaposto().Matches(modello))
        {
            var nome = m.Groups["nome"].Value;
            if (nome is not (SegnapostoCatalogo or SegnapostoSchema or SegnapostoTestoNativo))
                throw new FormatException($"Modello del prompt non valido: segnaposto sconosciuto {{{{{nome}}}}}.");

            visti.Add(nome);
            corrente.Append(modello, ultimo, m.Index - ultimo);
            ultimo = m.Index + m.Length;
            switch (nome)
            {
                case SegnapostoCatalogo:
                    corrente.Append(catalogo);
                    break;
                case SegnapostoSchema:
                    corrente.Append(schema);
                    break;
                default:
                    parti.Add(corrente.ToString());
                    corrente.Clear();
                    break;
            }
        }

        corrente.Append(modello, ultimo, modello.Length - ultimo);
        parti.Add(corrente.ToString());
        foreach (var richiesto in new[] { SegnapostoCatalogo, SegnapostoSchema, SegnapostoTestoNativo })
        {
            if (!visti.Contains(richiesto))
                throw new FormatException($"Modello del prompt non valido: manca il segnaposto {{{{{richiesto}}}}}.");
        }

        // La versione si calcola con il testo nativo lasciato come segnaposto.
        var conSegnaposto = string.Join("{{" + SegnapostoTestoNativo + "}}", parti);
        var impronta = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(conSegnaposto)))[..12];
        return new ModelloPromptEstrazione([.. parti], $"{prefissoVersione}-{impronta}", schema);
    }

    /// <summary>Inserisce il testo nativo: a capo LF, delimitatori neutralizzati, al massimo 50.000 caratteri.</summary>
    public PromptPagina Riempi(string? testoNativo)
    {
        var testo = Neutralizza(Neutralizza(ConLf(testoNativo ?? string.Empty), "<<<TESTO", "<<TESTO"), "TESTO>>>", "TESTO>>");

        var troncato = testo.Length > MassimoTestoNativo;
        if (troncato)
        {
            var fine = MassimoTestoNativo;
            if (char.IsHighSurrogate(testo[fine - 1]))
                fine--;
            testo = testo[..fine];
        }

        return new PromptPagina(string.Join(testo, _parti), troncato);
    }

    /// <summary>Compone la richiesta per una pagina: prompt, versione e schema sono quelli del modello.</summary>
    public RichiestaEstrazionePagina CreaRichiesta(string improntaPagina, byte[] immaginePng, int dpi, string? testoNativo)
    {
        var prompt = Riempi(testoNativo);
        return new RichiestaEstrazionePagina(improntaPagina, immaginePng, dpi, prompt.Testo, VersionePrompt, JsonSchema, prompt.TestoNativoTroncato);
    }

    // Ripete la sostituzione finché serve: "TESTO>>>>" non deve ricomporre il delimitatore.
    private static string Neutralizza(string testo, string da, string a)
    {
        while (testo.Contains(da, StringComparison.Ordinal))
            testo = testo.Replace(da, a, StringComparison.Ordinal);

        return testo;
    }

    private static string ConLf(string testo) => testo.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    [GeneratedRegex(@"\{\{(?<nome>[^{}\r\n]*)\}\}")]
    private static partial Regex Segnaposto();
}
