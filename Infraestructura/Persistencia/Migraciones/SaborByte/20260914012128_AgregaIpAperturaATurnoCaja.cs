using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaIpAperturaATurnoCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpApertura",
                schema: "caja",
                table: "TurnosCaja",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpApertura",
                schema: "caja",
                table: "TurnosCaja");
        }
    }
}
