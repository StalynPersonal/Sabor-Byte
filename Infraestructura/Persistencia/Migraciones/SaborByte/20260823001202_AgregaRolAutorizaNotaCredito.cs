using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaborByte.Infraestructura.Persistencia.Migraciones.SaborByte
{
    /// <inheritdoc />
    public partial class AgregaRolAutorizaNotaCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rol nuevo, dedicado exclusivamente a autorizar notas de crédito (ver
            // AutorizacionAppService) — se inserta con SQL directo porque SeedData.cs
            // solo siembra roles en una base totalmente vacía (sin usuarios todavía).
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM identidad.Roles WHERE Nombre = N'AutorizaNotaCredito')
                INSERT INTO identidad.Roles (Id, Nombre) VALUES (NEWID(), N'AutorizaNotaCredito');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM identidad.Roles WHERE Nombre = N'AutorizaNotaCredito';");
        }
    }
}
