using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Api.Tests;

// Levanta la Api real (pipeline, controllers, JWT, autorización) contra una base
// EF Core InMemory en vez de SQL Server, así las pruebas no dependen de un servidor
// de base de datos real. El entorno se fija a "Testing" para que Program.cs NO
// dispare la migración/seed automática pensada solo para Development.
public class SaborByteWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly string NombreBaseDatos = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" carga automáticamente appsettings.Testing.json (Jwt:Key, etc.)
        // ANTES de que Program.cs lo lea — a diferencia de inyectar configuración
        // vía ConfigureAppConfiguration, que llega demasiado tarde para minimal APIs
        // (Program.cs lee builder.Configuration["Jwt:Key"] antes de builder.Build()).
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // No basta con quitar solo DbContextOptions<T>: UseSqlServer ya registró sus
            // propios servicios internos de EF Core en el mismo IServiceCollection, y al
            // convivir con los de InMemory, EF lanza "Only a single database provider can
            // be registered". Se quita todo lo que EF Core haya registrado y se vuelve a
            // registrar limpio, solo con el proveedor InMemory.
            var descriptoresEfCore = services
                .Where(d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                .ToList();
            foreach (var descriptor in descriptoresEfCore)
                services.Remove(descriptor);

            services.AddDbContext<SaborByteDbContext>(opciones => opciones.UseInMemoryDatabase(NombreBaseDatos));
        });
    }

    public async Task<(Guid SucursalId, Guid CajaId)> SembrarDatosBasicosAsync(string usuarioAdmin, string password)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SaborByteDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var sucursal = new Dominio.Sucursales.Sucursal { Nombre = "Sucursal de Prueba" };
        var rolAdmin = new Dominio.Identidad.Rol { Nombre = "Admin" };
        var admin = new Dominio.Identidad.Usuario
        {
            NombreUsuario = usuarioAdmin,
            Nombre = "Administrador de Prueba",
            HashPassword = hasher.Hash(password)
        };
        var caja = new Dominio.Caja.Caja { SucursalId = sucursal.Id, Numero = "01" };

        db.Sucursales.Add(sucursal);
        db.Roles.Add(rolAdmin);
        db.Usuarios.Add(admin);
        db.UsuarioRoles.Add(new Dominio.Identidad.UsuarioRol { Usuario = admin, Rol = rolAdmin });
        db.UsuarioSucursales.Add(new Dominio.Identidad.UsuarioSucursal { Usuario = admin, SucursalId = sucursal.Id });
        db.Cajas.Add(caja);

        await db.SaveChangesAsync();

        return (sucursal.Id, caja.Id);
    }
}
