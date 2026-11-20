namespace DBEnigmistica.Application.Acquisizione;

/// <summary>Esito della lettura di un PDF: impronta del file e informazioni di ogni pagina.</summary>
public sealed record InfoPdf(byte[] ImprontaFile, IReadOnlyList<InfoPagina> Pagine);

/// <summary>Una pagina del PDF: dimensioni in punti, impronta del contenuto e testo nativo.</summary>
public sealed record InfoPagina(int Numero, double LarghezzaPunti, double AltezzaPunti, byte[] Impronta, string TestoNativo);

/// <summary>Una pagina resa come PNG; <paramref name="PixelNonBianchi"/> serve alla diagnostica del rendering.</summary>
public sealed record PaginaRenderizzata(int Numero, int Larghezza, int Altezza, int Dpi, byte[] Png, long PixelNonBianchi);
