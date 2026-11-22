using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Generatore;

/// <summary>Schema di destinazione dell'SQL di una Tipologia.</summary>
public enum SchemaTipologia
{
    Riviste,
    Catalogo,
}

/// <summary>SQL generato per una Tipologia: percorso relativo a <c>tipologie/</c> (per esempio <c>anagramma/v1.riviste.sql</c>) e contenuto.</summary>
public sealed record FileSql(string Percorso, string Contenuto, string IdTipologia, int Versione, SchemaTipologia Schema);

/// <summary>Il descrittore non si può tradurre in SQL sicuro.</summary>
public sealed class DescrittoreNonGenerabileException(string messaggio) : Exception(messaggio);

/// <summary>
/// Descrittore a SQL secondo la sezione 3.1 di progetto-1.1: tipi e vincoli da una lista chiusa, identificatori ricontrollati e quotati,
/// mai DEFAULT, trigger, funzioni, GRANT o ALTER. Salta la versione 0.
/// </summary>
public static partial class GeneratoreSql
{
    private const int MassimoByteIdentificatore = 63;
    private const int CaratteriConservati = 54;
    private const int MassimoCaratteriTesto = 4000;

    private static readonly Lazy<HashSet<string>> Riservati = new(CaricaParoleRiservate);

    /// <summary>Genera i file SQL di tutti i descrittori con SQL, nei due schemi, in ordine di id, versione e schema.</summary>
    public static IReadOnlyList<FileSql> GeneraTutti(IEnumerable<Descrittore> descrittori) =>
        [.. descrittori
            .Where(d => !d.EPrototipo)
            .OrderBy(d => d.Id, StringComparer.Ordinal).ThenBy(d => d.Versione)
            .SelectMany(d => new[] { SchemaTipologia.Riviste, SchemaTipologia.Catalogo }.Select(s => Genera(d, s)))];

