namespace DBEnigmistica.Domain.Services;

/// <summary>
/// Rappresenta una pagina estratta da un PDF
/// </summary>
public class PdfPageData
{
    public required int PageNumber { get; init; }
    public required byte[] ImageData { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required string ImageFormat { get; init; }
}
