using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class EliminaPropinaDescuentoEventoDeCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescripcionEvento",
                schema: "ventas",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "FechaEvento",
                schema: "ventas",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "MontoDescuento",
                schema: "ventas",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PorcentajePropina",
                schema: "ventas",
                table: "Cotizaciones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescripcionEvento",
                schema: "ventas",
                table: "Cotizaciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEvento",
                schema: "ventas",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDescuento",
                schema: "ventas",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajePropina",
                schema: "ventas",
                table: "Cotizaciones",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
