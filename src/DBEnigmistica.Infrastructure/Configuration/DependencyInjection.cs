using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Repositories;
using DBEnigmistica.Infrastructure.Services.Pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<EnigmisticaDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IMagazineRepository, MagazineRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameTypeSchemaRepository, GameTypeSchemaRepository>();
        services.AddScoped<IGameSolutionRepository, GameSolutionRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Services - PDF
        services.AddScoped<IPdfService, PdfService>();

        return services;
    }
}
