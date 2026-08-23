using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaAnulacionPagoCxcCxp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "AnuladoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "AnuladoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anulado",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "AnuladoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "Anulado",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropColumn(
                name: "AnuladoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                schema: "cxccxp",
                table: "PagosCxC");
        }
    }
}
