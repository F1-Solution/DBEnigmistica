using DBEnigmistica.Application.Services.Processing;
using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppGameExtractionResult = DBEnigmistica.Application.Services.Processing.GameExtractionResult;
using DomainGameExtractionResult = DBEnigmistica.Domain.AI.GameExtractionResult;

namespace DBEnigmistica.Tests.Unit.Services;

public class GameExtractionServiceTests
{
    private readonly Mock<IAiGameAnalyzer> _aiAnalyzerMock;
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<ILogger<GameExtractionService>> _loggerMock;
    private readonly GameExtractionService _service;

    public GameExtractionServiceTests()
    {
        _aiAnalyzerMock = new Mock<IAiGameAnalyzer>();
        _gameRepositoryMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<GameExtractionService>>();

        _service = new GameExtractionService(
            _aiAnalyzerMock.Object,
            _gameRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExtractAndSaveGameAsync_ValidGame_ReturnsSuccess()
    {
        // Arrange
        var page = new Page { Id = 1, PageNumber = 1, MagazineId = 1 };
        var region = new PageRegion
        {
            BoundingBox = new BoundingBox { X = 10, Y = 10, Width = 100, Height = 100 },
            Type = RegionType.Grid,
            Confidence = 0.9f
        };
        var ocrText = "Cruciverba: 1. Capitale d'Italia (4 lettere)";

        var classification = new GameClassificationResult
        {
            GameType = "Crossword",
            GameTitle = "Cruciverba Facile",
            Confidence = 0.95f,
            Description = "Un cruciverba semplice"
        };

        var extractionResult = new DomainGameExtractionResult
        {
            GameType = "Crossword",
            Fields = new Dictionary<string, object?>()
        };

        _aiAnalyzerMock
            .Setup(x => x.ClassifyGameAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(classification);

        _aiAnalyzerMock
            .Setup(x => x.ExtractGameDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractionResult);

        _gameRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game g, CancellationToken ct) => g);

        _gameRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ExtractAndSaveGameAsync(page, region, ocrText);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Game.Should().NotBeNull();
        result.Game!.GameType.Should().Be(GameType.Crossword);
        result.Game.Title.Should().Be("Cruciverba Facile");
        result.Classification.Should().Be(classification);

        _gameRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
        _gameRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExtractAndSaveGameAsync_UnknownGameType_ReturnsFailure()
    {
        // Arrange
        var page = new Page { Id = 1, PageNumber = 1, MagazineId = 1 };
        var region = new PageRegion
        {
            BoundingBox = new BoundingBox { X = 10, Y = 10, Width = 100, Height = 100 },
            Type = RegionType.Grid,
            Confidence = 0.9f
        };
        var ocrText = "Some unrecognizable game text";

        var classification = new GameClassificationResult
        {
            GameType = "UnknownType",
            GameTitle = "Unknown",
            Confidence = 0.3f,
            Description = "Cannot classify"
        };

        _aiAnalyzerMock
            .Setup(x => x.ClassifyGameAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(classification);

        // Act
        var result = await _service.ExtractAndSaveGameAsync(page, region, ocrText);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("non riconosciuto");
        result.Game.Should().BeNull();

        _gameRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExtractGamesFromPageAsync_MultipleRegions_ProcessesAll()
    {
        // Arrange
        var page = new Page { Id = 1, PageNumber = 1, MagazineId = 1 };
        var regions = new List<PageRegion>
        {
            new() { BoundingBox = new BoundingBox { X = 0, Y = 0, Width = 50, Height = 50 }, Type = RegionType.Grid, Confidence = 0.9f },
            new() { BoundingBox = new BoundingBox { X = 60, Y = 0, Width = 50, Height = 50 }, Type = RegionType.Table, Confidence = 0.8f }
        };
        var ocrTexts = new Dictionary<PageRegion, string>
        {
            { regions[0], "Game 1 text" },
            { regions[1], "Game 2 text" }
        };

        var classification = new GameClassificationResult
        {
            GameType = "Crossword",
            GameTitle = "Test Game",
            Confidence = 0.9f
        };

        _aiAnalyzerMock
            .Setup(x => x.ClassifyGameAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(classification);

        _aiAnalyzerMock
            .Setup(x => x.ExtractGameDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DomainGameExtractionResult
            {
                GameType = "Crossword",
                Fields = new Dictionary<string, object?>()
            });

        _gameRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game g, CancellationToken ct) => g);

        _gameRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var results = await _service.ExtractGamesFromPageAsync(page, regions, ocrTexts);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        _aiAnalyzerMock.Verify(x => x.ClassifyGameAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
