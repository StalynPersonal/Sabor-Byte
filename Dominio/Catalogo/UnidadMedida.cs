namespace SaborByte.Dominio.Catalogo;

// Catálogo global (no por sucursal): reemplaza el texto libre que tenía Producto para
// su unidad de medida (Unidad, Libra, Kg, Litro...), siguiendo el mismo patrón que
// MetodoPago — Admin lo administra desde Central sin tocar código.
public class UnidadMedida
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;
}
