using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Sucursales;

namespace SaborByte.Infraestructura.Persistencia;

// Datos mínimos para poder probar el flujo Fase 1 (login -> abrir caja -> vender -> cerrar caja)
// en un entorno recién creado. Solo pensado para desarrollo/demo, no para producción.
public static class SeedData
{
    public static async Task EjecutarAsync(SaborByteDbContext db, IPasswordHasher passwordHasher)
    {
        if (await db.Usuarios.AnyAsync())
            return; // ya sembrado

        var sucursal = new Sucursal { Nombre = "Sucursal Principal" };
        db.Sucursales.Add(sucursal);

        var roles = new[] { "Admin", "Supervisor", "Cajero", "Mesero", "Cocina" }
            .Select(nombre => new Rol { Nombre = nombre })
            .ToList();
        db.Roles.AddRange(roles);

        var rolAdmin = roles.First(r => r.Nombre == "Admin");

        var admin = new Usuario
        {
            NombreUsuario = "admin",
            Nombre = "Administrador",
            HashPassword = passwordHasher.Hash("Admin#2026"),
            Email = "admin@saborbyte.local"
        };
        db.Usuarios.Add(admin);
        db.UsuarioRoles.Add(new UsuarioRol { Usuario = admin, Rol = rolAdmin });
        db.UsuarioSucursales.Add(new UsuarioSucursal { Usuario = admin, SucursalId = sucursal.Id });

        db.Cajas.Add(new Dominio.Caja.Caja
        {
            SucursalId = sucursal.Id,
            Numero = "01"
        });

        await db.SaveChangesAsync();
    }
}
