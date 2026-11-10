using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DBEnigmistica.Tests.Unit.Services;

public class SchemaValidationServiceTests
{
    private readonly Mock<ILogger<SchemaValidationService>> _loggerMock;
    private readonly Mock<IGameTypeSchemaRepository> _repositoryMock;
    private readonly SchemaValidationService _service;

    public SchemaValidationServiceTests()
    {
        _loggerMock = new Mock<ILogger<SchemaValidationService>>();
        _repositoryMock = new Mock<IGameTypeSchemaRepository>();
        _service = new SchemaValidationService(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task ValidateSchemaConsistency_ValidSchema_ReturnsTrue()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "test_game",
            GameTypeName = "TestGame",
            Description = "Test game type",
            Columns = new List<ColumnSchema>
            {
                new() { Name = "title", DataType = "TEXT", IsRequired = true },
                new() { Name = "difficulty", DataType = "INTEGER", IsRequired = false }
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateSchemaConsistency_EmptyTableName_ReturnsFalse()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "",
            GameTypeName = "TestGame",
            Description = "Test",
            Columns = new List<ColumnSchema>()
        };

        // Act
        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Nome tabella"));
    }

    [Fact]
    public async Task ValidateSchemaConsistency_InvalidTableName_ReturnsFalse()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "123invalid",
            GameTypeName = "TestGame",
            Description = "Test",
            Columns = new List<ColumnSchema>()
        };

        // Act
        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Nome tabella non valido"));
    }

    [Fact]
    public async Task ValidateSchemaConsistency_NoColumns_ReturnsFalse()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "test_game",
            GameTypeName = "TestGame",
            Description = "Test",
            Columns = new List<ColumnSchema>()
        };

        // Act
        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("almeno una colonna"));
    }

    [Fact]
    public async Task ValidateSchemaConsistency_DuplicateColumns_ReturnsFalse()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "test_game",
            GameTypeName = "TestGame",
            Description = "Test",
            Columns = new List<ColumnSchema>
            {
                new() { Name = "title", DataType = "TEXT", IsRequired = true },
                new() { Name = "title", DataType = "TEXT", IsRequired = false }
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("duplicate"));
    }

    [Fact]
    public async Task ValidateSchemaConsistency_InvalidColumnName_ReturnsFalse()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "test_game",
            GameTypeName = "TestGame",
            Description = "Test",
            Columns = new List<ColumnSchema>
            {
                new() { Name = "invalid-name", DataType = "TEXT", IsRequired = true }
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("nome colonna non valido"));
    }

    [Fact]
    public async Task ValidateSchemaConsistency_ReservedColumnName_ReturnsFalse()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "test_game",
            GameTypeName = "TestGame",
            Description = "Test",
            Columns = new List<ColumnSchema>
            {
                new() { Name = "id", DataType = "INTEGER", IsRequired = true }
            }
        };

        // Act
        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("riservato"));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("ID")]
    [InlineData("game_id")]
    public async Task ValidateSchemaConsistency_ReservedColumnNames_AreRejectedCaseInsensitively(string columnName)
    {
        var schema = NewSchema(new ColumnSchema { Name = columnName, DataType = "INTEGER", IsRequired = true });

        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be($"Colonna '{columnName}': nome riservato");
    }

    [Fact]
    public async Task ValidateSchemaConsistency_ExistingTableName_ReturnsFalseWithMessage()
    {
        _repositoryMock.Setup(r => r.TableNameExistsAsync("test_game")).ReturnsAsync(true);
        var schema = NewSchema(new ColumnSchema { Name = "title", DataType = "TEXT" });

        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Il nome tabella 'test_game' esiste già");
    }

    [Fact]
    public async Task ValidateSchemaConsistency_EmptyTableName_FixesMessage()
    {
        var schema = new GameTableSchema
        {
            TableName = "",
            GameTypeName = "TestGame",
            Description = "Test",
            Columns = [new ColumnSchema { Name = "title", DataType = "TEXT" }]
        };

        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Nome tabella obbligatorio");
    }

    [Fact]
    public async Task ValidateSchemaConsistency_EmptyColumnName_FixesMessage()
    {
        var schema = NewSchema(new ColumnSchema { Name = "", DataType = "TEXT" });

        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        isValid.Should().BeFalse();
        errors.Should().Contain("Il nome della colonna non può essere vuoto");
    }

    [Fact]
    public async Task ValidateSchemaConsistency_MissingDataType_FixesMessage()
    {
        var schema = NewSchema(new ColumnSchema { Name = "title", DataType = "" });

        var (isValid, errors) = await _service.ValidateSchemaConsistencyAsync(schema);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Tipo di dato obbligatorio per la colonna: title");
    }

    private static GameTableSchema NewSchema(params ColumnSchema[] columns) => new()
    {
        TableName = "test_game",
        GameTypeName = "TestGame",
        Description = "Test",
        Columns = columns.ToList()
    };
}
