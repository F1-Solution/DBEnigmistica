using DBEnigmistica.AI.Configuration;
using DBEnigmistica.AI.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Tests.Unit.FornitoreAi;

public class UscitaClaudeCodeTests
{
    private sealed class TempoFisso(DateTimeOffset adesso) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => adesso;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    // Giovedì 19/11/2026, 10:00 UTC.
    private static readonly TempoFisso Adesso = new(new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero));

    private static UscitaClaudeCode Errore(string testo, int codice = 1) =>
        UscitaClaudeCode.Analizza($"{{\"type\":\"result\",\"is_error\":true,\"result\":\"{testo}\"}}", string.Empty, codice, Adesso);

    [Fact]
    public void LimiteSettimanale_ConGiornoEOra_RipresaIlLunediSuccessivo()
    {
        var uscita = Errore("You've hit your weekly limit · resets Mon 12:00am");

        uscita.Tipo.Should().Be(TipoUscitaClaudeCode.FinestraEsaurita);
        uscita.Ripresa.Should().Be(new DateTimeOffset(2026, 11, 23, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void FormatoConEpoch_RiconosceLOraDiRipresa()
    {
        var uscita = Errore("Claude AI usage limit reached|1795600000");

        uscita.Tipo.Should().Be(TipoUscitaClaudeCode.FinestraEsaurita);
        uscita.Ripresa.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1795600000));
    }

    [Fact]
    public void RispostaInTestoConRecintoMarkdown_EstraeIlJson()
    {
        var uscita = UscitaClaudeCode.Analizza(
            "{\"type\":\"result\",\"is_error\":false,\"result\":\"```json\\n{\\\"a\\\":1}\\n```\"}", string.Empty, 0, Adesso);

        uscita.Tipo.Should().Be(TipoUscitaClaudeCode.Successo);
        uscita.Json.Should().Be("{\"a\":1}");
    }

    [Fact]
    public void SuccessoSenzaJson_HaMessaggioEJsonNullo()
    {
        var uscita = UscitaClaudeCode.Analizza("{\"type\":\"result\",\"is_error\":false,\"result\":\"ciao\"}", string.Empty, 0, Adesso);

        uscita.Tipo.Should().Be(TipoUscitaClaudeCode.Successo);
        uscita.Json.Should().BeNull();
        uscita.Messaggio.Should().NotBeNull();
    }

    [Fact]
    public void ErroreGenerico_EDefinitivo()
    {
        Errore("qualcosa e' andato storto").Tipo.Should().Be(TipoUscitaClaudeCode.Definitivo);
    }

    [Theory]
    [InlineData("registra", null, "Cartella delle Registrazioni mancante")]
    [InlineData("riproduci", " ", "Cartella delle Registrazioni mancante")]
    [InlineData("boh", "c:\\x", "Modalità delle Registrazioni non valida")]
    public void Registrazioni_ConfigurazioneIncoerente_VieneRifiutata(string modalita, string? cartella, string atteso)
    {
        var opzioni = new FornitoreAiOptions
        {
            Tipo = "claude-code",
            Modello = "m",
            ClaudeCode = new ClaudeCodeOptions { Eseguibile = "claude" },
            Registrazioni = new RegistrazioniOptions { Modalita = modalita, Cartella = cartella },
        };

        var risultato = new FornitoreAiOptionsValidator(new AmbienteCompleto()).Validate(null, opzioni);

        risultato.Failures.Should().ContainSingle().Which.Should().StartWith(atteso);
    }

    [Theory]
    [InlineData("{\"type\":\"result\",\"is_error\":true,\"result\":\"TESTO-DELLA-PAGINA 529 Overloaded\"}", 1)]
    [InlineData("{\"type\":\"result\",\"is_error\":true,\"result\":\"TESTO-DELLA-PAGINA You've hit your session limit\"}", 1)]
    [InlineData("{\"type\":\"result\",\"is_error\":true,\"result\":\"TESTO-DELLA-PAGINA Not logged in\"}", 1)]
    [InlineData("{\"type\":\"result\",\"is_error\":true,\"result\":\"TESTO-DELLA-PAGINA boh\"}", 1)]
    [InlineData("TESTO-DELLA-PAGINA non json", 1)]
    public void MessaggioDiErrore_NonContieneTestoDellaPaginaEHaIlCodiceDUscita(string stdout, int codice)
    {
        var uscita = UscitaClaudeCode.Analizza(stdout, "TESTO-DELLA-PAGINA stderr", codice, Adesso);

        uscita.Tipo.Should().NotBe(TipoUscitaClaudeCode.Successo);
        uscita.Messaggio.Should().NotContain("TESTO-DELLA-PAGINA").And.Contain($"codice d'uscita {codice}");
    }

    [Theory]
    [InlineData("{\"usage\":{\"input_tokens\":99999999999,\"output_tokens\":-3,\"cache_read_input_tokens\":1.5},\"total_cost_usd\":1e999}")]
    [InlineData("{\"usage\":{\"input_tokens\":\"x\"},\"total_cost_usd\":\"1\"}")]
    [InlineData("{\"usage\":{\"input_tokens\":2147483647,\"cache_creation_input_tokens\":2147483647},\"total_cost_usd\":-1}")]
    [InlineData("{\"usage\":5,\"total_cost_usd\":null}")]
    public void ConsumiStrani_NonFannoLanciareEdiventanoNulli(string consumi)
    {
        var json = "{\"type\":\"result\",\"is_error\":false,\"result\":\"\",\"structured_output\":{\"a\":1}," + consumi.TrimStart('{');

        var uscita = () => UscitaClaudeCode.Analizza(json, string.Empty, 0, Adesso);

        var risultato = uscita.Should().NotThrow().Subject;
        risultato.Tipo.Should().Be(TipoUscitaClaudeCode.Successo);
        risultato.Json.Should().Be("{\"a\":1}");
        risultato.CostoUsd.Should().BeNull();
        risultato.TokenUscita.Should().BeNull();
    }

    private sealed class AmbienteCompleto : IAmbienteFornitoreAi
    {
        public string CartellaUtente => "utente";

        public bool EWindows => false;

        public string? Variabile(string nome) => nome == "CLAUDE_CODE_OAUTH_TOKEN" ? "t" : null;

        public bool FileEsiste(string percorso) => false;

        public bool EseguibileNelPath(string nome) => true;
    }
}
