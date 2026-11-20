using System.Security.Cryptography;
using DBEnigmistica.Application.Acquisizione;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Infrastructure.Services.Archivio;

/// <summary>Archivio su file system: nomi derivati solo da impronte e numeri, scrittura atomica con file temporaneo e rinomina.</summary>
public sealed class ArchivioFileSystem(IOptions<ArchivioOptions> options) : IArchivio
{
    private const int LunghezzaImpronta = 32;

    public async Task<string> ArchiviaRivistaAsync(string percorsoPdf, byte[] improntaFile, CancellationToken cancellationToken = default)
    {
        var relativo = $"riviste/{Esadecimale(improntaFile)}.pdf";
        var destinazione = Risolvi(relativo);
        if (File.Exists(destinazione))
            return relativo;

        Directory.CreateDirectory(Path.GetDirectoryName(destinazione)!);
        var temporaneo = $"{destinazione}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var sorgente = File.OpenRead(percorsoPdf))
            await using (var uscita = new FileStream(temporaneo, FileMode.CreateNew, FileAccess.Write))
            {
                await sorgente.CopyToAsync(uscita, cancellationToken);
            }

            await using (var copia = File.OpenRead(temporaneo))
            {
                var calcolata = await SHA256.HashDataAsync(copia, cancellationToken);
                if (!calcolata.AsSpan().SequenceEqual(improntaFile))
                    throw new InvalidDataException("L'impronta del PDF non corrisponde al file: il PDF è cambiato durante la copia.");
            }

            File.Move(temporaneo, destinazione, overwrite: true);
        }
        finally
        {
            File.Delete(temporaneo);
        }

        return relativo;
    }

    public async Task<string> SalvaPaginaAsync(byte[] improntaFile, int numeroPagina, byte[] png, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numeroPagina, 1);
        var relativo = $"pagine/{Esadecimale(improntaFile)}/{numeroPagina:D3}.png";
        var destinazione = Risolvi(relativo);

        Directory.CreateDirectory(Path.GetDirectoryName(destinazione)!);
        var temporaneo = $"{destinazione}.{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllBytesAsync(temporaneo, png, cancellationToken);
            File.Move(temporaneo, destinazione, overwrite: true);
        }
        finally
        {
            File.Delete(temporaneo);
        }

        return relativo;
    }

    public async Task<byte[]> LeggiAsync(string percorsoRelativo, CancellationToken cancellationToken = default) =>
        await File.ReadAllBytesAsync(Risolvi(percorsoRelativo), cancellationToken);

    private static string Esadecimale(byte[] impronta)
    {
        if (impronta is not { Length: LunghezzaImpronta })
            throw new ArgumentException($"L'impronta deve essere di {LunghezzaImpronta} byte (SHA-256).", nameof(impronta));

        return Convert.ToHexStringLower(impronta);
    }

    // Unico punto in cui un percorso diventa un file: rifiuta assoluti, "..", caratteri non validi e uscite dalla radice.
    private string Risolvi(string percorsoRelativo)
    {
        var cartella = options.Value.Cartella;
        if (string.IsNullOrWhiteSpace(cartella))
            throw new InvalidOperationException($"Cartella dell'Archivio non configurata: imposta {ArchivioOptions.EnvironmentKey} o Archivio:Cartella.");

        if (string.IsNullOrWhiteSpace(percorsoRelativo))
            throw new ArgumentException("Percorso dell'Archivio vuoto.", nameof(percorsoRelativo));

        var segmenti = percorsoRelativo.Split('/', '\\');
        if (Path.IsPathRooted(percorsoRelativo) || percorsoRelativo.Contains(':') || percorsoRelativo.StartsWith('/') || percorsoRelativo.StartsWith('\\')
            || segmenti.Any(s => s == ".." || s == "." || s.Length == 0 || s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
        {
            throw new ArgumentException("Percorso dell'Archivio non valido: deve essere relativo e senza '..'.", nameof(percorsoRelativo));
        }

        var radice = Path.GetFullPath(cartella);
        var completo = Path.GetFullPath(Path.Combine(radice, Path.Combine(segmenti)));
        var prefisso = radice.EndsWith(Path.DirectorySeparatorChar) ? radice : radice + Path.DirectorySeparatorChar;
        if (!completo.StartsWith(prefisso, StringComparison.Ordinal))
            throw new ArgumentException("Percorso dell'Archivio fuori dalla radice.", nameof(percorsoRelativo));

        return completo;
    }
}
