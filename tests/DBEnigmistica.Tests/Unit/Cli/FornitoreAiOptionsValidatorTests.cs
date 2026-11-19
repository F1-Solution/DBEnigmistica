using DBEnigmistica.Console.Configuration;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Cli;

public class FornitoreAiOptionsValidatorTests
{
    private const string Utente = "utente";

    private static string CredenzialiClaude => Path.Combine(Utente, ".claude", ".credentials.json");

    private static FornitoreAiOptions ClaudeCode(string eseguibile = "claude", string modello = "claude-sonnet-5-5") => new()
    {
        Tipo = "claude-code",
        Modello = modello,
        ClaudeCode = new ClaudeCodeOptions { Eseguibile = eseguibile }
    };

    private static FornitoreAiOptions Api(string? chiave, string modello = "claude-sonnet-5-5") => new()
    {
        Tipo = "api-anthropic",
        Modello = modello,
        Api = new ApiOptions { ChiaveApi = chiave }
    };

    private static string[] Fallimenti(FornitoreAiOptions options, FakeAmbienteFornitoreAi ambiente)
    {
        var result = new FornitoreAiOptionsValidator(ambiente).Validate(null, options);
        return result.Failed ? result.Failures!.ToArray() : [];
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("openai")]
    public void Tipo_VuotoOSconosciuto_ChiedeDiImpostareFornitoreAiTipo(string? tipo)
    {
        var options = ClaudeCode();
        options.Tipo = tipo;

        Fallimenti(options, new FakeAmbienteFornitoreAi()).Should()
            .Contain("Fornitore AI non configurato: imposta FornitoreAi__Tipo (claude-code o api-anthropic).");
    }

    [Fact]
    public void ClaudeCode_EseguibileNomeAssenteDalPath_DiceCheClaudeCodeNonEStatoTrovato()
    {
        var ambiente = new FakeAmbienteFornitoreAi { Token = "token" };

        var fallimenti = Fallimenti(ClaudeCode("claude"), ambiente);

        fallimenti.Should().ContainSingle().Which.Should().StartWith("Claude Code non trovato:").And.Contain("claude");
    }

    [Fact]
    public void ClaudeCode_EseguibileConPercorsoAssolutoInesistente_DiceCheClaudeCodeNonEStatoTrovato()
    {
        var percorso = Path.Combine(Path.GetTempPath(), "inesistente", "claude.exe");
        var ambiente = new FakeAmbienteFornitoreAi { Token = "token" };

        Fallimenti(ClaudeCode(percorso), ambiente).Should()
            .ContainSingle().Which.Should().StartWith("Claude Code non trovato:").And.Contain(percorso);
    }

    [Fact]
    public void ClaudeCode_EseguibileVuoto_DiceCheClaudeCodeNonEStatoTrovato()
    {
        var ambiente = new FakeAmbienteFornitoreAi { Token = "token" };

        Fallimenti(ClaudeCode(" "), ambiente).Should()
            .ContainSingle().Which.Should().StartWith("Claude Code non trovato:");
    }

    [Fact]
    public void ClaudeCode_SenzaTokenNeCredenziali_DiceCheClaudeCodeNonEAutenticato()
    {
        var ambiente = new FakeAmbienteFornitoreAi { NelPath = { "claude" } };

        Fallimenti(ClaudeCode(), ambiente).Should().ContainSingle().Which.Should().Be(
            "Claude Code non autenticato: imposta CLAUDE_CODE_OAUTH_TOKEN oppure esegui l'accesso con claude.");
    }

    [Fact]
    public void ClaudeCode_ConTokenVuoto_NonEAutenticato()
    {
        var ambiente = new FakeAmbienteFornitoreAi { NelPath = { "claude" }, Token = "  " };

        Fallimenti(ClaudeCode(), ambiente).Should().ContainSingle().Which.Should().StartWith("Claude Code non autenticato");
    }

    [Fact]
    public void ClaudeCode_ConEseguibileNelPathETokenValorizzato_EValido()
    {
        var ambiente = new FakeAmbienteFornitoreAi { NelPath = { "claude" }, Token = "token" };

        Fallimenti(ClaudeCode(), ambiente).Should().BeEmpty();
    }

    [Fact]
    public void ClaudeCode_ConEseguibileAssolutoEFileDiCredenziali_EValido()
    {
        var percorso = Path.Combine(Path.GetTempPath(), "bin", "claude.exe");
        var ambiente = new FakeAmbienteFornitoreAi { File = { percorso, CredenzialiClaude } };

        Fallimenti(ClaudeCode(percorso), ambiente).Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApiAnthropic_ConChiaveVuota_ChiedeDiImpostareLaChiave(string? chiave)
    {
        Fallimenti(Api(chiave), new FakeAmbienteFornitoreAi()).Should()
            .ContainSingle().Which.Should().Be("Chiave API mancante: imposta FornitoreAi__Api__ChiaveApi.");
    }

    [Fact]
    public void ApiAnthropic_ConChiave_EValido()
    {
        Fallimenti(Api("chiave-di-prova"), new FakeAmbienteFornitoreAi()).Should().BeEmpty();
    }

    [Fact]
    public void ApiAnthropic_NonRichiedeClaudeCode()
    {
        Fallimenti(Api("chiave-di-prova"), new FakeAmbienteFornitoreAi()).Should()
            .NotContain(f => f.Contains("Claude Code"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Modello_Vuoto_ChiedeDiImpostareFornitoreAiModello(string? modello)
    {
        var options = Api("chiave-di-prova");
        options.Modello = modello;

        Fallimenti(options, new FakeAmbienteFornitoreAi()).Should()
            .ContainSingle().Which.Should().Be("Modello del Fornitore AI mancante: imposta FornitoreAi__Modello.");
    }

    [Fact]
    public void PiuProblemi_SonoRiportatiTuttiInsieme()
    {
        var options = ClaudeCode(modello: "");

        Fallimenti(options, new FakeAmbienteFornitoreAi()).Should().HaveCount(3);
    }

    [Fact]
    public void LaChiaveApi_NonCompareNeiMessaggi()
    {
        const string chiave = "CHIAVE-SEGRETA-DI-PROVA";
        var options = Api(chiave);
        options.Modello = "";

        var messaggi = string.Join('\n', Fallimenti(options, new FakeAmbienteFornitoreAi()));

        messaggi.Should().NotContain(chiave);
    }

    private sealed class FakeAmbienteFornitoreAi : IAmbienteFornitoreAi
    {
        public string? Token { get; init; }
        public HashSet<string> NelPath { get; } = [];
        public HashSet<string> File { get; } = [];

        public string CartellaUtente => Utente;

        public string? Variabile(string nome) => nome == "CLAUDE_CODE_OAUTH_TOKEN" ? Token : null;

        public bool FileEsiste(string percorso) => File.Contains(percorso);

        public bool EseguibileNelPath(string nome) => NelPath.Contains(nome);
    }
}
