using System.Globalization;
using System.Text;

namespace DBEnigmistica.Infrastructure.Services.Pdf;

/// <summary>Le pagine che il generatore sa produrre.</summary>
public enum TipoPaginaSintetica
{
    /// <summary>Griglia di cruciverba con numeri e definizioni, in Helvetica (non incorporato).</summary>
    Griglia,
    /// <summary>Giochi brevi in Times-Roman e Courier, font standard non incorporati.</summary>
    FontStandard,
    /// <summary>Testo in Verdana: TrueType non standard e non incorporato.</summary>
    FontNonIncorporato,
    /// <summary>Solo un'immagine, senza testo nativo.</summary>
    SoloImmagine
}

/// <summary>Produce al volo PDF sintetici e deterministici (A4), senza contenuti di Riviste.</summary>
public static class GeneratorePdfSintetici
{
    private const int LarghezzaPunti = 595;
    private const int AltezzaPunti = 842;
    private const int LatoImmagine = 100;

    /// <summary>Un documento con le pagine indicate, oppure con tutte e quattro nell'ordine dell'enum.</summary>
    public static byte[] Documento(params TipoPaginaSintetica[] pagine) =>
        Costruisci(pagine.Length == 0 ? Enum.GetValues<TipoPaginaSintetica>() : pagine, cifrato: false);

    /// <summary>Un PDF con un dizionario di cifratura che nessuna password vuota apre.</summary>
    public static byte[] PdfCifrato() => Costruisci([TipoPaginaSintetica.Griglia], cifrato: true);

    /// <summary>Byte che hanno l'intestazione di un PDF ma non ne sono uno.</summary>
    public static byte[] PdfDanneggiato() => Encoding.ASCII.GetBytes("%PDF-1.4\nquesto non e' un PDF valido\n%%EOF\n");

    /// <summary>Il documento del Set sintetico "set-sintetico-v1": le pagine s01-s06, con testi inventati.</summary>
    public static byte[] SetSinteticoV1() =>
        CostruisciDaContenuti(
        [
            ContenutoGriglia(), ContenutoFontStandard(), ContenutoGiochiBreviVerdana(),
            Contenuto(TipoPaginaSintetica.SoloImmagine), ContenutoRedazionale(), ContenutoAnagrammaEGiocoNuovo()
        ], cifrato: false);

    private static byte[] Costruisci(IReadOnlyList<TipoPaginaSintetica> pagine, bool cifrato) =>
        CostruisciDaContenuti([.. pagine.Select(Contenuto)], cifrato);

