using Microsoft.EntityFrameworkCore;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Dominio.Caja;
using SaborByte.Dominio.Catalogo;
using SaborByte.Dominio.Identidad;
using SaborByte.Dominio.Sucursales;

namespace SaborByte.Infraestructura.Persistencia;

// Datos mínimos para poder probar el flujo Fase 1 (login -> abrir caja -> vender -> cerrar caja)
// en un entorno recién creado. Solo pensado para desarrollo/demo, no para producción.
public static class SeedData
{
    public static async Task EjecutarAsync(SaborByteDbContext db, IPasswordHasher passwordHasher)
    {
        if (await db.Usuarios.AnyAsync())
            return; // ya sembrado

        var empresa = new Empresa { Nombre = "Sabor Byte" };
        db.Empresas.Add(empresa);

        var sucursal = new Sucursal { EmpresaId = empresa.Id, Nombre = "Sucursal Principal", Codigo = "01" };
        db.Sucursales.Add(sucursal);

        var roles = new[] { "Admin", "Supervisor", "Cajero", "Mesero", "Cocina", "AutorizaNotaCredito" }
            .Select(nombre => new Rol { Nombre = nombre })
            .ToList();
        db.Roles.AddRange(roles);

        var rolAdmin = roles.First(r => r.Nombre == "Admin");

        var admin = new Usuario
        {
            NombreUsuario = "admin",
            Nombre = "Administrador",
            HashPassword = passwordHasher.Hash("Admin#2026"),
            Email = "admin@saborbyte.local"
        };
        db.Usuarios.Add(admin);
        db.UsuarioRoles.Add(new UsuarioRol { Usuario = admin, Rol = rolAdmin });
        db.UsuarioSucursales.Add(new UsuarioSucursal { Usuario = admin, SucursalId = sucursal.Id });

        var rolSupervisor = roles.First(r => r.Nombre == "Supervisor");
        var supervisor = new Usuario
        {
            NombreUsuario = "supervisor",
            Nombre = "Supervisor de Turno",
            HashPassword = passwordHasher.Hash("Supervisor#2026"),
            Email = "supervisor@saborbyte.local"
        };
        db.Usuarios.Add(supervisor);
        db.UsuarioRoles.Add(new UsuarioRol { Usuario = supervisor, Rol = rolSupervisor });
        db.UsuarioSucursales.Add(new UsuarioSucursal { Usuario = supervisor, SucursalId = sucursal.Id });

        db.Cajas.Add(new Dominio.Caja.Caja
        {
            SucursalId = sucursal.Id,
            Numero = "01"
        });

        await db.SaveChangesAsync();
    }

