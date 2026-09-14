using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaCajaIdASesionActiva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CajaId",
                schema: "identidad",
                table: "SesionesActivas",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CajaId",
                schema: "identidad",
                table: "SesionesActivas");
        }
    }
}
