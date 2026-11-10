using System.CommandLine;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Console.Commands.Solution;

public class LinkSolutionCommand : Command
{
    public LinkSolutionCommand() : base("link-solution", "Associa una soluzione a un gioco")
    {
        var gameIdOption = new Option<int>(
            "--game-id",
            "ID del gioco a cui associare la soluzione"
        ) { IsRequired = true };

        var solutionOption = new Option<string>(
            "--solution",
            "Testo della soluzione"
        ) { IsRequired = true };

        var pageIdOption = new Option<int?>(
            "--page-id",
            "ID della pagina contenente la soluzione (opzionale)"
        );

        var solutionTypeOption = new Option<SolutionType>(
            "--type",
            getDefaultValue: () => SolutionType.InSameIssue,
            "Tipo di soluzione: InSamePage, InSameIssue, InNextIssue, External"
        );

        AddOption(gameIdOption);
        AddOption(solutionOption);
        AddOption(pageIdOption);
        AddOption(solutionTypeOption);

        this.SetHandler(async (gameId, solution, pageId, solutionType, sp) =>
        {
            var matchingService = sp.GetRequiredService<ISolutionMatchingService>();

            try
            {
                System.Console.WriteLine($"\n🔗 Associazione soluzione al gioco #{gameId}...");
                
                var solutionId = await matchingService.LinkSolutionToGameAsync(
                    gameId, 
                    solution, 
                    pageId, 
                    solutionType
                );

                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✓ Soluzione associata con successo! (ID: {solutionId})");
                System.Console.ResetColor();
                System.Console.WriteLine($"  Gioco: #{gameId}");
                System.Console.WriteLine($"  Soluzione: {solution}");
                System.Console.WriteLine($"  Tipo: {solutionType}");
                if (pageId.HasValue)
                {
                    System.Console.WriteLine($"  Pagina: #{pageId}");
                }
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: {ex.Message}");
                System.Console.ResetColor();
            }
        }, gameIdOption, solutionOption, pageIdOption, solutionTypeOption, 
           new ServiceProviderBinder());
    }

    private class ServiceProviderBinder : System.CommandLine.Binding.BinderBase<IServiceProvider>
    {
        protected override IServiceProvider GetBoundValue(System.CommandLine.Binding.BindingContext bindingContext)
        {
            return bindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider 
                ?? throw new InvalidOperationException("Service provider not found");
        }
    }
}