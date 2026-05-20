using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanBora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    senha_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    telefone_ddd = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    telefone_numero = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    taxa_plataforma = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    gratuito = table.Column<bool>(type: "boolean", nullable: true),
                    chave_pix = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cnh = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_usuarios_criado_por_usuario_id",
                        column: x => x.criado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    local_evento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    data_partida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    local_partida = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    preco_assento = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_cpf",
                table: "usuarios",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_criado_por_usuario_id",
                table: "usuarios",
                column: "criado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true,
                filter: "\"email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_slug",
                table: "usuarios",
                column: "slug",
                unique: true,
                filter: "\"slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_vans_gerente_usuario_id",
                table: "vans",
                column: "gerente_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_vans_placa",
                table: "vans",
                column: "placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_viagem_vans_motorista_usuario_id",
                table: "viagem_vans",
                column: "motorista_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_viagem_vans_van_id",
                table: "viagem_vans",
                column: "van_id");

            migrationBuilder.CreateIndex(
                name: "ix_viagem_vans_viagem_id_van_id",
                table: "viagem_vans",
                columns: new[] { "viagem_id", "van_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_viagens_gerente_usuario_id",
                table: "viagens",
                column: "gerente_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "viagem_vans");

            migrationBuilder.DropTable(
                name: "vans");

            migrationBuilder.DropTable(
                name: "viagens");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
