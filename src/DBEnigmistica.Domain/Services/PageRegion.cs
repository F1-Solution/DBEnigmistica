namespace DBEnigmistica.Domain.Services;

/// <summary>
/// Rappresenta una regione identificata sulla pagina
/// </summary>
public class PageRegion
{
    public required BoundingBox BoundingBox { get; init; }
    public required RegionType Type { get; init; }
    public string? Text { get; init; }
    public float Confidence { get; init; }
}

/// <summary>
/// Tipo di regione sulla pagina
/// </summary>
public enum RegionType
{
    Unknown,
    Title,
    Text,
    Image,
    Grid,
    Table,
    Diagram
}
