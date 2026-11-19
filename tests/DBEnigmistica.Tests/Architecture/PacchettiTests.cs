using FluentAssertions;

namespace DBEnigmistica.Tests.Architecture;

/// <summary>Nella fase 0 nessun pacchetto di un Fornitore AI entra nella soluzione (criterio 6).</summary>
public class PacchettiTests
{
    private static string RadiceRepository()
    {
        var cartella = new DirectoryInfo(AppContext.BaseDirectory);
        while (cartella is not null && !File.Exists(Path.Combine(cartella.FullName, "Directory.Packages.props")))
            cartella = cartella.Parent;
        return cartella?.FullName ?? throw new InvalidOperationException("Directory.Packages.props non trovato.");
    }

    [Fact]
    public void DirectoryPackagesProps_NonDichiaraPacchettiAnthropic()
    {
        var percorso = Path.Combine(RadiceRepository(), "Directory.Packages.props");

        var pacchetti = System.Xml.Linq.XDocument.Load(percorso)
            .Descendants("PackageVersion")
            .Select(e => (string?)e.Attribute("Include") ?? string.Empty)
            .Where(n => n.StartsWith("Anthropic", StringComparison.OrdinalIgnoreCase))
            .ToList();

        pacchetti.Should().BeEmpty("la fase 0 non ammette pacchetti di un Fornitore AI");
    }

    [Fact]
    public void Progetti_NonReferenzianoPacchettiAnthropic()
    {
        var radice = RadiceRepository();

        var colpevoli = Directory.EnumerateFiles(radice, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .SelectMany(f => System.Xml.Linq.XDocument.Load(f).Descendants("PackageReference")
                .Select(e => (string?)e.Attribute("Include") ?? string.Empty)
                .Where(n => n.StartsWith("Anthropic", StringComparison.OrdinalIgnoreCase))
                .Select(n => $"{Path.GetFileName(f)}: {n}"))
            .ToList();

        colpevoli.Should().BeEmpty();
    }
}
