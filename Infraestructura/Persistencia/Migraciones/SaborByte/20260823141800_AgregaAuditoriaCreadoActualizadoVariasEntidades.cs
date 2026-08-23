using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaAuditoriaCreadoActualizadoVariasEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                schema: "identidad",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActualizadoPorUsuarioId",
                schema: "identidad",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                schema: "identidad",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "identidad",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                schema: "sucursales",
                table: "Sucursales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActualizadoPorUsuarioId",
                schema: "sucursales",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "sucursales",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                schema: "facturacion",
                table: "SecuenciasNcf",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActualizadoPorUsuarioId",
                schema: "facturacion",
                table: "SecuenciasNcf",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "facturacion",
                table: "SecuenciasNcf",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                schema: "cxccxp",
                table: "Proveedores",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "Proveedores",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "clientes",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualizadoEn",
                schema: "caja",
                table: "Cajas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActualizadoPorUsuarioId",
                schema: "caja",
                table: "Cajas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                schema: "caja",
                table: "Cajas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "caja",
                table: "Cajas",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                schema: "identidad",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ActualizadoPorUsuarioId",
                schema: "identidad",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                schema: "identidad",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "identidad",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ActualizadoPorUsuarioId",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                schema: "facturacion",
                table: "SecuenciasNcf");

            migrationBuilder.DropColumn(
                name: "ActualizadoPorUsuarioId",
                schema: "facturacion",
                table: "SecuenciasNcf");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "facturacion",
                table: "SecuenciasNcf");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                schema: "cxccxp",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "clientes",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ActualizadoEn",
                schema: "caja",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ActualizadoPorUsuarioId",
                schema: "caja",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                schema: "caja",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                schema: "caja",
                table: "Cajas");
        }
    }
}
