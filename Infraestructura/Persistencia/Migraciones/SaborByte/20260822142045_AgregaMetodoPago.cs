using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaMetodoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetodosPago",
                schema: "catalogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EsEfectivo = table.Column<bool>(type: "bit", nullable: false),
                    RequiereComprobante = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.Id);
                });

            // Semilla de los 4 métodos que reemplazan al enum FormaPago (Efectivo=0, Tarjeta=1, Transferencia=2, Deposito=3)
            migrationBuilder.Sql(@"
                INSERT INTO catalogo.MetodosPago (Id, Nombre, EsEfectivo, RequiereComprobante, Activo) VALUES
                ('9c3b1a10-0001-4a00-8000-000000000001', 'Efectivo', 1, 0, 1),
                ('9c3b1a10-0001-4a00-8000-000000000002', 'Tarjeta', 0, 1, 1),
                ('9c3b1a10-0001-4a00-8000-000000000003', 'Transferencia', 0, 0, 1),
                ('9c3b1a10-0001-4a00-8000-000000000004', 'Deposito', 0, 0, 1);
            ");

            migrationBuilder.AddColumn<Guid>(
                name: "MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MetodoPagoId",
                schema: "caja",
                table: "MovimientosCaja",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MetodoPagoId",
                schema: "facturacion",
                table: "FacturaPagos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MetodoPagoId",
                schema: "caja",
                table: "DenominacionesCierre",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Backfill desde las columnas FormaPago (enum int) que todavía existen en este punto
            migrationBuilder.Sql(@"
                UPDATE caja.MovimientosCaja SET MetodoPagoId = CASE FormaPago
                    WHEN 0 THEN '9c3b1a10-0001-4a00-8000-000000000001'
                    WHEN 1 THEN '9c3b1a10-0001-4a00-8000-000000000002'
                    WHEN 2 THEN '9c3b1a10-0001-4a00-8000-000000000003'
                    WHEN 3 THEN '9c3b1a10-0001-4a00-8000-000000000004'
                    ELSE '9c3b1a10-0001-4a00-8000-000000000001'
                END;

                UPDATE facturacion.FacturaPagos SET MetodoPagoId = CASE FormaPago
                    WHEN 0 THEN '9c3b1a10-0001-4a00-8000-000000000001'
                    WHEN 1 THEN '9c3b1a10-0001-4a00-8000-000000000002'
                    WHEN 2 THEN '9c3b1a10-0001-4a00-8000-000000000003'
                    WHEN 3 THEN '9c3b1a10-0001-4a00-8000-000000000004'
                    ELSE '9c3b1a10-0001-4a00-8000-000000000001'
                END;

                UPDATE caja.DenominacionesCierre SET MetodoPagoId = CASE FormaPago
                    WHEN 0 THEN '9c3b1a10-0001-4a00-8000-000000000001'
                    WHEN 1 THEN '9c3b1a10-0001-4a00-8000-000000000002'
                    WHEN 2 THEN '9c3b1a10-0001-4a00-8000-000000000003'
                    WHEN 3 THEN '9c3b1a10-0001-4a00-8000-000000000004'
                    ELSE '9c3b1a10-0001-4a00-8000-000000000001'
                END;
            ");

            // PagosCxC/PagosCxP: FormaPago era texto libre — se mapea por nombre, con Efectivo como respaldo
            migrationBuilder.Sql(@"
                UPDATE p SET p.MetodoPagoId = ISNULL(m.Id, '9c3b1a10-0001-4a00-8000-000000000001')
                FROM cxccxp.PagosCxC p
                LEFT JOIN catalogo.MetodosPago m ON m.Nombre = p.FormaPago;

                UPDATE p SET p.MetodoPagoId = ISNULL(m.Id, '9c3b1a10-0001-4a00-8000-000000000001')
                FROM cxccxp.PagosCxP p
                LEFT JOIN catalogo.MetodosPago m ON m.Nombre = p.FormaPago;
            ");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                schema: "caja",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                schema: "facturacion",
                table: "FacturaPagos");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                schema: "caja",
                table: "DenominacionesCierre");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCxP_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxP",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCxC_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxC",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_MetodoPagoId",
                schema: "caja",
                table: "MovimientosCaja",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaPagos_MetodoPagoId",
                schema: "facturacion",
                table: "FacturaPagos",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_DenominacionesCierre_MetodoPagoId",
                schema: "caja",
                table: "DenominacionesCierre",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MetodosPago_Nombre",
                schema: "catalogo",
                table: "MetodosPago",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DenominacionesCierre_MetodosPago_MetodoPagoId",
                schema: "caja",
                table: "DenominacionesCierre",
                column: "MetodoPagoId",
                principalSchema: "catalogo",
                principalTable: "MetodosPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FacturaPagos_MetodosPago_MetodoPagoId",
                schema: "facturacion",
                table: "FacturaPagos",
                column: "MetodoPagoId",
                principalSchema: "catalogo",
                principalTable: "MetodosPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_MetodosPago_MetodoPagoId",
                schema: "caja",
                table: "MovimientosCaja",
                column: "MetodoPagoId",
                principalSchema: "catalogo",
                principalTable: "MetodosPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PagosCxC_MetodosPago_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxC",
                column: "MetodoPagoId",
                principalSchema: "catalogo",
                principalTable: "MetodosPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PagosCxP_MetodosPago_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxP",
                column: "MetodoPagoId",
                principalSchema: "catalogo",
                principalTable: "MetodosPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DenominacionesCierre_MetodosPago_MetodoPagoId",
                schema: "caja",
                table: "DenominacionesCierre");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturaPagos_MetodosPago_MetodoPagoId",
                schema: "facturacion",
                table: "FacturaPagos");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_MetodosPago_MetodoPagoId",
                schema: "caja",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosCxC_MetodosPago_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropForeignKey(
                name: "FK_PagosCxP_MetodosPago_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropTable(
                name: "MetodosPago",
                schema: "catalogo");

            migrationBuilder.DropIndex(
                name: "IX_PagosCxP_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropIndex(
                name: "IX_PagosCxC_MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCaja_MetodoPagoId",
                schema: "caja",
                table: "MovimientosCaja");

            migrationBuilder.DropIndex(
                name: "IX_FacturaPagos_MetodoPagoId",
                schema: "facturacion",
                table: "FacturaPagos");

            migrationBuilder.DropIndex(
                name: "IX_DenominacionesCierre_MetodoPagoId",
                schema: "caja",
                table: "DenominacionesCierre");

            migrationBuilder.DropColumn(
                name: "MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxP");

            migrationBuilder.DropColumn(
                name: "MetodoPagoId",
                schema: "cxccxp",
                table: "PagosCxC");

            migrationBuilder.DropColumn(
                name: "MetodoPagoId",
                schema: "caja",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "MetodoPagoId",
                schema: "facturacion",
                table: "FacturaPagos");

            migrationBuilder.DropColumn(
                name: "MetodoPagoId",
                schema: "caja",
                table: "DenominacionesCierre");

            migrationBuilder.AddColumn<string>(
                name: "FormaPago",
                schema: "cxccxp",
                table: "PagosCxP",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FormaPago",
                schema: "cxccxp",
                table: "PagosCxC",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FormaPago",
                schema: "caja",
                table: "MovimientosCaja",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FormaPago",
                schema: "facturacion",
                table: "FacturaPagos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FormaPago",
                schema: "caja",
                table: "DenominacionesCierre",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
