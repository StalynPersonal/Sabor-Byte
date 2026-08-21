using FacturacionElectronicaDGII;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Infraestructura.Facturacion;
using SaborByte.Infraestructura.Persistencia;

namespace SaborByte.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SaborByteDbContext>(opciones =>
            opciones.UseSqlServer(configuration.GetConnectionString("SaborByteDb")));

        services.AddScoped<IServicioFacturacionElectronica, ServicioFacturacionElectronicaDgii>();
        services.AddScoped<IFacturacionElectronicaGateway, FacturacionElectronicaGateway>();

        return services;
    }
}
