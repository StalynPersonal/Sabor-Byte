using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarTasaItbisYRespuestaDgii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TasaItbis",
                schema: "catalogo",
                table: "Productos",
                type: "decimal(5,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvioDgii",
                schema: "facturacion",
                table: "Facturas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRespuestaDgii",
                schema: "facturacion",
                table: "Facturas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MensajeDgii",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TasaItbis",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "decimal(5,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TasaItbis",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaEnvioDgii",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "FechaRespuestaDgii",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "MensajeDgii",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TasaItbis",
                schema: "facturacion",
                table: "FacturaDetalles");
        }
    }
}
