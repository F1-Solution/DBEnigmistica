using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DBEnigmistica.Tests.Integration;

/// <summary>Date solo in UTC (criterio 13) e cancellazione delle pagine-soluzione in uso (decisione 3).</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class DateEPagineIntegrationTests : IntegrationTestBase
{
    public DateEPagineIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task No_context_schema_has_a_column_without_time_zone()
    {
        var colonne = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns " +
            "WHERE table_schema IN ('riviste','catalogo','tipologie','registro') " +
            "AND data_type IN ('timestamp without time zone','date','time without time zone')");
        var conFuso = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns " +
            "WHERE table_schema IN ('riviste','catalogo','tipologie','registro') " +
            "AND data_type = 'timestamp with time zone'");

        colonne.Should().Be(0);
        conFuso.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task Dates_are_read_back_as_the_same_utc_instant()
    {
        var istante = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc).AddTicks(1230);
        var cliente = await SeedSoggettoAsync();
        var gioco = await SeedGiocoOriginaleAsync(configure: g =>
            g.AggiungiCessione(cliente.Id, istante, new Periodo(istante, istante.AddDays(1)), false, Territorio.Mondo, false));

        await using var read = NewContext();
        var cessione = (await read.Cessioni.SingleAsync(c => c.GiocoId == gioco));

        cessione.StipulataIl.Kind.Should().Be(DateTimeKind.Utc);
        cessione.StipulataIl.Should().Be(istante);
        cessione.Periodo.Inizio.Kind.Should().Be(DateTimeKind.Utc);
        cessione.Periodo.Fine!.Value.Kind.Should().Be(DateTimeKind.Utc);
        cessione.Periodo.Fine.Should().Be(istante.AddDays(1));
    }

    [Fact]
    public async Task Magazine_dates_are_read_back_as_utc()
    {
        var (id, _) = await SeedMagazineWithPagesAsync(magazine: NewMagazine(publicationDate: Utc(2024, 5, 6)));

        await using var read = NewContext();
        var rivista = await read.Magazines.SingleAsync(m => m.Id == id);

        rivista.PublicationDate.Kind.Should().Be(DateTimeKind.Utc);
        rivista.PublicationDate.Should().Be(Utc(2024, 5, 6));
    }

    private async Task<(int PageA, int MagazineA, int GameB)> SeedRivisteConPaginaInUsoAsync(bool tramiteSoluzione)
    {
        var (magazineA, pagineA) = await SeedMagazineWithPagesAsync(2, NewMagazine("Rivista A"));
        var (_, pagineB) = await SeedMagazineWithPagesAsync(1, NewMagazine("Rivista B"));

        int gameB;
        if (tramiteSoluzione)
        {
            gameB = await SeedGameAsync(pagineB[0]);
            await using var context = NewContext();
            context.GameSolutions.Add(NewSolution(gameB, solutionPageId: pagineA[1]));
            await context.SaveChangesAsync();
        }
        else
        {
            gameB = await SeedGameAsync(pagineB[0], g => g.SolutionPageId = pagineA[1]);
        }

        return (pagineA[1], magazineA, gameB);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Deleting_a_magazine_with_a_page_used_by_another_magazine_is_refused_with_the_game_named(bool tramiteSoluzione)
    {
        var (_, magazineA, gameB) = await SeedRivisteConPaginaInUsoAsync(tramiteSoluzione);

        await using var context = NewContext();
        var repository = new MagazineRepository(context);
        var delete = async () =>
        {
            await repository.DeleteAsync(magazineA);
            await repository.SaveChangesAsync();
        };

        var errore = (await delete.Should().ThrowAsync<PaginaSoluzioneInUsoException>()).Which;
        errore.Message.Should().Contain("Rivista A").And.Contain($"Gioco {gameB}").And.Contain("Rivista B");
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.magazines")).Should().Be(2);
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.pages")).Should().Be(3);
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.games")).Should().Be(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Direct_sql_delete_of_a_used_page_is_rejected_by_the_foreign_key(bool tramiteSoluzione)
    {
        var (paginaA, _, _) = await SeedRivisteConPaginaInUsoAsync(tramiteSoluzione);

        var stato = await SqlStateAsync("DELETE FROM riviste.pages WHERE \"Id\" = @p", parameters: P("p", paginaA));

        stato.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Direct_sql_delete_of_a_magazine_with_a_page_used_elsewhere_is_rejected()
    {
        var (_, magazineA, _) = await SeedRivisteConPaginaInUsoAsync(false);

        var stato = await SqlStateAsync("DELETE FROM riviste.magazines WHERE \"Id\" = @m", parameters: P("m", magazineA));

        stato.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Deleting_a_magazine_whose_solution_pages_are_used_only_by_its_own_games_succeeds()
    {
        var (magazineId, pagine) = await SeedMagazineWithPagesAsync(2);
        var game = await SeedGameAsync(pagine[0], g => g.SolutionPageId = pagine[1]);
        await using (var setup = NewContext())
        {
            setup.GameSolutions.Add(NewSolution(game, solutionPageId: pagine[1]));
            await setup.SaveChangesAsync();
        }

        await using (var write = NewContext())
        {
            var repository = new MagazineRepository(write);
            await repository.DeleteAsync(magazineId);
            await repository.SaveChangesAsync();
        }

        (await ScalarAsync<long>("SELECT count(*) FROM riviste.magazines")).Should().Be(0);
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.games")).Should().Be(0);
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.game_solutions")).Should().Be(0);
    }

    private static async Task<PostgresException> PostgresErrorAsync(Func<Task> azione)
    {
        try
        {
            await azione();
        }
        catch (Exception e)
        {
            for (var inner = e; inner is not null; inner = inner.InnerException!)
            {
                if (inner is PostgresException postgres)
                    return postgres;
                if (inner.InnerException is null)
                    break;
            }

            throw new Xunit.Sdk.XunitException($"Eccezione inattesa: {e}");
        }

        throw new Xunit.Sdk.XunitException("Nessun errore: il salvataggio doveva essere rifiutato.");
    }

    private async Task VerificaDipendenteIntattoAsync(int gameB, int paginaA, bool tramiteSoluzione)
    {
        await using var read = NewContext();
        var riferimento = tramiteSoluzione
            ? (await read.GameSolutions.AsNoTracking().SingleAsync(s => s.GameId == gameB)).SolutionPageId
            : (await read.Games.AsNoTracking().SingleAsync(g => g.Id == gameB)).SolutionPageId;
        riferimento.Should().Be(paginaA);
        (await read.Pages.CountAsync(p => p.Id == paginaA)).Should().Be(1);
    }

    private async Task CaricaDipendenteAsync(EnigmisticaDbContext context, int gameB, bool tramiteSoluzione)
    {
        if (tramiteSoluzione)
            await context.GameSolutions.SingleAsync(s => s.GameId == gameB);
        else
            await context.Games.SingleAsync(g => g.Id == gameB);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Repository_delete_with_the_dependent_loaded_in_the_same_context_is_refused_and_nothing_changes(bool tramiteSoluzione)
    {
        var (paginaA, magazineA, gameB) = await SeedRivisteConPaginaInUsoAsync(tramiteSoluzione);

        await using (var context = NewContext())
        {
            await CaricaDipendenteAsync(context, gameB, tramiteSoluzione);
            var repository = new MagazineRepository(context);
            var delete = async () =>
            {
                await repository.DeleteAsync(magazineA);
                await repository.SaveChangesAsync();
            };
            await delete.Should().ThrowAsync<PaginaSoluzioneInUsoException>();
        }

        await VerificaDipendenteIntattoAsync(gameB, paginaA, tramiteSoluzione);
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.magazines")).Should().Be(2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Direct_context_remove_of_a_used_magazine_or_page_is_rejected_with_the_dependent_loaded(bool tramiteSoluzione)
    {
        var (paginaA, magazineA, gameB) = await SeedRivisteConPaginaInUsoAsync(tramiteSoluzione);

        await using (var context = NewContext())
        {
            await CaricaDipendenteAsync(context, gameB, tramiteSoluzione);
            context.Remove(await context.Magazines.Include(m => m.Pages).SingleAsync(m => m.Id == magazineA));
            var errore = await PostgresErrorAsync(() => context.SaveChangesAsync());
            errore.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
        }

        await using (var context = NewContext())
        {
            await CaricaDipendenteAsync(context, gameB, tramiteSoluzione);
            context.Remove(await context.Pages.SingleAsync(p => p.Id == paginaA));
            var errore = await PostgresErrorAsync(() => context.SaveChangesAsync());
            errore.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
        }

        await VerificaDipendenteIntattoAsync(gameB, paginaA, tramiteSoluzione);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generic_repository_delete_of_a_used_page_is_rejected_with_the_dependent_loaded(bool tramiteSoluzione)
    {
        var (paginaA, _, gameB) = await SeedRivisteConPaginaInUsoAsync(tramiteSoluzione);

        await using (var context = NewContext())
        {
            await CaricaDipendenteAsync(context, gameB, tramiteSoluzione);
            var repository = new Repository<Page>(context);
            var errore = await PostgresErrorAsync(async () =>
            {
                await repository.DeleteAsync(paginaA);
                await repository.SaveChangesAsync();
            });
            errore.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
        }

        await VerificaDipendenteIntattoAsync(gameB, paginaA, tramiteSoluzione);
    }

    [Fact]
    public async Task Deleting_a_missing_magazine_does_nothing()
    {
        await using var context = NewContext();
        var repository = new MagazineRepository(context);

        await repository.DeleteAsync(12345);

        (await repository.SaveChangesAsync()).Should().Be(0);
    }
}
