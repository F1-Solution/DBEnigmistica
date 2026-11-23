using DBEnigmistica.AI.Configuration;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Console.Commands;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DBEnigmistica.Tests.Unit.Cli;

public class FornitoreAiCheckTests
{
    private static readonly string EseguibileInesistente = Path.Combine(Path.GetTempPath(), "inesistente", "claude.exe");

    public static TheoryData<string[]> ComandiSenzaAi => new()
    {
        { ["--help"] },
        { ["list-games"] },
        { ["export-stats"] },
    };

    [Fact]
    public async Task CheckAi_SenzaTipo_EsceConTreEDiceCosaImpostare()
    {
        var result = await RunAsync(["check-ai"], ("FornitoreAi:Tipo", ""));

        result.ExitCode.Should().Be(ExitCodes.Configuration);
        result.Error.Should().Contain("FornitoreAi__Tipo");
        result.Output.Should().NotContain("Fornitore AI:");
    }

    [Fact]
    public async Task CheckAi_ConEseguibileMancante_EsceConTreEDiceCheClaudeCodeNonEStatoTrovato()
    {
        var result = await RunAsync(
            ["check-ai"],
            ("FornitoreAi:Tipo", "claude-code"),
            ("FornitoreAi:Modello", "claude-sonnet-5-5"),
            ("FornitoreAi:ClaudeCode:Eseguibile", EseguibileInesistente));

        result.ExitCode.Should().Be(ExitCodes.Configuration);
        result.Error.Should().Contain("Claude Code non trovato").And.Contain(EseguibileInesistente);
    }

    [Fact]
    public async Task CheckAi_ApiAnthropic_InFase1_EsceConTreEDiceDiUsareClaudeCode()
    {
        var result = await RunAsync(
            ["check-ai"],
            ("FornitoreAi:Tipo", "api-anthropic"),
            ("FornitoreAi:Modello", "claude-sonnet-5-5"),
            ("FornitoreAi:Api:ChiaveApi", "CHIAVE-SEGRETA-DI-PROVA"));

        result.ExitCode.Should().Be(ExitCodes.Configuration);
        result.Error.Should().Contain("api-anthropic non è disponibile in fase 1").And.Contain("claude-code");
        (result.Output + result.Error).Should().NotContain("CHIAVE-SEGRETA-DI-PROVA");
    }

    [Fact]
    public void CheckAi_ConfigurazioneValida_ConAmbienteFintoDiClaudeCode_StampaFornitoreEModelloEEsceConZero()
    {
        var streams = NewStreams();
        var opzioni = OpzioniClaudeCode().Value;
        var ambiente = new AmbienteConChiave(null) { Token = "t", EseguibileNelPathSempre = true };

        new FornitoreAiOptionsValidator(ambiente).Validate(null, opzioni).Succeeded.Should().BeTrue();
        var codice = new CheckAiHandler(Options.Create(opzioni), ambiente, streams).Execute();

        codice.Should().Be(ExitCodes.Success);
        streams.Out.ToString()!.Should().Contain("claude-code").And.Contain("claude-sonnet-5-5").And.Contain("Configurazione valida.");
    }

    [Fact]
    public void CheckAi_ConChiaveApiNellAmbiente_AvvisaCheNonVerraPassataAClaudeCode()
    {
        var streams = NewStreams();
        var handler = new CheckAiHandler(OpzioniClaudeCode(), new AmbienteConChiave("CHIAVE-SEGRETA"), streams);

        var codice = handler.Execute();

        codice.Should().Be(ExitCodes.Success);
        var output = streams.Out.ToString()!;
        output.Should().Contain("Avviso: ANTHROPIC_API_KEY").And.Contain("non verrà passata a Claude Code");
        output.Should().Contain("Configurazione valida.");
        output.Should().NotContain("CHIAVE-SEGRETA");
    }

    [Fact]
    public void CheckAi_SenzaChiaveApiNellAmbiente_NonStampaAvvisi()
    {
        var streams = NewStreams();
        var handler = new CheckAiHandler(OpzioniClaudeCode(), new AmbienteConChiave(null), streams);

        handler.Execute().Should().Be(ExitCodes.Success);

        streams.Out.ToString()!.Should().NotContain("Avviso");
    }

    [Fact]
    public void CheckAi_ApiAnthropic_NonAvvisaSullaChiaveDellAmbiente()
    {
        var streams = NewStreams();
        var opzioni = Options.Create(new FornitoreAiOptions { Tipo = "api-anthropic", Modello = "m" });
        var handler = new CheckAiHandler(opzioni, new AmbienteConChiave("x"), streams);

        handler.Execute();

        streams.Out.ToString()!.Should().NotContain("Avviso");
    }

