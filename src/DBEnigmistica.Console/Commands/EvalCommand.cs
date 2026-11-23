using System.CommandLine;
using System.Text;
using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Application.Valutazione.TempiRevisione;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Infrastructure.Prototipo;
using DBEnigmistica.Infrastructure.Services.Pdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Console.Commands;

/// <summary>Valutazione: <c>eval extraction</c>, <c>eval draft</c>, <c>eval review-time</c> e, nascosto, <c>eval sintetico aggiorna-chiavi</c>.</summary>
public class EvalCommand : Command
{
    public EvalCommand(CommandRunner runner)
        : base("eval", "Valuta l'estrazione sui Giochi di un set")
    {
        Subcommands.Add(new EvalExtractionCommand(runner));
        Subcommands.Add(new EvalDraftCommand(runner));
        Subcommands.Add(new EvalSinteticoCommand(runner));
        Subcommands.Add(new EvalReviewTimeCommand(runner));
    }
}

internal static class OpzioniEval
{
    public static Option<DirectoryInfo> Set() => new("--set") { Description = "Cartella del set (contiene set.json)", Required = true };

    public static Option<string> Fornitore() => new("--fornitore") { Description = "Id del fornitore delle Registrazioni", DefaultValueFactory = _ => "finto" };

    public static Option<string> Modello() => new("--modello") { Description = "Modello delle Registrazioni", DefaultValueFactory = _ => "finto-1" };

    public static Option<int> Dpi() => new("--dpi") { Description = "Risoluzione delle Registrazioni", DefaultValueFactory = _ => 200 };
}

public class EvalExtractionCommand : Command
{
    public EvalExtractionCommand(CommandRunner runner)
        : base("extraction", "Misura l'estrazione su un set e la confronta con la Linea di base: --replay legge le Registrazioni, --live chiama il Fornitore AI")
    {
        var set = OpzioniEval.Set();
        var fornitore = OpzioniEval.Fornitore();
        var modello = OpzioniEval.Modello();
        var dpi = OpzioniEval.Dpi();
        var replay = new Option<bool>("--replay") { Description = "Legge le Registrazioni: nessuna chiamata AI" };
        var live = new Option<bool>("--live") { Description = "Chiama il Fornitore AI configurato: richiede --limite" };
        var limite = new Option<int?>("--limite") { Description = "Numero massimo di chiamate al Fornitore AI in questa esecuzione (solo con --live)" };
        var output = new Option<DirectoryInfo?>("--output") { Description = "Cartella in cui scrivere rapporto.json e rapporto.md" };
        Options.Add(set);
        Options.Add(fornitore);
        Options.Add(modello);
        Options.Add(dpi);
        Options.Add(replay);
        Options.Add(live);
        Options.Add(limite);
        Options.Add(output);

        // Solo --live usa il Fornitore AI e ne valida la configurazione all'avvio.
        this.SetAction((parseResult, ct) => runner.RunAsync<EvalExtractionHandler>(
            parseResult.GetValue(live) ? HostRequirements.Ai : HostRequirements.None,
            (handler, token) => handler.ExecuteAsync(
                new ParametriRegressione(
                    parseResult.GetRequiredValue(set).FullName, parseResult.GetRequiredValue(fornitore),
                    parseResult.GetRequiredValue(modello), parseResult.GetRequiredValue(dpi)),
                parseResult.GetValue(replay), parseResult.GetValue(live), parseResult.GetValue(limite), parseResult.GetValue(output), token),
            ct));
    }
}

public class EvalDraftCommand : Command
{
    public EvalDraftCommand(CommandRunner runner)
        : base("draft", "Scrive le bozze dei risultati attesi da una Registrazione (attesi/<id>.json)")
    {
        var set = OpzioniEval.Set();
        var fornitore = OpzioniEval.Fornitore();
        var modello = OpzioniEval.Modello();
        var dpi = OpzioniEval.Dpi();
        var sovrascrivi = new Option<bool>("--sovrascrivi") { Description = "Sostituisce anche le bozze già scritte; un risultato corretto non si tocca mai" };
        Options.Add(set);
        Options.Add(fornitore);
        Options.Add(modello);
        Options.Add(dpi);
        Options.Add(sovrascrivi);

        this.SetAction((parseResult, ct) => runner.RunAsync<EvalDraftHandler>(
            HostRequirements.None,
            (handler, token) => handler.ExecuteAsync(
                parseResult.GetRequiredValue(set).FullName, parseResult.GetRequiredValue(fornitore),
                parseResult.GetRequiredValue(modello), parseResult.GetRequiredValue(dpi), parseResult.GetValue(sovrascrivi), token),
            ct));
    }
}

