using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoParqueoFinal.Migrations
{
    /// <inheritdoc />
    public partial class Prueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaHora",
                table: "Bitacoras",
                newName: "FechaHoraSalida");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHoraEntrada",
                table: "Bitacoras",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaHoraEntrada",
                table: "Bitacoras");

            migrationBuilder.RenameColumn(
                name: "FechaHoraSalida",
                table: "Bitacoras",
                newName: "FechaHora");
        }
    }
}
