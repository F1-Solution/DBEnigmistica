using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Console.Commands;
using DBEnigmistica.Infrastructure.Services.Pdf;
using DBEnigmistica.Infrastructure.Services.Registrazioni;
using DBEnigmistica.Tests.Unit.Creazione;

namespace DBEnigmistica.Tests.Unit.Valutazione;

/// <summary>Servizi reali della valutazione montati a mano, senza host.</summary>
internal static class ServiziDiProva
{
    public static string SetVersionato => FileDelRepository.Percorso("tests", "DBEnigmistica.Tests", "Fixtures", "SetSintetico");

    public static readonly IVersionePromptEstrazione Versione = new VersionePromptCatalogo();

    public static RisolutorePagineSet Risolutore() => new(new PdfRenderPagine(), new GeneratorePdfSetSintetico());

    public static ServizioRegressioneEstrazione Servizio() => new(Risolutore(), Versione);

    public static IFornitoreAiEstrazione Riproduci(string cartellaSet) =>
        new FornitoreAiRiproduci(
            "finto", "finto-1", new ArchivioRegistrazioniFileSystem(Path.Combine(cartellaSet, "registrazioni")),
            new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson());

    public static ParametriRegressione Parametri(string cartellaSet, bool dalVivo = false) => new(cartellaSet, "finto", "finto-1", 200, dalVivo);

    public static ProduttoreRegistrazioniSintetiche Produttore() =>
        new(Risolutore(), Versione, new FabbricaArchivioRegistrazioniFileSystem(), new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson(), Servizio());

    public static GeneratoreBozzeAttese Bozze() =>
        new(Risolutore(), Versione, new FabbricaArchivioRegistrazioniFileSystem(), new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson());

    public static void CopiaCartella(string da, string a)
    {
        Directory.CreateDirectory(a);
        foreach (var file in Directory.GetFiles(da))
            File.Copy(file, Path.Combine(a, Path.GetFileName(file)));
        foreach (var cartella in Directory.GetDirectories(da))
            CopiaCartella(cartella, Path.Combine(a, Path.GetFileName(cartella)));
    }
}