public class EvalReviewTimeCommand : Command
{
    public EvalReviewTimeCommand(CommandRunner runner)
        : base("review-time", "Rapporto dei tempi di revisione dei candidati del prototipo, a confronto con le soglie di G1")
    {
        var output = new Option<DirectoryInfo?>("--output") { Description = "Cartella in cui scrivere review-time.json e review-time.md" };
        var candidati = new Option<FileInfo?>("--candidati") { Description = "Legge i candidati da un file JSON (set sintetico) invece che dal database" };
        Options.Add(output);
        Options.Add(candidati);

        this.SetAction((parseResult, ct) => runner.RunAsync<EvalReviewTimeHandler>(
            parseResult.GetValue(candidati) is null ? HostRequirements.Database : HostRequirements.None,
            (handler, token) => handler.ExecuteAsync(parseResult.GetValue(candidati), parseResult.GetValue(output), token),
            ct));
    }
}

public class EvalSinteticoCommand : Command
{
    public EvalSinteticoCommand(CommandRunner runner)
        : base("sintetico", "Operazioni sul Set sintetico")
    {
        Hidden = true;
        var set = OpzioniEval.Set();
        var fornitore = OpzioniEval.Fornitore();
        var modello = OpzioniEval.Modello();
        var dpi = OpzioniEval.Dpi();
        var aggiorna = new Command("aggiorna-chiavi", "Ricalcola Registrazioni e Linea di base dalle risposte del Set sintetico") { Hidden = true };
        aggiorna.Options.Add(set);
        aggiorna.Options.Add(fornitore);
        aggiorna.Options.Add(modello);
        aggiorna.Options.Add(dpi);
        aggiorna.SetAction((parseResult, ct) => runner.RunAsync<EvalSinteticoHandler>(
            HostRequirements.None,
            (handler, token) => handler.AggiornaChiaviAsync(
                parseResult.GetRequiredValue(set).FullName, parseResult.GetRequiredValue(fornitore),
                parseResult.GetRequiredValue(modello), parseResult.GetRequiredValue(dpi), token),
            ct));
        Subcommands.Add(aggiorna);
    }
}

/// <summary>Il generatore di PDF dei set sintetici: per ora solo "set-sintetico-v1".</summary>
public sealed class GeneratorePdfSetSintetico : IGeneratorePdfSet
{
    public const string SetSinteticoV1 = "set-sintetico-v1";

    public byte[] Genera(string nomeGeneratore) =>
        nomeGeneratore == SetSinteticoV1
            ? GeneratorePdfSintetici.SetSinteticoV1()
            : throw new SetNonValidoException($"Generatore sconosciuto: {nomeGeneratore}.");
}

public sealed class EvalExtractionHandler(
    ServizioRegressioneEstrazione servizio,
    IFabbricaArchivioRegistrazioni fabbrica,
    IInterpreteRispostaGrezza interprete,
    IValidatoreSchemaJson validatore,
    IServiceProvider servizi,
    IOptions<FornitoreAiOptions> opzioniAi,
    CliStreams streams)
{
    public async Task<int> ExecuteAsync(
        ParametriRegressione parametri, bool replay, bool live, int? limite, DirectoryInfo? output, CancellationToken cancellationToken)
    {
        if (replay == live)
            return Errore("Indica una sola modalità: --replay oppure --live.");

        // L'interruttore si controlla prima di ogni chiamata o lancio di processo.
        if (live && !opzioniAi.Value.DalVivoAbilitato)
        {
            streams.Error.WriteLine(FornitoreAiOptions.MessaggioDalVivoDisattivato);
            return ExitCodes.Configuration;
        }

        if (live && limite is not > 0)
            return Errore("Con --live serve --limite con un numero di chiamate maggiore di zero.");

        try
        {
            var fornitore = replay
                ? new FornitoreAiRiproduci(parametri.Fornitore, parametri.Modello, fabbrica.Apri(Path.Combine(parametri.CartellaSet, "registrazioni")), interprete, validatore)
                : (IFornitoreAiEstrazione)new FornitoreAiConLimite(servizi.GetRequiredService<IFornitoreAiEstrazione>(), limite, new ContatoreChiamateAi());
            var risultato = await servizio.EseguiAsync(parametri with { DalVivo = live }, fornitore, cancellationToken);

            if (output is not null)
            {
                output.Create();
                await File.WriteAllTextAsync(Path.Combine(output.FullName, "rapporto.json"), risultato.Json, new UTF8Encoding(false), cancellationToken);
                await File.WriteAllTextAsync(Path.Combine(output.FullName, "rapporto.md"), risultato.Markdown, new UTF8Encoding(false), cancellationToken);
                streams.Out.WriteLine($"Rapporto scritto in {output.FullName}");
            }

            streams.Out.Write(risultato.Markdown);
            return risultato.Regressioni.Count == 0 ? ExitCodes.Success : ExitCodes.Error;
        }
        catch (ValutazioneException ex)
        {
            return Errore(ex.Message);
        }
    }

    private int Errore(string messaggio)
    {
        streams.Error.WriteLine(messaggio);
        return ExitCodes.Error;
    }
}

