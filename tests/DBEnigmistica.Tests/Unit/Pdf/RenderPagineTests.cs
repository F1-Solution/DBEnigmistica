using DBEnigmistica.Application.Acquisizione;
using DBEnigmistica.Infrastructure.Services.Pdf;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Pdf;

public class RenderPagineTests
{
    // Le pagine sintetiche sono A4 (595x842 punti): a 150 DPI 1240x1754, a 200 DPI 1653x2339.
    private static readonly PdfRenderPagine Render = new();

    private static byte[] Documento() => GeneratorePdfSintetici.Documento();

    [Theory]
    [InlineData(150, 1240, 1754)]
    [InlineData(200, 1653, 2339)]
    public async Task Renderizza_PaginaSintetica_HaLeDimensioniAttese(int dpi, int larghezza, int altezza)
    {
        var pagina = await Render.RenderizzaAsync(Documento(), 1, dpi);

        pagina.Larghezza.Should().Be(larghezza);
        pagina.Altezza.Should().Be(altezza);
        pagina.Dpi.Should().Be(dpi);
        var png = PngTestHelper.Decodifica(pagina.Png);
        png.Larghezza.Should().Be(larghezza);
        png.Altezza.Should().Be(altezza);
        png.Blocchi.Should().OnlyContain(b => b.CrcLetto == b.CrcCalcolato);
    }

    [Theory]
    [InlineData(1, "griglia")]
    [InlineData(2, "font standard non incorporati")]
    [InlineData(3, "font TrueType non incorporato e non standard")]
    [InlineData(4, "solo immagine")]
    public async Task Renderizza_PagineConFontNonIncorporati_HannoPixelNonBianchi(int numero, string descrizione)
    {
        var pagina = await Render.RenderizzaAsync(Documento(), numero, 150);

        // Una pagina con testo o disegni resa senza errori ma bianca indicherebbe font mancanti.
        pagina.PixelNonBianchi.Should().BeGreaterThan(2_000, $"la pagina {numero} ({descrizione}) deve avere contenuto visibile");
    }

    [Fact]
    public async Task Renderizza_StessaPaginaDueVolte_DaGliStessiByteDelPng()
    {
        var primo = await Render.RenderizzaAsync(Documento(), 1, 150);
        var secondo = await Render.RenderizzaAsync(Documento(), 1, 150);

        secondo.Png.Should().Equal(primo.Png);
    }

    [Fact]
    public async Task Leggi_StessoPdfDueVolte_DaLeStesseImpronte()
    {
        var primo = await Render.LeggiAsync(Documento());
        var secondo = await Render.LeggiAsync(Documento());

        secondo.ImprontaFile.Should().Equal(primo.ImprontaFile);
        secondo.Pagine.Select(p => Convert.ToHexString(p.Impronta))
            .Should().Equal(primo.Pagine.Select(p => Convert.ToHexString(p.Impronta)));
    }

    [Fact]
    public async Task Leggi_PagineDiverse_HannoImpronteDiverse()
    {
        var info = await Render.LeggiAsync(Documento());

        info.Pagine.Should().HaveCount(4);
        info.Pagine.Select(p => Convert.ToHexString(p.Impronta)).Distinct().Should().HaveCount(4);
        info.Pagine.Should().OnlyContain(p => p.Impronta.Length == 32);
        info.ImprontaFile.Should().HaveCount(32);
    }

    [Fact]
    public async Task Leggi_StessaPaginaInDocumentiDiversi_HaLaStessaImpronta()
    {
        var intero = await Render.LeggiAsync(Documento());
        var soloGriglia = await Render.LeggiAsync(GeneratorePdfSintetici.Documento(TipoPaginaSintetica.Griglia));

        soloGriglia.ImprontaFile.Should().NotEqual(intero.ImprontaFile);
        soloGriglia.Pagine[0].Impronta.Should().Equal(intero.Pagine[0].Impronta);
    }

    [Fact]
    public async Task Leggi_TestoNativo_PresenteNelleGrigliaEAssenteNellaPaginaSoloImmagine()
    {
        var info = await Render.LeggiAsync(Documento());

        info.Pagine[0].TestoNativo.Should().Contain("ORIZZONTALI");
        info.Pagine[2].TestoNativo.Should().Contain("Verdana");
        info.Pagine[3].TestoNativo.Should().BeEmpty();
    }

