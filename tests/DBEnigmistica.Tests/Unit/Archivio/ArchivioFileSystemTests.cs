using System.Security.Cryptography;
using DBEnigmistica.Infrastructure.Services.Archivio;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Tests.Unit.Archivio;

public sealed class ArchivioFileSystemTests : IDisposable
{
    private readonly string _radice = Path.Combine(Path.GetTempPath(), $"dbe-archivio-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_radice))
            Directory.Delete(_radice, recursive: true);
    }

    private ArchivioFileSystem NuovoArchivio() =>
        new(Options.Create(new ArchivioOptions { Cartella = _radice }));

    private string PdfDiProva(byte[] contenuto)
    {
        Directory.CreateDirectory(_radice);
        var percorso = Path.Combine(Path.GetTempPath(), $"dbe-sorgente-{Guid.NewGuid():N}.pdf");
        File.WriteAllBytes(percorso, contenuto);
        return percorso;
    }

    private static byte[] Contenuto(int seme) => Enumerable.Range(0, 5000).Select(i => (byte)(i * seme)).ToArray();

    [Fact]
    public async Task ArchiviaRivista_CopiaIlPdfConNomeUgualeAllImpronta()
    {
        var contenuto = Contenuto(3);
        var sorgente = PdfDiProva(contenuto);
        var impronta = SHA256.HashData(contenuto);

        var relativo = await NuovoArchivio().ArchiviaRivistaAsync(sorgente, impronta);

        relativo.Should().Be($"riviste/{Convert.ToHexStringLower(impronta)}.pdf");
        File.ReadAllBytes(Path.Combine(_radice, "riviste", $"{Convert.ToHexStringLower(impronta)}.pdf")).Should().Equal(contenuto);
    }

    [Fact]
    public async Task ArchiviaRivista_DueVolte_NonDuplicaENonLasciaFileTemporanei()
    {
        var contenuto = Contenuto(5);
        var sorgente = PdfDiProva(contenuto);
        var impronta = SHA256.HashData(contenuto);
        var archivio = NuovoArchivio();

        var primo = await archivio.ArchiviaRivistaAsync(sorgente, impronta);
        var secondo = await archivio.ArchiviaRivistaAsync(sorgente, impronta);

        secondo.Should().Be(primo);
        Directory.GetFiles(_radice, "*", SearchOption.AllDirectories).Should().ContainSingle()
            .Which.Should().EndWith(".pdf");
    }

    [Fact]
    public async Task ArchiviaRivista_ImprontaNonCorrispondente_LanciaENonLasciaNulla()
    {
        var sorgente = PdfDiProva(Contenuto(7));
        var improntaSbagliata = SHA256.HashData(Contenuto(9));

        var act = () => NuovoArchivio().ArchiviaRivistaAsync(sorgente, improntaSbagliata);

        await act.Should().ThrowAsync<InvalidDataException>();
        Directory.GetFiles(_radice, "*", SearchOption.AllDirectories).Should().BeEmpty();
    }

    [Fact]
    public async Task SalvaPagina_ScriveIlPngInCartellaPerImpronta()
    {
        var impronta = SHA256.HashData(Contenuto(2));
        var png = Contenuto(11);

        var relativo = await NuovoArchivio().SalvaPaginaAsync(impronta, 7, png);

        relativo.Should().Be($"pagine/{Convert.ToHexStringLower(impronta)}/007.png");
        File.ReadAllBytes(Path.Combine(_radice, "pagine", Convert.ToHexStringLower(impronta), "007.png")).Should().Equal(png);
    }

    [Fact]
    public async Task SalvaPagina_DueVolte_NonDuplicaESostituisceIlContenuto()
    {
        var impronta = SHA256.HashData(Contenuto(2));
        var archivio = NuovoArchivio();

        await archivio.SalvaPaginaAsync(impronta, 1, Contenuto(3));
        var relativo = await archivio.SalvaPaginaAsync(impronta, 1, Contenuto(4));

        Directory.GetFiles(_radice, "*", SearchOption.AllDirectories).Should().ContainSingle();
        (await archivio.LeggiAsync(relativo)).Should().Equal(Contenuto(4));
    }

    [Fact]
    public async Task Leggi_RestituisceIByteSalvati()
    {
        var archivio = NuovoArchivio();
        var relativo = await archivio.SalvaPaginaAsync(SHA256.HashData(Contenuto(1)), 2, Contenuto(8));

        (await archivio.LeggiAsync(relativo)).Should().Equal(Contenuto(8));
    }

    [Theory]
    [InlineData("../fuori.png")]
    [InlineData("pagine/../../fuori.png")]
    [InlineData("pagine\\..\\..\\fuori.png")]
    [InlineData("/etc/passwd")]
    [InlineData("\\windows\\win.ini")]
    [InlineData("C:\\Windows\\win.ini")]
    [InlineData("C:/Windows/win.ini")]
    [InlineData("")]
    [InlineData("pagine/a\0b.png")]
    public async Task Leggi_PercorsoConPuntiPuntiOAssoluto_ERifiutato(string percorso)
    {
        var act = () => NuovoArchivio().LeggiAsync(percorso);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SalvaPagina_NumeroPaginaNonValido_Lancia()
    {
        var act = () => NuovoArchivio().SalvaPaginaAsync(SHA256.HashData(Contenuto(1)), 0, Contenuto(1));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task SalvaPagina_ImprontaDiLunghezzaErrata_Lancia()
    {
        var act = () => NuovoArchivio().SalvaPaginaAsync(new byte[5], 1, Contenuto(1));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Archivio_SenzaCartellaConfigurata_LanciaErroreDiConfigurazione()
    {
        var archivio = new ArchivioFileSystem(Options.Create(new ArchivioOptions { Cartella = null }));

        var act = () => archivio.SalvaPaginaAsync(SHA256.HashData(Contenuto(1)), 1, Contenuto(1));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Archivio*");
    }
}
