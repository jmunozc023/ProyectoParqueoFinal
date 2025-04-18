using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoParqueoFinal.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bitacoras_Vehiculos_VehiculosIdVehiculo",
                table: "Bitacoras");

            migrationBuilder.AlterColumn<int>(
                name: "VehiculosIdVehiculo",
                table: "Bitacoras",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Bitacoras_Vehiculos_VehiculosIdVehiculo",
                table: "Bitacoras",
                column: "VehiculosIdVehiculo",
                principalTable: "Vehiculos",
                principalColumn: "IdVehiculo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bitacoras_Vehiculos_VehiculosIdVehiculo",
                table: "Bitacoras");

            migrationBuilder.AlterColumn<int>(
                name: "VehiculosIdVehiculo",
                table: "Bitacoras",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bitacoras_Vehiculos_VehiculosIdVehiculo",
                table: "Bitacoras",
                column: "VehiculosIdVehiculo",
                principalTable: "Vehiculos",
                principalColumn: "IdVehiculo",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
