using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBEnigmistica.Infrastructure.Migrations
{
    /// <summary>Registro delle migrazioni SQL delle Tipologie applicate da <c>dbe migrate</c>, senza entità EF (scritta a mano).</summary>
    [DbContext(typeof(EnigmisticaDbContext))]
    [Migration("20261121140000_TipologieMigrazioniApplicate")]
    public partial class TipologieMigrazioniApplicate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // I permessi di lettura arrivano dai default privileges dello schema (VincoliRuoliERegistro), perché la crea il ruolo che migra.
            migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS tipologie;");
            migrationBuilder.Sql("""
                CREATE TABLE tipologie.migrazioni_applicate (
                    id bigint GENERATED ALWAYS AS IDENTITY,
                    tipologia varchar(30) NOT NULL,
                    versione integer NOT NULL,
                    schema varchar(10) NOT NULL,
                    checksum bytea NOT NULL,
                    applicata_il timestamptz NOT NULL DEFAULT now(),
                    CONSTRAINT pk_migrazioni_applicate PRIMARY KEY (id),
                    CONSTRAINT uq_migrazioni_applicate UNIQUE (tipologia, versione, schema),
                    CONSTRAINT ck_migrazioni_applicate_versione CHECK (versione >= 1),
                    CONSTRAINT ck_migrazioni_applicate_schema CHECK (schema IN ('riviste', 'catalogo')),
                    CONSTRAINT ck_migrazioni_applicate_checksum CHECK (octet_length(checksum) = 32)
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lo schema tipologie appartiene alla Baseline e resta.
            migrationBuilder.Sql("DROP TABLE IF EXISTS tipologie.migrazioni_applicate;");
        }
    }
}
