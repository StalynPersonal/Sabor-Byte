using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregaDetalleNotaCreditoYMotivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProximoNumeroNota",
                schema: "sucursales",
                table: "Sucursales",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<Guid>(
                name: "MotivoId",
                schema: "facturacion",
                table: "NotasCreditoDebito",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroNota",
                schema: "facturacion",
                table: "NotasCreditoDebito",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadAcreditada",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MotivosNotaCredito",
                schema: "facturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosNotaCredito", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotasCreditoDebitoDetalle",
                schema: "facturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotaCreditoDebitoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasCreditoDebitoDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasCreditoDebitoDetalle_FacturaDetalles_FacturaDetalleId",
                        column: x => x.FacturaDetalleId,
                        principalSchema: "facturacion",
                        principalTable: "FacturaDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoDebitoDetalle_NotasCreditoDebito_NotaCreditoDebitoId",
                        column: x => x.NotaCreditoDebitoId,
                        principalSchema: "facturacion",
                        principalTable: "NotasCreditoDebito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoDebito_MotivoId",
                schema: "facturacion",
                table: "NotasCreditoDebito",
                column: "MotivoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoDebito_NumeroNota",
                schema: "facturacion",
                table: "NotasCreditoDebito",
                column: "NumeroNota",
                unique: true,
                filter: "[NumeroNota] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MotivosNotaCredito_Nombre",
                schema: "facturacion",
                table: "MotivosNotaCredito",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoDebitoDetalle_FacturaDetalleId",
                schema: "facturacion",
                table: "NotasCreditoDebitoDetalle",
                column: "FacturaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoDebitoDetalle_NotaCreditoDebitoId",
                schema: "facturacion",
                table: "NotasCreditoDebitoDetalle",
                column: "NotaCreditoDebitoId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotasCreditoDebito_MotivosNotaCredito_MotivoId",
                schema: "facturacion",
                table: "NotasCreditoDebito",
                column: "MotivoId",
                principalSchema: "facturacion",
                principalTable: "MotivosNotaCredito",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Backfill: las líneas de factura que ya existían no tenían Código/UnidadMedida
            // propios — se completan con el dato actual del producto (mejor esfuerzo; si el
            // producto cambió desde la venta, no se puede reconstruir el valor exacto de ese
            // momento).
            migrationBuilder.Sql(@"
                UPDATE fd
                SET fd.Codigo = p.Codigo, fd.UnidadMedida = p.UnidadMedida
                FROM facturacion.FacturaDetalles fd
                JOIN catalogo.Productos p ON p.Id = fd.ProductoId
                WHERE fd.UnidadMedida = '' OR fd.UnidadMedida IS NULL;

                UPDATE facturacion.FacturaDetalles SET UnidadMedida = 'Unidad' WHERE UnidadMedida = '' OR UnidadMedida IS NULL;
            ");

            // Semilla de motivos comunes — antes era texto libre sin catálogo.
            migrationBuilder.Sql(@"
                INSERT INTO facturacion.MotivosNotaCredito (Id, Nombre, Activo) VALUES
                    (NEWID(), 'Producto dañado o defectuoso', 1),
                    (NEWID(), 'Error de cobro', 1),
                    (NEWID(), 'Cliente insatisfecho', 1),
                    (NEWID(), 'Devolución de mercancía', 1),
                    (NEWID(), 'Otro', 1);
            ");

            // Backfill: a la nota que ya existía se le asigna un número interno (mismo
            // formato CodigoSucursal + TipoInterno + Secuencia que las nuevas), y se
            // vincula a un motivo del catálogo según su texto libre original (o 'Otro'
            // si no coincide con ninguno). El detalle por línea queda vacío para notas
            // históricas: no hay forma confiable de reconstruir a qué línea aplicaba.
            migrationBuilder.Sql(@"
                ;WITH NotaConTipo AS (
                    SELECT n.Id, n.SucursalId, n.Tipo,
                           CASE WHEN n.Tipo = 0 THEN '34' ELSE '33' END AS TipoInterno,
                           ROW_NUMBER() OVER (PARTITION BY n.SucursalId, n.Tipo ORDER BY n.FechaEmision, n.Id) AS Secuencia
                    FROM facturacion.NotasCreditoDebito n
                    WHERE n.NumeroNota IS NULL
                )
                UPDATE n
                SET n.NumeroNota = s.Codigo + nct.TipoInterno + RIGHT('00000' + CAST(nct.Secuencia AS VARCHAR(5)), 5),
                    n.MotivoId = ISNULL(
                        (SELECT TOP 1 m.Id FROM facturacion.MotivosNotaCredito m WHERE m.Nombre = 'Otro'),
                        NULL)
                FROM facturacion.NotasCreditoDebito n
                JOIN NotaConTipo nct ON nct.Id = n.Id
                JOIN sucursales.Sucursales s ON s.Id = n.SucursalId;

                UPDATE s
                SET s.ProximoNumeroNota = ISNULL(mx.MaxSecuencia, 0) + 1
                FROM sucursales.Sucursales s
                OUTER APPLY (
                    SELECT MAX(CAST(RIGHT(n.NumeroNota, 5) AS BIGINT)) AS MaxSecuencia
                    FROM facturacion.NotasCreditoDebito n
                    WHERE n.SucursalId = s.Id AND n.NumeroNota IS NOT NULL
                ) mx;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotasCreditoDebito_MotivosNotaCredito_MotivoId",
                schema: "facturacion",
                table: "NotasCreditoDebito");

            migrationBuilder.DropTable(
                name: "MotivosNotaCredito",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "NotasCreditoDebitoDetalle",
                schema: "facturacion");

            migrationBuilder.DropIndex(
                name: "IX_NotasCreditoDebito_MotivoId",
                schema: "facturacion",
                table: "NotasCreditoDebito");

            migrationBuilder.DropIndex(
                name: "IX_NotasCreditoDebito_NumeroNota",
                schema: "facturacion",
                table: "NotasCreditoDebito");

            migrationBuilder.DropColumn(
                name: "ProximoNumeroNota",
                schema: "sucursales",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "MotivoId",
                schema: "facturacion",
                table: "NotasCreditoDebito");

            migrationBuilder.DropColumn(
                name: "NumeroNota",
                schema: "facturacion",
                table: "NotasCreditoDebito");

            migrationBuilder.DropColumn(
                name: "CantidadAcreditada",
                schema: "facturacion",
                table: "FacturaDetalles");

            migrationBuilder.DropColumn(
                name: "Codigo",
                schema: "facturacion",
                table: "FacturaDetalles");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                schema: "facturacion",
                table: "FacturaDetalles");
        }
    }
}
