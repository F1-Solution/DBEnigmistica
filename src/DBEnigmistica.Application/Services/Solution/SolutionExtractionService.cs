using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Domain.Services;

namespace DBEnigmistica.Application.Services.Solution;

public class SolutionExtractionService : ISolutionExtractionService
{
    private readonly IRepository<Page> _pageRepository;
    private readonly IOcrService _ocrService;
    private readonly IAiGameAnalyzer _aiAnalyzer;

    public SolutionExtractionService(
        IRepository<Page> pageRepository,
        IOcrService ocrService,
        IAiGameAnalyzer aiAnalyzer)
    {
        _pageRepository = pageRepository;
        _ocrService = ocrService;
        _aiAnalyzer = aiAnalyzer;
    }

    public async Task<IEnumerable<ExtractedSolution>> ExtractSolutionsFromPageAsync(
        int pageId, 
        CancellationToken cancellationToken = default)
    {
        var page = await _pageRepository.GetByIdAsync(pageId);
        if (page == null || page.OcrText == null)
        {
            return Enumerable.Empty<ExtractedSolution>();
        }

        return await AnalyzeSolutionPatternAsync(page.OcrText, pageId, cancellationToken);
    }

    public async Task<IEnumerable<ExtractedSolution>> AnalyzeSolutionPatternAsync(
        string text, 
        int pageId, 
        CancellationToken cancellationToken = default)
    {
        var solutions = new List<ExtractedSolution>();
        
        await Task.CompletedTask;
        return solutions;
    }
}