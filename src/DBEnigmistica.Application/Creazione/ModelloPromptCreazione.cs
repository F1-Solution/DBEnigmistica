using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Modelli del prompt di creazione v1 e compilatore (progetto 1b.1, sezione 6).</summary>
public sealed partial class ModelloPromptCreazione
{
    public const string PrefissoVersione = "creazione-v1";
    public const string SegnapostoDescrittore = "descrittore";
    public const string SegnapostoRegole = "regole";
    public const string SegnapostoBrief = "brief";
    public const string SegnapostoSchemaCruciverba = "schema_cruciverba";
    public const string FamigliaLogico = "logico";

    private static readonly string[] Comuni = [SegnapostoDescrittore, SegnapostoRegole, SegnapostoBrief];

    private static readonly string[] Etichette = ["molto facile", "facile", "media", "difficile", "molto difficile"];

    private readonly Dictionary<string, string> _modelli;

    private ModelloPromptCreazione(Dictionary<string, string> modelli) => _modelli = modelli;

    /// <summary>Costruisce i modelli dal file Markdown; un segnaposto sconosciuto, mancante o in più fa fallire il caricamento.</summary>
    public static ModelloPromptCreazione Carica(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        var testo = ConLf(markdown);
        var modelli = new Dictionary<string, string>();
        foreach (Match m in Blocco().Matches(testo))
        {
            var famiglia = m.Groups["famiglia"].Value;
            if (famiglia != m.Groups["chiusura"].Value)
                throw new FormatException($"Modello del prompt non valido: i marcatori della famiglia {famiglia} non corrispondono.");

            var modello = m.Groups["corpo"].Value;
            ControllaSegnaposti(famiglia, modello);
            if (!modelli.TryAdd(famiglia, modello))
                throw new FormatException($"Modello del prompt non valido: famiglia {famiglia} ripetuta.");
        }

        if (modelli.Count == 0)
            throw new FormatException("Modello del prompt non valido: nessun modello trovato.");

        return new ModelloPromptCreazione(modelli);
    }

    public bool HaModello(string famiglia) => _modelli.ContainsKey(famiglia);

    /// <summary>
    /// Compila la richiesta di un candidato. Le regole in bozza si accettano solo con <paramref name="consentiBozza"/> (test).
    /// </summary>
    public RichiestaCreazione Compila(
        DescrittoreTipologia descrittore,
        BriefCreazione brief,
        SchemaCaselle? caselle,
        bool consentiBozza = false)
    {
        ArgumentNullException.ThrowIfNull(descrittore);
        ArgumentNullException.ThrowIfNull(brief);
        if (descrittore.Famiglia == FamigliaLogico)
            throw new InvalidOperationException("La famiglia logico non usa l'AI: non esiste un modello di prompt.");

        if (!_modelli.TryGetValue(descrittore.Famiglia, out var modello))
            throw new InvalidOperationException($"Nessun modello di prompt per la famiglia {descrittore.Famiglia}.");

        if (!descrittore.Regole.Approvata && !consentiBozza)
            throw new InvalidOperationException("Le regole della Tipologia sono in bozza: servono regole approvate dal proprietario.");

        if (brief.Variante is null)
            throw new InvalidOperationException("Il brief non ha la variante.");

        if (descrittore.Famiglia == "cruciverba" && caselle is null)
            throw new InvalidOperationException("Il cruciverba richiede lo schema delle caselle.");

        var briefJson = brief.ToJson();
        var testoDescrittore = TestoDescrittore(descrittore);
        var testoRegole = TestoRegole(descrittore.Regole);
        var testoBrief = TestoBrief(brief);
        var testoCaselle = caselle is null ? string.Empty : TestoSchemaCaselle(caselle);
        foreach (var ingresso in new[] { testoDescrittore, testoRegole, testoBrief, testoCaselle, briefJson })
        {
            if (ingresso.Contains('\0'))
                throw new ArgumentException("Il carattere U+0000 non è ammesso nel prompt.");
        }

        var versione = CalcolaVersione(modello, testoDescrittore, testoRegole, DerivatoreSchemaCreazione.Generico(descrittore));
        var schema = DerivatoreSchemaCreazione.Deriva(descrittore, brief, caselle);
        var prompt = Segnaposto().Replace(modello, m => m.Groups["nome"].Value switch
        {
            SegnapostoDescrittore => testoDescrittore,
            SegnapostoRegole => testoRegole,
            SegnapostoBrief => testoBrief,
            _ => testoCaselle,
        });
        return new RichiestaCreazione(descrittore.Id, descrittore.Versione, briefJson, prompt, versione, schema);
    }

