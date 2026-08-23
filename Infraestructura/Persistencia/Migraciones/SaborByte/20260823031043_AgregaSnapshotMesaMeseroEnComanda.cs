using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaSnapshotMesaMeseroEnComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreMesero",
                schema: "pedidos",
                table: "Comandas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroMesa",
                schema: "pedidos",
                table: "Comandas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreMesero",
                schema: "pedidos",
                table: "Comandas");

            migrationBuilder.DropColumn(
                name: "NumeroMesa",
                schema: "pedidos",
                table: "Comandas");
        }
    }
}
