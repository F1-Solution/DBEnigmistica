using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Tests.Integration;

public class RepositoryIntegrationTests : IDisposable
{
    private readonly EnigmisticaDbContext _context;
    private readonly MagazineRepository _magazineRepository;
    private readonly GameRepository _gameRepository;

    public RepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<EnigmisticaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EnigmisticaDbContext(options);
        _magazineRepository = new MagazineRepository(_context);
        _gameRepository = new GameRepository(_context);
    }

    [Fact]
    public async Task Magazine_CRUD_Operations_WorkCorrectly()
    {
        // Create
        var magazine = new Magazine
        {
            Title = "La Settimana Enigmistica",
            PublicationDate = new DateTime(2024, 1, 15),
            Periodicity = MagazinePeriodicity.Weekly,
            IssueNumber = "4500",
            Year = 2024,
            SourceFilePath = "/test/magazine.pdf",
            ProcessedDate = DateTime.UtcNow
        };

        var created = await _magazineRepository.AddAsync(magazine);
        await _magazineRepository.SaveChangesAsync();

        created.Id.Should().BeGreaterThan(0);

        // Read
        var retrieved = await _magazineRepository.GetByIdAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("La Settimana Enigmistica");
        retrieved.IssueNumber.Should().Be("4500");

        // Update
        retrieved.IssueNumber = "4501";
        await _magazineRepository.UpdateAsync(retrieved);
        await _magazineRepository.SaveChangesAsync();

        var updated = await _magazineRepository.GetByIdAsync(created.Id);
        updated!.IssueNumber.Should().Be("4501");

        // Delete
        await _magazineRepository.DeleteAsync(created.Id);
        await _magazineRepository.SaveChangesAsync();

        var deleted = await _magazineRepository.GetByIdAsync(created.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Magazine_GetByTitleAndDate_FindsCorrectMagazine()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15);
        var magazine1 = new Magazine
        {
            Title = "La Settimana Enigmistica",
            PublicationDate = date,
            Periodicity = MagazinePeriodicity.Weekly,
            Year = 2024,
            SourceFilePath = "/test/mag1.pdf",
            ProcessedDate = DateTime.UtcNow
        };

        var magazine2 = new Magazine
        {
            Title = "Enigmistica Italiana",
            PublicationDate = date,
            Periodicity = MagazinePeriodicity.Monthly,
            Year = 2024,
            SourceFilePath = "/test/mag2.pdf",
            ProcessedDate = DateTime.UtcNow
        };

        await _magazineRepository.AddAsync(magazine1);
        await _magazineRepository.AddAsync(magazine2);
        await _magazineRepository.SaveChangesAsync();

        // Act
        var found = await _magazineRepository.GetByTitleAndDateAsync("La Settimana Enigmistica", date);

        // Assert
        found.Should().NotBeNull();
        found!.Title.Should().Be("La Settimana Enigmistica");
    }

    [Fact]
    public async Task Magazine_WithPages_LoadsCorrectly()
    {
        // Arrange
        var magazine = new Magazine
        {
            Title = "Test Magazine",
            PublicationDate = DateTime.UtcNow,
            Periodicity = MagazinePeriodicity.Monthly,
            Year = 2024,
            SourceFilePath = "/test/mag.pdf",
            ProcessedDate = DateTime.UtcNow
        };

        await _magazineRepository.AddAsync(magazine);
        await _magazineRepository.SaveChangesAsync();

        var page1 = new Page { MagazineId = magazine.Id, PageNumber = 1, Width = 800, Height = 1200, ProcessedDate = DateTime.UtcNow };
        var page2 = new Page { MagazineId = magazine.Id, PageNumber = 2, Width = 800, Height = 1200, ProcessedDate = DateTime.UtcNow };

        await _magazineRepository.AddPageAsync(page1);
        await _magazineRepository.AddPageAsync(page2);
        await _magazineRepository.SaveChangesAsync();

        // Act
        var withPages = await _magazineRepository.GetWithPagesAsync(magazine.Id);

        // Assert
        withPages.Should().NotBeNull();
        withPages!.Pages.Should().HaveCount(2);
        withPages.Pages.Should().Contain(p => p.PageNumber == 1);
        withPages.Pages.Should().Contain(p => p.PageNumber == 2);
    }

    [Fact]
    public async Task Game_Creation_WithValidData_Succeeds()
    {
        // Arrange - Create magazine and page first
        var magazine = new Magazine
        {
            Title = "Test Magazine",
            PublicationDate = DateTime.UtcNow,
            Periodicity = MagazinePeriodicity.Monthly,
            Year = 2024,
            SourceFilePath = "/test/mag.pdf",
            ProcessedDate = DateTime.UtcNow
        };
        await _magazineRepository.AddAsync(magazine);
        await _magazineRepository.SaveChangesAsync();

        var page = new Page { MagazineId = magazine.Id, PageNumber = 1, ProcessedDate = DateTime.UtcNow };
        await _magazineRepository.AddPageAsync(page);
        await _magazineRepository.SaveChangesAsync();

        // Act - Create game
        var game = new Game
        {
            PageId = page.Id,
            GameType = GameType.Crossword,
            Title = "Cruciverba Facile",
            Description = "Un cruciverba semplice",
            PositionX = 100,
            PositionY = 200,
            BoundingBoxWidth = 300,
            BoundingBoxHeight = 400,
            ClassificationConfidence = 0.95,
            ExtractedDate = DateTime.UtcNow
        };

        await _gameRepository.AddAsync(game);
        await _gameRepository.SaveChangesAsync();

        // Assert
        game.Id.Should().BeGreaterThan(0);
        
        var retrieved = await _gameRepository.GetByIdAsync(game.Id);
        retrieved.Should().NotBeNull();
        retrieved!.GameType.Should().Be(GameType.Crossword);
        retrieved.Title.Should().Be("Cruciverba Facile");
    }

    [Fact]
    public async Task Game_GetByPageId_ReturnsCorrectGames()
    {
        // Arrange
        var magazine = new Magazine
        {
            Title = "Test Magazine",
            PublicationDate = DateTime.UtcNow,
            Periodicity = MagazinePeriodicity.Monthly,
            Year = 2024,
            SourceFilePath = "/test/mag.pdf",
            ProcessedDate = DateTime.UtcNow
        };
        await _magazineRepository.AddAsync(magazine);
        await _magazineRepository.SaveChangesAsync();

        var page = new Page { MagazineId = magazine.Id, PageNumber = 1, ProcessedDate = DateTime.UtcNow };
        await _magazineRepository.AddPageAsync(page);
        await _magazineRepository.SaveChangesAsync();

        var game1 = new Game
        {
            PageId = page.Id,
            GameType = GameType.Crossword,
            Title = "Game 1",
            ExtractedDate = DateTime.UtcNow
        };
        var game2 = new Game
        {
            PageId = page.Id,
            GameType = GameType.Rebus,
            Title = "Game 2",
            ExtractedDate = DateTime.UtcNow
        };

        await _gameRepository.AddAsync(game1);
        await _gameRepository.AddAsync(game2);
        await _gameRepository.SaveChangesAsync();

        // Act
        var games = await _gameRepository.GetByPageIdAsync(page.Id);

        // Assert
        games.Should().HaveCount(2);
        games.Should().Contain(g => g.GameType == GameType.Crossword);
        games.Should().Contain(g => g.GameType == GameType.Rebus);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
