using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoParqueoFinal.Migrations
{
    /// <inheritdoc />
    public partial class Ley76002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UsaEspacio7600",
                table: "Bitacoras",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsaEspacio7600",
                table: "Bitacoras");
        }
    }
}
