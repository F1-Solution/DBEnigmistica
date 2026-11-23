using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
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

            entity.Property(e => e.Impronta).HasColumnName("impronta").HasColumnType("bytea");

            entity.HasIndex(e => e.Impronta).IsUnique();
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

            entity.Property(e => e.Impronta).HasColumnName("impronta").HasColumnType("bytea");

            entity.HasIndex(e => e.Impronta).IsUnique();
            entity.HasIndex(e => new { e.MagazineId, e.PageNumber });
        });

        ConfiguraGame(modelBuilder);
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
            entity.ToTable("games", Schema, t =>
            {
                t.HasCheckConstraint("ck_games_origine_rivista", "origine = 'rivista'");
                t.HasCheckConstraint("ck_games_stato_tipologia", "stato_tipologia IN ('nota', 'proposta')");
                t.HasCheckConstraint("ck_games_versione_tipologia", "versione_tipologia >= 0");
                t.HasCheckConstraint("ck_games_riquadro",
                    "(riquadro_x IS NULL OR riquadro_x BETWEEN 0 AND 1) AND (riquadro_y IS NULL OR riquadro_y BETWEEN 0 AND 1) " +
                    "AND (riquadro_larghezza IS NULL OR riquadro_larghezza BETWEEN 0 AND 1) AND (riquadro_altezza IS NULL OR riquadro_altezza BETWEEN 0 AND 1)");
                t.HasCheckConstraint("ck_games_confidenza", "confidenza IS NULL OR confidenza BETWEEN 0 AND 1");
                t.HasCheckConstraint("ck_games_trascrizione_proposta", "trascrizione_proposta IS NULL OR stato_tipologia = 'proposta'");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipologia).HasColumnName("tipologia").HasMaxLength(30).IsRequired();
            entity.Property(e => e.VersioneTipologia).HasColumnName("versione_tipologia");
            entity.Property(e => e.StatoTipologia).HasColumnName("stato_tipologia").HasMaxLength(10)
                .HasConversion(v => v == StatoTipologia.Proposta ? "proposta" : "nota",
                    v => v == "proposta" ? StatoTipologia.Proposta : StatoTipologia.Nota);
            entity.Property(e => e.RiquadroX).HasColumnName("riquadro_x").HasColumnType("numeric(6,5)");
            entity.Property(e => e.RiquadroY).HasColumnName("riquadro_y").HasColumnType("numeric(6,5)");
            entity.Property(e => e.RiquadroLarghezza).HasColumnName("riquadro_larghezza").HasColumnType("numeric(6,5)");
            entity.Property(e => e.RiquadroAltezza).HasColumnName("riquadro_altezza").HasColumnType("numeric(6,5)");
            entity.Property(e => e.Confidenza).HasColumnName("confidenza");
            entity.Property(e => e.Avvisi).HasColumnName("avvisi").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.TrascrizioneProposta).HasColumnName("trascrizione_proposta").HasColumnType("text");
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

            entity.HasIndex(e => e.Tipologia);
            entity.HasIndex(e => e.PageId);
            entity.HasIndex(e => e.SolutionPageId);
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
