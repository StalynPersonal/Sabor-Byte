using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaIpOrigenAFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpOrigen",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpOrigen",
                schema: "facturacion",
                table: "Facturas");
        }
    }
}
