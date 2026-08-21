using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarPedidosComandas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pedidos");

            migrationBuilder.AddColumn<Guid>(
                name: "ComandaId",
                schema: "facturacion",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComandaCancelaciones",
                schema: "pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComandaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComandaItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CanceladoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolQueCancelo = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InventarioRevertido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComandaCancelaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comandas",
                schema: "pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroComanda = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MesaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MeseroId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    OrigenCreacion = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comandas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mesas",
                schema: "pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Salon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComandaItems",
                schema: "pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComandaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InventarioDescontado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComandaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComandaItems_Comandas_ComandaId",
                        column: x => x.ComandaId,
                        principalSchema: "pedidos",
                        principalTable: "Comandas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComandaItemIngredientes",
                schema: "pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComandaItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComandaItemIngredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComandaItemIngredientes_ComandaItems_ComandaItemId",
                        column: x => x.ComandaItemId,
                        principalSchema: "pedidos",
                        principalTable: "ComandaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComandaItemIngredientes_ComandaItemId",
                schema: "pedidos",
                table: "ComandaItemIngredientes",
                column: "ComandaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ComandaItems_ComandaId",
                schema: "pedidos",
                table: "ComandaItems",
                column: "ComandaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comandas_SucursalId_NumeroComanda",
                schema: "pedidos",
                table: "Comandas",
                columns: new[] { "SucursalId", "NumeroComanda" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComandaCancelaciones",
                schema: "pedidos");

            migrationBuilder.DropTable(
                name: "ComandaItemIngredientes",
                schema: "pedidos");

            migrationBuilder.DropTable(
                name: "Mesas",
                schema: "pedidos");

            migrationBuilder.DropTable(
                name: "ComandaItems",
                schema: "pedidos");

            migrationBuilder.DropTable(
                name: "Comandas",
                schema: "pedidos");

            migrationBuilder.DropColumn(
                name: "ComandaId",
                schema: "facturacion",
                table: "Facturas");
        }
    }
}
