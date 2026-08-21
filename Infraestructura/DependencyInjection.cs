using FacturacionElectronicaDGII;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Infraestructura.Facturacion;
using SaborByte.Infraestructura.Identidad;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SaborByteDbContext>(opciones =>
            opciones.UseSqlServer(configuration.GetConnectionString("SaborByteDb")));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<SaborByteDbContext>());

        services.Configure<FacturacionElectronicaOpciones>(configuration.GetSection("FacturacionElectronica"));
        services.AddScoped(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FacturacionElectronicaOpciones>>().Value);
        services.AddScoped<IServicioFacturacionElectronica, ServicioFacturacionElectronicaDgii>();
        services.AddScoped<IFacturacionElectronicaGateway, FacturacionElectronicaGateway>();

        services.AddScoped<IPasswordHasher, PasswordHasherAdaptador>();
        services.AddScoped<IGeneradorTokenJwt, GeneradorTokenJwt>();

        services.AddScoped<Aplicacion.Identidad.AutenticacionAppService>();
        services.AddScoped<Aplicacion.Catalogo.ProductoAppService>();
        services.AddScoped<Aplicacion.Inventario.InventarioAppService>();
        services.AddScoped<Aplicacion.Facturacion.VentaAppService>();
        services.AddScoped<Aplicacion.Caja.CajaAppService>();
        services.AddScoped<Aplicacion.Pedidos.ComandaAppService>();
        services.AddScoped<Aplicacion.Clientes.ClienteAppService>();
        services.AddScoped<Aplicacion.CxcCxp.CxcCxpAppService>();

        return services;
    }
}
