using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace SaborByte.Api.Tests;

public class AuthEndpointTests(SaborByteWebApplicationFactory factory) : IClassFixture<SaborByteWebApplicationFactory>
{
    [Fact]
    public async Task Login_CredencialesCorrectas_DevuelveToken()
    {
        await factory.SembrarDatosBasicosAsync("admin", "Clave#123");
        var cliente = factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new { nombreUsuario = "admin", password = "Clave#123" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.True(cuerpo!.ContainsKey("token"));
    }

    [Fact]
    public async Task Login_PasswordIncorrecta_Devuelve401()
    {
        await factory.SembrarDatosBasicosAsync("admin2", "Clave#123");
        var cliente = factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new { nombreUsuario = "admin2", password = "otra-clave" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_SinToken_Devuelve401()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.GetAsync($"/api/productos?sucursalId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task EndpointAdminOnly_UsuarioSinRolAdmin_Devuelve403()
    {
        var (sucursalId, _) = await factory.SembrarDatosBasicosAsync("admin3", "Clave#123");

        // Crea un segundo usuario sin rol Admin directamente en la base de pruebas.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SaborByte.Infraestructura.Persistencia.SaborByteDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<SaborByte.Aplicacion.Interfaces.IPasswordHasher>();
        var rolCajero = new SaborByte.Dominio.Identidad.Rol { Nombre = "Cajero" };
        var cajero = new SaborByte.Dominio.Identidad.Usuario
        {
            NombreUsuario = "cajero-prueba",
            Nombre = "Cajero de Prueba",
            HashPassword = hasher.Hash("Clave#123")
        };
        db.Roles.Add(rolCajero);
        db.Usuarios.Add(cajero);
        db.UsuarioRoles.Add(new SaborByte.Dominio.Identidad.UsuarioRol { Usuario = cajero, Rol = rolCajero });
        db.UsuarioSucursales.Add(new SaborByte.Dominio.Identidad.UsuarioSucursal { Usuario = cajero, SucursalId = sucursalId });
        await db.SaveChangesAsync();

        var cliente = factory.CreateClient();
        var loginResp = await cliente.PostAsJsonAsync("/api/auth/login", new { nombreUsuario = "cajero-prueba", password = "Clave#123" });
        var login = await loginResp.Content.ReadFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>();
        var token = login!["token"].GetString();

        cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var respuesta = await cliente.GetAsync("/api/usuarios"); // restringido a rol Admin

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }
}
