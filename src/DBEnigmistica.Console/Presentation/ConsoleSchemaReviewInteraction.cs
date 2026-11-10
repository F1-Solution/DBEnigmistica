using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Domain.AI;
using Spectre.Console;

namespace DBEnigmistica.Console.Presentation;

/// <summary>Resa a schermo con Spectre.Console della revisione di uno schema.</summary>
public sealed class ConsoleSchemaReviewInteraction(IAnsiConsole console) : ISchemaReviewInteraction
{
    public void ShowProposal(GameTableSchema schema)
    {
        console.WriteLine();
        console.Write(new Rule($"PROPOSTA SCHEMA: {Markup.Escape(schema.GameTypeName)}").LeftJustified());
        console.MarkupLineInterpolated($"Nome tabella: {schema.TableName}");

        if (!string.IsNullOrEmpty(schema.Description))
            console.MarkupLineInterpolated($"Descrizione: {schema.Description}");

        var table = new Table().AddColumn("Colonna").AddColumn("Tipo").AddColumn("Nullable").AddColumn("Note");
        foreach (var column in schema.Columns)
        {
            var notes = column.IsRequired ? "obbligatoria" : "";
            if (!string.IsNullOrEmpty(column.Description))
                notes = $"{notes} {column.Description}".Trim();

            table.AddRow(
                Markup.Escape(column.Name),
                Markup.Escape(column.DataType),
                column.IsNullable ? "sì" : "no",
                Markup.Escape(notes));
        }

        console.Write(table);

        if (schema.Indexes.Count > 0)
            console.MarkupLineInterpolated($"Indici: {string.Join(", ", schema.Indexes)}");
    }

    public string? AskApproval()
    {
        console.WriteLine();
        return Ask("Vuoi approvare questo schema? (s/n/m per modificare)");
    }

    public void ShowEditHelp()
    {
        console.WriteLine();
        console.MarkupLine("[bold]Modalità modifica schema[/]. Comandi disponibili:");
        console.MarkupLine("  rename-table <nuovo_nome>");
        console.MarkupLine("  add-column <nome> <tipo> [[nullable]]");
        console.MarkupLine("  remove-column <nome>");
        console.MarkupLine("  modify-column <nome> <nuovo_tipo>");
        console.MarkupLine("  done (per finire)");
        console.WriteLine();
    }

    public string? AskEditCommand() => Ask("Modifica>");

    public void ShowEditResult(string message) => console.MarkupLineInterpolated($"{message}");

    private string? Ask(string prompt)
    {
        if (!console.Profile.Capabilities.Interactive)
            return null;

        console.Markup($"{Markup.Escape(prompt)} ");
        return System.Console.ReadLine();
    }
}