    internal static string TestoDescrittore(DescrittoreTipologia d)
    {
        var testo = new StringBuilder();
        testo.Append(CultureInfo.InvariantCulture, $"Tipologia `{d.Id}`, versione {d.Versione}: {d.Nome}.\n");
        testo.Append($"Famiglia: {d.Famiglia}. Resa: {d.Resa}.\n");
        testo.Append($"Descrizione: {d.Descrizione}\n");
        testo.Append("Campi di `dati`, in quest'ordine:");
        foreach (var campo in d.Campi)
            testo.Append('\n').Append(RigaCampo(campo));

        foreach (var elenco in d.Elenchi)
        {
            testo.Append($"\nElenco `{elenco.Nome}` ({elenco.EtichettaONome})");
            if (elenco.Descrizione is not null)
                testo.Append(". ").Append(elenco.Descrizione);
            testo.Append(" Ogni voce ha:");
            foreach (var campo in elenco.Campi)
                testo.Append('\n').Append(RigaCampo(campo));
        }

        return testo.ToString();
    }

    private static string RigaCampo(CampoDescrittore c)
    {
        var tipo = c.Tipo switch
        {
            "testo" => $"testo, al massimo {c.Max} caratteri",
            "testo_lungo" => "testo lungo",
            "intero" => "numero intero",
            "decimale" => "numero decimale",
            "booleano" => "vero o falso",
            "data" => "data AAAA-MM-GG",
            "enumerazione" => "uno tra: " + string.Join(", ", c.Valori),
            "griglia" => "griglia",
            _ => throw new InvalidOperationException(DerivatoreSchemaCreazione.MessaggioImmagini),
        };
        var riga = $"- `{c.Nome}` ({c.EtichettaONome}): {tipo}";
        if (c.Obbligatorio || c.Ruolo == "soluzione")
            riga += ", obbligatorio";
        if (c.Descrizione is not null)
            riga += ". " + c.Descrizione;
        return riga;
    }

    internal static string TestoRegole(RegoleDescrittore r) =>
        r.Testo + "\n" + (r.Approvata
            ? $"Versione delle regole: {r.Versione}, approvate il {r.ApprovateIl}."
            : $"Versione delle regole: {r.Versione}, bozza non approvata (solo test).");

    internal static string TestoBrief(BriefCreazione b)
    {
        var testo = new StringBuilder("<<<BRIEF\n");
        testo.Append("Tema: ").Append(Neutralizza(b.Tema ?? "libero")).Append('\n');
        if (b.Sottotipo is not null)
            testo.Append("Sottotipo: ").Append(Neutralizza(b.Sottotipo)).Append('\n');
        if (b.Dimensioni is { } d)
            testo.Append(CultureInfo.InvariantCulture, $"Dimensioni: {d.Righe} righe e {d.Colonne} colonne\n");
        testo.Append(CultureInfo.InvariantCulture, $"Difficoltà: {b.Difficolta} su 5 ({Etichette[Math.Clamp(b.Difficolta, 1, 5) - 1]})\n");
        if (b.Vincoli.Count == 0)
        {
            testo.Append("Vincoli: nessuno\n");
        }
        else
        {
            testo.Append("Vincoli:\n");
            foreach (var vincolo in b.Vincoli)
                testo.Append("- ").Append(Neutralizza(vincolo)).Append('\n');
        }

        testo.Append(CultureInfo.InvariantCulture, $"Variante: {b.Variante}\n");
        testo.Append("BRIEF>>>");
        return testo.ToString();
    }

