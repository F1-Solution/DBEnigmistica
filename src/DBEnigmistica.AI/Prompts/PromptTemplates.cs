namespace DBEnigmistica.AI.Prompts;

/// <summary>
/// Template dei prompt per l'AI
/// </summary>
public static class PromptTemplates
{
    public const string GameClassification = @"
Sei un esperto di giochi enigmistici italiani. Analizza il seguente testo estratto da una rivista di enigmistica e classifica il tipo di gioco.

TESTO:
{text}

METADATA (se disponibili):
{metadata}

Rispondi in formato JSON con questa struttura:
{{
  ""gameType"": ""tipo del gioco (Crossword, Rebus, Logic, Anagram, WordSearch, Sudoku, Cryptic, Quiz, Charade, Riddle, Unknown)"",
  ""gameTitle"": ""titolo del gioco se presente"",
  ""confidence"": 0.95,
  ""description"": ""breve descrizione del gioco identificato"",
  ""metadata"": {{
    ""difficulty"": ""facile/medio/difficile se deducibile"",
    ""subType"": ""sottotipo specifico se applicabile""
  }}
}}

Considera questi criteri:
- Cruciverba (Crossword): presenza di definizioni numerate, schema a griglia
- Rebus: presenza di immagini con lettere, chiave numerica tipo ""3,5 = 8""
- Logica (Logic): Sudoku, Kakuro, problemi logici
- Anagramma (Anagram): anagrammi, anagrammi con scarto/cambio
- Altri tipi in base alle caratteristiche tipiche
";

    public const string SchemaGeneration = @"
Sei un esperto database architect. Devi progettare una tabella PostgreSQL per memorizzare un tipo specifico di gioco enigmistico.

DESCRIZIONE DEL GIOCO:
{gameDescription}

NOME TIPO GIOCO: {gameTypeName}

La tabella deve:
1. Avere un Id (chiave primaria)
2. Avere un GameId (foreign key verso la tabella Games)
3. Contenere colonne specifiche per memorizzare tutti i dati caratteristici di questo tipo di gioco
4. Usare tipi di dati appropriati per PostgreSQL
5. Usare JSON per dati strutturati complessi

Rispondi in formato JSON con questa struttura:
{{
  ""gameTypeName"": ""nome del tipo di gioco"",
  ""tableName"": ""nome_tabella_in_snake_case"",
  ""description"": ""descrizione della tabella"",
  ""columns"": [
    {{
      ""name"": ""nome_colonna"",
      ""dataType"": ""tipo PostgreSQL (integer, varchar, text, boolean, json, timestamp, etc.)"",
      ""isNullable"": true,
      ""isRequired"": false,
      ""description"": ""descrizione del campo"",
      ""maxLength"": 500
    }}
  ],
  ""indexes"": [""GameId""]
}}

NON includere colonne Id e GameId nella lista - sono già gestite.
";

    public const string DataExtraction = @"
Sei un esperto di giochi enigmistici. Estrai i dati specifici dal seguente gioco di tipo {gameType}.

TESTO DEL GIOCO:
{text}

CONTESTO AGGIUNTIVO:
{context}

CAMPI DA ESTRARRE:
{fields}

Rispondi in formato JSON con questa struttura:
{{
  ""gameType"": ""{gameType}"",
  ""confidence"": 0.85,
  ""fields"": {{
    ""campo1"": ""valore1"",
    ""campo2"": ""valore2""
  }},
  ""warnings"": [""eventuali avvisi se alcuni dati non sono chiari""]
}}

Estrai accuratamente i dati richiesti. Se un campo non è presente o non è chiaro, usa null.
";

    public const string MarkdownAnalysis = @"
Sei un esperto database architect. Analizza la seguente descrizione di un gioco enigmistico in formato Markdown e progetta una tabella PostgreSQL adeguata per memorizzarlo.

CONTENUTO MARKDOWN:
{markdownContent}

La tabella deve:
1. Avere un Id (chiave primaria)
2. Avere un GameId (foreign key verso la tabella Games)
3. Contenere colonne specifiche per memorizzare tutti i dati caratteristici descritti nel markdown
4. Usare tipi di dati appropriati per PostgreSQL
5. Usare JSON per dati strutturati complessi

Rispondi in formato JSON con questa struttura:
{{
  ""gameTypeName"": ""nome del tipo di gioco estratto dal markdown"",
  ""tableName"": ""nome_tabella_in_snake_case"",
  ""description"": ""descrizione della tabella"",
  ""columns"": [
    {{
      ""name"": ""nome_colonna"",
      ""dataType"": ""tipo PostgreSQL"",
      ""isNullable"": true,
      ""isRequired"": false,
      ""description"": ""descrizione del campo"",
      ""maxLength"": null
    }}
  ],
  ""indexes"": [""GameId""]
}}

NON includere colonne Id e GameId nella lista - sono già gestite.
";
}
