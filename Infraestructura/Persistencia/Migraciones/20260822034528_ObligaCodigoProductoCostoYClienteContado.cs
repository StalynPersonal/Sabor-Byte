using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ObligaCodigoProductoCostoYClienteContado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.AddColumn<bool>(
                name: "EsGenerico",
                schema: "clientes",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Backfill: los productos que ya existían sin costo unitario quedan en 0
            // (antes era "desconocido"; ahora el campo es obligatorio).
            migrationBuilder.Sql(@"
                UPDATE catalogo.Productos SET CostoUnitario = 0 WHERE CostoUnitario IS NULL;
            ");

            // Backfill: a los productos sin código se les asigna uno temporal único por
            // sucursal (GEN0001, GEN0002...) — quedan marcados para que el admin les ponga
            // un código real desde la pantalla de Productos, pero no bloquean el arranque.
            migrationBuilder.Sql(@"
                ;WITH SinCodigo AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY SucursalId ORDER BY Id) AS Fila
                    FROM catalogo.Productos
                    WHERE Codigo IS NULL OR LTRIM(RTRIM(Codigo)) = ''
                )
                UPDATE p SET p.Codigo = 'GEN' + RIGHT('0000' + CAST(sc.Fila AS VARCHAR(4)), 4)
                FROM catalogo.Productos p
                JOIN SinCodigo sc ON sc.Id = p.Id;
            ");

            // Backfill: crea el cliente genérico ""Cliente Contado"" en cada sucursal que
            // tenga facturas sin cliente y no lo tenga ya, y se lo asigna a esas facturas
            // (Factura.ClienteId pasa a ser obligatorio).
            migrationBuilder.Sql(@"
                INSERT INTO clientes.Clientes (Id, SucursalId, NombreORazonSocial, TipoCliente, Activo, CreadoEn, EsGenerico)
                SELECT NEWID(), s.SucursalId, 'Cliente Contado', 1, 1, SYSUTCDATETIME(), 1
                FROM (SELECT DISTINCT SucursalId FROM facturacion.Facturas WHERE ClienteId IS NULL) s
                WHERE NOT EXISTS (
                    SELECT 1 FROM clientes.Clientes c WHERE c.SucursalId = s.SucursalId AND c.EsGenerico = 1
                );

                UPDATE f
                SET f.ClienteId = cc.Id
                FROM facturacion.Facturas f
                JOIN clientes.Clientes cc ON cc.SucursalId = f.SucursalId AND cc.EsGenerico = 1
                WHERE f.ClienteId IS NULL;
            ");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostoUnitario",
                schema: "catalogo",
                table: "Productos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                schema: "catalogo",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                schema: "facturacion",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos",
                columns: new[] { "SucursalId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EsGenerico",
                schema: "clientes",
                table: "Clientes");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostoUnitario",
                schema: "catalogo",
                table: "Productos",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                schema: "catalogo",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                schema: "facturacion",
                table: "Facturas",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_SucursalId_Codigo",
                schema: "catalogo",
                table: "Productos",
                columns: new[] { "SucursalId", "Codigo" },
                unique: true,
                filter: "[Codigo] IS NOT NULL");
        }
    }
}
