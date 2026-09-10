using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class CreaGastos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gastos");

            migrationBuilder.CreateTable(
                name: "CategoriasGasto",
                schema: "gastos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasGasto", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "gastos",
                table: "CategoriasGasto",
                columns: new[] { "Id", "Nombre", "Activo" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "Electricidad", true },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "Agua", true },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "Internet", true },
                    { new Guid("11111111-0000-0000-0000-000000000004"), "Alquiler", true },
                    { new Guid("11111111-0000-0000-0000-000000000005"), "Transporte", true },
                    { new Guid("11111111-0000-0000-0000-000000000006"), "Mantenimiento", true },
                    { new Guid("11111111-0000-0000-0000-000000000007"), "Otros", true }
                });

            migrationBuilder.CreateTable(
                name: "Gastos",
                schema: "gastos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaGasto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoriaGastoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EsGastoDelNegocio = table.Column<bool>(type: "bit", nullable: false),
                    MetodoPagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnoCajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovimientoCajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnuladoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gastos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gastos_CategoriasGasto_CategoriaGastoId",
                        column: x => x.CategoriaGastoId,
                        principalSchema: "gastos",
                        principalTable: "CategoriasGasto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_MetodosPago_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalSchema: "catalogo",
                        principalTable: "MetodosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_CategoriaGastoId",
                schema: "gastos",
                table: "Gastos",
                column: "CategoriaGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_MetodoPagoId",
                schema: "gastos",
                table: "Gastos",
                column: "MetodoPagoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gastos",
                schema: "gastos");

            migrationBuilder.DropTable(
                name: "CategoriasGasto",
                schema: "gastos");
        }
    }
}