    // Datos de demo del catálogo (15 platos/bebidas + 10 insumos con receta) — separado de
    // EjecutarAsync porque ese método no vuelve a correr una vez ya hay usuarios (y para
    // cuando se pidió esto ya había datos reales de prueba en la base). Se guarda contra
    // un código propio de este lote (no contra "hay algún producto"), para no duplicar si
    // la Api se reinicia pero tampoco chocar con productos que el usuario ya haya creado.
    public static async Task SeedCatalogoDemoAsync(SaborByteDbContext db)
    {
        if (await db.Productos.AnyAsync(p => p.Codigo == "PLT-001"))
            return;

        var sucursalIds = await db.Sucursales.Select(s => s.Id).ToListAsync();

        var libra = await ObtenerOCrearUnidadAsync(db, "Libra");
        var unidad = await ObtenerOCrearUnidadAsync(db, "Unidad");

        var catPlatosFuertes = await ObtenerOCrearCategoriaAsync(db, "Platos Fuertes", 1);
        var catAcompanantes = await ObtenerOCrearCategoriaAsync(db, "Acompañantes", 2);
        var catEnsaladas = await ObtenerOCrearCategoriaAsync(db, "Ensaladas", 3);
        var catBebidas = await ObtenerOCrearCategoriaAsync(db, "Bebidas", 4);
        var catPostres = await ObtenerOCrearCategoriaAsync(db, "Postres", 5);

        // --- Insumos (10) ---
        Producto CrearInsumo(string codigo, string nombre, Guid unidadMedidaId, decimal costoUnitario, string imagenSeed)
        {
            var insumo = new Producto
            {
                Nombre = nombre,
                Codigo = codigo,
                TipoProducto = TipoProducto.Insumo,
                CategoriaId = catPlatosFuertes.Id, // los insumos no se navegan por categoría en el menú; cualquiera válida sirve
                UnidadMedidaId = unidadMedidaId,
                CostoUnitario = costoUnitario,
                Precio = 0,
                ImagenUrl = $"https://picsum.photos/seed/{imagenSeed}/400/300"
            };
            db.Productos.Add(insumo);
            db.StockPorSucursal.AddRange(sucursalIds.Select(sid => new StockSucursal
            {
                ProductoId = insumo.Id,
                SucursalId = sid,
                StockActual = 100 // arranca con existencia para poder vender/probar de inmediato
            }));
            return insumo;
        }

        var pollo = CrearInsumo("INS-001", "Pollo", libra.Id, 90m, "insumo-pollo");
        var carneRes = CrearInsumo("INS-002", "Carne de Res", libra.Id, 180m, "insumo-carne-res");
        var chuletaCerdo = CrearInsumo("INS-003", "Chuleta de Cerdo", libra.Id, 140m, "insumo-chuleta-cerdo");
        var filetePescado = CrearInsumo("INS-004", "Filete de Pescado", libra.Id, 220m, "insumo-pescado");
        var camarones = CrearInsumo("INS-005", "Camarones", libra.Id, 380m, "insumo-camarones");
        var platanoVerde = CrearInsumo("INS-006", "Plátano Verde", unidad.Id, 15m, "insumo-platano");
        var arroz = CrearInsumo("INS-007", "Arroz", libra.Id, 35m, "insumo-arroz");
        var habichuelasRojas = CrearInsumo("INS-008", "Habichuelas Rojas", libra.Id, 55m, "insumo-habichuelas");
        var lechuga = CrearInsumo("INS-009", "Lechuga", unidad.Id, 40m, "insumo-lechuga");
        var tomate = CrearInsumo("INS-010", "Tomate", libra.Id, 45m, "insumo-tomate");

        // --- Productos vendibles (15), con receta donde aplica ---
        Producto CrearVendible(string codigo, string nombre, decimal precio, Guid categoriaId, string imagenSeed,
            params (Producto Insumo, decimal Cantidad, bool Opcional)[] receta)
        {
            var producto = new Producto
            {
                Nombre = nombre,
                Codigo = codigo,
                TipoProducto = TipoProducto.Vendible,
                CategoriaId = categoriaId,
                UnidadMedidaId = unidad.Id,
                Precio = precio,
                ImagenUrl = $"https://picsum.photos/seed/{imagenSeed}/400/300"
            };

            foreach (var (insumo, cantidad, opcional) in receta)
            {
                producto.Receta.Add(new ProductoIngrediente
                {
                    ProductoId = producto.Id,
                    InsumoId = insumo.Id,
                    CantidadUsada = cantidad,
                    IncluidoPorDefecto = true,
                    Opcional = opcional
                });
            }

            db.Productos.Add(producto);
            return producto;
        }

        CrearVendible("PLT-001", "Pollo Guisado", 250m, catPlatosFuertes.Id, "plato-pollo-guisado",
            (pollo, 1m, false));
        CrearVendible("PLT-002", "Res Guisada", 320m, catPlatosFuertes.Id, "plato-res-guisada",
            (carneRes, 1m, false));
        CrearVendible("PLT-003", "Chuleta Frita", 280m, catPlatosFuertes.Id, "plato-chuleta-frita",
            (chuletaCerdo, 1m, false));
        CrearVendible("PLT-004", "Pescado Frito", 350m, catPlatosFuertes.Id, "plato-pescado-frito",
            (filetePescado, 1m, false));
        CrearVendible("PLT-005", "Camarones al Ajillo", 450m, catPlatosFuertes.Id, "plato-camarones-ajillo",
            (camarones, 0.5m, false));
        CrearVendible("PLT-006", "Mofongo con Chicharrón", 300m, catPlatosFuertes.Id, "plato-mofongo",
            (platanoVerde, 3m, false), (chuletaCerdo, 0.3m, false));
        CrearVendible("PLT-007", "Sancocho", 380m, catPlatosFuertes.Id, "plato-sancocho",
            (carneRes, 0.5m, false), (pollo, 0.5m, false), (platanoVerde, 1m, false));
        CrearVendible("PLT-008", "Arroz Blanco", 80m, catAcompanantes.Id, "plato-arroz-blanco",
            (arroz, 0.5m, false));
        CrearVendible("PLT-009", "Habichuelas Rojas Guisadas", 80m, catAcompanantes.Id, "plato-habichuelas",
            (habichuelasRojas, 0.5m, false));
        CrearVendible("PLT-010", "Tostones", 100m, catAcompanantes.Id, "plato-tostones",
            (platanoVerde, 2m, false));
        // Ensalada con ambos ingredientes opcionales: demuestra la exclusión de ingredientes
        // al pedir (ej. "sin tomate") — ver ComandaItemIngrediente / Mesero-Caja.
        CrearVendible("PLT-011", "Ensalada Verde", 120m, catEnsaladas.Id, "plato-ensalada-verde",
            (lechuga, 1m, true), (tomate, 0.3m, true));
        CrearVendible("PLT-012", "Jugo de Chinola", 90m, catBebidas.Id, "bebida-jugo-chinola");
        CrearVendible("PLT-013", "Refresco de Cola", 70m, catBebidas.Id, "bebida-refresco-cola");
        CrearVendible("PLT-014", "Agua Embotellada", 50m, catBebidas.Id, "bebida-agua");
        CrearVendible("PLT-015", "Flan de Coco", 110m, catPostres.Id, "postre-flan-coco");

        await db.SaveChangesAsync();
    }

