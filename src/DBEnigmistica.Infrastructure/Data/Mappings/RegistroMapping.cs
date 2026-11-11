using DBEnigmistica.Domain.Registro;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Data.Mappings;

/// <summary>Schema registro: la sola tabella degli eventi, in sola aggiunta.</summary>
internal static class RegistroMapping
{
    public static void ConfiguraRegistro(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventoRegistro>(e =>
        {
            e.ToTable("eventi", "registro", t =>
            {
                t.HasCheckConstraint("ck_eventi_tipo", "tipo ~ '^[a-z][a-z0-9_]{0,59}$'");
                t.HasCheckConstraint("ck_eventi_entita", "entita ~ '^[a-z][a-z0-9_]{0,59}$'");
                t.HasCheckConstraint("ck_eventi_entita_id", "entita_id !~ '^[[:space:]]*$'");
                t.HasCheckConstraint("ck_eventi_autore", "autore !~ '^[[:space:]]*$'");
                t.HasCheckConstraint("ck_eventi_dati_json", "dati IS JSON OBJECT");
                t.HasCheckConstraint("ck_eventi_impronta_precedente", "octet_length(impronta_precedente) = 32");
                t.HasCheckConstraint("ck_eventi_impronta", "octet_length(impronta) = 32");
            });
            e.HasKey(x => x.Id);
            // L'id serve solo all'ordine di lettura e non entra nell'impronta.
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.AvvenutoIl).HasColumnName("avvenuto_il");
            e.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(60).IsRequired();
            e.Property(x => x.Entita).HasColumnName("entita").HasMaxLength(60).IsRequired();
            e.Property(x => x.EntitaId).HasColumnName("entita_id").HasMaxLength(100).IsRequired();
            e.Property(x => x.Autore).HasColumnName("autore").HasMaxLength(200).IsRequired();
            e.Property(x => x.Dati).HasColumnName("dati").HasColumnType("text").IsRequired();
            e.Property(x => x.ImprontaPrecedente).HasColumnName("impronta_precedente").IsRequired();
            e.Property(x => x.Impronta).HasColumnName("impronta").IsRequired();

            e.HasIndex(x => x.ImprontaPrecedente).IsUnique();
            e.HasIndex(x => x.Impronta).IsUnique();
        });
    }
}
