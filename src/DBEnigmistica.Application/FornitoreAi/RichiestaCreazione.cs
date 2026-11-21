using System.Security.Cryptography;
using System.Text;

namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Richiesta di un candidato: prompt compilato e JSON Schema li produce solo il compilatore del prompt.</summary>
public sealed record RichiestaCreazione(
    string Tipologia,
    int VersioneTipologia,
    string BriefJson,
    string Prompt,
    string VersionePrompt,
    string JsonSchema)
{
    /// <summary>SHA-256 di prompt, byte 0x00 e schema, in esadecimale minuscolo.</summary>
    public string ImprontaRichiesta => Convert.ToHexStringLower(ImprontaByte());

    public byte[] ImprontaByte()
    {
        var prompt = Encoding.UTF8.GetBytes(Prompt);
        var schema = Encoding.UTF8.GetBytes(JsonSchema);
        var dati = new byte[prompt.Length + 1 + schema.Length];
        prompt.CopyTo(dati, 0);
        schema.CopyTo(dati, prompt.Length + 1);
        return SHA256.HashData(dati);
    }
}
