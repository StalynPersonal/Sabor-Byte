using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class MarcaBebidasDemoComoInventariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Corrección puntual del catálogo de demo sembrado en SeedData.SeedCatalogoDemoAsync:
            // el Agua Embotellada, Jugo de Chinola y Refresco de Cola son justo el caso de uso
            // de "Vendible de reventa" (se compran y venden igual, sin receta) — se marcan
            // Inventariable ahora que el campo existe, y se les crea su renglón de stock en
            // cada sucursal existente (mismo criterio que un Insumo nuevo).
            migrationBuilder.Sql(@"
                UPDATE [catalogo].[Productos]
                SET [Inventariable] = 1
                WHERE [Codigo] IN ('PLT-012', 'PLT-013', 'PLT-014');

                INSERT INTO [catalogo].[StockSucursal] ([Id], [ProductoId], [SucursalId], [StockActual], [StockMinimo], [StockMaximo])
                SELECT NEWID(), p.[Id], s.[Id], 50, NULL, NULL
                FROM [catalogo].[Productos] p
                CROSS JOIN [sucursales].[Sucursales] s
                WHERE p.[Codigo] IN ('PLT-012', 'PLT-013', 'PLT-014')
                  AND NOT EXISTS (
                      SELECT 1 FROM [catalogo].[StockSucursal] ss
                      WHERE ss.[ProductoId] = p.[Id] AND ss.[SucursalId] = s.[Id]
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
