using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanBora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialUsuariosPerfis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    senha_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    telefone_ddd = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    telefone_numero = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfis",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_por_perfil_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    taxa_plataforma = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    gratuito = table.Column<bool>(type: "boolean", nullable: true),
                    cnh = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfis", x => x.id);
                    table.ForeignKey(
                        name: "FK_perfis_perfis_criado_por_perfil_id",
                        column: x => x.criado_por_perfil_id,
                        principalTable: "perfis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_perfis_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_perfis_criado_por_perfil_id",
                table: "perfis",
                column: "criado_por_perfil_id");

            migrationBuilder.CreateIndex(
                name: "ix_perfis_slug",
                table: "perfis",
                column: "slug",
                unique: true,
                filter: "\"slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_perfis_usuario_id",
                table: "perfis",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_cpf",
                table: "usuarios",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true,
                filter: "\"email\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "perfis");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
