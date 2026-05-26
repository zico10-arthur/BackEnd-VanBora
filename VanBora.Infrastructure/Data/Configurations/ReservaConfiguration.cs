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

        builder.Property(r => r.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(r => r.UsuarioId)
            .IsRequired()
            .HasColumnName("usuario_id");

        builder.Property(r => r.ViagemVanId)
            .IsRequired()
            .HasColumnName("viagem_van_id");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        builder.Property(r => r.ValorTotal)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasColumnName("valor_total");

        builder.Property(r => r.TaxaPlataforma)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasColumnName("taxa_plataforma");

        builder.Property(r => r.CodigoPix)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("codigo_pix");

        builder.Property(r => r.TransacaoId)
            .HasMaxLength(100)
            .HasColumnName("transacao_id");

        builder.Property(r => r.PagoEm)
            .HasColumnName("pago_em");

        builder.Property(r => r.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        builder.Property(r => r.ExpiraEm)
            .IsRequired()
            .HasColumnName("expira_em");

        // Relacionamento: Reserva → Usuario
        builder.HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento: Reserva → ViagemVan
        builder.HasOne(r => r.ViagemVan)
            .WithMany(vv => vv.Reservas)
            .HasForeignKey(r => r.ViagemVanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento: Reserva → Itens (cascade)
        builder.HasMany(r => r.Itens)
            .WithOne(i => i.Reserva)
            .HasForeignKey(i => i.ReservaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