public sealed class EvalDraftHandler(GeneratoreBozzeAttese generatore, CliStreams streams)
{
    public async Task<int> ExecuteAsync(string cartellaSet, string fornitore, string modello, int dpi, bool sovrascrivi, CancellationToken cancellationToken)
    {
        try
        {
            var esito = await generatore.BozzaAsync(cartellaSet, fornitore, modello, dpi, sovrascrivi, cancellationToken);
            streams.Out.WriteLine($"Bozze scritte: {esito.Scritte.Count}" + (esito.Scritte.Count > 0 ? $" ({string.Join(", ", esito.Scritte)})" : string.Empty));
            if (esito.Saltate.Count > 0)
                streams.Out.WriteLine($"Pagine con un risultato atteso già presente, non toccate: {string.Join(", ", esito.Saltate)}");

            return ExitCodes.Success;
        }
        catch (ValutazioneException ex)
        {
            streams.Error.WriteLine(ex.Message);
            return ExitCodes.Error;
        }
    }
}

/// <summary>Scrive il rapporto dei tempi di revisione; esce 0 anche se le soglie non sono rispettate: il rapporto confronta, non decide.</summary>
public sealed class EvalReviewTimeHandler(ServizioTempiRevisione servizio, IConfiguration configurazione, CliStreams streams)
{
    public async Task<int> ExecuteAsync(FileInfo? candidati, DirectoryInfo? output, CancellationToken cancellationToken)
    {
        try
        {
            ILettoreCandidatiRapporto lettore = candidati is null
                ? new LettoreCandidatiRapporto(configurazione)
                : new LettoreDaFile(await File.ReadAllTextAsync(candidati.FullName, cancellationToken));
            var rapporto = await servizio.EseguiAsync(lettore, cancellationToken);

            if (output is not null)
            {
                output.Create();
                await File.WriteAllTextAsync(Path.Combine(output.FullName, "review-time.json"), rapporto.InJson(), new UTF8Encoding(false), cancellationToken);
                await File.WriteAllTextAsync(Path.Combine(output.FullName, "review-time.md"), rapporto.InMarkdown(), new UTF8Encoding(false), cancellationToken);
                streams.Out.WriteLine($"Rapporto scritto in {output.FullName}");
            }

            streams.Out.Write(rapporto.InMarkdown());
            return ExitCodes.Success;
        }
        catch (Exception ex) when (ex is FormatException or IOException or UnauthorizedAccessException)
        {
            streams.Error.WriteLine(ex.Message);
            return ExitCodes.Error;
        }
    }

    private sealed class LettoreDaFile(string json) : ILettoreCandidatiRapporto
    {
        public Task<IReadOnlyList<CandidatoPerRapporto>> LeggiAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(LettoreCandidatiDaFile.Leggi(json));
    }
}

public sealed class EvalSinteticoHandler(ProduttoreRegistrazioniSintetiche produttore, CliStreams streams)
{
    public async Task<int> AggiornaChiaviAsync(string cartellaSet, string fornitore, string modello, int dpi, CancellationToken cancellationToken)
    {
        try
        {
            var pagine = await produttore.AggiornaAsync(cartellaSet, fornitore, modello, dpi, cancellationToken: cancellationToken);
            streams.Out.WriteLine($"Registrazioni e Linea di base ricalcolate: {pagine} pagine.");
            return ExitCodes.Success;
        }
        catch (ValutazioneException ex)
        {
            streams.Error.WriteLine(ex.Message);
            return ExitCodes.Error;
        }
    }
}
