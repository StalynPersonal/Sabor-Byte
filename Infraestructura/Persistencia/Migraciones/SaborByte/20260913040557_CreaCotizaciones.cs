using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class CreaCotizaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                schema: "ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClienteTelefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DescripcionEvento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaEvento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PorcentajePropina = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CargadaEnCarritoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CotizacionItems",
                schema: "ventas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TasaItbis = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionItems_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalSchema: "ventas",
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionItems_CotizacionId",
                schema: "ventas",
                table: "CotizacionItems",
                column: "CotizacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CotizacionItems",
                schema: "ventas");

            migrationBuilder.DropTable(
                name: "Cotizaciones",
                schema: "ventas");
        }
    }
}
