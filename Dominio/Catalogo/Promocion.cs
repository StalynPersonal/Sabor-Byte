namespace SaborByte.Dominio.Catalogo;

public enum TipoDescuentoPromocion
{
    Porcentaje,
    MontoFijo
}

// Descuento automático que Caja aplica solo al facturar, SIN necesitar autorización de
// supervisor (a diferencia del descuento manual — ver VentaAppService.CrearVentaAsync).
// Simple a propósito: un producto o una categoría, un rango de fechas, sin reglas de
// acumulación/cupones/horarios.
public class Promocion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // null = aplica a todas las sucursales; si se especifica, solo esa.
    public Guid? SucursalId { get; set; }

    public required string Nombre { get; set; }

    // Exactamente uno de los dos debe tener valor — ver PromocionAppService.ValidarDatos.
    public Guid? ProductoId { get; set; }
    public Guid? CategoriaId { get; set; }

    public TipoDescuentoPromocion TipoDescuento { get; set; }
    public decimal Valor { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public bool Activo { get; set; } = true;
}
