using SaborByte.Dominio.Catalogo;

namespace SaborByte.Aplicacion.Catalogo.Dtos;

public class ProductoResumenDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? ImagenUrl { get; set; }
    public string? CodigoBarra { get; set; }
    public decimal Precio { get; set; }
    public bool AplicaItbis { get; set; }
    public TipoProducto TipoProducto { get; set; }
}

public class ComponenteComboRequestDto
{
    public Guid ProductoIncluidoId { get; set; }
    public decimal Cantidad { get; set; } = 1;
}

public class CrearComboRequestDto
{
    public required string Nombre { get; set; }
    public decimal Precio { get; set; }
    public Guid? CategoriaId { get; set; }
    public List<ComponenteComboRequestDto> Componentes { get; set; } = [];
}