    /// <summary>Genera il file di un descrittore per uno schema; un prototipo o un descrittore non sicuro è un errore.</summary>
    public static FileSql Genera(Descrittore d, SchemaTipologia schema)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.EPrototipo)
            throw new DescrittoreNonGenerabileException($"{d.Id}: la versione 0 è un prototipo e non ha SQL.");

        Controlla(d);

        var nomeSchema = NomeSchema(schema);
        var @base = NomiSql.Tabella(d);
        var istruzioni = new List<string> { TabellaPrincipale(d, schema, nomeSchema, @base) };
        istruzioni.AddRange(d.Elenchi.Select(e => TabellaElenco(e, nomeSchema, @base)));
        istruzioni.AddRange(Indici(d, nomeSchema, @base));

        var testo = new StringBuilder();
        testo.Append(CultureInfo.InvariantCulture, $"-- Generato da dbe types generate: {d.Id} v{d.Versione}, schema {nomeSchema}. Non modificare a mano.\n\n");
        testo.Append(string.Join("\n\n", istruzioni)).Append('\n');

        var estensione = schema == SchemaTipologia.Riviste ? "riviste" : "catalogo";
        return new FileSql($"{d.Id}/v{d.Versione}.{estensione}.sql", testo.ToString(), d.Id, d.Versione, schema);
    }

    /// <summary>Nome del file SQL relativo a <c>tipologie/</c>.</summary>
    public static string Percorso(string id, int versione, SchemaTipologia schema) =>
        $"{id}/v{versione}.{(schema == SchemaTipologia.Riviste ? "riviste" : "catalogo")}.sql";

    private static string NomeSchema(SchemaTipologia schema) => schema switch
    {
        SchemaTipologia.Riviste => "riviste",
        SchemaTipologia.Catalogo => "catalogo",
        _ => throw new ArgumentOutOfRangeException(nameof(schema), schema, "Schema non previsto."),
    };

    private static string TabellaPrincipale(Descrittore d, SchemaTipologia schema, string nomeSchema, string tabella)
    {
        var (tipoChiave, nucleo) = schema == SchemaTipologia.Riviste
            ? ("integer", "riviste.\"games\" (\"Id\")")
            : ("uuid", "catalogo.\"giochi\" (\"id\")");

        var colonne = new List<string> { $"\"gioco_id\" {tipoChiave} NOT NULL" };
        colonne.AddRange(d.Campi.Select(Colonna));

        var vincoli = new List<string>
        {
            $"CONSTRAINT {Quota(Vincolo("pk", tabella))} PRIMARY KEY (\"gioco_id\")",
            $"CONSTRAINT {Quota(Vincolo("fk", tabella, "gioco"))} FOREIGN KEY (\"gioco_id\") REFERENCES {nucleo} ON DELETE CASCADE",
        };
        vincoli.AddRange(VincoliEnumerazione(d.Campi, tabella));
        return Crea(nomeSchema, tabella, colonne, vincoli);
    }

    private static string TabellaElenco(ElencoDescrittore e, string nomeSchema, string @base)
    {
        var tabella = $"{@base}__{e.Nome}";
        var colonne = new List<string> { "\"gioco_id\" " + (nomeSchema == "riviste" ? "integer" : "uuid") + " NOT NULL", "\"posizione\" integer NOT NULL" };
        colonne.AddRange(e.Campi.Select(Colonna));

        var vincoli = new List<string>
        {
            $"CONSTRAINT {Quota(Vincolo("pk", tabella))} PRIMARY KEY (\"gioco_id\", \"posizione\")",
            $"CONSTRAINT {Quota(Vincolo("fk", tabella, "gioco"))} FOREIGN KEY (\"gioco_id\") REFERENCES {nomeSchema}.{Quota(@base)} (\"gioco_id\") ON DELETE CASCADE",
            $"CONSTRAINT {Quota(Vincolo("ck", tabella, "posizione"))} CHECK (\"posizione\" >= 0)",
        };
        vincoli.AddRange(VincoliEnumerazione(e.Campi, tabella));
        return Crea(nomeSchema, tabella, colonne, vincoli);
    }

    private static string Crea(string nomeSchema, string tabella, List<string> colonne, List<string> vincoli)
    {
        var righe = colonne.Concat(vincoli).Select(r => "    " + r);
        return $"CREATE TABLE {nomeSchema}.{Quota(tabella)} (\n{string.Join(",\n", righe)}\n);";
    }

    private static string Colonna(CampoDescrittore c) =>
        $"{Quota(c.Nome)} {TipoSql(c)}{(c.Obbligatorio ? " NOT NULL" : string.Empty)}";

    private static string TipoSql(CampoDescrittore c) => c.Tipo switch
    {
        TipoCampo.Testo => string.Create(CultureInfo.InvariantCulture, $"varchar({c.Max})"),
        TipoCampo.TestoLungo => "text",
        TipoCampo.Intero => "integer",
        TipoCampo.Decimale => "numeric(12,4)",
        TipoCampo.Booleano => "boolean",
        TipoCampo.Data => "timestamptz",
        TipoCampo.Enumerazione => "text",
        TipoCampo.Griglia => "jsonb",
        TipoCampo.Immagine => "uuid",
        _ => throw new DescrittoreNonGenerabileException($"Tipo di campo non previsto: {c.Tipo}."),
    };

    private static IEnumerable<string> VincoliEnumerazione(IReadOnlyList<CampoDescrittore> campi, string tabella) =>
        campi.Where(c => c.Tipo == TipoCampo.Enumerazione).Select(c =>
            $"CONSTRAINT {Quota(Vincolo("ck", tabella, c.Nome))} CHECK ({Quota(c.Nome)} IN ({string.Join(", ", c.Valori!.Select(v => "'" + v + "'"))}))");

    private static IEnumerable<string> Indici(Descrittore d, string nomeSchema, string @base)
    {
        foreach (var c in d.Campi.Where(c => c.Indice))
            yield return IndiceSu(nomeSchema, @base, c.Nome);

        foreach (var e in d.Elenchi)
            foreach (var c in e.Campi.Where(c => c.Indice))
                yield return IndiceSu(nomeSchema, $"{@base}__{e.Nome}", c.Nome);
    }

    private static string IndiceSu(string nomeSchema, string tabella, string colonna) =>
        $"CREATE INDEX {Quota(Vincolo("ix", tabella, colonna))} ON {nomeSchema}.{Quota(tabella)} ({Quota(colonna)});";

    // pk_<tabella>, fk_<tabella>__gioco, ck_/ix_<tabella>__<colonna>; oltre 63 byte: primi 54 caratteri, "_" e 8 esadecimali dello SHA-256.
    private static string Vincolo(string prefisso, string tabella, string? colonna = null)
    {
        var nome = colonna is null ? $"{prefisso}_{tabella}" : $"{prefisso}_{tabella}__{colonna}";
        if (Encoding.UTF8.GetByteCount(nome) <= MassimoByteIdentificatore)
            return nome;

        var impronta = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(nome)))[..8].ToLowerInvariant();
        return $"{nome[..CaratteriConservati]}_{impronta}";
    }

    private static string Quota(string identificatore) => NomiSql.Quota(identificatore);

    private static void Controlla(Descrittore d)
    {
        if (!IdValido().IsMatch(d.Id) || d.Id.Length > 30)
            throw new DescrittoreNonGenerabileException($"Id di Tipologia non ammesso: '{d.Id}'.");
        if (d.Versione is < 1 or > 999)
            throw new DescrittoreNonGenerabileException($"{d.Id}: versione fuori da 1-999.");
        if (d.Campi.Count is < 1 or > 60 || d.Elenchi.Count > 10)
            throw new DescrittoreNonGenerabileException($"{d.Id}: troppi o troppo pochi campi o elenchi.");

        Nomi(d.Id, d.Campi.Select(c => c.Nome).Concat(d.Elenchi.Select(e => e.Nome)), 40);
        ControllaCampi(d.Id, d.Campi, consentiGriglia: true);

        foreach (var e in d.Elenchi)
        {
            Nomi($"{d.Id}.{e.Nome}", e.Campi.Select(c => c.Nome), 40);
            if (e.Nome.Length > 20)
                throw new DescrittoreNonGenerabileException($"{d.Id}: il nome dell'elenco '{e.Nome}' supera 20 caratteri.");
            if (e.Campi.Count is < 1 or > 30)
                throw new DescrittoreNonGenerabileException($"{d.Id}.{e.Nome}: numero di campi non ammesso.");
            ControllaCampi($"{d.Id}.{e.Nome}", e.Campi, consentiGriglia: false);
        }
    }

    private static void Nomi(string contesto, IEnumerable<string> nomi, int massimo)
    {
        var visti = new HashSet<string>(StringComparer.Ordinal);
        foreach (var nome in nomi)
        {
            if (!Identificatore().IsMatch(nome) || nome.Length > massimo + 1 || Riservati.Value.Contains(nome))
                throw new DescrittoreNonGenerabileException($"{contesto}: nome non ammesso '{nome}'.");
            if (!visti.Add(nome))
                throw new DescrittoreNonGenerabileException($"{contesto}: nome ripetuto '{nome}'.");
        }
    }

    private static void ControllaCampi(string contesto, IReadOnlyList<CampoDescrittore> campi, bool consentiGriglia)
    {
        foreach (var c in campi)
        {
            if (!Enum.IsDefined(c.Tipo))
                throw new DescrittoreNonGenerabileException($"{contesto}.{c.Nome}: tipo non previsto.");
            if (c.Tipo == TipoCampo.Testo != c.Max.HasValue || c.Max is < 1 or > MassimoCaratteriTesto)
                throw new DescrittoreNonGenerabileException($"{contesto}.{c.Nome}: 'max' ammesso solo per i testi, da 1 a {MassimoCaratteriTesto}.");
            if (c.Tipo == TipoCampo.Griglia && !consentiGriglia)
                throw new DescrittoreNonGenerabileException($"{contesto}.{c.Nome}: una griglia non può stare in un elenco.");
            if (c.Indice && c.Tipo is not (TipoCampo.Intero or TipoCampo.Testo or TipoCampo.Enumerazione or TipoCampo.Data))
                throw new DescrittoreNonGenerabileException($"{contesto}.{c.Nome}: indice non ammesso per questo tipo.");

            var valori = c.Valori ?? [];
            if (c.Tipo == TipoCampo.Enumerazione)
            {
                if (valori.Count is < 1 or > 50 || valori.Distinct(StringComparer.Ordinal).Count() != valori.Count || valori.Any(v => !ValoreEnumerazione().IsMatch(v)))
                    throw new DescrittoreNonGenerabileException($"{contesto}.{c.Nome}: valori di enumerazione non ammessi.");
            }
            else if (valori.Count > 0)
            {
                throw new DescrittoreNonGenerabileException($"{contesto}.{c.Nome}: 'valori' ammessi solo per le enumerazioni.");
            }
        }
    }

    private static HashSet<string> CaricaParoleRiservate()
    {
        using var documento = JsonDocument.Parse(RisorseTipologie.Leggi("_meta/descrittore.schema.json"));
        return [.. documento.RootElement.GetProperty("$defs").GetProperty("nomiVietati").GetProperty("enum").EnumerateArray().Select(e => e.GetString()!)];
    }

    [GeneratedRegex(@"^[a-z][a-z0-9]*(-[a-z0-9]+)*\z")]
    private static partial Regex IdValido();

    [GeneratedRegex(@"^[a-z][a-z0-9_]{0,40}\z")]
    private static partial Regex Identificatore();

    [GeneratedRegex(@"^[a-z0-9_]{1,40}\z")]
    private static partial Regex ValoreEnumerazione();

}
