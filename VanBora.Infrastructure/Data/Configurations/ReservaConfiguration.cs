using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;

namespace VanBora.Infrastructure.Data.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("reservas");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(r => r.UsuarioId).HasColumnName("usuario_id");
        builder.Property(r => r.ViagemVanId).HasColumnName("viagem_van_id");
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(30).HasColumnName("status");
        builder.Property(r => r.ValorTotal).HasPrecision(18, 2).HasColumnName("valor_total");
        builder.Property(r => r.TaxaPlataforma).HasPrecision(18, 2).HasColumnName("taxa_plataforma");
        builder.Property(r => r.CodigoPix).IsRequired().HasMaxLength(500).HasColumnName("codigo_pix");
        builder.Property(r => r.TransacaoId).HasMaxLength(100).HasColumnName("transacao_id");
        builder.Property(r => r.PagoEm).HasColumnName("pago_em");
        builder.Property(r => r.CriadoEm).HasColumnName("criado_em");
        builder.Property(r => r.ExpiraEm).HasColumnName("expira_em");

        builder.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ViagemVan)
            .WithMany(vv => vv.Reservas)
            .HasForeignKey(r => r.ViagemVanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Itens)
            .WithOne(i => i.Reserva)
            .HasForeignKey(i => i.ReservaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_itens");

        builder.HasIndex(r => r.UsuarioId).HasDatabaseName("ix_reservas_usuario_id");
        builder.HasIndex(r => r.ViagemVanId).HasDatabaseName("ix_reservas_viagem_van_id");
        builder.HasIndex(r => new { r.Status, r.ExpiraEm }).HasDatabaseName("ix_reservas_status_expira");
    }
}
