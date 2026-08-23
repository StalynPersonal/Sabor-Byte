using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class ProductoGlobalStockPorSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Crear StockSucursal ANTES de tocar Productos, para poder respaldar
            //    el stock que hoy vive en Productos.StockActual/Minimo/Maximo.
            migrationBuilder.CreateTable(
                name: "StockSucursal",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockActual = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    StockMaximo = table.Column<decimal>(type: "decimal(18,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSucursal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockSucursal_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "catalogo",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2) Backfill: una fila de StockSucursal por cada insumo existente, con el
            //    stock que tenía en su única sucursal actual, ANTES de perder esas columnas.
            migrationBuilder.Sql(@"
                INSERT INTO catalogo.StockSucursal (Id, ProductoId, SucursalId, StockActual, StockMinimo, StockMaximo)
                SELECT NEWID(), Id, SucursalId, StockActual, StockMinimo, StockMaximo
                FROM catalogo.Productos
                WHERE TipoProducto = 0;");

            migrationBuilder.DropIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockActual",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockMaximo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                schema: "catalogo",
                table: "Categorias");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                schema: "catalogo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSucursal_ProductoId_SucursalId",
                schema: "catalogo",
                table: "StockSucursal",
                columns: new[] { "ProductoId", "SucursalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockSucursal",
                schema: "catalogo");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Codigo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.AddColumn<decimal>(
                name: "StockActual",
                schema: "catalogo",
                table: "Productos",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StockMaximo",
                schema: "catalogo",
                table: "Productos",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockMinimo",
                schema: "catalogo",
                table: "Productos",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                schema: "catalogo",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                schema: "catalogo",
                table: "Categorias",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos",
                columns: new[] { "SucursalId", "Codigo" },
                unique: true);
        }
    }
}
