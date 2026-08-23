namespace SaborByte.Dominio.Pedidos;

public enum EstadoItemComanda
{
    Pendiente,
    EnPreparacion,
    Listo,
    Entregado,
    Cancelado
}

public class ComandaItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ComandaId { get; set; }
    public Comanda? Comanda { get; set; }

    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty; // snapshot al pedir
    public decimal Cantidad { get; set; }
    public EstadoItemComanda Estado { get; set; } = EstadoItemComanda.Pendiente;
    public string? Notas { get; set; }
    public decimal PrecioUnitario { get; set; }

    // Si el descuento de inventario (por receta) ya se aplicó para este ítem —
    // se usa para saber si una cancelación posterior debe revertir el kardex.
    public bool InventarioDescontado { get; set; }

    public ICollection<ComandaItemIngrediente> IngredientesExcluidos { get; set; } = [];
}

public class ComandaItemIngrediente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ComandaItemId { get; set; }
    public Guid IngredienteId { get; set; } // Producto (Insumo)
    public string NombreIngrediente { get; set; } = string.Empty; // snapshot al pedir
}
