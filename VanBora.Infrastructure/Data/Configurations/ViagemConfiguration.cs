using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VanBora.Domain.Entities;

namespace VanBora.Infrastructure.Data.Configurations;

public class ViagemConfiguration : IEntityTypeConfiguration<Viagem>
{
    public void Configure(EntityTypeBuilder<Viagem> builder)
    {
        builder.ToTable("viagens");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(v => v.GerenteUsuarioId)
            .IsRequired()
            .HasColumnName("gerente_usuario_id");

        builder.Property(v => v.NomeEvento)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("nome_evento");

        builder.Property(v => v.DataEvento)
            .IsRequired()
            .HasColumnName("data_evento");

        builder.Property(v => v.LocalEvento)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("local_evento");

        builder.Property(v => v.DataPartida)
            .IsRequired()
            .HasColumnName("data_partida");

        builder.Property(v => v.LocalPartida)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnName("local_partida");

        builder.Property(v => v.PrecoAssento)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasColumnName("preco_assento");

        builder.Property(v => v.PossuiIngresso)
            .IsRequired()
            .HasColumnName("possui_ingresso");

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        builder.Property(v => v.CriadoEm)
            .IsRequired()
            .HasColumnName("criado_em");

        // Relacionamento: Viagem → Usuario (Gerente)
        // Sem navigation property reversa em Usuario
        builder.HasOne(v => v.GerenteUsuario)
            .WithMany()
            .HasForeignKey(v => v.GerenteUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento: Viagem → ViagemVan (1:N)
        builder.HasMany(v => v.ViagemVans)
            .WithOne(vv => vv.Viagem)
            .HasForeignKey(vv => vv.ViagemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
