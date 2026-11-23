using DBEnigmistica.Application.Valutazione.TempiRevisione;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Commands;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Tests.Unit.Valutazione.TempiRevisione;

/// <summary><c>dbe eval review-time</c> sui 30 candidati sintetici letti da file: nessun database, codice 0 anche con soglie mancate.</summary>
public sealed class EvalReviewTimeHandlerTests : IDisposable
{
    private static readonly string Candidati = FileDelRepository.Percorso("tests", "DBEnigmistica.Tests", "Fixtures", "CandidatiSintetici", "candidati.json");

    private readonly string _radice = Path.Combine(Path.GetTempPath(), "dbe-review-time-" + Guid.NewGuid().ToString("N"));
    private readonly StringWriter _out = new();
    private readonly StringWriter _err = new();

    public void Dispose()
    {
        if (Directory.Exists(_radice))
            Directory.Delete(_radice, recursive: true);
    }

    private EvalReviewTimeHandler Handler() =>
        new(new ServizioTempiRevisione(), new ConfigurationBuilder().Build(), new CliStreams(_out, _err));

    [Fact]
    public async Task ConFileEOutput_ScriveJsonEMarkdownEEsceZeroAncheConSoglieMancate()
    {
        var uscita = new DirectoryInfo(Path.Combine(_radice, "uscita"));

        var codice = await Handler().ExecuteAsync(new FileInfo(Candidati), uscita, CancellationToken.None);

        codice.Should().Be(ExitCodes.Success);
        File.ReadAllText(Path.Combine(uscita.FullName, "review-time.json")).Should().Contain("\"gruppo\": \"anagramma\"").And.Contain("\"tempo_entro_soglia\": false");
        File.ReadAllText(Path.Combine(uscita.FullName, "review-time.md")).Should().Contain("misura indicativa");
        _out.ToString().Should().Contain("Tempi di revisione");
    }

    [Fact]
    public async Task ConTempoNegativo_EsceUnoConUnMessaggio()
    {
        Directory.CreateDirectory(_radice);
        var file = Path.Combine(_radice, "negativo.json");
        await File.WriteAllTextAsync(file, "{\"versione_formato\":1,\"candidati\":[{\"tipologia\":\"sudoku\",\"versione_tipologia\":0,\"stato\":\"accettato\",\"tempo_attivo_ms\":-1}]}");

        var codice = await Handler().ExecuteAsync(new FileInfo(file), null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Error);
        _err.ToString().Should().Contain("negativo");
    }

    [Fact]
    public async Task ConFileNonLeggibile_EsceUnoConUnMessaggio()
    {
        Directory.CreateDirectory(_radice);
        var rotto = Path.Combine(_radice, "rotto.json");
        await File.WriteAllTextAsync(rotto, "{ non json");

        var codice = await Handler().ExecuteAsync(new FileInfo(rotto), null, CancellationToken.None);

        codice.Should().Be(ExitCodes.Error);
        _err.ToString().Should().Contain("non leggibile");
    }
}