    [Fact]
    public void Contenitore_RisolveLaPortaDiEstrazioneConLaCatenaDeiDecoratori()
    {
        using var host = CliHost.Build(HostRequirements.None, NewStreams(), Impostazioni(
            ("FornitoreAi:Tipo", "claude-code"),
            ("FornitoreAi:Modello", "claude-sonnet-5-5"),
            ("FornitoreAi:ClaudeCode:Eseguibile", "claude"),
            ("FornitoreAi:DalVivoAbilitato", "true")));
        using var scope = host.Services.CreateScope();

        var fornitore = scope.ServiceProvider.GetRequiredService<IFornitoreAiEstrazione>();

        fornitore.IdFornitore.Should().Be("claude-code");
        fornitore.Modello.Should().Be("claude-sonnet-5-5");
        scope.ServiceProvider.GetRequiredService<IRegistroChiamateAi>().Should().BeOfType<RegistroChiamateAiInMemoria>();
    }

    private static IOptions<FornitoreAiOptions> OpzioniClaudeCode() =>
        Options.Create(new FornitoreAiOptions
        {
            Tipo = "claude-code",
            Modello = "claude-sonnet-5-5",
            ClaudeCode = new ClaudeCodeOptions { Eseguibile = "claude" },
        });

    private sealed class AmbienteConChiave(string? chiave) : IAmbienteFornitoreAi
    {
        public string? Token { get; init; }

        public bool EseguibileNelPathSempre { get; init; }

        public string CartellaUtente => "utente";

        public bool EWindows => false;

        public string? Variabile(string nome) => nome switch
        {
            "ANTHROPIC_API_KEY" => chiave,
            "CLAUDE_CODE_OAUTH_TOKEN" => Token,
            _ => null,
        };

        public bool FileEsiste(string percorso) => false;

        public bool EseguibileNelPath(string nome) => EseguibileNelPathSempre;
    }

    [Theory]
    [MemberData(nameof(ComandiSenzaAi))]
    public async Task ComandiCheNonUsanoLAi_NonSegnalanoErroriDiConfigurazioneAi(string[] args)
    {
        var result = await RunAsync(args, ("FornitoreAi:Tipo", ""), ("FornitoreAi:Modello", ""));

        (result.Output + result.Error).Should().NotContain("non configurato").And.NotContain("FornitoreAi__").And.NotContain("Modello del Fornitore AI");
    }

    [Fact]
    public async Task Host_SenzaConfigurazioneAi_SiAvviaPerIComandiCheNonUsanoLAi()
    {
        using var host = CliHost.Build(HostRequirements.Database, NewStreams(), Impostazioni(
            ("ConnectionStrings:DefaultConnection", "Host=localhost;Database=x"),
            ("FornitoreAi:Tipo", "")));

        var start = () => host.StartAsync();

        await start.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Host_SenzaConfigurazioneAi_ConRequisitoAi_FallisceConLaValidazione()
    {
        using var host = CliHost.Build(HostRequirements.Ai, NewStreams(), Impostazioni(("FornitoreAi:Tipo", "")));

        var start = () => host.StartAsync();

        var thrown = await start.Should().ThrowAsync<Microsoft.Extensions.Options.OptionsValidationException>();
        thrown.Which.Failures.Should().Contain(f => f.Contains("FornitoreAi__Tipo"));
    }

    [Fact]
    public void Contenitore_SiCostruisceConValidateOnBuild_EPerLAnalizzatoreRisolveIlSegnaposto()
    {
        using var host = CliHost.Build(HostRequirements.None, NewStreams(), Impostazioni());
        using var scope = host.Services.CreateScope();

        scope.ServiceProvider.GetRequiredService<IAiGameAnalyzer>().Should().BeOfType<FornitoreAiNonDisponibile>();
    }

    [Fact]
    public void ServiziAi_NonRegistranoNessunClientHttp()
    {
        var services = new ServiceCollection();

        services.AddAiServices(new ConfigurationBuilder().Build());

        services.Should().NotContain(d =>
            (d.ServiceType.FullName ?? "").Contains("Http") ||
            (d.ImplementationType != null && (d.ImplementationType.FullName ?? "").Contains("Http")));
        services.Should().ContainSingle(d => d.ServiceType == typeof(IAiGameAnalyzer));
    }

    [Fact]
    public async Task FornitoreAiNonDisponibile_LanciaIlMessaggioDellaFase1()
    {
        var analizzatore = new FornitoreAiNonDisponibile();

        var chiamata = () => analizzatore.ClassifyGameAsync("testo");

        await chiamata.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Il Fornitore AI è disponibile dalla fase 1");
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunAsync(
        string[] args,
        params (string Key, string Value)[] settings)
    {
        var output = new StringWriter();
        var error = new StringWriter();

        var code = await CliApp.RunAsync(
            args, output, error, Impostazioni(settings.Append(("ConnectionStrings:DefaultConnection", "")).ToArray()));

        return (code, output.ToString(), error.ToString());
    }

    private static CliStreams NewStreams() => new(new StringWriter(), new StringWriter());

    private static Action<IConfigurationBuilder> Impostazioni(params (string Key, string Value)[] settings) =>
        builder => builder.AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)));
}
