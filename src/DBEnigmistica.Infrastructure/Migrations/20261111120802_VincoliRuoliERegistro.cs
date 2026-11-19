using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBEnigmistica.Infrastructure.Migrations
{
    /// <summary>SQL che EF non esprime: colonne generate, esclusione, trigger, ruoli e permessi (scritta a mano).</summary>
    [DbContext(typeof(EnigmisticaDbContext))]
    [Migration("20261111120802_VincoliRuoliERegistro")]
    public partial class VincoliRuoliERegistro : Migration
    {
        private static readonly string[] Ruoli = ["dbe_estrazione", "dbe_creazione", "dbe_originalita", "dbe_consegna"];

        private static readonly string[] Schemi = ["riviste", "catalogo", "tipologie", "registro"];

        private static readonly (string Tabella, string Nome)[] VincoliPaginaSoluzione =
        [
            ("games", "FK_games_pages_SolutionPageId"),
            ("game_solutions", "FK_game_solutions_pages_SolutionPageId")
        ];

        private const string Lettura = "SELECT";
        private const string Scrittura = "SELECT, INSERT, UPDATE, DELETE";
        private const string Aggiunta = "SELECT, INSERT";

        // Ruolo, schema, privilegi sulle tabelle; chi può inserire usa anche le sequenze delle identità.
        private static readonly (string Ruolo, string Schema, string Privilegi)[] Permessi =
        [
            ("dbe_estrazione", "riviste", Scrittura),
            ("dbe_estrazione", "tipologie", Lettura),
            ("dbe_estrazione", "registro", Aggiunta),
            ("dbe_creazione", "catalogo", Scrittura),
            ("dbe_creazione", "tipologie", Lettura),
            ("dbe_creazione", "registro", Aggiunta),
            ("dbe_originalita", "riviste", Lettura),
            ("dbe_originalita", "catalogo", Lettura),
            ("dbe_originalita", "tipologie", Lettura),
            ("dbe_consegna", "catalogo", Lettura),
            ("dbe_consegna", "tipologie", Lettura),
            ("dbe_consegna", "registro", Aggiunta)
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PagineSoluzioneControllateAlCommit(migrationBuilder);
            CessioniColonneGenerateEEsclusione(migrationBuilder);
            TriggerIneditoSenzaRitorno(migrationBuilder);
            TriggerRegistroInSolaAggiunta(migrationBuilder);
            RuoliEPermessi(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // I ruoli valgono per tutto il cluster e non si eliminano.
            foreach (var (ruolo, schema, _) in Permessi)
            {
                migrationBuilder.Sql($"ALTER DEFAULT PRIVILEGES IN SCHEMA {schema} REVOKE ALL ON TABLES FROM {ruolo};");
                migrationBuilder.Sql($"ALTER DEFAULT PRIVILEGES IN SCHEMA {schema} REVOKE ALL ON SEQUENCES FROM {ruolo};");
                migrationBuilder.Sql($"REVOKE ALL ON ALL TABLES IN SCHEMA {schema} FROM {ruolo};");
                migrationBuilder.Sql($"REVOKE ALL ON ALL SEQUENCES IN SCHEMA {schema} FROM {ruolo};");
                migrationBuilder.Sql($"REVOKE ALL ON SCHEMA {schema} FROM {ruolo};");
            }

            foreach (var vincolo in VincoliPaginaSoluzione)
            {
                migrationBuilder.Sql($"ALTER TABLE riviste.{vincolo.Tabella} ALTER CONSTRAINT \"{vincolo.Nome}\" NOT DEFERRABLE;");
            }

            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_eventi_catena ON registro.eventi;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS registro.verifica_aggiunta();");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_eventi_no_truncate ON registro.eventi;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_eventi_no_delete ON registro.eventi;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_eventi_no_update ON registro.eventi;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS registro.sola_aggiunta();");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tr_giochi_inedito_senza_ritorno ON catalogo.giochi;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS catalogo.inedito_senza_ritorno();");
            migrationBuilder.Sql("ALTER TABLE catalogo.cessioni DROP CONSTRAINT IF EXISTS ex_cessioni_esclusive_sovrapposte;");
            migrationBuilder.Sql("ALTER TABLE catalogo.cessioni DROP COLUMN IF EXISTS territorio_intervallo, DROP COLUMN IF EXISTS periodo;");
        }

        private static void PagineSoluzioneControllateAlCommit(MigrationBuilder migrationBuilder)
        {
            // Resta NO ACTION, ma il controllo avviene al commit: la cascata di una Rivista cancella prima i riferimenti interni.
            foreach (var vincolo in VincoliPaginaSoluzione)
            {
                migrationBuilder.Sql($"ALTER TABLE riviste.{vincolo.Tabella} ALTER CONSTRAINT \"{vincolo.Nome}\" DEFERRABLE INITIALLY DEFERRED;");
            }
        }

        private static void CessioniColonneGenerateEEsclusione(MigrationBuilder migrationBuilder)
        {
            // MONDO copre tutti i 676 codici di due lettere; un paese è l'intervallo di un solo codice.
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.cessioni
                  ADD COLUMN periodo tstzrange
                    GENERATED ALWAYS AS (tstzrange(inizio, fine, '[)')) STORED,
                  ADD COLUMN territorio_intervallo int4range
                    GENERATED ALWAYS AS (
                      CASE WHEN territorio = 'MONDO' THEN int4range(0, 676)
                      ELSE int4range(
                        (ascii(substr(territorio, 1, 1)) - 65) * 26 + ascii(substr(territorio, 2, 1)) - 65,
                        (ascii(substr(territorio, 1, 1)) - 65) * 26 + ascii(substr(territorio, 2, 1)) - 64)
                      END) STORED;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE catalogo.cessioni ADD CONSTRAINT ex_cessioni_esclusive_sovrapposte
                  EXCLUDE USING gist (gioco_id WITH =, periodo WITH &&, territorio_intervallo WITH &&)
                  WHERE (esclusiva);
                """);
        }

        private static void TriggerIneditoSenzaRitorno(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION catalogo.inedito_senza_ritorno() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                  IF OLD.stato_inedito = 'pubblicato' AND (
                       NEW.stato_inedito <> 'pubblicato'
                       OR NEW.prima_pubblicazione_il IS DISTINCT FROM OLD.prima_pubblicazione_il
                       OR NEW.prima_pubblicazione_luogo IS DISTINCT FROM OLD.prima_pubblicazione_luogo) THEN
                    RAISE EXCEPTION 'Il Gioco è già stato pubblicato: lo stato di inedito non torna indietro'
                      USING ERRCODE = 'check_violation';
                  END IF;
                  RETURN NEW;
                END
                $$;
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER tr_giochi_inedito_senza_ritorno BEFORE UPDATE ON catalogo.giochi
                  FOR EACH ROW EXECUTE FUNCTION catalogo.inedito_senza_ritorno();
                """);
        }

        private static void TriggerRegistroInSolaAggiunta(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION registro.sola_aggiunta() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                  RAISE EXCEPTION 'registro.eventi è in sola aggiunta' USING ERRCODE = 'insufficient_privilege';
                END
                $$;
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER tr_eventi_no_update BEFORE UPDATE ON registro.eventi
                  FOR EACH ROW EXECUTE FUNCTION registro.sola_aggiunta();
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER tr_eventi_no_delete BEFORE DELETE ON registro.eventi
                  FOR EACH ROW EXECUTE FUNCTION registro.sola_aggiunta();
                """);
            // Stesso lucchetto del repository (4419001, rientrante): anche un INSERT diretto deve continuare la catena.
            migrationBuilder.Sql("""
                CREATE FUNCTION registro.verifica_aggiunta() RETURNS trigger LANGUAGE plpgsql AS $$
                DECLARE
                  ultima bytea;
                  ultimo_id bigint;
                  id_generato bigint;
                BEGIN
                  PERFORM pg_advisory_xact_lock(4419001);
                  SELECT impronta, id INTO ultima, ultimo_id FROM registro.eventi ORDER BY id DESC LIMIT 1;
                  IF NOT FOUND THEN
                    ultima := decode(repeat('00', 32), 'hex');
                    ultimo_id := 0;
                  END IF;

                  BEGIN
                    id_generato := currval(pg_get_serial_sequence('registro.eventi', 'id'));
                  EXCEPTION WHEN object_not_in_prerequisite_state THEN
                    id_generato := NULL;
                  END;

                  IF NEW.id IS DISTINCT FROM id_generato OR NEW.id <= ultimo_id THEN
                    RAISE EXCEPTION 'registro.eventi: l''id lo assegna il database, in ordine crescente'
                      USING ERRCODE = 'check_violation';
                  END IF;
                  IF NEW.impronta_precedente IS DISTINCT FROM ultima THEN
                    RAISE EXCEPTION 'registro.eventi: l''impronta precedente non è quella dell''ultima riga'
                      USING ERRCODE = 'check_violation';
                  END IF;
                  RETURN NEW;
                END
                $$;
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER tr_eventi_catena BEFORE INSERT ON registro.eventi
                  FOR EACH ROW EXECUTE FUNCTION registro.verifica_aggiunta();
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER tr_eventi_no_truncate BEFORE TRUNCATE ON registro.eventi
                  FOR EACH STATEMENT EXECUTE FUNCTION registro.sola_aggiunta();
                """);
        }

        private static void RuoliEPermessi(MigrationBuilder migrationBuilder)
        {
            foreach (var ruolo in Ruoli)
            {
                migrationBuilder.Sql($$"""
                    DO $$
                    BEGIN
                      IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '{{ruolo}}') THEN
                        CREATE ROLE {{ruolo}} NOLOGIN;
                      END IF;
                    END
                    $$;
                    """);
                // Un ruolo già esistente (il cluster è condiviso) viene riportato ai privilegi minimi.
                migrationBuilder.Sql($"ALTER ROLE {ruolo} NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;");
            }

            migrationBuilder.Sql($"REVOKE ALL ON SCHEMA {string.Join(", ", Schemi)} FROM PUBLIC;");

            foreach (var (ruolo, schema, privilegi) in Permessi)
            {
                migrationBuilder.Sql($"GRANT USAGE ON SCHEMA {schema} TO {ruolo};");
                migrationBuilder.Sql($"GRANT {privilegi} ON ALL TABLES IN SCHEMA {schema} TO {ruolo};");
                migrationBuilder.Sql($"ALTER DEFAULT PRIVILEGES IN SCHEMA {schema} GRANT {privilegi} ON TABLES TO {ruolo};");

                if (privilegi != Lettura)
                {
                    migrationBuilder.Sql($"GRANT USAGE ON ALL SEQUENCES IN SCHEMA {schema} TO {ruolo};");
                    migrationBuilder.Sql($"ALTER DEFAULT PRIVILEGES IN SCHEMA {schema} GRANT USAGE ON SEQUENCES TO {ruolo};");
                }
            }

            // Il registro non si modifica da nessun ruolo, nemmeno con i privilegi predefiniti.
            migrationBuilder.Sql("REVOKE UPDATE, DELETE, TRUNCATE ON registro.eventi FROM PUBLIC;");
            foreach (var ruolo in Ruoli)
            {
                migrationBuilder.Sql($"REVOKE UPDATE, DELETE, TRUNCATE ON registro.eventi FROM {ruolo};");
            }
        }
    }
}
