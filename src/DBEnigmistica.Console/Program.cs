using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.Configuration;
using DBEnigmistica.Console.Commands;
using DBEnigmistica.Console.Commands.Solution;
using DBEnigmistica.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.CommandLine;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Configure Serilog  
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting DBEnigmistica CLI");

    // Build host
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            // Register all layers
            services.AddInfrastructure(configuration);
            services.AddAiServices(configuration);
            services.AddApplicationServices();
        })
        .Build();

    // Create root command
    var rootCommand = new RootCommand("DBEnigmistica - Console application per l'estrazione di giochi enigmistici da PDF");

    // Add all commands
    rootCommand.AddCommand(new ScanFolderCommand());
    rootCommand.AddCommand(new ScanFileCommand());
    rootCommand.AddCommand(new AnalyzeGameCommand());
    rootCommand.AddCommand(new AnalyzeGameTextCommand());
    rootCommand.AddCommand(new ListGamesCommand());
    rootCommand.AddCommand(new ExportSchemaCommand());
    rootCommand.AddCommand(new ExportGamesCommand());
    rootCommand.AddCommand(new ExportStatsCommand());
    rootCommand.AddCommand(new MigrateCommand());
    
    // Solution commands
    rootCommand.AddCommand(new LinkSolutionCommand());
    rootCommand.AddCommand(new ListUnsolvedCommand());
    rootCommand.AddCommand(new VerifySolutionCommand());
    rootCommand.AddCommand(new ScanSolutionsCommand());
    rootCommand.AddCommand(new ExportSolutionsCommand());

    // Configure command handler to use DI
    rootCommand.SetHandler((context) =>
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(@"
╔════════════════════════════════════════════════════════════╗
║                                                            ║
║              🎮  D B E n i g m i s t i c a  🎮             ║
║                                                            ║
║         Estrazione giochi da riviste enigmistiche         ║
║                                                            ║
╚════════════════════════════════════════════════════════════╝
");
        System.Console.ResetColor();
        System.Console.WriteLine("Usa --help per vedere i comandi disponibili.");
        System.Console.WriteLine();
        System.Console.WriteLine("Comandi principali:");
        System.Console.WriteLine("  scan-folder        - Scansiona una cartella di PDF");
        System.Console.WriteLine("  scan-file          - Scansiona un singolo PDF");
        System.Console.WriteLine("  analyze-game       - Analizza nuovo tipo di gioco da file");
        System.Console.WriteLine("  analyze-game-text  - Analizza nuovo tipo di gioco da testo");
        System.Console.WriteLine("  list-games         - Lista tipologie di giochi");
        System.Console.WriteLine("  export-schema      - Esporta schemi in JSON");
        System.Console.WriteLine("  export-games       - Esporta giochi in JSON/CSV");
        System.Console.WriteLine("  export-stats       - Esporta statistiche riviste");
        System.Console.WriteLine("  migrate            - Applica migrazioni database");
        System.Console.WriteLine();
        System.Console.WriteLine("Comandi soluzioni:");
        System.Console.WriteLine("  link-solution      - Associa soluzione a gioco");
        System.Console.WriteLine("  list-unsolved      - Lista giochi senza soluzione");
        System.Console.WriteLine("  verify-solution    - Verifica soluzione proposta");
        System.Console.WriteLine("  scan-solutions     - Scansiona pagina per soluzioni");
        System.Console.WriteLine("  export-solutions   - Esporta giochi con soluzioni");
        System.Console.WriteLine();
    });

    // Inject services into command handlers
    using var scope = host.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    // Parse and invoke command
    var exitCode = await rootCommand.InvokeAsync(args);

    Log.Information("Application terminated with exit code {ExitCode}", exitCode);
    return exitCode;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

