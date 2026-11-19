using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DBEnigmistica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "riviste");

            migrationBuilder.EnsureSchema(
                name: "catalogo");

            migrationBuilder.EnsureSchema(
                name: "registro");

            migrationBuilder.EnsureSchema(
                name: "tipologie");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.CreateTable(
                name: "eventi",
                schema: "registro",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    avvenuto_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entita = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entita_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    autore = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dati = table.Column<string>(type: "text", nullable: false),
                    impronta_precedente = table.Column<byte[]>(type: "bytea", nullable: false),
                    impronta = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventi", x => x.id);
                    table.CheckConstraint("ck_eventi_autore", "autore !~ '^[[:space:]]*$'");
                    table.CheckConstraint("ck_eventi_dati_json", "dati IS JSON OBJECT");
                    table.CheckConstraint("ck_eventi_entita", "entita ~ '^[a-z][a-z0-9_]{0,59}$'");
                    table.CheckConstraint("ck_eventi_entita_id", "entita_id !~ '^[[:space:]]*$'");
                    table.CheckConstraint("ck_eventi_impronta", "octet_length(impronta) = 32");
                    table.CheckConstraint("ck_eventi_impronta_precedente", "octet_length(impronta_precedente) = 32");
                    table.CheckConstraint("ck_eventi_tipo", "tipo ~ '^[a-z][a-z0-9_]{0,59}$'");
                });

            migrationBuilder.CreateTable(
                name: "game_type_schemas",
                schema: "tipologie",
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
                schema: "riviste",
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
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SolutionsInNextIssue = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_magazines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "soggetti",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    denominazione = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    identificativo_fiscale = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    creato_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_soggetti", x => x.id);
                    table.CheckConstraint("ck_soggetti_denominazione", "btrim(denominazione) <> ''");
                    table.CheckConstraint("ck_soggetti_tipo", "tipo IN ('persona','organizzazione')");
                });

            migrationBuilder.CreateTable(
                name: "pages",
                schema: "riviste",
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
                        principalSchema: "riviste",
                        principalTable: "magazines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "giochi",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipologia = table.Column<string>(type: "character varying(61)", maxLength: 61, nullable: false),
                    versione_tipologia = table.Column<int>(type: "integer", nullable: false),
                    titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    origine = table.Column<string>(type: "text", nullable: false),
                    titolare_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creato_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    prima_pubblicazione_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prima_pubblicazione_luogo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    pd_anno = table.Column<int>(type: "integer", nullable: true),
                    pd_autore = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pd_motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    stato_inedito = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_giochi", x => x.id);
                    table.CheckConstraint("ck_giochi_origine", "origine IN ('originale','licenza','pubblico_dominio')");
                    table.CheckConstraint("ck_giochi_pd_solo_pubblico_dominio", "pd_autore IS NULL OR origine = 'pubblico_dominio'");
                    table.CheckConstraint("ck_giochi_pd_tutti_o_nessuno", "(pd_autore IS NULL) = (pd_anno IS NULL) AND (pd_anno IS NULL) = (pd_motivo IS NULL)");
                    table.CheckConstraint("ck_giochi_stato_inedito", "(stato_inedito = 'inedito' AND prima_pubblicazione_il IS NULL AND prima_pubblicazione_luogo IS NULL) OR (stato_inedito = 'pubblicato' AND prima_pubblicazione_il IS NOT NULL AND prima_pubblicazione_luogo IS NOT NULL)");
                    table.CheckConstraint("ck_giochi_stato_inedito_valori", "stato_inedito IN ('inedito','pubblicato')");
                    table.CheckConstraint("ck_giochi_tipologia", "tipologia ~ '^[a-z][a-z0-9-]{1,60}$'");
                    table.CheckConstraint("ck_giochi_versione_tipologia", "versione_tipologia > 0");
                    table.ForeignKey(
                        name: "FK_giochi_soggetti_titolare_id",
                        column: x => x.titolare_id,
                        principalSchema: "catalogo",
                        principalTable: "soggetti",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "games",
                schema: "riviste",
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
                    ClassificationConfidence = table.Column<double>(type: "double precision", nullable: true),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    SolutionPageId = table.Column<int>(type: "integer", nullable: true),
                    HasSolution = table.Column<bool>(type: "boolean", nullable: false),
                    SolutionNotes = table.Column<string>(type: "text", nullable: true),
                    origine = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.Id);
                    table.CheckConstraint("ck_games_origine_rivista", "origine = 'rivista'");
                    table.ForeignKey(
                        name: "FK_games_pages_PageId",
                        column: x => x.PageId,
                        principalSchema: "riviste",
                        principalTable: "pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_games_pages_SolutionPageId",
                        column: x => x.SolutionPageId,
                        principalSchema: "riviste",
                        principalTable: "pages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "cessioni",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gioco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stipulata_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    esclusiva = table.Column<bool>(type: "boolean", nullable: false),
                    territorio = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    come_inedito = table.Column<bool>(type: "boolean", nullable: false),
                    fine = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    inizio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cessioni", x => x.id);
                    table.CheckConstraint("ck_cessioni_fine", "fine IS NULL OR fine > inizio");
                    table.CheckConstraint("ck_cessioni_territorio", "territorio ~ '^([A-Z]{2}|MONDO)$'");
                    table.ForeignKey(
                        name: "FK_cessioni_giochi_gioco_id",
                        column: x => x.gioco_id,
                        principalSchema: "catalogo",
                        principalTable: "giochi",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cessioni_soggetti_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "catalogo",
                        principalTable: "soggetti",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "licenze_in_entrata",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gioco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    licenziante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estremi_accordo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fine = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    inizio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_licenze_in_entrata", x => x.id);
                    table.CheckConstraint("ck_licenze_estremi", "btrim(estremi_accordo) <> ''");
                    table.CheckConstraint("ck_licenze_fine", "fine IS NULL OR fine > inizio");
                    table.ForeignKey(
                        name: "FK_licenze_in_entrata_giochi_gioco_id",
                        column: x => x.gioco_id,
                        principalSchema: "catalogo",
                        principalTable: "giochi",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_licenze_in_entrata_soggetti_licenziante_id",
                        column: x => x.licenziante_id,
                        principalSchema: "catalogo",
                        principalTable: "soggetti",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "anagram_games",
                schema: "riviste",
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
                        principalSchema: "riviste",
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_solutions",
                schema: "riviste",
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
                        principalSchema: "riviste",
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_solutions_pages_SolutionPageId",
                        column: x => x.SolutionPageId,
                        principalSchema: "riviste",
                        principalTable: "pages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "logic_games",
                schema: "riviste",
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
                        principalSchema: "riviste",
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_anagram_games_GameId",
                schema: "riviste",
                table: "anagram_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cessioni_cliente_id",
                schema: "catalogo",
                table: "cessioni",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_cessioni_gioco_id",
                schema: "catalogo",
                table: "cessioni",
                column: "gioco_id");

            migrationBuilder.CreateIndex(
                name: "IX_crossword_games_GameId",
                schema: "riviste",
                table: "crossword_games",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventi_impronta",
                schema: "registro",
                table: "eventi",
                column: "impronta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventi_impronta_precedente",
                schema: "registro",
                table: "eventi",
                column: "impronta_precedente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_GameId",
                schema: "riviste",
                table: "game_solutions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_IsVerified",
                schema: "riviste",
                table: "game_solutions",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_SolutionPageId",
                schema: "riviste",
                table: "game_solutions",
                column: "SolutionPageId");

            migrationBuilder.CreateIndex(
                name: "IX_game_solutions_SolutionType",
                schema: "riviste",
                table: "game_solutions",
                column: "SolutionType");

            migrationBuilder.CreateIndex(
                name: "IX_game_type_schemas_GameTypeName",
                schema: "tipologie",
                table: "game_type_schemas",
                column: "GameTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_game_type_schemas_TableName",
                schema: "tipologie",
                table: "game_type_schemas",
                column: "TableName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_games_GameType",
                schema: "riviste",
                table: "games",
                column: "GameType");

            migrationBuilder.CreateIndex(
                name: "IX_games_PageId",
                schema: "riviste",
                table: "games",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_games_SolutionPageId",
                schema: "riviste",
                table: "games",
                column: "SolutionPageId");

            migrationBuilder.CreateIndex(
                name: "IX_giochi_tipologia",
                schema: "catalogo",
                table: "giochi",
                column: "tipologia");

            migrationBuilder.CreateIndex(
                name: "IX_giochi_titolare_id",
                schema: "catalogo",
                table: "giochi",
                column: "titolare_id");

            migrationBuilder.CreateIndex(
                name: "IX_licenze_in_entrata_gioco_id",
                schema: "catalogo",
                table: "licenze_in_entrata",
                column: "gioco_id");

            migrationBuilder.CreateIndex(
                name: "IX_licenze_in_entrata_licenziante_id",
                schema: "catalogo",
                table: "licenze_in_entrata",
                column: "licenziante_id");

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
                name: "IX_magazines_Title_PublicationDate",
                schema: "riviste",
                table: "magazines",
                columns: new[] { "Title", "PublicationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_magazines_Year",
                schema: "riviste",
                table: "magazines",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_pages_MagazineId_PageNumber",
                schema: "riviste",
                table: "pages",
                columns: new[] { "MagazineId", "PageNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_rebus_games_GameId",
                schema: "riviste",
                table: "rebus_games",
                column: "GameId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anagram_games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "cessioni",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "crossword_games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "eventi",
                schema: "registro");

            migrationBuilder.DropTable(
                name: "game_solutions",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "game_type_schemas",
                schema: "tipologie");

            migrationBuilder.DropTable(
                name: "licenze_in_entrata",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "logic_games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "rebus_games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "giochi",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "games",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "soggetti",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "pages",
                schema: "riviste");

            migrationBuilder.DropTable(
                name: "magazines",
                schema: "riviste");
        }
    }
}
