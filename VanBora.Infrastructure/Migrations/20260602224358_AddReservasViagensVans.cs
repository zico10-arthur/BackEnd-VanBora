using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanBora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservasViagensVans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gerente_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    capacidade = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vans", x => x.id);
                    table.ForeignKey(
                        name: "FK_vans_usuarios_gerente_usuario_id",
                        column: x => x.gerente_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "viagens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gerente_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_evento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    local_evento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_partida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    local_partida = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    preco_assento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    possui_ingresso = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viagens", x => x.id);
                    table.ForeignKey(
                        name: "FK_viagens_usuarios_gerente_usuario_id",
                        column: x => x.gerente_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "viagem_vans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    viagem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    van_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motorista_usuario_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viagem_vans", x => x.id);
                    table.ForeignKey(
                        name: "FK_viagem_vans_usuarios_motorista_usuario_id",
                        column: x => x.motorista_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_viagem_vans_vans_van_id",
                        column: x => x.van_id,
                        principalTable: "vans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_viagem_vans_viagens_viagem_id",
                        column: x => x.viagem_id,
                        principalTable: "viagens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viagem_van_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxa_plataforma = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    codigo_pix = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                name: "item_reservas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserva_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_assento = table.Column<int>(type: "integer", nullable: false),
                    preco_assento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_moeda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    nome_passageiro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email_passageiro = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    telefone_ddd = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    telefone_numero = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    cpf_passageiro = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_reservas", x => x.id);
                    table.ForeignKey(
                        name: "FK_item_reservas_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalTable: "reservas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_reservas_reserva_assento",
                table: "item_reservas",
                columns: new[] { "reserva_id", "numero_assento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reservas_status_expira",
                table: "reservas",
                columns: new[] { "status", "expira_em" });

            migrationBuilder.CreateIndex(
                name: "ix_reservas_usuario_id",
                table: "reservas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservas_viagem_van_id",
                table: "reservas",
                column: "viagem_van_id");

            migrationBuilder.CreateIndex(
                name: "IX_vans_gerente_usuario_id",
                table: "vans",
                column: "gerente_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_viagem_vans_motorista_usuario_id",
                table: "viagem_vans",
                column: "motorista_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_viagem_vans_van_id",
                table: "viagem_vans",
                column: "van_id");

            migrationBuilder.CreateIndex(
                name: "IX_viagem_vans_viagem_id",
                table: "viagem_vans",
                column: "viagem_id");

            migrationBuilder.CreateIndex(
                name: "IX_viagens_gerente_usuario_id",
                table: "viagens",
                column: "gerente_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_reservas");

            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "viagem_vans");

            migrationBuilder.DropTable(
                name: "vans");

            migrationBuilder.DropTable(
                name: "viagens");
        }
    }
}
