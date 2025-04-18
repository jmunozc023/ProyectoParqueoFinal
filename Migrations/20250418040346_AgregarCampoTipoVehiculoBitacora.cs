using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoParqueoFinal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoTipoVehiculoBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoVehiculo",
                table: "Bitacoras",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoVehiculo",
                table: "Bitacoras");
        }
    }
}
