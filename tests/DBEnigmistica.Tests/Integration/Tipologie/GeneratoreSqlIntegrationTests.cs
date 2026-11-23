using DBEnigmistica.Tests.Unit.Tipologie;
using DBEnigmistica.Tests.Unit.Tipologie.Generatore;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Generatore;
using FluentAssertions;
using Npgsql;

namespace DBEnigmistica.Tests.Integration.Tipologie;

/// <summary>L'SQL generato si applica su PostgreSQL 18 e non crea nulla oltre alle tabelle attese.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class GeneratoreSqlIntegrationTests : IntegrationTestBase
{
    private const string ContaTabelle =
        "SELECT count(*) FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace WHERE c.relkind IN ('r', 'p') AND n.nspname NOT IN ('pg_catalog', 'information_schema', 'pg_toast')";

    private const string ContaOggetti =
        "SELECT (SELECT count(*) FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace WHERE n.nspname NOT IN ('pg_catalog', 'information_schema'))"
        + " + (SELECT count(*) FROM pg_trigger WHERE NOT tgisinternal)";

    public GeneratoreSqlIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private static async Task<long> ScalareAsync(NpgsqlConnection connessione, NpgsqlTransaction? transazione, string sql)
    {
        await using var comando = new NpgsqlCommand(sql, connessione, transazione);
        return (long)(await comando.ExecuteScalarAsync())!;
    }

    private static async Task EseguiAsync(NpgsqlConnection connessione, NpgsqlTransaction transazione, string sql)
    {
        await using var comando = new NpgsqlCommand(sql, connessione, transazione);
        await comando.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task L_sql_delle_Tipologie_reali_si_applica_in_una_transazione()
    {
        var catalogo = new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella);
        await using var connessione = new NpgsqlConnection(ConnectionString);
        await connessione.OpenAsync();
        await using var transazione = await connessione.BeginTransactionAsync();
        var prima = await ScalareAsync(connessione, transazione, ContaTabelle);

        var attese = 0;
        foreach (var file in GeneratoreSql.GeneraTutti(catalogo.Descrittori))
        {
            await EseguiAsync(connessione, transazione, file.Contenuto);
            var d = catalogo.Descrittori.Single(x => x.Id == file.IdTipologia && x.Versione == file.Versione);
            attese += 1 + d.Elenchi.Count;
        }

        (await ScalareAsync(connessione, transazione, ContaTabelle)).Should().Be(prima + attese);
        await transazione.RollbackAsync();
    }

    [Fact]
    public async Task Il_fuzz_accettato_si_applica_e_non_crea_altro_che_le_tabelle_attese()
    {
        await using var connessione = new NpgsqlConnection(ConnectionString);
        await connessione.OpenAsync();
        var tabelleBase = await ScalareAsync(connessione, null, ContaTabelle);
        var oggettiBase = await ScalareAsync(connessione, null, ContaOggetti);

        // Ogni decimo descrittore accettato (il test intero con 10.000 descrittori durerebbe minuti): campione deterministico.
        var applicati = 0;
        var accettati = 0;
        foreach (var d in FuzzDescrittori.Genera())
        {
            foreach (var schema in new[] { SchemaTipologia.Riviste, SchemaTipologia.Catalogo })
            {
                FileSql file;
                try
                {
                    file = GeneratoreSql.Genera(d, schema);
                }
                catch (DescrittoreNonGenerabileException)
                {
                    continue;
                }

                if (accettati++ % 10 != 0)
                    continue;

                await using var transazione = await connessione.BeginTransactionAsync();
                await EseguiAsync(connessione, transazione, file.Contenuto);

                (await ScalareAsync(connessione, transazione, ContaTabelle)).Should().Be(tabelleBase + 1 + d.Elenchi.Count, file.Percorso);
                (await ScalareAsync(connessione, transazione, ContaOggetti)).Should().Be(oggettiBase, file.Percorso);
                await transazione.RollbackAsync();
                applicati++;
            }
        }

        applicati.Should().BeGreaterThan(300);
        (await ScalareAsync(connessione, null, ContaTabelle)).Should().Be(tabelleBase);
    }
}
