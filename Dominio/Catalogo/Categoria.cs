using SaborByte.Dominio.Comun;

namespace SaborByte.Dominio.Catalogo;

public class Categoria : EntidadBase
{
    public required string Nombre { get; set; }
    public int Orden { get; set; }
}
