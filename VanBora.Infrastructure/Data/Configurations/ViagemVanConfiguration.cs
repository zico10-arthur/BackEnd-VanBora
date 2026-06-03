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
        builder.Property(vv => vv.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(vv => vv.ViagemId).HasColumnName("viagem_id");
        builder.Property(vv => vv.VanId).HasColumnName("van_id");
        builder.Property(vv => vv.MotoristaUsuarioId).HasColumnName("motorista_usuario_id");

        builder.HasOne(vv => vv.Van)
            .WithMany(v => v.ViagemVans)
            .HasForeignKey(vv => vv.VanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(vv => vv.MotoristaUsuario)
            .WithMany()
            .HasForeignKey(vv => vv.MotoristaUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(vv => vv.Reservas)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_reservas");
    }
}
