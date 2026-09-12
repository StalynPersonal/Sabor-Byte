using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class EliminaFormatoImpresionDeliveryDeSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormatoImpresionDelivery",
                schema: "sucursales",
                table: "Sucursales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormatoImpresionDelivery",
                schema: "sucursales",
                table: "Sucursales",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
