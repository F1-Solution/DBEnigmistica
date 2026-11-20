using DBEnigmistica.AI.Configuration;
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
            "Claude Code non autenticato: esegui claude e poi /login");
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
    [InlineData("chiave-di-prova")]
    public void ApiAnthropic_InFase1_ERifiutatoConUnMessaggioChiaro(string? chiave)
    {
        Fallimenti(Api(chiave), new FakeAmbienteFornitoreAi()).Should().ContainSingle().Which.Should().Be(
            "Il Fornitore AI api-anthropic non è disponibile in fase 1: imposta FornitoreAi__Tipo=claude-code.");
    }

    [Theory]
    [InlineData("claude.cmd")]
    [InlineData("claude.bat")]
    [InlineData("C:\\x\\claude.cmd")]
    [InlineData("C:\\x\\claude")]
    [InlineData("C:\\x\\claude.cmd.")]
    public void ClaudeCode_SuWindows_AccettaSoloExe(string eseguibile)
    {
        var ambiente = new FakeAmbienteFornitoreAi { EWindows = true, Token = "t", File = { eseguibile }, NelPath = { eseguibile } };

        Fallimenti(ClaudeCode(eseguibile), ambiente).Should().ContainSingle().Which.Should().StartWith("Serve l'eseguibile nativo di Claude Code (.exe)");
    }

    [Fact]
    public void ClaudeCode_SuWindows_NomeSenzaEstensioneCercaSoloExeNelPath()
    {
        var soloCmd = new FakeAmbienteFornitoreAi { EWindows = true, Token = "t", NelPath = { "claude.cmd", "claude" } };
        var conExe = new FakeAmbienteFornitoreAi { EWindows = true, Token = "t", NelPath = { "claude.exe" } };

        Fallimenti(ClaudeCode("claude"), soloCmd).Should().ContainSingle().Which.Should().StartWith("Claude Code non trovato");
        Fallimenti(ClaudeCode("claude"), conExe).Should().BeEmpty();
    }

    [Fact]
    public void ClaudeCode_SuWindows_PercorsoExeEsistente_EValido()
    {
        var percorso = "C:\\x\\CLAUDE.EXE";

        Fallimenti(ClaudeCode(percorso), new FakeAmbienteFornitoreAi { EWindows = true, Token = "t", File = { percorso } }).Should().BeEmpty();
    }

    [Fact]
    public void ClaudeCode_SuWindows_PercorsoConSlashEsistente_EValido()
    {
        var percorso = "C:/x/claude.exe";

        Fallimenti(ClaudeCode(percorso), new FakeAmbienteFornitoreAi { EWindows = true, Token = "t", File = { percorso } }).Should().BeEmpty();
    }

    [Theory]
    [InlineData("C:\\x\\claude.cmd")]
    [InlineData("C:/x/claude.cmd")]
    public void ClaudeCode_SuWindows_PercorsoCmd_ERifiutatoConMessaggioExe(string percorso)
    {
        var ambiente = new FakeAmbienteFornitoreAi { EWindows = true, Token = "t", File = { percorso } };

        Fallimenti(ClaudeCode(percorso), ambiente).Should().ContainSingle().Which.Should().Contain("(.exe)");
    }

    [Fact]
    public void ClaudeCode_SuAmbienteNonWindows_PercorsoUnixEsistente_EValido()
    {
        var percorso = "/usr/bin/claude";

        Fallimenti(ClaudeCode(percorso), new FakeAmbienteFornitoreAi { EWindows = false, Token = "t", File = { percorso } }).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(3601)]
    public void Timeout_FuoriIntervallo_ERifiutato(int secondi)
    {
        var options = ClaudeCode();
        options.ClaudeCode.TimeoutSecondi = secondi;

        Fallimenti(options, Valido()).Should().ContainSingle().Which.Should().Contain("TimeoutSecondi").And.Contain("3600");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(3600)]
    public void Timeout_Valido_EAccettato(int? secondi)
    {
        var options = ClaudeCode();
        options.ClaudeCode.TimeoutSecondi = secondi;

        Fallimenti(options, Valido()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LimiteChiamate_SottoUno_ERifiutato(int limite)
    {
        var options = ClaudeCode();
        options.LimiteChiamate = limite;

        Fallimenti(options, Valido()).Should().ContainSingle().Which.Should().Contain("LimiteChiamate");
    }

    [Fact]
    public void Riproduzione_NonRichiedeEseguibileNeLogin()
    {
        var options = ClaudeCode();
        options.Registrazioni = new RegistrazioniOptions { Modalita = "riproduci", Cartella = "c:\\reg" };

        Fallimenti(options, new FakeAmbienteFornitoreAi()).Should().BeEmpty();
    }

    [Fact]
    public void Registrazioni_SonoValidateAncheSeClaudeCodeNonVa()
    {
        var options = ClaudeCode();
        options.Registrazioni = new RegistrazioniOptions { Modalita = "registra", Cartella = null };

        Fallimenti(options, new FakeAmbienteFornitoreAi()).Should().Contain(f => f.StartsWith("Cartella delle Registrazioni mancante"))
            .And.Contain(f => f.StartsWith("Claude Code non trovato"));
    }

    private static FakeAmbienteFornitoreAi Valido() => new() { Token = "t", NelPath = { "claude" } };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Modello_Vuoto_ChiedeDiImpostareFornitoreAiModello(string? modello)
    {
        var options = ClaudeCode();
        options.Modello = modello;

        Fallimenti(options, Valido()).Should()
            .ContainSingle().Which.Should().Be("Modello del Fornitore AI mancante: imposta FornitoreAi__Modello.");
    }

    [Fact]
    public void PiuProblemi_SonoRiportatiTuttiInsieme()
    {
        var options = ClaudeCode(modello: "");

        Fallimenti(options, new FakeAmbienteFornitoreAi()).Should().HaveCount(3);
    }

    private sealed class FakeAmbienteFornitoreAi : IAmbienteFornitoreAi
    {
        public bool EWindows { get; init; }
        public string? Token { get; init; }
        public HashSet<string> NelPath { get; } = [];
        public HashSet<string> File { get; } = [];

        public string CartellaUtente => Utente;

        public string? Variabile(string nome) => nome == "CLAUDE_CODE_OAUTH_TOKEN" ? Token : null;

        public bool FileEsiste(string percorso) => File.Contains(percorso);

        public bool EseguibileNelPath(string nome) => NelPath.Contains(nome);
    }
}
