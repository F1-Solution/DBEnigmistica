using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace DBEnigmistica.Application.Services.Export;

/// <summary>
/// Servizio per esportare dati in vari formati
/// </summary>
public class DataExportService
{
    private readonly IMagazineRepository _magazineRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<DataExportService> _logger;

    public DataExportService(
        IMagazineRepository magazineRepository,
        IGameRepository gameRepository,
        ILogger<DataExportService> logger)
    {
        _magazineRepository = magazineRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    /// <summary>
    /// Esporta tutti i giochi in formato JSON
    /// </summary>
    public async Task<string> ExportGamesAsJsonAsync(
        int? magazineId = null,
        bool prettyPrint = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting games to JSON. MagazineId: {MagazineId}", magazineId);

        IEnumerable<Game> games;
        if (magazineId.HasValue)
        {
            games = await _gameRepository.GetByMagazineIdAsync(magazineId.Value);
        }
        else
        {
            games = await _gameRepository.GetAllAsync();
        }

        var exportData = games.Select(g => new
        {
            g.Id,
            g.GameType,
            g.Title,
            g.Description,
            g.Difficulty,
            Position = new { g.PositionX, g.PositionY },
            BoundingBox = new { Width = g.BoundingBoxWidth, Height = g.BoundingBoxHeight },
            g.ClassificationConfidence,
            g.ExtractedDate,
            g.PageId
        }).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = prettyPrint,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(new
        {
            ExportDate = DateTime.UtcNow,
            TotalGames = exportData.Count,
            MagazineId = magazineId,
            Games = exportData
        }, options);

        _logger.LogInformation("Exported {Count} games to JSON", exportData.Count);
        return json;
    }

    /// <summary>
    /// Esporta giochi in formato CSV
    /// </summary>
    public async Task<string> ExportGamesAsCsvAsync(
        int? magazineId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting games to CSV. MagazineId: {MagazineId}", magazineId);

        IEnumerable<Game> games;
        if (magazineId.HasValue)
        {
            games = await _gameRepository.GetByMagazineIdAsync(magazineId.Value);
        }
        else
        {
            games = await _gameRepository.GetAllAsync();
        }

        var csv = new StringBuilder();
        
        csv.AppendLine(CsvFieldFormatter.SeparatorDirective);
        csv.AppendLine("Id,GameType,Title,Description,Difficulty,PositionX,PositionY,Width,Height,Confidence,ExtractedDate,PageId");

        // Rows
        foreach (var game in games)
        {
            csv.AppendLine($"{CsvFieldFormatter.Number(game.Id)}," +
                          $"{EscapeCsvField(game.GameType.ToString())}," +
                          $"{EscapeCsvField(game.Title)}," +
                          $"{EscapeCsvField(game.Description)}," +
                          $"{EscapeCsvField(game.Difficulty)}," +
                          $"{CsvFieldFormatter.Number(game.PositionX)}," +
                          $"{CsvFieldFormatter.Number(game.PositionY)}," +
                          $"{CsvFieldFormatter.Number(game.BoundingBoxWidth)}," +
                          $"{CsvFieldFormatter.Number(game.BoundingBoxHeight)}," +
                          $"{CsvFieldFormatter.Number(game.ClassificationConfidence)}," +
                          $"{CsvFieldFormatter.Date(game.ExtractedDate)}," +
                          $"{CsvFieldFormatter.Number(game.PageId)}");
        }

        _logger.LogInformation("Exported {Count} games to CSV", games.Count());
        return csv.ToString();
    }

    /// <summary>
    /// Esporta riviste con statistiche
    /// </summary>
    public async Task<string> ExportMagazineStatsAsJsonAsync(
        bool prettyPrint = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting magazine statistics");

        var magazines = await _magazineRepository.GetAllAsync();
        var magazineList = magazines.ToList();

        var stats = new List<object>();
        foreach (var magazine in magazineList)
        {
            var withPages = await _magazineRepository.GetWithPagesAndGamesAsync(magazine.Id);
            if (withPages == null) continue;

            var totalGames = withPages.Pages.Sum(p => p.Games.Count);
            var gamesByType = withPages.Pages
                .SelectMany(p => p.Games)
                .GroupBy(g => g.GameType)
                .Select(g => new { GameType = g.Key.ToString(), Count = g.Count() })
                .ToList();

            stats.Add(new
            {
                magazine.Id,
                magazine.Title,
                magazine.IssueNumber,
                magazine.PublicationDate,
                magazine.Year,
                TotalPages = withPages.Pages.Count,
                TotalGames = totalGames,
                GamesByType = gamesByType,
                ProcessedDate = magazine.ProcessedDate
            });
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = prettyPrint,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(new
        {
            ExportDate = DateTime.UtcNow,
            TotalMagazines = stats.Count,
            Magazines = stats
        }, options);

        _logger.LogInformation("Exported statistics for {Count} magazines", stats.Count);
        return json;
    }

    /// <summary>
    /// Salva export su file
    /// </summary>
    public async Task SaveToFileAsync(
        string content,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        _logger.LogInformation("Saved export to file: {FilePath}", filePath);
    }

    /// <summary>
    /// Escape CSV field per gestire virgole e quotes
    /// </summary>
    private static string EscapeCsvField(string? field) => CsvFieldFormatter.Format(field);
}
