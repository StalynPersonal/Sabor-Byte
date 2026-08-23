using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaInventariableAProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Inventariable",
                schema: "catalogo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Los Insumos ya existentes son inventariables por definición (ver
            // Producto.Inventariable) — sin esto, quedarían con Inventariable=false hasta
            // que alguien los reguarde desde Central, y el módulo de Inventario dejaría de
            // verlos. TipoProducto.Insumo = 0 en el enum.
            migrationBuilder.Sql("UPDATE [catalogo].[Productos] SET [Inventariable] = 1 WHERE [TipoProducto] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inventariable",
                schema: "catalogo",
                table: "Productos");
        }
    }
}
