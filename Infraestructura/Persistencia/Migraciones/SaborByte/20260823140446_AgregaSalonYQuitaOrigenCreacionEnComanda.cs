using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaSalonYQuitaOrigenCreacionEnComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrigenCreacion",
                schema: "pedidos",
                table: "Comandas");

            migrationBuilder.AddColumn<string>(
                name: "NombreSalon",
                schema: "pedidos",
                table: "Comandas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreSalon",
                schema: "pedidos",
                table: "Comandas");

            migrationBuilder.AddColumn<int>(
                name: "OrigenCreacion",
                schema: "pedidos",
                table: "Comandas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
