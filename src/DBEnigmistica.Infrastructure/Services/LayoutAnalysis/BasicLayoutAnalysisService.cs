using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Infrastructure.Services.LayoutAnalysis;

/// <summary>
/// Servizio base per l'analisi del layout di una pagina
/// </summary>
public class BasicLayoutAnalysisService : ILayoutAnalysisService
{
    private readonly ILogger<BasicLayoutAnalysisService> _logger;

    public BasicLayoutAnalysisService(ILogger<BasicLayoutAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<PageRegion>> AnalyzeLayoutAsync(byte[] imageData, OcrResult ocrResult, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing page layout with {WordCount} words", ocrResult.Words.Count());

        return await Task.Run(() =>
        {
            var regions = new List<PageRegion>();

            // Raggruppa le parole in regioni basate sulla prossimità
            var wordGroups = GroupWordsByProximity(ocrResult.Words.ToList());

            foreach (var group in wordGroups)
            {
                var boundingBox = CalculateBoundingBox(group);
                var text = string.Join(" ", group.Select(w => w.Text));
                var avgConfidence = group.Average(w => w.Confidence);

                // Determina il tipo di regione in base alle caratteristiche
                var regionType = DetermineRegionType(group, boundingBox);

                regions.Add(new PageRegion
                {
                    BoundingBox = boundingBox,
                    Type = regionType,
                    Text = text,
                    Confidence = avgConfidence
                });
            }

            _logger.LogInformation("Identified {RegionCount} regions", regions.Count);
            return regions.AsEnumerable();
        }, cancellationToken);
    }

    public async Task<IEnumerable<PageRegion>> DetectGridsAsync(byte[] imageData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting grids in image");

        return await Task.Run(() =>
        {
            // Implementazione base - da migliorare con analisi delle linee dell'immagine
            var grids = new List<PageRegion>();
            
            _logger.LogWarning("Grid detection not fully implemented yet");
            
            return grids.AsEnumerable();
        }, cancellationToken);
    }

    private List<List<OcrWord>> GroupWordsByProximity(List<OcrWord> words)
    {
        if (!words.Any())
            return [];

        var groups = new List<List<OcrWord>>();
        var currentGroup = new List<OcrWord> { words[0] };
        const int maxVerticalDistance = 20;
        const int maxHorizontalGap = 50;

        for (int i = 1; i < words.Count; i++)
        {
            var prevWord = currentGroup.Last();
            var currentWord = words[i];

            var verticalDistance = Math.Abs(currentWord.BoundingBox.Y - prevWord.BoundingBox.Y);
            var horizontalGap = currentWord.BoundingBox.X - prevWord.BoundingBox.Right;

            // Se le parole sono vicine, aggiungi al gruppo corrente
            if (verticalDistance <= maxVerticalDistance && horizontalGap <= maxHorizontalGap)
            {
                currentGroup.Add(currentWord);
            }
            else
            {
                // Inizia un nuovo gruppo
                groups.Add(currentGroup);
                currentGroup = new List<OcrWord> { currentWord };
            }
        }

        if (currentGroup.Any())
        {
            groups.Add(currentGroup);
        }

        return groups;
    }

    private BoundingBox CalculateBoundingBox(List<OcrWord> words)
    {
        if (!words.Any())
            return new BoundingBox { X = 0, Y = 0, Width = 0, Height = 0 };

        var minX = words.Min(w => w.BoundingBox.X);
        var minY = words.Min(w => w.BoundingBox.Y);
        var maxX = words.Max(w => w.BoundingBox.Right);
        var maxY = words.Max(w => w.BoundingBox.Bottom);

        return new BoundingBox
        {
            X = minX,
            Y = minY,
            Width = maxX - minX,
            Height = maxY - minY
        };
    }

    private RegionType DetermineRegionType(List<OcrWord> words, BoundingBox boundingBox)
    {
        // Euristica semplice per determinare il tipo di regione
        var totalChars = words.Sum(w => w.Text.Length);
        var avgWordLength = totalChars / (double)words.Count;

        // Se ci sono poche parole con caratteri grandi, probabilmente è un titolo
        if (words.Count <= 5 && boundingBox.Height > 40)
        {
            return RegionType.Title;
        }

        // Se ci sono molte parole, probabilmente è testo
        if (words.Count > 10)
        {
            return RegionType.Text;
        }

        // Se l'area è quadrata e compatta, potrebbe essere una griglia
        var aspectRatio = (double)boundingBox.Width / boundingBox.Height;
        if (Math.Abs(aspectRatio - 1.0) < 0.3 && words.Count > 5)
        {
            return RegionType.Grid;
        }

        return RegionType.Unknown;
    }
}