    private static byte[] CostruisciDaContenuti(IReadOnlyList<string> contenuti, bool cifrato)
    {
        var pagine = contenuti;
        // Oggetti fissi: 1 catalogo, 2 pagine, 3 risorse, 4-7 font, 8 descrittore Verdana, 9 immagine, 10 cifratura.
        const int primoOggettoPagina = 11;
        var oggetti = new List<byte[]>
        {
            Testo("<< /Type /Catalog /Pages 2 0 R >>"),
            Testo($"<< /Type /Pages /Kids [{string.Join(' ', pagine.Select((_, i) => $"{primoOggettoPagina + i * 2} 0 R"))}] /Count {pagine.Count} >>"),
            Testo("<< /Font << /F1 4 0 R /F2 5 0 R /F3 6 0 R /F4 7 0 R >> /XObject << /Im1 9 0 R >> >>"),
            Testo("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
            Testo("<< /Type /Font /Subtype /Type1 /BaseFont /Times-Roman /Encoding /WinAnsiEncoding >>"),
            Testo("<< /Type /Font /Subtype /Type1 /BaseFont /Courier /Encoding /WinAnsiEncoding >>"),
            Testo("<< /Type /Font /Subtype /TrueType /BaseFont /Verdana /Encoding /WinAnsiEncoding /FirstChar 32 /LastChar 126 " +
                  $"/Widths [{string.Join(' ', Enumerable.Repeat(600, 95))}] /FontDescriptor 8 0 R >>"),
            Testo("<< /Type /FontDescriptor /FontName /Verdana /Flags 32 /FontBBox [-50 -207 1447 1000] /ItalicAngle 0 " +
                  "/Ascent 1005 /Descent -210 /CapHeight 727 /StemV 80 >>"),
            Flusso($"<< /Type /XObject /Subtype /Image /Width {LatoImmagine} /Height {LatoImmagine} /ColorSpace /DeviceGray /BitsPerComponent 8", ImmagineAScacchi()),
            Testo(cifrato
                ? "<< /Filter /Standard /V 1 /R 2 /P -4 /O <" + new string('A', 64) + "> /U <" + new string('B', 64) + "> >>"
                : "null")
        };

        for (var i = 0; i < pagine.Count; i++)
        {
            oggetti.Add(Testo($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {LarghezzaPunti} {AltezzaPunti}] /Resources 3 0 R /Contents {primoOggettoPagina + i * 2 + 1} 0 R >>"));
            oggetti.Add(Flusso("<<", Encoding.Latin1.GetBytes(pagine[i])));
        }

        return Serializza(oggetti, cifrato);
    }

    private static byte[] Serializza(List<byte[]> oggetti, bool cifrato)
    {
        using var uscita = new MemoryStream();
        Scrivi(uscita, "%PDF-1.4\n");
        var offset = new List<int>();
        for (var i = 0; i < oggetti.Count; i++)
        {
            offset.Add((int)uscita.Length);
            Scrivi(uscita, $"{i + 1} 0 obj\n");
            uscita.Write(oggetti[i]);
            Scrivi(uscita, "\nendobj\n");
        }

        var xref = (int)uscita.Length;
        Scrivi(uscita, $"xref\n0 {oggetti.Count + 1}\n0000000000 65535 f \n");
        foreach (var o in offset)
            Scrivi(uscita, $"{o:D10} 00000 n \n");

        var cifratura = cifrato ? " /Encrypt 10 0 R /ID [<00112233445566778899AABBCCDDEEFF> <00112233445566778899AABBCCDDEEFF>]" : string.Empty;
        Scrivi(uscita, $"trailer\n<< /Size {oggetti.Count + 1} /Root 1 0 R{cifratura} >>\nstartxref\n{xref}\n%%EOF\n");
        return uscita.ToArray();
    }

    private static byte[] Testo(string valore) => Encoding.Latin1.GetBytes(valore);

    // Un flusso con il dizionario aperto: chiude il dizionario aggiungendo /Length.
    private static byte[] Flusso(string dizionarioAperto, byte[] dati)
    {
        using var uscita = new MemoryStream();
        Scrivi(uscita, $"{dizionarioAperto} /Length {dati.Length} >>\nstream\n");
        uscita.Write(dati);
        Scrivi(uscita, "\nendstream");
        return uscita.ToArray();
    }

    private static void Scrivi(Stream uscita, string valore) => uscita.Write(Encoding.Latin1.GetBytes(valore));

    private static byte[] ImmagineAScacchi()
    {
        var pixel = new byte[LatoImmagine * LatoImmagine];
        for (var y = 0; y < LatoImmagine; y++)
            for (var x = 0; x < LatoImmagine; x++)
                pixel[y * LatoImmagine + x] = (x / 10 + y / 10) % 2 == 0 ? (byte)(30 + x) : (byte)(220 - y);
        return pixel;
    }

    private static string Contenuto(TipoPaginaSintetica tipo) => tipo switch
    {
        TipoPaginaSintetica.Griglia => ContenutoGriglia(),
        TipoPaginaSintetica.FontStandard => ContenutoFontStandard(),
        TipoPaginaSintetica.FontNonIncorporato => ContenutoFontNonIncorporato(),
        TipoPaginaSintetica.SoloImmagine => "q 400 0 0 400 100 300 cm /Im1 Do Q",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private static string ContenutoGriglia()
    {
        const int lato = 40, x0 = 60, y0 = 540, celle = 7;
        var s = new StringBuilder();
        s.Append(Riga("F1", 18, 60, 780, "CRUCIVERBA DI PROVA"));
        s.Append("0.8 w\n");
        for (var i = 0; i <= celle; i++)
        {
            s.Append($"{x0} {y0 + i * lato} m {x0 + celle * lato} {y0 + i * lato} l S\n");
            s.Append($"{x0 + i * lato} {y0} m {x0 + i * lato} {y0 + celle * lato} l S\n");
        }

        // Caselle nere e numeri di partenza delle definizioni.
        foreach (var (c, r) in new[] { (3, 3), (0, 6), (6, 0), (2, 5), (4, 1) })
            s.Append($"{x0 + c * lato} {y0 + r * lato} {lato} {lato} re f\n");

        var numero = 1;
        foreach (var (c, r) in new[] { (0, 0), (1, 0), (2, 0), (0, 1), (0, 3), (5, 5) })
            s.Append(Riga("F1", 8, x0 + c * lato + 3, y0 + (celle - 1 - r) * lato + lato - 10, numero++.ToString(CultureInfo.InvariantCulture)));

        s.Append(Riga("F1", 14, 60, 500, "ORIZZONTALI"));
        string[] orizzontali = ["1. Capitale d'Italia (4)", "2. Il contrario di no (2)", "3. Fiume di Torino (2)"];
        for (var i = 0; i < orizzontali.Length; i++)
            s.Append(Riga("F1", 11, 60, 480 - i * 16, orizzontali[i]));

        s.Append(Riga("F1", 14, 60, 400, "VERTICALI"));
        string[] verticali = ["1. Lo e' il sole al mattino (5)", "2. Metallo prezioso (3)", "3. Si accende per leggere (5)"];
        for (var i = 0; i < verticali.Length; i++)
            s.Append(Riga("F1", 11, 60, 380 - i * 16, verticali[i]));

        return s.ToString();
    }

    private static string ContenutoFontStandard()
    {
        var s = new StringBuilder();
        s.Append(Riga("F2", 20, 60, 780, "GIOCHI BREVI"));
        string[] anagrammi = ["ANAGRAMMA: AMORE = ROMA (4)", "ANAGRAMMA: TRENO = TERNO (5)", "ANAGRAMMA: CASA = SACA (4)"];
        for (var i = 0; i < anagrammi.Length; i++)
            s.Append(Riga("F2", 14, 60, 730 - i * 24, anagrammi[i]));

        s.Append(Riga("F3", 12, 60, 600, "SCIARADA: CAR + TA = CARTA"));
        s.Append(Riga("F3", 12, 60, 580, "CAMBIO DI LETTERA: MARE - MALE - SALE"));
        s.Append(Riga("F2", 10, 60, 540, "Soluzioni a pagina 62"));
        return s.ToString();
    }

    private static string ContenutoFontNonIncorporato()
    {
        var s = new StringBuilder();
        s.Append(Riga("F4", 20, 60, 780, "Verdana non incorporato"));
        string[] righe =
        [
            "Il testo di questa pagina usa un font TrueType non standard",
            "che il PDF non porta con se': il lettore deve sostituirlo.",
            "Orizzontali: 1. Lago (4)  2. Mare (4)  3. Monte (5)",
            "Verticali: 1. Fiume (5)  2. Collina (7)"
        ];
        for (var i = 0; i < righe.Length; i++)
            s.Append(Riga("F4", 13, 60, 730 - i * 22, righe[i]));
        return s.ToString();
    }

    private static string ContenutoGiochiBreviVerdana()
    {
        var s = new StringBuilder();
        s.Append(Riga("F4", 18, 60, 780, "Giochi brevi in Verdana"));
        s.Append(Riga("F4", 13, 60, 720, "ANAGRAMMA: Il cielo di notte (5 = 5)"));
        s.Append(Riga("F4", 13, 60, 698, "Una stella cerca il suo posto e lo trova."));
        s.Append(Riga("F4", 13, 60, 620, "SCIARADA: Il mio primo e' un saluto, il secondo un metallo (3, 4 = 7)"));
        s.Append(Riga("F4", 13, 60, 598, "Il tutto si vede dopo la pioggia."));
        return s.ToString();
    }

    private static string ContenutoRedazionale()
    {
        var s = new StringBuilder();
        s.Append(Riga("F2", 20, 60, 780, "Lettera della redazione"));
        string[] righe =
        [
            "Cari lettori, questo numero di prova non contiene giochi.",
            "Troverete soltanto un breve saluto della redazione inventata",
            "e l'invito a tornare nel prossimo numero."
        ];
        for (var i = 0; i < righe.Length; i++)
            s.Append(Riga("F2", 12, 60, 730 - i * 20, righe[i]));
        return s.ToString();
    }

    private static string ContenutoAnagrammaEGiocoNuovo()
    {
        var s = new StringBuilder();
        s.Append(Riga("F2", 18, 60, 780, "GIOCHI VARI"));
        s.Append(Riga("F2", 13, 60, 730, "ANAGRAMMA: Il mare calmo (4 = 4)"));
        s.Append(Riga("F2", 13, 60, 710, "Una barca lenta torna al porto."));
        s.Append(Riga("F3", 13, 60, 600, "SCARTO DI PROVA (6/5)"));
        s.Append(Riga("F3", 12, 60, 580, "Il pescatore lascia la rete e resta a riva."));
        return s.ToString();
    }

    private static string Riga(string font, int corpo, int x, int y, string testo) =>
        $"BT /{font} {corpo} Tf {x} {y} Td ({testo.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")}) Tj ET\n";
}
