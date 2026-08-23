using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregaCodigoYCodigosBarraProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_CodigoBarra",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                schema: "catalogo",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CodigosBarraProducto",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoBarra = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosBarraProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosBarraProducto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "catalogo",
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Preserva el CodigoBarra existente de cada producto como su primer código de
            // barra asociado, antes de borrar la columna vieja — evita perder datos ya
            // cargados al pasar del campo único a la colección.
            migrationBuilder.Sql("""
                INSERT INTO catalogo.CodigosBarraProducto (Id, ProductoId, CodigoBarra)
                SELECT NEWID(), Id, CodigoBarra
                FROM catalogo.Productos
                WHERE CodigoBarra IS NOT NULL AND LTRIM(RTRIM(CodigoBarra)) <> '';
                """);

            migrationBuilder.DropColumn(
                name: "CodigoBarra",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos",
                columns: new[] { "SucursalId", "Codigo" },
                unique: true,
                filter: "[Codigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosBarraProducto_CodigoBarra",
                schema: "catalogo",
                table: "CodigosBarraProducto",
                column: "CodigoBarra");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosBarraProducto_ProductoId",
                schema: "catalogo",
                table: "CodigosBarraProducto",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigosBarraProducto",
                schema: "catalogo");

            migrationBuilder.DropIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Codigo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "CodigoBarra",
                schema: "catalogo",
                table: "Productos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoBarra",
                schema: "catalogo",
                table: "Productos",
                column: "CodigoBarra");
        }
    }
}
