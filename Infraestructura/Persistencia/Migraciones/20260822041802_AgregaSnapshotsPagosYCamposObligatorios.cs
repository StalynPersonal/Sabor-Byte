using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregaSnapshotsPagosYCamposObligatorios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoCaja",
                schema: "caja",
                table: "TurnosCaja",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoSucursal",
                schema: "caja",
                table: "TurnosCaja",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            // Backfill: quién creó cada producto/categoría ya existente — no hay forma de saber
            // el usuario real, así que se le atribuye a "admin" (primer usuario del seed) en vez
            // de dejarlo en un GUID vacío sin sentido.
            migrationBuilder.Sql(@"
                DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM identidad.Usuarios WHERE NombreUsuario = 'admin');
                UPDATE catalogo.Productos SET CreadoPorUsuarioId = @AdminId WHERE CreadoPorUsuarioId = '00000000-0000-0000-0000-000000000000';
                UPDATE catalogo.Categorias SET CreadoPorUsuarioId = @AdminId WHERE CreadoPorUsuarioId = '00000000-0000-0000-0000-000000000000';
            ");

            // Backfill: a cada sucursal que tenga productos sin categoría se le crea una
            // categoría ""General"" (si no tiene ninguna todavía) y se la asigna a esos productos.
            migrationBuilder.Sql(@"
                DECLARE @AdminId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM identidad.Usuarios WHERE NombreUsuario = 'admin');

                INSERT INTO catalogo.Categorias (Id, SucursalId, Nombre, Orden, CreadoEn, CreadoPorUsuarioId)
                SELECT NEWID(), s.SucursalId, 'General', 0, SYSUTCDATETIME(), @AdminId2
                FROM (SELECT DISTINCT SucursalId FROM catalogo.Productos WHERE CategoriaId = '00000000-0000-0000-0000-000000000000') s
                WHERE NOT EXISTS (SELECT 1 FROM catalogo.Categorias c WHERE c.SucursalId = s.SucursalId);

                ;WITH CategoriaPorDefecto AS (
                    SELECT SucursalId, MIN(Id) AS CategoriaId
                    FROM catalogo.Categorias
                    GROUP BY SucursalId
                )
                UPDATE p
                SET p.CategoriaId = cpd.CategoriaId
                FROM catalogo.Productos p
                JOIN CategoriaPorDefecto cpd ON cpd.SucursalId = p.SucursalId
                WHERE p.CategoriaId = '00000000-0000-0000-0000-000000000000';
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "catalogo",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoriaId",
                schema: "catalogo",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CajaCodigo",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteNombre",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClienteRncOCedula",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SucursalCodigo",
                schema: "facturacion",
                table: "Facturas",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TasaItbis",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "decimal(5,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AplicaItbis",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Backfill: antes, TasaItbis NULL significaba "exento"; ya no queda ningún NULL
            // en esta base (se normalizó a 0 arriba), así que se reconstruye AplicaItbis a
            // partir de si la línea efectivamente cobró ITBIS.
            migrationBuilder.Sql(@"
                UPDATE facturacion.FacturaDetalles SET AplicaItbis = 1 WHERE Itbis > 0 OR TasaItbis > 0;
            ");

            migrationBuilder.AddColumn<string>(
                name: "CodigoCaja",
                schema: "caja",
                table: "DenominacionesCierre",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoSucursal",
                schema: "caja",
                table: "DenominacionesCierre",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "catalogo",
                table: "Categorias",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoSucursal",
                schema: "caja",
                table: "Cajas",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacturaPagos",
                schema: "facturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormaPago = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumeroComprobante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaPagos_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalSchema: "facturacion",
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaPagos_FacturaId",
                schema: "facturacion",
                table: "FacturaPagos",
                column: "FacturaId");

            // Backfill en cadena (el orden importa: cada nivel depende del anterior ya
            // actualizado): Cajas <- Sucursales, TurnosCaja <- Cajas, DenominacionesCierre
            // <- TurnosCaja, Facturas <- TurnosCaja/Cajas y Facturas <- Clientes.
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.CodigoSucursal = s.Codigo
                FROM caja.Cajas c
                JOIN sucursales.Sucursales s ON s.Id = c.SucursalId;

                UPDATE tc
                SET tc.CodigoSucursal = c.CodigoSucursal, tc.CodigoCaja = c.Numero
                FROM caja.TurnosCaja tc
                JOIN caja.Cajas c ON c.Id = tc.CajaId;

                UPDATE dc
                SET dc.CodigoSucursal = tc.CodigoSucursal, dc.CodigoCaja = tc.CodigoCaja
                FROM caja.DenominacionesCierre dc
                JOIN caja.TurnosCaja tc ON tc.Id = dc.TurnoCajaId;

                UPDATE f
                SET f.SucursalCodigo = tc.CodigoSucursal, f.CajaCodigo = tc.CodigoCaja
                FROM facturacion.Facturas f
                JOIN caja.TurnosCaja tc ON tc.Id = f.CajaTurnoId;

                UPDATE f
                SET f.ClienteNombre = cl.NombreORazonSocial, f.ClienteRncOCedula = cl.RncOCedula
                FROM facturacion.Facturas f
                JOIN clientes.Clientes cl ON cl.Id = f.ClienteId;
            ");

            // Backfill: reconstruye FacturaPagos para las facturas ya existentes a partir de
            // los MovimientosCaja de tipo Venta que ya tenían (mismo FormaPago/Monto; el
            // número de comprobante de tarjeta no se puede reconstruir retroactivamente).
            migrationBuilder.Sql(@"
                INSERT INTO facturacion.FacturaPagos (Id, FacturaId, FormaPago, Monto, NumeroComprobante)
                SELECT NEWID(), m.FacturaId, m.FormaPago, m.Monto, NULL
                FROM caja.MovimientosCaja m
                WHERE m.Tipo = 0 AND m.FacturaId IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturaPagos",
                schema: "facturacion");

            migrationBuilder.DropColumn(
                name: "CodigoCaja",
                schema: "caja",
                table: "TurnosCaja");

            migrationBuilder.DropColumn(
                name: "CodigoSucursal",
                schema: "caja",
                table: "TurnosCaja");

            migrationBuilder.DropColumn(
                name: "CajaCodigo",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ClienteNombre",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ClienteRncOCedula",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "SucursalCodigo",
                schema: "facturacion",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "AplicaItbis",
                schema: "facturacion",
                table: "FacturaDetalles");

            migrationBuilder.DropColumn(
                name: "CodigoCaja",
                schema: "caja",
                table: "DenominacionesCierre");

            migrationBuilder.DropColumn(
                name: "CodigoSucursal",
                schema: "caja",
                table: "DenominacionesCierre");

            migrationBuilder.DropColumn(
                name: "CodigoSucursal",
                schema: "caja",
                table: "Cajas");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "catalogo",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoriaId",
                schema: "catalogo",
                table: "Productos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<decimal>(
                name: "TasaItbis",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "decimal(5,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                schema: "facturacion",
                table: "FacturaDetalles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreadoPorUsuarioId",
                schema: "catalogo",
                table: "Categorias",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
