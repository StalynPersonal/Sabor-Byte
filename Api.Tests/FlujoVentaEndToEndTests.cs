using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SaborByte.Dominio.Catalogo;
using SaborByte.Infraestructura.Persistencia;
using Xunit;

namespace SaborByte.Api.Tests;

// Prueba de integración de punta a punta contra el pipeline HTTP real: login,
// abrir turno, vender, verificar el resumen — el mismo flujo que se validó a mano
// con curl durante el desarrollo, ahora automatizado.
public class FlujoVentaEndToEndTests(SaborByteWebApplicationFactory factory) : IClassFixture<SaborByteWebApplicationFactory>
{
    [Fact]
    public async Task FlujoCompleto_LoginAbrirCajaVenderYCerrar_Funciona()
    {
        var (sucursalId, cajaId) = await factory.SembrarDatosBasicosAsync("admin-flujo", "Clave#123");

        Guid productoId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SaborByteDbContext>();
            var producto = new Producto
            {
                SucursalId = sucursalId,
                Nombre = "Hamburguesa de Prueba",
                Precio = 200m,
                AplicaItbis = true,
                TipoProducto = TipoProducto.Vendible
            };
            db.Productos.Add(producto);
            await db.SaveChangesAsync();
            productoId = producto.Id;
        }

        var cliente = factory.CreateClient();

        var loginResp = await cliente.PostAsJsonAsync("/api/auth/login", new { nombreUsuario = "admin-flujo", password = "Clave#123" });
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
        var login = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        var token = login.GetProperty("token").GetString();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var aperturaResp = await cliente.PostAsJsonAsync("/api/caja/turnos/abrir",
            new { cajaId, montoAperturaEfectivo = 500m });
        Assert.Equal(HttpStatusCode.OK, aperturaResp.StatusCode);
        var apertura = await aperturaResp.Content.ReadFromJsonAsync<JsonElement>();
        var turnoId = apertura.GetProperty("turnoCajaId").GetGuid();

        // Turno duplicado en la misma caja debe rechazarse (regla de negocio clave).
        var segundaApertura = await cliente.PostAsJsonAsync("/api/caja/turnos/abrir",
            new { cajaId, montoAperturaEfectivo = 100m });
        Assert.Equal(HttpStatusCode.BadRequest, segundaApertura.StatusCode);

        var ventaResp = await cliente.PostAsJsonAsync($"/api/ventas?sucursalId={sucursalId}", new
        {
            turnoCajaId = turnoId,
            formaPago = 0,
            items = new[] { new { productoId, cantidad = 2, descuento = 0 } }
        });
        Assert.Equal(HttpStatusCode.OK, ventaResp.StatusCode);
        var venta = await ventaResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(400m, venta.GetProperty("subtotal").GetDecimal());
        Assert.Equal(472m, venta.GetProperty("total").GetDecimal()); // 400 + 18% ITBIS

        var resumenResp = await cliente.GetAsync($"/api/caja/turnos/{turnoId}/resumen");
        Assert.Equal(HttpStatusCode.OK, resumenResp.StatusCode);
        var resumen = await resumenResp.Content.ReadFromJsonAsync<JsonElement>();
        var totales = resumen.GetProperty("totales").EnumerateArray().ToList();
        Assert.Single(totales);
        Assert.Equal(472m, totales[0].GetProperty("esperado").GetDecimal());

        var cierreResp = await cliente.PostAsJsonAsync("/api/caja/turnos/cerrar", new
        {
            turnoCajaId = turnoId,
            denominaciones = new[] { new { formaPago = 0, denominacion = 500, cantidad = 1 } }
        });
        Assert.Equal(HttpStatusCode.NoContent, cierreResp.StatusCode);

        // Ya cerrado: un segundo cierre debe rechazarse.
        var segundoCierre = await cliente.PostAsJsonAsync("/api/caja/turnos/cerrar",
            new { turnoCajaId = turnoId, denominaciones = Array.Empty<object>() });
        Assert.Equal(HttpStatusCode.BadRequest, segundoCierre.StatusCode);
    }
}
