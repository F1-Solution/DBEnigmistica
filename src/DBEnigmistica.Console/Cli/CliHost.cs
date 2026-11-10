using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.Configuration;
using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Console.Commands;
using DBEnigmistica.Console.Configuration;
using DBEnigmistica.Console.Presentation;
using DBEnigmistica.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Spectre.Console;

namespace DBEnigmistica.Console.Cli;

/// <summary>Costruisce il Generic Host di un'esecuzione, con configurazione e opzioni validate all'avvio.</summary>
public static class CliHost
{
    public static IHost Build(
        HostRequirements requirements,
        CliStreams streams,
        Action<IConfigurationBuilder>? configure)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true,
            ContentRootPath = AppContext.BaseDirectory,
            EnvironmentName = ResolveEnvironmentName()
        });

        ConfigureConfiguration(builder, configure);
        ConfigureLogging(builder);
        ConfigureServices(builder.Services, builder.Configuration, requirements, streams);

        return builder.Build();
    }

    private static string ResolveEnvironmentName() =>
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environments.Production;

    // Ordine delle fonti: json, json d'ambiente, user-secrets (solo Development), variabili d'ambiente.
    private static void ConfigureConfiguration(HostApplicationBuilder builder, Action<IConfigurationBuilder>? configure)
    {
        var config = builder.Configuration;
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        config.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

        if (builder.Environment.IsDevelopment())
            config.AddUserSecrets(typeof(CliHost).Assembly, optional: true);

        config.AddEnvironmentVariables();
        configure?.Invoke(config);
    }

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            // Gli errori di avvio (es. configurazione non valida) sono riportati da CommandRunner senza stack trace.
            .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Fatal)
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger, dispose: true);
    }

    private static void ConfigureServices(
        IServiceCollection services,
        IConfiguration configuration,
        HostRequirements requirements,
        CliStreams streams)
    {
        services.AddSingleton(streams);
        services.AddSingleton(streams.Console);
        services.AddScoped<ISchemaReviewInteraction, ConsoleSchemaReviewInteraction>();

        services.AddInfrastructure(configuration);
        services.AddAiServices(configuration);
        services.AddApplicationServices();
        services.AddHandlers();

        AddValidatedOptions(services, requirements);
    }

    private static void AddValidatedOptions(IServiceCollection services, HostRequirements requirements)
    {
        if (requirements.HasFlag(HostRequirements.Database))
        {
            services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();
            services.AddOptions<DatabaseOptions>()
                .BindConfiguration(DatabaseOptions.SectionName)
                .ValidateOnStart();
        }

        if (requirements.HasFlag(HostRequirements.Ai))
        {
            services.AddSingleton<IValidateOptions<AnthropicOptions>, AnthropicOptionsValidator>();
            services.AddOptions<AnthropicOptions>()
                .BindConfiguration(AnthropicOptions.SectionName)
                .ValidateOnStart();
        }
    }
}
