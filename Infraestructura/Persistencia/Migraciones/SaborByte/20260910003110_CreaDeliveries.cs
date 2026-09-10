using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class CreaDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deliveries");

            migrationBuilder.CreateTable(
                name: "Deliveries",
                schema: "deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbonosDelivery",
                schema: "deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroComprobante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Anulado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnuladoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonosDelivery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbonosDelivery_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalSchema: "deliveries",
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbonosDelivery_MetodosPago_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalSchema: "catalogo",
                        principalTable: "MetodosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturasDelivery",
                schema: "deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MontoFactura = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoDelivery = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AsignadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasDelivery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturasDelivery_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalSchema: "deliveries",
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbonosDelivery_DeliveryId",
                schema: "deliveries",
                table: "AbonosDelivery",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosDelivery_MetodoPagoId",
                schema: "deliveries",
                table: "AbonosDelivery",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasDelivery_DeliveryId",
                schema: "deliveries",
                table: "FacturasDelivery",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasDelivery_FacturaId",
                schema: "deliveries",
                table: "FacturasDelivery",
                column: "FacturaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbonosDelivery",
                schema: "deliveries");

            migrationBuilder.DropTable(
                name: "FacturasDelivery",
                schema: "deliveries");

            migrationBuilder.DropTable(
                name: "Deliveries",
                schema: "deliveries");
        }
    }
}
