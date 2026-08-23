using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaComprobanteYCreadorEnCxcCxp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroComprobante",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroComprobante",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "CuentasPorPagar",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "CuentasPorCobrar",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroComprobante",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "NumeroComprobante",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "CuentasPorPagar");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "CuentasPorCobrar");
        }
    }
}
