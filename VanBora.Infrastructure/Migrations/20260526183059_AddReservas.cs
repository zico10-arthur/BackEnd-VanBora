using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanBora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viagem_van_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    taxa_plataforma = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    codigo_pix = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    transacao_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pago_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_viagem_vans_viagem_van_id",
                        column: x => x.viagem_van_id,
                        principalTable: "viagem_vans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "itens_reserva",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserva_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_assento = table.Column<int>(type: "integer", nullable: false),
                    preco_assento_valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    nome_passageiro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email_passageiro = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    telefone_passageiro_ddd = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    telefone_passageiro_valor = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    cpf_passageiro = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_reserva", x => x.id);
                    table.ForeignKey(
                        name: "FK_itens_reserva_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalTable: "reservas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_itens_reserva_reserva_id_assento",
                table: "itens_reserva",
                columns: new[] { "reserva_id", "numero_assento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservas_usuario_id",
                table: "reservas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_viagem_van_id",
                table: "reservas",
                column: "viagem_van_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_reserva");

            migrationBuilder.DropTable(
                name: "reservas");
        }
    }
}
