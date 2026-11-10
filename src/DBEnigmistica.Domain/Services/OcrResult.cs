namespace DBEnigmistica.Domain.Services;

/// <summary>
/// Risultato dell'OCR su una pagina
/// </summary>
public class OcrResult
{
    public required string Text { get; init; }
    public required float Confidence { get; init; }
    public IEnumerable<OcrWord> Words { get; init; } = [];
}

/// <summary>
/// Una parola riconosciuta dall'OCR
/// </summary>
public class OcrWord
{
    public required string Text { get; init; }
    public required float Confidence { get; init; }
    public required BoundingBox BoundingBox { get; init; }
}

/// <summary>
/// Bounding box di un elemento sulla pagina
/// </summary>
public class BoundingBox
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }

    public int Right => X + Width;
    public int Bottom => Y + Height;
}
