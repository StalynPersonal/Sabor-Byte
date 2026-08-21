namespace SaborByte.Web.Api.Dtos;

public enum EstadoComanda
{
    Abierta,
    EnviadaCocina,
    Cerrada,
    Cancelada
}

public enum EstadoItemComanda
{
    Pendiente,
    EnPreparacion,
    Listo,
    Entregado,
    Cancelado
}

public enum RolQueCancelo
{
    Mesero,
    Cocina,
    Supervisor
}

public class ItemComandaRequestDto
{
    public Guid ProductoId { get; set; }
    public decimal Cantidad { get; set; } = 1;
    public string? Notas { get; set; }
    public List<Guid> IngredientesExcluidosIds { get; set; } = [];
}

public class CrearComandaRequestDto
{
    public Guid? MesaId { get; set; }
    public Guid? MeseroId { get; set; }
    public List<ItemComandaRequestDto> Items { get; set; } = [];
}

public class ComandaItemDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public EstadoItemComanda Estado { get; set; }
    public string? Notas { get; set; }
    public List<Guid> IngredientesExcluidosIds { get; set; } = [];
}

public class ComandaDto
{
    public Guid Id { get; set; }
    public int NumeroComanda { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? MeseroId { get; set; }
    public EstadoComanda Estado { get; set; }
    public DateTime CreadoEn { get; set; }
    public List<ComandaItemDto> Items { get; set; } = [];
}

public class CambiarEstadoItemRequestDto
{
    public EstadoItemComanda NuevoEstado { get; set; }
}

public class CancelarItemRequestDto
{
    public string Motivo { get; set; } = string.Empty;
    public RolQueCancelo Rol { get; set; }
}

public class CancelarComandaRequestDto
{
    public string Motivo { get; set; } = string.Empty;
    public RolQueCancelo Rol { get; set; }
}
