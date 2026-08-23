using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class EliminaRncDeSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rnc",
                schema: "sucursales",
                table: "Sucursales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rnc",
                schema: "sucursales",
                table: "Sucursales",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
