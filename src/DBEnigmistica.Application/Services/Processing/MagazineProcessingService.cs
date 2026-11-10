using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Application.Services.Processing;

public class MagazineProcessingService
{
    private readonly IPdfService _pdfService;
    private readonly IOcrService _ocrService;
    private readonly ILayoutAnalysisService _layoutService;
    private readonly GameExtractionService _gameExtractionService;
    private readonly IMagazineRepository _magazineRepository;
    private readonly ILogger<MagazineProcessingService> _logger;

    public MagazineProcessingService(
        IPdfService pdfService,
        IOcrService ocrService,
        ILayoutAnalysisService layoutService,
        GameExtractionService gameExtractionService,
        IMagazineRepository magazineRepository,
        ILogger<MagazineProcessingService> logger)
    {
        _pdfService = pdfService;
        _ocrService = ocrService;
        _layoutService = layoutService;
        _gameExtractionService = gameExtractionService;
        _magazineRepository = magazineRepository;
        _logger = logger;
    }

    public async Task<MagazineProcessingResult> ProcessMagazineAsync(
        string pdfPath,
        MagazineMetadata metadata,
        IProgress<ProcessingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new MagazineProcessingResult
        {
            StartTime = DateTime.UtcNow,
            PdfPath = pdfPath
        };

        try
        {
            _logger.LogInformation("Starting magazine processing: {PdfPath}", pdfPath);
            progress?.Report(new ProcessingProgress 
            { 
                Stage = "Inizializzazione", 
                Message = "Apertura file PDF..." 
            });

            // Step 1: Extract pages from PDF
            _logger.LogInformation("Extracting pages from PDF");
            var pdfPagesEnum = await _pdfService.ExtractPagesAsync(pdfPath, cancellationToken);
            var pdfPages = pdfPagesEnum.ToList();
            
            if (pdfPages.Count == 0)
            {
                throw new InvalidOperationException("Nessuna pagina estratta dal PDF");
            }

            result.TotalPages = pdfPages.Count;
            _logger.LogInformation("Extracted {PageCount} pages", pdfPages.Count);

            // Step 2: Create Magazine entity
            var magazine = new Magazine
            {
                Title = metadata.Title ?? Path.GetFileNameWithoutExtension(pdfPath),
                IssueNumber = metadata.IssueNumber?.ToString(),
                PublicationDate = metadata.PublicationDate ?? DateTime.UtcNow,
                Periodicity = metadata.Periodicity,
                Year = (metadata.PublicationDate ?? DateTime.UtcNow).Year,
                SourceFilePath = pdfPath,
                ProcessedDate = DateTime.UtcNow
            };

            await _magazineRepository.AddAsync(magazine, cancellationToken);
            await _magazineRepository.SaveChangesAsync(cancellationToken);
            result.MagazineId = magazine.Id;

            _logger.LogInformation("Created magazine record with ID: {MagazineId}", magazine.Id);

            // Step 3: Process each page
            for (int i = 0; i < pdfPages.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Processing cancelled by user");
                    result.WasCancelled = true;
                    break;
                }

                var pdfPage = pdfPages[i];
                var pageNumber = i + 1;

                try
                {
                    progress?.Report(new ProcessingProgress
                    {
                        Stage = "Elaborazione pagine",
                        Message = $"Elaborazione pagina {pageNumber}/{pdfPages.Count}",
                        CurrentPage = pageNumber,
                        TotalPages = pdfPages.Count,
                        PercentComplete = (int)((pageNumber / (double)pdfPages.Count) * 100)
                    });

                    var pageResult = await ProcessPageAsync(
                        magazine, 
                        pdfPage, 
                        pageNumber,
                        cancellationToken);

                    result.PagesProcessed++;
                    result.GamesExtracted += pageResult.GamesFound;
                    result.PageResults.Add(pageResult);

                    _logger.LogInformation(
                        "Page {PageNumber} processed: {GamesFound} games found",
                        pageNumber, pageResult.GamesFound);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error processing page {PageNumber}", pageNumber);
                    
                    result.PageResults.Add(new PageProcessingResult
                    {
                        PageNumber = pageNumber,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            result.EndTime = DateTime.UtcNow;
            result.Success = true;

            _logger.LogInformation(
                "Magazine processing completed. Pages: {PagesProcessed}/{TotalPages}, Games: {GamesExtracted}",
                result.PagesProcessed, result.TotalPages, result.GamesExtracted);

            progress?.Report(new ProcessingProgress
            {
                Stage = "Completato",
                Message = $"Elaborazione completata: {result.GamesExtracted} giochi estratti",
                PercentComplete = 100
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error processing magazine: {PdfPath}", pdfPath);
            
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            
            return result;
        }
    }

    private async Task<PageProcessingResult> ProcessPageAsync(
        Magazine magazine,
        PdfPageData pdfPage,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var result = new PageProcessingResult
        {
            PageNumber = pageNumber
        };

        try
        {
            // Step 1: Create Page entity
            var page = new Page
            {
                MagazineId = magazine.Id,
                PageNumber = pageNumber,
                Width = pdfPage.Width,
                Height = pdfPage.Height,
                ProcessedDate = DateTime.UtcNow
            };

            await _magazineRepository.AddPageAsync(page, cancellationToken);
            await _magazineRepository.SaveChangesAsync(cancellationToken);

            // Step 2: OCR - Extract text from page image
            _logger.LogDebug("Performing OCR on page {PageNumber}", pageNumber);
            var ocrResult = await _ocrService.RecognizeTextAsync(
                pdfPage.ImageData, 
                cancellationToken);

            if (ocrResult == null || string.IsNullOrWhiteSpace(ocrResult.Text))
            {
                _logger.LogWarning("No text extracted from page {PageNumber}", pageNumber);
                result.Success = true; // Not an error, just empty page
                return result;
            }

            // Step 3: Layout Analysis - Find game regions
            _logger.LogDebug("Analyzing layout for page {PageNumber}", pageNumber);
            var regionsEnum = await _layoutService.AnalyzeLayoutAsync(
                pdfPage.ImageData,
                ocrResult, 
                cancellationToken);

            // Consider Grid and Table regions as potential games
            var gameRegions = regionsEnum
                .Where(r => r.Type == RegionType.Grid || r.Type == RegionType.Table)
                .ToList();

            if (gameRegions.Count == 0)
            {
                _logger.LogInformation("No game regions found on page {PageNumber}", pageNumber);
                result.Success = true;
                return result;
            }

            _logger.LogInformation(
                "Found {RegionCount} potential game regions on page {PageNumber}",
                gameRegions.Count, pageNumber);

            // Step 4: Extract OCR text for each region
            var regionTexts = new Dictionary<PageRegion, string>();
            foreach (var region in gameRegions)
            {
                var regionWords = ocrResult.Words
                    .Where(w => IsWordInRegion(w, region.BoundingBox))
                    .ToList();

                var regionText = string.Join(" ", regionWords.Select(w => w.Text));
                regionTexts[region] = regionText;
            }

            // Step 5: Extract games using AI
            var extractionResults = await _gameExtractionService.ExtractGamesFromPageAsync(
                page,
                gameRegions,
                regionTexts,
                cancellationToken);

            result.GamesFound = extractionResults.Count(r => r.Success);
            result.Success = true;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing page {PageNumber}", pageNumber);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    private bool IsWordInRegion(OcrWord word, BoundingBox region)
    {
        return word.BoundingBox.X >= region.X &&
               word.BoundingBox.Y >= region.Y &&
               word.BoundingBox.X + word.BoundingBox.Width <= region.X + region.Width &&
               word.BoundingBox.Y + word.BoundingBox.Height <= region.Y + region.Height;
    }

    public async Task<List<MagazineProcessingResult>> ProcessFolderAsync(
        string folderPath,
        MagazineMetadata defaultMetadata,
        IProgress<ProcessingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<MagazineProcessingResult>();

        _logger.LogInformation("Processing folder: {FolderPath}", folderPath);

        var pdfFiles = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly);
        
        if (pdfFiles.Length == 0)
        {
            _logger.LogWarning("No PDF files found in folder: {FolderPath}", folderPath);
            return results;
        }

        _logger.LogInformation("Found {FileCount} PDF files", pdfFiles.Length);

        for (int i = 0; i < pdfFiles.Length; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var pdfFile = pdfFiles[i];
            var fileName = Path.GetFileNameWithoutExtension(pdfFile);

            progress?.Report(new ProcessingProgress
            {
                Stage = "Elaborazione cartella",
                Message = $"File {i + 1}/{pdfFiles.Length}: {fileName}",
                CurrentFile = i + 1,
                TotalFiles = pdfFiles.Length
            });

            var metadata = new MagazineMetadata
            {
                Title = defaultMetadata.Title ?? fileName,
                Publisher = defaultMetadata.Publisher,
                IssueNumber = defaultMetadata.IssueNumber,
                PublicationDate = defaultMetadata.PublicationDate,
                Periodicity = defaultMetadata.Periodicity
            };

            var result = await ProcessMagazineAsync(
                pdfFile, 
                metadata, 
                progress, 
                cancellationToken);

            results.Add(result);
        }

        return results;
    }
}

public class MagazineMetadata
{
    public string? Title { get; set; }
    public string? Publisher { get; set; }
    public int? IssueNumber { get; set; }
    public DateTime? PublicationDate { get; set; }
    public MagazinePeriodicity Periodicity { get; set; } = MagazinePeriodicity.Monthly;
}

public class MagazineProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool WasCancelled { get; set; }
    
    public int MagazineId { get; set; }
    public string PdfPath { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public int TotalPages { get; set; }
    public int PagesProcessed { get; set; }
    public int GamesExtracted { get; set; }
    
    public List<PageProcessingResult> PageResults { get; set; } = new();
    
    public TimeSpan Duration => EndTime - StartTime;
}

public class PageProcessingResult
{
    public int PageNumber { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int GamesFound { get; set; }
}

public class ProcessingProgress
{
    public string Stage { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int CurrentFile { get; set; }
    public int TotalFiles { get; set; }
    public int PercentComplete { get; set; }
}
