using System.IO.Compression;

namespace DBEnigmistica.Tests.Unit.Pdf;

/// <summary>Lettore PNG minimo e indipendente dal codificatore, per verificare firma, CRC e pixel.</summary>
internal static class PngTestHelper
{
    public static readonly byte[] Firma = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public sealed record Blocco(string Tipo, byte[] Dati, uint CrcLetto, uint CrcCalcolato);

    public sealed record Immagine(int Larghezza, int Altezza, byte[] Rgb, IReadOnlyList<Blocco> Blocchi);

    // CRC-32 bit a bit: volutamente diverso dall'implementazione a tabella del progetto.
    public static uint Crc32(byte[] tipo, byte[] dati)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in tipo.Concat(dati))
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }

        return ~crc;
    }

    public static Immagine Decodifica(byte[] png)
    {
        if (!png.AsSpan(0, 8).SequenceEqual(Firma))
            throw new InvalidDataException("Firma PNG errata.");

        var blocchi = new List<Blocco>();
        var posizione = 8;
        while (posizione < png.Length)
        {
            var lunghezza = (int)ReadUInt32(png, posizione);
            var tipo = png.AsSpan(posizione + 4, 4).ToArray();
            var dati = png.AsSpan(posizione + 8, lunghezza).ToArray();
            var crcLetto = ReadUInt32(png, posizione + 8 + lunghezza);
            blocchi.Add(new Blocco(System.Text.Encoding.ASCII.GetString(tipo), dati, crcLetto, Crc32(tipo, dati)));
            posizione += 12 + lunghezza;
        }

        var ihdr = blocchi[0].Dati;
        var larghezza = (int)ReadUInt32(ihdr, 0);
        var altezza = (int)ReadUInt32(ihdr, 4);
        if (ihdr[8] != 8 || ihdr[9] != 2 || ihdr[10] != 0 || ihdr[11] != 0 || ihdr[12] != 0)
            throw new InvalidDataException("Attesi RGB a 8 bit, senza interlacciamento.");

        using var compresso = new MemoryStream(blocchi.Where(b => b.Tipo == "IDAT").SelectMany(b => b.Dati).ToArray());
        using var zlib = new ZLibStream(compresso, CompressionMode.Decompress);
        using var scompresso = new MemoryStream();
        zlib.CopyTo(scompresso);
        var righe = scompresso.ToArray();

        var stride = larghezza * 3;
        if (righe.Length != altezza * (stride + 1))
            throw new InvalidDataException("Dimensione dei dati decompressi inattesa.");

        return new Immagine(larghezza, altezza, RimuoviFiltri(righe, larghezza, altezza), blocchi);
    }

    private static byte[] RimuoviFiltri(byte[] righe, int larghezza, int altezza)
    {
        var stride = larghezza * 3;
        var rgb = new byte[stride * altezza];
        for (var y = 0; y < altezza; y++)
        {
            var filtro = righe[y * (stride + 1)];
            for (var x = 0; x < stride; x++)
            {
                var grezzo = righe[y * (stride + 1) + 1 + x];
                var sinistra = x >= 3 ? rgb[y * stride + x - 3] : 0;
                rgb[y * stride + x] = filtro switch
                {
                    0 => grezzo,
                    1 => (byte)(grezzo + sinistra),
                    _ => throw new InvalidDataException($"Filtro {filtro} non gestito dal test.")
                };
            }
        }

        return rgb;
    }

    private static uint ReadUInt32(byte[] dati, int offset) =>
        (uint)(dati[offset] << 24 | dati[offset + 1] << 16 | dati[offset + 2] << 8 | dati[offset + 3]);
}
