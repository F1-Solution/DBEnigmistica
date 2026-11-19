using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Repositories;

public class MagazineRepository : Repository<Magazine>, IMagazineRepository
{
    public MagazineRepository(EnigmisticaDbContext context) : base(context)
    {
    }

    public async Task<Magazine?> GetByTitleAndDateAsync(string title, DateTime publicationDate)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.Title == title && m.PublicationDate.Date == publicationDate.Date);
    }

    public async Task<IEnumerable<Magazine>> GetByYearAsync(int year)
    {
        return await _dbSet
            .Where(m => m.Year == year)
            .OrderBy(m => m.PublicationDate)
            .ToListAsync();
    }

    public async Task<Magazine?> GetWithPagesAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Pages)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Magazine?> GetWithPagesAndGamesAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Pages)
                .ThenInclude(p => p.Games)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddPageAsync(Page page, CancellationToken cancellationToken = default)
    {
        await _context.Set<Page>().AddAsync(page, cancellationToken);
    }

    /// <summary>Rifiuta la cancellazione se una pagina è pagina-soluzione di un Gioco di un'altra Rivista.</summary>
    public override async Task DeleteAsync(int id)
    {
        var rivista = await GetByIdAsync(id);
        if (rivista is null)
            return;

        await VerificaPagineSoluzioneAsync(rivista);
        _dbSet.Remove(rivista);
    }

    private async Task VerificaPagineSoluzioneAsync(Magazine rivista)
    {
        var pagine = await _context.Pages.AsNoTracking()
            .Where(p => p.MagazineId == rivista.Id)
            .ToDictionaryAsync(p => p.Id, p => p.PageNumber);
        if (pagine.Count == 0)
            return;

        var ids = pagine.Keys.ToList();

        var daGiochi = await _context.Games.AsNoTracking()
            .Where(g => g.Page.MagazineId != rivista.Id && g.SolutionPageId != null && ids.Contains(g.SolutionPageId.Value))
            .Select(g => new UsoPagina(g.SolutionPageId!.Value, g.Id, g.Title, g.Page.Magazine.Title, g.Page.Magazine.IssueNumber))
            .ToListAsync();

        var daSoluzioni = await _context.GameSolutions.AsNoTracking()
            .Where(s => s.Game.Page.MagazineId != rivista.Id && s.SolutionPageId != null && ids.Contains(s.SolutionPageId.Value))
            .Select(s => new UsoPagina(s.SolutionPageId!.Value, s.GameId, s.Game.Title, s.Game.Page.Magazine.Title, s.Game.Page.Magazine.IssueNumber))
            .ToListAsync();

        var usi = daGiochi.Concat(daSoluzioni).DistinctBy(u => (u.PaginaId, u.GiocoId)).ToList();
        if (usi.Count == 0)
            return;

        var righe = usi.GroupBy(u => u.PaginaId).OrderBy(g => pagine[g.Key]).Select(g =>
            $"La pagina {pagine[g.Key]} della Rivista «{rivista.Title}» è pagina-soluzione di: " +
            string.Join(", ", g.OrderBy(u => u.GiocoId).Select(Descrivi)) + ".");
        throw new PaginaSoluzioneInUsoException(string.Join(" ", righe));
    }

    private static string Descrivi(UsoPagina u)
    {
        var titolo = string.IsNullOrWhiteSpace(u.GiocoTitolo) ? string.Empty : $" «{u.GiocoTitolo}»";
        var numero = string.IsNullOrWhiteSpace(u.RivistaNumero) ? string.Empty : $" n. {u.RivistaNumero}";
        return $"Gioco {u.GiocoId}{titolo} (Rivista «{u.RivistaTitolo}»{numero})";
    }

    private sealed record UsoPagina(int PaginaId, int GiocoId, string? GiocoTitolo, string RivistaTitolo, string? RivistaNumero);
}
