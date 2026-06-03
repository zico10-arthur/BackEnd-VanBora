using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Data.Configurations;

public class VanConfiguration : IEntityTypeConfiguration<Van>
{
    public void Configure(EntityTypeBuilder<Van> builder)
    {
        builder.ToTable("vans");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(v => v.GerenteUsuarioId).HasColumnName("gerente_usuario_id");
        builder.Property(v => v.Nome).IsRequired().HasMaxLength(100).HasColumnName("nome");
        builder.OwnsOne(v => v.Placa, placa =>
        {
            placa.Property(p => p.Valor).IsRequired().HasMaxLength(8).HasColumnName("placa");
        });
        builder.Property(v => v.Modelo).IsRequired().HasMaxLength(100).HasColumnName("modelo");
        builder.Property(v => v.Capacidade).HasColumnName("capacidade");
        builder.Property(v => v.Ativo).HasColumnName("ativo");
        builder.Property(v => v.CriadoEm).HasColumnName("criado_em");

        builder.HasOne(v => v.GerenteUsuario)
            .WithMany()
            .HasForeignKey(v => v.GerenteUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(v => v.ViagemVans)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_viagemVans");
    }
}
