using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaCodigoAPromocion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                schema: "catalogo",
                table: "Promociones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Rellena las promociones que ya existían (creadas antes de este cambio) con un
            // código secuencial real — sin esto, todas quedarían con "" y el índice único de
            // abajo fallaría en cuanto hubiera más de una fila.
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.Codigo = 'PROMO-' + RIGHT('00000' + CAST(nums.Numero AS varchar(5)), 5)
                FROM [catalogo].[Promociones] p
                JOIN (
                    SELECT Id, ROW_NUMBER() OVER (ORDER BY FechaInicio) AS Numero
                    FROM [catalogo].[Promociones]
                ) nums ON nums.Id = p.Id;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_Codigo",
                schema: "catalogo",
                table: "Promociones",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Promociones_Codigo",
                schema: "catalogo",
                table: "Promociones");

            migrationBuilder.DropColumn(
                name: "Codigo",
                schema: "catalogo",
                table: "Promociones");
        }
    }
}
