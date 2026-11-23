using DBEnigmistica.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace DBEnigmistica.Tests.Integration.Tipologie;

/// <summary>Migrazione della tabella delle migrazioni applicate delle Tipologie (R12): si applica, si annulla e rispetta i ruoli.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class MigrazioneTipologieTests : IntegrationTestBase
{
    private const string Precedente = "20261120212444_PrototipoCreazione";
    private const string Migrazione = "20261121140000_TipologieMigrazioniApplicate";
    private const string Tabella = "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'tipologie' AND table_name = 'migrazioni_applicate'";

    private readonly PostgresFixture _fixture;

    public MigrazioneTipologieTests(PostgresFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    private async Task<string> CreaDatabaseVuotoAsync()
    {
        var nome = $"vuoto_{Guid.NewGuid():N}";
        await using var connessione = new NpgsqlConnection(_fixture.ConnectionStringFor("postgres"));
        await connessione.OpenAsync();
        await using var comando = new NpgsqlCommand($"CREATE DATABASE \"{nome}\"", connessione);
        await comando.ExecuteNonQueryAsync();
        return nome;
    }

    private EnigmisticaDbContext ContestoSu(string database) =>
        new(new DbContextOptionsBuilder<EnigmisticaDbContext>().UseNpgsql(_fixture.ConnectionStringFor(database)).Options);

    private static async Task<long> ContaAsync(EnigmisticaDbContext contesto)
    {
        var connessione = (NpgsqlConnection)contesto.Database.GetDbConnection();
        if (connessione.State != System.Data.ConnectionState.Open)
            await connessione.OpenAsync();
        await using var comando = new NpgsqlCommand(Tabella, connessione);
        return (long)(await comando.ExecuteScalarAsync())!;
    }

    [Fact]
    public async Task Su_un_database_vuoto_la_migrazione_si_applica_e_si_annulla()
    {
        var database = await CreaDatabaseVuotoAsync();
        try
        {
            await using var contesto = ContestoSu(database);
            var migratore = contesto.GetService<IMigrator>();

            await migratore.MigrateAsync(Migrazione);
            (await ContaAsync(contesto)).Should().Be(1);

            await migratore.MigrateAsync(Precedente);
            (await ContaAsync(contesto)).Should().Be(0);

            await migratore.MigrateAsync(Migrazione);
            (await ContaAsync(contesto)).Should().Be(1);
        }
        finally
        {
            await _fixture.DropDatabaseAsync(database);
        }
    }

    [Fact]
    public async Task La_tabella_ha_le_colonne_attese()
    {
        var colonne = new List<string>();
        await using var connessione = new NpgsqlConnection(ConnectionString);
        await connessione.OpenAsync();
        await using var comando = new NpgsqlCommand(
            "SELECT column_name FROM information_schema.columns WHERE table_schema = 'tipologie' AND table_name = 'migrazioni_applicate' ORDER BY ordinal_position",
            connessione);
        await using var lettore = await comando.ExecuteReaderAsync();
        while (await lettore.ReadAsync())
            colonne.Add(lettore.GetString(0));

        colonne.Should().Equal("id", "tipologia", "versione", "schema", "checksum", "applicata_il");
    }

    [Fact]
    public async Task Il_ruolo_di_estrazione_legge_ma_non_scrive_e_il_database_rifiuta_dati_malformati()
    {
        (await SqlStateAsync("SELECT count(*) FROM tipologie.migrazioni_applicate", "dbe_estrazione")).Should().BeNull();
        (await SqlStateAsync(Inserimento("riviste", 32), "dbe_estrazione")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);

        (await SqlStateAsync(Inserimento("riviste", 32))).Should().BeNull();
        (await SqlStateAsync(Inserimento("riviste", 32))).Should().Be(PostgresErrorCodes.UniqueViolation);
        (await SqlStateAsync(Inserimento("altro", 32))).Should().Be(PostgresErrorCodes.CheckViolation);
        (await SqlStateAsync(Inserimento("catalogo", 16))).Should().Be(PostgresErrorCodes.CheckViolation);
    }

    private static string Inserimento(string schema, int byteChecksum) =>
        $"INSERT INTO tipologie.migrazioni_applicate (tipologia, versione, schema, checksum) VALUES ('anagramma', 1, '{schema}', decode(repeat('ab', {byteChecksum}), 'hex'))";
}
