namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Porta di validazione di un JSON contro il suo JSON Schema; restituisce gli errori, vuoto se valido.</summary>
public interface IValidatoreSchemaJson
{
    IReadOnlyList<string> Valida(string json, string jsonSchema);
}
