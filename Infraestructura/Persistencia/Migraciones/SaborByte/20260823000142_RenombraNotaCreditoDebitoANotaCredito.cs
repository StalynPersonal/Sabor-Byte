using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class RenombraNotaCreditoDebitoANotaCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename real (no drop+recreate): no hay notas de débito en el sistema, así
            // que "NotaCreditoDebito" pasa a llamarse "NotaCredito" en todos lados.
            migrationBuilder.RenameTable(
                name: "NotasCreditoDebitoDetalle", schema: "facturacion",
                newName: "NotasCreditoDetalle", newSchema: "facturacion");

            migrationBuilder.RenameTable(
                name: "NotasCreditoDebito", schema: "facturacion",
                newName: "NotasCredito", newSchema: "facturacion");

            migrationBuilder.RenameColumn(
                name: "NotaCreditoDebitoId", schema: "facturacion", table: "NotasCreditoDetalle",
                newName: "NotaCreditoId");

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCreditoDetalle",
                name: "IX_NotasCreditoDebitoDetalle_NotaCreditoDebitoId", newName: "IX_NotasCreditoDetalle_NotaCreditoId");

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCreditoDetalle",
                name: "IX_NotasCreditoDebitoDetalle_FacturaDetalleId", newName: "IX_NotasCreditoDetalle_FacturaDetalleId");

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCredito",
                name: "IX_NotasCreditoDebito_MotivoId", newName: "IX_NotasCredito_MotivoId");

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCredito",
                name: "IX_NotasCreditoDebito_NumeroNota", newName: "IX_NotasCredito_NumeroNota");

            // Ya no hace falta: nunca se emiten notas de débito, siempre es Crédito.
            migrationBuilder.DropColumn(
                name: "Tipo", schema: "facturacion", table: "NotasCredito");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tipo", schema: "facturacion", table: "NotasCredito",
                type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCredito",
                name: "IX_NotasCredito_NumeroNota", newName: "IX_NotasCreditoDebito_NumeroNota");

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCredito",
                name: "IX_NotasCredito_MotivoId", newName: "IX_NotasCreditoDebito_MotivoId");

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCreditoDetalle",
                name: "IX_NotasCreditoDetalle_FacturaDetalleId", newName: "IX_NotasCreditoDebitoDetalle_FacturaDetalleId");

            migrationBuilder.RenameIndex(
                schema: "facturacion", table: "NotasCreditoDetalle",
                name: "IX_NotasCreditoDetalle_NotaCreditoId", newName: "IX_NotasCreditoDebitoDetalle_NotaCreditoDebitoId");

            migrationBuilder.RenameColumn(
                name: "NotaCreditoId", schema: "facturacion", table: "NotasCreditoDetalle",
                newName: "NotaCreditoDebitoId");

            migrationBuilder.RenameTable(
                name: "NotasCredito", schema: "facturacion",
                newName: "NotasCreditoDebito", newSchema: "facturacion");

            migrationBuilder.RenameTable(
                name: "NotasCreditoDetalle", schema: "facturacion",
                newName: "NotasCreditoDebitoDetalle", newSchema: "facturacion");
        }
    }
}
