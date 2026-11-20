using DBEnigmistica.Infrastructure.Services.Pdf;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Pdf;

public class PngEncoderTests
{
    private static byte[] Gradiente(int larghezza, int altezza)
    {
        var rgb = new byte[larghezza * altezza * 3];
        for (var y = 0; y < altezza; y++)
            for (var x = 0; x < larghezza; x++)
            {
                var i = (y * larghezza + x) * 3;
                rgb[i] = (byte)(x * 7 + y);
                rgb[i + 1] = (byte)(y * 5);
                rgb[i + 2] = (byte)(x ^ y);
            }

        return rgb;
    }

    [Fact]
    public void Codifica_HaFirmaIhdrPrimoEIendUltimoConCrcNoto()
    {
        var png = PngEncoder.CodificaRgb(Gradiente(13, 9), 13, 9);

        png.AsSpan(0, 8).ToArray().Should().Equal(PngTestHelper.Firma);
        var immagine = PngTestHelper.Decodifica(png);
        immagine.Blocchi.First().Tipo.Should().Be("IHDR");
        immagine.Blocchi.Last().Tipo.Should().Be("IEND");
        // CRC-32 di "IEND" senza dati: costante nota di ogni PNG valido.
        png.TakeLast(12).Should().Equal(0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82);
    }

    [Fact]
    public void Codifica_OgniBloccoHaCrcCorretto()
    {
        var immagine = PngTestHelper.Decodifica(PngEncoder.CodificaRgb(Gradiente(64, 40), 64, 40));

        immagine.Blocchi.Should().OnlyContain(b => b.CrcLetto == b.CrcCalcolato);
    }

    [Fact]
    public void Codifica_DatiDecompressiUgualiAiPixel()
    {
        var rgb = Gradiente(31, 17);

        var immagine = PngTestHelper.Decodifica(PngEncoder.CodificaRgb(rgb, 31, 17));

        immagine.Larghezza.Should().Be(31);
        immagine.Altezza.Should().Be(17);
        immagine.Rgb.Should().Equal(rgb);
    }

    [Fact]
    public void CodificaBgrx_ConvertePixelEIgnoraIlPadding()
    {
        // 2x2 pixel BGRx con stride di 12 byte (4 di padding per riga).
        byte[] bgrx =
        [
            10, 20, 30, 255, 40, 50, 60, 255, 9, 9, 9, 9,
            70, 80, 90, 255, 100, 110, 120, 255, 9, 9, 9, 9
        ];

        var immagine = PngTestHelper.Decodifica(PngEncoder.CodificaBgrx(bgrx, 2, 2, 12));

        immagine.Rgb.Should().Equal(30, 20, 10, 60, 50, 40, 90, 80, 70, 120, 110, 100);
    }

    [Fact]
    public void Codifica_StessoInputDueVolte_DaGliStessiByte()
    {
        var rgb = Gradiente(120, 80);

        var primo = PngEncoder.CodificaRgb(rgb, 120, 80);
        var secondo = PngEncoder.CodificaRgb(rgb, 120, 80);

        secondo.Should().Equal(primo);
    }

    [Theory]
    [InlineData(0, 5, 0)]
    [InlineData(5, 0, 0)]
    [InlineData(5, 5, 10)]
    public void Codifica_ParametriNonValidi_Lancia(int larghezza, int altezza, int lunghezza)
    {
        var act = () => PngEncoder.CodificaRgb(new byte[lunghezza], larghezza, altezza);

        act.Should().Throw<ArgumentException>();
    }
}
