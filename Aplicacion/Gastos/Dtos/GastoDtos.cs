namespace SaborByte.Aplicacion.Gastos.Dtos;

public class CategoriaGastoDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public bool Activo { get; set; }
}

public class GuardarCategoriaGastoRequestDto
{
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;
}

public class GastoDto
{
    public Guid Id { get; set; }
    public DateTime FechaGasto { get; set; }
    public Guid CategoriaGastoId { get; set; }
    public string CategoriaGastoNombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public bool EsGastoDelNegocio { get; set; }
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public bool AfectoCaja { get; set; }
    public string RegistradoPorNombre { get; set; } = string.Empty;
    public bool Anulado { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public string? AnuladoPorNombre { get; set; }
    public string? MotivoAnulacion { get; set; }
}

public class RegistrarGastoRequestDto
{
    public DateTime FechaGasto { get; set; } = DateTime.UtcNow;
    public Guid CategoriaGastoId { get; set; }
    public required string Descripcion { get; set; }
    public decimal Monto { get; set; }
    public bool EsGastoDelNegocio { get; set; } = true;
    public Guid MetodoPagoId { get; set; }

    // Solo aplica si el método de pago es efectivo: la caja/turno donde salió el dinero,
    // para que el cuadre de esa caja no muestre una diferencia sin explicar. Si el gasto
    // no es en efectivo, o no hay turno abierto, se deja null y no se toca ninguna caja.
    public Guid? TurnoCajaId { get; set; }
}

public class AnularGastoRequestDto
{
    public required string Motivo { get; set; }
}

public class ResumenGastosDto
{
    public decimal TotalPeriodo { get; set; }
    public List<GastoPorCategoriaDto> PorCategoria { get; set; } = [];
}

public class GastoPorCategoriaDto
{
    public string CategoriaGastoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}
