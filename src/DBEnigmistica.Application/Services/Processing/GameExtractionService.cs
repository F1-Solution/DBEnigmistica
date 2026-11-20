using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Application.Services.Processing;

public class GameExtractionService
{
    private readonly IAiGameAnalyzer _aiAnalyzer;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameExtractionService> _logger;

    public GameExtractionService(
        IAiGameAnalyzer aiAnalyzer,
        IGameRepository gameRepository,
        ILogger<GameExtractionService> logger)
    {
        _aiAnalyzer = aiAnalyzer;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<GameExtractionResult> ExtractAndSaveGameAsync(
        Page page,
        PageRegion region,
        string ocrText,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Extracting game from page {PageNumber}, region at ({X},{Y})",
                page.PageNumber, region.BoundingBox.X, region.BoundingBox.Y);

            // Step 1: Classify game type
            var classification = await _aiAnalyzer.ClassifyGameAsync(
                ocrText,
                null,
                cancellationToken);

            // Parse game type from string
            if (!Enum.TryParse<Domain.Enums.GameType>(classification.GameType, ignoreCase: true, out var gameType) ||
                gameType == Domain.Enums.GameType.Unknown)
            {
                _logger.LogWarning("Could not classify game type '{GameType}'. Confidence: {Confidence}",
                    classification.GameType, classification.Confidence);
                return new GameExtractionResult
                {
                    Success = false,
                    ErrorMessage = $"Tipo di gioco '{classification.GameType}' non riconosciuto"
                };
            }

            _logger.LogInformation(
                "Game classified as {GameType} with confidence {Confidence}",
                gameType, classification.Confidence);

            // Step 2: Create Game entity
            var game = new Game
            {
                Origine = Origine.Rivista,
                PageId = page.Id,
                GameType = gameType,
                Title = classification.GameTitle,
                Description = classification.Description,
                PositionX = region.BoundingBox.X,
                PositionY = region.BoundingBox.Y,
                BoundingBoxWidth = region.BoundingBox.Width,
                BoundingBoxHeight = region.BoundingBox.Height,
                ClassificationConfidence = classification.Confidence,
                ExtractedDate = DateTime.UtcNow
            };

            // Step 3: Extract specific game data based on type
            var extractionResult = await _aiAnalyzer.ExtractGameDataAsync(
                classification.GameType.ToString(),
                ocrText,
                null,
                cancellationToken);

            if (extractionResult == null)
            {
                _logger.LogWarning("Game data extraction returned null for {GameType}",
                    classification.GameType);
            }

            // Step 4: Save to database
            await _gameRepository.AddAsync(game, cancellationToken);
            await _gameRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully extracted and saved game {GameId} of type {GameType}",
                game.Id, game.GameType);

            return new GameExtractionResult
            {
                Success = true,
                Game = game,
                ExtractedData = extractionResult,
                Classification = classification
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error extracting game from page {PageNumber}", page.PageNumber);

            return new GameExtractionResult
            {
                Success = false,
                ErrorMessage = $"Errore durante l'estrazione: {ex.Message}"
            };
        }
    }

    public async Task<List<GameExtractionResult>> ExtractGamesFromPageAsync(
        Page page,
        List<PageRegion> gameRegions,
        Dictionary<PageRegion, string> ocrTexts,
        CancellationToken cancellationToken = default)
    {
        var results = new List<GameExtractionResult>();

        foreach (var region in gameRegions)
        {
            if (!ocrTexts.TryGetValue(region, out var ocrText))
            {
                _logger.LogWarning("No OCR text found for region at ({X},{Y})",
                    region.BoundingBox.X, region.BoundingBox.Y);
                continue;
            }

            var result = await ExtractAndSaveGameAsync(
                page,
                region,
                ocrText,
                cancellationToken);

            results.Add(result);
        }

        return results;
    }
}

public class GameExtractionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Game? Game { get; set; }
    public object? ExtractedData { get; set; }
    public GameClassificationResult? Classification { get; set; }
}
