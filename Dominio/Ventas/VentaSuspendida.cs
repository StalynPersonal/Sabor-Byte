namespace SaborByte.Dominio.Ventas;

// Un carrito de Caja "puesto en pausa": el cajero estaba armando una venta (aún no
// facturada, sin pagos ni comanda asociada) y algo lo interrumpió — se guarda con un
// nombre/referencia para retomarlo después exactamente como quedó, sin perder el turno
// atendiendo a otro cliente. No es una venta real hasta que se recupera y se factura.
public class VentaSuspendida
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }

    // Turno donde se guardó — sirve para bloquear el cierre de ese turno si queda alguna
    // sin resolver (ver CajaAppService.CerrarTurnoAsync), igual que ya se hace con
    // comandas abiertas y deliveries con saldo pendiente.
    public Guid? TurnoCajaId { get; set; }

    public required string Nombre { get; set; }

    // Snapshot del cliente: si se borra o cambia después de guardar, la venta suspendida
    // sigue mostrando con quién iba (igual criterio que Factura.ClienteNombre).
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }

    public decimal PorcentajePropina { get; set; }
    public decimal MontoDescuento { get; set; }
    public bool DescuentoAutorizado { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public Guid CreadoPorUsuarioId { get; set; }

    public ICollection<VentaSuspendidaItem> Items { get; set; } = [];
}

public class VentaSuspendidaItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VentaSuspendidaId { get; set; }
    public VentaSuspendida? VentaSuspendida { get; set; }

    public Guid ProductoId { get; set; }
    public Guid CategoriaId { get; set; } // snapshot, para recalcular promociones al recuperar
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Precio { get; set; } // precio base (sin ITBIS), snapshot al guardar
    public decimal Cantidad { get; set; }

    public ICollection<VentaSuspendidaItemIngrediente> IngredientesExcluidos { get; set; } = [];
}

public class VentaSuspendidaItemIngrediente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VentaSuspendidaItemId { get; set; }
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
}
