using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Clientes;
using SaborByte.Dominio.Comun;
using SaborByte.Dominio.CxcCxp;
using SaborByte.Dominio.Facturacion;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Inventario;
using SaborByte.Dominio.Pedidos;
using SaborByte.Dominio.Sucursales;

namespace SaborByte.Infraestructura.Persistencia;

public class SaborByteDbContext(DbContextOptions<SaborByteDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<UsuarioSucursal> UsuarioSucursales => Set<UsuarioSucursal>();
    public DbSet<SesionActiva> SesionesActivas => Set<SesionActiva>();

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<CodigoBarraProducto> CodigosBarraProducto => Set<CodigoBarraProducto>();
    public DbSet<ProductoIngrediente> ProductoIngredientes => Set<ProductoIngrediente>();
    public DbSet<ComboItem> ComboItems => Set<ComboItem>();
    public DbSet<StockSucursal> StockPorSucursal => Set<StockSucursal>();

    public DbSet<Dominio.Caja.Caja> Cajas => Set<Dominio.Caja.Caja>();
    public DbSet<Dominio.Caja.TurnoCaja> TurnosCaja => Set<Dominio.Caja.TurnoCaja>();
    public DbSet<Dominio.Caja.MovimientoCaja> MovimientosCaja => Set<Dominio.Caja.MovimientoCaja>();
    public DbSet<Dominio.Caja.DenominacionCierre> DenominacionesCierre => Set<Dominio.Caja.DenominacionCierre>();
    public DbSet<Dominio.Caja.PorcentajePropina> PorcentajesPropina => Set<Dominio.Caja.PorcentajePropina>();
    public DbSet<Dominio.Caja.DenominacionEfectivo> DenominacionesEfectivo => Set<Dominio.Caja.DenominacionEfectivo>();

    public DbSet<SecuenciaNcf> SecuenciasNcf => Set<SecuenciaNcf>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaDetalle> FacturaDetalles => Set<FacturaDetalle>();
    public DbSet<FacturaPago> FacturaPagos => Set<FacturaPago>();
    public DbSet<MotivoNotaCredito> MotivosNotaCredito => Set<MotivoNotaCredito>();
    public DbSet<NotaCredito> NotasCredito => Set<NotaCredito>();
    public DbSet<NotaCreditoDetalle> NotasCreditoDetalle => Set<NotaCreditoDetalle>();

    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<ComandaItem> ComandaItems => Set<ComandaItem>();
    public DbSet<ComandaItemIngrediente> ComandaItemIngredientes => Set<ComandaItemIngrediente>();
    public DbSet<ComandaCancelacion> ComandaCancelaciones => Set<ComandaCancelacion>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<CuentaPorCobrar> CuentasPorCobrar => Set<CuentaPorCobrar>();
    public DbSet<PagoCxC> PagosCxC => Set<PagoCxC>();
    public DbSet<CuentaPorPagar> CuentasPorPagar => Set<CuentaPorPagar>();
    public DbSet<PagoCxP> PagosCxP => Set<PagoCxP>();

    public DbSet<AutorizacionSupervisor> AutorizacionesSupervisor => Set<AutorizacionSupervisor>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<Empresa>(b =>
        {
            b.ToTable("Empresas", "sucursales");
            b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            b.Property(x => x.Rnc).HasMaxLength(20);
        });

        modelBuilder.Entity<Sucursal>(b =>
        {
            b.ToTable("Sucursales", "sucursales");
            b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            b.Property(x => x.Codigo).HasMaxLength(2);
            b.HasIndex(x => x.Codigo).IsUnique();
            b.Property(x => x.ProximoNumeroNota).HasDefaultValue(1L);
            b.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(b =>
        {
            b.ToTable("Usuarios", "identidad");
            b.Property(x => x.NombreUsuario).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.NombreUsuario).IsUnique();
        });

        modelBuilder.Entity<Rol>(b =>
        {
            b.ToTable("Roles", "identidad");
            b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Permiso>(b =>
        {
            b.ToTable("Permisos", "identidad");
            b.Property(x => x.Modulo).HasMaxLength(100).IsRequired();
            b.Property(x => x.Accion).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<RolPermiso>(b =>
        {
            b.ToTable("RolPermisos", "identidad");
            b.HasKey(x => new { x.RolId, x.PermisoId });
            b.HasOne(x => x.Rol).WithMany(r => r.Permisos).HasForeignKey(x => x.RolId);
            b.HasOne(x => x.Permiso).WithMany().HasForeignKey(x => x.PermisoId);
        });

        modelBuilder.Entity<UsuarioRol>(b =>
        {
            b.ToTable("UsuarioRoles", "identidad");
            b.HasKey(x => new { x.UsuarioId, x.RolId });
            b.HasOne(x => x.Usuario).WithMany(u => u.Roles).HasForeignKey(x => x.UsuarioId);
            b.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.RolId);
        });

        modelBuilder.Entity<UsuarioSucursal>(b =>
        {
            b.ToTable("UsuarioSucursales", "identidad");
            b.HasKey(x => new { x.UsuarioId, x.SucursalId });
            b.HasOne(x => x.Usuario).WithMany(u => u.SucursalesAsignadas).HasForeignKey(x => x.UsuarioId);
            b.HasOne(x => x.Sucursal).WithMany().HasForeignKey(x => x.SucursalId);
        });

        modelBuilder.Entity<SesionActiva>(b =>
        {
            b.ToTable("SesionesActivas", "identidad");
            b.Property(x => x.IpOrigen).HasMaxLength(45);
            b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
            // Consulta más frecuente: sesiones abiertas (FechaCierre IS NULL) ordenadas por
            // última actividad, para el panel de "usuarios activos ahora".
            b.HasIndex(x => x.FechaCierre);
        });

        modelBuilder.Entity<Categoria>(b =>
        {
            b.ToTable("Categorias", "catalogo");
            b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<MetodoPago>(b =>
        {
            b.ToTable("MetodosPago", "catalogo");
            b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<UnidadMedida>(b =>
        {
            b.ToTable("UnidadesMedida", "catalogo");
            b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Producto>(b =>
        {
            b.ToTable("Productos", "catalogo");
            b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            b.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            b.Property(x => x.CostoUnitario).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            b.Property(x => x.TasaItbis).HasColumnType("decimal(5,4)").HasDefaultValue(0.18m);
            b.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
            // Único en toda la empresa: el catálogo es compartido por todas las sucursales
            // (lo que varía por sucursal es el stock, ver StockSucursal).
            b.HasIndex(x => x.Codigo).IsUnique();
            b.HasOne(x => x.UnidadMedida).WithMany().HasForeignKey(x => x.UnidadMedidaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockSucursal>(b =>
        {
            b.ToTable("StockSucursal", "catalogo");
            b.Property(x => x.StockActual).HasColumnType("decimal(18,3)");
            b.Property(x => x.StockMinimo).HasColumnType("decimal(18,3)");
            b.Property(x => x.StockMaximo).HasColumnType("decimal(18,3)");
            b.HasIndex(x => new { x.ProductoId, x.SucursalId }).IsUnique();
            b.HasOne(x => x.Producto).WithMany(p => p.StockPorSucursal).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CodigoBarraProducto>(b =>
        {
            b.ToTable("CodigosBarraProducto", "catalogo");
            b.Property(x => x.CodigoBarra).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.CodigoBarra);
            b.HasOne(x => x.Producto).WithMany(p => p.CodigosBarra).HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductoIngrediente>(b =>
        {
            b.ToTable("ProductoIngredientes", "catalogo");
            b.Property(x => x.CantidadUsada).HasColumnType("decimal(18,3)");
            b.HasOne(x => x.Producto).WithMany(p => p.Receta).HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Insumo).WithMany().HasForeignKey(x => x.InsumoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComboItem>(b =>
        {
            b.ToTable("ComboItems", "catalogo");
            b.Property(x => x.Cantidad).HasColumnType("decimal(18,3)");
            b.HasOne(x => x.Combo).WithMany(p => p.ComponentesCombo).HasForeignKey(x => x.ComboId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ProductoIncluido).WithMany().HasForeignKey(x => x.ProductoIncluidoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MotivoNotaCredito>(b =>
        {
            b.ToTable("MotivosNotaCredito", "facturacion");
            b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            b.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<NotaCredito>(b =>
        {
            b.ToTable("NotasCredito", "facturacion");
            b.Property(x => x.NumeroNota).HasMaxLength(9);
            b.HasIndex(x => x.NumeroNota).IsUnique();
            b.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
            b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            b.Property(x => x.NumeroNcf).HasMaxLength(20);
            b.Property(x => x.TipoComprobante).HasMaxLength(10);
            b.HasOne<MotivoNotaCredito>().WithMany().HasForeignKey(x => x.MotivoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotaCreditoDetalle>(b =>
        {
            b.ToTable("NotasCreditoDetalle", "facturacion");
            b.Property(x => x.Cantidad).HasColumnType("decimal(18,3)");
            b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.Nota).WithMany(n => n.Detalle).HasForeignKey(x => x.NotaCreditoId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.FacturaDetalle).WithMany().HasForeignKey(x => x.FacturaDetalleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Dominio.Caja.Caja>(b =>
        {
            b.ToTable("Cajas", "caja");
            b.Property(x => x.Numero).HasMaxLength(50).IsRequired();
            b.Property(x => x.CodigoSucursal).HasMaxLength(2);
            b.HasIndex(x => new { x.SucursalId, x.Numero }).IsUnique();
            b.Property(x => x.ProximoNumeroFactura).HasDefaultValue(1L);
        });

        modelBuilder.Entity<Dominio.Caja.PorcentajePropina>(b =>
        {
            b.ToTable("PorcentajesPropina", "caja");
            b.Property(x => x.Valor).HasColumnType("decimal(5,2)");
            b.HasIndex(x => x.Valor).IsUnique();
        });

        modelBuilder.Entity<Dominio.Caja.DenominacionEfectivo>(b =>
        {
            b.ToTable("DenominacionesEfectivo", "caja");
            b.HasIndex(x => x.Valor).IsUnique();
        });

        modelBuilder.Entity<Dominio.Caja.TurnoCaja>(b =>
        {
            b.ToTable("TurnosCaja", "caja");
            b.Property(x => x.CodigoSucursal).HasMaxLength(2);
            b.Property(x => x.CodigoCaja).HasMaxLength(50);
            b.Property(x => x.MontoAperturaEfectivo).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.Caja).WithMany().HasForeignKey(x => x.CajaId);
            // Máximo un turno Abierto por caja a la vez.
            b.HasIndex(x => x.CajaId).IsUnique().HasFilter("[Estado] = 0");
        });

        modelBuilder.Entity<Dominio.Caja.MovimientoCaja>(b =>
        {
            b.ToTable("MovimientosCaja", "caja");
            b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.TurnoCaja).WithMany(t => t.Movimientos).HasForeignKey(x => x.TurnoCajaId);
            b.HasOne(x => x.MetodoPago).WithMany().HasForeignKey(x => x.MetodoPagoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Dominio.Caja.DenominacionCierre>(b =>
        {
            b.ToTable("DenominacionesCierre", "caja");
            b.Property(x => x.CodigoSucursal).HasMaxLength(2);
            b.Property(x => x.CodigoCaja).HasMaxLength(50);
            b.Property(x => x.Denominacion).HasColumnType("decimal(18,2)");
            b.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.TurnoCaja).WithMany(t => t.DenominacionesCierre).HasForeignKey(x => x.TurnoCajaId);
            b.HasOne(x => x.MetodoPago).WithMany().HasForeignKey(x => x.MetodoPagoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SecuenciaNcf>(b =>
        {
            b.ToTable("SecuenciasNcf", "facturacion");
            b.Property(x => x.TipoComprobante).HasMaxLength(10).IsRequired();
            b.Property(x => x.Serie).HasMaxLength(5).IsRequired();
        });

        modelBuilder.Entity<Factura>(b =>
        {
            b.ToTable("Facturas", "facturacion");
            b.Property(x => x.ClienteNombre).HasMaxLength(200).IsRequired();
            b.Property(x => x.ClienteRncOCedula).HasMaxLength(20);
            b.Property(x => x.SucursalCodigo).HasMaxLength(2);
            b.Property(x => x.CajaCodigo).HasMaxLength(50);
            b.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            b.Property(x => x.Itbis).HasColumnType("decimal(18,2)");
            b.Property(x => x.Descuento).HasColumnType("decimal(18,2)");
            b.Property(x => x.Propina).HasColumnType("decimal(18,2)");
            b.Property(x => x.Total).HasColumnType("decimal(18,2)");
            b.Property(x => x.NumeroFactura).HasMaxLength(9);
            b.HasIndex(x => x.NumeroFactura).IsUnique();
            b.Property(x => x.NumeroNcf).HasMaxLength(20);
            b.Property(x => x.TipoComprobante).HasMaxLength(10);
            b.Property(x => x.XmlFirmadoDgii).HasColumnType("nvarchar(max)");
            b.Property(x => x.CodigoSeguridadDgii).HasMaxLength(6);
            b.Property(x => x.TrackIdDgii).HasMaxLength(100);
            b.Property(x => x.MensajeDgii).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<FacturaDetalle>(b =>
        {
            b.ToTable("FacturaDetalles", "facturacion");
            b.Property(x => x.NombreProducto).HasMaxLength(200).IsRequired();
            b.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
            b.Property(x => x.UnidadMedida).HasMaxLength(50);
            b.Property(x => x.Cantidad).HasColumnType("decimal(18,3)");
            b.Property(x => x.CantidadAcreditada).HasColumnType("decimal(18,3)").HasDefaultValue(0m);
            b.Property(x => x.PrecioUnitario).HasColumnType("decimal(18,2)");
            b.Property(x => x.Descuento).HasColumnType("decimal(18,2)");
            b.Property(x => x.TasaItbis).HasColumnType("decimal(5,4)").HasDefaultValue(0m);
            b.Property(x => x.Itbis).HasColumnType("decimal(18,2)");
            b.Property(x => x.Total).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.Factura).WithMany(f => f.Detalle).HasForeignKey(x => x.FacturaId);
        });

        modelBuilder.Entity<FacturaPago>(b =>
        {
            b.ToTable("FacturaPagos", "facturacion");
            b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            b.Property(x => x.NumeroComprobante).HasMaxLength(50);
            b.HasOne(x => x.Factura).WithMany(f => f.Pagos).HasForeignKey(x => x.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.MetodoPago).WithMany().HasForeignKey(x => x.MetodoPagoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovimientoInventario>(b =>
        {
            b.ToTable("MovimientosInventario", "inventario");
            b.Property(x => x.Cantidad).HasColumnType("decimal(18,3)");
            b.Property(x => x.CostoUnitario).HasColumnType("decimal(18,2)");
            b.Property(x => x.SaldoResultante).HasColumnType("decimal(18,3)");
        });

        modelBuilder.Entity<Mesa>(b =>
        {
            b.ToTable("Mesas", "pedidos");
            b.Property(x => x.Numero).HasMaxLength(20).IsRequired();
            b.Property(x => x.Salon).HasMaxLength(100);
        });

        modelBuilder.Entity<Comanda>(b =>
        {
            b.ToTable("Comandas", "pedidos");
            b.Property(x => x.Id).ValueGeneratedNever(); // el GUID lo genera el cliente
            b.Property(x => x.NumeroComanda).UseIdentityColumn();
            b.HasIndex(x => new { x.SucursalId, x.NumeroComanda }).IsUnique();
        });

        modelBuilder.Entity<ComandaItem>(b =>
        {
            b.ToTable("ComandaItems", "pedidos");
            b.Property(x => x.NombreProducto).HasMaxLength(200).IsRequired();
            b.Property(x => x.Cantidad).HasColumnType("decimal(18,3)");
            b.Property(x => x.PrecioUnitario).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.Comanda).WithMany(c => c.Items).HasForeignKey(x => x.ComandaId);
        });

        modelBuilder.Entity<ComandaItemIngrediente>(b =>
        {
            b.ToTable("ComandaItemIngredientes", "pedidos");
            b.HasOne<ComandaItem>().WithMany(i => i.IngredientesExcluidos).HasForeignKey(x => x.ComandaItemId);
        });

        modelBuilder.Entity<ComandaCancelacion>(b =>
        {
            b.ToTable("ComandaCancelaciones", "pedidos");
            b.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Cliente>(b =>
        {
            b.ToTable("Clientes", "clientes");
            b.Property(x => x.NombreORazonSocial).HasMaxLength(200).IsRequired();
            b.Property(x => x.RncOCedula).HasMaxLength(20);
            b.Property(x => x.EsGenerico).HasDefaultValue(false);
            // Único cuando no es null: varios clientes "Consumo" sin RNC no chocan entre sí,
            // pero dos clientes con el mismo RNC/Cédula en la misma sucursal sí deben chocar.
            b.HasIndex(x => new { x.SucursalId, x.RncOCedula }).IsUnique().HasFilter("[RncOCedula] IS NOT NULL");
        });

        modelBuilder.Entity<Proveedor>(b =>
        {
            b.ToTable("Proveedores", "cxccxp");
            b.Property(x => x.NombreORazonSocial).HasMaxLength(200).IsRequired();
            b.Property(x => x.Rnc).HasMaxLength(20);
        });

        modelBuilder.Entity<CuentaPorCobrar>(b =>
        {
            b.ToTable("CuentasPorCobrar", "cxccxp");
            b.Property(x => x.MontoOriginal).HasColumnType("decimal(18,2)");
            b.Property(x => x.SaldoPendiente).HasColumnType("decimal(18,2)");
            b.HasMany(x => x.Pagos).WithOne(p => p.Cuenta).HasForeignKey(p => p.CuentaPorCobrarId);
        });

        modelBuilder.Entity<PagoCxC>(b =>
        {
            b.ToTable("PagosCxC", "cxccxp");
            b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            b.Property(x => x.NumeroComprobante).HasMaxLength(50);
            b.Property(x => x.MotivoAnulacion).HasMaxLength(300);
            b.HasOne(x => x.MetodoPago).WithMany().HasForeignKey(x => x.MetodoPagoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CuentaPorPagar>(b =>
        {
            b.ToTable("CuentasPorPagar", "cxccxp");
            b.Property(x => x.DocumentoReferencia).HasMaxLength(50).IsRequired();
            b.Property(x => x.MontoOriginal).HasColumnType("decimal(18,2)");
            b.Property(x => x.SaldoPendiente).HasColumnType("decimal(18,2)");
            b.HasMany(x => x.Pagos).WithOne(p => p.Cuenta).HasForeignKey(p => p.CuentaPorPagarId);
        });

        modelBuilder.Entity<PagoCxP>(b =>
        {
            b.ToTable("PagosCxP", "cxccxp");
            b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            b.Property(x => x.NumeroComprobante).HasMaxLength(50);
            b.Property(x => x.MotivoAnulacion).HasMaxLength(300);
            b.HasOne(x => x.MetodoPago).WithMany().HasForeignKey(x => x.MetodoPagoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AutorizacionSupervisor>(b =>
        {
            b.ToTable("AutorizacionesSupervisor", "identidad");
            b.Property(x => x.Accion).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<LogAuditoria>(b =>
        {
            b.ToTable("LogsAuditoria", "comun");
            b.Property(x => x.Accion).HasMaxLength(100).IsRequired();
            b.Property(x => x.Entidad).HasMaxLength(100).IsRequired();
            b.Property(x => x.Detalle).HasMaxLength(1000);
        });
    }
}
