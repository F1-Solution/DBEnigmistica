using DBEnigmistica.Domain.Catalogo;
using DBEnigmistica.Domain.Diritti;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBEnigmistica.Infrastructure.Data.Mappings;

/// <summary>Schema catalogo: Soggetti, Giochi originali, licenze in entrata e Cessioni.</summary>
internal static class CatalogoMapping
{
    private const string Schema = "catalogo";

    public static void ConfiguraCatalogo(this ModelBuilder modelBuilder)
    {
        ConfiguraSoggetti(modelBuilder);
        ConfiguraGiochi(modelBuilder);
        ConfiguraLicenze(modelBuilder);
        ConfiguraCessioni(modelBuilder);
    }

    private static void ConfiguraSoggetti(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Soggetto>(e =>
        {
            e.ToTable("soggetti", Schema, t =>
            {
                t.HasCheckConstraint("ck_soggetti_tipo", "tipo IN ('persona','organizzazione')");
                t.HasCheckConstraint("ck_soggetti_denominazione", "btrim(denominazione) <> ''");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Tipo).HasColumnName("tipo").HasConversion(Convertitori.TipoTesto).IsRequired();
            e.Property(x => x.Denominazione).HasColumnName("denominazione").HasMaxLength(200).IsRequired();
            e.Property(x => x.IdentificativoFiscale).HasColumnName("identificativo_fiscale").HasMaxLength(32);
            e.Property(x => x.Note).HasColumnName("note");
            e.Property(x => x.CreatoIl).HasColumnName("creato_il");
        });
    }

    private static void ConfiguraGiochi(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GiocoOriginale>(e =>
        {
            e.ToTable("giochi", Schema, ConfiguraVincoliGiochi);
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Tipologia).HasColumnName("tipologia").HasMaxLength(61).IsRequired();
            e.Property(x => x.VersioneTipologia).HasColumnName("versione_tipologia");
            e.Property(x => x.Titolo).HasColumnName("titolo").HasMaxLength(200);
            // Origine senza default: la dichiara sempre chi crea il Gioco.
            e.Property(x => x.Origine).HasColumnName("origine").HasConversion(Convertitori.OrigineTesto).IsRequired();
            e.Property(x => x.TitolareId).HasColumnName("titolare_id");
            e.Property(x => x.CreatoIl).HasColumnName("creato_il");

            e.Property<string?>("_provaAutore").HasColumnName("pd_autore").HasMaxLength(200);
            e.Property<int?>("_provaAnno").HasColumnName("pd_anno");
            e.Property<string?>("_provaMotivo").HasColumnName("pd_motivo").HasMaxLength(1000);
            e.Property<string>("_statoInedito").HasColumnName("stato_inedito").IsRequired();
            e.Property<DateTime?>("_primaPubblicazioneIl").HasColumnName("prima_pubblicazione_il");
            e.Property<string?>("_primaPubblicazioneLuogo").HasColumnName("prima_pubblicazione_luogo").HasMaxLength(300);

            e.Ignore(x => x.ProvaPubblicoDominio);
            e.Ignore(x => x.StatoInedito);

            e.HasOne<Soggetto>().WithMany().HasForeignKey(x => x.TitolareId).OnDelete(DeleteBehavior.NoAction);
            e.HasMany(x => x.Licenze).WithOne().HasForeignKey(l => l.GiocoId).OnDelete(DeleteBehavior.Cascade);
            // Un Gioco ceduto non si cancella.
            e.HasMany(x => x.Cessioni).WithOne().HasForeignKey(c => c.GiocoId).OnDelete(DeleteBehavior.NoAction);
            e.Navigation(x => x.Licenze).UsePropertyAccessMode(PropertyAccessMode.Field);
            e.Navigation(x => x.Cessioni).UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasIndex(x => x.Tipologia);
            e.HasIndex(x => x.TitolareId);
        });
    }

    private static void ConfiguraVincoliGiochi(TableBuilder<GiocoOriginale> t)
    {
        t.HasCheckConstraint("ck_giochi_origine", "origine IN ('originale','licenza','pubblico_dominio')");
        t.HasCheckConstraint("ck_giochi_tipologia", "tipologia ~ '^[a-z][a-z0-9-]{1,60}$'");
        t.HasCheckConstraint("ck_giochi_versione_tipologia", "versione_tipologia > 0");
        t.HasCheckConstraint("ck_giochi_stato_inedito_valori", "stato_inedito IN ('inedito','pubblicato')");
        t.HasCheckConstraint("ck_giochi_pd_tutti_o_nessuno",
            "(pd_autore IS NULL) = (pd_anno IS NULL) AND (pd_anno IS NULL) = (pd_motivo IS NULL)");
        t.HasCheckConstraint("ck_giochi_pd_solo_pubblico_dominio",
            "pd_autore IS NULL OR origine = 'pubblico_dominio'");
        t.HasCheckConstraint("ck_giochi_stato_inedito",
            "(stato_inedito = 'inedito' AND prima_pubblicazione_il IS NULL AND prima_pubblicazione_luogo IS NULL) " +
            "OR (stato_inedito = 'pubblicato' AND prima_pubblicazione_il IS NOT NULL AND prima_pubblicazione_luogo IS NOT NULL)");
    }

    private static void ConfiguraLicenze(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LicenzaInEntrata>(e =>
        {
            e.ToTable("licenze_in_entrata", Schema, t =>
            {
                t.HasCheckConstraint("ck_licenze_estremi", "btrim(estremi_accordo) <> ''");
                t.HasCheckConstraint("ck_licenze_fine", "fine IS NULL OR fine > inizio");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.GiocoId).HasColumnName("gioco_id");
            e.Property(x => x.LicenzianteId).HasColumnName("licenziante_id");
            e.Property(x => x.EstremiAccordo).HasColumnName("estremi_accordo").HasMaxLength(500).IsRequired();
            e.ComplexProperty(x => x.Periodo, MappaPeriodo);
            e.HasOne<Soggetto>().WithMany().HasForeignKey(x => x.LicenzianteId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.GiocoId);
        });
    }

    private static void ConfiguraCessioni(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cessione>(e =>
        {
            e.ToTable("cessioni", Schema, t =>
            {
                t.HasCheckConstraint("ck_cessioni_fine", "fine IS NULL OR fine > inizio");
                t.HasCheckConstraint("ck_cessioni_territorio", "territorio ~ '^([A-Z]{2}|MONDO)$'");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.GiocoId).HasColumnName("gioco_id");
            e.Property(x => x.ClienteId).HasColumnName("cliente_id");
            e.Property(x => x.StipulataIl).HasColumnName("stipulata_il");
            e.Property(x => x.Esclusiva).HasColumnName("esclusiva");
            e.Property(x => x.ComeInedito).HasColumnName("come_inedito");
            e.Property(x => x.Territorio).HasColumnName("territorio").HasMaxLength(5)
                .HasConversion(Convertitori.TerritorioTesto).IsRequired();
            e.ComplexProperty(x => x.Periodo, MappaPeriodo);
            e.HasOne<Soggetto>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => x.GiocoId);
        });
    }

    private static void MappaPeriodo(ComplexPropertyBuilder<Periodo> b)
    {
        b.Property(p => p.Inizio).HasColumnName("inizio");
        b.Property(p => p.Fine).HasColumnName("fine");
    }
}
