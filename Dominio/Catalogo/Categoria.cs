using SaborByte.Dominio.Comun;

namespace SaborByte.Dominio.Catalogo;

// Catálogo de toda la empresa, igual que Producto — no se duplica por sucursal.
public class Categoria : EntidadBase
{
    public required string Nombre { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}
