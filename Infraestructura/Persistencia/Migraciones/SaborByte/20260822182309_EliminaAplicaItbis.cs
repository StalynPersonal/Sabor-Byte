using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class EliminaAplicaItbis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AplicaItbis",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "AplicaItbis",
                schema: "facturacion",
                table: "FacturaDetalles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AplicaItbis",
                schema: "catalogo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaItbis",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
