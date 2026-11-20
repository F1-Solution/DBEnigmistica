namespace DBEnigmistica.Application.Acquisizione;

/// <summary>Archivio su file system delle Riviste e delle immagini delle pagine; i percorsi restituiti sono relativi alla radice.</summary>
public interface IArchivio
{
    /// <summary>Copia la Rivista come <c>riviste/&lt;impronta&gt;.pdf</c>; se esiste già non fa nulla.</summary>
    Task<string> ArchiviaRivistaAsync(string percorsoPdf, byte[] improntaFile, CancellationToken cancellationToken = default);

    /// <summary>Salva il PNG come <c>pagine/&lt;impronta-rivista&gt;/&lt;nnn&gt;.png</c>, sostituendo un PNG già presente.</summary>
    Task<string> SalvaPaginaAsync(byte[] improntaFile, int numeroPagina, byte[] png, CancellationToken cancellationToken = default);

    /// <summary>Legge un file dell'Archivio; rifiuta percorsi assoluti o con <c>..</c>.</summary>
    Task<byte[]> LeggiAsync(string percorsoRelativo, CancellationToken cancellationToken = default);
}
