using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanBora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuorumMinimo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "quorum_minimo",
                table: "viagens",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quorum_minimo",
                table: "viagens");
        }
    }
}
