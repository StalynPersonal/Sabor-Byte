using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarCajaFacturacionInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "caja");

            migrationBuilder.EnsureSchema(
                name: "facturacion");

            migrationBuilder.EnsureSchema(
                name: "inventario");

            migrationBuilder.AddColumn<decimal>(
                name: "StockActual",
                schema: "catalogo",
                table: "Productos",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Cajas",
                schema: "caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    IpPermitida = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HostnamePermitido = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                schema: "facturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CajaTurnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroNcf = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoComprobante = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Itbis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstadoDgii = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                schema: "inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SaldoResultante = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nota = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecuenciasNcf",
                schema: "facturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoComprobante = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SecuenciaInicial = table.Column<long>(type: "bigint", nullable: false),
                    SecuenciaProxima = table.Column<long>(type: "bigint", nullable: false),
                    SecuenciaFinal = table.Column<long>(type: "bigint", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuenciasNcf", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TurnosCaja",
                schema: "caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioAperturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioCierreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaHoraApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoAperturaEfectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalSchema: "caja",
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturaDetalles",
                schema: "facturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Itbis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaDetalles_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalSchema: "facturacion",
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DenominacionesCierre",
                schema: "caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnoCajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormaPago = table.Column<int>(type: "int", nullable: false),
                    Denominacion = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenominacionesCierre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DenominacionesCierre_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalSchema: "caja",
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                schema: "caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnoCajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FormaPago = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_TurnosCaja_TurnoCajaId",
                        column: x => x.TurnoCajaId,
                        principalSchema: "caja",
                        principalTable: "TurnosCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_SucursalId_Numero",
                schema: "caja",
                table: "Cajas",
                columns: new[] { "SucursalId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DenominacionesCierre_TurnoCajaId",
                schema: "caja",
                table: "DenominacionesCierre",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaDetalles_FacturaId",
                schema: "facturacion",
                table: "FacturaDetalles",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_TurnoCajaId",
                schema: "caja",
                table: "MovimientosCaja",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_CajaId",
                schema: "caja",
                table: "TurnosCaja",
                column: "CajaId",
                unique: true,
                filter: "[Estado] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DenominacionesCierre",
                schema: "caja");

            migrationBuilder.DropTable(
                name: "FacturaDetalles",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "MovimientosCaja",
                schema: "caja");

            migrationBuilder.DropTable(
                name: "MovimientosInventario",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "SecuenciasNcf",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "Facturas",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "TurnosCaja",
                schema: "caja");

            migrationBuilder.DropTable(
                name: "Cajas",
                schema: "caja");

            migrationBuilder.DropColumn(
                name: "StockActual",
                schema: "catalogo",
                table: "Productos");
        }
    }
}
