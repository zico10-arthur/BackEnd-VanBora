using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;
using VanBora.Domain.Enums;

namespace VanBora.Infrastructure.Data.Configurations;

public class ViagemConfiguration : IEntityTypeConfiguration<Viagem>
{
    public void Configure(EntityTypeBuilder<Viagem> builder)
    {
        builder.ToTable("viagens");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever().HasColumnName("id");
        builder.Property(v => v.GerenteUsuarioId).HasColumnName("gerente_usuario_id");
        builder.Property(v => v.NomeEvento).IsRequired().HasMaxLength(200).HasColumnName("nome_evento");
        builder.Property(v => v.DataEvento).HasColumnName("data_evento");
        builder.Property(v => v.LocalEvento).IsRequired().HasMaxLength(200).HasColumnName("local_evento");
        builder.Property(v => v.DataPartida).HasColumnName("data_partida");
        builder.Property(v => v.LocalPartida).IsRequired().HasMaxLength(200).HasColumnName("local_partida");
        builder.Property(v => v.PrecoAssento).HasPrecision(18, 2).HasColumnName("preco_assento");
        builder.Property(v => v.PossuiIngresso).HasColumnName("possui_ingresso");
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(20).HasColumnName("status");
        builder.Property(v => v.CriadoEm).HasColumnName("criado_em");

        builder.HasOne(v => v.GerenteUsuario)
            .WithMany()
            .HasForeignKey(v => v.GerenteUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.ViagemVans)
            .WithOne(vv => vv.Viagem)
            .HasForeignKey(vv => vv.ViagemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.ViagemVans)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_viagemVans");
    }
}
