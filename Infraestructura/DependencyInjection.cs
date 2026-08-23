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
        services.AddHttpClient<IServicioFacturacionElectronica, ServicioFacturacionElectronicaDgii>();
        services.AddScoped<IFacturacionElectronicaGateway, FacturacionElectronicaGateway>();

        services.AddScoped<IPasswordHasher, PasswordHasherAdaptador>();
        services.AddScoped<IGeneradorTokenJwt, GeneradorTokenJwt>();

        services.AddScoped<Aplicacion.Identidad.AutenticacionAppService>();
        services.AddScoped<Aplicacion.Catalogo.ProductoAppService>();
        services.AddScoped<Aplicacion.Catalogo.CategoriaAppService>();
        services.AddScoped<Aplicacion.Catalogo.MetodoPagoAppService>();
        services.AddScoped<Aplicacion.Catalogo.UnidadMedidaAppService>();
        services.AddScoped<Aplicacion.Inventario.InventarioAppService>();
        services.AddScoped<Aplicacion.Facturacion.VentaAppService>();
        services.AddScoped<Aplicacion.Facturacion.FacturaAppService>();
        services.AddScoped<Aplicacion.Facturacion.NotaCreditoAppService>();
        services.AddScoped<Aplicacion.Facturacion.SecuenciaNcfAppService>();
        services.AddScoped<Aplicacion.Facturacion.MotivoNotaCreditoAppService>();
        services.AddScoped<Aplicacion.Caja.CajaAppService>();
        services.AddScoped<Aplicacion.Caja.ConfiguracionCajaAppService>();
        services.AddScoped<Aplicacion.Pedidos.ComandaAppService>();
        services.AddScoped<Aplicacion.Pedidos.MesaAppService>();
        services.AddScoped<Aplicacion.Clientes.ClienteAppService>();
        services.AddScoped<Aplicacion.CxcCxp.CxcCxpAppService>();
        services.AddScoped<Aplicacion.Reportes.ReporteAppService>();
        services.AddScoped<Aplicacion.Identidad.UsuarioAppService>();
        services.AddScoped<Aplicacion.Sucursales.SucursalAppService>();
        services.AddScoped<Aplicacion.Sucursales.EmpresaAppService>();
        services.AddScoped<IEmailSender, Email.SmtpEmailSender>();
        services.AddScoped<IAuditoriaService, Auditoria.AuditoriaService>();
        services.AddScoped<Aplicacion.Identidad.AutorizacionAppService>();

        return services;
    }
}
