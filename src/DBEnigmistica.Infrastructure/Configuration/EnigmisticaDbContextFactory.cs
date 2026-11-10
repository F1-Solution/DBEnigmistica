using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DBEnigmistica.Infrastructure.Configuration;

/// <summary>
/// Factory per la creazione del DbContext a design-time (per le migrazioni EF Core)
/// </summary>
public class EnigmisticaDbContextFactory : IDesignTimeDbContextFactory<EnigmisticaDbContext>
{
    public EnigmisticaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EnigmisticaDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Imposta la variabile d'ambiente ConnectionStrings__DefaultConnection con la stringa di connessione per le migrazioni.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new EnigmisticaDbContext(optionsBuilder.Options);
    }
}
