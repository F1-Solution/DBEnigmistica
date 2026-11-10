using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DBEnigmistica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_type_schemas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SchemaJson = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsUserValidated = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_type_schemas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "magazines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Periodicity = table.Column<string>(type: "text", nullable: false),
                    IssueNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    SourceFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_magazines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MagazineId = table.Column<int>(type: "integer", nullable: false),
                    PageNumber = table.Column<int>(type: "integer", nullable: false),
                    OcrText = table.Column<string>(type: "text", nullable: true),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pages_magazines_MagazineId",
                        column: x => x.MagazineId,
                        principalTable: "magazines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    GameType = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PositionX = table.Column<int>(type: "integer", nullable: true),
                    PositionY = table.Column<int>(type: "integer", nullable: true),
                    BoundingBoxWidth = table.Column<int>(type: "integer", nullable: true),
                    BoundingBoxHeight = table.Column<int>(type: "integer", nullable: true),
                    Difficulty = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ExtractedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClassificationConfidence = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_games_pages_PageId",
                        column: x => x.PageId,
                        principalTable: "pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "anagram_games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    AnagramType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Clue = table.Column<string>(type: "text", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    WordCount = table.Column<int>(type: "integer", nullable: true),
                    LengthPattern = table.Column<string>(type: "text", nullable: true),
                    DiscardedLetters = table.Column<string>(type: "text", nullable: true),
                    AddedLetters = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anagram_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_anagram_games_games_GameId",
                        column: x => x.GameId,
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crossword_games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    Columns = table.Column<int>(type: "integer", nullable: false),
                    GridStructure = table.Column<string>(type: "text", nullable: true),
                    HorizontalClues = table.Column<string>(type: "text", nullable: true),
                    VerticalClues = table.Column<string>(type: "text", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    IsSyllabic = table.Column<bool>(type: "boolean", nullable: false),
                    IsFreeSchema = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crossword_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crossword_games_games_GameId",
                        column: x => x.GameId,
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logic_games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    SubType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProblemText = table.Column<string>(type: "text", nullable: true),
                    InitialData = table.Column<string>(type: "text", nullable: true),
                    GameStructure = table.Column<string>(type: "text", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    Constraints = table.Column<string>(type: "text", nullable: true),
                    GridSize = table.Column<string>(type: "text", nullable: true),
                    EstimatedDifficulty = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logic_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_logic_games_games_GameId",
                        column: x => x.GameId,
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rebus_games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RebusText = table.Column<string>(type: "text", nullable: true),
                    ImageDescriptions = table.Column<string>(type: "text", nullable: true),
                    ImagePaths = table.Column<string>(type: "text", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    Reading = table.Column<string>(type: "text", nullable: true),
                    IsPictorial = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rebus_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rebus_games_games_GameId",
                        column: x => x.GameId,
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_anagram_games_GameId",
                table: "anagram_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_crossword_games_GameId",
                table: "crossword_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_type_schemas_GameTypeName",
                table: "game_type_schemas",
                column: "GameTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_game_type_schemas_TableName",
                table: "game_type_schemas",
                column: "TableName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_games_GameType",
                table: "games",
                column: "GameType");

            migrationBuilder.CreateIndex(
                name: "IX_games_PageId",
                table: "games",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_logic_games_GameId",
                table: "logic_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logic_games_SubType",
                table: "logic_games",
                column: "SubType");

            migrationBuilder.CreateIndex(
                name: "IX_magazines_Title_PublicationDate",
                table: "magazines",
                columns: new[] { "Title", "PublicationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_magazines_Year",
                table: "magazines",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_pages_MagazineId_PageNumber",
                table: "pages",
                columns: new[] { "MagazineId", "PageNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_rebus_games_GameId",
                table: "rebus_games",
                column: "GameId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anagram_games");

            migrationBuilder.DropTable(
                name: "crossword_games");

            migrationBuilder.DropTable(
                name: "game_type_schemas");

            migrationBuilder.DropTable(
                name: "logic_games");

            migrationBuilder.DropTable(
                name: "rebus_games");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "pages");

            migrationBuilder.DropTable(
                name: "magazines");
        }
    }
}
