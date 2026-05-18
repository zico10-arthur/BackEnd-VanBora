using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;

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

        builder.Property(u => u.DataAtualizacao)
            .HasColumnName("data_atualizacao");

        // Tipo de usuário (Passageiro, Gerente, Motorista, Admin)
        builder.Property(u => u.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("tipo");

        // Campos específicos do Gerente
        builder.Property(u => u.Slug)
            .HasMaxLength(100)
            .HasColumnName("slug");

        builder.Property(u => u.TaxaPlataforma)
            .HasPrecision(5, 2)
            .HasColumnName("taxa_plataforma");

        builder.Property(u => u.Gratuito)
            .HasColumnName("gratuito");

        builder.Property(u => u.ChavePix)
            .HasMaxLength(100)
            .HasColumnName("chave_pix");

        // CNH (Value Object, específico do Motorista)
        builder.OwnsOne(u => u.CNH, cnh =>
        {
            cnh.Property(c => c.Valor)
                .HasMaxLength(11)
                .HasColumnName("cnh");
        });

        // Auto-relacionamento: Gerente que criou o Motorista
        builder.Property(u => u.CriadoPorUsuarioId)
            .HasColumnName("criado_por_usuario_id");

        builder.HasOne(u => u.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(u => u.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(u => u.Slug)
            .HasDatabaseName("ix_usuarios_slug")
            .IsUnique()
            .HasFilter("\"slug\" IS NOT NULL");
    }
}
