namespace SaborByte.Dominio.Cotizaciones;

public enum EstadoCotizacion
{
    Pendiente,
    Aceptada,
    Rechazada
}

// Presupuesto para un pedido/evento futuro (ej. cumpleaños, boda) — se arma con productos
// y cantidades estimadas, con precio de ese momento congelado (igual que una Factura, no
// se recalcula solo). Se crea desde Central o desde Caja; cuando el cliente confirma se
// marca Aceptada y cualquier cajero puede cargarla en el carrito para facturarla, con los
// mismos precios con que se cotizó. No es una venta ni afecta inventario hasta que eso
// pasa y se factura normal desde Caja.
public class Cotizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }

    // Snapshot del cliente — si se borra o cambia después, la cotización sigue mostrando
    // con quién iba (mismo criterio que Factura.ClienteNombre). Puede no tener Cliente
    // registrado todavía (prospecto que aún no es cliente formal).
    public Guid? ClienteId { get; set; }
    public required string ClienteNombre { get; set; }
    public string? ClienteTelefono { get; set; }

    public DateTime? FechaVencimiento { get; set; }
    public string? Notas { get; set; }

    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Pendiente;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public Guid CreadoPorUsuarioId { get; set; }

    // Se llena la primera vez que un cajero la carga en el carrito desde Caja — no bloquea
    // volver a cargarla (por si la primera vez algo se canceló antes de facturar), solo
    // informa en el listado que ya se usó.
    public DateTime? CargadaEnCarritoEn { get; set; }

    public ICollection<CotizacionItem> Items { get; set; } = [];
}

public class CotizacionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }

    public Guid ProductoId { get; set; }
    public Guid CategoriaId { get; set; } // snapshot, para recalcular promociones al cargar en el carrito
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Precio { get; set; } // precio base (sin ITBIS), snapshot al cotizar
    public decimal TasaItbis { get; set; } // snapshot, para poder mostrar un total estimado real
    public decimal Cantidad { get; set; }
}
