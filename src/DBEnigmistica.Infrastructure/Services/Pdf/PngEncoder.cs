using System.Buffers.Binary;
using System.IO.Compression;

namespace DBEnigmistica.Infrastructure.Services.Pdf;

/// <summary>Codificatore PNG RGB a 8 bit, deterministico: stessi pixel, stessi byte.</summary>
public static class PngEncoder
{
    private static readonly byte[] Firma = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly uint[] TabellaCrc = CreaTabellaCrc();

    public static byte[] CodificaRgb(ReadOnlySpan<byte> rgb, int larghezza, int altezza)
    {
        Convalida(larghezza, altezza);
        if (rgb.Length != larghezza * altezza * 3)
            throw new ArgumentException("La lunghezza dei pixel non corrisponde alle dimensioni.", nameof(rgb));

        return Codifica(rgb, larghezza, altezza);
    }

    /// <summary>Codifica pixel BGRx (come quelli di PDFium); i byte di riempimento di ogni riga sono ignorati.</summary>
    public static byte[] CodificaBgrx(ReadOnlySpan<byte> bgrx, int larghezza, int altezza, int stride)
    {
        Convalida(larghezza, altezza);
        if (stride < larghezza * 4 || bgrx.Length < (long)stride * altezza)
            throw new ArgumentException("Stride o lunghezza dei pixel non validi.", nameof(bgrx));

        var rgb = new byte[larghezza * altezza * 3];
        for (var y = 0; y < altezza; y++)
        {
            var sorgente = bgrx.Slice(y * stride, larghezza * 4);
            var destinazione = rgb.AsSpan(y * larghezza * 3, larghezza * 3);
            for (var x = 0; x < larghezza; x++)
            {
                destinazione[x * 3] = sorgente[x * 4 + 2];
                destinazione[x * 3 + 1] = sorgente[x * 4 + 1];
                destinazione[x * 3 + 2] = sorgente[x * 4];
            }
        }

        return Codifica(rgb, larghezza, altezza);
    }

    private static byte[] Codifica(ReadOnlySpan<byte> rgb, int larghezza, int altezza)
    {
        var stride = larghezza * 3;
        var filtrati = new byte[altezza * (stride + 1)];
        for (var y = 0; y < altezza; y++)
        {
            var corrente = rgb.Slice(y * stride, stride);
            var uscita = filtrati.AsSpan(y * (stride + 1));
            uscita[0] = 1; // filtro Sub: ogni byte meno quello del pixel a sinistra.
            for (var x = 0; x < stride; x++)
                uscita[1 + x] = (byte)(corrente[x] - (x >= 3 ? corrente[x - 3] : 0));
        }

        using var compresso = new MemoryStream();
        using (var zlib = new ZLibStream(compresso, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(filtrati);

        using var png = new MemoryStream();
        png.Write(Firma);
        var ihdr = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdr, (uint)larghezza);
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(4), (uint)altezza);
        ihdr[8] = 8; // 8 bit per canale
        ihdr[9] = 2; // RGB
        ScriviBlocco(png, "IHDR"u8, ihdr);
        ScriviBlocco(png, "IDAT"u8, compresso.GetBuffer().AsSpan(0, (int)compresso.Length));
        ScriviBlocco(png, "IEND"u8, []);
        return png.ToArray();
    }

    private static void ScriviBlocco(Stream destinazione, ReadOnlySpan<byte> tipo, ReadOnlySpan<byte> dati)
    {
        Span<byte> quattro = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(quattro, (uint)dati.Length);
        destinazione.Write(quattro);
        destinazione.Write(tipo);
        destinazione.Write(dati);

        var crc = 0xFFFFFFFFu;
        crc = AggiornaCrc(crc, tipo);
        crc = AggiornaCrc(crc, dati);
        BinaryPrimitives.WriteUInt32BigEndian(quattro, ~crc);
        destinazione.Write(quattro);
    }

    private static uint AggiornaCrc(uint crc, ReadOnlySpan<byte> dati)
    {
        foreach (var b in dati)
            crc = TabellaCrc[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc;
    }

    private static uint[] CreaTabellaCrc()
    {
        var tabella = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            tabella[n] = c;
        }

        return tabella;
    }

    private static void Convalida(int larghezza, int altezza)
    {
        if (larghezza <= 0 || altezza <= 0)
            throw new ArgumentException("Larghezza e altezza devono essere positive.");
    }
}