    [Fact]
    public async Task Leggi_RestituisceLeDimensioniInPunti()
    {
        var info = await Render.LeggiAsync(Documento());

        info.Pagine[0].LarghezzaPunti.Should().BeApproximately(595, 0.5);
        info.Pagine[0].AltezzaPunti.Should().BeApproximately(842, 0.5);
        info.Pagine.Select(p => p.Numero).Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public async Task Leggi_DaFile_DaLoStessoRisultatoDaiByte()
    {
        var percorso = Path.Combine(Path.GetTempPath(), $"dbe-{Guid.NewGuid():N}.pdf");
        await File.WriteAllBytesAsync(percorso, Documento());
        try
        {
            var daFile = await ((IRenderPagine)Render).LeggiAsync(percorso);
            var daByte = await Render.LeggiAsync(Documento());

            daFile.ImprontaFile.Should().Equal(daByte.ImprontaFile);
        }
        finally
        {
            File.Delete(percorso);
        }
    }

    [Fact]
    public async Task PdfCifrato_LanciaErroreChiaroDiPdfCifrato()
    {
        var act = () => Render.RenderizzaAsync(GeneratorePdfSintetici.PdfCifrato(), 1, 150);

        var eccezione = (await act.Should().ThrowAsync<PdfNonLeggibileException>()).Which;
        eccezione.Motivo.Should().Be(MotivoPdfNonLeggibile.Cifrato);
        eccezione.Message.Should().Contain("cifrato");
    }

    [Fact]
    public async Task LeggiPdfCifrato_LanciaErroreChiaroDiPdfCifrato()
    {
        var act = () => Render.LeggiAsync(GeneratorePdfSintetici.PdfCifrato());

        (await act.Should().ThrowAsync<PdfNonLeggibileException>()).Which.Motivo.Should().Be(MotivoPdfNonLeggibile.Cifrato);
    }

    [Fact]
    public async Task PdfDanneggiato_LanciaErroreChiaroDiPdfDanneggiato()
    {
        var act = () => Render.RenderizzaAsync(GeneratorePdfSintetici.PdfDanneggiato(), 1, 150);

        var eccezione = (await act.Should().ThrowAsync<PdfNonLeggibileException>()).Which;
        eccezione.Motivo.Should().Be(MotivoPdfNonLeggibile.Danneggiato);
        eccezione.Message.Should().Contain("danneggiato");
    }

    [Fact]
    public async Task LeggiPdfDanneggiato_LanciaErroreChiaroDiPdfDanneggiato()
    {
        var act = () => Render.LeggiAsync(GeneratorePdfSintetici.PdfDanneggiato());

        (await act.Should().ThrowAsync<PdfNonLeggibileException>()).Which.Motivo.Should().Be(MotivoPdfNonLeggibile.Danneggiato);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public async Task Renderizza_NumeroPaginaFuoriIntervallo_Lancia(int numero)
    {
        var act = () => Render.RenderizzaAsync(Documento(), numero, 150);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(5000)]
    public async Task Renderizza_DpiFuoriIntervallo_Lancia(int dpi)
    {
        var act = () => Render.RenderizzaAsync(Documento(), 1, dpi);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Renderizza_FileInesistente_LanciaFileNotFound()
    {
        var act = () => ((IRenderPagine)Render).RenderizzaAsync(Path.Combine(Path.GetTempPath(), "non-esiste.pdf"), 1, 150);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public void GeneratorePdfSintetici_ProduceSempreGliStessiByte()
    {
        GeneratorePdfSintetici.Documento().Should().Equal(GeneratorePdfSintetici.Documento());
    }

    [Fact]
    public async Task Leggi_ConPdfMutatiACaso_DaSoloRisultatoOErroreDiPdfNonLeggibile()
    {
        var originale = Documento();
        var casuale = new Random(20261120);
        var inattesi = new List<string>();

        for (var i = 0; i < 300; i++)
        {
            var mutato = Muta(originale, casuale);
            try
            {
                await Render.LeggiAsync(mutato);
            }
            catch (PdfNonLeggibileException)
            {
                // Esito previsto per un PDF rovinato.
            }
            catch (Exception ex)
            {
                inattesi.Add($"iterazione {i}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        inattesi.Should().BeEmpty("ogni PDF rovinato deve dare un errore di PDF non leggibile");
    }

    [Fact]
    public async Task Renderizza_ConPdfMutatiACaso_DaSoloRisultatoOErroreDiPdfNonLeggibile()
    {
        var originale = Documento();
        var casuale = new Random(7);
        var inattesi = new List<string>();

        for (var i = 0; i < 40; i++)
        {
            try
            {
                await Render.RenderizzaAsync(Muta(originale, casuale), 1 + casuale.Next(4), 150);
            }
            catch (Exception ex) when (ex is PdfNonLeggibileException or ArgumentOutOfRangeException)
            {
                // Pagina fuori intervallo o PDF rovinato: previsto.
            }
            catch (Exception ex)
            {
                inattesi.Add($"iterazione {i}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        inattesi.Should().BeEmpty();
    }

    // Tronca oppure sovrascrive da 1 a 40 byte in punti a caso del file.
    private static byte[] Muta(byte[] originale, Random casuale)
    {
        if (casuale.Next(4) == 0)
            return originale[..casuale.Next(1, originale.Length)];

        var copia = (byte[])originale.Clone();
        for (var n = casuale.Next(1, 41); n > 0; n--)
            copia[casuale.Next(copia.Length)] = (byte)casuale.Next(256);

        return copia;
    }

    [Fact]
    public async Task Renderizza_PaginaCheSuperaIlLimiteDiPixel_ERifiutataConUnMessaggioChiaro()
    {
        // 300 DPI su una pagina A4 sta nel limite (A3 a 300 DPI); 600 DPI sull'A4 non ci sta.
        var act = () => Render.RenderizzaAsync(Documento(), 1, 600);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("troppo grande");
    }
}
