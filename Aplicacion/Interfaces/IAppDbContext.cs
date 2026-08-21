using Microsoft.EntityFrameworkCore;
using SaborByte.Dominio.Caja;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Clientes;
using SaborByte.Dominio.Comun;
using SaborByte.Dominio.CxcCxp;
using SaborByte.Dominio.Facturacion;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Inventario;
using SaborByte.Dominio.Pedidos;
using SaborByte.Dominio.Sucursales;

namespace SaborByte.Aplicacion.Interfaces;

// Punto único de acceso a datos para los servicios de aplicación, implementado
// por el DbContext real en Infraestructura. Evita depender de EF Core/SQL Server
// concretos desde Aplicacion, sin caer en un repositorio por entidad.
public interface IAppDbContext
{
    DbSet<Sucursal> Sucursales { get; }

    DbSet<Usuario> Usuarios { get; }
    DbSet<Rol> Roles { get; }
    DbSet<UsuarioRol> UsuarioRoles { get; }

    DbSet<Categoria> Categorias { get; }
    DbSet<Producto> Productos { get; }
    DbSet<ProductoIngrediente> ProductoIngredientes { get; }
    DbSet<ComboItem> ComboItems { get; }

    DbSet<Dominio.Caja.Caja> Cajas { get; }
    DbSet<TurnoCaja> TurnosCaja { get; }
    DbSet<MovimientoCaja> MovimientosCaja { get; }
    DbSet<DenominacionCierre> DenominacionesCierre { get; }

    DbSet<SecuenciaNcf> SecuenciasNcf { get; }
    DbSet<Factura> Facturas { get; }
    DbSet<FacturaDetalle> FacturaDetalles { get; }
    DbSet<NotaCreditoDebito> NotasCreditoDebito { get; }

    DbSet<MovimientoInventario> MovimientosInventario { get; }

    DbSet<Mesa> Mesas { get; }
    DbSet<Comanda> Comandas { get; }
    DbSet<ComandaItem> ComandaItems { get; }
    DbSet<ComandaItemIngrediente> ComandaItemIngredientes { get; }
    DbSet<ComandaCancelacion> ComandaCancelaciones { get; }

    DbSet<Cliente> Clientes { get; }

    DbSet<Proveedor> Proveedores { get; }
    DbSet<CuentaPorCobrar> CuentasPorCobrar { get; }
    DbSet<PagoCxC> PagosCxC { get; }
    DbSet<CuentaPorPagar> CuentasPorPagar { get; }
    DbSet<PagoCxP> PagosCxP { get; }

    DbSet<AutorizacionSupervisor> AutorizacionesSupervisor { get; }
    DbSet<LogAuditoria> LogsAuditoria { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
