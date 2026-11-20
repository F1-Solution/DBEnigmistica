using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Corvus.Text.Json;
using Corvus.Text.Json.Validator;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.AI.Services;

/// <summary>Validatore JSON Schema bozza 2020-12 su Corvus.Text.Json.Validator; gli schemi compilati si riusano.</summary>
public sealed class ValidatoreSchemaJson : IValidatoreSchemaJson
{
    private const int MassimoErrori = 10;

    private readonly ConcurrentDictionary<string, JsonSchema> _schemi = new(StringComparer.Ordinal);

    // Gli schemi senza $id hanno bisogno di un URI canonico: lo si ricava dall'impronta del testo.
    private static string UriCanonico(string schema) =>
        "https://schemi.dbenigmistica.invalid/" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(schema))).ToLowerInvariant();

    public IReadOnlyList<string> Valida(string json, string jsonSchema)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return [$"JSON non valido: {ex.Message}"];
        }

        JsonSchema schema;
        try
        {
            schema = _schemi.GetOrAdd(jsonSchema, testo => JsonSchema.FromText(testo, UriCanonico(testo)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new InvalidOperationException($"JSON Schema non utilizzabile: {ex.Message}", ex);
        }

        using var raccolta = JsonSchemaResultsCollector.Create(JsonSchemaResultsLevel.Detailed);
        if (schema.Validate(json, raccolta))
            return [];

        // Si scartano le righe senza messaggio e quelle generiche "deve corrispondere al sottoschema": restano posizione e motivo.
        var errori = new List<string>();
        foreach (var risultato in raccolta.EnumerateResults())
        {
            if (risultato.IsMatch)
                continue;

            var messaggio = risultato.GetMessageText();
            if (string.IsNullOrWhiteSpace(messaggio) || messaggio.Contains("to match the subschema", StringComparison.OrdinalIgnoreCase))
                continue;

            var posizione = risultato.GetDocumentEvaluationLocationText();
            var voce = $"{(string.IsNullOrEmpty(posizione) ? "(radice)" : posizione)}: {messaggio}";
            if (!errori.Contains(voce))
                errori.Add(voce);

            if (errori.Count == MassimoErrori)
                break;
        }

        return errori.Count > 0 ? errori : ["La risposta non rispetta lo schema"];
    }
}
