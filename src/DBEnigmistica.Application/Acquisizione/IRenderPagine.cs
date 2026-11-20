namespace DBEnigmistica.Application.Acquisizione;

/// <summary>Perché un PDF non si può leggere.</summary>
public enum MotivoPdfNonLeggibile
{
    Cifrato,
    Danneggiato
}

/// <summary>Il PDF è cifrato o danneggiato e non si può leggere.</summary>
public sealed class PdfNonLeggibileException(MotivoPdfNonLeggibile motivo, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public MotivoPdfNonLeggibile Motivo { get; } = motivo;
}

/// <summary>Lettura e rendering delle pagine di un PDF (porta verso PDFium e PdfPig).</summary>
public interface IRenderPagine
{
    /// <summary>Calcola l'impronta SHA-256 del file e legge dimensioni, impronta e testo nativo di ogni pagina.</summary>
    Task<InfoPdf> LeggiAsync(string percorsoPdf, CancellationToken cancellationToken = default);

    /// <summary>Rende una pagina (numero da 1) come PNG alla risoluzione indicata.</summary>
    Task<PaginaRenderizzata> RenderizzaAsync(string percorsoPdf, int numeroPagina, int dpi, CancellationToken cancellationToken = default);
}
