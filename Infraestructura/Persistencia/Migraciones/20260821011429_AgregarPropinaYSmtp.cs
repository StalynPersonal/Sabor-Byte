using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarPropinaYSmtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                schema: "sucursales",
                table: "Sucursales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                schema: "sucursales",
                table: "Sucursales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPuerto",
                schema: "sucursales",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpRemitente",
                schema: "sucursales",
                table: "Sucursales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpUsaSsl",
                schema: "sucursales",
                table: "Sucursales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsuario",
                schema: "sucursales",
                table: "Sucursales",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Propina",
                schema: "facturacion",
                table: "Facturas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpHost",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "SmtpPuerto",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "SmtpRemitente",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "SmtpUsaSsl",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "SmtpUsuario",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Propina",
                schema: "facturacion",
                table: "Facturas");
        }
    }
}
