namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Pagina da leggere: immagine PNG alla risoluzione <paramref name="Dpi"/>, prompt già compilato e JSON Schema.</summary>
public sealed record RichiestaEstrazionePagina(
    string ImprontaPagina,
    byte[] ImmaginePng,
    int Dpi,
    string Prompt,
    string VersionePrompt,
    string JsonSchema,
    bool TestoNativoTroncato = false);
