using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Data.Mappings;

/// <summary>Schemi riviste e tipologie: le tabelle esistenti, spostate negli schemi per contesto.</summary>
internal static class RivisteMapping
{
    private const string Schema = "riviste";

    public static void ConfiguraRiviste(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Magazine>(entity =>
        {
            entity.ToTable("magazines", Schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SourceFilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IssueNumber).HasMaxLength(50);
            entity.Property(e => e.Periodicity).HasConversion<string>();

            entity.HasIndex(e => new { e.Title, e.PublicationDate });
            entity.HasIndex(e => e.Year);
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("pages", Schema);
            entity.HasKey(e => e.Id);

            entity.HasOne(p => p.Magazine)
                .WithMany(m => m.Pages)
                .HasForeignKey(p => p.MagazineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.MagazineId, e.PageNumber });
        });

        ConfiguraGame(modelBuilder);
        ConfiguraDettagli(modelBuilder);
        ConfiguraSoluzioni(modelBuilder);
    }

    public static void ConfiguraTipologie(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameTypeSchema>(entity =>
        {
            entity.ToTable("game_type_schemas", "tipologie");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameTypeName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SchemaJson).IsRequired();

            entity.HasIndex(e => e.GameTypeName);
            entity.HasIndex(e => e.TableName).IsUnique();
        });
    }

    private static void ConfiguraGame(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            // Origine senza default: un Gioco estratto la dichiara sempre, e il database accetta solo 'rivista'.
            entity.ToTable("games", Schema,
                t => t.HasCheckConstraint("ck_games_origine_rivista", "origine = 'rivista'"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameType).HasConversion<string>();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Origine)
                .HasColumnName("origine")
                .HasConversion(Convertitori.OrigineTesto)
                .IsRequired();

            entity.HasOne(g => g.Page)
                .WithMany(p => p.Games)
                .HasForeignKey(g => g.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Il vincolo è DEFERRABLE INITIALLY DEFERRED; ClientNoAction impedisce a EF di azzerare il riferimento in silenzio.
            entity.HasOne(g => g.SolutionPage)
                .WithMany()
                .HasForeignKey(g => g.SolutionPageId)
                .OnDelete(DeleteBehavior.ClientNoAction);

            entity.HasIndex(e => e.GameType);
            entity.HasIndex(e => e.PageId);
            entity.HasIndex(e => e.SolutionPageId);
        });
    }

    private static void ConfiguraDettagli(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CrosswordGame>(entity =>
        {
            entity.ToTable("crossword_games", Schema);
            entity.HasKey(e => e.Id);
            entity.HasOne(cg => cg.Game).WithOne().HasForeignKey<CrosswordGame>(cg => cg.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.GameId).IsUnique();
        });

        modelBuilder.Entity<RebusGame>(entity =>
        {
            entity.ToTable("rebus_games", Schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            entity.HasOne(rg => rg.Game).WithOne().HasForeignKey<RebusGame>(rg => rg.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.GameId).IsUnique();
        });

        modelBuilder.Entity<LogicGame>(entity =>
        {
            entity.ToTable("logic_games", Schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubType).HasMaxLength(100);
            entity.HasOne(lg => lg.Game).WithOne().HasForeignKey<LogicGame>(lg => lg.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.GameId).IsUnique();
            entity.HasIndex(e => e.SubType);
        });

        modelBuilder.Entity<AnagramGame>(entity =>
        {
            entity.ToTable("anagram_games", Schema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AnagramType).HasMaxLength(100);
            entity.HasOne(ag => ag.Game).WithOne().HasForeignKey<AnagramGame>(ag => ag.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.GameId).IsUnique();
        });
    }

    private static void ConfiguraSoluzioni(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameSolution>(entity =>
        {
            entity.ToTable("game_solutions", Schema);
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SolutionText).IsRequired();
            entity.Property(e => e.SolutionType).HasConversion<string>();

            entity.HasOne(gs => gs.Game).WithMany().HasForeignKey(gs => gs.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Stessa regola di games.SolutionPageId: la pagina in uso non si cancella e EF non azzera il riferimento.
            entity.HasOne(gs => gs.SolutionPage).WithMany().HasForeignKey(gs => gs.SolutionPageId)
                .OnDelete(DeleteBehavior.ClientNoAction);

            entity.HasIndex(e => e.GameId);
            entity.HasIndex(e => e.SolutionPageId);
            entity.HasIndex(e => e.SolutionType);
            entity.HasIndex(e => e.IsVerified);
        });
    }
}
