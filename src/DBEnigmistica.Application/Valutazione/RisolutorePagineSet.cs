using DBEnigmistica.Application.Acquisizione;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Produce i byte del PDF di un set sintetico dal nome del suo generatore.</summary>
public interface IGeneratorePdfSet
{
    byte[] Genera(string nomeGeneratore);
}

/// <summary>Una pagina del set con l'impronta (esadecimale minuscolo) che è la chiave delle sue Registrazioni, il testo nativo e, se richiesto, il PNG.</summary>
public sealed record PaginaRisolta(PaginaSet Pagina, string Impronta, string TestoNativo = "", byte[]? Png = null);

/// <summary>Calcola l'impronta di ogni pagina di un set leggendo il PDF, dal file o dal generatore.</summary>
public sealed class RisolutorePagineSet(IRenderPagine render, IGeneratorePdfSet generatore)
{
    /// <param name="set">Il set.</param>
    /// <param name="dpiRender">Se indicato, ogni pagina si rende in PNG a questa risoluzione (serve solo dal vivo).</param>
    /// <param name="cancellationToken">Annullamento.</param>
    public async Task<IReadOnlyList<PaginaRisolta>> RisolviAsync(SetValutazione set, int? dpiRender = null, CancellationToken cancellationToken = default)
    {
        var risolte = new List<PaginaRisolta>();
        var temporanei = new List<string>();
        var perPercorso = new Dictionary<string, InfoPdf>(StringComparer.OrdinalIgnoreCase);
        string? temporaneo = null;
        try
        {
            foreach (var pagina in set.Pagine)
            {
                var percorso = LettoreSet.PercorsoPdf(set, pagina);
                if (percorso is null)
                {
                    // Il documento del generatore si scrive una volta sola, in un file temporaneo.
                    if (temporaneo is null)
                    {
                        temporaneo = Path.Combine(Path.GetTempPath(), $"dbe-set-{Guid.NewGuid():N}.pdf");
                        temporanei.Add(temporaneo);
                        await File.WriteAllBytesAsync(temporaneo, generatore.Genera(set.Generatore!), cancellationToken);
                    }

                    percorso = temporaneo;
                }

                if (!perPercorso.TryGetValue(percorso, out var info))
                {
                    info = await LeggiAsync(render, percorso, pagina.Id, cancellationToken);
                    perPercorso[percorso] = info;
                }

                var indice = pagina.NumeroPagina!.Value;
                if (indice > info.Pagine.Count)
                    throw new SetNonValidoException($"Pagina {pagina.Id}: il PDF ha {info.Pagine.Count} pagine, ma numero_pagina è {indice}.");

                var png = dpiRender is { } dpi ? (await render.RenderizzaAsync(percorso, indice, dpi, cancellationToken)).Png : null;
                risolte.Add(new PaginaRisolta(pagina, Convert.ToHexStringLower(info.Pagine[indice - 1].Impronta), info.Pagine[indice - 1].TestoNativo, png));
            }
        }
        finally
        {
            foreach (var file in temporanei)
                File.Delete(file);
        }

        return risolte;
    }

    private static async Task<InfoPdf> LeggiAsync(IRenderPagine render, string percorso, string id, CancellationToken cancellationToken)
    {
        try
        {
            return await render.LeggiAsync(percorso, cancellationToken);
        }
        catch (Exception ex) when (ex is PdfNonLeggibileException or FileNotFoundException or DirectoryNotFoundException)
        {
            throw new SetNonValidoException($"Pagina {id}: il PDF non si può leggere ({ex.GetType().Name}).", ex);
        }
    }
}
