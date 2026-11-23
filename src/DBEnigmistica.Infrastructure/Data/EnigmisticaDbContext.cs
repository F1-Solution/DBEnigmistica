using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Domain.Catalogo;
using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Registro;
using DBEnigmistica.Infrastructure.Data.Mappings;
using DBEnigmistica.Infrastructure.Prototipo;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Data;

public class EnigmisticaDbContext : DbContext
{
    public EnigmisticaDbContext(DbContextOptions<EnigmisticaDbContext> options)
        : base(options)
    {
    }

    // Schema riviste
    public DbSet<Magazine> Magazines => Set<Magazine>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameSolution> GameSolutions => Set<GameSolution>();

    // Schema tipologie
    public DbSet<GameTypeSchema> GameTypeSchemas => Set<GameTypeSchema>();

    // Schema catalogo
    public DbSet<Soggetto> Soggetti => Set<Soggetto>();
    public DbSet<GiocoOriginale> GiochiOriginali => Set<GiocoOriginale>();
    public DbSet<LicenzaInEntrata> LicenzeInEntrata => Set<LicenzaInEntrata>();
    public DbSet<Cessione> Cessioni => Set<Cessione>();

    // Schema registro
    public DbSet<EventoRegistro> EventiRegistro => Set<EventoRegistro>();

    // Schema prototipo
    public DbSet<Candidato> Candidati => Set<Candidato>();
    public DbSet<ChiamataAiPrototipoRiga> ChiamateAiPrototipo => Set<ChiamataAiPrototipoRiga>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Serve al vincolo di esclusione sulle Cessioni (uuid con &&).
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.ConfiguraRiviste();
        modelBuilder.ConfiguraTipologie();
        modelBuilder.ConfiguraCatalogo();
        modelBuilder.ConfiguraRegistro();
        modelBuilder.ConfiguraPrototipo();
    }
}
