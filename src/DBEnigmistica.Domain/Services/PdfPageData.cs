namespace DBEnigmistica.Domain.Services;

/// <summary>
/// Rappresenta una pagina di un PDF con il suo testo nativo
/// </summary>
public class PdfPageData
{
    public required int PageNumber { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public string NativeText { get; init; } = string.Empty;
}
