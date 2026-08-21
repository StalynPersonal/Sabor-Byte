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
