using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBEnigmistica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PrototipoCreazione : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "prototipo");

            migrationBuilder.CreateTable(
                name: "candidati",
                schema: "prototipo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipologia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    versione_tipologia = table.Column<int>(type: "integer", nullable: false),
                    famiglia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ordine_generazione = table.Column<int>(type: "integer", nullable: false),
                    generato_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    origine = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    brief = table.Column<string>(type: "jsonb", nullable: false),
                    seme = table.Column<long>(type: "bigint", nullable: true),
                    versione_generatore = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    schema_caselle = table.Column<string>(type: "jsonb", nullable: true),
                    versione_prompt = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    impronta_richiesta = table.Column<byte[]>(type: "bytea", nullable: true),
                    stato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    messaggio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    contenuto_generato = table.Column<string>(type: "jsonb", nullable: true),
                    contenuto_finale = table.Column<string>(type: "jsonb", nullable: true),
                    differenze = table.Column<string>(type: "jsonb", nullable: true),
                    tempo_attivo_ms = table.Column<long>(type: "bigint", nullable: false),
                    ultima_attivita_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revisione_iniziata_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revisione_conclusa_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_scarto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    nota_scarto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidati", x => x.id);
                    table.CheckConstraint("ck_candidati_accettato_finale", "stato <> 'accettato' OR contenuto_finale IS NOT NULL");
                    table.CheckConstraint("ck_candidati_contenuto_generato", "(contenuto_generato IS NOT NULL) = (stato IN ('da_rivedere', 'in_revisione', 'accettato', 'scartato'))");
                    table.CheckConstraint("ck_candidati_famiglia", "famiglia IN ('cruciverba', 'gioco-breve', 'crittografia', 'rebus', 'logico')");
                    table.CheckConstraint("ck_candidati_impronta_ai", "(modo = 'ai') = (impronta_richiesta IS NOT NULL)");
                    table.CheckConstraint("ck_candidati_impronta_lunghezza", "impronta_richiesta IS NULL OR octet_length(impronta_richiesta) = 32");
                    table.CheckConstraint("ck_candidati_modo", "modo IN ('ai', 'algoritmo')");
                    table.CheckConstraint("ck_candidati_motivo_scarto", "(stato = 'scartato') = (motivo_scarto IS NOT NULL)");
                    table.CheckConstraint("ck_candidati_motivo_scarto_valori", "motivo_scarto IS NULL OR motivo_scarto IN ('soluzione_errata', 'regole_non_rispettate', 'incroci_incoerenti', 'qualita_insufficiente', 'banale_o_troppo_facile', 'contenuto_inadatto', 'altro')");
                    table.CheckConstraint("ck_candidati_ordine", "ordine_generazione >= 1");
                    table.CheckConstraint("ck_candidati_origine", "origine = 'originale'");
                    table.CheckConstraint("ck_candidati_prompt_ai", "(modo = 'ai') = (versione_prompt IS NOT NULL)");
                    table.CheckConstraint("ck_candidati_revisione_conclusa", "(revisione_conclusa_il IS NOT NULL) = (stato IN ('accettato', 'scartato'))");
                    table.CheckConstraint("ck_candidati_seme_algoritmo", "modo <> 'algoritmo' OR seme IS NOT NULL");
                    table.CheckConstraint("ck_candidati_stato", "stato IN ('in_generazione', 'non_valido_schema', 'errore_generazione', 'da_rivedere', 'in_revisione', 'accettato', 'scartato')");
                    table.CheckConstraint("ck_candidati_tempo_attivo", "tempo_attivo_ms >= 0");
                    table.CheckConstraint("ck_candidati_versione_generatore", "(seme IS NULL) = (versione_generatore IS NULL)");
                    table.CheckConstraint("ck_candidati_versione_generatore_formato", "versione_generatore IS NULL OR versione_generatore ~ '^[a-z0-9][a-z0-9.-]{0,39}$'");
                    table.CheckConstraint("ck_candidati_versione_tipologia", "versione_tipologia >= 0");
                });

            migrationBuilder.CreateTable(
                name: "chiamate_ai",
                schema: "prototipo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    avviata_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    durata_ms = table.Column<int>(type: "integer", nullable: false),
                    uso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fornitore = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    modello = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    versione_prompt = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    impronta_richiesta = table.Column<byte[]>(type: "bytea", nullable: false),
                    tentativo = table.Column<int>(type: "integer", nullable: false),
                    correzione = table.Column<bool>(type: "boolean", nullable: false),
                    esito = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ripresa_il = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    codice_uscita = table.Column<int>(type: "integer", nullable: true),
                    messaggio_errore = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    token_ingresso = table.Column<int>(type: "integer", nullable: true),
                    token_uscita = table.Column<int>(type: "integer", nullable: true),
                    token_cache_creazione = table.Column<int>(type: "integer", nullable: true),
                    token_cache_lettura = table.Column<int>(type: "integer", nullable: true),
                    costo = table.Column<decimal>(type: "numeric(12,6)", nullable: true),
                    costo_valuta = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    costo_tipo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    risposta_grezza = table.Column<string>(type: "text", nullable: true),
                    risposta_sha256 = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chiamate_ai", x => x.id);
                    table.CheckConstraint("ck_chiamate_ai_costo", "costo IS NULL OR costo >= 0");
                    table.CheckConstraint("ck_chiamate_ai_costo_tipo", "(costo IS NULL) = (costo_tipo IS NULL) AND (costo_tipo IS NULL OR costo_tipo IN ('addebitato', 'indicativo'))");
                    table.CheckConstraint("ck_chiamate_ai_costo_valuta", "(costo IS NULL) = (costo_valuta IS NULL) AND (costo_valuta IS NULL OR costo_valuta ~ '^[A-Z]{3}$')");
                    table.CheckConstraint("ck_chiamate_ai_durata", "durata_ms >= 0");
                    table.CheckConstraint("ck_chiamate_ai_esito", "esito IN ('riuscita', 'risposta_non_valida', 'finestra_esaurita', 'errore_transitorio', 'errore_definitivo')");
                    table.CheckConstraint("ck_chiamate_ai_fornitore", "fornitore ~ '^[a-z][a-z0-9-]{0,39}$'");
                    table.CheckConstraint("ck_chiamate_ai_impronta", "octet_length(impronta_richiesta) = 32");
                    table.CheckConstraint("ck_chiamate_ai_risposta_sha256", "risposta_sha256 IS NULL OR octet_length(risposta_sha256) = 32");
                    table.CheckConstraint("ck_chiamate_ai_tentativo", "tentativo >= 1");
                    table.CheckConstraint("ck_chiamate_ai_token", "(token_ingresso IS NULL OR token_ingresso >= 0) AND (token_uscita IS NULL OR token_uscita >= 0) AND (token_cache_creazione IS NULL OR token_cache_creazione >= 0) AND (token_cache_lettura IS NULL OR token_cache_lettura >= 0)");
                    table.CheckConstraint("ck_chiamate_ai_uso", "uso = 'creazione'");
                    table.ForeignKey(
                        name: "FK_chiamate_ai_candidati_candidato_id",
                        column: x => x.candidato_id,
                        principalSchema: "prototipo",
                        principalTable: "candidati",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_candidati__tipologia_stato_ordine",
                schema: "prototipo",
                table: "candidati",
                columns: new[] { "tipologia", "stato", "ordine_generazione" });

            migrationBuilder.CreateIndex(
                name: "ux_candidati__tipologia_ordine",
                schema: "prototipo",
                table: "candidati",
                columns: new[] { "tipologia", "ordine_generazione" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chiamate_ai__avviata_il",
                schema: "prototipo",
                table: "chiamate_ai",
                column: "avviata_il");

            migrationBuilder.CreateIndex(
                name: "ix_chiamate_ai__candidato_id",
                schema: "prototipo",
                table: "chiamate_ai",
                column: "candidato_id");

            // Solo il ruolo di creazione tocca lo schema prototipo, senza DELETE: un candidato non si cancella.
            migrationBuilder.Sql("REVOKE ALL ON SCHEMA prototipo FROM PUBLIC;");
            migrationBuilder.Sql("GRANT USAGE ON SCHEMA prototipo TO dbe_creazione;");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON prototipo.candidati TO dbe_creazione;");
            // Delle chiamate si aggiorna solo il testo della risposta (cancellabile): metadati e SHA-256 restano immutabili.
            migrationBuilder.Sql("GRANT SELECT, INSERT ON prototipo.chiamate_ai TO dbe_creazione;");
            migrationBuilder.Sql("GRANT UPDATE (risposta_grezza) ON prototipo.chiamate_ai TO dbe_creazione;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("REVOKE ALL ON ALL TABLES IN SCHEMA prototipo FROM dbe_creazione;");
            migrationBuilder.Sql("REVOKE ALL ON SCHEMA prototipo FROM dbe_creazione;");

            migrationBuilder.DropTable(
                name: "chiamate_ai",
                schema: "prototipo");

            migrationBuilder.DropTable(
                name: "candidati",
                schema: "prototipo");
        }
    }
}
