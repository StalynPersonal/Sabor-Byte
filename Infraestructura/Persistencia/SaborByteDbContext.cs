using Microsoft.EntityFrameworkCore;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Sucursales;

namespace SaborByte.Infraestructura.Persistencia;

public class SaborByteDbContext(DbContextOptions<SaborByteDbContext> options) : DbContext(options)
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
    }
}
