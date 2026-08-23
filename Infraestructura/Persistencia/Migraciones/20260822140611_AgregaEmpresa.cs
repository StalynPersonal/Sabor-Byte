using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregaEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                schema: "sucursales",
                table: "Sucursales",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Empresas",
                schema: "sucursales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rnc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            // Backfill: antes no existía Empresa — se crea una por cada sucursal que quede
            // sin asignar (usando su propio nombre como punto de partida; el admin puede
            // renombrarla y agrupar sucursales bajo una sola empresa después desde Central).
            // Se usa una tabla temporal con OUTPUT para mapear 1 a 1 sin depender de que los
            // nombres sean únicos (dos sucursales podrían compartir nombre).
            migrationBuilder.Sql(@"
                CREATE TABLE #MapaEmpresaSucursal (SucursalId UNIQUEIDENTIFIER, EmpresaId UNIQUEIDENTIFIER);

                DECLARE @SucursalId UNIQUEIDENTIFIER, @NuevaEmpresaId UNIQUEIDENTIFIER, @Nombre NVARCHAR(200), @Rnc NVARCHAR(20);

                DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                    SELECT Id, Nombre, Rnc FROM sucursales.Sucursales WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

                OPEN cur;
                FETCH NEXT FROM cur INTO @SucursalId, @Nombre, @Rnc;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SET @NuevaEmpresaId = NEWID();
                    INSERT INTO sucursales.Empresas (Id, Nombre, Rnc, Activa, CreadoEn)
                    VALUES (@NuevaEmpresaId, @Nombre, @Rnc, 1, SYSUTCDATETIME());

                    INSERT INTO #MapaEmpresaSucursal (SucursalId, EmpresaId) VALUES (@SucursalId, @NuevaEmpresaId);

                    FETCH NEXT FROM cur INTO @SucursalId, @Nombre, @Rnc;
                END
                CLOSE cur;
                DEALLOCATE cur;

                UPDATE s
                SET s.EmpresaId = m.EmpresaId
                FROM sucursales.Sucursales s
                JOIN #MapaEmpresaSucursal m ON m.SucursalId = s.Id;

                DROP TABLE #MapaEmpresaSucursal;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaId",
                schema: "sucursales",
                table: "Sucursales",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sucursales_Empresas_EmpresaId",
                schema: "sucursales",
                table: "Sucursales",
                column: "EmpresaId",
                principalSchema: "sucursales",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sucursales_Empresas_EmpresaId",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropTable(
                name: "Empresas",
                schema: "sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Sucursales_EmpresaId",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                schema: "sucursales",
                table: "Sucursales");
        }
    }
}
