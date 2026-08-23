using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaUnidadMedida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                });

            // Semilla: una fila por cada valor de texto ya usado en Productos.UnidadMedida
            // (para no perder datos), más un set de unidades comunes de restaurante.
            migrationBuilder.Sql(@"
                INSERT INTO catalogo.UnidadesMedida (Id, Nombre, Activo)
                SELECT NEWID(), valores.Nombre, 1
                FROM (SELECT DISTINCT UnidadMedida AS Nombre FROM catalogo.Productos
                      WHERE UnidadMedida IS NOT NULL AND LTRIM(RTRIM(UnidadMedida)) <> '') AS valores
                WHERE NOT EXISTS (SELECT 1 FROM catalogo.UnidadesMedida um WHERE um.Nombre = valores.Nombre);

                INSERT INTO catalogo.UnidadesMedida (Id, Nombre, Activo)
                SELECT NEWID(), comunes.Nombre, 1
                FROM (VALUES ('Unidad'), ('Libra'), ('Onza'), ('Kg'), ('Gramo'), ('Litro'), ('Galon'), ('Docena')) AS comunes(Nombre)
                WHERE NOT EXISTS (SELECT 1 FROM catalogo.UnidadesMedida um WHERE um.Nombre = comunes.Nombre);
            ");

            migrationBuilder.AddColumn<Guid>(
                name: "UnidadMedidaId",
                schema: "catalogo",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Backfill: mapea el texto libre existente a la fila del catálogo con el mismo
            // nombre; cualquier producto sin unidad (NULL/vacío) cae en 'Unidad' por defecto.
            migrationBuilder.Sql(@"
                UPDATE p SET p.UnidadMedidaId = um.Id
                FROM catalogo.Productos p
                JOIN catalogo.UnidadesMedida um ON um.Nombre = p.UnidadMedida;

                UPDATE p SET p.UnidadMedidaId = (SELECT TOP 1 Id FROM catalogo.UnidadesMedida WHERE Nombre = 'Unidad')
                FROM catalogo.Productos p
                WHERE p.UnidadMedidaId = '00000000-0000-0000-0000-000000000000';
            ");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_UnidadMedidaId",
                schema: "catalogo",
                table: "Productos",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_Nombre",
                schema: "catalogo",
                table: "UnidadesMedida",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_UnidadesMedida_UnidadMedidaId",
                schema: "catalogo",
                table: "Productos",
                column: "UnidadMedidaId",
                principalSchema: "catalogo",
                principalTable: "UnidadesMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_UnidadesMedida_UnidadMedidaId",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "UnidadesMedida",
                schema: "catalogo");

            migrationBuilder.DropIndex(
                name: "IX_Productos_UnidadMedidaId",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UnidadMedidaId",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                schema: "catalogo",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
