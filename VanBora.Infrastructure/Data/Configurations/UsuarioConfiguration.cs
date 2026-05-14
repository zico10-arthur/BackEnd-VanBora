using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("nome");

        builder.OwnsOne(u => u.CPF, cpf =>
        {
            cpf.Property(c => c.Valor)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnName("cpf");

            cpf.HasIndex(c => c.Valor)
                .IsUnique()
                .HasDatabaseName("ix_usuarios_cpf");
        });

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasMaxLength(254)
                .HasColumnName("email");

            email.HasIndex(e => e.Valor)
                .IsUnique()
                .HasDatabaseName("ix_usuarios_email")
                .HasFilter("\"email\" IS NOT NULL");
        });

        builder.Property(u => u.SenhaHash)
            .HasMaxLength(255)
            .HasColumnName("senha_hash");

        builder.OwnsOne(u => u.Telefone, telefone =>
        {
            telefone.Property(t => t.DDD)
                .HasMaxLength(2)
                .HasColumnName("telefone_ddd");

            telefone.Property(t => t.Numero)
                .HasMaxLength(9)
                .HasColumnName("telefone_numero");
        });

        builder.Property(u => u.Ativo)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("ativo");

        builder.Property(u => u.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        // Relacionamento 1:N com Perfil
        builder.HasMany(u => u.Perfis)
            .WithOne(p => p.Usuario)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
