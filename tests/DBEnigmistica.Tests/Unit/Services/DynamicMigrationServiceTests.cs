using DBEnigmistica.Domain.AI;
using DBEnigmistica.Infrastructure.Data;
using DBEnigmistica.Infrastructure.Services.Database;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DBEnigmistica.Tests.Unit.Services;

public class DynamicMigrationServiceTests
{
    private readonly Mock<ILogger<DynamicMigrationService>> _loggerMock;
    private readonly DynamicMigrationService _service;

    public DynamicMigrationServiceTests()
    {
        _loggerMock = new Mock<ILogger<DynamicMigrationService>>();
        var dbContextMock = new Mock<EnigmisticaDbContext>();
        _service = new DynamicMigrationService(_loggerMock.Object, dbContextMock.Object);
    }

    [Fact]
    public void GenerateCreateTableSql_BasicSchema_GeneratesCorrectSql()
    {
        // Arrange
        var schema = new GameTableSchema
        {
            TableName = "test_games",
            GameTypeName = "TestGame",
            Description = "Test game",
            Columns = new List<ColumnSchema>
            {
                new() { Name = "title", DataType = "TEXT", IsRequired = true, Description = "Game title" },
                new() { Name = "difficulty", DataType = "INTEGER", IsRequired = false }
            }
        };

        // Act - Use reflection to call private method for testing
        var method = typeof(DynamicMigrationService).GetMethod(
            "GenerateCreateTableSql",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var sql = method?.Invoke(_service, new object[] { schema }) as string;

        // Assert
        sql.Should().NotBeNullOrEmpty();
        sql.Should().Contain("CREATE TABLE IF NOT EXISTS test_games");
        sql.Should().Contain("id SERIAL PRIMARY KEY");
        sql.Should().Contain("game_id INTEGER NOT NULL");
        sql.Should().Contain("title TEXT NOT NULL");
        sql.Should().Contain("difficulty INTEGER");
        sql.Should().Contain("FOREIGN KEY (game_id) REFERENCES games(id)");
        sql.Should().Contain("CREATE INDEX");
    }

    [Theory]
    [InlineData("string", "TEXT")]
    [InlineData("int", "INTEGER")]
    [InlineData("bool", "BOOLEAN")]
    [InlineData("date", "DATE")]
    [InlineData("datetime", "TIMESTAMP")]
    [InlineData("json", "JSONB")]
    [InlineData("decimal", "DECIMAL")]
    public void MapDataType_ValidTypes_MapsCorrectly(string inputType, string expectedSqlType)
    {
        // Arrange & Act - Use reflection to call private method
        var method = typeof(DynamicMigrationService).GetMethod(
            "MapDataType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = method?.Invoke(_service, new object[] { inputType }) as string;

        // Assert
        result.Should().Be(expectedSqlType);
    }

    [Fact]
    public void MapDataType_UnknownType_ReturnsText()
    {
        // Arrange & Act
        var method = typeof(DynamicMigrationService).GetMethod(
            "MapDataType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = method?.Invoke(_service, new object[] { "unknown_type" }) as string;

        // Assert
        result.Should().Be("TEXT");
    }
}
