using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaNumeroTurnoCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroTurno",
                schema: "caja",
                table: "TurnosCaja",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_NumeroTurno",
                schema: "caja",
                table: "TurnosCaja",
                column: "NumeroTurno",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TurnosCaja_NumeroTurno",
                schema: "caja",
                table: "TurnosCaja");

            migrationBuilder.DropColumn(
                name: "NumeroTurno",
                schema: "caja",
                table: "TurnosCaja");
        }
    }
}
