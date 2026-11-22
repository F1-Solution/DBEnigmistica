using DBEnigmistica.Console.Cli;
using DBEnigmistica.Infrastructure.Tipologie;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DBEnigmistica.Tests.Integration.Tipologie;

/// <summary><c>dbe migrate</c> per le Tipologie: ordine, idempotenza, checksum, SET LOCAL, ruoli e versioni 0.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class MigratoreTipologieIntegrationTests : IntegrationTestBase
{
    private static readonly string[] Tipologie = ["anagramma", "cambio-di-lettera", "cruciverba-classico", "sciarada"];

    public MigratoreTipologieIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private async Task<EsitoMigrazioniTipologie> ApplicaAsync(IEnumerable<MigrazioneSql>? migrazioni = null)
    {
        await using var connessione = new NpgsqlConnection(ConnectionString);
        return await new MigratoreTipologie().ApplicaAsync(connessione, migrazioni ?? FonteMigrazioniTipologie.DalleRisorse(), CancellationToken.None);
    }

    private static MigrazioneSql Prova(string sql, string tipologia = "prova", int versione = 1, string schema = "riviste") =>
        new($"{tipologia}/v{versione}.{schema}.sql", tipologia, versione, schema, sql);

    [Fact]
    public void La_fonte_legge_gli_SQL_incorporati_e_non_ha_versioni_0()
    {
        var fonte = FonteMigrazioniTipologie.DalleRisorse();

        fonte.Select(m => m.Percorso).Should().Equal(Tipologie.SelectMany(t => new[] { $"{t}/v1.riviste.sql", $"{t}/v1.catalogo.sql" }));
        fonte.Should().OnlyContain(m => m.Versione >= 1 && !m.Sql.Contains('\r'));
    }

    [Fact]
    public async Task Applica_le_Tipologie_registra_i_checksum_e_salta_le_v0()
    {
        var esito = await ApplicaAsync();

        esito.Applicate.Should().HaveCount(8);
        (await ScalarAsync<long>("SELECT count(*) FROM tipologie.migrazioni_applicate")).Should().Be(8);
        foreach (var tipologia in Tipologie)
        {
            var tabella = "gioco_" + tipologia.Replace('-', '_');
            (await ScalarAsync<long>($"SELECT count(*) FROM information_schema.tables WHERE table_name = '{tabella}' AND table_schema IN ('riviste', 'catalogo')")).Should().Be(2);
        }

        (await ScalarAsync<long>("SELECT count(*) FROM information_schema.tables WHERE table_name LIKE 'gioco\\_sudoku%'")).Should().Be(0);
        (await ScalarAsync<long>("SELECT count(*) FROM tipologie.migrazioni_applicate WHERE octet_length(checksum) = 32")).Should().Be(8);
    }

    [Fact]
    public async Task Una_seconda_esecuzione_non_fa_nulla()
    {
        await ApplicaAsync();

        var seconda = await ApplicaAsync();

        seconda.Applicate.Should().BeEmpty();
        seconda.GiaApplicate.Should().Be(8);
        (await ScalarAsync<long>("SELECT count(*) FROM tipologie.migrazioni_applicate")).Should().Be(8);
    }

    [Fact]
    public async Task Un_checksum_alterato_da_errore_e_non_applica_nient_altro()
    {
        await ApplicaAsync([Prova("CREATE TABLE riviste.prova_uno (a int);")]);
        var alterata = Prova("CREATE TABLE riviste.prova_uno (a int, b int);");
        var nuova = Prova("CREATE TABLE riviste.prova_due (a int);", versione: 2);

        var azione = () => ApplicaAsync([alterata, nuova]);

        (await azione.Should().ThrowAsync<ChecksumMigrazioneAlteratoException>()).Which.Percorso.Should().Be("prova/v1.riviste.sql");
        (await ScalarAsync<long>("SELECT count(*) FROM information_schema.tables WHERE table_name = 'prova_due'")).Should().Be(0);
    }

    [Fact]
    public async Task Ogni_file_gira_con_SET_LOCAL_dei_timeout_che_non_restano_sulla_connessione()
    {
        await using var connessione = new NpgsqlConnection(ConnectionString);
        await connessione.OpenAsync();
        var sql = "CREATE TABLE riviste.prova_timeout AS SELECT current_setting('lock_timeout') AS lt, current_setting('statement_timeout') AS st;";

        await new MigratoreTipologie().ApplicaAsync(connessione, [Prova(sql)], CancellationToken.None);

        (await ScalarAsync<string>("SELECT lt FROM riviste.prova_timeout")).Should().Be("5s");
        (await ScalarAsync<string>("SELECT st FROM riviste.prova_timeout")).Should().Be("1min");
        await using var comando = new NpgsqlCommand("SHOW lock_timeout", connessione);
        (await comando.ExecuteScalarAsync()).Should().Be("0");
    }

    [Fact]
    public async Task Un_file_che_fallisce_si_annulla_per_intero_e_non_si_registra()
    {
        var azione = () => ApplicaAsync([Prova("CREATE TABLE riviste.prova_rotta (a int); SELECT * FROM tabella_inesistente;")]);

        await azione.Should().ThrowAsync<PostgresException>();

        (await ScalarAsync<long>("SELECT count(*) FROM information_schema.tables WHERE table_name = 'prova_rotta'")).Should().Be(0);
        (await ScalarAsync<long>("SELECT count(*) FROM tipologie.migrazioni_applicate")).Should().Be(0);
    }

    [Fact]
    public async Task Le_tabelle_ereditano_i_permessi_dei_ruoli()
    {
        await ApplicaAsync();

        (await SqlStateAsync("SELECT count(*) FROM riviste.gioco_anagramma", "dbe_estrazione")).Should().BeNull();
        (await SqlStateAsync("DELETE FROM riviste.gioco_anagramma", "dbe_estrazione")).Should().BeNull();
        (await SqlStateAsync("SELECT count(*) FROM riviste.gioco_anagramma", "dbe_creazione")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("SELECT count(*) FROM catalogo.gioco_anagramma", "dbe_creazione")).Should().BeNull();
        (await SqlStateAsync("DELETE FROM catalogo.gioco_anagramma", "dbe_consegna")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task Il_comando_migrate_applica_le_Tipologie_e_la_seconda_volta_non_fa_nulla()
    {
        var coppie = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["ConnectionStrings:DefaultConnection"] = ConnectionString };

        async Task<(int Codice, string Uscita, string Errori)> MigrateAsync()
        {
            var o = new StringWriter();
            var e = new StringWriter();
            var codice = await CliApp.RunAsync(["migrate", "--yes"], o, e, b => b.AddInMemoryCollection(coppie));
            return (codice, o.ToString(), e.ToString());
        }

        var prima = await MigrateAsync();
        var seconda = await MigrateAsync();

        prima.Codice.Should().Be(0, prima.Errori);
        prima.Uscita.Should().Contain("8 SQL delle Tipologie applicati");
        seconda.Codice.Should().Be(0, seconda.Errori);
        seconda.Uscita.Should().Contain("SQL delle Tipologie già applicato");
        (await ScalarAsync<long>("SELECT count(*) FROM tipologie.migrazioni_applicate")).Should().Be(8);
    }

    [Fact]
    public async Task Il_comando_migrate_con_un_checksum_registrato_alterato_esce_con_errore()
    {
        var coppie = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { ["ConnectionStrings:DefaultConnection"] = ConnectionString };
        await CliApp.RunAsync(["migrate", "--yes"], new StringWriter(), new StringWriter(), b => b.AddInMemoryCollection(coppie));
        await ExecuteAsync("UPDATE tipologie.migrazioni_applicate SET checksum = decode(repeat('00', 32), 'hex') WHERE tipologia = 'anagramma' AND schema = 'riviste'");

        var errori = new StringWriter();
        var codice = await CliApp.RunAsync(["migrate", "--yes"], new StringWriter(), errori, b => b.AddInMemoryCollection(coppie));

        codice.Should().Be(ExitCodes.Error);
        errori.ToString().Should().Contain("anagramma/v1.riviste.sql").And.Contain("checksum");
    }
}
