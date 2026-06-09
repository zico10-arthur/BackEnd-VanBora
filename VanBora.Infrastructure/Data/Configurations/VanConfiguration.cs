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

        builder.Property(v => v.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(v => v.GerenteUsuarioId)
            .IsRequired()
            .HasColumnName("gerente_usuario_id");

        builder.Property(v => v.Nome)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("nome");

        builder.OwnsOne(v => v.Placa, placa =>
        {
            placa.Property(p => p.Valor)
                .IsRequired()
                .HasMaxLength(8)
                .HasColumnName("placa");

            placa.HasIndex(p => p.Valor)
                .IsUnique()
                .HasDatabaseName("ix_vans_placa");
        });

        builder.Property(v => v.Modelo)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("modelo");

        builder.Property(v => v.Capacidade)
            .IsRequired()
            .HasColumnName("capacidade");

        builder.Property(v => v.Ativo)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("ativo");

        builder.Property(v => v.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        // Relacionamento: Van → Usuario (Gerente)
        // Sem navigation property reversa em Usuario (DDD — aggregate não conhece os filhos)
        builder.HasOne(v => v.GerenteUsuario)
            .WithMany()
            .HasForeignKey(v => v.GerenteUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