    internal static string TestoSchemaCaselle(SchemaCaselle s)
    {
        var testo = new StringBuilder();
        testo.Append(CultureInfo.InvariantCulture, $"Righe: {s.Righe}. Colonne: {s.Colonne}.\n");
        testo.Append("Caselle, una riga di testo per riga dello schema (`#` nera, `.` bianca):\n");
        foreach (var riga in s.Celle)
            testo.Append(riga).Append('\n');
        testo.Append("Parole orizzontali:");
        foreach (var p in s.Parole.Where(p => p.Direzione == SchemaCaselle.Orizzontale))
            testo.Append(CultureInfo.InvariantCulture, $"\n- {p.Numero} orizzontale: riga {p.Riga}, colonna {p.Colonna}, {p.Lunghezza} lettere");
        testo.Append("\nParole verticali:");
        foreach (var p in s.Parole.Where(p => p.Direzione == SchemaCaselle.Verticale))
            testo.Append(CultureInfo.InvariantCulture, $"\n- {p.Numero} verticale: riga {p.Riga}, colonna {p.Colonna}, {p.Lunghezza} lettere");
        return testo.ToString();
    }

    private static string CalcolaVersione(string modello, string descrittore, string regole, string schemaGenerico)
    {
        var m = Segnaposto().Replace(modello, x => x.Groups["nome"].Value switch
        {
            SegnapostoDescrittore => descrittore,
            SegnapostoRegole => regole,
            _ => x.Value,
        });
        var byteM = Encoding.UTF8.GetBytes(m);
        var byteS = Encoding.UTF8.GetBytes(schemaGenerico);
        var dati = new byte[byteM.Length + 1 + byteS.Length];
        byteM.CopyTo(dati, 0);
        byteS.CopyTo(dati, byteM.Length + 1);
        return $"{PrefissoVersione}-{Convert.ToHexStringLower(SHA256.HashData(dati))[..12]}";
    }

    private static void ControllaSegnaposti(string famiglia, string modello)
    {
        var attesi = famiglia == "cruciverba" ? [.. Comuni, SegnapostoSchemaCruciverba] : Comuni;
        var trovati = new List<string>();
        foreach (Match m in Segnaposto().Matches(modello))
        {
            var nome = m.Groups["nome"].Value;
            if (!attesi.Contains(nome))
                throw new FormatException($"Modello del prompt non valido: segnaposto non ammesso {{{{{nome}}}}} nella famiglia {famiglia}.");
            trovati.Add(nome);
        }

        foreach (var richiesto in attesi)
        {
            if (trovati.Count(t => t == richiesto) != 1)
                throw new FormatException($"Modello del prompt non valido: il segnaposto {{{{{richiesto}}}}} deve comparire una volta nella famiglia {famiglia}.");
        }
    }

    private static string Neutralizza(string testo)
    {
        foreach (var (da, a) in new[] { ("<<<BRIEF", "<<BRIEF"), ("BRIEF>>>", "BRIEF>>") })
        {
            while (testo.Contains(da, StringComparison.Ordinal))
                testo = testo.Replace(da, a, StringComparison.Ordinal);
        }

        return testo;
    }

    private static string ConLf(string testo) => testo.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    [GeneratedRegex(@"\{\{(?<nome>[^{}\r\n]*)\}\}")]
    private static partial Regex Segnaposto();

    [GeneratedRegex(@"<!-- inizio-modello:(?<famiglia>[a-z-]+) -->\n(?<corpo>.*?)\n<!-- fine-modello:(?<chiusura>[a-z-]+) -->", RegexOptions.Singleline)]
    private static partial Regex Blocco();
}
