using DBEnigmistica.AI.Configuration;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Domain.Interfaces;
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
    public async Task CheckAi_ApiAnthropicConChiaveVuota_EsceConTreEDiceDiImpostareLaChiave()
    {
        var result = await RunAsync(
            ["check-ai"],
            ("FornitoreAi:Tipo", "api-anthropic"),
            ("FornitoreAi:Modello", "claude-sonnet-5-5"),
            ("FornitoreAi:Api:ChiaveApi", ""));

        result.ExitCode.Should().Be(ExitCodes.Configuration);
        result.Error.Should().Contain("FornitoreAi__Api__ChiaveApi");
    }

    [Fact]
    public async Task CheckAi_ConfigurazioneValida_StampaFornitoreEModelloSenzaLaChiaveEEsceConZero()
    {
        const string chiave = "CHIAVE-SEGRETA-DI-PROVA";

        var result = await RunAsync(
            ["check-ai"],
            ("FornitoreAi:Tipo", "api-anthropic"),
            ("FornitoreAi:Modello", "modello-di-prova"),
            ("FornitoreAi:Api:ChiaveApi", chiave));

        result.ExitCode.Should().Be(ExitCodes.Success);
        result.Output.Should().Contain("api-anthropic").And.Contain("modello-di-prova");
        (result.Output + result.Error).Should().NotContain(chiave);
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
