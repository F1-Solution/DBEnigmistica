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
}
