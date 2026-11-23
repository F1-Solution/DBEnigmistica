using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DBEnigmistica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NucleoRivisteTipologie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nessuna conversione dei Giochi del vecchio codice: chi ne ha va fermato prima di toccare lo schema.
            migrationBuilder.Sql(ControlloTabellaVuota(
                "La tabella riviste.games contiene Giochi del vecchio codice, che non si convertono: svuota la tabella (per esempio con DELETE FROM riviste.games;) e riesegui dbe migrate."));

            migrationBuilder.DropTable(
                name: "anagram_games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "crossword_games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "logic_games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "rebus_games",
                schema: "riviste");

            migrationBuilder.DropIndex(
                name: "IX_games_GameType",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "BoundingBoxHeight",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "BoundingBoxWidth",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "GameType",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "PositionX",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "PositionY",
                schema: "riviste",
                table: "games");

            migrationBuilder.RenameColumn(
                name: "ClassificationConfidence",
                schema: "riviste",
                table: "games",
                newName: "confidenza");

            migrationBuilder.AddColumn<byte[]>(
                name: "impronta",
                schema: "riviste",
                table: "pages",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "impronta",
                schema: "riviste",
                table: "magazines",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "avvisi",
                schema: "riviste",
                table: "games",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<decimal>(
                name: "riquadro_altezza",
                schema: "riviste",
                table: "games",
                type: "numeric(6,5)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "riquadro_larghezza",
                schema: "riviste",
                table: "games",
                type: "numeric(6,5)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "riquadro_x",
                schema: "riviste",
                table: "games",
                type: "numeric(6,5)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "riquadro_y",
                schema: "riviste",
                table: "games",
                type: "numeric(6,5)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stato_tipologia",
                schema: "riviste",
                table: "games",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tipologia",
                schema: "riviste",
                table: "games",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "trascrizione_proposta",
                schema: "riviste",
                table: "games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "versione_tipologia",
                schema: "riviste",
                table: "games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_pages_impronta",
                schema: "riviste",
                table: "pages",
                column: "impronta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_magazines_impronta",
                schema: "riviste",
                table: "magazines",
                column: "impronta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_games_tipologia",
                schema: "riviste",
                table: "games",
                column: "tipologia");

            migrationBuilder.AddCheckConstraint(
                name: "ck_games_confidenza",
                schema: "riviste",
                table: "games",
                sql: "confidenza IS NULL OR confidenza BETWEEN 0 AND 1");

            migrationBuilder.AddCheckConstraint(
                name: "ck_games_riquadro",
                schema: "riviste",
                table: "games",
                sql: "(riquadro_x IS NULL OR riquadro_x BETWEEN 0 AND 1) AND (riquadro_y IS NULL OR riquadro_y BETWEEN 0 AND 1) AND (riquadro_larghezza IS NULL OR riquadro_larghezza BETWEEN 0 AND 1) AND (riquadro_altezza IS NULL OR riquadro_altezza BETWEEN 0 AND 1)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_games_stato_tipologia",
                schema: "riviste",
                table: "games",
                sql: "stato_tipologia IN ('nota', 'proposta')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_games_trascrizione_proposta",
                schema: "riviste",
                table: "games",
                sql: "trascrizione_proposta IS NULL OR stato_tipologia = 'proposta'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_games_versione_tipologia",
                schema: "riviste",
                table: "games",
                sql: "versione_tipologia >= 0");

            // I valori di riempimento servono solo ad aggiungere le colonne: ogni Gioco dichiara i propri.
            migrationBuilder.Sql(
                "ALTER TABLE riviste.games ALTER COLUMN avvisi DROP DEFAULT, ALTER COLUMN stato_tipologia DROP DEFAULT, " +
                "ALTER COLUMN tipologia DROP DEFAULT, ALTER COLUMN versione_tipologia DROP DEFAULT;");
        }

        private static string ControlloTabellaVuota(string messaggio) => $$"""
            DO $$
            BEGIN
              IF EXISTS (SELECT 1 FROM riviste.games) THEN
                RAISE EXCEPTION '{{messaggio}}' USING ERRCODE = 'feature_not_supported';
              END IF;
            END
            $$;
            """;

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ControlloTabellaVuota(
                "La tabella riviste.games contiene Giochi con Tipologia, che non tornano al vecchio schema: svuota la tabella (per esempio con DELETE FROM riviste.games;) e riesegui dbe migrate."));

            migrationBuilder.DropIndex(
                name: "IX_pages_impronta",
                schema: "riviste",
                table: "pages");

            migrationBuilder.DropIndex(
                name: "IX_magazines_impronta",
                schema: "riviste",
                table: "magazines");

            migrationBuilder.DropIndex(
                name: "IX_games_tipologia",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropCheckConstraint(
                name: "ck_games_confidenza",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropCheckConstraint(
                name: "ck_games_riquadro",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropCheckConstraint(
                name: "ck_games_stato_tipologia",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropCheckConstraint(
                name: "ck_games_trascrizione_proposta",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropCheckConstraint(
                name: "ck_games_versione_tipologia",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "impronta",
                schema: "riviste",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "impronta",
                schema: "riviste",
                table: "magazines");

            migrationBuilder.DropColumn(
                name: "avvisi",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "riquadro_altezza",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "riquadro_larghezza",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "riquadro_x",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "riquadro_y",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "stato_tipologia",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "tipologia",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "trascrizione_proposta",
                schema: "riviste",
                table: "games");

            migrationBuilder.DropColumn(
                name: "versione_tipologia",
                schema: "riviste",
                table: "games");

            migrationBuilder.RenameColumn(
                name: "confidenza",
                schema: "riviste",
                table: "games",
                newName: "ClassificationConfidence");

            migrationBuilder.AddColumn<int>(
                name: "BoundingBoxHeight",
                schema: "riviste",
                table: "games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoundingBoxWidth",
                schema: "riviste",
                table: "games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameType",
                schema: "riviste",
                table: "games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("ALTER TABLE riviste.games ALTER COLUMN \"GameType\" DROP DEFAULT;");

            migrationBuilder.AddColumn<int>(
                name: "PositionX",
                schema: "riviste",
                table: "games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionY",
                schema: "riviste",
                table: "games",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "anagram_games",
                schema: "riviste",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    AddedLetters = table.Column<string>(type: "text", nullable: true),
                    AnagramType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Clue = table.Column<string>(type: "text", nullable: true),
                    DiscardedLetters = table.Column<string>(type: "text", nullable: true),
                    LengthPattern = table.Column<string>(type: "text", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    WordCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anagram_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_anagram_games_games_GameId",
                        column: x => x.GameId,
                        principalSchema: "riviste",
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crossword_games",
                schema: "riviste",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Columns = table.Column<int>(type: "integer", nullable: false),
                    GridStructure = table.Column<string>(type: "text", nullable: true),
                    HorizontalClues = table.Column<string>(type: "text", nullable: true),
                    IsFreeSchema = table.Column<bool>(type: "boolean", nullable: false),
                    IsSyllabic = table.Column<bool>(type: "boolean", nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    VerticalClues = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crossword_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crossword_games_games_GameId",
                        column: x => x.GameId,
                        principalSchema: "riviste",
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logic_games",
                schema: "riviste",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    Constraints = table.Column<string>(type: "text", nullable: true),
                    EstimatedDifficulty = table.Column<string>(type: "text", nullable: true),
                    GameStructure = table.Column<string>(type: "text", nullable: true),
                    GridSize = table.Column<string>(type: "text", nullable: true),
                    InitialData = table.Column<string>(type: "text", nullable: true),
                    ProblemText = table.Column<string>(type: "text", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    SubType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logic_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_logic_games_games_GameId",
                        column: x => x.GameId,
                        principalSchema: "riviste",
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rebus_games",
                schema: "riviste",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    ImageDescriptions = table.Column<string>(type: "text", nullable: true),
                    ImagePaths = table.Column<string>(type: "text", nullable: true),
                    IsPictorial = table.Column<bool>(type: "boolean", nullable: false),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reading = table.Column<string>(type: "text", nullable: true),
                    RebusText = table.Column<string>(type: "text", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rebus_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rebus_games_games_GameId",
                        column: x => x.GameId,
                        principalSchema: "riviste",
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_games_GameType",
                schema: "riviste",
                table: "games",
                column: "GameType");

            migrationBuilder.CreateIndex(
                name: "IX_anagram_games_GameId",
                schema: "riviste",
                table: "anagram_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_crossword_games_GameId",
                schema: "riviste",
                table: "crossword_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logic_games_GameId",
                schema: "riviste",
                table: "logic_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logic_games_SubType",
                schema: "riviste",
                table: "logic_games",
                column: "SubType");

            migrationBuilder.CreateIndex(
                name: "IX_rebus_games_GameId",
                schema: "riviste",
                table: "rebus_games",
                column: "GameId",
                unique: true);
        }
    }
}
