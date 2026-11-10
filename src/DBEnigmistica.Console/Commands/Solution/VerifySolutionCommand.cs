using System.CommandLine;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Console.Commands.Solution;

public class VerifySolutionCommand : Command
{
    public VerifySolutionCommand() : base("verify-solution", "Verifica o rifiuta una soluzione")
    {
        var solutionIdOption = new Option<int>(
            "--solution-id",
            "ID della soluzione da verificare"
        ) { IsRequired = true };

        var verifiedOption = new Option<bool>(
            "--verified",
            "Indica se la soluzione è corretta (true/false)"
        ) { IsRequired = true };

        var notesOption = new Option<string?>(
            "--notes",
            "Note sulla verifica (opzionale)"
        );

        AddOption(solutionIdOption);
        AddOption(verifiedOption);
        AddOption(notesOption);

        this.SetHandler(async (solutionId, verified, notes, sp) =>
        {
            var matchingService = sp.GetRequiredService<ISolutionMatchingService>();

            try
            {
                System.Console.WriteLine($"\n✓ Verifica della soluzione #{solutionId}...");
                
                await matchingService.VerifySolutionAsync(
                    solutionId,
                    verified,
                    notes
                );

                System.Console.ForegroundColor = verified ? ConsoleColor.Green : ConsoleColor.Yellow;
                var status = verified ? "✓ Approvata" : "✗ Rifiutata";
                System.Console.WriteLine($"{status}");
                System.Console.ResetColor();

                System.Console.WriteLine($"  ID Soluzione: #{solutionId}");
                System.Console.WriteLine($"  Stato: {(verified ? "Verificata" : "Non verificata")}");
                
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    System.Console.WriteLine($"  Note: {notes}");
                }
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Errore: {ex.Message}");
                System.Console.ResetColor();
            }
        }, solutionIdOption, verifiedOption, notesOption, 
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
