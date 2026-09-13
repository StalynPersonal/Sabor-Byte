using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaAntiguedadYLimiteADeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturasDelivery_FacturaId",
                schema: "deliveries",
                table: "FacturasDelivery");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaQuitada",
                schema: "deliveries",
                table: "FacturasDelivery",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoQuitar",
                schema: "deliveries",
                table: "FacturasDelivery",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Quitada",
                schema: "deliveries",
                table: "FacturasDelivery",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "QuitadoPorUsuarioId",
                schema: "deliveries",
                table: "FacturasDelivery",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteSaldoPendiente",
                schema: "deliveries",
                table: "Deliveries",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturasDelivery_FacturaId",
                schema: "deliveries",
                table: "FacturasDelivery",
                column: "FacturaId",
                unique: true,
                filter: "[Quitada] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturasDelivery_FacturaId",
                schema: "deliveries",
                table: "FacturasDelivery");

            migrationBuilder.DropColumn(
                name: "FechaQuitada",
                schema: "deliveries",
                table: "FacturasDelivery");

            migrationBuilder.DropColumn(
                name: "MotivoQuitar",
                schema: "deliveries",
                table: "FacturasDelivery");

            migrationBuilder.DropColumn(
                name: "Quitada",
                schema: "deliveries",
                table: "FacturasDelivery");

            migrationBuilder.DropColumn(
                name: "QuitadoPorUsuarioId",
                schema: "deliveries",
                table: "FacturasDelivery");

            migrationBuilder.DropColumn(
                name: "LimiteSaldoPendiente",
                schema: "deliveries",
                table: "Deliveries");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasDelivery_FacturaId",
                schema: "deliveries",
                table: "FacturasDelivery",
                column: "FacturaId",
                unique: true);
        }
    }
}
