using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class CreaVentasSuspendidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ventas");

            migrationBuilder.CreateTable(
                name: "VentasSuspendidas",
                schema: "ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnoCajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PorcentajePropina = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DescuentoAutorizado = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasSuspendidas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VentaSuspendidaItems",
                schema: "ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VentaSuspendidaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaSuspendidaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaSuspendidaItems_VentasSuspendidas_VentaSuspendidaId",
                        column: x => x.VentaSuspendidaId,
                        principalSchema: "ventas",
                        principalTable: "VentasSuspendidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaSuspendidaItemIngredientes",
                schema: "ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VentaSuspendidaItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreIngrediente = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaSuspendidaItemIngredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaSuspendidaItemIngredientes_VentaSuspendidaItems_VentaSuspendidaItemId",
                        column: x => x.VentaSuspendidaItemId,
                        principalSchema: "ventas",
                        principalTable: "VentaSuspendidaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentaSuspendidaItemIngredientes_VentaSuspendidaItemId",
                schema: "ventas",
                table: "VentaSuspendidaItemIngredientes",
                column: "VentaSuspendidaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaSuspendidaItems_VentaSuspendidaId",
                schema: "ventas",
                table: "VentaSuspendidaItems",
                column: "VentaSuspendidaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentaSuspendidaItemIngredientes",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "VentaSuspendidaItems",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "VentasSuspendidas",
                schema: "ventas");
        }
    }
}
