using Microsoft.EntityFrameworkCore;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Aplicacion.Tests;

// Cada prueba obtiene su propia base en memoria aislada (nombre único por llamada),
// así las pruebas pueden correr en paralelo sin pisarse datos entre sí.
public static class PruebaDbContextFactory
{
    public static SaborByteDbContext Crear()
    {
        var opciones = new DbContextOptionsBuilder<SaborByteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SaborByteDbContext(opciones);
    }
}
