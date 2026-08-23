using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregaNumeroFacturaInternoYCodigos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                schema: "sucursales",
                table: "Sucursales",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroFactura",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProximoNumeroFactura",
                schema: "caja",
                table: "Cajas",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            // Backfill: asigna un código de 2 dígitos a cada sucursal existente (antes no
            // existía el campo). Con una sola sucursal hoy, esto simplemente le pone "01".
            migrationBuilder.Sql(@"
                ;WITH SucCodigos AS (
                    SELECT Id, RIGHT('00' + CAST(ROW_NUMBER() OVER (ORDER BY Nombre, Id) AS VARCHAR(2)), 2) AS Codigo
                    FROM sucursales.Sucursales
                )
                UPDATE s SET s.Codigo = sc.Codigo
                FROM sucursales.Sucursales s
                JOIN SucCodigos sc ON sc.Id = s.Id;
            ");

            // Backfill: numera correlativamente (por caja) las facturas que ya existían antes
            // de este campo, en el mismo formato CodigoSucursal+CodigoCaja+Secuencia(5).
            migrationBuilder.Sql(@"
                ;WITH FacturaCaja AS (
                    SELECT f.Id AS FacturaId, tc.CajaId,
                           ROW_NUMBER() OVER (PARTITION BY tc.CajaId ORDER BY f.FechaEmision, f.Id) AS Secuencia
                    FROM facturacion.Facturas f
                    JOIN caja.TurnosCaja tc ON tc.Id = f.CajaTurnoId
                )
                UPDATE f
                SET f.NumeroFactura = s.Codigo + c.Numero + RIGHT('00000' + CAST(fc.Secuencia AS VARCHAR(5)), 5)
                FROM facturacion.Facturas f
                JOIN FacturaCaja fc ON fc.FacturaId = f.Id
                JOIN caja.Cajas c ON c.Id = fc.CajaId
                JOIN sucursales.Sucursales s ON s.Id = c.SucursalId;
            ");

            // Deja el contador de cada caja apuntando después de la última factura ya numerada,
            // para que la siguiente venta continúe la secuencia sin reutilizar números.
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.ProximoNumeroFactura = ISNULL(mx.MaxSecuencia, 0) + 1
                FROM caja.Cajas c
                OUTER APPLY (
                    SELECT MAX(CAST(RIGHT(f.NumeroFactura, 5) AS BIGINT)) AS MaxSecuencia
                    FROM facturacion.Facturas f
                    JOIN caja.TurnosCaja tc ON tc.Id = f.CajaTurnoId
                    WHERE tc.CajaId = c.Id AND f.NumeroFactura IS NOT NULL
                ) mx;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_Codigo",
                schema: "sucursales",
                table: "Sucursales",
                column: "Codigo",
                unique: true,
                filter: "[Codigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_NumeroFactura",
                schema: "facturacion",
                table: "Facturas",
                column: "NumeroFactura",
                unique: true,
                filter: "[NumeroFactura] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sucursales_Codigo",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_NumeroFactura",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "Codigo",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "NumeroFactura",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ProximoNumeroFactura",
                schema: "caja",
                table: "Cajas");
        }
    }
}
