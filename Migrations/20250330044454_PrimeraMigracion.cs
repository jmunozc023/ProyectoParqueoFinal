using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoParqueoFinal.Migrations
{
    /// <inheritdoc />
    public partial class PrimeraMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parqueos",
                columns: table => new
                {
                    IdParqueo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreParqueo = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CapacidadAutomoviles = table.Column<int>(type: "int", nullable: false),
                    CapacidadMotocicletas = table.Column<int>(type: "int", nullable: false),
                    CapacidadLey7600 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parqueos", x => x.IdParqueo);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    NumeroCarne = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RequiereCambioPassword = table.Column<bool>(type: "bit", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    IdVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Marca = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NumeroPlaca = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    TipoVehiculo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UsaEspacio7600 = table.Column<bool>(type: "bit", nullable: false),
                    UsuariosIdUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.IdVehiculo);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Usuarios_UsuariosIdUsuario",
                        column: x => x.UsuariosIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bitacoras",
                columns: table => new
                {
                    IdBitacora = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoIngreso = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehiculosIdVehiculo = table.Column<int>(type: "int", nullable: false),
                    ParqueoIdParqueo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bitacoras", x => x.IdBitacora);
                    table.ForeignKey(
                        name: "FK_Bitacoras_Parqueos_ParqueoIdParqueo",
                        column: x => x.ParqueoIdParqueo,
                        principalTable: "Parqueos",
                        principalColumn: "IdParqueo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bitacoras_Vehiculos_VehiculosIdVehiculo",
                        column: x => x.VehiculosIdVehiculo,
                        principalTable: "Vehiculos",
                        principalColumn: "IdVehiculo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacoras_ParqueoIdParqueo",
                table: "Bitacoras",
                column: "ParqueoIdParqueo");

            migrationBuilder.CreateIndex(
                name: "IX_Bitacoras_VehiculosIdVehiculo",
                table: "Bitacoras",
                column: "VehiculosIdVehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_UsuariosIdUsuario",
                table: "Vehiculos",
                column: "UsuariosIdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bitacoras");

            migrationBuilder.DropTable(
                name: "Parqueos");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
