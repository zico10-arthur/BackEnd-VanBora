using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Data.Configurations;

public class ItemReservaConfiguration : IEntityTypeConfiguration<ItemReserva>
{
    public void Configure(EntityTypeBuilder<ItemReserva> builder)
    {
        builder.ToTable("itens_reserva");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(i => i.ReservaId)
            .IsRequired()
            .HasColumnName("reserva_id");

        builder.Property(i => i.NumeroAssento)
            .IsRequired()
            .HasColumnName("numero_assento");

        builder.Property(i => i.NomePassageiro)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("nome_passageiro");

        // --- PrecoAssento (Dinheiro - Value Object) ---
        builder.OwnsOne(i => i.PrecoAssento, dinheiro =>
        {
            dinheiro.Property(d => d.Valor)
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasColumnName("preco_assento_valor");
        });

        // --- EmailPassageiro (Value Object) ---
        builder.OwnsOne(i => i.EmailPassageiro, email =>
        {
            email.Property(e => e.Valor)
                .IsRequired()
                .HasMaxLength(254)
                .HasColumnName("email_passageiro");
        });

        // --- TelefonePassageiro (Value Object) ---
        builder.OwnsOne(i => i.TelefonePassageiro, tel =>
        {
            tel.Property(t => t.DDD)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnName("telefone_passageiro_ddd");

            tel.Property(t => t.Numero)
                .IsRequired()
                .HasMaxLength(9)
                .HasColumnName("telefone_passageiro_valor");
        });

        // --- CPFPassageiro (Value Object) ---
        builder.OwnsOne(i => i.CPFPassageiro, cpf =>
        {
            cpf.Property(c => c.Valor)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnName("cpf_passageiro");
        });

        // Índice único: mesmo assento não pode ser duplicado na mesma reserva
        builder.HasIndex(i => new { i.ReservaId, i.NumeroAssento })
            .IsUnique()
            .HasDatabaseName("ix_itens_reserva_reserva_id_assento");
    }
}
