using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarDatosDgiiAFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoSeguridadDgii",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackIdDgii",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlFirmadoDgii",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoSeguridadDgii",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TrackIdDgii",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "XmlFirmadoDgii",
                schema: "facturacion",
                table: "Facturas");
        }
    }
}
