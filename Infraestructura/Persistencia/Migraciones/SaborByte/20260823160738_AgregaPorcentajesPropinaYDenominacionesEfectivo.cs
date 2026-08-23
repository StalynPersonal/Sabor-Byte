using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaPorcentajesPropinaYDenominacionesEfectivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DenominacionesEfectivo",
                schema: "caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenominacionesEfectivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PorcentajesPropina",
                schema: "caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PorcentajesPropina", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DenominacionesEfectivo_Valor",
                schema: "caja",
                table: "DenominacionesEfectivo",
                column: "Valor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PorcentajesPropina_Valor",
                schema: "caja",
                table: "PorcentajesPropina",
                column: "Valor",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DenominacionesEfectivo",
                schema: "caja");

            migrationBuilder.DropTable(
                name: "PorcentajesPropina",
                schema: "caja");
        }
    }
}
