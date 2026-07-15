using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanBora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoResetSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoResetSenha",
                table: "usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiracaoCodigoResetSenha",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoResetSenha",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "ExpiracaoCodigoResetSenha",
                table: "usuarios");
        }
    }
}
