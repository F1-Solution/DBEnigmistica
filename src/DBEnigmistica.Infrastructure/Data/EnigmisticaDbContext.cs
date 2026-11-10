using DBEnigmistica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Data;

public class EnigmisticaDbContext : DbContext
{
    public EnigmisticaDbContext(DbContextOptions<EnigmisticaDbContext> options) 
        : base(options)
    {
    }

    // DbSets per le entità principali
    public DbSet<Magazine> Magazines => Set<Magazine>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Game> Games => Set<Game>();
    
    // DbSets per le tipologie specifiche di giochi
    public DbSet<CrosswordGame> CrosswordGames => Set<CrosswordGame>();
    public DbSet<RebusGame> RebusGames => Set<RebusGame>();
    public DbSet<LogicGame> LogicGames => Set<LogicGame>();
    public DbSet<AnagramGame> AnagramGames => Set<AnagramGame>();
    
    // DbSet per gli schemi dinamici
    public DbSet<GameTypeSchema> GameTypeSchemas => Set<GameTypeSchema>();
    
    // DbSet per le soluzioni
    public DbSet<GameSolution> GameSolutions => Set<GameSolution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurazione Magazine
        modelBuilder.Entity<Magazine>(entity =>
        {
            entity.ToTable("magazines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SourceFilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IssueNumber).HasMaxLength(50);
            entity.Property(e => e.Periodicity).HasConversion<string>();
            
            entity.HasIndex(e => new { e.Title, e.PublicationDate });
            entity.HasIndex(e => e.Year);
        });

        // Configurazione Page
        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("pages");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(p => p.Magazine)
                .WithMany(m => m.Pages)
                .HasForeignKey(p => p.MagazineId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.MagazineId, e.PageNumber });
        });

        // Configurazione Game
        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("games");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameType).HasConversion<string>();
            entity.Property(e => e.Title).HasMaxLength(200);
            
            entity.HasOne(g => g.Page)
                .WithMany(p => p.Games)
                .HasForeignKey(g => g.PageId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.GameType);
            entity.HasIndex(e => e.PageId);
        });

        // Configurazione CrosswordGame
        modelBuilder.Entity<CrosswordGame>(entity =>
        {
            entity.ToTable("crossword_games");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(cg => cg.Game)
                .WithOne()
                .HasForeignKey<CrosswordGame>(cg => cg.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.GameId).IsUnique();
        });

        // Configurazione RebusGame
        modelBuilder.Entity<RebusGame>(entity =>
        {
            entity.ToTable("rebus_games");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
            
            entity.HasOne(rg => rg.Game)
                .WithOne()
                .HasForeignKey<RebusGame>(rg => rg.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.GameId).IsUnique();
        });

        // Configurazione LogicGame
        modelBuilder.Entity<LogicGame>(entity =>
        {
            entity.ToTable("logic_games");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubType).HasMaxLength(100);
            
            entity.HasOne(lg => lg.Game)
                .WithOne()
                .HasForeignKey<LogicGame>(lg => lg.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.GameId).IsUnique();
            entity.HasIndex(e => e.SubType);
        });

        // Configurazione AnagramGame
        modelBuilder.Entity<AnagramGame>(entity =>
        {
            entity.ToTable("anagram_games");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AnagramType).HasMaxLength(100);
            
            entity.HasOne(ag => ag.Game)
                .WithOne()
                .HasForeignKey<AnagramGame>(ag => ag.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.GameId).IsUnique();
        });

        // Configurazione GameTypeSchema
        modelBuilder.Entity<GameTypeSchema>(entity =>
        {
            entity.ToTable("game_type_schemas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameTypeName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SchemaJson).IsRequired();
            
            entity.HasIndex(e => e.GameTypeName);
            entity.HasIndex(e => e.TableName).IsUnique();
        });

        // Configurazione GameSolution
        modelBuilder.Entity<GameSolution>(entity =>
        {
            entity.ToTable("game_solutions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.SolutionText).IsRequired();
            entity.Property(e => e.SolutionType).HasConversion<string>();
            
            entity.HasOne(gs => gs.Game)
                .WithMany()
                .HasForeignKey(gs => gs.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(gs => gs.SolutionPage)
                .WithMany()
                .HasForeignKey(gs => gs.SolutionPageId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.GameId);
            entity.HasIndex(e => e.SolutionPageId);
            entity.HasIndex(e => e.SolutionType);
            entity.HasIndex(e => e.IsVerified);
        });
    }
}
