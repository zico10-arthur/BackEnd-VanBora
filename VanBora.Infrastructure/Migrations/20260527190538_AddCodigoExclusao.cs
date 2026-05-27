using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanBora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoExclusao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_exclusao",
                table: "usuarios",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "codigo_exclusao_expira_em",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigo_exclusao",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "codigo_exclusao_expira_em",
                table: "usuarios");
        }
    }
}