    // Valores iniciales de los catálogos de propina/denominaciones — antes vivían fijos en
    // el código de Caja; ahora son tablas editables desde Central, pero necesitan arrancar
    // con algo para no dejar los selectores vacíos justo después de la migración.
    public static async Task SeedConfiguracionCajaAsync(SaborByteDbContext db)
    {
        if (!await db.PorcentajesPropina.AnyAsync())
        {
            db.PorcentajesPropina.AddRange(
                new PorcentajePropina { Valor = 0m },
                new PorcentajePropina { Valor = 10m },
                new PorcentajePropina { Valor = 15m });
        }

        if (!await db.DenominacionesEfectivo.AnyAsync())
        {
            db.DenominacionesEfectivo.AddRange(
                new int[] { 2000, 1000, 500, 200, 100, 50, 25, 10, 5, 1 }
                    .Select(v => new DenominacionEfectivo { Valor = v }));
        }

        await db.SaveChangesAsync();
    }

    private static async Task<UnidadMedida> ObtenerOCrearUnidadAsync(SaborByteDbContext db, string nombre)
    {
        var existente = await db.UnidadesMedida.FirstOrDefaultAsync(u => u.Nombre == nombre);
        if (existente is not null)
            return existente;

        var nueva = new UnidadMedida { Nombre = nombre };
        db.UnidadesMedida.Add(nueva);
        return nueva;
    }

    private static async Task<Categoria> ObtenerOCrearCategoriaAsync(SaborByteDbContext db, string nombre, int orden)
    {
        var existente = await db.Categorias.FirstOrDefaultAsync(c => c.Nombre == nombre);
        if (existente is not null)
            return existente;

        var nueva = new Categoria { Nombre = nombre, Orden = orden };
        db.Categorias.Add(nueva);
        return nueva;
    }
}
