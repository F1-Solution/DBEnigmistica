using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Infrastructure.Prototipo;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Data.Mappings;

/// <summary>Schema prototipo: candidati e chiamate AI di creazione (progetto 1b.1, sezione 4). Nessuna colonna per immagini.</summary>
internal static class PrototipoMapping
{
    private const string Schema = "prototipo";

    public static void ConfiguraPrototipo(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidato>(e =>
        {
            e.ToTable("candidati", Schema, t =>
            {
                t.HasCheckConstraint("ck_candidati_versione_tipologia", "versione_tipologia >= 0");
                t.HasCheckConstraint("ck_candidati_famiglia", "famiglia IN ('cruciverba', 'gioco-breve', 'crittografia', 'rebus', 'logico')");
                t.HasCheckConstraint("ck_candidati_ordine", "ordine_generazione >= 1");
                t.HasCheckConstraint("ck_candidati_modo", "modo IN ('ai', 'algoritmo')");
                t.HasCheckConstraint("ck_candidati_origine", "origine = 'originale'");
                t.HasCheckConstraint("ck_candidati_seme_algoritmo", "modo <> 'algoritmo' OR seme IS NOT NULL");
                t.HasCheckConstraint("ck_candidati_versione_generatore", "(seme IS NULL) = (versione_generatore IS NULL)");
                t.HasCheckConstraint("ck_candidati_versione_generatore_formato", "versione_generatore IS NULL OR versione_generatore ~ '^[a-z0-9][a-z0-9.-]{0,39}$'");
                t.HasCheckConstraint("ck_candidati_prompt_ai", "(modo = 'ai') = (versione_prompt IS NOT NULL)");
                t.HasCheckConstraint("ck_candidati_impronta_ai", "(modo = 'ai') = (impronta_richiesta IS NOT NULL)");
                t.HasCheckConstraint("ck_candidati_impronta_lunghezza", "impronta_richiesta IS NULL OR octet_length(impronta_richiesta) = 32");
                t.HasCheckConstraint(
                    "ck_candidati_stato",
                    "stato IN ('in_generazione', 'non_valido_schema', 'errore_generazione', 'da_rivedere', 'in_revisione', 'accettato', 'scartato')");
                t.HasCheckConstraint("ck_candidati_tempo_attivo", "tempo_attivo_ms >= 0");
                t.HasCheckConstraint("ck_candidati_motivo_scarto", "(stato = 'scartato') = (motivo_scarto IS NOT NULL)");
                t.HasCheckConstraint(
                    "ck_candidati_motivo_scarto_valori",
                    "motivo_scarto IS NULL OR motivo_scarto IN ('soluzione_errata', 'regole_non_rispettate', 'incroci_incoerenti', 'qualita_insufficiente', 'banale_o_troppo_facile', 'contenuto_inadatto', 'altro')");
                t.HasCheckConstraint(
                    "ck_candidati_contenuto_generato",
                    "(contenuto_generato IS NOT NULL) = (stato IN ('da_rivedere', 'in_revisione', 'accettato', 'scartato'))");
                t.HasCheckConstraint("ck_candidati_revisione_conclusa", "(revisione_conclusa_il IS NOT NULL) = (stato IN ('accettato', 'scartato'))");
                t.HasCheckConstraint("ck_candidati_accettato_finale", "stato <> 'accettato' OR contenuto_finale IS NOT NULL");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Tipologia).HasColumnName("tipologia").HasMaxLength(30).IsRequired();
            e.Property(x => x.VersioneTipologia).HasColumnName("versione_tipologia");
            e.Property(x => x.Famiglia).HasColumnName("famiglia").HasMaxLength(20).IsRequired();
            e.Property(x => x.OrdineGenerazione).HasColumnName("ordine_generazione");
            e.Property(x => x.GeneratoIl).HasColumnName("generato_il");
            e.Property(x => x.Modo).HasColumnName("modo").HasMaxLength(12).IsRequired();
            e.Property(x => x.Origine).HasColumnName("origine").HasMaxLength(12).IsRequired();
            e.Property(x => x.Brief).HasColumnName("brief").HasColumnType("jsonb").IsRequired();
            e.Property(x => x.Seme).HasColumnName("seme");
            e.Property(x => x.VersioneGeneratore).HasColumnName("versione_generatore").HasMaxLength(40);
            e.Property(x => x.SchemaCaselle).HasColumnName("schema_caselle").HasColumnType("jsonb");
            e.Property(x => x.VersionePrompt).HasColumnName("versione_prompt").HasMaxLength(40);
            e.Property(x => x.ImprontaRichiesta).HasColumnName("impronta_richiesta");
            e.Property(x => x.Stato).HasColumnName("stato").HasMaxLength(20).IsRequired();
            e.Property(x => x.Messaggio).HasColumnName("messaggio").HasMaxLength(2000);
            e.Property(x => x.ContenutoGenerato).HasColumnName("contenuto_generato").HasColumnType("jsonb");
            e.Property(x => x.ContenutoFinale).HasColumnName("contenuto_finale").HasColumnType("jsonb");
            e.Property(x => x.Differenze).HasColumnName("differenze").HasColumnType("jsonb");
            e.Property(x => x.TempoAttivoMs).HasColumnName("tempo_attivo_ms");
            e.Property(x => x.UltimaAttivitaIl).HasColumnName("ultima_attivita_il");
            e.Property(x => x.RevisioneIniziataIl).HasColumnName("revisione_iniziata_il");
            e.Property(x => x.RevisioneConclusaIl).HasColumnName("revisione_conclusa_il");
            e.Property(x => x.MotivoScarto).HasColumnName("motivo_scarto").HasMaxLength(30);
            e.Property(x => x.NotaScarto).HasColumnName("nota_scarto").HasMaxLength(1000);

            e.HasIndex(x => new { x.Tipologia, x.OrdineGenerazione }).IsUnique().HasDatabaseName("ux_candidati__tipologia_ordine");
            e.HasIndex(x => new { x.Tipologia, x.Stato, x.OrdineGenerazione }).HasDatabaseName("ix_candidati__tipologia_stato_ordine");
        });

        modelBuilder.Entity<ChiamataAiPrototipoRiga>(e =>
        {
            e.ToTable("chiamate_ai", Schema, t =>
            {
                t.HasCheckConstraint("ck_chiamate_ai_durata", "durata_ms >= 0");
                t.HasCheckConstraint("ck_chiamate_ai_uso", "uso = 'creazione'");
                t.HasCheckConstraint("ck_chiamate_ai_fornitore", "fornitore ~ '^[a-z][a-z0-9-]{0,39}$'");
                t.HasCheckConstraint("ck_chiamate_ai_impronta", "octet_length(impronta_richiesta) = 32");
                t.HasCheckConstraint("ck_chiamate_ai_tentativo", "tentativo >= 1");
                t.HasCheckConstraint(
                    "ck_chiamate_ai_esito",
                    "esito IN ('riuscita', 'risposta_non_valida', 'finestra_esaurita', 'errore_transitorio', 'errore_definitivo')");
                t.HasCheckConstraint(
                    "ck_chiamate_ai_token",
                    "(token_ingresso IS NULL OR token_ingresso >= 0) AND (token_uscita IS NULL OR token_uscita >= 0) "
                    + "AND (token_cache_creazione IS NULL OR token_cache_creazione >= 0) AND (token_cache_lettura IS NULL OR token_cache_lettura >= 0)");
                t.HasCheckConstraint("ck_chiamate_ai_costo", "costo IS NULL OR costo >= 0");
                t.HasCheckConstraint("ck_chiamate_ai_costo_valuta", "(costo IS NULL) = (costo_valuta IS NULL) AND (costo_valuta IS NULL OR costo_valuta ~ '^[A-Z]{3}$')");
                t.HasCheckConstraint("ck_chiamate_ai_costo_tipo", "(costo IS NULL) = (costo_tipo IS NULL) AND (costo_tipo IS NULL OR costo_tipo IN ('addebitato', 'indicativo'))");
                t.HasCheckConstraint("ck_chiamate_ai_risposta_sha256", "risposta_sha256 IS NULL OR octet_length(risposta_sha256) = 32");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.CandidatoId).HasColumnName("candidato_id");
            e.Property(x => x.AvviataIl).HasColumnName("avviata_il");
            e.Property(x => x.DurataMs).HasColumnName("durata_ms");
            e.Property(x => x.Uso).HasColumnName("uso").HasMaxLength(20).IsRequired();
            e.Property(x => x.Fornitore).HasColumnName("fornitore").HasMaxLength(40).IsRequired();
            e.Property(x => x.Modello).HasColumnName("modello").HasMaxLength(100).IsRequired();
            e.Property(x => x.VersionePrompt).HasColumnName("versione_prompt").HasMaxLength(40).IsRequired();
            e.Property(x => x.ImprontaRichiesta).HasColumnName("impronta_richiesta").IsRequired();
            e.Property(x => x.Tentativo).HasColumnName("tentativo");
            e.Property(x => x.Correzione).HasColumnName("correzione");
            e.Property(x => x.Esito).HasColumnName("esito").HasMaxLength(30).IsRequired();
            e.Property(x => x.RipresaIl).HasColumnName("ripresa_il");
            e.Property(x => x.CodiceUscita).HasColumnName("codice_uscita");
            e.Property(x => x.MessaggioErrore).HasColumnName("messaggio_errore").HasMaxLength(1000);
            e.Property(x => x.TokenIngresso).HasColumnName("token_ingresso");
            e.Property(x => x.TokenUscita).HasColumnName("token_uscita");
            e.Property(x => x.TokenCacheCreazione).HasColumnName("token_cache_creazione");
            e.Property(x => x.TokenCacheLettura).HasColumnName("token_cache_lettura");
            e.Property(x => x.Costo).HasColumnName("costo").HasColumnType("numeric(12,6)");
            e.Property(x => x.CostoValuta).HasColumnName("costo_valuta").HasMaxLength(3);
            e.Property(x => x.CostoTipo).HasColumnName("costo_tipo").HasMaxLength(12);
            e.Property(x => x.RispostaGrezza).HasColumnName("risposta_grezza").HasColumnType("text");
            e.Property(x => x.RispostaSha256).HasColumnName("risposta_sha256");

            e.HasOne<Candidato>().WithMany().HasForeignKey(x => x.CandidatoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CandidatoId).HasDatabaseName("ix_chiamate_ai__candidato_id");
            e.HasIndex(x => x.AvviataIl).HasDatabaseName("ix_chiamate_ai__avviata_il");
        });
    }
}
