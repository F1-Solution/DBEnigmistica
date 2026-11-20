using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using DBEnigmistica.Console.Commands;
using DBEnigmistica.Console.Commands.Solution;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Console.Cli;

/// <summary>Punto d'ingresso in processo della riga di comando: riceve argomenti e scrittori, restituisce il codice d'uscita.</summary>
public static class CliApp
{
    public const string Description =
        "DBEnigmistica - estrazione di giochi enigmistici da riviste in PDF";

    private const string VerboseOptionName = "--verbose";

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        Action<IConfigurationBuilder>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var streams = new CliStreams(output, error);
        // Il runner nasce prima del parsing, quindi legge --verbose direttamente dagli argomenti.
        var verbose = args.Contains(VerboseOptionName, StringComparer.Ordinal);
        var root = BuildRootCommand(streams, new CommandRunner(streams, configure, verbose));

        var invocation = new InvocationConfiguration
        {
            Output = output,
            Error = error,
            EnableDefaultExceptionHandler = true
        };

        return await root.Parse(args).InvokeAsync(invocation, cancellationToken);
    }

    public static RootCommand BuildRootCommand(CliStreams streams, CommandRunner runner)
    {
        var root = new RootCommand(Description);

        root.Options.Add(new Option<bool>(VerboseOptionName)
        {
            Description = "Mostra i dettagli completi degli errori e i log di EF Core",
            Recursive = true
        });

        root.Subcommands.Add(new ScanFolderCommand(streams));
        root.Subcommands.Add(new ScanFileCommand(streams));
        root.Subcommands.Add(new AnalyzeGameCommand(streams));
        root.Subcommands.Add(new AnalyzeGameTextCommand(streams));
        root.Subcommands.Add(new ListGamesCommand(runner));
        root.Subcommands.Add(new ExportSchemaCommand(runner));
        root.Subcommands.Add(new ExportGamesCommand(runner));
        root.Subcommands.Add(new ExportStatsCommand(runner));
        root.Subcommands.Add(new MigrateCommand(runner));
        root.Subcommands.Add(new LinkSolutionCommand(runner));
        root.Subcommands.Add(new ListUnsolvedCommand(runner));
        root.Subcommands.Add(new VerifySolutionCommand(runner));
        root.Subcommands.Add(new ScanSolutionsCommand(streams));
        root.Subcommands.Add(new ExportSolutionsCommand(runner));
        root.Subcommands.Add(new CheckAiCommand(runner));
        root.Subcommands.Add(new VerifyIntegrityCommand(runner));
        root.Subcommands.Add(new DiagnosticaRenderCommand(runner));

        root.SetAction(_ =>
        {
            streams.Out.WriteLine(Description);
            streams.Out.WriteLine("Usa --help per vedere i comandi disponibili.");
            return ExitCodes.Success;
        });

        UseAssemblyVersion(root);
        return root;
    }

    // Il valore predefinito leggerebbe l'assembly di ingresso del processo, che nei test non è questo.
    private static void UseAssemblyVersion(RootCommand root)
    {
        foreach (var option in root.Options.OfType<VersionOption>())
            option.Action = new AssemblyVersionAction();
    }

    private sealed class AssemblyVersionAction : SynchronousCommandLineAction
    {
        public override int Invoke(ParseResult parseResult)
        {
            var assembly = typeof(CliApp).Assembly;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                          ?? assembly.GetName().Version?.ToString(3)
                          ?? "0.0.0";

            parseResult.InvocationConfiguration.Output.WriteLine(version.Split('+')[0]);
            return ExitCodes.Success;
        }
    }
}
