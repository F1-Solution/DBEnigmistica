using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DBEnigmistica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSolutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SolutionsInNextIssue",
                table: "magazines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSolution",
                table: "games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolutionNotes",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SolutionPageId",
                table: "games",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "game_solutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    SolutionText = table.Column<string>(type: "text", nullable: false),
                    SolutionPageId = table.Column<int>(type: "integer", nullable: true),
                    SolutionType = table.Column<string>(type: "text", nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: true),
                    PositionY = table.Column<int>(type: "integer", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ExtractedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_solutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_game_solutions_games_GameId",
                        column: x => x.GameId,
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_solutions_pages_SolutionPageId",
                        column: x => x.SolutionPageId,
                        principalTable: "pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_games_SolutionPageId",
                table: "games",
                column: "SolutionPageId");

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_GameId",
                table: "game_solutions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_IsVerified",
                table: "game_solutions",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_SolutionPageId",
                table: "game_solutions",
                column: "SolutionPageId");

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_SolutionType",
                table: "game_solutions",
                column: "SolutionType");

            migrationBuilder.AddForeignKey(
                name: "FK_games_pages_SolutionPageId",
                table: "games",
                column: "SolutionPageId",
                principalTable: "pages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_games_pages_SolutionPageId",
                table: "games");

            migrationBuilder.DropTable(
                name: "game_solutions");

            migrationBuilder.DropIndex(
                name: "IX_games_SolutionPageId",
                table: "games");

            migrationBuilder.DropColumn(
                name: "SolutionsInNextIssue",
                table: "magazines");

            migrationBuilder.DropColumn(
                name: "HasSolution",
                table: "games");

            migrationBuilder.DropColumn(
                name: "Solution",
                table: "games");

            migrationBuilder.DropColumn(
                name: "SolutionNotes",
                table: "games");

            migrationBuilder.DropColumn(
                name: "SolutionPageId",
                table: "games");
        }
    }
}
