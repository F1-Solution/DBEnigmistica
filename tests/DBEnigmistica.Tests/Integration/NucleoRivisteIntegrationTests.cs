using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace DBEnigmistica.Tests.Integration;

/// <summary>Nucleo <c>riviste.games</c> con Tipologia, riquadro 0-1, impronte e migrazione che si ferma sui Giochi vecchi (ondata 1.2, D4).</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class NucleoRivisteIntegrationTests : IntegrationTestBase
{
    private const string MigrazionePrecedente = "20261121140000_TipologieMigrazioniApplicate";

    // Colonna del vecchio schema, composta per non lasciare il vecchio nome nel codice.
    private const string ColonnaVecchia = "Game" + "Type";

    private const string InsertGioco =
        "INSERT INTO riviste.games (\"PageId\", \"ExtractedDate\", \"HasSolution\", origine, tipologia, versione_tipologia, " +
        "stato_tipologia, riquadro_x, riquadro_y, riquadro_larghezza, riquadro_altezza, avvisi, trascrizione_proposta) " +
        "VALUES (@p, now(), false, 'rivista', 'cruciverba-classico', 1, @stato, @x, @y, @l, @a, '[]'::jsonb, @t)";

    public NucleoRivisteIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private static NpgsqlParameter[] Gioco(int pagina, string stato = "nota", decimal? x = 0.1m, decimal? y = 0.2m,
        decimal? l = 0.5m, decimal? a = 0.5m, string? trascrizione = null) =>
    [
        P("p", pagina), P("stato", stato), P("x", x), P("y", y), P("l", l), P("a", a), P("t", trascrizione)
    ];

    private async Task<int> PaginaAsync() => (await SeedMagazineWithPagesAsync()).PageIds[0];

    [Theory]
    [InlineData(-0.1, 0.2, 0.5, 0.5)]
    [InlineData(1.5, 0.2, 0.5, 0.5)]
    [InlineData(0.1, -0.00001, 0.5, 0.5)]
    [InlineData(0.1, 0.2, 1.00001, 0.5)]
    [InlineData(0.1, 0.2, 0.5, 2.0)]
    public async Task Il_database_rifiuta_un_riquadro_fuori_da_0_1(double x, double y, double l, double a)
    {
        var pagina = await PaginaAsync();

        var stato = await SqlStateAsync(InsertGioco,
            parameters: Gioco(pagina, x: (decimal)x, y: (decimal)y, l: (decimal)l, a: (decimal)a));

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Il_database_accetta_un_riquadro_ai_bordi_e_uno_assente()
    {
        var pagina = await PaginaAsync();

        (await SqlStateAsync(InsertGioco, parameters: Gioco(pagina, x: 0m, y: 0m, l: 1m, a: 1m))).Should().BeNull();
        (await SqlStateAsync(InsertGioco, parameters: Gioco(pagina, x: null, y: null, l: null, a: null))).Should().BeNull();
    }

    [Fact]
    public async Task Un_Gioco_proposto_si_salva_con_la_trascrizione_senza_tabella_dedicata()
    {
        var pagina = await PaginaAsync();

        (await SqlStateAsync(InsertGioco, parameters: Gioco(pagina, "proposta", trascrizione: "Testo inventato"))).Should().BeNull();

        (await ScalarAsync<string>("SELECT trascrizione_proposta FROM riviste.games")).Should().Be("Testo inventato");
        (await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'riviste' AND table_name IN " +
            "('crossword_games', 'anagram_games', 'logic_games', 'rebus_games')")).Should().Be(0);
    }

    [Fact]
    public async Task La_trascrizione_con_una_Tipologia_nota_viene_rifiutata()
    {
        var pagina = await PaginaAsync();

        var stato = await SqlStateAsync(InsertGioco, parameters: Gioco(pagina, "nota", trascrizione: "Testo inventato"));

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Lo_stato_della_Tipologia_ammette_solo_nota_e_proposta()
    {
        var pagina = await PaginaAsync();

        (await SqlStateAsync(InsertGioco, parameters: Gioco(pagina, "altro"))).Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Il_nucleo_non_ha_piu_le_colonne_vecchie()
    {
        var colonne = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'riviste' AND table_name = 'games' " +
            "AND column_name IN ('" + ColonnaVecchia + "', 'PositionX', 'PositionY', 'BoundingBoxWidth', 'BoundingBoxHeight')");

        colonne.Should().Be(0);
    }

    [Theory]
    [InlineData("magazines")]
    [InlineData("pages")]
    public async Task Un_impronta_duplicata_viene_rifiutata_ma_piu_valori_assenti_no(string tabella)
    {
        var (_, pagine) = await SeedMagazineWithPagesAsync(2);
        var (_, altre) = await SeedMagazineWithPagesAsync(1);
        var impronta = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
        var sql = tabella == "magazines"
            ? "UPDATE riviste.magazines SET impronta = @i WHERE \"Id\" = (SELECT \"MagazineId\" FROM riviste.pages WHERE \"Id\" = @p)"
            : "UPDATE riviste.pages SET impronta = @i WHERE \"Id\" = @p";

        (await SqlStateAsync(sql, parameters: [P("i", impronta), P("p", pagine[0])])).Should().BeNull();
        (await SqlStateAsync(sql, parameters: [P("i", impronta), P("p", altre[0])])).Should().Be(PostgresErrorCodes.UniqueViolation);
        (await ScalarAsync<long>($"SELECT count(*) FROM riviste.{tabella} WHERE impronta IS NULL")).Should().BePositive();
    }

    [Fact]
    public async Task I_ruoli_mantengono_i_permessi_sulle_colonne_nuove()
    {
        var pagina = await PaginaAsync();

        (await SqlStateAsync(InsertGioco, "dbe_estrazione", Gioco(pagina))).Should().BeNull();
        (await SqlStateAsync("SELECT tipologia, riquadro_x, avvisi, trascrizione_proposta FROM riviste.games", "dbe_originalita"))
            .Should().BeNull();
        (await SqlStateAsync(InsertGioco, "dbe_originalita", Gioco(pagina))).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("SELECT impronta FROM riviste.pages", "dbe_creazione")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task Su_un_database_vuoto_la_migrazione_si_annulla_e_si_riapplica()
    {
        await using var context = NewContext();
        var migrator = context.GetService<IMigrator>();

        await migrator.MigrateAsync(MigrazionePrecedente);
        (await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'riviste' AND table_name = 'crossword_games'"))
            .Should().Be(1);

        await migrator.MigrateAsync();
        (await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'riviste' AND table_name = 'games' " +
            "AND column_name IN ('tipologia', 'versione_tipologia', 'stato_tipologia', 'confidenza', 'avvisi', 'trascrizione_proposta')"))
            .Should().Be(6);
        (await context.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Con_Giochi_vecchi_la_migrazione_si_ferma_con_un_messaggio_chiaro_e_non_cambia_nulla()
    {
        await using var context = NewContext();
        var migrator = context.GetService<IMigrator>();
        var (_, pagine) = await SeedMagazineWithPagesAsync();
        await migrator.MigrateAsync(MigrazionePrecedente);
        await ExecuteAsync(
            "INSERT INTO riviste.games (\"PageId\", \"" + ColonnaVecchia + "\", \"ExtractedDate\", \"HasSolution\", origine) " +
            "VALUES (@p, 'Crossword', now(), false, 'rivista')", P("p", pagine[0]));

        var applica = async () => await migrator.MigrateAsync();

        var eccezione = (await applica.Should().ThrowAsync<Exception>()).Which;
        Messaggi(eccezione).Should().Contain("riviste.games").And.Contain("svuota").And.Contain("dbe migrate");
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.games")).Should().Be(1);
        (await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'riviste' AND table_name = 'games' AND column_name = '" + ColonnaVecchia + "'"))
            .Should().Be(1);
    }

    [Theory]
    [InlineData("Maiuscola")]
    [InlineData("1cifra")]
    [InlineData("con spazio")]
    [InlineData("a")]
    public async Task Il_database_rifiuta_una_Tipologia_con_formato_non_valido(string tipologia)
    {
        var pagina = await PaginaAsync();

        var stato = await SqlStateAsync(InsertGioco.Replace("'cruciverba-classico'", "@tip"),
            parameters: [.. Gioco(pagina), P("tip", tipologia)]);

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Il_database_accetta_una_Tipologia_lunga_come_nelle_altre_tabelle()
    {
        var pagina = await PaginaAsync();

        var stato = await SqlStateAsync(InsertGioco.Replace("'cruciverba-classico'", "@tip"),
            parameters: [.. Gioco(pagina), P("tip", "a" + new string('b', 60))]);

        stato.Should().BeNull();
    }

    [Fact]
    public async Task Il_database_rifiuta_la_versione_zero_della_Tipologia()
    {
        var pagina = await PaginaAsync();

        var stato = await SqlStateAsync(InsertGioco.Replace("'rivista', 'cruciverba-classico', 1,", "'rivista', 'cruciverba-classico', 0,"),
            parameters: Gioco(pagina));

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Theory]
    [InlineData("magazines")]
    [InlineData("pages")]
    public async Task Il_database_rifiuta_un_impronta_che_non_e_di_32_byte(string tabella)
    {
        var (_, pagine) = await SeedMagazineWithPagesAsync();
        var sql = tabella == "magazines"
            ? "UPDATE riviste.magazines SET impronta = @i WHERE \"Id\" = (SELECT \"MagazineId\" FROM riviste.pages WHERE \"Id\" = @p)"
            : "UPDATE riviste.pages SET impronta = @i WHERE \"Id\" = @p";

        (await SqlStateAsync(sql, parameters: [P("i", new byte[31]), P("p", pagine[0])])).Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Con_Giochi_con_Tipologia_il_Down_si_ferma_con_un_messaggio_chiaro_e_lo_schema_resta_invariato()
    {
        await using var context = NewContext();
        var migrator = context.GetService<IMigrator>();
        var pagina = await PaginaAsync();
        (await SqlStateAsync(InsertGioco, parameters: Gioco(pagina))).Should().BeNull();

        var annulla = async () => await migrator.MigrateAsync(MigrazionePrecedente);

        var eccezione = (await annulla.Should().ThrowAsync<Exception>()).Which;
        Messaggi(eccezione).Should().Contain("riviste.games").And.Contain("svuota").And.Contain("prima di annullare").And.NotContain("dbe migrate");
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.games")).Should().Be(1);
        (await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'riviste' AND table_name = 'games' " +
            "AND column_name IN ('tipologia', 'versione_tipologia', 'stato_tipologia', 'confidenza', 'avvisi', 'trascrizione_proposta')"))
            .Should().Be(6);
        (await context.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Il_modello_non_ha_modifiche_in_sospeso_rispetto_all_ultima_migrazione()
    {
        await using var context = NewContext();

        context.Database.HasPendingModelChanges().Should().BeFalse();
    }

    private static string Messaggi(Exception? eccezione)
    {
        var testo = new System.Text.StringBuilder();
        for (; eccezione is not null; eccezione = eccezione.InnerException)
            testo.AppendLine(eccezione.Message);
        return testo.ToString();
    }
}
