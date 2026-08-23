using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregaSesionesActivas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SesionesActivas",
                schema: "identidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpOrigen = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesActivas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SesionesActivas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "identidad",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioSucursales_SucursalId",
                schema: "identidad",
                table: "UsuarioSucursales",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesActivas_FechaCierre",
                schema: "identidad",
                table: "SesionesActivas",
                column: "FechaCierre");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesActivas_UsuarioId",
                schema: "identidad",
                table: "SesionesActivas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioSucursales_Sucursales_SucursalId",
                schema: "identidad",
                table: "UsuarioSucursales",
                column: "SucursalId",
                principalSchema: "sucursales",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioSucursales_Sucursales_SucursalId",
                schema: "identidad",
                table: "UsuarioSucursales");

            migrationBuilder.DropTable(
                name: "SesionesActivas",
                schema: "identidad");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioSucursales_SucursalId",
                schema: "identidad",
                table: "UsuarioSucursales");
        }
    }
}
