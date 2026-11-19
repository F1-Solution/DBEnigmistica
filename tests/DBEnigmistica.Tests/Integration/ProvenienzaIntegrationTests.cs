using DBEnigmistica.Domain.Catalogo;
using DBEnigmistica.Domain.Diritti;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DBEnigmistica.Tests.Integration;

/// <summary>Origine obbligatoria, Cessioni esclusive e stato di inedito, imposti dal database (criteri 9 e 13).</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class ProvenienzaIntegrationTests : IntegrationTestBase
{
    public ProvenienzaIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private const string InsertGiocoSenzaOrigine =
        "INSERT INTO catalogo.giochi (id, tipologia, versione_tipologia, stato_inedito, creato_il) " +
        "VALUES (@id, 'sudoku', 1, 'inedito', now())";

    private const string InsertGiocoConOrigine =
        "INSERT INTO catalogo.giochi (id, tipologia, versione_tipologia, origine, stato_inedito, creato_il) " +
        "VALUES (@id, 'sudoku', 1, @origine, 'inedito', now())";

    private async Task<int> SeedPaginaAsync() => (await SeedMagazineWithPagesAsync()).PageIds[0];

    [Fact]
    public async Task Original_game_without_origin_is_rejected_with_not_null_violation()
    {
        var stato = await SqlStateAsync(InsertGiocoSenzaOrigine, parameters: P("id", Guid.CreateVersion7()));

        stato.Should().Be(PostgresErrorCodes.NotNullViolation);
    }

    [Fact]
    public async Task Extracted_game_without_origin_is_rejected_with_not_null_violation()
    {
        var pagina = await SeedPaginaAsync();

        var stato = await SqlStateAsync(
            "INSERT INTO riviste.games (\"PageId\", \"GameType\", \"ExtractedDate\", \"HasSolution\") " +
            "VALUES (@p, 'Crossword', now(), false)",
            parameters: P("p", pagina));

        stato.Should().Be(PostgresErrorCodes.NotNullViolation);
    }

    [Fact]
    public async Task Both_origin_columns_have_no_default()
    {
        var conDefault = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE column_name = 'origine' " +
            "AND table_schema IN ('riviste','catalogo') AND column_default IS NOT NULL");
        var colonne = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE column_name = 'origine' " +
            "AND table_schema IN ('riviste','catalogo') AND is_nullable = 'NO'");

        conDefault.Should().Be(0);
        colonne.Should().Be(2);
    }

    [Fact]
    public async Task Extracted_game_with_an_origin_other_than_magazine_is_rejected_by_the_check()
    {
        var pagina = await SeedPaginaAsync();

        var stato = await SqlStateAsync(
            "INSERT INTO riviste.games (\"PageId\", \"GameType\", \"ExtractedDate\", \"HasSolution\", origine) " +
            "VALUES (@p, 'Crossword', now(), false, 'originale')",
            parameters: P("p", pagina));

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Original_game_with_origin_magazine_or_unknown_is_rejected_by_the_check()
    {
        (await SqlStateAsync(InsertGiocoConOrigine, parameters: [P("id", Guid.CreateVersion7()), P("origine", "rivista")]))
            .Should().Be(PostgresErrorCodes.CheckViolation);
        (await SqlStateAsync(InsertGiocoConOrigine, parameters: [P("id", Guid.CreateVersion7()), P("origine", "altro")]))
            .Should().Be(PostgresErrorCodes.CheckViolation);
        (await SqlStateAsync(InsertGiocoConOrigine, parameters: [P("id", Guid.CreateVersion7()), P("origine", "licenza")]))
            .Should().BeNull();
    }

    [Fact]
    public async Task Public_domain_proof_is_all_or_nothing_and_only_for_public_domain()
    {
        const string sql = "INSERT INTO catalogo.giochi (id, tipologia, versione_tipologia, origine, stato_inedito, creato_il, " +
                           "pd_autore, pd_anno, pd_motivo) VALUES (@id, 'sudoku', 1, @o, 'inedito', now(), @a, @y, @m)";

        (await SqlStateAsync(sql, parameters: [P("id", Guid.CreateVersion7()), P("o", "pubblico_dominio"), P("a", "Dante"), P("y", null), P("m", "x")]))
            .Should().Be(PostgresErrorCodes.CheckViolation);
        (await SqlStateAsync(sql, parameters: [P("id", Guid.CreateVersion7()), P("o", "originale"), P("a", "Dante"), P("y", 1321), P("m", "x")]))
            .Should().Be(PostgresErrorCodes.CheckViolation);
        (await SqlStateAsync(sql, parameters: [P("id", Guid.CreateVersion7()), P("o", "pubblico_dominio"), P("a", "Dante"), P("y", 1321), P("m", "x")]))
            .Should().BeNull();
    }

    [Fact]
    public async Task Original_game_is_saved_without_proofs_and_read_back_whole()
    {
        var titolare = await SeedSoggettoAsync("Titolare");
        var licenziante = await SeedSoggettoAsync("Licenziante");
        var cliente = await SeedSoggettoAsync("Cliente");
        var id = await SeedGiocoOriginaleAsync(Origine.Licenza, g =>
        {
            g.AssegnaTitolare(titolare.Id);
            g.AggiungiLicenza(licenziante.Id, "Contratto 1", new Periodo(Utc(2026, 1, 1), null));
            g.AggiungiCessione(cliente.Id, Utc(2026, 2, 1), new Periodo(Utc(2026, 3, 1), Utc(2026, 9, 1)),
                true, Territorio.Paese("IT"), true);
        });
        var senzaProve = await SeedGiocoOriginaleAsync(Origine.PubblicoDominio);

        await using var read = NewContext();
        var gioco = await read.GiochiOriginali.Include(g => g.Licenze).Include(g => g.Cessioni).SingleAsync(g => g.Id == id);

        gioco.Origine.Should().Be(Origine.Licenza);
        gioco.TitolareId.Should().Be(titolare.Id);
        gioco.Licenze.Should().ContainSingle().Which.Periodo.Fine.Should().BeNull();
        var cessione = gioco.Cessioni.Should().ContainSingle().Subject;
        cessione.Territorio.Should().Be(Territorio.Paese("IT"));
        cessione.Periodo.Fine.Should().Be(Utc(2026, 9, 1));
        cessione.ComeInedito.Should().BeTrue();

        var vuoto = await read.GiochiOriginali.SingleAsync(g => g.Id == senzaProve);
        vuoto.TitolareId.Should().BeNull();
        vuoto.ProvaPubblicoDominio.Should().BeNull();
    }

    [Fact]
    public async Task Public_domain_proof_survives_a_round_trip()
    {
        var id = await SeedGiocoOriginaleAsync(Origine.PubblicoDominio,
            g => g.RegistraProvaPubblicoDominio(new ProvaPubblicoDominio("Dante", 1321, "Morto da secoli")));

        await using var read = NewContext();
        var prova = (await read.GiochiOriginali.SingleAsync(g => g.Id == id)).ProvaPubblicoDominio;

        prova.Should().Be(new ProvaPubblicoDominio("Dante", 1321, "Morto da secoli"));
    }

    private async Task<int> InserisciCessioneAsync(
        Guid gioco, Guid cliente, DateTime inizio, DateTime? fine, bool esclusiva, string territorio)
    {
        return await ExecuteAsync(
            "INSERT INTO catalogo.cessioni (id, gioco_id, cliente_id, stipulata_il, inizio, fine, esclusiva, territorio, come_inedito) " +
            "VALUES (@id, @g, @c, now(), @i, @f, @e, @t, false)",
            P("id", Guid.CreateVersion7()), P("g", gioco), P("c", cliente), P("i", inizio), P("f", fine),
            P("e", esclusiva), P("t", territorio));
    }

    private async Task<string?> StatoCessioneAsync(
        Guid gioco, Guid cliente, DateTime inizio, DateTime? fine, bool esclusiva, string territorio)
    {
        try
        {
            await InserisciCessioneAsync(gioco, cliente, inizio, fine, esclusiva, territorio);
            return null;
        }
        catch (PostgresException e)
        {
            return e.SqlState;
        }
    }

    [Theory]
    [InlineData("IT", "IT")]
    [InlineData("MONDO", "IT")]
    [InlineData("IT", "MONDO")]
    [InlineData("MONDO", "MONDO")]
    public async Task Overlapping_exclusive_assignments_are_rejected_by_the_exclusion_constraint(string primo, string secondo)
    {
        var cliente = await SeedSoggettoAsync();
        var gioco = await SeedGiocoOriginaleAsync();
        await InserisciCessioneAsync(gioco, cliente.Id, Utc(2026, 1, 1), Utc(2026, 6, 1), true, primo);

        var stato = await StatoCessioneAsync(gioco, cliente.Id, Utc(2026, 3, 1), null, true, secondo);

        stato.Should().Be(PostgresErrorCodes.ExclusionViolation);
    }

    [Theory]
    [InlineData("IT", "FR", 3, true)]
    [InlineData("IT", "IT", 6, true)]
    [InlineData("IT", "IT", 3, false)]
    [InlineData("AA", "ZZ", 3, true)]
    public async Task Disjoint_or_non_exclusive_assignments_are_accepted(string primo, string secondo, int meseSecondo, bool esclusiva)
    {
        var cliente = await SeedSoggettoAsync();
        var gioco = await SeedGiocoOriginaleAsync();
        await InserisciCessioneAsync(gioco, cliente.Id, Utc(2026, 1, 1), Utc(2026, 6, 1), true, primo);

        var stato = await StatoCessioneAsync(gioco, cliente.Id, Utc(2026, meseSecondo, 1), null, esclusiva, secondo);

        stato.Should().BeNull();
    }

    [Fact]
    public async Task Exclusive_assignments_of_different_games_do_not_conflict()
    {
        var cliente = await SeedSoggettoAsync();
        var uno = await SeedGiocoOriginaleAsync();
        var due = await SeedGiocoOriginaleAsync();
        await InserisciCessioneAsync(uno, cliente.Id, Utc(2026, 1, 1), null, true, "MONDO");

        (await StatoCessioneAsync(due, cliente.Id, Utc(2026, 1, 1), null, true, "MONDO")).Should().BeNull();
    }

    [Fact]
    public async Task Assignment_with_a_malformed_territory_is_rejected()
    {
        var cliente = await SeedSoggettoAsync();
        var gioco = await SeedGiocoOriginaleAsync();

        (await StatoCessioneAsync(gioco, cliente.Id, Utc(2026, 1, 1), null, false, "it"))
            .Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Game_with_assignments_cannot_be_deleted()
    {
        var cliente = await SeedSoggettoAsync();
        var gioco = await SeedGiocoOriginaleAsync();
        await InserisciCessioneAsync(gioco, cliente.Id, Utc(2026, 1, 1), null, false, "MONDO");

        var stato = await SqlStateAsync("DELETE FROM catalogo.giochi WHERE id = @g", parameters: P("g", gioco));

        stato.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task First_publication_can_be_recorded_once_through_the_domain()
    {
        var id = await SeedGiocoOriginaleAsync();

        await using (var write = NewContext())
        {
            var gioco = await write.GiochiOriginali.SingleAsync(g => g.Id == id);
            gioco.RegistraPrimaPubblicazione(new PrimaPubblicazione(Utc(2026, 5, 1), "Sito pubblico"));
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var stato = (await read.GiochiOriginali.SingleAsync(g => g.Id == id)).StatoInedito;
        stato.EInedito.Should().BeFalse();
        stato.Prima!.Luogo.Should().Be("Sito pubblico");
    }

    [Fact]
    public async Task Published_game_cannot_go_back_to_unpublished()
    {
        var id = await SeedGiocoOriginaleAsync(configure: g =>
            g.RegistraPrimaPubblicazione(new PrimaPubblicazione(Utc(2026, 5, 1), "Sito pubblico")));

        var stato = await SqlStateAsync(
            "UPDATE catalogo.giochi SET stato_inedito = 'inedito', prima_pubblicazione_il = NULL, " +
            "prima_pubblicazione_luogo = NULL WHERE id = @g",
            parameters: P("g", id));

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task First_publication_date_and_place_cannot_be_changed()
    {
        var id = await SeedGiocoOriginaleAsync(configure: g =>
            g.RegistraPrimaPubblicazione(new PrimaPubblicazione(Utc(2026, 5, 1), "Sito pubblico")));

        (await SqlStateAsync("UPDATE catalogo.giochi SET prima_pubblicazione_il = @d WHERE id = @g",
                parameters: [P("g", id), P("d", Utc(2026, 6, 1))]))
            .Should().Be(PostgresErrorCodes.CheckViolation);
        (await SqlStateAsync("UPDATE catalogo.giochi SET prima_pubblicazione_luogo = 'Altrove' WHERE id = @g",
                parameters: P("g", id)))
            .Should().Be(PostgresErrorCodes.CheckViolation);
        (await SqlStateAsync("UPDATE catalogo.giochi SET titolo = 'Nuovo titolo' WHERE id = @g", parameters: P("g", id)))
            .Should().BeNull();
    }

    [Fact]
    public async Task Inconsistent_publication_state_is_rejected_by_the_check()
    {
        var id = await SeedGiocoOriginaleAsync();

        var stato = await SqlStateAsync(
            "UPDATE catalogo.giochi SET stato_inedito = 'pubblicato' WHERE id = @g", parameters: P("g", id));

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }
}
