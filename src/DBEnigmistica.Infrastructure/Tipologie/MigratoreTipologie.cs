using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Caricamento;
using Npgsql;

namespace DBEnigmistica.Infrastructure.Tipologie;

/// <summary>SQL di una Tipologia da applicare: <c>Schema</c> è <c>riviste</c> o <c>catalogo</c>, <c>Sql</c> ha a capo LF.</summary>
public sealed record MigrazioneSql(string Percorso, string Tipologia, int Versione, string Schema, string Sql)
{
    /// <summary>SHA-256 dei byte UTF-8 (senza BOM) del file.</summary>
    public byte[] Checksum() => SHA256.HashData(new UTF8Encoding(false).GetBytes(Sql));
}

/// <summary>Un SQL già applicato risulta registrato con un checksum diverso da quello del file.</summary>
public sealed class ChecksumMigrazioneAlteratoException(string percorso)
    : Exception($"L'SQL già applicato {percorso} è stato modificato: il checksum registrato è diverso. Non si modifica un file applicato, si crea una versione nuova del descrittore.")
{
    public string Percorso { get; } = percorso;
}

/// <summary>Esito di <see cref="MigratoreTipologie.ApplicaAsync"/>.</summary>
public sealed record EsitoMigrazioniTipologie(IReadOnlyList<string> Applicate, int GiaApplicate);

/// <summary>Sorgente degli SQL: i file incorporati nell'assembly Tipologie, quindi presenti anche nell'immagine.</summary>
public static partial class FonteMigrazioniTipologie
{
    /// <summary>Gli SQL <c>&lt;id&gt;/v&lt;n&gt;.{riviste,catalogo}.sql</c> in ordine di id, versione e schema (riviste prima).</summary>
    public static IReadOnlyList<MigrazioneSql> DalleRisorse() =>
        [.. RisorseTipologie.Elenca(".sql")
            .Select(percorso => (Percorso: percorso, Corrispondenza: NomeRisorsa().Match(percorso)))
            .Where(r => r.Corrispondenza.Success)
            .Select(r => new MigrazioneSql(
                r.Percorso,
                r.Corrispondenza.Groups["id"].Value,
                int.Parse(r.Corrispondenza.Groups["n"].Value, System.Globalization.CultureInfo.InvariantCulture),
                r.Corrispondenza.Groups["schema"].Value,
                RisorseTipologie.Leggi(r.Percorso)))
            .OrderBy(m => m.Tipologia, StringComparer.Ordinal).ThenBy(m => m.Versione).ThenByDescending(m => m.Schema, StringComparer.Ordinal)];

    [GeneratedRegex(@"^(?<id>[a-z][a-z0-9]*(-[a-z0-9]+)*)/v(?<n>[0-9]{1,3})\.(?<schema>riviste|catalogo)\.sql\z")]
    private static partial Regex NomeRisorsa();
}

/// <summary>
/// Applica gli SQL delle Tipologie dopo le migrazioni EF: una transazione per file, con <c>SET LOCAL</c> dei timeout prima del file,
/// registrazione in <c>tipologie.migrazioni_applicate</c> con il checksum SHA-256. È idempotente; un checksum alterato è un errore.
/// </summary>
public sealed class MigratoreTipologie
{
    private const long ChiaveLucchetto = 4419002;
    private const string LockTimeout = "5s";
    private const string StatementTimeout = "60s";

    /// <summary>Applica le migrazioni non ancora registrate, nell'ordine ricevuto.</summary>
    public async Task<EsitoMigrazioniTipologie> ApplicaAsync(
        NpgsqlConnection connessione, IEnumerable<MigrazioneSql> migrazioni, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connessione);
        var elenco = migrazioni.ToList();
        var chiudi = connessione.State != ConnectionState.Open;
        if (chiudi)
            await connessione.OpenAsync(cancellationToken);

        try
        {
            // Prima si verificano tutti i checksum registrati: un file alterato ferma tutto prima di applicare altro.
            foreach (var migrazione in elenco)
            {
                var registrato = await LeggiChecksumAsync(connessione, null, migrazione, cancellationToken);
                if (registrato is not null && !registrato.AsSpan().SequenceEqual(migrazione.Checksum()))
                    throw new ChecksumMigrazioneAlteratoException(migrazione.Percorso);
            }

            var applicate = new List<string>();
            foreach (var migrazione in elenco)
            {
                if (await ApplicaUnaAsync(connessione, migrazione, cancellationToken))
                    applicate.Add(migrazione.Percorso);
            }

            return new EsitoMigrazioniTipologie(applicate, elenco.Count - applicate.Count);
        }
        finally
        {
            if (chiudi)
                await connessione.CloseAsync();
        }
    }

    private static async Task<bool> ApplicaUnaAsync(NpgsqlConnection connessione, MigrazioneSql migrazione, CancellationToken cancellationToken)
    {
        await using var transazione = await connessione.BeginTransactionAsync(cancellationToken);

        // Il lucchetto serializza due migrate in parallelo; poi si riguarda il registro dentro la transazione.
        await EseguiAsync(connessione, transazione, $"SELECT pg_advisory_xact_lock({ChiaveLucchetto})", cancellationToken);
        var registrato = await LeggiChecksumAsync(connessione, transazione, migrazione, cancellationToken);
        if (registrato is not null)
        {
            if (!registrato.AsSpan().SequenceEqual(migrazione.Checksum()))
                throw new ChecksumMigrazioneAlteratoException(migrazione.Percorso);
            await transazione.CommitAsync(cancellationToken);
            return false;
        }

        await EseguiAsync(connessione, transazione, $"SET LOCAL lock_timeout = '{LockTimeout}'", cancellationToken);
        await EseguiAsync(connessione, transazione, $"SET LOCAL statement_timeout = '{StatementTimeout}'", cancellationToken);
        await EseguiAsync(connessione, transazione, migrazione.Sql, cancellationToken);

        await using (var inserimento = new NpgsqlCommand(
            "INSERT INTO tipologie.migrazioni_applicate (tipologia, versione, schema, checksum) VALUES (@tipologia, @versione, @schema, @checksum)",
            connessione, transazione))
        {
            inserimento.Parameters.AddWithValue("tipologia", migrazione.Tipologia);
            inserimento.Parameters.AddWithValue("versione", migrazione.Versione);
            inserimento.Parameters.AddWithValue("schema", migrazione.Schema);
            inserimento.Parameters.AddWithValue("checksum", migrazione.Checksum());
            await inserimento.ExecuteNonQueryAsync(cancellationToken);
        }

        await transazione.CommitAsync(cancellationToken);
        return true;
    }

    private static async Task<byte[]?> LeggiChecksumAsync(
        NpgsqlConnection connessione, NpgsqlTransaction? transazione, MigrazioneSql migrazione, CancellationToken cancellationToken)
    {
        await using var comando = new NpgsqlCommand(
            "SELECT checksum FROM tipologie.migrazioni_applicate WHERE tipologia = @tipologia AND versione = @versione AND schema = @schema",
            connessione, transazione);
        comando.Parameters.AddWithValue("tipologia", migrazione.Tipologia);
        comando.Parameters.AddWithValue("versione", migrazione.Versione);
        comando.Parameters.AddWithValue("schema", migrazione.Schema);
        return await comando.ExecuteScalarAsync(cancellationToken) as byte[];
    }

    private static async Task EseguiAsync(NpgsqlConnection connessione, NpgsqlTransaction transazione, string sql, CancellationToken cancellationToken)
    {
        await using var comando = new NpgsqlCommand(sql, connessione, transazione);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }
}
