using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Data.Configurations;

public class ViagemVanConfiguration : IEntityTypeConfiguration<ViagemVan>
{
    public void Configure(EntityTypeBuilder<ViagemVan> builder)
    {
        builder.ToTable("viagem_vans");

        builder.HasKey(vv => vv.Id);

        builder.Property(vv => vv.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(vv => vv.ViagemId)
            .IsRequired()
            .HasColumnName("viagem_id");

        builder.Property(vv => vv.VanId)
            .IsRequired()
            .HasColumnName("van_id");

        builder.Property(vv => vv.MotoristaUsuarioId)
            .HasColumnName("motorista_usuario_id");

        // Relacionamento: ViagemVan → Viagem
        builder.HasOne(vv => vv.Viagem)
            .WithMany(v => v.ViagemVans)
            .HasForeignKey(vv => vv.ViagemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento: ViagemVan → Van
        builder.HasOne(vv => vv.Van)
            .WithMany(v => v.ViagemVans)
            .HasForeignKey(vv => vv.VanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento: ViagemVan → Usuario (Motorista)
        // Sem navigation property reversa em Usuario
        builder.HasOne(vv => vv.MotoristaUsuario)
            .WithMany()
            .HasForeignKey(vv => vv.MotoristaUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        // Index único: uma van não pode estar duplicada na mesma viagem
        builder.HasIndex(vv => new { vv.ViagemId, vv.VanId })
            .IsUnique()
            .HasDatabaseName("ix_viagem_vans_viagem_id_van_id");
    }
}
