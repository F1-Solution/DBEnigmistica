namespace DBEnigmistica.Domain.Services;

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
