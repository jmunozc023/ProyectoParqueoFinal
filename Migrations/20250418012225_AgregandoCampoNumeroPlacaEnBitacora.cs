using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoParqueoFinal.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoCampoNumeroPlacaEnBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroPlaca",
                table: "Bitacoras",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroPlaca",
                table: "Bitacoras");
        }
    }
}
