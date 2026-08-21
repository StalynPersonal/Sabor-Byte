using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Facturacion;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Inventario;
using SaborByte.Dominio.Pedidos;
using SaborByte.Dominio.Sucursales;

namespace SaborByte.Infraestructura.Persistencia;

public class SaborByteDbContext(DbContextOptions<SaborByteDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<UsuarioSucursal> UsuarioSucursales => Set<UsuarioSucursal>();

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ProductoIngrediente> ProductoIngredientes => Set<ProductoIngrediente>();

    public DbSet<Dominio.Caja.Caja> Cajas => Set<Dominio.Caja.Caja>();
    public DbSet<Dominio.Caja.TurnoCaja> TurnosCaja => Set<Dominio.Caja.TurnoCaja>();
    public DbSet<Dominio.Caja.MovimientoCaja> MovimientosCaja => Set<Dominio.Caja.MovimientoCaja>();
    public DbSet<Dominio.Caja.DenominacionCierre> DenominacionesCierre => Set<Dominio.Caja.DenominacionCierre>();

    public DbSet<SecuenciaNcf> SecuenciasNcf => Set<SecuenciaNcf>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaDetalle> FacturaDetalles => Set<FacturaDetalle>();

    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<ComandaItem> ComandaItems => Set<ComandaItem>();
    public DbSet<ComandaItemIngrediente> ComandaItemIngredientes => Set<ComandaItemIngrediente>();
    public DbSet<ComandaCancelacion> ComandaCancelaciones => Set<ComandaCancelacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<Sucursal>(b =>
        {
            b.ToTable("Sucursales", "sucursales");
            b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            b.Property(x => x.Rnc).HasMaxLength(20);
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
        });

        modelBuilder.Entity<Categoria>(b =>
        {
            b.ToTable("Categorias", "catalogo");
            b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Producto>(b =>
        {
            b.ToTable("Productos", "catalogo");
            b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            b.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            b.Property(x => x.StockMinimo).HasColumnType("decimal(18,3)");
            b.Property(x => x.StockMaximo).HasColumnType("decimal(18,3)");
            b.Property(x => x.StockActual).HasColumnType("decimal(18,3)");
            b.HasIndex(x => x.CodigoBarra);
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

        modelBuilder.Entity<Dominio.Caja.Caja>(b =>
        {
            b.ToTable("Cajas", "caja");
            b.Property(x => x.Numero).HasMaxLength(50).IsRequired();
            b.HasIndex(x => new { x.SucursalId, x.Numero }).IsUnique();
        });

        modelBuilder.Entity<Dominio.Caja.TurnoCaja>(b =>
        {
            b.ToTable("TurnosCaja", "caja");
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
        });

        modelBuilder.Entity<Dominio.Caja.DenominacionCierre>(b =>
        {
            b.ToTable("DenominacionesCierre", "caja");
            b.Property(x => x.Denominacion).HasColumnType("decimal(18,2)");
            b.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.TurnoCaja).WithMany(t => t.DenominacionesCierre).HasForeignKey(x => x.TurnoCajaId);
        });

        modelBuilder.Entity<SecuenciaNcf>(b =>
        {
            b.ToTable("SecuenciasNcf", "facturacion");
            b.Property(x => x.TipoComprobante).HasMaxLength(10).IsRequired();
        });

        modelBuilder.Entity<Factura>(b =>
        {
            b.ToTable("Facturas", "facturacion");
            b.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            b.Property(x => x.Itbis).HasColumnType("decimal(18,2)");
            b.Property(x => x.Descuento).HasColumnType("decimal(18,2)");
            b.Property(x => x.Total).HasColumnType("decimal(18,2)");
            b.Property(x => x.NumeroNcf).HasMaxLength(20);
            b.Property(x => x.TipoComprobante).HasMaxLength(10);
        });

        modelBuilder.Entity<FacturaDetalle>(b =>
        {
            b.ToTable("FacturaDetalles", "facturacion");
            b.Property(x => x.NombreProducto).HasMaxLength(200).IsRequired();
            b.Property(x => x.Cantidad).HasColumnType("decimal(18,3)");
            b.Property(x => x.PrecioUnitario).HasColumnType("decimal(18,2)");
            b.Property(x => x.Descuento).HasColumnType("decimal(18,2)");
            b.Property(x => x.Itbis).HasColumnType("decimal(18,2)");
            b.Property(x => x.Total).HasColumnType("decimal(18,2)");
            b.HasOne(x => x.Factura).WithMany(f => f.Detalle).HasForeignKey(x => x.FacturaId);
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
    }
}
