using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaValidacionCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_SucursalId_RncOCedula",
                schema: "clientes",
                table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_SucursalId_RncOCedula",
                schema: "clientes",
                table: "Clientes",
                columns: new[] { "SucursalId", "RncOCedula" },
                unique: true,
                filter: "[RncOCedula] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_SucursalId_RncOCedula",
                schema: "clientes",
                table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_SucursalId_RncOCedula",
                schema: "clientes",
                table: "Clientes",
                columns: new[] { "SucursalId", "RncOCedula" });
        }
    }
}
