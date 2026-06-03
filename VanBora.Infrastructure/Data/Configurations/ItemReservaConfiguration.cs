using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Data.Configurations;

public class ItemReservaConfiguration : IEntityTypeConfiguration<ItemReserva>
{
    public void Configure(EntityTypeBuilder<ItemReserva> builder)
    {
        builder.ToTable("item_reservas");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(i => i.ReservaId).HasColumnName("reserva_id");
        builder.Property(i => i.NumeroAssento).HasColumnName("numero_assento");
        builder.Property(i => i.NomePassageiro).IsRequired().HasMaxLength(200).HasColumnName("nome_passageiro");

        builder.OwnsOne(i => i.PrecoAssento, d =>
        {
            d.Property(x => x.Valor).HasPrecision(18, 2).HasColumnName("preco_assento");
            d.Property(x => x.Moeda).HasMaxLength(3).HasColumnName("preco_moeda");
        });

        builder.OwnsOne(i => i.EmailPassageiro, e =>
        {
            e.Property(x => x.Valor).HasMaxLength(254).HasColumnName("email_passageiro");
        });

        builder.OwnsOne(i => i.TelefonePassageiro, t =>
        {
            t.Property(x => x.DDD).HasMaxLength(2).HasColumnName("telefone_ddd");
            t.Property(x => x.Numero).HasMaxLength(9).HasColumnName("telefone_numero");
        });

        builder.OwnsOne(i => i.CPFPassageiro, c =>
        {
            c.Property(x => x.Valor).HasMaxLength(11).HasColumnName("cpf_passageiro");
        });

        builder.HasIndex(i => new { i.ReservaId, i.NumeroAssento })
            .IsUnique()
            .HasDatabaseName("ix_item_reservas_reserva_assento");
    }
}
