using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DBEnigmistica.Tests.Unit.Cli;

public class SchemaReviewDialogTests
{
    private sealed class ScriptedInteraction(string? approval, params string?[] editCommands) : ISchemaReviewInteraction
    {
        private readonly Queue<string?> _edits = new(editCommands);

        public List<string> Messages { get; } = [];

        public void ShowProposal(GameTableSchema schema) { }

        public string? AskApproval() => approval;

        public void ShowEditHelp() { }

        public string? AskEditCommand() => _edits.Count > 0 ? _edits.Dequeue() : null;

        public void ShowEditResult(string message) => Messages.Add(message);
    }

    private static GameTableSchema Schema() => new()
    {
        GameTypeName = "Cruciverba",
        TableName = "cruciverba",
        Columns = [new ColumnSchema { Name = "griglia", DataType = "text" }]
    };

    private static SchemaValidationService Service(ISchemaReviewInteraction? interaction) =>
        new(NullLogger<SchemaValidationService>.Instance, new Mock<IGameTypeSchemaRepository>().Object, interaction);

    [Theory]
    [InlineData("s")]
    [InlineData("SI")]
    [InlineData("sì")]
    [InlineData("yes")]
    public async Task Approval_ReturnsProposedSchema(string answer)
    {
        var proposed = Schema();

        var (approved, schema) = await Service(new ScriptedInteraction(answer)).ValidateSchemaWithUserAsync(proposed);

        approved.Should().BeTrue();
        schema.Should().BeSameAs(proposed);
    }

    [Theory]
    [InlineData("n")]
    [InlineData("")]
    [InlineData(null)]
    public async Task OtherAnswers_RejectTheSchema(string? answer)
    {
        var (approved, schema) = await Service(new ScriptedInteraction(answer)).ValidateSchemaWithUserAsync(Schema());

        approved.Should().BeFalse();
        schema.Should().BeNull();
    }

    [Fact]
    public async Task Modify_AppliesEditCommandsUntilDone()
    {
        var interaction = new ScriptedInteraction(
            "m", "rename-table nuova", "add-column colore text nullable", "modify-column griglia varchar", "done");

        var (approved, schema) = await Service(interaction).ValidateSchemaWithUserAsync(Schema());

        approved.Should().BeTrue();
        schema!.TableName.Should().Be("nuova");
        schema.Columns.Select(c => c.Name).Should().Equal("griglia", "colore");
        schema.Columns[0].DataType.Should().Be("varchar");
        schema.Columns[1].IsNullable.Should().BeTrue();
    }

    [Fact]
    public async Task Modify_EndsAtEndOfInputInsteadOfLoopingForever()
    {
        var interaction = new ScriptedInteraction("m", "comando-sconosciuto");

        var (approved, _) = await Service(interaction).ValidateSchemaWithUserAsync(Schema());

        approved.Should().BeTrue();
        interaction.Messages.Should().Contain("Comando non riconosciuto");
    }

    [Fact]
    public async Task WithoutInteraction_ThrowsInsteadOfReadingTheConsole()
    {
        var act = () => Service(null).ValidateSchemaWithUserAsync(Schema());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
