using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Data.Configurations;

public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("perfis");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(p => p.UsuarioId)
            .IsRequired()
            .HasColumnName("usuario_id");

        builder.Property(p => p.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("tipo");

        builder.Property(p => p.Ativo)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("ativo");

        builder.Property(p => p.CriadoPorPerfilId)
            .HasColumnName("criado_por_perfil_id");

        // Campos específicos do Gerente
        builder.Property(p => p.Slug)
            .HasMaxLength(100)
            .HasColumnName("slug");

        builder.Property(p => p.TaxaPlataforma)
            .HasPrecision(5, 2)
            .HasColumnName("taxa_plataforma");

        builder.Property(p => p.Gratuito)
            .HasColumnName("gratuito");

        // Campo específico do Motorista (Value Object)
        builder.OwnsOne(p => p.CNH, cnh =>
        {
            cnh.Property(c => c.Valor)
                .HasMaxLength(11)
                .HasColumnName("cnh");
        });

        builder.Property(p => p.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        // Índices
        builder.HasIndex(p => p.Slug)
            .HasDatabaseName("ix_perfis_slug")
            .IsUnique()
            .HasFilter("\"slug\" IS NOT NULL");

        builder.HasIndex(p => p.UsuarioId)
            .HasDatabaseName("ix_perfis_usuario_id");

        // Relacionamentos
        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Perfis)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CriadoPorPerfil)
            .WithMany(p => p.MotoristasCriados)
            .HasForeignKey(p => p.CriadoPorPerfilId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
